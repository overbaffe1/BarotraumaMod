using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.CharacterEditor;
using Barotrauma.Extensions;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using LightInject;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	// Token: 0x020000CC RID: 204
	internal class LuaCsSetup : IDisposable, IEventScreenSelected, IEvent<IEventScreenSelected>, IEvent, IEventEnabledPackageListChanged, IEvent<IEventEnabledPackageListChanged>, IEventReloadAllPackages, IEvent<IEventReloadAllPackages>
	{
		// Token: 0x06001AE7 RID: 6887 RVA: 0x0010A124 File Offset: 0x00108324
		public void PromptCSharpMods(Action<bool> onSelection, bool joiningServer)
		{
			LuaCsSetup.<>c__DisplayClass0_0 CS$<>8__locals1 = new LuaCsSetup.<>c__DisplayClass0_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.onSelection = onSelection;
			ImmutableArray<ContentPackage> contentPackages = (from p in this.PackageManagementService.GetLoadedUnrestrictedPackages()
			where p.Name != "LuaCsForBarotrauma"
			select p).ToImmutableArray<ContentPackage>();
			ISettingList<string> csRunPolicy = this._csRunPolicy;
			if (((csRunPolicy != null) ? csRunPolicy.Value : null) == "Enabled")
			{
				this.IsCsEnabledForSession = true;
				CS$<>8__locals1.onSelection(true);
				return;
			}
			ISettingList<string> csRunPolicy2 = this._csRunPolicy;
			if (((csRunPolicy2 != null) ? csRunPolicy2.Value : null) == "Disabled")
			{
				this.IsCsEnabledForSession = false;
				CS$<>8__locals1.onSelection(false);
				return;
			}
			if (contentPackages.None(null))
			{
				CS$<>8__locals1.onSelection(true);
				return;
			}
			LuaCsSetup.<>c__DisplayClass0_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("warning");
			Vector2? relativeSize = new Vector2?(new Vector2(0.3f, 0.55f));
			Point? point = new Point?(new Point(400, 500));
			CS$<>8__locals2.messageBox = new GUIMessageBox(headerText, string.Empty, Array.Empty<LocalizedString>(), relativeSize, point, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			Vector2 relativeSize2 = new Vector2(1f, 0.75f);
			RectTransform rectTransform = CS$<>8__locals1.messageBox.Content.RectTransform;
			Anchor anchor = Anchor.TopLeft;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			GUILayoutGroup msgBoxLayout = new GUILayoutGroup(new RectTransform(relativeSize2, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			Vector2 relativeSize3 = new Vector2(1f, 0f);
			RectTransform rectTransform2 = msgBoxLayout.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			RectTransform rectT = new RectTransform(relativeSize3, rectTransform2, anchor2, pivot2, minSize2, point, ScaleBasis.Normal);
			RichString text = "The following mods contain CSharp code OR Unsandboxed Lua Code";
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, true, "", null);
			Vector2 relativeSize4 = new Vector2(1f, 0.4f);
			RectTransform rectTransform3 = msgBoxLayout.RectTransform;
			Anchor anchor3 = Anchor.TopLeft;
			Pivot? pivot3 = null;
			point = null;
			Point? minSize3 = point;
			point = null;
			GUIListBox packageListBox = new GUIListBox(new RectTransform(relativeSize4, rectTransform3, anchor3, pivot3, minSize3, point, ScaleBasis.Normal), false, null, "", true, false)
			{
				CurrentSelectMode = GUIListBox.SelectMode.None
			};
			ImmutableArray<ContentPackage>.Enumerator enumerator = contentPackages.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ContentPackage package = enumerator.Current;
				Vector2 relativeSize5 = new Vector2(1f, 0.15f);
				RectTransform rectTransform4 = packageListBox.Content.RectTransform;
				Anchor anchor4 = Anchor.TopLeft;
				Pivot? pivot4 = null;
				point = null;
				Point? minSize4 = point;
				point = null;
				GUIFrame packageFrame = new GUIFrame(new RectTransform(relativeSize5, rectTransform4, anchor4, pivot4, minSize4, point, ScaleBasis.Normal), "ListBoxElement", null);
				Vector2 one = Vector2.One;
				RectTransform rectTransform5 = packageFrame.RectTransform;
				Anchor anchor5 = Anchor.TopLeft;
				Pivot? pivot5 = null;
				point = null;
				Point? minSize5 = point;
				point = null;
				GUILayoutGroup packageLayout = new GUILayoutGroup(new RectTransform(one, rectTransform5, anchor5, pivot5, minSize5, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
				Vector2 relativeSize6 = new Vector2(0.7f, 1f);
				RectTransform rectTransform6 = packageLayout.RectTransform;
				Anchor anchor6 = Anchor.TopLeft;
				Pivot? pivot6 = null;
				point = null;
				Point? minSize6 = point;
				point = null;
				new GUITextBlock(new RectTransform(relativeSize6, rectTransform6, anchor6, pivot6, minSize6, point, ScaleBasis.Normal), package.Name, null, null, Alignment.Left, false, "", null);
				Vector2 relativeSize7 = new Vector2(0.3f, 1f);
				RectTransform rectTransform7 = packageLayout.RectTransform;
				Anchor anchor7 = Anchor.CenterRight;
				Pivot? pivot7 = null;
				point = null;
				Point? minSize7 = point;
				point = null;
				new GUIButton(new RectTransform(relativeSize7, rectTransform7, anchor7, pivot7, minSize7, point, ScaleBasis.Normal), "Open Folder", Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object obj)
				{
					string directory = package.Dir;
					if (string.IsNullOrEmpty(directory))
					{
						return false;
					}
					ToolBox.OpenFileWithShell(directory);
					return true;
				};
			}
			string bodyText = joiningServer ? "You are joining a server that includes mods with C# code OR unrestricted Lua code. These mods are not sandboxed and may access your computer without restrictions. If you trust these mods, select 'Enable C# for this session'. Otherwise, select 'Cancel' to run only Lua mods." : "You have enabled mods that include C# code. These mods are not sandboxed and may access your computer without restrictions. If you trust these mods, select 'Enable C# for this session'. Otherwise, select 'Cancel' to run only Sandboxed Lua mods.";
			Vector2 relativeSize8 = new Vector2(1f, 0f);
			RectTransform rectTransform8 = msgBoxLayout.RectTransform;
			Anchor anchor8 = Anchor.TopLeft;
			Pivot? pivot8 = null;
			point = null;
			Point? minSize8 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize8, rectTransform8, anchor8, pivot8, minSize8, point, ScaleBasis.Normal), bodyText, null, null, Alignment.Left, true, "", null).Wrap = true;
			Vector2 relativeSize9 = new Vector2(1f, 0.25f);
			RectTransform rectTransform9 = CS$<>8__locals1.messageBox.Content.RectTransform;
			Anchor anchor9 = Anchor.BottomCenter;
			Pivot? pivot9 = null;
			point = null;
			Point? minSize9 = point;
			point = null;
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(relativeSize9, rectTransform9, anchor9, pivot9, minSize9, point, ScaleBasis.Normal), false, Anchor.TopCenter);
			Vector2 relativeSize10 = new Vector2(0.8f, 0f);
			RectTransform rectTransform10 = buttonLayout.RectTransform;
			Anchor anchor10 = Anchor.TopLeft;
			Pivot? pivot10 = null;
			point = null;
			Point? minSize10 = point;
			point = null;
			GUIButton guibutton = new GUIButton(new RectTransform(relativeSize10, rectTransform10, anchor10, pivot10, minSize10, point, ScaleBasis.Normal), "Enable C# for this session", Alignment.Center, "", null);
			guibutton.TextBlock.AutoScaleHorizontal = true;
			guibutton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.IsCsEnabledForSession = true;
				CS$<>8__locals1.onSelection(true);
				CS$<>8__locals1.messageBox.Close();
				return true;
			};
			Vector2 relativeSize11 = new Vector2(0.8f, 0f);
			RectTransform rectTransform11 = buttonLayout.RectTransform;
			Anchor anchor11 = Anchor.TopLeft;
			Pivot? pivot11 = null;
			point = null;
			Point? minSize11 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize11, rectTransform11, anchor11, pivot11, minSize11, point, ScaleBasis.Normal), "Cancel", Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.IsCsEnabledForSession = false;
				CS$<>8__locals1.onSelection(false);
				CS$<>8__locals1.messageBox.Close();
				return true;
			};
		}

		// Token: 0x06001AE8 RID: 6888 RVA: 0x0010A6CC File Offset: 0x001088CC
		private void SetupServicesProviderClient(IServicesProvider serviceProvider)
		{
			serviceProvider.RegisterServiceType<IUIStylesService, UIStylesService>(ServiceLifetime.Singleton, null);
			serviceProvider.RegisterServiceType<IParserServiceAsync<ResourceParserInfo, IStylesResourceInfo>, ModConfigFileParserService>(ServiceLifetime.Transient, null);
			serviceProvider.RegisterServiceType<IUIStylesCollection.IFactory, UIStylesCollection.Factory>(ServiceLifetime.Transient, null);
			serviceProvider.RegisterServiceType<ISettingsMenuSystem, SettingsMenuSystem>(ServiceLifetime.Singleton, null);
		}

		// Token: 0x170006DE RID: 1758
		// (get) Token: 0x06001AE9 RID: 6889 RVA: 0x0010A6EE File Offset: 0x001088EE
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

		// Token: 0x170006DF RID: 1759
		// (get) Token: 0x06001AEA RID: 6890 RVA: 0x0010A704 File Offset: 0x00108904
		// (set) Token: 0x06001AEB RID: 6891 RVA: 0x0010A70B File Offset: 0x0010890B
		public static int DebugConsoleCommandVanillaIndex { get; private set; }

		// Token: 0x06001AEC RID: 6892 RVA: 0x0010A714 File Offset: 0x00108914
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

		// Token: 0x06001AED RID: 6893 RVA: 0x0010A771 File Offset: 0x00108971
		private void SubscribeToLuaCsEvents()
		{
			this.EventService.Subscribe<IEventScreenSelected>(this);
			this.EventService.Subscribe<IEventEnabledPackageListChanged>(this);
			this.EventService.Subscribe<IEventReloadAllPackages>(this);
		}

		// Token: 0x170006E0 RID: 1760
		// (get) Token: 0x06001AEE RID: 6894 RVA: 0x0010A79C File Offset: 0x0010899C
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

		// Token: 0x170006E1 RID: 1761
		// (get) Token: 0x06001AEF RID: 6895 RVA: 0x0010A7C7 File Offset: 0x001089C7
		public ILoggerService Logger
		{
			get
			{
				return this._servicesProvider.GetService<ILoggerService>();
			}
		}

		// Token: 0x170006E2 RID: 1762
		// (get) Token: 0x06001AF0 RID: 6896 RVA: 0x0010A7D4 File Offset: 0x001089D4
		public IConfigService ConfigService
		{
			get
			{
				return this._servicesProvider.GetService<IConfigService>();
			}
		}

		// Token: 0x170006E3 RID: 1763
		// (get) Token: 0x06001AF1 RID: 6897 RVA: 0x0010A7E1 File Offset: 0x001089E1
		public IPackageManagementService PackageManagementService
		{
			get
			{
				return this._servicesProvider.GetService<IPackageManagementService>();
			}
		}

		// Token: 0x170006E4 RID: 1764
		// (get) Token: 0x06001AF2 RID: 6898 RVA: 0x0010A7EE File Offset: 0x001089EE
		public IPluginManagementService PluginManagementService
		{
			get
			{
				return this._servicesProvider.GetService<IPluginManagementService>();
			}
		}

		// Token: 0x170006E5 RID: 1765
		// (get) Token: 0x06001AF3 RID: 6899 RVA: 0x0010A7FB File Offset: 0x001089FB
		public ILuaScriptManagementService LuaScriptManagementService
		{
			get
			{
				return this._servicesProvider.GetService<ILuaScriptManagementService>();
			}
		}

		// Token: 0x170006E6 RID: 1766
		// (get) Token: 0x06001AF4 RID: 6900 RVA: 0x0010A808 File Offset: 0x00108A08
		public INetworkingService NetworkingService
		{
			get
			{
				return this._servicesProvider.GetService<INetworkingService>();
			}
		}

		// Token: 0x170006E7 RID: 1767
		// (get) Token: 0x06001AF5 RID: 6901 RVA: 0x0010A818 File Offset: 0x00108A18
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

		// Token: 0x170006E8 RID: 1768
		// (get) Token: 0x06001AF6 RID: 6902 RVA: 0x0010A844 File Offset: 0x00108A44
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

		// Token: 0x170006E9 RID: 1769
		// (get) Token: 0x06001AF7 RID: 6903 RVA: 0x0010A86F File Offset: 0x00108A6F
		public Script Lua
		{
			get
			{
				return this.LuaScriptManagementService.InternalScript;
			}
		}

		// Token: 0x170006EA RID: 1770
		// (get) Token: 0x06001AF8 RID: 6904 RVA: 0x0010A87C File Offset: 0x00108A7C
		// (set) Token: 0x06001AF9 RID: 6905 RVA: 0x0010A890 File Offset: 0x00108A90
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

		// Token: 0x170006EB RID: 1771
		// (get) Token: 0x06001AFA RID: 6906 RVA: 0x0010A90D File Offset: 0x00108B0D
		public bool IsCsEnabled
		{
			get
			{
				ISettingList<string> csRunPolicy = this._csRunPolicy;
				return ((csRunPolicy != null) ? csRunPolicy.Value : null) == "Enabled" || this.IsCsEnabledForSession;
			}
		}

		// Token: 0x170006EC RID: 1772
		// (get) Token: 0x06001AFB RID: 6907 RVA: 0x0010A935 File Offset: 0x00108B35
		public string CsRunPolicyValue
		{
			get
			{
				ISettingList<string> csRunPolicy = this._csRunPolicy;
				return ((csRunPolicy != null) ? csRunPolicy.Value : null) ?? "Prompt";
			}
		}

		// Token: 0x170006ED RID: 1773
		// (get) Token: 0x06001AFC RID: 6908 RVA: 0x0010A952 File Offset: 0x00108B52
		// (set) Token: 0x06001AFD RID: 6909 RVA: 0x0010A965 File Offset: 0x00108B65
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

		// Token: 0x170006EE RID: 1774
		// (get) Token: 0x06001AFE RID: 6910 RVA: 0x0010A979 File Offset: 0x00108B79
		public bool UseCaching
		{
			get
			{
				ISettingBase<bool> useCaching = this._useCaching;
				return useCaching == null || useCaching.Value;
			}
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x0010A98C File Offset: 0x00108B8C
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

		// Token: 0x06001B00 RID: 6912 RVA: 0x0010AA20 File Offset: 0x00108C20
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

		// Token: 0x06001B01 RID: 6913 RVA: 0x0010AACC File Offset: 0x00108CCC
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
			this.SetupServicesProviderClient(servicesProvider);
			servicesProvider.CompileAndRun();
			return servicesProvider;
		}

		// Token: 0x170006EF RID: 1775
		// (get) Token: 0x06001B02 RID: 6914 RVA: 0x0010AC74 File Offset: 0x00108E74
		// (set) Token: 0x06001B03 RID: 6915 RVA: 0x0010AC7C File Offset: 0x00108E7C
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

		// Token: 0x06001B04 RID: 6916 RVA: 0x0010AC85 File Offset: 0x00108E85
		public void OnEnabledPackageListChanged(CorePackage package, IEnumerable<RegularPackage> regularPackages)
		{
			this.ProcessEnabledPackageChanges(new CorePackage[]
			{
				package
			}.Concat(regularPackages).ToImmutableArray<ContentPackage>());
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x0010ACA2 File Offset: 0x00108EA2
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

		// Token: 0x06001B06 RID: 6918 RVA: 0x0010ACBC File Offset: 0x00108EBC
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

		// Token: 0x06001B07 RID: 6919 RVA: 0x0010AD1A File Offset: 0x00108F1A
		public void SetRunState(RunState targetRunState)
		{
			if (this.CurrentRunState == targetRunState)
			{
				return;
			}
			this._runStateMachine.GotoState(targetRunState);
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x0010AD33 File Offset: 0x00108F33
		private ImmutableArray<ContentPackage> GetEnabledPackagesList()
		{
			return this.GetLuaCsEnabledPackagesList(ContentPackageManager.EnabledPackages.Regular.ToImmutableArray<ContentPackage>());
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x0010AD48 File Offset: 0x00108F48
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

		// Token: 0x06001B0A RID: 6922 RVA: 0x0010ADE0 File Offset: 0x00108FE0
		private StateMachine<RunState> SetupStateMachine()
		{
			return new StateMachine<RunState>(false, RunState.Unloaded, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateUnloaded_OnEnter|68_0), null).AddState(RunState.LoadedNoExec, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateLoadedNoExec_OnEnter|68_1), null).AddState(RunState.Running, new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateRunning_OnEnter|68_2), new Action<State<RunState>>(this.<SetupStateMachine>g__RunStateRunning_OnExit|68_3));
		}

		// Token: 0x170006F0 RID: 1776
		// (get) Token: 0x06001B0B RID: 6923 RVA: 0x0010AE32 File Offset: 0x00109032
		// (set) Token: 0x06001B0C RID: 6924 RVA: 0x0010AE3A File Offset: 0x0010903A
		[Obsolete]
		public LuaCsPerformanceCounter PerformanceCounter { get; private set; } = new LuaCsPerformanceCounter();

		// Token: 0x170006F1 RID: 1777
		// (get) Token: 0x06001B0D RID: 6925 RVA: 0x0010AE43 File Offset: 0x00109043
		[Obsolete("Use PluginManagementService instead.")]
		public IPluginManagementService PluginPackageManager
		{
			get
			{
				return this.PluginManagementService;
			}
		}

		// Token: 0x170006F2 RID: 1778
		// (get) Token: 0x06001B0E RID: 6926 RVA: 0x0010AE4B File Offset: 0x0010904B
		public ILuaCsHook Hook
		{
			get
			{
				return this.EventService;
			}
		}

		// Token: 0x170006F3 RID: 1779
		// (get) Token: 0x06001B0F RID: 6927 RVA: 0x0010AE53 File Offset: 0x00109053
		public INetworkingService Networking
		{
			get
			{
				return this.NetworkingService;
			}
		}

		// Token: 0x170006F4 RID: 1780
		// (get) Token: 0x06001B10 RID: 6928 RVA: 0x0010AE5B File Offset: 0x0010905B
		public ILuaCsTimer Timer
		{
			get
			{
				return this._servicesProvider.GetService<ILuaCsTimer>();
			}
		}

		// Token: 0x06001B11 RID: 6929 RVA: 0x0010AE68 File Offset: 0x00109068
		public DynValue CallLuaFunction(object function, params object[] args)
		{
			return this.LuaScriptManagementService.CallFunctionSafe(function, args);
		}

		// Token: 0x06001B12 RID: 6930 RVA: 0x0010AE78 File Offset: 0x00109078
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

		// Token: 0x06001B13 RID: 6931 RVA: 0x0010AF34 File Offset: 0x00109134
		public void OnScreenSelected(Screen screen)
		{
			Action<bool> <>9__1;
			CoroutineManager.Invoke(delegate
			{
				if (screen is MainMenuScreen || screen is ModDownloadScreen || screen is ServerListScreen)
				{
					this.SetRunState(RunState.Unloaded);
					this.SetRunState(RunState.LoadedNoExec);
					return;
				}
				if (!(screen is CampaignEndScreen) && !(screen is CharacterEditorScreen) && !(screen is EventEditorScreen) && !(screen is GameScreen) && !(screen is LevelEditorScreen) && !(screen is NetLobbyScreen) && !(screen is ParticleEditorScreen) && !(screen is RoundSummaryScreen) && !(screen is SpriteEditorScreen) && !(screen is SubEditorScreen) && !(screen is TestScreen))
				{
					ILoggerService logger = this.Logger;
					string str = "LuaCsSetup: Received an unknown screen ";
					Screen screen2 = screen;
					logger.LogError(str + (((screen2 != null) ? screen2.GetType().Name : null) ?? "'null screen'") + ". Retarding load state to 'unloaded'.");
					this.SetRunState(RunState.Unloaded);
					return;
				}
				if (screen is NetLobbyScreen && this.CurrentRunState != RunState.Running)
				{
					GameClient client = GameMain.Client;
					if (!(((client != null) ? client.ClientPeer : null) is P2POwnerPeer))
					{
						LuaCsSetup <>4__this = this;
						Action<bool> onSelection;
						if ((onSelection = <>9__1) == null)
						{
							onSelection = (<>9__1 = delegate(bool selection)
							{
								this.SetRunState(RunState.Running);
							});
						}
						<>4__this.PromptCSharpMods(onSelection, true);
						return;
					}
				}
				this.SetRunState(RunState.Running);
			}, 0f);
		}

		// Token: 0x06001B14 RID: 6932 RVA: 0x0010AF6C File Offset: 0x0010916C
		private void DisposeLuaCsConfig()
		{
			this._csRunPolicy = null;
			this._hideUserNamesInLogs = null;
		}

		// Token: 0x06001B15 RID: 6933 RVA: 0x0010AF7C File Offset: 0x0010917C
		public static void PrintLuaError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06001B16 RID: 6934 RVA: 0x0010AFB0 File Offset: 0x001091B0
		public static void PrintCsError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06001B17 RID: 6935 RVA: 0x0010AFE4 File Offset: 0x001091E4
		public static void PrintGenericError(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06001B18 RID: 6936 RVA: 0x0010B018 File Offset: 0x00109218
		internal void PrintMessage(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x0010B060 File Offset: 0x00109260
		public static void PrintCsMessage(object message)
		{
			ILoggerService logger = LuaCsSetup.Instance.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<object>(message);
			logger.LogMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x0010B0A6 File Offset: 0x001092A6
		internal void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.HandleException(ex, null);
		}

		// Token: 0x06001B1D RID: 6941 RVA: 0x0010B0E4 File Offset: 0x001092E4
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateUnloaded_OnEnter|68_0(State<RunState> currentState)
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

		// Token: 0x06001B1E RID: 6942 RVA: 0x0010B1D0 File Offset: 0x001093D0
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateLoadedNoExec_OnEnter|68_1(State<RunState> currentState)
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

		// Token: 0x06001B1F RID: 6943 RVA: 0x0010B298 File Offset: 0x00109498
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateRunning_OnEnter|68_2(State<RunState> currentState)
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
			if (GameMain.Client != null)
			{
				this.EventService.PublishEvent<IEventServerConnected>(delegate(IEventServerConnected p)
				{
					p.OnServerConnected();
				});
			}
			this.CurrentRunState = RunState.Running;
		}

		// Token: 0x06001B20 RID: 6944 RVA: 0x0010B3D0 File Offset: 0x001095D0
		[CompilerGenerated]
		private void <SetupStateMachine>g__RunStateRunning_OnExit|68_3(State<RunState> currentState)
		{
			this.EventService.Call("stop", Array.Empty<object>());
			this.Logger.LogResults(this.PackageManagementService.StopRunningPackages());
			this.Logger.LogMessage("LuaCs running state exited", null, null);
		}

		// Token: 0x04000DC0 RID: 3520
		public const string PackageName = "LuaCsForBarotrauma";

		// Token: 0x04000DC1 RID: 3521
		private static LuaCsSetup _luaCsSetup;

		// Token: 0x04000DC3 RID: 3523
		public const bool IsServer = false;

		// Token: 0x04000DC4 RID: 3524
		public const bool IsClient = true;

		// Token: 0x04000DC5 RID: 3525
		private readonly IServicesProvider _servicesProvider;

		// Token: 0x04000DC6 RID: 3526
		private PerformanceCounterService _performanceCounterService;

		// Token: 0x04000DC7 RID: 3527
		private IEventService _eventService;

		// Token: 0x04000DC8 RID: 3528
		private LuaGame _game;

		// Token: 0x04000DC9 RID: 3529
		private ISettingBase<bool> _isCsEnabledForSession;

		// Token: 0x04000DCA RID: 3530
		private ISettingList<string> _csRunPolicy;

		// Token: 0x04000DCB RID: 3531
		private ISettingBase<bool> _hideUserNamesInLogs;

		// Token: 0x04000DCC RID: 3532
		private ISettingBase<bool> _useCaching;

		// Token: 0x04000DCD RID: 3533
		private RunState _runState;

		// Token: 0x04000DCE RID: 3534
		private readonly StateMachine<RunState> _runStateMachine;
	}
}
