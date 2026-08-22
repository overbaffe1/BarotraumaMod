using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000073 RID: 115
	internal class RoundSummary
	{
		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x060010A3 RID: 4259 RVA: 0x0009E6BF File Offset: 0x0009C8BF
		// (set) Token: 0x060010A4 RID: 4260 RVA: 0x0009E6C7 File Offset: 0x0009C8C7
		public GUILayoutGroup ButtonArea { get; private set; }

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x060010A5 RID: 4261 RVA: 0x0009E6D0 File Offset: 0x0009C8D0
		// (set) Token: 0x060010A6 RID: 4262 RVA: 0x0009E6D8 File Offset: 0x0009C8D8
		public GUIButton ContinueButton { get; private set; }

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x060010A7 RID: 4263 RVA: 0x0009E6E1 File Offset: 0x0009C8E1
		// (set) Token: 0x060010A8 RID: 4264 RVA: 0x0009E6E9 File Offset: 0x0009C8E9
		public GUIComponent Frame { get; private set; }

		// Token: 0x060010A9 RID: 4265 RVA: 0x0009E6F4 File Offset: 0x0009C8F4
		public RoundSummary(GameMode gameMode, IEnumerable<Mission> selectedMissions, Location startLocation, Location endLocation)
		{
			this.gameMode = gameMode;
			this.selectedMissions = selectedMissions.ToList<Mission>();
			this.startLocation = startLocation;
			this.endLocation = endLocation;
			CampaignMode campaignMode = gameMode as CampaignMode;
			if (campaignMode != null)
			{
				foreach (Faction faction in campaignMode.Factions)
				{
					this.initialFactionReputations.Add(faction.Prefab.Identifier, faction.Reputation.Value);
				}
			}
		}

		// Token: 0x060010AA RID: 4266 RVA: 0x0009E7BC File Offset: 0x0009C9BC
		public GUIFrame CreateSummaryFrame(GameSession gameSession, string endMessage, CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None, TraitorManager.TraitorResults? traitorResults = null)
		{
			bool singleplayer = GameMain.NetworkMember == null;
			bool flag;
			if (!gameSession.GameMode.IsSinglePlayer)
			{
				flag = gameSession.CrewManager.GetCharacters().All((Character c) => c.IsDead || c.IsIncapacitated || c.IsBot);
			}
			else
			{
				flag = gameSession.CrewManager.GetCharacters().All((Character c) => c.IsDead || c.IsIncapacitated);
			}
			bool gameOver = flag;
			if (!singleplayer)
			{
				SoundPlayer.OverrideMusicType = (gameOver ? "crewdead" : "endround").ToIdentifier();
				SoundPlayer.OverrideMusicDuration = new float?(18f);
			}
			Vector2 relativeSize = GUI.Canvas.RelativeSize;
			RectTransform canvas = GUI.Canvas;
			Anchor anchor = Anchor.Center;
			Pivot? pivot = null;
			Point? minSize = null;
			Point? point = null;
			GUIFrame background = new GUIFrame(new RectTransform(relativeSize, canvas, anchor, pivot, minSize, point, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				UserData = this
			};
			List<GUIComponent> rightPanels = new List<GUIComponent>();
			int minWidth = 400;
			int minHeight = 350;
			int padding = GUI.IntScale(25f);
			Vector2 relativeSize2 = new Vector2(0.35f, 0.4f);
			RectTransform rectTransform = background.RectTransform;
			Anchor anchor2 = Anchor.TopCenter;
			point = new Point?(new Point(minWidth, minHeight));
			GUIFrame crewFrame = new GUIFrame(new RectTransform(relativeSize2, rectTransform, anchor2, null, point, null, ScaleBasis.Normal), "", null);
			GUIFrame crewFrameInner = new GUIFrame(new RectTransform(new Point(crewFrame.Rect.Width - padding * 2, crewFrame.Rect.Height - padding * 2), crewFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "InnerFrame", null);
			GUILayoutGroup crewContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), crewFrameInner.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize3 = new Vector2(1f, 0f);
			RectTransform rectTransform2 = crewContent.RectTransform;
			Anchor anchor3 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			Point? minSize2 = null;
			point = null;
			RectTransform rectT = new RectTransform(relativeSize3, rectTransform2, anchor3, pivot2, minSize2, point, ScaleBasis.Normal);
			RichString text = TextManager.Get("crew");
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock crewHeader = new GUITextBlock(rectT, text, null, font, Alignment.TopLeft, false, "", null);
			crewHeader.RectTransform.MinSize = new Point(0, GUI.IntScale((float)crewHeader.Rect.Height * 2f));
			GUIListBox crewList = this.CreateCrewList(crewContent, from c in gameSession.CrewManager.GetCharacterInfos(false)
			where c.TeamID != CharacterTeamType.Team2
			select c, traitorResults);
			if (traitorResults != null && traitorResults.Value.VotedAsTraitorClientSessionId > 0)
			{
				GUIComponent traitorInfoPanel = RoundSummary.CreateTraitorInfoPanel(crewList.Content, traitorResults.Value, this.crewListAnimDelay);
				traitorInfoPanel.RectTransform.SetAsFirstChild();
				GUIFrame spacing = new GUIFrame(new RectTransform(new Point(0, GUI.IntScale(20f)), crewList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				spacing.RectTransform.RepositionChildInHierarchy(1);
			}
			if (gameSession.Missions.Any((Mission m) => m is CombatMission))
			{
				crewHeader.Text = CombatMission.GetTeamName(CharacterTeamType.Team1);
				Vector2 relativeSize4 = crewFrame.RectTransform.RelativeSize;
				RectTransform rectTransform3 = background.RectTransform;
				Anchor anchor4 = Anchor.TopCenter;
				point = new Point?(new Point(minWidth, minHeight));
				GUIFrame crewFrame2 = new GUIFrame(new RectTransform(relativeSize4, rectTransform3, anchor4, null, point, null, ScaleBasis.Normal), "", null);
				rightPanels.Add(crewFrame2);
				GUIFrame crewFrameInner2 = new GUIFrame(new RectTransform(new Point(crewFrame2.Rect.Width - padding * 2, crewFrame2.Rect.Height - padding * 2), crewFrame2.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "InnerFrame", null);
				GUILayoutGroup crewContent2 = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), crewFrameInner2.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true
				};
				Vector2 relativeSize5 = new Vector2(1f, 0f);
				RectTransform rectTransform4 = crewContent2.RectTransform;
				Anchor anchor5 = Anchor.TopLeft;
				Pivot? pivot3 = null;
				Point? minSize3 = null;
				point = null;
				RectTransform rectT2 = new RectTransform(relativeSize5, rectTransform4, anchor5, pivot3, minSize3, point, ScaleBasis.Normal);
				RichString text2 = CombatMission.GetTeamName(CharacterTeamType.Team2);
				font = GUIStyle.SubHeadingFont;
				GUITextBlock crewHeader2 = new GUITextBlock(rectT2, text2, null, font, Alignment.TopLeft, false, "", null);
				crewHeader2.RectTransform.MinSize = new Point(0, GUI.IntScale((float)crewHeader2.Rect.Height * 2f));
				this.CreateCrewList(crewContent2, from c in gameSession.CrewManager.GetCharacterInfos(false)
				where c.TeamID == CharacterTeamType.Team2
				select c, traitorResults);
				if (CombatMission.Winner != CharacterTeamType.None)
				{
					RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), crewHeader.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get((CombatMission.Winner == CharacterTeamType.Team1) ? "pvpmode.victory" : "pvpmode.defeat");
					font = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectT3, text3, new Color?((CombatMission.Winner == CharacterTeamType.Team1) ? GUIStyle.Green : GUIStyle.Red), font, Alignment.TopRight, false, "", null);
					Vector2 relativeSize6 = new Vector2(1f, 0f);
					RectTransform rectTransform5 = crewHeader2.RectTransform;
					Anchor anchor6 = Anchor.TopLeft;
					Pivot? pivot4 = null;
					Point? minSize4 = null;
					point = null;
					RectTransform rectT4 = new RectTransform(relativeSize6, rectTransform5, anchor6, pivot4, minSize4, point, ScaleBasis.Normal);
					RichString text4 = TextManager.Get((CombatMission.Winner == CharacterTeamType.Team2) ? "pvpmode.victory" : "pvpmode.defeat");
					font = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectT4, text4, new Color?((CombatMission.Winner == CharacterTeamType.Team2) ? GUIStyle.Green : GUIStyle.Red), font, Alignment.TopRight, false, "", null);
				}
			}
			LocalizedString headerText = this.GetHeaderText(gameOver, transitionType);
			GUITextBlock headerTextBlock = null;
			if (!headerText.IsNullOrEmpty())
			{
				Vector2 relativeSize7 = new Vector2(1f, 0.5f);
				RectTransform rectTransform6 = crewFrame.RectTransform;
				Anchor anchor7 = Anchor.TopLeft;
				Pivot? pivot5 = new Pivot?(Pivot.BottomLeft);
				Point? minSize5 = null;
				point = null;
				RectTransform rectT5 = new RectTransform(relativeSize7, rectTransform6, anchor7, pivot5, minSize5, point, ScaleBasis.Normal);
				RichString text5 = headerText;
				font = GUIStyle.LargeFont;
				headerTextBlock = new GUITextBlock(rectT5, text5, null, font, Alignment.BottomLeft, true, "", null);
			}
			CampaignMode campaignMode = this.gameMode as CampaignMode;
			if (campaignMode != null)
			{
				Vector2 relativeSize8 = crewFrame.RectTransform.RelativeSize;
				RectTransform rectTransform7 = background.RectTransform;
				Anchor anchor8 = Anchor.TopCenter;
				point = new Point?(crewFrame.RectTransform.MinSize);
				GUIFrame reputationframe = new GUIFrame(new RectTransform(relativeSize8, rectTransform7, anchor8, null, point, null, ScaleBasis.Normal), "", null);
				rightPanels.Add(reputationframe);
				GUIFrame reputationframeInner = new GUIFrame(new RectTransform(new Point(reputationframe.Rect.Width - padding * 2, reputationframe.Rect.Height - padding * 2), reputationframe.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "InnerFrame", null);
				GUILayoutGroup reputationContent = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.95f), reputationframeInner.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true
				};
				Vector2 relativeSize9 = new Vector2(1f, 0f);
				RectTransform rectTransform8 = reputationContent.RectTransform;
				Anchor anchor9 = Anchor.TopLeft;
				Pivot? pivot6 = null;
				Point? minSize6 = null;
				point = null;
				RectTransform rectT6 = new RectTransform(relativeSize9, rectTransform8, anchor9, pivot6, minSize6, point, ScaleBasis.Normal);
				RichString text6 = TextManager.Get("reputation");
				font = GUIStyle.SubHeadingFont;
				GUITextBlock reputationHeader = new GUITextBlock(rectT6, text6, null, font, Alignment.TopLeft, false, "", null);
				reputationHeader.RectTransform.MinSize = new Point(0, GUI.IntScale((float)reputationHeader.Rect.Height * 2f));
				this.CreateReputationInfoPanel(reputationContent, campaignMode);
			}
			Vector2 relativeSize10 = new Vector2(0.5f, 0.4f);
			RectTransform rectTransform9 = background.RectTransform;
			Anchor anchor10 = Anchor.TopCenter;
			point = new Point?(new Point(minWidth, minHeight / 4));
			GUIFrame missionframe = new GUIFrame(new RectTransform(relativeSize10, rectTransform9, anchor10, null, point, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup missionFrameContent = new GUILayoutGroup(new RectTransform(new Point(missionframe.Rect.Width - padding * 2, missionframe.Rect.Height - padding * 2), missionframe.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.03f
			};
			GUIFrame missionframeInner = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), missionFrameContent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			GUILayoutGroup missionContent = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.93f), missionframeInner.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			List<Mission> missionsToDisplay = new List<Mission>(from m in this.selectedMissions
			where m.Prefab.ShowInMenus
			select m);
			if (this.startLocation != null)
			{
				foreach (Mission mission in this.startLocation.SelectedMissions.Union(from m in this.startLocation.AvailableMissions
				where m.Prefab.IsSideObjective
				select m))
				{
					if (!missionsToDisplay.Contains(mission) && mission.Prefab.ShowInMenus && (mission.Locations[0] == mission.Locations[1] || mission.Locations.Contains((campaignMode != null) ? campaignMode.Map.SelectedLocation : null)))
					{
						missionsToDisplay.Add(mission);
					}
				}
			}
			GUIListBox missionList = new GUIListBox(new RectTransform(Vector2.One, missionContent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = GUI.IntScale(15f)
			};
			missionList.ContentBackground.Color = Color.Transparent;
			this.ButtonArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), missionFrameContent.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), true, Anchor.BottomRight)
			{
				RelativeSpacing = 0.025f
			};
			missionFrameContent.Recalculate();
			missionContent.Recalculate();
			if (!string.IsNullOrWhiteSpace(endMessage))
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetServerMessage(endMessage), null, null, Alignment.Left, true, "", null).CanBeFocused = false;
			}
			float animDelay = this.missionIconAnimDelay;
			foreach (Mission mission2 in missionsToDisplay)
			{
				List<LocalizedString> textContent = new List<LocalizedString>();
				if (this.selectedMissions.Contains(mission2))
				{
					textContent.Add(mission2.Completed ? mission2.SuccessMessage : mission2.FailureMessage);
					RichString repText = mission2.GetReputationRewardText();
					if (!repText.IsNullOrEmpty())
					{
						textContent.Add(repText);
					}
					int totalReward = mission2.GetFinalReward(Submarine.MainSub);
					if (totalReward > 0)
					{
						textContent.Add(mission2.GetMissionRewardText(Submarine.MainSub));
						if (GameMain.IsMultiplayer)
						{
							Character controlled = Character.Controlled;
							if (controlled != null && mission2.Completed)
							{
								ValueTuple<int, int, float> rewardShare = Mission.GetRewardShare(controlled.Wallet.RewardDistribution, from c in GameSession.GetSessionCrewCharacters(CharacterType.Player)
								where c != controlled
								select c, Option<int>.Some(totalReward));
								int share = rewardShare.Item1;
								int percentage = rewardShare.Item2;
								if (share > 0)
								{
									string shareFormatted = string.Format(CultureInfo.InvariantCulture, "{0:N0}", share);
									string tag = "crewwallet.missionreward.get";
									ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
									array[0] = new ValueTuple<string, string>("[money]", shareFormatted ?? "");
									int num = 1;
									string item = "[share]";
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
									defaultInterpolatedStringHandler.AppendFormatted<int>(percentage);
									array[num] = new ValueTuple<string, string>(item, defaultInterpolatedStringHandler.ToStringAndClear());
									RichString yourShareString = TextManager.GetWithVariables(tag, array);
									textContent.Add(yourShareString);
								}
							}
						}
					}
				}
				else
				{
					RichString repText2 = mission2.GetReputationRewardText();
					if (!repText2.IsNullOrEmpty())
					{
						textContent.Add(repText2);
					}
					textContent.Add(mission2.GetMissionRewardText(Submarine.MainSub));
					textContent.Add(mission2.Description);
					textContent.AddRange(mission2.ShownMessages);
				}
				GUIImage missionIcon;
				RoundSummary.CreateMissionEntry(missionList.Content, mission2.Name, textContent, mission2.Difficulty.GetValueOrDefault(), mission2.Prefab.Icon, mission2.Prefab.IconColor, mission2.GetDifficultyToolTipText(), out missionIcon);
				if (this.selectedMissions.Contains(mission2))
				{
					bool success;
					if (!(mission2 is CombatMission))
					{
						success = mission2.Completed;
					}
					else
					{
						GameClient client = GameMain.Client;
						success = CombatMission.IsInWinningTeam((client != null) ? client.Character : null);
					}
					RoundSummary.UpdateMissionStateIcon(success, missionIcon, animDelay);
					animDelay += 0.25f;
				}
			}
			if (!missionsToDisplay.Any<Mission>())
			{
				GUILayoutGroup missionContentHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), missionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					RelativeSpacing = 0.025f,
					Stretch = true,
					CanBeFocused = true
				};
				GUIImage missionIcon2 = new GUIImage(new RectTransform(new Point(missionContentHorizontal.Rect.Height), missionContentHorizontal.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "NoMissionIcon", true);
				RectTransform rectT7 = new RectTransform(new Vector2(1f, 0f), missionContentHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text7 = TextManager.Get("nomission");
				font = GUIStyle.LargeFont;
				new GUITextBlock(rectT7, text7, null, font, Alignment.Left, false, "", null);
			}
			if (gameSession != null)
			{
				EventManager eventManager = gameSession.EventManager;
				if (eventManager != null)
				{
					EventLog eventLog = eventManager.EventLog;
					if (eventLog != null)
					{
						eventLog.CreateEventLogUI(missionList.Content, traitorResults);
					}
				}
			}
			RoundSummary.AddSeparators(missionList.Content);
			this.ContinueButton = new GUIButton(new RectTransform(new Vector2(0.25f, 1f), this.ButtonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null);
			this.ButtonArea.RectTransform.NonScaledSize = new Point(this.ButtonArea.Rect.Width, this.ContinueButton.Rect.Height);
			this.ButtonArea.RectTransform.IsFixedSize = true;
			missionFrameContent.Recalculate();
			int panelSpacing = GUI.IntScale(20f);
			int totalHeight = crewFrame.Rect.Height + panelSpacing + missionframe.Rect.Height;
			int totalWidth = crewFrame.Rect.Width;
			crewFrame.RectTransform.AbsoluteOffset = new Point(0, (GameMain.GraphicsHeight - totalHeight) / 2);
			missionframe.RectTransform.AbsoluteOffset = new Point(0, crewFrame.Rect.Bottom + panelSpacing);
			if (rightPanels.Any<GUIComponent>())
			{
				totalWidth = crewFrame.Rect.Width * 2 + panelSpacing;
				if (headerTextBlock != null)
				{
					headerTextBlock.RectTransform.MinSize = new Point(totalWidth, 0);
				}
				crewFrame.RectTransform.AbsoluteOffset = new Point(-(crewFrame.Rect.Width + panelSpacing) / 2, crewFrame.RectTransform.AbsoluteOffset.Y);
				foreach (GUIComponent rightPanel in rightPanels)
				{
					rightPanel.RectTransform.AbsoluteOffset = new Point((rightPanel.Rect.Width + panelSpacing) / 2, crewFrame.RectTransform.AbsoluteOffset.Y);
				}
			}
			this.Frame = background;
			return background;
		}

		// Token: 0x060010AB RID: 4267 RVA: 0x0009FAAC File Offset: 0x0009DCAC
		public void CreateReputationInfoPanel(GUIComponent parent, CampaignMode campaignMode)
		{
			GUIListBox reputationList = new GUIListBox(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			reputationList.ContentBackground.Color = Color.Transparent;
			foreach (Faction faction in from f in campaignMode.Factions
			orderby f.Prefab.MenuOrder, f.Prefab.Name
			select f)
			{
				float initialReputation = faction.Reputation.Value;
				if (!this.initialFactionReputations.TryGetValue(faction.Prefab.Identifier, out initialReputation))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not determine reputation change for faction \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(faction.Prefab.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" (faction was not present at the start of the round).");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				GUIFrame factionFrame = RoundSummary.CreateReputationElement(reputationList.Content, faction.Prefab.Name, faction.Reputation, initialReputation, faction.Prefab.ShortDescription, faction.Prefab.Description, faction.Prefab.Icon, faction.Prefab.BackgroundPortrait, faction.Prefab.IconColor);
				RoundSummary.<CreateReputationInfoPanel>g__CreatePathUnlockElement|31_0(factionFrame, faction, null);
			}
			float maxDescriptionHeight = 0f;
			foreach (GUIComponent child in reputationList.Content.Children)
			{
				GUITextBlock descriptionElement2 = child.FindChild("description", true) as GUITextBlock;
				float descriptionHeight = descriptionElement2.TextSize.Y * 1.1f;
				GUIComponent unlockInfoComponent = child.FindChild("unlockinfo", false);
				if (unlockInfoComponent != null)
				{
					descriptionHeight += 1.25f * (float)unlockInfoComponent.Rect.Height;
				}
				maxDescriptionHeight = Math.Max(maxDescriptionHeight, descriptionHeight);
			}
			foreach (GUIComponent child2 in reputationList.Content.Children)
			{
				GUITextBlock headerElement = child2.FindChild("header", true) as GUITextBlock;
				GUITextBlock descriptionElement = child2.FindChild("description", true) as GUITextBlock;
				descriptionElement.RectTransform.NonScaledSize = new Point(descriptionElement.Rect.Width, (int)maxDescriptionHeight);
				descriptionElement.RectTransform.IsFixedSize = true;
				child2.RectTransform.NonScaledSize = new Point(child2.Rect.Width, headerElement.Rect.Height + descriptionElement.RectTransform.Parent.Children.Sum((RectTransform c) => c.Rect.Height + ((GUILayoutGroup)descriptionElement.Parent).AbsoluteSpacing));
			}
		}

		// Token: 0x060010AC RID: 4268 RVA: 0x0009FE18 File Offset: 0x0009E018
		private static GUIComponent CreateTraitorInfoPanel(GUIComponent parent, TraitorManager.TraitorResults traitorResults, float iconAnimDelay)
		{
			Client traitorClient = traitorResults.GetTraitorClient();
			Character traitorCharacter = (traitorClient != null) ? traitorClient.Character : null;
			string resultTag = traitorResults.VotedCorrectTraitor ? (traitorResults.ObjectiveSuccessful ? "traitor.blameresult.correct.objectivesuccessful" : "traitor.blameresult.correct.objectivefailed") : "traitor.blameresult.failure";
			List<LocalizedString> textContent = new List<LocalizedString>
			{
				TextManager.GetWithVariable("traitor.blameresult", "[name]", ((traitorCharacter != null) ? traitorCharacter.Name : null) ?? "unknown", FormatCapitals.No),
				TextManager.Get(resultTag)
			};
			if (traitorResults.MoneyPenalty > 0)
			{
				textContent.Add(TextManager.GetWithVariable("traitor.blameresult.failure.penalty", "[money]", TextManager.FormatCurrency(traitorResults.MoneyPenalty, false), FormatCapitals.No));
			}
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("TraitorMissionIcon");
			Sprite icon = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
			GUIImage missionIcon;
			GUIComponent content = RoundSummary.CreateMissionEntry(parent, string.Empty, textContent, 0, icon, GUIStyle.Red, null, out missionIcon);
			RoundSummary.UpdateMissionStateIcon(traitorResults.VotedCorrectTraitor, missionIcon, iconAnimDelay);
			return content;
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x0009FF14 File Offset: 0x0009E114
		public static GUIComponent CreateMissionEntry(GUIComponent parent, LocalizedString header, List<LocalizedString> textContent, int difficultyIconCount, Sprite icon, Color iconColor, RichString difficultyTooltipText, out GUIImage missionIcon)
		{
			int spacing = GUI.IntScale(5f);
			int defaultLineHeight = (int)GUIStyle.Font.MeasureChar('T').Y;
			int iconSize = (int)(GUIStyle.SubHeadingFont.MeasureChar('T').Y + (float)(defaultLineHeight * 6));
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Point(parent.Rect.Width, iconSize), parent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = spacing,
				CanBeFocused = true
			};
			if (icon != null)
			{
				missionIcon = new GUIImage(new RectTransform(new Point(iconSize), content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), icon, true, null)
				{
					Color = iconColor,
					HoverColor = iconColor,
					SelectedColor = iconColor,
					CanBeFocused = false
				};
				missionIcon.RectTransform.IsFixedSize = true;
			}
			else
			{
				missionIcon = null;
			}
			GUILayoutGroup missionTextGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.744f, 0f), content.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = spacing
			};
			content.Recalculate();
			RichString missionNameString = RichString.Rich(header, null);
			List<RichString> contentStrings = new List<RichString>(from t in textContent
			select RichString.Rich(t, null));
			if (!header.IsNullOrEmpty())
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), missionTextGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = missionNameString;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				GUITextBlock nameText = new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, true, "", null);
				nameText.RectTransform.MinSize = new Point(0, (int)nameText.TextSize.Y);
			}
			GUILayoutGroup difficultyIndicatorGroup = null;
			if (difficultyIconCount > 0)
			{
				difficultyIndicatorGroup = new GUILayoutGroup(new RectTransform(new Point(missionTextGroup.Rect.Width, defaultLineHeight), missionTextGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft)
				{
					AbsoluteSpacing = 1,
					CanBeFocused = true
				};
				difficultyIndicatorGroup.RectTransform.MinSize = new Point(0, defaultLineHeight);
				Color difficultyColor = Mission.GetDifficultyColor(difficultyIconCount);
				for (int i = 0; i < difficultyIconCount; i++)
				{
					GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, difficultyIndicatorGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), "DifficultyIndicator", true);
					guiimage.Color = difficultyColor;
					guiimage.ToolTip = difficultyTooltipText;
				}
			}
			GUITextBlock firstContentText = null;
			foreach (RichString contentString in contentStrings)
			{
				GUITextBlock text = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), missionTextGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), contentString, null, null, Alignment.Left, true, "", null);
				text.RectTransform.MinSize = new Point(0, (int)text.TextSize.Y);
				if (firstContentText == null)
				{
					firstContentText = text;
				}
			}
			if (difficultyIndicatorGroup != null && firstContentText != null)
			{
				difficultyIndicatorGroup.RectTransform.AbsoluteOffset = new Point((int)firstContentText.Padding.X, 0);
			}
			missionTextGroup.RectTransform.MinSize = new Point(0, missionTextGroup.Children.Sum((GUIComponent c) => c.Rect.Height + missionTextGroup.AbsoluteSpacing) - missionTextGroup.AbsoluteSpacing);
			missionTextGroup.Recalculate();
			content.RectTransform.MinSize = new Point(0, Math.Max(missionTextGroup.Rect.Height, iconSize));
			return content;
		}

		// Token: 0x060010AE RID: 4270 RVA: 0x000A0368 File Offset: 0x0009E568
		public static void AddSeparators(GUIComponent container)
		{
			List<GUIComponent> children = container.Children.ToList<GUIComponent>();
			if (children.Count < 2)
			{
				return;
			}
			GUIComponent lastChild = children.Last<GUIComponent>();
			foreach (GUIComponent child in children)
			{
				if (child != lastChild)
				{
					GUIFrame separator = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
					separator.RectTransform.RepositionChildInHierarchy(container.GetChildIndex(child) + 1);
				}
			}
		}

		// Token: 0x060010AF RID: 4271 RVA: 0x000A0438 File Offset: 0x0009E638
		public static void UpdateMissionStateIcon(bool success, GUIImage missionIcon, float delay = 0.5f)
		{
			if (missionIcon == null)
			{
				return;
			}
			string style = success ? "MissionCompletedIcon" : "MissionFailedIcon";
			GUIImage stateIcon = missionIcon.GetChild<GUIImage>();
			if (string.IsNullOrEmpty(style))
			{
				if (stateIcon != null)
				{
					stateIcon.Visible = false;
					return;
				}
			}
			else
			{
				bool wasVisible = stateIcon != null && stateIcon.Visible;
				if (stateIcon == null)
				{
					stateIcon = new GUIImage(new RectTransform(Vector2.One, missionIcon.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), style, true);
				}
				stateIcon.Visible = true;
				if (!wasVisible)
				{
					stateIcon.FadeIn(delay, 0.15f, false);
					stateIcon.Pulsate(Vector2.One, Vector2.One * 1.5f, 1f + delay);
				}
			}
		}

		// Token: 0x060010B0 RID: 4272 RVA: 0x000A04F8 File Offset: 0x0009E6F8
		private LocalizedString GetHeaderText(bool gameOver, CampaignMode.TransitionType transitionType)
		{
			Submarine mainSub = Submarine.MainSub;
			LocalizedString localizedString;
			if (mainSub == null || !mainSub.AtEndExit)
			{
				Location location = this.startLocation;
				localizedString = ((location != null) ? location.DisplayName : null);
			}
			else
			{
				Location location2 = this.endLocation;
				localizedString = ((location2 != null) ? location2.DisplayName : null);
			}
			LocalizedString locationName = localizedString;
			string textTag;
			if (this.gameMode is PvPMode)
			{
				textTag = "RoundSummaryRoundHasEnded";
			}
			else if (gameOver)
			{
				textTag = "RoundSummaryGameOver";
			}
			else
			{
				switch (transitionType)
				{
				case CampaignMode.TransitionType.LeaveLocation:
				{
					Location location3 = this.startLocation;
					locationName = ((location3 != null) ? location3.DisplayName : null);
					textTag = "RoundSummaryLeaving";
					break;
				}
				case CampaignMode.TransitionType.ProgressToNextLocation:
				{
					Location location4 = this.endLocation;
					locationName = ((location4 != null) ? location4.DisplayName : null);
					textTag = "RoundSummaryProgress";
					break;
				}
				case CampaignMode.TransitionType.ReturnToPreviousLocation:
				{
					Location location5 = this.startLocation;
					locationName = ((location5 != null) ? location5.DisplayName : null);
					textTag = "RoundSummaryReturn";
					break;
				}
				case CampaignMode.TransitionType.ReturnToPreviousEmptyLocation:
				{
					Location location6 = this.startLocation;
					locationName = ((location6 != null) ? location6.DisplayName : null);
					textTag = "RoundSummaryReturnToEmptyLocation";
					break;
				}
				case CampaignMode.TransitionType.ProgressToNextEmptyLocation:
				{
					Location location7 = this.endLocation;
					locationName = ((location7 != null) ? location7.DisplayName : null);
					textTag = "RoundSummaryProgressToEmptyLocation";
					break;
				}
				default:
					if (Submarine.MainSub == null)
					{
						textTag = "RoundSummaryRoundHasEnded";
					}
					else
					{
						textTag = (Submarine.MainSub.AtEndExit ? "RoundSummaryProgress" : "RoundSummaryReturn");
					}
					break;
				}
			}
			Location location8 = this.startLocation;
			if (((location8 != null) ? location8.Biome : null) != null && this.startLocation.Biome.IsEndBiome && locationName == null)
			{
				locationName = this.startLocation.DisplayName;
			}
			if (textTag == null)
			{
				return "";
			}
			if (locationName == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(110, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error while creating round summary: could not determine destination location. Start location: ");
				Location location9 = this.startLocation;
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(((location9 != null) ? location9.DisplayName : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(", end location: ");
				Location location10 = this.endLocation;
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(((location10 != null) ? location10.DisplayName : null) ?? "null");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				locationName = "[UNKNOWN]";
			}
			LocalizedString subName = string.Empty;
			SubmarineInfo currentOrPending = SubmarineSelection.CurrentOrPendingSubmarine();
			if (currentOrPending != null)
			{
				subName = currentOrPending.DisplayName;
			}
			return TextManager.GetWithVariables(textTag, new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[sub]", subName),
				new ValueTuple<string, LocalizedString>("[location]", locationName)
			});
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x000A0760 File Offset: 0x0009E960
		private GUIListBox CreateCrewList(GUIComponent parent, IEnumerable<CharacterInfo> characterInfos, TraitorManager.TraitorResults? traitorResults)
		{
			Vector2 relativeSize = new Vector2(1f, 0f);
			RectTransform rectTransform = parent.RectTransform;
			Anchor anchor = Anchor.TopCenter;
			Point? point = new Point?(new Point(0, (int)(30f * GUI.Scale)));
			GUILayoutGroup headerFrame = new GUILayoutGroup(new RectTransform(relativeSize, rectTransform, anchor, null, point, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			Vector2 relativeSize2 = new Vector2(0.05f, 1f);
			RectTransform rectTransform2 = headerFrame.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			GUIButton jobButton = new GUIButton(new RectTransform(relativeSize2, rectTransform2, anchor2, pivot, minSize, point, ScaleBasis.Normal), TextManager.Get("tabmenu.job"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			Vector2 relativeSize3 = new Vector2(0.35f, 1f);
			RectTransform rectTransform3 = headerFrame.RectTransform;
			Anchor anchor3 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			GUIButton characterButton = new GUIButton(new RectTransform(relativeSize3, rectTransform3, anchor3, pivot2, minSize2, point, ScaleBasis.Normal), TextManager.Get("name"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			if (this.gameMode is PvPMode)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.RespawnManager : null) != null)
				{
					Vector2 relativeSize4 = new Vector2(0.05f, 1f);
					RectTransform rectTransform4 = headerFrame.RectTransform;
					Anchor anchor4 = Anchor.TopLeft;
					Pivot? pivot3 = null;
					point = null;
					Point? minSize3 = point;
					point = null;
					GUIButton killButton = new GUIButton(new RectTransform(relativeSize4, rectTransform4, anchor4, pivot3, minSize3, point, ScaleBasis.Normal), TextManager.Get("killcount"), Alignment.Center, "GUIButtonSmallFreeScale", null);
					this.killColumnWidth = killButton.Rect.Width;
					Vector2 relativeSize5 = new Vector2(0.05f, 1f);
					RectTransform rectTransform5 = headerFrame.RectTransform;
					Anchor anchor5 = Anchor.TopLeft;
					Pivot? pivot4 = null;
					point = null;
					Point? minSize4 = point;
					point = null;
					GUIButton deathButton = new GUIButton(new RectTransform(relativeSize5, rectTransform5, anchor5, pivot4, minSize4, point, ScaleBasis.Normal), TextManager.Get("deathcount"), Alignment.Center, "GUIButtonSmallFreeScale", null);
					this.deathColumnWidth = deathButton.Rect.Width;
					goto IL_289;
				}
			}
			Vector2 relativeSize6 = new Vector2(0.25f, 1f);
			RectTransform rectTransform6 = headerFrame.RectTransform;
			Anchor anchor6 = Anchor.TopLeft;
			Pivot? pivot5 = null;
			point = null;
			Point? minSize5 = point;
			point = null;
			GUIButton statusButton = new GUIButton(new RectTransform(relativeSize6, rectTransform6, anchor6, pivot5, minSize5, point, ScaleBasis.Normal), TextManager.Get("label.statuslabel"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			this.statusColumnWidth = statusButton.Rect.Width;
			IL_289:
			foreach (GUIButton btn in headerFrame.GetAllChildren<GUIButton>())
			{
				btn.TextBlock.Font = GUIStyle.HotkeyFont;
				btn.ForceUpperCase = ForceUpperCase.Yes;
				btn.CanBeFocused = false;
			}
			this.jobColumnWidth = jobButton.Rect.Width;
			this.characterColumnWidth = characterButton.Rect.Width;
			Vector2 one = Vector2.One;
			RectTransform rectTransform7 = parent.RectTransform;
			Anchor anchor7 = Anchor.TopLeft;
			Pivot? pivot6 = null;
			point = null;
			Point? minSize6 = point;
			point = null;
			GUIListBox crewList = new GUIListBox(new RectTransform(one, rectTransform7, anchor7, pivot6, minSize6, point, ScaleBasis.Normal), false, null, "", true, false)
			{
				Padding = new Vector4(4f, 10f, 0f, 0f) * GUI.Scale,
				AutoHideScrollBar = false
			};
			crewList.ContentBackground.Color = Color.Transparent;
			headerFrame.RectTransform.RelativeSize -= new Vector2(crewList.ScrollBar.RectTransform.RelativeSize.X, 0f);
			this.killCounts.Clear();
			if (GameMain.NetworkMember != null)
			{
				foreach (CharacterInfo characterInfo in characterInfos)
				{
					if (characterInfo != null)
					{
						Character character = characterInfo.Character;
						Client ownerClient = GameMain.NetworkMember.ConnectedClients.FirstOrDefault((Client c) => c.Character == character);
						int killCount = 0;
						int deathCount = 0;
						foreach (Mission mission in this.selectedMissions)
						{
							CombatMission combatMission = mission as CombatMission;
							if (combatMission != null)
							{
								killCount += ((ownerClient == null) ? combatMission.GetBotKillCount(characterInfo) : combatMission.GetClientKillCount(ownerClient));
								deathCount += ((ownerClient == null) ? combatMission.GetBotDeathCount(characterInfo) : combatMission.GetClientDeathCount(ownerClient));
							}
						}
						this.killCounts[characterInfo] = killCount;
						this.deathCounts[characterInfo] = deathCount;
					}
				}
			}
			float delay = this.crewListAnimDelay;
			foreach (CharacterInfo characterInfo2 in from ci in characterInfos
			orderby this.killCounts.GetValueOrDefault(ci) descending
			select ci)
			{
				if (characterInfo2 != null)
				{
					this.CreateCharacterElement(characterInfo2, crewList, traitorResults, delay);
					delay += this.crewListAnimDelay;
				}
			}
			this.missionIconAnimDelay = delay;
			return crewList;
		}

		// Token: 0x060010B2 RID: 4274 RVA: 0x000A0CDC File Offset: 0x0009EEDC
		private void CreateCharacterElement(CharacterInfo characterInfo, GUIListBox listBox, TraitorManager.TraitorResults? traitorResults, float animDelay)
		{
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Point(listBox.Content.Rect.Width, GUI.IntScale(45f)), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null);
			guiframe.CanBeFocused = false;
			guiframe.UserData = characterInfo;
			Character controlled = Character.Controlled;
			guiframe.Color = ((((controlled != null) ? controlled.Info : null) == characterInfo) ? TabMenu.OwnCharacterBGColor : Color.Transparent);
			GUIFrame frame = guiframe;
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				AbsoluteSpacing = 2,
				Stretch = true
			};
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Point(this.jobColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				characterInfo.DrawJobIcon(sb, component.Rect, false);
			}, null);
			guicustomComponent.ToolTip = (characterInfo.Job.Name ?? "");
			guicustomComponent.HoverColor = Color.White;
			guicustomComponent.SelectedColor = Color.White;
			GUITextBlock characterNameBlock = new GUITextBlock(new RectTransform(new Point(this.characterColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ToolBox.LimitString(characterInfo.Name, GUIStyle.Font, this.characterColumnWidth), new Color?(characterInfo.Job.Prefab.UIColor), null, Alignment.Center, false, "", null);
			LocalizedString statusText = TextManager.Get("StatusOK");
			Color statusColor = GUIStyle.Green;
			Character character = characterInfo.Character;
			if (character == null || character.IsDead)
			{
				if (character == null && (characterInfo.IsNewHire || characterInfo.BotStatus == BotStatus.ActiveService) && characterInfo.CauseOfDeath == null)
				{
					statusText = TextManager.Get("CampaignCrew.NewHire");
					statusColor = GUIStyle.Blue;
				}
				else if (characterInfo.CauseOfDeath == null)
				{
					statusText = TextManager.Get("CauseOfDeathDescription.Unknown");
					statusColor = Color.DarkRed;
				}
				else if (characterInfo.CauseOfDeath.Type == CauseOfDeathType.Affliction && characterInfo.CauseOfDeath.Affliction == null)
				{
					string errorMsg = "Character \"[name]\" had an invalid cause of death (the type of the cause of death was Affliction, but affliction was not specified).";
					DebugConsole.ThrowError(errorMsg.Replace("[name]", characterInfo.Name), null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("RoundSummary:InvalidCauseOfDeath", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", characterInfo.SpeciesName.Value));
					statusText = TextManager.Get("CauseOfDeathDescription.Unknown");
					statusColor = GUIStyle.Red;
				}
				else
				{
					statusText = ((characterInfo.CauseOfDeath.Type == CauseOfDeathType.Affliction) ? characterInfo.CauseOfDeath.Affliction.CauseOfDeathDescription : TextManager.Get("CauseOfDeathDescription." + characterInfo.CauseOfDeath.Type.ToString()));
					statusColor = Color.DarkRed;
				}
			}
			else if (character.IsUnconscious)
			{
				statusText = TextManager.Get("Unconscious");
				statusColor = Color.DarkOrange;
			}
			else if (character.Vitality / character.MaxVitality < 0.8f)
			{
				statusText = TextManager.Get("Injured");
				statusColor = Color.DarkOrange;
			}
			if (this.gameMode is PvPMode)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.RespawnManager : null) != null)
				{
					new GUITextBlock(new RectTransform(new Point(this.killColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), this.killCounts.GetValueOrDefault(characterInfo).ToString(), null, null, Alignment.Center, false, "", null);
					new GUITextBlock(new RectTransform(new Point(this.deathColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), this.deathCounts.GetValueOrDefault(characterInfo).ToString(), null, null, Alignment.Center, false, "", null);
					goto IL_55B;
				}
			}
			new GUITextBlock(new RectTransform(new Point(this.statusColumnWidth, paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ToolBox.LimitString(statusText.Value, GUIStyle.SmallFont, this.statusColumnWidth), new Color?(statusColor), GUIStyle.SmallFont, Alignment.Center, false, "", null).ToolTip = statusText.Value;
			IL_55B:
			frame.FadeIn(animDelay, 0.15f, false);
			foreach (GUIComponent child in frame.GetAllChildren())
			{
				child.FadeIn(animDelay, 0.15f, false);
			}
			if (traitorResults != null && GameMain.NetworkMember != null)
			{
				Client clientVotedAsTraitor = traitorResults.Value.GetTraitorClient();
				bool isTraitor = clientVotedAsTraitor != null && clientVotedAsTraitor.Character == character;
				if (isTraitor)
				{
					GUIImage img = new GUIImage(new RectTransform(new Point(paddedFrame.Rect.Height), paddedFrame.RectTransform, Anchor.CenterRight, null, ScaleBasis.Normal, false), "TraitorVoteButton", GUIImage.ScalingMode.None)
					{
						IgnoreLayoutGroups = true,
						ToolTip = TextManager.GetWithVariable("traitor.blameresult", "[name]", characterInfo.Name, FormatCapitals.No)
					};
					img.FadeIn(1f + animDelay, 0.15f, false);
					img.Pulsate(Vector2.One, Vector2.One * 1.5f, 1.5f + animDelay);
				}
			}
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x000A1384 File Offset: 0x0009F584
		private static GUIFrame CreateReputationElement(GUIComponent parent, LocalizedString name, Reputation reputation, float initialReputation, LocalizedString shortDescription, LocalizedString fullDescription, Sprite icon, Sprite backgroundPortrait, Color iconColor)
		{
			RoundSummary.<>c__DisplayClass41_0 CS$<>8__locals1 = new RoundSummary.<>c__DisplayClass41_0();
			CS$<>8__locals1.backgroundPortrait = backgroundPortrait;
			CS$<>8__locals1.reputation = reputation;
			CS$<>8__locals1.initialReputation = initialReputation;
			GUIFrame factionFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (CS$<>8__locals1.backgroundPortrait != null)
			{
				GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(Vector2.One, factionFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent customComponent)
				{
					CS$<>8__locals1.backgroundPortrait.Draw(sb, customComponent.Rect.Center.ToVector2(), customComponent.Color, CS$<>8__locals1.backgroundPortrait.size / 2f, 0f, (float)customComponent.Rect.Width / CS$<>8__locals1.backgroundPortrait.size.X, SpriteEffects.None, null);
				}, null);
				guicustomComponent.HideElementsOutsideFrame = true;
				guicustomComponent.IgnoreLayoutGroups = true;
				guicustomComponent.Color = iconColor * 0.2f;
			}
			GUILayoutGroup factionInfoHorizontal = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), factionFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f),
				Stretch = true
			};
			new GUIImage(new RectTransform(Vector2.One * 0.7f, factionInfoHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), icon, true, null).Color = iconColor;
			GUILayoutGroup factionTextContent = new GUILayoutGroup(new RectTransform(Vector2.One, factionInfoHorizontal.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(10f),
				Stretch = true
			};
			factionInfoHorizontal.Recalculate();
			RectTransform rectT = new RectTransform(new Point(factionTextContent.Rect.Width, GUI.IntScale(40f)), factionTextContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text = name;
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock header = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null)
			{
				Padding = Vector4.Zero,
				UserData = "header"
			};
			header.RectTransform.IsFixedSize = true;
			GUILayoutGroup sliderHolder = new GUILayoutGroup(new RectTransform(new Point((int)((float)factionTextContent.Rect.Width * 0.8f), GUI.IntScale(20f)), factionTextContent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			sliderHolder.RectTransform.IsFixedSize = true;
			factionTextContent.Recalculate();
			new GUICustomComponent(new RectTransform(new Vector2(0.8f, 1f), sliderHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent customComponent)
			{
				RoundSummary.DrawReputationBar(sb, customComponent.Rect, CS$<>8__locals1.reputation.NormalizedValue, (float)CS$<>8__locals1.reputation.MinReputation, (float)CS$<>8__locals1.reputation.MaxReputation);
			}, null);
			RoundSummary.<>c__DisplayClass41_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(new Vector2(0.5f, 1f), sliderHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			CS$<>8__locals2.reputationText = new GUITextBlock(rectT2, text2, null, font, Alignment.CenterLeft, false, "", null);
			CS$<>8__locals1.<CreateReputationElement>g__SetReputationText|2(CS$<>8__locals1.reputationText);
			Reputation reputation2 = CS$<>8__locals1.reputation;
			if (reputation2 != null)
			{
				reputation2.OnReputationValueChanged.RegisterOverwriteExisting("RefreshRoundSummary".ToIdentifier(), delegate(Reputation _)
				{
					base.<CreateReputationElement>g__SetReputationText|2(CS$<>8__locals1.reputationText);
				});
			}
			new GUIFrame(new RectTransform(new Vector2(1f, 0f), factionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, GUI.IntScale(5f))
			}, null, null);
			RectTransform rectT3 = new RectTransform(new Vector2(0.8f, 0.6f), factionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = shortDescription;
			font = GUIStyle.SmallFont;
			GUITextBlock factionDescription = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, true, "", null)
			{
				UserData = "description",
				Padding = Vector4.Zero
			};
			if (shortDescription != fullDescription && !fullDescription.IsNullOrEmpty())
			{
				factionDescription.ToolTip = fullDescription;
			}
			new GUIFrame(new RectTransform(new Vector2(1f, 0f), factionTextContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, GUI.IntScale(5f))
			}, null, null);
			factionInfoHorizontal.Recalculate();
			factionTextContent.Recalculate();
			return factionFrame;
		}

		// Token: 0x060010B4 RID: 4276 RVA: 0x000A18CC File Offset: 0x0009FACC
		public static void DrawReputationBar(SpriteBatch sb, Rectangle rect, float normalizedReputation, float minReputation, float maxReputation)
		{
			int segmentWidth = rect.Width / 5;
			rect.Width = segmentWidth * 5;
			for (int i = 0; i < 5; i++)
			{
				GUI.DrawRectangle(sb, new Rectangle(rect.X + segmentWidth * i, rect.Y, segmentWidth, rect.Height), Reputation.GetReputationColor((float)i / 5f), true, 0f, 1f);
				GUI.DrawRectangle(sb, new Rectangle(rect.X + segmentWidth * i, rect.Y, segmentWidth, rect.Height), GUIStyle.ColorInventoryBackground, false, 0f, 1f);
			}
			GUI.DrawRectangle(sb, rect, GUIStyle.ColorInventoryBackground, false, 0f, 1f);
			GUI.Arrow.Draw(sb, new Vector2((float)rect.X + (float)rect.Width * normalizedReputation, (float)rect.Y), GUIStyle.ColorInventoryBackground, 0f, GUI.Scale, SpriteEffects.FlipVertically, null);
			GUI.Arrow.Draw(sb, new Vector2((float)rect.X + (float)rect.Width * normalizedReputation, (float)rect.Y), GUIStyle.TextColorNormal, 0f, GUI.Scale * 0.8f, SpriteEffects.FlipVertically, null);
			Vector2 pos = new Vector2((float)rect.X, (float)rect.Bottom);
			string text = ((int)minReputation).ToString();
			Color color = GUIStyle.TextColorNormal;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUI.DrawString(sb, pos, text, color, null, 0, smallFont, ForceUpperCase.Inherit);
			string maxRepText = ((int)maxReputation).ToString();
			Vector2 textSize = GUIStyle.SmallFont.MeasureString(maxRepText, false);
			Vector2 pos2 = new Vector2((float)rect.Right - textSize.X, (float)rect.Bottom);
			string text2 = maxRepText;
			Color color2 = GUIStyle.TextColorNormal;
			smallFont = GUIStyle.SmallFont;
			GUI.DrawString(sb, pos2, text2, color2, null, 0, smallFont, ForceUpperCase.Inherit);
		}

		// Token: 0x060010B5 RID: 4277 RVA: 0x000A1AC0 File Offset: 0x0009FCC0
		[CompilerGenerated]
		internal static void <CreateReputationInfoPanel>g__CreatePathUnlockElement|31_0(GUIComponent reputationFrame, Faction faction, Location location)
		{
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (gameSession == null)
			{
				flag = (null != null);
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				flag = (((campaign != null) ? campaign.Map : null) != null);
			}
			if (!flag)
			{
				return;
			}
			IEnumerable<LocationConnection> connectionsBetweenBiomes = from c in GameMain.GameSession.Campaign.Map.Connections
			where c.Locations[0].Biome != c.Locations[1].Biome
			select c;
			using (IEnumerator<LocationConnection> enumerator = connectionsBetweenBiomes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					LocationConnection connection = enumerator.Current;
					if (connection.Locked && (connection.Locations[0].Discovered || connection.Locations[1].Discovered))
					{
						if (!(from c in connectionsBetweenBiomes
						where !c.Locked
						select c).Any((LocationConnection c) => (c.Locations[0].Biome == connection.Locations[0].Biome && c.Locations[1].Biome == connection.Locations[1].Biome) || (c.Locations[1].Biome == connection.Locations[0].Biome && c.Locations[0].Biome == connection.Locations[1].Biome)))
						{
							Location gateLocation = connection.Locations[0].IsGateBetweenBiomes ? connection.Locations[0] : connection.Locations[1];
							EventPrefab unlockEvent = EventPrefab.GetUnlockPathEvent(gateLocation.LevelData.Biome.Identifier, gateLocation.Faction);
							if (unlockEvent != null)
							{
								if (unlockEvent.Faction.IsEmpty)
								{
									if (location == null)
									{
										continue;
									}
									if (gateLocation != location)
									{
										continue;
									}
								}
								else if (faction == null || faction.Prefab.Identifier != unlockEvent.Faction)
								{
									continue;
								}
								if (unlockEvent != null)
								{
									Reputation unlockReputation = gateLocation.Reputation;
									if (!unlockEvent.Faction.IsEmpty)
									{
										Faction unlockFaction = GameMain.GameSession.Campaign.Factions.Find((Faction f) => f.Prefab.Identifier == unlockEvent.Faction);
										unlockReputation = ((unlockFaction != null) ? unlockFaction.Reputation : null);
									}
									float normalizedUnlockReputation = MathUtils.InverseLerp((float)unlockReputation.MinReputation, (float)unlockReputation.MaxReputation, (float)unlockEvent.UnlockPathReputation);
									string tag = "lockedpathreputationrequirement";
									ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
									array[0] = new ValueTuple<string, LocalizedString>("[reputation]", Reputation.GetFormattedReputationText(normalizedUnlockReputation, (float)unlockEvent.UnlockPathReputation, true));
									int num = 1;
									string item = "[biomename]";
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
									defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.orange‖");
									defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(connection.LevelData.Biome.DisplayName);
									defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
									array[num] = new ValueTuple<string, LocalizedString>(item, defaultInterpolatedStringHandler.ToStringAndClear());
									RichString unlockText = RichString.Rich(TextManager.GetWithVariables(tag, array), null);
									GUITextBlock unlockInfoPanel = new GUITextBlock(new RectTransform(new Vector2(0.8f, 0f), reputationFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
									{
										MinSize = new Point(0, GUI.IntScale(30f)),
										AbsoluteOffset = new Point(0, GUI.IntScale(3f))
									}, unlockText, new Color?(GUIStyle.TextColorNormal), null, Alignment.Center, false, "GUIButtonRound", null);
									unlockInfoPanel.Color = Color.Lerp(unlockInfoPanel.Color, Color.Black, 0.8f);
									unlockInfoPanel.UserData = "unlockinfo";
									if (unlockInfoPanel.TextSize.X > (float)unlockInfoPanel.Rect.Width * 0.7f)
									{
										unlockInfoPanel.Font = GUIStyle.SmallFont;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0400082E RID: 2094
		private float crewListAnimDelay = 0.25f;

		// Token: 0x0400082F RID: 2095
		private float missionIconAnimDelay;

		// Token: 0x04000830 RID: 2096
		private const float JobColumnWidthPercentage = 0.05f;

		// Token: 0x04000831 RID: 2097
		private const float CharacterColumnWidthPercentage = 0.35f;

		// Token: 0x04000832 RID: 2098
		private const float StatusColumnWidthPercentage = 0.25f;

		// Token: 0x04000833 RID: 2099
		private const float KillColumnWidthPercentage = 0.05f;

		// Token: 0x04000834 RID: 2100
		private const float DeathColumnWidthPercentage = 0.05f;

		// Token: 0x04000835 RID: 2101
		private int jobColumnWidth;

		// Token: 0x04000836 RID: 2102
		private int characterColumnWidth;

		// Token: 0x04000837 RID: 2103
		private int statusColumnWidth;

		// Token: 0x04000838 RID: 2104
		private int killColumnWidth;

		// Token: 0x04000839 RID: 2105
		private int deathColumnWidth;

		// Token: 0x0400083A RID: 2106
		private readonly List<Mission> selectedMissions;

		// Token: 0x0400083B RID: 2107
		private readonly Location startLocation;

		// Token: 0x0400083C RID: 2108
		private readonly Location endLocation;

		// Token: 0x0400083D RID: 2109
		private readonly GameMode gameMode;

		// Token: 0x0400083E RID: 2110
		private readonly Dictionary<Identifier, float> initialFactionReputations = new Dictionary<Identifier, float>();

		// Token: 0x04000842 RID: 2114
		private readonly Dictionary<CharacterInfo, int> killCounts = new Dictionary<CharacterInfo, int>();

		// Token: 0x04000843 RID: 2115
		private readonly Dictionary<CharacterInfo, int> deathCounts = new Dictionary<CharacterInfo, int>();
	}
}
