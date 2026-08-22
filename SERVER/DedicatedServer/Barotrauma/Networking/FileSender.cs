using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.IO;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200036A RID: 874
	internal class FileSender
	{
		// Token: 0x17000E66 RID: 3686
		// (get) Token: 0x060033C7 RID: 13255 RVA: 0x0015D8E3 File Offset: 0x0015BAE3
		public IReadOnlyList<FileSender.FileTransferOut> ActiveTransfers
		{
			get
			{
				return this.activeTransfers;
			}
		}

		// Token: 0x060033C8 RID: 13256 RVA: 0x0015D8EB File Offset: 0x0015BAEB
		public FileSender(ServerPeer serverPeer, int mtu)
		{
			this.peer = serverPeer;
			this.chunkLen = mtu - 200;
			this.activeTransfers = new List<FileSender.FileTransferOut>();
		}

		// Token: 0x060033C9 RID: 13257 RVA: 0x0015D914 File Offset: 0x0015BB14
		public FileSender.FileTransferOut StartTransfer(NetworkConnection recipient, FileTransferType fileType, string filePath)
		{
			if (this.activeTransfers.Count >= 16)
			{
				return null;
			}
			if (this.activeTransfers.Count((FileSender.FileTransferOut t) => t.Connection == recipient) > 5)
			{
				return null;
			}
			if (!File.Exists(filePath))
			{
				DebugConsole.ThrowError("Failed to initiate file transfer (file \"" + filePath + "\" not found).\n" + Environment.StackTrace, null, null, false, false);
				return null;
			}
			FileSender.FileTransferOut transfer = null;
			try
			{
				transfer = new FileSender.FileTransferOut(recipient, fileType, filePath)
				{
					ID = 1
				};
				while (this.activeTransfers.Any((FileSender.FileTransferOut t) => t.Connection == recipient && t.ID == transfer.ID))
				{
					transfer.ID++;
				}
				this.activeTransfers.Add(transfer);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to initiate file transfer", e, null, false, false);
				return null;
			}
			FileSender.StartTime = DateTime.Now;
			this.OnStarted(transfer);
			GameServer server = GameMain.Server;
			ushort lastClientListUpdateID = server.LastClientListUpdateID;
			server.LastClientListUpdateID = lastClientListUpdateID + 1;
			return transfer;
		}

		// Token: 0x060033CA RID: 13258 RVA: 0x0015DA40 File Offset: 0x0015BC40
		public void Update(float deltaTime)
		{
			int numRemoved = this.activeTransfers.RemoveAll((FileSender.FileTransferOut t) => t.Connection.Status != NetworkConnectionStatus.Connected);
			List<FileSender.FileTransferOut> endedTransfers = this.activeTransfers.FindAll((FileSender.FileTransferOut t) => t.Connection.Status != NetworkConnectionStatus.Connected || t.Status == FileTransferStatus.Finished || t.Status == FileTransferStatus.Canceled || t.Status == FileTransferStatus.Error);
			foreach (FileSender.FileTransferOut transfer in endedTransfers)
			{
				this.activeTransfers.Remove(transfer);
				this.OnEnded(transfer);
			}
			foreach (FileSender.FileTransferOut transfer2 in this.activeTransfers)
			{
				transfer2.WaitTimer -= deltaTime;
				if (transfer2.WaitTimer <= 0f)
				{
					this.Send(transfer2);
				}
			}
			if (numRemoved > 0 || endedTransfers.Count > 0)
			{
				GameServer server = GameMain.Server;
				ushort lastClientListUpdateID = server.LastClientListUpdateID;
				server.LastClientListUpdateID = lastClientListUpdateID + 1;
			}
		}

		// Token: 0x060033CB RID: 13259 RVA: 0x0015DB7C File Offset: 0x0015BD7C
		private void Send(FileSender.FileTransferOut transfer)
		{
			try
			{
				if (!transfer.Acknowledged)
				{
					IWriteMessage message = new WriteOnlyMessage();
					message.WriteByte(9);
					if (transfer.Connection == GameMain.Server.OwnerConnection)
					{
						message.WriteByte(3);
						message.WriteByte((byte)transfer.ID);
						message.WriteByte((byte)transfer.FileType);
						message.WriteString(transfer.FilePath);
						this.peer.Send(message, transfer.Connection, DeliveryMethod.Unreliable, true);
						transfer.Status = FileTransferStatus.Finished;
					}
					else
					{
						message.WriteByte(1);
						message.WriteByte((byte)transfer.ID);
						message.WriteByte((byte)transfer.FileType);
						message.WriteInt32(transfer.Data.Length);
						message.WriteString(transfer.FileName);
						this.peer.Send(message, transfer.Connection, DeliveryMethod.Unreliable, true);
						transfer.Status = FileTransferStatus.Sending;
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DebugConsole.Log("Sending file transfer initiation message: ");
							DebugConsole.Log("  File: " + transfer.FileName);
							DebugConsole.Log("  Size: " + transfer.Data.Length.ToString());
							DebugConsole.Log("  ID: " + transfer.ID.ToString());
						}
					}
					transfer.WaitTimer = 0.1f;
				}
				else
				{
					int i = 0;
					while ((double)i < Math.Floor((double)transfer.PacketsPerUpdate))
					{
						long remaining = (long)(transfer.Data.Length - transfer.SentOffset);
						int sendByteCount = (remaining > (long)this.chunkLen) ? this.chunkLen : ((int)remaining);
						IWriteMessage message = new WriteOnlyMessage();
						message.WriteByte(9);
						message.WriteByte(2);
						message.WriteByte((byte)transfer.ID);
						message.WriteInt32(transfer.SentOffset);
						message.WriteUInt16((ushort)sendByteCount);
						int chunkDestPos = message.BytePosition;
						message.BitPosition += sendByteCount * 8;
						message.LengthBits = Math.Max(message.LengthBits, message.BitPosition);
						Array.Copy(transfer.Data, transfer.SentOffset, message.Buffer, chunkDestPos, sendByteCount);
						transfer.SentOffset += sendByteCount;
						this.peer.Send(message, transfer.Connection, DeliveryMethod.Unreliable, false);
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Sending ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(sendByteCount);
							defaultInterpolatedStringHandler.AppendLiteral(" bytes of the file ");
							defaultInterpolatedStringHandler.AppendFormatted(transfer.FileName);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(transfer.SentOffset / 1000);
							defaultInterpolatedStringHandler.AppendLiteral("/");
							defaultInterpolatedStringHandler.AppendFormatted<int>(transfer.Data.Length / 1000);
							defaultInterpolatedStringHandler.AppendLiteral(" kB sent)");
							DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						if (transfer.SentOffset >= transfer.Data.Length)
						{
							transfer.SentOffset = transfer.KnownReceivedOffset;
							transfer.WaitTimer = 1f;
						}
						transfer.PacketsPerUpdate = Math.Min((float)FileSender.FileTransferOut.MaxPacketsPerUpdate, transfer.PacketsPerUpdate + 0.05f);
						i++;
					}
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("FileSender threw an exception when trying to send data", e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("FileSender.Update:Exception", GameAnalyticsManager.ErrorSeverity.Error, "FileSender threw an exception when trying to send data:\n" + e.Message + "\n" + e.StackTrace.CleanupStackTrace());
				transfer.Status = FileTransferStatus.Error;
			}
		}

		// Token: 0x060033CC RID: 13260 RVA: 0x0015DF00 File Offset: 0x0015C100
		public void CancelTransfer(FileSender.FileTransferOut transfer)
		{
			transfer.Status = FileTransferStatus.Canceled;
			this.activeTransfers.Remove(transfer);
			this.OnEnded(transfer);
			GameMain.Server.SendCancelTransferMsg(transfer);
		}

		// Token: 0x060033CD RID: 13261 RVA: 0x0015DF30 File Offset: 0x0015C130
		public void ReadFileRequest(IReadMessage inc, Client client)
		{
			FileSender.<>c__DisplayClass17_0 CS$<>8__locals1 = new FileSender.<>c__DisplayClass17_0();
			CS$<>8__locals1.inc = inc;
			FileTransferMessageType messageType = (FileTransferMessageType)CS$<>8__locals1.inc.ReadByte();
			if (messageType == FileTransferMessageType.Cancel)
			{
				byte transferId = CS$<>8__locals1.inc.ReadByte();
				FileSender.FileTransferOut matchingTransfer = this.activeTransfers.Find((FileSender.FileTransferOut t) => t.Connection == CS$<>8__locals1.inc.Sender && t.ID == (int)transferId);
				if (matchingTransfer != null)
				{
					this.CancelTransfer(matchingTransfer);
				}
				return;
			}
			if (messageType == FileTransferMessageType.Data)
			{
				byte transferId = CS$<>8__locals1.inc.ReadByte();
				FileSender.FileTransferOut matchingTransfer2 = this.activeTransfers.Find((FileSender.FileTransferOut t) => t.Connection == CS$<>8__locals1.inc.Sender && t.ID == (int)transferId);
				if (matchingTransfer2 != null)
				{
					matchingTransfer2.Acknowledged = true;
					int expecting = CS$<>8__locals1.inc.ReadInt32();
					int lastSeen = Math.Min(matchingTransfer2.SentOffset, CS$<>8__locals1.inc.ReadInt32());
					matchingTransfer2.KnownReceivedOffset = Math.Max(expecting, matchingTransfer2.KnownReceivedOffset);
					if (matchingTransfer2.SentOffset < matchingTransfer2.KnownReceivedOffset)
					{
						matchingTransfer2.WaitTimer = 0f;
						matchingTransfer2.SentOffset = matchingTransfer2.KnownReceivedOffset;
					}
					if (lastSeen - matchingTransfer2.KnownReceivedOffset >= this.chunkLen * 10 || matchingTransfer2.SentOffset >= matchingTransfer2.Data.Length)
					{
						matchingTransfer2.SentOffset = matchingTransfer2.KnownReceivedOffset;
						matchingTransfer2.WaitTimer = 1f;
					}
					if (matchingTransfer2.KnownReceivedOffset >= matchingTransfer2.Data.Length)
					{
						matchingTransfer2.Status = FileTransferStatus.Finished;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Finished sending file \"");
						defaultInterpolatedStringHandler.AppendFormatted(matchingTransfer2.FilePath);
						defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
						defaultInterpolatedStringHandler.AppendFormatted(client.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\". Took ");
						defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(DateTime.Now - FileSender.StartTime);
						DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				return;
			}
			FileTransferType fileType = (FileTransferType)CS$<>8__locals1.inc.ReadByte();
			switch (fileType)
			{
			case FileTransferType.Submarine:
			{
				string fileName = CS$<>8__locals1.inc.ReadString();
				string fileHash = CS$<>8__locals1.inc.ReadString();
				SubmarineInfo requestedSubmarine = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == fileName && s.MD5Hash.StringRepresentation == fileHash);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(45, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Received a submarine file request from \"");
				defaultInterpolatedStringHandler2.AppendFormatted(client.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\" (");
				defaultInterpolatedStringHandler2.AppendFormatted(fileName);
				defaultInterpolatedStringHandler2.AppendLiteral(").");
				DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
				if (requestedSubmarine != null)
				{
					if (this.activeTransfers.Any((FileSender.FileTransferOut t) => t.Connection == CS$<>8__locals1.inc.Sender && t.FilePath == requestedSubmarine.FilePath))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(68, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Ignoring a submarine file request from \"");
						defaultInterpolatedStringHandler3.AppendFormatted(client.Name);
						defaultInterpolatedStringHandler3.AppendLiteral("\" (");
						defaultInterpolatedStringHandler3.AppendFormatted(fileName);
						defaultInterpolatedStringHandler3.AppendLiteral(") - already transferring.");
						DebugConsole.Log(defaultInterpolatedStringHandler3.ToStringAndClear());
						return;
					}
					this.StartTransfer(CS$<>8__locals1.inc.Sender, FileTransferType.Submarine, requestedSubmarine.FilePath);
					return;
				}
				break;
			}
			case FileTransferType.CampaignSave:
				if (GameMain.GameSession != null && !this.ActiveTransfers.Any((FileSender.FileTransferOut t) => t.Connection == CS$<>8__locals1.inc.Sender && t.FileType == FileTransferType.CampaignSave))
				{
					this.StartTransfer(CS$<>8__locals1.inc.Sender, FileTransferType.CampaignSave, GameMain.GameSession.DataPath.LoadPath);
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
					if (campaign != null)
					{
						client.LastCampaignSaveSendTime = new ValueTuple<ushort, float>(campaign.LastSaveID, (float)NetTime.Now);
						return;
					}
				}
				break;
			case FileTransferType.Mod:
			{
				string modName = CS$<>8__locals1.inc.ReadString();
				Md5Hash modHash = Md5Hash.StringAsHash(CS$<>8__locals1.inc.ReadString());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(39, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("Received a mod file request from \"");
				defaultInterpolatedStringHandler4.AppendFormatted(client.Name);
				defaultInterpolatedStringHandler4.AppendLiteral("\" (");
				defaultInterpolatedStringHandler4.AppendFormatted(modName);
				defaultInterpolatedStringHandler4.AppendLiteral(").");
				DebugConsole.Log(defaultInterpolatedStringHandler4.ToStringAndClear());
				if (!GameMain.Server.ServerSettings.AllowModDownloads)
				{
					return;
				}
				ModSender modSender = GameMain.Server.ModSender;
				if (modSender == null || !modSender.Ready)
				{
					return;
				}
				ContentPackage mod = ContentPackageManager.AllPackages.FirstOrDefault((ContentPackage p) => p.Hash.Equals(modHash));
				if (mod == null)
				{
					return;
				}
				string modCompressedPath = ModSender.GetCompressedModPath(mod);
				if (!File.Exists(modCompressedPath))
				{
					return;
				}
				if (this.activeTransfers.Any((FileSender.FileTransferOut t) => t.Connection == CS$<>8__locals1.inc.Sender && t.FilePath == modCompressedPath))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(62, 2);
					defaultInterpolatedStringHandler5.AppendLiteral("Ignoring a mod file request from \"");
					defaultInterpolatedStringHandler5.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler5.AppendLiteral("\" (");
					defaultInterpolatedStringHandler5.AppendFormatted(modName);
					defaultInterpolatedStringHandler5.AppendLiteral(") - already transferring.");
					DebugConsole.Log(defaultInterpolatedStringHandler5.ToStringAndClear());
					return;
				}
				this.StartTransfer(CS$<>8__locals1.inc.Sender, FileTransferType.Mod, modCompressedPath);
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x040019DE RID: 6622
		private const int MaxTransferCount = 16;

		// Token: 0x040019DF RID: 6623
		private const int MaxTransferCountPerRecipient = 5;

		// Token: 0x040019E0 RID: 6624
		public FileSender.FileTransferDelegate OnStarted;

		// Token: 0x040019E1 RID: 6625
		public FileSender.FileTransferDelegate OnEnded;

		// Token: 0x040019E2 RID: 6626
		private readonly List<FileSender.FileTransferOut> activeTransfers;

		// Token: 0x040019E3 RID: 6627
		private readonly int chunkLen;

		// Token: 0x040019E4 RID: 6628
		private readonly ServerPeer peer;

		// Token: 0x040019E5 RID: 6629
		public static DateTime StartTime;

		// Token: 0x02000BBC RID: 3004
		public class FileTransferOut
		{
			// Token: 0x170015FD RID: 5629
			// (get) Token: 0x060061B1 RID: 25009 RVA: 0x0020CC83 File Offset: 0x0020AE83
			// (set) Token: 0x060061B2 RID: 25010 RVA: 0x0020CC8B File Offset: 0x0020AE8B
			public string FileName { get; private set; }

			// Token: 0x170015FE RID: 5630
			// (get) Token: 0x060061B3 RID: 25011 RVA: 0x0020CC94 File Offset: 0x0020AE94
			// (set) Token: 0x060061B4 RID: 25012 RVA: 0x0020CC9C File Offset: 0x0020AE9C
			public string FilePath { get; private set; }

			// Token: 0x170015FF RID: 5631
			// (get) Token: 0x060061B5 RID: 25013 RVA: 0x0020CCA5 File Offset: 0x0020AEA5
			// (set) Token: 0x060061B6 RID: 25014 RVA: 0x0020CCAD File Offset: 0x0020AEAD
			public FileTransferType FileType { get; private set; }

			// Token: 0x17001600 RID: 5632
			// (get) Token: 0x060061B7 RID: 25015 RVA: 0x0020CCB6 File Offset: 0x0020AEB6
			public float Progress
			{
				get
				{
					return (float)this.KnownReceivedOffset / (float)this.Data.Length;
				}
			}

			// Token: 0x17001601 RID: 5633
			// (get) Token: 0x060061B8 RID: 25016 RVA: 0x0020CCC9 File Offset: 0x0020AEC9
			// (set) Token: 0x060061B9 RID: 25017 RVA: 0x0020CCD1 File Offset: 0x0020AED1
			public float WaitTimer
			{
				get
				{
					return this.waitTimer;
				}
				set
				{
					if (value > 0f)
					{
						this.PacketsPerUpdate = Math.Max(this.PacketsPerUpdate / 4f, 1f);
					}
					this.waitTimer = value;
				}
			}

			// Token: 0x17001602 RID: 5634
			// (get) Token: 0x060061BA RID: 25018 RVA: 0x0020CCFE File Offset: 0x0020AEFE
			// (set) Token: 0x060061BB RID: 25019 RVA: 0x0020CD06 File Offset: 0x0020AF06
			public float PacketsPerUpdate { get; set; } = 1f;

			// Token: 0x17001603 RID: 5635
			// (get) Token: 0x060061BC RID: 25020 RVA: 0x0020CD0F File Offset: 0x0020AF0F
			public byte[] Data { get; }

			// Token: 0x17001604 RID: 5636
			// (get) Token: 0x060061BD RID: 25021 RVA: 0x0020CD17 File Offset: 0x0020AF17
			// (set) Token: 0x060061BE RID: 25022 RVA: 0x0020CD1F File Offset: 0x0020AF1F
			public int SentOffset { get; set; }

			// Token: 0x17001605 RID: 5637
			// (get) Token: 0x060061BF RID: 25023 RVA: 0x0020CD28 File Offset: 0x0020AF28
			public NetworkConnection Connection { get; }

			// Token: 0x17001606 RID: 5638
			// (get) Token: 0x060061C0 RID: 25024 RVA: 0x0020CD30 File Offset: 0x0020AF30
			public DateTime StartingTime { get; }

			// Token: 0x060061C1 RID: 25025 RVA: 0x0020CD38 File Offset: 0x0020AF38
			public FileTransferOut(NetworkConnection recipient, FileTransferType fileType, string filePath)
			{
				this.Connection = recipient;
				this.FileType = fileType;
				this.FilePath = filePath;
				this.FileName = Path.GetFileName(filePath);
				this.Acknowledged = false;
				this.SentOffset = 0;
				this.KnownReceivedOffset = 0;
				this.Status = FileTransferStatus.NotStarted;
				this.StartingTime = DateTime.Now;
				int maxRetries = 4;
				for (int i = 0; i <= maxRetries; i++)
				{
					try
					{
						this.Data = File.ReadAllBytes(filePath, false);
					}
					catch (IOException e)
					{
						if (i >= maxRetries)
						{
							throw;
						}
						DebugConsole.NewMessage("Failed to initiate a file transfer {" + e.Message + "}, retrying in 250 ms...", new Color?(Color.Red), false);
						Thread.Sleep(250);
					}
				}
			}

			// Token: 0x04003A22 RID: 14882
			public FileTransferStatus Status;

			// Token: 0x04003A26 RID: 14886
			private float waitTimer;

			// Token: 0x04003A27 RID: 14887
			public static int MaxPacketsPerUpdate = 10;

			// Token: 0x04003A2A RID: 14890
			public bool Acknowledged;

			// Token: 0x04003A2C RID: 14892
			public int KnownReceivedOffset;

			// Token: 0x04003A2F RID: 14895
			public int ID;
		}

		// Token: 0x02000BBD RID: 3005
		// (Invoke) Token: 0x060061C4 RID: 25028
		public delegate void FileTransferDelegate(FileSender.FileTransferOut fileStreamReceiver);
	}
}
