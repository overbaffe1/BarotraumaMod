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
	// Token: 0x0200020B RID: 523
	public static class ModUtils
	{
		// Token: 0x060024FD RID: 9469 RVA: 0x000F4048 File Offset: 0x000F2248
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

		// Token: 0x020009CA RID: 2506
		public static class ItemPrefab
		{
			// Token: 0x06005AE4 RID: 23268 RVA: 0x001FD11C File Offset: 0x001FB31C
			internal static Barotrauma.ItemPrefab GetItemPrefab(string itemNameOrId)
			{
				return (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as Barotrauma.ItemPrefab;
			}
		}

		// Token: 0x020009CB RID: 2507
		public static class Client
		{
			// Token: 0x06005AE5 RID: 23269 RVA: 0x001FD144 File Offset: 0x001FB344
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

			// Token: 0x06005AE6 RID: 23270 RVA: 0x001FD176 File Offset: 0x001FB376
			internal static void UnbanPlayer(string playerName)
			{
				GameMain.Server.UnbanPlayer(playerName);
			}

			// Token: 0x06005AE7 RID: 23271 RVA: 0x001FD184 File Offset: 0x001FB384
			internal static void BanPlayer(string player, string reason, bool range = false, float seconds = -1f)
			{
				if (seconds == -1f)
				{
					GameMain.Server.BanPlayer(player, reason, null);
					return;
				}
				GameMain.Server.BanPlayer(player, reason, new TimeSpan?(TimeSpan.FromSeconds((double)seconds)));
			}

			// Token: 0x17001573 RID: 5491
			// (get) Token: 0x06005AE8 RID: 23272 RVA: 0x001FD1C7 File Offset: 0x001FB3C7
			internal static IReadOnlyList<Barotrauma.Networking.Client> ClientList
			{
				get
				{
					if (GameMain.IsSingleplayer)
					{
						return new List<Barotrauma.Networking.Client>();
					}
					return GameMain.Server.ConnectedClients;
				}
			}
		}

		// Token: 0x020009CC RID: 2508
		public static class Definitions
		{
			// Token: 0x0400346D RID: 13421
			public const string LuaCsForBarotrauma = "LuaCsForBarotrauma";
		}

		// Token: 0x020009CD RID: 2509
		public static class Environment
		{
			// Token: 0x06005AE9 RID: 23273 RVA: 0x001FD1E0 File Offset: 0x001FB3E0
			internal static void SetCurrentThreadAsMain()
			{
				ModUtils.Environment.MainThreadId = Thread.CurrentThread.ManagedThreadId;
			}

			// Token: 0x17001574 RID: 5492
			// (get) Token: 0x06005AEA RID: 23274 RVA: 0x001FD1F1 File Offset: 0x001FB3F1
			// (set) Token: 0x06005AEB RID: 23275 RVA: 0x001FD1F8 File Offset: 0x001FB3F8
			public static int MainThreadId { get; private set; } = int.MinValue;

			// Token: 0x17001575 RID: 5493
			// (get) Token: 0x06005AEC RID: 23276 RVA: 0x001FD200 File Offset: 0x001FB400
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

			// Token: 0x0400346F RID: 13423
			public static readonly Platform CurrentPlatform = Platform.Windows;

			// Token: 0x04003470 RID: 13424
			public static readonly Target CurrentTarget = Target.Server;
		}

		// Token: 0x020009CE RID: 2510
		public static class Logging
		{
			// Token: 0x06005AEE RID: 23278 RVA: 0x001FD244 File Offset: 0x001FB444
			public static void PrintMessage(string s)
			{
				LuaCsSetup.Instance.Logger.LogMessage(s ?? "", null, null);
			}

			// Token: 0x06005AEF RID: 23279 RVA: 0x001FD27C File Offset: 0x001FB47C
			public static void PrintWarning(string s)
			{
				LuaCsSetup.Instance.Logger.Log(s ?? "", new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
			}

			// Token: 0x06005AF0 RID: 23280 RVA: 0x001FD2A2 File Offset: 0x001FB4A2
			public static void PrintError(string s)
			{
				LuaCsSetup.Instance.Logger.LogError(s ?? "");
			}
		}

		// Token: 0x020009CF RID: 2511
		public static class IO
		{
			// Token: 0x06005AF1 RID: 23281 RVA: 0x001FD2C0 File Offset: 0x001FB4C0
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

			// Token: 0x06005AF2 RID: 23282 RVA: 0x001FD2F4 File Offset: 0x001FB4F4
			public static string PrepareFilePathString(string filePath)
			{
				return ModUtils.IO.PrepareFilePathString(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
			}

			// Token: 0x06005AF3 RID: 23283 RVA: 0x001FD307 File Offset: 0x001FB507
			public static string PrepareFilePathString(string path, string fileName)
			{
				return Path.Combine(ModUtils.IO.SanitizePath(path), ModUtils.IO.SanitizeFileName(fileName));
			}

			// Token: 0x06005AF4 RID: 23284 RVA: 0x001FD31C File Offset: 0x001FB51C
			public static string SanitizeFileName(string fileName)
			{
				foreach (char c in Path.GetInvalidFileNameCharsCrossPlatform())
				{
					fileName = fileName.Replace(c, '_');
				}
				return fileName;
			}

			// Token: 0x06005AF5 RID: 23285 RVA: 0x001FD374 File Offset: 0x001FB574
			public static string GetContentPackageDir(ContentPackage package)
			{
				return ModUtils.IO.SanitizePath(Path.GetFullPath(package.Dir));
			}

			// Token: 0x06005AF6 RID: 23286 RVA: 0x001FD388 File Offset: 0x001FB588
			public static string SanitizePath(string path)
			{
				foreach (char c in Path.GetInvalidPathChars())
				{
					path = path.Replace(c.ToString(), "_");
				}
				return path.CleanUpPath();
			}

			// Token: 0x06005AF7 RID: 23287 RVA: 0x001FD3C8 File Offset: 0x001FB5C8
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

			// Token: 0x06005AF8 RID: 23288 RVA: 0x001FD5AC File Offset: 0x001FB7AC
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

			// Token: 0x06005AF9 RID: 23289 RVA: 0x001FD79C File Offset: 0x001FB99C
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

			// Token: 0x06005AFA RID: 23290 RVA: 0x001FD95C File Offset: 0x001FBB5C
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

			// Token: 0x02000EA8 RID: 3752
			public enum IOActionResultState
			{
				// Token: 0x04004302 RID: 17154
				Success,
				// Token: 0x04004303 RID: 17155
				FileNotFound,
				// Token: 0x04004304 RID: 17156
				FilePathNull,
				// Token: 0x04004305 RID: 17157
				FilePathInvalid,
				// Token: 0x04004306 RID: 17158
				DirectoryMissing,
				// Token: 0x04004307 RID: 17159
				PathTooLong,
				// Token: 0x04004308 RID: 17160
				InvalidOperation,
				// Token: 0x04004309 RID: 17161
				IOFailure,
				// Token: 0x0400430A RID: 17162
				UnknownError
			}
		}

		// Token: 0x020009D0 RID: 2512
		public static class Game
		{
			// Token: 0x06005AFB RID: 23291 RVA: 0x001FDA50 File Offset: 0x001FBC50
			public static bool IsRoundInProgress()
			{
				return GameMain.GameSession != null && Level.Loaded != null;
			}
		}

		// Token: 0x020009D1 RID: 2513
		public static class Threading
		{
			// Token: 0x06005AFC RID: 23292 RVA: 0x001FDA66 File Offset: 0x001FBC66
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool GetBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 1, 1) > 0;
			}

			// Token: 0x06005AFD RID: 23293 RVA: 0x001FDA73 File Offset: 0x001FBC73
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

			// Token: 0x06005AFE RID: 23294 RVA: 0x001FDA8B File Offset: 0x001FBC8B
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool CheckIfClearAndSetBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 1, 0) < 1;
			}

			// Token: 0x06005AFF RID: 23295 RVA: 0x001FDA98 File Offset: 0x001FBC98
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool CheckIfSetAndClearBool(ref int var)
			{
				return Interlocked.CompareExchange(ref var, 0, 1) > 0;
			}
		}
	}
}
