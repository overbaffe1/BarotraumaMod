using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.CharacterEditor;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Media;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using Barotrauma.Steam;
using Barotrauma.Transition;
using Barotrauma.Tutorials;
using EventInput;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Hyper.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x02000062 RID: 98
	internal class GameMain : Game
	{
		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06000D47 RID: 3399 RVA: 0x0007AE02 File Offset: 0x00079002
		public static bool IsSingleplayer
		{
			get
			{
				return GameMain.NetworkMember == null;
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06000D48 RID: 3400 RVA: 0x0007AE0C File Offset: 0x0007900C
		public static bool IsMultiplayer
		{
			get
			{
				return GameMain.NetworkMember != null;
			}
		}

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x06000D49 RID: 3401 RVA: 0x0007AE16 File Offset: 0x00079016
		// (set) Token: 0x06000D4A RID: 3402 RVA: 0x0007AE1D File Offset: 0x0007901D
		public static int CurrentUpdateRate { get; private set; }

		// Token: 0x06000D4B RID: 3403 RVA: 0x0007AE25 File Offset: 0x00079025
		public static void ResetNetLobbyScreen()
		{
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (netLobbyScreen != null)
			{
				netLobbyScreen.Release();
			}
			GameMain.NetLobbyScreen = new NetLobbyScreen();
			ModDownloadScreen modDownloadScreen = GameMain.ModDownloadScreen;
			if (modDownloadScreen != null)
			{
				modDownloadScreen.Release();
			}
			GameMain.ModDownloadScreen = new ModDownloadScreen();
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x06000D4C RID: 3404 RVA: 0x0007AE5B File Offset: 0x0007905B
		// (set) Token: 0x06000D4D RID: 3405 RVA: 0x0007AE62 File Offset: 0x00079062
		public static Thread MainThread { get; private set; }

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x06000D4E RID: 3406 RVA: 0x0007AE6A File Offset: 0x0007906A
		public static ContentPackage VanillaContent
		{
			get
			{
				return ContentPackageManager.VanillaCorePackage;
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x06000D4F RID: 3407 RVA: 0x0007AE71 File Offset: 0x00079071
		// (set) Token: 0x06000D50 RID: 3408 RVA: 0x0007AE78 File Offset: 0x00079078
		public static GameSession GameSession
		{
			get
			{
				return GameMain.gameSession;
			}
			set
			{
				if (GameMain.gameSession == value)
				{
					return;
				}
				GameSession gameSession = GameMain.gameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) != null && GameMain.gameSession.GameMode != ((value != null) ? value.GameMode : null))
				{
					GameMain.gameSession.GameMode.Remove();
				}
				GameMain.gameSession = value;
			}
		}

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06000D51 RID: 3409 RVA: 0x0007AECE File Offset: 0x000790CE
		// (set) Token: 0x06000D52 RID: 3410 RVA: 0x0007AEF5 File Offset: 0x000790F5
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

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x06000D53 RID: 3411 RVA: 0x0007AEFD File Offset: 0x000790FD
		// (set) Token: 0x06000D54 RID: 3412 RVA: 0x0007AF05 File Offset: 0x00079105
		public bool HasLoaded { get; private set; }

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000D55 RID: 3413 RVA: 0x0007AF10 File Offset: 0x00079110
		// (remove) Token: 0x06000D56 RID: 3414 RVA: 0x0007AF48 File Offset: 0x00079148
		public event Action ResolutionChanged;

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06000D57 RID: 3415 RVA: 0x0007AF7D File Offset: 0x0007917D
		// (set) Token: 0x06000D58 RID: 3416 RVA: 0x0007AF84 File Offset: 0x00079184
		public static bool IsExiting { get; private set; }

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x06000D59 RID: 3417 RVA: 0x0007AF8C File Offset: 0x0007918C
		// (set) Token: 0x06000D5A RID: 3418 RVA: 0x0007AF93 File Offset: 0x00079193
		public static bool IsFirstLaunch { get; private set; }

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x06000D5B RID: 3419 RVA: 0x0007AF9B File Offset: 0x0007919B
		// (set) Token: 0x06000D5C RID: 3420 RVA: 0x0007AFA2 File Offset: 0x000791A2
		public static GameMain Instance { get; private set; }

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x06000D5D RID: 3421 RVA: 0x0007AFAA File Offset: 0x000791AA
		// (set) Token: 0x06000D5E RID: 3422 RVA: 0x0007AFB1 File Offset: 0x000791B1
		public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06000D5F RID: 3423 RVA: 0x0007AFB9 File Offset: 0x000791B9
		// (set) Token: 0x06000D60 RID: 3424 RVA: 0x0007AFC0 File Offset: 0x000791C0
		public static WindowMode WindowMode { get; private set; }

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06000D61 RID: 3425 RVA: 0x0007AFC8 File Offset: 0x000791C8
		// (set) Token: 0x06000D62 RID: 3426 RVA: 0x0007AFCF File Offset: 0x000791CF
		public static int GraphicsWidth { get; private set; }

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06000D63 RID: 3427 RVA: 0x0007AFD7 File Offset: 0x000791D7
		// (set) Token: 0x06000D64 RID: 3428 RVA: 0x0007AFDE File Offset: 0x000791DE
		public static int GraphicsHeight { get; private set; }

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06000D65 RID: 3429 RVA: 0x0007AFE8 File Offset: 0x000791E8
		public static bool WindowActive
		{
			get
			{
				bool result;
				try
				{
					result = (GameMain.Instance != null && !GameMain.IsExiting && GameMain.Instance.IsActive);
				}
				catch (NullReferenceException)
				{
					result = false;
				}
				return result;
			}
		}

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06000D66 RID: 3430 RVA: 0x0007B02C File Offset: 0x0007922C
		public static NetworkMember NetworkMember
		{
			get
			{
				return GameMain.Client;
			}
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x06000D67 RID: 3431 RVA: 0x0007B033 File Offset: 0x00079233
		// (set) Token: 0x06000D68 RID: 3432 RVA: 0x0007B03A File Offset: 0x0007923A
		public static RasterizerState ScissorTestEnable { get; private set; }

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x06000D69 RID: 3433 RVA: 0x0007B042 File Offset: 0x00079242
		public bool LoadingScreenOpen
		{
			get
			{
				return this.loadingScreenOpen;
			}
		}

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x06000D6A RID: 3434 RVA: 0x0007B04A File Offset: 0x0007924A
		// (set) Token: 0x06000D6B RID: 3435 RVA: 0x0007B052 File Offset: 0x00079252
		public bool Paused { get; private set; }

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x06000D6C RID: 3436 RVA: 0x0007B05B File Offset: 0x0007925B
		// (set) Token: 0x06000D6D RID: 3437 RVA: 0x0007B062 File Offset: 0x00079262
		public static ChatMode ActiveChatMode { get; set; } = ChatMode.Radio;

		// Token: 0x06000D6E RID: 3438 RVA: 0x0007B06C File Offset: 0x0007926C
		public GameMain(string[] args)
		{
			base.Content.RootDirectory = "Content";
			GameMain.GraphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				GraphicsProfile = GraphicsProfile.Reach
			};
			GameMain.GraphicsDeviceManager.ApplyChanges();
			base.Window.Title = "Barotrauma";
			GameMain.Instance = this;
			if (!Directory.Exists(base.Content.RootDirectory))
			{
				throw new Exception("Content folder not found. If you are trying to compile the game from the source code and own a legal copy of the game, you can copy the Content folder from the game's files to BarotraumaShared/Content.");
			}
			GameSettings.Init();
			CreatureMetrics.Init();
			this.ConsoleArguments = args.ToImmutableArray<string>();
			this.EgsExchangeCode = EosInterface.Login.ParseEgsExchangeCode(args);
			try
			{
				this.ConnectCommand = Barotrauma.Networking.ConnectCommand.Parse(this.ConsoleArguments);
				string clientNameFlagArg = args.FirstOrDefault((string arg) => arg.StartsWith("-username"));
				if (clientNameFlagArg != null)
				{
					int nextIndex = args.IndexOf(clientNameFlagArg) + 1;
					if (nextIndex < args.Length)
					{
						this.clientName = args[nextIndex];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				DebugConsole.ThrowError("Failed to parse console arguments (" + string.Join<string>(' ', this.ConsoleArguments) + ")", e, null, false, false);
				this.ConnectCommand = Option<Barotrauma.Networking.ConnectCommand>.None();
			}
			if (this.ConsoleArguments.Contains("-lenienthandshake"))
			{
				NetConfig.UseLenientHandshake = true;
			}
			GUI.KeyboardDispatcher = new KeyboardDispatcher(base.Window);
			GameMain.PerformanceCounter = new PerformanceCounter();
			base.IsFixedTimeStep = false;
			GameMain.ResetFrameTime();
			this.fixedTime = new GameTime();
			Settings.AllowSleep = true;
			Settings.ContinuousPhysics = false;
			Settings.VelocityIterations = 1;
			Settings.PositionIterations = 1;
			GameMain.MainThread = Thread.CurrentThread;
			GameWindow window = base.Window;
			EventHandler<FileDropEventArgs> value;
			if ((value = GameMain.<>O.<0>__OnFileDropped) == null)
			{
				value = (GameMain.<>O.<0>__OnFileDropped = new EventHandler<FileDropEventArgs>(GameMain.OnFileDropped));
			}
			window.FileDropped += value;
			LuaCsSetup.Instance.GetType();
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x0007B258 File Offset: 0x00079458
		public static void ExecuteAfterContentFinishedLoading(Action action)
		{
			if (GameMain.contentLoaded)
			{
				action();
				return;
			}
			GameMain.postContentLoadActions.Enqueue(action);
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x0007B274 File Offset: 0x00079474
		public static void OnFileDropped(object sender, FileDropEventArgs args)
		{
			Screen screen = Screen.Selected;
			if (screen == null)
			{
				return;
			}
			string filePath = args.FilePath;
			if (string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}
			string extension = Path.GetExtension(filePath).ToLower();
			FileInfo info = new FileInfo(args.FilePath);
			if (!info.Exists)
			{
				return;
			}
			screen.OnFileDropped(filePath, extension);
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x0007B2C4 File Offset: 0x000794C4
		public void ApplyGraphicsSettings(bool recalculateFontsAndStyles = false)
		{
			int display = GameSettings.CurrentConfig.Graphics.Display;
			GameMain.GraphicsWidth = GameSettings.CurrentConfig.Graphics.Width;
			GameMain.GraphicsHeight = GameSettings.CurrentConfig.Graphics.Height;
			if (GameMain.GraphicsWidth <= 0 || GameMain.GraphicsHeight <= 0)
			{
				GameMain.GraphicsWidth = base.GraphicsDevice.DisplayMode.Width;
				GameMain.GraphicsHeight = base.GraphicsDevice.DisplayMode.Height;
				GameMain.<ApplyGraphicsSettings>g__updateConfig|117_0();
			}
			WindowMode displayMode = GameSettings.CurrentConfig.Graphics.DisplayMode;
			if (displayMode != WindowMode.Windowed)
			{
				if (displayMode == WindowMode.BorderlessWindowed)
				{
					GameMain.GraphicsWidth = base.GraphicsDevice.DisplayMode.Width;
					GameMain.GraphicsHeight = base.GraphicsDevice.DisplayMode.Height;
					GameMain.<ApplyGraphicsSettings>g__updateConfig|117_0();
				}
			}
			else
			{
				GameMain.GraphicsWidth = Math.Min(base.GraphicsDevice.DisplayMode.Width, GameMain.GraphicsWidth);
				GameMain.GraphicsHeight = Math.Min(base.GraphicsDevice.DisplayMode.Height, GameMain.GraphicsHeight);
				GameMain.<ApplyGraphicsSettings>g__updateConfig|117_0();
			}
			GameMain.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.Reach;
			GameMain.GraphicsDeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
			GameMain.GraphicsDeviceManager.PreferMultiSampling = false;
			GameMain.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = GameSettings.CurrentConfig.Graphics.VSync;
			this.SetWindowMode(GameSettings.CurrentConfig.Graphics.DisplayMode, display);
			this.defaultViewport = new Viewport(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			if (recalculateFontsAndStyles)
			{
				GUIStyle.RecalculateFonts();
				GUIStyle.RecalculateSizeRestrictions();
			}
			Action resolutionChanged = this.ResolutionChanged;
			if (resolutionChanged == null)
			{
				return;
			}
			resolutionChanged();
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x0007B458 File Offset: 0x00079658
		public void SetWindowMode(WindowMode windowMode, int display)
		{
			WindowMode prevDisplayMode = GameMain.WindowMode;
			if (base.Window.TargetDisplay != display && prevDisplayMode != WindowMode.Windowed)
			{
				GameMain.GraphicsDeviceManager.IsFullScreen = false;
				GameMain.GraphicsDeviceManager.ApplyChanges();
			}
			base.Window.TargetDisplay = display;
			GameMain.WindowMode = windowMode;
			GameMain.GraphicsDeviceManager.HardwareModeSwitch = (windowMode != WindowMode.BorderlessWindowed);
			GameMain.GraphicsDeviceManager.IsFullScreen = (windowMode == WindowMode.Fullscreen || windowMode == WindowMode.BorderlessWindowed);
			base.Window.IsBorderless = !GameMain.GraphicsDeviceManager.HardwareModeSwitch;
			GameMain.GraphicsDeviceManager.PreferredBackBufferWidth = GameMain.GraphicsWidth;
			GameMain.GraphicsDeviceManager.PreferredBackBufferHeight = GameMain.GraphicsHeight;
			GameMain.GraphicsDeviceManager.ApplyChanges();
			if (windowMode == WindowMode.BorderlessWindowed)
			{
				GameMain.GraphicsWidth = base.GraphicsDevice.PresentationParameters.Bounds.Width;
				GameMain.GraphicsHeight = base.GraphicsDevice.PresentationParameters.Bounds.Height;
				base.GraphicsDevice.Viewport = new Viewport(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
				base.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
				GameMain.GraphicsDeviceManager.PreferredBackBufferWidth = GameMain.GraphicsWidth;
				GameMain.GraphicsDeviceManager.PreferredBackBufferHeight = GameMain.GraphicsHeight;
				GameMain.GraphicsDeviceManager.ApplyChanges();
			}
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x0007B5A4 File Offset: 0x000797A4
		public void ResetViewPort()
		{
			base.GraphicsDevice.Viewport = this.defaultViewport;
			base.GraphicsDevice.ScissorRectangle = this.defaultViewport.Bounds;
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x0007B5D0 File Offset: 0x000797D0
		protected override void Initialize()
		{
			base.Initialize();
			this.ApplyGraphicsSettings(false);
			GameMain.ScissorTestEnable = new RasterizerState
			{
				ScissorTestEnable = true
			};
			HyperTypeDescriptionProvider.Add(typeof(Character));
			HyperTypeDescriptionProvider.Add(typeof(Item));
			HyperTypeDescriptionProvider.Add(typeof(ItemComponent));
			HyperTypeDescriptionProvider.Add(typeof(Hull));
			GameMain.performanceCounterTimer = Stopwatch.StartNew();
			GameMain.ResetIMEWorkaround();
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x0007B648 File Offset: 0x00079848
		protected override void LoadContent()
		{
			GameMain.GraphicsWidth = base.GraphicsDevice.Viewport.Width;
			GameMain.GraphicsHeight = base.GraphicsDevice.Viewport.Height;
			this.ApplyGraphicsSettings(false);
			ConvertUnits.SetDisplayUnitToSimUnitRatio(100f);
			GameMain.spriteBatch = new SpriteBatch(base.GraphicsDevice);
			TextureLoader.Init(base.GraphicsDevice, false);
			WaterRenderer.Instance = new WaterRenderer(base.GraphicsDevice);
			GraphicsQuad.Init(base.GraphicsDevice);
			this.loadingScreenOpen = true;
			GameMain.TitleScreen = new LoadingScreen(base.GraphicsDevice)
			{
				WaitForLanguageSelection = (GameSettings.CurrentConfig.Language == LanguageIdentifier.None)
			};
			EosAccount.LoginPlatformSpecific();
			this.initialLoadingThread = new Thread(new ThreadStart(this.Load))
			{
				Name = "Load"
			};
			this.initialLoadingThread.Start();
		}

		// Token: 0x06000D76 RID: 3446 RVA: 0x0007B730 File Offset: 0x00079930
		private unsafe void Load()
		{
			GameMain.<Load>g__log|122_0("LOADING COROUTINE");
			ContentPackageManager.LoadVanillaFileList();
			if (GameMain.TitleScreen.WaitForLanguageSelection)
			{
				ContentPackageManager.VanillaCorePackage.LoadFilesOfType<TextFile>();
				GameMain.TitleScreen.AvailableLanguages = TextManager.AvailableLanguages.OrderBy(delegate(LanguageIdentifier l)
				{
					Identifier identifier = "english".ToIdentifier();
					return l.Value != identifier;
				}).ThenBy((LanguageIdentifier l) => l.Value).ToArray<LanguageIdentifier>();
				while (GameMain.TitleScreen.WaitForLanguageSelection)
				{
					Thread.Sleep(16);
				}
				LanguageIdentifier selectedLanguage = GameSettings.CurrentConfig.Language;
				ContentPackageManager.VanillaCorePackage.UnloadFilesOfType<TextFile>();
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Language = selectedLanguage;
				GameSettings.SetCurrentConfig(config);
				GameSettings.SaveCurrentConfig();
			}
			GameMain.SoundManager = new SoundManager();
			GameMain.SoundManager.ApplySettings();
			if (GameSettings.CurrentConfig.EnableSplashScreen && !this.ConsoleArguments.Contains("-skipintro"))
			{
				ConcurrentQueue<LoadingScreen.PendingSplashScreen> pendingSplashScreens = GameMain.TitleScreen.PendingSplashScreens;
				float baseVolume = MathHelper.Clamp(GameSettings.CurrentConfig.Audio.SoundVolume * 2f, 0f, 1f);
				pendingSplashScreens.Enqueue(new LoadingScreen.PendingSplashScreen("Content/SplashScreens/Splash_UTG.webm", baseVolume * 0.5f));
				pendingSplashScreens.Enqueue(new LoadingScreen.PendingSplashScreen("Content/SplashScreens/Splash_FF.webm", baseVolume));
				pendingSplashScreens.Enqueue(new LoadingScreen.PendingSplashScreen("Content/SplashScreens/Splash_Daedalic.webm", baseVolume * 0.1f));
			}
			GUI.Init();
			LegacySteamUgcTransition.Prepare();
			IEnumerable<ContentPackageManager.LoadProgress> contentPackageLoadRoutine = ContentPackageManager.Init();
			foreach (float progress in (from p in contentPackageLoadRoutine
			select p.Result).Successes<float, ContentPackageManager.LoadProgress.Error>())
			{
				if (GameMain.IsExiting)
				{
					break;
				}
				GameMain.TitleScreen.LoadState = MathHelper.Lerp(1f, 70f, progress);
			}
			if (GameMain.IsExiting)
			{
				return;
			}
			CorePackage corePackage = ContentPackageManager.EnabledPackages.Core;
			ContentPackageManager.LoadProgress.Error error;
			if (corePackage.EnableError.TryUnwrap(out error))
			{
				ImmutableArray<string> errorMessages;
				if (error.ErrorsOrException.TryGet(out errorMessages))
				{
					throw new Exception("Error while loading the core content package \"" + corePackage.Name + "\": " + errorMessages.First<string>());
				}
				Exception exception;
				if (error.ErrorsOrException.TryGet(out exception))
				{
					throw new Exception("Error while loading the core content package \"" + corePackage.Name + "\": " + exception.Message, exception);
				}
			}
			TextManager.VerifyLanguageAvailable();
			SocialOverlay.Init();
			DebugConsole.Init();
			ContentPackageManager.LogEnabledRegularPackageErrors();
			GameAnalyticsManager.InitIfConsented();
			TaskPool.Add("InitRelayNetworkAccess", SteamManager.InitRelayNetworkAccess(), delegate(Task t)
			{
			});
			HintManager.Init();
			CoreEntityPrefab.InitCorePrefabs();
			GameModePreset.Init();
			SaveUtil.DeleteDownloadedSubs();
			SubmarineInfo.RefreshSavedSubs();
			GameMain.TitleScreen.LoadState = 75f;
			GameMain.GameScreen = new GameScreen(GameMain.GraphicsDeviceManager.GraphicsDevice);
			GameMain.ParticleManager = new ParticleManager(GameMain.GameScreen.Cam);
			GameMain.LightManager = new LightManager(base.GraphicsDevice);
			GameMain.TitleScreen.LoadState = 80f;
			GameMain.MainMenuScreen = new MainMenuScreen(this);
			GameMain.ServerListScreen = new ServerListScreen();
			GameMain.TitleScreen.LoadState = 85f;
			if (SteamManager.IsInitialized)
			{
				SteamFriends.OnGameRichPresenceJoinRequested += this.OnInvitedToSteamGame;
				SteamFriends.OnGameLobbyJoinRequested += this.OnSteamLobbyJoinRequested;
				List<Achievement> achievements;
				if (SteamManager.TryGetUnlockedAchievements(out achievements) && !achievements.Any<Achievement>() && SteamManager.GetStatInt(AchievementStat.GameLaunchCount) <= 0)
				{
					GameMain.IsFirstLaunch = true;
					GameAnalyticsManager.AddDesignEvent("FirstLaunch");
				}
				SteamManager.IncrementStat(AchievementStat.GameLaunchCount, 1, true);
			}
			Action action2;
			if ((action2 = GameMain.<>O.<1>__ProcessLaunchCountEos) == null)
			{
				action2 = (GameMain.<>O.<1>__ProcessLaunchCountEos = new Action(GameMain.ProcessLaunchCountEos));
			}
			EosAccount.ExecuteAfterLogin(action2);
			GameMain.SubEditorScreen = new SubEditorScreen();
			GameMain.TestScreen = new TestScreen();
			GameMain.TitleScreen.LoadState = 90f;
			GameMain.ParticleEditorScreen = new ParticleEditorScreen();
			GameMain.TitleScreen.LoadState = 95f;
			GameMain.LevelEditorScreen = new LevelEditorScreen();
			GameMain.SpriteEditorScreen = new SpriteEditorScreen();
			GameMain.EventEditorScreen = new EventEditorScreen();
			GameMain.CharacterEditorScreen = new CharacterEditorScreen();
			GameMain.CampaignEndScreen = new CampaignEndScreen();
			GameMain.MainMenuScreen.Select();
			ContainerTagPrefab.CheckForContainerTagErrors();
			foreach (Identifier steamError in SteamManager.InitializationErrors)
			{
				new GUIMessageBox(TextManager.Get("Error"), TextManager.Get(steamError), null, null, GUIMessageBox.Type.Default);
			}
			Action onGameMainHasLoaded = GameSettings.OnGameMainHasLoaded;
			if (onGameMainHasLoaded != null)
			{
				onGameMainHasLoaded();
			}
			GameMain.TitleScreen.LoadState = 100f;
			this.HasLoaded = true;
			GameMain.<Load>g__log|122_0("LOADING COROUTINE FINISHED");
			GameMain.contentLoaded = true;
			Action action;
			while (GameMain.postContentLoadActions.TryDequeue(out action))
			{
				action();
			}
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x0007BC60 File Offset: 0x00079E60
		private static void ProcessLaunchCountEos()
		{
			if (!EosInterface.Core.IsInitialized)
			{
				return;
			}
			EosInterface.Presence.OnJoinGame.Register("onJoinGame".ToIdentifier(), delegate(EosInterface.Presence.JoinGameInfo jgi)
			{
				GameMain.<ProcessLaunchCountEos>g__trySetConnectCommand|123_0(jgi.JoinCommand);
			});
			EosInterface.Presence.OnInviteAccepted.Register("onInviteAccepted".ToIdentifier(), delegate(EosInterface.Presence.AcceptInviteInfo aii)
			{
				GameMain.<ProcessLaunchCountEos>g__trySetConnectCommand|123_0(aii.JoinCommand);
			});
			TaskPool.AddWithResult<Result<ImmutableDictionary<AchievementStat, int>, EosInterface.QueryStatsError>>("Eos.GameMain.Load.QueryStats", EosInterface.Achievements.QueryStats(new AchievementStat[1]), delegate([Nullable(new byte[]
			{
				1,
				0
			})] Result<ImmutableDictionary<AchievementStat, int>, EosInterface.QueryStatsError> result)
			{
				result.Match(delegate(ImmutableDictionary<AchievementStat, int> stats)
				{
					int launchCount;
					if (!stats.TryGetValue(AchievementStat.GameLaunchCount, out launchCount))
					{
						return;
					}
					if (launchCount > 0)
					{
						return;
					}
					GameMain.IsFirstLaunch = true;
					GameAnalyticsManager.AddDesignEvent("FirstLaunch_Epic");
				}, delegate(EosInterface.QueryStatsError error)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to query stats for launch count: ");
					defaultInterpolatedStringHandler.AppendFormatted<EosInterface.QueryStatsError>(error);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				});
				string name = "Eos.GameMain.Load.IngestStat";
				Task task = EosInterface.Achievements.IngestStats(new ValueTuple<AchievementStat, int>[]
				{
					new ValueTuple<AchievementStat, int>(AchievementStat.GameLaunchCount, 1)
				});
				Action<Task> onCompletion;
				if ((onCompletion = GameMain.<>O.<2>__IgnoredCallback) == null)
				{
					onCompletion = (GameMain.<>O.<2>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
				}
				TaskPool.Add(name, task, onCompletion);
			});
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x0007BD10 File Offset: 0x00079F10
		protected override void UnloadContent()
		{
			TextureLoader.CancelAll();
			CoroutineManager.StopCoroutines("Load");
			Video.Close();
			VoipCapture instance = VoipCapture.Instance;
			if (instance != null)
			{
				instance.Dispose();
			}
			SoundManager soundManager = GameMain.SoundManager;
			if (soundManager != null)
			{
				soundManager.Dispose();
			}
			GameMain.MainThread = null;
		}

		// Token: 0x06000D79 RID: 3449 RVA: 0x0007BD4C File Offset: 0x00079F4C
		private void OnInvitedToSteamGame(Friend friend, string connectCommand)
		{
			this.OnInvitedToSteamGame(connectCommand);
		}

		// Token: 0x06000D7A RID: 3450 RVA: 0x0007BD58 File Offset: 0x00079F58
		private void OnInvitedToSteamGame(string connectCommand)
		{
			DebugConsole.NewMessage("Invited to Steam game, connect command: " + connectCommand, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
			try
			{
				this.ConnectCommand = Barotrauma.Networking.ConnectCommand.Parse(ToolBox.SplitCommand(connectCommand));
			}
			catch (IndexOutOfRangeException e)
			{
				DebugConsole.Log("Failed to parse a Steam friend's connect invitation command (" + connectCommand + ")\n" + e.StackTrace.CleanupStackTrace());
				this.ConnectCommand = Option<Barotrauma.Networking.ConnectCommand>.None();
			}
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x0007BDD4 File Offset: 0x00079FD4
		private void OnSteamLobbyJoinRequested(Lobby lobby, Steamworks.SteamId friendId)
		{
			SteamManager.JoinLobby(lobby.Id, true);
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x0007BDE8 File Offset: 0x00079FE8
		protected override void Update(GameTime gameTime)
		{
			Timing.Accumulator += gameTime.ElapsedGameTime.TotalSeconds;
			if (Timing.Accumulator > Timing.AccumulatorMax)
			{
				Timing.Accumulator = 0.016666666666666666;
			}
			CrossThread.ProcessTasks();
			PlayerInput.UpdateVariable();
			if (GameMain.SoundManager != null)
			{
				if (GameMain.WindowActive || !GameSettings.CurrentConfig.Audio.MuteOnFocusLost)
				{
					GameMain.SoundManager.ListenerGain = GameMain.SoundManager.CompressionDynamicRangeGain;
				}
				else
				{
					GameMain.SoundManager.ListenerGain = 0f;
				}
			}
			while (Timing.Accumulator >= 0.016666666666666666)
			{
				Timing.TotalTime += 0.016666666666666666;
				if (!this.Paused)
				{
					Timing.TotalTimeUnpaused += 0.016666666666666666;
				}
				Stopwatch sw = new Stopwatch();
				sw.Start();
				this.fixedTime.IsRunningSlowly = gameTime.IsRunningSlowly;
				TimeSpan addTime = new TimeSpan(0, 0, 0, 0, 16);
				this.fixedTime.ElapsedGameTime = addTime;
				this.fixedTime.TotalGameTime = this.fixedTime.TotalGameTime.Add(addTime);
				base.Update(this.fixedTime);
				PlayerInput.Update(0.016666666666666666);
				SocialOverlay instance = SocialOverlay.Instance;
				if (instance != null)
				{
					instance.Update();
				}
				if (this.loadingScreenOpen)
				{
					GameMain.ResetFrameTime();
					if (!GameMain.TitleScreen.PlayingSplashScreen)
					{
						SoundPlayer.Update(0.016666668f);
						GUI.ClearUpdateList();
						GUI.UpdateGUIMessageBoxesOnly(0.016666668f);
					}
					if (GameMain.TitleScreen.LoadState >= 100f && !GameMain.TitleScreen.PlayingSplashScreen && (!GameMain.waitForKeyHit || ((PlayerInput.GetKeyboardState.GetPressedKeys().Length != 0 || PlayerInput.PrimaryMouseButtonClicked()) && GameMain.WindowActive)))
					{
						this.loadingScreenOpen = false;
					}
					GameClient client = GameMain.Client;
					if (client != null)
					{
						client.Update(0.016666668f);
					}
				}
				else if (this.HasLoaded)
				{
					ConnectCommand connectCommand;
					if (this.ConnectCommand.TryUnwrap(out connectCommand))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Processing connect command: ");
						defaultInterpolatedStringHandler.AppendFormatted<ConnectCommand>(connectCommand);
						defaultInterpolatedStringHandler.AppendLiteral("...");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
						if (GameMain.Client != null)
						{
							GameMain.Client.Quit();
							GameMain.Client = null;
						}
						GameMain.MainMenuScreen.Select();
						string clientNameString = this.clientName ?? MultiplayerPreferences.Instance.PlayerName.FallbackNullOrEmpty(SteamManager.GetUsername());
						ConnectCommand.SteamLobbyId lobbyId;
						ConnectCommand.NameAndP2PEndpoints nameAndEndpoint;
						ConnectCommand.NameAndLidgrenEndpoint nameAndLidgrenEndpoint;
						if (connectCommand.SteamLobbyIdOption.TryUnwrap(out lobbyId))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(26, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Connecting to lobby ID ");
							defaultInterpolatedStringHandler2.AppendFormatted<ConnectCommand.SteamLobbyId>(lobbyId);
							defaultInterpolatedStringHandler2.AppendLiteral("...");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
							SteamManager.JoinLobby(lobbyId.Value, true);
						}
						else if (connectCommand.NameAndP2PEndpointsOption.TryUnwrap(out nameAndEndpoint))
						{
							string serverName = nameAndEndpoint.ServerName;
							ImmutableArray<P2PEndpoint> endpoints = nameAndEndpoint.Endpoints;
							GameMain.Client = new GameClient(clientNameString, endpoints.Cast<Endpoint>().ToImmutableArray<Endpoint>(), string.IsNullOrWhiteSpace(serverName) ? endpoints.First<P2PEndpoint>().StringRepresentation : serverName, Option<int>.None());
							DebugConsole.NewMessage("Connecting to endpoint " + endpoints.First<P2PEndpoint>().StringRepresentation + "...", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
						}
						else if (connectCommand.NameAndLidgrenEndpointOption.TryUnwrap(out nameAndLidgrenEndpoint))
						{
							string lidgrenServerName = nameAndLidgrenEndpoint.ServerName;
							LidgrenEndpoint endpoint = nameAndLidgrenEndpoint.Endpoint;
							GameMain.Client = new GameClient(clientNameString, endpoint, string.IsNullOrWhiteSpace(lidgrenServerName) ? endpoint.StringRepresentation : lidgrenServerName, Option<int>.None());
						}
						else
						{
							DebugConsole.NewMessage("Cannot connect: unrecognized connect command.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
						}
						this.ConnectCommand = Option<Barotrauma.Networking.ConnectCommand>.None();
					}
					SoundPlayer.Update(0.016666668f);
					if ((PlayerInput.KeyDown(Keys.LeftControl) || PlayerInput.KeyDown(Keys.RightControl)) && (PlayerInput.KeyDown(Keys.LeftShift) || PlayerInput.KeyDown(Keys.RightShift)) && PlayerInput.KeyHit(Keys.Tab))
					{
						SocialOverlay socialOverlay = SocialOverlay.Instance;
						if (socialOverlay != null)
						{
							socialOverlay.IsOpen = !socialOverlay.IsOpen;
							if (socialOverlay.IsOpen)
							{
								socialOverlay.RefreshFriendList();
							}
						}
					}
					if (PlayerInput.KeyHit(Keys.Escape) && GameMain.WindowActive)
					{
						if (GUI.KeyboardDispatcher.Subscriber != null)
						{
							GUITextBox textBox = GUI.KeyboardDispatcher.Subscriber as GUITextBox;
							if (textBox != null)
							{
								textBox.Deselect();
							}
							GUI.KeyboardDispatcher.Subscriber = null;
						}
						else
						{
							SocialOverlay instance2 = SocialOverlay.Instance;
							if (instance2 != null && instance2.IsOpen)
							{
								SocialOverlay.Instance.IsOpen = false;
							}
							else
							{
								GUIComponent visibleBox = GUIMessageBox.VisibleBox;
								if (visibleBox is GUIMessageBox)
								{
									string text = visibleBox.UserData as string;
									if (text != null && text == "verificationprompt")
									{
										((GUIMessageBox)GUIMessageBox.VisibleBox).Close();
										goto IL_64C;
									}
								}
								GUIComponent visibleBox2 = GUIMessageBox.VisibleBox;
								RoundSummary roundSummary = ((visibleBox2 != null) ? visibleBox2.UserData : null) as RoundSummary;
								if (roundSummary != null)
								{
									GUIButton continueButton = roundSummary.ContinueButton;
									if (continueButton != null && continueButton.Visible)
									{
										GUIMessageBox.MessageBoxes.Remove(GUIMessageBox.VisibleBox);
										goto IL_64C;
									}
								}
								if (ObjectiveManager.ContentRunning)
								{
									ObjectiveManager.CloseActiveContentGUI();
								}
								else if (GameSession.IsTabMenuOpen)
								{
									GameMain.gameSession.ToggleTabMenu();
								}
								else
								{
									visibleBox = GUIMessageBox.VisibleBox;
									if (visibleBox is GUIMessageBox)
									{
										string text = visibleBox.UserData as string;
										if (text != null && text == "bugreporter")
										{
											((GUIMessageBox)GUIMessageBox.VisibleBox).Close();
											goto IL_64C;
										}
									}
									if (GUI.PauseMenuOpen)
									{
										GUI.TogglePauseMenu();
									}
									else
									{
										GameSession gameSession = GameMain.GameSession;
										CampaignMode campaignMode = (gameSession != null) ? gameSession.Campaign : null;
										if (campaignMode != null && campaignMode.ShowCampaignUI && !campaignMode.ForceMapUI)
										{
											GameMain.GameSession.Campaign.ShowCampaignUI = false;
										}
										else if ((Character.Controlled == null || !GameMain.<Update>g__itemHudActive|128_0()) && CharacterHealth.OpenHealthWindow == null && !CrewManager.IsCommandInterfaceOpen)
										{
											SubEditorScreen editor = Screen.Selected as SubEditorScreen;
											if (editor != null && !editor.WiringMode)
											{
												Character controlled = Character.Controlled;
												if (((controlled != null) ? controlled.SelectedItem : null) != null)
												{
													goto IL_64C;
												}
											}
											GUI.TogglePauseMenu();
										}
									}
								}
							}
						}
					}
					IL_64C:
					GUI.ClearUpdateList();
					if (DebugConsole.IsOpen || DebugConsole.Paused || GUI.PauseMenuOpen || GUI.SettingsMenuOpen)
					{
						goto IL_68D;
					}
					GameSession gameSession2 = GameMain.GameSession;
					if (((gameSession2 != null) ? gameSession2.GameMode : null) is TutorialMode && ObjectiveManager.ContentRunning)
					{
						goto IL_68D;
					}
					bool paused = false;
					IL_6A7:
					this.Paused = paused;
					GameSession gameSession3 = GameMain.GameSession;
					if (((gameSession3 != null) ? gameSession3.GameMode : null) != null && GameMain.GameSession.GameMode.Paused)
					{
						this.Paused = true;
						GameMain.GameSession.GameMode.UpdateWhilePaused(0.016666668f);
					}
					if (GameMain.NetworkMember == null && !GameMain.WindowActive && !this.Paused && GameSettings.CurrentConfig.PauseOnFocusLost && Screen.Selected != GameMain.MainMenuScreen && Screen.Selected != GameMain.ServerListScreen && Screen.Selected != GameMain.NetLobbyScreen && Screen.Selected != GameMain.SubEditorScreen && Screen.Selected != GameMain.LevelEditorScreen)
					{
						GUI.TogglePauseMenu();
						this.Paused = true;
					}
					Screen.Selected.AddToGUIUpdateList();
					GameClient client2 = GameMain.Client;
					if (client2 != null)
					{
						client2.AddToGUIUpdateList();
					}
					SubmarinePreview.AddToGUIUpdateList();
					FileSelection.AddToGUIUpdateList();
					DebugConsole.AddToGUIUpdateList();
					DebugConsole.Update(0.016666668f);
					if (!this.Paused)
					{
						Screen.Selected.Update(0.016666666666666666);
					}
					else
					{
						if (ObjectiveManager.ContentRunning)
						{
							GameSession gameSession4 = GameMain.GameSession;
							TutorialMode tutorialMode = ((gameSession4 != null) ? gameSession4.GameMode : null) as TutorialMode;
							if (tutorialMode != null)
							{
								ObjectiveManager.VideoPlayer.Update();
								tutorialMode.Update(0.016666668f);
								goto IL_81D;
							}
						}
						if (Screen.Selected.Cam == null)
						{
							DebugConsole.Paused = false;
						}
						else
						{
							Screen.Selected.Cam.MoveCamera(0.016666668f, DebugConsole.Paused, DebugConsole.Paused, true, null);
						}
					}
					IL_81D:
					GameClient client3 = GameMain.Client;
					if (client3 != null)
					{
						client3.Update(0.016666668f);
					}
					GUI.Update(0.016666668f);
					goto IL_83C;
					IL_68D:
					paused = (GameMain.NetworkMember == null || !GameMain.NetworkMember.GameStarted);
					goto IL_6A7;
				}
				IL_83C:
				CoroutineManager.Update(this.Paused, 0.016666668f);
				SteamManager.Update(0.016666668f);
				EosInterface.Core.Update();
				TaskPool.Update();
				SoundManager soundManager = GameMain.SoundManager;
				if (soundManager != null)
				{
					soundManager.Update();
				}
				Timing.Accumulator -= 0.016666666666666666;
				GameMain.updateCount++;
				sw.Stop();
				GameMain.PerformanceCounter.AddElapsedTicks("Update", sw.ElapsedTicks);
				GameMain.PerformanceCounter.UpdateTimeGraph.Update((float)sw.ElapsedTicks * 1000f / (float)Stopwatch.Frequency);
			}
			if (!this.Paused)
			{
				Timing.Alpha = Timing.Accumulator / 0.016666666666666666;
			}
			if (GameMain.performanceCounterTimer.ElapsedMilliseconds > 1000L)
			{
				GameMain.CurrentUpdateRate = (int)Math.Round((double)GameMain.updateCount / ((double)GameMain.performanceCounterTimer.ElapsedMilliseconds / 1000.0));
				GameMain.performanceCounterTimer.Restart();
				GameMain.updateCount = 0;
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x0007C73C File Offset: 0x0007A93C
		public static void ResetFrameTime()
		{
			Timing.Accumulator = 0.0;
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x0007C74C File Offset: 0x0007A94C
		private void FixRazerCortex()
		{
			BlendState oldBlendState = base.GraphicsDevice.BlendState;
			base.GraphicsDevice.BlendState = ((oldBlendState == BlendState.Opaque) ? BlendState.NonPremultiplied : BlendState.Opaque);
			base.GraphicsDevice.BlendState = oldBlendState;
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x0007C790 File Offset: 0x0007A990
		protected override void Draw(GameTime gameTime)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			this.FixRazerCortex();
			double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
			if (Timing.FrameLimit > 0)
			{
				double step = 1.0 / (double)Timing.FrameLimit;
				while (!GameSettings.CurrentConfig.Graphics.VSync && sw.Elapsed.TotalSeconds + deltaTime < step)
				{
					Thread.Sleep(1);
				}
			}
			GameMain.PerformanceCounter.Update(sw.Elapsed.TotalSeconds + deltaTime);
			if (this.loadingScreenOpen)
			{
				GameMain.TitleScreen.Draw(GameMain.spriteBatch, base.GraphicsDevice, (float)deltaTime);
			}
			else if (this.HasLoaded)
			{
				Screen.Selected.Draw(deltaTime, base.GraphicsDevice, GameMain.spriteBatch);
			}
			if (GameMain.DebugDraw && GUI.MouseOn != null)
			{
				GameMain.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				if (PlayerInput.IsCtrlDown() && PlayerInput.KeyDown(Keys.G))
				{
					List<GUIComponent> hierarchy = new List<GUIComponent>();
					for (GUIComponent currComponent = GUI.MouseOn; currComponent != null; currComponent = currComponent.Parent)
					{
						hierarchy.Add(currComponent);
					}
					Microsoft.Xna.Framework.Color[] colors = new Microsoft.Xna.Framework.Color[]
					{
						Microsoft.Xna.Framework.Color.Lime,
						Microsoft.Xna.Framework.Color.Yellow,
						Microsoft.Xna.Framework.Color.Aqua,
						Microsoft.Xna.Framework.Color.Red
					};
					for (int index = 0; index < hierarchy.Count; index++)
					{
						GUIComponent component = hierarchy[index];
						if (component != null)
						{
							Rectangle mouseRect = component.MouseRect;
							Rectangle rect = component.Rect;
							if (mouseRect.IsEmpty)
							{
								mouseRect = rect;
							}
							mouseRect.Location += new ValueTuple<int, int>(index % 2, index % 4 / 2);
							GUI.DrawRectangle(GameMain.spriteBatch, mouseRect, colors[index % 4], false, 0f, 1f);
						}
					}
				}
				else
				{
					GUI.DrawRectangle(GameMain.spriteBatch, GUI.MouseOn.MouseRect, Microsoft.Xna.Framework.Color.Lime, false, 0f, 1f);
					GUI.DrawRectangle(GameMain.spriteBatch, GUI.MouseOn.Rect, Microsoft.Xna.Framework.Color.Cyan, false, 0f, 1f);
				}
				GameMain.spriteBatch.End();
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw", sw.ElapsedTicks);
			GameMain.PerformanceCounter.DrawTimeGraph.Update((float)sw.ElapsedTicks * 1000f / (float)Stopwatch.Frequency);
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x0007CA20 File Offset: 0x0007AC20
		public static void QuitToMainMenu(bool save, bool showVerificationPrompt)
		{
			if (showVerificationPrompt)
			{
				string text = (Screen.Selected is CharacterEditorScreen || Screen.Selected is SubEditorScreen) ? "PauseMenuQuitVerificationEditor" : "PauseMenuQuitVerification";
				GUIMessageBox msgBox = new GUIMessageBox("", TextManager.Get(text), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("Cancel")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
				{
					UserData = "verificationprompt"
				};
				msgBox.Buttons[0].OnClicked = delegate(GUIButton yesBtn, object userdata)
				{
					GameMain.QuitToMainMenu(save);
					return true;
				};
				GUIButton guibutton = msgBox.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
				GUIButton guibutton2 = msgBox.Buttons[1];
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
			}
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x0007CB48 File Offset: 0x0007AD48
		public static void QuitToMainMenu(bool save)
		{
			CreatureMetrics.Save();
			if (save)
			{
				GUI.SetSavingIndicatorState(true);
				CampaignMode campaign = GameMain.GameSession.Campaign;
				if (campaign != null)
				{
					campaign.HandleSaveAndQuit();
				}
				if (GameMain.GameSession.Submarine != null && !GameMain.GameSession.Submarine.Removed)
				{
					GameMain.GameSession.SubmarineInfo = new SubmarineInfo(GameMain.GameSession.Submarine);
				}
				CampaignMode campaign2 = GameMain.GameSession.Campaign;
				if (campaign2 != null)
				{
					campaign2.End(CampaignMode.TransitionType.None);
				}
				SaveUtil.SaveGame(GameMain.GameSession.DataPath, false);
			}
			if (GameMain.Client != null)
			{
				GameMain.Client.Quit();
				GameMain.Client = null;
			}
			CoroutineManager.StopCoroutines("EndCinematic");
			if (GameMain.GameSession != null && GameMain.GameSession.IsRunning)
			{
				AchievementManager.OnRoundEnded(GameMain.GameSession, true);
				GameAnalyticsManager.ProgressionStatus progressionStatus = GameAnalyticsManager.ProgressionStatus.Fail;
				GameMode gameMode = GameMain.GameSession.GameMode;
				GameAnalyticsManager.AddProgressionEvent(progressionStatus, ((gameMode != null) ? gameMode.Preset.Identifier.Value : null) ?? "none", (double)GameMain.GameSession.RoundDuration);
				string str = "QuitRound:";
				GameMode gameMode2 = GameMain.GameSession.GameMode;
				string eventId = str + (((gameMode2 != null) ? gameMode2.Preset.Identifier.Value : null) ?? "none") + ":";
				GameMain.GameSession.LogEndRoundStats(eventId, null);
				TutorialMode tutorialMode = GameMain.GameSession.GameMode as TutorialMode;
				if (tutorialMode != null)
				{
					Tutorial tutorial = tutorialMode.Tutorial;
					if (tutorial != null)
					{
						tutorial.Stop();
					}
				}
			}
			GUIMessageBox.CloseAll();
			GameMain.MainMenuScreen.Select();
			GameMain.GameSession = null;
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0007CCDC File Offset: 0x0007AEDC
		public void ShowBugReporter()
		{
			if (GUIMessageBox.VisibleBox != null && GUIMessageBox.VisibleBox.UserData as string == "bugreporter")
			{
				return;
			}
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("bugreportbutton"), "", null, null, GUIMessageBox.Type.Default)
			{
				UserData = "bugreporter",
				DrawOnTop = true
			};
			GUILayoutGroup linkHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.025f
			};
			linkHolder.RectTransform.MaxSize = new Point(int.MaxValue, linkHolder.Rect.Height);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 1f), linkHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("bugreportfeedbackform"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton.UserData = "https://steamcommunity.com/app/602960/discussions/1/";
			guibutton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (!SteamManager.OverlayCustomUrl(userdata as string))
				{
					GameMain.ShowOpenUriPrompt(userdata as string, "openlinkinbrowserprompt", null);
				}
				msgBox.Close();
				return true;
			};
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(1f, 1f), linkHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("bugreportgithubform"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton2.UserData = "https://github.com/FakeFishGames/Barotrauma/discussions/new?category=bug-reports";
			guibutton2.OnClicked = delegate(GUIButton btn, object userdata)
			{
				GameMain.ShowOpenUriPrompt(userdata as string, "openlinkinbrowserprompt", null);
				msgBox.Close();
				return true;
			};
			msgBox.InnerFrame.RectTransform.MinSize = new Point(0, msgBox.InnerFrame.Rect.Height + linkHolder.Rect.Height + msgBox.Content.AbsoluteSpacing * 2 + (int)(50f * GUI.Scale));
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x0007CF14 File Offset: 0x0007B114
		public CoroutineHandle ShowLoading(IEnumerable<CoroutineStatus> loader, bool waitKeyHit = true)
		{
			GameMain.waitForKeyHit = waitKeyHit;
			this.loadingScreenOpen = true;
			GameMain.TitleScreen.LoadState = 0f;
			return CoroutineManager.StartCoroutine(GameMain.TitleScreen.DoLoading(loader), "");
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x0007CF48 File Offset: 0x0007B148
		protected override void OnExiting(object sender, EventArgs args)
		{
			GameMain.IsExiting = true;
			CreatureMetrics.Save();
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
			DebugConsole.NewMessage("Exiting...", null, false);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.Quit();
			}
			SteamManager.ShutDown();
			try
			{
				SaveUtil.CleanUnnecessarySaveFiles();
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error while cleaning unnecessary save files", e2, null, false, false);
			}
			if (GameAnalyticsManager.SendUserStatistics)
			{
				GameAnalyticsManager.ShutDown();
			}
			if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.SaveLogs();
			}
			base.OnExiting(sender, args);
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x0007D028 File Offset: 0x0007B228
		public static GUIMessageBox ShowOpenUriPrompt(string url, string promptTextTag = "openlinkinbrowserprompt", string promptExtensionTag = null)
		{
			LocalizedString text = TextManager.GetWithVariable(promptTextTag, "[link]", url, FormatCapitals.No);
			LocalizedString extensionText = TextManager.Get(promptExtensionTag);
			if (!extensionText.IsNullOrEmpty())
			{
				LocalizedString left = text;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("\n\n");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(extensionText);
				text = left + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return GameMain.ShowOpenUriPrompt(url, text);
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x0007D090 File Offset: 0x0007B290
		public static GUIMessageBox ShowOpenUriPrompt(string url, LocalizedString promptText)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}
			GUIComponent visibleBox = GUIMessageBox.VisibleBox;
			if (((visibleBox != null) ? visibleBox.UserData : null) as string == "verificationprompt")
			{
				return null;
			}
			GUIMessageBox msgBox = new GUIMessageBox("", promptText, new LocalizedString[]
			{
				TextManager.Get("Yes"),
				TextManager.Get("No")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				UserData = "verificationprompt"
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
			{
				try
				{
					ToolBox.OpenFileWithShell(url);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to open the url " + url, e, null, false, false);
				}
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			return msgBox;
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x0007D198 File Offset: 0x0007B398
		public static void ResetIMEWorkaround()
		{
			Rectangle rect = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			TextInput.SetTextInputRect(rect);
			TextInput.StartTextInput();
			TextInput.SetTextInputRect(rect);
			TextInput.StopTextInput();
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x0007D200 File Offset: 0x0007B400
		[CompilerGenerated]
		internal unsafe static void <ApplyGraphicsSettings>g__updateConfig|117_0()
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.Graphics.Width = GameMain.GraphicsWidth;
			config.Graphics.Height = GameMain.GraphicsHeight;
			GameSettings.SetCurrentConfig(config);
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x0007D241 File Offset: 0x0007B441
		[CompilerGenerated]
		internal static void <Load>g__log|122_0(string str)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(str, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
			}
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x0007D260 File Offset: 0x0007B460
		[CompilerGenerated]
		internal static void <ProcessLaunchCountEos>g__trySetConnectCommand|123_0(string commandStr)
		{
			GameMain.Instance.ConnectCommand = GameMain.Instance.ConnectCommand.Fallback(Barotrauma.Networking.ConnectCommand.Parse(commandStr));
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x0007D284 File Offset: 0x0007B484
		[CompilerGenerated]
		internal static bool <Update>g__itemHudActive|128_0()
		{
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedItem : null) == null)
			{
				return false;
			}
			if (!Character.Controlled.SelectedItem.ActiveHUDs.Any((ItemComponent ic) => ic.GuiFrame != null))
			{
				Item item = Character.Controlled.ViewTarget as Item;
				bool? flag;
				if (item == null)
				{
					flag = null;
				}
				else
				{
					ItemPrefab prefab = item.Prefab;
					flag = ((prefab != null) ? new bool?(prefab.FocusOnSelected) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
			return true;
		}

		// Token: 0x040006D5 RID: 1749
		public static bool ShowFPS;

		// Token: 0x040006D6 RID: 1750
		public static bool ShowPerf;

		// Token: 0x040006D7 RID: 1751
		public static bool DebugDraw;

		// Token: 0x040006D8 RID: 1752
		public static bool DevMode;

		// Token: 0x040006D9 RID: 1753
		public static PerformanceCounter PerformanceCounter;

		// Token: 0x040006DA RID: 1754
		private static Stopwatch performanceCounterTimer;

		// Token: 0x040006DB RID: 1755
		private static int updateCount = 0;

		// Token: 0x040006DD RID: 1757
		public static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;

		// Token: 0x040006DE RID: 1758
		public readonly ImmutableArray<string> ConsoleArguments;

		// Token: 0x040006DF RID: 1759
		public readonly Option<string> EgsExchangeCode;

		// Token: 0x040006E0 RID: 1760
		public static GameScreen GameScreen;

		// Token: 0x040006E1 RID: 1761
		public static MainMenuScreen MainMenuScreen;

		// Token: 0x040006E2 RID: 1762
		public static NetLobbyScreen NetLobbyScreen;

		// Token: 0x040006E3 RID: 1763
		public static ModDownloadScreen ModDownloadScreen;

		// Token: 0x040006E4 RID: 1764
		public static ServerListScreen ServerListScreen;

		// Token: 0x040006E5 RID: 1765
		public static SubEditorScreen SubEditorScreen;

		// Token: 0x040006E6 RID: 1766
		public static TestScreen TestScreen;

		// Token: 0x040006E7 RID: 1767
		public static ParticleEditorScreen ParticleEditorScreen;

		// Token: 0x040006E8 RID: 1768
		public static LevelEditorScreen LevelEditorScreen;

		// Token: 0x040006E9 RID: 1769
		public static SpriteEditorScreen SpriteEditorScreen;

		// Token: 0x040006EA RID: 1770
		public static EventEditorScreen EventEditorScreen;

		// Token: 0x040006EB RID: 1771
		public static CharacterEditorScreen CharacterEditorScreen;

		// Token: 0x040006EC RID: 1772
		public static CampaignEndScreen CampaignEndScreen;

		// Token: 0x040006ED RID: 1773
		public static LightManager LightManager;

		// Token: 0x040006EE RID: 1774
		public static SoundManager SoundManager;

		// Token: 0x040006F0 RID: 1776
		private static GameSession gameSession;

		// Token: 0x040006F1 RID: 1777
		public static ParticleManager ParticleManager;

		// Token: 0x040006F2 RID: 1778
		private static World world;

		// Token: 0x040006F3 RID: 1779
		public static LoadingScreen TitleScreen;

		// Token: 0x040006F4 RID: 1780
		private bool loadingScreenOpen;

		// Token: 0x040006F5 RID: 1781
		private Thread initialLoadingThread;

		// Token: 0x040006F7 RID: 1783
		private readonly GameTime fixedTime;

		// Token: 0x040006F8 RID: 1784
		public Option<ConnectCommand> ConnectCommand = Option<Barotrauma.Networking.ConnectCommand>.None();

		// Token: 0x040006F9 RID: 1785
		private string clientName;

		// Token: 0x040006FA RID: 1786
		private static SpriteBatch spriteBatch;

		// Token: 0x040006FB RID: 1787
		private Viewport defaultViewport;

		// Token: 0x04000704 RID: 1796
		public static GameClient Client;

		// Token: 0x04000707 RID: 1799
		private const GraphicsProfile GfxProfile = GraphicsProfile.Reach;

		// Token: 0x04000709 RID: 1801
		private static bool contentLoaded;

		// Token: 0x0400070A RID: 1802
		private static readonly Queue<Action> postContentLoadActions = new Queue<Action>();

		// Token: 0x0400070B RID: 1803
		private static bool waitForKeyHit = true;

		// Token: 0x02000835 RID: 2101
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003D4F RID: 15695
			public static EventHandler<FileDropEventArgs> <0>__OnFileDropped;

			// Token: 0x04003D50 RID: 15696
			public static Action <1>__ProcessLaunchCountEos;

			// Token: 0x04003D51 RID: 15697
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <2>__IgnoredCallback;
		}
	}
}
