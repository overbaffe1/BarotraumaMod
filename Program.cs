using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Debugging;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.Direct3D11;

namespace Barotrauma
{
	// Token: 0x020000FD RID: 253
	public static class Program
	{
		// Token: 0x060023FB RID: 9211 RVA: 0x00167C08 File Offset: 0x00165E08
		[STAThread]
		private static void Main(string[] args)
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += Program.CrashHandler;
			Program.Game = null;
			string executableDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			Directory.SetCurrentDirectory(executableDir);
			Action<string, Color> newMessage = delegate(string s, Color c)
			{
				DebugConsole.NewMessage(s, new Color?(c), false);
			};
			Action<string> log;
			if ((log = Program.<>O.<0>__Log) == null)
			{
				log = (Program.<>O.<0>__Log = new Action<string>(DebugConsole.Log));
			}
			DebugConsoleCore.Init(newMessage, log);
			StoreIntegration.Init(ref args);
			Program.EnableNvOptimus();
			Program.Game = new GameMain(args);
			Program.Game.Run();
			Program.Game.Dispose();
			Program.FreeNvOptimus();
			CrossThread.ProcessTasks();
		}

		// Token: 0x060023FC RID: 9212 RVA: 0x00167CC4 File Offset: 0x00165EC4
		private static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception unhandledException = args.ExceptionObject as Exception;
			try
			{
				GameMain game = Program.Game;
				if (game != null)
				{
					game.Exit();
				}
				Program.CrashDump(Program.Game, "crashreport.log", unhandledException);
				GameMain game2 = Program.Game;
				if (game2 != null)
				{
					game2.Dispose();
				}
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
				File.WriteAllText("crashreportslim.log", slimCrashReport, null, true);
			}
		}

		// Token: 0x060023FD RID: 9213 RVA: 0x00167D80 File Offset: 0x00165F80
		public static void CrashMessageBox(string message, string filePath)
		{
			MessageBox.ShowWrapped(MessageBox.Flags.Error, "Oops! Barotrauma just crashed.", message, 60, null);
			if (!string.IsNullOrWhiteSpace(filePath))
			{
				ToolBox.OpenFileWithShell(filePath);
			}
		}

		// Token: 0x060023FE RID: 9214 RVA: 0x00167DA0 File Offset: 0x00165FA0
		private static void CrashDump(GameMain game, string filePath, Exception exception)
		{
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
			DebugConsole.DequeueMessages();
			Md5Hash exeHash = null;
			try
			{
				string exePath = Assembly.GetEntryAssembly().Location;
				exeHash = Md5Hash.CalculateForFile(exePath, Md5Hash.StringHashOptions.BytePerfect);
			}
			catch
			{
			}
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Barotrauma Client crash report (generated on " + DateTime.Now.ToString() + ")");
			sb.AppendLine();
			sb.AppendLine("Barotrauma seems to have crashed. Sorry for the inconvenience! ");
			sb.AppendLine();
			string dxgiErrorHelpText = Program.GetDXGIErrorHelpText(game, exception);
			if (!string.IsNullOrEmpty(dxgiErrorHelpText))
			{
				sb.AppendLine(dxgiErrorHelpText);
				sb.AppendLine();
			}
			try
			{
				if (exception.StackTrace.Contains("Barotrauma.GameMain.Load"))
				{
					XDocument doc = XMLExtensions.TryLoadXml("config_player.xml");
					if (((doc != null) ? doc.Root : null) != null)
					{
						XElement newElement = new XElement(doc.Root.Name);
						newElement.Add(doc.Root.Attributes());
						Identifier[] contentPackageTags = new Identifier[]
						{
							"contentpackage".ToIdentifier(),
							"contentpackages".ToIdentifier()
						};
						newElement.Add(from e in doc.Root.Elements()
						where !contentPackageTags.Contains(e.NameAsIdentifier())
						select e);
						newElement.Add(new XElement("core", new XAttribute("path", "Content/ContentPackages/Vanilla.xml")));
						XDocument newDoc = new XDocument(new object[]
						{
							newElement
						});
						newDoc.Save("config_player.xml");
						sb.AppendLine("To prevent further startup errors, installed mods will be disabled the next time you launch the game.");
						sb.AppendLine();
					}
				}
			}
			catch
			{
			}
			if (exeHash != null && exeHash.StringRepresentation != null)
			{
				sb.AppendLine(exeHash.StringRepresentation);
			}
			sb.AppendLine();
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
			StringBuilder stringBuilder2 = sb;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(19, 3, stringBuilder2);
			appendInterpolatedStringHandler.AppendLiteral("Graphics mode: ");
			appendInterpolatedStringHandler.AppendFormatted<int>(GameSettings.CurrentConfig.Graphics.Width);
			appendInterpolatedStringHandler.AppendLiteral("x");
			appendInterpolatedStringHandler.AppendFormatted<int>(GameSettings.CurrentConfig.Graphics.Height);
			appendInterpolatedStringHandler.AppendLiteral(" (");
			appendInterpolatedStringHandler.AppendFormatted<WindowMode>(GameSettings.CurrentConfig.Graphics.DisplayMode);
			appendInterpolatedStringHandler.AppendLiteral(")");
			stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
			sb.AppendLine("VSync " + (GameSettings.CurrentConfig.Graphics.VSync ? "ON" : "OFF"));
			sb.AppendLine("Language: " + GameSettings.CurrentConfig.Language.ToString());
			if (ContentPackageManager.EnabledPackages.All != null)
			{
				StringBuilder stringBuilder4 = sb;
				string str = "Selected content packages: ";
				string str2;
				if (ContentPackageManager.EnabledPackages.All.Any<ContentPackage>())
				{
					str2 = string.Join(", ", ContentPackageManager.EnabledPackages.All.Select(delegate(ContentPackage c)
					{
						string name2 = c.Name;
						string str8 = " (";
						Md5Hash hash = c.Hash;
						return name2 + str8 + (((hash != null) ? hash.ShortRepresentation : null) ?? "unknown") + ")";
					}));
				}
				else
				{
					str2 = "None";
				}
				stringBuilder4.AppendLine(str + str2);
			}
			sb.AppendLine("Level seed: " + ((Level.Loaded == null) ? "no level loaded" : Level.Loaded.Seed));
			StringBuilder stringBuilder5 = sb;
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
			stringBuilder5.AppendLine(str3 + str5);
			sb.AppendLine("Selected screen: " + ((Screen.Selected == null) ? "None" : Screen.Selected.ToString()));
			if (SteamManager.IsInitialized)
			{
				sb.AppendLine("SteamManager initialized");
			}
			else if (EosInterface.IdQueries.IsLoggedIntoEosConnect)
			{
				sb.AppendLine("Logged in to EOS connect");
			}
			if (GameMain.Client != null)
			{
				sb.AppendLine("Client (" + (GameMain.Client.GameStarted ? "Round had started)" : "Round hadn't been started)"));
			}
			sb.AppendLine();
			sb.AppendLine("System info:");
			StringBuilder stringBuilder6 = sb;
			string str6 = "    Operating system: ";
			OperatingSystem osversion = Environment.OSVersion;
			stringBuilder6.AppendLine(str6 + ((osversion != null) ? osversion.ToString() : null) + (Environment.Is64BitOperatingSystem ? " 64 bit" : " x86"));
			if (game == null)
			{
				sb.AppendLine("    Game not initialized");
			}
			else if (game.GraphicsDevice == null)
			{
				sb.AppendLine("    Graphics device not set");
			}
			else
			{
				if (game.GraphicsDevice.Adapter == null)
				{
					sb.AppendLine("    Graphics adapter not set");
				}
				else
				{
					sb.AppendLine("    GPU name: " + game.GraphicsDevice.Adapter.Description);
					StringBuilder stringBuilder7 = sb;
					string str7 = "    Display mode: ";
					DisplayMode currentDisplayMode = game.GraphicsDevice.Adapter.CurrentDisplayMode;
					stringBuilder7.AppendLine(str7 + ((currentDisplayMode != null) ? currentDisplayMode.ToString() : null));
				}
				sb.AppendLine("    GPU status: " + game.GraphicsDevice.GraphicsDeviceStatus.ToString());
			}
			sb.AppendLine();
			stringBuilder2 = sb;
			StringBuilder stringBuilder8 = stringBuilder2;
			appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(14, 2, stringBuilder2);
			appendInterpolatedStringHandler.AppendLiteral("Exception: ");
			appendInterpolatedStringHandler.AppendFormatted(exception.Message);
			appendInterpolatedStringHandler.AppendLiteral(" (");
			appendInterpolatedStringHandler.AppendFormatted<Type>(exception.GetType());
			appendInterpolatedStringHandler.AppendLiteral(")");
			stringBuilder8.AppendLine(ref appendInterpolatedStringHandler);
			SharpDXException sharpDxException = exception as SharpDXException;
			if (sharpDxException != null && sharpDxException.HResult == -2005270523)
			{
				Device dxDevice = (Device)game.GraphicsDevice.Handle;
				ResultDescriptor resultDescriptor = ResultDescriptor.Find(dxDevice.DeviceRemovedReason);
				string descriptor = ((resultDescriptor != null) ? resultDescriptor.ApiCode : null) ?? "UNKNOWN";
				stringBuilder2 = sb;
				StringBuilder stringBuilder9 = stringBuilder2;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(26, 2, stringBuilder2);
				appendInterpolatedStringHandler.AppendLiteral("Device removed reason: ");
				appendInterpolatedStringHandler.AppendFormatted(descriptor);
				appendInterpolatedStringHandler.AppendLiteral(" (");
				appendInterpolatedStringHandler.AppendFormatted<Result>(dxDevice.DeviceRemovedReason);
				appendInterpolatedStringHandler.AppendLiteral(")");
				stringBuilder9.AppendLine(ref appendInterpolatedStringHandler);
			}
			if (exception.TargetSite != null)
			{
				sb.AppendLine("Target site: " + exception.TargetSite.ToString());
			}
			if (exception.StackTrace != null)
			{
				sb.AppendLine("Stack trace: ");
				sb.AppendLine(exception.StackTrace.CleanupStackTrace());
				sb.AppendLine();
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
				string crashHeader = exception.Message;
				if (exception.TargetSite != null)
				{
					crashHeader = crashHeader + " " + exception.TargetSite.ToString();
				}
				GameAnalyticsManager.AddErrorEvent(GameAnalyticsManager.ErrorSeverity.Critical, crashHeader);
				GameAnalyticsManager.AddErrorEvent(GameAnalyticsManager.ErrorSeverity.Critical, crashHeader + "\n\n" + sb.ToString());
				GameAnalyticsManager.ShutDown();
			}
			sb.AppendLine("Last debug messages:");
			for (int i = DebugConsole.Messages.Count - 1; i >= 0; i--)
			{
				sb.AppendLine("[" + DebugConsole.Messages[i].Time + "] " + DebugConsole.Messages[i].Text);
			}
			string crashReport = sb.ToString();
			File.WriteAllText(filePath, crashReport, null, true);
			if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.SaveLogs();
			}
			string msg = string.Empty;
			if (GameAnalyticsManager.SendUserStatistics)
			{
				msg = "A crash report (\"" + filePath + "\") was saved in the root folder of the game and sent to the developers.";
			}
			else
			{
				msg = "A crash report (\"" + filePath + "\") was saved in the root folder of the game. The error was not sent to the developers because user statistics have been disabled, but if you'd like to help fix this bug, you may post it on Barotrauma's GitHub issue tracker: https://github.com/Regalis11/Barotrauma/issues/";
			}
			if (string.IsNullOrEmpty(dxgiErrorHelpText))
			{
				msg = msg + "\n\n" + dxgiErrorHelpText;
			}
			Program.CrashMessageBox(msg, filePath);
		}

		// Token: 0x060023FF RID: 9215 RVA: 0x001686C4 File Offset: 0x001668C4
		private static string GetDXGIErrorHelpText(GameMain game, Exception exception)
		{
			string text = string.Empty;
			SharpDXException sharpDxException = exception as SharpDXException;
			if (sharpDxException != null && sharpDxException.HResult == -2005270523)
			{
				Device dxDevice = (Device)game.GraphicsDevice.Handle;
				ResultDescriptor resultDescriptor = ResultDescriptor.Find(dxDevice.DeviceRemovedReason);
				string descriptor = ((resultDescriptor != null) ? resultDescriptor.ApiCode : null) ?? "UNKNOWN";
				string str = text;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The crash was caused by the DirectX error ");
				defaultInterpolatedStringHandler.AppendFormatted(descriptor);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Result>(dxDevice.DeviceRemovedReason);
				defaultInterpolatedStringHandler.AppendLiteral("). ");
				string str2 = defaultInterpolatedStringHandler.ToStringAndClear();
				string str3 = "This is a common DirectX error that can be related to various different issues, such as outdated drivers, RAM problems or an overclocked or otherwise overstressed GPU. There are several potential ways to fix the issue: ensuring your graphics drivers and DirectX installation are up-to-date, disabling overclocking and adjusting various GPU-specific settings. ";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(134, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("You may also be able to find potential solutions to the problem by using the error code ");
				defaultInterpolatedStringHandler2.AppendFormatted(descriptor);
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted<Result>(dxDevice.DeviceRemovedReason);
				defaultInterpolatedStringHandler2.AppendLiteral(") and your GPU manufacturer as search terms.");
				text = str + str2 + str3 + defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			return text;
		}

		// Token: 0x06002400 RID: 9216 RVA: 0x001687CA File Offset: 0x001669CA
		private static void EnableNvOptimus()
		{
			if (NativeLibrary.TryLoad("nvapi64.dll", out Program.nvApi64Dll))
			{
				DebugConsole.Log("Loaded nvapi64.dll successfully");
			}
		}

		// Token: 0x06002401 RID: 9217 RVA: 0x001687E7 File Offset: 0x001669E7
		private static void FreeNvOptimus()
		{
		}

		// Token: 0x040011F1 RID: 4593
		private static GameMain Game;

		// Token: 0x040011F2 RID: 4594
		private static IntPtr nvApi64Dll = IntPtr.Zero;

		// Token: 0x02000BF3 RID: 3059
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400495D RID: 18781
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<string> <0>__Log;
		}
	}
}
