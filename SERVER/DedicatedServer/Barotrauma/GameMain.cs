using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Steam;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Hyper.ComponentModel;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002D RID: 45
	internal class GameMain
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x000314BA File Offset: 0x0002F6BA
		public static bool IsSingleplayer
		{
			get
			{
				return GameMain.NetworkMember == null;
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x0600054B RID: 1355 RVA: 0x000314C4 File Offset: 0x0002F6C4
		public static bool IsMultiplayer
		{
			get
			{
				return GameMain.NetworkMember != null;
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x0600054C RID: 1356 RVA: 0x000314CE File Offset: 0x0002F6CE
		// (set) Token: 0x0600054D RID: 1357 RVA: 0x000314F5 File Offset: 0x0002F6F5
		public static World World
		{
			get
			{
				if (GameMain.world == null)
				{
					GameMain.world = new World(new Vector2(0f, -9.82f));
				}
				return GameMain.world;
			}
			set
			{
				GameMain.world = value;
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x0600054E RID: 1358 RVA: 0x000314FD File Offset: 0x0002F6FD
		public static NetworkMember NetworkMember
		{
			get
			{
				return GameMain.Server;
			}
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x0600054F RID: 1359 RVA: 0x00031504 File Offset: 0x0002F704
		// (set) Token: 0x06000550 RID: 1360 RVA: 0x0003150B File Offset: 0x0002F70B
		public static GameMain Instance { get; private set; }

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000551 RID: 1361 RVA: 0x00031513 File Offset: 0x0002F713
		// (set) Token: 0x06000552 RID: 1362 RVA: 0x0003151A File Offset: 0x0002F71A
		public static Thread MainThread { get; private set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x00031522 File Offset: 0x0002F722
		public static ContentPackage VanillaContent
		{
			get
			{
				return ContentPackageManager.VanillaCorePackage;
			}
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0003152C File Offset: 0x0002F72C
		public GameMain(string[] args)
		{
			GameMain.Instance = this;
			this.CommandLineArgs = args;
			GameMain.World = new World(new Vector2(0f, -9.82f));
			Settings.AllowSleep = true;
			Settings.ContinuousPhysics = false;
			Settings.VelocityIterations = 1;
			Settings.PositionIterations = 1;
			Console.WriteLine("Loading game settings");
			GameSettings.Init();
			if (!this.CommandLineArgs.Any((string a) => a.Trim().Equals("-ownerkey", StringComparison.OrdinalIgnoreCase)))
			{
				Console.WriteLine("Initializing SteamManager");
				SteamManager.Initialize();
				if (!SteamManager.SteamworksLibExists)
				{
					Console.WriteLine("Initializing EosManager");
					EosInterface.Core.InitError initError;
					if (EosInterface.Core.Init(EosInterface.ApplicationCredentials.Server, false).TryUnwrapFailure(out initError))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
						defaultInterpolatedStringHandler.AppendLiteral("EOS failed to initialize: ");
						defaultInterpolatedStringHandler.AppendFormatted<EosInterface.Core.InitError>(initError);
						Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			Console.WriteLine("Initializing GameScreen");
			GameMain.GameScreen = new GameScreen();
			GameMain.MainThread = Thread.CurrentThread;
			LuaCsSetup.Instance.GetType();
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x0003163B File Offset: 0x0002F83B
		public void Init()
		{
			CoreEntityPrefab.InitCorePrefabs();
			GameModePreset.Init();
			ContentPackageManager.Init().Consume<ContentPackageManager.LoadProgress>();
			ContentPackageManager.LogEnabledRegularPackageErrors();
			SubmarineInfo.RefreshSavedSubs();
			Screen.SelectNull();
			GameMain.NetLobbyScreen = new NetLobbyScreen();
			this.CheckContentPackage();
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x00031670 File Offset: 0x0002F870
		private void CheckContentPackage()
		{
			if (GameMain.Version < GameMain.VanillaContent.GameVersion)
			{
				DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariables("versionmismatchwarning", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[gameversion]", GameMain.Version.ToString()),
					new ValueTuple<string, string>("[contentversion]", GameMain.VanillaContent.GameVersion.ToString())
				}), null, null, false, false);
			}
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x000316E8 File Offset: 0x0002F8E8
		public void StartServer()
		{
			string name = "Server";
			int port = 27015;
			int queryPort = 27016;
			bool publiclyVisible = false;
			string password = "";
			bool enableUpnp = false;
			int maxPlayers = 10;
			Option.UnspecifiedNone none = Option.None;
			Option<int> ownerKey = none;
			none = Option.None;
			Option<P2PEndpoint> ownerEndpoint = none;
			IPAddress listenIp = IPAddress.Any;
			XDocument doc = XMLExtensions.TryLoadXml("serversettings.xml");
			if (((doc != null) ? doc.Root : null) == null)
			{
				DebugConsole.AddWarning("File \"serversettings.xml\" not found. Starting the server with default settings.", null);
			}
			else
			{
				name = doc.Root.GetAttributeString("ServerName", doc.Root.GetAttributeString("name", "Server"));
				port = doc.Root.GetAttributeInt("Port", 27015);
				queryPort = doc.Root.GetAttributeInt("QueryPort", 27016);
				publiclyVisible = doc.Root.GetAttributeBool("IsPublic", false);
				enableUpnp = doc.Root.GetAttributeBool("EnableUPnP", false);
				maxPlayers = doc.Root.GetAttributeInt("MaxPlayers", 10);
				password = doc.Root.GetAttributeString("password", "");
				ownerKey = Option<int>.None();
			}
			for (int i = 0; i < this.CommandLineArgs.Length; i++)
			{
				string text = this.CommandLineArgs[i].Trim().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
					{
						if (!(text == "-ip"))
						{
							goto IL_4A6;
						}
						IPAddress address;
						if (IPAddress.TryParse(this.CommandLineArgs[i + 1], out address))
						{
							listenIp = address;
							goto IL_4A6;
						}
						DebugConsole.ThrowError("Invalid Ip Address '" + this.CommandLineArgs[i + 1] + "'.", null, null, false, false);
						goto IL_4A6;
					}
					case 4:
					case 8:
					case 12:
					case 13:
					case 14:
					case 15:
					case 16:
						goto IL_4A6;
					case 5:
					{
						char c = text[1];
						if (c != 'n')
						{
							if (c != 'p')
							{
								if (c != 'u')
								{
									goto IL_4A6;
								}
								if (!(text == "-upnp"))
								{
									goto IL_4A6;
								}
							}
							else
							{
								if (!(text == "-port"))
								{
									goto IL_4A6;
								}
								int.TryParse(this.CommandLineArgs[i + 1], out port);
								i++;
								goto IL_4A6;
							}
						}
						else
						{
							if (!(text == "-name"))
							{
								goto IL_4A6;
							}
							name = this.CommandLineArgs[i + 1];
							i++;
							goto IL_4A6;
						}
						break;
					}
					case 6:
						if (!(text == "-pipes"))
						{
							goto IL_4A6;
						}
						i += 2;
						goto IL_4A6;
					case 7:
						if (!(text == "-public"))
						{
							goto IL_4A6;
						}
						bool.TryParse(this.CommandLineArgs[i + 1], out publiclyVisible);
						i++;
						goto IL_4A6;
					case 9:
					{
						char c = text[1];
						if (c != 'e')
						{
							if (c != 'o')
							{
								if (c != 'p')
								{
									goto IL_4A6;
								}
								if (!(text == "-password"))
								{
									goto IL_4A6;
								}
								password = this.CommandLineArgs[i + 1];
								i++;
								goto IL_4A6;
							}
							else
							{
								if (!(text == "-ownerkey"))
								{
									goto IL_4A6;
								}
								int key;
								if (int.TryParse(this.CommandLineArgs[i + 1], out key))
								{
									ownerKey = Option<int>.Some(key);
								}
								i++;
								goto IL_4A6;
							}
						}
						else
						{
							if (!(text == "-endpoint"))
							{
								goto IL_4A6;
							}
							ownerEndpoint = P2PEndpoint.Parse(this.CommandLineArgs[i + 1]);
							i++;
							goto IL_4A6;
						}
						break;
					}
					case 10:
						if (!(text == "-queryport"))
						{
							goto IL_4A6;
						}
						int.TryParse(this.CommandLineArgs[i + 1], out queryPort);
						i++;
						goto IL_4A6;
					case 11:
					{
						char c = text[1];
						if (c != 'e')
						{
							if (c != 'm')
							{
								if (c != 'n')
								{
									goto IL_4A6;
								}
								if (!(text == "-nopassword"))
								{
									goto IL_4A6;
								}
								password = "";
								goto IL_4A6;
							}
							else
							{
								if (!(text == "-maxplayers"))
								{
									goto IL_4A6;
								}
								int.TryParse(this.CommandLineArgs[i + 1], out maxPlayers);
								i++;
								goto IL_4A6;
							}
						}
						else if (!(text == "-enableupnp"))
						{
							goto IL_4A6;
						}
						break;
					}
					case 17:
						if (!(text == "-lenienthandshake"))
						{
							goto IL_4A6;
						}
						NetConfig.UseLenientHandshake = true;
						goto IL_4A6;
					default:
						goto IL_4A6;
					}
					bool.TryParse(this.CommandLineArgs[i + 1], out enableUpnp);
					i++;
				}
				IL_4A6:;
			}
			GameMain.Server = new GameServer(name, listenIp, port, queryPort, publiclyVisible, password, enableUpnp, maxPlayers, ownerKey, ownerEndpoint);
			GameMain.Server.StartServer(true);
			for (int j = 0; j < this.CommandLineArgs.Length; j++)
			{
				string text2 = this.CommandLineArgs[j].Trim().ToLowerInvariant();
				if (text2 != null)
				{
					int length = text2.Length;
					switch (length)
					{
					case 6:
						if (!(text2 == "-karma"))
						{
							goto IL_6E9;
						}
						break;
					case 7:
					case 8:
					case 11:
						goto IL_6E9;
					case 9:
					{
						if (!(text2 == "-language"))
						{
							goto IL_6E9;
						}
						LanguageIdentifier language = this.CommandLineArgs[j + 1].ToLanguageIdentifier();
						if (ServerLanguageOptions.Options.Any((ServerLanguageOptions.LanguageOption o) => o.Identifier == language))
						{
							GameMain.Server.ServerSettings.Language = language;
						}
						j++;
						goto IL_6E9;
					}
					case 10:
					{
						if (!(text2 == "-playstyle"))
						{
							goto IL_6E9;
						}
						PlayStyle playStyle;
						Enum.TryParse<PlayStyle>(this.CommandLineArgs[j + 1], out playStyle);
						GameMain.Server.ServerSettings.PlayStyle = playStyle;
						j++;
						goto IL_6E9;
					}
					case 12:
					{
						if (!(text2 == "-karmapreset"))
						{
							goto IL_6E9;
						}
						string karmaPresetName = this.CommandLineArgs[j + 1];
						GameMain.Server.ServerSettings.KarmaPreset = karmaPresetName;
						j++;
						goto IL_6E9;
					}
					case 13:
						if (!(text2 == "-karmaenabled"))
						{
							goto IL_6E9;
						}
						break;
					default:
						if (length != 20)
						{
							if (length != 22)
							{
								goto IL_6E9;
							}
							if (!(text2 == "-banafterwrongpassword"))
							{
								goto IL_6E9;
							}
							bool banAfterWrongPassword;
							bool.TryParse(this.CommandLineArgs[j + 1], out banAfterWrongPassword);
							GameMain.Server.ServerSettings.BanAfterWrongPassword = banAfterWrongPassword;
							goto IL_6E9;
						}
						else
						{
							if (!(text2 == "-multiclienttestmode"))
							{
								goto IL_6E9;
							}
							j++;
							goto IL_6E9;
						}
						break;
					}
					bool karmaEnabled;
					bool.TryParse(this.CommandLineArgs[j + 1], out karmaEnabled);
					GameMain.Server.ServerSettings.KarmaEnabled = karmaEnabled;
					j++;
				}
				IL_6E9:;
			}
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x00031DF3 File Offset: 0x0002FFF3
		public void CloseServer()
		{
			GameServer server = GameMain.Server;
			if (server != null)
			{
				server.Quit();
			}
			GameMain.ShouldRun = false;
			GameMain.Server = null;
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x00031E14 File Offset: 0x00030014
		public void Run()
		{
			HyperTypeDescriptionProvider.Add(typeof(Character));
			HyperTypeDescriptionProvider.Add(typeof(Item));
			HyperTypeDescriptionProvider.Add(typeof(ItemComponent));
			HyperTypeDescriptionProvider.Add(typeof(Hull));
			this.Init();
			this.StartServer();
			GameMain.ResetFrameTime();
			double frequency = (double)Stopwatch.Frequency;
			if (frequency <= 1500.0)
			{
				DebugConsole.NewMessage("WARNING: Stopwatch frequency under 1500 ticks per second. Expect significant syncing accuracy issues.", new Color?(Color.Yellow), false);
			}
			Stopwatch performanceCounterTimer = Stopwatch.StartNew();
			GameMain.stopwatch = Stopwatch.StartNew();
			long prevTicks = GameMain.stopwatch.ElapsedTicks;
			while (GameMain.ShouldRun)
			{
				long currTicks = GameMain.stopwatch.ElapsedTicks;
				double elapsedTime = (double)Math.Max(currTicks - prevTicks, 0L) / frequency;
				Timing.Accumulator += elapsedTime;
				if (Timing.Accumulator > Timing.AccumulatorMax)
				{
					Timing.Accumulator = 0.016666666666666666;
				}
				CrossThread.ProcessTasks();
				prevTicks = currTicks;
				while (Timing.Accumulator >= 0.016666666666666666)
				{
					performanceCounterTimer.Start();
					Timing.TotalTime += 0.016666666666666666;
					Timing.TotalTimeUnpaused += 0.016666666666666666;
					DebugConsole.Update();
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) == null || !GameMain.GameSession.GameMode.Paused)
					{
						Screen selected = Screen.Selected;
						if (selected != null)
						{
							selected.Update(0.01666666753590107);
						}
					}
					GameMain.Server.Update(0.016666668f);
					if (GameMain.Server == null)
					{
						break;
					}
					SteamManager.Update(0.016666668f);
					EosInterface.Core.Update();
					TaskPool.Update();
					CoroutineManager.Update(false, 0.016666668f);
					performanceCounterTimer.Stop();
					if (LuaCsSetup.Instance.PerformanceCounterService.EnablePerformanceCounter)
					{
						LuaCsSetup.Instance.PerformanceCounterService.AddElapsedTicks(new SimplePerformanceData("Update", performanceCounterTimer.ElapsedTicks));
					}
					if (LuaCsSetup.Instance.PerformanceCounter.EnablePerformanceCounter)
					{
						LuaCsSetup.Instance.PerformanceCounter.UpdateElapsedTime = (double)performanceCounterTimer.ElapsedTicks / (double)Stopwatch.Frequency;
					}
					performanceCounterTimer.Reset();
					Timing.Accumulator -= 0.016666666666666666;
					GameMain.updateCount++;
				}
				GameServer server = GameMain.Server;
				if (((server != null) ? server.OwnerConnection : null) == null)
				{
					DebugConsole.UpdateCommandLine((int)(Timing.Accumulator * 800.0));
				}
				else
				{
					DebugConsole.Clear();
				}
				int frameTime = (int)((double)(GameMain.stopwatch.ElapsedTicks - prevTicks) / frequency * 1000.0);
				frameTime = Math.Max(0, frameTime);
				Thread.Sleep(Math.Max((16 - frameTime) / 2, 0));
				if (performanceCounterTimer.ElapsedMilliseconds > 1000L)
				{
					int updateRate = (int)Math.Round((double)GameMain.updateCount / ((double)performanceCounterTimer.ElapsedMilliseconds / 1000.0));
					GameMain.prevUpdateRates.Enqueue(updateRate);
					if (GameMain.prevUpdateRates.Count >= 10)
					{
						int avgUpdateRate = (int)GameMain.prevUpdateRates.Average();
						if ((double)avgUpdateRate < 58.8 && GameMain.GameSession != null && (double)GameMain.GameSession.RoundDuration > 1.0)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Running slowly (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(avgUpdateRate);
							defaultInterpolatedStringHandler.AppendLiteral(" updates/s)!");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							if (GameMain.Server != null)
							{
								foreach (Client c in GameMain.Server.ConnectedClients)
								{
									if (c.Connection == GameMain.Server.OwnerConnection || c.Permissions != ClientPermissions.None)
									{
										GameServer server2 = GameMain.Server;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 1);
										defaultInterpolatedStringHandler2.AppendLiteral("Server running slowly (");
										defaultInterpolatedStringHandler2.AppendFormatted<int>(avgUpdateRate);
										defaultInterpolatedStringHandler2.AppendLiteral(" updates/s)!");
										server2.SendConsoleMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), c, new Color?(Color.Orange));
									}
								}
							}
						}
						GameMain.prevUpdateRates.Clear();
					}
					performanceCounterTimer.Restart();
					GameMain.updateCount = 0;
				}
			}
			GameMain.stopwatch.Stop();
			this.CloseServer();
			SteamManager.ShutDown();
			SaveUtil.CleanUnnecessarySaveFiles();
			if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.SaveLogs();
			}
			if (GameAnalyticsManager.SendUserStatistics)
			{
				GameAnalyticsManager.ShutDown();
			}
			GameMain.MainThread = null;
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00032294 File Offset: 0x00030494
		public static void ResetFrameTime()
		{
			Timing.Accumulator = 0.0;
			Stopwatch stopwatch = GameMain.stopwatch;
			if (stopwatch != null)
			{
				stopwatch.Restart();
			}
			GameMain.prevUpdateRates.Clear();
			GameMain.updateCount = 0;
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x000322C4 File Offset: 0x000304C4
		public CoroutineHandle ShowLoading(IEnumerable<CoroutineStatus> loader, bool waitKeyHit = true)
		{
			return CoroutineManager.StartCoroutine(loader, "");
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x000322D4 File Offset: 0x000304D4
		public void Exit()
		{
			GameMain.ShouldRun = false;
			try
			{
				if (LuaCsSetup.Instance != null)
				{
					LuaCsSetup.Instance.Dispose();
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error while disposing of LuaCsForBarotrauma: " + e.Message + " | " + e.StackTrace, null, null, false, false);
			}
		}

		// Token: 0x040002B3 RID: 691
		public static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;

		// Token: 0x040002B4 RID: 692
		private static World world;

		// Token: 0x040002B5 RID: 693
		public static GameServer Server;

		// Token: 0x040002B6 RID: 694
		public static GameSession GameSession;

		// Token: 0x040002B9 RID: 697
		public static GameScreen GameScreen;

		// Token: 0x040002BA RID: 698
		public static NetLobbyScreen NetLobbyScreen;

		// Token: 0x040002BB RID: 699
		public static readonly Screen SubEditorScreen = UnimplementedScreen.Instance;

		// Token: 0x040002BC RID: 700
		public static bool ShouldRun = true;

		// Token: 0x040002BD RID: 701
		private static Stopwatch stopwatch;

		// Token: 0x040002BE RID: 702
		private static readonly Queue<int> prevUpdateRates = new Queue<int>();

		// Token: 0x040002BF RID: 703
		private static int updateCount = 0;

		// Token: 0x040002C0 RID: 704
		public readonly string[] CommandLineArgs;
	}
}
