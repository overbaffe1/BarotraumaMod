using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using LightInject;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	// Token: 0x0200003E RID: 62
	internal class LuaCsSetup : IDisposable, IEventScreenSelected, IEvent<IEventScreenSelected>, IEvent, IEventEnabledPackageListChanged, IEvent<IEventEnabledPackageListChanged>, IEventReloadAllPackages, IEvent<IEventReloadAllPackages>
	{
		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000922 RID: 2338 RVA: 0x0005BEC7 File Offset: 0x0005A0C7
		public static LuaCsSetup Instance
		{
			get
			{
				LuaCsSetup result;
				if ((result = LuaCsSetup._luaCsSetup) == null)
				{
					result = (LuaCsSetup._luaCsSetup = new LuaCsSetup());
				}
				return result;
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000923 RID: 2339 RVA: 0x0005BEDD File Offset: 0x0005A0DD
		// (set) Token: 0x06000924 RID: 2340 RVA: 0x0005BEE4 File Offset: 0x0005A0E4
		public static int DebugConsoleCommandVanillaIndex { get; private set; }

		// Token: 0x06000925 RID: 2341 RVA: 0x0005BEEC File Offset: 0x0005A0EC
		private LuaCsSetup()
		{
			if (LuaCsSetup._luaCsSetup != null)
			{
				throw new Exception("Tried to create another LuaCsSetup instance");
			}
			LuaCsSetup.DebugConsoleCommandVanillaIndex = DebugConsole.Commands.Count;
			this._servicesProvider = this.SetupServicesProvider();
			this._runStateMachine = this.SetupStateMachine();
			this.SubscribeToLuaCsEvents();
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0005BF49 File Offset: 0x0005A149
		private void SubscribeToLuaCsEvents()
		{
			this.EventService.Subscribe<IEventScreenSelected>(this);
			this.EventService.Subscribe<IEventEnabledPackageListChanged>(this);
			this.EventService.Subscribe<IEventReloadAllPackages>(this);
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000927 RID: 2343 RVA: 0x0005BF74 File Offset: 0x0005A174
		public PerformanceCounterService PerformanceCounterService
		{
			get
			{
				PerformanceCounterService result;
				if ((result = this._performanceCounterService) == null)
				{
					result = (this._performanceCounterService = this._servicesProvider.GetService<PerformanceCounterService>());
				}
				return result;
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000928 RID: 2344 RVA: 0x0005BF9F File Offset: 0x0005A19F
		public ILoggerService Logger
		{
			get
			{
				return this._servicesProvider.GetService<ILoggerService>();
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000929 RID: 2345 RVA: 0x0005BFAC File Offset: 0x0005A1AC
		public IConfigService ConfigService
		{
			get
			{
				return this._servicesProvider.GetService<IConfigService>();
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x0600092A RID: 2346 RVA: 0x0005BFB9 File Offset: 0x0005A1B9
		public IPackageManagementService PackageManagementService
		{
			get
			{
				return this._servicesProvider.GetService<IPackageManagementService>();
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x0600092B RID: 2347 RVA: 0x0005BFC6 File Offset: 0x0005A1C6
		public IPluginManagementService PluginManagementService
		{
			get
			{
				return this._servicesProvider.GetService<IPluginManagementService>();
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x0600092C RID: 2348 RVA: 0x0005BFD3 File Offset: 0x0005A1D3
		public ILuaScriptManagementService LuaScriptManagementService
		{
			get
			{
				return this._servicesProvider.GetService<ILuaScriptManagementService>();
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x0005BFE0 File Offset: 0x0005A1E0
		public INetworkingService NetworkingService
		{
			get
			{
				return this._servicesProvider.GetService<INetworkingService>();
			}
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x0600092E RID: 2350 RVA: 0x0005BFF0 File Offset: 0x0005A1F0
		public IEventService EventService
		{
			get
			{
				IEventService result;
				if ((result = this._eventService) == null)
				{
					result = (this._eventService = this._servicesProvider.GetService<IEventService>());
				}
				return result;
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x0600092F RID: 2351 RVA: 0x0005C01C File Offset: 0x0005A21C
		public LuaGame Game
		{
			get
			{
				LuaGame result;
				if ((result = this._game) == null)
				{
					result = (this._game = this._servicesProvider.GetService<LuaGame>());
				}
				return result;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000930 RID: 2352 RVA: 0x0005C047 File Offset: 0x0005A247
		public Script Lua
		{
			get
			{
				return this.LuaScriptManagementService.InternalScript;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x0005C054 File Offset: 0x0005A254
		// (set) Token: 0x06000932 RID: 2354 RVA: 0x0005C068 File Offset: 0x0005A268
		public bool IsCsEnabledForSession
		{
			get
			{
				ISettingBase<bool> isCsEnabledForSession = this._isCsEnabledForSession;
				return isCsEnabledForSession != null && isCsEnabledForSession.Value;
			}
			internal set
			{
				ISettingBase<bool> isCsEnabledForSession = this._isCsEnabledForSession;
				if (isCsEnabledForSession != null)
				{
					isCsEnabledForSession.TrySetValue(value);
				}
				if (this._isCsEnabledForSession != null)
				{
					if (this._isCsEnabledForSession.GetConfigInfo() == null)
					{
						ILoggerService logger = this.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Config info was nil while trying to save ");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(this.IsCsEnabledForSession);
						logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					this.ConfigService.SaveConfigValue(this._isCsEnabledForSession);
				}
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000933 RID: 2355 RVA: 0x0005C0E8 File Offset: 0x0005A2E8
		public bool IsCsEnabled
		{
			get
			{
				ISettingList<string> csRunPolicy = this._csRunPolicy;
				string a = (csRunPolicy != null) ? csRunPolicy.Value : null;
				return a == "Enabled" || a == "Prompt";
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000934 RID: 2356 RVA: 0x0005C129 File Offset: 0x0005A329
		public string CsRunPolicyValue
		{
			get
			{
				ISettingList<string> csRunPolicy = this._csRunPolicy;
				return ((csRunPolicy != null) ? csRunPolicy.Value : null) ?? "Prompt";
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000935 RID: 2357 RVA: 0x0005C146 File Offset: 0x0005A346
		// (set) Token: 0x06000936 RID: 2358 RVA: 0x0005C159 File Offset: 0x0005A359
		public bool HideUserNamesInLogs
		{
			get
			{
				ISettingBase<bool> hideUserNamesInLogs = this._hideUserNamesInLogs;
				return hideUserNamesInLogs != null && hideUserNamesInLogs.Value;
			}
			internal set
			{
				ISettingBase<bool> hideUserNamesInLogs = this._hideUserNamesInLogs;
				if (hideUserNamesInLogs == null)
				{
					return;
				}
				hideUserNamesInLogs.TrySetValue(value);
			}
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000937 RID: 2359 RVA: 0x0005C16D File Offset: 0x0005A36D
		public bool UseCaching
		{
			get
			{
				ISettingBase<bool> useCaching = this._useCaching;
				return useCaching == null || useCaching.Value;
			}
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0005C180 File Offset: 0x0005A380
		public static ContentPackage GetLuaCsPackage()
		{
			RegularPackage result;
			if ((result = ContentPackageManager.EnabledPackages.Regular.FirstOrDefault((RegularPackage cp) => cp.NameMatches("LuaCsForBarotrauma"), null)) == null)
			{
				if ((result = ContentPackageManager.LocalPackages.FirstOrDefault((ContentPackage cp) => cp.NameMatches("LuaCsForBarotrauma"))) == null)
				{
					result = ContentPackageManager.WorkshopPackages.FirstOrDefault((ContentPackage cp) => cp.NameMatches("LuaCsForBarotrauma"));
				}
			}
			return result;
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0005C214 File Offset: 0x0005A414
		private void LoadLuaCsConfig()
		{
			ContentPackage luaCsPackage = LuaCsSetup.GetLuaCsPackage();
			ISettingList<string> val;
			this._csRunPolicy = (this.ConfigService.TryGetConfig<ISettingList<string>>(luaCsPackage, "CsRunPolicy", out val) ? val : null);
			ISettingBase<bool> val2;
			this._hideUserNamesInLogs = (this.ConfigService.TryGetConfig<ISettingBase<bool>>(luaCsPackage, "HideUserNamesInLogs", out val2) ? val2 : null);
			ISettingBase<bool> val3;
			this._useCaching = (this.ConfigService.TryGetConfig<ISettingBase<bool>>(luaCsPackage, "UseCaching", out val3) ? val3 : null);
			ISettingBase<bool> val4;
			this._isCsEnabledForSession = (this.ConfigService.TryGetConfig<ISettingBase<bool>>(luaCsPackage, "IsCsEnabledForSession", out val4) ? val4 : null);
			if (!ContentPackageManager.EnabledPackages.All.Contains(luaCsPackage))
			{
				luaCsPackage.UnloadFilesOfType<TextFile>();
				luaCsPackage.LoadFilesOfType<TextFile>();
			}
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x0005C2C0 File Offset: 0x0005A4C0
		private IServicesProvider SetupServicesProvider()
		{
			ServicesProvider servicesProvider = new ServicesProvider();
			servicesProvider.RegisterServiceType<ILoggerService, LoggerService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<PerformanceCounterService, PerformanceCounterService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IStorageService, StorageService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<ISafeStorageService, SafeStorageService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IEventService, EventService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceResolver<ILuaCsHook>((ServiceContainer factory) => factory.GetInstance<IEventService>());
			servicesProvider.RegisterServiceType<IPackageManagementService, PackageManagementService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IAssemblyManagementService, PluginManagementService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceResolver<IPluginManagementService>((ServiceContainer factory) => factory.GetInstance<IAssemblyManagementService>());
			servicesProvider.RegisterServiceType<ILuaScriptManagementService, LuaScriptManagementService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IConfigService, ConfigService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<INetworkingService, NetworkingService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<INetworkIdProvider, NetworkingIdProvider>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<HarmonyEventPatchesService, HarmonyEventPatchesService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IConsoleCommandsService, ConsoleCommandsService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<MainMenuPatch, MainMenuPatch>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceResolver<ILuaConfigService>((ServiceContainer factory) => factory.GetInstance<IConfigService>());
			servicesProvider.RegisterServiceType<IAssemblyLoaderService.IFactory, Barotrauma.LuaCs.AssemblyLoader.Factory>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<ISettingsRegistrationProvider, SettingsEntryRegistrar>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IModConfigService, ModConfigService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>, ModConfigFileParserService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>, ModConfigFileParserService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>, ModConfigFileParserService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo>, SettingsFileParserService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo>, SettingsFileParserService>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<INetworkIdProvider, NetworkingIdProvider>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<IDefaultLuaRegistrar, DefaultLuaRegistrar>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ILuaPatcher, LuaPatcherService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ILuaUserDataService, LuaUserDataService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ISafeLuaUserDataService, SafeLuaUserDataService>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ILuaCsInfoProvider, LuaCsInfoProvider>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<ILuaScriptLoader, LuaScriptLoader>(ServiceLifetime.Transient, null);
			servicesProvider.RegisterServiceType<LuaGame, LuaGame>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ILuaCsTimer, LuaCsTimer>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IStorageServiceConfig, StorageServiceConfig>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<ILuaScriptServicesConfig, LuaScriptServicesConfig>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IConfigServiceConfig, ConfigServiceConfig>(ServiceLifetime.Singleton, null);
			servicesProvider.RegisterServiceType<IPackageManagementServiceConfig, PackageManagementServiceConfig>(ServiceLifetime.Singleton, null);
			servicesProvider.CompileAndRun();
			return servicesProvider;
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x0600093B RID: 2363 RVA: 0x0005C461 File Offset: 0x0005A661
		// (set) Token: 0x0600093C RID: 2364 RVA: 0x0005C469 File Offset: 0x0005A669
		public RunState CurrentRunState
		{
			get
			{
				return this._runState;
			}
			private set
			{
				this._runState = value;
			}
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0005C472 File Offset: 0x0005A672
		public void OnEnabledPackageListChanged(CorePackage package, IEnumerable<RegularPackage> regularPackages)
		{
			this.ProcessEnabledPackageChanges(new CorePackage[]
			{
				package
			}.Concat(regularPackages).ToImmutableArray<ContentPackage>());
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x0005C48F File Offset: 0x0005A68F
		public void OnReloadAllPackages()
		{
			CoroutineManager.Invoke(delegate
			{
				this.SetRunState(RunState.Unloaded);
				CoroutineManager.Invoke(delegate
				{
					this.SetRunState(RunState.Running);
				}, 0.25f);
			}, 0f);
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x0005C4A8 File Offset: 0x0005A6A8
		private void ProcessEnabledPackageChanges(ImmutableArray<ContentPackage> packages)
		{
			if (this.CurrentRunState < RunState.LoadedNoExec)
			{
				return;
			}
			RunState state = this.CurrentRunState;
			if (this.CurrentRunState > RunState.LoadedNoExec)
			{
				this.SetRunState(RunState.LoadedNoExec);
			}
			this.Logger.LogResults(this.PackageManagementService.SyncLoadedPackagesList(this.GetLuaCsEnabledPackagesList(packages)));
			this.ConfigService.LoadSavedConfigsValues();
			this.SetRunState(state);
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x0005C506 File Offset: 0x0005A706
		public void SetRunState(RunState targetRunState)
		{
			if (this.CurrentRunState == targetRunState)
			{
				return;
			}
			this._runStateMachine.GotoState(targetRunState);
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x0005C51F File Offset: 0x0005A71F
		private ImmutableArray<ContentPackage> GetEnabledPackagesList()
		{
			return this.GetLuaCsEnabledPackagesList(ContentPackageManager.EnabledPackages.Regular.ToImmutableArray<ContentPackage>());
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x0005C534 File Offset: 0x0005A734
		private ImmutableArray<ContentPackage> GetLuaCsEnabledPackagesList(ImmutableArray<ContentPackage> enabledRegular)
		{
			if (!enabledRegular.Any((ContentPackage p) => p.Name.Equals("LuaCsForBarotrauma", StringComparison.InvariantCultureIgnoreCase)))
			{
				ContentPackage luaCs = ContentPackageManager.AllPackages.FirstOrDefault((ContentPackage p) => p.Name.Equals("LuaCsForBarotrauma", StringComparison.InvariantCultureIgnoreCase));
				if (luaCs == null)
				{
					DebugConsole.ThrowError("The 'LuaCsForBarotrauma' mod could not be found. Please subscribe to it and add it to the EnabledPackages List!", new NullReferenceException("The 'LuaCsForBarotrauma' mod could not be found. Please subscribe to it and add it to the EnabledPackages List!"), null, true, false);
					return enabledRegular;
				}
				enabledRegular = new ContentPackage[]
				{
					luaCs
				}.Concat(enabledRegular).ToImmutableArray<ContentPackage>();
			}
			return enabledRegular;
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x0005C5CC File Offset: 0x0005A7CC
		private StateMachine<RunState> SetupStateMachine()
		{
			return new StateMachine<RunState>(false, RunState.Unloaded, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateUnloaded_OnEnter|66_0), null).AddState(RunState.LoadedNoExec, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateLoadedNoExec_OnEnter|66_1), null).AddState(RunState.Running, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateRunning_OnEnter|66_2), new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateRunning_OnExit|66_3));
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0005C61E File Offset: 0x0005A81E
		private void CheckReadyToRun(Action onReadyToRun)
		{
			if (onReadyToRun != null)
			{
				onReadyToRun();
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000945 RID: 2373 RVA: 0x0005C629 File Offset: 0x0005A829
		// (set) Token: 0x06000946 RID: 2374 RVA: 0x0005C631 File Offset: 0x0005A831
		[Obsolete]
		public LuaCsPerformanceCounter PerformanceCounter { get; private set; } = new LuaCsPerformanceCounter();

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x0005C63A File Offset: 0x0005A83A
		[Obsolete("Use PluginManagementService instead.")]
		public IPluginManagementService PluginPackageManager
		{
			get
			{
				return this.PluginManagementService;
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000948 RID: 2376 RVA: 0x0005C642 File Offset: 0x0005A842
		public ILuaCsHook Hook
		{
			get
			{
				return this.EventService;
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000949 RID: 2377 RVA: 0x0005C64A File Offset: 0x0005A84A
		public INetworkingService Networking
		{
			get
			{
				return this.NetworkingService;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x0600094A RID: 2378 RVA: 0x0005C652 File Offset: 0x0005A852
		public ILuaCsTimer Timer
		{
			get
			{
				return this._servicesProvider.GetService<ILuaCsTimer>();
			}
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0005C65F File Offset: 0x0005A85F
		public DynValue CallLuaFunction(object function, params object[] args)
		{
			return this.LuaScriptManagementService.CallFunctionSafe(function, args);
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0005C670 File Offset: 0x0005A870
		public void Dispose()
		{
			try
			{
				this.SetRunState(RunState.Unloaded);
			}
			catch (Exception e)
			{
				this.Logger.LogError(e.Message);
			}
			try
			{
				this.DisposeLuaCsConfig();
				this.PluginManagementService.Dispose();
				this.LuaScriptManagementService.Dispose();
				this.ConfigService.Dispose();
				this.PackageManagementService.Dispose();
				this.EventService.Dispose();
				this._eventService = null;
				this._game = null;
				this.PerformanceCounter = null;
				this._servicesProvider.DisposeAndReset();
			}
			catch (Exception e2)
			{
				Console.WriteLine(e2);
				throw;
			}
			LuaCsSetup._luaCsSetup = null;
			GC.SuppressFinalize(this);
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0005C72C File Offset: 0x0005A92C
		public void OnScreenSelected(Screen screen)
		{
			if (screen == UnimplementedScreen.Instance)
			{
				this.SetRunState(RunState.Unloaded);
			}
			this.SetRunState(RunState.Running);
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x0005C744 File Offset: 0x0005A944
		private void DisposeLuaCsConfig()
		{
			this._csRunPolicy = null;
			this._hideUserNamesInLogs = null;
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0005C754 File Offset: 0x0005A954
		public static void PrintLuaError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0005C788 File Offset: 0x0005A988
		public static void PrintCsError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0005C7BC File Offset: 0x0005A9BC
		public static void PrintGenericError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x0005C7F0 File Offset: 0x0005A9F0
		internal void PrintMessage(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0005C838 File Offset: 0x0005AA38
		public static void PrintCsMessage(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0005C87E File Offset: 0x0005AA7E
		internal void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.HandleException(ex, null);
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x0005C8BC File Offset: 0x0005AABC
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateUnloaded_OnEnter|66_0(State<RunState> currentState)
		{
			this.Logger.LogMessage("LuaCs unloaded state entered", null, null);
			this.Logger.LogResults(this.PackageManagementService.StopRunningPackages());
			this.DisposeLuaCsConfig();
			this.Logger.LogResults(this.PackageManagementService.UnloadAllPackages());
			this.EventService.Reset();
			this.ConfigService.Reset();
			this.LuaScriptManagementService.Reset();
			this.PackageManagementService.Reset();
			this.NetworkingService.Reset();
			this.Game.Reset();
			this._servicesProvider.GetService<MainMenuPatch>().Reset();
			this.Logger.LogMessage("Services have been reset", null, null);
			this.SubscribeToLuaCsEvents();
			this.CurrentRunState = RunState.Unloaded;
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x0005C9A8 File Offset: 0x0005ABA8
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateLoadedNoExec_OnEnter|66_1(State<RunState> currentState)
		{
			this.Logger.LogMessage("LuaCs no execution state entered", null, null);
			this.Logger.LogResults(this.PackageManagementService.StopRunningPackages());
			if (!this.PackageManagementService.IsAnyPackageLoaded())
			{
				foreach (ISettingsRegistrationProvider registrationProvider in this._servicesProvider.GetAllServices<ISettingsRegistrationProvider>())
				{
					registrationProvider.RegisterTypeProviders(this.ConfigService, null);
				}
				this.Logger.LogResults(this.PackageManagementService.LoadPackagesInfo(this.GetEnabledPackagesList()));
				this.Logger.LogResults(this.ConfigService.LoadSavedConfigsValues());
				this.LoadLuaCsConfig();
			}
			this.CurrentRunState = RunState.LoadedNoExec;
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x0005CA70 File Offset: 0x0005AC70
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateRunning_OnEnter|66_2(State<RunState> currentState)
		{
			if (!this.PackageManagementService.IsAnyPackageLoaded())
			{
				foreach (ISettingsRegistrationProvider registrationProvider in this._servicesProvider.GetAllServices<ISettingsRegistrationProvider>())
				{
					registrationProvider.RegisterTypeProviders(this.ConfigService, null);
				}
				this.Logger.LogResults(this.PackageManagementService.LoadPackagesInfo(this.GetEnabledPackagesList()));
				this.Logger.LogResults(this.ConfigService.LoadSavedConfigsValues());
				this.LoadLuaCsConfig();
			}
			string csEnabled = this.IsCsEnabled ? "enabled" : "disabled";
			this.Logger.LogMessage("LuaCs running state entered. Running under commit " + AssemblyInfo.GitRevision + ", CSharp is " + csEnabled, null, null);
			if (!this.PackageManagementService.IsAnyPackageRunning())
			{
				this.Logger.LogResults(this.PackageManagementService.ExecuteLoadedPackages(this.GetEnabledPackagesList(), this.IsCsEnabled));
			}
			GameMain.Server.ServerSettings.LoadClientPermissions();
			this.CurrentRunState = RunState.Running;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0005CB84 File Offset: 0x0005AD84
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateRunning_OnExit|66_3(State<RunState> currentState)
		{
			this.EventService.Call("stop", Array.Empty<object>());
			this.Logger.LogResults(this.PackageManagementService.StopRunningPackages());
			this.Logger.LogMessage("LuaCs running state exited", null, null);
		}

		// Token: 0x04000418 RID: 1048
		public const string PackageName = "LuaCsForBarotrauma";

		// Token: 0x04000419 RID: 1049
		private static LuaCsSetup _luaCsSetup;

		// Token: 0x0400041B RID: 1051
		public const bool IsServer = true;

		// Token: 0x0400041C RID: 1052
		public const bool IsClient = false;

		// Token: 0x0400041D RID: 1053
		private readonly IServicesProvider _servicesProvider;

		// Token: 0x0400041E RID: 1054
		private PerformanceCounterService _performanceCounterService;

		// Token: 0x0400041F RID: 1055
		private IEventService _eventService;

		// Token: 0x04000420 RID: 1056
		private LuaGame _game;

		// Token: 0x04000421 RID: 1057
		private ISettingBase<bool> _isCsEnabledForSession;

		// Token: 0x04000422 RID: 1058
		private ISettingList<string> _csRunPolicy;

		// Token: 0x04000423 RID: 1059
		private ISettingBase<bool> _hideUserNamesInLogs;

		// Token: 0x04000424 RID: 1060
		private ISettingBase<bool> _useCaching;

		// Token: 0x04000425 RID: 1061
		private RunState _runState;

		// Token: 0x04000426 RID: 1062
		private readonly StateMachine<RunState> _runStateMachine;
	}
}
