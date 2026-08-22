using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000398 RID: 920
	[NullableContext(1)]
	[Nullable(0)]
	internal static class SaveUtil
	{
		// Token: 0x170011D4 RID: 4564
		// (get) Token: 0x060044BB RID: 17595 RVA: 0x00265040 File Offset: 0x00263240
		public static string TempPath
		{
			get
			{
				return Path.Combine(new string[]
				{
					SaveUtil.GetSaveFolder(SaveUtil.SaveType.Singleplayer),
					"temp"
				});
			}
		}

		// Token: 0x060044BC RID: 17596 RVA: 0x0026506C File Offset: 0x0026326C
		public static void EnsureSaveFolderExists()
		{
			try
			{
				Directory.CreateDirectory(SaveUtil.DefaultSaveFolder);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to create the default save folder \"" + SaveUtil.DefaultSaveFolder + "\"!", e, null, false, false);
			}
		}

		// Token: 0x060044BD RID: 17597 RVA: 0x002650B8 File Offset: 0x002632B8
		public static void SaveGame(CampaignDataPath filePath, bool isSavingOnLoading = false)
		{
			if (!isSavingOnLoading && File.Exists(filePath.SavePath))
			{
				SaveUtil.BackupSave(filePath.SavePath);
			}
			DebugConsole.Log("Saving the game to: " + filePath.ToString());
			Directory.CreateDirectory(SaveUtil.TempPath, true);
			try
			{
				SaveUtil.ClearFolder(SaveUtil.TempPath, new string[]
				{
					GameMain.GameSession.SubmarineInfo.FilePath
				});
			}
			catch (Exception e)
			{
				SaveUtil.<SaveGame>g__LogErrorAndSendToClients|17_0("Failed to clear folder", e);
				return;
			}
			try
			{
				GameMain.GameSession.Save(Path.Combine(new string[]
				{
					SaveUtil.TempPath,
					"gamesession.xml"
				}), isSavingOnLoading);
				if (!isSavingOnLoading)
				{
					GameMain.GameSession.DataPath = CampaignDataPath.CreateRegular(filePath.SavePath);
				}
			}
			catch (Exception e2)
			{
				SaveUtil.<SaveGame>g__LogErrorAndSendToClients|17_0("Error saving gamesession", e2);
				return;
			}
			try
			{
				string mainSubPath = null;
				if (GameMain.GameSession.SubmarineInfo != null)
				{
					mainSubPath = Path.Combine(new string[]
					{
						SaveUtil.TempPath,
						GameMain.GameSession.SubmarineInfo.Name + ".sub"
					});
					GameMain.GameSession.SubmarineInfo.SaveAs(mainSubPath, null);
					for (int i = 0; i < GameMain.GameSession.OwnedSubmarines.Count; i++)
					{
						if (GameMain.GameSession.OwnedSubmarines[i].Name == GameMain.GameSession.SubmarineInfo.Name)
						{
							GameMain.GameSession.OwnedSubmarines[i] = GameMain.GameSession.SubmarineInfo;
						}
					}
				}
				if (GameMain.GameSession.OwnedSubmarines != null)
				{
					for (int j = 0; j < GameMain.GameSession.OwnedSubmarines.Count; j++)
					{
						SubmarineInfo storedInfo = GameMain.GameSession.OwnedSubmarines[j];
						string subPath = Path.Combine(new string[]
						{
							SaveUtil.TempPath,
							storedInfo.Name + ".sub"
						});
						if (!(mainSubPath == subPath))
						{
							storedInfo.SaveAs(subPath, null);
						}
					}
				}
			}
			catch (Exception e3)
			{
				SaveUtil.<SaveGame>g__LogErrorAndSendToClients|17_0("Error saving submarine", e3);
				return;
			}
			try
			{
				SaveUtil.CompressDirectory(SaveUtil.TempPath, filePath.SavePath);
			}
			catch (Exception e4)
			{
				SaveUtil.<SaveGame>g__LogErrorAndSendToClients|17_0("Error compressing save file", e4);
			}
		}

		// Token: 0x060044BE RID: 17598 RVA: 0x00265354 File Offset: 0x00263554
		public static void LoadGame(CampaignDataPath path)
		{
			Submarine.Unload();
			GameMain.GameSession = null;
			DebugConsole.Log("Loading save file: " + path.LoadPath);
			SaveUtil.DecompressToDirectory(path.LoadPath, SaveUtil.TempPath);
			XDocument doc = XMLExtensions.TryLoadXml(Path.Combine(new string[]
			{
				SaveUtil.TempPath,
				"gamesession.xml"
			}));
			if (doc == null)
			{
				return;
			}
			if (!SaveUtil.IsSaveFileCompatible(doc))
			{
				throw new Exception("The save file \"" + path.LoadPath + "\" is not compatible with this version of Barotrauma.");
			}
			SubmarineInfo selectedSub;
			List<SubmarineInfo> ownedSubmarines = SaveUtil.LoadOwnedSubmarines(doc, out selectedSub);
			GameMain.GameSession = new GameSession(selectedSub, ownedSubmarines, doc, path);
		}

		// Token: 0x060044BF RID: 17599 RVA: 0x002653F0 File Offset: 0x002635F0
		public static List<SubmarineInfo> LoadOwnedSubmarines(XDocument saveDoc, out SubmarineInfo selectedSub)
		{
			string subPath = Path.Combine(new string[]
			{
				SaveUtil.TempPath,
				saveDoc.Root.GetAttributeString("submarine", "")
			}) + ".sub";
			selectedSub = new SubmarineInfo(subPath, "", null, true, false);
			List<SubmarineInfo> ownedSubmarines = new List<SubmarineInfo>();
			XElement root = saveDoc.Root;
			XElement ownedSubsElement = (root != null) ? root.Element("ownedsubmarines") : null;
			if (ownedSubsElement == null)
			{
				return ownedSubmarines;
			}
			foreach (XElement subElement in ownedSubsElement.Elements())
			{
				string subName = subElement.GetAttributeString("name", "");
				string ownedSubPath = Path.Combine(new string[]
				{
					SaveUtil.TempPath,
					subName + ".sub"
				});
				if (!File.Exists(ownedSubPath))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(115, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find the submarine \"");
					defaultInterpolatedStringHandler.AppendFormatted(subName);
					defaultInterpolatedStringHandler.AppendLiteral("\" (");
					defaultInterpolatedStringHandler.AppendFormatted(ownedSubPath);
					defaultInterpolatedStringHandler.AppendLiteral(")! The save file may be corrupted. Removing the submarine from owned submarines...");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					ownedSubmarines.Add(new SubmarineInfo(ownedSubPath, "", null, true, false));
				}
			}
			return ownedSubmarines;
		}

		// Token: 0x060044C0 RID: 17600 RVA: 0x00265554 File Offset: 0x00263754
		[NullableContext(2)]
		public static bool IsSaveFileCompatible(XDocument saveDoc)
		{
			return SaveUtil.IsSaveFileCompatible((saveDoc != null) ? saveDoc.Root : null);
		}

		// Token: 0x060044C1 RID: 17601 RVA: 0x00265567 File Offset: 0x00263767
		[NullableContext(2)]
		public static bool IsSaveFileCompatible(XElement saveDocRoot)
		{
			return ((saveDocRoot != null) ? saveDocRoot.Attribute("version") : null) != null;
		}

		// Token: 0x060044C2 RID: 17602 RVA: 0x00265584 File Offset: 0x00263784
		public static void DeleteSave(string filePath)
		{
			try
			{
				File.Delete(filePath, false);
				string[] backups = SaveUtil.GetBackupPaths(Path.GetDirectoryName(filePath) ?? "", Path.GetFileNameWithoutExtension(filePath));
				foreach (string backup in backups)
				{
					File.Delete(backup, false);
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("ERROR: deleting save file \"" + filePath + "\" failed.", e, null, false, false);
			}
			string fullPath = Path.GetFullPath(Path.GetDirectoryName(filePath) ?? "");
			if (fullPath.Equals(Path.GetFullPath(SaveUtil.DefaultMultiplayerSaveFolder)) || fullPath == Path.GetFullPath(SaveUtil.GetSaveFolder(SaveUtil.SaveType.Multiplayer)))
			{
				string characterDataSavePath = MultiPlayerCampaign.GetCharacterDataSavePath(filePath);
				if (File.Exists(characterDataSavePath))
				{
					try
					{
						File.Delete(characterDataSavePath, false);
					}
					catch (Exception e2)
					{
						DebugConsole.ThrowError("ERROR: deleting character data file \"" + characterDataSavePath + "\" failed.", e2, null, false, false);
					}
				}
			}
		}

		// Token: 0x060044C3 RID: 17603 RVA: 0x00265680 File Offset: 0x00263880
		public static string GetSaveFolder(SaveUtil.SaveType saveType)
		{
			string folder = string.Empty;
			if (!string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath))
			{
				folder = GameSettings.CurrentConfig.SavePath;
				if (saveType == SaveUtil.SaveType.Multiplayer)
				{
					folder = Path.Combine(new string[]
					{
						folder,
						"Multiplayer"
					});
				}
				if (!Directory.Exists(folder))
				{
					DebugConsole.AddWarning("Could not find the custom save folder \"" + folder + "\", creating the folder...", null);
					try
					{
						Directory.CreateDirectory(folder, false);
					}
					catch (Exception e)
					{
						DebugConsole.ThrowError("Could not find the custom save folder \"" + folder + "\". Using the default save path instead.", e, null, false, false);
						folder = string.Empty;
					}
				}
			}
			if (string.IsNullOrEmpty(folder))
			{
				folder = ((saveType == SaveUtil.SaveType.Singleplayer) ? SaveUtil.DefaultSaveFolder : SaveUtil.DefaultMultiplayerSaveFolder);
			}
			return folder;
		}

		// Token: 0x060044C4 RID: 17604 RVA: 0x00265740 File Offset: 0x00263940
		public static IReadOnlyList<CampaignMode.SaveInfo> GetSaveFiles(SaveUtil.SaveType saveType, bool includeInCompatible = true, bool logLoadErrors = true)
		{
			string defaultFolder = (saveType == SaveUtil.SaveType.Singleplayer) ? SaveUtil.DefaultSaveFolder : SaveUtil.DefaultMultiplayerSaveFolder;
			if (!Directory.Exists(defaultFolder))
			{
				DebugConsole.Log("Save folder \"" + defaultFolder + " not found! Attempting to create a new folder...");
				try
				{
					Directory.CreateDirectory(defaultFolder, false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to create the folder \"" + defaultFolder + "\"!", e, null, false, false);
				}
			}
			List<string> files = Directory.GetFiles(defaultFolder, "*.save", SearchOption.TopDirectoryOnly).ToList<string>();
			string folder = SaveUtil.GetSaveFolder(saveType);
			if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
			{
				files.AddRange(Directory.GetFiles(folder, "*.save", SearchOption.TopDirectoryOnly));
			}
			string legacyFolder = (saveType == SaveUtil.SaveType.Singleplayer) ? SaveUtil.LegacySaveFolder : SaveUtil.LegacyMultiplayerSaveFolder;
			if (Directory.Exists(legacyFolder))
			{
				files.AddRange(Directory.GetFiles(legacyFolder, "*.save", SearchOption.TopDirectoryOnly));
			}
			files = files.Distinct<string>().ToList<string>();
			List<CampaignMode.SaveInfo> saveInfos = new List<CampaignMode.SaveInfo>();
			foreach (string file in files)
			{
				XElement docRoot = SaveUtil.ExtractGameSessionRootElementFromSaveFile(file, logLoadErrors);
				if (includeInCompatible || SaveUtil.IsSaveFileCompatible(docRoot))
				{
					if (docRoot == null)
					{
						List<CampaignMode.SaveInfo> list = saveInfos;
						string filePath = file;
						Option.UnspecifiedNone none = Option.None;
						list.Add(new CampaignMode.SaveInfo(filePath, none, "", RespawnMode.None, ImmutableArray<string>.Empty));
					}
					else
					{
						List<string> enabledContentPackageNames = new List<string>();
						string enabledContentPackagePathsStr = docRoot.GetAttributeStringUnrestricted("selectedcontentpackages", string.Empty);
						foreach (string packagePath in enabledContentPackagePathsStr.Split('|', StringSplitOptions.None))
						{
							if (!string.IsNullOrEmpty(packagePath))
							{
								string fileName = Path.GetFileNameWithoutExtension(packagePath);
								if (fileName == "filelist")
								{
									enabledContentPackageNames.Add(Path.GetFileName(Path.GetDirectoryName(packagePath) ?? ""));
								}
								else
								{
									enabledContentPackageNames.Add(fileName);
								}
							}
						}
						string enabledContentPackageNamesStr = docRoot.GetAttributeStringUnrestricted("selectedcontentpackagenames", string.Empty);
						foreach (string packageName in Regex.Split(enabledContentPackageNamesStr, "(?<!(?<!\\\\)*\\\\)\\|"))
						{
							if (!string.IsNullOrEmpty(packageName))
							{
								enabledContentPackageNames.Add(packageName.Replace("\\|", "|"));
							}
						}
						saveInfos.Add(new CampaignMode.SaveInfo(file, docRoot.GetAttributeDateTime("savetime"), docRoot.GetAttributeStringUnrestricted("submarine", ""), docRoot.GetAttributeEnum("respawnmode", RespawnMode.None), enabledContentPackageNames.ToImmutableArray<string>()));
					}
				}
			}
			return saveInfos;
		}

		// Token: 0x060044C5 RID: 17605 RVA: 0x002659F0 File Offset: 0x00263BF0
		public static string CreateSavePath(SaveUtil.SaveType saveType, string fileName = "Save_Default")
		{
			fileName = ToolBox.RemoveInvalidFileNameChars(fileName);
			string folder = SaveUtil.GetSaveFolder(saveType);
			if (fileName == "Save_Default")
			{
				fileName = TextManager.Get("SaveFile.DefaultName").Value;
				if (fileName.Length == 0)
				{
					fileName = "Save";
				}
			}
			if (!Directory.Exists(folder))
			{
				DebugConsole.Log("Save folder \"" + folder + "\" not found. Created new folder");
				Directory.CreateDirectory(folder, true);
			}
			string extension = ".save";
			string pathWithoutExtension = Path.Combine(new string[]
			{
				folder,
				fileName
			});
			if (!File.Exists(pathWithoutExtension + extension))
			{
				return pathWithoutExtension + extension;
			}
			int i = 0;
			while (File.Exists(pathWithoutExtension + " " + i.ToString() + extension))
			{
				i++;
			}
			return pathWithoutExtension + " " + i.ToString() + extension;
		}

		// Token: 0x060044C6 RID: 17606 RVA: 0x00265AC4 File Offset: 0x00263CC4
		public static void CompressStringToFile(string fileName, string value)
		{
			byte[] b = Encoding.UTF8.GetBytes(value);
			FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, null, true);
			if (fileStream == null)
			{
				throw new Exception("Failed to create file \"" + fileName + "\"");
			}
			using (FileStream f2 = fileStream)
			{
				using (GZipStream gz = new GZipStream(f2, CompressionMode.Compress, false))
				{
					gz.Write(b, 0, b.Length);
				}
			}
		}

		// Token: 0x060044C7 RID: 17607 RVA: 0x00265B50 File Offset: 0x00263D50
		private static void CompressFile(string sDir, string sRelativePath, GZipStream zipStream)
		{
			if (sRelativePath.Length > 255)
			{
				throw new Exception("Failed to compress \"" + sDir + "\" (file name length > 255).");
			}
			zipStream.WriteByte((byte)sRelativePath.Length);
			zipStream.WriteByte(0);
			zipStream.WriteByte(0);
			zipStream.WriteByte(0);
			byte[] strBytes = Encoding.Unicode.GetBytes(sRelativePath.CleanUpPathCrossPlatform(false, ""));
			zipStream.Write(strBytes, 0, strBytes.Length);
			byte[] bytes = File.ReadAllBytes(Path.Combine(new string[]
			{
				sDir,
				sRelativePath
			}), true);
			zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
			zipStream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x060044C8 RID: 17608 RVA: 0x00265BFC File Offset: 0x00263DFC
		public static void CompressDirectory(string sInDir, string sOutFile)
		{
			IEnumerable<string> sFiles = Directory.GetFiles(sInDir, "*.*", SearchOption.AllDirectories);
			int iDirLen = (sInDir[sInDir.Length - 1] == Path.DirectorySeparatorChar) ? sInDir.Length : (sInDir.Length + 1);
			FileStream fileStream = File.Open(sOutFile, FileMode.Create, FileAccess.Write, null, true);
			if (fileStream == null)
			{
				throw new Exception("Failed to create file \"" + sOutFile + "\"");
			}
			using (FileStream outFile = fileStream)
			{
				using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
				{
					foreach (string sFilePath in sFiles)
					{
						string sRelativePath = sFilePath.Substring(iDirLen);
						SaveUtil.CompressFile(sInDir, sRelativePath, str);
					}
				}
			}
		}

		// Token: 0x060044C9 RID: 17609 RVA: 0x00265CEC File Offset: 0x00263EEC
		public static Stream DecompressFileToStream(string fileName)
		{
			FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, null, true);
			if (fileStream == null)
			{
				throw new Exception("Failed to open file \"" + fileName + "\"");
			}
			Stream result;
			using (FileStream originalFileStream = fileStream)
			{
				MemoryStream streamToReturn = new MemoryStream();
				using (GZipStream gzipStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
				{
					gzipStream.CopyTo(streamToReturn);
					streamToReturn.Position = 0L;
					result = streamToReturn;
				}
			}
			return result;
		}

		// Token: 0x060044CA RID: 17610 RVA: 0x00265D7C File Offset: 0x00263F7C
		private static bool IsExtractionPathValid(string rootDir, string fileDir)
		{
			string rootDirFull = SaveUtil.<IsExtractionPathValid>g__getFullPath|30_0(rootDir);
			string fileDirFull = SaveUtil.<IsExtractionPathValid>g__getFullPath|30_0(fileDir);
			return fileDirFull.StartsWith(rootDirFull, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060044CB RID: 17611 RVA: 0x00265DA0 File Offset: 0x00263FA0
		[NullableContext(2)]
		private static bool DecompressFile([Nullable(1)] BinaryReader reader, [NotNullWhen(true)] out string fileName, [NotNullWhen(true)] out byte[] fileContent)
		{
			fileName = null;
			fileContent = null;
			if (reader.PeekChar() < 0)
			{
				return false;
			}
			int nameLen = reader.ReadInt32();
			if (nameLen > 255)
			{
				throw new Exception("Failed to decompress (file name length > 255). The file may be corrupted.");
			}
			byte[] strBytes = reader.ReadBytes(nameLen * 2);
			string sFileName = Encoding.Unicode.GetString(strBytes).Replace('\\', '/');
			fileName = sFileName;
			int contentLen = reader.ReadInt32();
			fileContent = reader.ReadBytes(contentLen);
			return true;
		}

		// Token: 0x060044CC RID: 17612 RVA: 0x00265E0C File Offset: 0x0026400C
		public static void DecompressToDirectory(string sCompressedFile, string sDir)
		{
			DebugConsole.Log(string.Concat(new string[]
			{
				"Decompressing ",
				sCompressedFile,
				" to ",
				sDir,
				"..."
			}));
			for (int i = 0; i <= 4; i++)
			{
				try
				{
					using (Stream memStream = SaveUtil.DecompressFileToStream(sCompressedFile))
					{
						using (BinaryReader reader = new BinaryReader(memStream))
						{
							string fileName;
							byte[] contentBytes;
							while (SaveUtil.DecompressFile(reader, out fileName, out contentBytes))
							{
								string sFilePath = Path.Combine(new string[]
								{
									sDir,
									fileName
								});
								string sFinalDir = Path.GetDirectoryName(sFilePath) ?? "";
								if (!SaveUtil.IsExtractionPathValid(sDir, sFinalDir))
								{
									throw new InvalidOperationException("Error extracting \"" + fileName + "\": cannot be extracted to parent directory");
								}
								Directory.CreateDirectory(sFinalDir, false);
								FileStream fileStream = File.Open(sFilePath, FileMode.Create, FileAccess.Write, null, true);
								if (fileStream == null)
								{
									throw new Exception("Failed to create file \"" + sFilePath + "\"");
								}
								using (FileStream outFile = fileStream)
								{
									outFile.Write(contentBytes, 0, contentBytes.Length);
									continue;
								}
								break;
							}
							break;
						}
					}
				}
				catch (IOException e)
				{
					if (i >= 4 || !File.Exists(sCompressedFile))
					{
						throw;
					}
					DebugConsole.NewMessage(string.Concat(new string[]
					{
						"Failed decompress file \"",
						sCompressedFile,
						"\" {",
						e.Message,
						"}, retrying in 250 ms..."
					}), new Color?(Color.Red), false);
					Thread.Sleep(250);
				}
			}
		}

		// Token: 0x060044CD RID: 17613 RVA: 0x00265FC0 File Offset: 0x002641C0
		public static IEnumerable<string> EnumerateContainedFiles(string sCompressedFile)
		{
			HashSet<string> paths = new HashSet<string>();
			for (int i = 0; i <= 4; i++)
			{
				try
				{
					paths.Clear();
					using (Stream memStream = SaveUtil.DecompressFileToStream(sCompressedFile))
					{
						using (BinaryReader reader = new BinaryReader(memStream))
						{
							string fileName;
							byte[] array;
							while (SaveUtil.DecompressFile(reader, out fileName, out array))
							{
								paths.Add(fileName);
							}
							break;
						}
					}
				}
				catch (IOException e)
				{
					if (i >= 4 || !File.Exists(sCompressedFile))
					{
						throw;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to decompress file \"");
					defaultInterpolatedStringHandler.AppendFormatted(sCompressedFile);
					defaultInterpolatedStringHandler.AppendLiteral("\" for enumeration {");
					defaultInterpolatedStringHandler.AppendFormatted(e.Message);
					defaultInterpolatedStringHandler.AppendLiteral("}, retrying in 250 ms...");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
					Thread.Sleep(250);
				}
			}
			return paths;
		}

		// Token: 0x060044CE RID: 17614 RVA: 0x002660CC File Offset: 0x002642CC
		[return: Nullable(2)]
		public static XDocument DecompressSaveAndLoadGameSessionDoc(string savePath)
		{
			DebugConsole.Log("Loading game session doc: " + savePath);
			try
			{
				SaveUtil.DecompressToDirectory(savePath, SaveUtil.TempPath);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error decompressing " + savePath, e, null, false, false);
				return null;
			}
			return XMLExtensions.TryLoadXml(Path.Combine(new string[]
			{
				SaveUtil.TempPath,
				"gamesession.xml"
			}));
		}

		// Token: 0x060044CF RID: 17615 RVA: 0x00266144 File Offset: 0x00264344
		[return: Nullable(2)]
		public static XElement ExtractGameSessionRootElementFromSaveFile(string savePath, bool logLoadErrors = true)
		{
			for (int i = 0; i <= 4; i++)
			{
				try
				{
					using (Stream memStream = SaveUtil.DecompressFileToStream(savePath))
					{
						using (BinaryReader reader = new BinaryReader(memStream))
						{
							string fileName;
							byte[] fileContent;
							while (SaveUtil.DecompressFile(reader, out fileName, out fileContent))
							{
								if (!(fileName != "gamesession.xml"))
								{
									int tagOpenerStartIndex = -1;
									for (int j = 0; j < fileContent.Length; j++)
									{
										if (fileContent[j] == 60)
										{
											if (tagOpenerStartIndex >= 0)
											{
												return null;
											}
											tagOpenerStartIndex = j;
										}
										else if (j > 0 && fileContent[j] == 63 && fileContent[j - 1] == 60)
										{
											tagOpenerStartIndex = -1;
										}
										else if (fileContent[j] == 62 && tagOpenerStartIndex >= 0)
										{
											Encoding utf = Encoding.UTF8;
											Span<byte> span = fileContent.AsSpan<byte>();
											int num = tagOpenerStartIndex;
											string elemStr = utf.GetString(span.Slice(num, j - num)) + "/>";
											try
											{
												return XElement.Parse(elemStr);
											}
											catch (Exception e)
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
												defaultInterpolatedStringHandler.AppendLiteral("Failed to parse gamesession root in \"");
												defaultInterpolatedStringHandler.AppendFormatted(savePath);
												defaultInterpolatedStringHandler.AppendLiteral("\": {");
												defaultInterpolatedStringHandler.AppendFormatted(e.Message);
												defaultInterpolatedStringHandler.AppendLiteral("}.");
												DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
												return null;
											}
										}
									}
								}
							}
							break;
						}
					}
				}
				catch (IOException e2)
				{
					if (i >= 4 || !File.Exists(savePath))
					{
						throw;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(74, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to decompress file \"");
					defaultInterpolatedStringHandler2.AppendFormatted(savePath);
					defaultInterpolatedStringHandler2.AppendLiteral("\" for root extraction (");
					defaultInterpolatedStringHandler2.AppendFormatted(e2.Message);
					defaultInterpolatedStringHandler2.AppendLiteral("), retrying in 250 ms...");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Red), false);
					Thread.Sleep(250);
				}
				catch (InvalidDataException e3)
				{
					if (logLoadErrors)
					{
						DebugConsole.ThrowError("Failed to decompress file \"" + savePath + "\" for root extraction.", e3, null, false, false);
					}
					return null;
				}
			}
			return null;
		}

		// Token: 0x060044D0 RID: 17616 RVA: 0x002663E0 File Offset: 0x002645E0
		public static void DeleteDownloadedSubs()
		{
			if (Directory.Exists(SaveUtil.SubmarineDownloadFolder))
			{
				SaveUtil.ClearFolder(SaveUtil.SubmarineDownloadFolder, null);
			}
		}

		// Token: 0x060044D1 RID: 17617 RVA: 0x002663FC File Offset: 0x002645FC
		public static void CleanUnnecessarySaveFiles()
		{
			if (Directory.Exists(SaveUtil.CampaignDownloadFolder))
			{
				SaveUtil.ClearFolder(SaveUtil.CampaignDownloadFolder, null);
				Directory.Delete(SaveUtil.CampaignDownloadFolder, true, true);
			}
			if (Directory.Exists(SaveUtil.TempPath))
			{
				SaveUtil.ClearFolder(SaveUtil.TempPath, null);
				Directory.Delete(SaveUtil.TempPath, true, true);
			}
		}

		// Token: 0x060044D2 RID: 17618 RVA: 0x00266450 File Offset: 0x00264650
		public static void ClearFolder(string folderName, [Nullable(new byte[]
		{
			2,
			1
		})] string[] ignoredFileNames = null)
		{
			DirectoryInfo dir = new DirectoryInfo(folderName);
			foreach (FileInfo fi in dir.GetFiles())
			{
				if (ignoredFileNames != null)
				{
					bool ignore = false;
					foreach (string ignoredFile in ignoredFileNames)
					{
						if (Path.GetFileName(fi.FullName).Equals(Path.GetFileName(ignoredFile)))
						{
							ignore = true;
							break;
						}
					}
					if (ignore)
					{
						continue;
					}
				}
				fi.IsReadOnly = false;
				fi.Delete();
			}
			foreach (DirectoryInfo di in dir.GetDirectories())
			{
				SaveUtil.ClearFolder(di.FullName, ignoredFileNames);
				for (int i = 0; i <= 4; i++)
				{
					try
					{
						di.Delete();
						break;
					}
					catch (IOException)
					{
						if (i >= 4)
						{
							throw;
						}
						Thread.Sleep(250);
					}
				}
			}
		}

		// Token: 0x060044D3 RID: 17619 RVA: 0x00266570 File Offset: 0x00264770
		public static string FormatBackupExtension(uint index)
		{
			return string.Format(".save.bk{0}", index);
		}

		// Token: 0x060044D4 RID: 17620 RVA: 0x00266582 File Offset: 0x00264782
		public static string FormatBackupCharacterDataExtension(uint index)
		{
			return string.Format(".xml.bk{0}", index);
		}

		// Token: 0x060044D5 RID: 17621 RVA: 0x00266594 File Offset: 0x00264794
		public static void BackupSave(string savePath)
		{
			string path = Path.GetDirectoryName(savePath) ?? "";
			string fileName = Path.GetFileNameWithoutExtension(savePath);
			string characterDataSavePath = MultiPlayerCampaign.GetCharacterDataSavePath(savePath);
			string characterDataFileName = Path.GetFileNameWithoutExtension(characterDataSavePath);
			ImmutableArray<SaveUtil.BackupIndexData> indexData = SaveUtil.GetIndexData(path, fileName);
			uint freeIndex = SaveUtil.<BackupSave>g__GetFreeIndex|42_0(indexData);
			string newBackupPath = Path.Combine(new string[]
			{
				path,
				"." + fileName + SaveUtil.FormatBackupExtension(freeIndex)
			});
			string newCharacterDataBackupPath = Path.Combine(new string[]
			{
				path,
				"." + characterDataFileName + SaveUtil.FormatBackupCharacterDataExtension(freeIndex)
			});
			try
			{
				SaveUtil.<BackupSave>g__BackupFile|42_1(savePath, newBackupPath);
				if (File.Exists(characterDataSavePath))
				{
					SaveUtil.<BackupSave>g__BackupFile|42_1(characterDataSavePath, newCharacterDataBackupPath);
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to create a backup of the save file.", e, null, false, false);
			}
		}

		// Token: 0x060044D6 RID: 17622 RVA: 0x00266668 File Offset: 0x00264868
		public static void DeleteIfExists(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		// Token: 0x060044D7 RID: 17623 RVA: 0x00266678 File Offset: 0x00264878
		[NullableContext(0)]
		public static ImmutableArray<SaveUtil.BackupIndexData> GetIndexData([Nullable(1)] string fullPath)
		{
			string path = Path.GetDirectoryName(fullPath) ?? "";
			string fileName = Path.GetFileNameWithoutExtension(fullPath);
			return SaveUtil.GetIndexData(path, fileName);
		}

		// Token: 0x060044D8 RID: 17624 RVA: 0x002666A4 File Offset: 0x002648A4
		private static string[] GetBackupPaths(string path, string baseName)
		{
			try
			{
				return Directory.GetFiles(path, "." + baseName + ".save.bk*", SaveUtil.BackupEnumerationOptions);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to get backup paths.", e, null, false, false);
			}
			return Array.Empty<string>();
		}

		// Token: 0x060044D9 RID: 17625 RVA: 0x002666F8 File Offset: 0x002648F8
		public static bool TryGetBackupIndexFromFileName(string filePath, out uint index)
		{
			string extension = Path.GetExtension(filePath);
			if (extension.Length < ".bk".Length)
			{
				DebugConsole.ThrowError("The file name \"" + filePath + "\" does not have a valid backup extension.", null, null, false, false);
				index = 0U;
				return false;
			}
			string indexStr = extension.Substring(".bk".Length);
			bool result = uint.TryParse(indexStr, out index);
			if (!result)
			{
				DebugConsole.ThrowError("Failed to parse the backup index from the file name \"" + filePath + "\".", null, null, false, false);
			}
			return result;
		}

		// Token: 0x060044DA RID: 17626 RVA: 0x00266774 File Offset: 0x00264974
		[return: Nullable(0)]
		private static ImmutableArray<SaveUtil.BackupIndexData> GetIndexData(string path, string baseName)
		{
			ImmutableArray<SaveUtil.BackupIndexData>.Builder builder = ImmutableArray.CreateBuilder<SaveUtil.BackupIndexData>();
			string[] foundBackups = SaveUtil.GetBackupPaths(path, baseName);
			foreach (string backupPath in foundBackups)
			{
				uint index;
				if (SaveUtil.TryGetBackupIndexFromFileName(backupPath, out index))
				{
					XElement gameSession = SaveUtil.ExtractGameSessionRootElementFromSaveFile(backupPath, false);
					if (gameSession == null)
					{
						DebugConsole.AddWarning("Failed to load gamesession root from \"" + backupPath + "\". Skipping this backup.", null);
					}
					else
					{
						SerializableDateTime saveTime = gameSession.GetAttributeDateTime("savetime").Fallback(SerializableDateTime.FromUtcUnixTime(0L));
						Identifier locationNameIdentifier = gameSession.GetAttributeIdentifier("currentlocation", Identifier.Empty);
						int locationNameFormatIndex = gameSession.GetAttributeInt("currentlocationnameformatindex", -1);
						Identifier locationType = gameSession.GetAttributeIdentifier("locationtype", Identifier.Empty);
						LevelData.LevelType levelType = gameSession.GetAttributeEnum("nextleveltype", LevelData.LevelType.LocationConnection);
						builder.Add(new SaveUtil.BackupIndexData(index, locationNameIdentifier, locationNameFormatIndex, locationType, levelType, saveTime));
					}
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x060044DB RID: 17627 RVA: 0x00266860 File Offset: 0x00264A60
		public static string GetBackupPath(string savePath, uint index)
		{
			string path = Path.GetDirectoryName(savePath) ?? "";
			string fileName = Path.GetFileNameWithoutExtension(savePath);
			return Path.Combine(new string[]
			{
				path,
				"." + fileName + SaveUtil.FormatBackupExtension(index)
			});
		}

		// Token: 0x060044DC RID: 17628 RVA: 0x002668A8 File Offset: 0x00264AA8
		private static void SetHidden(string filePath)
		{
			try
			{
				File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to set the backup file as hidden.", e, null, false, false);
			}
		}

		// Token: 0x060044DE RID: 17630 RVA: 0x002669EC File Offset: 0x00264BEC
		[CompilerGenerated]
		internal static void <SaveGame>g__LogErrorAndSendToClients|17_0(string errorMsg, Exception e)
		{
			DebugConsole.ThrowError(errorMsg, e, null, false, false);
		}

		// Token: 0x060044DF RID: 17631 RVA: 0x002669F8 File Offset: 0x00264BF8
		[CompilerGenerated]
		internal static string <IsExtractionPathValid>g__getFullPath|30_0(string dir)
		{
			return (string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(dir)).CleanUpPathCrossPlatform(false, "");
		}

		// Token: 0x060044E0 RID: 17632 RVA: 0x00266A1C File Offset: 0x00264C1C
		[CompilerGenerated]
		internal static uint <BackupSave>g__GetFreeIndex|42_0(IEnumerable<SaveUtil.BackupIndexData> indexData)
		{
			if (!indexData.Any<SaveUtil.BackupIndexData>())
			{
				return 0U;
			}
			if (indexData.Count<SaveUtil.BackupIndexData>() >= SaveUtil.MaxBackupCount)
			{
				return (from b in indexData
				orderby b.SaveTime
				select b).First<SaveUtil.BackupIndexData>().Index;
			}
			uint highestIndex = indexData.Max((SaveUtil.BackupIndexData b) => b.Index);
			uint nextIndex = highestIndex + 1U;
			if (indexData.Any((SaveUtil.BackupIndexData b) => b.Index == nextIndex))
			{
				uint i = 0U;
				while ((ulong)i < (ulong)((long)SaveUtil.MaxBackupCount))
				{
					if (indexData.All((SaveUtil.BackupIndexData b) => b.Index != i))
					{
						return i;
					}
					uint j = i;
					i = j + 1U;
				}
				throw new InvalidOperationException("Failed to find a free index for the backup.");
			}
			return nextIndex;
		}

		// Token: 0x060044E1 RID: 17633 RVA: 0x00266B1B File Offset: 0x00264D1B
		[CompilerGenerated]
		internal static void <BackupSave>g__BackupFile|42_1(string sourcePath, string destPath)
		{
			SaveUtil.DeleteIfExists(destPath);
			File.Copy(sourcePath, destPath, true);
			SaveUtil.SetHidden(destPath);
		}

		// Token: 0x040023FF RID: 9215
		public const string GameSessionFileName = "gamesession.xml";

		// Token: 0x04002400 RID: 9216
		private static readonly string LegacySaveFolder = Path.Combine(new string[]
		{
			"Data",
			"Saves"
		});

		// Token: 0x04002401 RID: 9217
		private static readonly string LegacyMultiplayerSaveFolder = Path.Combine(new string[]
		{
			SaveUtil.LegacySaveFolder,
			"Multiplayer"
		});

		// Token: 0x04002402 RID: 9218
		public static readonly string DefaultSaveFolder = Path.Combine(new string[]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Daedalic Entertainment GmbH",
			"Barotrauma"
		});

		// Token: 0x04002403 RID: 9219
		public static string DefaultMultiplayerSaveFolder = Path.Combine(new string[]
		{
			SaveUtil.DefaultSaveFolder,
			"Multiplayer"
		});

		// Token: 0x04002404 RID: 9220
		public static readonly string SubmarineDownloadFolder = Path.Combine(new string[]
		{
			"Submarines",
			"Downloaded"
		});

		// Token: 0x04002405 RID: 9221
		public static readonly string CampaignDownloadFolder = Path.Combine(new string[]
		{
			"Data",
			"Saves",
			"Multiplayer_Downloaded"
		});

		// Token: 0x04002406 RID: 9222
		public const string BackupExtension = ".bk";

		// Token: 0x04002407 RID: 9223
		public const string FullBackupExtension = ".save.bk";

		// Token: 0x04002408 RID: 9224
		public const string BackupExtensionFormat = ".save.bk{0}";

		// Token: 0x04002409 RID: 9225
		public const string BackupCharacterDataExtensionStart = ".xml.bk";

		// Token: 0x0400240A RID: 9226
		public const string BackupCharacterDataFormat = ".xml.bk{0}";

		// Token: 0x0400240B RID: 9227
		public static int MaxBackupCount = 3;

		// Token: 0x0400240C RID: 9228
		private static readonly EnumerationOptions BackupEnumerationOptions = new EnumerationOptions
		{
			MatchType = MatchType.Win32,
			AttributesToSkip = FileAttributes.System,
			IgnoreInaccessible = true
		};

		// Token: 0x020010BC RID: 4284
		[NullableContext(0)]
		public enum SaveType
		{
			// Token: 0x0400598A RID: 22922
			Singleplayer,
			// Token: 0x0400598B RID: 22923
			Multiplayer
		}

		// Token: 0x020010BD RID: 4285
		[NullableContext(0)]
		[NetworkSerialize(861)]
		public readonly struct BackupIndexData : INetSerializableStruct, IEquatable<SaveUtil.BackupIndexData>
		{
			// Token: 0x06008DA5 RID: 36261 RVA: 0x003B25A2 File Offset: 0x003B07A2
			public BackupIndexData(uint Index, Identifier LocationNameIdentifier, int LocationNameFormatIndex, Identifier LocationType, LevelData.LevelType LevelType, SerializableDateTime SaveTime)
			{
				this.Index = Index;
				this.LocationNameIdentifier = LocationNameIdentifier;
				this.LocationNameFormatIndex = LocationNameFormatIndex;
				this.LocationType = LocationType;
				this.LevelType = LevelType;
				this.SaveTime = SaveTime;
			}

			// Token: 0x17001C82 RID: 7298
			// (get) Token: 0x06008DA6 RID: 36262 RVA: 0x003B25D1 File Offset: 0x003B07D1
			// (set) Token: 0x06008DA7 RID: 36263 RVA: 0x003B25D9 File Offset: 0x003B07D9
			public uint Index { get; set; }

			// Token: 0x17001C83 RID: 7299
			// (get) Token: 0x06008DA8 RID: 36264 RVA: 0x003B25E2 File Offset: 0x003B07E2
			// (set) Token: 0x06008DA9 RID: 36265 RVA: 0x003B25EA File Offset: 0x003B07EA
			public Identifier LocationNameIdentifier { get; set; }

			// Token: 0x17001C84 RID: 7300
			// (get) Token: 0x06008DAA RID: 36266 RVA: 0x003B25F3 File Offset: 0x003B07F3
			// (set) Token: 0x06008DAB RID: 36267 RVA: 0x003B25FB File Offset: 0x003B07FB
			public int LocationNameFormatIndex { get; set; }

			// Token: 0x17001C85 RID: 7301
			// (get) Token: 0x06008DAC RID: 36268 RVA: 0x003B2604 File Offset: 0x003B0804
			// (set) Token: 0x06008DAD RID: 36269 RVA: 0x003B260C File Offset: 0x003B080C
			public Identifier LocationType { get; set; }

			// Token: 0x17001C86 RID: 7302
			// (get) Token: 0x06008DAE RID: 36270 RVA: 0x003B2615 File Offset: 0x003B0815
			// (set) Token: 0x06008DAF RID: 36271 RVA: 0x003B261D File Offset: 0x003B081D
			public LevelData.LevelType LevelType { get; set; }

			// Token: 0x17001C87 RID: 7303
			// (get) Token: 0x06008DB0 RID: 36272 RVA: 0x003B2626 File Offset: 0x003B0826
			// (set) Token: 0x06008DB1 RID: 36273 RVA: 0x003B262E File Offset: 0x003B082E
			public SerializableDateTime SaveTime { get; set; }

			// Token: 0x06008DB2 RID: 36274 RVA: 0x003B2638 File Offset: 0x003B0838
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("BackupIndexData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06008DB3 RID: 36275 RVA: 0x003B2684 File Offset: 0x003B0884
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Index = ");
				builder.Append(this.Index.ToString());
				builder.Append(", LocationNameIdentifier = ");
				builder.Append(this.LocationNameIdentifier.ToString());
				builder.Append(", LocationNameFormatIndex = ");
				builder.Append(this.LocationNameFormatIndex.ToString());
				builder.Append(", LocationType = ");
				builder.Append(this.LocationType.ToString());
				builder.Append(", LevelType = ");
				builder.Append(this.LevelType.ToString());
				builder.Append(", SaveTime = ");
				builder.Append(this.SaveTime.ToString());
				return true;
			}

			// Token: 0x06008DB4 RID: 36276 RVA: 0x003B277D File Offset: 0x003B097D
			[CompilerGenerated]
			public static bool operator !=(SaveUtil.BackupIndexData left, SaveUtil.BackupIndexData right)
			{
				return !(left == right);
			}

			// Token: 0x06008DB5 RID: 36277 RVA: 0x003B2789 File Offset: 0x003B0989
			[CompilerGenerated]
			public static bool operator ==(SaveUtil.BackupIndexData left, SaveUtil.BackupIndexData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008DB6 RID: 36278 RVA: 0x003B2794 File Offset: 0x003B0994
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((EqualityComparer<uint>.Default.GetHashCode(this.<Index>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationNameIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<LocationNameFormatIndex>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationType>k__BackingField)) * -1521134295 + EqualityComparer<LevelData.LevelType>.Default.GetHashCode(this.<LevelType>k__BackingField)) * -1521134295 + EqualityComparer<SerializableDateTime>.Default.GetHashCode(this.<SaveTime>k__BackingField);
			}

			// Token: 0x06008DB7 RID: 36279 RVA: 0x003B2824 File Offset: 0x003B0A24
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SaveUtil.BackupIndexData && this.Equals((SaveUtil.BackupIndexData)obj);
			}

			// Token: 0x06008DB8 RID: 36280 RVA: 0x003B283C File Offset: 0x003B0A3C
			[CompilerGenerated]
			public bool Equals(SaveUtil.BackupIndexData other)
			{
				return EqualityComparer<uint>.Default.Equals(this.<Index>k__BackingField, other.<Index>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LocationNameIdentifier>k__BackingField, other.<LocationNameIdentifier>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<LocationNameFormatIndex>k__BackingField, other.<LocationNameFormatIndex>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LocationType>k__BackingField, other.<LocationType>k__BackingField) && EqualityComparer<LevelData.LevelType>.Default.Equals(this.<LevelType>k__BackingField, other.<LevelType>k__BackingField) && EqualityComparer<SerializableDateTime>.Default.Equals(this.<SaveTime>k__BackingField, other.<SaveTime>k__BackingField);
			}

			// Token: 0x06008DB9 RID: 36281 RVA: 0x003B28DC File Offset: 0x003B0ADC
			[CompilerGenerated]
			public void Deconstruct(out uint Index, out Identifier LocationNameIdentifier, out int LocationNameFormatIndex, out Identifier LocationType, out LevelData.LevelType LevelType, out SerializableDateTime SaveTime)
			{
				Index = this.Index;
				LocationNameIdentifier = this.LocationNameIdentifier;
				LocationNameFormatIndex = this.LocationNameFormatIndex;
				LocationType = this.LocationType;
				LevelType = this.LevelType;
				SaveTime = this.SaveTime;
			}
		}
	}
}
