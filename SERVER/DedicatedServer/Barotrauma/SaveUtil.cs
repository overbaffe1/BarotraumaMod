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
	// Token: 0x020002CD RID: 717
	[NullableContext(1)]
	[Nullable(0)]
	internal static class SaveUtil
	{
		// Token: 0x17000DDF RID: 3551
		// (get) Token: 0x0600303D RID: 12349 RVA: 0x0014B57C File Offset: 0x0014977C
		public static string TempPath
		{
			get
			{
				return Path.Combine(new string[]
				{
					SaveUtil.GetSaveFolder(SaveUtil.SaveType.Singleplayer),
					"temp_server"
				});
			}
		}

		// Token: 0x0600303E RID: 12350 RVA: 0x0014B59C File Offset: 0x0014979C
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

		// Token: 0x0600303F RID: 12351 RVA: 0x0014B5E8 File Offset: 0x001497E8
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

		// Token: 0x06003040 RID: 12352 RVA: 0x0014B884 File Offset: 0x00149A84
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

		// Token: 0x06003041 RID: 12353 RVA: 0x0014B920 File Offset: 0x00149B20
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

		// Token: 0x06003042 RID: 12354 RVA: 0x0014BA84 File Offset: 0x00149C84
		[NullableContext(2)]
		public static bool IsSaveFileCompatible(XDocument saveDoc)
		{
			return SaveUtil.IsSaveFileCompatible((saveDoc != null) ? saveDoc.Root : null);
		}

		// Token: 0x06003043 RID: 12355 RVA: 0x0014BA97 File Offset: 0x00149C97
		[NullableContext(2)]
		public static bool IsSaveFileCompatible(XElement saveDocRoot)
		{
			return ((saveDocRoot != null) ? saveDocRoot.Attribute("version") : null) != null;
		}

		// Token: 0x06003044 RID: 12356 RVA: 0x0014BAB4 File Offset: 0x00149CB4
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

		// Token: 0x06003045 RID: 12357 RVA: 0x0014BBB0 File Offset: 0x00149DB0
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

		// Token: 0x06003046 RID: 12358 RVA: 0x0014BC70 File Offset: 0x00149E70
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

		// Token: 0x06003047 RID: 12359 RVA: 0x0014BF20 File Offset: 0x0014A120
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

		// Token: 0x06003048 RID: 12360 RVA: 0x0014BFF4 File Offset: 0x0014A1F4
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

		// Token: 0x06003049 RID: 12361 RVA: 0x0014C080 File Offset: 0x0014A280
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

		// Token: 0x0600304A RID: 12362 RVA: 0x0014C12C File Offset: 0x0014A32C
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

		// Token: 0x0600304B RID: 12363 RVA: 0x0014C21C File Offset: 0x0014A41C
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

		// Token: 0x0600304C RID: 12364 RVA: 0x0014C2AC File Offset: 0x0014A4AC
		private static bool IsExtractionPathValid(string rootDir, string fileDir)
		{
			string rootDirFull = SaveUtil.<IsExtractionPathValid>g__getFullPath|30_0(rootDir);
			string fileDirFull = SaveUtil.<IsExtractionPathValid>g__getFullPath|30_0(fileDir);
			return fileDirFull.StartsWith(rootDirFull, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600304D RID: 12365 RVA: 0x0014C2D0 File Offset: 0x0014A4D0
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

		// Token: 0x0600304E RID: 12366 RVA: 0x0014C33C File Offset: 0x0014A53C
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

		// Token: 0x0600304F RID: 12367 RVA: 0x0014C4F0 File Offset: 0x0014A6F0
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

		// Token: 0x06003050 RID: 12368 RVA: 0x0014C5FC File Offset: 0x0014A7FC
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

		// Token: 0x06003051 RID: 12369 RVA: 0x0014C674 File Offset: 0x0014A874
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

		// Token: 0x06003052 RID: 12370 RVA: 0x0014C910 File Offset: 0x0014AB10
		public static void DeleteDownloadedSubs()
		{
			if (Directory.Exists(SaveUtil.SubmarineDownloadFolder))
			{
				SaveUtil.ClearFolder(SaveUtil.SubmarineDownloadFolder, null);
			}
		}

		// Token: 0x06003053 RID: 12371 RVA: 0x0014C92C File Offset: 0x0014AB2C
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

		// Token: 0x06003054 RID: 12372 RVA: 0x0014C980 File Offset: 0x0014AB80
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

		// Token: 0x06003055 RID: 12373 RVA: 0x0014CAA0 File Offset: 0x0014ACA0
		public static string FormatBackupExtension(uint index)
		{
			return string.Format(".save.bk{0}", index);
		}

		// Token: 0x06003056 RID: 12374 RVA: 0x0014CAB2 File Offset: 0x0014ACB2
		public static string FormatBackupCharacterDataExtension(uint index)
		{
			return string.Format(".xml.bk{0}", index);
		}

		// Token: 0x06003057 RID: 12375 RVA: 0x0014CAC4 File Offset: 0x0014ACC4
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

		// Token: 0x06003058 RID: 12376 RVA: 0x0014CB98 File Offset: 0x0014AD98
		public static void DeleteIfExists(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		// Token: 0x06003059 RID: 12377 RVA: 0x0014CBA8 File Offset: 0x0014ADA8
		[NullableContext(0)]
		public static ImmutableArray<SaveUtil.BackupIndexData> GetIndexData([Nullable(1)] string fullPath)
		{
			string path = Path.GetDirectoryName(fullPath) ?? "";
			string fileName = Path.GetFileNameWithoutExtension(fullPath);
			return SaveUtil.GetIndexData(path, fileName);
		}

		// Token: 0x0600305A RID: 12378 RVA: 0x0014CBD4 File Offset: 0x0014ADD4
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

		// Token: 0x0600305B RID: 12379 RVA: 0x0014CC28 File Offset: 0x0014AE28
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

		// Token: 0x0600305C RID: 12380 RVA: 0x0014CCA4 File Offset: 0x0014AEA4
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

		// Token: 0x0600305D RID: 12381 RVA: 0x0014CD90 File Offset: 0x0014AF90
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

		// Token: 0x0600305E RID: 12382 RVA: 0x0014CDD8 File Offset: 0x0014AFD8
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

		// Token: 0x06003060 RID: 12384 RVA: 0x0014CF1C File Offset: 0x0014B11C
		[CompilerGenerated]
		internal static void <SaveGame>g__LogErrorAndSendToClients|17_0(string errorMsg, Exception e)
		{
			DebugConsole.ThrowError(errorMsg, e, null, false, false);
			if (GameMain.Server != null)
			{
				foreach (Client client in GameMain.Server.ConnectedClients)
				{
					GameMain.Server.SendDirectChatMessage(errorMsg + "\n" + e.StackTrace.CleanupStackTrace(), client, ChatMessageType.Error);
				}
			}
		}

		// Token: 0x06003061 RID: 12385 RVA: 0x0014CF9C File Offset: 0x0014B19C
		[CompilerGenerated]
		internal static string <IsExtractionPathValid>g__getFullPath|30_0(string dir)
		{
			return (string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(dir)).CleanUpPathCrossPlatform(false, "");
		}

		// Token: 0x06003062 RID: 12386 RVA: 0x0014CFC0 File Offset: 0x0014B1C0
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

		// Token: 0x06003063 RID: 12387 RVA: 0x0014D0BF File Offset: 0x0014B2BF
		[CompilerGenerated]
		internal static void <BackupSave>g__BackupFile|42_1(string sourcePath, string destPath)
		{
			SaveUtil.DeleteIfExists(destPath);
			File.Copy(sourcePath, destPath, true);
			SaveUtil.SetHidden(destPath);
		}

		// Token: 0x0400182D RID: 6189
		public const string GameSessionFileName = "gamesession.xml";

		// Token: 0x0400182E RID: 6190
		private static readonly string LegacySaveFolder = Path.Combine(new string[]
		{
			"Data",
			"Saves"
		});

		// Token: 0x0400182F RID: 6191
		private static readonly string LegacyMultiplayerSaveFolder = Path.Combine(new string[]
		{
			SaveUtil.LegacySaveFolder,
			"Multiplayer"
		});

		// Token: 0x04001830 RID: 6192
		public static readonly string DefaultSaveFolder = Path.Combine(new string[]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Daedalic Entertainment GmbH",
			"Barotrauma"
		});

		// Token: 0x04001831 RID: 6193
		public static string DefaultMultiplayerSaveFolder = Path.Combine(new string[]
		{
			SaveUtil.DefaultSaveFolder,
			"Multiplayer"
		});

		// Token: 0x04001832 RID: 6194
		public static readonly string SubmarineDownloadFolder = Path.Combine(new string[]
		{
			"Submarines",
			"Downloaded"
		});

		// Token: 0x04001833 RID: 6195
		public static readonly string CampaignDownloadFolder = Path.Combine(new string[]
		{
			"Data",
			"Saves",
			"Multiplayer_Downloaded"
		});

		// Token: 0x04001834 RID: 6196
		public const string BackupExtension = ".bk";

		// Token: 0x04001835 RID: 6197
		public const string FullBackupExtension = ".save.bk";

		// Token: 0x04001836 RID: 6198
		public const string BackupExtensionFormat = ".save.bk{0}";

		// Token: 0x04001837 RID: 6199
		public const string BackupCharacterDataExtensionStart = ".xml.bk";

		// Token: 0x04001838 RID: 6200
		public const string BackupCharacterDataFormat = ".xml.bk{0}";

		// Token: 0x04001839 RID: 6201
		public static int MaxBackupCount = 3;

		// Token: 0x0400183A RID: 6202
		private static readonly EnumerationOptions BackupEnumerationOptions = new EnumerationOptions
		{
			MatchType = MatchType.Win32,
			AttributesToSkip = FileAttributes.System,
			IgnoreInaccessible = true
		};

		// Token: 0x02000B6B RID: 2923
		[NullableContext(0)]
		public enum SaveType
		{
			// Token: 0x0400396C RID: 14700
			Singleplayer,
			// Token: 0x0400396D RID: 14701
			Multiplayer
		}

		// Token: 0x02000B6C RID: 2924
		[NullableContext(0)]
		[NetworkSerialize(861)]
		public readonly struct BackupIndexData : INetSerializableStruct, IEquatable<SaveUtil.BackupIndexData>
		{
			// Token: 0x060060BF RID: 24767 RVA: 0x0020AD0E File Offset: 0x00208F0E
			public BackupIndexData(uint Index, Identifier LocationNameIdentifier, int LocationNameFormatIndex, Identifier LocationType, LevelData.LevelType LevelType, SerializableDateTime SaveTime)
			{
				this.Index = Index;
				this.LocationNameIdentifier = LocationNameIdentifier;
				this.LocationNameFormatIndex = LocationNameFormatIndex;
				this.LocationType = LocationType;
				this.LevelType = LevelType;
				this.SaveTime = SaveTime;
			}

			// Token: 0x170015EA RID: 5610
			// (get) Token: 0x060060C0 RID: 24768 RVA: 0x0020AD3D File Offset: 0x00208F3D
			// (set) Token: 0x060060C1 RID: 24769 RVA: 0x0020AD45 File Offset: 0x00208F45
			public uint Index { get; set; }

			// Token: 0x170015EB RID: 5611
			// (get) Token: 0x060060C2 RID: 24770 RVA: 0x0020AD4E File Offset: 0x00208F4E
			// (set) Token: 0x060060C3 RID: 24771 RVA: 0x0020AD56 File Offset: 0x00208F56
			public Identifier LocationNameIdentifier { get; set; }

			// Token: 0x170015EC RID: 5612
			// (get) Token: 0x060060C4 RID: 24772 RVA: 0x0020AD5F File Offset: 0x00208F5F
			// (set) Token: 0x060060C5 RID: 24773 RVA: 0x0020AD67 File Offset: 0x00208F67
			public int LocationNameFormatIndex { get; set; }

			// Token: 0x170015ED RID: 5613
			// (get) Token: 0x060060C6 RID: 24774 RVA: 0x0020AD70 File Offset: 0x00208F70
			// (set) Token: 0x060060C7 RID: 24775 RVA: 0x0020AD78 File Offset: 0x00208F78
			public Identifier LocationType { get; set; }

			// Token: 0x170015EE RID: 5614
			// (get) Token: 0x060060C8 RID: 24776 RVA: 0x0020AD81 File Offset: 0x00208F81
			// (set) Token: 0x060060C9 RID: 24777 RVA: 0x0020AD89 File Offset: 0x00208F89
			public LevelData.LevelType LevelType { get; set; }

			// Token: 0x170015EF RID: 5615
			// (get) Token: 0x060060CA RID: 24778 RVA: 0x0020AD92 File Offset: 0x00208F92
			// (set) Token: 0x060060CB RID: 24779 RVA: 0x0020AD9A File Offset: 0x00208F9A
			public SerializableDateTime SaveTime { get; set; }

			// Token: 0x060060CC RID: 24780 RVA: 0x0020ADA4 File Offset: 0x00208FA4
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

			// Token: 0x060060CD RID: 24781 RVA: 0x0020ADF0 File Offset: 0x00208FF0
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

			// Token: 0x060060CE RID: 24782 RVA: 0x0020AEE9 File Offset: 0x002090E9
			[CompilerGenerated]
			public static bool operator !=(SaveUtil.BackupIndexData left, SaveUtil.BackupIndexData right)
			{
				return !(left == right);
			}

			// Token: 0x060060CF RID: 24783 RVA: 0x0020AEF5 File Offset: 0x002090F5
			[CompilerGenerated]
			public static bool operator ==(SaveUtil.BackupIndexData left, SaveUtil.BackupIndexData right)
			{
				return left.Equals(right);
			}

			// Token: 0x060060D0 RID: 24784 RVA: 0x0020AF00 File Offset: 0x00209100
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((EqualityComparer<uint>.Default.GetHashCode(this.<Index>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationNameIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<LocationNameFormatIndex>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationType>k__BackingField)) * -1521134295 + EqualityComparer<LevelData.LevelType>.Default.GetHashCode(this.<LevelType>k__BackingField)) * -1521134295 + EqualityComparer<SerializableDateTime>.Default.GetHashCode(this.<SaveTime>k__BackingField);
			}

			// Token: 0x060060D1 RID: 24785 RVA: 0x0020AF90 File Offset: 0x00209190
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SaveUtil.BackupIndexData && this.Equals((SaveUtil.BackupIndexData)obj);
			}

			// Token: 0x060060D2 RID: 24786 RVA: 0x0020AFA8 File Offset: 0x002091A8
			[CompilerGenerated]
			public bool Equals(SaveUtil.BackupIndexData other)
			{
				return EqualityComparer<uint>.Default.Equals(this.<Index>k__BackingField, other.<Index>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LocationNameIdentifier>k__BackingField, other.<LocationNameIdentifier>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<LocationNameFormatIndex>k__BackingField, other.<LocationNameFormatIndex>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LocationType>k__BackingField, other.<LocationType>k__BackingField) && EqualityComparer<LevelData.LevelType>.Default.Equals(this.<LevelType>k__BackingField, other.<LevelType>k__BackingField) && EqualityComparer<SerializableDateTime>.Default.Equals(this.<SaveTime>k__BackingField, other.<SaveTime>k__BackingField);
			}

			// Token: 0x060060D3 RID: 24787 RVA: 0x0020B048 File Offset: 0x00209248
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
