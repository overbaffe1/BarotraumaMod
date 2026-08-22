using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000078 RID: 120
	[NullableContext(2)]
	[Nullable(0)]
	internal class DeathPrompt
	{
		// Token: 0x06001110 RID: 4368 RVA: 0x000A6E42 File Offset: 0x000A5042
		private DeathPrompt()
		{
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x000A6E4C File Offset: 0x000A504C
		public static void Create(float delay)
		{
			if (!RespawnManager.UseDeathPrompt)
			{
				return;
			}
			if (GameMain.GameSession.DeathPrompt != null)
			{
				return;
			}
			if (DeathPrompt.createPromptCoroutine != null && CoroutineManager.IsCoroutineRunning(DeathPrompt.createPromptCoroutine))
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null || !gameSession.IsRunning)
			{
				return;
			}
			DeathPrompt.createPromptCoroutine = CoroutineManager.Invoke(delegate
			{
				if (GameMain.GameSession != null)
				{
					GameMain.GameSession.DeathPrompt = new DeathPrompt();
					GameMain.GameSession.DeathPrompt.CreatePrompt();
					SoundPlayer.OverrideMusicType = "crewdead".ToIdentifier();
					SoundPlayer.OverrideMusicDuration = new float?(25f);
				}
			}, delay);
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x000A6EBE File Offset: 0x000A50BE
		public void AddToGUIUpdateList()
		{
			GUIComponent guicomponent = this.content;
			if (guicomponent == null)
			{
				return;
			}
			guicomponent.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x000A6ED4 File Offset: 0x000A50D4
		private void CreatePrompt()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			bool flag;
			if (networkMember != null)
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				if (serverSettings != null)
				{
					flag = (serverSettings.RespawnMode == RespawnMode.Permadeath);
					goto IL_25;
				}
			}
			flag = false;
			IL_25:
			bool permadeath = flag;
			networkMember = GameMain.NetworkMember;
			bool flag2;
			if (networkMember != null)
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				if (serverSettings != null)
				{
					flag2 = serverSettings.IronmanModeActive;
					goto IL_48;
				}
			}
			flag2 = false;
			IL_48:
			bool ironman = flag2;
			GUICustomComponent background = new GUICustomComponent(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawBackground), null)
			{
				UserData = this
			};
			background.FadeIn(0f, 5f, false);
			GUIImage foreground = new GUIImage(new RectTransform(new Vector2(1f, GUI.RelativeHorizontalAspectRatio), background.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(0, GUI.IntScale(-20f))
			}, "DeathScreenForeground", GUIImage.ScalingMode.None)
			{
				Color = Color.White
			};
			foreground.FadeIn(0f, 5f, false);
			foreground.Pulsate(Vector2.One, Vector2.One * 0.8f, 25f);
			this.deathPromptFrame = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.3f), background.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null)
			{
				UserData = this
			};
			this.deathPromptFrame.FadeIn(0f, 1f, false);
			RectTransform rectTransform = new RectTransform(new Vector2(0.5f, 0.1f), background.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0f, 0.2f);
			RichString text = string.Empty;
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectTransform, text, null, largeFont, Alignment.TopCenter, false, "", null).TextGetter = delegate()
			{
				if (GameMain.Client.EndRoundTimeRemaining <= 0f)
				{
					return string.Empty;
				}
				return TextManager.GetWithVariable("endinground", "[time]", ToolBox.SecondsToReadableTime(GameMain.Client.EndRoundTimeRemaining), FormatCapitals.No).Fallback(ToolBox.SecondsToReadableTime(GameMain.Client.EndRoundTimeRemaining), false);
			};
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), this.deathPromptFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("deathprompt.header");
			largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text2, null, largeFont, Alignment.Center, false, "", null).FadeIn(0f, 1f, false);
			GameClient client = GameMain.Client;
			CauseOfDeath causeOfDeath2;
			if (client == null)
			{
				causeOfDeath2 = null;
			}
			else
			{
				Character character = client.Character;
				causeOfDeath2 = ((character != null) ? character.CauseOfDeath : null);
			}
			CauseOfDeath causeOfDeath = causeOfDeath2;
			if (causeOfDeath != null && causeOfDeath.Type != CauseOfDeathType.Unknown)
			{
				LocalizedString causeOfDeathDescription = (causeOfDeath.Affliction != null) ? causeOfDeath.Affliction.SelfCauseOfDeathDescription : TextManager.Get(new string[]
				{
					"Self_CauseOfDeathDescription." + causeOfDeath.Type.ToString(),
					"Self_CauseOfDeathDescription.Damage"
				});
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), causeOfDeathDescription, null, null, Alignment.Left, false, "", null).FadeIn(2f, 1f, false);
			}
			if (permadeath)
			{
				if (ironman)
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.permadeathnotification") + "\n\n" + TextManager.Get("deathprompt.ironmanexplanation"), null, null, Alignment.Left, true, "", null).FadeIn(3f, 1f, false);
				}
				else
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.permadeathnotification") + '\n' + TextManager.Get("deathprompt.takeoverbotexplanation"), null, null, Alignment.Left, true, "", null).FadeIn(3f, 1f, false);
				}
			}
			else if (RespawnManager.SkillLossPercentageOnDeath > 0f)
			{
				string skillLossAmount = ((int)RespawnManager.SkillLossPercentageOnDeath).ToString();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color: ");
				defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.Red.ToStringHex());
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted(skillLossAmount);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
				string skillLossText = defaultInterpolatedStringHandler.ToStringAndClear();
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(TextManager.GetWithVariable("respawnskillpenalty", "[percentage]", skillLossText, FormatCapitals.No), null), null, null, Alignment.Left, false, "", null).FadeIn(3f, 1f, false);
			}
			GUILayoutGroup decisionButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			if (ironman)
			{
				GUILayoutGroup buttonContainerMiddle = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), decisionButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
				new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainerMiddle.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("spectatebutton"), Alignment.Center, "", null)
				{
					OnClicked = delegate(GUIButton btn, object userdata)
					{
						GameClient client2 = GameMain.Client;
						if (client2 != null)
						{
							client2.SendRespawnPromptResponse(true);
						}
						this.Close();
						return true;
					}
				}.FadeIn(4f, 1f, true);
			}
			else
			{
				GUILayoutGroup buttonContainerLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), decisionButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				GUILayoutGroup buttonContainerRight = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), decisionButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				new GUIButton(new RectTransform(new Vector2(1f, 1f), buttonContainerLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("respawnquestionpromptwait"), Alignment.Center, "", null)
				{
					OnClicked = delegate(GUIButton btn, object userdata)
					{
						GameClient client2 = GameMain.Client;
						if (client2 != null)
						{
							client2.SendRespawnPromptResponse(true);
						}
						this.Close();
						return true;
					}
				}.FadeIn(4f, 1f, true);
				if (permadeath)
				{
					if (GameMain.Client != null && GameMain.Client.ServerSettings.AllowBotTakeoverOnPermadeath)
					{
						GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 1f), buttonContainerRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.takeoverbot"), Alignment.Center, "", null);
						guibutton.Enabled = false;
						guibutton.OnAddedToGUIUpdateList = delegate(GUIComponent component)
						{
							component.Enabled = DeathPrompt.GetAvailableBots().Any<CharacterInfo>();
						};
						guibutton.OnClicked = delegate(GUIButton btn, object userdata)
						{
							if (this.takeOverBotPanel == null)
							{
								DeathPrompt.CreateTakeOverBotPanel(this.deathPromptFrame, this);
							}
							else
							{
								GUIComponent parent = this.takeOverBotPanel.Parent;
								if (parent != null)
								{
									parent.RemoveChild(this.takeOverBotPanel);
								}
								this.takeOverBotPanel = null;
							}
							return true;
						};
						guibutton.FadeIn(4f, 1f, true);
					}
				}
				else
				{
					GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(1f, 1f), buttonContainerRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.respawnnow"), Alignment.Center, "", null);
					guibutton2.OnClicked = delegate(GUIButton btn, object userdata)
					{
						GameClient client2 = GameMain.Client;
						if (client2 != null)
						{
							client2.SendRespawnPromptResponse(false);
						}
						this.Close();
						return true;
					};
					networkMember = GameMain.NetworkMember;
					bool enabled;
					if (networkMember != null)
					{
						ServerSettings serverSettings = networkMember.ServerSettings;
						if (serverSettings != null)
						{
							enabled = (serverSettings.RespawnMode == RespawnMode.MidRound);
							goto IL_9F4;
						}
					}
					enabled = false;
					IL_9F4:
					guibutton2.Enabled = enabled;
					GUIButton respawnNowButton = guibutton2;
					networkMember = GameMain.NetworkMember;
					if (networkMember != null)
					{
						ServerSettings serverSettings = networkMember.ServerSettings;
						if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.BetweenRounds)
						{
							respawnNowButton.ToolTip = TextManager.Get("respawnnotavailable.respawnmode.betweenrounds");
						}
					}
					respawnNowButton.FadeIn(4f, 1f, true);
				}
				GUILayoutGroup infoButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopRight)
				{
					Stretch = true,
					RelativeSpacing = 0.025f
				};
				if (permadeath)
				{
					if (Level.IsLoadedFriendlyOutpost)
					{
						new GUIButton(new RectTransform(new Vector2(0.6f, 1f), infoButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("npctitle.hrmanager"), Alignment.Center, "GUIButtonSmall", null)
						{
							OnClicked = delegate(GUIButton btn, object userdata)
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
								this.Close();
								return true;
							}
						}.FadeIn(5f, 1f, true);
					}
				}
				else
				{
					new GUIButton(new RectTransform(new Vector2(0.6f, 1f), infoButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.showskills"), Alignment.Center, "GUIButtonSmall", null)
					{
						OnClicked = delegate(GUIButton btn, object userdata)
						{
							if (this.skillPanel == null)
							{
								GUIComponent parent = this.deathPromptFrame;
								GameClient client2 = GameMain.Client;
								CharacterInfo characterInfo;
								if (client2 == null)
								{
									characterInfo = null;
								}
								else
								{
									Character character2 = client2.Character;
									characterInfo = ((character2 != null) ? character2.Info : null);
								}
								CharacterInfo characterInfo2;
								if ((characterInfo2 = characterInfo) == null)
								{
									GameClient client3 = GameMain.Client;
									characterInfo2 = ((client3 != null) ? client3.CharacterInfo : null);
								}
								this.CreateSkillPanel(parent, characterInfo2);
							}
							else
							{
								GUIComponent parent2 = this.skillPanel.Parent;
								if (parent2 != null)
								{
									parent2.RemoveChild(this.skillPanel);
								}
								this.skillPanel = null;
							}
							return true;
						}
					}.FadeIn(5f, 1f, true);
					new GUIButton(new RectTransform(new Vector2(0.6f, 1f), infoButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("deathprompt.newcharacter"), Alignment.Center, "GUIButtonSmall", null)
					{
						OnClicked = delegate(GUIButton btn, object userdata)
						{
							if (this.newCharacterPanel == null)
							{
								this.CreateNewCharacterPanel(this.deathPromptFrame);
							}
							else
							{
								GUIComponent parent = this.newCharacterPanel.Parent;
								if (parent != null)
								{
									parent.RemoveChild(this.newCharacterPanel);
								}
								this.newCharacterPanel = null;
							}
							return true;
						}
					}.FadeIn(5f, 1f, true);
				}
			}
			this.content = background;
		}

		// Token: 0x06001114 RID: 4372 RVA: 0x000A7B10 File Offset: 0x000A5D10
		[NullableContext(1)]
		private void CreateSkillPanel(GUIComponent parent, [Nullable(2)] CharacterInfo characterInfo)
		{
			if (characterInfo == null)
			{
				return;
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 1f), parent.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.8f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			GUILayoutGroup middleColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("Skills");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock leftHeader = new GUITextBlock(rectT, text, new Color?(GUIStyle.TextColorBright), subHeadingFont, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), middleColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("deathprompt.SkillsLostHeader");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock middleHeader = new GUITextBlock(rectT2, text2, new Color?(GUIStyle.TextColorBright), subHeadingFont, Alignment.Left, false, "", null);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("deathprompt.respawnnow");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock rightHeader = new GUITextBlock(rectT3, text3, new Color?(GUIStyle.TextColorBright), subHeadingFont, Alignment.Left, false, "", null);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				leftHeader,
				middleHeader,
				rightHeader
			});
			foreach (Skill skill in from s in characterInfo.Job.GetSkills()
			orderby s.Level descending
			select s)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), skill.DisplayName, null, null, Alignment.Left, false, "", null);
				int previousSkill = (int)skill.HighestLevelDuringRound;
				int reducedSkill = (int)RespawnManager.GetReducedSkill(characterInfo, skill, RespawnManager.SkillLossPercentageOnDeath, null);
				int reducedSkillOnImmediateRespawn = (int)RespawnManager.GetReducedSkill(characterInfo, skill, RespawnManager.SkillLossPercentageOnImmediateRespawn, new float?((float)reducedSkill));
				int skillLoss = reducedSkill - previousSkill;
				int skillLossOnImmediateRespawn = reducedSkillOnImmediateRespawn - previousSkill;
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), middleColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
				defaultInterpolatedStringHandler.AppendFormatted<int>(reducedSkill);
				defaultInterpolatedStringHandler.AppendLiteral(" (‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.Red.ToStringHex());
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<int>(skillLoss);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖)");
				new GUITextBlock(rectT4, RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear(), null), null, null, Alignment.Left, false, "", null);
				RectTransform rectT5 = new RectTransform(new Vector2(1f, 0f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 3);
				defaultInterpolatedStringHandler2.AppendFormatted<int>(reducedSkillOnImmediateRespawn);
				defaultInterpolatedStringHandler2.AppendLiteral(" (‖color:");
				defaultInterpolatedStringHandler2.AppendFormatted(GUIStyle.Red.ToStringHex());
				defaultInterpolatedStringHandler2.AppendLiteral("‖");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(skillLossOnImmediateRespawn);
				defaultInterpolatedStringHandler2.AppendLiteral("‖end‖)");
				new GUITextBlock(rectT5, RichString.Rich(defaultInterpolatedStringHandler2.ToStringAndClear(), null), null, null, Alignment.Left, false, "", null);
			}
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 0.15f), leftColumn.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.IgnoreLayoutGroups = true;
			guibutton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				GUIComponent parent2 = frame.Parent;
				if (parent2 != null)
				{
					parent2.RemoveChild(frame);
				}
				this.skillPanel = null;
				return true;
			};
			this.skillPanel = frame;
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x000A8170 File Offset: 0x000A6370
		[NullableContext(1)]
		private void CreateNewCharacterPanel(GUIComponent parent)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 1.5f), parent.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			GameMain.NetLobbyScreen.CreatePlayerFrame(content, false, true);
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				GUIComponent parent2 = frame.Parent;
				if (parent2 != null)
				{
					parent2.RemoveChild(frame);
				}
				this.newCharacterPanel = null;
				return true;
			};
			Action <>9__2;
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ApplySettingsYes"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				Action onYes;
				if ((onYes = <>9__2) == null)
				{
					onYes = (<>9__2 = delegate()
					{
						GameClient client = GameMain.Client;
						if (client != null)
						{
							client.SendCharacterInfo(GameMain.Client.PendingName);
						}
						GameMain.NetLobbyScreen.CampaignCharacterDiscarded = false;
						GUIComponent parent2 = frame.Parent;
						if (parent2 != null)
						{
							parent2.RemoveChild(frame);
						}
						this.newCharacterPanel = null;
					});
				}
				netLobbyScreen.TryDiscardCampaignCharacter(onYes);
				return true;
			};
			this.newCharacterPanel = frame;
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x000A836C File Offset: 0x000A656C
		public static void CreateTakeOverBotPanel()
		{
			GUIFrame panelHolder = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.3f), GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			GUIComponent takeOverBotPanel = DeathPrompt.CreateTakeOverBotPanel(panelHolder, null);
			if (takeOverBotPanel != null)
			{
				takeOverBotPanel.RectTransform.SetPosition(Anchor.Center, null);
				GUIMessageBox.MessageBoxes.Add(panelHolder);
			}
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x000A83F0 File Offset: 0x000A65F0
		private static GUIComponent CreateTakeOverBotPanel([Nullable(1)] GUIComponent parent, DeathPrompt deathPrompt)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) == null)
			{
				return null;
			}
			GameSession gameSession2 = GameMain.GameSession;
			MultiPlayerCampaign campaign = ((gameSession2 != null) ? gameSession2.Campaign : null) as MultiPlayerCampaign;
			if (campaign == null)
			{
				return null;
			}
			if (campaign.CampaignUI == null)
			{
				campaign.InitCampaignUI();
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 1f), parent.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), "", null);
			DeathPrompt.takeOverBotPanelFrame = frame;
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			GUIListBox botList = new GUIListBox(new RectTransform(new Vector2(1f, 0.9f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			foreach (CharacterInfo c in DeathPrompt.GetAvailableBots())
			{
				CampaignUI campaignUI = campaign.CampaignUI;
				GUIComponent characterFrame = (campaignUI != null) ? campaignUI.HRManagerUI.CreateCharacterFrame(c, botList, true) : null;
				if (characterFrame != null)
				{
					characterFrame.UserData = c;
				}
			}
			botList.UpdateScrollBarSize();
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				GUIMessageBox.MessageBoxes.Remove(frame.Parent);
				GUIComponent parent2 = frame.Parent;
				if (parent2 != null)
				{
					parent2.RemoveChild(frame);
				}
				if (deathPrompt != null)
				{
					deathPrompt.takeOverBotPanel = null;
				}
				return true;
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("inputtype.select"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.Enabled = false;
			guibutton.OnAddedToGUIUpdateList = delegate(GUIComponent component)
			{
				component.Enabled = (botList.SelectedData is CharacterInfo);
			};
			guibutton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				CharacterInfo selectedCharacter = botList.SelectedData as CharacterInfo;
				if (selectedCharacter != null)
				{
					GameClient client = GameMain.Client;
					if (client != null)
					{
						if (!DeathPrompt.GetAvailableBots().Contains(selectedCharacter))
						{
							DeathPrompt.CreateTakeOverBotPanel(frame, deathPrompt);
							return true;
						}
						client.SendTakeOverBotRequest(selectedCharacter);
						GUIMessageBox.MessageBoxes.Remove(frame.Parent);
						DeathPrompt deathPrompt2 = deathPrompt;
						if (deathPrompt2 != null)
						{
							deathPrompt2.Close();
						}
						return true;
					}
				}
				DebugConsole.ThrowError("Conditions for sending bot takeover request not met", null, null, false, false);
				return false;
			};
			if (deathPrompt != null)
			{
				deathPrompt.takeOverBotPanel = frame;
			}
			return frame;
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x000A8730 File Offset: 0x000A6930
		public void UpdateBotList()
		{
			if (this.deathPromptFrame != null && DeathPrompt.takeOverBotPanelFrame != null)
			{
				DeathPrompt.CloseBotPanel();
				DeathPrompt.CreateTakeOverBotPanel(this.deathPromptFrame, this);
			}
		}

		// Token: 0x06001119 RID: 4377 RVA: 0x000A8754 File Offset: 0x000A6954
		[NullableContext(1)]
		private static IEnumerable<CharacterInfo> GetAvailableBots()
		{
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			if (crewManager != null)
			{
				return crewManager.GetCharacterInfos(true).Where(delegate(CharacterInfo c)
				{
					if (!c.IsOnReserveBench)
					{
						if (c.Character != null)
						{
							Character character = c.Character;
							if (character != null && character.IsBot && !character.IsDead)
							{
								return true;
							}
						}
						return c.Character == null && c.IsNewHire;
					}
					return true;
				});
			}
			return Enumerable.Empty<CharacterInfo>();
		}

		// Token: 0x0600111A RID: 4378 RVA: 0x000A87A8 File Offset: 0x000A69A8
		[NullableContext(1)]
		private void DrawBackground(SpriteBatch spriteBatch, GUICustomComponent guiCustomComponent)
		{
			GUIComponentStyle background = GUIStyle.GetComponentStyle("DeathScreenBackground");
			if (background != null)
			{
				GUI.DrawBackgroundSprite(spriteBatch, background.GetDefaultSprite(), Color.White * ((float)guiCustomComponent.Color.A / 255f), null, SpriteEffects.None);
			}
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x000A87F8 File Offset: 0x000A69F8
		public void Close()
		{
			if (GameMain.GameSession != null)
			{
				GameMain.GameSession.DeathPrompt = null;
			}
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x000A880C File Offset: 0x000A6A0C
		public static void CloseBotPanel()
		{
			GUIComponent frame = DeathPrompt.takeOverBotPanelFrame;
			if (frame != null)
			{
				GUIMessageBox.MessageBoxes.Remove(frame.Parent);
				GUIComponent parent = frame.Parent;
				if (parent != null)
				{
					parent.RemoveChild(frame);
				}
			}
			DeathPrompt.takeOverBotPanelFrame = null;
		}

		// Token: 0x04000888 RID: 2184
		private static CoroutineHandle createPromptCoroutine;

		// Token: 0x04000889 RID: 2185
		private GUIFrame deathPromptFrame;

		// Token: 0x0400088A RID: 2186
		private GUIComponent skillPanel;

		// Token: 0x0400088B RID: 2187
		private GUIComponent newCharacterPanel;

		// Token: 0x0400088C RID: 2188
		private GUIComponent takeOverBotPanel;

		// Token: 0x0400088D RID: 2189
		private GUIComponent content;

		// Token: 0x0400088E RID: 2190
		private static GUIComponent takeOverBotPanelFrame;
	}
}
