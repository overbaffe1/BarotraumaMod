using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;
using Barotrauma.IO;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002F4 RID: 756
	public static class ModUtils
	{
		// Token: 0x06003DD5 RID: 15829 RVA: 0x002314D4 File Offset: 0x0022F6D4
		public static V TryGetOrSet<K, V>(this IDictionary<K, V> dict, K key, Func<V> valueFactory) where K : IEquatable<K>
		{
			V dictValue;
			if (dict.TryGetValue(key, out dictValue))
			{
				return dictValue;
			}
			if (valueFactory != null)
			{
				dict.Add(key, valueFactory());
				return dict[key];
			}
			return default(V);
		}

		// Token: 0x02000F99 RID: 3993
		public static class ItemPrefab
		{
			// Token: 0x060089B5 RID: 35253 RVA: 0x003A88F8 File Offset: 0x003A6AF8
			internal static Barotrauma.ItemPrefab GetItemPrefab(string itemNameOrId)
			{
				return (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as Barotrauma.ItemPrefab;
			}
		}

		// Token: 0x02000F9A RID: 3994
		public static class Client
		{
			// Token: 0x060089B6 RID: 35254 RVA: 0x003A8920 File Offset: 0x003A6B20
			internal static ulong GetSteamId(Barotrauma.Networking.Client client)
			{
				AccountId outValue;
				if (client.AccountId.TryUnwrap(out outValue))
				{
					SteamId steamId = outValue as SteamId;
					if (steamId != null)
					{
						return steamId.Value;
					}
				}
				return 0UL;
			}

			// Token: 0x17001C37 RID: 7223
			// (get) Token: 0x060089B7 RID: 35255 RVA: 0x003A8952 File Offset: 0x003A6B52
			internal static IReadOnlyList<Barotrauma.Networking.Client> ClientList
			{
				get
				{
					if (GameMain.IsSingleplayer)
					{
						return new List<Barotrauma.Networking.Client>();
					}
					return GameMain.Client.ConnectedClients;
				}
			}
		}

		// Token: 0x02000F9B RID: 3995
		public static class Definitions
		{
			// Token: 0x0400562E RID: 22062
			public const string LuaCsForBarotrauma = "LuaCsForBarotrauma";
		}

		// Token: 0x02000F9C RID: 3996
		public static class Environment
		{
			// Token: 0x060089B8 RID: 35256 RVA: 0x003A896B File Offset: 0x003A6B6B
			internal static void SetCurrentThreadAsMain()
			{
				ModUtils.Environment.MainThreadId = Thread.CurrentThread.ManagedThreadId;
			}

			// Token: 0x17001C38 RID: 7224
			// (get) Token: 0x060089B9 RID: 35257 RVA: 0x003A897C File Offset: 0x003A6B7C
			// (set) Token: 0x060089BA RID: 35258 RVA: 0x003A8983 File Offset: 0x003A6B83
			public static int MainThreadId { get; private set; } = int.MinValue;

			// Token: 0x17001C39 RID: 7225
			// (get) Token: 0x060089BB RID: 35259 RVA: 0x003A898B File Offset: 0x003A6B8B
			public static bool IsMainThread
			{
				get
				{
					if (ModUtils.Environment.MainThreadId == -2147483648)
					{
						throw new ArgumentNullException("MainThread ID not set.");
					}
					return Thread.CurrentThread.ManagedThreadId == ModUtils.Environment.MainThreadId;
				}
			}

			// Token: 0x04005630 RID: 22064
			public static readonly Platform CurrentPlatform = Platform.Windows;

			// Token: 0x04005631 RID: 22065
			public static readonly Target CurrentTarget = Target.Client;
		}

		// Token: 0x02000F9D RID: 3997
		public static class Logging
		{
			// Token: 0x060089BD RID: 35261 RVA: 0x003A89D0 File Offset: 0x003A6BD0
			public static void PrintMessage(string s)
			{
				LuaCsSetup.Instance.Logger.LogMessage(s ?? "", null, null);
			}

			// Token: 0x060089BE RID: 35262 RVA: 0x003A8A08 File Offset: 0x003A6C08
			public static void PrintWarning(string s)
			{
				LuaCsSetup.Instance.Logger.Log(s ?? "", new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
			}

			// Token: 0x060089BF RID: 35263 RVA: 0x003A8A2E File Offset: 0x003A6C2E
			public static void PrintError(string s)
			{
				LuaCsSetup.Instance.Logger.LogError(s ?? "");
			}
		}

		// Token: 0x02000F9E RID: 3998
		public static class IO
		{
			// Token: 0x060089C0 RID: 35264 RVA: 0x003A8A4C File Offset: 0x003A6C4C
			public static IEnumerable<string> FindAllFilesInDirectory(string folder, string pattern, SearchOption option)
			{
				IEnumerable<string> result;
				try
				{
					result = Directory.GetFiles(folder, pattern, option);
				}
				catch (DirectoryNotFoundException e)
				{
					result = new string[0];
				}
				return result;
			}

			// Token: 0x060089C1 RID: 35265 RVA: 0x003A8A80 File Offset: 0x003A6C80
			public static string PrepareFilePathString(string filePath)
			{
				return ModUtils.IO.PrepareFilePathString(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
			}

			// Token: 0x060089C2 RID: 35266 RVA: 0x003A8A93 File Offset: 0x003A6C93
			public static string PrepareFilePathString(string path, string fileName)
			{
				return Path.Combine(ModUtils.IO.SanitizePath(path), ModUtils.IO.SanitizeFileName(fileName));
			}

			// Token: 0x060089C3 RID: 35267 RVA: 0x003A8AA8 File Offset: 0x003A6CA8
			public static string SanitizeFileName(string fileName)
			{
				foreach (char c in Path.GetInvalidFileNameCharsCrossPlatform())
				{
					fileName = fileName.Replace(c, '_');
				}
				return fileName;
			}

			// Token: 0x060089C4 RID: 35268 RVA: 0x003A8B00 File Offset: 0x003A6D00
			public static string GetContentPackageDir(ContentPackage package)
			{
				return ModUtils.IO.SanitizePath(Path.GetFullPath(package.Dir));
			}

			// Token: 0x060089C5 RID: 35269 RVA: 0x003A8B14 File Offset: 0x003A6D14
			public static string SanitizePath(string path)
			{
				foreach (char c in Path.GetInvalidPathChars())
				{
					path = path.Replace(c.ToString(), "_");
				}
				return path.CleanUpPath();
			}

			// Token: 0x060089C6 RID: 35270 RVA: 0x003A8B54 File Offset: 0x003A6D54
			public static ModUtils.IO.IOActionResultState GetOrCreateFileText(string filePath, out string fileText, Func<string> fileDataFactory = null, bool createFile = true)
			{
				fileText = null;
				string fp = Path.GetFullPath(ModUtils.IO.SanitizePath(filePath));
				ModUtils.IO.IOActionResultState ioActionResultState = ModUtils.IO.IOActionResultState.Success;
				if (createFile)
				{
					ioActionResultState = ModUtils.IO.CreateFilePath(ModUtils.IO.SanitizePath(filePath), out fp, fileDataFactory);
				}
				else if (!File.Exists(fp))
				{
					return ModUtils.IO.IOActionResultState.FileNotFound;
				}
				if (ioActionResultState == ModUtils.IO.IOActionResultState.Success)
				{
					try
					{
						fileText = File.ReadAllText(fp);
						return ModUtils.IO.IOActionResultState.Success;
					}
					catch (ArgumentNullException ane)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: An argument is null. path: " + (fp ?? "null") + " | Exception Details: " + ane.Message);
						return ModUtils.IO.IOActionResultState.FilePathNull;
					}
					catch (ArgumentException ae)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: An argument is invalid. path: " + (fp ?? "null") + " | Exception Details: " + ae.Message);
						return ModUtils.IO.IOActionResultState.FilePathInvalid;
					}
					catch (DirectoryNotFoundException dnfe)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Cannot find directory. path: " + (fp ?? "null") + " | Exception Details: " + dnfe.Message);
						return ModUtils.IO.IOActionResultState.DirectoryMissing;
					}
					catch (PathTooLongException ptle)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: path length is over 200 characters. path: " + (fp ?? "null") + " | Exception Details: " + ptle.Message);
						return ModUtils.IO.IOActionResultState.PathTooLong;
					}
					catch (NotSupportedException nse)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: " + (fp ?? "null") + "  | Exception Details: " + nse.Message);
						return ModUtils.IO.IOActionResultState.InvalidOperation;
					}
					catch (IOException ioe)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: " + (fp ?? "null") + "  | Exception Details: " + ioe.Message);
						return ModUtils.IO.IOActionResultState.IOFailure;
					}
					catch (Exception e)
					{
						ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Unknown/Other Exception. path: " + (fp ?? "null") + " | ExceptionMessage: " + e.Message);
						return ModUtils.IO.IOActionResultState.UnknownError;
					}
					return ioActionResultState;
				}
				return ioActionResultState;
			}

			// Token: 0x060089C7 RID: 35271 RVA: 0x003A8D38 File Offset: 0x003A6F38
			public static ModUtils.IO.IOActionResultState CreateFilePath(string filePath, out string formattedFilePath, Func<string> fileDataFactory = null)
			{
				string file = Path.GetFileName(filePath);
				string path = Path.GetDirectoryName(filePath);
				formattedFilePath = ModUtils.IO.PrepareFilePathString(path, file);
				ModUtils.IO.IOActionResultState result;
				try
				{
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					if (!File.Exists(formattedFilePath))
					{
						File.WriteAllText(formattedFilePath, (fileDataFactory == null) ? "" : fileDataFactory());
					}
					result = ModUtils.IO.IOActionResultState.Success;
				}
				catch (ArgumentNullException ane)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: An argument is null. path: " + (formattedFilePath ?? "null") + "  | Exception Details: " + ane.Message);
					result = ModUtils.IO.IOActionResultState.FilePathNull;
				}
				catch (ArgumentException ae)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: An argument is invalid. path: " + (formattedFilePath ?? "null") + " | Exception Details: " + ae.Message);
					result = ModUtils.IO.IOActionResultState.FilePathInvalid;
				}
				catch (DirectoryNotFoundException dnfe)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Cannot find directory. path: " + (path ?? "null") + " | Exception Details: " + dnfe.Message);
					result = ModUtils.IO.IOActionResultState.DirectoryMissing;
				}
				catch (PathTooLongException ptle)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: path length is over 200 characters. path: " + (formattedFilePath ?? "null") + " | Exception Details: " + ptle.Message);
					result = ModUtils.IO.IOActionResultState.PathTooLong;
				}
				catch (NotSupportedException nse)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: " + (formattedFilePath ?? "null") + " | Exception Details: " + nse.Message);
					result = ModUtils.IO.IOActionResultState.InvalidOperation;
				}
				catch (IOException ioe)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: " + (formattedFilePath ?? "null") + " | Exception Details: " + ioe.Message);
					result = ModUtils.IO.IOActionResultState.IOFailure;
				}
				catch (Exception e)
				{
					ModUtils.Logging.PrintError("ModUtils::CreateFilePath() | Exception: Unknown/Other Exception. path: " + (path ?? "null") + " | Exception Details: " + e.Message);
					result = ModUtils.IO.IOActionResultState.UnknownError;
				}
				return result;
			}

			// Token: 0x060089C8 RID: 35272 RVA: 0x003A8F28 File Offset: 0x003A7128
			public static ModUtils.IO.IOActionResultState WriteFileText(string filePath, string fileText)
			{
				string fp;
				ModUtils.IO.IOActionResultState ioActionResultState = ModUtils.IO.CreateFilePath(filePath, out fp, null);
				if (ioActionResultState == ModUtils.IO.IOActionResultState.Success)
				{
					try
					{
						File.WriteAllText(fp, fileText);
						return ModUtils.IO.IOActionResultState.Success;
					}
					catch (ArgumentNullException ane)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: An argument is null. path: " + (fp ?? "null") + " | Exception Details: " + ane.Message);
						return ModUtils.IO.IOActionResultState.FilePathNull;
					}
					catch (ArgumentException ae)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: An argument is invalid. path: " + (fp ?? "null") + " | Exception Details: " + ae.Message);
						return ModUtils.IO.IOActionResultState.FilePathInvalid;
					}
					catch (DirectoryNotFoundException dnfe)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: Cannot find directory. path: " + (fp ?? "null") + " | Exception Details: " + dnfe.Message);
						return ModUtils.IO.IOActionResultState.DirectoryMissing;
					}
					catch (PathTooLongException ptle)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: path length is over 200 characters. path: " + (fp ?? "null") + " | Exception Details: " + ptle.Message);
						return ModUtils.IO.IOActionResultState.PathTooLong;
					}
					catch (NotSupportedException nse)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: Operation not supported on your platform/environment (permissions?). path: " + (fp ?? "null") + " | Exception Details: " + nse.Message);
						return ModUtils.IO.IOActionResultState.InvalidOperation;
					}
					catch (IOException ioe)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: IO tasks failed (Operation not supported). path: " + (fp ?? "null") + " | Exception Details: " + ioe.Message);
						return ModUtils.IO.IOActionResultState.IOFailure;
					}
					catch (Exception e)
					{
						ModUtils.Logging.PrintError("ModUtils::WriteFileText() | Exception: Unknown/Other Exception. path: " + (fp ?? "null") + " | ExceptionMessage: " + e.Message);
						return ModUtils.IO.IOActionResultState.UnknownError;
					}
					return ioActionResultState;
				}
				return ioActionResultState;
			}

			// Token: 0x060089C9 RID: 35273 RVA: 0x003A90E8 File Offset: 0x003A72E8
			public static bool LoadOrCreateTypeXml<T>(out T instance, string filepath, Func<T> typeFactory = null, bool createFile = true) where T : class, new()
			{
				instance = default(T);
				filepath = filepath.CleanUpPath();
				string fileText;
				if (ModUtils.IO.GetOrCreateFileText(filepath, out fileText, (typeFactory != null) ? delegate()
				{
					string result;
					using (StringWriter sw = new StringWriter())
					{
						Func<T> typeFactory2 = typeFactory;
						T t = (typeFactory2 != null) ? typeFactory2() : default(T);
						if (t != null)
						{
							XmlSerializer s2 = new XmlSerializer(typeof(T));
							s2.Serialize(sw, t);
							result = sw.ToString();
						}
						else
						{
							result = "";
						}
					}
					return result;
				} : null, createFile) == ModUtils.IO.IOActionResultState.Success)
				{
					XmlSerializer s = new XmlSerializer(typeof(T));
					try
					{
						using (TextReader tr = new StringReader(fileText))
						{
							instance = (T)((object)s.Deserialize(tr));
							return true;
						}
					}
					catch (InvalidOperationException ioe)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error while parsing type data for ");
						defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
						defaultInterpolatedStringHandler.AppendLiteral(".");
						ModUtils.Logging.PrintError(defaultInterpolatedStringHandler.ToStringAndClear());
						instance = default(T);
						return false;
					}
					return false;
				}
				return false;
			}

			// Token: 0x02001568 RID: 5480
			public enum IOActionResultState
			{
				// Token: 0x04006866 RID: 26726
				Success,
				// Token: 0x04006867 RID: 26727
				FileNotFound,
				// Token: 0x04006868 RID: 26728
				FilePathNull,
				// Token: 0x04006869 RID: 26729
				FilePathInvalid,
				// Token: 0x0400686A RID: 26730
				DirectoryMissing,
				// Token: 0x0400686B RID: 26731
				PathTooLong,
				// Token: 0x0400686C RID: 26732
				InvalidOperation,
				// Token: 0x0400686D RID: 26733
				IOFailure,
				// Token: 0x0400686E RID: 26734
				UnknownError
			}
		}

		// Token: 0x02000F9F RID: 3999
		public static class Game
		{
			// Token: 0x060089CA RID: 35274 RVA: 0x003A91DC File Offset: 0x003A73DC
			public static bool IsRoundInProgress()
			{
				return (Screen.Selected == null || !Screen.Selected.IsEditor) && GameMain.GameSession != null && Level.Loaded != null;
			}
		}

		// Token: 0x02000FA0 RID: 4000
		public static class Threading
		{
			// Token: 0x060089CB RID: 35275 RVA: 0x003A9207 File Offset: 0x003A7407
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool GetBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 1, 1) > 0;
			}

			// Token: 0x060089CC RID: 35276 RVA: 0x003A9214 File Offset: 0x003A7414
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void SetBool(ref int var, bool value)
			{
				if (value)
				{
					Interlocked.CompareExchange(ref var, 1, 0);
					return;
				}
				Interlocked.CompareExchange(ref var, 0, 1);
			}

			// Token: 0x060089CD RID: 35277 RVA: 0x003A922C File Offset: 0x003A742C
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool CheckIfClearAndSetBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 1, 0) < 1;
			}

			// Token: 0x060089CE RID: 35278 RVA: 0x003A9239 File Offset: 0x003A7439
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool CheckIfSetAndClearBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 0, 1) > 0;
			}
		}
	}
}
