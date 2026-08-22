using System;
using Barotrauma.Extensions;
using FluentResults;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F1 RID: 1265
	public class SettingsMenuSystem : ISettingsMenuSystem, ISystem, IReusableService, IService, IDisposable
	{
		// Token: 0x06005245 RID: 21061 RVA: 0x002C2206 File Offset: 0x002C0406
		public SettingsMenuSystem(IPackageManagementService packageManagementService, IConfigService configService, ILoggerService loggerService)
		{
			this._packageManagementService = packageManagementService;
			this._configService = configService;
			this._loggerService = loggerService;
			SettingsMenuSystem.SystemInstance = this;
			this._harmony = Harmony.CreateAndPatchAll(typeof(SettingsMenuSystem), null);
		}

		// Token: 0x06005246 RID: 21062 RVA: 0x002C223F File Offset: 0x002C043F
		[HarmonyPatch(typeof(SettingsMenu), "CreateModsTab")]
		[HarmonyPostfix]
		private static void SettingsMenu_CreateModsTab_Post(SettingsMenu __instance)
		{
			SettingsMenuSystem.SystemInstance._settingsMenuInstance = __instance;
			SettingsMenuSystem.SystemInstance.CreateSettingsMenu(__instance);
		}

		// Token: 0x06005247 RID: 21063 RVA: 0x002C2258 File Offset: 0x002C0458
		private void CreateSettingsMenu(SettingsMenu __instance)
		{
			this.DisposeMenuFrames();
			int tabCount = Enum.GetValues<SettingsMenu.Tab>().Length;
			SettingsMenu.Tab tabGameplayIndex = (SettingsMenu.Tab)tabCount;
			SettingsMenu.Tab tabControlsIndex = tabCount + SettingsMenu.Tab.AudioAndVC;
			this._gameplayContentFrame = this.CreateNewContentTab(tabGameplayIndex, __instance, GUIStyle.ComponentStyles.ContainsKey("SettingsMenuTab.LuaCsSettings") ? "SettingsMenuTab.LuaCsSettings" : "SettingsMenuTab.Mods", "LuaCsForBarotrauma.SettingsMenu.ModGameplayButton");
			this._gameplayMenuInstance = new ModsGameplaySettingsMenu(this._gameplayContentFrame, this._packageManagementService, this._configService, this._loggerService, __instance);
		}

		// Token: 0x06005248 RID: 21064 RVA: 0x002C22D0 File Offset: 0x002C04D0
		private GUIFrame CreateNewContentTab(SettingsMenu.Tab tab, SettingsMenu settingsMenuInstance, string settingsMenuTabName, string settingMenuHoverTextIdent)
		{
			ValueTuple<GUIButton, GUIFrame> tabContent;
			if (settingsMenuInstance.tabContents.TryGetValue(tab, out tabContent))
			{
				return tabContent.Item2;
			}
			GUIFrame contentFr = new GUIFrame(new RectTransform(Vector2.One * 0.95f, settingsMenuInstance.contentFrame.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), null, null);
			GUIButton button = new GUIButton(new RectTransform(Vector2.One, settingsMenuInstance.tabber.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Smallest), "", Alignment.Center, settingsMenuTabName, null)
			{
				ToolTip = TextManager.Get(settingMenuHoverTextIdent),
				OnClicked = delegate(GUIButton b, object _)
				{
					settingsMenuInstance.SelectTab(tab);
					return false;
				}
			};
			button.RectTransform.MaxSize = RectTransform.MaxPoint;
			button.Children.ForEach(delegate(GUIComponent c)
			{
				c.RectTransform.MaxSize = RectTransform.MaxPoint;
			});
			settingsMenuInstance.tabContents.Add(tab, new ValueTuple<GUIButton, GUIFrame>(button, contentFr));
			return contentFr;
		}

		// Token: 0x06005249 RID: 21065 RVA: 0x002C2432 File Offset: 0x002C0632
		[HarmonyPatch(typeof(SettingsMenu), "ApplyInstalledModChanges")]
		[HarmonyPostfix]
		private static void SettingsMenu_ApplyInstalledModChanges_Post()
		{
			ModsGameplaySettingsMenu gameplayMenuInstance = SettingsMenuSystem.SystemInstance._gameplayMenuInstance;
			if (gameplayMenuInstance != null)
			{
				gameplayMenuInstance.ApplyInstalledModChanges();
			}
			ModsControlsSettingsMenu controlsMenuInstance = SettingsMenuSystem.SystemInstance._controlsMenuInstance;
			if (controlsMenuInstance == null)
			{
				return;
			}
			controlsMenuInstance.ApplyInstalledModChanges();
		}

		// Token: 0x0600524A RID: 21066 RVA: 0x002C245D File Offset: 0x002C065D
		private void DisposeMenuFrames()
		{
			ModsControlsSettingsMenu controlsMenuInstance = this._controlsMenuInstance;
			if (controlsMenuInstance != null)
			{
				controlsMenuInstance.Dispose();
			}
			ModsGameplaySettingsMenu gameplayMenuInstance = this._gameplayMenuInstance;
			if (gameplayMenuInstance != null)
			{
				gameplayMenuInstance.Dispose();
			}
			this._controlsMenuInstance = null;
			this._gameplayMenuInstance = null;
		}

		// Token: 0x0600524B RID: 21067 RVA: 0x002C248F File Offset: 0x002C068F
		public void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			this.DisposeMenuFrames();
			GC.SuppressFinalize(this);
		}

		// Token: 0x170014DC RID: 5340
		// (get) Token: 0x0600524C RID: 21068 RVA: 0x002C24AB File Offset: 0x002C06AB
		// (set) Token: 0x0600524D RID: 21069 RVA: 0x002C24B8 File Offset: 0x002C06B8
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x0600524E RID: 21070 RVA: 0x002C24C6 File Offset: 0x002C06C6
		public Result Reset()
		{
			throw new NotImplementedException();
		}

		// Token: 0x04002BA0 RID: 11168
		private ModsControlsSettingsMenu _controlsMenuInstance;

		// Token: 0x04002BA1 RID: 11169
		private ModsGameplaySettingsMenu _gameplayMenuInstance;

		// Token: 0x04002BA2 RID: 11170
		private GUIFrame _gameplayContentFrame;

		// Token: 0x04002BA3 RID: 11171
		private GUIFrame _controlsContentFrame;

		// Token: 0x04002BA4 RID: 11172
		private SettingsMenu _settingsMenuInstance;

		// Token: 0x04002BA5 RID: 11173
		private readonly Harmony _harmony;

		// Token: 0x04002BA6 RID: 11174
		private readonly IPackageManagementService _packageManagementService;

		// Token: 0x04002BA7 RID: 11175
		private readonly IConfigService _configService;

		// Token: 0x04002BA8 RID: 11176
		private readonly ILoggerService _loggerService;

		// Token: 0x04002BA9 RID: 11177
		private static SettingsMenuSystem SystemInstance;

		// Token: 0x04002BAA RID: 11178
		private int _isDisposed;
	}
}
