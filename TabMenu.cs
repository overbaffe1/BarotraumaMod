using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000B6 RID: 182
	internal class TabMenu
	{
		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x060016D7 RID: 5847 RVA: 0x000D917B File Offset: 0x000D737B
		// (set) Token: 0x060016D8 RID: 5848 RVA: 0x000D9182 File Offset: 0x000D7382
		public static TabMenu.InfoFrameTab SelectedTab { get; private set; }

		// Token: 0x060016D9 RID: 5849 RVA: 0x000D918C File Offset: 0x000D738C
		public void Initialize()
		{
			TabMenu.spectateIcon = GUIStyle.GetComponentStyle("SpectateIcon").Sprites[GUIComponent.ComponentState.None][0];
			TabMenu.disconnectedIcon = GUIStyle.GetComponentStyle("DisconnectedIcon").Sprites[GUIComponent.ComponentState.None][0];
			TabMenu.ownerIcon = GUIStyle.GetComponentStyle("OwnerIcon").GetDefaultSprite();
			TabMenu.moderatorIcon = GUIStyle.GetComponentStyle("ModeratorIcon").GetDefaultSprite();
			TabMenu.initialized = true;
		}

		// Token: 0x060016DA RID: 5850 RVA: 0x000D9208 File Offset: 0x000D7408
		public TabMenu()
		{
			if (!TabMenu.initialized)
			{
				this.Initialize();
			}
			if (Level.Loaded == null)
			{
				TabMenu.SelectedTab = TabMenu.InfoFrameTab.Crew;
			}
			this.CreateInfoFrame(TabMenu.SelectedTab);
			this.SelectInfoFrameTab(TabMenu.SelectedTab);
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x000D9278 File Offset: 0x000D7478
		public void Update(float deltaTime)
		{
			float menuOpenSpeed = deltaTime * 10f;
			if (this.isTransferMenuOpen)
			{
				if (this.transferMenuStateCompleted)
				{
					this.transferMenuOpenState = ((this.transferMenuOpenState < 0.25f) ? Math.Min(0.25f, this.transferMenuOpenState + menuOpenSpeed / 2f) : 0.25f);
				}
				else if (this.transferMenuOpenState > 0.15f)
				{
					this.transferMenuStateCompleted = false;
					this.transferMenuOpenState = Math.Max(0.15f, this.transferMenuOpenState - menuOpenSpeed);
				}
				else
				{
					this.transferMenuStateCompleted = true;
				}
			}
			else
			{
				this.transferMenuStateCompleted = false;
				if (this.transferMenuOpenState < 1f)
				{
					this.transferMenuOpenState = Math.Min(1f, this.transferMenuOpenState + menuOpenSpeed);
				}
			}
			if (this.transferMenu != null && this.transferMenuButton != null)
			{
				int pos = (int)(this.transferMenuOpenState * (float)(-(float)this.transferMenu.Rect.Height));
				this.transferMenu.RectTransform.AbsoluteOffset = new Point(0, pos);
				this.transferMenuButton.RectTransform.AbsoluteOffset = new Point(0, -pos - this.transferMenu.Rect.Height);
			}
			GameSession.UpdateTalentNotificationIndicator(this.talentPointNotification);
			TalentMenu talentMenu = this.talentMenu;
			if (talentMenu != null)
			{
				talentMenu.Update();
			}
			if (TabMenu.SelectedTab != TabMenu.InfoFrameTab.Crew)
			{
				return;
			}
			if (this.linkedGUIList == null)
			{
				return;
			}
			if (GameMain.IsMultiplayer)
			{
				for (int i = 0; i < this.linkedGUIList.Count; i++)
				{
					this.linkedGUIList[i].TryPingRefresh();
					this.linkedGUIList[i].TryPermissionIconRefresh(this.GetPermissionIcon(this.linkedGUIList[i].Client));
					if (this.linkedGUIList[i].HasMultiplayerCharacterChanged() || this.linkedGUIList[i].HasCharacterDied())
					{
						this.RemoveCurrentElements();
						this.CreateMultiPlayerList(true);
						return;
					}
				}
				return;
			}
			for (int j = 0; j < this.linkedGUIList.Count; j++)
			{
				if (this.linkedGUIList[j].HasCharacterDied())
				{
					this.RemoveCurrentElements();
					this.CreateSinglePlayerList(true);
					return;
				}
			}
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x000D9491 File Offset: 0x000D7691
		public void AddToGUIUpdateList()
		{
			GUIFrame guiframe = this.infoFrame;
			if (guiframe != null)
			{
				guiframe.AddToGUIUpdateList(false, 0);
			}
			GUIButton jobInfoFrame = NetLobbyScreen.JobInfoFrame;
			if (jobInfoFrame == null)
			{
				return;
			}
			jobInfoFrame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x000D94B7 File Offset: 0x000D76B7
		public static void OnRoundEnded()
		{
			TabMenu.storedMessages.Clear();
			TabMenu.PendingChanges = false;
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x000D94CC File Offset: 0x000D76CC
		private void CreateInfoFrame(TabMenu.InfoFrameTab selectedTab)
		{
			TabMenu.<>c__DisplayClass38_0 CS$<>8__locals1 = new TabMenu.<>c__DisplayClass38_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.selectedTab = selectedTab;
			this.tabButtons.Clear();
			this.infoFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.infoFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			Vector2 contentFrameSize = new Vector2(0.45f, 0.667f);
			this.contentFrame = new GUIFrame(new RectTransform(contentFrameSize, this.infoFrame.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.12f)
			}, "", null);
			GUILayoutGroup horizontalLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.958f, 0.943f), this.contentFrame.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(0, GUI.IntScale(25f))
			}, true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f
			};
			CS$<>8__locals1.buttonArea = new GUILayoutGroup(new RectTransform(new Vector2(0.07f, 1f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			GUILayoutGroup innerLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.92f, 1f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			float absoluteSpacing = innerLayoutGroup.RelativeSpacing * (float)innerLayoutGroup.Rect.Height;
			GameSession gameSession = GameMain.GameSession;
			int multiplier = (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode) ? 2 : 1;
			int infoFrameHolderHeight = Math.Min((int)(0.97f * (float)innerLayoutGroup.Rect.Height), (int)((float)innerLayoutGroup.Rect.Height - (float)multiplier * ((float)GUI.IntScale(15f) + absoluteSpacing)));
			this.infoFrameHolder = new GUIFrame(new RectTransform(new Point(innerLayoutGroup.Rect.Width, infoFrameHolderHeight), innerLayoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
			GUIButton crewButton = CS$<>8__locals1.<CreateInfoFrame>g__createTabButton|0(TabMenu.InfoFrameTab.Crew, "crew");
			GameSession gameSession2 = GameMain.GameSession;
			if (!(((gameSession2 != null) ? gameSession2.GameMode : null) is TestGameMode))
			{
				GUIButton missionBtn = CS$<>8__locals1.<CreateInfoFrame>g__createTabButton|0(TabMenu.InfoFrameTab.Mission, "mission");
				this.eventLogNotification = GameSession.CreateNotificationIcon(missionBtn, true);
				GUIComponent guicomponent = this.eventLogNotification;
				GameSession gameSession3 = GameMain.GameSession;
				bool? flag;
				if (gameSession3 == null)
				{
					flag = null;
				}
				else
				{
					EventManager eventManager = gameSession3.EventManager;
					if (eventManager == null)
					{
						flag = null;
					}
					else
					{
						EventLog eventLog = eventManager.EventLog;
						flag = ((eventLog != null) ? new bool?(eventLog.UnreadEntries) : null);
					}
				}
				bool? flag2 = flag;
				guicomponent.Visible = flag2.GetValueOrDefault();
				if (this.eventLogNotification.Visible)
				{
					this.eventLogNotification.Pulsate(Vector2.One, Vector2.One * 2f, 1f);
				}
			}
			GameSession gameSession4 = GameMain.GameSession;
			CampaignMode campaignMode = ((gameSession4 != null) ? gameSession4.GameMode : null) as CampaignMode;
			if (campaignMode != null)
			{
				TabMenu.<>c__DisplayClass38_1 CS$<>8__locals2 = new TabMenu.<>c__DisplayClass38_1();
				GUIButton reputationButton = CS$<>8__locals1.<CreateInfoFrame>g__createTabButton|0(TabMenu.InfoFrameTab.Reputation, "reputation");
				GUIFrame balanceFrame = new GUIFrame(new RectTransform(new Point(innerLayoutGroup.Rect.Width, innerLayoutGroup.Rect.Height - infoFrameHolderHeight), innerLayoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "InnerFrame", null);
				GUILayoutGroup salaryFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.66f, 1f), balanceFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
				CS$<>8__locals2.salaryScrollBar = null;
				CS$<>8__locals2.salaryPercentage = null;
				GameSession gameSession5 = GameMain.GameSession;
				if (((gameSession5 != null) ? gameSession5.GameMode : null) is MultiPlayerCampaign)
				{
					TabMenu.<>c__DisplayClass38_2 CS$<>8__locals3 = new TabMenu.<>c__DisplayClass38_2();
					CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals2;
					float value2 = (float)campaignMode.Bank.RewardDistribution;
					new GUITextBlock(new RectTransform(new Vector2(0.25f, 1f), salaryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("defaultsalary"), null, null, Alignment.Center, false, "", null).AutoScaleHorizontal = true;
					CS$<>8__locals3.CS$<>8__locals1.salaryScrollBar = new GUIScrollBar(new RectTransform(new Vector2(0.4f, 1f), salaryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null)
					{
						Range = new Vector2(0f, 1f),
						BarScrollValue = value2 / 100f,
						Step = 0.01f,
						BarSize = 0.1f
					};
					CS$<>8__locals3.CS$<>8__locals1.salaryPercentage = new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), salaryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "0", null, null, Alignment.Center, false, "", null)
					{
						Text = TabMenu.<CreateInfoFrame>g__ValueToPercentage|38_4((float)TabMenu.RoundRewardDistribution(CS$<>8__locals3.CS$<>8__locals1.salaryScrollBar.BarScroll, CS$<>8__locals3.CS$<>8__locals1.salaryScrollBar.Step))
					};
					CS$<>8__locals3.CS$<>8__locals1.salaryScrollBar.OnMoved = delegate(GUIScrollBar scrollBar, float value)
					{
						CS$<>8__locals3.CS$<>8__locals1.salaryPercentage.Text = TabMenu.<CreateInfoFrame>g__ValueToPercentage|38_4((float)TabMenu.RoundRewardDistribution(value, scrollBar.Step));
						return true;
					};
					CS$<>8__locals3.CS$<>8__locals1.salaryScrollBar.OnReleased = delegate(GUIScrollBar bar, float scroll)
					{
						int newRewardDistribution = TabMenu.RoundRewardDistribution(scroll, bar.Step);
						Option.UnspecifiedNone none = Option.None;
						TabMenu.SetRewardDistribution(none, newRewardDistribution);
						return true;
					};
					TabMenu.<>c__DisplayClass38_2 CS$<>8__locals4 = CS$<>8__locals3;
					GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), salaryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ResetSalaries"), Alignment.Center, "GUIButtonSmall", null);
					guibutton.TextBlock.AutoScaleHorizontal = true;
					guibutton.ToolTip = TextManager.Get("resetsalaries.tooltip");
					guibutton.OnClicked = delegate(GUIButton button, object userData)
					{
						LocalizedString header = TextManager.Get("ResetSalaries");
						LocalizedString body = TextManager.Get("ResetSalaries.Warning");
						Action onConfirm;
						if ((onConfirm = TabMenu.<>O.<0>__ResetRewardDistributions) == null)
						{
							onConfirm = (TabMenu.<>O.<0>__ResetRewardDistributions = new Action(TabMenu.ResetRewardDistributions));
						}
						GUI.AskForConfirmation(header, body, onConfirm, null, null, null);
						return true;
					};
					CS$<>8__locals4.resetButton = guibutton;
					CS$<>8__locals3.<CreateInfoFrame>g__UpdateSliderEnabled|7();
					Identifier defaultSalaryEventIdentifier = "DefaultSalarySlider".ToIdentifier();
					GameClient client = GameMain.Client;
					if (client != null)
					{
						NamedEvent<GameClient.PermissionChangedEvent> onPermissionChanged = client.OnPermissionChanged;
						if (onPermissionChanged != null)
						{
							onPermissionChanged.RegisterOverwriteExisting(defaultSalaryEventIdentifier, delegate(GameClient.PermissionChangedEvent _)
							{
								base.<CreateInfoFrame>g__UpdateSliderEnabled|7();
							});
						}
					}
				}
				CS$<>8__locals2.balanceText = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), balanceFrame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Right, false, "", null);
				if (GameMain.IsMultiplayer)
				{
					CS$<>8__locals2.balanceText.ToolTip = TextManager.Get("bankdescription");
				}
				GUIFrame bottomDisclaimerFrame = new GUIFrame(new RectTransform(new Vector2(contentFrameSize.X, 0.1f), this.infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point(this.contentFrame.Rect.X, this.contentFrame.Rect.Bottom + GUI.IntScale(8f))
				}, null, null);
				TabMenu.PendingChangesFrame = new GUIFrame(new RectTransform(Vector2.One, bottomDisclaimerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null && netLobbyScreen.CampaignCharacterDiscarded)
				{
					NetLobbyScreen.CreateChangesPendingFrame(TabMenu.PendingChangesFrame);
				}
				TabMenu.<CreateInfoFrame>g__SetBalanceText|38_3(CS$<>8__locals2.balanceText, campaignMode.Bank.Balance);
				Identifier eventIdentifier = "CreateInfoFrame".ToIdentifier();
				campaignMode.OnMoneyChanged.RegisterOverwriteExisting(eventIdentifier, delegate(WalletChangedEvent e)
				{
					if (!e.Owner.IsNone())
					{
						return;
					}
					TabMenu.<CreateInfoFrame>g__SetBalanceText|38_3(CS$<>8__locals2.balanceText, e.Wallet.Balance);
					if (CS$<>8__locals2.salaryPercentage != null && CS$<>8__locals2.salaryScrollBar != null)
					{
						float rewardDistribution = (float)e.Wallet.RewardDistribution;
						CS$<>8__locals2.salaryScrollBar.BarScrollValue = rewardDistribution / 100f;
						CS$<>8__locals2.salaryPercentage.Text = TabMenu.<CreateInfoFrame>g__ValueToPercentage|38_4(rewardDistribution);
					}
				});
				this.registeredEvents.Add(eventIdentifier);
			}
			if (Submarine.MainSub != null)
			{
				CS$<>8__locals1.<CreateInfoFrame>g__createTabButton|0(TabMenu.InfoFrameTab.Submarine, "submarine");
			}
			CS$<>8__locals1.talentsButton = CS$<>8__locals1.<CreateInfoFrame>g__createTabButton|0(TabMenu.InfoFrameTab.Talents, "tabmenu.character");
			GUIButton talentsButton = CS$<>8__locals1.talentsButton;
			talentsButton.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(talentsButton.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
			{
				GUIComponent talentsButton2 = CS$<>8__locals1.talentsButton;
				Character controlled = Character.Controlled;
				bool enabled;
				if (((controlled != null) ? controlled.Info : null) == null)
				{
					GameClient client2 = GameMain.Client;
					enabled = (((client2 != null) ? client2.CharacterInfo : null) != null);
				}
				else
				{
					enabled = true;
				}
				talentsButton2.Enabled = enabled;
				if (!CS$<>8__locals1.talentsButton.Enabled && CS$<>8__locals1.selectedTab == TabMenu.InfoFrameTab.Talents)
				{
					CS$<>8__locals1.<>4__this.SelectInfoFrameTab(TabMenu.InfoFrameTab.Crew);
				}
			}));
			this.talentPointNotification = GameSession.CreateNotificationIcon(CS$<>8__locals1.talentsButton, true);
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x000D9EBC File Offset: 0x000D80BC
		public void SelectInfoFrameTab(TabMenu.InfoFrameTab selectedTab)
		{
			TabMenu.SelectedTab = selectedTab;
			this.CreateInfoFrame(selectedTab);
			this.tabButtons.ForEach(delegate(GUIButton tb)
			{
				tb.Selected = ((TabMenu.InfoFrameTab)tb.UserData == selectedTab);
			});
			switch (selectedTab)
			{
			case TabMenu.InfoFrameTab.Crew:
				this.CreateCrewListFrame(this.infoFrameHolder);
				return;
			case TabMenu.InfoFrameTab.Mission:
				this.CreateMissionInfo(this.infoFrameHolder);
				return;
			case TabMenu.InfoFrameTab.Reputation:
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.RoundSummary : null) != null)
				{
					GameSession gameSession2 = GameMain.GameSession;
					CampaignMode campaignMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
					if (campaignMode != null)
					{
						this.infoFrameHolder.ClearChildren();
						GUIFrame reputationFrame = new GUIFrame(new RectTransform(Vector2.One, this.infoFrameHolder.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
						GameMain.GameSession.RoundSummary.CreateReputationInfoPanel(reputationFrame, campaignMode);
						return;
					}
				}
				break;
			}
			case TabMenu.InfoFrameTab.Submarine:
				TabMenu.CreateSubmarineInfo(this.infoFrameHolder, Submarine.MainSub);
				return;
			case TabMenu.InfoFrameTab.Talents:
			{
				TalentMenu talentMenu = this.talentMenu;
				GUIFrame parent = this.infoFrameHolder;
				Character controlled = Character.Controlled;
				CharacterInfo characterInfo;
				if ((characterInfo = ((controlled != null) ? controlled.Info : null)) == null)
				{
					GameClient client = GameMain.Client;
					characterInfo = ((client != null) ? client.CharacterInfo : null);
				}
				talentMenu.CreateGUI(parent, characterInfo);
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x060016E0 RID: 5856 RVA: 0x000DA024 File Offset: 0x000D8224
		private void CreateCrewListFrame(GUIFrame crewFrame)
		{
			GameSession gameSession = GameMain.GameSession;
			List<Character> list;
			if (gameSession == null)
			{
				list = null;
			}
			else
			{
				CrewManager crewManager = gameSession.CrewManager;
				list = ((crewManager != null) ? crewManager.GetCharacters() : null);
			}
			List<Character> list2;
			if ((list2 = list) == null)
			{
				(list2 = new List<Character>()).Add(TestScreen.dummyCharacter);
			}
			this.crew = list2;
			this.teamIDs = (from c in this.crew
			select c.TeamID).Distinct<CharacterTeamType>().ToList<CharacterTeamType>();
			if (this.teamIDs.Count > 1)
			{
				GameClient client = GameMain.Client;
				if (((client != null) ? client.Character : null) != null)
				{
					CharacterTeamType ownTeam = GameMain.Client.Character.TeamID;
					this.teamIDs = (from i in this.teamIDs
					orderby i != ownTeam, i
					select i).ToList<CharacterTeamType>();
				}
			}
			if (!this.teamIDs.Any<CharacterTeamType>())
			{
				this.teamIDs.Add(CharacterTeamType.None);
			}
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(Vector2.One, crewFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			this.crewListArray = new GUIListBox[this.teamIDs.Count];
			GUILayoutGroup[] headerFrames = new GUILayoutGroup[this.teamIDs.Count];
			float nameHeight = 0.075f;
			Vector2 crewListSize = new Vector2(1f, 1f / (float)this.teamIDs.Count - ((this.teamIDs.Count > 1) ? (nameHeight * 1.1f) : 0f));
			for (int k = 0; k < this.teamIDs.Count; k++)
			{
				if (this.teamIDs.Count > 1)
				{
					GUITextBlock nameText = new GUITextBlock(new RectTransform(new Vector2(1f, nameHeight), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CombatMission.GetTeamName(this.teamIDs[k]), new Color?(CombatMission.GetTeamColor(this.teamIDs[k])), null, Alignment.Left, false, "", null)
					{
						ForceUpperCase = ForceUpperCase.Yes
					};
					GUIImage teamIcon = new GUIImage(new RectTransform(Vector2.One, nameText.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.BothHeight), (this.teamIDs[k] == CharacterTeamType.Team2) ? "SeparatistIcon" : "CoalitionIcon", GUIImage.ScalingMode.None)
					{
						Color = nameText.TextColor
					};
					nameText.Padding = new Vector4((float)teamIcon.Rect.Width + nameText.Padding.X, nameText.Padding.Y, nameText.Padding.Z, nameText.Padding.W);
				}
				headerFrames[k] = new GUILayoutGroup(new RectTransform(Vector2.Zero, content.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.BottomLeft), null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point(2, -1)
				}, true, Anchor.TopLeft)
				{
					Stretch = true,
					AbsoluteSpacing = 2,
					UserData = k
				};
				GUIListBox crewList = new GUIListBox(new RectTransform(crewListSize, content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					Padding = new Vector4(2f, 5f, 0f, 0f),
					AutoHideScrollBar = false,
					PlaySoundOnSelect = true
				};
				crewList.UpdateDimensions();
				if (this.teamIDs.Count > 1)
				{
					crewList.OnSelected = delegate(GUIComponent component, object obj)
					{
						for (int i = 0; i < this.crewListArray.Length; i++)
						{
							if (this.crewListArray[i] != crewList)
							{
								this.crewListArray[i].Deselect();
							}
						}
						this.SelectElement(component.UserData, crewList);
						return true;
					};
				}
				else
				{
					crewList.OnSelected = delegate(GUIComponent component, object obj)
					{
						this.SelectElement(component.UserData, crewList);
						return true;
					};
				}
				this.crewListArray[k] = crewList;
			}
			for (int j = 0; j < this.teamIDs.Count; j++)
			{
				headerFrames[j].RectTransform.RelativeSize = new Vector2(1f - (float)this.crewListArray[j].ScrollBar.Rect.Width / (float)this.crewListArray[j].Rect.Width, GUIStyle.HotkeyFont.Size / (float)crewFrame.RectTransform.Rect.Height * 1.5f);
				if (!GameMain.IsMultiplayer)
				{
					this.CreateSinglePlayerListContentHolder(headerFrames[j]);
				}
				else
				{
					this.CreateMultiPlayerListContentHolder(headerFrames[j]);
				}
			}
			crewFrame.RectTransform.AbsoluteOffset = new Point(0, headerFrames[0].Rect.Height * headerFrames.Length - ((this.teamIDs.Count > 1) ? GUI.IntScale(10f) : 0));
			float totalRelativeHeight = 0f;
			if (this.teamIDs.Count > 1)
			{
				totalRelativeHeight += (float)this.teamIDs.Count * nameHeight;
			}
			headerFrames.ForEach(delegate(GUILayoutGroup f)
			{
				totalRelativeHeight += f.RectTransform.RelativeSize.Y;
			});
			this.crewListArray.ForEach(delegate(GUIListBox f)
			{
				totalRelativeHeight += f.RectTransform.RelativeSize.Y;
			});
			if (totalRelativeHeight > 1f)
			{
				float heightOverflow = totalRelativeHeight - 1f;
				float heightToReduce = heightOverflow / (float)this.crewListArray.Length;
				this.crewListArray.ForEach(delegate(GUIListBox l)
				{
					l.RectTransform.Resize(l.RectTransform.RelativeSize - new Vector2(0f, heightToReduce), true);
					l.UpdateDimensions();
				});
			}
			if (GameMain.IsMultiplayer)
			{
				this.CreateMultiPlayerList(false);
				this.CreateMultiPlayerLogContent(crewFrame);
				return;
			}
			this.CreateSinglePlayerList(false);
		}

		// Token: 0x060016E1 RID: 5857 RVA: 0x000DA640 File Offset: 0x000D8840
		private void CreateSinglePlayerListContentHolder(GUILayoutGroup headerFrame)
		{
			GUIButton jobButton = new GUIButton(new RectTransform(new Vector2(0f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tabmenu.job"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			GUIButton characterButton = new GUIButton(new RectTransform(new Vector2(0f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("name"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			this.sizeMultiplier = (float)(headerFrame.Rect.Width - headerFrame.AbsoluteSpacing * (headerFrame.CountChildren - 1)) / (float)headerFrame.Rect.Width;
			jobButton.RectTransform.RelativeSize = new Vector2(0.138f * this.sizeMultiplier, 1f);
			characterButton.RectTransform.RelativeSize = new Vector2((1f - 0.138f * this.sizeMultiplier) * this.sizeMultiplier, 1f);
			jobButton.TextBlock.Font = (characterButton.TextBlock.Font = GUIStyle.HotkeyFont);
			jobButton.CanBeFocused = (characterButton.CanBeFocused = false);
			jobButton.TextBlock.ForceUpperCase = (characterButton.TextBlock.ForceUpperCase = ForceUpperCase.Yes);
			this.jobColumnWidth = jobButton.Rect.Width;
			this.characterColumnWidth = characterButton.Rect.Width;
		}

		// Token: 0x060016E2 RID: 5858 RVA: 0x000DA7EC File Offset: 0x000D89EC
		private void CreateSinglePlayerList(bool refresh)
		{
			if (refresh)
			{
				this.crew = GameMain.GameSession.CrewManager.GetCharacters();
			}
			this.linkedGUIList = new List<TabMenu.LinkedGUI>();
			int i;
			Func<Character, bool> <>9__0;
			int j;
			for (i = 0; i < this.teamIDs.Count; i = j + 1)
			{
				IEnumerable<Character> source = this.crew;
				Func<Character, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((Character c) => c.TeamID == this.teamIDs[i]));
				}
				foreach (Character character in source.Where(predicate))
				{
					this.CreateSinglePlayerCharacterElement(character, i);
				}
				j = i;
			}
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x000DA8C4 File Offset: 0x000D8AC4
		private void CreateSinglePlayerCharacterElement(Character character, int i)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.crewListArray[i].Content.Rect.Width, GUI.IntScale(33f)), this.crewListArray[i].Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
			{
				UserData = character,
				Color = ((Character.Controlled == character) ? TabMenu.OwnCharacterBGColor : Color.Transparent)
			};
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Point(this.jobColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				character.Info.DrawJobIcon(sb, component.Rect, false);
			}, null);
			guicustomComponent.CanBeFocused = false;
			guicustomComponent.HoverColor = Color.White;
			guicustomComponent.SelectedColor = Color.White;
			GUITextBlock characterNameBlock = new GUITextBlock(new RectTransform(new Point(this.characterColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ToolBox.LimitString(character.Info.Name, GUIStyle.Font, this.characterColumnWidth), new Color?(character.Info.Job.Prefab.UIColor), null, Alignment.Center, false, "", null);
			paddedFrame.Recalculate();
			this.linkedGUIList.Add(new TabMenu.LinkedGUI(character, frame, null));
		}

		// Token: 0x060016E4 RID: 5860 RVA: 0x000DAAC0 File Offset: 0x000D8CC0
		private void CreateMultiPlayerListContentHolder(GUILayoutGroup headerFrame)
		{
			GameSession gameSession = GameMain.GameSession;
			bool isCampaign = ((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign;
			GUIButton jobButton = new GUIButton(new RectTransform(new Vector2(0.138f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tabmenu.job"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			GUIButton characterButton = new GUIButton(new RectTransform(new Vector2(0.45f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("name"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			GameSession gameSession2 = GameMain.GameSession;
			if (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode)
			{
				GUIButton killButton = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("killcount"), Alignment.Center, "GUIButtonSmallFreeScale", null);
				this.killColumnWidth = killButton.Rect.Width;
				GUIButton deathButton = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathcount"), Alignment.Center, "GUIButtonSmallFreeScale", null);
				this.deathColumnWidth = deathButton.Rect.Width;
			}
			GUIButton pingButton = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverlistping"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			if (isCampaign)
			{
				GUIButton walletButton = new GUIButton(new RectTransform(new Vector2(0.206f, 1f), headerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("crewwallet.wallet"), Alignment.Center, "GUIButtonSmallFreeScale", null)
				{
					TextBlock = 
					{
						Font = GUIStyle.HotkeyFont
					},
					CanBeFocused = false,
					ForceUpperCase = ForceUpperCase.Yes
				};
				this.walletColumnWidth = walletButton.Rect.Width;
			}
			foreach (GUIButton btn in headerFrame.GetAllChildren<GUIButton>())
			{
				btn.TextBlock.Font = GUIStyle.HotkeyFont;
				btn.CanBeFocused = false;
				btn.ForceUpperCase = ForceUpperCase.Yes;
			}
			this.jobColumnWidth = jobButton.Rect.Width;
			this.characterColumnWidth = characterButton.Rect.Width;
			this.pingColumnWidth = pingButton.Rect.Width;
		}

		// Token: 0x060016E5 RID: 5861 RVA: 0x000DAE14 File Offset: 0x000D9014
		private void CreateMultiPlayerList(bool refresh)
		{
			if (refresh)
			{
				this.crew = GameMain.GameSession.CrewManager.GetCharacters();
			}
			this.linkedGUIList = new List<TabMenu.LinkedGUI>();
			IReadOnlyList<Client> connectedClients = GameMain.Client.ConnectedClients;
			int teamID;
			Func<Character, bool> <>9__0;
			int teamID2;
			for (teamID = 0; teamID < this.teamIDs.Count; teamID = teamID2 + 1)
			{
				IEnumerable<Character> source = this.crew;
				Func<Character, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((Character c) => c.TeamID == this.teamIDs[teamID]));
				}
				using (IEnumerator<Character> enumerator = source.Where(predicate).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character character = enumerator.Current;
						if (character is AICharacter || !connectedClients.Any((Client c) => c.Character == null && c.Name == character.Name))
						{
							this.CreateMultiPlayerCharacterElement(character, GameMain.Client.PreviouslyConnectedClients.FirstOrDefault((Client c) => c.Character == character), teamID, null);
						}
					}
				}
				CrewManager crewManager = GameMain.GameSession.CrewManager;
				foreach (CharacterInfo characterInfo in (((crewManager != null) ? crewManager.GetReserveBenchInfos() : null) ?? Enumerable.Empty<CharacterInfo>()))
				{
					this.CreateMultiPlayerCharacterElement(null, null, teamID, characterInfo);
				}
				teamID2 = teamID;
			}
			for (int i = 0; i < connectedClients.Count; i++)
			{
				Client client = connectedClients[i];
				if (client.Character == null || client.Character.IsDead)
				{
					this.CreateMultiPlayerClientElement(client);
				}
			}
		}

		// Token: 0x060016E6 RID: 5862 RVA: 0x000DAFF0 File Offset: 0x000D91F0
		private void CreateMultiPlayerCharacterElement(Character character, Client client, int teamID, CharacterInfo justCharacterInfo = null)
		{
			CharacterInfo characterInfo = justCharacterInfo ?? character.Info;
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.crewListArray[teamID].Content.Rect.Width, GUI.IntScale(33f)), this.crewListArray[teamID].Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
			{
				UserData = ((character != null) ? character : characterInfo),
				Color = ((GameMain.NetworkMember != null && GameMain.Client.Character == character) ? TabMenu.OwnCharacterBGColor : Color.Transparent)
			};
			if (client != null)
			{
				GUIFrame guiframe = frame;
				guiframe.OnSecondaryClicked = (GUIComponent.SecondaryButtonDownHandler)Delegate.Combine(guiframe.OnSecondaryClicked, new GUIComponent.SecondaryButtonDownHandler(delegate(GUIComponent component, object data)
				{
					NetLobbyScreen.CreateModerationContextMenu(client);
					return true;
				}));
			}
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Point(this.jobColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				if (client != null)
				{
					this.DrawClientJobIcon(sb, component.Rect, client);
					return;
				}
				CharacterInfo characterInfo = characterInfo;
				if (characterInfo == null)
				{
					return;
				}
				characterInfo.DrawJobIcon(sb, component.Rect, false);
			}, null);
			guicustomComponent.CanBeFocused = false;
			guicustomComponent.HoverColor = Color.White;
			guicustomComponent.SelectedColor = Color.White;
			if (client != null)
			{
				GUIImage permissionIcon;
				this.CreateNameWithPermissionIcon(client, paddedFrame, out permissionIcon);
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
				{
					Func<Mission, int> <>9__3;
					new GUITextBlock(new RectTransform(new Point(this.killColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
					{
						IEnumerable<Mission> missions = GameMain.GameSession.Missions;
						Func<Mission, int> selector;
						if ((selector = <>9__3) == null)
						{
							selector = (<>9__3 = delegate(Mission m)
							{
								CombatMission combatMission = m as CombatMission;
								if (combatMission == null)
								{
									return 0;
								}
								return combatMission.GetClientKillCount(client);
							});
						}
						return missions.Sum(selector).ToString();
					};
					Func<Mission, int> <>9__5;
					new GUITextBlock(new RectTransform(new Point(this.deathColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
					{
						IEnumerable<Mission> missions = GameMain.GameSession.Missions;
						Func<Mission, int> selector;
						if ((selector = <>9__5) == null)
						{
							selector = (<>9__5 = delegate(Mission m)
							{
								CombatMission combatMission = m as CombatMission;
								if (combatMission == null)
								{
									return 0;
								}
								return combatMission.GetClientDeathCount(client);
							});
						}
						return missions.Sum(selector).ToString();
					};
				}
				this.linkedGUIList.Add(new TabMenu.LinkedGUI(client, frame, new GUITextBlock(new RectTransform(new Point(this.pingColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), client.Ping.ToString(), null, null, Alignment.Center, false, "", null), permissionIcon));
			}
			else
			{
				GUITextBlock characterNameBlock = new GUITextBlock(new RectTransform(new Point(this.characterColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ToolBox.LimitString(characterInfo.Name, GUIStyle.Font, this.characterColumnWidth), new Color?(characterInfo.Job.Prefab.UIColor), null, Alignment.Center, false, "", null);
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode)
				{
					Func<Mission, int> <>9__7;
					new GUITextBlock(new RectTransform(new Point(this.killColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
					{
						IEnumerable<Mission> missions = GameMain.GameSession.Missions;
						Func<Mission, int> selector;
						if ((selector = <>9__7) == null)
						{
							selector = (<>9__7 = delegate(Mission m)
							{
								CombatMission combatMission = m as CombatMission;
								if (combatMission == null)
								{
									return 0;
								}
								return combatMission.GetBotKillCount(characterInfo);
							});
						}
						return missions.Sum(selector).ToString();
					};
					Func<Mission, int> <>9__9;
					new GUITextBlock(new RectTransform(new Point(this.deathColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
					{
						IEnumerable<Mission> missions = GameMain.GameSession.Missions;
						Func<Mission, int> selector;
						if ((selector = <>9__9) == null)
						{
							selector = (<>9__9 = delegate(Mission m)
							{
								CombatMission combatMission = m as CombatMission;
								if (combatMission == null)
								{
									return 0;
								}
								return combatMission.GetBotDeathCount(characterInfo);
							});
						}
						return missions.Sum(selector).ToString();
					};
				}
				if (character is AICharacter)
				{
					this.linkedGUIList.Add(new TabMenu.LinkedGUI(character, frame, new GUITextBlock(new RectTransform(new Point(this.pingColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("tabmenu.bot"), null, null, Alignment.Center, false, "", null)
					{
						ForceUpperCase = ForceUpperCase.Yes
					}));
				}
				else if (characterInfo.IsOnReserveBench)
				{
					new GUIImage(new RectTransform(new Point(this.pingColumnWidth, paddedFrame.Rect.Height - 4), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "CrewManagementReserveBenchIconReserve", true).ToolTip = TextManager.Get("ReserveBenchStatus.Reserve");
				}
				if (characterInfo.IsOnReserveBench)
				{
					GUIFrame guiframe2 = new GUIFrame(new RectTransform(new Point(paddedFrame.Rect.Width - 1, frame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
					{
						AbsoluteOffset = new Point(1, 0)
					}, null, new Color?(Color.Black * 0.7f));
					guiframe2.IgnoreLayoutGroups = true;
					guiframe2.CanBeFocused = false;
				}
			}
			if (character != null)
			{
				this.CreateWalletCrewFrame(character, paddedFrame);
			}
			else if (characterInfo.IsOnReserveBench)
			{
				new GUILayoutGroup(new RectTransform(new Point(this.walletColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.Center).CanBeFocused = false;
			}
			paddedFrame.Recalculate();
		}

		// Token: 0x060016E7 RID: 5863 RVA: 0x000DB634 File Offset: 0x000D9834
		private void CreateMultiPlayerClientElement(Client client)
		{
			int teamIndex = this.GetTeamIndex(client);
			if (teamIndex == -1)
			{
				teamIndex = 0;
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.crewListArray[teamIndex].Content.Rect.Width, GUI.IntScale(33f)), this.crewListArray[teamIndex].Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
			{
				UserData = client,
				Color = Color.Transparent
			};
			GUIFrame guiframe = frame;
			guiframe.OnSecondaryClicked = (GUIComponent.SecondaryButtonDownHandler)Delegate.Combine(guiframe.OnSecondaryClicked, new GUIComponent.SecondaryButtonDownHandler(delegate(GUIComponent component, object data)
			{
				NetLobbyScreen.CreateModerationContextMenu(client);
				return true;
			}));
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Point(this.jobColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				this.DrawClientJobIcon(sb, component.Rect, client);
			}, null);
			guicustomComponent.CanBeFocused = false;
			guicustomComponent.HoverColor = Color.White;
			guicustomComponent.SelectedColor = Color.White;
			GUIImage permissionIcon;
			this.CreateNameWithPermissionIcon(client, paddedFrame, out permissionIcon);
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
			{
				Func<Mission, int> <>9__3;
				new GUITextBlock(new RectTransform(new Point(this.killColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
				{
					IEnumerable<Mission> missions = GameMain.GameSession.Missions;
					Func<Mission, int> selector;
					if ((selector = <>9__3) == null)
					{
						selector = (<>9__3 = delegate(Mission m)
						{
							CombatMission combatMission = m as CombatMission;
							if (combatMission == null)
							{
								return 0;
							}
							return combatMission.GetClientKillCount(client);
						});
					}
					return missions.Sum(selector).ToString();
				};
				Func<Mission, int> <>9__5;
				new GUITextBlock(new RectTransform(new Point(this.deathColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null).TextGetter = delegate()
				{
					IEnumerable<Mission> missions = GameMain.GameSession.Missions;
					Func<Mission, int> selector;
					if ((selector = <>9__5) == null)
					{
						selector = (<>9__5 = delegate(Mission m)
						{
							CombatMission combatMission = m as CombatMission;
							if (combatMission == null)
							{
								return 0;
							}
							return combatMission.GetClientDeathCount(client);
						});
					}
					return missions.Sum(selector).ToString();
				};
			}
			this.linkedGUIList.Add(new TabMenu.LinkedGUI(client, frame, new GUITextBlock(new RectTransform(new Point(this.pingColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), client.Ping.ToString(), null, null, Alignment.Center, false, "", null), permissionIcon));
			this.CreateWalletCrewFrame(client.Character, paddedFrame);
			paddedFrame.Recalculate();
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x000DB944 File Offset: 0x000D9B44
		private int GetTeamIndex(Client client)
		{
			if (this.teamIDs.Count <= 1)
			{
				return 0;
			}
			if (client.Character != null)
			{
				return this.teamIDs.IndexOf(client.Character.TeamID);
			}
			if (client.CharacterID != 0)
			{
				using (IEnumerator<Character> enumerator = this.crew.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character c = enumerator.Current;
						if (client.CharacterID == c.ID)
						{
							return this.teamIDs.IndexOf(c.TeamID);
						}
					}
					goto IL_D5;
				}
			}
			foreach (Character c2 in this.crew)
			{
				if (client.Name == c2.Name)
				{
					return this.teamIDs.IndexOf(c2.TeamID);
				}
			}
			IL_D5:
			return this.teamIDs.IndexOf(client.TeamID);
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x000DBA58 File Offset: 0x000D9C58
		private void CreateWalletCrewFrame(Character character, GUILayoutGroup paddedFrame)
		{
			TabMenu.<>c__DisplayClass61_0 CS$<>8__locals1 = new TabMenu.<>c__DisplayClass61_0();
			CS$<>8__locals1.character = character;
			GameSession gameSession = GameMain.GameSession;
			if (!(((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign))
			{
				return;
			}
			GUILayoutGroup walletLayout = new GUILayoutGroup(new RectTransform(new Point(this.walletColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.Center)
			{
				CanBeFocused = false
			};
			GUILayoutGroup paddedLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 1f), walletLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIFrame guiframe = new GUIFrame(new RectTransform(Vector2.One, paddedLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			guiframe.IgnoreLayoutGroups = true;
			guiframe.ToolTip = TextManager.Get("walletdescription");
			if (CS$<>8__locals1.character == null || CS$<>8__locals1.character.IsBot)
			{
				return;
			}
			Sprite walletSprite = GUIStyle.CrewWalletIconSmall.Value.Sprite;
			CS$<>8__locals1.icon = new GUIImage(new RectTransform(Vector2.One, paddedLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), walletSprite, true, null)
			{
				CanBeFocused = false
			};
			TabMenu.<>c__DisplayClass61_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(Vector2.One, paddedLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = string.Empty;
			GUIFont font = GUIStyle.Font;
			CS$<>8__locals2.walletBlock = new GUITextBlock(rectT, text, null, font, Alignment.Right, false, "", null)
			{
				AutoScaleHorizontal = true,
				Padding = Vector4.Zero,
				CanBeFocused = false
			};
			CS$<>8__locals1.largeIcon = new GUIImage(new RectTransform(Vector2.One, paddedLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), walletSprite, true, null)
			{
				CanBeFocused = false,
				IgnoreLayoutGroups = true,
				Visible = false
			};
			if (CS$<>8__locals1.character.IsBot)
			{
				CS$<>8__locals1.largeIcon.Visible = true;
				CS$<>8__locals1.icon.Visible = false;
				CS$<>8__locals1.walletBlock.Visible = false;
				CS$<>8__locals1.largeIcon.Enabled = false;
				return;
			}
			walletLayout.Recalculate();
			paddedLayoutGroup.Recalculate();
			TabMenu.<CreateWalletCrewFrame>g__SetWalletText|61_0(CS$<>8__locals1.walletBlock, CS$<>8__locals1.character.Wallet, CS$<>8__locals1.icon, CS$<>8__locals1.largeIcon);
			GameSession gameSession2 = GameMain.GameSession;
			MultiPlayerCampaign campaign = ((gameSession2 != null) ? gameSession2.Campaign : null) as MultiPlayerCampaign;
			if (campaign != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted("CreateWalletCrewFrame");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(CS$<>8__locals1.character.ID);
				Identifier eventIdentifier = new Identifier(defaultInterpolatedStringHandler.ToStringAndClear());
				campaign.OnMoneyChanged.RegisterOverwriteExisting(eventIdentifier, delegate(WalletChangedEvent e)
				{
					Character owner;
					if (!e.Owner.TryUnwrap(out owner) || owner != CS$<>8__locals1.character)
					{
						return;
					}
					TabMenu.<CreateWalletCrewFrame>g__SetWalletText|61_0(CS$<>8__locals1.walletBlock, e.Wallet, CS$<>8__locals1.icon, CS$<>8__locals1.largeIcon);
				});
				this.registeredEvents.Add(eventIdentifier);
			}
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x000DBDB0 File Offset: 0x000D9FB0
		private void CreateNameWithPermissionIcon(Client client, GUILayoutGroup paddedFrame, out GUIImage permissionIcon)
		{
			Sprite permissionIconSprite = this.GetPermissionIcon(client);
			Character character = client.Character;
			JobPrefab jobPrefab;
			if (character == null)
			{
				jobPrefab = null;
			}
			else
			{
				CharacterInfo info = character.Info;
				if (info == null)
				{
					jobPrefab = null;
				}
				else
				{
					Job job = info.Job;
					jobPrefab = ((job != null) ? job.Prefab : null);
				}
			}
			JobPrefab prefab = jobPrefab;
			Color nameColor = (prefab != null) ? prefab.UIColor : Color.White;
			Point iconSize = new Point((int)((float)paddedFrame.Rect.Height * 0.8f));
			float characterNameWidthAdjustment = (float)((iconSize.X + paddedFrame.AbsoluteSpacing) / this.characterColumnWidth);
			GUITextBlock characterNameBlock = new GUITextBlock(new RectTransform(new Point(this.characterColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ToolBox.LimitString(client.Name, GUIStyle.Font, (int)((float)this.characterColumnWidth - (float)paddedFrame.Rect.Width * characterNameWidthAdjustment)), new Color?(nameColor), null, Alignment.Center, false, "", null);
			float iconWidth = (float)iconSize.X / (float)this.characterColumnWidth;
			int xOffset = (int)((float)this.jobColumnWidth + characterNameBlock.TextPos.X - GUIStyle.Font.MeasureString(characterNameBlock.Text, false).X / 2f - (float)paddedFrame.AbsoluteSpacing - iconWidth * (float)paddedFrame.Rect.Width);
			permissionIcon = new GUIImage(new RectTransform(new Vector2(iconWidth, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(xOffset + 2, 0)
			}, permissionIconSprite, null, GUIImage.ScalingMode.None)
			{
				IgnoreLayoutGroups = true
			};
			if (client.Character != null && client.Character.IsDead)
			{
				characterNameBlock.Strikethrough = new GUITextBlock.StrikethroughSettings(null, GUI.IntScale(1f), GUI.IntScale(5f));
			}
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x000DBFAE File Offset: 0x000DA1AE
		private Sprite GetPermissionIcon(Client client)
		{
			if (GameMain.NetworkMember == null || client == null || !client.HasPermissions)
			{
				return null;
			}
			if (client.IsOwner)
			{
				return TabMenu.ownerIcon;
			}
			return TabMenu.moderatorIcon;
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x000DBFD8 File Offset: 0x000DA1D8
		private void DrawClientJobIcon(SpriteBatch spriteBatch, Rectangle area, Client client)
		{
			if (client.Spectating)
			{
				TabMenu.spectateIcon.Draw(spriteBatch, area, Color.White, SpriteEffects.None, null);
				return;
			}
			if (client.Character == null || !client.InGame)
			{
				Vector2 stringOffset = GUIStyle.Font.MeasureString("• • •", false) / 2f;
				GUIStyle.Font.DrawString(spriteBatch, "• • •", area.Center.ToVector2() - stringOffset, Color.White, ForceUpperCase.Inherit, false);
				return;
			}
			CharacterInfo info = client.Character.Info;
			if (info == null)
			{
				return;
			}
			info.DrawJobIcon(spriteBatch, area, false);
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x000DC080 File Offset: 0x000DA280
		private void DrawDisconnectedIcon(SpriteBatch spriteBatch, Rectangle area)
		{
			TabMenu.disconnectedIcon.Draw(spriteBatch, area, GUIStyle.Red, SpriteEffects.None, null);
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x000DC0B0 File Offset: 0x000DA2B0
		private bool SelectElement(object userData, GUIComponent crewList)
		{
			Character character = userData as Character;
			Client client = userData as Client;
			GUIComponent existingPreview = this.infoFrameHolder.FindChild("SelectedCharacter", false);
			if (existingPreview != null)
			{
				this.infoFrameHolder.RemoveChild(existingPreview);
			}
			CharacterInfo characterInfo = userData as CharacterInfo;
			if (characterInfo != null && characterInfo.IsOnReserveBench)
			{
				return true;
			}
			GUIFrame background = new GUIFrame(new RectTransform(new Vector2(0.543f, 0.69f), this.infoFrameHolder.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(-0.061f, 0f)
			}, "", null)
			{
				UserData = "SelectedCharacter"
			};
			if (character != null)
			{
				if (GameMain.Client == null)
				{
					GUIComponent preview = character.Info.CreateInfoFrame(background, false, null);
				}
				else
				{
					GUIComponent preview2 = character.Info.CreateInfoFrame(background, false, this.GetPermissionIcon(GameMain.Client.ConnectedClients.Find((Client c) => c.Character == character)));
					GameMain.Client.SelectCrewCharacter(character, preview2);
					if (!character.IsBot)
					{
						GameSession gameSession = GameMain.GameSession;
						MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
						if (mpCampaign != null)
						{
							this.CreateWalletFrame(background, character, mpCampaign);
						}
					}
				}
				GUIButton talentButton = background.FindChild("managebottalentsbutton", true) as GUIButton;
				if (talentButton != null && talentButton.Enabled)
				{
					talentButton.OnClicked = delegate(GUIButton button, object o)
					{
						this.talentMenu.CreateGUI(this.infoFrameHolder, character.Info);
						return true;
					};
				}
			}
			else if (client != null)
			{
				GUIComponent preview3 = this.CreateClientInfoFrame(background, client, this.GetPermissionIcon(client));
				GameClient client2 = GameMain.Client;
				if (client2 != null)
				{
					client2.SelectCrewClient(client, preview3);
				}
				if (client.Character != null)
				{
					GameSession gameSession2 = GameMain.GameSession;
					MultiPlayerCampaign mpCampaign2 = ((gameSession2 != null) ? gameSession2.Campaign : null) as MultiPlayerCampaign;
					if (mpCampaign2 != null)
					{
						this.CreateWalletFrame(background, client.Character, mpCampaign2);
					}
				}
			}
			return true;
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x000DC2C4 File Offset: 0x000DA4C4
		private void CreateWalletFrame(GUIComponent parent, Character character, MultiPlayerCampaign campaign)
		{
			TabMenu.<>c__DisplayClass67_0 CS$<>8__locals1 = new TabMenu.<>c__DisplayClass67_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.campaign = campaign;
			if (CS$<>8__locals1.campaign == null)
			{
				throw new ArgumentNullException("campaign", "Tried to create a wallet frame when campaign was null");
			}
			if (CS$<>8__locals1.character == null)
			{
				throw new ArgumentNullException("character", "Tried to create a wallet frame for a null character");
			}
			this.isTransferMenuOpen = false;
			this.transferMenuOpenState = 1f;
			CS$<>8__locals1.salaryCrew = (from c in GameSession.GetSessionCrewCharacters(CharacterType.Player)
			where c != CS$<>8__locals1.character
			select c).ToImmutableHashSet<Character>();
			CS$<>8__locals1.targetWallet = CS$<>8__locals1.character.Wallet;
			GUIFrame walletFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.35f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 1.02f)
			}, "", null);
			GUILayoutGroup walletLayout = new GUILayoutGroup(new RectTransform(ToolBox.PaddingSizeParentRelative(walletFrame.RectTransform, 0.9f), walletFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup headerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.33f), walletLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIImage icon = new GUIImage(new RectTransform(Vector2.One, headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "CrewWalletIconLarge", true);
			float relativeX = (float)icon.RectTransform.NonScaledSize.X / (float)icon.Parent.RectTransform.NonScaledSize.X;
			GUILayoutGroup headerTextLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f - relativeX, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 1f), headerTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("crewwallet.wallet");
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			GUIFrame guiframe = new GUIFrame(new RectTransform(Vector2.One, headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			guiframe.IgnoreLayoutGroups = true;
			guiframe.ToolTip = TextManager.Get("walletdescription");
			TabMenu.<>c__DisplayClass67_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(new Vector2(0.5f, 1f), headerTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.FormatCurrency(CS$<>8__locals1.targetWallet.Balance, true);
			font = GUIStyle.SubHeadingFont;
			CS$<>8__locals2.moneyBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.Right, false, "", null);
			GUILayoutGroup middleLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.66f), walletLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup salaryTextLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), middleLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			RectTransform rectT3 = new RectTransform(new Vector2(0.5f, 1f), salaryTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("crewwallet.salary");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock salaryTitle = new GUITextBlock(rectT3, text3, null, font, Alignment.BottomLeft, false, "", null);
			CS$<>8__locals1.rewardBlock = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), salaryTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.BottomRight, false, "", null);
			GUIFrame guiframe2 = new GUIFrame(new RectTransform(Vector2.One, middleLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			guiframe2.IgnoreLayoutGroups = true;
			guiframe2.ToolTip = TextManager.Get("crewwallet.salary.tooltip");
			GUILayoutGroup sliderLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), middleLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.Center);
			CS$<>8__locals1.salarySlider = new GUIScrollBar(new RectTransform(new Vector2(0.9f, 1f), sliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.03f, null, "GUISlider", null)
			{
				Range = new Vector2(0f, 1f),
				BarScrollValue = (float)CS$<>8__locals1.targetWallet.RewardDistribution / 100f,
				Step = 0.01f,
				BarSize = 0.1f,
				OnMoved = delegate(GUIScrollBar bar, float scroll)
				{
					int rewardDistribution = TabMenu.RoundRewardDistribution(scroll, bar.Step);
					base.<CreateWalletFrame>g__SetRewardText|3(rewardDistribution, CS$<>8__locals1.rewardBlock);
					return true;
				},
				OnReleased = delegate(GUIScrollBar bar, float scroll)
				{
					int newRewardDistribution = TabMenu.RoundRewardDistribution(scroll, bar.Step);
					if (newRewardDistribution == CS$<>8__locals1.targetWallet.RewardDistribution)
					{
						return false;
					}
					TabMenu.SetRewardDistribution(Option.Some<Character>(CS$<>8__locals1.character), newRewardDistribution);
					return true;
				}
			};
			CS$<>8__locals1.<CreateWalletFrame>g__SetRewardText|3(CS$<>8__locals1.targetWallet.RewardDistribution, CS$<>8__locals1.rewardBlock);
			GUIScissorComponent scissorComponent = new GUIScissorComponent(new RectTransform(new Vector2(0.85f, 1.25f), walletFrame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal))
			{
				CanBeFocused = false
			};
			this.transferMenu = new GUIFrame(new RectTransform(Vector2.One, scissorComponent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup transferMenuLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.8f), this.transferMenu.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUILayoutGroup paddedTransferMenuLayout = new GUILayoutGroup(new RectTransform(ToolBox.PaddingSizeParentRelative(transferMenuLayout.RectTransform, 0.85f), transferMenuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), paddedTransferMenuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUILayoutGroup leftLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.5f), leftLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = CS$<>8__locals1.character.Name;
			font = GUIStyle.SubHeadingFont;
			GUITextBlock leftName = new GUITextBlock(rectT4, text4, null, font, Alignment.CenterLeft, false, "", null);
			CS$<>8__locals1.leftBalance = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), leftLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.FormatCurrency(CS$<>8__locals1.targetWallet.Balance, true), null, null, Alignment.Left, false, "", null)
			{
				TextColor = GUIStyle.Blue
			};
			CS$<>8__locals1.rightLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopRight);
			TabMenu.<>c__DisplayClass67_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0.5f), CS$<>8__locals1.rightLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			CS$<>8__locals3.rightName = new GUITextBlock(rectT5, text5, null, font, Alignment.CenterRight, false, "", null);
			CS$<>8__locals1.rightBalance = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), CS$<>8__locals1.rightLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Right, false, "", null)
			{
				TextColor = GUIStyle.Red
			};
			GUILayoutGroup centerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), mainLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.Center)
			{
				IgnoreLayoutGroups = true
			};
			new GUIFrame(new RectTransform(new Vector2(0f, 1f), centerLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "VerticalLine", null).IgnoreLayoutGroups = true;
			CS$<>8__locals1.centerButton = new GUIButton(new RectTransform(new Vector2(1f), centerLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIButtonTransferArrow", null);
			GUILayoutGroup inputLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), paddedTransferMenuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			CS$<>8__locals1.transferAmountInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), inputLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.ForceHidden, null)
			{
				MinValueInt = new int?(0)
			};
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), paddedTransferMenuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUILayoutGroup centerButtonLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			CS$<>8__locals1.resetButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), centerButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("reset"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				Enabled = false
			};
			CS$<>8__locals1.confirmButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), centerButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("confirm"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				Enabled = false
			};
			CS$<>8__locals1.layoutGroups = ImmutableArray.Create<GUILayoutGroup>(new GUILayoutGroup[]
			{
				transferMenuLayout,
				paddedTransferMenuLayout,
				mainLayout,
				leftLayout,
				CS$<>8__locals1.rightLayout
			});
			MedicalClinicUI.EnsureTextDoesntOverflow(CS$<>8__locals1.character.Name, leftName, leftLayout.Rect, new ImmutableArray<GUILayoutGroup>?(CS$<>8__locals1.layoutGroups));
			this.transferMenuButton = new GUIButton(new RectTransform(new Vector2(0.5f, 0.2f), walletFrame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), Alignment.Center, "UIToggleButtonVertical", null)
			{
				ToolTip = TextManager.Get("crewwallet.transfer.tooltip"),
				OnClicked = delegate(GUIButton button, object o)
				{
					CS$<>8__locals1.<>4__this.isTransferMenuOpen = !CS$<>8__locals1.<>4__this.isTransferMenuOpen;
					if (!CS$<>8__locals1.<>4__this.isTransferMenuOpen)
					{
						CS$<>8__locals1.transferAmountInput.IntValue = 0;
					}
					TabMenu.<CreateWalletFrame>g__ToggleTransferMenuIcon|67_4(button, CS$<>8__locals1.<>4__this.isTransferMenuOpen);
					return true;
				}
			};
			CS$<>8__locals1.eventIdentifier = "CreateWalletFrame".ToIdentifier();
			TabMenu.<CreateWalletFrame>g__ToggleTransferMenuIcon|67_4(this.transferMenuButton, this.isTransferMenuOpen);
			TabMenu.<CreateWalletFrame>g__ToggleCenterButton|67_5(CS$<>8__locals1.centerButton, this.isSending);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.OnPermissionChanged.RegisterOverwriteExisting(CS$<>8__locals1.eventIdentifier, delegate(GameClient.PermissionChangedEvent e)
				{
					base.<CreateWalletFrame>g__UpdateWalletInterface|2(false);
				});
			}
			CS$<>8__locals1.<CreateWalletFrame>g__UpdateWalletInterface|2(true);
		}

		// Token: 0x060016F0 RID: 5872 RVA: 0x000DD154 File Offset: 0x000DB354
		private static void SetRewardDistribution(Option<Character> character, int newValue)
		{
			NetWalletSetSalaryUpdate netWalletSetSalaryUpdate = default(NetWalletSetSalaryUpdate);
			netWalletSetSalaryUpdate.Target = from c in character
			select c.ID;
			netWalletSetSalaryUpdate.NewRewardDistribution = newValue;
			INetSerializableStruct transfer = netWalletSetSalaryUpdate;
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.REWARD_DISTRIBUTION);
			transfer.Write(msg);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x060016F1 RID: 5873 RVA: 0x000DD1D8 File Offset: 0x000DB3D8
		private static void ResetRewardDistributions()
		{
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.RESET_REWARD_DISTRIBUTION);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x060016F2 RID: 5874 RVA: 0x000DD20E File Offset: 0x000DB40E
		private static int RoundRewardDistribution(float scroll, float step)
		{
			return (int)MathUtils.RoundTowardsClosest(scroll * 100f, step * 100f);
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x000DD224 File Offset: 0x000DB424
		private GUIComponent CreateClientInfoFrame(GUIFrame frame, Client client, Sprite permissionIcon = null)
		{
			Character character = client.Character;
			GUIComponent paddedFrame;
			if (((character != null) ? character.Info : null) == null)
			{
				paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.874f, 0.58f), frame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, 0.05f)
				}, false, Anchor.TopLeft)
				{
					RelativeSpacing = 0.05f
				};
				GUILayoutGroup headerArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.322f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				new GUICustomComponent(new RectTransform(new Vector2(0.425f, 1f), headerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
				{
					this.DrawClientJobIcon(sb, component.Rect, client);
				}, null);
				GUIFont font = (paddedFrame.Rect.Width < 280) ? GUIStyle.SmallFont : GUIStyle.Font;
				GUILayoutGroup headerTextArea = new GUILayoutGroup(new RectTransform(new Vector2(0.575f, 1f), headerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					RelativeSpacing = 0.02f,
					Stretch = true
				};
				GUITextBlock clientNameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ToolBox.LimitString(client.Name, GUIStyle.Font, headerTextArea.Rect.Width), new Color?(Color.White), GUIStyle.Font, Alignment.Left, false, "", null)
				{
					ForceUpperCase = ForceUpperCase.Yes,
					Padding = Vector4.Zero
				};
				if (permissionIcon != null)
				{
					Point iconSize = permissionIcon.SourceRect.Size;
					int iconWidth = (int)((float)clientNameBlock.Rect.Height / (float)iconSize.Y * (float)iconSize.X);
					new GUIImage(new RectTransform(new Point(iconWidth, clientNameBlock.Rect.Height), clientNameBlock.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
					{
						AbsoluteOffset = new Point(-iconWidth - 2, 0)
					}, permissionIcon, null, GUIImage.ScalingMode.None).IgnoreLayoutGroups = true;
				}
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), headerTextArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), client.Spectating ? TextManager.Get("playingasspectator") : TextManager.Get("tabmenu.inlobby"), new Color?(Color.White), font, Alignment.Left, true, "", null).Padding = Vector4.Zero;
			}
			else
			{
				paddedFrame = client.Character.Info.CreateInfoFrame(frame, false, permissionIcon);
			}
			return paddedFrame;
		}

		// Token: 0x060016F4 RID: 5876 RVA: 0x000DD598 File Offset: 0x000DB798
		private void CreateMultiPlayerLogContent(GUIFrame crewFrame)
		{
			GUIFrame logContainer = new GUIFrame(new RectTransform(new Vector2(0.543f, 0.717f), this.infoFrameHolder.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(-0.145f, 0f)
			}, "", null);
			GUIFrame innerFrame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), logContainer.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.0475f)
			}, null, null);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(Vector2.One, innerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.logList = new GUIListBox(new RectTransform(Vector2.One, content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Padding = new Vector4(0f, 10f * GUI.Scale, 0f, 10f * GUI.Scale),
				UserData = crewFrame,
				AutoHideScrollBar = false,
				Spacing = (int)(5f * GUI.Scale)
			};
			foreach (ValueTuple<string, PlayerConnectionChangeType> valueTuple in TabMenu.storedMessages)
			{
				string message = valueTuple.Item1;
				PlayerConnectionChangeType type = valueTuple.Item2;
				this.AddLineToLog(message, type);
			}
			this.logList.BarScroll = 1f;
		}

		// Token: 0x060016F5 RID: 5877 RVA: 0x000DD79C File Offset: 0x000DB99C
		public static void StorePlayerConnectionChangeMessage(ChatMessage message)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null || !gameSession.IsRunning)
			{
				return;
			}
			string msg = ChatMessage.GetTimeStamp() + message.TextWithSender;
			TabMenu.storedMessages.Add(new ValueTuple<string, PlayerConnectionChangeType>(msg, message.ChangeType));
			if (GameSession.IsTabMenuOpen && TabMenu.SelectedTab == TabMenu.InfoFrameTab.Crew)
			{
				TabMenu instance = GameSession.TabMenuInstance;
				instance.AddLineToLog(msg, message.ChangeType);
				instance.RemoveCurrentElements();
				instance.CreateMultiPlayerList(true);
			}
		}

		// Token: 0x060016F6 RID: 5878 RVA: 0x000DD818 File Offset: 0x000DBA18
		private void RemoveCurrentElements()
		{
			for (int i = 0; i < this.crewListArray.Length; i++)
			{
				for (int j = 0; j < this.linkedGUIList.Count; j++)
				{
					this.linkedGUIList[j].Remove(this.crewListArray[i].Content);
				}
			}
			this.linkedGUIList.Clear();
			foreach (GUIListBox crewList in this.crewListArray)
			{
				crewList.Content.ClearChildren();
			}
		}

		// Token: 0x060016F7 RID: 5879 RVA: 0x000DD8A0 File Offset: 0x000DBAA0
		private void AddLineToLog(string line, PlayerConnectionChangeType type)
		{
			Color textColor = Color.White;
			switch (type)
			{
			case PlayerConnectionChangeType.Joined:
				textColor = GUIStyle.Green;
				break;
			case PlayerConnectionChangeType.Kicked:
				textColor = GUIStyle.Orange;
				break;
			case PlayerConnectionChangeType.Disconnected:
				textColor = GUIStyle.Yellow;
				break;
			case PlayerConnectionChangeType.Banned:
				textColor = GUIStyle.Red;
				break;
			}
			if (this.logList != null)
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), this.logList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = RichString.Rich(line, null);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, true, "", null)
				{
					TextColor = textColor,
					CanBeFocused = false,
					UserData = line
				};
				textBlock.CalculateHeightFromText(0, false);
				if (textBlock.HasColorHighlight)
				{
					foreach (RichTextData data in textBlock.RichTextData.Value)
					{
						textBlock.ClickableAreas.Add(new GUITextBlock.ClickableArea
						{
							Data = data,
							OnClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.SelectPlayer),
							OnSecondaryClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.ShowPlayerContextMenu)
						});
					}
				}
			}
		}

		// Token: 0x060016F8 RID: 5880 RVA: 0x000DDA20 File Offset: 0x000DBC20
		private void CreateMissionInfo(GUIFrame infoFrame)
		{
			Level loaded = Level.Loaded;
			if (((loaded != null) ? loaded.LevelData : null) == null)
			{
				DebugConsole.ThrowError("Failed to display mission info in the tab menu (no level loaded).\n" + Environment.StackTrace, null, null, false, false);
				return;
			}
			infoFrame.ClearChildren();
			GUIFrame missionFrame = new GUIFrame(new RectTransform(Vector2.One, infoFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
			int padding = (int)(0.0245f * (float)missionFrame.Rect.Height);
			GUIFrame missionFrameContent = new GUIFrame(new RectTransform(new Point(missionFrame.Rect.Width - padding * 2, missionFrame.Rect.Height - padding * 2), infoFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null);
			Location location = GameMain.GameSession.StartLocation;
			if (Level.Loaded.Type == LevelData.LevelType.LocationConnection && location == null)
			{
				location = GameMain.GameSession.EndLocation;
			}
			GUILayoutGroup locationInfoContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.3f), missionFrameContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(10f)
			};
			Sprite portrait = location.Type.GetPortrait(location.PortraitId);
			bool hasPortrait = portrait != null && portrait.SourceRect.Width > 0 && portrait.SourceRect.Height > 0;
			int contentWidth = missionFrameContent.Rect.Width;
			if (hasPortrait)
			{
				float portraitAspectRatio = (float)(portrait.SourceRect.Width / portrait.SourceRect.Height);
				GUIImage portraitImage = new GUIImage(new RectTransform(new Vector2(0.45f, 1f), locationInfoContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), portrait, null, GUIImage.ScalingMode.ScaleToFitLargestExtent)
				{
					IgnoreLayoutGroups = true
				};
				locationInfoContainer.Recalculate();
				portraitImage.RectTransform.NonScaledSize = new Point(Math.Min((int)((float)portraitImage.Rect.Size.Y * portraitAspectRatio), portraitImage.Rect.Width), portraitImage.Rect.Size.Y);
			}
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), locationInfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = location.DisplayName;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), locationInfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = location.GetLocationTypeToDisplay().Name;
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			Faction faction = location.Faction;
			if (((faction != null) ? faction.Prefab : null) != null)
			{
				RectTransform rectT3 = new RectTransform(new Vector2(0.5f, 0f), locationInfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = TextManager.Get("Faction");
				font = GUIStyle.SubHeadingFont;
				GUITextBlock factionLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.CenterLeft, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, 1f), factionLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), location.Faction.Prefab.Name, null, null, Alignment.CenterRight, false, "", null);
			}
			RectTransform rectT4 = new RectTransform(new Vector2(0.5f, 0f), locationInfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get(new string[]
			{
				"Biome",
				"location"
			});
			font = GUIStyle.SubHeadingFont;
			GUITextBlock biomeLabel = new GUITextBlock(rectT4, text4, null, font, Alignment.CenterLeft, false, "", null);
			new GUITextBlock(new RectTransform(new Vector2(1f, 1f), biomeLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Level.Loaded.LevelData.Biome.DisplayName, null, null, Alignment.CenterRight, false, "", null);
			RectTransform rectT5 = new RectTransform(new Vector2(0.5f, 0f), locationInfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("LevelDifficulty");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock difficultyLabel = new GUITextBlock(rectT5, text5, null, font, Alignment.CenterLeft, false, "", null);
			new GUITextBlock(new RectTransform(new Vector2(1f, 1f), difficultyLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("percentageformat", "[value]", ((int)Level.Loaded.LevelData.Difficulty).ToString(), FormatCapitals.No), null, null, Alignment.CenterRight, false, "", null);
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), missionFrameContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(0, locationInfoContainer.Rect.Height + padding)
			}, "HorizontalLine", null).CanBeFocused = false;
			int locationInfoYOffset = locationInfoContainer.Rect.Height + padding * 2;
			GUIListBox missionList = new GUIListBox(new RectTransform(new Point(contentWidth, missionFrameContent.Rect.Height - locationInfoYOffset), missionFrameContent.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(0, locationInfoYOffset)
			}, false, null, "", true, false);
			missionList.ContentBackground.Color = Color.Transparent;
			missionList.Spacing = GUI.IntScale(15f);
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Missions : null) != null)
			{
				using (IEnumerator<Mission> enumerator = GameMain.GameSession.Missions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TabMenu.<>c__DisplayClass77_0 CS$<>8__locals1 = new TabMenu.<>c__DisplayClass77_0();
						CS$<>8__locals1.mission = enumerator.Current;
						if (CS$<>8__locals1.mission.Prefab.ShowInMenus)
						{
							List<LocalizedString> textContent = new List<LocalizedString>
							{
								CS$<>8__locals1.mission.GetMissionRewardText(Submarine.MainSub),
								CS$<>8__locals1.mission.GetReputationRewardText(),
								CS$<>8__locals1.mission.Description
							};
							textContent.AddRange(CS$<>8__locals1.mission.ShownMessages);
							RoundSummary.CreateMissionEntry(missionList.Content, CS$<>8__locals1.mission.Name, textContent, CS$<>8__locals1.mission.Difficulty.GetValueOrDefault(), CS$<>8__locals1.mission.Prefab.Icon, CS$<>8__locals1.mission.Prefab.IconColor, CS$<>8__locals1.mission.GetDifficultyToolTipText(), out CS$<>8__locals1.missionIcon);
							if (CS$<>8__locals1.missionIcon != null)
							{
								CS$<>8__locals1.<CreateMissionInfo>g__UpdateMissionStateIcon|0();
								Mission mission2 = CS$<>8__locals1.mission;
								mission2.OnMissionStateChanged = (Action<Mission>)Delegate.Combine(mission2.OnMissionStateChanged, new Action<Mission>(delegate(Mission mission)
								{
									base.<CreateMissionInfo>g__UpdateMissionStateIcon|0();
								}));
							}
						}
					}
					goto IL_971;
				}
			}
			GUILayoutGroup missionTextGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), missionList.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT6 = new RectTransform(new Vector2(1f, 0f), missionTextGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("NoMission");
			font = GUIStyle.LargeFont;
			new GUITextBlock(rectT6, text6, null, font, Alignment.Left, false, "", null);
			IL_971:
			GameSession gameSession2 = GameMain.GameSession;
			if (gameSession2 != null)
			{
				EventManager eventManager = gameSession2.EventManager;
				if (eventManager != null)
				{
					EventLog eventLog = eventManager.EventLog;
					if (eventLog != null)
					{
						eventLog.CreateEventLogUI(missionList.Content, null);
					}
				}
			}
			GameMain.GameSession.EnableEventLogNotificationIcon(false);
			RoundSummary.AddSeparators(missionList.Content);
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x000DE408 File Offset: 0x000DC608
		private static void CreateSubmarineInfo(GUIFrame infoFrame, Submarine sub)
		{
			if (sub == null)
			{
				return;
			}
			GUIFrame subInfoFrame = new GUIFrame(new RectTransform(Vector2.One, infoFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(Vector2.One * 0.97f, subInfoFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton previewButton = new GUIButton(new RectTransform(new Vector2(1f, 0.43f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				OnClicked = delegate(GUIButton btn, object obj)
				{
					SubmarinePreview.Create(sub.Info);
					return false;
				}
			};
			Sprite sprite;
			if ((sprite = sub.Info.PreviewImage) == null)
			{
				SubmarineInfo submarineInfo = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name.Equals(sub.Info.Name, StringComparison.OrdinalIgnoreCase));
				sprite = ((submarineInfo != null) ? submarineInfo.PreviewImage : null);
			}
			Sprite previewImage = sprite;
			if (previewImage == null)
			{
				new GUITextBlock(new RectTransform(Vector2.One, previewButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SubPreviewImageNotFound"), null, null, Alignment.Left, false, "", null);
			}
			else
			{
				GUIFrame submarinePreviewBackground = new GUIFrame(new RectTransform(Vector2.One, previewButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					Color = Color.Black,
					HoverColor = Color.Black,
					SelectedColor = Color.Black,
					PressedColor = Color.Black,
					CanBeFocused = false
				};
				new GUIImage(new RectTransform(new Vector2(0.98f), submarinePreviewBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), previewImage, true, null).CanBeFocused = false;
				new GUIFrame(new RectTransform(Vector2.One, submarinePreviewBackground.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerGlow", new Color?(Color.Black)).CanBeFocused = false;
			}
			GUIFrame guiframe = new GUIFrame(new RectTransform(Vector2.One * 0.12f, previewButton.RectTransform, Anchor.BottomRight, new Pivot?(Pivot.BottomRight), null, null, ScaleBasis.BothHeight)
			{
				AbsoluteOffset = new Point((int)(0.03f * (float)previewButton.Rect.Height))
			}, "ExpandButton", new Color?(Color.White));
			guiframe.Color = Color.White;
			guiframe.HoverColor = Color.White;
			guiframe.PressedColor = Color.White;
			GUILayoutGroup subInfoTextLayout = new GUILayoutGroup(new RectTransform(Vector2.One, paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			LocalizedString localizedString;
			if (sub.Info.HasTag(SubmarineTag.Shuttle))
			{
				localizedString = TextManager.Get("shuttle");
			}
			else
			{
				string tag = "submarine.classandtier";
				ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
				int num = 0;
				string item = "[class]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("submarineclass.");
				defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(sub.Info.SubmarineClass);
				array[num] = new ValueTuple<string, LocalizedString>(item, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
				int num2 = 1;
				string item2 = "[tier]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("submarinetier.");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(sub.Info.Tier);
				array[num2] = new ValueTuple<string, LocalizedString>(item2, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
				localizedString = TextManager.GetWithVariables(tag, array);
			}
			LocalizedString className = localizedString;
			int nameHeight = (int)GUIStyle.LargeFont.MeasureString(sub.Info.DisplayName, true).Y;
			int classHeight = (int)GUIStyle.SubHeadingFont.MeasureString(className, false).Y;
			RectTransform rectT = new RectTransform(new Point(subInfoTextLayout.Rect.Width, nameHeight + HUDLayoutSettings.Padding / 2), subInfoTextLayout.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text = sub.Info.DisplayName;
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock submarineNameText = new GUITextBlock(rectT, text, null, font, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			submarineNameText.RectTransform.MinSize = new Point(0, (int)submarineNameText.TextSize.Y);
			RectTransform rectT2 = new RectTransform(new Point(subInfoTextLayout.Rect.Width, classHeight), subInfoTextLayout.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text2 = className;
			font = GUIStyle.SubHeadingFont;
			GUITextBlock submarineClassText = new GUITextBlock(rectT2, text2, null, font, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			submarineClassText.RectTransform.MinSize = new Point(0, (int)submarineClassText.TextSize.Y);
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			CampaignMode campaign = gameMode as CampaignMode;
			if (campaign != null)
			{
				GUILayoutGroup headerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.09f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, 0.43f)
				}, true, Anchor.TopLeft)
				{
					Stretch = true
				};
				GUIImage headerIcon = new GUIImage(new RectTransform(Vector2.One, headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "SubmarineIcon", GUIImage.ScalingMode.None);
				RectTransform rectT3 = new RectTransform(Vector2.One, headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = TextManager.Get("uicategory.upgrades");
				font = GUIStyle.LargeFont;
				new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
				GUILayoutGroup upgradeRootLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.48f), paddedFrame.RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.BottomLeft), null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				GUIListBox upgradeCategoryPanel = UpgradeStore.CreateUpgradeCategoryList(new RectTransform(new Vector2(0.4f, 1f), upgradeRootLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal));
				upgradeCategoryPanel.HideChildrenOutsideFrame = true;
				UpgradeStore.UpdateCategoryList(upgradeCategoryPanel, campaign, sub, UpgradeStore.GetApplicableCategories(sub).ToArray<UpgradeCategory>());
				GUIComponent[] toRemove = upgradeCategoryPanel.Content.FindChildren((GUIComponent c) => !c.Enabled).ToArray<GUIComponent>();
				toRemove.ForEach(delegate(GUIComponent c)
				{
					upgradeCategoryPanel.RemoveChild(c);
				});
				GUIListBox upgradePanel = new GUIListBox(new RectTransform(new Vector2(0.6f, 1f), upgradeRootLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
				upgradeCategoryPanel.OnSelected = delegate(GUIComponent component, object userData)
				{
					upgradePanel.ClearChildren();
					if (userData is UpgradeStore.CategoryData)
					{
						UpgradeStore.CategoryData categoryData = (UpgradeStore.CategoryData)userData;
						if (Submarine.MainSub != null)
						{
							foreach (UpgradePrefab prefab in categoryData.Prefabs)
							{
								GUIFrame frame = UpgradeStore.CreateUpgradeFrame(prefab, categoryData.Category, campaign, new RectTransform(new Vector2(1f, 0.3f), upgradePanel.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false).Frame;
								UpgradeStore.UpdateUpgradeEntry(frame, prefab, categoryData.Category, campaign);
							}
						}
					}
					return true;
				};
				return;
			}
			GUIListBox specsListBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.57f), paddedFrame.RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.BottomLeft), null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				CurrentSelectMode = GUIListBox.SelectMode.None
			};
			sub.Info.CreateSpecsWindow(specsListBox, GUIStyle.Font, false, false, true, false);
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x000DECE8 File Offset: 0x000DCEE8
		public static void CreateSkillList(Character character, CharacterInfo info, GUIListBox parent)
		{
			parent.Content.ClearChildren();
			List<GUITextBlock> skillNames = new List<GUITextBlock>();
			foreach (Skill skill in from s in info.Job.GetSkills()
			orderby s.Level descending
			select s)
			{
				GUILayoutGroup skillContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					CanBeFocused = true
				};
				RectTransform rectT = new RectTransform(new Vector2(0.7f, 0f), skillContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("skillname.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(skill.Identifier);
				GUITextBlock skillName = new GUITextBlock(rectT, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(skill.Identifier.Value, true), null, null, Alignment.Left, false, "", null);
				skillNames.Add(skillName);
				skillName.RectTransform.MinSize = new Point(0, skillName.Rect.Height);
				skillContainer.RectTransform.MinSize = new Point(0, skillName.Rect.Height);
				new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), skillContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Math.Floor((double)skill.Level).ToString("F0"), null, null, Alignment.TopRight, false, "", null);
				float modifiedSkillLevel = MathF.Floor((character != null) ? character.GetSkillLevel(skill.Identifier) : skill.Level);
				if (!MathUtils.NearlyEqual(MathF.Floor(modifiedSkillLevel), MathF.Floor(skill.Level), 0.0001f))
				{
					int skillChange = (int)MathF.Floor(modifiedSkillLevel - MathF.Floor(skill.Level));
					string text;
					if (skillChange <= 0)
					{
						if (skillChange >= 0)
						{
							text = GUIStyle.TextColorNormal.ToStringHex();
						}
						else
						{
							text = GUIStyle.Red.ToStringHex();
						}
					}
					else
					{
						text = GUIStyle.Green.ToStringHex();
					}
					string stringColor = text;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("(‖color:");
					defaultInterpolatedStringHandler2.AppendFormatted(stringColor);
					defaultInterpolatedStringHandler2.AppendLiteral("‖");
					defaultInterpolatedStringHandler2.AppendFormatted(((skillChange > 0) ? "+" : string.Empty) + skillChange.ToString());
					defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖)");
					RichString changeText = RichString.Rich(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
					new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), skillContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), changeText, null, null, Alignment.Left, false, "", null).Padding = Vector4.Zero;
				}
				skillContainer.Recalculate();
			}
			parent.RecalculateChildren();
			GUITextBlock.AutoScaleAndNormalize(skillNames, true, false, null);
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x000DF0C0 File Offset: 0x000DD2C0
		public void OnExperienceChanged(Character character)
		{
			if (character != Character.Controlled)
			{
				return;
			}
			this.talentMenu.UpdateTalentInfo();
		}

		// Token: 0x060016FC RID: 5884 RVA: 0x000DF0D8 File Offset: 0x000DD2D8
		public void OnClose()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign == null)
			{
				return;
			}
			foreach (Identifier identifier in this.registeredEvents)
			{
				campaign.OnMoneyChanged.TryDeregister(identifier);
			}
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x000DF17A File Offset: 0x000DD37A
		[CompilerGenerated]
		internal static void <CreateInfoFrame>g__SetBalanceText|38_3(GUITextBlock text, int balance)
		{
			text.Text = TextManager.GetWithVariable("bankbalanceformat", "[money]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", balance), FormatCapitals.No);
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x000DF1B4 File Offset: 0x000DD3B4
		[CompilerGenerated]
		internal static LocalizedString <CreateInfoFrame>g__ValueToPercentage|38_4(float value)
		{
			string tag = "percentageformat";
			string varName = "[value]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round(value));
			return TextManager.GetWithVariable(tag, varName, defaultInterpolatedStringHandler.ToStringAndClear(), FormatCapitals.No);
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x000DF1F4 File Offset: 0x000DD3F4
		[CompilerGenerated]
		internal static void <CreateWalletCrewFrame>g__SetWalletText|61_0(GUITextBlock block, Wallet wallet, GUIImage icon, GUIImage largeIcon)
		{
			block.Text = TextManager.FormatCurrency(wallet.Balance, true);
			block.ToolTip = string.Empty;
			if (wallet.Balance >= 1000000)
			{
				block.Text = TextManager.Get("crewwallet.balance.toomuchtoshow");
				block.ToolTip = block.Text;
			}
			largeIcon.Visible = false;
			icon.Visible = true;
			block.Visible = true;
			if (50 > block.Rect.Width)
			{
				largeIcon.Visible = true;
				icon.Visible = false;
				block.Visible = false;
				largeIcon.ToolTip = block.Text;
			}
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x000DF29C File Offset: 0x000DD49C
		[CompilerGenerated]
		internal static void <CreateWalletFrame>g__ToggleTransferMenuIcon|67_4(GUIButton btn, bool open)
		{
			foreach (GUIComponent child in btn.Children)
			{
				child.SpriteEffects = (open ? SpriteEffects.None : SpriteEffects.FlipVertically);
			}
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x000DF2F0 File Offset: 0x000DD4F0
		[CompilerGenerated]
		internal static void <CreateWalletFrame>g__ToggleCenterButton|67_5(GUIButton btn, bool isSending)
		{
			foreach (GUIComponent child in btn.Children)
			{
				child.SpriteEffects = (isSending ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
			}
		}

		// Token: 0x06001703 RID: 5891 RVA: 0x000DF344 File Offset: 0x000DD544
		[CompilerGenerated]
		internal static void <CreateWalletFrame>g__SendTransaction|67_6(Option<Character> to, Option<Character> from, int amount)
		{
			NetWalletTransfer netWalletTransfer = default(NetWalletTransfer);
			netWalletTransfer.Sender = from option in @from
			select option.ID;
			netWalletTransfer.Receiver = from option in to
			select option.ID;
			netWalletTransfer.Amount = amount;
			INetSerializableStruct transfer = netWalletTransfer;
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.TRANSFER_MONEY);
			transfer.Write(msg);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x04000B8F RID: 2959
		public static bool PendingChanges = false;

		// Token: 0x04000B90 RID: 2960
		private static bool initialized = false;

		// Token: 0x04000B91 RID: 2961
		private static UISprite spectateIcon;

		// Token: 0x04000B92 RID: 2962
		private static UISprite disconnectedIcon;

		// Token: 0x04000B93 RID: 2963
		private static Sprite ownerIcon;

		// Token: 0x04000B94 RID: 2964
		private static Sprite moderatorIcon;

		// Token: 0x04000B96 RID: 2966
		private GUIFrame infoFrame;

		// Token: 0x04000B97 RID: 2967
		private GUIFrame contentFrame;

		// Token: 0x04000B98 RID: 2968
		private readonly List<GUIButton> tabButtons = new List<GUIButton>();

		// Token: 0x04000B99 RID: 2969
		private GUIFrame infoFrameHolder;

		// Token: 0x04000B9A RID: 2970
		private List<TabMenu.LinkedGUI> linkedGUIList;

		// Token: 0x04000B9B RID: 2971
		private GUIListBox logList;

		// Token: 0x04000B9C RID: 2972
		private GUIListBox[] crewListArray;

		// Token: 0x04000B9D RID: 2973
		private float sizeMultiplier = 1f;

		// Token: 0x04000B9E RID: 2974
		private IEnumerable<Character> crew;

		// Token: 0x04000B9F RID: 2975
		private List<CharacterTeamType> teamIDs;

		// Token: 0x04000BA0 RID: 2976
		private const string inLobbyString = "• • •";

		// Token: 0x04000BA1 RID: 2977
		public static GUIFrame PendingChangesFrame = null;

		// Token: 0x04000BA2 RID: 2978
		public static Color OwnCharacterBGColor = Color.Gold * 0.7f;

		// Token: 0x04000BA3 RID: 2979
		private bool isTransferMenuOpen;

		// Token: 0x04000BA4 RID: 2980
		private bool isSending;

		// Token: 0x04000BA5 RID: 2981
		private GUIComponent transferMenu;

		// Token: 0x04000BA6 RID: 2982
		private GUIButton transferMenuButton;

		// Token: 0x04000BA7 RID: 2983
		private float transferMenuOpenState;

		// Token: 0x04000BA8 RID: 2984
		private bool transferMenuStateCompleted;

		// Token: 0x04000BA9 RID: 2985
		private readonly HashSet<Identifier> registeredEvents = new HashSet<Identifier>();

		// Token: 0x04000BAA RID: 2986
		private readonly TalentMenu talentMenu = new TalentMenu();

		// Token: 0x04000BAB RID: 2987
		private const float JobColumnWidthPercentage = 0.138f;

		// Token: 0x04000BAC RID: 2988
		private const float CharacterColumnWidthPercentage = 0.45f;

		// Token: 0x04000BAD RID: 2989
		private const float KillColumnWidthPercentage = 0.1f;

		// Token: 0x04000BAE RID: 2990
		private const float DeathColumnWidthPercentage = 0.1f;

		// Token: 0x04000BAF RID: 2991
		private const float PingColumnWidthPercentage = 0.15f;

		// Token: 0x04000BB0 RID: 2992
		private const float WalletColumnWidthPercentage = 0.206f;

		// Token: 0x04000BB1 RID: 2993
		private int jobColumnWidth;

		// Token: 0x04000BB2 RID: 2994
		private int characterColumnWidth;

		// Token: 0x04000BB3 RID: 2995
		private int pingColumnWidth;

		// Token: 0x04000BB4 RID: 2996
		private int walletColumnWidth;

		// Token: 0x04000BB5 RID: 2997
		private int deathColumnWidth;

		// Token: 0x04000BB6 RID: 2998
		private int killColumnWidth;

		// Token: 0x04000BB7 RID: 2999
		[TupleElementNames(new string[]
		{
			"message",
			"type"
		})]
		private static readonly List<ValueTuple<string, PlayerConnectionChangeType>> storedMessages = new List<ValueTuple<string, PlayerConnectionChangeType>>();

		// Token: 0x04000BB8 RID: 3000
		private GUIImage talentPointNotification;

		// Token: 0x04000BB9 RID: 3001
		private GUIImage eventLogNotification;

		// Token: 0x020009F2 RID: 2546
		public enum InfoFrameTab
		{
			// Token: 0x040042B0 RID: 17072
			Crew,
			// Token: 0x040042B1 RID: 17073
			Mission,
			// Token: 0x040042B2 RID: 17074
			Reputation,
			// Token: 0x040042B3 RID: 17075
			Submarine,
			// Token: 0x040042B4 RID: 17076
			Talents
		}

		// Token: 0x020009F3 RID: 2547
		private class LinkedGUI
		{
			// Token: 0x0600736E RID: 29550 RVA: 0x0036F244 File Offset: 0x0036D444
			public LinkedGUI(Client client, GUIFrame frame, GUITextBlock textBlock, GUIImage permissionIcon)
			{
				this.Client = client;
				this.textBlock = textBlock;
				this.frame = frame;
				this.permissionIcon = permissionIcon;
				this.character = ((client != null) ? client.Character : null);
				this.wasCharacterAlive = (((client != null) ? client.Character : null) != null && !client.Character.IsDead);
			}

			// Token: 0x0600736F RID: 29551 RVA: 0x0036F2AB File Offset: 0x0036D4AB
			public LinkedGUI(Character character, GUIFrame frame, GUITextBlock textBlock)
			{
				this.character = character;
				this.textBlock = textBlock;
				this.frame = frame;
				this.wasCharacterAlive = (character != null && !character.IsDead);
			}

			// Token: 0x06007370 RID: 29552 RVA: 0x0036F2E0 File Offset: 0x0036D4E0
			public bool HasMultiplayerCharacterChanged()
			{
				if (this.Client == null)
				{
					return false;
				}
				if (GameSettings.CurrentConfig.VerboseLogging && this.Client.Character != this.character)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Refreshing tab menu crew list (client \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.Client.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\"'s character changed from \"");
					Character character = this.character;
					defaultInterpolatedStringHandler.AppendFormatted(((character != null) ? character.Name : null) ?? "null");
					defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
					Character character2 = this.Client.Character;
					defaultInterpolatedStringHandler.AppendFormatted(((character2 != null) ? character2.Name : null) ?? "null");
					defaultInterpolatedStringHandler.AppendLiteral("\")");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return this.Client.Character != this.character;
			}

			// Token: 0x06007371 RID: 29553 RVA: 0x0036F3D4 File Offset: 0x0036D5D4
			public bool HasCharacterDied()
			{
				if (this.character == null)
				{
					return false;
				}
				Character character = this.character;
				bool isAlive = character != null && !character.IsDead;
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					if (this.wasCharacterAlive && !isAlive)
					{
						string message;
						if (this.Client != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Refreshing tab menu crew list (client \"");
							defaultInterpolatedStringHandler.AppendFormatted(this.Client.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\"'s character \"");
							Character character2 = this.character;
							defaultInterpolatedStringHandler.AppendFormatted(((character2 != null) ? character2.Name : null) ?? "null");
							defaultInterpolatedStringHandler.AppendLiteral("\" died)");
							message = defaultInterpolatedStringHandler.ToStringAndClear();
						}
						else
						{
							string str = "Refreshing tab menu crew list (character \"";
							Character character3 = this.character;
							message = str + (((character3 != null) ? character3.Name : null) ?? "null") + "\" died)";
						}
						DebugConsole.Log(message);
					}
					else if (!this.wasCharacterAlive && isAlive)
					{
						string message2;
						if (this.Client != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(74, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Refreshing tab menu crew list (client \"");
							defaultInterpolatedStringHandler2.AppendFormatted(this.Client.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\"'s character \"");
							Character character4 = this.character;
							defaultInterpolatedStringHandler2.AppendFormatted(((character4 != null) ? character4.Name : null) ?? "null");
							defaultInterpolatedStringHandler2.AppendLiteral("\" came back to life)");
							message2 = defaultInterpolatedStringHandler2.ToStringAndClear();
						}
						else
						{
							string str2 = "Refreshing tab menu crew list (character \"";
							Character character5 = this.character;
							message2 = str2 + (((character5 != null) ? character5.Name : null) ?? "null") + "\" came back to life)";
						}
						DebugConsole.Log(message2);
					}
				}
				return isAlive != this.wasCharacterAlive;
			}

			// Token: 0x06007372 RID: 29554 RVA: 0x0036F588 File Offset: 0x0036D788
			public void TryPingRefresh()
			{
				if (this.Client == null)
				{
					return;
				}
				if (this.currentPing == this.Client.Ping)
				{
					return;
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.ConnectedClients.Contains(this.Client))
				{
					this.currentPing = this.Client.Ping;
					this.textBlock.Text = this.currentPing.ToString();
					this.textBlock.TextColor = this.GetPingColor();
					this.textBlock.ToolTip = string.Empty;
					return;
				}
				this.currentPing = 0;
				this.textBlock.Text = "-";
				this.textBlock.TextColor = GUIStyle.Red;
				this.textBlock.ToolTip = TextManager.Get("causeofdeathdescription.disconnected");
			}

			// Token: 0x06007373 RID: 29555 RVA: 0x0036F66E File Offset: 0x0036D86E
			public void TryPermissionIconRefresh(Sprite icon)
			{
				if (this.Client == null || this.permissionIcon == null)
				{
					return;
				}
				this.permissionIcon.Sprite = icon;
			}

			// Token: 0x06007374 RID: 29556 RVA: 0x0036F68D File Offset: 0x0036D88D
			private Color GetPingColor()
			{
				if (this.currentPing < 100)
				{
					return GUIStyle.Green;
				}
				if (this.currentPing < 200)
				{
					return GUIStyle.Yellow;
				}
				return GUIStyle.Red;
			}

			// Token: 0x06007375 RID: 29557 RVA: 0x0036F6C6 File Offset: 0x0036D8C6
			public void Remove(GUIFrame parent)
			{
				parent.RemoveChild(this.frame);
			}

			// Token: 0x040042B5 RID: 17077
			private const ushort lowPingThreshold = 100;

			// Token: 0x040042B6 RID: 17078
			private const ushort mediumPingThreshold = 200;

			// Token: 0x040042B7 RID: 17079
			public readonly Client Client;

			// Token: 0x040042B8 RID: 17080
			private ushort currentPing;

			// Token: 0x040042B9 RID: 17081
			private readonly Character character;

			// Token: 0x040042BA RID: 17082
			private readonly bool wasCharacterAlive;

			// Token: 0x040042BB RID: 17083
			private readonly GUITextBlock textBlock;

			// Token: 0x040042BC RID: 17084
			private readonly GUIFrame frame;

			// Token: 0x040042BD RID: 17085
			private readonly GUIImage permissionIcon;
		}

		// Token: 0x020009F4 RID: 2548
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040042BE RID: 17086
			public static Action <0>__ResetRewardDistributions;
		}
	}
}
