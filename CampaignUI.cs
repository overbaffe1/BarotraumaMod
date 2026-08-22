using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000103 RID: 259
	internal class CampaignUI
	{
		// Token: 0x170009CE RID: 2510
		// (get) Token: 0x06002448 RID: 9288 RVA: 0x0016E792 File Offset: 0x0016C992
		public CampaignMode.InteractionType SelectedTab
		{
			get
			{
				return this.selectedTab;
			}
		}

		// Token: 0x170009CF RID: 2511
		// (get) Token: 0x06002449 RID: 9289 RVA: 0x0016E79A File Offset: 0x0016C99A
		// (set) Token: 0x0600244A RID: 9290 RVA: 0x0016E7A2 File Offset: 0x0016C9A2
		public LevelData SelectedLevel { get; private set; }

		// Token: 0x170009D0 RID: 2512
		// (get) Token: 0x0600244B RID: 9291 RVA: 0x0016E7AB File Offset: 0x0016C9AB
		// (set) Token: 0x0600244C RID: 9292 RVA: 0x0016E7B3 File Offset: 0x0016C9B3
		private GUIButton StartButton { get; set; }

		// Token: 0x170009D1 RID: 2513
		// (get) Token: 0x0600244D RID: 9293 RVA: 0x0016E7BC File Offset: 0x0016C9BC
		public CampaignMode Campaign { get; }

		// Token: 0x170009D2 RID: 2514
		// (get) Token: 0x0600244E RID: 9294 RVA: 0x0016E7C4 File Offset: 0x0016C9C4
		// (set) Token: 0x0600244F RID: 9295 RVA: 0x0016E7CC File Offset: 0x0016C9CC
		public HRManagerUI HRManagerUI { get; set; }

		// Token: 0x170009D3 RID: 2515
		// (get) Token: 0x06002450 RID: 9296 RVA: 0x0016E7D5 File Offset: 0x0016C9D5
		// (set) Token: 0x06002451 RID: 9297 RVA: 0x0016E7DD File Offset: 0x0016C9DD
		public Store Store { get; private set; }

		// Token: 0x170009D4 RID: 2516
		// (get) Token: 0x06002452 RID: 9298 RVA: 0x0016E7E6 File Offset: 0x0016C9E6
		// (set) Token: 0x06002453 RID: 9299 RVA: 0x0016E7EE File Offset: 0x0016C9EE
		public UpgradeStore UpgradeStore { get; set; }

		// Token: 0x170009D5 RID: 2517
		// (get) Token: 0x06002454 RID: 9300 RVA: 0x0016E7F7 File Offset: 0x0016C9F7
		// (set) Token: 0x06002455 RID: 9301 RVA: 0x0016E7FF File Offset: 0x0016C9FF
		public MedicalClinicUI MedicalClinic { get; set; }

		// Token: 0x06002456 RID: 9302 RVA: 0x0016E808 File Offset: 0x0016CA08
		public CampaignUI(CampaignMode campaign, GUIComponent container)
		{
			this.Campaign = campaign;
			if (campaign.Map == null)
			{
				throw new InvalidOperationException("Failed to create campaign UI (campaign map was null).");
			}
			if (campaign.Map.CurrentLocation == null)
			{
				throw new InvalidOperationException("Failed to create campaign UI (current location not set).");
			}
			this.CreateUI(container);
			campaign.Map.OnLocationSelected = new Action<Location, LocationConnection>(this.SelectLocation);
			campaign.Map.OnMissionsSelected = delegate(LocationConnection connection, IEnumerable<Mission> missions)
			{
				GUIListBox guilistBox = this.missionList;
				if (((guilistBox != null) ? guilistBox.Content : null) != null)
				{
					foreach (GUIComponent missionElement in this.missionList.Content.Children)
					{
						GUITickBox tickBox = missionElement.FindChild((GUIComponent c) => c is GUITickBox, true) as GUITickBox;
						if (tickBox != null)
						{
							tickBox.Selected = missions.Contains(tickBox.UserData as Mission);
						}
					}
				}
			};
		}

		// Token: 0x06002457 RID: 9303 RVA: 0x0016E898 File Offset: 0x0016CA98
		private void CreateUI(GUIComponent container)
		{
			container.ClearChildren();
			this.tabs = new GUIFrame[Enum.GetValues(typeof(CampaignMode.InteractionType)).Length];
			this.tabs[3] = this.CreateDefaultTabContainer(container, new Vector2(0.9f), true);
			GUIFrame mapFrame = new GUIFrame(new RectTransform(Vector2.One, this.GetTabContainer(CampaignMode.InteractionType.Map).RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			GUICustomComponent mapContainer = new GUICustomComponent(new RectTransform(Vector2.One, mapFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawMap), new Action<float, GUICustomComponent>(this.UpdateMap));
			GUIFrame notificationFrame = new GUIFrame(new RectTransform(new Point(mapContainer.Rect.Width, GUI.IntScale(40f)), mapContainer.RectTransform, Anchor.BottomCenter, null, ScaleBasis.Normal, false), "ChatBox", null);
			new GUIFrame(new RectTransform(Vector2.One, mapFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerGlow", new Color?(Color.Black * 0.9f)).CanBeFocused = false;
			new GUICustomComponent(new RectTransform(new Vector2(0.98f, 1f), notificationFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawMapNotifications), null).HideElementsOutsideFrame = true;
			GUIImage notificationHeader = new GUIImage(new RectTransform(new Vector2(0.1f, 1f), notificationFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), "GUISlopedHeaderRight", GUIImage.ScalingMode.None);
			RectTransform rectT = new RectTransform(Vector2.One, notificationHeader.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("breakingnews");
			GUIFont largeFont = GUIStyle.LargeFont;
			GUITextBlock text = new GUITextBlock(rectT, text2, null, largeFont, Alignment.Left, false, "", null);
			notificationHeader.RectTransform.MinSize = new Point((int)(text.TextSize.X * 1.3f), 0);
			GUIFrame crewTab = new GUIFrame(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			this.tabs[4] = crewTab;
			this.HRManagerUI = new HRManagerUI(this, crewTab);
			GUIFrame storeTab = new GUIFrame(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			this.tabs[5] = storeTab;
			this.Store = new Store(this, storeTab);
			this.tabs[6] = new GUIFrame(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			this.UpgradeStore = new UpgradeStore(this, this.GetTabContainer(CampaignMode.InteractionType.Upgrade));
			this.tabs[7] = new GUIFrame(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			this.tabs[8] = new GUIFrame(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.Black * 0.9f));
			this.MedicalClinic = new MedicalClinicUI(this.Campaign.MedicalClinic, this.GetTabContainer(CampaignMode.InteractionType.MedicalClinic));
			this.locationInfoPanel = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.75f), this.GetTabContainer(CampaignMode.InteractionType.Map).RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f)
			}, "", new Color?(Color.Black))
			{
				Visible = false
			};
			this.SelectTab(CampaignMode.InteractionType.Map, null);
			this.prevResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06002458 RID: 9304 RVA: 0x0016EDE4 File Offset: 0x0016CFE4
		private GUIFrame CreateDefaultTabContainer(GUIComponent container, Vector2 frameSize, bool visible = true)
		{
			GUIFrame innerFrame = new GUIFrame(new RectTransform(frameSize, container.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null)
			{
				Visible = visible
			};
			new GUIFrame(new RectTransform(innerFrame.Rect.Size - GUIStyle.ItemFrameMargin, innerFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null).UserData = "container";
			return innerFrame;
		}

		// Token: 0x06002459 RID: 9305 RVA: 0x0016EE80 File Offset: 0x0016D080
		public GUIComponent GetTabContainer(CampaignMode.InteractionType tab)
		{
			GUIFrame tabFrame = this.tabs[(int)tab];
			return ((tabFrame != null) ? tabFrame.GetChildByUserData("container") : null) ?? tabFrame;
		}

		// Token: 0x0600245A RID: 9306 RVA: 0x0016EEAC File Offset: 0x0016D0AC
		private void DrawMap(SpriteBatch spriteBatch, GUICustomComponent mapContainer)
		{
			if (GameMain.GraphicsWidth != this.prevResolution.X || GameMain.GraphicsHeight != this.prevResolution.Y)
			{
				this.CreateUI(this.tabs[3].Parent);
			}
			CampaignMode campaign = this.Campaign;
			if (campaign == null)
			{
				return;
			}
			Map map = campaign.Map;
			if (map == null)
			{
				return;
			}
			map.Draw(this.Campaign, spriteBatch, mapContainer);
		}

		// Token: 0x0600245B RID: 9307 RVA: 0x0016EF12 File Offset: 0x0016D112
		private void DrawMapNotifications(SpriteBatch spriteBatch, GUICustomComponent notificationContainer)
		{
			CampaignMode campaign = this.Campaign;
			if (campaign == null)
			{
				return;
			}
			Map map = campaign.Map;
			if (map == null)
			{
				return;
			}
			map.DrawNotifications(spriteBatch, notificationContainer);
		}

		// Token: 0x0600245C RID: 9308 RVA: 0x0016EF30 File Offset: 0x0016D130
		private void UpdateMap(float deltaTime, GUICustomComponent mapContainer)
		{
			CampaignMode campaign = this.Campaign;
			Map map = (campaign != null) ? campaign.Map : null;
			if (map == null)
			{
				return;
			}
			if (this.selectedLocation != null && this.selectedLocation == this.Campaign.GetCurrentDisplayLocation())
			{
				map.SelectLocation(-1);
			}
			map.Update(this.Campaign, deltaTime, mapContainer);
			foreach (GUITickBox tickBox in this.missionTickBoxes)
			{
				bool disable = this.hasMaxMissions && !tickBox.Selected;
				tickBox.Enabled = (CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap) && !disable);
				tickBox.Box.DisabledColor = (disable ? (tickBox.Box.Color * 0.5f) : (tickBox.Box.Color * 0.8f));
				foreach (GUIComponent child in tickBox.Parent.Parent.Children)
				{
					GUITextBlock textBlock = child as GUITextBlock;
					if (textBlock != null)
					{
						textBlock.SelectedTextColor = (textBlock.HoverTextColor = (textBlock.TextColor = (disable ? new Color(textBlock.TextColor, 0.5f) : new Color(textBlock.TextColor, 1f))));
					}
				}
			}
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x0016F0E4 File Offset: 0x0016D2E4
		public void Update(float deltaTime)
		{
			switch (this.SelectedTab)
			{
			case CampaignMode.InteractionType.Map:
				if (this.StartButton != null)
				{
					GUIComponent startButton = this.StartButton;
					bool enabled;
					if (CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
					{
						Character controlled = Character.Controlled;
						enabled = (controlled != null && !controlled.IsIncapacitated);
					}
					else
					{
						enabled = false;
					}
					startButton.Enabled = enabled;
				}
				break;
			case CampaignMode.InteractionType.Crew:
			{
				HRManagerUI hrmanagerUI = this.HRManagerUI;
				if (hrmanagerUI == null)
				{
					return;
				}
				hrmanagerUI.Update();
				return;
			}
			case CampaignMode.InteractionType.Store:
			{
				Store store = this.Store;
				if (store == null)
				{
					return;
				}
				store.Update(deltaTime);
				return;
			}
			case CampaignMode.InteractionType.Upgrade:
				break;
			case CampaignMode.InteractionType.PurchaseSub:
			{
				SubmarineSelection submarineSelection = this.submarineSelection;
				if (submarineSelection == null)
				{
					return;
				}
				submarineSelection.Update();
				return;
			}
			case CampaignMode.InteractionType.MedicalClinic:
			{
				MedicalClinicUI medicalClinic = this.MedicalClinic;
				if (medicalClinic == null)
				{
					return;
				}
				medicalClinic.Update(deltaTime);
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x0016F198 File Offset: 0x0016D398
		public void RefreshLocationInfo()
		{
			if (this.selectedLocation != null)
			{
				CampaignMode campaign = this.Campaign;
				bool flag;
				if (campaign == null)
				{
					flag = (null != null);
				}
				else
				{
					Map map = campaign.Map;
					flag = (((map != null) ? map.SelectedConnection : null) != null);
				}
				if (flag)
				{
					this.SelectLocation(this.selectedLocation, this.Campaign.Map.SelectedConnection);
				}
			}
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x0016F1EC File Offset: 0x0016D3EC
		public void SelectLocation(Location location, LocationConnection connection)
		{
			this.missionTickBoxes.Clear();
			this.missionRewardTexts.Clear();
			this.locationInfoPanel.ClearChildren();
			if (this.selectedTab == CampaignMode.InteractionType.Map)
			{
				this.SelectTab(CampaignMode.InteractionType.Map, null);
				this.locationInfoPanel.Visible = (location != null);
			}
			Location prevSelectedLocation = this.selectedLocation;
			GUIListBox guilistBox = this.missionList;
			float prevMissionListScroll = (guilistBox != null) ? guilistBox.BarScroll : 0f;
			this.selectedLocation = location;
			if (location == null)
			{
				return;
			}
			int padding = GUI.IntScale(20f);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(this.locationInfoPanel.Rect.Size - new Point(padding * 2), this.locationInfoPanel.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopRight)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = location.DisplayName;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = location.GetLocationTypeToDisplay().Name;
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			Sprite portrait = location.Type.GetPortrait(location.PortraitId);
			portrait.EnsureLazyLoaded(false);
			GUICustomComponent portraitContainer = new GUICustomComponent(new RectTransform(new Vector2(1f, 0.3f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent customComponent)
			{
				portrait.Draw(sb, customComponent.Rect.Center.ToVector2(), Color.Gray, portrait.size / 2f, 0f, Math.Max((float)customComponent.Rect.Width / portrait.size.X, (float)customComponent.Rect.Height / portrait.size.Y), SpriteEffects.None, null);
			}, null)
			{
				HideElementsOutsideFrame = true
			};
			GUILayoutGroup textContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), portraitContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			LocationConnection connection2 = connection;
			if (((connection2 != null) ? connection2.LevelData : null) != null)
			{
				Faction faction = location.Faction;
				if (((faction != null) ? faction.Prefab : null) != null)
				{
					RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), textContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get("Faction");
					font = GUIStyle.SubHeadingFont;
					GUITextBlock factionLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.CenterLeft, false, "", null);
					new GUITextBlock(new RectTransform(new Vector2(1f, 1f), factionLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), location.Faction.Prefab.Name, new Color?(location.Faction.Prefab.IconColor), null, Alignment.CenterRight, false, "", null);
				}
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), textContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = TextManager.Get(new string[]
				{
					"Biome",
					"location"
				});
				font = GUIStyle.SubHeadingFont;
				GUITextBlock biomeLabel = new GUITextBlock(rectT4, text4, null, font, Alignment.CenterLeft, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, 1f), biomeLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), connection.Biome.DisplayName, null, null, Alignment.CenterRight, false, "", null);
				RectTransform rectT5 = new RectTransform(new Vector2(1f, 0f), textContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = TextManager.Get("LevelDifficulty");
				font = GUIStyle.SubHeadingFont;
				GUITextBlock difficultyLabel = new GUITextBlock(rectT5, text5, null, font, Alignment.CenterLeft, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, 1f), difficultyLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("percentageformat", "[value]", ((int)connection.LevelData.Difficulty).ToString(), FormatCapitals.No), null, null, Alignment.CenterRight, false, "", null);
				if (connection.LevelData.HasBeaconStation)
				{
					GUILayoutGroup beaconStationContent = new GUILayoutGroup(new RectTransform(biomeLabel.RectTransform.NonScaledSize, textContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft);
					string style = connection.LevelData.IsBeaconActive ? "BeaconStationActive" : "BeaconStationInactive";
					GUIImage icon = new GUIImage(new RectTransform(new Point((int)((float)beaconStationContent.Rect.Height * 1.2f)), beaconStationContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), style, true)
					{
						Color = MapGenerationParams.Instance.IndicatorColor,
						HoverColor = Color.Lerp(MapGenerationParams.Instance.IndicatorColor, Color.White, 0.5f),
						ToolTip = RichString.Rich(TextManager.Get(connection.LevelData.IsBeaconActive ? "BeaconStationActiveTooltip" : "BeaconStationInactiveTooltip"), null)
					};
					RectTransform rectT6 = new RectTransform(Vector2.One, beaconStationContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text6 = TextManager.Get(new string[]
					{
						"submarinetype.beaconstation",
						"beaconstationsonarlabel"
					});
					font = GUIStyle.SubHeadingFont;
					GUITextBlock guitextBlock = new GUITextBlock(rectT6, text6, null, font, Alignment.CenterLeft, false, "", null);
					guitextBlock.Padding = Vector4.Zero;
					guitextBlock.ToolTip = icon.ToolTip;
				}
				if (connection.LevelData.HasHuntingGrounds)
				{
					GUILayoutGroup huntingGroundsContent = new GUILayoutGroup(new RectTransform(biomeLabel.RectTransform.NonScaledSize, textContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft);
					GUIImage icon2 = new GUIImage(new RectTransform(new Point((int)((float)huntingGroundsContent.Rect.Height * 1.5f)), huntingGroundsContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "HuntingGrounds", true)
					{
						Color = MapGenerationParams.Instance.IndicatorColor,
						HoverColor = Color.Lerp(MapGenerationParams.Instance.IndicatorColor, Color.White, 0.5f),
						ToolTip = RichString.Rich(TextManager.Get("HuntingGroundsTooltip"), null)
					};
					RectTransform rectT7 = new RectTransform(Vector2.One, huntingGroundsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text7 = TextManager.Get("missionname.huntinggrounds");
					font = GUIStyle.SubHeadingFont;
					GUITextBlock guitextBlock2 = new GUITextBlock(rectT7, text7, null, font, Alignment.CenterLeft, false, "", null);
					guitextBlock2.Padding = Vector4.Zero;
					guitextBlock2.ToolTip = icon2.ToolTip;
				}
			}
			this.missionList = new GUIListBox(new RectTransform(new Vector2(1f, 0.4f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = (int)(5f * GUI.yScale)
			};
			this.missionList.OnSelected = delegate(GUIComponent selected, object userdata)
			{
				GUITickBox tickBox2 = selected.FindChild((GUIComponent c) => c is GUITickBox, true) as GUITickBox;
				if (GUI.MouseOn == tickBox2)
				{
					return false;
				}
				if (tickBox2 != null && CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap) && tickBox2.Enabled)
				{
					tickBox2.Selected = !tickBox2.Selected;
				}
				return true;
			};
			LocationConnection connection3 = connection;
			this.SelectedLevel = ((connection3 != null) ? connection3.LevelData : null);
			Location currentDisplayLocation = this.Campaign.GetCurrentDisplayLocation();
			if (connection != null && connection.Locations.Contains(currentDisplayLocation))
			{
				List<Mission> availableMissions = (from m in currentDisplayLocation.GetMissionsInConnection(connection)
				where m.Prefab.ShowInMenus || GameMain.DebugDraw
				select m).ToList<Mission>();
				if (availableMissions.None(null))
				{
					availableMissions.Insert(0, null);
				}
				availableMissions.AddRange(from m in location.AvailableMissions
				where m.Locations[0] == m.Locations[1]
				select m);
				availableMissions.Sort((Mission m1, Mission m2) => (m1 != null && m1.Prefab.IsSideObjective).CompareTo(m2 != null && m2.Prefab.IsSideObjective));
				this.missionList.Content.ClearChildren();
				bool isPrevMissionInNextLocation = false;
				using (List<Mission>.Enumerator enumerator = availableMissions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Mission mission = enumerator.Current;
						bool isMissionInNextLocation = mission != null && location.AvailableMissions.Contains(mission);
						if (isMissionInNextLocation && !isPrevMissionInNextLocation)
						{
							RectTransform rectT8 = new RectTransform(new Vector2(1f, 0f), this.missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
							RichString text8 = TextManager.Get("outpostmissions");
							font = GUIStyle.SubHeadingFont;
							new GUITextBlock(rectT8, text8, null, font, Alignment.Center, true, "", null).CanBeFocused = false;
							new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), this.missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null).CanBeFocused = false;
						}
						isPrevMissionInNextLocation = isMissionInNextLocation;
						GUIFrame missionPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), this.missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
						{
							UserData = mission
						};
						GUILayoutGroup missionTextContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), missionPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
						{
							Stretch = true,
							CanBeFocused = true,
							AbsoluteSpacing = GUI.IntScale(5f)
						};
						Mission mission2 = mission;
						LocalizedString missionName = ((mission2 != null) ? mission2.Name : null) ?? TextManager.Get("NoMission");
						if (mission != null)
						{
							MissionPrefab prefab = mission.Prefab;
							if (prefab != null && prefab.IsSideObjective)
							{
								missionName = TextManager.AddPunctuation(':', new LocalizedString[]
								{
									TextManager.Get("sideobjective"),
									missionName
								});
							}
						}
						if (GameMain.DebugDraw && mission != null)
						{
							if (!mission.Prefab.ShowInMenus)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
								defaultInterpolatedStringHandler.AppendLiteral("[HIDDEN] ");
								defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(missionName);
								missionName = defaultInterpolatedStringHandler.ToStringAndClear();
							}
							LocalizedString left = missionName;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 1);
							defaultInterpolatedStringHandler2.AppendLiteral(" (");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(mission.Prefab.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral(")");
							missionName = left + defaultInterpolatedStringHandler2.ToStringAndClear();
						}
						RectTransform rectT9 = new RectTransform(new Vector2(1f, 0f), missionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
						RichString text9 = missionName;
						font = GUIStyle.SubHeadingFont;
						GUITextBlock missionNameBlock = new GUITextBlock(rectT9, text9, null, font, Alignment.Left, true, "", null);
						missionNameBlock.RectTransform.MinSize = new Point(0, GUI.IntScale(15f));
						if (mission == null)
						{
							missionTextContent.RectTransform.MinSize = (missionNameBlock.RectTransform.MinSize = new Point(0, GUI.IntScale(35f)));
							missionTextContent.ChildAnchor = Anchor.CenterLeft;
						}
						else
						{
							GUITickBox tickBox = null;
							if (!isMissionInNextLocation && mission.Prefab.ShowInMenus && !mission.Prefab.IsSideObjective)
							{
								GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One * 0.9f, missionNameBlock.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Smallest)
								{
									AbsoluteOffset = new Point((int)missionNameBlock.Padding.X, 0)
								}, string.Empty, null, "");
								guitickBox.UserData = mission;
								Location currentLocation = this.Campaign.Map.CurrentLocation;
								guitickBox.Selected = (currentLocation != null && currentLocation.SelectedMissions.Contains(mission));
								tickBox = guitickBox;
								tickBox.RectTransform.MinSize = new Point(tickBox.Rect.Height, 0);
								tickBox.RectTransform.IsFixedSize = true;
								tickBox.Enabled = CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap);
								GUITickBox guitickBox2 = tickBox;
								guitickBox2.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox2.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tb)
								{
									if (!CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
									{
										return false;
									}
									if (tb.Selected)
									{
										this.Campaign.Map.CurrentLocation.SelectMission(mission);
									}
									else
									{
										this.Campaign.Map.CurrentLocation.DeselectMission(mission);
									}
									foreach (GUITextBlock rewardText2 in this.missionRewardTexts)
									{
										Mission otherMission = rewardText2.UserData as Mission;
										rewardText2.Text = otherMission.GetMissionRewardText(Submarine.MainSub);
									}
									this.UpdateMaxMissions(connection.OtherLocation(currentDisplayLocation));
									MultiPlayerCampaign multiPlayerCampaign = this.Campaign as MultiPlayerCampaign;
									if (multiPlayerCampaign != null && !multiPlayerCampaign.SuppressStateSending && CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
									{
										GameClient client = GameMain.Client;
										if (client != null)
										{
											client.SendCampaignState();
										}
									}
									return true;
								}));
								this.missionTickBoxes.Add(tickBox);
							}
							GUILayoutGroup difficultyIndicatorGroup = null;
							if (mission.Difficulty != null)
							{
								difficultyIndicatorGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.9f), missionNameBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
								{
									AbsoluteOffset = new Point((int)missionNameBlock.Padding.Z, 0)
								}, true, Anchor.CenterRight)
								{
									AbsoluteSpacing = 1,
									UserData = "difficulty"
								};
								difficultyIndicatorGroup.SetAsFirstChild();
								Color difficultyColor = mission.GetDifficultyColor();
								int i = 0;
								for (;;)
								{
									int num = i;
									int? difficulty = mission.Difficulty;
									if (!(num < difficulty.GetValueOrDefault() & difficulty != null))
									{
										break;
									}
									GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One * 0.9f, difficultyIndicatorGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest)
									{
										IsFixedSize = true
									}, "DifficultyIndicator", true);
									guiimage.Color = difficultyColor;
									guiimage.SelectedColor = difficultyColor;
									guiimage.HoverColor = difficultyColor;
									guiimage.ToolTip = mission.GetDifficultyToolTipText();
									i++;
								}
							}
							float extraPadding = 0f;
							float extraZPadding = (float)((difficultyIndicatorGroup != null) ? (mission.Difficulty.Value * (difficultyIndicatorGroup.Children.First<GUIComponent>().Rect.Width + difficultyIndicatorGroup.AbsoluteSpacing)) : 0);
							missionNameBlock.Padding = new Vector4(missionNameBlock.Padding.X + (float)((tickBox != null) ? tickBox.Rect.Width : 0) * 1.2f + extraPadding, missionNameBlock.Padding.Y, missionNameBlock.Padding.Z + extraZPadding + extraPadding, missionNameBlock.Padding.W);
							missionNameBlock.CalculateHeightFromText(0, false);
							new GUIFrame(new RectTransform(new Vector2(1f, 0f), missionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
							{
								MinSize = new Point(0, GUI.IntScale(10f))
							}, null, null);
							GUITextBlock rewardText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), missionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(mission.GetMissionRewardText(Submarine.MainSub), null), null, null, Alignment.Left, true, "", null)
							{
								UserData = mission
							};
							this.missionRewardTexts.Add(rewardText);
							LocalizedString reputationText = mission.GetReputationRewardText();
							if (!reputationText.IsNullOrEmpty())
							{
								new GUITextBlock(new RectTransform(new Vector2(1f, 0f), missionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(reputationText, null), null, null, Alignment.Left, true, "", null);
							}
							new GUITextBlock(new RectTransform(new Vector2(1f, 0f), missionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(mission.Description, null), null, null, Alignment.Left, true, "", null);
						}
						missionPanel.RectTransform.MinSize = new Point(0, (int)((float)missionTextContent.Children.Sum((GUIComponent c) => c.Rect.Height + missionTextContent.AbsoluteSpacing) / missionTextContent.RectTransform.RelativeSize.Y) + GUI.IntScale(0f));
						GUIComponent child;
						using (IEnumerator<GUIComponent> enumerator2 = missionTextContent.Children.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								child = enumerator2.Current;
								GUITextBlock textBlock = child as GUITextBlock;
								if (textBlock != null)
								{
									textBlock.Color = (textBlock.SelectedColor = (textBlock.HoverColor = Color.Transparent));
									textBlock.SelectedTextColor = (textBlock.HoverTextColor = textBlock.TextColor);
								}
							}
						}
						missionPanel.OnAddedToGUIUpdateList = delegate(GUIComponent c)
						{
							missionTextContent.Children.ForEach(delegate(GUIComponent child)
							{
								child.State = c.State;
							});
							GUILayoutGroup group = missionTextContent.FindChild("difficulty", true) as GUILayoutGroup;
							if (group != null)
							{
								group.State = c.State;
							}
						};
						if (mission != availableMissions.Last<Mission>())
						{
							new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), this.missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null).CanBeFocused = false;
						}
					}
				}
				if (prevSelectedLocation == this.selectedLocation)
				{
					this.missionList.BarScroll = prevMissionListScroll;
					this.missionList.UpdateDimensions();
					this.missionList.UpdateScrollBarSize();
				}
			}
			Location destination = connection.OtherLocation(currentDisplayLocation);
			this.UpdateMaxMissions(destination);
			GUILayoutGroup buttonArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			RectTransform rectT10 = new RectTransform(new Vector2(0.6f, 1f), buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text10 = "";
			font = GUIStyle.SubHeadingFont;
			Func<Mission, bool> <>9__11;
			new GUITextBlock(rectT10, text10, null, font, Alignment.Left, false, "", null).TextGetter = delegate()
			{
				int missionCount = 0;
				if (GameMain.GameSession != null)
				{
					Map map2 = this.Campaign.Map;
					bool flag;
					if (map2 == null)
					{
						flag = (null != null);
					}
					else
					{
						Location currentLocation2 = map2.CurrentLocation;
						flag = (((currentLocation2 != null) ? currentLocation2.SelectedMissions : null) != null);
					}
					if (flag)
					{
						IEnumerable<Mission> selectedMissions = this.Campaign.Map.CurrentLocation.SelectedMissions;
						Func<Mission, bool> predicate;
						if ((predicate = <>9__11) == null)
						{
							predicate = (<>9__11 = ((Mission m) => m.Locations.Contains(location) && !GameMain.GameSession.Missions.Contains(m) && !m.Prefab.IsSideObjective));
						}
						missionCount = selectedMissions.Count(predicate);
					}
				}
				char punctuationSymbol = ':';
				LocalizedString[] array = new LocalizedString[2];
				array[0] = TextManager.Get("Missions");
				int num2 = 1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler3.AppendFormatted<int>(missionCount);
				defaultInterpolatedStringHandler3.AppendLiteral("/");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.Campaign.Settings.TotalMaxMissionCount);
				array[num2] = defaultInterpolatedStringHandler3.ToStringAndClear();
				return TextManager.AddPunctuation(punctuationSymbol, array);
			};
			Func<GUIComponent, bool> <>9__14;
			this.StartButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("StartCampaignButton"), Alignment.Center, "GUIButtonLarge", null)
			{
				OnClicked = delegate(GUIButton btn, object obj)
				{
					if (this.missionList.Content.FindChild(delegate(GUIComponent c)
					{
						GUITickBox tickBox2 = c as GUITickBox;
						return tickBox2 != null && tickBox2.Selected;
					}, true) == null)
					{
						IEnumerable<GUIComponent> children = this.missionList.Content.Children;
						Func<GUIComponent, bool> predicate;
						if ((predicate = <>9__14) == null)
						{
							predicate = (<>9__14 = delegate(GUIComponent c)
							{
								Mission mission3 = c.UserData as Mission;
								if (mission3 != null)
								{
									MissionPrefab prefab2 = mission3.Prefab;
									if (prefab2 != null && prefab2.ShowInMenus && !prefab2.IsSideObjective)
									{
										IEnumerable<Location> locations = mission3.Locations;
										CampaignMode campaign = this.Campaign;
										Location value;
										if (campaign == null)
										{
											value = null;
										}
										else
										{
											Map map2 = campaign.Map;
											value = ((map2 != null) ? map2.CurrentLocation : null);
										}
										return locations.Contains(value);
									}
								}
								return false;
							});
						}
						if (children.Any(predicate))
						{
							GUIMessageBox noMissionVerification = new GUIMessageBox(string.Empty, TextManager.Get("nomissionprompt"), new LocalizedString[]
							{
								TextManager.Get("yes"),
								TextManager.Get("no")
							}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
							noMissionVerification.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
							{
								Action startRound2 = this.StartRound;
								if (startRound2 != null)
								{
									startRound2();
								}
								noMissionVerification.Close();
								return true;
							};
							noMissionVerification.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(noMissionVerification.Close);
							return true;
						}
					}
					Action startRound = this.StartRound;
					if (startRound != null)
					{
						startRound();
					}
					return true;
				},
				Enabled = true,
				Visible = CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap)
			};
			buttonArea.RectTransform.MinSize = new Point(0, this.StartButton.RectTransform.MinSize.Y);
			if (Level.Loaded != null)
			{
				LocationConnection connection4 = connection;
				if (((connection4 != null) ? connection4.LevelData : null) == Level.Loaded.LevelData)
				{
					Location currentDisplayLocation2 = currentDisplayLocation;
					Map map = this.Campaign.Map;
					if (currentDisplayLocation2 == ((map != null) ? map.CurrentLocation : null))
					{
						this.StartButton.Visible = false;
						this.missionList.Enabled = false;
					}
				}
			}
		}

		// Token: 0x06002460 RID: 9312 RVA: 0x00170958 File Offset: 0x0016EB58
		public void SelectTab(CampaignMode.InteractionType tab, Character npc = null)
		{
			if (this.Campaign.ShowCampaignUI || (this.Campaign.ForceMapUI && tab == CampaignMode.InteractionType.Map))
			{
				HintManager.OnShowCampaignInterface(tab);
			}
			this.selectedTab = tab;
			for (int i = 0; i < this.tabs.Length; i++)
			{
				if (this.tabs[i] != null)
				{
					this.tabs[i].Visible = (this.selectedTab == (CampaignMode.InteractionType)i);
				}
			}
			this.locationInfoPanel.Visible = (tab == CampaignMode.InteractionType.Map && this.selectedLocation != null);
			switch (this.selectedTab)
			{
			case CampaignMode.InteractionType.Map:
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					Map map = gameSession.Map;
					if (map != null)
					{
						map.ResetPendingSub();
					}
				}
				foreach (GUITextBlock rewardText in this.missionRewardTexts)
				{
					Mission mission = (Mission)rewardText.UserData;
					rewardText.Text = mission.GetMissionRewardText(Submarine.MainSub);
				}
				break;
			}
			case CampaignMode.InteractionType.Crew:
				this.HRManagerUI.UpdateCrew();
				this.HRManagerUI.UpdateHireables();
				return;
			case CampaignMode.InteractionType.Store:
				this.Store.SelectStore(npc);
				return;
			case CampaignMode.InteractionType.Upgrade:
				break;
			case CampaignMode.InteractionType.PurchaseSub:
				if (this.submarineSelection == null)
				{
					this.submarineSelection = new SubmarineSelection(false, delegate()
					{
						this.Campaign.ShowCampaignUI = false;
					}, this.tabs[7].RectTransform);
				}
				this.submarineSelection.RefreshSubmarineDisplay(true, true);
				return;
			default:
				return;
			}
		}

		// Token: 0x06002461 RID: 9313 RVA: 0x00170ADC File Offset: 0x0016ECDC
		public static LocalizedString GetMoney()
		{
			string tag = "PlayerCredits";
			string varName = "[credits]";
			GameSession gameSession = GameMain.GameSession;
			return TextManager.GetWithVariable(tag, varName, (((gameSession != null) ? gameSession.Campaign : null) == null) ? "0" : string.Format(CultureInfo.InvariantCulture, "{0:N0}", GameMain.GameSession.Campaign.GetBalance(null)), FormatCapitals.No);
		}

		// Token: 0x06002462 RID: 9314 RVA: 0x00170B3C File Offset: 0x0016ED3C
		public static LocalizedString GetTotalBalance()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			return TextManager.FormatCurrency((campaign != null) ? campaign.GetBalance(null) : 0, true);
		}

		// Token: 0x06002463 RID: 9315 RVA: 0x00170B70 File Offset: 0x0016ED70
		public static LocalizedString GetBankBalance()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			return TextManager.FormatCurrency((campaign != null) ? campaign.Bank.Balance : 0, true);
		}

		// Token: 0x06002464 RID: 9316 RVA: 0x00170BA8 File Offset: 0x0016EDA8
		public static LocalizedString GetWalletBalance()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			return TextManager.FormatCurrency((campaign != null) ? campaign.Wallet.Balance : 0, true);
		}

		// Token: 0x06002465 RID: 9317 RVA: 0x00170BDE File Offset: 0x0016EDDE
		private void UpdateMaxMissions(Location location)
		{
			this.hasMaxMissions = (this.Campaign.NumberOfSelectableMissionsAtLocation(location) >= this.Campaign.Settings.TotalMaxMissionCount);
		}

		// Token: 0x06002466 RID: 9318 RVA: 0x00170C08 File Offset: 0x0016EE08
		public static CampaignUI.PlayerBalanceElement? AddBalanceElement(GUIComponent elementParent, Vector2 relativeSize)
		{
			GUILayoutGroup parent = new GUILayoutGroup(new RectTransform(relativeSize, elementParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopRight);
			if (GameMain.IsSingleplayer)
			{
				GUIComponent parent2 = parent;
				bool visible = true;
				LocalizedString text = TextManager.Get("campaignstore.balance");
				GUITextBlock.TextGetterHandler textGetter;
				if ((textGetter = CampaignUI.<>O.<0>__GetTotalBalance) == null)
				{
					textGetter = (CampaignUI.<>O.<0>__GetTotalBalance = new GUITextBlock.TextGetterHandler(CampaignUI.GetTotalBalance));
				}
				CampaignUI.<AddBalanceElement>g__AddBalance|57_0(parent2, visible, text, textGetter);
				return null;
			}
			bool displaySeparateBalances = CampaignMode.AllowedToManageWallets();
			GUIComponent parent3 = parent;
			bool visible2 = displaySeparateBalances;
			LocalizedString text2 = TextManager.Get("campaignstore.total");
			GUITextBlock.TextGetterHandler textGetter2;
			if ((textGetter2 = CampaignUI.<>O.<0>__GetTotalBalance) == null)
			{
				textGetter2 = (CampaignUI.<>O.<0>__GetTotalBalance = new GUITextBlock.TextGetterHandler(CampaignUI.GetTotalBalance));
			}
			GUILayoutGroup totalBalanceContainer = CampaignUI.<AddBalanceElement>g__AddBalance|57_0(parent3, visible2, text2, textGetter2);
			GUIComponent parent4 = parent;
			bool visible3 = displaySeparateBalances;
			LocalizedString text3 = TextManager.Get("crewwallet.bank");
			GUITextBlock.TextGetterHandler textGetter3;
			if ((textGetter3 = CampaignUI.<>O.<1>__GetBankBalance) == null)
			{
				textGetter3 = (CampaignUI.<>O.<1>__GetBankBalance = new GUITextBlock.TextGetterHandler(CampaignUI.GetBankBalance));
			}
			GUILayoutGroup bankBalanceContainer = CampaignUI.<AddBalanceElement>g__AddBalance|57_0(parent4, visible3, text3, textGetter3);
			GUIComponent parent5 = parent;
			bool visible4 = true;
			LocalizedString text4 = TextManager.Get("crewwallet.wallet");
			GUITextBlock.TextGetterHandler textGetter4;
			if ((textGetter4 = CampaignUI.<>O.<2>__GetWalletBalance) == null)
			{
				textGetter4 = (CampaignUI.<>O.<2>__GetWalletBalance = new GUITextBlock.TextGetterHandler(CampaignUI.GetWalletBalance));
			}
			CampaignUI.<AddBalanceElement>g__AddBalance|57_0(parent5, visible4, text4, textGetter4);
			CampaignUI.PlayerBalanceElement playerBalanceElement = new CampaignUI.PlayerBalanceElement(displaySeparateBalances, parent, totalBalanceContainer, bankBalanceContainer);
			parent.Recalculate();
			return new CampaignUI.PlayerBalanceElement?(playerBalanceElement);
		}

		// Token: 0x06002467 RID: 9319 RVA: 0x00170D34 File Offset: 0x0016EF34
		public static CampaignUI.PlayerBalanceElement? UpdateBalanceElement(CampaignUI.PlayerBalanceElement? playerBalanceElement)
		{
			if (playerBalanceElement != null)
			{
				CampaignUI.PlayerBalanceElement balanceElement = playerBalanceElement.GetValueOrDefault();
				bool displaySeparateBalances = CampaignMode.AllowedToManageWallets();
				if (displaySeparateBalances != balanceElement.DisplaySeparateBalances)
				{
					balanceElement.TotalBalanceContainer.Visible = displaySeparateBalances;
					balanceElement.BankBalanceContainer.Visible = displaySeparateBalances;
					playerBalanceElement = new CampaignUI.PlayerBalanceElement?(new CampaignUI.PlayerBalanceElement(balanceElement, displaySeparateBalances));
					balanceElement.ParentComponent.Recalculate();
				}
			}
			return playerBalanceElement;
		}

		// Token: 0x0600246A RID: 9322 RVA: 0x00170E50 File Offset: 0x0016F050
		[CompilerGenerated]
		internal static GUILayoutGroup <AddBalanceElement>g__AddBalance|57_0(GUIComponent parent, bool visible, LocalizedString text, GUITextBlock.TextGetterHandler textGetter)
		{
			float balanceContainerWidth = GameMain.IsSingleplayer ? 1f : 0.33333334f;
			RectTransform rt = new RectTransform(new Vector2(balanceContainerWidth, 1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(GUI.IntScale(GUI.AdjustForTextScale(120f)), int.MaxValue)
			};
			GUILayoutGroup balanceContainer = new GUILayoutGroup(rt, false, Anchor.TopRight)
			{
				RelativeSpacing = 0.005f,
				Visible = visible
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.5f), balanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = text;
			GUIFont font = GUIStyle.Font;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text2, null, font, Alignment.BottomRight, false, "", null);
			guitextBlock.AutoScaleVertical = true;
			guitextBlock.ForceUpperCase = ForceUpperCase.Yes;
			GUITextBlock guitextBlock2 = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), balanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.White), GUIStyle.SubHeadingFont, Alignment.TopRight, false, "", null);
			guitextBlock2.AutoScaleVertical = true;
			guitextBlock2.TextScale = 1.1f;
			guitextBlock2.TextGetter = textGetter;
			return balanceContainer;
		}

		// Token: 0x04001213 RID: 4627
		private CampaignMode.InteractionType selectedTab;

		// Token: 0x04001214 RID: 4628
		private GUIFrame[] tabs;

		// Token: 0x04001215 RID: 4629
		private Point prevResolution;

		// Token: 0x04001216 RID: 4630
		private GUIComponent locationInfoPanel;

		// Token: 0x04001217 RID: 4631
		private GUIListBox missionList;

		// Token: 0x04001218 RID: 4632
		private readonly List<GUITickBox> missionTickBoxes = new List<GUITickBox>();

		// Token: 0x04001219 RID: 4633
		private readonly List<GUITextBlock> missionRewardTexts = new List<GUITextBlock>();

		// Token: 0x0400121A RID: 4634
		private bool hasMaxMissions;

		// Token: 0x0400121B RID: 4635
		private SubmarineSelection submarineSelection;

		// Token: 0x0400121C RID: 4636
		private Location selectedLocation;

		// Token: 0x0400121D RID: 4637
		public Action StartRound;

		// Token: 0x02000C16 RID: 3094
		public readonly struct PlayerBalanceElement
		{
			// Token: 0x06007ADC RID: 31452 RVA: 0x00382739 File Offset: 0x00380939
			public PlayerBalanceElement(bool displaySeparateBalances, GUILayoutGroup parentComponent, GUILayoutGroup totalBalanceContainer, GUILayoutGroup bankBalanceContainer)
			{
				this.DisplaySeparateBalances = displaySeparateBalances;
				this.ParentComponent = parentComponent;
				this.TotalBalanceContainer = totalBalanceContainer;
				this.BankBalanceContainer = bankBalanceContainer;
			}

			// Token: 0x06007ADD RID: 31453 RVA: 0x00382758 File Offset: 0x00380958
			public PlayerBalanceElement(CampaignUI.PlayerBalanceElement element, bool displaySeparateBalances)
			{
				this.DisplaySeparateBalances = displaySeparateBalances;
				this.ParentComponent = element.ParentComponent;
				this.TotalBalanceContainer = element.TotalBalanceContainer;
				this.BankBalanceContainer = element.BankBalanceContainer;
			}

			// Token: 0x040049E8 RID: 18920
			public readonly bool DisplaySeparateBalances;

			// Token: 0x040049E9 RID: 18921
			public readonly GUILayoutGroup ParentComponent;

			// Token: 0x040049EA RID: 18922
			public readonly GUILayoutGroup TotalBalanceContainer;

			// Token: 0x040049EB RID: 18923
			public readonly GUILayoutGroup BankBalanceContainer;
		}

		// Token: 0x02000C17 RID: 3095
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040049EC RID: 18924
			public static GUITextBlock.TextGetterHandler <0>__GetTotalBalance;

			// Token: 0x040049ED RID: 18925
			public static GUITextBlock.TextGetterHandler <1>__GetBankBalance;

			// Token: 0x040049EE RID: 18926
			public static GUITextBlock.TextGetterHandler <2>__GetWalletBalance;
		}
	}
}
