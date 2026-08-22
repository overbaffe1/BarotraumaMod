using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Barotrauma.PerkBehaviors;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000117 RID: 279
	internal class NetLobbyScreen : Screen
	{
		// Token: 0x17000A04 RID: 2564
		// (get) Token: 0x060025A1 RID: 9633 RVA: 0x001870C9 File Offset: 0x001852C9
		public GUITextBox ChatInput
		{
			get
			{
				return this.chatInput;
			}
		}

		// Token: 0x17000A05 RID: 2565
		// (get) Token: 0x060025A2 RID: 9634 RVA: 0x001870D1 File Offset: 0x001852D1
		// (set) Token: 0x060025A3 RID: 9635 RVA: 0x001870D9 File Offset: 0x001852D9
		public GUIFrame MissionTypeFrame { get; private set; }

		// Token: 0x17000A06 RID: 2566
		// (get) Token: 0x060025A4 RID: 9636 RVA: 0x001870E2 File Offset: 0x001852E2
		// (set) Token: 0x060025A5 RID: 9637 RVA: 0x001870EA File Offset: 0x001852EA
		public GUIFrame CampaignSetupFrame { get; private set; }

		// Token: 0x17000A07 RID: 2567
		// (get) Token: 0x060025A6 RID: 9638 RVA: 0x001870F3 File Offset: 0x001852F3
		// (set) Token: 0x060025A7 RID: 9639 RVA: 0x001870FB File Offset: 0x001852FB
		public GUIFrame CampaignFrame { get; private set; }

		// Token: 0x17000A08 RID: 2568
		// (get) Token: 0x060025A8 RID: 9640 RVA: 0x00187104 File Offset: 0x00185304
		// (set) Token: 0x060025A9 RID: 9641 RVA: 0x0018710C File Offset: 0x0018530C
		public GUIButton QuitCampaignButton { get; private set; }

		// Token: 0x17000A09 RID: 2569
		// (get) Token: 0x060025AA RID: 9642 RVA: 0x00187115 File Offset: 0x00185315
		// (set) Token: 0x060025AB RID: 9643 RVA: 0x0018711D File Offset: 0x0018531D
		public GUITextBox LevelSeedBox { get; private set; }

		// Token: 0x17000A0A RID: 2570
		// (get) Token: 0x060025AC RID: 9644 RVA: 0x00187126 File Offset: 0x00185326
		// (set) Token: 0x060025AD RID: 9645 RVA: 0x0018712E File Offset: 0x0018532E
		public GUIButton SettingsButton { get; private set; }

		// Token: 0x17000A0B RID: 2571
		// (get) Token: 0x060025AE RID: 9646 RVA: 0x00187137 File Offset: 0x00185337
		// (set) Token: 0x060025AF RID: 9647 RVA: 0x0018713F File Offset: 0x0018533F
		public GUIButton ServerMessageButton { get; private set; }

		// Token: 0x17000A0C RID: 2572
		// (get) Token: 0x060025B0 RID: 9648 RVA: 0x00187148 File Offset: 0x00185348
		// (set) Token: 0x060025B1 RID: 9649 RVA: 0x0018714F File Offset: 0x0018534F
		public static GUIButton JobInfoFrame { get; set; }

		// Token: 0x17000A0D RID: 2573
		// (get) Token: 0x060025B2 RID: 9650 RVA: 0x00187158 File Offset: 0x00185358
		public bool Spectating
		{
			get
			{
				GUITickBox guitickBox = this.spectateBox;
				return guitickBox != null && guitickBox.Selected && guitickBox.Visible;
			}
		}

		// Token: 0x17000A0E RID: 2574
		// (get) Token: 0x060025B3 RID: 9651 RVA: 0x00187180 File Offset: 0x00185380
		public bool AFKSelected
		{
			get
			{
				GUITickBox guitickBox = this.afkBox;
				return guitickBox != null && guitickBox.Selected && guitickBox.Visible;
			}
		}

		// Token: 0x17000A0F RID: 2575
		// (get) Token: 0x060025B4 RID: 9652 RVA: 0x001871A8 File Offset: 0x001853A8
		public bool PermadeathMode
		{
			get
			{
				GameClient client = GameMain.Client;
				if (client == null)
				{
					return false;
				}
				ServerSettings serverSettings = client.ServerSettings;
				return ((serverSettings != null) ? new RespawnMode?(serverSettings.RespawnMode) : null).GetValueOrDefault() == RespawnMode.Permadeath;
			}
		}

		// Token: 0x17000A10 RID: 2576
		// (get) Token: 0x060025B5 RID: 9653 RVA: 0x001871E9 File Offset: 0x001853E9
		public bool PermanentlyDead
		{
			get
			{
				CharacterInfo characterInfo = this.campaignCharacterInfo;
				return characterInfo != null && characterInfo.PermanentlyDead;
			}
		}

		// Token: 0x17000A11 RID: 2577
		// (get) Token: 0x060025B6 RID: 9654 RVA: 0x001871FC File Offset: 0x001853FC
		// (set) Token: 0x060025B7 RID: 9655 RVA: 0x00187204 File Offset: 0x00185404
		public GUIButton PlayerFrame { get; private set; }

		// Token: 0x17000A12 RID: 2578
		// (get) Token: 0x060025B8 RID: 9656 RVA: 0x0018720D File Offset: 0x0018540D
		// (set) Token: 0x060025B9 RID: 9657 RVA: 0x00187215 File Offset: 0x00185415
		public GUIButton SubVisibilityButton { get; private set; }

		// Token: 0x17000A13 RID: 2579
		// (get) Token: 0x060025BA RID: 9658 RVA: 0x0018721E File Offset: 0x0018541E
		// (set) Token: 0x060025BB RID: 9659 RVA: 0x00187226 File Offset: 0x00185426
		public CharacterInfo.AppearanceCustomizationMenu CharacterAppearanceCustomizationMenu { get; set; }

		// Token: 0x17000A14 RID: 2580
		// (get) Token: 0x060025BC RID: 9660 RVA: 0x0018722F File Offset: 0x0018542F
		// (set) Token: 0x060025BD RID: 9661 RVA: 0x00187237 File Offset: 0x00185437
		public GUIFrame JobSelectionFrame { get; private set; }

		// Token: 0x17000A15 RID: 2581
		// (get) Token: 0x060025BE RID: 9662 RVA: 0x00187240 File Offset: 0x00185440
		// (set) Token: 0x060025BF RID: 9663 RVA: 0x00187248 File Offset: 0x00185448
		public GUIFrame JobPreferenceContainer { get; private set; }

		// Token: 0x17000A16 RID: 2582
		// (get) Token: 0x060025C0 RID: 9664 RVA: 0x00187251 File Offset: 0x00185451
		// (set) Token: 0x060025C1 RID: 9665 RVA: 0x00187259 File Offset: 0x00185459
		public GUIListBox JobList { get; private set; }

		// Token: 0x17000A17 RID: 2583
		// (get) Token: 0x060025C2 RID: 9666 RVA: 0x00187262 File Offset: 0x00185462
		// (set) Token: 0x060025C3 RID: 9667 RVA: 0x0018726A File Offset: 0x0018546A
		public bool CampaignCharacterDiscarded { get; set; }

		// Token: 0x17000A18 RID: 2584
		// (get) Token: 0x060025C4 RID: 9668 RVA: 0x00187273 File Offset: 0x00185473
		// (set) Token: 0x060025C5 RID: 9669 RVA: 0x0018727B File Offset: 0x0018547B
		public GUIComponent FileTransferFrame { get; private set; }

		// Token: 0x17000A19 RID: 2585
		// (get) Token: 0x060025C6 RID: 9670 RVA: 0x00187284 File Offset: 0x00185484
		// (set) Token: 0x060025C7 RID: 9671 RVA: 0x0018728C File Offset: 0x0018548C
		public GUITextBlock FileTransferTitle { get; private set; }

		// Token: 0x17000A1A RID: 2586
		// (get) Token: 0x060025C8 RID: 9672 RVA: 0x00187295 File Offset: 0x00185495
		// (set) Token: 0x060025C9 RID: 9673 RVA: 0x0018729D File Offset: 0x0018549D
		public GUIProgressBar FileTransferProgressBar { get; private set; }

		// Token: 0x17000A1B RID: 2587
		// (get) Token: 0x060025CA RID: 9674 RVA: 0x001872A6 File Offset: 0x001854A6
		// (set) Token: 0x060025CB RID: 9675 RVA: 0x001872AE File Offset: 0x001854AE
		public GUITextBlock FileTransferProgressText { get; private set; }

		// Token: 0x17000A1C RID: 2588
		// (get) Token: 0x060025CC RID: 9676 RVA: 0x001872B7 File Offset: 0x001854B7
		// (set) Token: 0x060025CD RID: 9677 RVA: 0x001872BF File Offset: 0x001854BF
		public GUITickBox Favorite { get; private set; }

		// Token: 0x17000A1D RID: 2589
		// (get) Token: 0x060025CE RID: 9678 RVA: 0x001872C8 File Offset: 0x001854C8
		// (set) Token: 0x060025CF RID: 9679 RVA: 0x001872D0 File Offset: 0x001854D0
		public GUILayoutGroup LogButtons { get; private set; }

		// Token: 0x17000A1E RID: 2590
		// (get) Token: 0x060025D0 RID: 9680 RVA: 0x001872D9 File Offset: 0x001854D9
		// (set) Token: 0x060025D1 RID: 9681 RVA: 0x001872E1 File Offset: 0x001854E1
		public GUIListBox SubList { get; private set; }

		// Token: 0x17000A1F RID: 2591
		// (get) Token: 0x060025D2 RID: 9682 RVA: 0x001872EA File Offset: 0x001854EA
		// (set) Token: 0x060025D3 RID: 9683 RVA: 0x001872F2 File Offset: 0x001854F2
		public GUIDropDown ShuttleList { get; private set; }

		// Token: 0x17000A20 RID: 2592
		// (get) Token: 0x060025D4 RID: 9684 RVA: 0x001872FB File Offset: 0x001854FB
		// (set) Token: 0x060025D5 RID: 9685 RVA: 0x00187303 File Offset: 0x00185503
		public GUIListBox ModeList { get; private set; }

		// Token: 0x17000A21 RID: 2593
		// (get) Token: 0x060025D6 RID: 9686 RVA: 0x0018730C File Offset: 0x0018550C
		// (set) Token: 0x060025D7 RID: 9687 RVA: 0x00187314 File Offset: 0x00185514
		public int SelectedModeIndex
		{
			get
			{
				return this.selectedModeIndex;
			}
			set
			{
				if (this.HighlightedModeIndex == this.selectedModeIndex)
				{
					this.ModeList.Select(value, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
				}
				this.selectedModeIndex = value;
			}
		}

		// Token: 0x17000A22 RID: 2594
		// (get) Token: 0x060025D8 RID: 9688 RVA: 0x0018733B File Offset: 0x0018553B
		// (set) Token: 0x060025D9 RID: 9689 RVA: 0x00187348 File Offset: 0x00185548
		public int HighlightedModeIndex
		{
			get
			{
				return this.ModeList.SelectedIndex;
			}
			set
			{
				this.ModeList.Select(value, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
			}
		}

		// Token: 0x060025DA RID: 9690 RVA: 0x0018735C File Offset: 0x0018555C
		public IReadOnlyList<SubmarineInfo> GetSubList()
		{
			GameClient client = GameMain.Client;
			IReadOnlyList<SubmarineInfo> readOnlyList = (client != null) ? client.ServerSubmarines : null;
			return readOnlyList ?? Array.Empty<SubmarineInfo>();
		}

		// Token: 0x17000A23 RID: 2595
		// (get) Token: 0x060025DB RID: 9691 RVA: 0x00187385 File Offset: 0x00185585
		// (set) Token: 0x060025DC RID: 9692 RVA: 0x0018738D File Offset: 0x0018558D
		public GUITextBox CharacterNameBox { get; private set; }

		// Token: 0x17000A24 RID: 2596
		// (get) Token: 0x060025DD RID: 9693 RVA: 0x00187396 File Offset: 0x00185596
		// (set) Token: 0x060025DE RID: 9694 RVA: 0x0018739E File Offset: 0x0018559E
		public GUIListBox TeamPreferenceListBox { get; private set; }

		// Token: 0x17000A25 RID: 2597
		// (get) Token: 0x060025DF RID: 9695 RVA: 0x001873A7 File Offset: 0x001855A7
		private CharacterTeamType TeamPreference
		{
			get
			{
				if (this.SelectedMode != GameModePreset.PvP)
				{
					return CharacterTeamType.Team1;
				}
				return MultiplayerPreferences.Instance.TeamPreference;
			}
		}

		// Token: 0x17000A26 RID: 2598
		// (get) Token: 0x060025E0 RID: 9696 RVA: 0x001873C2 File Offset: 0x001855C2
		// (set) Token: 0x060025E1 RID: 9697 RVA: 0x001873CA File Offset: 0x001855CA
		public GUIButton StartButton { get; private set; }

		// Token: 0x17000A27 RID: 2599
		// (get) Token: 0x060025E2 RID: 9698 RVA: 0x001873D3 File Offset: 0x001855D3
		// (set) Token: 0x060025E3 RID: 9699 RVA: 0x001873DB File Offset: 0x001855DB
		public GUIButton EndButton { get; private set; }

		// Token: 0x17000A28 RID: 2600
		// (get) Token: 0x060025E4 RID: 9700 RVA: 0x001873E4 File Offset: 0x001855E4
		// (set) Token: 0x060025E5 RID: 9701 RVA: 0x001873EC File Offset: 0x001855EC
		public GUITickBox ReadyToStartBox { get; private set; }

		// Token: 0x17000A29 RID: 2601
		// (get) Token: 0x060025E6 RID: 9702 RVA: 0x001873F5 File Offset: 0x001855F5
		public SubmarineInfo SelectedShuttle
		{
			get
			{
				return this.ShuttleList.SelectedData as SubmarineInfo;
			}
		}

		// Token: 0x17000A2A RID: 2602
		// (get) Token: 0x060025E7 RID: 9703 RVA: 0x00187407 File Offset: 0x00185607
		// (set) Token: 0x060025E8 RID: 9704 RVA: 0x00187414 File Offset: 0x00185614
		public bool UsingShuttle
		{
			get
			{
				return this.shuttleTickBox.Selected;
			}
			set
			{
				this.shuttleTickBox.Selected = value;
			}
		}

		// Token: 0x17000A2B RID: 2603
		// (get) Token: 0x060025E9 RID: 9705 RVA: 0x00187422 File Offset: 0x00185622
		public GameModePreset SelectedMode
		{
			get
			{
				return this.ModeList.SelectedData as GameModePreset;
			}
		}

		// Token: 0x17000A2C RID: 2604
		// (get) Token: 0x060025EA RID: 9706 RVA: 0x00187434 File Offset: 0x00185634
		// (set) Token: 0x060025EB RID: 9707 RVA: 0x00187490 File Offset: 0x00185690
		public IEnumerable<Identifier> MissionTypes
		{
			get
			{
				return from t in this.missionTypeTickBoxes
				where t.Selected
				select (Identifier)t.UserData;
			}
			set
			{
				bool changed = false;
				foreach (GUITickBox missionTypeTickBox in this.missionTypeTickBoxes)
				{
					bool prevSelected = missionTypeTickBox.Selected;
					missionTypeTickBox.Selected = value.Contains((Identifier)missionTypeTickBox.UserData);
					if (prevSelected != missionTypeTickBox.Selected)
					{
						changed = true;
					}
				}
				if (changed)
				{
					this.RefreshOutpostDropdown();
				}
			}
		}

		// Token: 0x17000A2D RID: 2605
		// (get) Token: 0x060025EC RID: 9708 RVA: 0x001874EC File Offset: 0x001856EC
		public List<JobVariant> JobPreferences
		{
			get
			{
				GUIListBox jobList = this.JobList;
				if (((jobList != null) ? jobList.Content : null) == null)
				{
					return new List<JobVariant>();
				}
				List<JobVariant> jobPreferences = new List<JobVariant>();
				foreach (GUIComponent child in this.JobList.Content.Children)
				{
					JobVariant jobPrefab = child.UserData as JobVariant;
					if (jobPrefab != null)
					{
						jobPreferences.Add(jobPrefab);
					}
				}
				return jobPreferences;
			}
		}

		// Token: 0x17000A2E RID: 2606
		// (get) Token: 0x060025ED RID: 9709 RVA: 0x00187574 File Offset: 0x00185774
		// (set) Token: 0x060025EE RID: 9710 RVA: 0x0018757C File Offset: 0x0018577C
		public string LevelSeed
		{
			get
			{
				return this.levelSeed;
			}
			set
			{
				if (this.levelSeed == value)
				{
					return;
				}
				this.levelSeed = value;
				int intSeed = ToolBox.StringToInt(this.levelSeed);
				LocationType locationType = LocationType.Random(new MTRandom(intSeed), null, null, false, (LocationType lt) => lt.UsePortraitInRandomLoadingScreens);
				this.backgroundSprite = ((locationType != null) ? locationType.GetPortrait(intSeed) : null);
				this.LevelSeedBox.Text = this.levelSeed;
			}
		}

		// Token: 0x17000A2F RID: 2607
		// (get) Token: 0x060025EF RID: 9711 RVA: 0x0018760C File Offset: 0x0018580C
		private static int PanelBorderSize
		{
			get
			{
				return GUI.IntScale(20f);
			}
		}

		// Token: 0x060025F0 RID: 9712 RVA: 0x00187618 File Offset: 0x00185818
		private static Point GetSizeWithoutBorder(GUIComponent parent)
		{
			return new Point(parent.Rect.Width - NetLobbyScreen.PanelBorderSize * 2, parent.Rect.Height - NetLobbyScreen.PanelBorderSize * 2);
		}

		// Token: 0x060025F1 RID: 9713 RVA: 0x00187648 File Offset: 0x00185848
		public NetLobbyScreen()
		{
			GUILayoutGroup contentArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), this.Frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			GUILayoutGroup horizontalLayout = new GUILayoutGroup(new RectTransform(Vector2.One, contentArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			GUIFrame mainPanel = new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup mainPanelLayout = new GUILayoutGroup(new RectTransform(new Point(mainPanel.Rect.Width, mainPanel.Rect.Height - NetLobbyScreen.PanelBorderSize), mainPanel.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup serverInfoHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), mainPanelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.025f
			};
			this.CreateServerInfoContents(serverInfoHolder);
			GUILayoutGroup mainPanelTopLayout = new GUILayoutGroup(new RectTransform(new Point(mainPanel.Rect.Width - NetLobbyScreen.PanelBorderSize * 2, mainPanel.Rect.Height / 2), mainPanelLayout.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			GUILayoutGroup mainPanelBottomLayout = new GUILayoutGroup(new RectTransform(new Point(mainPanel.Rect.Width - NetLobbyScreen.PanelBorderSize * 2, mainPanel.Rect.Height / 2), mainPanelLayout.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			this.CreateGameModeDropdown(mainPanelTopLayout);
			this.CreateSubmarineListPanel(mainPanelTopLayout);
			this.CreateSubmarineInfoPanel(mainPanelTopLayout);
			this.CreateGameModePanel(mainPanelBottomLayout);
			this.CreateGameModeSettingsPanel(mainPanelBottomLayout);
			this.CreateGeneralSettingsPanel(mainPanelBottomLayout);
			mainPanelBottomLayout.Recalculate();
			foreach (GUIComponent child in mainPanelBottomLayout.GetAllChildren<GUIComponent>())
			{
				if (!this.traitorDangerGroup.Children.Contains(child))
				{
					child.DisabledColor = new Color(child.Color, (float)child.Color.A / 255f * 0.8f);
					GUITextBlock textBlock = child as GUITextBlock;
					if (textBlock != null)
					{
						textBlock.DisabledTextColor = new Color(textBlock.TextColor, (float)textBlock.TextColor.A / 255f * 0.8f);
					}
				}
			}
			GUIFrame sidePanel = new GUIFrame(new RectTransform(new Vector2(0.3f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup sidePanelLayout = new GUILayoutGroup(new RectTransform(NetLobbyScreen.GetSizeWithoutBorder(sidePanel), sidePanel.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			this.CreateSidePanelContents(sidePanelLayout);
			GUILayoutGroup bottomBar = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), contentArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft)
			{
				Stretch = true,
				IsHorizontal = true,
				RelativeSpacing = 0.005f
			};
			this.CreateBottomPanelContents(bottomBar);
		}

		// Token: 0x060025F2 RID: 9714 RVA: 0x00187B80 File Offset: 0x00185D80
		private void AssignComponentToServerSetting(GUIComponent component, string settingName)
		{
			this.settingAssignedComponents[component] = settingName;
		}

		// Token: 0x060025F3 RID: 9715 RVA: 0x00187B8F File Offset: 0x00185D8F
		public void AssignComponentsToServerSettings()
		{
			this.settingAssignedComponents.ForEach(delegate(KeyValuePair<GUIComponent, string> kvp)
			{
				GameMain.Client.ServerSettings.AssignGUIComponent(kvp.Value, kvp.Key);
			});
		}

		// Token: 0x060025F4 RID: 9716 RVA: 0x00187BBC File Offset: 0x00185DBC
		private void CreateServerInfoContents(GUIComponent parent)
		{
			GUIFrame serverInfoFrame = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(Vector2.One, serverInfoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawServerBanner), null);
			guicustomComponent.HideElementsOutsideFrame = true;
			guicustomComponent.IgnoreLayoutGroups = true;
			GUIFrame serverInfoContent = new GUIFrame(new RectTransform(new Vector2(0.98f, 0.9f), serverInfoFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup serverLabelContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.05f), serverInfoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			RectTransform rectT = new RectTransform(new Vector2(0.3f, 1f), serverLabelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont font = GUIStyle.SmallFont;
			this.playstyleText = new GUITextBlock(rectT, text, new Color?(Color.White), font, Alignment.Center, false, "GUISlopedHeader", null);
			RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), serverLabelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = "";
			font = GUIStyle.SmallFont;
			this.publicOrPrivateText = new GUITextBlock(rectT2, text2, new Color?(Color.White), font, Alignment.Center, false, "GUISlopedHeader", null);
			RectTransform rectTransform = new RectTransform(new Vector2(0.2f, 0.3f), serverInfoContent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.AbsoluteOffset = new Point(GUI.IntScale(3f));
			RichString text3 = string.Empty;
			font = GUIStyle.LargeFont;
			GUITextBlock serverNameShadow = new GUITextBlock(rectTransform, text3, new Color?(Color.Black), font, Alignment.Left, false, "", null)
			{
				IgnoreLayoutGroups = true
			};
			RectTransform rectT3 = new RectTransform(new Vector2(0.2f, 0.3f), serverInfoContent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = string.Empty;
			font = GUIStyle.LargeFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT3, text4, new Color?(GUIStyle.TextColorBright), font, Alignment.Left, false, "", null);
			guitextBlock.IgnoreLayoutGroups = true;
			guitextBlock.TextGetter = (serverNameShadow.TextGetter = delegate()
			{
				GameClient client = GameMain.Client;
				return (client != null) ? client.ServerName : null;
			});
			this.ServerMessageButton = new GUIButton(new RectTransform(new Vector2(0.2f, 0.15f), serverInfoContent.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("workshopitemdescription"), Alignment.Center, "GUIButtonSmall", null)
			{
				IgnoreLayoutGroups = true,
				OnClicked = delegate(GUIButton bt, object userdata)
				{
					GameClient client = GameMain.Client;
					ServerSettings serverSettings = (client != null) ? client.ServerSettings : null;
					if (serverSettings != null)
					{
						this.CreateServerMessagePopup(serverSettings.ServerName, serverSettings.ServerMessageText);
					}
					return true;
				}
			};
			this.playStyleIconContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.4f), serverInfoContent.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), true, Anchor.BottomRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			GUILayoutGroup topRightContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.5f), serverInfoContent.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.TopRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f),
				CanBeFocused = true
			};
			this.SettingsButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), topRightContainer.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsButton"), Alignment.Center, "GUIButtonFreeScale", null);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One, topRightContainer.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.BothHeight), "", null, "GUIServerListFavoriteTickBox");
			guitickBox.Selected = false;
			guitickBox.ToolTip = TextManager.Get("addtofavorites");
			guitickBox.OnSelected = delegate(GUITickBox tickbox)
			{
				if (GameMain.Client == null)
				{
					return true;
				}
				ServerInfo info = GameMain.Client.CreateServerInfoFromSettings();
				if (tickbox.Selected)
				{
					GameMain.ServerListScreen.AddToFavoriteServers(info);
				}
				else
				{
					GameMain.ServerListScreen.RemoveFromFavoriteServers(info);
				}
				tickbox.ToolTip = TextManager.Get(tickbox.Selected ? "removefromfavorites" : "addtofavorites");
				return true;
			};
			this.Favorite = guitickBox;
		}

		// Token: 0x060025F5 RID: 9717 RVA: 0x00188130 File Offset: 0x00186330
		private void CreateServerMessagePopup(string serverName, string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}
			LocalizedString headerText = serverName;
			LocalizedString text2 = string.Empty;
			Point? point = new Point?(new Point(GUI.IntScale(650f), GUI.IntScale(650f)));
			GUIMessageBox popup = new GUIMessageBox(headerText, text2, null, point, GUIMessageBox.Type.Default);
			popup.Header.Font = GUIStyle.LargeFont;
			popup.Header.RectTransform.MinSize = new Point(0, (int)popup.Header.TextSize.Y);
			Vector2 relativeSize = new Vector2(1f, 0.7f);
			RectTransform rectTransform = popup.Content.RectTransform;
			Anchor anchor = Anchor.TopLeft;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			GUIListBox textListBox = new GUIListBox(new RectTransform(relativeSize, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), false, null, "", true, false);
			Vector2 relativeSize2 = new Vector2(1f, 0f);
			RectTransform rectTransform2 = textListBox.Content.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			GUITextBlock text = new GUITextBlock(new RectTransform(relativeSize2, rectTransform2, anchor2, pivot2, minSize2, point, ScaleBasis.Normal), message, null, null, Alignment.Left, true, "", null)
			{
				CanBeFocused = false
			};
			text.RectTransform.MinSize = new Point(0, (int)text.TextSize.Y);
		}

		// Token: 0x060025F6 RID: 9718 RVA: 0x001882A0 File Offset: 0x001864A0
		public void RefreshPlaystyleIcons()
		{
			GUIComponent guicomponent = this.playStyleIconContainer;
			if (guicomponent != null)
			{
				guicomponent.ClearChildren();
			}
			GameClient client = GameMain.Client;
			NetworkConnection networkConnection;
			if (client == null)
			{
				networkConnection = null;
			}
			else
			{
				ClientPeer clientPeer = client.ClientPeer;
				networkConnection = ((clientPeer != null) ? clientPeer.ServerConnection : null);
			}
			NetworkConnection serverConnection = networkConnection;
			if (serverConnection == null || serverConnection.Endpoint == null)
			{
				return;
			}
			ServerInfo serverInfo = ServerInfo.FromServerEndpoints(serverConnection.Endpoint.ToEnumerable<Endpoint>().ToImmutableArray<Endpoint>(), GameMain.Client.ServerSettings);
			IEnumerable<Identifier> playStyleTags = serverInfo.GetPlayStyleTags();
			foreach (Identifier tag in playStyleTags)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("PlayStyleIcon.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(tag);
				GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
				Sprite playStyleIcon = (componentStyle != null) ? componentStyle.GetSprite(GUIComponent.ComponentState.None) : null;
				if (playStyleIcon != null)
				{
					GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, this.playStyleIconContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), playStyleIcon, true, null);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("servertagdescription.");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(tag);
					guiimage.ToolTip = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
					guiimage.Color = Color.White;
				}
			}
		}

		// Token: 0x060025F7 RID: 9719 RVA: 0x00188418 File Offset: 0x00186618
		private void CreateGameModeDropdown(GUIComponent parent)
		{
			GUILayoutGroup gameModeHolder = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			GUITextBlock modeLabel = NetLobbyScreen.CreateSubHeader("GameMode", gameModeHolder, null);
			GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), modeLabel.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Votes"), null, null, Alignment.CenterRight, false, "", null);
			guitextBlock.UserData = "modevotes";
			guitextBlock.Visible = false;
			this.ModeList = new GUIListBox(new RectTransform(Vector2.One, gameModeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = new GUIListBox.OnSelectedHandler(this.VotableClicked)
			};
			foreach (GameModePreset mode in GameModePreset.List)
			{
				NetLobbyScreen.<>c__DisplayClass234_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass234_0();
				if (!mode.IsSinglePlayer)
				{
					CS$<>8__locals1.modeFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), this.ModeList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
					{
						UserData = mode
					};
					CS$<>8__locals1.modeContent = new GUILayoutGroup(new RectTransform(new Vector2(0.76f, 0.9f), CS$<>8__locals1.modeFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
					{
						AbsoluteSpacing = GUI.IntScale(5f),
						Stretch = true
					};
					NetLobbyScreen.<>c__DisplayClass234_0 CS$<>8__locals2 = CS$<>8__locals1;
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.modeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text = mode.Name;
					GUIFont font = GUIStyle.SubHeadingFont;
					CS$<>8__locals2.modeTitle = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
					CS$<>8__locals1.modeTitle.RectTransform.NonScaledSize = new Point(int.MaxValue, (int)CS$<>8__locals1.modeTitle.TextSize.Y);
					CS$<>8__locals1.modeTitle.RectTransform.IsFixedSize = true;
					NetLobbyScreen.<>c__DisplayClass234_0 CS$<>8__locals3 = CS$<>8__locals1;
					RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.modeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = mode.Description;
					font = GUIStyle.SmallFont;
					CS$<>8__locals3.modeDescription = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, true, "", null);
					CS$<>8__locals1.modeDescription.Padding = new Vector4(CS$<>8__locals1.modeDescription.Padding.X, CS$<>8__locals1.modeDescription.Padding.Y, (float)GUI.IntScale(30f), CS$<>8__locals1.modeDescription.Padding.W);
					CS$<>8__locals1.modeTitle.HoverColor = (CS$<>8__locals1.modeDescription.HoverColor = (CS$<>8__locals1.modeTitle.SelectedColor = (CS$<>8__locals1.modeDescription.SelectedColor = Color.Transparent)));
					CS$<>8__locals1.modeTitle.HoverTextColor = (CS$<>8__locals1.modeDescription.HoverTextColor = CS$<>8__locals1.modeTitle.TextColor);
					CS$<>8__locals1.modeTitle.TextColor = (CS$<>8__locals1.modeDescription.TextColor = CS$<>8__locals1.modeTitle.TextColor * 0.5f);
					CS$<>8__locals1.modeFrame.OnAddedToGUIUpdateList = delegate(GUIComponent c)
					{
						CS$<>8__locals1.modeTitle.State = (CS$<>8__locals1.modeDescription.State = c.State);
					};
					CS$<>8__locals1.modeDescription.RectTransform.SizeChanged += delegate()
					{
						CS$<>8__locals1.modeDescription.RectTransform.NonScaledSize = new Point(CS$<>8__locals1.modeDescription.Rect.Width, (int)CS$<>8__locals1.modeDescription.TextSize.Y);
						RectTransform rectTransform = CS$<>8__locals1.modeFrame.RectTransform;
						int x = 0;
						IEnumerable<GUIComponent> children = CS$<>8__locals1.modeContent.Children;
						Func<GUIComponent, int> selector;
						if ((selector = CS$<>8__locals1.<>9__2) == null)
						{
							selector = (CS$<>8__locals1.<>9__2 = ((GUIComponent c) => c.Rect.Height + CS$<>8__locals1.modeContent.AbsoluteSpacing));
						}
						rectTransform.MinSize = new Point(x, (int)((float)children.Sum(selector) / CS$<>8__locals1.modeContent.RectTransform.RelativeSize.Y));
					};
					new GUIImage(new RectTransform(new Vector2(0.2f, 0.8f), CS$<>8__locals1.modeFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = new Vector2(0.02f, 0f)
					}, "GameModeIcon." + mode.Identifier.ToString(), true);
				}
			}
		}

		// Token: 0x060025F8 RID: 9720 RVA: 0x0018895C File Offset: 0x00186B5C
		private void CreateSubmarineListPanel(GUIComponent parent)
		{
			NetLobbyScreen.<>c__DisplayClass235_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass235_0();
			CS$<>8__locals1.<>4__this = this;
			GUILayoutGroup submarineListHolder = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			GUITextBlock subLabel = NetLobbyScreen.CreateSubHeader("Submarine", submarineListHolder, null);
			this.SubVisibilityButton = new GUIButton(new RectTransform(Vector2.One * 1.2f, subLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
			{
				AbsoluteOffset = new Point(0, GUI.IntScale(5f))
			}, Alignment.Center, "EyeButton", null)
			{
				OnClicked = delegate(GUIButton button, object o)
				{
					CS$<>8__locals1.<>4__this.CreateSubmarineVisibilityMenu();
					return false;
				}
			};
			this.clientHiddenElements.Add(this.SubVisibilityButton);
			GUILayoutGroup filterContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), submarineListHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			NetLobbyScreen.<>c__DisplayClass235_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(new Vector2(0.001f, 1f), filterContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("serverlog.filter");
			GUIFont font = GUIStyle.Font;
			CS$<>8__locals2.searchTitle = new GUITextBlock(rectT, text3, null, font, Alignment.CenterLeft, false, "", null);
			RectTransform rectT2 = new RectTransform(Vector2.One, filterContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.Font;
			this.subSearchBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, true, true);
			filterContainer.RectTransform.MinSize = this.subSearchBox.RectTransform.MinSize;
			this.subSearchBox.OnSelected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = false;
			};
			this.subSearchBox.OnDeselected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = true;
			};
			this.subSearchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				CS$<>8__locals1.<>4__this.UpdateSubVisibility();
				return true;
			};
			this.SubList = new GUIListBox(new RectTransform(new Vector2(1f, 0.93f), submarineListHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = new GUIListBox.OnSelectedHandler(this.VotableClicked)
			};
			GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), subLabel.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Votes"), null, null, Alignment.CenterRight, false, "", null);
			guitextBlock.UserData = "subvotes";
			guitextBlock.Visible = false;
			guitextBlock.CanBeFocused = false;
		}

		// Token: 0x060025F9 RID: 9721 RVA: 0x00188CDC File Offset: 0x00186EDC
		private void CreateSubmarineInfoPanel(GUIComponent parent)
		{
			GUILayoutGroup submarineInfoHolder = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.005f
			};
			this.subPreviewContainer = new GUIFrame(new RectTransform(Vector2.One, submarineInfoHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.subPreviewContainer.RectTransform.SizeChanged += delegate()
			{
				if (this.SelectedSub != null)
				{
					this.CreateSubPreview(this.SelectedSub);
				}
			};
		}

		// Token: 0x060025FA RID: 9722 RVA: 0x00188D90 File Offset: 0x00186F90
		private GUIComponent CreateGameModePanel(GUIComponent parent)
		{
			GUIFrame gameModeSpecificFrame = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.CampaignSetupFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				Visible = false
			};
			this.CampaignFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				Visible = false
			};
			GUILayoutGroup campaignContent = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.5f), this.CampaignFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.3f), campaignContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("gamemode.multiplayercampaign");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 0.3f), campaignContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("quitbutton"), Alignment.Center, "", null);
			guibutton.OnClicked = delegate(GUIButton _, object __)
			{
				if (GameMain.Client == null)
				{
					return false;
				}
				if (GameMain.Client.GameStarted)
				{
					GameMain.Client.RequestEndRound(false, false);
				}
				else
				{
					GameMain.Client.RequestEndRound(false, true);
				}
				return true;
			};
			this.QuitCampaignButton = guibutton;
			this.MissionTypeFrame = new GUIFrame(new RectTransform(Vector2.One, gameModeSpecificFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup missionHolder = new GUILayoutGroup(new RectTransform(Vector2.One, this.MissionTypeFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			NetLobbyScreen.CreateSubHeader("MissionType", missionHolder, null);
			GUIListBox guilistBox = new GUIListBox(new RectTransform(Vector2.One, missionHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.OnSelected = ((GUIComponent component, object obj) => false);
			this.missionTypeList = guilistBox;
			this.clientDisabledElements.Add(this.missionTypeList);
			List<Identifier> missionTypes = MissionPrefab.GetAllMultiplayerSelectableMissionTypes().ToList<Identifier>();
			GUILayoutGroup buttonGroup = new GUILayoutGroup(new RectTransform(Vector2.UnitX, this.missionTypeList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Identifier missionType;
			GUIButton selectAllMissionsButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("selectall"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton _, object _)
				{
					IEnumerable<Identifier> validMissions = this.<CreateGameModePanel>g__GetValidMissions|237_1();
					validMissions.ForEach(delegate(Identifier missionType)
					{
						ServerSettings serverSettings = GameMain.Client.ServerSettings;
						if (serverSettings == null)
						{
							return;
						}
						serverSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, missionType, default(Identifier), 0);
					});
					return true;
				}
			};
			GUIButton deselectAllMissionsButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deselectall"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton _, object _)
				{
					IEnumerable<Identifier> validMissions = this.<CreateGameModePanel>g__GetValidMissions|237_1();
					GameClient client = GameMain.Client;
					if (client != null)
					{
						client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, validMissions.First<Identifier>(), default(Identifier), 0);
					}
					validMissions.Skip(1).ForEach(delegate(Identifier missionType)
					{
						GameClient client2 = GameMain.Client;
						if (client2 == null)
						{
							return;
						}
						client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, default(Identifier), missionType, 0);
					});
					return true;
				}
			};
			buttonGroup.RectTransform.MinSize = new ValueTuple<int, int>(0, buttonGroup.Children.Max((GUIComponent child) => child.Rect.Height));
			this.missionTypeTickBoxes = new GUITickBox[missionTypes.Count];
			int index = 0;
			using (IEnumerator<Identifier> enumerator = (from t in missionTypes
			orderby TextManager.Get("MissionType." + t.Value).Value
			select t).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					missionType = enumerator.Current;
					GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), this.missionTypeList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, GUI.IntScale(30f))
					}, null, null)
					{
						UserData = missionType
					};
					this.missionTypeTickBoxes[index] = new GUITickBox(new RectTransform(Vector2.One, frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MissionType." + missionType.ToString()), null, "")
					{
						UserData = missionType,
						ToolTip = TextManager.Get("MissionTypeDescription." + missionType.ToString()),
						OnSelected = delegate(GUITickBox tickbox)
						{
							this.RefreshOutpostDropdown();
							if (tickbox.Selected)
							{
								GameClient client = GameMain.Client;
								if (client != null)
								{
									ServerSettings serverSettings = client.ServerSettings;
									ServerSettings.NetFlags dataToSend = ServerSettings.NetFlags.Misc;
									Identifier addedMissionType = (Identifier)tickbox.UserData;
									Identifier removedMissionType = default(Identifier);
									serverSettings.ClientAdminWrite(dataToSend, addedMissionType, removedMissionType, 0);
								}
							}
							else
							{
								Identifier firstValidMission = this.<CreateGameModePanel>g__GetValidMissions|237_1().First<Identifier>();
								if (this.missionTypeTickBoxes.None((GUITickBox tickBox) => tickBox.Selected && tickBox.Parent.Visible))
								{
									GameClient client2 = GameMain.Client;
									Identifier removedMissionType;
									if (client2 != null)
									{
										ServerSettings serverSettings2 = client2.ServerSettings;
										ServerSettings.NetFlags dataToSend2 = ServerSettings.NetFlags.Misc;
										Identifier addedMissionType2 = firstValidMission;
										removedMissionType = default(Identifier);
										serverSettings2.ClientAdminWrite(dataToSend2, addedMissionType2, removedMissionType, 0);
									}
									removedMissionType = (Identifier)tickbox.UserData;
									if (removedMissionType == firstValidMission)
									{
										return true;
									}
								}
								GameClient client3 = GameMain.Client;
								if (client3 != null)
								{
									ServerSettings serverSettings3 = client3.ServerSettings;
									ServerSettings.NetFlags dataToSend3 = ServerSettings.NetFlags.Misc;
									Identifier removedMissionType = (Identifier)tickbox.UserData;
									serverSettings3.ClientAdminWrite(dataToSend3, default(Identifier), removedMissionType, 0);
								}
							}
							return true;
						}
					};
					frame.RectTransform.MinSize = this.missionTypeTickBoxes[index].RectTransform.MinSize;
					index++;
				}
			}
			this.clientDisabledElements.Add(selectAllMissionsButton);
			this.clientDisabledElements.Add(deselectAllMissionsButton);
			this.clientDisabledElements.AddRange(this.missionTypeTickBoxes);
			return gameModeSpecificFrame;
		}

		// Token: 0x060025FB RID: 9723 RVA: 0x00189450 File Offset: 0x00187650
		private GUIComponent CreateGameModeSettingsPanel(GUIComponent parent)
		{
			this.gameModeSettingsLayout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			NetLobbyScreen.CreateSubHeader("GameModeSettings", this.gameModeSettingsLayout, null);
			this.gameModeSettingsContent = new GUIListBox(new RectTransform(Vector2.One, this.gameModeSettingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false).Content;
			GUITextBlock winScoreHeader = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsWinScorePvP"), null, null, Alignment.Left, false, "", null)
			{
				CanBeFocused = false
			};
			this.clientDisabledElements.Add(winScoreHeader);
			this.pvpOnlyElements.Add(winScoreHeader);
			GUIScrollBar winScorePvPSlider;
			GUITextBlock winScorePvPSliderLabel;
			GUIComponent winScoreContainer = NetLobbyScreen.CreateLabeledSlider(this.gameModeSettingsContent, string.Empty, string.Empty, "ServerSettingsWinScorePvPTooltip", out winScorePvPSlider, out winScorePvPSliderLabel, null, null);
			winScorePvPSlider.Range = new Vector2(10f, 1000f);
			winScorePvPSlider.StepValue = 10f;
			winScorePvPSlider.OnMoved = delegate(GUIScrollBar scrollBar, float _)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				if (text2 == null)
				{
					return false;
				}
				text2.Text = TextManager.GetWithVariable("ServerSettingsWinScoreValuePvP", "[value]", ((int)Math.Round((double)scrollBar.BarScrollValue, 0)).ToString(), FormatCapitals.No);
				return true;
			};
			winScorePvPSlider.OnReleased = delegate(GUIScrollBar scrollBar, float _)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.AssignComponentToServerSetting(winScorePvPSlider, "WinScorePvP");
			winScorePvPSlider.OnMoved(winScorePvPSlider, winScorePvPSlider.BarScroll);
			this.clientDisabledElements.AddRange(winScoreContainer.GetAllChildren());
			this.pvpOnlyElements.Add(winScoreContainer);
			GUIScrollBar slider;
			GUITextBlock sliderLabel;
			GUIComponent sliderContainer = NetLobbyScreen.CreateLabeledSlider(this.gameModeSettingsContent, string.Empty, "gamemodesettings.stunresistance", "gamemodesettings.stunresistancetooltip", out slider, out sliderLabel, null, null);
			LocalizedString stunResistLabel = sliderLabel.Text;
			slider.Step = 0.1f;
			slider.Range = new Vector2(0f, 1f);
			slider.OnReleased = delegate(GUIScrollBar scrollbar, float value)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			slider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				((GUITextBlock)scrollBar.UserData).Text = stunResistLabel.Replace("[percentage]", ((int)MathUtils.Round(scrollBar.BarScrollValue * 100f, 10f)).ToString(), StringComparison.Ordinal);
				return true;
			};
			this.AssignComponentToServerSetting(slider, "PvPStunResist");
			slider.OnMoved(slider, slider.BarScroll);
			this.clientDisabledElements.AddRange(sliderContainer.GetAllChildren());
			this.pvpOnlyElements.Add(sliderContainer);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.4f, 0.06f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsTrackOpponentInPvP"), null, "");
			guitickBox.ToolTip = TextManager.Get("gamemodesettings.markenemylocationtooltip");
			guitickBox.Selected = (GameMain.Client != null && GameMain.Client.ServerSettings.TrackOpponentInPvP);
			guitickBox.OnSelected = delegate(GUITickBox tt)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			GUITickBox markApproximateEnemyLocationToggle = guitickBox;
			this.AssignComponentToServerSetting(markApproximateEnemyLocationToggle, "TrackOpponentInPvP");
			this.clientDisabledElements.Add(markApproximateEnemyLocationToggle);
			this.pvpOnlyElements.Add(markApproximateEnemyLocationToggle);
			winScoreHeader.RectTransform.MinSize = new Point(0, markApproximateEnemyLocationToggle.RectTransform.MinSize.Y);
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(Vector2.One, this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("gamemodesettings.spawnmonsters"), null, "");
			guitickBox2.ToolTip = TextManager.Get("gamemodesettings.spawnmonsterstooltip");
			guitickBox2.Selected = (GameMain.Client != null && GameMain.Client.ServerSettings.PvPSpawnMonsters);
			guitickBox2.OnSelected = delegate(GUITickBox box)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			GUITickBox spawnMonstersTickbox = guitickBox2;
			this.AssignComponentToServerSetting(spawnMonstersTickbox, "PvPSpawnMonsters");
			this.clientDisabledElements.Add(spawnMonstersTickbox);
			this.pvpOnlyElements.Add(spawnMonstersTickbox);
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(Vector2.One, this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("gamemodesettings.spawnwrecks"), null, "");
			guitickBox3.ToolTip = TextManager.Get("gamemodesettings.spawnwreckstooltip");
			guitickBox3.Selected = (GameMain.Client != null && GameMain.Client.ServerSettings.PvPSpawnWrecks);
			guitickBox3.OnSelected = delegate(GUITickBox box)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			GUITickBox spawnWrecksTickbox = guitickBox3;
			this.AssignComponentToServerSetting(spawnWrecksTickbox, "PvPSpawnWrecks");
			this.clientDisabledElements.Add(spawnWrecksTickbox);
			this.pvpOnlyElements.Add(spawnWrecksTickbox);
			GUILayoutGroup outpostHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Visible = false,
				Stretch = true
			};
			GUITextBlock outpostLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), outpostHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("gamemodesettings.outpost"), null, null, Alignment.Left, true, "", null);
			this.outpostDropdown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), outpostHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 6, "", false, false, Alignment.CenterLeft, 2f)
			{
				ToolTip = TextManager.Get("gamemodesettings.outposttooltip"),
				AfterSelected = delegate(GUIComponent component, object obj)
				{
					if (this.outpostDropdownUpToDate && obj != null)
					{
						GameClient client2 = GameMain.Client;
						if (client2 != null)
						{
							client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
						}
					}
					return true;
				}
			};
			this.outpostDropdown.ListBox.RectTransform.SetPosition(Anchor.BottomLeft, new Pivot?(Pivot.TopLeft));
			this.clientDisabledElements.AddRange(outpostHolder.GetAllChildren());
			this.outpostDropdown.AddItem(TextManager.Get("random"), "Random".ToIdentifier(), null, null, null);
			foreach (SubmarineInfo submarineInfo in SubmarineInfo.SavedSubmarines.DistinctBy((SubmarineInfo s) => s.Name))
			{
				this.outpostDropdown.AddItem(submarineInfo.DisplayName, submarineInfo.Name.ToIdentifier(), submarineInfo.Description, null, null);
			}
			this.AssignComponentToServerSetting(this.outpostDropdown, "SelectedOutpostName");
			outpostHolder.RectTransform.MinSize = new Point(0, this.outpostDropdown.RectTransform.MinSize.Y);
			this.campaignHiddenElements.Add(outpostHolder);
			GUILayoutGroup biomeHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUITextBlock biomeLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), biomeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("biome"), null, null, Alignment.Left, true, "", null);
			GUIDropDown guidropDown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), biomeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 6, "", false, false, Alignment.CenterLeft, 2f);
			guidropDown.AfterSelected = delegate(GUIComponent component, object obj)
			{
				if (obj != null)
				{
					GameClient client2 = GameMain.Client;
					if (client2 != null)
					{
						client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
					}
				}
				return true;
			};
			GUIDropDown biomeDropdown = guidropDown;
			biomeDropdown.ListBox.RectTransform.SetPosition(Anchor.BottomLeft, new Pivot?(Pivot.TopLeft));
			this.clientDisabledElements.AddRange(biomeHolder.GetAllChildren());
			biomeDropdown.AddItem(TextManager.Get("random"), "Random".ToIdentifier(), null, null, null);
			foreach (Biome biome in from b in Biome.Prefabs
			orderby b.MinDifficulty
			select b)
			{
				if (!biome.IsEndBiome)
				{
					biomeDropdown.AddItem(biome.DisplayName, biome.Identifier, null, null, null);
				}
			}
			this.AssignComponentToServerSetting(biomeDropdown, "Biome");
			biomeHolder.RectTransform.MinSize = new Point(0, biomeDropdown.RectTransform.MinSize.Y);
			this.campaignHiddenElements.Add(biomeHolder);
			GUITextBlock seedLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.1f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LevelSeed"), null, null, Alignment.Left, false, "", null)
			{
				CanBeFocused = false
			};
			this.LevelSeedBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), seedLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			this.LevelSeedBox.OnDeselected += delegate(GUITextBox textBox, Keys key)
			{
				GameClient client2 = GameMain.Client;
				if (client2 == null)
				{
					return;
				}
				client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.LevelSeed, default(Identifier), default(Identifier), 0);
			};
			this.campaignDisabledElements.Add(this.LevelSeedBox);
			this.campaignDisabledElements.Add(seedLabel);
			this.clientDisabledElements.Add(this.LevelSeedBox);
			this.clientDisabledElements.Add(seedLabel);
			this.LevelSeed = ToolBox.RandomSeed(8);
			GUITextBlock difficultySliderLabel;
			GUIComponent levelDifficultyHolder = NetLobbyScreen.CreateLabeledSlider(this.gameModeSettingsContent, "LevelDifficulty", "", "LevelDifficultyExplanation", out this.levelDifficultySlider, out difficultySliderLabel, new float?(0.01f), new Vector2?(new Vector2(0f, 100f)));
			this.levelDifficultySlider.OnReleased = delegate(GUIScrollBar scrollbar, float value)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.levelDifficultySlider.OnMoved = delegate(GUIScrollBar scrollbar, float value)
			{
				if (!EventManagerSettings.Prefabs.Any<EventManagerSettings>())
				{
					return true;
				}
				GUITextBlock difficultySliderLabel = difficultySliderLabel;
				LocalizedString name = EventManagerSettings.GetByDifficultyPercentile(value).Name;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(3, 1);
				defaultInterpolatedStringHandler5.AppendLiteral(" (");
				defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(TextManager.GetWithVariable("percentageformat", "[value]", ((int)Math.Round((double)scrollbar.BarScrollValue)).ToString(), FormatCapitals.No));
				defaultInterpolatedStringHandler5.AppendLiteral(")");
				difficultySliderLabel.Text = name + defaultInterpolatedStringHandler5.ToStringAndClear();
				difficultySliderLabel.TextColor = ToolBox.GradientLerp(scrollbar.BarScroll, new Color[]
				{
					GUIStyle.Green,
					GUIStyle.Orange,
					GUIStyle.Red
				});
				return true;
			};
			this.AssignComponentToServerSetting(this.levelDifficultySlider, "SelectedLevelDifficulty");
			this.campaignDisabledElements.AddRange(levelDifficultyHolder.GetAllChildren());
			this.clientDisabledElements.AddRange(levelDifficultyHolder.GetAllChildren());
			NetLobbyScreen.CreateSubHeader("BotSettings", this.gameModeSettingsContent, null);
			GUILayoutGroup botCountSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.7f, 0f), botCountSettingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("BotCount"), null, null, Alignment.Left, true, "", null);
			GUISelectionCarousel<int> botCountSelection = new GUISelectionCarousel<int>(new RectTransform(new Vector2(0.5f, 1f), botCountSettingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<int, LocalizedString>>());
			for (int i = 0; i <= NetConfig.MaxPlayers; i++)
			{
				botCountSelection.AddElement(i, i.ToString(), null);
			}
			this.AssignComponentToServerSetting(botCountSelection, "BotCount");
			this.clientDisabledElements.AddRange(botCountSettingHolder.GetAllChildren());
			this.botSettingsElements.Add(botCountSelection);
			GUILayoutGroup botSpawnModeSettingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.7f, 0f), botSpawnModeSettingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("BotSpawnMode"), null, null, Alignment.Left, true, "", null);
			GUISelectionCarousel<BotSpawnMode> botSpawnModeSelection = new GUISelectionCarousel<BotSpawnMode>(new RectTransform(new Vector2(0.5f, 1f), botSpawnModeSettingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<BotSpawnMode, LocalizedString>>());
			foreach (BotSpawnMode botSpawnMode in Enum.GetValues(typeof(BotSpawnMode)).Cast<BotSpawnMode>())
			{
				GUISelectionCarousel<BotSpawnMode> botSpawnModeSelection3 = botSpawnModeSelection;
				BotSpawnMode value2 = botSpawnMode;
				LocalizedString text = botSpawnMode.ToString();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
				defaultInterpolatedStringHandler.AppendLiteral("botspawnmode.");
				defaultInterpolatedStringHandler.AppendFormatted<BotSpawnMode>(botSpawnMode);
				defaultInterpolatedStringHandler.AppendLiteral(".tooltip");
				botSpawnModeSelection3.AddElement(value2, text, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
			}
			GUISelectionCarousel<BotSpawnMode> botSpawnModeSelection2 = botSpawnModeSelection;
			botSpawnModeSelection2.OnValueChanged = (GUISelectionCarousel<BotSpawnMode>.OnValueChangedHandler)Delegate.Combine(botSpawnModeSelection2.OnValueChanged, new GUISelectionCarousel<BotSpawnMode>.OnValueChangedHandler(delegate(GUISelectionCarousel<BotSpawnMode> _)
			{
				GameClient client2 = GameMain.Client;
				if (client2 == null)
				{
					return;
				}
				client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			}));
			this.AssignComponentToServerSetting(botSpawnModeSelection, "BotSpawnMode");
			this.clientDisabledElements.AddRange(botSpawnModeSettingHolder.GetAllChildren());
			this.botSettingsElements.Add(botSpawnModeSelection);
			GUISelectionCarousel<int> guiselectionCarousel = botCountSelection;
			guiselectionCarousel.OnValueChanged = (GUISelectionCarousel<int>.OnValueChangedHandler)Delegate.Combine(guiselectionCarousel.OnValueChanged, new GUISelectionCarousel<int>.OnValueChangedHandler(delegate(GUISelectionCarousel<int> _)
			{
				botSpawnModeSelection.Enabled = (GameMain.Client.ServerSettings.BotCount > 0);
				GameClient client2 = GameMain.Client;
				if (client2 == null)
				{
					return;
				}
				client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			}));
			NetLobbyScreen.CreateSubHeader("TraitorSettings", this.gameModeSettingsContent, null);
			new GUIFrame(new RectTransform(new Point(1, GUI.IntScale(5f)), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			GUITextBlock traitorProbabilityText;
			GUIComponent traitorProbabilityHolder = NetLobbyScreen.CreateLabeledSlider(this.gameModeSettingsContent, "traitor.probability", "", "traitor.probability.tooltip", out this.traitorProbabilitySlider, out traitorProbabilityText, new float?(0.01f), new Vector2?(new Vector2(0f, 1f)));
			this.traitorProbabilitySlider.OnMoved = delegate(GUIScrollBar scrollbar, float value)
			{
				traitorProbabilityText.Text = TextManager.GetWithVariable("percentageformat", "[value]", ((int)Math.Round((double)(scrollbar.BarScrollValue * 100f))).ToString(), FormatCapitals.No);
				traitorProbabilityText.TextColor = ((value <= 0f) ? GUIStyle.Green : ToolBox.GradientLerp(scrollbar.BarScroll, new Color[]
				{
					GUIStyle.Yellow,
					GUIStyle.Orange,
					GUIStyle.Red
				}));
				this.RefreshEnabledElements();
				return true;
			};
			this.traitorProbabilitySlider.OnMoved(this.traitorProbabilitySlider, this.traitorProbabilitySlider.BarScroll);
			GUIScrollBar guiscrollBar = this.traitorProbabilitySlider;
			guiscrollBar.OnReleased = (GUIScrollBar.OnMovedHandler)Delegate.Combine(guiscrollBar.OnReleased, new GUIScrollBar.OnMovedHandler(delegate(GUIScrollBar scrollbar, float value)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			}));
			this.AssignComponentToServerSetting(this.traitorProbabilitySlider, "TraitorProbability");
			this.traitorElements.Clear();
			this.clientDisabledElements.AddRange(traitorProbabilityHolder.GetAllChildren());
			GUILayoutGroup traitorDangerHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			GUITextBlock dangerLevelLabel = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), traitorDangerHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("traitor.dangerlevelsetting"), null, null, Alignment.Left, true, "", null)
			{
				ToolTip = TextManager.Get("traitor.dangerlevelsetting.tooltip")
			};
			GUILayoutGroup traitorDangerContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), traitorDangerHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			GUIButton[] traitorDangerButtons = new GUIButton[2];
			GUIButton[] array = traitorDangerButtons;
			int num = 0;
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), traitorDangerContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleLeft", null);
			guibutton.OnClicked = delegate(GUIButton button, object obj)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, default(Identifier), default(Identifier), -1);
				}
				return true;
			};
			array[num] = guibutton;
			this.traitorDangerGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 1f), traitorDangerContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 1
			};
			for (int j = 1; j <= 3; j++)
			{
				Color difficultyColor = Mission.GetDifficultyColor(j);
				GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.75f), this.traitorDangerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "DifficultyIndicator", true);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler2.AppendFormatted(Color.White.ToStringHex());
				defaultInterpolatedStringHandler2.AppendLiteral("‖");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("traitor.dangerlevel.");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(j);
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()));
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
				LocalizedString left = defaultInterpolatedStringHandler2.ToStringAndClear() + "\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("traitor.dangerlevel.");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(j);
				defaultInterpolatedStringHandler4.AppendLiteral(".description");
				guiimage.ToolTip = RichString.Rich(left + TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear()), null);
				guiimage.Color = difficultyColor;
				guiimage.DisabledColor = Color.Gray * 0.5f;
			}
			GUIButton[] array2 = traitorDangerButtons;
			int num2 = 1;
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), traitorDangerContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleRight", null);
			guibutton2.OnClicked = delegate(GUIButton button, object obj)
			{
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Misc, default(Identifier), default(Identifier), 1);
				}
				return true;
			};
			array2[num2] = guibutton2;
			traitorDangerContainer.InheritTotalChildrenMinHeight();
			GameClient client = GameMain.Client;
			this.SetTraitorDangerIndicators((client != null) ? client.ServerSettings.TraitorDangerLevel : 1);
			this.traitorElements.Add(dangerLevelLabel);
			this.traitorElements.AddRange(this.traitorDangerGroup.Children);
			this.traitorElements.AddRange(traitorDangerButtons);
			GUILayoutGroup traitorsMinPlayerCountHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), this.gameModeSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.7f, 0f), traitorsMinPlayerCountHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsTraitorsMinPlayerCount"), null, null, Alignment.Left, true, "", null).ToolTip = TextManager.Get("ServerSettingsTraitorsMinPlayerCountToolTip");
			GUISelectionCarousel<int> traitorsMinPlayerCount = new GUISelectionCarousel<int>(new RectTransform(new Vector2(0.5f, 1f), traitorsMinPlayerCountHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<int, LocalizedString>>());
			for (int k = 1; k <= NetConfig.MaxPlayers; k++)
			{
				traitorsMinPlayerCount.AddElement(k, k.ToString(), null);
			}
			GUISelectionCarousel<int> guiselectionCarousel2 = traitorsMinPlayerCount;
			guiselectionCarousel2.OnValueChanged = (GUISelectionCarousel<int>.OnValueChangedHandler)Delegate.Combine(guiselectionCarousel2.OnValueChanged, new GUISelectionCarousel<int>.OnValueChangedHandler(delegate(GUISelectionCarousel<int> _)
			{
				GameClient client2 = GameMain.Client;
				if (client2 == null)
				{
					return;
				}
				client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			}));
			this.AssignComponentToServerSetting(traitorsMinPlayerCount, "TraitorsMinPlayerCount");
			this.traitorElements.AddRange(traitorsMinPlayerCountHolder.Children);
			foreach (GUIComponent traitorElement in this.traitorElements)
			{
				if (!this.clientDisabledElements.Contains(traitorElement))
				{
					this.clientDisabledElements.Add(traitorElement);
				}
			}
			return this.gameModeSettingsContent;
		}

		// Token: 0x060025FC RID: 9724 RVA: 0x0018ABEC File Offset: 0x00188DEC
		private void SelectRespawnTab()
		{
			this.SelectTabShared(this.respawnTabButton, this.upgradesTabButton, this.respawnSettings, this.disembarkPerkSettings);
		}

		// Token: 0x060025FD RID: 9725 RVA: 0x0018AC0C File Offset: 0x00188E0C
		private void SelectUpgradesTab()
		{
			this.SelectTabShared(this.upgradesTabButton, this.respawnTabButton, this.disembarkPerkSettings, this.respawnSettings);
		}

		// Token: 0x060025FE RID: 9726 RVA: 0x0018AC2C File Offset: 0x00188E2C
		private void SelectTabShared(GUIButton buttonToEnable, GUIButton buttonToDisable, ICollection<GUIComponent> elementsToEnable, ICollection<GUIComponent> elementsToDisable)
		{
			if (buttonToEnable == null || buttonToDisable == null)
			{
				return;
			}
			buttonToDisable.Selected = false;
			buttonToEnable.Selected = true;
			foreach (GUIComponent element in elementsToDisable)
			{
				element.Visible = (element.Enabled = false);
			}
			foreach (GUIComponent element2 in elementsToEnable)
			{
				element2.Visible = (element2.Enabled = true);
			}
		}

		// Token: 0x060025FF RID: 9727 RVA: 0x0018ACD8 File Offset: 0x00188ED8
		private GUIComponent CreateGeneralSettingsPanel(GUIComponent parent)
		{
			GUILayoutGroup mainContainer = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup tabContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.066f), mainContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			this.respawnTabButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), tabContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("respawnsettings"), Alignment.Center, "GUITabButton", null)
			{
				Selected = true
			};
			this.upgradesTabButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), tabContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("disembarkpointsettings"), Alignment.Center, "GUITabButton", null);
			this.respawnTabButton.OnClicked = delegate(GUIButton button, object _)
			{
				this.SelectRespawnTab();
				return true;
			};
			this.upgradesTabButton.OnClicked = delegate(GUIButton button, object _)
			{
				this.SelectUpgradesTab();
				return true;
			};
			GUIFrame mainFrame = new GUIFrame(new RectTransform(new Vector2(1f, 1f - tabContainer.RectTransform.RelativeSize.Y), mainContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup settingsLayout = new GUILayoutGroup(new RectTransform(Vector2.One, mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIListBox settingsList = new GUIListBox(new RectTransform(Vector2.One, settingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			this.respawnSettings.Add(settingsLayout);
			this.CreateDisembarkPointPanel(mainFrame);
			GUIFrame settingsContent = settingsList.Content;
			GUILayoutGroup respawnModeHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), settingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			this.respawnModeLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 0f), respawnModeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RespawnMode"), null, null, Alignment.Left, true, "", null);
			this.respawnModeSelection = new GUISelectionCarousel<RespawnMode>(new RectTransform(new Vector2(0.6f, 1f), respawnModeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<RespawnMode, LocalizedString>>());
			foreach (RespawnMode respawnMode in from RespawnMode rm in Enum.GetValues(typeof(RespawnMode))
			where rm > RespawnMode.None
			select rm)
			{
				GUISelectionCarousel<RespawnMode> guiselectionCarousel = this.respawnModeSelection;
				RespawnMode value2 = respawnMode;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler.AppendLiteral("respawnmode.");
				defaultInterpolatedStringHandler.AppendFormatted<RespawnMode>(respawnMode);
				LocalizedString text = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("respawnmode.");
				defaultInterpolatedStringHandler2.AppendFormatted<RespawnMode>(respawnMode);
				defaultInterpolatedStringHandler2.AppendLiteral(".tooltip");
				guiselectionCarousel.AddElement(value2, text, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
			}
			GUISelectionCarousel<RespawnMode> guiselectionCarousel2 = this.respawnModeSelection;
			guiselectionCarousel2.ElementSelectionCondition = (Func<RespawnMode, bool>)Delegate.Combine(guiselectionCarousel2.ElementSelectionCondition, new Func<RespawnMode, bool>((RespawnMode value) => value != RespawnMode.Permadeath || this.SelectedMode == GameModePreset.MultiPlayerCampaign));
			GUISelectionCarousel<RespawnMode> guiselectionCarousel3 = this.respawnModeSelection;
			guiselectionCarousel3.OnValueChanged = (GUISelectionCarousel<RespawnMode>.OnValueChangedHandler)Delegate.Combine(guiselectionCarousel3.OnValueChanged, new GUISelectionCarousel<RespawnMode>.OnValueChangedHandler(delegate(GUISelectionCarousel<RespawnMode> _)
			{
				GameClient client = GameMain.Client;
				if (client == null)
				{
					return;
				}
				client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			}));
			this.AssignComponentToServerSetting(this.respawnModeSelection, "RespawnMode");
			GUILayoutGroup shuttleHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), settingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One, shuttleHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RespawnShuttle"), null, "");
			guitickBox.ToolTip = TextManager.Get("RespawnShuttleExplanation");
			guitickBox.Selected = true;
			guitickBox.OnSelected = delegate(GUITickBox box)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.shuttleTickBox = guitickBox;
			this.AssignComponentToServerSetting(this.shuttleTickBox, "UseRespawnShuttle");
			this.midRoundRespawnSettings.Add(this.shuttleTickBox);
			this.shuttleTickBox.TextBlock.RectTransform.SizeChanged += delegate()
			{
				this.shuttleTickBox.TextBlock.AutoScaleHorizontal = true;
				this.shuttleTickBox.TextBlock.TextScale = 1f;
				if (this.shuttleTickBox.TextBlock.TextScale < 0.75f)
				{
					this.shuttleTickBox.TextBlock.Wrap = true;
					this.shuttleTickBox.TextBlock.AutoScaleHorizontal = true;
					this.shuttleTickBox.TextBlock.TextScale = 1f;
				}
			};
			this.ShuttleList = new GUIDropDown(new RectTransform(Vector2.One, shuttleHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 10, "", false, false, Alignment.CenterLeft, 1f)
			{
				OnSelected = delegate(GUIComponent component, object obj)
				{
					SubmarineInfo subInfo = (SubmarineInfo)obj;
					this.ShuttleList.Text = subInfo.DisplayName;
					this.ShuttleList.ToolTip = subInfo.Description;
					NetLobbyScreen.SelectShuttle(subInfo);
					return true;
				}
			};
			this.ShuttleList.ListBox.RectTransform.MinSize = new Point(250, 0);
			shuttleHolder.RectTransform.MinSize = new Point(0, this.ShuttleList.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			this.midRoundRespawnSettings.Add(this.ShuttleList);
			GUIComponent parent2 = settingsContent;
			string headerTag = "ServerSettingsRespawnInterval";
			string valueLabelTag = "";
			string tooltipTag = "";
			Vector2? range = new Vector2?(new Vector2(10f, 600f));
			GUIScrollBar respawnIntervalSlider;
			GUITextBlock respawnIntervalSliderLabel;
			this.respawnIntervalElement = NetLobbyScreen.CreateLabeledSlider(parent2, headerTag, valueLabelTag, tooltipTag, out respawnIntervalSlider, out respawnIntervalSliderLabel, null, range);
			LocalizedString intervalLabel = respawnIntervalSliderLabel.Text;
			respawnIntervalSlider.StepValue = 10f;
			respawnIntervalSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = intervalLabel + " " + ToolBox.SecondsToReadableTime(scrollBar.BarScrollValue);
				return true;
			};
			respawnIntervalSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			respawnIntervalSlider.OnMoved(respawnIntervalSlider, respawnIntervalSlider.BarScroll);
			this.AssignComponentToServerSetting(respawnIntervalSlider, "RespawnInterval");
			GUIScrollBar minRespawnSlider;
			GUITextBlock minRespawnSliderLabel;
			GUIComponent minRespawnElement = NetLobbyScreen.CreateLabeledSlider(settingsContent, "ServerSettingsMinRespawn", "", "ServerSettingsMinRespawnToolTip", out minRespawnSlider, out minRespawnSliderLabel, new float?(0.1f), new Vector2?(new Vector2(0f, 1f)));
			minRespawnSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = ToolBox.GetFormattedPercentage(scrollBar.BarScrollValue);
				return true;
			};
			minRespawnSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			minRespawnSlider.OnMoved(minRespawnSlider, minRespawnSlider.BarScroll);
			this.midRoundRespawnSettings.AddRange(minRespawnElement.GetAllChildren());
			this.AssignComponentToServerSetting(minRespawnSlider, "MinRespawnRatio");
			GUIScrollBar respawnDurationSlider;
			GUITextBlock respawnDurationSliderLabel;
			GUIComponent respawnDurationElement = NetLobbyScreen.CreateLabeledSlider(settingsContent, "ServerSettingsRespawnDuration", "", "ServerSettingsRespawnDurationTooltip", out respawnDurationSlider, out respawnDurationSliderLabel, new float?(0.1f), new Vector2?(new Vector2(60f, 660f)));
			respawnDurationSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = ((scrollBar.BarScrollValue <= 0f) ? TextManager.Get("Unlimited") : ToolBox.SecondsToReadableTime(scrollBar.BarScrollValue));
				return true;
			};
			respawnDurationSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			respawnDurationSlider.ScrollToValue = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				if (barScroll < 1f)
				{
					return barScroll * (scrollBar.Range.Y - scrollBar.Range.X) + scrollBar.Range.X;
				}
				return 0f;
			};
			respawnDurationSlider.ValueToScroll = delegate(GUIScrollBar scrollBar, float value)
			{
				if (value > 0f)
				{
					return (value - scrollBar.Range.X) / (scrollBar.Range.Y - scrollBar.Range.X);
				}
				return 1f;
			};
			respawnDurationSlider.OnMoved(respawnDurationSlider, respawnDurationSlider.BarScroll);
			this.midRoundRespawnSettings.AddRange(respawnDurationElement.GetAllChildren());
			this.AssignComponentToServerSetting(respawnDurationSlider, "MaxTransportTime");
			GUIComponent parent3 = settingsContent;
			string headerTag2 = "ServerSettingsSkillLossPercentageOnDeath";
			string valueLabelTag2 = "";
			string tooltipTag2 = "ServerSettingsSkillLossPercentageOnDeathToolTip";
			range = new Vector2?(new Vector2(0f, 100f));
			GUIScrollBar skillLossSlider;
			GUITextBlock skillLossSliderLabel;
			GUIComponent skillLossElement = NetLobbyScreen.CreateLabeledSlider(parent3, headerTag2, valueLabelTag2, tooltipTag2, out skillLossSlider, out skillLossSliderLabel, null, range);
			skillLossSlider.StepValue = 1f;
			skillLossSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = TextManager.GetWithVariable("percentageformat", "[value]", ((int)Math.Round((double)scrollBar.BarScrollValue)).ToString(), FormatCapitals.No);
				return true;
			};
			skillLossSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.permadeathDisabledRespawnSettings.AddRange(skillLossElement.GetAllChildren());
			this.clientDisabledElements.AddRange(skillLossElement.GetAllChildren());
			this.AssignComponentToServerSetting(skillLossSlider, "SkillLossPercentageOnDeath");
			skillLossSlider.OnMoved(skillLossSlider, skillLossSlider.BarScroll);
			GUIComponent parent4 = settingsContent;
			string headerTag3 = "ServerSettingsSkillLossPercentageOnImmediateRespawn";
			string valueLabelTag3 = "";
			string tooltipTag3 = "ServerSettingsSkillLossPercentageOnImmediateRespawnToolTip";
			range = new Vector2?(new Vector2(0f, 100f));
			GUIScrollBar skillLossImmediateRespawnSlider;
			GUITextBlock skillLossImmediateRespawnSliderLabel;
			GUIComponent skillLossImmediateRespawnElement = NetLobbyScreen.CreateLabeledSlider(parent4, headerTag3, valueLabelTag3, tooltipTag3, out skillLossImmediateRespawnSlider, out skillLossImmediateRespawnSliderLabel, null, range);
			skillLossImmediateRespawnSlider.StepValue = 1f;
			skillLossImmediateRespawnSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = TextManager.GetWithVariable("percentageformat", "[value]", ((int)Math.Round((double)scrollBar.BarScrollValue)).ToString(), FormatCapitals.No);
				return true;
			};
			skillLossImmediateRespawnSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.midRoundRespawnSettings.AddRange(skillLossImmediateRespawnElement.GetAllChildren());
			this.permadeathDisabledRespawnSettings.AddRange(skillLossImmediateRespawnElement.GetAllChildren());
			this.AssignComponentToServerSetting(skillLossImmediateRespawnSlider, "SkillLossPercentageOnImmediateRespawn");
			skillLossImmediateRespawnSlider.OnMoved(skillLossImmediateRespawnSlider, skillLossImmediateRespawnSlider.BarScroll);
			GUIComponent parent5 = settingsContent;
			string headerTag4 = "ServerSettings.ReplaceCostPercentage";
			string valueLabelTag4 = "";
			string tooltipTag4 = "ServerSettings.ReplaceCostPercentage.tooltip";
			range = new Vector2?(new Vector2(0f, 200f));
			GUIScrollBar newCharacterCostSlider;
			GUITextBlock newCharacterCostSliderLabel;
			GUIComponent newCharacterCostSliderElement = NetLobbyScreen.CreateLabeledSlider(parent5, headerTag4, valueLabelTag4, tooltipTag4, out newCharacterCostSlider, out newCharacterCostSliderLabel, new float?(10f), range);
			newCharacterCostSlider.StepValue = 10f;
			newCharacterCostSlider.OnMoved = delegate(GUIScrollBar scrollBar, float _)
			{
				GUITextBlock textBlock = scrollBar.UserData as GUITextBlock;
				int currentMultiplier = (int)Math.Round((double)scrollBar.BarScrollValue);
				if (currentMultiplier < 1)
				{
					textBlock.Text = TextManager.Get("ServerSettings.ReplaceCostPercentage.Free");
				}
				else
				{
					textBlock.Text = TextManager.GetWithVariable("percentageformat", "[value]", currentMultiplier.ToString(), FormatCapitals.No);
				}
				return true;
			};
			newCharacterCostSlider.OnReleased = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.clientDisabledElements.AddRange(newCharacterCostSliderElement.GetAllChildren());
			this.permadeathEnabledRespawnSettings.AddRange(newCharacterCostSliderElement.GetAllChildren());
			this.ironmanDisabledRespawnSettings.AddRange(newCharacterCostSliderElement.GetAllChildren());
			this.AssignComponentToServerSetting(newCharacterCostSlider, "ReplaceCostPercentage");
			newCharacterCostSlider.OnMoved(newCharacterCostSlider, newCharacterCostSlider.BarScroll);
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(Vector2.One, settingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("AllowBotTakeover"), null, "");
			guitickBox2.ToolTip = TextManager.Get("AllowBotTakeover.Tooltip");
			guitickBox2.Selected = (GameMain.Client != null && GameMain.Client.ServerSettings.AllowBotTakeoverOnPermadeath);
			guitickBox2.OnSelected = delegate(GUITickBox box)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			GUITickBox allowBotTakeoverTickbox = guitickBox2;
			this.AssignComponentToServerSetting(allowBotTakeoverTickbox, "AllowBotTakeoverOnPermadeath");
			this.permadeathEnabledRespawnSettings.Add(allowBotTakeoverTickbox);
			this.ironmanDisabledRespawnSettings.Add(allowBotTakeoverTickbox);
			this.clientDisabledElements.Add(allowBotTakeoverTickbox);
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(Vector2.One, settingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("IronmanMode").ToUpper(), null, "");
			guitickBox3.ToolTip = TextManager.Get("IronmanMode.Tooltip");
			guitickBox3.Selected = (GameMain.Client != null && GameMain.Client.ServerSettings.IronmanMode);
			guitickBox3.OnSelected = delegate(GUITickBox box)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			GUITickBox ironmanTickbox = guitickBox3;
			this.AssignComponentToServerSetting(ironmanTickbox, "IronmanMode");
			this.permadeathEnabledRespawnSettings.Add(ironmanTickbox);
			this.clientDisabledElements.Add(ironmanTickbox);
			foreach (GUIComponent respawnElement in this.midRoundRespawnSettings)
			{
				if (!this.clientDisabledElements.Contains(respawnElement))
				{
					this.clientDisabledElements.Add(respawnElement);
				}
			}
			return settingsContent;
		}

		// Token: 0x06002600 RID: 9728 RVA: 0x0018BAE8 File Offset: 0x00189CE8
		public void CreateDisembarkPointPanel(GUIComponent parent)
		{
			NetLobbyScreen.<>c__DisplayClass252_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass252_0();
			CS$<>8__locals1.<>4__this = this;
			GUILayoutGroup settingsLayout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				Visible = false
			};
			CS$<>8__locals1.settingsList = new GUIListBox(new RectTransform(Vector2.One, settingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				SelectMultiple = true,
				DisabledColor = Color.White * 0.1f
			};
			this.disembarkPerkSettingList = CS$<>8__locals1.settingsList;
			this.noPerksAvailableDisclaimer = new GUIFrame(new RectTransform(Vector2.One, settingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				Visible = false,
				IgnoreLayoutGroups = true
			};
			RectTransform rectT = new RectTransform(Vector2.One, this.noPerksAvailableDisclaimer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("noperksavailable");
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.Center, true, "", null);
			guitextBlock.TextColor = GUIStyle.Red;
			guitextBlock.Shadow = true;
			this.disembarkPerkDisabledDisclaimer = new GUIFrame(new RectTransform(Vector2.One, settingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				IgnoreLayoutGroups = true
			};
			GUILayoutGroup disclaimerLayout = new GUILayoutGroup(new RectTransform(Vector2.One, this.disembarkPerkDisabledDisclaimer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.3f), disclaimerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("disembarkpointselectteam");
			font = GUIStyle.LargeFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.BottomCenter, false, "", null).TextColor = GUIStyle.Red;
			GUILayoutGroup teamSelectLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.7f), disclaimerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			CS$<>8__locals1.<CreateDisembarkPointPanel>g__CreateTeamDisclaimerButtons|2(teamSelectLayout);
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.055f), settingsLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, GUI.IntScale(28f));
			RichString text3 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			this.disembarkPerkFooterText = new GUITextBlock(rectTransform, text3, new Color?(GUIStyle.TextColorBright), font, Alignment.CenterRight, false, null, new Color?(Color.Black * 0.8f))
			{
				Padding = new Vector4(10f, 0f, 10f, 0f) * GUI.Scale
			};
			this.UpdatePerkFooterText(CS$<>8__locals1.settingsList);
			CS$<>8__locals1.settingsList.AfterSelected = delegate(GUIComponent component, object o)
			{
				GameClient client2 = GameMain.Client;
				ServerSettings settings = (client2 != null) ? client2.ServerSettings : null;
				if (settings == null)
				{
					return false;
				}
				CS$<>8__locals1.<>4__this.UpdatePerkFooterText(CS$<>8__locals1.settingsList);
				if (CS$<>8__locals1.<>4__this.isUpdatingPerks)
				{
					return false;
				}
				if (!ServerSettings.HasPermissionToChangePerks())
				{
					return false;
				}
				CharacterTeamType teamPreference = MultiplayerPreferences.Instance.TeamPreference;
				if (teamPreference == CharacterTeamType.Team2)
				{
					settings.SelectedSeparatistsPerks = base.<CreateDisembarkPointPanel>g__PerksFromSelectedElements|3();
				}
				else
				{
					settings.SelectedCoalitionPerks = base.<CreateDisembarkPointPanel>g__PerksFromSelectedElements|3();
				}
				settings.ClientAdminWritePerks();
				return true;
			};
			this.disembarkPerkSettings.Add(settingsLayout);
			Identifier disembarkPerkCategory = Identifier.Empty;
			foreach (DisembarkPerkPrefab disembarkPerkPrefab in from p in DisembarkPerkPrefab.Prefabs
			orderby p.SortCategory, p.SortKey, p.Cost
			select p)
			{
				Identifier sortCategory = disembarkPerkPrefab.SortCategory;
				if (disembarkPerkCategory != sortCategory)
				{
					disembarkPerkCategory = disembarkPerkPrefab.SortCategory;
					if (!disembarkPerkCategory.IsEmpty)
					{
						GUIFrame categoryFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), CS$<>8__locals1.settingsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
						{
							CanBeFocused = false
						};
						RectTransform rectT3 = new RectTransform(Vector2.One, categoryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
						defaultInterpolatedStringHandler.AppendLiteral("perkcategory.");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(disembarkPerkPrefab.SortCategory);
						RichString text4 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
						font = GUIStyle.SubHeadingFont;
						new GUITextBlock(rectT3, text4, null, font, Alignment.Center, false, "", null);
					}
				}
				GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals1.settingsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
				{
					UserData = disembarkPerkPrefab,
					ToolTip = disembarkPerkPrefab.Description
				};
				GUILayoutGroup prefabLayout = new GUILayoutGroup(new RectTransform(Vector2.One, frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true
				};
				GUITextBlock perkLabel = new GUITextBlock(new RectTransform(new Vector2(0.8f, 1f), prefabLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), disembarkPerkPrefab.Name, null, null, Alignment.CenterLeft, false, "", null)
				{
					DisabledTextColor = Color.White * 0.1f,
					DisabledColor = Color.White * 0.1f,
					CanBeFocused = false
				};
				perkLabel.Text = ToolBox.LimitString(perkLabel.Text, perkLabel.Font, perkLabel.Rect.Width);
				GUITextBlock guitextBlock2 = new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), prefabLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), disembarkPerkPrefab.Cost.ToString(), null, null, Alignment.Right, false, "", null);
				guitextBlock2.DisabledTextColor = Color.White * 0.1f;
				guitextBlock2.DisabledColor = Color.White * 0.1f;
				guitextBlock2.CanBeFocused = false;
			}
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			NamedEvent<GameClient.PermissionChangedEvent> onPermissionChanged = client.OnPermissionChanged;
			if (onPermissionChanged == null)
			{
				return;
			}
			onPermissionChanged.RegisterOverwriteExisting("CreateDisembarkPointPanel".ToIdentifier(), delegate(GameClient.PermissionChangedEvent _)
			{
				CS$<>8__locals1.<>4__this.UpdateDisembarkPointListFromServerSettings();
			});
		}

		// Token: 0x06002601 RID: 9729 RVA: 0x0018C310 File Offset: 0x0018A510
		private void UpdatePerkFooterText(GUIListBox box)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			int? num;
			if (networkMember == null)
			{
				num = null;
			}
			else
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				num = ((serverSettings != null) ? new int?(serverSettings.DisembarkPointAllowance) : null);
			}
			int? num2 = num;
			int pointsLeft = num2.GetValueOrDefault(-1);
			bool ignorePerksThatCantApplyWithoutSub = GameSession.ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(this.SelectedMode, this.MissionTypes);
			foreach (GUIComponent child in box.Content.Children)
			{
				if (box.AllSelected.Contains(child))
				{
					DisembarkPerkPrefab perkPrefab = child.UserData as DisembarkPerkPrefab;
					if (perkPrefab != null)
					{
						if (ignorePerksThatCantApplyWithoutSub)
						{
							if (perkPrefab.PerkBehaviors.Any((PerkBase b) => !b.CanApplyWithoutSubmarine()))
							{
								continue;
							}
						}
						pointsLeft -= perkPrefab.Cost;
					}
				}
			}
			this.disembarkPerkFooterText.Text = TextManager.GetWithVariable("disembarkpointleft", "[amount]", pointsLeft.ToString(), FormatCapitals.No);
			this.disembarkPerkFooterText.TextColor = ((pointsLeft < 0) ? GUIStyle.Red : GUIStyle.TextColorBright);
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x0018C454 File Offset: 0x0018A654
		public void UpdateDisembarkPointListFromServerSettings()
		{
			if (this.disembarkPerkSettingList == null || this.disembarkPerkDisabledDisclaimer == null || this.disembarkPerkFooterText == null)
			{
				return;
			}
			CharacterTeamType teamPreference = MultiplayerPreferences.Instance.TeamPreference;
			bool flag = teamPreference - CharacterTeamType.Team1 <= 1;
			bool hasTeamPreference = flag;
			if (this.SelectedMode != GameModePreset.PvP)
			{
				teamPreference = CharacterTeamType.Team1;
				hasTeamPreference = true;
			}
			this.disembarkPerkDisabledDisclaimer.Visible = !hasTeamPreference;
			this.disembarkPerkFooterText.Visible = hasTeamPreference;
			this.<UpdateDisembarkPointListFromServerSettings>g__SetEnabled|254_0(hasTeamPreference);
			if (!ServerSettings.HasPermissionToChangePerks())
			{
				this.<UpdateDisembarkPointListFromServerSettings>g__SetEnabled|254_0(false);
			}
			this.isUpdatingPerks = true;
			bool hasAvailablePerks = false;
			GameClient client = GameMain.Client;
			ServerSettings settings = (client != null) ? client.ServerSettings : null;
			if (settings != null)
			{
				Identifier[] array;
				if (teamPreference != CharacterTeamType.Team1)
				{
					if (teamPreference != CharacterTeamType.Team2)
					{
						array = Array.Empty<Identifier>();
					}
					else
					{
						array = settings.SelectedSeparatistsPerks;
					}
				}
				else
				{
					array = settings.SelectedCoalitionPerks;
				}
				Identifier[] selectedPerks = array;
				bool ignorePerksThatCantApplyWithoutSub = GameSession.ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(this.SelectedMode, this.MissionTypes);
				this.disembarkPerkSettingList.Deselect();
				foreach (GUIComponent child in this.disembarkPerkSettingList.Content.Children)
				{
					NetLobbyScreen.<>c__DisplayClass254_0 CS$<>8__locals1;
					CS$<>8__locals1.child = child;
					object userData = CS$<>8__locals1.child.UserData;
					DisembarkPerkPrefab perkPrefab = userData as DisembarkPerkPrefab;
					if (perkPrefab != null)
					{
						bool shouldSelect = selectedPerks.Contains(perkPrefab.Identifier);
						bool hasPrerequisite = !perkPrefab.Prerequisite.IsEmpty;
						bool isMutuallyExclusivePerkSelected = selectedPerks.Any((Identifier p) => perkPrefab.MutuallyExclusivePerks.Contains(p));
						NetLobbyScreen.<UpdateDisembarkPointListFromServerSettings>g__TogglePerkElement|254_3(true, ref CS$<>8__locals1);
						if (shouldSelect)
						{
							this.disembarkPerkSettingList.Select(CS$<>8__locals1.child.UserData, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Disabled);
						}
						if (hasPrerequisite)
						{
							bool enabled = selectedPerks.Contains(perkPrefab.Prerequisite);
							NetLobbyScreen.<UpdateDisembarkPointListFromServerSettings>g__TogglePerkElement|254_3(enabled, ref CS$<>8__locals1);
						}
						if (isMutuallyExclusivePerkSelected)
						{
							NetLobbyScreen.<UpdateDisembarkPointListFromServerSettings>g__TogglePerkElement|254_3(false, ref CS$<>8__locals1);
						}
						if (ignorePerksThatCantApplyWithoutSub)
						{
							if (perkPrefab.PerkBehaviors.Any((PerkBase b) => !b.CanApplyWithoutSubmarine()))
							{
								NetLobbyScreen.<UpdateDisembarkPointListFromServerSettings>g__TogglePerkElement|254_3(false, ref CS$<>8__locals1);
							}
						}
						if (CS$<>8__locals1.child.Enabled)
						{
							hasAvailablePerks = true;
						}
					}
				}
			}
			this.noPerksAvailableDisclaimer.Visible = !hasAvailablePerks;
			if (!hasAvailablePerks)
			{
				this.disembarkPerkDisabledDisclaimer.Visible = false;
			}
			this.UpdatePerkFooterText(this.disembarkPerkSettingList);
			this.isUpdatingPerks = false;
		}

		// Token: 0x06002603 RID: 9731 RVA: 0x0018C6F8 File Offset: 0x0018A8F8
		public static void SelectShuttle(SubmarineInfo info)
		{
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			client.RequestSelectSub(info, SelectedSubType.Shuttle);
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x0018C70C File Offset: 0x0018A90C
		public static GUITextBlock CreateSubHeader(string textTag, GUIComponent parent, string toolTipTag = null)
		{
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.055f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, GUI.IntScale(28f));
			RichString text = TextManager.Get(textTag);
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock header = new GUITextBlock(rectTransform, text, new Color?(GUIStyle.TextColorBright), subHeadingFont, Alignment.BottomLeft, false, "", null)
			{
				CanBeFocused = false
			};
			if (!toolTipTag.IsNullOrEmpty())
			{
				header.ToolTip = TextManager.Get(toolTipTag);
			}
			return header;
		}

		// Token: 0x06002605 RID: 9733 RVA: 0x0018C7C0 File Offset: 0x0018A9C0
		public static GUIComponent CreateLabeledSlider(GUIComponent parent, string headerTag, string valueLabelTag, string tooltipTag, out GUIScrollBar slider, out GUITextBlock label, float? step = null, Vector2? range = null)
		{
			GUITextBlock guitextBlock;
			return NetLobbyScreen.CreateLabeledSlider(parent, headerTag, valueLabelTag, tooltipTag, out slider, out label, out guitextBlock, step, range);
		}

		// Token: 0x06002606 RID: 9734 RVA: 0x0018C7E0 File Offset: 0x0018A9E0
		public static GUIComponent CreateLabeledSlider(GUIComponent parent, string headerTag, string valueLabelTag, string tooltipTag, out GUIScrollBar slider, out GUITextBlock label, out GUITextBlock header, float? step = null, Vector2? range = null)
		{
			GUILayoutGroup verticalLayout = null;
			header = null;
			if (!headerTag.IsNullOrEmpty())
			{
				verticalLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true
				};
				header = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(headerTag), null, null, Alignment.CenterLeft, false, "", null)
				{
					CanBeFocused = false
				};
				header.RectTransform.MinSize = new Point(0, (int)header.TextSize.Y);
			}
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, (headerTag == null) ? 0f : 0.5f), (verticalLayout ?? parent).RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			new GUIFrame(new RectTransform(new Point(GUI.IntScale(5f), 0), container.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			slider = new GUIScrollBar(new RectTransform(new Vector2(0.5f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null);
			if (step != null)
			{
				slider.Step = step.Value;
			}
			if (range != null)
			{
				slider.Range = range.Value;
			}
			container.RectTransform.MinSize = new Point(0, slider.RectTransform.MinSize.Y);
			container.RectTransform.MaxSize = new Point(int.MaxValue, slider.RectTransform.MaxSize.Y);
			if (verticalLayout != null)
			{
				verticalLayout.InheritTotalChildrenMinHeight();
			}
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 1f), container.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			RichString text = string.IsNullOrEmpty(valueLabelTag) ? "" : TextManager.Get(valueLabelTag);
			GUIFont smallFont = GUIStyle.SmallFont;
			label = new GUITextBlock(rectT, text, null, smallFont, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			slider.UserData = label;
			slider.ToolTip = (label.ToolTip = TextManager.Get(tooltipTag));
			return verticalLayout ?? container;
		}

		// Token: 0x06002607 RID: 9735 RVA: 0x0018CB08 File Offset: 0x0018AD08
		public static GUINumberInput CreateLabeledNumberInput(GUIComponent parent, string labelTag, int min, int max, string toolTipTag = null, GUIFont font = null)
		{
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f,
				ToolTip = TextManager.Get(labelTag)
			};
			GUITextBlock label = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(labelTag), null, font, Alignment.CenterLeft, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			if (!string.IsNullOrEmpty(toolTipTag))
			{
				label.ToolTip = TextManager.Get(toolTipTag);
			}
			GUINumberInput input = new GUINumberInput(new RectTransform(new Vector2(0.3f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				MinValueInt = new int?(min),
				MaxValueInt = new int?(max)
			};
			container.RectTransform.MinSize = new Point(0, input.RectTransform.MinSize.Y);
			container.RectTransform.MaxSize = new Point(int.MaxValue, input.RectTransform.MaxSize.Y);
			return input;
		}

		// Token: 0x06002608 RID: 9736 RVA: 0x0018CCC4 File Offset: 0x0018AEC4
		public static GUIDropDown CreateLabeledDropdown(GUIComponent parent, string labelTag, int numElements, string toolTipTag = null)
		{
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f,
				ToolTip = TextManager.Get(labelTag)
			};
			GUITextBlock label = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(labelTag), null, null, Alignment.CenterLeft, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			if (!string.IsNullOrEmpty(toolTipTag))
			{
				label.ToolTip = TextManager.Get(toolTipTag);
			}
			GUIDropDown input = new GUIDropDown(new RectTransform(new Vector2(0.3f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, numElements, "", false, false, Alignment.CenterLeft, 1f);
			container.RectTransform.MinSize = new Point(0, input.RectTransform.MinSize.Y);
			container.RectTransform.MaxSize = new Point(int.MaxValue, input.RectTransform.MaxSize.Y);
			return input;
		}

		// Token: 0x06002609 RID: 9737 RVA: 0x0018CE54 File Offset: 0x0018B054
		private void CreateSidePanelContents(GUIComponent rightPanel)
		{
			GUIFrame myCharacterFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.55f), rightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup myCharacterContent = new GUILayoutGroup(new RectTransform(new Vector2(1f), myCharacterFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup checkBoxContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), myCharacterContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.spectateBox = new GUITickBox(new RectTransform(new Vector2(0.6f, 1f), checkBoxContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("spectatebutton"), null, "")
			{
				Selected = false,
				OnSelected = new GUITickBox.OnSelectedHandler(this.ToggleSpectate),
				UserData = "spectate"
			};
			this.afkBox = new GUITickBox(new RectTransform(new Vector2(0.4f, 1f), checkBoxContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("afkbutton"), null, "")
			{
				Selected = false,
				ToolTip = TextManager.Get("afkbutton.tooltip")
			};
			checkBoxContainer.RectTransform.MinSize = new Point(0, this.spectateBox.RectTransform.MinSize.Y);
			this.playerInfoContent = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), myCharacterContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIFrame logFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.45f), rightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup logContents = new GUILayoutGroup(new RectTransform(Vector2.One, logFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup socialHolder = null;
			GUILayoutGroup serverLogHolder = null;
			this.LogButtons = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), logContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			this.clientHiddenElements.Add(this.LogButtons);
			this.chatPanelTabButtons.Add(new GUIButton(new RectTransform(new Vector2(0.5f, 1.25f), this.LogButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Chat"), Alignment.Center, "GUITabButton", null)
			{
				Selected = true,
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (socialHolder != null)
					{
						socialHolder.Visible = true;
					}
					if (serverLogHolder != null)
					{
						serverLogHolder.Visible = false;
					}
					this.chatPanelTabButtons.ForEach(delegate(GUIButton otherBtn)
					{
						otherBtn.Selected = (otherBtn == button);
					});
					return true;
				}
			});
			this.chatPanelTabButtons.Add(new GUIButton(new RectTransform(new Vector2(0.5f, 1.25f), this.LogButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerLog"), Alignment.Center, "GUITabButton", null)
			{
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (socialHolder != null)
					{
						socialHolder.Visible = false;
					}
					if (serverLogHolder != null && !serverLogHolder.Visible)
					{
						GameClient client = GameMain.Client;
						bool flag;
						if (client == null)
						{
							flag = (null != null);
						}
						else
						{
							ServerSettings serverSettings = client.ServerSettings;
							flag = (((serverSettings != null) ? serverSettings.ServerLog : null) != null);
						}
						if (!flag)
						{
							return false;
						}
						serverLogHolder.Visible = true;
						GameMain.Client.ServerSettings.ServerLog.AssignLogFrame(this.serverLogReverseButton, this.serverLogBox, this.serverLogFilterTicks.Content, this.serverLogFilter);
					}
					this.chatPanelTabButtons.ForEach(delegate(GUIButton otherBtn)
					{
						otherBtn.Selected = (otherBtn == button);
					});
					return true;
				}
			});
			GUITextBlock.AutoScaleAndNormalize(from btn in this.chatPanelTabButtons
			select btn.TextBlock, true, false, null);
			GUIFrame logHolderBottom = new GUIFrame(new RectTransform(Vector2.One, logContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			socialHolder = new GUILayoutGroup(new RectTransform(Vector2.One, logHolderBottom.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), socialHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			GUILayoutGroup socialHolderHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), socialHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.chatBox = new GUIListBox(new RectTransform(new Vector2(0.6f, 1f), socialHolderHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			this.PlayerList = new GUIListBox(new RectTransform(new Vector2(0.4f, 1f), socialHolderHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent component, object userdata)
				{
					this.SelectPlayer(userdata as Client);
					return true;
				}
			};
			new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), socialHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			this.chatRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.07f), socialHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			this.RefreshChatrow();
			serverLogHolder = new GUILayoutGroup(new RectTransform(Vector2.One, logHolderBottom.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				Visible = false
			};
			new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), serverLogHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			GUILayoutGroup serverLogHolderHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), serverLogHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup serverLogListboxLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), serverLogHolderHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.serverLogReverseButton = new GUIButton(new RectTransform(new Vector2(1f, 0.05f), serverLogListboxLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "UIToggleButtonVertical", null);
			this.serverLogBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.95f), serverLogListboxLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false
			};
			GUIListBox guilistBox = new GUIListBox(new RectTransform(new Vector2(0.5f, 1f), serverLogHolderHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, false, null, "", true, false);
			guilistBox.OnSelected = ((GUIComponent component, object userdata) => false);
			this.serverLogFilterTicks = guilistBox;
			new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), serverLogHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			this.serverLogFilter = new GUITextBox(new RectTransform(new Vector2(1f, 0.07f), serverLogHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				MaxTextLength = new int?(200),
				Font = GUIStyle.SmallFont
			};
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x0018D8EC File Offset: 0x0018BAEC
		private void CreateBottomPanelContents(GUIComponent bottomBar)
		{
			GUILayoutGroup bottomBarLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), bottomBar.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft)
			{
				Stretch = true,
				IsHorizontal = true,
				RelativeSpacing = 0.005f
			};
			GUILayoutGroup bottomBarMid = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), bottomBar.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft)
			{
				Stretch = true,
				IsHorizontal = true,
				RelativeSpacing = 0.005f
			};
			GUILayoutGroup bottomBarRight = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), bottomBar.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft)
			{
				Stretch = true,
				IsHorizontal = true,
				RelativeSpacing = 0.005f
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), bottomBarLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("disconnect"), Alignment.Center, "", null);
			guibutton.OnClicked = delegate(GUIButton bt, object userdata)
			{
				GameMain.QuitToMainMenu(false, true);
				return true;
			};
			GUIButton disconnectButton = guibutton;
			disconnectButton.TextBlock.AutoScaleHorizontal = true;
			this.FileTransferFrame = new GUIFrame(new RectTransform(Vector2.One, bottomBarLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "TextFrame", null);
			GUILayoutGroup fileTransferContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), this.FileTransferFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.5f), fileTransferContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont smallFont = GUIStyle.SmallFont;
			this.FileTransferTitle = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
			GUILayoutGroup fileTransferBottom = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), fileTransferContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			this.FileTransferProgressBar = new GUIProgressBar(new RectTransform(new Vector2(0.6f, 1f), fileTransferBottom.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(Color.DarkGreen), "", true);
			RectTransform rectT2 = new RectTransform(Vector2.One, this.FileTransferProgressBar.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = "";
			smallFont = GUIStyle.SmallFont;
			this.FileTransferProgressText = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.CenterLeft, false, "", null);
			new GUIButton(new RectTransform(new Vector2(0.4f, 1f), fileTransferBottom.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				FileReceiver.FileTransferIn transfer = this.FileTransferFrame.UserData as FileReceiver.FileTransferIn;
				if (transfer == null)
				{
					return false;
				}
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.CancelFileTransfer(transfer);
				}
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.FileReceiver.StopTransfer(transfer, false);
				}
				return true;
			};
			this.roundControlsHolder = new GUILayoutGroup(new RectTransform(Vector2.One, bottomBarRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			GUIFrame readyToStartContainer = new GUIFrame(new RectTransform(Vector2.One, this.roundControlsHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "TextFrame", null)
			{
				Visible = false
			};
			this.ReadyToStartBox = new GUITickBox(new RectTransform(new Vector2(0.95f, 0.75f), readyToStartContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("ReadyToStartTickBox"), null, "");
			this.joinOnGoingRoundButton = new GUIButton(new RectTransform(Vector2.One, this.roundControlsHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListJoin"), Alignment.Center, "", null);
			GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, this.roundControlsHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("endround"), Alignment.Center, "", null);
			guibutton2.Color = GUIStyle.Red;
			guibutton2.OnClicked = delegate(GUIButton btn, object obj)
			{
				if (GameMain.Client == null)
				{
					return true;
				}
				GameSession gameSession = GameMain.GameSession;
				GUI.CreateVerificationPrompt((((gameSession != null) ? gameSession.GameMode : null) is CampaignMode) ? "PauseMenuReturnToServerLobbyVerification" : "EndRoundSubNotAtLevelEnd", delegate
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.RequestEndRound(false, false);
				});
				return true;
			};
			guibutton2.Visible = false;
			guibutton2.IgnoreLayoutGroups = true;
			this.EndButton = guibutton2;
			this.StartButton = new GUIButton(new RectTransform(Vector2.One, this.roundControlsHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("StartGameButton"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object obj)
				{
					if (GameMain.Client == null)
					{
						return true;
					}
					this.SetAFKSelected(false);
					if (this.CampaignSetupFrame.Visible && this.CampaignSetupUI != null)
					{
						this.CampaignSetupUI.StartGameClicked(btn, obj);
					}
					else
					{
						GameClient client = GameMain.Client;
						GameSession gameSession = GameMain.GameSession;
						bool continueCampaign;
						if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode)
						{
							GUIFrame campaignSetupFrame = this.CampaignSetupFrame;
							continueCampaign = (campaignSetupFrame == null || !campaignSetupFrame.Visible);
						}
						else
						{
							continueCampaign = false;
						}
						client.RequestStartRound(continueCampaign);
						CoroutineManager.StartCoroutine(NetLobbyScreen.WaitForStartRound(this.StartButton), "WaitForStartRound");
					}
					return true;
				}
			};
			this.clientHiddenElements.Add(this.StartButton);
			bottomBar.RectTransform.MinSize = new Point(0, (int)Math.Max((float)this.ReadyToStartBox.RectTransform.MinSize.Y / 0.75f, (float)this.StartButton.RectTransform.MinSize.Y));
			RectTransform rectT3 = new RectTransform(Vector2.One, bottomBarMid.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = "";
			smallFont = GUIStyle.SmallFont;
			this.autoRestartText = new GUITextBlock(rectT3, text3, null, smallFont, Alignment.Center, false, "TextFrame", null);
			GUIFrame autoRestartBoxContainer = new GUIFrame(new RectTransform(Vector2.One, bottomBarMid.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "TextFrame", null);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.95f, 0.75f), autoRestartBoxContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("AutoRestart"), null, "");
			guitickBox.OnSelected = delegate(GUITickBox tickBox)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
				return true;
			};
			this.autoRestartBox = guitickBox;
			this.clientDisabledElements.Add(this.autoRestartBox);
			this.AssignComponentToServerSetting(this.autoRestartBox, "AutoRestart");
		}

		// Token: 0x0600260B RID: 9739 RVA: 0x0018E128 File Offset: 0x0018C328
		public void StopWaitingForStartRound()
		{
			CoroutineManager.StopCoroutines("WaitForStartRound");
			if (this.StartButton != null)
			{
				this.StartButton.Enabled = true;
			}
			GUI.ClearCursorWait();
		}

		// Token: 0x0600260C RID: 9740 RVA: 0x0018E14D File Offset: 0x0018C34D
		public static IEnumerable<CoroutineStatus> WaitForStartRound(GUIButton startButton)
		{
			NetLobbyScreen.<WaitForStartRound>d__265 <WaitForStartRound>d__ = new NetLobbyScreen.<WaitForStartRound>d__265(-2);
			<WaitForStartRound>d__.<>3__startButton = startButton;
			return <WaitForStartRound>d__;
		}

		// Token: 0x0600260D RID: 9741 RVA: 0x0018E160 File Offset: 0x0018C360
		public override void Deselect()
		{
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.OnPermissionChanged.TryDeregister("CreateDisembarkPointPanel".ToIdentifier());
			}
			this.SaveAppearance();
			this.chatInput.Deselect();
			this.CampaignCharacterDiscarded = false;
			CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = this.CharacterAppearanceCustomizationMenu;
			if (characterAppearanceCustomizationMenu != null)
			{
				characterAppearanceCustomizationMenu.Dispose();
			}
			this.JobSelectionFrame = null;
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x0018E1C0 File Offset: 0x0018C3C0
		public override void Select()
		{
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			this.visibilityMenuOrder.Clear();
			CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = this.CharacterAppearanceCustomizationMenu;
			if (characterAppearanceCustomizationMenu != null)
			{
				characterAppearanceCustomizationMenu.Dispose();
			}
			this.JobSelectionFrame = null;
			Character.Controlled = null;
			GameMain.LightManager.LosEnabled = false;
			GUI.PreventPauseMenuToggle = false;
			this.CampaignCharacterDiscarded = false;
			GUIComponent guicomponent = this.changesPendingText;
			if (guicomponent != null)
			{
				GUIComponent parent = guicomponent.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.changesPendingText);
				}
			}
			this.changesPendingText = null;
			this.RefreshChatrow();
			this.clientDisabledElements.ForEach(delegate(GUIComponent c)
			{
				c.Enabled = false;
			});
			this.clientHiddenElements.ForEach(delegate(GUIComponent c)
			{
				c.Visible = false;
			});
			this.RefreshEnabledElements();
			this.createPendingChangesText = false;
			TabMenu.PendingChanges = false;
			if (GameMain.Client != null)
			{
				this.joinOnGoingRoundButton.Visible = GameMain.Client.GameStarted;
				this.ReadyToStartBox.Selected = false;
				GameMain.Client.SetReadyToStart(this.ReadyToStartBox);
			}
			else
			{
				this.joinOnGoingRoundButton.Visible = false;
			}
			this.SetSpectate(this.spectateBox.Selected);
			if (GameMain.Client != null)
			{
				this.afkBox.Visible = (GameMain.Client.IsServerOwner || GameMain.Client.ServerSettings.AllowAFK);
				GameMain.Client.Voting.ResetVotes(GameMain.Client.ConnectedClients);
				this.joinOnGoingRoundButton.OnClicked = delegate(GUIButton btn, object userdata)
				{
					GUITickBox guitickBox = this.afkBox;
					if (guitickBox != null && guitickBox.Selected)
					{
						this.afkBox.Selected = false;
						this.afkBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
					}
					GameMain.Client.SendJoinOngoingRequest(btn);
					return true;
				};
				this.ReadyToStartBox.OnSelected = new GUITickBox.OnSelectedHandler(GameMain.Client.SetReadyToStart);
			}
			this.roundControlsHolder.Children.ForEach(delegate(GUIComponent c)
			{
				c.IgnoreLayoutGroups = !c.Visible;
			});
			this.roundControlsHolder.Recalculate();
			this.AssignComponentsToServerSettings();
			this.RefreshPlaystyleIcons();
			base.Select();
		}

		// Token: 0x0600260F RID: 9743 RVA: 0x0018E3CC File Offset: 0x0018C5CC
		public void RefreshEnabledElements()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			GameClient client = GameMain.Client;
			ServerSettings settings = client.ServerSettings;
			bool manageSettings = NetLobbyScreen.<RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions.ManageSettings);
			bool campaignSelected = this.CampaignFrame.Visible || this.CampaignSetupFrame.Visible;
			bool campaignStarted = this.CampaignFrame.Visible;
			bool gameStarted = client != null && client.GameStarted;
			foreach (GUIComponent element in this.clientDisabledElements)
			{
				element.Enabled = manageSettings;
			}
			this.traitorElements.ForEach(delegate(GUIComponent e)
			{
				e.Enabled &= (settings.TraitorProbability > 0f);
			});
			this.SetTraitorDangerIndicators(settings.TraitorDangerLevel);
			this.respawnModeSelection.Enabled = (this.respawnModeLabel.Enabled = (manageSettings && !gameStarted));
			this.midRoundRespawnSettings.ForEach(delegate(GUIComponent e)
			{
				e.Enabled &= (settings.RespawnMode != RespawnMode.BetweenRounds);
			});
			this.permadeathDisabledRespawnSettings.ForEach(delegate(GUIComponent e)
			{
				e.Enabled &= (settings.RespawnMode != RespawnMode.Permadeath);
			});
			this.permadeathEnabledRespawnSettings.ForEach(delegate(GUIComponent e)
			{
				e.Enabled &= (settings.RespawnMode == RespawnMode.Permadeath && !gameStarted);
			});
			this.ironmanDisabledRespawnSettings.ForEach(delegate(GUIComponent e)
			{
				e.Enabled &= !settings.IronmanMode;
			});
			this.respawnIntervalElement.GetAllChildren().ForEach(delegate(GUIComponent e)
			{
				e.Enabled = (settings.RespawnMode != RespawnMode.BetweenRounds & manageSettings);
			});
			this.shuttleTickBox.Enabled &= !gameStarted;
			if (this.ShuttleList != null)
			{
				this.ShuttleList.Enabled &= (this.shuttleTickBox.Enabled && NetLobbyScreen.<RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions.SelectSub));
				this.ShuttleList.ButtonEnabled = this.ShuttleList.Enabled;
			}
			if (this.SubList != null)
			{
				this.SubList.Enabled = (!campaignStarted && (settings.AllowSubVoting || NetLobbyScreen.<RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions.SelectSub)));
			}
			if (this.ModeList != null)
			{
				this.ModeList.Enabled = (!gameStarted && (settings.AllowModeVoting || NetLobbyScreen.<RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions.SelectMode)));
			}
			this.RefreshStartButtonVisibility();
			this.botSettingsElements.ForEach(delegate(GUIComponent b)
			{
				b.Enabled = (!campaignStarted & manageSettings);
			});
			this.campaignDisabledElements.ForEach(delegate(GUIComponent e)
			{
				e.Enabled = (!campaignSelected & manageSettings);
			});
			this.levelDifficultySlider.ToolTip = (this.levelDifficultySlider.Enabled ? string.Empty : TextManager.Get("campaigndifficultydisabled"));
			foreach (GUIComponent element2 in this.clientHiddenElements)
			{
				element2.Visible = manageSettings;
			}
			this.ReadyToStartBox.Parent.Visible = !gameStarted;
			this.LogButtons.Visible = NetLobbyScreen.<RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions.ServerLog);
			if (client != null)
			{
				client.UpdateLogButtonPermissions();
			}
			this.roundControlsHolder.Children.ForEach(delegate(GUIComponent c)
			{
				c.IgnoreLayoutGroups = !c.Visible;
			});
			this.roundControlsHolder.Children.ForEach(delegate(GUIComponent c)
			{
				c.RectTransform.RelativeSize = Vector2.One;
			});
			this.roundControlsHolder.Recalculate();
			this.SettingsButton.OnClicked = new GUIButton.OnClickedHandler(settings.ToggleSettingsFrame);
			this.RefreshGameModeContent();
		}

		// Token: 0x06002610 RID: 9744 RVA: 0x0018E7A8 File Offset: 0x0018C9A8
		public void ShowSpectateButton()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			this.joinOnGoingRoundButton.Visible = true;
			this.joinOnGoingRoundButton.Enabled = true;
			this.StartButton.Visible = false;
		}

		// Token: 0x06002611 RID: 9745 RVA: 0x0018E7D8 File Offset: 0x0018C9D8
		public void SetCampaignCharacterInfo(CharacterInfo newCampaignCharacterInfo)
		{
			if (newCampaignCharacterInfo != null)
			{
				if (this.CampaignCharacterDiscarded)
				{
					return;
				}
				if (this.campaignCharacterInfo != newCampaignCharacterInfo)
				{
					this.campaignCharacterInfo = newCampaignCharacterInfo;
					this.SaveAppearance();
					this.UpdatePlayerFrame(this.campaignCharacterInfo, false);
					return;
				}
			}
			else if (this.campaignCharacterInfo != null)
			{
				this.campaignCharacterInfo = null;
				this.UpdatePlayerFrame(null, false);
			}
		}

		// Token: 0x06002612 RID: 9746 RVA: 0x0018E82D File Offset: 0x0018CA2D
		private void UpdatePlayerFrame(CharacterInfo characterInfo, bool allowEditing = true)
		{
			this.UpdatePlayerFrame(characterInfo, allowEditing, this.playerInfoContent, false);
		}

		// Token: 0x06002613 RID: 9747 RVA: 0x0018E840 File Offset: 0x0018CA40
		public void CreatePlayerFrame(GUIComponent parent, bool createPendingText = true, bool alwaysAllowEditing = false)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			Character controlled = Character.Controlled;
			CharacterInfo characterInfo;
			if ((characterInfo = ((controlled != null) ? controlled.Info : null)) == null)
			{
				characterInfo = ((this.playerInfoContent.UserData as CharacterInfo) ?? GameMain.Client.CharacterInfo);
			}
			this.UpdatePlayerFrame(characterInfo, alwaysAllowEditing || this.campaignCharacterInfo == null, parent, createPendingText);
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x0018E8A0 File Offset: 0x0018CAA0
		private void UpdatePlayerFrame(CharacterInfo characterInfo, bool allowEditing, GUIComponent parent, bool createPendingText = false)
		{
			NetLobbyScreen.<>c__DisplayClass273_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass273_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.createPendingText = createPendingText;
			CS$<>8__locals1.parent = parent;
			if (GameMain.Client == null)
			{
				return;
			}
			this.spectateBox.Enabled = !this.PermanentlyDead;
			this.createPendingChangesText = CS$<>8__locals1.createPendingText;
			if (characterInfo == null || this.CampaignCharacterDiscarded)
			{
				characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, GameMain.Client.Name, null, null, 0, Rand.RandSync.Unsynced, default(Identifier));
				characterInfo.RecreateHead(MultiplayerPreferences.Instance);
				GameMain.Client.CharacterInfo = characterInfo;
				characterInfo.OmitJobInMenus = true;
			}
			CS$<>8__locals1.parent.ClearChildren();
			NetLobbyScreen.<>c__DisplayClass273_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameSession gameSession = GameMain.GameSession;
			CS$<>8__locals2.isGameRunning = (gameSession != null && gameSession.IsRunning);
			CS$<>8__locals1.parent.ClearChildren();
			CS$<>8__locals1.parent.UserData = characterInfo;
			bool flag;
			if (CS$<>8__locals1.isGameRunning && GameMain.Client.PendingName != string.Empty)
			{
				GameClient client = GameMain.Client;
				string a;
				if (client == null)
				{
					a = null;
				}
				else
				{
					Character character = client.Character;
					a = ((character != null) ? character.Name : null);
				}
				flag = (a != GameMain.Client.PendingName);
			}
			else
			{
				flag = false;
			}
			bool nameChangePending = flag;
			GUIComponent guicomponent = this.changesPendingText;
			if (guicomponent != null)
			{
				GUIComponent parent2 = guicomponent.Parent;
				if (parent2 != null)
				{
					parent2.RemoveChild(this.changesPendingText);
				}
			}
			this.changesPendingText = null;
			if (TabMenu.PendingChanges)
			{
				this.CreateChangesPendingText();
			}
			this.CharacterNameBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.065f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), (!nameChangePending) ? characterInfo.Name : GameMain.Client.PendingName, null, null, Alignment.Center, false, "", null, false, true)
			{
				MaxTextLength = new int?(32),
				OverflowClip = true
			};
			if (!allowEditing || (this.PermanentlyDead && !characterInfo.RenamingEnabled))
			{
				this.CharacterNameBox.Readonly = true;
				this.CharacterNameBox.Enabled = false;
			}
			else
			{
				GUITextBox characterNameBox = this.CharacterNameBox;
				characterNameBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(characterNameBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox tb, string text)
				{
					CS$<>8__locals1.<>4__this.CharacterNameBox.Deselect();
					return true;
				}));
				this.CharacterNameBox.OnDeselected += delegate(GUITextBox tb, Keys key)
				{
					if (GameMain.Client == null)
					{
						return;
					}
					string newName = Client.SanitizeName(tb.Text, 32);
					if (newName == GameMain.Client.Name)
					{
						return;
					}
					if (string.IsNullOrWhiteSpace(newName))
					{
						tb.Text = GameMain.Client.Name;
						return;
					}
					if (CS$<>8__locals1.isGameRunning)
					{
						GameMain.Client.PendingName = tb.Text;
						TabMenu.PendingChanges = true;
						if (CS$<>8__locals1.createPendingText)
						{
							CS$<>8__locals1.<>4__this.CreateChangesPendingText();
						}
					}
					else
					{
						CS$<>8__locals1.<>4__this.ReadyToStartBox.Selected = false;
					}
					GameMain.Client.SetName(tb.Text);
				};
			}
			new GUIFrame(new RectTransform(new Vector2(1f, 0.006f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (allowEditing && (!this.PermadeathMode || !CS$<>8__locals1.isGameRunning))
			{
				GUILayoutGroup characterInfoTabs = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.07f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				this.jobPreferencesButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), characterInfoTabs.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("JobPreferences"), Alignment.Center, "GUITabButton", null)
				{
					Selected = true,
					OnClicked = new GUIButton.OnClickedHandler(this.SelectJobPreferencesTab)
				};
				this.appearanceButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), characterInfoTabs.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("CharacterAppearance"), Alignment.Center, "GUITabButton", null)
				{
					OnClicked = new GUIButton.OnClickedHandler(this.SelectAppearanceTab)
				};
				GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
				{
					this.jobPreferencesButton.TextBlock,
					this.appearanceButton.TextBlock
				});
				if (this.characterInfoFrame != null)
				{
					this.characterInfoFrame.RectTransform.SizeChanged -= this.RecalculateSubDescription;
				}
				this.characterInfoFrame = new GUIFrame(new RectTransform(Vector2.One, CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				this.characterInfoFrame.RectTransform.SizeChanged += this.RecalculateSubDescription;
				this.JobPreferenceContainer = new GUIFrame(new RectTransform(Vector2.One, this.characterInfoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
				characterInfo.CreateIcon(new RectTransform(new Vector2(1f, 0.4f), this.JobPreferenceContainer.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, 0.025f)
				});
				this.JobList = new GUIListBox(new RectTransform(new Vector2(1f, 0.6f), this.JobPreferenceContainer.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), true, null, "", true, false)
				{
					Enabled = true,
					PlaySoundOnSelect = true,
					OnSelected = ((GUIComponent child, object obj) => !child.IsParentOf(GUI.MouseOn, true) && CS$<>8__locals1.<>4__this.OpenJobSelection(child, obj))
				};
				for (int i = 0; i < 3; i++)
				{
					JobVariant jobPrefab = null;
					while (i < MultiplayerPreferences.Instance.JobPreferences.Count)
					{
						MultiplayerPreferences.JobPreference jobPreference = MultiplayerPreferences.Instance.JobPreferences[i];
						JobPrefab prefab;
						if (JobPrefab.Prefabs.TryGet(jobPreference.JobIdentifier, out prefab) && !prefab.HiddenJob)
						{
							int variant = Math.Min(jobPreference.Variant, prefab.Variants - 1);
							jobPrefab = new JobVariant(prefab, variant);
							break;
						}
						MultiplayerPreferences.Instance.JobPreferences.RemoveAt(i);
					}
					GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(0.333f, 1f), this.JobList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElementSquare", null);
					guiframe.CanBeFocused = true;
					guiframe.UserData = jobPrefab;
				}
				this.UpdateJobPreferences(characterInfo);
				this.appearanceFrame = new GUIFrame(new RectTransform(Vector2.One, this.characterInfoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null)
				{
					Visible = false,
					Color = Color.White
				};
			}
			else
			{
				characterInfo.CreateIcon(new RectTransform(new Vector2(1f, 0.16f), CS$<>8__locals1.parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal));
				if (this.PermanentlyDead)
				{
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text4 = TextManager.Get("deceased");
					GUIFont font = GUIStyle.LargeFont;
					new GUITextBlock(rectT, text4, null, font, Alignment.Center, false, "", null);
					GameClient client2 = GameMain.Client;
					ServerSettings serverSettings = (client2 != null) ? client2.ServerSettings : null;
					if (serverSettings != null && serverSettings.IronmanModeActive)
					{
						new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("lobby.ironmaninfo"), null, null, Alignment.Center, true, "", null);
					}
					else
					{
						new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("lobby.permadeathinfo"), null, null, Alignment.Center, true, "", null);
						new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("lobby.permadeathoptionsexplanation"), null, null, Alignment.Center, true, "", null);
					}
				}
				else
				{
					RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = characterInfo.Job.Name;
					GUIFont font = GUIStyle.SubHeadingFont;
					GUITextBlock guitextBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.Center, true, "", null);
					guitextBlock.HoverColor = Color.Transparent;
					guitextBlock.SelectedColor = Color.Transparent;
					RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get("Skills");
					font = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
					foreach (Skill skill in characterInfo.Job.GetSkills())
					{
						Color textColor = Color.White * (0.5f + skill.Level / 200f);
						GUITextBlock skillText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "  - " + TextManager.AddPunctuation(':', new LocalizedString[]
						{
							TextManager.Get("SkillName." + skill.Identifier.ToString()),
							((int)skill.Level).ToString()
						}), new Color?(textColor), GUIStyle.SmallFont, Alignment.Left, false, "", null);
					}
				}
				new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				GameClient client3 = GameMain.Client;
				bool flag2;
				if (client3 == null)
				{
					flag2 = true;
				}
				else
				{
					ServerSettings serverSettings2 = client3.ServerSettings;
					flag2 = (((serverSettings2 != null) ? new RespawnMode?(serverSettings2.RespawnMode) : null).GetValueOrDefault() != RespawnMode.Permadeath);
				}
				if (flag2)
				{
					GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.8f, 0.1f), CS$<>8__locals1.parent.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("CreateNew"), Alignment.Center, "", null);
					guibutton.IgnoreLayoutGroups = true;
					guibutton.OnClicked = delegate(GUIButton btn, object userdata)
					{
						NetLobbyScreen <>4__this = CS$<>8__locals1.<>4__this;
						Action onYes;
						if ((onYes = CS$<>8__locals1.<>9__4) == null)
						{
							onYes = (CS$<>8__locals1.<>9__4 = delegate()
							{
								CS$<>8__locals1.<>4__this.UpdatePlayerFrame(null, true, CS$<>8__locals1.parent, false);
							});
						}
						<>4__this.TryDiscardCampaignCharacter(onYes);
						return true;
					};
				}
			}
			this.TeamPreferenceListBox = null;
			if (this.SelectedMode == GameModePreset.PvP)
			{
				this.TeamPreferenceListBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.04f), CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), true, null, null, true, false)
				{
					Enabled = true,
					KeepSpaceForScrollBar = false,
					PlaySoundOnSelect = true,
					ScrollBarEnabled = false,
					ScrollBarVisible = false
				};
				this.TeamPreferenceListBox.RectTransform.MinSize = new Point(0, GUI.IntScale(30f));
				this.TeamPreferenceListBox.UpdateDimensions();
				Color team1Color = new Color(0, 110, 150, 255);
				this.pvpTeamChoiceTeam1 = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), this.TeamPreferenceListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("teampreference.team1"), null, null, Alignment.Center, false, null, null)
				{
					UserData = CharacterTeamType.Team1,
					CanBeFocused = true,
					Padding = Vector4.One * 10f * GUI.Scale,
					Color = Color.Lerp(team1Color, Color.Black, 0.7f) * 0.7f,
					HoverColor = team1Color * 0.95f,
					SelectedColor = team1Color * 0.8f,
					OutlineColor = team1Color,
					TextColor = Color.White,
					HoverTextColor = Color.White,
					SelectedTextColor = Color.White,
					DisabledColor = team1Color * 0.25f,
					DisabledTextColor = Color.Gray
				};
				Color noPreferenceColor = new Color(100, 100, 100, 255);
				this.pvpTeamChoiceMiddleButton = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), this.TeamPreferenceListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Center, false, null, null)
				{
					UserData = CharacterTeamType.None,
					CanBeFocused = true,
					Padding = Vector4.One * 10f * GUI.Scale,
					Color = Color.Lerp(noPreferenceColor, Color.Black, 0.7f) * 0.7f,
					HoverColor = noPreferenceColor * 0.95f,
					SelectedColor = noPreferenceColor * 0.8f,
					OutlineColor = noPreferenceColor,
					TextColor = Color.White,
					HoverTextColor = Color.White,
					SelectedTextColor = Color.White
				};
				Color team2Color = new Color(150, 110, 0, 255);
				this.pvpTeamChoiceTeam2 = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), this.TeamPreferenceListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("teampreference.team2"), null, null, Alignment.Center, false, null, null)
				{
					UserData = CharacterTeamType.Team2,
					CanBeFocused = true,
					Padding = Vector4.One * 10f * GUI.Scale,
					Color = Color.Lerp(team2Color, Color.Black, 0.7f) * 0.7f,
					HoverColor = team2Color * 0.95f,
					SelectedColor = team2Color * 0.8f,
					OutlineColor = team2Color,
					TextColor = Color.White,
					HoverTextColor = Color.White,
					SelectedTextColor = Color.White,
					DisabledColor = team2Color * 0.25f,
					DisabledTextColor = Color.Gray
				};
				CharacterTeamType prevTeamSelection = MultiplayerPreferences.Instance.TeamPreference;
				this.ResetPvpTeamSelection();
				GUIListBox teamPreferenceListBox = this.TeamPreferenceListBox;
				teamPreferenceListBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(teamPreferenceListBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
				{
					if ((CharacterTeamType)obj == CharacterTeamType.None)
					{
						GameClient client4 = GameMain.Client;
						bool flag3;
						if (client4 == null)
						{
							flag3 = false;
						}
						else
						{
							ServerSettings serverSettings3 = client4.ServerSettings;
							flag3 = (((serverSettings3 != null) ? new PvpTeamSelectionMode?(serverSettings3.PvpTeamSelectionMode) : null).GetValueOrDefault() == PvpTeamSelectionMode.PlayerChoice);
						}
						if (flag3)
						{
							CS$<>8__locals1.<>4__this.TeamPreferenceListBox.Select(((double)Rand.Value(Rand.RandSync.Unsynced) < 0.5) ? CharacterTeamType.Team1 : CharacterTeamType.Team2, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
							Color teamColor = ((CharacterTeamType)CS$<>8__locals1.<>4__this.TeamPreferenceListBox.SelectedData == CharacterTeamType.Team1) ? team1Color : team2Color;
							CS$<>8__locals1.<>4__this.TeamPreferenceListBox.SelectedComponent.Flash(new Color?(teamColor), 1f, true, false, null);
							return true;
						}
					}
					return false;
				}));
				GUIListBox teamPreferenceListBox2 = this.TeamPreferenceListBox;
				teamPreferenceListBox2.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(teamPreferenceListBox2.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
				{
					CharacterTeamType newTeamPreference = (CharacterTeamType)obj;
					if (newTeamPreference == CharacterTeamType.None)
					{
						GameClient client4 = GameMain.Client;
						bool flag3;
						if (client4 == null)
						{
							flag3 = false;
						}
						else
						{
							ServerSettings serverSettings3 = client4.ServerSettings;
							flag3 = (((serverSettings3 != null) ? new PvpTeamSelectionMode?(serverSettings3.PvpTeamSelectionMode) : null).GetValueOrDefault() == PvpTeamSelectionMode.PlayerChoice);
						}
						if (flag3)
						{
							return false;
						}
					}
					CharacterTeamType oldPreference = MultiplayerPreferences.Instance.TeamPreference;
					MultiplayerPreferences.Instance.TeamPreference = newTeamPreference;
					CS$<>8__locals1.<>4__this.UpdateSelectedSub(newTeamPreference);
					if (newTeamPreference != oldPreference)
					{
						GameClient client5 = GameMain.Client;
						if (client5 != null)
						{
							client5.ForceNameJobTeamUpdate();
						}
						GameSettings.SaveCurrentConfig();
					}
					CS$<>8__locals1.<>4__this.RefreshPvpTeamSelectionButtons();
					CS$<>8__locals1.<>4__this.UpdateDisembarkPointListFromServerSettings();
					NetLobbyScreen <>4__this = CS$<>8__locals1.<>4__this;
					GameClient client6 = GameMain.Client;
					CharacterInfo characterInfo2;
					if ((characterInfo2 = ((client6 != null) ? client6.CharacterInfo : null)) == null)
					{
						Character controlled = Character.Controlled;
						characterInfo2 = ((controlled != null) ? controlled.Info : null);
					}
					<>4__this.UpdateJobPreferences(characterInfo2);
					CS$<>8__locals1.<>4__this.JobSelectionFrame = null;
					CS$<>8__locals1.<>4__this.RefreshChatrow();
					return true;
				}));
				if (prevTeamSelection != CharacterTeamType.None)
				{
					this.TeamPreferenceListBox.Select(prevTeamSelection, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
				}
			}
		}

		// Token: 0x06002615 RID: 9749 RVA: 0x0018FA88 File Offset: 0x0018DC88
		public void UpdateSelectedSub(CharacterTeamType preference)
		{
			bool votingEnabled = GameMain.NetworkMember.ServerSettings.SubSelectionMode == SelectionMode.Vote;
			GUIListBox subList = this.SubList;
			subList.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Remove(subList.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
			if (preference > CharacterTeamType.Team1)
			{
				if (preference == CharacterTeamType.Team2)
				{
					SubmarineInfo selectedEnemySub = this.SelectedEnemySub;
					if (selectedEnemySub != null)
					{
						this.TrySelectSub(selectedEnemySub.Name, selectedEnemySub.MD5Hash.StringRepresentation, SelectedSubType.EnemySub, this.SubList, false);
						if (!votingEnabled)
						{
							this.SubList.Select(selectedEnemySub, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
						}
					}
				}
			}
			else
			{
				SubmarineInfo selectedSub = this.SelectedSub;
				if (selectedSub != null)
				{
					this.TrySelectSub(selectedSub.Name, selectedSub.MD5Hash.StringRepresentation, SelectedSubType.Sub, this.SubList, false);
					if (!votingEnabled)
					{
						this.SubList.Select(selectedSub, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
					}
				}
			}
			GUIListBox subList2 = this.SubList;
			subList2.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(subList2.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x0018FB7C File Offset: 0x0018DD7C
		public void TryDiscardCampaignCharacter(Action onYes)
		{
			GUIMessageBox confirmation = new GUIMessageBox(TextManager.Get("NewCampaignCharacterHeader"), TextManager.Get("NewCampaignCharacterText"), new LocalizedString[]
			{
				TextManager.Get("Yes"),
				TextManager.Get("No")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUIButton guibutton = confirmation.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(confirmation.Close));
			GUIButton guibutton2 = confirmation.Buttons[0];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn2, object userdata2)
			{
				this.CampaignCharacterDiscarded = true;
				this.campaignCharacterInfo = null;
				onYes();
				return true;
			}));
			GUIButton guibutton3 = confirmation.Buttons[1];
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(confirmation.Close));
		}

		// Token: 0x06002617 RID: 9751 RVA: 0x0018FC8C File Offset: 0x0018DE8C
		private void CreateChangesPendingText()
		{
			if (!this.createPendingChangesText || this.changesPendingText != null || this.playerInfoContent == null)
			{
				return;
			}
			GUIComponent guicomponent = this.changesPendingText;
			if (guicomponent != null)
			{
				GUIComponent parent = guicomponent.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.changesPendingText);
				}
			}
			this.changesPendingText = new GUIFrame(new RectTransform(new Vector2(1f, 0.065f), this.playerInfoContent.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, -0.03f)
			}, "OuterGlow", null)
			{
				Color = Color.Black,
				IgnoreLayoutGroups = true
			};
			GUITextBlock text = new GUITextBlock(new RectTransform(Vector2.One, this.changesPendingText.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("tabmenu.characterchangespending"), new Color?(GUIStyle.Orange), null, Alignment.Center, false, null, null);
			this.changesPendingText.RectTransform.MinSize = new Point((int)(text.TextSize.X * 1.2f), (int)(text.TextSize.Y * 2f));
		}

		// Token: 0x06002618 RID: 9752 RVA: 0x0018FDEC File Offset: 0x0018DFEC
		public static void CreateChangesPendingFrame(GUIComponent parent)
		{
			parent.ClearChildren();
			GUIFrame changesPendingFrame = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlow", null)
			{
				Color = Color.Black
			};
			new GUITextBlock(new RectTransform(Vector2.One, changesPendingFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("tabmenu.characterchangespending"), new Color?(GUIStyle.Orange), null, Alignment.Center, false, null, null).AutoScaleHorizontal = true;
		}

		// Token: 0x06002619 RID: 9753 RVA: 0x0018FEB0 File Offset: 0x0018E0B0
		private static void CreateJobVariantTooltip(JobPrefab jobPrefab, CharacterTeamType team, int variant, bool isPvPMode, GUIComponent parentSlot)
		{
			NetLobbyScreen.jobVariantTooltip = new GUIFrame(new RectTransform(new Point((int)(400f * GUI.Scale), (int)(180f * GUI.Scale)), GUI.Canvas, Anchor.TopLeft, new Pivot?(Pivot.BottomRight), ScaleBasis.Normal, false), "GUIToolTip", null)
			{
				UserData = new JobVariant(jobPrefab, variant)
			};
			NetLobbyScreen.jobVariantTooltip.RectTransform.AbsoluteOffset = new Point(parentSlot.Rect.Right, parentSlot.Rect.Y);
			if (NetLobbyScreen.jobVariantTooltip.Rect.X < 0)
			{
				NetLobbyScreen.jobVariantTooltip.RectTransform.SetPosition(Anchor.TopLeft, new Pivot?(Pivot.BottomLeft));
				NetLobbyScreen.jobVariantTooltip.RectTransform.AbsoluteOffset = new Point(parentSlot.Rect.X, parentSlot.Rect.Y);
			}
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), NetLobbyScreen.jobVariantTooltip.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = (int)(15f * GUI.Scale)
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.GetWithVariable("startingequipmentname", "[number]", (variant + 1).ToString(), FormatCapitals.No);
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null);
			IEnumerable<Identifier> itemIdentifiers = (from it in jobPrefab.GetJobItems(variant, (JobPrefab.JobItem it) => it.ShowPreview)
			select it.GetItemIdentifier(team, isPvPMode)).Distinct<Identifier>();
			int itemsPerRow = 5;
			int rows = (int)Math.Max(Math.Ceiling((double)((float)itemIdentifiers.Count<Identifier>() / (float)itemsPerRow)), 1.0);
			new GUICustomComponent(new RectTransform(new Vector2(1f, 0.4f * (float)rows), content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				NetLobbyScreen.DrawJobVariantItems(sb, component, new JobVariant(jobPrefab, variant), team, isPvPMode, itemsPerRow);
			}, null);
			NetLobbyScreen.jobVariantTooltip.RectTransform.MinSize = new Point(0, content.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height + content.AbsoluteSpacing));
		}

		// Token: 0x0600261A RID: 9754 RVA: 0x001901B8 File Offset: 0x0018E3B8
		private void SetTraitorDangerIndicators(int dangerLevel)
		{
			int i = 0;
			foreach (GUIComponent child in this.traitorDangerGroup.Children)
			{
				GUIComponent guicomponent = child;
				bool enabled;
				if (i < dangerLevel)
				{
					GameClient client = GameMain.Client;
					ServerSettings serverSettings = (client != null) ? client.ServerSettings : null;
					enabled = (serverSettings != null && serverSettings.TraitorProbability > 0f);
				}
				else
				{
					enabled = false;
				}
				guicomponent.Enabled = enabled;
				i++;
			}
		}

		// Token: 0x0600261B RID: 9755 RVA: 0x0019023C File Offset: 0x0018E43C
		public bool ToggleSpectate(GUITickBox tickBox)
		{
			this.SetSpectate(tickBox.Selected);
			return false;
		}

		// Token: 0x0600261C RID: 9756 RVA: 0x0019024C File Offset: 0x0018E44C
		public void SetSpectate(bool spectate)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			this.spectateBox.Selected = spectate;
			if (spectate)
			{
				CharacterInfo characterInfo = GameMain.Client.CharacterInfo;
				if (characterInfo != null)
				{
					characterInfo.Remove();
				}
				GameMain.Client.CharacterInfo = null;
				this.playerInfoContent.ClearChildren();
				new GUITextBlock(new RectTransform(Vector2.One, this.playerInfoContent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("PlayingAsSpectator"), null, null, Alignment.Center, false, "", null);
				if (this.SelectedMode == GameModePreset.PvP)
				{
					this.ResetPvpTeamSelection();
					return;
				}
			}
			else
			{
				this.UpdatePlayerFrame(this.campaignCharacterInfo, this.campaignCharacterInfo == null);
			}
		}

		// Token: 0x0600261D RID: 9757 RVA: 0x0019032C File Offset: 0x0018E52C
		public void RefreshPvpTeamSelectionButtons()
		{
			NetLobbyScreen.<>c__DisplayClass282_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.pvpTeamChoiceMiddleButton == null || this.pvpTeamChoiceTeam1 == null || this.pvpTeamChoiceTeam2 == null)
			{
				return;
			}
			CS$<>8__locals1.serverSettings = GameMain.Client.ServerSettings;
			CS$<>8__locals1.currentTeam = MultiplayerPreferences.Instance.TeamPreference;
			bool pvpPlayerChoiceMode = CS$<>8__locals1.serverSettings.PvpTeamSelectionMode == PvpTeamSelectionMode.PlayerChoice;
			this.pvpTeamChoiceMiddleButton.Text = TextManager.Get(pvpPlayerChoiceMode ? "PvP.PickRandom" : "teampreference.nopreference");
			if (pvpPlayerChoiceMode && CS$<>8__locals1.serverSettings.PvpAutoBalanceThreshold > 0)
			{
				this.pvpTeamChoiceTeam1.Enabled = (CS$<>8__locals1.currentTeam == CharacterTeamType.Team1 || this.<RefreshPvpTeamSelectionButtons>g__CanJoinTeam1|282_0(ref CS$<>8__locals1));
				this.pvpTeamChoiceTeam2.Enabled = (CS$<>8__locals1.currentTeam == CharacterTeamType.Team2 || this.<RefreshPvpTeamSelectionButtons>g__CanJoinTeam2|282_1(ref CS$<>8__locals1));
				this.pvpTeamChoiceTeam1.ToolTip = ((!this.pvpTeamChoiceTeam1.Enabled) ? TextManager.Get("PvP.TeamDisabledBecauseBalance") : null);
				this.pvpTeamChoiceTeam2.ToolTip = ((!this.pvpTeamChoiceTeam2.Enabled) ? TextManager.Get("PvP.TeamDisabledBecauseBalance") : null);
				this.pvpTeamChoiceMiddleButton.Enabled = (this.<RefreshPvpTeamSelectionButtons>g__CanJoinTeam1|282_0(ref CS$<>8__locals1) && this.<RefreshPvpTeamSelectionButtons>g__CanJoinTeam2|282_1(ref CS$<>8__locals1));
				return;
			}
			this.pvpTeamChoiceTeam1.Enabled = true;
			this.pvpTeamChoiceTeam2.Enabled = true;
			this.pvpTeamChoiceTeam1.ToolTip = null;
			this.pvpTeamChoiceTeam2.ToolTip = null;
			this.pvpTeamChoiceMiddleButton.Enabled = true;
		}

		// Token: 0x0600261E RID: 9758 RVA: 0x001904B5 File Offset: 0x0018E6B5
		public void ResetPvpTeamSelection()
		{
			GUIListBox teamPreferenceListBox = this.TeamPreferenceListBox;
			if (teamPreferenceListBox != null)
			{
				teamPreferenceListBox.Deselect();
			}
			MultiplayerPreferences.Instance.TeamPreference = CharacterTeamType.None;
			this.RefreshPvpTeamSelectionButtons();
			this.RefreshChatrow();
			GameMain.Client.ForceNameJobTeamUpdate();
		}

		// Token: 0x0600261F RID: 9759 RVA: 0x001904EC File Offset: 0x0018E6EC
		public void SetAllowSpectating(bool allowSpectating)
		{
			if (GameMain.Client != null && GameMain.Client.IsServerOwner)
			{
				return;
			}
			if (this.campaignCharacterInfo != null && this.campaignCharacterInfo.PermanentlyDead)
			{
				return;
			}
			if (this.spectateBox.Selected && !allowSpectating)
			{
				this.spectateBox.Selected = false;
			}
			this.spectateBox.Visible = allowSpectating;
		}

		// Token: 0x06002620 RID: 9760 RVA: 0x0019054B File Offset: 0x0018E74B
		public void SetAllowAFK(bool allowAFK)
		{
			if (this.afkBox.Visible != allowAFK)
			{
				this.afkBox.Selected = false;
				this.afkBox.Visible = allowAFK;
			}
		}

		// Token: 0x06002621 RID: 9761 RVA: 0x00190574 File Offset: 0x0018E774
		public void SetAFKSelected(bool selected)
		{
			if (this.afkBox.Selected != selected)
			{
				this.afkBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				this.afkBox.Selected = selected;
			}
		}

		// Token: 0x06002622 RID: 9762 RVA: 0x001905C5 File Offset: 0x0018E7C5
		public void SetAutoRestart(bool enabled, float timer = 0f)
		{
			this.autoRestartBox.Selected = enabled;
			this.autoRestartTimer = timer;
		}

		// Token: 0x06002623 RID: 9763 RVA: 0x001905DA File Offset: 0x0018E7DA
		public void SetMissionTypes(IEnumerable<Identifier> missionTypes)
		{
			this.MissionTypes = missionTypes;
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x001905E4 File Offset: 0x0018E7E4
		private void RefreshOutpostDropdown()
		{
			Identifier randomOutpostIdentifier = "Random".ToIdentifier();
			this.outpostDropdown.Parent.Visible = this.MissionTypeFrame.Visible;
			if (!this.outpostDropdown.Parent.Visible)
			{
				return;
			}
			this.outpostDropdownUpToDate = false;
			NetworkMember networkMember = GameMain.NetworkMember;
			Identifier prevSelected = (networkMember != null) ? networkMember.ServerSettings.SelectedOutpostName : Identifier.Empty;
			this.outpostDropdown.ClearChildren();
			this.outpostDropdown.AddItem(TextManager.Get("Random"), randomOutpostIdentifier, null, null, null);
			HashSet<Identifier> validOutpostTagsForMissions = new HashSet<Identifier>();
			IEnumerable<Type> suitableMissionClasses = (this.SelectedMode == GameModePreset.PvP) ? MissionPrefab.PvPMissionClasses.Values : MissionPrefab.CoOpMissionClasses.Values;
			foreach (Identifier missionType in this.MissionTypes)
			{
				foreach (MissionPrefab missionPrefab in MissionPrefab.Prefabs)
				{
					if (suitableMissionClasses.Contains(missionPrefab.MissionClass))
					{
						Identifier identifier = missionPrefab.Type;
						if (!(identifier != missionType) && !missionPrefab.SingleplayerOnly)
						{
							identifier = missionPrefab.AllowOutpostSelectionFromTag;
							if (!identifier.IsEmpty)
							{
								validOutpostTagsForMissions.Add(missionPrefab.AllowOutpostSelectionFromTag);
							}
						}
					}
				}
			}
			if (validOutpostTagsForMissions.Any<Identifier>())
			{
				foreach (SubmarineInfo submarineInfo in SubmarineInfo.SavedSubmarines.DistinctBy((SubmarineInfo s) => s.Name))
				{
					if (submarineInfo.Type == SubmarineType.Outpost && validOutpostTagsForMissions.Any(new Func<Identifier, bool>(submarineInfo.OutpostTags.Contains)))
					{
						this.outpostDropdown.AddItem(submarineInfo.DisplayName, submarineInfo.Name.ToIdentifier(), submarineInfo.Description, null, null);
					}
				}
				if (!this.outpostDropdown.ListBox.Select(prevSelected, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled))
				{
					object selectedData = this.outpostDropdown.SelectedData;
					if (selectedData is Identifier)
					{
						Identifier selectedIdentifier = (Identifier)selectedData;
						if (selectedIdentifier != randomOutpostIdentifier)
						{
							this.outpostDropdown.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
						}
					}
					this.outpostDropdown.ListBox.Select(randomOutpostIdentifier, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
				}
				GameMain.Client.ServerSettings.AssignGUIComponent("SelectedOutpostName", this.outpostDropdown);
			}
			else
			{
				this.outpostDropdown.Parent.Visible = false;
				GameMain.Client.ServerSettings.AssignGUIComponent("SelectedOutpostName", null);
			}
			this.outpostDropdownUpToDate = true;
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x0019091C File Offset: 0x0018EB1C
		public void UpdateSubList(GUIComponent subList, IEnumerable<SubmarineInfo> submarines)
		{
			if (subList == null)
			{
				return;
			}
			subList.ClearChildren();
			foreach (SubmarineInfo sub in submarines)
			{
				this.AddSubmarine(subList, sub);
			}
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x00190970 File Offset: 0x0018EB70
		private void AddSubmarine(GUIComponent subList, SubmarineInfo sub)
		{
			GUIListBox listBox = subList as GUIListBox;
			if (listBox != null)
			{
				subList = listBox.Content;
			}
			else
			{
				GUIDropDown dropDown = subList as GUIDropDown;
				if (dropDown != null)
				{
					subList = dropDown.ListBox.Content;
				}
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), subList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, (int)(GUIStyle.SmallFont.LineHeight * 2.3f))
			}, "ListBoxElement", null)
			{
				ToolTip = sub.Description,
				UserData = sub
			};
			GUILayoutGroup frameLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUITextBlock subTextBlock = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), frameLayout.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), ToolBox.LimitString(sub.DisplayName.Value, GUIStyle.Font, subList.Rect.Width - 65), null, null, Alignment.CenterLeft, false, "", null)
			{
				ToolTip = sub.Description,
				UserData = "nametext",
				CanBeFocused = true
			};
			GUIFrame pvpContainer = new GUIFrame(new RectTransform(new Vector2(0.3f, 1f), frameLayout.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(0.5f, 1f), pvpContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), "CoalitionIcon", null);
			guiframe.Visible = false;
			guiframe.UserData = "coalitionIcon";
			guiframe.CanBeFocused = false;
			GUIFrame guiframe2 = new GUIFrame(new RectTransform(new Vector2(0.5f, 1f), pvpContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "SeparatistIcon", null);
			guiframe2.Visible = false;
			guiframe2.UserData = "separatistsIcon";
			guiframe2.CanBeFocused = false;
			SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(delegate(SubmarineInfo s)
			{
				if (s.Name == sub.Name)
				{
					Md5Hash md5Hash3 = s.MD5Hash;
					string a2 = (md5Hash3 != null) ? md5Hash3.StringRepresentation : null;
					Md5Hash md5Hash4 = sub.MD5Hash;
					return a2 == ((md5Hash4 != null) ? md5Hash4.StringRepresentation : null);
				}
				return false;
			}) ?? SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == sub.Name);
			if (matchingSub == null)
			{
				subTextBlock.TextColor = new Color(subTextBlock.TextColor, 0.5f);
				frame.ToolTip = TextManager.Get("SubNotFound");
			}
			else
			{
				if (!(((matchingSub != null) ? matchingSub.MD5Hash : null) == null))
				{
					Md5Hash md5Hash = matchingSub.MD5Hash;
					string a = (md5Hash != null) ? md5Hash.StringRepresentation : null;
					Md5Hash md5Hash2 = sub.MD5Hash;
					if (!(a != ((md5Hash2 != null) ? md5Hash2.StringRepresentation : null)))
					{
						if (subList == this.ShuttleList || subList == this.ShuttleList.ListBox || subList == this.ShuttleList.ListBox.Content)
						{
							subTextBlock.TextColor = new Color(subTextBlock.TextColor, sub.HasTag(SubmarineTag.Shuttle) ? 1f : 0.6f);
							goto IL_41A;
						}
						goto IL_41A;
					}
				}
				subTextBlock.TextColor = new Color(subTextBlock.TextColor, 0.5f);
				frame.ToolTip = TextManager.Get("SubDoesntMatch");
			}
			IL_41A:
			if (!sub.RequiredContentPackagesInstalled)
			{
				subTextBlock.TextColor = Color.Lerp(subTextBlock.TextColor, Color.DarkRed, 0.5f);
				frame.ToolTip = TextManager.Get("ContentPackageMismatch") + "\n\n" + frame.ToolTip.SanitizedString;
			}
			this.CreateSubmarineClassText(frame, sub, subTextBlock, subList);
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x00190E08 File Offset: 0x0018F008
		private void CreateSubmarineClassText(GUIComponent parent, SubmarineInfo sub, GUITextBlock subTextBlock, GUIComponent subList)
		{
			GUIFont smallFont;
			if (sub.HasTag(SubmarineTag.Shuttle))
			{
				RectTransform rectTransform = new RectTransform(new Vector2(0.5f, 1f), parent.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
				rectTransform.AbsoluteOffset = new Point(GUI.IntScale(20f), 0);
				RichString text = TextManager.Get(new string[]
				{
					"Shuttle",
					"RespawnShuttle"
				});
				smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.CenterRight, false, "", null);
				guitextBlock.TextColor = subTextBlock.TextColor * 0.8f;
				RichString toolTip = subTextBlock.ToolTip;
				guitextBlock.ToolTip = ((toolTip != null) ? toolTip.SanitizedString : null);
				guitextBlock.CanBeFocused = false;
				if (subList != this.SubList.Content)
				{
					return;
				}
				subTextBlock.TextColor *= 0.8f;
				using (IEnumerator<GUIComponent> enumerator = parent.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GUIComponent child = enumerator.Current;
						child.Color *= 0.8f;
					}
					return;
				}
			}
			GUILayoutGroup infoContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.25f, 1f), parent.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(GUI.IntScale(20f), 0)
			}, false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.5f), infoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.GetWithVariable("currencyformat", "[credits]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", sub.Price), FormatCapitals.No);
			smallFont = GUIStyle.SmallFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT, text2, null, smallFont, Alignment.BottomRight, false, "", null);
			guitextBlock2.Padding = Vector4.Zero;
			guitextBlock2.UserData = "pricetext";
			guitextBlock2.TextColor = subTextBlock.TextColor * 0.8f;
			guitextBlock2.CanBeFocused = false;
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.5f), infoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("submarineclass.");
			defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(sub.SubmarineClass);
			RichString text3 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			smallFont = GUIStyle.SmallFont;
			GUITextBlock guitextBlock3 = new GUITextBlock(rectT2, text3, null, smallFont, Alignment.TopRight, false, "", null);
			guitextBlock3.Padding = Vector4.Zero;
			guitextBlock3.UserData = "classtext";
			guitextBlock3.TextColor = subTextBlock.TextColor * 0.8f;
			guitextBlock3.ToolTip = subTextBlock.ToolTip;
			guitextBlock3.CanBeFocused = false;
		}

		// Token: 0x06002628 RID: 9768 RVA: 0x00191158 File Offset: 0x0018F358
		public bool VotableClicked(GUIComponent component, object userData)
		{
			if (GameMain.Client == null)
			{
				return false;
			}
			VoteType voteType;
			if (component.Parent == GameMain.NetLobbyScreen.SubList.Content)
			{
				bool flag = this.SelectedMode == GameModePreset.PvP;
				bool flag2 = flag;
				if (flag2)
				{
					CharacterTeamType teamPreference = MultiplayerPreferences.Instance.TeamPreference;
					bool flag3 = teamPreference - CharacterTeamType.Team1 <= 1;
					flag2 = !flag3;
				}
				if (flag2)
				{
					if (this.TeamPreferenceListBox == null)
					{
						GameClient client = GameMain.Client;
						this.UpdatePlayerFrame((client != null) ? client.CharacterInfo : null, true);
					}
					foreach (GUIComponent child in this.TeamPreferenceListBox.Content.Children)
					{
						object userData2 = child.UserData;
						if (!(userData2 is CharacterTeamType) || (CharacterTeamType)userData2 != CharacterTeamType.None)
						{
							child.Flash(new Color?(GUIStyle.Red), 1f, true, false, null);
						}
					}
					return false;
				}
				if (!GameMain.Client.ServerSettings.AllowSubVoting)
				{
					SubmarineInfo selectedSub = (SubmarineInfo)component.UserData;
					SelectedSubType type2;
					if (this.SelectedMode != GameModePreset.PvP)
					{
						type2 = SelectedSubType.Sub;
					}
					else
					{
						CharacterTeamType teamPreference2 = MultiplayerPreferences.Instance.TeamPreference;
						SelectedSubType selectedSubType;
						if (teamPreference2 > CharacterTeamType.Team1)
						{
							if (teamPreference2 != CharacterTeamType.Team2)
							{
								throw new NotImplementedException();
							}
							selectedSubType = SelectedSubType.EnemySub;
						}
						else
						{
							selectedSubType = SelectedSubType.Sub;
						}
						type2 = selectedSubType;
					}
					SelectedSubType type = type2;
					if (this.SelectedMode == GameModePreset.MultiPlayerCampaign && this.CampaignSetupUI != null)
					{
						if (selectedSub.Price > CampaignSettings.CurrentSettings.InitialMoney)
						{
							new GUIMessageBox(TextManager.Get("warning"), TextManager.Get("campaignsubtooexpensive"), null, null, GUIMessageBox.Type.Default);
						}
						if (!selectedSub.IsCampaignCompatible)
						{
							new GUIMessageBox(TextManager.Get("warning"), TextManager.Get("campaignsubincompatible"), null, null, GUIMessageBox.Type.Default);
						}
					}
					if (!selectedSub.RequiredContentPackagesInstalled)
					{
						GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ContentPackageMismatch"), selectedSub.RequiredContentPackages.Any<string>() ? TextManager.GetWithVariable("ContentPackageMismatchWarning", "[requiredcontentpackages]", string.Join(", ", selectedSub.RequiredContentPackages), FormatCapitals.No) : TextManager.Get("ContentPackageMismatchWarningGeneric"), new LocalizedString[]
						{
							TextManager.Get("Yes"),
							TextManager.Get("No")
						}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
						msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
						GUIButton guibutton = msgBox.Buttons[0];
						guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object obj)
						{
							GameMain.Client.RequestSelectSub(obj as SubmarineInfo, type);
							return true;
						}));
						msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
						return false;
					}
					if (GameMain.Client.HasPermission(ClientPermissions.SelectSub))
					{
						GameMain.Client.RequestSelectSub(selectedSub, type);
						return true;
					}
					return false;
				}
				else
				{
					SubmarineInfo sub = component.UserData as SubmarineInfo;
					if (sub != null)
					{
						this.CreateSubPreview(sub);
					}
					voteType = VoteType.Sub;
				}
			}
			else
			{
				if (component.Parent != GameMain.NetLobbyScreen.ModeList.Content)
				{
					return false;
				}
				if (!GameMain.Client.ServerSettings.AllowModeVoting)
				{
					if (!GameMain.Client.HasPermission(ClientPermissions.SelectMode))
					{
						return false;
					}
					Identifier presetName = ((GameModePreset)component.UserData).Identifier;
					if (this.HighlightedModeIndex == this.SelectedModeIndex && GameMain.NetLobbyScreen.ModeList.SelectedData as GameModePreset == GameModePreset.MultiPlayerCampaign && presetName != GameModePreset.MultiPlayerCampaign.Identifier)
					{
						GUIMessageBox verificationBox = new GUIMessageBox("", TextManager.Get("endcampaignverification"), new LocalizedString[]
						{
							TextManager.Get("yes"),
							TextManager.Get("no")
						}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
						GUIButton guibutton2 = verificationBox.Buttons[0];
						guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
						{
							GameMain.Client.RequestSelectMode(component.Parent.GetChildIndex(component));
							this.HighlightMode(this.SelectedModeIndex);
							verificationBox.Close(btn, userdata);
							return true;
						}));
						verificationBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(verificationBox.Close);
						return false;
					}
					GameMain.Client.RequestSelectMode(component.Parent.GetChildIndex(component));
					this.HighlightMode(this.SelectedModeIndex);
					if (presetName == "multiplayercampaign")
					{
						GUI.SetCursorWaiting(10, () => this.CampaignFrame.Visible || this.CampaignSetupFrame.Visible);
					}
					return presetName != "multiplayercampaign";
				}
				else
				{
					if (!((GameModePreset)userData).Votable)
					{
						return false;
					}
					voteType = VoteType.Mode;
				}
			}
			GameMain.Client.Vote(voteType, userData);
			return true;
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x001916EC File Offset: 0x0018F8EC
		public void AddPlayer(Client client)
		{
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.1f), this.PlayerList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, (int)(30f * GUI.Scale));
			RichString text = client.Name;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.CenterLeft, false, null, null)
			{
				Padding = Vector4.One * 10f * GUI.Scale,
				Color = Color.White * 0.25f,
				HoverColor = Color.White * 0.5f,
				SelectedColor = Color.White * 0.85f,
				OutlineColor = Color.White * 0.5f,
				TextColor = Color.White,
				SelectedTextColor = Color.Black,
				UserData = client
			};
			GUIImage soundIcon = new GUIImage(new RectTransform(Vector2.One * 0.8f, textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
			{
				AbsoluteOffset = new Point(5, 0)
			}, GUIStyle.GetComponentStyle("GUISoundIcon").GetDefaultSprite(), true, null)
			{
				UserData = new Pair<string, float>("soundicon", 0f),
				CanBeFocused = false,
				Visible = true,
				OverrideState = new GUIComponent.ComponentState?(GUIComponent.ComponentState.None),
				HoverColor = Color.White
			};
			GUIImage soundIconDisabled = new GUIImage(new RectTransform(Vector2.One * 0.8f, textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
			{
				AbsoluteOffset = new Point(5, 0)
			}, "GUISoundIconDisabled", GUIImage.ScalingMode.None)
			{
				UserData = "soundicondisabled",
				CanBeFocused = true,
				Visible = false,
				OverrideState = new GUIComponent.ComponentState?(GUIComponent.ComponentState.None),
				HoverColor = Color.White
			};
			GUIFrame readyTick = new GUIFrame(new RectTransform(new Vector2(0.6f, 0.6f), textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
			{
				AbsoluteOffset = new Point(10 + soundIcon.Rect.Width, 0)
			}, "GUIReadyToStart", null)
			{
				Visible = false,
				CanBeFocused = false,
				ToolTip = TextManager.Get("ReadyToStartTickBox"),
				UserData = "clientready"
			};
			GUICustomComponent downloadingThrobber = new GUICustomComponent(new RectTransform(Vector2.One, textBlock.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), this.DrawDownloadThrobber(client, new GUIComponent[]
			{
				soundIcon,
				soundIconDisabled,
				readyTick
			}), null);
		}

		// Token: 0x0600262A RID: 9770 RVA: 0x00191A24 File Offset: 0x0018FC24
		private Action<SpriteBatch, GUICustomComponent> DrawDownloadThrobber(Client client, params GUIComponent[] otherComponents)
		{
			return delegate(SpriteBatch sb, GUICustomComponent c)
			{
				NetLobbyScreen.DrawDownloadThrobber(client, otherComponents, sb, c);
			};
		}

		// Token: 0x0600262B RID: 9771 RVA: 0x00191A54 File Offset: 0x0018FC54
		private static void DrawDownloadThrobber(Client client, GUIComponent[] otherComponents, SpriteBatch spriteBatch, GUICustomComponent component)
		{
			if (!client.IsDownloading)
			{
				component.ToolTip = "";
				return;
			}
			component.HideElementsOutsideFrame = false;
			int drawRectX = (from c in otherComponents
			where c.Visible
			select c.Rect).Concat(new Rectangle(component.Parent.Rect.Right, component.Parent.Rect.Y, 0, component.Parent.Rect.Height).ToEnumerable<Rectangle>()).Min((Rectangle r) => r.X) - component.Parent.Rect.Height - 10;
			Rectangle drawRect = new Rectangle(drawRectX, component.Rect.Y, component.Parent.Rect.Height, component.Parent.Rect.Height);
			component.RectTransform.AbsoluteOffset = drawRect.Location - component.Parent.Rect.Location;
			component.RectTransform.NonScaledSize = drawRect.Size;
			GUISpriteSheet sheet = GUIStyle.GenericThrobber;
			GUISpriteSheet guispriteSheet = sheet;
			Vector2 pos = drawRect.Location.ToVector2();
			guispriteSheet.Draw(spriteBatch, (int)Math.Floor(Timing.TotalTime * 24.0) % sheet.FrameCount, pos, Color.White, Vector2.Zero, 0f, Vector2.One * (float)component.Parent.Rect.Height / sheet.FrameSize.ToVector2(), SpriteEffects.None, null);
			if (component.ToolTip.IsNullOrEmpty())
			{
				component.ToolTip = TextManager.Get("PlayerIsDownloadingFiles");
			}
		}

		// Token: 0x0600262C RID: 9772 RVA: 0x00191C60 File Offset: 0x0018FE60
		public void SetPlayerNameAndJobPreference(Client client)
		{
			GUITextBlock playerFrame = (GUITextBlock)this.PlayerList.Content.FindChild(client, false);
			if (playerFrame == null)
			{
				return;
			}
			playerFrame.Text = client.Name;
			playerFrame.ToolTip = "";
			Color color = Color.White;
			if (this.SelectedMode == GameModePreset.PvP)
			{
				CharacterTeamType preferredTeam = client.PreferredTeam;
				if (preferredTeam != CharacterTeamType.Team1)
				{
					if (preferredTeam != CharacterTeamType.Team2)
					{
						playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("none"), FormatCapitals.No);
					}
					else
					{
						color = new Color(150, 110, 0, 255);
						playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("teampreference.team2"), FormatCapitals.No);
					}
				}
				else
				{
					color = new Color(0, 110, 150, 255);
					playerFrame.ToolTip = TextManager.GetWithVariable("teampreference", "[team]", TextManager.Get("teampreference.team1"), FormatCapitals.No);
				}
			}
			else if (JobPrefab.Prefabs.ContainsKey(client.PreferredJob))
			{
				color = JobPrefab.Prefabs[client.PreferredJob].UIColor;
				playerFrame.ToolTip = TextManager.GetWithVariable("jobpreference", "[job]", JobPrefab.Prefabs[client.PreferredJob].Name, FormatCapitals.No);
			}
			else
			{
				playerFrame.ToolTip = TextManager.GetWithVariable("jobpreference", "[job]", TextManager.Get("none"), FormatCapitals.No);
			}
			playerFrame.Color = color * 0.4f;
			playerFrame.HoverColor = color * 0.6f;
			playerFrame.SelectedColor = color * 0.8f;
			playerFrame.OutlineColor = color * 0.5f;
			playerFrame.TextColor = color;
		}

		// Token: 0x0600262D RID: 9773 RVA: 0x00191E40 File Offset: 0x00190040
		public void SetPlayerVoiceIconState(Client client, bool muted, bool mutedLocally)
		{
			GUIComponent PlayerFrame = this.PlayerList.Content.FindChild(client, false);
			if (PlayerFrame == null)
			{
				return;
			}
			GUIComponent soundIcon = PlayerFrame.FindChild(delegate(GUIComponent c)
			{
				Pair<string, float> pair = c.UserData as Pair<string, float>;
				return pair != null && pair.First == "soundicon";
			}, false);
			GUIComponent soundIconDisabled = PlayerFrame.FindChild("soundicondisabled", false);
			Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
			if (!soundIcon.Visible)
			{
				userdata.Second = 0f;
			}
			soundIcon.Visible = (!muted && !mutedLocally);
			soundIconDisabled.Visible = (muted || mutedLocally);
			soundIconDisabled.ToolTip = TextManager.Get(mutedLocally ? "MutedLocally" : "MutedGlobally");
		}

		// Token: 0x0600262E RID: 9774 RVA: 0x00191EF0 File Offset: 0x001900F0
		public void SetPlayerSpeaking(Client client)
		{
			GUIComponent PlayerFrame = this.PlayerList.Content.FindChild(client, false);
			if (PlayerFrame == null)
			{
				return;
			}
			GUIComponent soundIcon = PlayerFrame.FindChild(delegate(GUIComponent c)
			{
				Pair<string, float> pair = c.UserData as Pair<string, float>;
				return pair != null && pair.First == "soundicon";
			}, false);
			Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
			userdata.Second = Math.Max(userdata.Second, 0.18f);
			soundIcon.Visible = true;
		}

		// Token: 0x0600262F RID: 9775 RVA: 0x00191F64 File Offset: 0x00190164
		public void RemovePlayer(Client client)
		{
			GUIComponent child = this.PlayerList.Content.GetChildByUserData(client);
			if (child != null)
			{
				this.PlayerList.RemoveChild(child);
			}
		}

		// Token: 0x06002630 RID: 9776 RVA: 0x00191F92 File Offset: 0x00190192
		public static Client ExtractClientFromClickableArea(GUITextBlock.ClickableArea area)
		{
			return area.Data.ExtractClient();
		}

		// Token: 0x06002631 RID: 9777 RVA: 0x00191FA0 File Offset: 0x001901A0
		public void SelectPlayer(GUITextBlock component, GUITextBlock.ClickableArea area)
		{
			Client client = NetLobbyScreen.ExtractClientFromClickableArea(area);
			if (client == null)
			{
				return;
			}
			GameMain.NetLobbyScreen.SelectPlayer(client);
		}

		// Token: 0x06002632 RID: 9778 RVA: 0x00191FC4 File Offset: 0x001901C4
		public void ShowPlayerContextMenu(GUITextBlock component, GUITextBlock.ClickableArea area)
		{
			Client client = NetLobbyScreen.ExtractClientFromClickableArea(area);
			if (client == null)
			{
				return;
			}
			NetLobbyScreen.CreateModerationContextMenu(client);
		}

		// Token: 0x06002633 RID: 9779 RVA: 0x00191FE4 File Offset: 0x001901E4
		public static void CreateModerationContextMenu(Client client)
		{
			if (GUIContextMenu.CurrentContextMenu != null)
			{
				return;
			}
			if (GameMain.IsSingleplayer || client == null)
			{
				return;
			}
			GameClient client2 = GameMain.Client;
			if (client2 != null)
			{
				IEnumerable<Client> previouslyConnectedClients = client2.PreviouslyConnectedClients;
				if (previouslyConnectedClients.Contains(client))
				{
					bool hasAccountId = client.AccountId.IsSome();
					bool canKick = GameMain.Client.HasPermission(ClientPermissions.Kick);
					bool canBan = GameMain.Client.HasPermission(ClientPermissions.Ban) && client.AllowKicking;
					bool canManagePermissions = GameMain.Client.HasPermission(ClientPermissions.ManagePermissions);
					if (client.SessionId == GameMain.Client.SessionId)
					{
						canBan = (canKick = (canManagePermissions = false));
					}
					List<ContextMenuOption> options = new List<ContextMenuOption>();
					AccountId accountId;
					if (client.AccountId.TryUnwrap(out accountId))
					{
						options.Add(new ContextMenuOption(accountId.ViewProfileLabel(), hasAccountId, delegate()
						{
							accountId.OpenProfile();
						}));
					}
					options.Add(new ContextMenuOption("ModerationMenu.ManagePlayer", true, delegate()
					{
						NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
						if (netLobbyScreen == null)
						{
							return;
						}
						netLobbyScreen.SelectPlayer(client);
					}));
					List<ContextMenuOption> rankOptions = new List<ContextMenuOption>();
					using (List<PermissionPreset>.Enumerator enumerator = PermissionPreset.List.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PermissionPreset rank = enumerator.Current;
							rankOptions.Add(new ContextMenuOption(rank.DisplayName, true, delegate()
							{
								LocalizedString label = TextManager.GetWithVariables((rank.Permissions == ClientPermissions.None) ? "clearrankprompt" : "giverankprompt", new ValueTuple<string, LocalizedString>[]
								{
									new ValueTuple<string, LocalizedString>("[user]", client.Name),
									new ValueTuple<string, LocalizedString>("[rank]", rank.DisplayName)
								});
								GUIMessageBox msgBox = new GUIMessageBox(string.Empty, label, new LocalizedString[]
								{
									TextManager.Get("Yes"),
									TextManager.Get("Cancel")
								}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
								msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
								{
									client.SetPermissions(rank.Permissions, rank.PermittedCommands);
									GameMain.Client.UpdateClientPermissions(client);
									msgBox.Close();
									return true;
								};
								msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
								{
									msgBox.Close();
									return true;
								};
							})
							{
								Tooltip = rank.Description
							});
						}
					}
					options.Add(new ContextMenuOption("Rank", canManagePermissions, rankOptions.ToArray()));
					Character character = client.Character;
					Color? color;
					if (character == null)
					{
						color = null;
					}
					else
					{
						CharacterInfo info = character.Info;
						color = ((info != null) ? new Color?(info.Job.Prefab.UIColor) : null);
					}
					Color clientColor = color ?? Color.White;
					if (GameMain.Client.ConnectedClients.Contains(client))
					{
						options.Add(new ContextMenuOption(client.MutedLocally ? "Unmute" : "Mute", client.SessionId != GameMain.Client.SessionId, delegate()
						{
							client.MutedLocally = !client.MutedLocally;
						}));
						bool kickEnabled = client.SessionId != GameMain.Client.SessionId && client.AllowKicking;
						ContextMenuOption kickOption;
						if (canKick)
						{
							kickOption = new ContextMenuOption("Kick", kickEnabled, delegate()
							{
								GameClient client4 = GameMain.Client;
								if (client4 == null)
								{
									return;
								}
								client4.CreateKickReasonPrompt(client.Name, false);
							});
						}
						else
						{
							kickOption = new ContextMenuOption("VoteToKick", kickEnabled, delegate()
							{
								GameClient client4 = GameMain.Client;
								if (client4 == null)
								{
									return;
								}
								client4.VoteForKick(client);
							});
						}
						options.Add(kickOption);
					}
					GameClient client3 = GameMain.Client;
					bool? flag;
					if (client3 == null)
					{
						flag = null;
					}
					else
					{
						ServerSettings serverSettings = client3.ServerSettings;
						if (serverSettings == null)
						{
							flag = null;
						}
						else
						{
							BanList banList = serverSettings.BanList;
							if (banList == null)
							{
								flag = null;
							}
							else
							{
								IReadOnlyList<BannedPlayer> bannedPlayers = banList.BannedPlayers;
								flag = ((bannedPlayers != null) ? new bool?(bannedPlayers.Any((BannedPlayer bp) => bp.MatchesClient(client))) : null);
							}
						}
					}
					bool? flag2 = flag;
					if (flag2.GetValueOrDefault())
					{
						options.Add(new ContextMenuOption("clientpermission.unban", canBan, delegate()
						{
							GameClient client4 = GameMain.Client;
							if (client4 == null)
							{
								return;
							}
							client4.UnbanPlayer(client.Name);
						}));
					}
					else
					{
						options.Add(new ContextMenuOption("Ban", canBan, delegate()
						{
							GameClient client4 = GameMain.Client;
							if (client4 == null)
							{
								return;
							}
							client4.CreateKickReasonPrompt(client.Name, true);
						}));
					}
					GUIContextMenu.CreateContextMenu(null, client.Name, new Color?(clientColor), options.ToArray());
					return;
				}
			}
		}

		// Token: 0x06002634 RID: 9780 RVA: 0x001923D0 File Offset: 0x001905D0
		public bool SelectPlayer(Client selectedClient)
		{
			bool myClient = selectedClient.SessionId == GameMain.Client.SessionId;
			bool hasManagePermissions = GameMain.Client.HasPermission(ClientPermissions.ManagePermissions);
			this.PlayerFrame = new GUIButton(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock)
					{
						this.ClosePlayerFrame(btn, userdata);
					}
					return true;
				}
			};
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.PlayerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			Vector2 frameSize = hasManagePermissions ? new Vector2(0.28f, 0.5f) : new Vector2(0.28f, 0.15f);
			GUIFrame playerFrameInner = new GUIFrame(new RectTransform(frameSize, this.PlayerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(550, 0)
			}, "", null);
			GUILayoutGroup paddedPlayerFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.88f), playerFrameInner.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.03f
			};
			GUILayoutGroup headerContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup headerTextContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), headerContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			GUILayoutGroup headerVolumeContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), headerContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 1f), headerTextContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = selectedClient.Name;
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock nameText = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			nameText.Text = ToolBox.LimitString(nameText.Text, nameText.Font, (int)((float)nameText.Rect.Width * 0.95f));
			if (hasManagePermissions && !selectedClient.IsOwner)
			{
				this.PlayerFrame.UserData = selectedClient;
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.05f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get("Rank");
				font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
				GUIDropDown rankDropDown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.1f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Rank"), 4, "", false, false, Alignment.CenterLeft, 1f)
				{
					UserData = selectedClient,
					Enabled = !myClient
				};
				foreach (PermissionPreset permissionPreset in PermissionPreset.List)
				{
					rankDropDown.AddItem(permissionPreset.DisplayName, permissionPreset, permissionPreset.Description, null, null);
				}
				rankDropDown.AddItem(TextManager.Get("CustomRank"), null, null, null, null);
				PermissionPreset currentPreset = PermissionPreset.List.Find((PermissionPreset p) => p.Permissions == selectedClient.Permissions && p.PermittedCommands.Count == selectedClient.PermittedConsoleCommands.Count && !p.PermittedCommands.Except(selectedClient.PermittedConsoleCommands).Any<DebugConsole.Command>());
				rankDropDown.SelectItem(currentPreset);
				GUIDropDown rankDropDown2 = rankDropDown;
				rankDropDown2.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(rankDropDown2.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent c, object userdata)
				{
					PermissionPreset selectedPreset = (PermissionPreset)userdata;
					if (selectedPreset != null)
					{
						Client client2 = this.PlayerFrame.UserData as Client;
						client2.SetPermissions(selectedPreset.Permissions, selectedPreset.PermittedCommands);
						GameMain.Client.UpdateClientPermissions(client2);
						this.PlayerFrame = null;
						this.SelectPlayer(client2);
					}
					return true;
				}));
				GUILayoutGroup permissionLabels = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				RectTransform rectT3 = new RectTransform(new Vector2(0.5f, 1f), permissionLabels.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = TextManager.Get("Permissions");
				font = GUIStyle.SubHeadingFont;
				GUITextBlock permissionLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
				RectTransform rectT4 = new RectTransform(new Vector2(0.5f, 1f), permissionLabels.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = TextManager.Get("PermittedConsoleCommands");
				font = GUIStyle.SubHeadingFont;
				GUITextBlock consoleCommandLabel = new GUITextBlock(rectT4, text4, null, font, Alignment.Left, true, "", null);
				GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
				{
					permissionLabel,
					consoleCommandLabel
				});
				GUILayoutGroup permissionContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUILayoutGroup listBoxContainerLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), permissionContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), listBoxContainerLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(new string[]
				{
					"all",
					"clientpermission.all"
				}), null, "");
				guitickBox.Enabled = !myClient;
				guitickBox.OnSelected = delegate(GUITickBox tickbox)
				{
					rankDropDown.SelectItem(null);
					Client client2 = this.PlayerFrame.UserData as Client;
					if (client2 == null)
					{
						return false;
					}
					foreach (GUIComponent child in tickbox.Parent.GetChild<GUIListBox>().Content.Children)
					{
						GUITickBox permissionTickBox = child as GUITickBox;
						permissionTickBox.Enabled = false;
						permissionTickBox.Selected = tickbox.Selected;
						permissionTickBox.Enabled = true;
					}
					GameMain.Client.UpdateClientPermissions(client2);
					return true;
				};
				GUIListBox permissionsBox = new GUIListBox(new RectTransform(Vector2.One, listBoxContainerLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					UserData = selectedClient
				};
				GUITickBox.OnSelectedHandler <>9__7;
				foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
				{
					ClientPermissions permission = (ClientPermissions)obj;
					if (permission != ClientPermissions.None && permission != ClientPermissions.All)
					{
						GUITickBox guitickBox2 = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), permissionsBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ClientPermission." + permission.ToString()), GUIStyle.SmallFont, "");
						guitickBox2.UserData = permission;
						guitickBox2.Selected = selectedClient.HasPermission(permission);
						guitickBox2.Enabled = !myClient;
						GUITickBox.OnSelectedHandler onSelected;
						if ((onSelected = <>9__7) == null)
						{
							onSelected = (<>9__7 = delegate(GUITickBox tickBox)
							{
								rankDropDown.SelectItem(null);
								Client client2 = this.PlayerFrame.UserData as Client;
								if (client2 == null)
								{
									return false;
								}
								ClientPermissions thisPermission = (ClientPermissions)tickBox.UserData;
								if (tickBox.Selected)
								{
									client2.GivePermission(thisPermission);
								}
								else
								{
									client2.RemovePermission(thisPermission);
								}
								if (tickBox.Enabled)
								{
									GameMain.Client.UpdateClientPermissions(client2);
								}
								return true;
							});
						}
						guitickBox2.OnSelected = onSelected;
						GUITickBox permissionTick = guitickBox2;
						permissionTick.ToolTip = (permissionTick.TextBlock.ToolTip = TextManager.Get("ClientPermission." + permission.ToString() + ".description"));
					}
				}
				GUILayoutGroup listBoxContainerRight = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), permissionContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUITickBox guitickBox3 = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), listBoxContainerRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(new string[]
				{
					"all",
					"clientpermission.all"
				}), null, "");
				guitickBox3.Enabled = !myClient;
				guitickBox3.OnSelected = delegate(GUITickBox tickbox)
				{
					rankDropDown.SelectItem(null);
					Client client2 = this.PlayerFrame.UserData as Client;
					if (client2 == null)
					{
						return false;
					}
					foreach (GUIComponent child in tickbox.Parent.GetChild<GUIListBox>().Content.Children)
					{
						GUITickBox commandTickBox2 = child as GUITickBox;
						commandTickBox2.Enabled = false;
						commandTickBox2.Selected = tickbox.Selected;
						commandTickBox2.Enabled = true;
					}
					GameMain.Client.UpdateClientPermissions(client2);
					return true;
				};
				GUIListBox commandList = new GUIListBox(new RectTransform(Vector2.One, listBoxContainerRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					UserData = selectedClient
				};
				GUITickBox.OnSelectedHandler <>9__9;
				foreach (DebugConsole.Command command in DebugConsole.Commands)
				{
					GUITickBox commandTickBox = new GUITickBox(new RectTransform(new Vector2(0.15f, 0.15f), commandList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), command.Names[0].Value, GUIStyle.SmallFont, "")
					{
						Selected = selectedClient.PermittedConsoleCommands.Contains(command),
						Enabled = !myClient,
						ToolTip = command.Help,
						UserData = command
					};
					GUITickBox guitickBox4 = commandTickBox;
					Delegate onSelected2 = guitickBox4.OnSelected;
					GUITickBox.OnSelectedHandler b;
					if ((b = <>9__9) == null)
					{
						b = (<>9__9 = delegate(GUITickBox tickBox)
						{
							rankDropDown.SelectItem(null);
							DebugConsole.Command selectedCommand = tickBox.UserData as DebugConsole.Command;
							Client client2 = this.PlayerFrame.UserData as Client;
							if (client2 == null)
							{
								return false;
							}
							if (!tickBox.Selected)
							{
								client2.PermittedConsoleCommands.Remove(selectedCommand);
							}
							else if (!client2.PermittedConsoleCommands.Contains(selectedCommand))
							{
								client2.PermittedConsoleCommands.Add(selectedCommand);
							}
							if (tickBox.Enabled)
							{
								GameMain.Client.UpdateClientPermissions(client2);
							}
							return true;
						});
					}
					guitickBox4.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(onSelected2, b);
				}
			}
			GUILayoutGroup buttonAreaTop = myClient ? null : new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.08f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup buttonAreaLower = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.08f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			if (!myClient)
			{
				if (GameMain.Client.HasPermission(ClientPermissions.Ban))
				{
					GameClient client = GameMain.Client;
					bool? flag;
					if (client == null)
					{
						flag = null;
					}
					else
					{
						ServerSettings serverSettings = client.ServerSettings;
						if (serverSettings == null)
						{
							flag = null;
						}
						else
						{
							BanList banList = serverSettings.BanList;
							if (banList == null)
							{
								flag = null;
							}
							else
							{
								IReadOnlyList<BannedPlayer> bannedPlayers = banList.BannedPlayers;
								flag = ((bannedPlayers != null) ? new bool?(bannedPlayers.Any((BannedPlayer bp) => bp.MatchesClient(selectedClient))) : null);
							}
						}
					}
					bool? flag2 = flag;
					GUIButton banButton;
					if (flag2.GetValueOrDefault())
					{
						banButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1f), buttonAreaTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("clientpermission.unban"), Alignment.Center, "", null)
						{
							UserData = selectedClient,
							OnClicked = delegate(GUIButton bt, object userdata)
							{
								GameClient client2 = GameMain.Client;
								if (client2 != null)
								{
									client2.UnbanPlayer(selectedClient.Name);
								}
								return true;
							}
						};
					}
					else
					{
						banButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1f), buttonAreaTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Ban"), Alignment.Center, "", null)
						{
							UserData = selectedClient,
							OnClicked = delegate(GUIButton bt, object userdata)
							{
								NetLobbyScreen.BanPlayer(selectedClient);
								return true;
							}
						};
					}
					GUIButton guibutton = banButton;
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(this.ClosePlayerFrame));
				}
				if (GameMain.Client != null && GameMain.Client.ConnectedClients.Contains(selectedClient))
				{
					if (GameMain.Client.ServerSettings.AllowVoteKick && selectedClient != null && selectedClient.AllowKicking)
					{
						GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.34f, 1f), buttonAreaLower.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("VoteToKick"), Alignment.Center, "", null);
						guibutton2.OnClicked = delegate(GUIButton btn, object userdata)
						{
							GameMain.Client.VoteForKick(selectedClient);
							btn.Enabled = false;
							return true;
						};
						guibutton2.UserData = selectedClient;
					}
					if (GameMain.Client.HasPermission(ClientPermissions.Kick) && selectedClient != null && selectedClient.AllowKicking)
					{
						GUIButton kickButton = new GUIButton(new RectTransform(new Vector2(0.34f, 1f), buttonAreaLower.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Kick"), Alignment.Center, "", null)
						{
							UserData = selectedClient,
							OnClicked = delegate(GUIButton bt, object userdata)
							{
								NetLobbyScreen.KickPlayer(selectedClient);
								return true;
							}
						};
						GUIButton guibutton3 = kickButton;
						guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(this.ClosePlayerFrame));
					}
					GUILayoutGroup volumeLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), headerVolumeContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
					GUILayoutGroup volumeTextLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), volumeLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
					new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), volumeTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("VoiceChatVolume"), null, null, Alignment.Left, false, "", null);
					GUITextBlock volumePercentageText = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), volumeTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ToolBox.GetFormattedPercentage(selectedClient.VoiceVolume), null, null, Alignment.Right, false, "", null);
					GUIScrollBar guiscrollBar = new GUIScrollBar(new RectTransform(new Vector2(1f, 0.5f), volumeLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null);
					guiscrollBar.Range = new Vector2(0f, 1f);
					guiscrollBar.BarScroll = selectedClient.VoiceVolume / 2f;
					guiscrollBar.OnMoved = delegate(GUIScrollBar _, float barScroll)
					{
						float newVolume = barScroll * 2f;
						selectedClient.VoiceVolume = newVolume;
						volumePercentageText.Text = ToolBox.GetFormattedPercentage(newVolume);
						return true;
					};
					GUITickBox guitickBox5 = new GUITickBox(new RectTransform(new Vector2(0.175f, 1f), headerVolumeContainer.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Mute"), null, "");
					guitickBox5.Selected = selectedClient.MutedLocally;
					guitickBox5.OnSelected = delegate(GUITickBox tickBox)
					{
						selectedClient.MutedLocally = tickBox.Selected;
						return true;
					};
				}
				if (buttonAreaTop.CountChildren > 0)
				{
					GUITextBlock.AutoScaleAndNormalize((from c in buttonAreaTop.Children
					select ((GUIButton)c).TextBlock).Concat(from c in buttonAreaLower.Children
					select ((GUIButton)c).TextBlock), true, false, null);
				}
			}
			AccountId accountId;
			if (selectedClient.AccountId.TryUnwrap(out accountId))
			{
				GUIButton viewSteamProfileButton = new GUIButton(new RectTransform(new Vector2(0.3f, 1f), headerTextContainer.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
				{
					MaxSize = new Point(int.MaxValue, (int)(40f * GUI.Scale))
				}, accountId.ViewProfileLabel(), Alignment.Center, "", null)
				{
					UserData = selectedClient
				};
				viewSteamProfileButton.TextBlock.AutoScaleHorizontal = true;
				viewSteamProfileButton.OnClicked = delegate(GUIButton bt, object userdata)
				{
					accountId.OpenProfile();
					return true;
				};
			}
			GUIButton guibutton4 = new GUIButton(new RectTransform(new Vector2(0f, 1f), buttonAreaLower.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null);
			guibutton4.IgnoreLayoutGroups = true;
			guibutton4.OnClicked = new GUIButton.OnClickedHandler(this.ClosePlayerFrame);
			float xSize = 1f / (float)buttonAreaLower.CountChildren;
			for (int i = 0; i < buttonAreaLower.CountChildren; i++)
			{
				buttonAreaLower.GetChild(i).RectTransform.RelativeSize = new Vector2(xSize, 1f);
			}
			buttonAreaLower.RectTransform.NonScaledSize = new Point(buttonAreaLower.Rect.Width, buttonAreaLower.RectTransform.Children.Max((RectTransform c) => c.NonScaledSize.Y));
			if (buttonAreaTop != null)
			{
				if (buttonAreaTop.CountChildren == 0)
				{
					paddedPlayerFrame.RemoveChild(buttonAreaTop);
				}
				else
				{
					for (int j = 0; j < buttonAreaTop.CountChildren; j++)
					{
						buttonAreaTop.GetChild(j).RectTransform.RelativeSize = new Vector2(0.33333334f, 1f);
					}
					RectTransform rectTransform = buttonAreaTop.RectTransform;
					RectTransform rectTransform2 = buttonAreaLower.RectTransform;
					Point nonScaledSize = new Point(buttonAreaLower.Rect.Width, Math.Max(buttonAreaLower.RectTransform.NonScaledSize.Y, buttonAreaTop.RectTransform.Children.Max((RectTransform c) => c.NonScaledSize.Y)));
					rectTransform2.NonScaledSize = nonScaledSize;
					rectTransform.NonScaledSize = nonScaledSize;
				}
			}
			return false;
		}

		// Token: 0x06002635 RID: 9781 RVA: 0x001939B4 File Offset: 0x00191BB4
		private bool ClosePlayerFrame(GUIButton button, object userData)
		{
			this.PlayerFrame = null;
			this.PlayerList.Deselect();
			return true;
		}

		// Token: 0x06002636 RID: 9782 RVA: 0x001939C9 File Offset: 0x00191BC9
		public static void KickPlayer(Client client)
		{
			if (GameMain.NetworkMember == null || client == null)
			{
				return;
			}
			GameMain.Client.CreateKickReasonPrompt(client.Name, false);
		}

		// Token: 0x06002637 RID: 9783 RVA: 0x001939E7 File Offset: 0x00191BE7
		public static void BanPlayer(Client client)
		{
			if (GameMain.NetworkMember == null || client == null)
			{
				return;
			}
			GameMain.Client.CreateKickReasonPrompt(client.Name, true);
		}

		// Token: 0x06002638 RID: 9784 RVA: 0x00193A05 File Offset: 0x00191C05
		public override void AddToGUIUpdateList()
		{
			base.AddToGUIUpdateList();
			GUIButton jobInfoFrame = NetLobbyScreen.JobInfoFrame;
			if (jobInfoFrame != null)
			{
				jobInfoFrame.AddToGUIUpdateList(false, 0);
			}
			CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = this.CharacterAppearanceCustomizationMenu;
			if (characterAppearanceCustomizationMenu != null)
			{
				characterAppearanceCustomizationMenu.AddToGUIUpdateList();
			}
			GUIFrame jobSelectionFrame = this.JobSelectionFrame;
			if (jobSelectionFrame == null)
			{
				return;
			}
			jobSelectionFrame.AddToGUIUpdateList(false, 1);
		}

		// Token: 0x06002639 RID: 9785 RVA: 0x00193A44 File Offset: 0x00191C44
		public override void Update(double deltaTime)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			this.UpdateMicIcon((float)deltaTime);
			foreach (GUIComponent child in this.PlayerList.Content.Children)
			{
				Client client = child.UserData as Client;
				if (client != null)
				{
					GUIImage soundIcon = child.FindChild(delegate(GUIComponent c)
					{
						Pair<string, float> pair = c.UserData as Pair<string, float>;
						return pair != null && pair.First == "soundicon";
					}, false) as GUIImage;
					if (soundIcon != null)
					{
						double voipAmplitude = 0.0;
						if (client.SessionId != GameMain.Client.SessionId)
						{
							VoipSound voipSound = client.VoipSound;
							voipAmplitude = (double)((voipSound != null) ? voipSound.CurrentAmplitude : 0f);
						}
						else
						{
							VoipCapture voip = VoipCapture.Instance;
							if (voip == null)
							{
								voipAmplitude = 0.0;
							}
							else if (voip.LastEnqueueAudio > DateTime.Now - new TimeSpan(0, 0, 0, 0, 100))
							{
								voipAmplitude = voip.LastAmplitude;
							}
						}
						VoipClient.UpdateVoiceIndicator(soundIcon, (float)voipAmplitude, (float)deltaTime);
					}
				}
			}
			this.autoRestartText.Visible = (this.autoRestartTimer > 0f && this.autoRestartBox.Selected);
			if (!MathUtils.NearlyEqual(this.autoRestartTimer, 0f, 0.0001f) && this.autoRestartBox.Selected)
			{
				this.autoRestartTimer = Math.Max(this.autoRestartTimer - (float)deltaTime, 0f);
				if (this.autoRestartTimer > 0f)
				{
					this.autoRestartText.Text = TextManager.Get("RestartingIn") + " " + ToolBox.SecondsToReadableTime(Math.Max(this.autoRestartTimer, 0f));
				}
			}
			CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = this.CharacterAppearanceCustomizationMenu;
			if (characterAppearanceCustomizationMenu != null)
			{
				characterAppearanceCustomizationMenu.Update();
			}
			if (this.JobSelectionFrame != null && PlayerInput.PrimaryMouseButtonDown() && !GUI.IsMouseOn(this.JobSelectionFrame))
			{
				this.JobList.Deselect();
				this.JobSelectionFrame.Visible = false;
			}
			NetLobbyScreen.UpdateJobVariantSelectionIfNeeded();
		}

		// Token: 0x0600263A RID: 9786 RVA: 0x00193C74 File Offset: 0x00191E74
		public static void UpdateJobVariantSelectionIfNeeded()
		{
			GUIComponent mouseOn = GUI.MouseOn;
			JobVariant jobPrefab = ((mouseOn != null) ? mouseOn.UserData : null) as JobVariant;
			if (jobPrefab != null)
			{
				GUIComponentStyle style = GUI.MouseOn.Style;
				if (((style != null) ? style.Name : null) == "JobVariantButton" && GUI.MouseOn.Parent != null)
				{
					bool isMultiplayer = GameMain.NetLobbyScreen != null && GameMain.NetworkMember != null;
					CharacterTeamType teamPreference = isMultiplayer ? GameMain.NetLobbyScreen.TeamPreference : CharacterTeamType.Team1;
					bool isPvPMode = isMultiplayer && GameMain.NetLobbyScreen.SelectedMode == GameModePreset.PvP;
					GUIComponent guicomponent = NetLobbyScreen.jobVariantTooltip;
					JobVariant prevVisibleVariant = ((guicomponent != null) ? guicomponent.UserData : null) as JobVariant;
					if (prevVisibleVariant == null || prevVisibleVariant.Prefab != jobPrefab.Prefab || prevVisibleVariant.Variant != jobPrefab.Variant)
					{
						NetLobbyScreen.CreateJobVariantTooltip(jobPrefab.Prefab, teamPreference, jobPrefab.Variant, isPvPMode, GUI.MouseOn.Parent);
					}
				}
			}
			if (NetLobbyScreen.jobVariantTooltip != null)
			{
				GUIComponent guicomponent2 = NetLobbyScreen.jobVariantTooltip;
				if (guicomponent2 != null)
				{
					guicomponent2.AddToGUIUpdateList(false, 1);
				}
				Rectangle mouseRect = NetLobbyScreen.jobVariantTooltip.MouseRect;
				mouseRect.Inflate(60f * GUI.Scale, 60f * GUI.Scale);
				if (!mouseRect.Contains(PlayerInput.MousePosition))
				{
					NetLobbyScreen.jobVariantTooltip = null;
				}
			}
		}

		// Token: 0x0600263B RID: 9787 RVA: 0x00193DBC File Offset: 0x00191FBC
		private void UpdateMicIcon(float deltaTime)
		{
			this.micCheckTimer -= deltaTime;
			if (this.micCheckTimer > 0f)
			{
				return;
			}
			Identifier newMicIconStyle = "GUIMicrophoneEnabled".ToIdentifier();
			if (GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Disabled)
			{
				newMicIconStyle = "GUIMicrophoneDisabled".ToIdentifier();
			}
			else
			{
				IReadOnlyList<string> voipCaptureDeviceNames = VoipCapture.GetCaptureDeviceNames();
				if (voipCaptureDeviceNames.Count == 0)
				{
					newMicIconStyle = "GUIMicrophoneUnavailable".ToIdentifier();
				}
			}
			if (newMicIconStyle != this.micIconStyle)
			{
				this.micIconStyle = newMicIconStyle;
				GUIStyle.Apply(this.micIcon, newMicIconStyle, null);
			}
			this.micCheckTimer = 1f;
		}

		// Token: 0x0600263C RID: 9788 RVA: 0x00193E58 File Offset: 0x00192058
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			Sprite sprite = this.backgroundSprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				return;
			}
			graphics.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			GUI.DrawBackgroundSprite(spriteBatch, this.backgroundSprite, Color.White, null, SpriteEffects.None);
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x00193ED0 File Offset: 0x001920D0
		private void DrawServerBanner(SpriteBatch spriteBatch, GUICustomComponent component)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (((networkMember != null) ? networkMember.ServerSettings : null) == null)
			{
				return;
			}
			PlayStyle playStyle = GameMain.NetworkMember.ServerSettings.PlayStyle;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PlayStyleBanner.");
			defaultInterpolatedStringHandler.AppendFormatted<PlayStyle>(playStyle);
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
			Sprite sprite = (componentStyle != null) ? componentStyle.GetSprite(GUIComponent.ComponentState.None) : null;
			if (sprite == null)
			{
				return;
			}
			GUI.DrawBackgroundSprite(spriteBatch, sprite, Color.White, new Rectangle?(component.Rect), SpriteEffects.None);
			if (this.prevPlayStyle == null || playStyle != this.prevPlayStyle.Value)
			{
				GUITextBlock guitextBlock = this.playstyleText;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("ServerTag.");
				defaultInterpolatedStringHandler2.AppendFormatted<PlayStyle>(playStyle);
				guitextBlock.Text = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
				this.playstyleText.Color = (sprite.SourceElement.GetAttributeColor("BannerColor") ?? Color.White);
				this.playstyleText.RectTransform.NonScaledSize = (this.playstyleText.Font.MeasureString(this.playstyleText.Text, false) + new Vector2(25f, 10f) * GUI.Scale).ToPoint();
				this.prevPlayStyle = new PlayStyle?(playStyle);
				GUILayoutGroup guilayoutGroup = this.playstyleText.Parent as GUILayoutGroup;
				if (guilayoutGroup != null)
				{
					guilayoutGroup.Recalculate();
				}
				GUIComponent guicomponent = this.playstyleText;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(21, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("ServerTagDescription.");
				defaultInterpolatedStringHandler3.AppendFormatted<PlayStyle>(playStyle);
				guicomponent.ToolTip = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			if (this.prevIsPublic == null || GameMain.NetworkMember.ServerSettings.IsPublic != this.prevIsPublic.Value)
			{
				this.publicOrPrivateText.Text = (GameMain.NetworkMember.ServerSettings.IsPublic ? TextManager.Get("PublicLobbyTag") : TextManager.Get("PrivateLobbyTag"));
				this.publicOrPrivateText.RectTransform.NonScaledSize = (this.publicOrPrivateText.Font.MeasureString(this.publicOrPrivateText.Text, false) + new Vector2(25f, 10f) * GUI.Scale).ToPoint();
				GUILayoutGroup guilayoutGroup2 = this.publicOrPrivateText.Parent as GUILayoutGroup;
				if (guilayoutGroup2 != null)
				{
					guilayoutGroup2.Recalculate();
				}
				this.prevIsPublic = new bool?(GameMain.NetworkMember.ServerSettings.IsPublic);
			}
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x0019418C File Offset: 0x0019238C
		private static void DrawJobVariantItems(SpriteBatch spriteBatch, GUICustomComponent component, JobVariant jobVariant, CharacterTeamType team, bool isPvPMode, int itemsPerRow)
		{
			IEnumerable<JobPrefab.JobItem> allJobItems = jobVariant.Prefab.GetJobItems(jobVariant.Variant, (JobPrefab.JobItem it) => it.ShowPreview);
			IEnumerable<Identifier> itemIdentifiers = (from it in allJobItems
			select it.GetItemIdentifier(team, isPvPMode)).Distinct<Identifier>();
			Point slotSize = new Point(component.Rect.Height);
			int spacing = (int)(5f * GUI.Scale);
			int slotCount = itemIdentifiers.Count<Identifier>();
			int slotCountPerRow = Math.Min(slotCount, itemsPerRow);
			int rows = (int)Math.Max(Math.Ceiling((double)((float)itemIdentifiers.Count<Identifier>() / (float)itemsPerRow)), 1.0);
			float totalWidth = (float)(slotSize.X * slotCountPerRow + spacing * (slotCountPerRow - 1));
			float totalHeight = (float)(slotSize.Y * rows + spacing * (rows - 1));
			if (totalWidth > (float)component.Rect.Width)
			{
				slotSize = new Point(Math.Min((int)Math.Floor((double)((float)(slotSize.X - spacing) * ((float)component.Rect.Width / totalWidth))), (int)Math.Floor((double)((float)(slotSize.Y - spacing) * ((float)component.Rect.Height / totalHeight)))));
			}
			int i = 0;
			Rectangle tooltipRect = Rectangle.Empty;
			LocalizedString tooltip = null;
			using (IEnumerator<Identifier> enumerator = itemIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier itemIdentifier = enumerator.Current;
					ItemPrefab itemPrefab = MapEntityPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab;
					if (itemPrefab != null)
					{
						int row = (int)Math.Floor((double)((float)i / (float)slotCountPerRow));
						int slotsPerThisRow = Math.Min(slotCount - row * slotCountPerRow, slotCountPerRow);
						Vector2 slotPos = new Vector2((float)component.Rect.Center.X + (float)(slotSize.X + spacing) * ((float)(i % slotCountPerRow) - (float)slotsPerThisRow * 0.5f), (float)(component.Rect.Bottom - rows * (slotSize.Y + spacing) + (slotSize.Y + spacing) * row));
						Rectangle slotRect = new Rectangle(slotPos.ToPoint(), slotSize);
						Sprite slotSpriteSmall = Inventory.SlotSpriteSmall;
						Vector2 pos = slotPos;
						float scale = (float)slotSize.X / (float)Inventory.SlotSpriteSmall.SourceRect.Width;
						slotSpriteSmall.Draw(spriteBatch, pos, slotRect.Contains(PlayerInput.MousePosition) ? Color.White : (Color.White * 0.6f), 0f, scale, SpriteEffects.None, null);
						Sprite icon = itemPrefab.InventoryIcon ?? itemPrefab.Sprite;
						float iconScale = Math.Min(Math.Min((float)slotSize.X / icon.size.X, (float)slotSize.Y / icon.size.Y), 2f) * 0.9f;
						icon.Draw(spriteBatch, slotPos + slotSize.ToVector2() * 0.5f, 0f, iconScale, SpriteEffects.None);
						int count = allJobItems.Where(delegate(JobPrefab.JobItem it)
						{
							Identifier itemIdentifier = it.GetItemIdentifier(team, isPvPMode);
							return itemIdentifier == itemIdentifier;
						}).Sum((JobPrefab.JobItem it) => it.Amount);
						if (count > 1)
						{
							string itemCountText = "x" + count.ToString();
							GUIStyle.Font.DrawString(spriteBatch, itemCountText, slotPos + slotSize.ToVector2() - GUIStyle.Font.MeasureString(itemCountText, false) - Vector2.UnitX * 5f, Color.White, ForceUpperCase.Inherit, false);
						}
						if (slotRect.Contains(PlayerInput.MousePosition))
						{
							tooltipRect = slotRect;
							tooltip = itemPrefab.Name + '\n' + itemPrefab.Description;
						}
						i++;
					}
				}
			}
			if (!tooltip.IsNullOrEmpty())
			{
				GUIComponent.DrawToolTip(spriteBatch, tooltip, tooltipRect, Anchor.BottomCenter, Pivot.TopLeft);
			}
		}

		// Token: 0x0600263F RID: 9791 RVA: 0x001945B4 File Offset: 0x001927B4
		public void NewChatMessage(ChatMessage message)
		{
			NetLobbyScreen.<>c__DisplayClass318_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass318_0();
			float prevSize = this.chatBox.BarSize;
			while (this.chatBox.Content.CountChildren > 60)
			{
				this.chatBox.RemoveChild(this.chatBox.Content.Children.First<GUIComponent>());
			}
			LocalizedString displayedChatRow = ChatMessage.GetTimeStamp();
			if (message.Type == ChatMessageType.Private)
			{
				displayedChatRow += TextManager.Get("PrivateMessageTag") + " ";
			}
			else if (message.Type == ChatMessageType.Team)
			{
				displayedChatRow += TextManager.Get("PvP.ChatMode.Team.ChatPrefixTag") + " ";
			}
			displayedChatRow += message.TextWithSender;
			NetLobbyScreen.<>c__DisplayClass318_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), this.chatBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = RichString.Rich(displayedChatRow, null);
			Color? textColor = new Color?(message.Color);
			Color? color = new Color?((this.chatBox.CountChildren % 2 == 0) ? Color.Transparent : (Color.Black * 0.1f));
			CS$<>8__locals2.msg = new GUITextBlock(rectT, text, textColor, GUIStyle.SmallFont, Alignment.Left, true, "", color)
			{
				UserData = message,
				CanBeFocused = false
			};
			CS$<>8__locals1.msg.CalculateHeightFromText(0, false);
			if (CS$<>8__locals1.msg.RichTextData != null)
			{
				foreach (RichTextData data in CS$<>8__locals1.msg.RichTextData.Value)
				{
					CS$<>8__locals1.msg.ClickableAreas.Add(new GUITextBlock.ClickableArea
					{
						Data = data,
						OnClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.SelectPlayer),
						OnSecondaryClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.ShowPlayerContextMenu)
					});
				}
			}
			CS$<>8__locals1.msg.RectTransform.SizeChanged += CS$<>8__locals1.<NewChatMessage>g__Recalculate|0;
			if ((prevSize == 1f && this.chatBox.BarScroll == 0f) || (prevSize < 1f && this.chatBox.BarScroll == 1f))
			{
				this.chatBox.BarScroll = 1f;
			}
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x00194833 File Offset: 0x00192A33
		private bool SelectJobPreferencesTab(GUIButton button, object userData)
		{
			this.jobPreferencesButton.Selected = true;
			this.appearanceButton.Selected = false;
			this.JobPreferenceContainer.Visible = true;
			this.appearanceFrame.Visible = false;
			return false;
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x00194868 File Offset: 0x00192A68
		private bool SelectAppearanceTab(GUIButton button, object _)
		{
			NetLobbyScreen.<>c__DisplayClass320_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass320_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.button = button;
			CS$<>8__locals1._ = _;
			this.jobPreferencesButton.Selected = false;
			this.appearanceButton.Selected = true;
			this.JobPreferenceContainer.Visible = false;
			this.appearanceFrame.Visible = true;
			this.appearanceFrame.ClearChildren();
			NetLobbyScreen.<>c__DisplayClass320_0 CS$<>8__locals2 = CS$<>8__locals1;
			CharacterInfo info;
			if ((info = GameMain.Client.CharacterInfo) == null)
			{
				Character controlled = Character.Controlled;
				info = ((controlled != null) ? controlled.Info : null);
			}
			CS$<>8__locals2.info = info;
			CharacterInfo.AppearanceCustomizationMenu characterAppearanceCustomizationMenu = this.CharacterAppearanceCustomizationMenu;
			if (characterAppearanceCustomizationMenu != null)
			{
				characterAppearanceCustomizationMenu.Dispose();
			}
			this.CharacterAppearanceCustomizationMenu = new CharacterInfo.AppearanceCustomizationMenu(CS$<>8__locals1.info, this.appearanceFrame, true)
			{
				OnHeadSwitch = delegate(CharacterInfo.AppearanceCustomizationMenu menu)
				{
					CS$<>8__locals1.<>4__this.UpdateJobPreferences(CS$<>8__locals1.info);
					CS$<>8__locals1.<>4__this.SelectAppearanceTab(CS$<>8__locals1.button, CS$<>8__locals1._);
				}
			};
			return false;
		}

		// Token: 0x06002642 RID: 9794 RVA: 0x0019492C File Offset: 0x00192B2C
		public bool SaveAppearance()
		{
			GameClient client = GameMain.Client;
			CharacterInfo info = (client != null) ? client.CharacterInfo : null;
			if (((info != null) ? info.Head : null) == null)
			{
				return false;
			}
			MultiplayerPreferences characterConfig = MultiplayerPreferences.Instance;
			characterConfig.TagSet.Clear();
			characterConfig.TagSet.UnionWith(info.Head.Preset.TagSet);
			characterConfig.HairIndex = info.Head.HairIndex;
			characterConfig.BeardIndex = info.Head.BeardIndex;
			characterConfig.MoustacheIndex = info.Head.MoustacheIndex;
			characterConfig.FaceAttachmentIndex = info.Head.FaceAttachmentIndex;
			characterConfig.HairColor = info.Head.HairColor;
			characterConfig.FacialHairColor = info.Head.FacialHairColor;
			characterConfig.SkinColor = info.Head.SkinColor;
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.IsRunning)
			{
				TabMenu.PendingChanges = true;
				this.CreateChangesPendingText();
			}
			GameSettings.SaveCurrentConfig();
			return true;
		}

		// Token: 0x06002643 RID: 9795 RVA: 0x00194A24 File Offset: 0x00192C24
		private bool SwitchJob(GUIButton _, object obj)
		{
			NetLobbyScreen.<>c__DisplayClass322_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass322_0();
			if (this.JobList == null || GameMain.Client == null)
			{
				return false;
			}
			int childIndex = this.JobList.SelectedIndex;
			GUIComponent child = this.JobList.SelectedComponent;
			if (child == null)
			{
				return false;
			}
			bool moveToNext = obj != null;
			NetLobbyScreen.<>c__DisplayClass322_0 CS$<>8__locals2 = CS$<>8__locals1;
			JobVariant jobVariant = obj as JobVariant;
			CS$<>8__locals2.jobPrefab = ((jobVariant != null) ? jobVariant.Prefab : null);
			object prevObj = child.UserData;
			GUIComponent existingChild = this.JobList.Content.FindChild(delegate(GUIComponent d)
			{
				JobVariant prefab = d.UserData as JobVariant;
				return prefab != null && prefab.Prefab == CS$<>8__locals1.jobPrefab;
			}, false);
			if (existingChild != null && obj != null)
			{
				existingChild.UserData = prevObj;
			}
			child.UserData = obj;
			for (int i = 0; i < 2; i++)
			{
				if (i < 2 && this.JobList.Content.GetChild(i).UserData == null)
				{
					this.JobList.Content.GetChild(i).UserData = this.JobList.Content.GetChild(i + 1).UserData;
					this.JobList.Content.GetChild(i + 1).UserData = null;
				}
			}
			CharacterInfo characterInfo;
			if ((characterInfo = GameMain.Client.CharacterInfo) == null)
			{
				Character controlled = Character.Controlled;
				characterInfo = ((controlled != null) ? controlled.Info : null);
			}
			this.UpdateJobPreferences(characterInfo);
			if (moveToNext)
			{
				GUIComponent emptyChild = this.JobList.Content.FindChild((GUIComponent c) => c.UserData == null && c.CanBeFocused, false);
				if (emptyChild != null)
				{
					this.JobList.Select(this.JobList.Content.GetChildIndex(emptyChild), GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
				}
				else
				{
					this.JobList.Deselect();
					if (this.JobSelectionFrame != null)
					{
						this.JobSelectionFrame.Visible = false;
					}
				}
			}
			else
			{
				this.OpenJobSelection(child, child.UserData);
			}
			return false;
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x00194BEC File Offset: 0x00192DEC
		private bool OpenJobSelection(GUIComponent _, object __)
		{
			NetLobbyScreen.<>c__DisplayClass323_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass323_0();
			CS$<>8__locals1.<>4__this = this;
			if (GameMain.GraphicsWidth != this.prevResolutionForJobSelectionFrame.X || GameMain.GraphicsHeight != this.prevResolutionForJobSelectionFrame.Y)
			{
				this.JobSelectionFrame = null;
			}
			this.prevResolutionForJobSelectionFrame = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			if (this.JobSelectionFrame != null)
			{
				this.JobSelectionFrame.Visible = true;
				return true;
			}
			IEnumerable<JobPrefab> allJobs = from jobPrefab in JobPrefab.Prefabs
			where !jobPrefab.HiddenJob && jobPrefab.MaxNumber > 0
			select jobPrefab;
			IEnumerable<JobVariant> availableJobs = from jobPrefab in allJobs
			where CS$<>8__locals1.<>4__this.JobList.Content.Children.All(delegate(GUIComponent c)
			{
				JobVariant prefab = c.UserData as JobVariant;
				return prefab == null || prefab.Prefab != jobPrefab;
			})
			select jobPrefab into j
			select new JobVariant(j, 0);
			availableJobs = availableJobs.Concat(from jobPrefab in allJobs
			where CS$<>8__locals1.<>4__this.JobList.Content.Children.Any(delegate(GUIComponent c)
			{
				JobVariant prefab = c.UserData as JobVariant;
				return prefab != null && prefab.Prefab == jobPrefab;
			})
			select jobPrefab into j
			select (JobVariant)CS$<>8__locals1.<>4__this.JobList.Content.FindChild(delegate(GUIComponent c)
			{
				JobVariant prefab = c.UserData as JobVariant;
				return prefab != null && prefab.Prefab == j;
			}, false).UserData);
			availableJobs = availableJobs.ToList<JobVariant>();
			CS$<>8__locals1.rowCount = (int)Math.Ceiling((double)((float)availableJobs.Count<JobVariant>() / 3f));
			CS$<>8__locals1.jobButtonSize = GUI.IntScale(150f);
			Point frameSize = new Point(this.characterInfoFrame.Rect.Width, (int)((float)(CS$<>8__locals1.jobButtonSize * Math.Min(CS$<>8__locals1.rowCount, 4)) / 0.95f));
			this.JobSelectionFrame = new GUIFrame(new RectTransform(frameSize, GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false), "GUIFrameListBox", null);
			CS$<>8__locals1.<OpenJobSelection>g__PositionJobSelectionFrame|6();
			this.characterInfoFrame.RectTransform.SizeChanged += delegate()
			{
				if (CS$<>8__locals1.<>4__this.characterInfoFrame != null)
				{
					GUIFrame jobSelectionFrame = CS$<>8__locals1.<>4__this.JobSelectionFrame;
					if (((jobSelectionFrame != null) ? jobSelectionFrame.RectTransform : null) != null)
					{
						Point size = new Point(CS$<>8__locals1.<>4__this.characterInfoFrame.Rect.Width, (int)((float)(CS$<>8__locals1.jobButtonSize * Math.Min(CS$<>8__locals1.rowCount, 4)) / 0.95f));
						CS$<>8__locals1.<>4__this.JobSelectionFrame.RectTransform.Resize(size, true);
						base.<OpenJobSelection>g__PositionJobSelectionFrame|6();
						return;
					}
				}
			};
			GUIListBox jobSelectionList = new GUIListBox(new RectTransform(Vector2.One * 0.95f, this.JobSelectionFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "GUIFrameListBox", true, false)
			{
				Padding = Vector4.One * (float)GUI.IntScale(10f)
			};
			GUILayoutGroup row = new GUILayoutGroup(new RectTransform(new Point(jobSelectionList.Content.Rect.Width, CS$<>8__locals1.jobButtonSize), jobSelectionList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			int itemsInRow = 0;
			foreach (JobVariant jobPrefab2 in availableJobs)
			{
				if (itemsInRow >= 3)
				{
					row = new GUILayoutGroup(new RectTransform(new Point(jobSelectionList.Content.Rect.Width, CS$<>8__locals1.jobButtonSize), jobSelectionList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
					{
						Stretch = true
					};
					itemsInRow = 0;
				}
				GUIButton guibutton = new GUIButton(new RectTransform(new Point(CS$<>8__locals1.jobButtonSize), row.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "ListBoxElementSquare", null);
				guibutton.UserData = jobPrefab2;
				GUIButton.OnClickedHandler onClicked;
				if ((onClicked = CS$<>8__locals1.<>9__10) == null)
				{
					onClicked = (CS$<>8__locals1.<>9__10 = ((GUIButton btn, object usdt) => !btn.IsParentOf(GUI.MouseOn, true) && CS$<>8__locals1.<>4__this.SwitchJob(btn, usdt)));
				}
				guibutton.OnClicked = onClicked;
				GUIButton jobButton = guibutton;
				itemsInRow++;
				GUIImage[] images = NetLobbyScreen.AddJobSpritesToGUIComponent(jobButton, jobPrefab2.Prefab, this.TeamPreference, this.SelectedMode == GameModePreset.PvP, false);
				if (images != null && images.Length != 0)
				{
					jobPrefab2.Variant = Math.Min(jobPrefab2.Variant, images.Length);
					int currVisible = jobPrefab2.Variant;
					GUIButton currSelected = null;
					GUIButton.OnClickedHandler <>9__11;
					for (int variantIndex = 0; variantIndex < images.Length; variantIndex++)
					{
						images[variantIndex].Visible = (currVisible == variantIndex);
						GUIButton variantButton = NetLobbyScreen.CreateJobVariantButton(jobPrefab2, variantIndex, images.Length, jobButton);
						GUIButton guibutton2 = variantButton;
						GUIButton.OnClickedHandler onClicked2;
						if ((onClicked2 = <>9__11) == null)
						{
							onClicked2 = (<>9__11 = delegate(GUIButton btn, object obj)
							{
								if (currSelected != null)
								{
									currSelected.Selected = false;
								}
								int selectedVariantIndex = ((JobVariant)obj).Variant;
								btn.Parent.UserData = obj;
								for (int i = 0; i < images.Length; i++)
								{
									images[i].Visible = (selectedVariantIndex == i);
								}
								currSelected = btn;
								currSelected.Selected = true;
								return false;
							});
						}
						guibutton2.OnClicked = onClicked2;
						if (currVisible == variantIndex)
						{
							currSelected = variantButton;
						}
					}
					if (currSelected != null)
					{
						currSelected.Selected = true;
					}
				}
			}
			return true;
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x0019507C File Offset: 0x0019327C
		private static GUIImage[] AddJobSpritesToGUIComponent(GUIComponent parent, JobPrefab jobPrefab, CharacterTeamType team, bool isPvPMode, bool selectedByPlayer)
		{
			List<Sprite> outfitPreviews = jobPrefab.GetJobOutfitSprites(team, isPvPMode).ToList<Sprite>();
			GUIFrame innerFrame = new GUIFrame(new RectTransform(Vector2.One * 0.85f, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUIImage[] retVal = new GUIImage[outfitPreviews.Count];
			if (outfitPreviews != null && outfitPreviews.Any<Sprite>())
			{
				for (int i = 0; i < outfitPreviews.Count; i++)
				{
					Sprite outfitPreview = outfitPreviews[i];
					float aspectRatio = outfitPreview.size.Y / outfitPreview.size.X;
					retVal[i] = new GUIImage(new RectTransform(new Vector2(0.7f / aspectRatio, 0.7f), innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), outfitPreview, true, null)
					{
						PressedColor = Color.White,
						CanBeFocused = false
					};
				}
			}
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1f, 0.35f), parent.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "OuterGlow", null);
			guiframe.Color = Color.Black;
			guiframe.HoverColor = Color.Black;
			guiframe.PressedColor = Color.Black;
			guiframe.SelectedColor = Color.Black;
			guiframe.CanBeFocused = false;
			GUITextBlock textBlock = new GUITextBlock((innerFrame.CountChildren == 0) ? new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal) : new RectTransform(new Vector2(selectedByPlayer ? 0.55f : 0.95f, 0.3f), parent.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), jobPrefab.Name, null, null, Alignment.BottomCenter, true, "", null)
			{
				Padding = Vector4.Zero,
				HoverColor = Color.Transparent,
				SelectedColor = Color.Transparent,
				TextColor = jobPrefab.UIColor,
				HoverTextColor = Color.Lerp(jobPrefab.UIColor, Color.White, 0.5f),
				CanBeFocused = false,
				AutoScaleHorizontal = true
			};
			textBlock.TextAlignment = (textBlock.WrappedText.Contains('\n', StringComparison.Ordinal) ? Alignment.BottomCenter : Alignment.Center);
			textBlock.RectTransform.SizeChanged += delegate()
			{
				textBlock.TextScale = 1f;
			};
			return retVal;
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x00195380 File Offset: 0x00193580
		public void SelectMode(int modeIndex)
		{
			if (modeIndex < 0 || modeIndex >= this.ModeList.Content.CountChildren)
			{
				return;
			}
			if ((GameModePreset)this.ModeList.Content.GetChild(modeIndex).UserData != GameModePreset.MultiPlayerCampaign)
			{
				this.ToggleCampaignMode(false);
			}
			GameModePreset prevMode = this.ModeList.Content.GetChild(this.selectedModeIndex).UserData as GameModePreset;
			if ((this.HighlightedModeIndex == this.selectedModeIndex || this.HighlightedModeIndex < 0) && this.ModeList.SelectedIndex != modeIndex)
			{
				this.ModeList.Select(modeIndex, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
			}
			this.selectedModeIndex = modeIndex;
			if (prevMode == GameModePreset.PvP != (this.SelectedMode == GameModePreset.PvP))
			{
				this.SaveAppearance();
				this.UpdatePlayerFrame(null, true);
				GameMain.Client.ConnectedClients.ForEach(new Action<Client>(this.SetPlayerNameAndJobPreference));
				this.ResetPvpTeamSelection();
			}
			if (this.SelectedMode != GameModePreset.MultiPlayerCampaign)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && Screen.Selected == this)
				{
					GameMain.GameSession = null;
				}
			}
			this.respawnModeSelection.Refresh();
			this.RefreshGameModeContent();
			this.RefreshEnabledElements();
			this.UpdateDisembarkPointListFromServerSettings();
		}

		// Token: 0x06002647 RID: 9799 RVA: 0x001954C2 File Offset: 0x001936C2
		public void HighlightMode(int modeIndex)
		{
			if (modeIndex < 0 || modeIndex >= this.ModeList.Content.CountChildren)
			{
				return;
			}
			this.HighlightedModeIndex = modeIndex;
			this.RefreshGameModeContent();
			this.RefreshEnabledElements();
		}

		// Token: 0x06002648 RID: 9800 RVA: 0x001954F0 File Offset: 0x001936F0
		private void RefreshMissionTypes()
		{
			IEnumerable<Type> suitableMissionClasses;
			if (this.SelectedMode == GameModePreset.Mission)
			{
				suitableMissionClasses = MissionPrefab.CoOpMissionClasses.Values;
			}
			else
			{
				if (this.SelectedMode != GameModePreset.PvP)
				{
					return;
				}
				suitableMissionClasses = MissionPrefab.PvPMissionClasses.Values;
			}
			for (int i = 0; i < this.missionTypeTickBoxes.Length; i++)
			{
				Identifier missionType = (Identifier)this.missionTypeTickBoxes[i].UserData;
				this.missionTypeTickBoxes[i].Parent.Visible = MissionPrefab.Prefabs.Any(delegate(MissionPrefab p)
				{
					Identifier type = p.Type;
					return type == missionType && suitableMissionClasses.Contains(p.MissionClass);
				});
			}
		}

		// Token: 0x06002649 RID: 9801 RVA: 0x001955A4 File Offset: 0x001937A4
		private void RefreshGameModeSettingsContent()
		{
			foreach (GUIComponent element in this.campaignHiddenElements)
			{
				NetLobbyScreen.<RefreshGameModeSettingsContent>g__SetElementVisible|328_0(element, this.SelectedMode != GameModePreset.MultiPlayerCampaign && this.SelectedMode != GameModePreset.SinglePlayerCampaign);
			}
			foreach (GUIComponent element2 in this.pvpOnlyElements)
			{
				NetLobbyScreen.<RefreshGameModeSettingsContent>g__SetElementVisible|328_0(element2, this.SelectedMode == GameModePreset.PvP);
			}
			if (this.respawnTabButton != null && this.upgradesTabButton != null)
			{
				if (this.SelectedMode == GameModePreset.MultiPlayerCampaign)
				{
					this.SelectRespawnTab();
					this.respawnTabButton.Enabled = (this.upgradesTabButton.Enabled = false);
				}
				else
				{
					this.respawnTabButton.Enabled = (this.upgradesTabButton.Enabled = true);
				}
			}
			this.gameModeSettingsLayout.Recalculate();
		}

		// Token: 0x0600264A RID: 9802 RVA: 0x001956CC File Offset: 0x001938CC
		private void RefreshGameModeContent()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			foreach (GUIComponent subElement in this.SubList.Content.Children)
			{
				subElement.CanBeFocused = true;
				foreach (GUITextBlock textBlock in subElement.GetAllChildren<GUITextBlock>())
				{
					textBlock.Enabled = true;
				}
			}
			this.SubList.Content.RectTransform.SortChildren(delegate(RectTransform rt1, RectTransform rt2)
			{
				SubmarineInfo s = rt1.GUIComponent.UserData as SubmarineInfo;
				SubmarineInfo s2 = rt2.GUIComponent.UserData as SubmarineInfo;
				return s.Name.CompareTo(s2.Name);
			});
			this.autoRestartBox.Parent.Visible = true;
			this.UpdateDisembarkPointListFromServerSettings();
			bool isPvP = this.SelectedMode == GameModePreset.PvP;
			foreach (GUIComponent child in this.SubList.Content.Children)
			{
				GUILayoutGroup container = child.GetChild<GUILayoutGroup>();
				GUIFrame imageFrame = container.GetChild<GUIFrame>();
				GUIComponent coalIcon = imageFrame.GetChildByUserData("coalitionIcon");
				GUIComponent sepIcon = imageFrame.GetChildByUserData("separatistsIcon");
				coalIcon.Visible = isPvP;
				sepIcon.Visible = isPvP;
				if (GameMain.NetworkMember.ServerSettings.SubSelectionMode != SelectionMode.Vote)
				{
					coalIcon.Enabled = (sepIcon.Enabled = false);
					SubmarineInfo info = child.UserData as SubmarineInfo;
					if (info != null)
					{
						if (this.SelectedSub == info)
						{
							coalIcon.Enabled = true;
						}
						if (this.SelectedEnemySub == info)
						{
							sepIcon.Enabled = true;
						}
					}
				}
			}
			this.UpdateSelectedSub(isPvP ? MultiplayerPreferences.Instance.TeamPreference : CharacterTeamType.None);
			this.RefreshGameModeSettingsContent();
			if (this.SelectedMode == GameModePreset.Mission || this.SelectedMode == GameModePreset.PvP)
			{
				this.MissionTypeFrame.Visible = true;
				this.CampaignFrame.Visible = (this.CampaignSetupFrame.Visible = false);
				this.RefreshMissionTypes();
			}
			else if (this.SelectedMode == GameModePreset.MultiPlayerCampaign)
			{
				this.MissionTypeFrame.Visible = (this.autoRestartBox.Parent.Visible = false);
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null && campaign.Map != null)
				{
					this.CampaignFrame.Visible = (this.QuitCampaignButton.Enabled = CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageRound));
					this.CampaignSetupFrame.Visible = false;
				}
				else
				{
					this.CampaignFrame.Visible = false;
					this.CampaignSetupFrame.Visible = true;
					if (!CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageRound))
					{
						this.CampaignSetupFrame.ClearChildren();
						RectTransform rectT = new RectTransform(new Vector2(0.8f, 0.5f), this.CampaignSetupFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
						RichString text = TextManager.Get("campaignstarting");
						GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
						new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, true, "", null);
					}
				}
				if (this.CampaignSetupUI != null)
				{
					foreach (GUIComponent subElement2 in this.SubList.Content.Children)
					{
						SubmarineInfo sub = subElement2.UserData as SubmarineInfo;
						bool tooExpensive = sub.Price > CampaignSettings.CurrentSettings.InitialMoney;
						if (tooExpensive || !sub.IsCampaignCompatible)
						{
							foreach (GUITextBlock textBlock2 in subElement2.GetAllChildren<GUITextBlock>())
							{
								textBlock2.DisabledTextColor = ((textBlock2.UserData as string == "pricetext" && tooExpensive) ? GUIStyle.Red : GUIStyle.TextColorNormal) * 0.7f;
								textBlock2.Enabled = false;
							}
						}
					}
					this.SubList.Content.RectTransform.SortChildren(delegate(RectTransform rt1, RectTransform rt2)
					{
						SubmarineInfo s = rt1.GUIComponent.UserData as SubmarineInfo;
						SubmarineInfo s2 = rt2.GUIComponent.UserData as SubmarineInfo;
						int p = s.Price;
						if (!s.IsCampaignCompatible)
						{
							p += 100000;
						}
						int p2 = s2.Price;
						if (!s2.IsCampaignCompatible)
						{
							p2 += 100000;
						}
						return p.CompareTo(p2) * 100 + s.Name.CompareTo(s2.Name);
					});
				}
			}
			else
			{
				this.MissionTypeFrame.Visible = (this.CampaignFrame.Visible = (this.CampaignSetupFrame.Visible = false));
				this.CampaignFrame.Visible = (this.CampaignSetupFrame.Visible = false);
			}
			this.ReadyToStartBox.Parent.Visible = !GameMain.Client.GameStarted;
			this.RefreshStartButtonVisibility();
			this.RefreshOutpostDropdown();
		}

		// Token: 0x0600264B RID: 9803 RVA: 0x00195BE8 File Offset: 0x00193DE8
		public void RefreshStartButtonVisibility()
		{
			GameSession gameSession = GameMain.GameSession;
			bool campaignActive = ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode;
			if (this.CampaignSetupUI != null)
			{
				GUIFrame campaignSetupFrame = this.CampaignSetupFrame;
				if (campaignSetupFrame != null && campaignSetupFrame.Visible)
				{
					this.StartButton.Visible = (!GameMain.Client.GameStarted && !this.CampaignSetupUI.LoadGameMenuVisible && (GameMain.Client.HasPermission(ClientPermissions.ManageRound) || GameMain.Client.HasPermission(ClientPermissions.ManageCampaign)));
					goto IL_B2;
				}
			}
			this.StartButton.Visible = ((this.SelectedMode != GameModePreset.MultiPlayerCampaign || campaignActive) && !GameMain.Client.GameStarted && GameMain.Client.HasPermission(ClientPermissions.ManageRound));
			IL_B2:
			this.StartButton.Enabled = true;
			if (GameSession.ShouldApplyDisembarkPoints(this.SelectedMode))
			{
				this.StartButton.Enabled = GameSession.ValidatedDisembarkPoints(this.SelectedMode, this.MissionTypes);
				this.StartButton.ToolTip = ((!this.StartButton.Enabled) ? TextManager.Get("DisembarkPointsNotValid") : string.Empty);
			}
			this.StartButton.IgnoreLayoutGroups = !this.StartButton.Visible;
			GUIComponent endButton = this.EndButton;
			bool visible;
			if (!this.StartButton.Visible)
			{
				GameClient client = GameMain.Client;
				if (client != null && client.GameStarted)
				{
					visible = (GameMain.Client.HasPermission(ClientPermissions.ManageRound) || (campaignActive && GameMain.Client.HasPermission(ClientPermissions.ManageCampaign)));
					goto IL_17C;
				}
			}
			visible = false;
			IL_17C:
			endButton.Visible = visible;
			this.EndButton.IgnoreLayoutGroups = !this.EndButton.Visible;
		}

		// Token: 0x0600264C RID: 9804 RVA: 0x00195D90 File Offset: 0x00193F90
		public void RefreshChatrow()
		{
			this.chatRow.ClearChildren();
			if (this.SelectedMode == GameModePreset.PvP)
			{
				GameClient client = GameMain.Client;
				bool flag;
				if (client == null)
				{
					flag = false;
				}
				else
				{
					ServerSettings serverSettings = client.ServerSettings;
					flag = (((serverSettings != null) ? new PvpTeamSelectionMode?(serverSettings.PvpTeamSelectionMode) : null).GetValueOrDefault() == PvpTeamSelectionMode.PlayerChoice);
				}
				if (flag && MultiplayerPreferences.Instance.TeamPreference != CharacterTeamType.None)
				{
					RectTransform chatSelectorRT = new RectTransform(new Vector2(0.25f, 1f), this.chatRow.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
					GUIDropDown guidropDown = new GUIDropDown(chatSelectorRT, null, 2, "", false, false, Alignment.CenterLeft, 1f);
					guidropDown.OnSelected = delegate(GUIComponent _, object userdata)
					{
						NetLobbyScreen.TeamChatSelected = (bool)userdata;
						return true;
					};
					this.chatSelector = guidropDown;
					this.chatSelector.AddItem(TextManager.Get("PvP.ChatMode.Team"), true, null, new Color?(ChatMessage.MessageColor[12]), null);
					this.chatSelector.AddItem(TextManager.Get("PvP.ChatMode.All"), false, null, new Color?(ChatMessage.MessageColor[0]), null);
					this.chatSelector.SelectItem(NetLobbyScreen.TeamChatSelected);
					goto IL_16E;
				}
			}
			NetLobbyScreen.TeamChatSelected = false;
			IL_16E:
			if (this.chatInput != null)
			{
				this.chatInput.RectTransform.Parent = this.chatRow.RectTransform;
			}
			else
			{
				this.chatInput = new GUITextBox(new RectTransform(new Vector2(0.75f, 1f), this.chatRow.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
				{
					MaxTextLength = new int?(200),
					Font = GUIStyle.SmallFont,
					DeselectAfterMessage = false
				};
				this.micIcon = new GUIImage(new RectTransform(new Vector2(0.05f, 1f), this.chatRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIMicrophoneUnavailable", GUIImage.ScalingMode.None);
				this.chatInput.Select(-1, true);
			}
			if (GameMain.Client != null)
			{
				this.chatInput.ResetDelegates();
				this.chatInput.OnEnterPressed = new GUITextBox.OnEnterHandler(GameMain.Client.EnterChatMessage);
				this.chatInput.OnTextChanged += GameMain.Client.TypingChatMessage;
				this.chatInput.OnDeselected += delegate(GUITextBox sender, Keys key)
				{
					GameClient client2 = GameMain.Client;
					if (client2 == null)
					{
						return;
					}
					client2.ChatBox.ChatManager.Clear();
				};
				ChatManager.RegisterKeys(this.chatInput, GameMain.Client.ChatBox.ChatManager);
			}
			this.chatRow.Recalculate();
		}

		// Token: 0x0600264D RID: 9805 RVA: 0x001960B4 File Offset: 0x001942B4
		public void ToggleCampaignMode(bool enabled)
		{
			if (!enabled)
			{
				if (this.campaignCharacterInfo != null)
				{
					this.campaignCharacterInfo = null;
					this.UpdatePlayerFrame(null, true);
					this.SetSpectate(this.spectateBox.Selected);
				}
				this.CampaignCharacterDiscarded = false;
			}
			this.RefreshEnabledElements();
			if (enabled && this.SelectedMode != GameModePreset.MultiPlayerCampaign)
			{
				this.ModeList.Select(GameModePreset.MultiPlayerCampaign, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled);
			}
		}

		// Token: 0x0600264E RID: 9806 RVA: 0x0019611C File Offset: 0x0019431C
		public void TryDisplayCampaignSubmarine(SubmarineInfo submarine)
		{
			string name = (submarine != null) ? submarine.Name : null;
			bool displayed = false;
			GUIListBox subList = this.SubList;
			subList.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Remove(subList.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
			this.SubList.Deselect();
			this.subPreviewContainer.ClearChildren();
			foreach (GUIComponent child in this.SubList.Content.Children)
			{
				SubmarineInfo sub = child.UserData as SubmarineInfo;
				if (sub != null && sub.Name == name)
				{
					this.SubList.Select(sub, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
					if (SubmarineInfo.SavedSubmarines.Contains(sub))
					{
						this.CreateSubPreview(sub);
						displayed = true;
						break;
					}
					break;
				}
			}
			GUIListBox subList2 = this.SubList;
			subList2.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(subList2.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
			if (!displayed)
			{
				this.CreateSubPreview(submarine);
			}
			this.UpdateSubVisibility();
		}

		// Token: 0x0600264F RID: 9807 RVA: 0x0019623C File Offset: 0x0019443C
		private bool ViewJobInfo(GUIButton button, object obj)
		{
			JobVariant jobPrefab = button.UserData as JobVariant;
			if (jobPrefab == null)
			{
				return false;
			}
			GUIComponent buttonContainer;
			NetLobbyScreen.JobInfoFrame = jobPrefab.Prefab.CreateInfoFrame(this.SelectedMode == GameModePreset.PvP, out buttonContainer);
			new GUIButton(new RectTransform(new Vector2(0.25f, 0.05f), buttonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null).OnClicked = new GUIButton.OnClickedHandler(this.CloseJobInfo);
			NetLobbyScreen.JobInfoFrame.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock)
				{
					this.CloseJobInfo(btn, userdata);
				}
				return true;
			};
			return true;
		}

		// Token: 0x06002650 RID: 9808 RVA: 0x001962FC File Offset: 0x001944FC
		private bool CloseJobInfo(GUIButton button, object obj)
		{
			NetLobbyScreen.JobInfoFrame = null;
			return true;
		}

		// Token: 0x06002651 RID: 9809 RVA: 0x00196308 File Offset: 0x00194508
		private void UpdateJobPreferences(CharacterInfo characterInfo)
		{
			if (characterInfo == null)
			{
				return;
			}
			GUICustomComponent characterIcon = this.JobPreferenceContainer.GetChild<GUICustomComponent>();
			this.JobPreferenceContainer.RemoveChild(characterIcon);
			characterInfo.CreateIcon(new RectTransform(new Vector2(1f, 0.4f), this.JobPreferenceContainer.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.025f)
			});
			GUIListBox listBox = this.JobPreferenceContainer.GetChild<GUIListBox>();
			List<MultiplayerPreferences.JobPreference> jobPreferences = new List<MultiplayerPreferences.JobPreference>();
			bool disableNext = false;
			GUIButton.OnClickedHandler <>9__0;
			GUIButton.OnClickedHandler <>9__1;
			for (int i = 0; i < listBox.Content.CountChildren; i++)
			{
				GUIComponent slot = listBox.Content.GetChild(i);
				slot.ClearChildren();
				slot.CanBeFocused = !disableNext;
				JobVariant jobPrefab = slot.UserData as JobVariant;
				if (jobPrefab != null)
				{
					GUIImage[] images = NetLobbyScreen.AddJobSpritesToGUIComponent(slot, jobPrefab.Prefab, this.TeamPreference, this.SelectedMode == GameModePreset.PvP, true);
					for (int variantIndex = 0; variantIndex < images.Length; variantIndex++)
					{
						int selectedVariantIndex = Math.Min(jobPrefab.Variant, images.Length);
						images[variantIndex].Visible = (images.Length == 1 || selectedVariantIndex == variantIndex);
						if (images.Length != 0)
						{
							GUIButton variantButton = NetLobbyScreen.CreateJobVariantButton(jobPrefab, variantIndex, images.Length, slot);
							GUIButton guibutton = variantButton;
							GUIButton.OnClickedHandler onClicked;
							if ((onClicked = <>9__0) == null)
							{
								onClicked = (<>9__0 = delegate(GUIButton btn, object obj)
								{
									btn.Parent.UserData = obj;
									this.UpdateJobPreferences(characterInfo);
									return false;
								});
							}
							guibutton.OnClicked = onClicked;
						}
					}
					GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.15f), slot.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.BothWidth)
					{
						RelativeOffset = new Vector2(0.075f)
					}, Alignment.Center, "GUIButtonInfo", null);
					guibutton2.UserData = jobPrefab;
					guibutton2.OnClicked = new GUIButton.OnClickedHandler(this.ViewJobInfo);
					GUIButton guibutton3 = new GUIButton(new RectTransform(new Vector2(0.15f), slot.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothWidth)
					{
						RelativeOffset = new Vector2(0.075f)
					}, Alignment.Center, "GUICancelButton", null);
					guibutton3.UserData = i;
					GUIButton.OnClickedHandler onClicked2;
					if ((onClicked2 = <>9__1) == null)
					{
						onClicked2 = (<>9__1 = delegate(GUIButton btn, object obj)
						{
							this.JobList.Select((int)obj, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
							this.SwitchJob(btn, null);
							if (this.JobSelectionFrame != null)
							{
								this.JobSelectionFrame.Visible = false;
							}
							this.JobList.Deselect();
							return false;
						});
					}
					guibutton3.OnClicked = onClicked2;
					jobPreferences.Add(new MultiplayerPreferences.JobPreference(jobPrefab.Prefab.Identifier, jobPrefab.Variant));
				}
				else
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.6f), slot.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), (i + 1).ToString(), new Color?(Color.White * (disableNext ? 0.15f : 0.5f)), GUIStyle.LargeFont, Alignment.Center, false, "", null).CanBeFocused = false;
					if (!disableNext)
					{
						RectTransform rectT = new RectTransform(new Vector2(1f, 0.4f), slot.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
						RichString text = TextManager.Get("clicktoselectjob");
						GUIFont smallFont = GUIStyle.SmallFont;
						new GUITextBlock(rectT, text, null, smallFont, Alignment.Center, true, "", null).CanBeFocused = false;
					}
					disableNext = true;
				}
			}
			GameMain.Client.ForceNameJobTeamUpdate();
			if (!MultiplayerPreferences.Instance.AreJobPreferencesEqual(jobPreferences))
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null && gameSession.IsRunning)
				{
					TabMenu.PendingChanges = true;
					this.CreateChangesPendingText();
				}
				MultiplayerPreferences.Instance.JobPreferences.Clear();
				MultiplayerPreferences.Instance.JobPreferences.AddRange(jobPreferences);
				GameSettings.SaveCurrentConfig();
			}
		}

		// Token: 0x06002652 RID: 9810 RVA: 0x00196748 File Offset: 0x00194948
		private static GUIButton CreateJobVariantButton(JobVariant jobPrefab, int variantIndex, int variantCount, GUIComponent slot)
		{
			float relativeSize = 0.18f;
			return new GUIButton(new RectTransform(new Vector2(relativeSize), slot.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.BothHeight)
			{
				RelativeOffset = new Vector2(relativeSize * 1.3f * ((float)variantIndex - (float)(variantCount - 1) / 2f), 0.02f)
			}, (variantIndex + 1).ToString(), Alignment.Center, "JobVariantButton", null)
			{
				Selected = (jobPrefab.Variant == variantIndex),
				UserData = new JobVariant(jobPrefab.Prefab, variantIndex)
			};
		}

		// Token: 0x06002653 RID: 9811 RVA: 0x001967F8 File Offset: 0x001949F8
		public bool TrySelectSub(string subName, string md5Hash, SelectedSubType type, GUIListBox subList, bool showPreview = true)
		{
			this.UpdateSubVisibility();
			if (GameMain.Client == null)
			{
				return false;
			}
			if (GameMain.Client.FileReceiver.ActiveTransfers.Any((FileReceiver.FileTransferIn t) => t.FileName == subName + ".sub"))
			{
				return false;
			}
			GUIComponent guicomponent = subList.Content.Children.FirstOrDefault(delegate(GUIComponent c)
			{
				SubmarineInfo s = c.UserData as SubmarineInfo;
				if (s != null && s.Name == subName)
				{
					Md5Hash md5Hash5 = s.MD5Hash;
					return ((md5Hash5 != null) ? md5Hash5.StringRepresentation : null) == md5Hash;
				}
				return false;
			});
			SubmarineInfo sub = ((guicomponent != null) ? guicomponent.UserData : null) as SubmarineInfo;
			if (sub != null)
			{
				if (subList == this.SubList && showPreview && (type != SelectedSubType.EnemySub || MultiplayerPreferences.Instance.TeamPreference == CharacterTeamType.Team2))
				{
					this.CreateSubPreview(sub);
				}
				SubmarineInfo submarineInfo;
				switch (type)
				{
				case SelectedSubType.Shuttle:
					submarineInfo = this.SelectedShuttle;
					break;
				case SelectedSubType.Sub:
					submarineInfo = this.SelectedSub;
					break;
				case SelectedSubType.EnemySub:
					submarineInfo = this.SelectedEnemySub;
					break;
				default:
					submarineInfo = null;
					break;
				}
				SubmarineInfo selectedSub = submarineInfo;
				if (selectedSub != null)
				{
					Md5Hash md5Hash2 = selectedSub.MD5Hash;
					if (((md5Hash2 != null) ? md5Hash2.StringRepresentation : null) == md5Hash && File.Exists(sub.FilePath))
					{
						if (type != SelectedSubType.Sub)
						{
							if (type == SelectedSubType.EnemySub)
							{
								this.SelectedEnemySub = sub;
							}
						}
						else
						{
							this.SelectedSub = sub;
						}
						return true;
					}
				}
			}
			if (sub == null)
			{
				GUIComponent guicomponent2 = subList.Content.Children.FirstOrDefault(delegate(GUIComponent c)
				{
					SubmarineInfo s = c.UserData as SubmarineInfo;
					return s != null && s.Name == subName;
				});
				sub = (((guicomponent2 != null) ? guicomponent2.UserData : null) as SubmarineInfo);
			}
			if (sub != null)
			{
				GUIDropDown subDropDown = subList.Parent as GUIDropDown;
				if (subDropDown != null)
				{
					subDropDown.SelectItem(sub);
				}
				else
				{
					subList.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Remove(subList.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
					CharacterTeamType preference = MultiplayerPreferences.Instance.TeamPreference;
					if (type != SelectedSubType.Sub)
					{
						if (type == SelectedSubType.EnemySub)
						{
							if (preference == CharacterTeamType.Team2)
							{
								subList.Select(sub, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
							}
							this.SelectedEnemySub = sub;
						}
					}
					else
					{
						bool flag = preference <= CharacterTeamType.Team1;
						if (flag)
						{
							subList.Select(sub, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
						}
						this.SelectedSub = sub;
					}
					subList.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(subList.OnSelected, new GUIListBox.OnSelectedHandler(this.VotableClicked));
				}
				switch (type)
				{
				case SelectedSubType.Shuttle:
					this.FailedSelectedShuttle = null;
					break;
				case SelectedSubType.Sub:
					this.FailedSelectedSub = null;
					break;
				case SelectedSubType.EnemySub:
					this.FailedSelectedEnemySub = null;
					break;
				}
				Md5Hash md5Hash3 = sub.MD5Hash;
				if (((md5Hash3 != null) ? md5Hash3.StringRepresentation : null) == md5Hash && SubmarineInfo.SavedSubmarines.Contains(sub))
				{
					return true;
				}
			}
			switch (type)
			{
			case SelectedSubType.Shuttle:
				this.FailedSelectedShuttle = new NetLobbyScreen.FailedSubInfo?(new NetLobbyScreen.FailedSubInfo(subName, md5Hash));
				break;
			case SelectedSubType.Sub:
				this.FailedSelectedSub = new NetLobbyScreen.FailedSubInfo?(new NetLobbyScreen.FailedSubInfo(subName, md5Hash));
				break;
			case SelectedSubType.EnemySub:
				this.FailedSelectedEnemySub = new NetLobbyScreen.FailedSubInfo?(new NetLobbyScreen.FailedSubInfo(subName, md5Hash));
				break;
			}
			LocalizedString errorMsg = "";
			if (sub == null || !SubmarineInfo.SavedSubmarines.Contains(sub))
			{
				errorMsg = TextManager.GetWithVariable("SubNotFoundError", "[subname]", subName, FormatCapitals.No) + " ";
			}
			else
			{
				Md5Hash md5Hash4 = sub.MD5Hash;
				if (((md5Hash4 != null) ? md5Hash4.StringRepresentation : null) == null)
				{
					errorMsg = TextManager.GetWithVariable("SubLoadError", "[subname]", subName, FormatCapitals.No) + " ";
					GUIComponent childByUserData = subList.Content.GetChildByUserData(sub);
					GUITextBlock textBlock = (childByUserData != null) ? childByUserData.GetChild<GUITextBlock>() : null;
					if (textBlock != null)
					{
						textBlock.TextColor = GUIStyle.Red;
					}
				}
				else
				{
					errorMsg = TextManager.GetWithVariables("SubDoesntMatchError", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[subname]", sub.Name),
						new ValueTuple<string, string>("[myhash]", sub.MD5Hash.ShortRepresentation),
						new ValueTuple<string, string>("[serverhash]", Md5Hash.GetShortHash(md5Hash))
					}) + " ";
				}
			}
			if (GameMain.Client.ServerSettings.AllowFileTransfers)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.RequestFile(FileTransferType.Submarine, subName, md5Hash);
				}
			}
			else
			{
				new GUIMessageBox(TextManager.Get("DownloadSubLabel"), errorMsg, null, null, GUIMessageBox.Type.Default);
			}
			return false;
		}

		// Token: 0x06002654 RID: 9812 RVA: 0x00196C78 File Offset: 0x00194E78
		public bool CheckIfCampaignSubMatches(SubmarineInfo serverSubmarine, NetLobbyScreen.SubmarineDeliveryData deliveryData)
		{
			if (GameMain.Client == null)
			{
				return false;
			}
			if (GameMain.Client.FileReceiver.ActiveTransfers.Any((FileReceiver.FileTransferIn t) => t.FileName == serverSubmarine.Name + ".sub"))
			{
				return false;
			}
			SubmarineInfo purchasableSub = SubmarineInfo.SavedSubmarines.FirstOrDefault(delegate(SubmarineInfo s)
			{
				if (s.Name == serverSubmarine.Name)
				{
					Md5Hash md5Hash = s.MD5Hash;
					string a = (md5Hash != null) ? md5Hash.StringRepresentation : null;
					Md5Hash md5Hash2 = serverSubmarine.MD5Hash;
					return a == ((md5Hash2 != null) ? md5Hash2.StringRepresentation : null);
				}
				return false;
			});
			if (purchasableSub != null)
			{
				return true;
			}
			NetLobbyScreen.FailedSubInfo fileInfo = new NetLobbyScreen.FailedSubInfo(serverSubmarine.Name, serverSubmarine.MD5Hash.StringRepresentation);
			if (deliveryData != NetLobbyScreen.SubmarineDeliveryData.Owned)
			{
				if (deliveryData == NetLobbyScreen.SubmarineDeliveryData.Campaign)
				{
					this.FailedCampaignSubs.Add(fileInfo);
				}
			}
			else
			{
				this.FailedOwnedSubs.Add(fileInfo);
			}
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.RequestFile(FileTransferType.Submarine, fileInfo.Name, fileInfo.Hash);
			}
			return false;
		}

		// Token: 0x06002655 RID: 9813 RVA: 0x00196D3E File Offset: 0x00194F3E
		private void CreateSubPreview(SubmarineInfo sub)
		{
			GUIComponent guicomponent = this.subPreviewContainer;
			if (guicomponent != null)
			{
				guicomponent.ClearChildren();
			}
			sub.CreatePreviewWindow(this.subPreviewContainer);
			this.RecalculateSubDescription();
		}

		// Token: 0x06002656 RID: 9814 RVA: 0x00196D64 File Offset: 0x00194F64
		private void RecalculateSubDescription()
		{
			GUIComponent guicomponent = this.subPreviewContainer;
			GUIComponent descriptionBox = (guicomponent != null) ? guicomponent.FindChild("descriptionbox", true) : null;
			if (descriptionBox != null && this.characterInfoFrame != null && (float)Math.Abs(descriptionBox.Rect.Height - this.characterInfoFrame.Rect.Height) < 80f * GUI.Scale)
			{
				descriptionBox.RectTransform.MaxSize = new Point(descriptionBox.Rect.Width, this.characterInfoFrame.Rect.Height);
			}
		}

		// Token: 0x06002657 RID: 9815 RVA: 0x00196DF0 File Offset: 0x00194FF0
		private void CreateSubmarineVisibilityMenu()
		{
			NetLobbyScreen.<>c__DisplayClass352_0 CS$<>8__locals1 = new NetLobbyScreen.<>c__DisplayClass352_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.messageBox = new GUIMessageBox(TextManager.Get("SubmarineVisibility"), "", Array.Empty<LocalizedString>(), new Vector2?(new Vector2(0.75f, 0.75f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			CS$<>8__locals1.messageBox.Content.ChildAnchor = Anchor.TopCenter;
			CS$<>8__locals1.columns = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), CS$<>8__locals1.messageBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			CS$<>8__locals1.visibleSubsList = CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__createColumnListBox|1("VisibleSubmarines");
			CS$<>8__locals1.centerColumn = CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__createColumn|0(0.1f);
			CS$<>8__locals1.hiddenSubsList = CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__createColumnListBox|1("HiddenSubmarines");
			IEnumerable<SubmarineInfo> serverSubmarines = GameMain.Client.ServerSubmarines;
			Func<SubmarineInfo, bool> keySelector;
			if ((keySelector = CS$<>8__locals1.<>9__13) == null)
			{
				keySelector = (CS$<>8__locals1.<>9__13 = ((SubmarineInfo s) => CS$<>8__locals1.<>4__this.visibilityMenuOrder.Contains(s)));
			}
			IOrderedEnumerable<SubmarineInfo> source = serverSubmarines.OrderBy(keySelector);
			Func<SubmarineInfo, int> keySelector2;
			if ((keySelector2 = CS$<>8__locals1.<>9__14) == null)
			{
				keySelector2 = (CS$<>8__locals1.<>9__14 = ((SubmarineInfo s) => CS$<>8__locals1.<>4__this.visibilityMenuOrder.IndexOf(s)));
			}
			foreach (SubmarineInfo sub in source.ThenBy(keySelector2))
			{
				CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__addSubToList|5(sub, GameMain.Client.ServerSettings.HiddenSubs.Contains(sub.Name) ? CS$<>8__locals1.hiddenSubsList : CS$<>8__locals1.visibleSubsList);
			}
			CS$<>8__locals1.visibleSubsList.OnRearranged = new GUIListBox.OnRearrangedHandler(CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__onRearranged|6);
			CS$<>8__locals1.hiddenSubsList.OnRearranged = new GUIListBox.OnRearrangedHandler(CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__onRearranged|6);
			CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__centerSpacing|3();
			CS$<>8__locals1.visibleToHidden = CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__centerButton|4("GUIButtonToggleRight");
			CS$<>8__locals1.visibleToHidden.OnClicked = delegate(GUIButton button, object o)
			{
				NetLobbyScreen.<CreateSubmarineVisibilityMenu>g__swapListItems|352_7(CS$<>8__locals1.visibleSubsList, CS$<>8__locals1.hiddenSubsList);
				return false;
			};
			CS$<>8__locals1.hiddenToVisible = CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__centerButton|4("GUIButtonToggleLeft");
			CS$<>8__locals1.hiddenToVisible.OnClicked = delegate(GUIButton button, object o)
			{
				NetLobbyScreen.<CreateSubmarineVisibilityMenu>g__swapListItems|352_7(CS$<>8__locals1.hiddenSubsList, CS$<>8__locals1.visibleSubsList);
				return false;
			};
			CS$<>8__locals1.<CreateSubmarineVisibilityMenu>g__centerSpacing|3();
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 0.1f), CS$<>8__locals1.messageBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
			{
				CS$<>8__locals1.messageBox.Close();
				return false;
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("OK"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
			{
				HashSet<string> hiddenSubs = GameMain.Client.ServerSettings.HiddenSubs;
				hiddenSubs.Clear();
				hiddenSubs.UnionWith(from c in CS$<>8__locals1.hiddenSubsList.Content.Children
				select (c.UserData as SubmarineInfo).Name);
				GameMain.Client.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.HiddenSubs, default(Identifier), default(Identifier), 0);
				CS$<>8__locals1.messageBox.Close();
				return false;
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.messageBox.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				GUIComponent draggedElement = CS$<>8__locals1.visibleSubsList.DraggedElement;
				if (draggedElement != null)
				{
					draggedElement.DrawManually(spriteBatch, true, true);
				}
				GUIComponent draggedElement2 = CS$<>8__locals1.hiddenSubsList.DraggedElement;
				if (draggedElement2 == null)
				{
					return;
				}
				draggedElement2.DrawManually(spriteBatch, true, true);
			}, delegate(float f, GUICustomComponent component)
			{
				NetLobbyScreen.<CreateSubmarineVisibilityMenu>g__handleDraggingAcrossLists|352_2(CS$<>8__locals1.visibleSubsList, CS$<>8__locals1.hiddenSubsList);
				NetLobbyScreen.<CreateSubmarineVisibilityMenu>g__handleDraggingAcrossLists|352_2(CS$<>8__locals1.hiddenSubsList, CS$<>8__locals1.visibleSubsList);
				if (PlayerInput.PrimaryMouseButtonClicked() && !GUI.IsMouseOn(CS$<>8__locals1.visibleToHidden) && !GUI.IsMouseOn(CS$<>8__locals1.hiddenToVisible))
				{
					if (!GUI.IsMouseOn(CS$<>8__locals1.hiddenSubsList) || !CS$<>8__locals1.hiddenSubsList.Content.IsParentOf(GUI.MouseOn, true))
					{
						CS$<>8__locals1.hiddenSubsList.Deselect();
					}
					if (!GUI.IsMouseOn(CS$<>8__locals1.visibleSubsList) || !CS$<>8__locals1.visibleSubsList.Content.IsParentOf(GUI.MouseOn, true))
					{
						CS$<>8__locals1.visibleSubsList.Deselect();
					}
				}
			});
		}

		// Token: 0x06002658 RID: 9816 RVA: 0x001971C8 File Offset: 0x001953C8
		public void UpdateSubVisibility()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			foreach (GUIComponent child in this.SubList.Content.Children)
			{
				SubmarineInfo sub = child.UserData as SubmarineInfo;
				if (sub != null)
				{
					GUIComponent guicomponent = child;
					if (!GameMain.Client.ServerSettings.HiddenSubs.Contains(sub.Name))
					{
						goto IL_89;
					}
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.SubmarineInfo : null) != null && GameMain.GameSession.SubmarineInfo.Name.Equals(sub.Name, StringComparison.OrdinalIgnoreCase))
					{
						goto IL_89;
					}
					bool visible = false;
					IL_B8:
					guicomponent.Visible = visible;
					continue;
					IL_89:
					visible = (string.IsNullOrEmpty(this.subSearchBox.Text) || sub.DisplayName.Contains(this.subSearchBox.Text, StringComparison.OrdinalIgnoreCase));
					goto IL_B8;
				}
			}
		}

		// Token: 0x06002659 RID: 9817 RVA: 0x001972BC File Offset: 0x001954BC
		public void OnRoundEnded()
		{
			this.CampaignCharacterDiscarded = false;
		}

		// Token: 0x0600265A RID: 9818 RVA: 0x001972C8 File Offset: 0x001954C8
		public void ShowStartRoundWarning(SerializableDateTime waitUntilTime, string team1SubName, ImmutableArray<DisembarkPerkPrefab> team1IncompatiblePerks, string team2SubName, ImmutableArray<DisembarkPerkPrefab> team2IncompatiblePerks)
		{
			DateTime startTime = DateTime.UtcNow;
			TimeSpan differenceFromStart = waitUntilTime.ToUtcValue() - startTime;
			this.StopWaitingForStartRound();
			GUIMessageBox.MessageBoxes.OfType<GUIMessageBox>().ForEachMod(delegate(GUIMessageBox mod)
			{
				string text = mod.UserData as string;
				if (text != null && text == "PleaseWaitPopup")
				{
					mod.Close();
				}
			});
			GUIMessageBox messageBox = new GUIMessageBox(TextManager.Get("warning"), TextManager.Get("startgamewarning"), Array.Empty<LocalizedString>(), new Vector2?(new Vector2(0.3f / GUI.AspectRatioAdjustment, 0.4f)), new Point?(new Point(400, 300)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				UserData = "RoundStartWarningBox"
			};
			GUILayoutGroup contentLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.7f), messageBox.Content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIListBox errorList = new GUIListBox(new RectTransform(new Vector2(1f, 0.7f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			foreach (DisembarkPerkPrefab perk in team1IncompatiblePerks)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.33f), errorList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NetLobbyScreen.<ShowStartRoundWarning>g__FormatWarning|356_3(perk, team1SubName), null, null, Alignment.Left, false, "", null);
			}
			foreach (DisembarkPerkPrefab perk2 in team2IncompatiblePerks)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.33f), errorList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NetLobbyScreen.<ShowStartRoundWarning>g__FormatWarning|356_3(perk2, team2SubName), null, null, Alignment.Left, false, "", null);
			}
			GUIProgressBar progress = new GUIProgressBar(new RectTransform(new Vector2(1f, 0.15f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Orange), "", true);
			GUITextBlock progressText = new GUITextBlock(new RectTransform(Vector2.One, progress.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("startggamewarningprogress", "[seconds]", ((int)differenceFromStart.TotalSeconds).ToString(), FormatCapitals.No), null, null, Alignment.Center, false, "", null)
			{
				Shadow = true,
				TextColor = Color.White
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, progress.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch batch, GUICustomComponent component)
			{
			}, delegate(float f, GUICustomComponent component)
			{
				float seconds = (float)(waitUntilTime.ToUtcValue() - DateTime.UtcNow).TotalSeconds;
				progress.BarSize = seconds / (float)differenceFromStart.TotalSeconds;
				progressText.Text = TextManager.GetWithVariable("startggamewarningprogress", "[seconds]", ((int)seconds).ToString(), FormatCapitals.No);
			});
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.BottomCenter);
			GUIButton cancelButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null);
			GUIButton guibutton = cancelButton;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object userData)
			{
				IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.RESPONSE_CANCEL_STARTGAME);
				GameClient client = GameMain.Client;
				if (client != null)
				{
					ClientPeer clientPeer = client.ClientPeer;
					if (clientPeer != null)
					{
						clientPeer.Send(msg, DeliveryMethod.Reliable, true);
					}
				}
				messageBox.Close();
				return true;
			}));
		}

		// Token: 0x0600265B RID: 9819 RVA: 0x00197789 File Offset: 0x00195989
		public void CloseStartRoundWarning()
		{
			GUIMessageBox.MessageBoxes.OfType<GUIMessageBox>().ForEachMod(delegate(GUIMessageBox mod)
			{
				string text = mod.UserData as string;
				if (text != null && text == "RoundStartWarningBox")
				{
					mod.Close();
				}
			});
		}

		// Token: 0x17000A30 RID: 2608
		// (get) Token: 0x0600265C RID: 9820 RVA: 0x001977B9 File Offset: 0x001959B9
		// (set) Token: 0x0600265D RID: 9821 RVA: 0x001977C1 File Offset: 0x001959C1
		public ushort LastUpdateID
		{
			get
			{
				return this.lastUpdateID;
			}
			set
			{
				this.lastUpdateID = value;
			}
		}

		// Token: 0x0600265E RID: 9822 RVA: 0x001977CC File Offset: 0x001959CC
		public void SetLevelDifficulty(float difficulty)
		{
			difficulty = MathHelper.Clamp(difficulty, 0f, 100f);
			this.levelDifficultySlider.BarScroll = difficulty / 100f;
			this.levelDifficultySlider.OnMoved(this.levelDifficultySlider, this.levelDifficultySlider.BarScroll);
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x0019781F File Offset: 0x00195A1F
		public void SetBotCount(int botCount)
		{
		}

		// Token: 0x06002660 RID: 9824 RVA: 0x00197821 File Offset: 0x00195A21
		public void SetBotSpawnMode(BotSpawnMode botSpawnMode)
		{
		}

		// Token: 0x06002661 RID: 9825 RVA: 0x00197823 File Offset: 0x00195A23
		public void SetTraitorProbability(float probability)
		{
			if (GameMain.NetworkMember != null)
			{
				GameMain.NetworkMember.ServerSettings.TraitorProbability = probability;
			}
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x0019783C File Offset: 0x00195A3C
		public void SetTraitorDangerLevel(int dangerLevel)
		{
			if (GameMain.NetworkMember != null)
			{
				GameMain.NetworkMember.ServerSettings.TraitorDangerLevel = dangerLevel;
			}
			this.SetTraitorDangerIndicators(dangerLevel);
		}

		// Token: 0x06002668 RID: 9832 RVA: 0x00197A3C File Offset: 0x00195C3C
		[CompilerGenerated]
		private IEnumerable<Identifier> <CreateGameModePanel>g__GetValidMissions|237_1()
		{
			return from tickBox in this.missionTypeTickBoxes
			where tickBox.Parent.Visible
			select (Identifier)tickBox.UserData;
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x00197A98 File Offset: 0x00195C98
		[CompilerGenerated]
		internal static void <UpdateDisembarkPointListFromServerSettings>g__TogglePerkElement|254_3(bool enabled, ref NetLobbyScreen.<>c__DisplayClass254_0 A_1)
		{
			A_1.child.Enabled = enabled;
			foreach (GUITextBlock text in A_1.child.GetAllChildren<GUITextBlock>())
			{
				text.Enabled = enabled;
			}
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x00197AF8 File Offset: 0x00195CF8
		[CompilerGenerated]
		private void <UpdateDisembarkPointListFromServerSettings>g__SetEnabled|254_0(bool enabled)
		{
			this.disembarkPerkSettingList.Enabled = enabled;
			foreach (GUIComponent child in this.disembarkPerkSettingList.Content.Children)
			{
				foreach (GUITextBlock block in child.GetAllChildren<GUITextBlock>())
				{
					block.Enabled = enabled;
				}
			}
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x00197CD2 File Offset: 0x00195ED2
		[CompilerGenerated]
		internal static bool <RefreshEnabledElements>g__HasPermission|268_10(ClientPermissions permissions)
		{
			return GameMain.Client != null && GameMain.Client.HasPermission(permissions);
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x00197CE8 File Offset: 0x00195EE8
		[CompilerGenerated]
		private bool <RefreshPvpTeamSelectionButtons>g__CanJoinTeam1|282_0(ref NetLobbyScreen.<>c__DisplayClass282_0 A_1)
		{
			int newTeam1Count = this.Team1Count + ((A_1.currentTeam != CharacterTeamType.Team1) ? 1 : 0);
			int newTeam2Count = this.Team2Count - ((A_1.currentTeam == CharacterTeamType.Team2) ? 1 : 0);
			return newTeam1Count - newTeam2Count <= A_1.serverSettings.PvpAutoBalanceThreshold;
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x00197D30 File Offset: 0x00195F30
		[CompilerGenerated]
		private bool <RefreshPvpTeamSelectionButtons>g__CanJoinTeam2|282_1(ref NetLobbyScreen.<>c__DisplayClass282_0 A_1)
		{
			int newTeam2Count = this.Team2Count + ((A_1.currentTeam != CharacterTeamType.Team2) ? 1 : 0);
			int newTeam1Count = this.Team1Count - ((A_1.currentTeam == CharacterTeamType.Team1) ? 1 : 0);
			return newTeam2Count - newTeam1Count <= A_1.serverSettings.PvpAutoBalanceThreshold;
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x00197D75 File Offset: 0x00195F75
		[CompilerGenerated]
		internal static void <RefreshGameModeSettingsContent>g__SetElementVisible|328_0(GUIComponent element, bool enabled)
		{
			element.Visible = enabled;
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x00197DA0 File Offset: 0x00195FA0
		[CompilerGenerated]
		internal static void <CreateSubmarineVisibilityMenu>g__handleDraggingAcrossLists|352_2(GUIListBox from, GUIListBox to)
		{
			if (to.Rect.Contains(PlayerInput.MousePosition) && from.DraggedElement != null)
			{
				GUIComponent draggedElement = from.DraggedElement;
				List<GUIComponent> selected = from.AllSelected.ToList<GUIComponent>();
				selected.Sort((GUIComponent a, GUIComponent b) => from.Content.GetChildIndex(a) - from.Content.GetChildIndex(b));
				float oldCount = (float)to.Content.CountChildren;
				float newCount = oldCount + (float)selected.Count;
				Point offset = draggedElement.RectTransform.AbsoluteOffset;
				offset += from.Content.Rect.Location;
				offset -= to.Content.Rect.Location;
				for (int i = 0; i < selected.Count; i++)
				{
					GUIComponent c = selected[i];
					c.Parent.RemoveChild(c);
					c.RectTransform.Parent = to.Content.RectTransform;
					c.RectTransform.RepositionChildInHierarchy((int)oldCount + i);
				}
				from.DraggedElement = null;
				from.Deselect();
				from.RecalculateChildren();
				from.RectTransform.RecalculateScale(true);
				to.RecalculateChildren();
				to.RectTransform.RecalculateScale(true);
				to.Select(selected);
				draggedElement.RectTransform.AbsoluteOffset = offset;
				to.DraggedElement = draggedElement;
				to.BarScroll *= oldCount / newCount;
			}
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x00197F40 File Offset: 0x00196140
		[CompilerGenerated]
		internal static void <CreateSubmarineVisibilityMenu>g__swapListItems|352_7(GUIListBox from, GUIListBox to)
		{
			to.Deselect();
			GUIComponent[] selected = from.AllSelected.ToArray<GUIComponent>();
			int lastIndex = from.Content.GetChildIndex(selected.LastOrDefault<GUIComponent>());
			int nextIndex = lastIndex + 1;
			GUIComponent nextComponent = null;
			if (lastIndex >= 0 && nextIndex < from.Content.CountChildren)
			{
				nextComponent = from.Content.GetChild(nextIndex);
			}
			foreach (GUIComponent frame in selected)
			{
				frame.Parent.RemoveChild(frame);
				frame.RectTransform.Parent = to.Content.RectTransform;
			}
			from.RecalculateChildren();
			from.RectTransform.RecalculateScale(true);
			to.RecalculateChildren();
			to.RectTransform.RecalculateScale(true);
			to.Select(selected);
			if (nextComponent != null)
			{
				from.Select(nextComponent.ToEnumerable<GUIComponent>());
			}
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x00198014 File Offset: 0x00196214
		[CompilerGenerated]
		internal static LocalizedString <ShowStartRoundWarning>g__FormatWarning|356_3(DisembarkPerkPrefab prefab, string subName)
		{
			string tag = "startgamewarningformat";
			ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[3];
			int num = 0;
			string item = "[category]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
			defaultInterpolatedStringHandler.AppendLiteral("perkcategory.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.SortCategory);
			array[num] = new ValueTuple<string, LocalizedString>(item, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
			array[1] = new ValueTuple<string, LocalizedString>("[perk]", prefab.Name);
			array[2] = new ValueTuple<string, LocalizedString>("[submarine]", subName);
			return TextManager.GetWithVariables(tag, array);
		}

		// Token: 0x040012DB RID: 4827
		private GUIListBox chatBox;

		// Token: 0x040012DC RID: 4828
		private GUILayoutGroup chatRow;

		// Token: 0x040012DD RID: 4829
		private GUIButton serverLogReverseButton;

		// Token: 0x040012DE RID: 4830
		private GUIListBox serverLogBox;

		// Token: 0x040012DF RID: 4831
		private GUIListBox serverLogFilterTicks;

		// Token: 0x040012E0 RID: 4832
		private static GUIComponent jobVariantTooltip;

		// Token: 0x040012E1 RID: 4833
		private GUIComponent playStyleIconContainer;

		// Token: 0x040012E2 RID: 4834
		private GUIDropDown chatSelector;

		// Token: 0x040012E3 RID: 4835
		public static bool TeamChatSelected;

		// Token: 0x040012E4 RID: 4836
		private GUITextBox chatInput;

		// Token: 0x040012E5 RID: 4837
		private GUITextBox serverLogFilter;

		// Token: 0x040012E6 RID: 4838
		private GUIImage micIcon;

		// Token: 0x040012E7 RID: 4839
		private GUIScrollBar levelDifficultySlider;

		// Token: 0x040012E8 RID: 4840
		private readonly List<GUIComponent> traitorElements = new List<GUIComponent>();

		// Token: 0x040012E9 RID: 4841
		private GUIScrollBar traitorProbabilitySlider;

		// Token: 0x040012EA RID: 4842
		private GUILayoutGroup traitorDangerGroup;

		// Token: 0x040012EB RID: 4843
		private GUIDropDown outpostDropdown;

		// Token: 0x040012EC RID: 4844
		private bool outpostDropdownUpToDate;

		// Token: 0x040012F1 RID: 4849
		private GUITickBox[] missionTypeTickBoxes;

		// Token: 0x040012F2 RID: 4850
		private GUIListBox missionTypeList;

		// Token: 0x040012F4 RID: 4852
		private GUIButton joinOnGoingRoundButton;

		// Token: 0x040012F5 RID: 4853
		private GUILayoutGroup roundControlsHolder;

		// Token: 0x040012F9 RID: 4857
		private GUITickBox spectateBox;

		// Token: 0x040012FA RID: 4858
		private GUITickBox afkBox;

		// Token: 0x040012FB RID: 4859
		private GUILayoutGroup playerInfoContent;

		// Token: 0x040012FC RID: 4860
		private GUIComponent changesPendingText;

		// Token: 0x040012FD RID: 4861
		private bool createPendingChangesText = true;

		// Token: 0x04001300 RID: 4864
		private GUITextBox subSearchBox;

		// Token: 0x04001301 RID: 4865
		private GUIComponent subPreviewContainer;

		// Token: 0x04001302 RID: 4866
		private GUITickBox autoRestartBox;

		// Token: 0x04001303 RID: 4867
		private GUITextBlock autoRestartText;

		// Token: 0x04001304 RID: 4868
		private GUITickBox shuttleTickBox;

		// Token: 0x04001305 RID: 4869
		private Sprite backgroundSprite;

		// Token: 0x04001306 RID: 4870
		private GUIButton jobPreferencesButton;

		// Token: 0x04001307 RID: 4871
		private GUIButton appearanceButton;

		// Token: 0x04001308 RID: 4872
		private GUIFrame characterInfoFrame;

		// Token: 0x04001309 RID: 4873
		private GUIFrame appearanceFrame;

		// Token: 0x0400130A RID: 4874
		private GUISelectionCarousel<RespawnMode> respawnModeSelection;

		// Token: 0x0400130B RID: 4875
		private GUITextBlock respawnModeLabel;

		// Token: 0x0400130C RID: 4876
		private GUIComponent respawnIntervalElement;

		// Token: 0x0400130D RID: 4877
		private readonly List<GUIComponent> midRoundRespawnSettings = new List<GUIComponent>();

		// Token: 0x0400130E RID: 4878
		private readonly List<GUIComponent> permadeathEnabledRespawnSettings = new List<GUIComponent>();

		// Token: 0x0400130F RID: 4879
		private readonly List<GUIComponent> permadeathDisabledRespawnSettings = new List<GUIComponent>();

		// Token: 0x04001310 RID: 4880
		private readonly List<GUIComponent> ironmanDisabledRespawnSettings = new List<GUIComponent>();

		// Token: 0x04001311 RID: 4881
		private readonly List<GUIComponent> campaignDisabledElements = new List<GUIComponent>();

		// Token: 0x04001312 RID: 4882
		private readonly List<GUIComponent> campaignHiddenElements = new List<GUIComponent>();

		// Token: 0x04001313 RID: 4883
		private readonly List<GUIComponent> pvpOnlyElements = new List<GUIComponent>();

		// Token: 0x04001314 RID: 4884
		private readonly List<GUIComponent> disembarkPerkSettings = new List<GUIComponent>();

		// Token: 0x04001315 RID: 4885
		private readonly List<GUIComponent> respawnSettings = new List<GUIComponent>();

		// Token: 0x04001317 RID: 4887
		private Point prevResolutionForJobSelectionFrame;

		// Token: 0x0400131B RID: 4891
		private Identifier micIconStyle;

		// Token: 0x0400131C RID: 4892
		private float micCheckTimer;

		// Token: 0x0400131D RID: 4893
		private const float MicCheckInterval = 1f;

		// Token: 0x0400131E RID: 4894
		private float autoRestartTimer;

		// Token: 0x0400131F RID: 4895
		private CharacterInfo campaignCharacterInfo;

		// Token: 0x04001321 RID: 4897
		private readonly List<GUIComponent> clientDisabledElements = new List<GUIComponent>();

		// Token: 0x04001322 RID: 4898
		private readonly List<GUIComponent> clientHiddenElements = new List<GUIComponent>();

		// Token: 0x04001323 RID: 4899
		private readonly List<GUIComponent> botSettingsElements = new List<GUIComponent>();

		// Token: 0x04001324 RID: 4900
		private readonly Dictionary<GUIComponent, string> settingAssignedComponents = new Dictionary<GUIComponent, string>();

		// Token: 0x0400132B RID: 4907
		private readonly List<GUIButton> chatPanelTabButtons = new List<GUIButton>();

		// Token: 0x0400132C RID: 4908
		private GUITextBlock publicOrPrivateText;

		// Token: 0x0400132D RID: 4909
		private GUITextBlock playstyleText;

		// Token: 0x04001331 RID: 4913
		private int selectedModeIndex;

		// Token: 0x04001332 RID: 4914
		public GUIListBox PlayerList;

		// Token: 0x04001333 RID: 4915
		public int Team1Count;

		// Token: 0x04001334 RID: 4916
		public int Team2Count;

		// Token: 0x04001337 RID: 4919
		private GUITextBlock pvpTeamChoiceTeam1;

		// Token: 0x04001338 RID: 4920
		private GUITextBlock pvpTeamChoiceMiddleButton;

		// Token: 0x04001339 RID: 4921
		private GUITextBlock pvpTeamChoiceTeam2;

		// Token: 0x0400133D RID: 4925
		[AllowNull]
		[MaybeNull]
		public SubmarineInfo SelectedSub;

		// Token: 0x0400133E RID: 4926
		[AllowNull]
		[MaybeNull]
		public SubmarineInfo SelectedEnemySub;

		// Token: 0x0400133F RID: 4927
		public MultiPlayerCampaignSetupUI CampaignSetupUI;

		// Token: 0x04001340 RID: 4928
		private const float MainPanelWidth = 0.7f;

		// Token: 0x04001341 RID: 4929
		private const float SidePanelWidth = 0.3f;

		// Token: 0x04001342 RID: 4930
		private const float PanelSpacing = 0.005f;

		// Token: 0x04001343 RID: 4931
		private GUIFrame gameModeSettingsContent;

		// Token: 0x04001344 RID: 4932
		private GUILayoutGroup gameModeSettingsLayout;

		// Token: 0x04001345 RID: 4933
		private GUIButton upgradesTabButton;

		// Token: 0x04001346 RID: 4934
		private GUIButton respawnTabButton;

		// Token: 0x04001347 RID: 4935
		private GUIListBox disembarkPerkSettingList;

		// Token: 0x04001348 RID: 4936
		private GUIComponent disembarkPerkDisabledDisclaimer;

		// Token: 0x04001349 RID: 4937
		private GUIComponent noPerksAvailableDisclaimer;

		// Token: 0x0400134A RID: 4938
		private GUITextBlock disembarkPerkFooterText;

		// Token: 0x0400134B RID: 4939
		private bool isUpdatingPerks;

		// Token: 0x0400134C RID: 4940
		public const string PleaseWaitPopupUserData = "PleaseWaitPopup";

		// Token: 0x0400134D RID: 4941
		private PlayStyle? prevPlayStyle;

		// Token: 0x0400134E RID: 4942
		private bool? prevIsPublic;

		// Token: 0x0400134F RID: 4943
		public NetLobbyScreen.FailedSubInfo? FailedSelectedSub;

		// Token: 0x04001350 RID: 4944
		public NetLobbyScreen.FailedSubInfo? FailedSelectedEnemySub;

		// Token: 0x04001351 RID: 4945
		public NetLobbyScreen.FailedSubInfo? FailedSelectedShuttle;

		// Token: 0x04001352 RID: 4946
		public List<NetLobbyScreen.FailedSubInfo> FailedCampaignSubs = new List<NetLobbyScreen.FailedSubInfo>();

		// Token: 0x04001353 RID: 4947
		public List<NetLobbyScreen.FailedSubInfo> FailedOwnedSubs = new List<NetLobbyScreen.FailedSubInfo>();

		// Token: 0x04001354 RID: 4948
		private readonly List<SubmarineInfo> visibilityMenuOrder = new List<SubmarineInfo>();

		// Token: 0x04001355 RID: 4949
		public const string SeparatistsIconUserData = "separatistsIcon";

		// Token: 0x04001356 RID: 4950
		public const string CoalitionIconUserData = "coalitionIcon";

		// Token: 0x04001357 RID: 4951
		private const string RoundStartWarningBoxUserData = "RoundStartWarningBox";

		// Token: 0x04001358 RID: 4952
		private ushort lastUpdateID;

		// Token: 0x04001359 RID: 4953
		private string levelSeed = "";

		// Token: 0x02000C76 RID: 3190
		public readonly struct FailedSubInfo
		{
			// Token: 0x06007C60 RID: 31840 RVA: 0x00386F52 File Offset: 0x00385152
			public FailedSubInfo(string name, string hash)
			{
				this.Name = name;
				this.Hash = hash;
			}

			// Token: 0x06007C61 RID: 31841 RVA: 0x00386F62 File Offset: 0x00385162
			public void Deconstruct(out string name, out string hash)
			{
				name = this.Name;
				hash = this.Hash;
			}

			// Token: 0x06007C62 RID: 31842 RVA: 0x00386F74 File Offset: 0x00385174
			private static bool StringsEqual(string a, string b)
			{
				return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
			}

			// Token: 0x06007C63 RID: 31843 RVA: 0x00386F7E File Offset: 0x0038517E
			public static bool operator ==(NetLobbyScreen.FailedSubInfo a, NetLobbyScreen.FailedSubInfo b)
			{
				return NetLobbyScreen.FailedSubInfo.StringsEqual(a.Name, b.Name) && NetLobbyScreen.FailedSubInfo.StringsEqual(a.Hash, b.Hash);
			}

			// Token: 0x06007C64 RID: 31844 RVA: 0x00386FA6 File Offset: 0x003851A6
			public static bool operator !=(NetLobbyScreen.FailedSubInfo a, NetLobbyScreen.FailedSubInfo b)
			{
				return !(a == b);
			}

			// Token: 0x06007C65 RID: 31845 RVA: 0x00386FB2 File Offset: 0x003851B2
			public override int GetHashCode()
			{
				return HashCode.Combine<string, string>(this.Name, this.Hash);
			}

			// Token: 0x06007C66 RID: 31846 RVA: 0x00386FC8 File Offset: 0x003851C8
			public override bool Equals(object obj)
			{
				if (obj is NetLobbyScreen.FailedSubInfo)
				{
					NetLobbyScreen.FailedSubInfo info = (NetLobbyScreen.FailedSubInfo)obj;
					if (this.Name == info.Name)
					{
						return this.Hash == info.Hash;
					}
				}
				return false;
			}

			// Token: 0x04004B30 RID: 19248
			public readonly string Name;

			// Token: 0x04004B31 RID: 19249
			public readonly string Hash;
		}

		// Token: 0x02000C77 RID: 3191
		public enum SubmarineDeliveryData
		{
			// Token: 0x04004B33 RID: 19251
			Owned,
			// Token: 0x04004B34 RID: 19252
			Campaign
		}
	}
}
