using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200045C RID: 1116
	internal class FileReceiver
	{
		// Token: 0x06004B05 RID: 19205 RVA: 0x00292C90 File Offset: 0x00290E90
		private static int GetMaxFileSizeInBytes(FileTransferType fileTransferType)
		{
			int result;
			if (fileTransferType == FileTransferType.Mod)
			{
				result = 524288000;
			}
			else
			{
				result = 52428800;
			}
			return result;
		}

		// Token: 0x1700132B RID: 4907
		// (get) Token: 0x06004B06 RID: 19206 RVA: 0x00292CB0 File Offset: 0x00290EB0
		public IReadOnlyList<FileReceiver.FileTransferIn> ActiveTransfers
		{
			get
			{
				return this.activeTransfers;
			}
		}

		// Token: 0x1700132C RID: 4908
		// (get) Token: 0x06004B07 RID: 19207 RVA: 0x00292CB8 File Offset: 0x00290EB8
		public bool HasActiveTransfers
		{
			get
			{
				return this.ActiveTransfers.Any<FileReceiver.FileTransferIn>();
			}
		}

		// Token: 0x06004B08 RID: 19208 RVA: 0x00292CC8 File Offset: 0x00290EC8
		public FileReceiver()
		{
			this.activeTransfers = new List<FileReceiver.FileTransferIn>();
			this.finishedTransfers = new List<ValueTuple<int, double>>();
		}

		// Token: 0x06004B09 RID: 19209 RVA: 0x00292D28 File Offset: 0x00290F28
		public void ReadMessage(IReadMessage inc)
		{
			switch (inc.ReadByte())
			{
			case 1:
			{
				byte transferId = inc.ReadByte();
				FileReceiver.FileTransferIn existingTransfer = this.activeTransfers.Find((FileReceiver.FileTransferIn t) => t.Connection.EndpointMatches(t.Connection.Endpoint) && t.ID == (int)transferId);
				this.finishedTransfers.RemoveAll(([TupleElementNames(new string[]
				{
					"transferId",
					"finishedTime"
				})] ValueTuple<int, double> t) => t.Item1 == (int)transferId);
				byte fileType = inc.ReadByte();
				int fileSize = inc.ReadInt32();
				string fileName = inc.ReadString();
				if (existingTransfer != null)
				{
					if (fileType != (byte)existingTransfer.FileType || fileSize != existingTransfer.FileSize || fileName != existingTransfer.FileName)
					{
						GameMain.Client.CancelFileTransfer((int)transferId);
						DebugConsole.AddWarning("File transfer error: file transfer initiated with an ID that's already in use", null);
						return;
					}
					GameMain.Client.UpdateFileTransfer(existingTransfer, existingTransfer.Received, existingTransfer.LastSeen, false);
					return;
				}
				else
				{
					if (fileType == 1 && Screen.Selected is ModDownloadScreen)
					{
						GameMain.Client.CancelFileTransfer((int)transferId);
						return;
					}
					string errorMsg;
					if (!this.ValidateInitialData(fileType, fileName, fileSize, out errorMsg))
					{
						GameMain.Client.CancelFileTransfer((int)transferId);
						DebugConsole.ThrowError("File transfer failed (" + errorMsg + ")", null, null, false, false);
						return;
					}
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.Log("Received file transfer initiation message: ");
						DebugConsole.Log("  File: " + fileName);
						DebugConsole.Log("  Size: " + fileSize.ToString());
						DebugConsole.Log("  ID: " + transferId.ToString());
					}
					string downloadFolder = this.downloadFolders[(FileTransferType)fileType];
					if (!Directory.Exists(downloadFolder))
					{
						try
						{
							Directory.CreateDirectory(downloadFolder, false);
						}
						catch (Exception e)
						{
							DebugConsole.ThrowError("Could not start a file transfer: failed to create the folder \"" + downloadFolder + "\".", e, null, false, false);
							break;
						}
					}
					FileReceiver.FileTransferIn newTransfer = new FileReceiver.FileTransferIn(inc.Sender, Path.Combine(new string[]
					{
						downloadFolder,
						fileName
					}), (FileTransferType)fileType)
					{
						ID = (int)transferId,
						Status = FileTransferStatus.Receiving,
						FileSize = fileSize
					};
					int maxRetries = 4;
					for (int i = 0; i <= maxRetries; i++)
					{
						try
						{
							newTransfer.OpenStream();
						}
						catch (IOException e2)
						{
							if (i >= maxRetries)
							{
								DebugConsole.NewMessage("Failed to initiate a file transfer {" + e2.Message + "}", new Color?(Color.Red), false);
								GameMain.Client.CancelFileTransfer((int)transferId);
								newTransfer.Status = FileTransferStatus.Error;
								this.OnTransferFailed(newTransfer);
								return;
							}
							DebugConsole.NewMessage("Failed to initiate a file transfer {" + e2.Message + "}, retrying in 250 ms...", new Color?(Color.Red), false);
							Thread.Sleep(250);
						}
					}
					this.activeTransfers.Add(newTransfer);
					GameMain.Client.UpdateFileTransfer(newTransfer, 0, 0, false);
					return;
				}
				break;
			}
			case 2:
			{
				byte transferId = inc.ReadByte();
				FileReceiver.FileTransferIn activeTransfer = this.activeTransfers.Find((FileReceiver.FileTransferIn t) => t.Connection.EndpointMatches(t.Connection.Endpoint) && t.ID == (int)transferId);
				if (activeTransfer == null)
				{
					this.finishedTransfers.RemoveAll(([TupleElementNames(new string[]
					{
						"transferId",
						"finishedTime"
					})] ValueTuple<int, double> t) => t.Item2 + 5.0 < Timing.TotalTime);
					if (!this.finishedTransfers.Any(([TupleElementNames(new string[]
					{
						"transferId",
						"finishedTime"
					})] ValueTuple<int, double> t) => t.Item1 == (int)transferId))
					{
						GameMain.Client.CancelFileTransfer((int)transferId);
						DebugConsole.AddWarning("File transfer error: received data without a transfer initiation message", null);
					}
					return;
				}
				int offset = inc.ReadInt32();
				int bytesToRead = (int)inc.ReadUInt16();
				if (offset != activeTransfer.Received)
				{
					activeTransfer.LastSeen = Math.Max(offset, activeTransfer.LastSeen);
					if (!activeTransfer.DataBuffer.ContainsKey(offset) && activeTransfer.DataBuffer.Count < 50)
					{
						activeTransfer.DataBuffer.Add(offset, inc.ReadBytes(bytesToRead));
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Received ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(bytesToRead);
					defaultInterpolatedStringHandler.AppendLiteral(" bytes of the file ");
					defaultInterpolatedStringHandler.AppendFormatted(activeTransfer.FileName);
					defaultInterpolatedStringHandler.AppendLiteral(" (ignoring: offset ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(offset);
					defaultInterpolatedStringHandler.AppendLiteral(", waiting for ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(activeTransfer.Received);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					GameMain.Client.UpdateFileTransfer(activeTransfer, activeTransfer.Received, activeTransfer.LastSeen, false);
					return;
				}
				activeTransfer.LastSeen = offset;
				if (activeTransfer.Received + bytesToRead > activeTransfer.FileSize)
				{
					GameMain.Client.CancelFileTransfer((int)transferId);
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"File transfer error: Received more data than expected (total received: ",
						activeTransfer.Received.ToString(),
						", msg received: ",
						(inc.LengthBytes - inc.BytePosition).ToString(),
						", msg length: ",
						inc.LengthBytes.ToString(),
						", msg read: ",
						inc.BytePosition.ToString(),
						", filesize: ",
						activeTransfer.FileSize.ToString()
					}), null, null, false, false);
					activeTransfer.Status = FileTransferStatus.Error;
					this.StopTransfer(activeTransfer, false);
					return;
				}
				try
				{
					activeTransfer.ReadBytes(inc, bytesToRead);
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 4);
						defaultInterpolatedStringHandler2.AppendLiteral("Received ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(bytesToRead);
						defaultInterpolatedStringHandler2.AppendLiteral(" bytes of the file ");
						defaultInterpolatedStringHandler2.AppendFormatted(activeTransfer.FileName);
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(activeTransfer.Received / 1000);
						defaultInterpolatedStringHandler2.AppendLiteral("/");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(activeTransfer.FileSize / 1000);
						defaultInterpolatedStringHandler2.AppendLiteral(" kB received)");
						DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					byte[] data;
					while (activeTransfer.DataBuffer.TryGetValue(activeTransfer.Received, out data))
					{
						activeTransfer.ReadBytes(data);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(55, 4);
						defaultInterpolatedStringHandler3.AppendLiteral("Read ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(data.Length);
						defaultInterpolatedStringHandler3.AppendLiteral(" bytes of buffer data of the file ");
						defaultInterpolatedStringHandler3.AppendFormatted(activeTransfer.FileName);
						defaultInterpolatedStringHandler3.AppendLiteral(" (");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(activeTransfer.Received / 1000);
						defaultInterpolatedStringHandler3.AppendLiteral("/");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(activeTransfer.FileSize / 1000);
						defaultInterpolatedStringHandler3.AppendLiteral(" kB received)");
						DebugConsole.Log(defaultInterpolatedStringHandler3.ToStringAndClear());
					}
				}
				catch (Exception e3)
				{
					GameMain.Client.CancelFileTransfer((int)transferId);
					DebugConsole.ThrowError("File transfer error: " + e3.Message, null, null, false, false);
					activeTransfer.Status = FileTransferStatus.Error;
					this.StopTransfer(activeTransfer, true);
					break;
				}
				GameMain.Client.UpdateFileTransfer(activeTransfer, activeTransfer.Received, activeTransfer.LastSeen, activeTransfer.Status == FileTransferStatus.Finished);
				if (activeTransfer.Status == FileTransferStatus.Finished)
				{
					activeTransfer.Dispose();
					string errorMessage;
					if (this.ValidateReceivedData(activeTransfer, out errorMessage))
					{
						this.finishedTransfers.Add(new ValueTuple<int, double>((int)transferId, Timing.TotalTime));
						this.StopTransfer(activeTransfer, false);
						this.OnFinished(activeTransfer);
						return;
					}
					new GUIMessageBox("File transfer aborted", errorMessage, null, null, GUIMessageBox.Type.Default);
					activeTransfer.Status = FileTransferStatus.Error;
					this.StopTransfer(activeTransfer, true);
					return;
				}
				break;
			}
			case 3:
			{
				byte transferId2 = inc.ReadByte();
				byte fileType2 = inc.ReadByte();
				string filePath = inc.ReadString();
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.Log("Received file transfer message on the same machine: ");
					DebugConsole.Log("  File: " + filePath);
					DebugConsole.Log("  ID: " + transferId2.ToString());
				}
				if (!File.Exists(filePath))
				{
					DebugConsole.ThrowError("File transfer on the same machine failed, file \"" + filePath + "\" not found.", null, null, false, false);
					GameMain.Client.CancelFileTransfer((int)transferId2);
					return;
				}
				FileReceiver.FileTransferIn directTransfer = new FileReceiver.FileTransferIn(inc.Sender, filePath, (FileTransferType)fileType2)
				{
					ID = (int)transferId2,
					Status = FileTransferStatus.Finished,
					FileSize = 0
				};
				this.OnFinished(directTransfer);
				return;
			}
			case 4:
			{
				byte transferId = inc.ReadByte();
				FileReceiver.FileTransferIn matchingTransfer = this.activeTransfers.Find((FileReceiver.FileTransferIn t) => t.Connection.EndpointMatches(t.Connection.Endpoint) && t.ID == (int)transferId);
				if (matchingTransfer != null)
				{
					new GUIMessageBox("File transfer cancelled", "The server has cancelled the transfer of the file \"" + matchingTransfer.FileName + "\".", null, null, GUIMessageBox.Type.Default);
					this.StopTransfer(matchingTransfer, false);
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06004B0A RID: 19210 RVA: 0x0029366C File Offset: 0x0029186C
		private bool ValidateInitialData(byte type, string fileName, int fileSize, out string errorMessage)
		{
			errorMessage = "";
			if (!Enum.IsDefined(typeof(FileTransferType), (int)type))
			{
				errorMessage = "Unknown file type";
				return false;
			}
			if (fileSize > FileReceiver.GetMaxFileSizeInBytes((FileTransferType)type))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
				defaultInterpolatedStringHandler.AppendLiteral("File too large (");
				defaultInterpolatedStringHandler.AppendFormatted(MathUtils.GetBytesReadable((long)fileSize));
				defaultInterpolatedStringHandler.AppendLiteral(" > ");
				defaultInterpolatedStringHandler.AppendFormatted(MathUtils.GetBytesReadable((long)FileReceiver.GetMaxFileSizeInBytes((FileTransferType)type)));
				defaultInterpolatedStringHandler.AppendLiteral(")");
				errorMessage = defaultInterpolatedStringHandler.ToStringAndClear();
				return false;
			}
			if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameCharsCrossPlatform().ToArray<char>()) > -1)
			{
				errorMessage = "Illegal characters in file name ''" + fileName + "''";
				return false;
			}
			if (type != 0)
			{
				if (type == 1)
				{
					if (Path.GetExtension(fileName) != ".save")
					{
						errorMessage = "Wrong file extension ''" + Path.GetExtension(fileName) + "''! (Expected .save)";
						return false;
					}
				}
			}
			else if (Path.GetExtension(fileName) != ".sub")
			{
				errorMessage = "Wrong file extension ''" + Path.GetExtension(fileName) + "''! (Expected .sub)";
				return false;
			}
			return true;
		}

		// Token: 0x06004B0B RID: 19211 RVA: 0x00293798 File Offset: 0x00291998
		private bool ValidateReceivedData(FileReceiver.FileTransferIn fileTransfer, out string ErrorMessage)
		{
			ErrorMessage = "";
			FileTransferType fileType = fileTransfer.FileType;
			if (fileType != FileTransferType.Submarine)
			{
				if (fileType == FileTransferType.CampaignSave)
				{
					try
					{
						IEnumerable<string> files = SaveUtil.EnumerateContainedFiles(fileTransfer.FilePath);
						foreach (string file in files)
						{
							string extension = Path.GetExtension(file);
							if ((!extension.Equals(".sub", StringComparison.OrdinalIgnoreCase) && !file.Equals("gamesession.xml")) || file.CleanUpPathCrossPlatform(false, "").Contains('/'))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Found unexpected file in \"");
								defaultInterpolatedStringHandler.AppendFormatted(fileTransfer.FileName);
								defaultInterpolatedStringHandler.AppendLiteral("\"! (");
								defaultInterpolatedStringHandler.AppendFormatted(file);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								ErrorMessage = defaultInterpolatedStringHandler.ToStringAndClear();
								return false;
							}
						}
					}
					catch (Exception e)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Loading received campaign save \"");
						defaultInterpolatedStringHandler2.AppendFormatted(fileTransfer.FileName);
						defaultInterpolatedStringHandler2.AppendLiteral("\" failed! {");
						defaultInterpolatedStringHandler2.AppendFormatted(e.Message);
						defaultInterpolatedStringHandler2.AppendLiteral("}");
						ErrorMessage = defaultInterpolatedStringHandler2.ToStringAndClear();
						return false;
					}
				}
			}
			else
			{
				Stream stream;
				try
				{
					stream = SaveUtil.DecompressFileToStream(fileTransfer.FilePath);
				}
				catch (Exception e2)
				{
					ErrorMessage = string.Concat(new string[]
					{
						"Loading received submarine \"",
						fileTransfer.FileName,
						"\" failed! {",
						e2.Message,
						"}"
					});
					return false;
				}
				if (stream == null)
				{
					ErrorMessage = "Decompressing received submarine file \"" + fileTransfer.FilePath + "\" failed!";
					return false;
				}
				try
				{
					stream.Position = 0L;
					XmlReaderSettings settings = new XmlReaderSettings
					{
						DtdProcessing = DtdProcessing.Prohibit,
						IgnoreProcessingInstructions = true
					};
					using (XmlReader reader = XmlReader.Create(stream, settings))
					{
						while (reader.Read())
						{
						}
					}
				}
				catch
				{
					if (stream != null)
					{
						stream.Close();
					}
					ErrorMessage = "Parsing file \"" + fileTransfer.FilePath + "\" failed! The file may not be a valid submarine file.";
					return false;
				}
				if (stream != null)
				{
					stream.Close();
				}
			}
			return true;
		}

		// Token: 0x06004B0C RID: 19212 RVA: 0x00293A10 File Offset: 0x00291C10
		public void StopTransfer(FileReceiver.FileTransferIn transfer, bool deleteFile = false)
		{
			if (transfer.Status != FileTransferStatus.Finished && transfer.Status != FileTransferStatus.Error)
			{
				transfer.Status = FileTransferStatus.Canceled;
			}
			if (this.activeTransfers.Contains(transfer))
			{
				this.activeTransfers.Remove(transfer);
			}
			transfer.Dispose();
			if (deleteFile && File.Exists(transfer.FilePath))
			{
				try
				{
					File.Delete(transfer.FilePath, false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Failed to delete file \"",
						transfer.FilePath,
						"\" (",
						e.Message,
						")"
					}), null, null, false, false);
				}
			}
		}

		// Token: 0x0400274A RID: 10058
		public FileReceiver.TransferInDelegate OnFinished;

		// Token: 0x0400274B RID: 10059
		public FileReceiver.TransferInDelegate OnTransferFailed;

		// Token: 0x0400274C RID: 10060
		private readonly List<FileReceiver.FileTransferIn> activeTransfers;

		// Token: 0x0400274D RID: 10061
		[TupleElementNames(new string[]
		{
			"transferId",
			"finishedTime"
		})]
		private readonly List<ValueTuple<int, double>> finishedTransfers;

		// Token: 0x0400274E RID: 10062
		private readonly ImmutableDictionary<FileTransferType, string> downloadFolders = new Dictionary<FileTransferType, string>
		{
			{
				FileTransferType.Submarine,
				SaveUtil.SubmarineDownloadFolder
			},
			{
				FileTransferType.CampaignSave,
				SaveUtil.CampaignDownloadFolder
			},
			{
				FileTransferType.Mod,
				"TempMods_Download"
			}
		}.ToImmutableDictionary<FileTransferType, string>();

		// Token: 0x020011C6 RID: 4550
		public class FileTransferIn : IDisposable
		{
			// Token: 0x17001CB6 RID: 7350
			// (get) Token: 0x060091CC RID: 37324 RVA: 0x003C61E9 File Offset: 0x003C43E9
			// (set) Token: 0x060091CD RID: 37325 RVA: 0x003C61F1 File Offset: 0x003C43F1
			public string FileName { get; private set; }

			// Token: 0x17001CB7 RID: 7351
			// (get) Token: 0x060091CE RID: 37326 RVA: 0x003C61FA File Offset: 0x003C43FA
			// (set) Token: 0x060091CF RID: 37327 RVA: 0x003C6202 File Offset: 0x003C4402
			public string FilePath { get; private set; }

			// Token: 0x17001CB8 RID: 7352
			// (get) Token: 0x060091D0 RID: 37328 RVA: 0x003C620B File Offset: 0x003C440B
			// (set) Token: 0x060091D1 RID: 37329 RVA: 0x003C6213 File Offset: 0x003C4413
			public int FileSize { get; set; }

			// Token: 0x17001CB9 RID: 7353
			// (get) Token: 0x060091D2 RID: 37330 RVA: 0x003C621C File Offset: 0x003C441C
			// (set) Token: 0x060091D3 RID: 37331 RVA: 0x003C6224 File Offset: 0x003C4424
			public int Received { get; private set; }

			// Token: 0x17001CBA RID: 7354
			// (get) Token: 0x060091D4 RID: 37332 RVA: 0x003C622D File Offset: 0x003C442D
			// (set) Token: 0x060091D5 RID: 37333 RVA: 0x003C6235 File Offset: 0x003C4435
			public int LastSeen { get; set; }

			// Token: 0x17001CBB RID: 7355
			// (get) Token: 0x060091D6 RID: 37334 RVA: 0x003C623E File Offset: 0x003C443E
			// (set) Token: 0x060091D7 RID: 37335 RVA: 0x003C6246 File Offset: 0x003C4446
			public FileTransferType FileType { get; private set; }

			// Token: 0x17001CBC RID: 7356
			// (get) Token: 0x060091D8 RID: 37336 RVA: 0x003C624F File Offset: 0x003C444F
			// (set) Token: 0x060091D9 RID: 37337 RVA: 0x003C6257 File Offset: 0x003C4457
			public FileTransferStatus Status { get; set; }

			// Token: 0x17001CBD RID: 7357
			// (get) Token: 0x060091DA RID: 37338 RVA: 0x003C6260 File Offset: 0x003C4460
			// (set) Token: 0x060091DB RID: 37339 RVA: 0x003C6268 File Offset: 0x003C4468
			public DateTime LastOffsetAckTime { get; private set; }

			// Token: 0x060091DC RID: 37340 RVA: 0x003C6271 File Offset: 0x003C4471
			public void RecordOffsetAckTime()
			{
				this.LastOffsetAckTime = DateTime.Now;
			}

			// Token: 0x17001CBE RID: 7358
			// (get) Token: 0x060091DD RID: 37341 RVA: 0x003C627E File Offset: 0x003C447E
			// (set) Token: 0x060091DE RID: 37342 RVA: 0x003C6286 File Offset: 0x003C4486
			public float BytesPerSecond { get; private set; }

			// Token: 0x17001CBF RID: 7359
			// (get) Token: 0x060091DF RID: 37343 RVA: 0x003C628F File Offset: 0x003C448F
			public float Progress
			{
				get
				{
					return (float)this.Received / (float)this.FileSize;
				}
			}

			// Token: 0x17001CC0 RID: 7360
			// (get) Token: 0x060091E0 RID: 37344 RVA: 0x003C62A0 File Offset: 0x003C44A0
			// (set) Token: 0x060091E1 RID: 37345 RVA: 0x003C62A8 File Offset: 0x003C44A8
			public FileStream WriteStream { get; private set; }

			// Token: 0x17001CC1 RID: 7361
			// (get) Token: 0x060091E2 RID: 37346 RVA: 0x003C62B1 File Offset: 0x003C44B1
			// (set) Token: 0x060091E3 RID: 37347 RVA: 0x003C62B9 File Offset: 0x003C44B9
			public int TimeStarted { get; private set; }

			// Token: 0x17001CC2 RID: 7362
			// (get) Token: 0x060091E4 RID: 37348 RVA: 0x003C62C2 File Offset: 0x003C44C2
			// (set) Token: 0x060091E5 RID: 37349 RVA: 0x003C62CA File Offset: 0x003C44CA
			public NetworkConnection Connection { get; private set; }

			// Token: 0x060091E6 RID: 37350 RVA: 0x003C62D4 File Offset: 0x003C44D4
			public FileTransferIn(NetworkConnection connection, string filePath, FileTransferType fileType)
			{
				this.FilePath = filePath;
				this.FileName = Path.GetFileName(this.FilePath);
				this.FileType = fileType;
				this.Connection = connection;
				this.Status = FileTransferStatus.NotStarted;
				this.LastOffsetAckTime = DateTime.Now - new TimeSpan(0, 0, 5, 0);
			}

			// Token: 0x060091E7 RID: 37351 RVA: 0x003C6338 File Offset: 0x003C4538
			public void OpenStream()
			{
				if (this.WriteStream != null)
				{
					this.WriteStream.Flush();
					this.WriteStream.Close();
					this.WriteStream.Dispose();
					this.WriteStream = null;
				}
				this.WriteStream = File.Open(this.FilePath, FileMode.Create, FileAccess.Write, null, true);
				this.TimeStarted = Environment.TickCount;
			}

			// Token: 0x060091E8 RID: 37352 RVA: 0x003C639D File Offset: 0x003C459D
			public void ReadBytes(IReadMessage inc, int bytesToRead)
			{
				if (this.Received + bytesToRead > this.FileSize)
				{
					bytesToRead -= this.Received + bytesToRead - this.FileSize;
				}
				this.ReadBytes(inc.ReadBytes(bytesToRead));
			}

			// Token: 0x060091E9 RID: 37353 RVA: 0x003C63D0 File Offset: 0x003C45D0
			public void ReadBytes(byte[] data)
			{
				this.Received += data.Length;
				this.WriteStream.Write(data, 0, data.Length);
				int passed = Environment.TickCount - this.TimeStarted;
				float psec = (float)passed / 1000f;
				this.BytesPerSecond = (float)this.Received / psec;
				List<int> outdatedKeys = (from k in this.DataBuffer.Keys
				where k < this.Received
				select k).ToList<int>();
				foreach (int key in outdatedKeys)
				{
					this.DataBuffer.Remove(key);
				}
				this.Status = ((this.Received >= this.FileSize) ? FileTransferStatus.Finished : FileTransferStatus.Receiving);
			}

			// Token: 0x060091EA RID: 37354 RVA: 0x003C64A8 File Offset: 0x003C46A8
			public void Dispose()
			{
				if (this.disposed)
				{
					return;
				}
				if (this.WriteStream != null)
				{
					this.WriteStream.Flush();
					this.WriteStream.Close();
					this.WriteStream.Dispose();
					this.WriteStream = null;
				}
				this.disposed = true;
			}

			// Token: 0x04005CF6 RID: 23798
			public int ID;

			// Token: 0x04005CF7 RID: 23799
			public const int DataBufferSize = 50;

			// Token: 0x04005CF8 RID: 23800
			public readonly Dictionary<int, byte[]> DataBuffer = new Dictionary<int, byte[]>();

			// Token: 0x04005CF9 RID: 23801
			private bool disposed;
		}

		// Token: 0x020011C7 RID: 4551
		// (Invoke) Token: 0x060091ED RID: 37357
		public delegate void TransferInDelegate(FileReceiver.FileTransferIn fileStreamReceiver);
	}
}
