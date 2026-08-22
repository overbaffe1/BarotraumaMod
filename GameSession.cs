using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.PerkBehaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200006D RID: 109
	[NullableContext(1)]
	[Nullable(0)]
	internal class GameSession
	{
		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06000F8C RID: 3980 RVA: 0x00094160 File Offset: 0x00092360
		// (set) Token: 0x06000F8D RID: 3981 RVA: 0x00094168 File Offset: 0x00092368
		[Nullable(0)]
		public RoundSummary RoundSummary { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06000F8E RID: 3982 RVA: 0x00094171 File Offset: 0x00092371
		public static bool IsTabMenuOpen
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				return ((gameSession != null) ? gameSession.tabMenu : null) != null;
			}
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06000F8F RID: 3983 RVA: 0x00094187 File Offset: 0x00092387
		[Nullable(0)]
		public static TabMenu TabMenuInstance
		{
			[NullableContext(0)]
			get
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return null;
				}
				return gameSession.tabMenu;
			}
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x0009419C File Offset: 0x0009239C
		public bool ToggleTabMenu()
		{
			if (GameMain.NetworkMember != null && GameMain.NetLobbyScreen != null)
			{
				CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = GameMain.NetLobbyScreen.CharacterAppearanceCustomizationMenu;
				if (characterAppearanceCustomizationMenu != null)
				{
					characterAppearanceCustomizationMenu.Dispose();
				}
				GameMain.NetLobbyScreen.CharacterAppearanceCustomizationMenu = null;
				if (GameMain.NetLobbyScreen.JobSelectionFrame != null)
				{
					GameMain.NetLobbyScreen.JobSelectionFrame.Visible = false;
				}
			}
			if (this.tabMenu == null && !(this.GameMode is TutorialMode) && !ConversationAction.IsDialogOpen)
			{
				this.tabMenu = new TabMenu();
				HintManager.OnShowTabMenu();
			}
			else
			{
				TabMenu tabMenu = this.tabMenu;
				if (tabMenu != null)
				{
					tabMenu.OnClose();
				}
				this.tabMenu = null;
				NetLobbyScreen.JobInfoFrame = null;
			}
			return true;
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06000F91 RID: 3985 RVA: 0x00094240 File Offset: 0x00092440
		public bool AllowHrManagerBotTakeover
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
				return serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath && !serverSettings.IronmanMode && Level.IsLoadedFriendlyOutpost;
			}
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x0009427C File Offset: 0x0009247C
		private void CreateTopLeftButtons()
		{
			if (this.topLeftButtonGroup != null)
			{
				this.topLeftButtonGroup.RectTransform.Parent = null;
				this.topLeftButtonGroup = null;
				this.crewListButton = (this.commandButton = (this.tabMenuButton = null));
			}
			this.topLeftButtonGroup = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.ButtonAreaTop, GUI.Canvas), true, Anchor.CenterLeft)
			{
				AbsoluteSpacing = HUDLayoutSettings.Padding,
				CanBeFocused = false
			};
			int buttonHeight = GUI.IntScale(40f);
			Vector2 buttonSpriteSize = GUIStyle.GetComponentStyle("CrewListToggleButton").GetDefaultSprite().size;
			int buttonWidth = (int)((float)buttonHeight / buttonSpriteSize.Y * buttonSpriteSize.X);
			Point buttonSize = new Point(buttonWidth, buttonHeight);
			GUIButton guibutton = new GUIButton(new RectTransform(buttonSize, this.topLeftButtonGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CrewListToggleButton", null);
			string tag = "hudbutton.crewlist";
			string varName = "[key]";
			GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
			guibutton.ToolTip = TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.CrewOrders), FormatCapitals.No);
			guibutton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (this.CrewManager == null)
				{
					return false;
				}
				this.CrewManager.IsCrewMenuOpen = !this.CrewManager.IsCrewMenuOpen;
				return true;
			};
			this.crewListButton = guibutton;
			GUIButton guibutton2 = new GUIButton(new RectTransform(buttonSize, this.topLeftButtonGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CommandButton", null);
			string tag2 = "hudbutton.commandinterface";
			string varName2 = "[key]";
			keyMap = GameSettings.CurrentConfig.KeyMap;
			guibutton2.ToolTip = TextManager.GetWithVariable(tag2, varName2, keyMap.KeyBindText(InputType.Command), FormatCapitals.No);
			guibutton2.OnClicked = delegate(GUIButton button, object userData)
			{
				if (this.CrewManager == null)
				{
					return false;
				}
				this.CrewManager.ToggleCommandUI();
				return true;
			};
			this.commandButton = guibutton2;
			GUIButton guibutton3 = new GUIButton(new RectTransform(buttonSize, this.topLeftButtonGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "TabMenuButton", null);
			string tag3 = "hudbutton.tabmenu";
			string varName3 = "[key]";
			keyMap = GameSettings.CurrentConfig.KeyMap;
			guibutton3.ToolTip = TextManager.GetWithVariable(tag3, varName3, keyMap.KeyBindText(InputType.InfoTab), FormatCapitals.No);
			guibutton3.OnClicked = ((GUIButton button, object userData) => this.ToggleTabMenu());
			this.tabMenuButton = guibutton3;
			this.talentPointNotification = GameSession.CreateNotificationIcon(this.tabMenuButton, true);
			this.eventLogNotification = GameSession.CreateNotificationIcon(this.tabMenuButton, true);
			this.deathChoiceInfoFrame = new GUIFrame(new RectTransform(new Vector2(0.5f, 1f), this.topLeftButtonGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(HUDLayoutSettings.ButtonAreaTop.Width / 3, int.MaxValue)
			}, null, null)
			{
				CanBeFocused = false,
				Visible = false
			};
			this.respawnInfoText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), this.deathChoiceInfoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, true, "", null)
			{
				CanBeFocused = false
			};
			this.deathChoiceButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), this.deathChoiceInfoFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				AbsoluteSpacing = HUDLayoutSettings.Padding,
				Stretch = true,
				Visible = false
			};
			GUIButton guibutton4 = new GUIButton(new RectTransform(Vector2.One * 0.9f, this.deathChoiceButtonContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("takeoverbotquestionprompttakeoverbot"), Alignment.Center, "GUIButtonSmall", null);
			guibutton4.OnClicked = delegate(GUIButton btn, object userdata)
			{
				DeathPrompt.CreateTakeOverBotPanel();
				return true;
			};
			this.takeOverBotButton = guibutton4;
			this.takeOverBotButton.TextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton5 = new GUIButton(new RectTransform(Vector2.One * 0.9f, this.deathChoiceButtonContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("npctitle.hrmanager"), Alignment.Center, "GUIButtonSmall", null);
			guibutton5.OnClicked = delegate(GUIButton btn, object userdata)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					campaign.ShowCampaignUI = true;
					CampaignUI campaignUI = campaign.CampaignUI;
					if (campaignUI != null)
					{
						campaignUI.SelectTab(CampaignMode.InteractionType.Crew, null);
					}
				}
				return true;
			};
			this.hrManagerButton = guibutton5;
			this.hrManagerButton.TextBlock.AutoScaleHorizontal = true;
			LocalizedString questionText = TextManager.GetWithVariable("respawnquestionprompt", "[percentage]", ((int)Math.Round((double)RespawnManager.SkillLossPercentageOnImmediateRespawn)).ToString(), FormatCapitals.No);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One * 0.9f, this.deathChoiceButtonContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("respawnquestionpromptrespawn"), null, "");
			guitickBox.ToolTip = questionText;
			guitickBox.OnSelected = delegate(GUITickBox tickbox)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.SendRespawnPromptResponse(!tickbox.Selected);
				}
				return true;
			};
			this.deathChoiceTickBox = guitickBox;
			this.prevTopLeftButtonsResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06000F93 RID: 3987 RVA: 0x00094828 File Offset: 0x00092A28
		public void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			GameMode gameMode = this.GameMode;
			if (gameMode != null)
			{
				gameMode.AddToGUIUpdateList();
			}
			TabMenu tabMenu = this.tabMenu;
			if (tabMenu != null)
			{
				tabMenu.AddToGUIUpdateList();
			}
			ObjectiveManager.AddToGUIUpdateList();
			CampaignMode campaign = this.GameMode as CampaignMode;
			if ((campaign == null || (!campaign.ForceMapUI && !campaign.ShowCampaignUI)) && !CoroutineManager.IsCoroutineRunning("LevelTransition") && !CoroutineManager.IsCoroutineRunning("SubmarineTransition"))
			{
				if (this.topLeftButtonGroup == null || this.prevTopLeftButtonsResolution.X != GameMain.GraphicsWidth || this.prevTopLeftButtonsResolution.Y != GameMain.GraphicsHeight)
				{
					this.CreateTopLeftButtons();
				}
				this.crewListButton.Selected = (this.CrewManager != null && this.CrewManager.IsCrewMenuOpen);
				this.commandButton.Selected = CrewManager.IsCommandInterfaceOpen;
				this.commandButton.Enabled = CrewManager.CanIssueOrders;
				this.tabMenuButton.Selected = GameSession.IsTabMenuOpen;
				this.topLeftButtonGroup.AddToGUIUpdateList(false, 0);
			}
			if (GameMain.NetworkMember != null)
			{
				CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = GameMain.NetLobbyScreen.CharacterAppearanceCustomizationMenu;
				if (characterAppearanceCustomizationMenu != null)
				{
					characterAppearanceCustomizationMenu.AddToGUIUpdateList();
				}
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					GUIFrame jobSelectionFrame = netLobbyScreen.JobSelectionFrame;
					if (jobSelectionFrame != null)
					{
						jobSelectionFrame.AddToGUIUpdateList(false, 1);
					}
				}
			}
			DeathPrompt deathPrompt = this.DeathPrompt;
			if (deathPrompt == null)
			{
				return;
			}
			deathPrompt.AddToGUIUpdateList();
		}

		// Token: 0x06000F94 RID: 3988 RVA: 0x00094980 File Offset: 0x00092B80
		[NullableContext(0)]
		public static GUIImage CreateNotificationIcon(GUIComponent parent, bool offset = true)
		{
			GUIImage indicator = new GUIImage(new RectTransform(new Vector2(0.45f), parent.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.BothWidth), "TalentPointNotification", GUIImage.ScalingMode.None)
			{
				Visible = false,
				CanBeFocused = false
			};
			Point notificationSize = indicator.RectTransform.NonScaledSize;
			if (offset)
			{
				indicator.RectTransform.AbsoluteOffset = new Point(-(notificationSize.X / 2), -(notificationSize.Y / 2));
			}
			return indicator;
		}

		// Token: 0x06000F95 RID: 3989 RVA: 0x00094A0C File Offset: 0x00092C0C
		public void EnableEventLogNotificationIcon(bool enabled)
		{
			if (this.eventLogNotification == null)
			{
				return;
			}
			if (!this.eventLogNotification.Visible && enabled)
			{
				this.eventLogNotification.Pulsate(Vector2.One, Vector2.One * 2f, 1f);
			}
			this.eventLogNotification.Visible = enabled;
		}

		// Token: 0x06000F96 RID: 3990 RVA: 0x00094A64 File Offset: 0x00092C64
		[NullableContext(0)]
		public static void UpdateTalentNotificationIndicator(GUIImage indicator)
		{
			if (indicator == null)
			{
				return;
			}
			Character controlled = Character.Controlled;
			indicator.Visible = (((controlled != null) ? controlled.Info : null) != null && Character.Controlled.Info.GetAvailableTalentPoints() > 0 && !Character.Controlled.HasUnlockedAllTalents());
		}

		// Token: 0x06000F97 RID: 3991 RVA: 0x00094AB0 File Offset: 0x00092CB0
		public void HUDScaleChanged()
		{
			this.CreateTopLeftButtons();
			GameMode gameMode = this.GameMode;
			if (gameMode == null)
			{
				return;
			}
			gameMode.HUDScaleChanged();
		}

		// Token: 0x06000F98 RID: 3992 RVA: 0x00094AC8 File Offset: 0x00092CC8
		[NullableContext(0)]
		public void SetRespawnInfo(string text, Color textColor, bool waitForNextRoundRespawn, bool hideButtons = false)
		{
			if (this.topLeftButtonGroup == null)
			{
				return;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			bool permadeathMode = serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath;
			NetworkMember networkMember2 = GameMain.NetworkMember;
			serverSettings = ((networkMember2 != null) ? networkMember2.ServerSettings : null);
			bool ironmanMode = serverSettings != null && serverSettings.IronmanModeActive;
			bool hasRespawnOptions;
			if (permadeathMode)
			{
				bool flag;
				if (!ironmanMode)
				{
					GameClient client = GameMain.Client;
					if (client != null)
					{
						flag = (client.CharacterInfo == null || client.CharacterInfo.PermanentlyDead);
						goto IL_76;
					}
				}
				flag = false;
				IL_76:
				hasRespawnOptions = flag;
			}
			else
			{
				Level loaded = Level.Loaded;
				bool flag2;
				if (loaded == null || loaded.Type != LevelData.LevelType.Outpost)
				{
					GameClient client2 = GameMain.Client;
					flag2 = (client2 != null && (client2.CharacterInfo == null || client2.HasSpawned));
				}
				else
				{
					flag2 = false;
				}
				hasRespawnOptions = flag2;
			}
			this.deathChoiceInfoFrame.Visible = (!text.IsNullOrEmpty() || hasRespawnOptions);
			if (!this.deathChoiceInfoFrame.Visible)
			{
				return;
			}
			this.respawnInfoText.Text = text;
			this.respawnInfoText.TextColor = textColor;
			if (!(GameMain.GameSession.GameMode is CampaignMode) || Character.Controlled != null)
			{
				this.deathChoiceButtonContainer.Visible = false;
				return;
			}
			this.deathChoiceButtonContainer.Visible = (hasRespawnOptions && !hideButtons);
			if (this.deathChoiceButtonContainer.Visible)
			{
				this.hrManagerButton.Visible = this.AllowHrManagerBotTakeover;
				if (permadeathMode && ironmanMode)
				{
					this.takeOverBotButton.Visible = false;
					this.deathChoiceTickBox.Visible = false;
					this.deathChoiceTickBox.Selected = false;
					return;
				}
				GUIComponent guicomponent = this.takeOverBotButton;
				bool visible;
				if (permadeathMode)
				{
					NetworkMember networkMember3 = GameMain.NetworkMember;
					serverSettings = ((networkMember3 != null) ? networkMember3.ServerSettings : null);
					visible = (serverSettings != null && serverSettings.AllowBotTakeoverOnPermadeath);
				}
				else
				{
					visible = false;
				}
				guicomponent.Visible = visible;
				this.deathChoiceTickBox.Visible = !permadeathMode;
				this.deathChoiceTickBox.Selected = !waitForNextRoundRespawn;
			}
		}

		// Token: 0x06000F99 RID: 3993 RVA: 0x00094CA0 File Offset: 0x00092EA0
		public void RefreshAnyOpenPlayerInfo()
		{
			if (GameSession.IsTabMenuOpen && TabMenu.SelectedTab == TabMenu.InfoFrameTab.Talents)
			{
				GameSession.TabMenuInstance.SelectInfoFrameTab(TabMenu.InfoFrameTab.Talents);
			}
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x00094CBC File Offset: 0x00092EBC
		[NullableContext(0)]
		public void Draw(SpriteBatch spriteBatch)
		{
			GameMode gameMode = this.GameMode;
			if (gameMode == null)
			{
				return;
			}
			gameMode.Draw(spriteBatch);
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06000F9B RID: 3995 RVA: 0x00094CCF File Offset: 0x00092ECF
		// (set) Token: 0x06000F9C RID: 3996 RVA: 0x00094CD7 File Offset: 0x00092ED7
		public Version LastSaveVersion { get; set; } = GameMain.Version;

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06000F9D RID: 3997 RVA: 0x00094CE0 File Offset: 0x00092EE0
		// (set) Token: 0x06000F9E RID: 3998 RVA: 0x00094CE8 File Offset: 0x00092EE8
		public float RoundDuration { get; private set; }

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06000F9F RID: 3999 RVA: 0x00094CF1 File Offset: 0x00092EF1
		public IEnumerable<Mission> Missions
		{
			get
			{
				return this.missions;
			}
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06000FA0 RID: 4000 RVA: 0x00094CF9 File Offset: 0x00092EF9
		public IEnumerable<Character> Casualties
		{
			get
			{
				return this.casualties;
			}
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x00094D01 File Offset: 0x00092F01
		public void IncrementPermadeath([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId)
		{
			this.permadeathsPerAccount[accountId] = this.permadeathsPerAccount.GetValueOrDefault(accountId, 0) + 1;
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x00094D1E File Offset: 0x00092F1E
		public int PermadeathCountForAccount([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId)
		{
			return this.permadeathsPerAccount.GetValueOrDefault(accountId, 0);
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06000FA3 RID: 4003 RVA: 0x00094D2D File Offset: 0x00092F2D
		// (set) Token: 0x06000FA4 RID: 4004 RVA: 0x00094D35 File Offset: 0x00092F35
		public bool IsRunning { get; private set; }

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06000FA5 RID: 4005 RVA: 0x00094D3E File Offset: 0x00092F3E
		// (set) Token: 0x06000FA6 RID: 4006 RVA: 0x00094D46 File Offset: 0x00092F46
		public bool RoundEnding { get; private set; }

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06000FA7 RID: 4007 RVA: 0x00094D4F File Offset: 0x00092F4F
		// (set) Token: 0x06000FA8 RID: 4008 RVA: 0x00094D57 File Offset: 0x00092F57
		[Nullable(2)]
		public Level Level { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06000FA9 RID: 4009 RVA: 0x00094D60 File Offset: 0x00092F60
		// (set) Token: 0x06000FAA RID: 4010 RVA: 0x00094D68 File Offset: 0x00092F68
		[Nullable(2)]
		public LevelData LevelData { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06000FAB RID: 4011 RVA: 0x00094D71 File Offset: 0x00092F71
		// (set) Token: 0x06000FAC RID: 4012 RVA: 0x00094D79 File Offset: 0x00092F79
		public bool MirrorLevel { get; private set; }

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06000FAD RID: 4013 RVA: 0x00094D82 File Offset: 0x00092F82
		[Nullable(2)]
		public Map Map
		{
			[NullableContext(2)]
			get
			{
				CampaignMode campaignMode = this.GameMode as CampaignMode;
				if (campaignMode == null)
				{
					return null;
				}
				return campaignMode.Map;
			}
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06000FAE RID: 4014 RVA: 0x00094D9A File Offset: 0x00092F9A
		[Nullable(2)]
		public CampaignMode Campaign
		{
			[NullableContext(2)]
			get
			{
				return this.GameMode as CampaignMode;
			}
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06000FAF RID: 4015 RVA: 0x00094DA8 File Offset: 0x00092FA8
		public Location StartLocation
		{
			get
			{
				if (this.Map != null)
				{
					return this.Map.CurrentLocation;
				}
				if (this.dummyLocations == null)
				{
					this.dummyLocations = ((this.LevelData == null) ? GameSession.CreateDummyLocations(string.Empty, null) : GameSession.CreateDummyLocations(this.LevelData, null));
				}
				if (this.dummyLocations == null)
				{
					throw new NullReferenceException("dummyLocations is null somehow!");
				}
				return this.dummyLocations[0];
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06000FB0 RID: 4016 RVA: 0x00094E14 File Offset: 0x00093014
		public Location EndLocation
		{
			get
			{
				if (this.Map != null)
				{
					return this.Map.SelectedLocation;
				}
				if (this.dummyLocations == null)
				{
					this.dummyLocations = ((this.LevelData == null) ? GameSession.CreateDummyLocations(string.Empty, null) : GameSession.CreateDummyLocations(this.LevelData, null));
				}
				if (this.dummyLocations == null)
				{
					throw new NullReferenceException("dummyLocations is null somehow!");
				}
				return this.dummyLocations[1];
			}
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x00094E7F File Offset: 0x0009307F
		// (set) Token: 0x06000FB2 RID: 4018 RVA: 0x00094E87 File Offset: 0x00093087
		public SubmarineInfo SubmarineInfo { get; set; }

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06000FB3 RID: 4019 RVA: 0x00094E90 File Offset: 0x00093090
		// (set) Token: 0x06000FB4 RID: 4020 RVA: 0x00094E98 File Offset: 0x00093098
		public SubmarineInfo EnemySubmarineInfo { get; set; }

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06000FB5 RID: 4021 RVA: 0x00094EA1 File Offset: 0x000930A1
		// (set) Token: 0x06000FB6 RID: 4022 RVA: 0x00094EA9 File Offset: 0x000930A9
		[Nullable(2)]
		public Submarine Submarine { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06000FB7 RID: 4023 RVA: 0x00094EB2 File Offset: 0x000930B2
		[Nullable(new byte[]
		{
			1,
			0
		})]
		public IEnumerable<ValueTuple<CharacterTeamType, Identifier>> UnlockedRecipes
		{
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			get
			{
				return this.unlockedRecipes;
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x00094EBA File Offset: 0x000930BA
		// (set) Token: 0x06000FB9 RID: 4025 RVA: 0x00094EC2 File Offset: 0x000930C2
		public CampaignDataPath DataPath { get; set; }

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06000FBA RID: 4026 RVA: 0x00094ECB File Offset: 0x000930CB
		public bool TraitorsEnabled
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				return ((networkMember != null) ? networkMember.ServerSettings : null) != null && GameMain.NetworkMember.ServerSettings.TraitorProbability > 0f;
			}
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x00094EF8 File Offset: 0x000930F8
		private GameSession(SubmarineInfo submarineInfo)
		{
			this.SubmarineInfo = submarineInfo;
			this.EnemySubmarineInfo = this.SubmarineInfo;
			GameMain.GameSession = this;
			this.EventManager = new EventManager();
		}

		// Token: 0x06000FBC RID: 4028 RVA: 0x00094F71 File Offset: 0x00093171
		private GameSession(SubmarineInfo submarineInfo, SubmarineInfo enemySubmarineInfo) : this(submarineInfo)
		{
			this.EnemySubmarineInfo = enemySubmarineInfo;
		}

		// Token: 0x06000FBD RID: 4029 RVA: 0x00094F84 File Offset: 0x00093184
		public GameSession(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			0,
			1
		})] Option<SubmarineInfo> enemySub, CampaignDataPath dataPath, GameModePreset gameModePreset, CampaignSettings settings, [Nullable(2)] string seed = null, [Nullable(2)] IEnumerable<Identifier> missionTypes = null) : this(submarineInfo)
		{
			this.DataPath = dataPath;
			this.CrewManager = new CrewManager(gameModePreset.IsSinglePlayer);
			this.GameMode = this.InstantiateGameMode(gameModePreset, seed, submarineInfo, settings, null, missionTypes);
			SubmarineInfo enemySubmarine;
			this.EnemySubmarineInfo = (enemySub.TryUnwrap(out enemySubmarine) ? enemySubmarine : submarineInfo);
			this.InitOwnedSubs(submarineInfo, null);
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x00094FE4 File Offset: 0x000931E4
		public GameSession(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			0,
			1
		})] Option<SubmarineInfo> enemySub, GameModePreset gameModePreset, [Nullable(2)] string seed = null, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<MissionPrefab> missionPrefabs = null) : this(submarineInfo)
		{
			this.CrewManager = new CrewManager(gameModePreset.IsSinglePlayer);
			this.GameMode = this.InstantiateGameMode(gameModePreset, seed, submarineInfo, CampaignSettings.Empty, missionPrefabs, null);
			SubmarineInfo enemySubmarine;
			this.EnemySubmarineInfo = (enemySub.TryUnwrap(out enemySubmarine) ? enemySubmarine : submarineInfo);
			this.InitOwnedSubs(submarineInfo, null);
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x00095040 File Offset: 0x00093240
		public GameSession(SubmarineInfo submarineInfo, List<SubmarineInfo> ownedSubmarines, XDocument doc, CampaignDataPath campaignData) : this(submarineInfo)
		{
			this.DataPath = campaignData;
			GameMain.GameSession = this;
			XElement root = doc.Root;
			if (root == null)
			{
				throw new NullReferenceException("Game session XML element is invalid: document is null.");
			}
			XElement rootElement = root;
			this.LastSaveVersion = doc.Root.GetAttributeVersion("version", GameMain.Version);
			foreach (XElement subElement in rootElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "gamemode") && !(a == "singleplayercampaign"))
				{
					if (!(a == "multiplayercampaign"))
					{
						if (a == "permadeaths")
						{
							this.permadeathsPerAccount = new Dictionary<Option<AccountId>, int>();
							foreach (XElement accountElement in subElement.Elements("account"))
							{
								XAttribute accountIdAttr = accountElement.Attribute("id");
								if (accountIdAttr != null)
								{
									XAttribute permadeathCountAttr = accountElement.Attribute("permadeathcount");
									if (permadeathCountAttr != null)
									{
										try
										{
											this.permadeathsPerAccount[AccountId.Parse(accountIdAttr.Value)] = int.Parse(permadeathCountAttr.Value);
										}
										catch (Exception e)
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 3);
											defaultInterpolatedStringHandler.AppendLiteral("Exception while trying to load permadeath counts!\n");
											defaultInterpolatedStringHandler.AppendFormatted<Exception>(e);
											defaultInterpolatedStringHandler.AppendLiteral("\n id: ");
											defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(accountIdAttr);
											defaultInterpolatedStringHandler.AppendLiteral("\n permadeathcount: ");
											defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(permadeathCountAttr);
											DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
										}
									}
								}
							}
						}
					}
					else
					{
						this.CrewManager = new CrewManager(false);
						MultiPlayerCampaign mpCampaign = MultiPlayerCampaign.LoadNew(subElement);
						this.GameMode = mpCampaign;
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
						{
							mpCampaign.LoadNewLevel();
							this.InitOwnedSubs(submarineInfo, ownedSubmarines);
							SaveUtil.SaveGame(campaignData, true);
						}
					}
				}
				else
				{
					this.CrewManager = new CrewManager(true);
					SinglePlayerCampaign campaign = SinglePlayerCampaign.Load(subElement);
					campaign.LoadNewLevel();
					this.GameMode = campaign;
					this.InitOwnedSubs(submarineInfo, ownedSubmarines);
				}
			}
		}

		// Token: 0x06000FC0 RID: 4032 RVA: 0x000952D8 File Offset: 0x000934D8
		private void InitOwnedSubs(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			2,
			1
		})] List<SubmarineInfo> ownedSubmarines = null)
		{
			this.OwnedSubmarines = (ownedSubmarines ?? new List<SubmarineInfo>());
			if (submarineInfo != null && !this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == submarineInfo.Name))
			{
				this.OwnedSubmarines.Add(submarineInfo);
			}
		}

		// Token: 0x06000FC1 RID: 4033 RVA: 0x00095334 File Offset: 0x00093534
		private GameMode InstantiateGameMode(GameModePreset gameModePreset, [Nullable(2)] string seed, SubmarineInfo selectedSub, CampaignSettings settings, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<MissionPrefab> missionPrefabs = null, [Nullable(2)] IEnumerable<Identifier> missionTypes = null)
		{
			if (gameModePreset.GameModeType == typeof(CoOpMode))
			{
				if (missionPrefabs == null)
				{
					return new CoOpMode(gameModePreset, missionTypes, seed ?? ToolBox.RandomSeed(8));
				}
				return new CoOpMode(gameModePreset, missionPrefabs);
			}
			else if (gameModePreset.GameModeType == typeof(PvPMode))
			{
				if (missionPrefabs == null)
				{
					return new PvPMode(gameModePreset, missionTypes, seed ?? ToolBox.RandomSeed(8));
				}
				return new PvPMode(gameModePreset, missionPrefabs);
			}
			else
			{
				if (gameModePreset.GameModeType == typeof(MultiPlayerCampaign))
				{
					MultiPlayerCampaign campaign = MultiPlayerCampaign.StartNew(seed ?? ToolBox.RandomSeed(8), settings);
					if (selectedSub != null)
					{
						campaign.Bank.Deduct(selectedSub.Price);
						campaign.Bank.Balance = Math.Max(campaign.Bank.Balance, 0);
					}
					return campaign;
				}
				if (gameModePreset.GameModeType == typeof(SinglePlayerCampaign))
				{
					SinglePlayerCampaign campaign2 = SinglePlayerCampaign.StartNew(seed ?? ToolBox.RandomSeed(8), settings);
					if (selectedSub != null)
					{
						campaign2.Bank.TryDeduct(selectedSub.Price);
						campaign2.Bank.Balance = Math.Max(campaign2.Bank.Balance, 0);
					}
					return campaign2;
				}
				if (gameModePreset.GameModeType == typeof(TutorialMode))
				{
					return new TutorialMode(gameModePreset);
				}
				if (gameModePreset.GameModeType == typeof(TestGameMode))
				{
					return new TestGameMode(gameModePreset);
				}
				if (gameModePreset.GameModeType == typeof(GameMode))
				{
					return new GameMode(gameModePreset);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a game mode of the type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(gameModePreset.GameModeType);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x00095500 File Offset: 0x00093700
		public static Location[] CreateDummyLocations(LevelData levelData, [Nullable(2)] LocationType forceLocationType = null)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
			OutpostGenerationParams forceParams = (levelData != null) ? levelData.ForceOutpostGenerationParams : null;
			if (forceLocationType == null && forceParams != null && forceParams.AllowedLocationTypes.Any<Identifier>() && !forceParams.AllowedLocationTypes.Contains("Any".ToIdentifier()))
			{
				forceLocationType = (from lt in LocationType.Prefabs
				where forceParams.AllowedLocationTypes.Contains(lt.Identifier)
				select lt).GetRandom(rand);
			}
			Location[] dummyLocations = GameSession.CreateDummyLocations(rand, forceLocationType);
			List<Faction> factions = new List<Faction>();
			foreach (FactionPrefab factionPrefab in FactionPrefab.Prefabs)
			{
				factions.Add(new Faction(new CampaignMetadata(), factionPrefab));
			}
			foreach (Location location in dummyLocations)
			{
				if (location.Type.HasOutpost)
				{
					location.Faction = CampaignMode.GetRandomFaction(factions, rand, false, true);
					location.SecondaryFaction = CampaignMode.GetRandomFaction(factions, rand, true, true);
				}
			}
			return dummyLocations;
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x00095638 File Offset: 0x00093838
		public static Location[] CreateDummyLocations(string seed, [Nullable(2)] LocationType forceLocationType = null)
		{
			return GameSession.CreateDummyLocations(new MTRandom(ToolBox.StringToInt(seed)), forceLocationType);
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x0009564C File Offset: 0x0009384C
		private static Location[] CreateDummyLocations(Random rand, [Nullable(2)] LocationType forceLocationType = null)
		{
			Location[] dummyLocations = new Location[2];
			for (int i = 0; i < 2; i++)
			{
				dummyLocations[i] = Location.CreateRandom(new Vector2((float)rand.NextDouble() * 10000f, (float)rand.NextDouble() * 10000f), null, null, rand, true, forceLocationType, null);
			}
			return dummyLocations;
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x000956AA File Offset: 0x000938AA
		[NullableContext(2)]
		public static bool ShouldApplyDisembarkPoints(GameModePreset preset)
		{
			return preset == null || preset == GameModePreset.Sandbox || preset == GameModePreset.Mission || preset == GameModePreset.PvP;
		}

		// Token: 0x06000FC6 RID: 4038 RVA: 0x000956CB File Offset: 0x000938CB
		public void LoadPreviousSave()
		{
			AchievementManager.OnRoundEnded(this, true);
			Submarine.Unload();
			SaveUtil.LoadGame(this.DataPath);
		}

		// Token: 0x06000FC7 RID: 4039 RVA: 0x000956E4 File Offset: 0x000938E4
		public void SwitchSubmarine(SubmarineInfo newSubmarine, bool transferItems, [Nullable(2)] Client client = null)
		{
			if (!this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == newSubmarine.Name))
			{
				this.OwnedSubmarines.Add(newSubmarine);
			}
			else
			{
				for (int i = 0; i < this.OwnedSubmarines.Count; i++)
				{
					if (this.OwnedSubmarines[i].Name == newSubmarine.Name)
					{
						newSubmarine = this.OwnedSubmarines[i];
						break;
					}
				}
			}
			this.Campaign.PendingSubmarineSwitch = newSubmarine;
			this.Campaign.TransferItemsOnSubSwitch = transferItems;
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x00095794 File Offset: 0x00093994
		public bool TryPurchaseSubmarine(SubmarineInfo newSubmarine, [Nullable(2)] Client client = null)
		{
			if (this.Campaign == null)
			{
				return false;
			}
			int price = newSubmarine.GetPrice(null, null);
			if (GameMain.NetworkMember != null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null || !networkMember.IsServer)
				{
					goto IL_4E;
				}
			}
			if (!this.Campaign.TryPurchase(client, price))
			{
				return false;
			}
			IL_4E:
			if (!this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == newSubmarine.Name))
			{
				GameAnalyticsManager.AddMoneySpentEvent(price, GameAnalyticsManager.MoneySink.SubmarinePurchase, newSubmarine.Name);
				this.OwnedSubmarines.Add(newSubmarine);
			}
			return true;
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x0009582C File Offset: 0x00093A2C
		public bool IsSubmarineOwned(SubmarineInfo query)
		{
			Submarine mainSub = Submarine.MainSub;
			return ((mainSub != null) ? mainSub.Info.Name : null) == query.Name || (this.OwnedSubmarines != null && this.OwnedSubmarines.Any((SubmarineInfo os) => os.Name == query.Name));
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00095894 File Offset: 0x00093A94
		public bool IsCurrentLocationRadiated()
		{
			Map map = this.Map;
			if (((map != null) ? map.CurrentLocation : null) == null || this.Campaign == null)
			{
				return false;
			}
			bool isRadiated = this.Map.CurrentLocation.IsRadiated();
			Level loaded = Level.Loaded;
			Location endLocation = (loaded != null) ? loaded.EndLocation : null;
			if (endLocation != null)
			{
				isRadiated |= endLocation.IsRadiated();
			}
			return isRadiated;
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x000958F0 File Offset: 0x00093AF0
		public void StartRound(string levelSeed, float? difficulty = null, [Nullable(2)] LevelGenerationParams levelGenerationParams = null, Identifier forceBiome = default(Identifier))
		{
			if (this.GameMode == null)
			{
				return;
			}
			LevelData randomLevel = null;
			bool pvpOnly = this.GameMode is PvPMode;
			foreach (Mission mission in this.Missions.Union(this.GameMode.Missions))
			{
				MissionPrefab missionPrefab = mission.Prefab;
				if (missionPrefab != null && missionPrefab.AllowedLocationTypes.Any<Identifier>() && !missionPrefab.AllowedConnectionTypes.Any<ValueTuple<Identifier, Identifier>>())
				{
					Random rand = new MTRandom(ToolBox.StringToInt(levelSeed));
					LocationType locationType = (from lt in LocationType.Prefabs
					orderby lt.UintIdentifier
					where missionPrefab.AllowedLocationTypes.Any((Identifier m) => m == lt.Identifier)
					select lt).GetRandom(rand);
					this.dummyLocations = GameSession.CreateDummyLocations(levelSeed, locationType);
					if (!GameSession.<StartRound>g__tryCreateFaction|128_5(mission.Prefab.RequiredLocationFaction, this.dummyLocations, delegate(Location loc, Faction fac)
					{
						loc.Faction = fac;
					}))
					{
						GameSession.<StartRound>g__tryCreateFaction|128_5(locationType.Faction, this.dummyLocations, delegate(Location loc, Faction fac)
						{
							loc.Faction = fac;
						});
						GameSession.<StartRound>g__tryCreateFaction|128_5(locationType.SecondaryFaction, this.dummyLocations, delegate(Location loc, Faction fac)
						{
							loc.SecondaryFaction = fac;
						});
					}
					randomLevel = LevelData.CreateRandom(levelSeed, difficulty, levelGenerationParams, forceBiome, true, pvpOnly);
					break;
				}
			}
			if (randomLevel == null)
			{
				randomLevel = LevelData.CreateRandom(levelSeed, difficulty, levelGenerationParams, forceBiome, false, pvpOnly);
			}
			this.StartRound(randomLevel, false, null, null);
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x00095AE4 File Offset: 0x00093CE4
		[NullableContext(2)]
		private bool TryGenerateStationAroundModule(SubmarineInfo moduleInfo, out Submarine outpostSub)
		{
			outpostSub = null;
			if (moduleInfo == null)
			{
				return false;
			}
			IEnumerable<OutpostGenerationParams> allSuitableOutpostParams = from outpostParam in OutpostGenerationParams.OutpostParams
			where base.<TryGenerateStationAroundModule>g__IsOutpostParamsSuitable|2(outpostParam)
			select outpostParam;
			OutpostGenerationParams suitableOutpostParams = (from p in allSuitableOutpostParams
			where p.AllowedLocationTypes.Any<Identifier>()
			select p).GetRandomUnsynced<OutpostGenerationParams>() ?? allSuitableOutpostParams.GetRandomUnsynced<OutpostGenerationParams>();
			if (suitableOutpostParams == null)
			{
				DebugConsole.AddWarning("No suitable generation parameters found for ForceOutpostModule, skipping outpost generation!", null);
				return false;
			}
			LocationType suitableLocationType = (from locationType in LocationType.Prefabs
			where suitableOutpostParams.AllowedLocationTypes.Contains(locationType.Identifier)
			select locationType).GetRandomUnsynced<LocationType>();
			if (suitableLocationType == null)
			{
				DebugConsole.AddWarning("No suitable location type found for ForceOutpostModule, skipping outpost generation!", null);
				return false;
			}
			OutpostGenerationParams.ModuleCount requiredFactionModuleCount = suitableOutpostParams.ModuleCounts.FirstOrDefault((OutpostGenerationParams.ModuleCount mc) => !mc.RequiredFaction.IsEmpty && moduleInfo.OutpostModuleInfo.ModuleFlags.Contains(mc.Identifier));
			Identifier requiredFactionId = (requiredFactionModuleCount != null) ? requiredFactionModuleCount.RequiredFaction : Identifier.Empty;
			if (requiredFactionId.IsEmpty)
			{
				outpostSub = OutpostGenerator.Generate(suitableOutpostParams, suitableLocationType, false, false);
				return outpostSub != null;
			}
			Location[] dummyLocations = GameSession.CreateDummyLocations("1337", suitableLocationType);
			Location dummyLocation = dummyLocations[0];
			FactionPrefab factionPrefab;
			if (FactionPrefab.Prefabs.TryGet(requiredFactionId, out factionPrefab))
			{
				if (factionPrefab.ControlledOutpostPercentage > factionPrefab.SecondaryControlledOutpostPercentage)
				{
					dummyLocation.Faction = new Faction(null, factionPrefab);
				}
				else
				{
					dummyLocation.SecondaryFaction = new Faction(null, factionPrefab);
				}
				outpostSub = OutpostGenerator.Generate(suitableOutpostParams, dummyLocation, false, false);
				return outpostSub != null;
			}
			return false;
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x00095C58 File Offset: 0x00093E58
		[NullableContext(2)]
		public void StartRound(LevelData levelData, bool mirrorLevel = false, SubmarineInfo startOutpost = null, SubmarineInfo endOutpost = null)
		{
			this.RoundDuration = 0f;
			AfflictionPrefab.LoadAllEffectsAndTreatmentSuitabilities();
			this.MirrorLevel = mirrorLevel;
			if (this.SubmarineInfo == null)
			{
				DebugConsole.ThrowError("Couldn't start game session, submarine not selected.", null, null, false, false);
				return;
			}
			if (this.SubmarineInfo.IsFileCorrupted)
			{
				DebugConsole.ThrowError("Couldn't start game session, submarine file corrupted.", null, null, false, false);
				return;
			}
			if (this.SubmarineInfo.SubmarineElement.Elements().Count<XElement>() == 0)
			{
				DebugConsole.ThrowError("Couldn't start game session, saved submarine is empty. The submarine file may be corrupted.", null, null, false, false);
				return;
			}
			Submarine.LockX = (Submarine.LockY = false);
			this.LevelData = levelData;
			Submarine.Unload();
			bool loadSubmarine = this.GameMode.Missions.None((Mission m) => !m.Prefab.LoadSubmarines);
			if (loadSubmarine)
			{
				Submarine outpostSub;
				if (this.TryGenerateStationAroundModule(this.ForceOutpostModule, out outpostSub))
				{
					this.Submarine = (Submarine.MainSub = (outpostSub ?? new Submarine(this.SubmarineInfo, true, null, null)));
				}
				else
				{
					this.Submarine = (Submarine.MainSub = new Submarine(this.SubmarineInfo, true, null, null));
				}
				using (IEnumerator<Submarine> enumerator = this.Submarine.GetConnectedSubs().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Submarine sub = enumerator.Current;
						sub.TeamID = CharacterTeamType.Team1;
						foreach (Item item in Item.ItemList)
						{
							if (item.Submarine == sub)
							{
								foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
								{
									wifiComponent.TeamID = sub.TeamID;
								}
							}
						}
					}
					goto IL_1D2;
				}
			}
			this.Submarine = (Submarine.MainSub = null);
			IL_1D2:
			this.GameMode.AddExtraMissions(this.LevelData);
			foreach (Mission mission in this.GameMode.Missions)
			{
				mission.SetLevel(levelData);
			}
			if (Submarine.MainSubs[1] == null && loadSubmarine)
			{
				SubmarineInfo submarineInfo;
				if (!(this.GameMode is PvPMode))
				{
					Mission mission3 = this.GameMode.Missions.FirstOrDefault((Mission m) => m.EnemySubmarineInfo != null);
					submarineInfo = ((mission3 != null) ? mission3.EnemySubmarineInfo : null);
				}
				else
				{
					submarineInfo = this.EnemySubmarineInfo;
				}
				SubmarineInfo enemySubmarineInfo = submarineInfo;
				if (enemySubmarineInfo != null)
				{
					Submarine.MainSubs[1] = new Submarine(enemySubmarineInfo, true, null, null);
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (serverSettings != null && serverSettings.LockAllDefaultWires && Submarine.MainSubs[0] != null)
			{
				List<Item> items = new List<Item>();
				items.AddRange(Submarine.MainSubs[0].GetItems(true));
				if (Submarine.MainSubs[1] != null)
				{
					items.AddRange(Submarine.MainSubs[1].GetItems(true));
				}
				foreach (Item item2 in items)
				{
					CircuitBox cb = item2.GetComponent<CircuitBox>();
					if (cb != null)
					{
						cb.TemporarilyLocked = true;
					}
					Wire wire = item2.GetComponent<Wire>();
					if (wire != null && !wire.NoAutoLock)
					{
						if (wire.Connections.Any((Connection c) => c != null))
						{
							wire.Locked = true;
						}
					}
				}
			}
			Level level = null;
			if (levelData != null)
			{
				level = Level.Generate(levelData, mirrorLevel, this.StartLocation, this.EndLocation, startOutpost, endOutpost);
			}
			this.InitializeLevel(level);
			Powered.Grids.Clear();
			this.casualties.Clear();
			GUIComponent guicomponent = GUIMessageBox.MessageBoxes.Find((GUIComponent mb) => mb.UserData is RoundSummary);
			RoundSummary existingRoundSummary = ((guicomponent != null) ? guicomponent.UserData : null) as RoundSummary;
			if (((existingRoundSummary != null) ? existingRoundSummary.ContinueButton : null) != null)
			{
				existingRoundSummary.ContinueButton.Visible = true;
			}
			CharacterHUD.ClearBossProgressBars();
			this.RoundSummary = new RoundSummary(this.GameMode, this.Missions, this.StartLocation, this.EndLocation);
			if (!(this.GameMode is TutorialMode) && !(this.GameMode is TestGameMode))
			{
				GUI.AddMessage("", Color.Transparent, new float?(3f), false, null);
				if (this.EndLocation != null && levelData != null)
				{
					GUI.AddMessage(levelData.Biome.DisplayName, Color.Lerp(Color.CadetBlue, Color.DarkRed, levelData.Difficulty / 100f), new float?(5f), false, null);
					GUI.AddMessage(TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("Destination"),
						this.EndLocation.DisplayName
					}), Color.CadetBlue, null, false, null);
					IEnumerable<Mission> missionsToShow = from m in this.missions
					where m.Prefab.ShowStartMessage
					select m;
					if (missionsToShow.Count<Mission>() > 1)
					{
						string joinedMissionNames = string.Join<LocalizedString>(", ", from m in this.missions
						where m.Prefab.ShowInMenus
						select m.Name);
						GUI.AddMessage(TextManager.AddPunctuation(':', new LocalizedString[]
						{
							TextManager.Get("Mission"),
							joinedMissionNames
						}), Color.CadetBlue, null, false, null);
					}
					else
					{
						Mission mission2 = missionsToShow.FirstOrDefault<Mission>();
						GUI.AddMessage(TextManager.AddPunctuation(':', new LocalizedString[]
						{
							TextManager.Get("Mission"),
							((mission2 != null) ? mission2.Name : null) ?? TextManager.Get("None")
						}), Color.CadetBlue, null, false, null);
					}
				}
				else
				{
					GUI.AddMessage(TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("Location"),
						this.StartLocation.DisplayName
					}), Color.CadetBlue, null, false, null);
				}
			}
			ReadyCheck.ReadyCheckCooldown = DateTime.MinValue;
			GUI.PreventPauseMenuToggle = false;
			HintManager.OnRoundStarted();
			this.EnableEventLogNotificationIcon(false);
			this.LogStartRoundStats();
			CampaignMode campaignMode = this.GameMode as CampaignMode;
			if (campaignMode != null && campaignMode.ItemsRelocatedToMainSub)
			{
				if (campaignMode.IsSinglePlayer)
				{
					new GUIMessageBox(string.Empty, TextManager.Get("itemrelocated"), null, null, GUIMessageBox.Type.Default);
				}
				campaignMode.ItemsRelocatedToMainSub = false;
			}
			EventManager eventManager = this.EventManager;
			if (eventManager != null)
			{
				EventLog eventLog = eventManager.EventLog;
				if (eventLog != null)
				{
					eventLog.Clear();
				}
			}
			if (campaignMode != null && !campaignMode.DivingSuitWarningShown && Level.Loaded != null && Level.Loaded.GetRealWorldDepth(0f) > 4000f)
			{
				CoroutineManager.Invoke(delegate
				{
					new GUIMessageBox(TextManager.Get("warning"), TextManager.Get("hint.upgradedivingsuits"), null, null, GUIMessageBox.Type.Default);
				}, 5f);
				campaignMode.DivingSuitWarningShown = true;
			}
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x000963FC File Offset: 0x000945FC
		[NullableContext(2)]
		private void InitializeLevel(Level level)
		{
			StatusEffect.StopAll();
			GameMain.LightManager.LosEnabled = ((GameMain.Client == null || GameMain.Client.CharacterInfo != null) && !GameMain.DevMode);
			if (GameMain.LightManager.LosEnabled)
			{
				GameMain.LightManager.LosAlpha = 1f;
			}
			if (GameMain.Client == null)
			{
				GameMain.LightManager.LosMode = GameSettings.CurrentConfig.Graphics.LosMode;
			}
			bool forceDocking = this.GameMode is TutorialMode;
			this.LevelData = ((level != null) ? level.LevelData : null);
			this.Level = level;
			GameSession.PlaceSubAtInitialPosition(this.Submarine, this.Level, true, forceDocking);
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.Info.IsOutpost || sub.Info.IsBeacon || sub.Info.IsWreck)
				{
					sub.DisableObstructedWayPoints();
				}
			}
			Entity.Spawner = new EntitySpawner();
			if (this.GameMode != null)
			{
				this.missions.Clear();
				this.missions.AddRange(this.GameMode.Missions);
				this.GameMode.Start();
				foreach (Mission mission in this.missions)
				{
					int prevEntityCount = Entity.GetEntities().Count;
					mission.Start(Level.Loaded);
					if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Entity.GetEntities().Count != prevEntityCount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Entity count has changed after starting a mission (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(mission.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(") as a client. ");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + "The clients should not instantiate entities themselves when starting the mission, but instead the server should inform the client of the spawned entities using Mission.ServerWriteInitial.", null, null, false, false);
					}
				}
				ObjectiveManager.ResetObjectives();
				EventManager eventManager = this.EventManager;
				if (eventManager != null)
				{
					eventManager.StartRound(Level.Loaded);
				}
				Level level2 = this.Level;
				AchievementManager.OnStartRound((level2 != null) ? level2.LevelData.Biome : null);
				this.GameMode.ShowStartMessage();
				if (GameMain.NetworkMember == null)
				{
					if (this.Level != null)
					{
						if (GameMain.GameSession.Missions.None((Mission m) => !m.Prefab.AllowOutpostNPCs))
						{
							this.Level.SpawnNPCs();
						}
						this.Level.SpawnCorpses();
						this.Level.PrepareBeaconStation();
					}
					else
					{
						foreach (Submarine sub2 in Submarine.Loaded)
						{
							bool flag;
							if (sub2 == null)
							{
								flag = (null != null);
							}
							else
							{
								SubmarineInfo info = sub2.Info;
								flag = (((info != null) ? info.OutpostGenerationParams : null) != null);
							}
							if (flag)
							{
								OutpostGenerator.SpawnNPCs(this.StartLocation, sub2);
							}
						}
					}
					CampaignMode campaign = this.Campaign;
					AutoItemPlacer.SpawnItems((campaign != null) ? new Identifier?(campaign.Settings.StartItemSet) : null);
				}
				MultiPlayerCampaign mpCampaign = this.GameMode as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					mpCampaign.UpgradeManager.ApplyUpgrades();
					mpCampaign.UpgradeManager.SanityCheckUpgrades();
				}
			}
			CreatureMetrics.RecentlyEncountered.Clear();
			Camera cam = GameMain.GameScreen.Cam;
			Character controlled = Character.Controlled;
			Vector2 position;
			if (controlled == null)
			{
				Submarine mainSub = Submarine.MainSub;
				position = ((mainSub != null) ? mainSub.WorldPosition : Submarine.Loaded.First<Submarine>().WorldPosition);
			}
			else
			{
				position = controlled.WorldPosition;
			}
			cam.Position = position;
			this.RoundDuration = 0f;
			GameMain.ResetFrameTime();
			this.IsRunning = true;
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x000967E0 File Offset: 0x000949E0
		[NullableContext(2)]
		public static void PlaceSubAtInitialPosition(Submarine sub, Level level, bool placeAtStart = true, bool forceDocking = false)
		{
			if (level == null || sub == null)
			{
				if (sub != null)
				{
					sub.SetPosition(Vector2.Zero, null, true);
				}
				return;
			}
			Submarine outpost = placeAtStart ? level.StartOutpost : level.EndOutpost;
			Vector2 originalSubPos = sub.WorldPosition;
			WayPoint spawnPoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.SpawnType.HasFlag(SpawnType.Submarine) && wp.Submarine == outpost);
			if (spawnPoint != null)
			{
				sub.SetPosition(spawnPoint.WorldPosition, null, true);
				sub.NeutralizeBallast();
				sub.EnableMaintainPosition();
			}
			else if (outpost != null)
			{
				Rectangle outpostBorders = outpost.GetDockedBorders(true);
				Rectangle subBorders = sub.GetDockedBorders(true);
				sub.SetPosition(outpost.WorldPosition - new Vector2(0f, (float)(outpostBorders.Height / 2 + subBorders.Height / 2)), null, true);
				float closestDistance = 0f;
				DockingPort myPort = null;
				DockingPort outPostPort = null;
				foreach (DockingPort port in DockingPort.List)
				{
					if (!port.IsHorizontal && !port.Docked)
					{
						if (port.Item.Submarine == outpost)
						{
							if (port.DockingTarget == null || (outPostPort != null && !outPostPort.MainDockingPort && port.MainDockingPort))
							{
								outPostPort = port;
							}
						}
						else if (port.Item.Submarine == sub && port.Item.WorldPosition.Y >= sub.WorldPosition.Y)
						{
							float dist = Vector2.DistanceSquared(port.Item.WorldPosition, outpost.WorldPosition);
							if ((myPort == null || dist < closestDistance || port.MainDockingPort) && (myPort == null || !myPort.MainDockingPort))
							{
								myPort = port;
								closestDistance = dist;
							}
						}
					}
				}
				if (myPort != null && outPostPort != null)
				{
					Vector2 portDiff = myPort.Item.WorldPosition - sub.WorldPosition;
					Vector2 spawnPos = outPostPort.Item.WorldPosition - portDiff - Vector2.UnitY * outPostPort.DockedDistance;
					bool startDocked = level.Type == LevelData.LevelType.Outpost || forceDocking;
					if (startDocked)
					{
						sub.SetPosition(spawnPos, null, true);
						myPort.Dock(outPostPort);
						myPort.Lock(true, false, true);
					}
					else
					{
						sub.SetPosition(spawnPos - Vector2.UnitY * 100f, null, true);
						sub.NeutralizeBallast();
						sub.EnableMaintainPosition();
					}
				}
				else
				{
					sub.NeutralizeBallast();
					sub.EnableMaintainPosition();
				}
			}
			else
			{
				sub.SetPosition(sub.FindSpawnPos(placeAtStart ? level.StartPosition : level.EndPosition, null, 0f, 0), null, true);
				sub.NeutralizeBallast();
				sub.EnableMaintainPosition();
			}
			foreach (Item item in sub.GetItems(true))
			{
				Steering steering = item.GetComponent<Steering>();
				if (steering != null && steering.MaintainPos)
				{
					steering.RefreshPosToMaintain();
				}
			}
			List<MapEntity> linkedSubs = MapEntity.MapEntityList.FindAll((MapEntity me) => me is LinkedSubmarine);
			foreach (MapEntity mapEntity in linkedSubs)
			{
				LinkedSubmarine ls = (LinkedSubmarine)mapEntity;
				if (ls.Sub != null && ls.Submarine == sub && ls.LoadSub && !ls.Sub.DockedTo.Contains(sub) && !sub.Info.LeftBehindDockingPortIDs.Contains(ls.OriginalLinkedToID) && ls.Sub.Info.SubmarineElement.Attribute("location") == null)
				{
					ls.SetPositionRelativeToMainSub();
				}
			}
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x00096C08 File Offset: 0x00094E08
		public void Update(float deltaTime)
		{
			this.RoundDuration += deltaTime;
			EventManager eventManager = this.EventManager;
			if (eventManager != null)
			{
				eventManager.Update(deltaTime);
			}
			GameMode gameMode = this.GameMode;
			if (gameMode != null)
			{
				gameMode.Update(deltaTime);
			}
			for (int i = this.missions.Count - 1; i >= 0; i--)
			{
				this.missions[i].Update(deltaTime);
			}
			this.UpdateProjSpecific(deltaTime);
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x00096C78 File Offset: 0x00094E78
		[NullableContext(2)]
		public Mission GetMission(int index)
		{
			if (index < 0 || index >= this.missions.Count)
			{
				return null;
			}
			return this.missions[index];
		}

		// Token: 0x06000FD2 RID: 4050 RVA: 0x00096C9A File Offset: 0x00094E9A
		public int GetMissionIndex(Mission mission)
		{
			return this.missions.IndexOf(mission);
		}

		// Token: 0x06000FD3 RID: 4051 RVA: 0x00096CA8 File Offset: 0x00094EA8
		public void EnforceMissionOrder(List<Identifier> missionIdentifiers)
		{
			List<Mission> sortedMissions = new List<Mission>();
			using (List<Identifier>.Enumerator enumerator = missionIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier missionId = enumerator.Current;
					Mission matchingMission = this.missions.Find((Mission m) => m.Prefab.Identifier == missionId);
					if (matchingMission != null)
					{
						sortedMissions.Add(matchingMission);
						this.missions.Remove(matchingMission);
					}
				}
			}
			this.missions.AddRange(sortedMissions);
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x00096D3C File Offset: 0x00094F3C
		private void UpdateProjSpecific(float deltaTime)
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			if (this.tabMenu == null)
			{
				if (PlayerInput.KeyHit(InputType.InfoTab) && !(GUI.KeyboardDispatcher.Subscriber is GUITextBox))
				{
					this.ToggleTabMenu();
				}
			}
			else
			{
				this.tabMenu.Update(deltaTime);
				if ((PlayerInput.KeyHit(InputType.InfoTab) || PlayerInput.KeyHit(Keys.Escape)) && !(GUI.KeyboardDispatcher.Subscriber is GUITextBox))
				{
					this.ToggleTabMenu();
				}
			}
			GameSession.UpdateTalentNotificationIndicator(this.talentPointNotification);
			if (GameMain.NetworkMember != null)
			{
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = netLobbyScreen.CharacterAppearanceCustomizationMenu;
					if (characterAppearanceCustomizationMenu != null)
					{
						characterAppearanceCustomizationMenu.Update();
					}
				}
				NetLobbyScreen netLobbyScreen2 = GameMain.NetLobbyScreen;
				if (((netLobbyScreen2 != null) ? netLobbyScreen2.JobSelectionFrame : null) != null && GameMain.NetLobbyScreen.JobSelectionFrame != null && PlayerInput.PrimaryMouseButtonDown() && !GUI.IsMouseOn(GameMain.NetLobbyScreen.JobSelectionFrame))
				{
					GameMain.NetLobbyScreen.JobList.Deselect();
					GameMain.NetLobbyScreen.JobSelectionFrame.Visible = false;
				}
			}
			HintManager.Update();
			ObjectiveManager.VideoPlayer.Update();
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x00096E44 File Offset: 0x00095044
		public static ImmutableHashSet<Character> GetSessionCrewCharacters(CharacterType type)
		{
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			if (crewManager == null)
			{
				return ImmutableHashSet<Character>.Empty;
			}
			HashSet<Character> characters = new HashSet<Character>();
			IEnumerable<Character> players = from c in crewManager.GetCharacters()
			where c.IsPlayer
			select c;
			IEnumerable<Character> bots = from c in crewManager.GetCharacters()
			where c.IsBot
			select c;
			if (type.HasFlag(CharacterType.Bot))
			{
				foreach (Character bot in bots)
				{
					characters.Add(bot);
				}
			}
			if (type.HasFlag(CharacterType.Player))
			{
				foreach (Character player in players)
				{
					characters.Add(player);
				}
			}
			return characters.ToImmutableHashSet<Character>();
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x00096F78 File Offset: 0x00095178
		public void EndRound(string endMessage, CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None, TraitorManager.TraitorResults? traitorResults = null, bool createRoundSummary = true)
		{
			this.RoundEnding = true;
			Powered.Grids.Clear();
			Powered.ChangedConnections.Clear();
			try
			{
				EventManager eventManager = this.EventManager;
				if (eventManager != null)
				{
					eventManager.TriggerOnEndRoundActions();
				}
				ImmutableHashSet<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
				int prevMoney = this.<EndRound>g__GetAmountOfMoney|139_0(crewCharacters);
				this.EndMissions(transitionType);
				foreach (Character character in crewCharacters)
				{
					character.CheckTalents(AbilityEffectType.OnRoundEnd);
				}
				if (GUI.PauseMenuOpen)
				{
					GUI.TogglePauseMenu();
				}
				if (GameSession.IsTabMenuOpen)
				{
					this.ToggleTabMenu();
				}
				DeathPrompt deathPrompt = this.DeathPrompt;
				if (deathPrompt != null)
				{
					deathPrompt.Close();
				}
				DeathPrompt.CloseBotPanel();
				GUI.PreventPauseMenuToggle = true;
				if (createRoundSummary && !(this.GameMode is TestGameMode) && Screen.Selected == GameMain.GameScreen && this.RoundSummary != null && transitionType != CampaignMode.TransitionType.End)
				{
					GUI.ClearMessages();
					GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent mb) => mb.UserData is RoundSummary);
					GUIFrame summaryFrame = this.RoundSummary.CreateSummaryFrame(this, endMessage, transitionType, traitorResults);
					GUIMessageBox.MessageBoxes.Add(summaryFrame);
					this.RoundSummary.ContinueButton.OnClicked = delegate(GUIButton _, object __)
					{
						GUIMessageBox.MessageBoxes.Remove(summaryFrame);
						return true;
					};
				}
				if (GameMain.NetLobbyScreen != null)
				{
					GameMain.NetLobbyScreen.OnRoundEnded();
				}
				TabMenu.OnRoundEnded();
				GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent mb) => mb.UserData as string == "ConversationAction" || ReadyCheck.IsReadyCheck(mb));
				ObjectiveManager.ResetUI();
				CharacterHUD.ClearBossProgressBars();
				AchievementManager.OnRoundEnded(this, false);
				GameMode gameMode = this.GameMode;
				if (gameMode != null)
				{
					gameMode.End(transitionType);
				}
				EventManager eventManager2 = this.EventManager;
				if (eventManager2 != null)
				{
					eventManager2.EndRound();
				}
				StatusEffect.StopAll();
				AfflictionPrefab.ClearAllEffects();
				this.IsRunning = false;
				GameAnalyticsManager.ProgressionStatus progressionStatus = this.CrewManager.GetCharacters().Any((Character c) => !c.IsDead) ? GameAnalyticsManager.ProgressionStatus.Complete : GameAnalyticsManager.ProgressionStatus.Fail;
				GameMode gameMode2 = this.GameMode;
				GameAnalyticsManager.AddProgressionEvent(progressionStatus, ((gameMode2 != null) ? gameMode2.Preset.Identifier.Value : null) ?? "none", (double)this.RoundDuration);
				string str = "EndRound:";
				GameMode gameMode3 = this.GameMode;
				string text;
				if (gameMode3 == null)
				{
					text = null;
				}
				else
				{
					GameModePreset preset = gameMode3.Preset;
					text = ((preset != null) ? preset.Identifier.Value : null);
				}
				string eventId = str + (text ?? "none") + ":";
				this.LogEndRoundStats(eventId, traitorResults);
				CampaignMode campaignMode = this.GameMode as CampaignMode;
				if (campaignMode != null)
				{
					GameAnalyticsManager.AddDesignEvent(eventId + "MoneyEarned", (double)(this.<EndRound>g__GetAmountOfMoney|139_0(crewCharacters) - prevMoney));
					campaignMode.TotalPlayTime += (double)this.RoundDuration;
				}
				HintManager.OnRoundEnded();
				this.missions.Clear();
			}
			catch (Exception e)
			{
				string errorMsg = "Unknown error while ending the round.";
				DebugConsole.ThrowError(errorMsg, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameSession.EndRound:UnknownError", GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + e.StackTrace);
			}
			finally
			{
				this.RoundEnding = false;
			}
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x000972EC File Offset: 0x000954EC
		public void EndMissions(CampaignMode.TransitionType transitionType)
		{
			ImmutableHashSet<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			foreach (Mission mission in this.missions)
			{
				mission.End(transitionType);
			}
			if (this.missions.Any<Mission>())
			{
				if (this.missions.Any((Mission m) => m.Completed))
				{
					foreach (Character character in crewCharacters)
					{
						character.CheckTalents(AbilityEffectType.OnAnyMissionCompleted);
					}
				}
				if (this.missions.All((Mission m) => m.Completed))
				{
					foreach (Character character2 in crewCharacters)
					{
						character2.CheckTalents(AbilityEffectType.OnAllMissionsCompleted);
					}
				}
			}
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x00097430 File Offset: 0x00095630
		public static PerkCollection GetPerks()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (serverSettings == null)
			{
				return PerkCollection.Empty;
			}
			ImmutableArray<DisembarkPerkPrefab>.Builder team1Builder = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			ImmutableArray<DisembarkPerkPrefab>.Builder team2Builder = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			foreach (Identifier coalitionPerk in serverSettings.SelectedCoalitionPerks)
			{
				DisembarkPerkPrefab disembarkPerk;
				if (DisembarkPerkPrefab.Prefabs.TryGet(coalitionPerk, out disembarkPerk))
				{
					team1Builder.Add(disembarkPerk);
				}
			}
			foreach (Identifier separatistsPerk in serverSettings.SelectedSeparatistsPerks)
			{
				DisembarkPerkPrefab disembarkPerk2;
				if (DisembarkPerkPrefab.Prefabs.TryGet(separatistsPerk, out disembarkPerk2))
				{
					team2Builder.Add(disembarkPerk2);
				}
			}
			return new PerkCollection(team1Builder.ToImmutable(), team2Builder.ToImmutable());
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x000974F0 File Offset: 0x000956F0
		public static bool ValidatedDisembarkPoints(GameModePreset preset, IEnumerable<Identifier> missionTypes)
		{
			GameSession.<>c__DisplayClass142_0 CS$<>8__locals1;
			CS$<>8__locals1.preset = preset;
			CS$<>8__locals1.missionTypes = missionTypes;
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings settings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (settings == null)
			{
				return false;
			}
			bool checkBothTeams = CS$<>8__locals1.preset == GameModePreset.PvP;
			PerkCollection perks = GameSession.GetPerks();
			int team1TotalCost = GameSession.<ValidatedDisembarkPoints>g__GetTotalCost|142_0(perks.Team1Perks, ref CS$<>8__locals1);
			if (team1TotalCost > settings.DisembarkPointAllowance)
			{
				return false;
			}
			if (checkBothTeams)
			{
				int team2TotalCost = GameSession.<ValidatedDisembarkPoints>g__GetTotalCost|142_0(perks.Team2Perks, ref CS$<>8__locals1);
				if (team2TotalCost > settings.DisembarkPointAllowance)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x00097574 File Offset: 0x00095774
		public static bool ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(GameModePreset preset, IEnumerable<Identifier> missionTypes)
		{
			if (preset == GameModePreset.Mission || preset == GameModePreset.PvP)
			{
				IEnumerable<Identifier> missionTypesToCheck = MissionMode.ValidateMissionTypes(missionTypes, (preset == GameModePreset.PvP) ? MissionPrefab.PvPMissionClasses : MissionPrefab.CoOpMissionClasses);
				using (IEnumerator<Identifier> enumerator = missionTypesToCheck.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Identifier missionType = enumerator.Current;
						IEnumerable<MissionPrefab> prefabs = MissionPrefab.Prefabs;
						Func<MissionPrefab, bool> predicate;
						Func<MissionPrefab, bool> <>9__0;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = delegate(MissionPrefab mp)
							{
								Identifier type = mp.Type;
								return type == missionType;
							});
						}
						foreach (MissionPrefab missionPrefab in prefabs.Where(predicate))
						{
							if (missionPrefab.LoadSubmarines)
							{
								return false;
							}
						}
					}
				}
				return true;
			}
			return true;
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x0009765C File Offset: 0x0009585C
		private void LogStartRoundStats()
		{
			if (!GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
			{
				return;
			}
			GameAnalyticsManager.ProgressionStatus progressionStatus = GameAnalyticsManager.ProgressionStatus.Start;
			GameMode gameMode = this.GameMode;
			string text;
			if (gameMode == null)
			{
				text = null;
			}
			else
			{
				GameModePreset preset = gameMode.Preset;
				text = ((preset != null) ? preset.Identifier.Value : null);
			}
			GameAnalyticsManager.AddProgressionEvent(progressionStatus, text ?? "none");
			string str = "StartRound:";
			GameMode gameMode2 = this.GameMode;
			string text2;
			if (gameMode2 == null)
			{
				text2 = null;
			}
			else
			{
				GameModePreset preset2 = gameMode2.Preset;
				text2 = ((preset2 != null) ? preset2.Identifier.Value : null);
			}
			string eventId = str + (text2 ?? "none") + ":";
			string str2 = eventId;
			string str3 = "Submarine:";
			Submarine mainSub = Submarine.MainSub;
			string text3;
			if (mainSub == null)
			{
				text3 = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				text3 = ((info != null) ? info.Name : null);
			}
			GameAnalyticsManager.AddDesignEvent(str2 + str3 + (text3 ?? "none"));
			string str4 = eventId;
			string str5 = "GameMode:";
			GameMode gameMode3 = this.GameMode;
			string text4;
			if (gameMode3 == null)
			{
				text4 = null;
			}
			else
			{
				GameModePreset preset3 = gameMode3.Preset;
				text4 = ((preset3 != null) ? preset3.Identifier.Value : null);
			}
			GameAnalyticsManager.AddDesignEvent(str4 + str5 + (text4 ?? "none"));
			string str6 = eventId;
			string str7 = "CrewSize:";
			CrewManager crewManager = this.CrewManager;
			int? num;
			if (crewManager == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
				num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
			}
			int? num2 = num;
			GameAnalyticsManager.AddDesignEvent(str6 + str7 + num2.GetValueOrDefault().ToString());
			foreach (Mission mission in this.missions)
			{
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"MissionType:",
					mission.Prefab.Type.ToString() ?? "none",
					":",
					mission.Prefab.Identifier.ToString()
				}));
			}
			if (Level.Loaded != null)
			{
				Identifier? identifier;
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					LevelGenerationParams generationParams = Level.Loaded.GenerationParams;
					identifier = ((generationParams != null) ? new Identifier?(generationParams.Identifier) : null);
				}
				else
				{
					Submarine startOutpost = Level.Loaded.StartOutpost;
					if (startOutpost == null)
					{
						identifier = null;
					}
					else
					{
						SubmarineInfo info2 = startOutpost.Info;
						if (info2 == null)
						{
							identifier = null;
						}
						else
						{
							OutpostGenerationParams outpostGenerationParams = info2.OutpostGenerationParams;
							identifier = ((outpostGenerationParams != null) ? new Identifier?(outpostGenerationParams.Identifier) : null);
						}
					}
				}
				Identifier levelId = identifier ?? "null".ToIdentifier();
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"LevelType:",
					Level.Loaded.Type.ToString(),
					":",
					levelId.ToString()
				}));
			}
			string str8 = eventId;
			string str9 = "Biome:";
			Level loaded = Level.Loaded;
			string text5;
			if (loaded == null)
			{
				text5 = null;
			}
			else
			{
				LevelData levelData = loaded.LevelData;
				if (levelData == null)
				{
					text5 = null;
				}
				else
				{
					Biome biome = levelData.Biome;
					text5 = ((biome != null) ? biome.Identifier.Value : null);
				}
			}
			GameAnalyticsManager.AddDesignEvent(str8 + str9 + (text5 ?? "none"));
			TutorialMode tutorialMode = this.GameMode as TutorialMode;
			if (tutorialMode != null)
			{
				GameAnalyticsManager.AddDesignEvent(eventId + tutorialMode.Tutorial.Identifier.ToString());
				if (GameMain.IsFirstLaunch)
				{
					GameAnalyticsManager.AddDesignEvent("FirstLaunch:" + eventId + tutorialMode.Tutorial.Identifier.ToString());
				}
			}
			GameAnalyticsManager.AddDesignEvent(eventId + "HintManager:" + (HintManager.Enabled ? "Enabled" : "Disabled"));
			CampaignMode campaignMode = this.GameMode as CampaignMode;
			if (campaignMode != null)
			{
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RadiationEnabled:" + campaignMode.Settings.RadiationEnabled.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:WorldHostility:" + campaignMode.Settings.WorldHostility.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShowHuskWarning:" + campaignMode.Settings.ShowHuskWarning.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:StartItemSet:" + campaignMode.Settings.StartItemSet.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:MaxMissionCount:" + campaignMode.Settings.MaxMissionCount.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RepairFailMultiplier:" + ((int)(campaignMode.Settings.RepairFailMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:FuelMultiplier:" + ((int)(campaignMode.Settings.FuelMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:MissionRewardMultiplier:" + ((int)(campaignMode.Settings.MissionRewardMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:CrewVitalityMultiplier:" + ((int)(campaignMode.Settings.CrewVitalityMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:NonCrewVitalityMultiplier:" + ((int)(campaignMode.Settings.NonCrewVitalityMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:OxygenMultiplier:" + ((int)(campaignMode.Settings.OxygenMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RepairFailMultiplier:" + ((int)(campaignMode.Settings.RepairFailMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShipyardPriceMultiplier:" + ((int)(campaignMode.Settings.ShipyardPriceMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShopPriceMultiplier:" + ((int)(campaignMode.Settings.ShopPriceMultiplier * 100f)).ToString());
				bool firstTimeInBiome = this.Map != null && !this.Map.Connections.Any((LocationConnection c) => c.Passed && c.Biome == this.LevelData.Biome);
				if (firstTimeInBiome)
				{
					string str10 = eventId;
					Level loaded2 = Level.Loaded;
					string text6;
					if (loaded2 == null)
					{
						text6 = null;
					}
					else
					{
						LevelData levelData2 = loaded2.LevelData;
						if (levelData2 == null)
						{
							text6 = null;
						}
						else
						{
							Biome biome2 = levelData2.Biome;
							text6 = ((biome2 != null) ? biome2.Identifier.Value : null);
						}
					}
					GameAnalyticsManager.AddDesignEvent(str10 + (text6 ?? "none") + "Discovered:Playtime", campaignMode.TotalPlayTime);
					string str11 = eventId;
					Level loaded3 = Level.Loaded;
					string text7;
					if (loaded3 == null)
					{
						text7 = null;
					}
					else
					{
						LevelData levelData3 = loaded3.LevelData;
						if (levelData3 == null)
						{
							text7 = null;
						}
						else
						{
							Biome biome3 = levelData3.Biome;
							text7 = ((biome3 != null) ? biome3.Identifier.Value : null);
						}
					}
					GameAnalyticsManager.AddDesignEvent(str11 + (text7 ?? "none") + "Discovered:PassedLevels", (double)campaignMode.TotalPassedLevels);
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
				if (serverSettings != null)
				{
					GameAnalyticsManager.AddDesignEvent("ServerSettings:RespawnMode:" + serverSettings.RespawnMode.ToString());
					GameAnalyticsManager.AddDesignEvent("ServerSettings:IronmanMode:" + serverSettings.IronmanModeActive.ToString());
					GameAnalyticsManager.AddDesignEvent("ServerSettings:AllowBotTakeoverOnPermadeath:" + serverSettings.AllowBotTakeoverOnPermadeath.ToString());
				}
			}
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x00097DA0 File Offset: 0x00095FA0
		public void LogEndRoundStats(string eventId, TraitorManager.TraitorResults? traitorResults = null)
		{
			if (!GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
			{
				return;
			}
			Submarine mainSub = Submarine.MainSub;
			bool? flag;
			if (mainSub == null)
			{
				flag = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				flag = ((info != null) ? new bool?(info.IsVanillaSubmarine()) : null);
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				string str = "Submarine:";
				Submarine mainSub2 = Submarine.MainSub;
				string text;
				if (mainSub2 == null)
				{
					text = null;
				}
				else
				{
					SubmarineInfo info2 = mainSub2.Info;
					text = ((info2 != null) ? info2.Name : null);
				}
				GameAnalyticsManager.AddDesignEvent(eventId + str + (text ?? "none"), (double)this.RoundDuration);
			}
			string str2 = "GameMode:";
			GameMode gameMode = this.GameMode;
			GameAnalyticsManager.AddDesignEvent(eventId + str2 + (((gameMode != null) ? gameMode.Name.Value : null) ?? "none"), (double)this.RoundDuration);
			string str3 = "CrewSize:";
			CrewManager crewManager = this.CrewManager;
			int? num;
			if (crewManager == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
				num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
			}
			int? num2 = num;
			GameAnalyticsManager.AddDesignEvent(eventId + str3 + num2.GetValueOrDefault().ToString(), (double)this.RoundDuration);
			foreach (Mission mission in this.missions)
			{
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"MissionType:",
					mission.Prefab.Type.ToString() ?? "none",
					":",
					mission.Prefab.Identifier.ToString(),
					":",
					mission.Completed ? "Completed" : "Failed"
				}), (double)this.RoundDuration);
			}
			if (!ContentPackageManager.ModsEnabled && Level.Loaded != null)
			{
				Identifier? identifier;
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					LevelGenerationParams generationParams = Level.Loaded.GenerationParams;
					identifier = ((generationParams != null) ? new Identifier?(generationParams.Identifier) : null);
				}
				else
				{
					Submarine startOutpost = Level.Loaded.StartOutpost;
					if (startOutpost == null)
					{
						identifier = null;
					}
					else
					{
						SubmarineInfo info3 = startOutpost.Info;
						if (info3 == null)
						{
							identifier = null;
						}
						else
						{
							OutpostGenerationParams outpostGenerationParams = info3.OutpostGenerationParams;
							identifier = ((outpostGenerationParams != null) ? new Identifier?(outpostGenerationParams.Identifier) : null);
						}
					}
				}
				Identifier levelId = identifier ?? "null".ToIdentifier();
				string str4 = "LevelType:";
				Level loaded = Level.Loaded;
				GameAnalyticsManager.AddDesignEvent(eventId + str4 + (((loaded != null) ? loaded.Type.ToString() : null) ?? ("none:" + levelId.ToString())), (double)this.RoundDuration);
				string str5 = "Biome:";
				Level loaded2 = Level.Loaded;
				string text2;
				if (loaded2 == null)
				{
					text2 = null;
				}
				else
				{
					LevelData levelData = loaded2.LevelData;
					if (levelData == null)
					{
						text2 = null;
					}
					else
					{
						Biome biome = levelData.Biome;
						text2 = ((biome != null) ? biome.Identifier.Value : null);
					}
				}
				GameAnalyticsManager.AddDesignEvent(eventId + str5 + (text2 ?? "none"), (double)this.RoundDuration);
			}
			if (traitorResults != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler.AppendLiteral("TraitorEvent:");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorResults.Value.TraitorEventIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(":");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(traitorResults.Value.ObjectiveSuccessful);
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("TraitorEvent:");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(traitorResults.Value.TraitorEventIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(":");
				defaultInterpolatedStringHandler2.AppendFormatted(traitorResults.Value.VotedCorrectTraitor ? "TraitorIdentifier" : "TraitorUnidentified");
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			TutorialMode tutorialMode = this.GameMode as TutorialMode;
			if (tutorialMode != null)
			{
				GameAnalyticsManager.AddDesignEvent(eventId + tutorialMode.Tutorial.Identifier.ToString());
				if (GameMain.IsFirstLaunch)
				{
					GameAnalyticsManager.AddDesignEvent("FirstLaunch:" + eventId + tutorialMode.Tutorial.Identifier.ToString());
				}
			}
			this.TimeSpentCleaning = (this.TimeSpentPainting = 0.0);
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x00098224 File Offset: 0x00096424
		public void KillCharacter(Character character)
		{
			if (this.CrewManager != null && this.CrewManager.GetCharacterInfos(false).Contains(character.Info))
			{
				this.casualties.Add(character);
			}
			CrewManager crewManager = this.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.KillCharacter(character, true);
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x00098271 File Offset: 0x00096471
		public void ReviveCharacter(Character character)
		{
			this.casualties.Remove(character);
			CrewManager crewManager = this.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.ReviveCharacter(character);
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x00098294 File Offset: 0x00096494
		public void UnlockRecipe(CharacterTeamType team, Identifier identifier, bool showNotifications)
		{
			if (this.unlockedRecipes.Add(new ValueTuple<CharacterTeamType, Identifier>(team, identifier)) && showNotifications)
			{
				foreach (Character character in GameSession.GetSessionCrewCharacters(CharacterType.Both))
				{
					if (character.TeamID == team)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler.AppendLiteral("entityname.");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
						LocalizedString recipeName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(identifier.Value, true);
						character.AddMessage(TextManager.GetWithVariable("recipeunlockednotification", "[name]", recipeName, FormatCapitals.No).Value, GUIStyle.Yellow, true, default(Identifier), null, 3f);
					}
				}
			}
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x00098388 File Offset: 0x00096588
		public bool HasUnlockedRecipe(Character character, Identifier itemIdentifier)
		{
			return character != null && this.unlockedRecipes.Contains(new ValueTuple<CharacterTeamType, Identifier>(character.TeamID, itemIdentifier));
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x000983A8 File Offset: 0x000965A8
		public static bool IsCompatibleWithEnabledContentPackages(IList<string> contentPackageNames, out LocalizedString errorMsg)
		{
			errorMsg = "";
			if (!contentPackageNames.Any<string>())
			{
				return true;
			}
			List<string> missingPackages = new List<string>();
			using (IEnumerator<string> enumerator = contentPackageNames.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string packageName = enumerator.Current;
					if (!ContentPackageManager.EnabledPackages.All.Any((ContentPackage cp) => cp.NameMatches(packageName)))
					{
						missingPackages.Add(packageName);
					}
				}
			}
			List<string> excessPackages = new List<string>();
			using (IEnumerator<ContentPackage> enumerator2 = ContentPackageManager.EnabledPackages.All.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ContentPackage cp = enumerator2.Current;
					if (cp.HasMultiplayerSyncedContent && !contentPackageNames.Any((string p) => cp.NameMatches(p)))
					{
						excessPackages.Add(cp.Name);
					}
				}
			}
			bool orderMismatch = false;
			if (missingPackages.Count == 0 && missingPackages.Count == 0)
			{
				ImmutableArray<ContentPackage> enabledPackages = (from cp in ContentPackageManager.EnabledPackages.All
				where cp.HasMultiplayerSyncedContent
				select cp).ToImmutableArray<ContentPackage>();
				int i = 0;
				while (i < contentPackageNames.Count && i < enabledPackages.Length)
				{
					if (!enabledPackages[i].NameMatches(contentPackageNames[i]))
					{
						orderMismatch = true;
						break;
					}
					i++;
				}
			}
			if (!orderMismatch && missingPackages.Count == 0 && excessPackages.Count == 0)
			{
				return true;
			}
			if (missingPackages.Count == 1)
			{
				errorMsg = TextManager.GetWithVariable("campaignmode.missingcontentpackage", "[missingcontentpackage]", missingPackages[0], FormatCapitals.No);
			}
			else if (missingPackages.Count > 1)
			{
				errorMsg = TextManager.GetWithVariable("campaignmode.missingcontentpackages", "[missingcontentpackages]", string.Join(", ", missingPackages), FormatCapitals.No);
			}
			if (excessPackages.Count == 1)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.incompatiblecontentpackage", "[incompatiblecontentpackage]", excessPackages[0], FormatCapitals.No);
			}
			else if (excessPackages.Count > 1)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.incompatiblecontentpackages", "[incompatiblecontentpackages]", string.Join(", ", excessPackages), FormatCapitals.No);
			}
			if (orderMismatch)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.contentpackageordermismatch", "[loadorder]", string.Join(", ", contentPackageNames), FormatCapitals.No);
			}
			return false;
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x00098684 File Offset: 0x00096884
		public void Save(string filePath, bool isSavingOnLoading)
		{
			CampaignMode campaign = this.GameMode as CampaignMode;
			if (campaign == null)
			{
				throw new NotSupportedException("GameSessions can only be saved when playing in a campaign mode.");
			}
			XDocument doc = new XDocument(new object[]
			{
				new XElement("Gamesession")
			});
			XElement root = doc.Root;
			if (root == null)
			{
				throw new NullReferenceException("Game session XML element is invalid: document is null.");
			}
			XElement rootElement = root;
			rootElement.Add(new XAttribute("savetime", SerializableDateTime.UtcNow.ToUnixTime()));
			XContainer xcontainer = rootElement;
			XName name = "currentlocation";
			Map map = this.Map;
			object obj;
			if (map == null)
			{
				obj = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				obj = ((currentLocation != null) ? currentLocation.NameIdentifier.Value : null);
			}
			xcontainer.Add(new XAttribute(name, obj ?? string.Empty));
			XContainer xcontainer2 = rootElement;
			XName name2 = "currentlocationnameformatindex";
			Map map2 = this.Map;
			int? num;
			if (map2 == null)
			{
				num = null;
			}
			else
			{
				Location currentLocation2 = map2.CurrentLocation;
				num = ((currentLocation2 != null) ? new int?(currentLocation2.NameFormatIndex) : null);
			}
			int? num2 = num;
			xcontainer2.Add(new XAttribute(name2, num2.GetValueOrDefault(-1)));
			XContainer xcontainer3 = rootElement;
			XName name3 = "locationtype";
			Map map3 = this.Map;
			Identifier? identifier;
			if (map3 == null)
			{
				identifier = null;
			}
			else
			{
				Location currentLocation3 = map3.CurrentLocation;
				if (currentLocation3 == null)
				{
					identifier = null;
				}
				else
				{
					LocationType type = currentLocation3.Type;
					identifier = ((type != null) ? new Identifier?(type.Identifier) : null);
				}
			}
			xcontainer3.Add(new XAttribute(name3, identifier ?? Identifier.Empty));
			XContainer xcontainer4 = rootElement;
			XName name4 = "nextleveltype";
			LevelData nextLevel = campaign.NextLevel;
			LevelData.LevelType levelType;
			if (nextLevel == null)
			{
				LevelData levelData = this.LevelData;
				levelType = ((levelData != null) ? levelData.Type : LevelData.LevelType.Outpost);
			}
			else
			{
				levelType = nextLevel.Type;
			}
			xcontainer4.Add(new XAttribute(name4, levelType));
			rootElement.Add(new XAttribute("ismultiplayer", campaign is MultiPlayerCampaign));
			this.LastSaveVersion = GameMain.Version;
			rootElement.Add(new XAttribute("version", GameMain.Version));
			Submarine submarine = this.Submarine;
			if (((submarine != null) ? submarine.Info : null) != null && !this.Submarine.Removed && this.Campaign != null)
			{
				bool hasNewPendingSub = this.Campaign.PendingSubmarineSwitch != null && this.Campaign.PendingSubmarineSwitch.MD5Hash.StringRepresentation != this.Submarine.Info.MD5Hash.StringRepresentation;
				if (hasNewPendingSub)
				{
					this.Campaign.SwitchSubs();
				}
			}
			rootElement.Add(new XAttribute("submarine", (this.SubmarineInfo == null) ? "" : this.SubmarineInfo.Name));
			if (this.OwnedSubmarines != null)
			{
				List<string> ownedSubmarineNames = new List<string>();
				XElement ownedSubsElement = new XElement("ownedsubmarines");
				rootElement.Add(ownedSubsElement);
				foreach (SubmarineInfo ownedSub in this.OwnedSubmarines)
				{
					ownedSubsElement.Add(new XElement("sub", new XAttribute("name", ownedSub.Name)));
				}
			}
			if (this.Map != null)
			{
				rootElement.Add(new XAttribute("mapseed", this.Map.Seed));
			}
			rootElement.Add(new XAttribute("selectedcontentpackagenames", string.Join("|", from cp in ContentPackageManager.EnabledPackages.All
			where cp.HasMultiplayerSyncedContent
			select cp.Name.Replace("|", "\\|"))));
			XElement permadeathsElement = new XElement("permadeaths");
			foreach (KeyValuePair<Option<AccountId>, int> kvp in this.permadeathsPerAccount)
			{
				AccountId accountId;
				if (kvp.Key.TryUnwrap(out accountId))
				{
					permadeathsElement.Add(new XElement("account", new object[]
					{
						new XAttribute("id", accountId.StringRepresentation),
						new XAttribute("permadeathcount", kvp.Value)
					}));
				}
			}
			rootElement.Add(permadeathsElement);
			XContainer xcontainer5 = rootElement;
			XName name5 = "respawnmode";
			NetworkMember networkMember = GameMain.NetworkMember;
			RespawnMode? respawnMode;
			if (networkMember == null)
			{
				respawnMode = null;
			}
			else
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				respawnMode = ((serverSettings != null) ? new RespawnMode?(serverSettings.RespawnMode) : null);
			}
			RespawnMode? respawnMode2 = respawnMode;
			xcontainer5.Add(new XAttribute(name5, respawnMode2.GetValueOrDefault()));
			((CampaignMode)this.GameMode).Save(doc.Root, isSavingOnLoading);
			doc.SaveSafe(filePath, SaveOptions.None, true, 0);
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x00098BF4 File Offset: 0x00096DF4
		[CompilerGenerated]
		internal static bool <StartRound>g__tryCreateFaction|128_5(Identifier factionIdentifier, Location[] locations, Action<Location, Faction> setter)
		{
			if (factionIdentifier.IsEmpty)
			{
				return false;
			}
			FactionPrefab prefab;
			if (!FactionPrefab.Prefabs.TryGet(factionIdentifier, out prefab))
			{
				return false;
			}
			if (locations.Length == 0)
			{
				return false;
			}
			Faction newFaction = new Faction(null, prefab);
			for (int i = 0; i < locations.Length; i++)
			{
				setter(locations[i], newFaction);
			}
			return true;
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x00098C44 File Offset: 0x00096E44
		[CompilerGenerated]
		internal static bool <TryGenerateStationAroundModule>g__IsSuitableLocationType|129_7(IEnumerable<Identifier> allowedLocationTypes, Identifier locationType)
		{
			return allowedLocationTypes.None(null) || allowedLocationTypes.Contains("Any".ToIdentifier()) || allowedLocationTypes.Contains(locationType);
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x00098C6C File Offset: 0x00096E6C
		[CompilerGenerated]
		private int <EndRound>g__GetAmountOfMoney|139_0(IEnumerable<Character> crew)
		{
			CampaignMode campaign = this.GameMode as CampaignMode;
			if (campaign == null)
			{
				return 0;
			}
			int result;
			if (GameMain.NetworkMember == null)
			{
				result = campaign.Bank.Balance;
			}
			else
			{
				result = crew.Sum((Character c) => c.Wallet.Balance) + campaign.Bank.Balance;
			}
			return result;
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x00098CD4 File Offset: 0x00096ED4
		[CompilerGenerated]
		internal static int <ValidatedDisembarkPoints>g__GetTotalCost|142_0([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<DisembarkPerkPrefab> perksToCheck, ref GameSession.<>c__DisplayClass142_0 A_1)
		{
			if ((A_1.preset == GameModePreset.Mission || A_1.preset == GameModePreset.PvP) && GameSession.ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(A_1.preset, A_1.missionTypes))
			{
				perksToCheck = (from p in perksToCheck
				where p.PerkBehaviors.All((PerkBase b) => b.CanApplyWithoutSubmarine())
				select p).ToImmutableArray<DisembarkPerkPrefab>();
			}
			return perksToCheck.Sum((DisembarkPerkPrefab p) => p.Cost);
		}

		// Token: 0x040007CC RID: 1996
		[Nullable(0)]
		private TabMenu tabMenu;

		// Token: 0x040007CD RID: 1997
		[Nullable(0)]
		private GUILayoutGroup topLeftButtonGroup;

		// Token: 0x040007CE RID: 1998
		[Nullable(0)]
		private GUIButton crewListButton;

		// Token: 0x040007CF RID: 1999
		[Nullable(0)]
		private GUIButton commandButton;

		// Token: 0x040007D0 RID: 2000
		[Nullable(0)]
		private GUIButton tabMenuButton;

		// Token: 0x040007D1 RID: 2001
		[Nullable(0)]
		private GUIImage talentPointNotification;

		// Token: 0x040007D2 RID: 2002
		[Nullable(0)]
		private GUIComponent deathChoiceInfoFrame;

		// Token: 0x040007D3 RID: 2003
		[Nullable(0)]
		private GUIComponent deathChoiceButtonContainer;

		// Token: 0x040007D4 RID: 2004
		[Nullable(0)]
		private GUITextBlock respawnInfoText;

		// Token: 0x040007D5 RID: 2005
		[Nullable(0)]
		private GUITickBox deathChoiceTickBox;

		// Token: 0x040007D6 RID: 2006
		[Nullable(0)]
		private GUIButton takeOverBotButton;

		// Token: 0x040007D7 RID: 2007
		[Nullable(0)]
		private GUIButton hrManagerButton;

		// Token: 0x040007D8 RID: 2008
		[Nullable(0)]
		public DeathPrompt DeathPrompt;

		// Token: 0x040007D9 RID: 2009
		[Nullable(0)]
		private GUIImage eventLogNotification;

		// Token: 0x040007DA RID: 2010
		private Point prevTopLeftButtonsResolution;

		// Token: 0x040007DC RID: 2012
		public readonly EventManager EventManager;

		// Token: 0x040007DD RID: 2013
		[Nullable(2)]
		public GameMode GameMode;

		// Token: 0x040007DE RID: 2014
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Location[] dummyLocations;

		// Token: 0x040007DF RID: 2015
		[Nullable(2)]
		public CrewManager CrewManager;

		// Token: 0x040007E1 RID: 2017
		public double TimeSpentCleaning;

		// Token: 0x040007E2 RID: 2018
		public double TimeSpentPainting;

		// Token: 0x040007E3 RID: 2019
		private readonly List<Mission> missions = new List<Mission>();

		// Token: 0x040007E4 RID: 2020
		private readonly HashSet<Character> casualties = new HashSet<Character>();

		// Token: 0x040007E5 RID: 2021
		[Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		private Dictionary<Option<AccountId>, int> permadeathsPerAccount = new Dictionary<Option<AccountId>, int>();

		// Token: 0x040007E6 RID: 2022
		public CharacterTeamType? WinningTeam;

		// Token: 0x040007EE RID: 2030
		[Nullable(2)]
		public SubmarineInfo ForceOutpostModule;

		// Token: 0x040007EF RID: 2031
		public List<SubmarineInfo> OwnedSubmarines = new List<SubmarineInfo>();

		// Token: 0x040007F1 RID: 2033
		[TupleElementNames(new string[]
		{
			"team",
			"identifier"
		})]
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly HashSet<ValueTuple<CharacterTeamType, Identifier>> unlockedRecipes = new HashSet<ValueTuple<CharacterTeamType, Identifier>>();

		// Token: 0x020008B3 RID: 2227
		[NullableContext(0)]
		public enum InfoFrameTab
		{
			// Token: 0x04003F16 RID: 16150
			Crew,
			// Token: 0x04003F17 RID: 16151
			Mission,
			// Token: 0x04003F18 RID: 16152
			MyCharacter,
			// Token: 0x04003F19 RID: 16153
			Traitor
		}
	}
}
