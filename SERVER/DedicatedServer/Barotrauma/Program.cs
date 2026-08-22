using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Barotrauma.Debugging;
using Barotrauma.IO;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000047 RID: 71
	public static class Program
	{
		// Token: 0x06000BB2 RID: 2994 RVA: 0x0006F5F8 File Offset: 0x0006D7F8
		public static bool TryStartChildServerRelay(string[] commandLineArgs)
		{
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				string a = commandLineArgs[i].Trim();
				if (a == "-pipes")
				{
					ChildServerRelay.Start(commandLineArgs[i + 2], commandLineArgs[i + 1]);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x0006F63C File Offset: 0x0006D83C
		[STAThread]
		private static void Main(string[] args)
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			AppDomain appDomain = currentDomain;
			EventHandler value;
			if ((value = Program.<>O.<0>__OnProcessExit) == null)
			{
				value = (Program.<>O.<0>__OnProcessExit = new EventHandler(Program.OnProcessExit));
			}
			appDomain.ProcessExit += value;
			currentDomain.UnhandledException += Program.CrashHandler;
			Program.TryStartChildServerRelay(args);
			string[] array = new string[9];
			array[0] = "Barotrauma Dedicated Server ";
			int num = 1;
			Version version = GameMain.Version;
			array[num] = ((version != null) ? version.ToString() : null);
			array[2] = " (";
			array[3] = AssemblyInfo.BuildString;
			array[4] = ", branch ";
			array[5] = AssemblyInfo.GitBranch;
			array[6] = ", revision ";
			array[7] = AssemblyInfo.GitRevision;
			array[8] = ")";
			Console.WriteLine(string.Concat(array));
			if (Console.IsOutputRedirected)
			{
				Console.WriteLine("Output redirection detected; colored text and command input will be disabled.");
			}
			if (Console.IsInputRedirected)
			{
				Console.WriteLine("Redirected input is detected but is not supported by this application. Input will be ignored.");
			}
			string executableDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			if (!File.Exists(Path.Combine(new string[]
			{
				executableDir,
				"workshop.txt"
			})))
			{
				Directory.SetCurrentDirectory(executableDir);
			}
			Action<string, Color> newMessage = delegate(string s, Color c)
			{
				DebugConsole.NewMessage(s, new Color?(c), false);
			};
			Action<string> log;
			if ((log = Program.<>O.<1>__Log) == null)
			{
				log = (Program.<>O.<1>__Log = new Action<string>(DebugConsole.Log));
			}
			DebugConsoleCore.Init(newMessage, log);
			Program.Game = new GameMain(args);
			Program.Game.Run();
			Program.ShutDown();
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0006F79D File Offset: 0x0006D99D
		private static void ShutDown()
		{
			if (Program.hasShutDown)
			{
				return;
			}
			Program.hasShutDown = true;
			if (GameAnalyticsManager.SendUserStatistics)
			{
				GameAnalyticsManager.ShutDown();
			}
			SteamManager.ShutDown();
			EosInterface.Core.CleanupAndQuit();
			while (EosInterface.Core.IsInitialized)
			{
				EosInterface.Core.Update();
				TaskPool.Update();
				Thread.Sleep(16);
			}
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x0006F7DD File Offset: 0x0006D9DD
		private static void OnProcessExit(object sender, EventArgs e)
		{
			Program.ShutDown();
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x0006F7E4 File Offset: 0x0006D9E4
		private static void NotifyCrash(string reportFilePath, Exception e)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 4);
			defaultInterpolatedStringHandler.AppendFormatted(reportFilePath);
			defaultInterpolatedStringHandler.AppendLiteral("||\n");
			defaultInterpolatedStringHandler.AppendFormatted(e.Message);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted(e.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral(") ");
			defaultInterpolatedStringHandler.AppendFormatted(e.StackTrace);
			string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
			if (e.InnerException != null)
			{
				Exception innerMost = e.GetInnermost();
				string str = errorMsg;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(22, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("\nInner exception: ");
				defaultInterpolatedStringHandler2.AppendFormatted(innerMost.Message);
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted(innerMost.GetType().Name);
				defaultInterpolatedStringHandler2.AppendLiteral(") ");
				defaultInterpolatedStringHandler2.AppendFormatted(e.StackTrace);
				errorMsg = str + defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			if (errorMsg.Length > 65535)
			{
				errorMsg = errorMsg.Substring(0, 65535);
			}
			ChildServerRelay.NotifyCrash(errorMsg);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.NotifyCrash();
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x0006F900 File Offset: 0x0006DB00
		private static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception unhandledException = args.ExceptionObject as Exception;
			string reportFilePath = "";
			try
			{
				reportFilePath = "servercrashreport.log";
				Program.CrashDump(ref reportFilePath, unhandledException);
			}
			catch (Exception exceptionHandlerError)
			{
				string slimCrashReport = "Exception handler failed: " + exceptionHandlerError.Message + "\n" + exceptionHandlerError.StackTrace;
				if (unhandledException != null)
				{
					slimCrashReport = string.Concat(new string[]
					{
						slimCrashReport,
						"\n\nInitial exception: ",
						unhandledException.Message,
						"\n",
						unhandledException.StackTrace
					});
				}
				File.WriteAllText("servercrashreportslim.log", slimCrashReport, null, true);
				reportFilePath = "";
			}
			Program.<CrashHandler>g__swallowExceptions|7_0(delegate
			{
				Program.NotifyCrash(reportFilePath, unhandledException);
			});
			Program.<CrashHandler>g__swallowExceptions|7_0(delegate
			{
				GameMain game = Program.Game;
				if (game == null)
				{
					return;
				}
				game.Exit();
			});
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x0006FA10 File Offset: 0x0006DC10
		private static void CrashDump(ref string filePath, Exception exception)
		{
			try
			{
				GameServer server = GameMain.Server;
				if (server != null)
				{
					ServerSettings serverSettings = server.ServerSettings;
					if (serverSettings != null)
					{
						serverSettings.SaveSettings();
					}
				}
				GameServer server2 = GameMain.Server;
				if (server2 != null)
				{
					ServerSettings serverSettings2 = server2.ServerSettings;
					if (serverSettings2 != null)
					{
						serverSettings2.BanList.Save();
					}
				}
				GameServer server3 = GameMain.Server;
				string a;
				if (server3 == null)
				{
					a = null;
				}
				else
				{
					ServerSettings serverSettings3 = server3.ServerSettings;
					a = ((serverSettings3 != null) ? serverSettings3.KarmaPreset : null);
				}
				if (a == "custom")
				{
					GameServer server4 = GameMain.Server;
					if (server4 != null)
					{
						KarmaManager karmaManager = server4.KarmaManager;
						if (karmaManager != null)
						{
							karmaManager.SaveCustomPreset();
						}
					}
					GameServer server5 = GameMain.Server;
					if (server5 != null)
					{
						KarmaManager karmaManager2 = server5.KarmaManager;
						if (karmaManager2 != null)
						{
							karmaManager2.Save();
						}
					}
				}
			}
			catch (Exception e)
			{
			}
			int existingFiles = 0;
			string originalFilePath = filePath;
			while (File.Exists(filePath))
			{
				existingFiles++;
				filePath = string.Concat(new string[]
				{
					Path.GetFileNameWithoutExtension(originalFilePath),
					" (",
					(existingFiles + 1).ToString(),
					")",
					Path.GetExtension(originalFilePath)
				});
			}
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Barotrauma Dedicated Server crash report (generated on " + DateTime.Now.ToString() + ")");
			sb.AppendLine("\n");
			sb.AppendLine("Barotrauma seems to have crashed. Sorry for the inconvenience! ");
			sb.AppendLine("\n");
			StringBuilder stringBuilder = sb;
			string[] array = new string[9];
			array[0] = "Game version ";
			int num = 1;
			Version version = GameMain.Version;
			array[num] = ((version != null) ? version.ToString() : null);
			array[2] = " (";
			array[3] = AssemblyInfo.BuildString;
			array[4] = ", branch ";
			array[5] = AssemblyInfo.GitBranch;
			array[6] = ", revision ";
			array[7] = AssemblyInfo.GitRevision;
			array[8] = ")";
			stringBuilder.AppendLine(string.Concat(array));
			sb.AppendLine("Language: " + GameSettings.CurrentConfig.Language.ToString());
			if (ContentPackageManager.EnabledPackages.All != null)
			{
				StringBuilder stringBuilder2 = sb;
				string str = "Selected content packages: ";
				string str2;
				if (ContentPackageManager.EnabledPackages.All.Any<ContentPackage>())
				{
					str2 = string.Join(", ", ContentPackageManager.EnabledPackages.All.Select(delegate(ContentPackage c)
					{
						string name2 = c.Name;
						string str7 = " (";
						Md5Hash hash = c.Hash;
						return name2 + str7 + (((hash != null) ? hash.ShortRepresentation : null) ?? "unknown") + ")";
					}));
				}
				else
				{
					str2 = "None";
				}
				stringBuilder2.AppendLine(str + str2);
			}
			sb.AppendLine("Level seed: " + ((Level.Loaded == null) ? "no level loaded" : Level.Loaded.Seed));
			StringBuilder stringBuilder3 = sb;
			string str3 = "Loaded submarine: ";
			string str5;
			if (Submarine.MainSub != null)
			{
				string name = Submarine.MainSub.Info.Name;
				string str4 = " (";
				Md5Hash md5Hash = Submarine.MainSub.Info.MD5Hash;
				str5 = name + str4 + ((md5Hash != null) ? md5Hash.ToString() : null) + ")";
			}
			else
			{
				str5 = "None";
			}
			stringBuilder3.AppendLine(str3 + str5);
			sb.AppendLine("Selected screen: " + ((Screen.Selected == null) ? "None" : Screen.Selected.ToString()));
			if (GameMain.Server != null)
			{
				sb.AppendLine("Server (" + (GameMain.Server.GameStarted ? "Round had started)" : "Round hadn't been started)"));
			}
			sb.AppendLine("\n");
			sb.AppendLine("System info:");
			StringBuilder stringBuilder4 = sb;
			string str6 = "    Operating system: ";
			OperatingSystem osversion = Environment.OSVersion;
			stringBuilder4.AppendLine(str6 + ((osversion != null) ? osversion.ToString() : null) + (Environment.Is64BitOperatingSystem ? " 64 bit" : " x86"));
			sb.AppendLine("\n");
			sb.AppendLine(string.Concat(new string[]
			{
				"Exception: ",
				exception.Message,
				" (",
				exception.GetType().ToString(),
				")"
			}));
			sb.AppendLine("Target site: " + exception.TargetSite.ToString());
			if (exception.StackTrace != null)
			{
				sb.AppendLine("Stack trace: ");
				sb.AppendLine(exception.StackTrace.CleanupStackTrace());
				sb.AppendLine("\n");
			}
			if (exception.InnerException != null)
			{
				sb.AppendLine("InnerException: " + exception.InnerException.Message);
				if (exception.InnerException.TargetSite != null)
				{
					sb.AppendLine("Target site: " + exception.InnerException.TargetSite.ToString());
				}
				if (exception.InnerException.StackTrace != null)
				{
					sb.AppendLine("Stack trace: ");
					sb.AppendLine(exception.InnerException.StackTrace.CleanupStackTrace());
				}
			}
			if (GameAnalyticsManager.SendUserStatistics)
			{
				GameAnalyticsManager.AddErrorEvent(GameAnalyticsManager.ErrorSeverity.Critical, sb.ToString());
				GameAnalyticsManager.ShutDown();
			}
			sb.AppendLine("Last debug messages:");
			DebugConsole.Clear();
			int i = DebugConsole.Messages.Count - 1;
			while (i > 0 && i > DebugConsole.Messages.Count - 15)
			{
				sb.AppendLine("   " + DebugConsole.Messages[i].Time + " - " + DebugConsole.Messages[i].Text);
				i--;
			}
			string crashReport = sb.ToString();
			if (!Console.IsOutputRedirected)
			{
				Console.ForegroundColor = ConsoleColor.Red;
			}
			Console.Write(crashReport);
			File.WriteAllText(filePath, sb.ToString(), null, true);
			if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.SaveLogs();
			}
			if (GameAnalyticsManager.SendUserStatistics)
			{
				Console.Write("A crash report (\"servercrashreport.log\") was saved in the root folder of the game and sent to the developers.");
			}
			else
			{
				Console.Write("A crash report(\"servercrashreport.log\") was saved in the root folder of the game. The error was not sent to the developers because user statistics have been disabled, but if you'd like to help fix this bug, you may post it on Barotrauma's GitHub issue tracker: https://github.com/Regalis11/Barotrauma/issues/");
			}
			SteamManager.ShutDown();
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0006FF9C File Offset: 0x0006E19C
		[CompilerGenerated]
		internal static void <CrashHandler>g__swallowExceptions|7_0(Action action)
		{
			try
			{
				action();
			}
			catch
			{
			}
		}

		// Token: 0x04000506 RID: 1286
		private static bool hasShutDown;

		// Token: 0x04000507 RID: 1287
		private static GameMain Game;

		// Token: 0x0200074A RID: 1866
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002C8F RID: 11407
			public static EventHandler <0>__OnProcessExit;

			// Token: 0x04002C90 RID: 11408
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<string> <1>__Log;
		}
	}
}
