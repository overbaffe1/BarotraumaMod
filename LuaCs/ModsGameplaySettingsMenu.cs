using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using Microsoft.Xna.Framework;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004EF RID: 1263
	internal sealed class ModsGameplaySettingsMenu : ModsSettingsMenuBase
	{
		// Token: 0x14000020 RID: 32
		// (add) Token: 0x06005230 RID: 21040 RVA: 0x002C1998 File Offset: 0x002BFB98
		// (remove) Token: 0x06005231 RID: 21041 RVA: 0x002C19D0 File Offset: 0x002BFBD0
		private event Action OnApplyInstalledModsChanges;

		// Token: 0x06005232 RID: 21042 RVA: 0x002C1A08 File Offset: 0x002BFC08
		public ModsGameplaySettingsMenu(GUIFrame contentFrame, IPackageManagementService packageManagementService, IConfigService configService, ILoggerService loggerService, SettingsMenu settingsMenuInstance)
		{
			ModsGameplaySettingsMenu.<>c__DisplayClass47_0 CS$<>8__locals1 = new ModsGameplaySettingsMenu.<>c__DisplayClass47_0();
			CS$<>8__locals1.configService = configService;
			CS$<>8__locals1.loggerService = loggerService;
			base..ctor(contentFrame, packageManagementService, CS$<>8__locals1.configService, settingsMenuInstance);
			CS$<>8__locals1.<>4__this = this;
			this._settingsInstancesGameplay = CS$<>8__locals1.configService.GetDisplayableConfigs().ToImmutableArray<ISettingBase>();
			this._loggerService = CS$<>8__locals1.loggerService;
			GUILayoutGroup mainLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), contentFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup menuTitleLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, ModsGameplaySettingsMenu.MenuTitleHeight), mainLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIUtil.Label(menuTitleLayoutGroup, ModsGameplaySettingsMenu.<.ctor>g__GetLocalizedString|47_2("LuaCsForBarotrauma.SettingsMenu.ModGameplayButton", "Mod Gameplay Settings"), GUIStyle.LargeFont, new Vector2(1f, 1f));
			GUILayoutGroup contentAreaLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.94f), mainLayoutGroup.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup searchBarLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, ModsGameplaySettingsMenu.SearchBarLayoutHeight), contentAreaLayoutGroup.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUIUtil.Label(searchBarLayoutGroup, "Search: ", GUIStyle.SubHeadingFont, new Vector2(ModsGameplaySettingsMenu.SearchBarLabelWidth, 1f));
			new GUITextBox(new RectTransform(new Vector2(ModsGameplaySettingsMenu.SearchBarTextBoxWidth, 0.1f), searchBarLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, true, true).OnTextChangedDelegate = delegate(GUITextBox btn, string txt)
			{
				base.<.ctor>g__GenerateDisplayFromFilter|1(txt);
				return true;
			};
			GUILayoutGroup settingsContentAreaGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, ModsGameplaySettingsMenu.ContentDisplayAreaHeightContainer), contentAreaLayoutGroup.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIUtil.Spacer(settingsContentAreaGroup, Vector2.One);
			ValueTuple<GUILayoutGroup, GUILayoutGroup> valueTuple = GUIUtil.CreateSidebars(settingsContentAreaGroup, true);
			this._modCategoryDisplayGroup = valueTuple.Item1;
			this._settingsDisplayGroup = valueTuple.Item2;
			this._modCategoryDisplayGroup.RectTransform.RelativeSize = new Vector2(ModsGameplaySettingsMenu.ContentLeftRightSplitPosition, ModsGameplaySettingsMenu.ContentDisplayAreaHeightInnerCategories);
			this._settingsDisplayGroup.RectTransform.RelativeSize = new Vector2(1f - ModsGameplaySettingsMenu.ContentLeftRightSplitPosition, ModsGameplaySettingsMenu.ContentDisplayAreaHeightInnerSettings);
			this._selectedCategory = "All";
			this.OnApplyInstalledModsChanges = delegate()
			{
				CS$<>8__locals1.<>4__this._settingsInstancesGameplay = CS$<>8__locals1.configService.GetDisplayableConfigs().ToImmutableArray<ISettingBase>();
				if (CS$<>8__locals1.<>4__this._selectedContentPackage != null && !base.<.ctor>g__GetTargetPackagesList|4().Contains(CS$<>8__locals1.<>4__this._selectedContentPackage))
				{
					CS$<>8__locals1.<>4__this._selectedContentPackage = null;
					CS$<>8__locals1.<>4__this._selectedCategory = string.Empty;
				}
				base.<.ctor>g__GenerateCategoryListDisplay|10(CS$<>8__locals1.<>4__this._modCategoryDisplayGroup, base.<.ctor>g__GetTargetPackagesList|4(), base.<.ctor>g__GetDisplayCategoriesList|3());
				base.<.ctor>g__GenerateSettingsListDisplay|11(CS$<>8__locals1.<>4__this._settingsDisplayGroup, base.<.ctor>g__GetDisplaySettingsList|5());
			};
			CS$<>8__locals1.<.ctor>g__GenerateCategoryListDisplay|10(this._modCategoryDisplayGroup, CS$<>8__locals1.<.ctor>g__GetTargetPackagesList|4(), CS$<>8__locals1.<.ctor>g__GetDisplayCategoriesList|3());
			CS$<>8__locals1.<.ctor>g__GenerateSettingsListDisplay|11(this._settingsDisplayGroup, CS$<>8__locals1.<.ctor>g__GetDisplaySettingsList|5());
		}

		// Token: 0x06005233 RID: 21043 RVA: 0x002C1D54 File Offset: 0x002BFF54
		protected override void DisposeInternal()
		{
			this.NewValuesCache.Clear();
			GUILayoutGroup modCategoryDisplayGroup = this._modCategoryDisplayGroup;
			if (modCategoryDisplayGroup != null)
			{
				modCategoryDisplayGroup.Parent.RemoveChild(this._modCategoryDisplayGroup);
			}
			GUILayoutGroup settingsDisplayGroup = this._settingsDisplayGroup;
			if (settingsDisplayGroup != null)
			{
				settingsDisplayGroup.Parent.RemoveChild(this._settingsDisplayGroup);
			}
			this._modCategoryDisplayGroup = null;
			this._settingsDisplayGroup = null;
		}

		// Token: 0x06005234 RID: 21044 RVA: 0x002C1DB4 File Offset: 0x002BFFB4
		public override void ApplyInstalledModChanges()
		{
			foreach (KeyValuePair<ISettingBase, OneOf<string, XElement>> kvp in this.NewValuesCache)
			{
				if (!kvp.Key.IsDisposed)
				{
					bool success = kvp.Key.TrySetSerializedValue(kvp.Value);
					if (success)
					{
						base.ConfigService.SaveConfigValue(kvp.Key);
						this._loggerService.LogDebug("Applied save value for " + kvp.Key.InternalName + " of " + kvp.Value.ToString(), null);
					}
				}
			}
			this.NewValuesCache.Clear();
			Action onApplyInstalledModsChanges = this.OnApplyInstalledModsChanges;
			if (onApplyInstalledModsChanges == null)
			{
				return;
			}
			onApplyInstalledModsChanges();
		}

		// Token: 0x06005236 RID: 21046 RVA: 0x002C2014 File Offset: 0x002C0214
		[CompilerGenerated]
		internal static string <.ctor>g__GetLocalizedString|47_2(string identifier, string defaultValue)
		{
			LocalizedString lstr = TextManager.Get(identifier);
			if (!lstr.IsNullOrWhiteSpace())
			{
				return lstr.Value;
			}
			return defaultValue;
		}

		// Token: 0x06005237 RID: 21047 RVA: 0x002C2038 File Offset: 0x002C0238
		[CompilerGenerated]
		internal static bool <.ctor>g__SettingMatchesQuery|47_7(ISettingBase setting, string queryText)
		{
			if (queryText.IsNullOrWhiteSpace())
			{
				return true;
			}
			queryText = queryText.ToLowerInvariant().Trim();
			if (setting.InternalName.ToLowerInvariant().Trim().Contains(queryText) || setting.OwnerPackage.Name.ToLowerInvariant().Trim().Contains(queryText))
			{
				return true;
			}
			IConfigDisplayInfo displayInfo = setting.GetDisplayInfo();
			return TextManager.Get(displayInfo.DisplayName).Value.ToLowerInvariant().Trim().Contains(queryText) || TextManager.Get(displayInfo.DisplayCategory).Value.ToLowerInvariant().Trim().Contains(queryText) || TextManager.Get(displayInfo.Description).Value.ToLowerInvariant().Trim().Contains(queryText) || TextManager.Get(displayInfo.Tooltip).Value.ToLowerInvariant().Trim().Contains(queryText);
		}

		// Token: 0x06005238 RID: 21048 RVA: 0x002C2122 File Offset: 0x002C0322
		[CompilerGenerated]
		internal static string <.ctor>g__GetPackageName|47_8(ContentPackage package)
		{
			if (package != null && package != ContentPackageManager.VanillaCorePackage)
			{
				return package.Name;
			}
			return "All";
		}

		// Token: 0x04002B6E RID: 11118
		private ImmutableArray<ISettingBase> _settingsInstancesGameplay;

		// Token: 0x04002B6F RID: 11119
		private GUILayoutGroup _modCategoryDisplayGroup;

		// Token: 0x04002B70 RID: 11120
		private GUILayoutGroup _settingsDisplayGroup;

		// Token: 0x04002B71 RID: 11121
		private string _selectedSearchQuery = string.Empty;

		// Token: 0x04002B72 RID: 11122
		private ContentPackage _selectedContentPackage;

		// Token: 0x04002B73 RID: 11123
		private string _selectedCategory = string.Empty;

		// Token: 0x04002B74 RID: 11124
		private ImmutableArray<ISettingBase> _currentlyDisplayedSettings;

		// Token: 0x04002B75 RID: 11125
		private ILoggerService _loggerService;

		// Token: 0x04002B76 RID: 11126
		private bool _promptOpen;

		// Token: 0x04002B77 RID: 11127
		private static float MenuTitleHeight = 0.06f;

		// Token: 0x04002B78 RID: 11128
		private static float ContentDisplayAreaHeightContainer = 0.93f;

		// Token: 0x04002B79 RID: 11129
		private static float ContentDisplayAreaHeightInnerCategories = 0.99f;

		// Token: 0x04002B7A RID: 11130
		private static float ContentDisplayAreaHeightInnerSettings = 0.97f;

		// Token: 0x04002B7B RID: 11131
		private static float ContentLeftRightSplitPosition = 0.3f;

		// Token: 0x04002B7C RID: 11132
		private static float SearchBarLayoutHeight = 0.06f;

		// Token: 0x04002B7D RID: 11133
		private static float SearchBarLabelWidth = 0.1f;

		// Token: 0x04002B7E RID: 11134
		private static float SearchBarLabelBoxSpacing = 0.05f;

		// Token: 0x04002B7F RID: 11135
		private static float SearchBarTextBoxWidth = 1f - ModsGameplaySettingsMenu.SearchBarLabelWidth - ModsGameplaySettingsMenu.SearchBarLabelBoxSpacing;

		// Token: 0x04002B80 RID: 11136
		private static float CategoriesDisplayListHeight = 0.945f;

		// Token: 0x04002B81 RID: 11137
		private static float CategoryButtonHeightRelative = 0.122f;

		// Token: 0x04002B82 RID: 11138
		private static float PackageSelectionButtonHeight = 0.07f;

		// Token: 0x04002B83 RID: 11139
		private static Color CategoryButtonHoverSelectColor = new Color(50, 50, 50, 255);

		// Token: 0x04002B84 RID: 11140
		private static Color CategoryButtonTextColor = Color.PeachPuff;

		// Token: 0x04002B85 RID: 11141
		private static Color CategoryButtonTextColorSelected = Color.White;

		// Token: 0x04002B86 RID: 11142
		private static Color CategoryButtonColorPressed = Color.TransparentBlack;

		// Token: 0x04002B87 RID: 11143
		private static float SettingLabelWidth = 0.6f;

		// Token: 0x04002B88 RID: 11144
		private static float SettingControlWidth = 0.4f;

		// Token: 0x04002B89 RID: 11145
		private static float SettingHeight = 0.05625f / ModsGameplaySettingsMenu.ContentDisplayAreaHeightContainer / ModsGameplaySettingsMenu.ContentDisplayAreaHeightInnerSettings;

		// Token: 0x04002B8A RID: 11146
		private static Color SettingEntryLabelTextColor = Color.PeachPuff;

		// Token: 0x04002B8B RID: 11147
		private static string SettingGUIFrameStyle = "";

		// Token: 0x04002B8C RID: 11148
		private static Color? SettingGUIFrameColor = null;

		// Token: 0x04002B8D RID: 11149
		private static Vector2 SettingsResetButtonTopSpacer = new Vector2(0f, 0.02f);

		// Token: 0x04002B8E RID: 11150
		private static Vector2 SettingsResetButtonDimensions = new Vector2(0.3f, 0.05f);

		// Token: 0x04002B8F RID: 11151
		private static string SettingsResetButtonStyle = "GUIButtonSmall";

		// Token: 0x04002B90 RID: 11152
		private static Color SettingsResetButtonColor = Color.DarkOliveGreen;

		// Token: 0x04002B91 RID: 11153
		private static Color SettingsResetButtonHoverColor = Color.Olive;

		// Token: 0x04002B92 RID: 11154
		private static Color SettingsResetButtonTextColor = Color.PeachPuff;

		// Token: 0x04002B93 RID: 11155
		private static Color SettingsResetButtonTextColorSelected = Color.White;

		// Token: 0x04002B94 RID: 11156
		private static Vector2 ResetConfirmationPromptDimensions = new Vector2(0.15f, 0.2f);

		// Token: 0x04002B95 RID: 11157
		private const string SettingsResetButtonText = "LuaCsForBarotrauma.SettingsMenu.ResetVisibleSettings";

		// Token: 0x04002B96 RID: 11158
		private const string SettingsResetPromptTitle = "LuaCsForBarotrauma.SettingsMenu.ResetPrompt.Title";

		// Token: 0x04002B97 RID: 11159
		private const string SettingsResetPromptContents = "LuaCsForBarotrauma.SettingsMenu.ResetPrompt.Message";

		// Token: 0x04002B98 RID: 11160
		private const string SettingsResetPromptYesText = "LuaCsForBarotrauma.SettingsMenu.ResetPrompt.Yes";

		// Token: 0x04002B99 RID: 11161
		private const string SettingsResetPromptNoText = "LuaCsForBarotrauma.SettingsMenu.ResetPrompt.No";
	}
}
