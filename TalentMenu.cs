using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000BB RID: 187
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class TalentMenu
	{
		// Token: 0x06001733 RID: 5939 RVA: 0x000DF9B8 File Offset: 0x000DDBB8
		public void CreateGUI(GUIFrame parent, [Nullable(2)] CharacterInfo characterInfo)
		{
			this.characterInfo = characterInfo;
			this.character = ((characterInfo != null) ? characterInfo.Character : null);
			parent.ClearChildren();
			this.talentButtons.Clear();
			this.talentShowCaseButtons.Clear();
			this.talentCornerIcons.Clear();
			this.showCaseTalentFrames.Clear();
			GUIFrame background = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
			int padding = GUI.IntScale(15f);
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(background.Rect.Width - padding, background.Rect.Height - padding), parent.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null);
			GUIFrame content = new GUIFrame(new RectTransform(new Vector2(0.98f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup contentLayout = new GUILayoutGroup(new RectTransform(Vector2.One, content.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				AbsoluteSpacing = GUI.IntScale(10f),
				Stretch = true
			};
			if (characterInfo == null)
			{
				return;
			}
			this.CreateStatPanel(contentLayout, characterInfo);
			new GUIFrame(new RectTransform(new Vector2(1f, 1f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			TalentTree talentTree;
			if (TalentTree.JobTalentTrees.TryGet(characterInfo.Job.Prefab.Identifier, out talentTree))
			{
				this.CreateTalentMenu(contentLayout, characterInfo, talentTree);
			}
			this.CreateFooter(contentLayout, characterInfo);
			this.UpdateTalentInfo();
			if (GameMain.NetworkMember != null && TalentMenu.IsOwnCharacter(characterInfo))
			{
				this.CreateMultiplayerCharacterSettings(frame, content);
			}
		}

		// Token: 0x06001734 RID: 5940 RVA: 0x000DFBEC File Offset: 0x000DDDEC
		private void CreateMultiplayerCharacterSettings(GUIComponent parent, GUIComponent content)
		{
			TalentMenu.<>c__DisplayClass29_0 CS$<>8__locals1 = new TalentMenu.<>c__DisplayClass29_0();
			CS$<>8__locals1.content = content;
			CS$<>8__locals1.<>4__this = this;
			if (this.skillLayout == null)
			{
				return;
			}
			CS$<>8__locals1.characterSettingsFrame = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				Visible = false
			};
			GUILayoutGroup characterLayout = new GUILayoutGroup(new RectTransform(Vector2.One, CS$<>8__locals1.characterSettingsFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame containerFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), characterLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup playerFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), containerFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GameMain.NetLobbyScreen.CreatePlayerFrame(playerFrame, false, true);
			if (!GameMain.NetLobbyScreen.PermadeathMode)
			{
				GameSession gameSession = GameMain.GameSession;
				if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
				{
					GUIButton newCharacterBox = new GUIButton(new RectTransform(new Vector2(0.5f, 0.2f), this.skillLayout.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), GameMain.NetLobbyScreen.CampaignCharacterDiscarded ? TextManager.Get("settings") : TextManager.Get("createnew"), Alignment.Center, "GUIButtonSmall", null)
					{
						IgnoreLayoutGroups = false,
						TextBlock = 
						{
							AutoScaleHorizontal = true
						}
					};
					Action <>9__1;
					newCharacterBox.OnClicked = delegate(GUIButton button, object o)
					{
						if (!GameMain.NetLobbyScreen.CampaignCharacterDiscarded)
						{
							NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
							Action onYes;
							if ((onYes = <>9__1) == null)
							{
								onYes = (<>9__1 = delegate()
								{
									newCharacterBox.Text = TextManager.Get("settings");
									if (TabMenu.PendingChangesFrame != null)
									{
										NetLobbyScreen.CreateChangesPendingFrame(TabMenu.PendingChangesFrame);
									}
									CS$<>8__locals1.<CreateMultiplayerCharacterSettings>g__OpenMenu|2();
								});
							}
							netLobbyScreen.TryDiscardCampaignCharacter(onYes);
							return true;
						}
						CS$<>8__locals1.<CreateMultiplayerCharacterSettings>g__OpenMenu|2();
						return true;
					};
					goto IL_2E4;
				}
			}
			if (this.characterInfo != null)
			{
				this.renameButton = new GUIButton(new RectTransform(new Vector2(0.5f, 0.2f), this.skillLayout.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("button.RenameCharacter"), Alignment.Center, "GUIButtonSmall", null)
				{
					Enabled = this.characterInfo.RenamingEnabled,
					ToolTip = TextManager.Get("permadeath.rename.description"),
					IgnoreLayoutGroups = false,
					TextBlock = 
					{
						AutoScaleHorizontal = true
					},
					OnClicked = delegate(GUIButton _, object _)
					{
						CS$<>8__locals1.<>4__this.CreateRenamePopup();
						return true;
					}
				};
			}
			IL_2E4:
			GUILayoutGroup characterCloseButtonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), characterLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.BottomCenter);
			new GUIButton(new RectTransform(new Vector2(0.4f, 1f), characterCloseButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ApplySettingsButton"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.SendCharacterInfo(GameMain.Client.PendingName);
				}
				GameMain.NetLobbyScreen.CampaignCharacterDiscarded = false;
				CS$<>8__locals1.characterSettingsFrame.Visible = false;
				CS$<>8__locals1.content.Visible = true;
				return true;
			};
		}

		// Token: 0x06001735 RID: 5941 RVA: 0x000DFF8C File Offset: 0x000DE18C
		private void CreateRenamePopup()
		{
			TalentMenu.<>c__DisplayClass30_0 CS$<>8__locals1 = new TalentMenu.<>c__DisplayClass30_0();
			CS$<>8__locals1.<>4__this = this;
			TalentMenu.<>c__DisplayClass30_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("button.RenameCharacter");
			RichString text2 = TextManager.Get("permadeath.rename.description");
			LocalizedString[] buttons = new LocalizedString[]
			{
				TextManager.Get("Confirm"),
				TextManager.Get("Cancel")
			};
			Point? point = new Point?(new Point(0, GUI.IntScale(230f)));
			CS$<>8__locals2.renamePopup = new GUIMessageBox(headerText, text2, buttons, null, point, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			TalentMenu.<>c__DisplayClass30_0 CS$<>8__locals3 = CS$<>8__locals1;
			Vector2 one = Vector2.One;
			RectTransform rectTransform = CS$<>8__locals1.renamePopup.Content.RectTransform;
			Anchor anchor = Anchor.TopLeft;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			GUITextBox guitextBox = new GUITextBox(new RectTransform(one, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			guitextBox.OnEnterPressed = delegate(GUITextBox textBox, string text)
			{
				textBox.Text = text.Trim();
				return true;
			};
			CS$<>8__locals3.newNameBox = guitextBox;
			GUIButton guibutton = CS$<>8__locals1.renamePopup.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object _)
			{
				string text3 = CS$<>8__locals1.newNameBox.Text;
				string newName = (text3 != null) ? text3.Trim() : null;
				if (newName == null || !(newName != ""))
				{
					CS$<>8__locals1.newNameBox.Flash(null, 1.5f, false, false, null);
					return false;
				}
				if (CS$<>8__locals1.<>4__this.characterInfo == null)
				{
					DebugConsole.ThrowError("Tried to rename character, but CharacterInfo completely missing!", null, null, false, false);
					return true;
				}
				if (CS$<>8__locals1.newNameBox.Text == CS$<>8__locals1.<>4__this.characterInfo.Name)
				{
					CS$<>8__locals1.renamePopup.Close();
					return true;
				}
				GameSession gameSession = GameMain.GameSession;
				HRManagerUI hrmanagerUI;
				if (gameSession == null)
				{
					hrmanagerUI = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					if (campaign == null)
					{
						hrmanagerUI = null;
					}
					else
					{
						CampaignUI campaignUI = campaign.CampaignUI;
						hrmanagerUI = ((campaignUI != null) ? campaignUI.HRManagerUI : null);
					}
				}
				HRManagerUI crewManagement = hrmanagerUI;
				if (crewManagement != null)
				{
					crewManagement.RenameCharacter(CS$<>8__locals1.<>4__this.characterInfo, newName);
					if (CS$<>8__locals1.<>4__this.nameBlock != null)
					{
						CS$<>8__locals1.<>4__this.nameBlock.Text = newName;
					}
					if (CS$<>8__locals1.<>4__this.renameButton != null)
					{
						CS$<>8__locals1.<>4__this.renameButton.Enabled = false;
					}
					CS$<>8__locals1.renamePopup.Close();
				}
				return true;
			}));
			GUIButton guibutton2 = CS$<>8__locals1.renamePopup.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(CS$<>8__locals1.renamePopup.Close));
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x000E0114 File Offset: 0x000DE314
		private void CreateStatPanel(GUIComponent parent, CharacterInfo info)
		{
			Job job = info.Job;
			GUILayoutGroup topLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.3f), parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUICustomComponent(new RectTransform(new Vector2(0.25f, 1f), topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch batch, GUICustomComponent component)
			{
				info.DrawIcon(batch, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null);
			GUILayoutGroup nameLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f),
				CanBeFocused = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), nameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = info.Name;
			GUIFont font = GUIStyle.SubHeadingFont;
			this.nameBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			if (!info.OmitJobInMenus)
			{
				this.nameBlock.TextColor = job.Prefab.UIColor;
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), nameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = job.Name;
				font = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null).TextColor = job.Prefab.UIColor;
			}
			if (info.PersonalityTrait != null)
			{
				LocalizedString traitString = TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("PersonalityTrait"),
					info.PersonalityTrait.DisplayName
				});
				Vector2 traitSize = GUIStyle.SmallFont.MeasureString(traitString, false);
				RectTransform rectT3 = new RectTransform(Vector2.One, nameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = traitString;
				font = GUIStyle.SmallFont;
				GUITextBlock traitBlock = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
				traitBlock.RectTransform.NonScaledSize = traitSize.Pad(traitBlock.Padding).ToPoint();
			}
			ImmutableHashSet<TalentPrefab> talentsOutsideTree = (from e in info.GetUnlockedTalentsOutsideTree()
			select TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == e)).ToImmutableHashSet<TalentPrefab>();
			if (talentsOutsideTree.Any((TalentPrefab t) => t != null && !t.IsHiddenExtraTalent))
			{
				new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), nameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				GUILayoutGroup extraTalentLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.55f), nameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.3f), extraTalentLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
				RichString text4 = TextManager.Get("talentmenu.extratalents");
				font = GUIStyle.SubHeadingFont;
				this.talentPointText = new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null)
				{
					AutoScaleVertical = true
				};
				this.talentPointText.RectTransform.MaxSize = new Point(int.MaxValue, (int)this.talentPointText.TextSize.Y);
				GUIListBox extraTalentList = new GUIListBox(new RectTransform(new Vector2(0.9f, 0.7f), extraTalentLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, null, "", true, false)
				{
					AutoHideScrollBar = false,
					ResizeContentToMakeSpaceForScrollBar = false
				};
				extraTalentList.ScrollBar.RectTransform.SetPosition(Anchor.BottomCenter, new Pivot?(Pivot.TopCenter));
				extraTalentLayout.Recalculate();
				extraTalentList.ForceLayoutRecalculation();
				foreach (TalentPrefab extraTalent in talentsOutsideTree)
				{
					if (extraTalent != null && !extraTalent.IsHiddenExtraTalent)
					{
						GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, extraTalentList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), extraTalent.Icon, true, null);
						guiimage.ToolTip = TalentMenu.GetTalentTooltip(extraTalent, this.characterInfo);
						guiimage.Color = GUIStyle.Green;
					}
				}
			}
			this.skillLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 1f), topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f),
				Stretch = true
			};
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0f), this.skillLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("skills");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock skillBlock = new GUITextBlock(rectT5, text5, null, font, Alignment.Left, false, "", null);
			this.skillListBox = new GUIListBox(new RectTransform(new Vector2(1f, 1f - skillBlock.RectTransform.RelativeSize.Y), this.skillLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false);
			TabMenu.CreateSkillList(info.Character, info, this.skillListBox);
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x000E0858 File Offset: 0x000DEA58
		private void CreateTalentMenu(GUIComponent parent, CharacterInfo info, TalentTree tree)
		{
			this.talentMainArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), null, null);
			TalentMenu.<>c__DisplayClass32_0 CS$<>8__locals1;
			CS$<>8__locals1.mainList = new GUIListBox(new RectTransform(Vector2.One, this.talentMainArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			this.startAnimation = TalentMenu.CreatePopupAnimationHandler(this.talentMainArea);
			if (info != null && info.TalentRefundPoints > 0 && info.ShowTalentResetPopupOnOpen)
			{
				this.CreateTalentResetPopup(this.talentMainArea);
			}
			this.selectedTalents = info.GetUnlockedTalentsInTree().ToHashSet<Identifier>();
			int specializationCount = tree.TalentSubTrees.Count((TalentSubTree t) => t.Type == TalentTreeType.Specialization);
			List<GUITextBlock> subTreeNames = new List<GUITextBlock>();
			ImmutableArray<TalentSubTree>.Enumerator enumerator = tree.TalentSubTrees.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TalentSubTree subTree = enumerator.Current;
				TalentTreeType type = subTree.Type;
				GUIListBox talentList;
				Vector2 treeSize;
				if (type != TalentTreeType.Specialization)
				{
					if (type != TalentTreeType.Primary)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid TalentTreeType \"");
						defaultInterpolatedStringHandler.AppendFormatted<TalentTreeType>(subTree.Type);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						throw new ArgumentOutOfRangeException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					talentList = CS$<>8__locals1.mainList;
					treeSize = new Vector2(1f, 0.5f);
				}
				else
				{
					talentList = TalentMenu.<CreateTalentMenu>g__GetSpecializationList|32_2(ref CS$<>8__locals1);
					treeSize = new Vector2(Math.Max(0.333f, 1f / (float)tree.TalentSubTrees.Count((TalentSubTree t) => t.Type == TalentTreeType.Specialization)), 1f);
				}
				GUIComponent talentParent = talentList.Content;
				GUILayoutGroup subTreeLayoutGroup = new GUILayoutGroup(new RectTransform(treeSize, talentParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
				{
					Stretch = true
				};
				if (subTree.Type != TalentTreeType.Primary)
				{
					GUIFrame subtreeTitleFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), subTreeLayoutGroup.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, GUI.IntScale(30f))
					}, null, null);
					subtreeTitleFrame.RectTransform.IsFixedSize = true;
					int elementPadding = GUI.IntScale(8f);
					Point headerSize = subtreeTitleFrame.RectTransform.NonScaledSize;
					GUIFrame subTreeTitleBackground = new GUIFrame(new RectTransform(new Point(headerSize.X - elementPadding, headerSize.Y), subtreeTitleFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "SubtreeHeader", null);
					List<GUITextBlock> list = subTreeNames;
					RectTransform rectT = new RectTransform(Vector2.One, subTreeTitleBackground.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
					RichString text = subTree.DisplayName;
					GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
					list.Add(new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null));
				}
				int optionAmount = subTree.TalentOptionStages.Length;
				for (int i = 0; i < optionAmount; i++)
				{
					TalentOption option = subTree.TalentOptionStages[i];
					this.CreateTalentOption(subTreeLayoutGroup, subTree, i, option, info, specializationCount);
				}
				subTreeLayoutGroup.RectTransform.Resize(new Point(subTreeLayoutGroup.Rect.Width, subTreeLayoutGroup.Children.Sum((GUIComponent c) => c.Rect.Height + subTreeLayoutGroup.AbsoluteSpacing)), true);
				subTreeLayoutGroup.RectTransform.MinSize = new Point(subTreeLayoutGroup.Rect.Width, subTreeLayoutGroup.Rect.Height);
				subTreeLayoutGroup.Recalculate();
				if (subTree.Type == TalentTreeType.Specialization)
				{
					talentList.RectTransform.Resize(new Point(talentList.Rect.Width, Math.Max(subTreeLayoutGroup.Rect.Height, talentList.Rect.Height)), true);
					talentList.RectTransform.MinSize = new Point(0, talentList.Rect.Height);
				}
			}
			GUIListBox specializationList = TalentMenu.<CreateTalentMenu>g__GetSpecializationList|32_2(ref CS$<>8__locals1);
			specializationList.Content.RectTransform.Resize(new Point(specializationList.Content.Children.Sum((GUIComponent c) => c.Rect.Width), specializationList.Rect.Height), specializationCount < 3);
			if (specializationCount > 3)
			{
				specializationList.RectTransform.MinSize = new Point(specializationList.Rect.Width, specializationList.Content.Rect.Height + (int)((float)specializationList.ScrollBar.Rect.Height * 0.9f));
			}
			GUITextBlock.AutoScaleAndNormalize(subTreeNames, true, false, null);
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x000E0DF8 File Offset: 0x000DEFF8
		private void CreateTalentResetPopup(GUIComponent parent)
		{
			int talentResetCount = 0;
			Character character = this.character;
			if (((character != null) ? character.Info : null) != null)
			{
				talentResetCount = Math.Min(this.character.Info.TalentResetCount, this.character.Info.GetCurrentLevel());
			}
			bool hasResetTalentsBefore = talentResetCount > 0;
			GUIFrame bgBlocker = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				IgnoreLayoutGroups = true
			};
			GUIFrame popup = new GUIFrame(new RectTransform(new Vector2(0.6f, 0.8f), bgBlocker.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup popupLayout = new GUILayoutGroup(new RectTransform(ToolBox.PaddingSizeParentRelative(popup.RectTransform, 0.95f), popup.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.15f), popupLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("talentresetheader");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null);
			new GUITextBlock(new RectTransform(new Vector2(1f, hasResetTalentsBefore ? 0.25f : 0.5f), popupLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("talentresetprompt"), null, null, Alignment.Left, true, "", null);
			if (hasResetTalentsBefore)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.25f), popupLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("talentresetpromptwarning", "[count]", talentResetCount.ToString(), FormatCapitals.No), null, null, Alignment.Left, true, "", null).TextColor = GUIStyle.Red;
			}
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.35f), popupLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("holdtoconfirm"), Alignment.Center, "", null);
			guibutton.RequireHold = true;
			guibutton.HoldDurationSeconds = 1.5f;
			guibutton.OnClicked = delegate(GUIButton button, object o)
			{
				if (this.character == null || this.characterInfo == null)
				{
					return false;
				}
				this.characterInfo.RefundTalents();
				this.selectedTalents.Clear();
				this.UpdateTalentInfo();
				bgBlocker.Visible = false;
				return true;
			};
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("decidelater"), Alignment.Center, "", null);
			guibutton2.RequireHold = false;
			guibutton2.OnClicked = delegate(GUIButton button, object userData)
			{
				GUIButton resetButton = this.talentResetButton;
				if (resetButton == null)
				{
					return false;
				}
				TalentMenu.StartAnimation startAnimation = this.startAnimation;
				if (startAnimation != null)
				{
					startAnimation(popup.Rect, resetButton.Rect, 0.25f);
				}
				resetButton.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				bgBlocker.Visible = false;
				if (this.characterInfo != null)
				{
					this.characterInfo.ShowTalentResetPopupOnOpen = false;
				}
				return true;
			};
		}

		// Token: 0x06001739 RID: 5945 RVA: 0x000E1200 File Offset: 0x000DF400
		private static TalentMenu.StartAnimation CreatePopupAnimationHandler(GUIComponent parent)
		{
			TalentMenu.<>c__DisplayClass34_0 CS$<>8__locals1 = new TalentMenu.<>c__DisplayClass34_0();
			CS$<>8__locals1.drawAnimation = false;
			CS$<>8__locals1.animDur = 1f;
			CS$<>8__locals1.animTimer = 0f;
			CS$<>8__locals1.drawRect = RectangleF.Empty;
			CS$<>8__locals1.animStartRect = RectangleF.Empty;
			CS$<>8__locals1.animEndRect = RectangleF.Empty;
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(CS$<>8__locals1.<CreatePopupAnimationHandler>g__OnDraw|1), new Action<float, GUICustomComponent>(CS$<>8__locals1.<CreatePopupAnimationHandler>g__OnUpdate|2));
			guicustomComponent.IgnoreLayoutGroups = true;
			guicustomComponent.CanBeFocused = false;
			return new TalentMenu.StartAnimation(CS$<>8__locals1.<CreatePopupAnimationHandler>g__StartAnimation|0);
		}

		// Token: 0x0600173A RID: 5946 RVA: 0x000E12B4 File Offset: 0x000DF4B4
		private void CreateTalentOption(GUIComponent parent, TalentSubTree subTree, int index, TalentOption talentOption, CharacterInfo info, int specializationCount)
		{
			int elementPadding = GUI.IntScale(8f);
			GameSession gameSession = GameMain.GameSession;
			int height = GUI.IntScale((float)((((gameSession != null) ? gameSession.Campaign : null) == null) ? 65 : 60) * ((specializationCount > 3) ? 0.97f : 1f));
			GUIFrame talentOptionFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, height)
			}, null, null);
			Point talentFrameSize = talentOptionFrame.RectTransform.NonScaledSize;
			GUIFrame talentBackground = new GUIFrame(new RectTransform(new Point(talentFrameSize.X - elementPadding, talentFrameSize.Y - elementPadding), talentOptionFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "TalentBackground", null)
			{
				Color = TalentMenu.talentStageStyles[TalentTree.TalentStages.Locked].Color
			};
			GUIFrame talentBackgroundHighlight = new GUIFrame(new RectTransform(Vector2.One, talentBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "TalentBackgroundGlow", null)
			{
				Visible = false
			};
			GUIImage cornerIcon = new GUIImage(new RectTransform(new Vector2(0.2f), talentOptionFrame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothHeight)
			{
				MaxSize = new Point(16)
			}, null, GUIImage.ScalingMode.None)
			{
				CanBeFocused = false,
				Color = TalentMenu.talentStageStyles[TalentTree.TalentStages.Locked].Color
			};
			Point iconSize = cornerIcon.RectTransform.NonScaledSize;
			cornerIcon.RectTransform.AbsoluteOffset = new Point(iconSize.X / 2, iconSize.Y / 2);
			GUILayoutGroup talentOptionCenterGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 0.9f), talentOptionFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft);
			GUILayoutGroup talentOptionLayoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, talentOptionCenterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			HashSet<Identifier> talentOptionIdentifiers = (from t in talentOption.TalentIdentifiers
			orderby t
			select t).ToHashSet<Identifier>();
			HashSet<TalentButton> buttonsToAdd = new HashSet<TalentButton>();
			Dictionary<GUILayoutGroup, ImmutableHashSet<Identifier>> showCaseTalentParents = new Dictionary<GUILayoutGroup, ImmutableHashSet<Identifier>>();
			Dictionary<Identifier, GUIComponent> showCaseTalentButtonsToAdd = new Dictionary<Identifier, GUIComponent>();
			foreach (KeyValuePair<Identifier, ImmutableHashSet<Identifier>> keyValuePair in talentOption.ShowCaseTalents)
			{
				Identifier identifier3;
				ImmutableHashSet<Identifier> immutableHashSet;
				keyValuePair.Deconstruct(out identifier3, out immutableHashSet);
				Identifier showCaseTalentIdentifier = identifier3;
				ImmutableHashSet<Identifier> talents = immutableHashSet;
				talentOptionIdentifiers.Add(showCaseTalentIdentifier);
				Point parentSize = talentBackground.RectTransform.NonScaledSize;
				GUIFrame showCaseFrame = new GUIFrame(new RectTransform(new Point((int)((float)parentSize.X / 3f * (float)(talents.Count - 1)), parentSize.Y), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), "GUITooltip", null)
				{
					UserData = showCaseTalentIdentifier,
					IgnoreLayoutGroups = true,
					Visible = false
				};
				GUILayoutGroup showcaseCenterGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.7f), showCaseFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.CenterLeft);
				GUILayoutGroup showcaseLayout = new GUILayoutGroup(new RectTransform(Vector2.One, showcaseCenterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true
				};
				showCaseTalentParents.Add(showcaseLayout, talents);
				this.showCaseTalentFrames.Add(showCaseFrame);
			}
			using (HashSet<Identifier>.Enumerator enumerator2 = talentOptionIdentifiers.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Identifier talentId = enumerator2.Current;
					TalentPrefab talent;
					if (TalentPrefab.TalentPrefabs.TryGet(talentId, out talent))
					{
						bool isShowCaseTalent = talentOption.ShowCaseTalents.ContainsKey(talentId);
						GUIComponent talentParent = talentOptionLayoutGroup;
						foreach (KeyValuePair<GUILayoutGroup, ImmutableHashSet<Identifier>> keyValuePair2 in showCaseTalentParents)
						{
							ImmutableHashSet<Identifier> immutableHashSet;
							GUILayoutGroup guilayoutGroup;
							keyValuePair2.Deconstruct(out guilayoutGroup, out immutableHashSet);
							GUILayoutGroup key = guilayoutGroup;
							ImmutableHashSet<Identifier> value = immutableHashSet;
							if (value.Contains(talentId))
							{
								talentParent = key;
								break;
							}
						}
						GUIFrame talentFrame = new GUIFrame(new RectTransform(Vector2.One, talentParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
						{
							CanBeFocused = false
						};
						GUIFrame croppedTalentFrame = new GUIFrame(new RectTransform(Vector2.One, talentFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothHeight), null, null);
						GUIButton talentButton3 = new GUIButton(new RectTransform(Vector2.One, croppedTalentFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
						{
							ToolTip = TalentMenu.GetTalentTooltip(talent, this.characterInfo),
							UserData = talent.Identifier,
							PressedColor = TalentMenu.pressedColor,
							Enabled = (info.Character != null),
							OnClicked = delegate(GUIButton button, object userData)
							{
								if (isShowCaseTalent)
								{
									foreach (GUIComponent component in this.showCaseTalentFrames)
									{
										object userData2 = component.UserData;
										if (userData2 is Identifier)
										{
											Identifier showcaseIdentifier = (Identifier)userData2;
											if (showcaseIdentifier == talentId)
											{
												component.RectTransform.ScreenSpaceOffset = new Point((int)((float)button.Rect.Location.X - (float)component.Rect.Width / 2f + (float)button.Rect.Width / 2f), button.Rect.Location.Y - component.Rect.Height);
												component.Visible = true;
												continue;
											}
										}
										component.Visible = false;
									}
									return true;
								}
								if (this.character == null)
								{
									return false;
								}
								Identifier talentIdentifier = (Identifier)userData;
								if (talentOption.MaxChosenTalents == 1)
								{
									foreach (Identifier identifier2 in this.selectedTalents)
									{
										if (!this.character.HasTalent(identifier2) && !(identifier2 == talentId) && talentOptionIdentifiers.Contains(identifier2))
										{
											this.selectedTalents.Remove(identifier2);
										}
									}
								}
								if (this.character.HasTalent(talentIdentifier))
								{
									return true;
								}
								if (TalentTree.IsViableTalentForCharacter(info.Character, talentIdentifier, this.selectedTalents))
								{
									if (!this.selectedTalents.Contains(talentIdentifier))
									{
										this.selectedTalents.Add(talentIdentifier);
									}
									else
									{
										this.selectedTalents.Remove(talentIdentifier);
									}
								}
								else
								{
									this.selectedTalents.Remove(talentIdentifier);
								}
								this.UpdateTalentInfo();
								return true;
							}
						};
						talentButton3.Color = (talentButton3.HoverColor = (talentButton3.PressedColor = (talentButton3.SelectedColor = (talentButton3.DisabledColor = Color.Transparent))));
						GUIComponent iconImage;
						if (talent.Icon == null)
						{
							RectTransform rectT = new RectTransform(Vector2.One, talentButton3.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
							RichString text = "???";
							GUIFont largeFont = GUIStyle.LargeFont;
							iconImage = new GUITextBlock(rectT, text, null, largeFont, Alignment.Center, false, null, null)
							{
								OutlineColor = GUIStyle.Red,
								TextColor = GUIStyle.Red,
								PressedColor = TalentMenu.unselectableColor,
								DisabledColor = TalentMenu.unselectableColor,
								CanBeFocused = false
							};
						}
						else
						{
							Color color;
							iconImage = new GUIImage(new RectTransform(Vector2.One, talentButton3.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), talent.Icon, true, null)
							{
								Color = (talent.ColorOverride.TryUnwrap(out color) ? color : Color.White),
								PressedColor = TalentMenu.unselectableColor,
								DisabledColor = TalentMenu.unselectableColor * 0.5f,
								CanBeFocused = false
							};
						}
						iconImage.Enabled = talentButton3.Enabled;
						if (isShowCaseTalent)
						{
							showCaseTalentButtonsToAdd.Add(talentId, iconImage);
						}
						else
						{
							buttonsToAdd.Add(new TalentButton(iconImage, talent));
						}
					}
				}
			}
			foreach (TalentButton button3 in buttonsToAdd)
			{
				this.talentButtons.Add(button3);
			}
			foreach (KeyValuePair<Identifier, GUIComponent> keyValuePair3 in showCaseTalentButtonsToAdd)
			{
				Identifier identifier3;
				GUIComponent guicomponent;
				keyValuePair3.Deconstruct(out identifier3, out guicomponent);
				Identifier key2 = identifier3;
				GUIComponent value2 = guicomponent;
				HashSet<TalentButton> buttons = new HashSet<TalentButton>();
				using (ImmutableHashSet<Identifier>.Enumerator enumerator6 = talentOption.ShowCaseTalents[key2].GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						Identifier identifier = enumerator6.Current;
						TalentButton? talentButton2 = this.talentButtons.FirstOrNull(delegate(TalentButton talentButton)
						{
							Identifier identifier2 = talentButton.Identifier;
							return identifier2 == identifier;
						});
						if (talentButton2 != null)
						{
							TalentButton button2 = talentButton2.GetValueOrDefault();
							buttons.Add(button2);
						}
					}
				}
				this.talentShowCaseButtons.Add(new TalentShowCaseButton(buttons.ToImmutableHashSet<TalentButton>(), value2));
			}
			this.talentCornerIcons.Add(new TalentCornerIcon(subTree.Identifier, index, cornerIcon, talentBackground, talentBackgroundHighlight));
		}

		// Token: 0x0600173B RID: 5947 RVA: 0x000E1CA4 File Offset: 0x000DFEA4
		private void CreateFooter(GUIComponent parent, CharacterInfo info)
		{
			GUILayoutGroup bottomLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.07f), parent.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			GUILayoutGroup experienceLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.59f, 1f), bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame experienceBarFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.5f), experienceLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.experienceBar = new GUIProgressBar(new RectTransform(new Vector2(1f, 1f), experienceBarFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), info.GetProgressTowardsNextLevel(), new Color?(GUIStyle.Green), "", true)
			{
				IsHorizontal = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 1f), experienceBarFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont font = GUIStyle.Font;
			this.experienceText = new GUITextBlock(rectT, text, null, font, Alignment.CenterRight, false, "", null)
			{
				Shadow = true,
				ToolTip = TextManager.Get("experiencetooltip")
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.5f), experienceLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text2 = "";
			font = GUIStyle.SubHeadingFont;
			this.talentPointText = new GUITextBlock(rectT2, text2, null, font, Alignment.CenterRight, false, "", null)
			{
				AutoScaleVertical = true
			};
			this.talentResetButton = new GUIButton(new RectTransform(new Vector2(0.19f, 1f), bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("reset"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(this.ResetTalentSelection)
			};
			this.talentApplyButton = new GUIButton(new RectTransform(new Vector2(0.19f, 1f), bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("applysettingsbutton"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(this.ApplyTalentSelection)
			};
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.talentResetButton.TextBlock,
				this.talentApplyButton.TextBlock
			});
		}

		// Token: 0x0600173C RID: 5948 RVA: 0x000E2008 File Offset: 0x000E0208
		private static RichString GetTalentTooltip(TalentPrefab talent, [Nullable(2)] CharacterInfo character)
		{
			LocalizedString progress = string.Empty;
			ValueTuple<Identifier, int> stat;
			if (character != null && talent.TrackedStat.TryUnwrap(out stat))
			{
				float statValue = character.GetSavedStatValue(StatTypes.None, stat.Item1);
				int intValue = (int)MathF.Round(statValue);
				progress = "\n\n";
				progress += ((statValue < (float)stat.Item2) ? TextManager.GetWithVariables("talentprogress", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[amount]", intValue.ToString()),
					new ValueTuple<string, string>("[max]", stat.Item2.ToString())
				}) : TextManager.Get("talentprogresscompleted"));
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(Color.White.ToStringHex());
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talent.DisplayName);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖\n\n");
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.ExtendColorToPercentageSigns(talent.Description.Value));
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(progress);
			return RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x0600173D RID: 5949 RVA: 0x000E213C File Offset: 0x000E033C
		private bool ResetTalentSelection(GUIButton guiButton, object userData)
		{
			if (this.characterInfo == null)
			{
				return false;
			}
			int newTalentCount = this.selectedTalents.Count - this.characterInfo.GetUnlockedTalentsInTree().Count<Identifier>();
			if (this.characterInfo.TalentRefundPoints > 0 && newTalentCount == 0)
			{
				this.CreateTalentResetPopup(this.talentMainArea);
				return true;
			}
			this.selectedTalents = this.characterInfo.GetUnlockedTalentsInTree().ToHashSet<Identifier>();
			this.UpdateTalentInfo();
			return true;
		}

		// Token: 0x0600173E RID: 5950 RVA: 0x000E21AC File Offset: 0x000E03AC
		private void ApplyTalents(Character controlledCharacter)
		{
			foreach (Identifier talent in TalentTree.CheckTalentSelection(controlledCharacter, this.selectedTalents))
			{
				controlledCharacter.GiveTalent(talent, true);
				if (GameMain.Client != null)
				{
					GameMain.Client.CreateEntityEvent(controlledCharacter, default(Character.UpdateTalentsEventData));
				}
			}
			this.UpdateTalentInfo();
		}

		// Token: 0x0600173F RID: 5951 RVA: 0x000E2230 File Offset: 0x000E0430
		private bool ApplyTalentSelection(GUIButton guiButton, object userData)
		{
			if (this.character == null)
			{
				return false;
			}
			this.ApplyTalents(this.character);
			return true;
		}

		// Token: 0x06001740 RID: 5952 RVA: 0x000E224C File Offset: 0x000E044C
		public void UpdateTalentInfo()
		{
			if (this.character == null || this.characterInfo == null)
			{
				return;
			}
			bool unlockedAllTalents = this.character.HasUnlockedAllTalents();
			if (this.experienceBar == null || this.experienceText == null)
			{
				return;
			}
			if (unlockedAllTalents)
			{
				this.experienceText.Text = string.Empty;
				this.experienceBar.BarSize = 1f;
			}
			else
			{
				GUITextBlock guitextBlock = this.experienceText;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.characterInfo.ExperiencePoints - this.characterInfo.GetExperienceRequiredForCurrentLevel());
				defaultInterpolatedStringHandler.AppendLiteral(" / ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.characterInfo.GetExperienceRequiredToLevelUp() - this.characterInfo.GetExperienceRequiredForCurrentLevel());
				guitextBlock.Text = defaultInterpolatedStringHandler.ToStringAndClear();
				this.experienceBar.BarSize = this.characterInfo.GetProgressTowardsNextLevel();
			}
			this.selectedTalents = TalentTree.CheckTalentSelection(this.character, this.selectedTalents).ToHashSet<Identifier>();
			string pointsLeft = this.characterInfo.GetAvailableTalentPoints().ToString();
			int talentCount = this.selectedTalents.Count - this.characterInfo.GetUnlockedTalentsInTree().Count<Identifier>();
			if (unlockedAllTalents)
			{
				GUITextBlock guitextBlock2 = this.talentPointText;
				if (guitextBlock2 != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
					defaultInterpolatedStringHandler2.AppendFormatted(Color.Gray.ToStringHex());
					defaultInterpolatedStringHandler2.AppendLiteral("‖");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("talentmenu.alltalentsunlocked"));
					defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
					guitextBlock2.SetRichText(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
			}
			else if (talentCount > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler3.AppendFormatted(GUIStyle.Red.ToStringHex());
				defaultInterpolatedStringHandler3.AppendLiteral("‖");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(-talentCount);
				defaultInterpolatedStringHandler3.AppendLiteral("‖color:end‖");
				string pointsUsed = defaultInterpolatedStringHandler3.ToStringAndClear();
				LocalizedString localizedString = TextManager.GetWithVariables("talentmenu.points.spending", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[amount]", pointsLeft),
					new ValueTuple<string, string>("[used]", pointsUsed)
				});
				GUITextBlock guitextBlock3 = this.talentPointText;
				if (guitextBlock3 != null)
				{
					guitextBlock3.SetRichText(localizedString);
				}
			}
			else
			{
				GUITextBlock guitextBlock4 = this.talentPointText;
				if (guitextBlock4 != null)
				{
					guitextBlock4.SetRichText(TextManager.GetWithVariable("talentmenu.points", "[amount]", pointsLeft, FormatCapitals.No));
				}
			}
			foreach (TalentCornerIcon cornerIcon in this.talentCornerIcons)
			{
				TalentTree.TalentStages state = TalentTree.GetTalentOptionStageState(this.character, cornerIcon.TalentTree, cornerIcon.Index, this.selectedTalents);
				TalentTreeStyle style = TalentMenu.talentStageStyles[state];
				GUIComponentStyle newStyle = style.ComponentStyle;
				cornerIcon.IconComponent.ApplyStyle(newStyle);
				cornerIcon.IconComponent.Color = newStyle.Color;
				cornerIcon.BackgroundComponent.Color = style.Color;
				cornerIcon.GlowComponent.Visible = (state == TalentTree.TalentStages.Highlighted);
			}
			foreach (TalentButton talentButton in this.talentButtons)
			{
				TalentTree.TalentStages stage = TalentMenu.<UpdateTalentInfo>g__GetTalentState|41_0(this.character, talentButton.Identifier, this.selectedTalents);
				TalentMenu.<UpdateTalentInfo>g__ApplyTalentIconColor|41_1(stage, talentButton.IconComponent, talentButton.Prefab.ColorOverride);
			}
			foreach (TalentShowCaseButton showCaseTalentButton in this.talentShowCaseButtons)
			{
				TalentTree.TalentStages collectiveTalentStage = TalentMenu.<UpdateTalentInfo>g__GetCollectiveTalentState|41_2(this.character, showCaseTalentButton.Buttons, this.selectedTalents);
				TalentMenu.<UpdateTalentInfo>g__ApplyTalentIconColor|41_1(collectiveTalentStage, showCaseTalentButton.IconComponent, Option<Color>.None());
			}
			if (this.skillListBox == null)
			{
				return;
			}
			TabMenu.CreateSkillList(this.character, this.characterInfo, this.skillListBox);
		}

		// Token: 0x06001741 RID: 5953 RVA: 0x000E2684 File Offset: 0x000E0884
		public void Update()
		{
			if (this.characterInfo == null || this.talentResetButton == null || this.talentApplyButton == null)
			{
				return;
			}
			int talentCount = this.selectedTalents.Count - this.characterInfo.GetUnlockedTalentsInTree().Count<Identifier>();
			this.talentApplyButton.Enabled = (this.character != null && talentCount > 0);
			this.talentResetButton.Enabled = (this.character != null && (talentCount > 0 || this.characterInfo.TalentRefundPoints > 0));
			if (talentCount == 0 && this.characterInfo.TalentRefundPoints > 0)
			{
				if (this.talentResetButton.FlashTimer <= 0f)
				{
					this.talentResetButton.Flash(new Color?(GUIStyle.Orange), 1.5f, false, false, null);
				}
				this.talentResetButton.Text = TalentMenu.refundText;
			}
			else
			{
				this.talentResetButton.Text = TalentMenu.resetText;
			}
			if (this.talentApplyButton.Enabled && this.talentApplyButton.FlashTimer <= 0f)
			{
				this.talentApplyButton.Flash(new Color?(GUIStyle.Orange), 1.5f, false, false, null);
			}
			Identifier identifier;
			while (this.showCaseClosureQueue.TryDequeue(out identifier))
			{
				using (HashSet<GUIComponent>.Enumerator enumerator = this.showCaseTalentFrames.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GUIComponent component = enumerator.Current;
						object userData = component.UserData;
						if (userData is Identifier)
						{
							Identifier showcaseIdentifier = (Identifier)userData;
							if (showcaseIdentifier == identifier)
							{
								component.Visible = false;
							}
						}
					}
					continue;
				}
				break;
			}
			bool mouseInteracted = PlayerInput.PrimaryMouseButtonClicked() || PlayerInput.SecondaryMouseButtonClicked() || PlayerInput.ScrollWheelSpeed != 0;
			bool keyboardInteracted = PlayerInput.KeyHit(Keys.Escape) || GameSettings.CurrentConfig.KeyMap.Bindings[InputType.InfoTab].IsHit();
			foreach (GUIComponent component2 in this.showCaseTalentFrames)
			{
				object userData = component2.UserData;
				if (userData is Identifier)
				{
					Identifier identifier2 = (Identifier)userData;
					component2.AddToGUIUpdateList(false, 1);
					if (component2.Visible && (keyboardInteracted || (mouseInteracted && !component2.Rect.Contains(PlayerInput.MousePosition))))
					{
						this.showCaseClosureQueue.Enqueue(identifier2);
					}
				}
			}
			NetLobbyScreen.UpdateJobVariantSelectionIfNeeded();
		}

		// Token: 0x06001742 RID: 5954 RVA: 0x000E2920 File Offset: 0x000E0B20
		[NullableContext(2)]
		private static bool IsOwnCharacter(CharacterInfo info)
		{
			if (info == null)
			{
				return false;
			}
			Character controlled = Character.Controlled;
			CharacterInfo characterInfo;
			if ((characterInfo = ((controlled != null) ? controlled.Info : null)) == null)
			{
				GameClient client = GameMain.Client;
				characterInfo = ((client != null) ? client.CharacterInfo : null);
			}
			CharacterInfo ownCharacterInfo = characterInfo;
			return ownCharacterInfo != null && info.GetIdentifierUsingOriginalName() == ownCharacterInfo.GetIdentifierUsingOriginalName();
		}

		// Token: 0x06001743 RID: 5955 RVA: 0x000E296C File Offset: 0x000E0B6C
		[NullableContext(2)]
		private static bool IsOnSameTeam(CharacterInfo info)
		{
			if (info == null)
			{
				return false;
			}
			Character controlled = Character.Controlled;
			CharacterTeamType? characterTeamType;
			if (controlled == null)
			{
				GameClient client = GameMain.Client;
				if (client == null)
				{
					characterTeamType = null;
				}
				else
				{
					Client myClient = client.MyClient;
					characterTeamType = ((myClient != null) ? new CharacterTeamType?(myClient.TeamID) : null);
				}
			}
			else
			{
				characterTeamType = new CharacterTeamType?(controlled.TeamID);
			}
			CharacterTeamType? ownCharacterTeam = characterTeamType;
			if (ownCharacterTeam == null)
			{
				return false;
			}
			CharacterTeamType teamID = info.TeamID;
			CharacterTeamType? characterTeamType2 = ownCharacterTeam;
			return teamID == characterTeamType2.GetValueOrDefault() & characterTeamType2 != null;
		}

		// Token: 0x06001744 RID: 5956 RVA: 0x000E29EC File Offset: 0x000E0BEC
		private static bool IsSpectatingInMultiplayer()
		{
			GameClient client = GameMain.Client;
			Client myClient = (client != null) ? client.MyClient : null;
			return myClient != null && myClient.Spectating;
		}

		// Token: 0x06001745 RID: 5957 RVA: 0x000E2A18 File Offset: 0x000E0C18
		public static bool CanManageTalents(CharacterInfo targetInfo)
		{
			if (GameMain.IsSingleplayer)
			{
				return true;
			}
			if (TalentMenu.IsOwnCharacter(targetInfo))
			{
				return true;
			}
			if (TalentMenu.IsSpectatingInMultiplayer())
			{
				return false;
			}
			Character character = targetInfo.Character;
			if (character == null || !character.IsBot)
			{
				return false;
			}
			if (!TalentMenu.IsOnSameTeam(targetInfo))
			{
				return false;
			}
			GameClient client = GameMain.Client;
			return client != null && client.HasPermission(ClientPermissions.ManageBotTalents);
		}

		// Token: 0x06001747 RID: 5959 RVA: 0x000E2AD0 File Offset: 0x000E0CD0
		// Note: this type is marked as 'beforefieldinit'.
		static TalentMenu()
		{
			Dictionary<TalentTree.TalentStages, TalentTreeStyle> dictionary = new Dictionary<TalentTree.TalentStages, TalentTreeStyle>();
			dictionary[TalentTree.TalentStages.Invalid] = new TalentTreeStyle("TalentTreeLocked", TalentMenu.lockedColor);
			dictionary[TalentTree.TalentStages.Locked] = new TalentTreeStyle("TalentTreeLocked", TalentMenu.lockedColor);
			dictionary[TalentTree.TalentStages.Unlocked] = new TalentTreeStyle("TalentTreePurchased", TalentMenu.unlockedColor);
			dictionary[TalentTree.TalentStages.Available] = new TalentTreeStyle("TalentTreeUnlocked", TalentMenu.availableColor);
			dictionary[TalentTree.TalentStages.Highlighted] = new TalentTreeStyle("TalentTreeAvailable", TalentMenu.availableColor);
			TalentMenu.talentStageStyles = dictionary.ToImmutableDictionary<TalentTree.TalentStages, TalentTreeStyle>();
			TalentMenu.refundText = TextManager.Get("refund");
			TalentMenu.resetText = TextManager.Get("reset");
		}

		// Token: 0x06001748 RID: 5960 RVA: 0x000E2C00 File Offset: 0x000E0E00
		[CompilerGenerated]
		internal static GUIListBox <CreateTalentMenu>g__GetSpecializationList|32_2(ref TalentMenu.<>c__DisplayClass32_0 A_0)
		{
			GUIListBox specList = A_0.mainList.Content.Children.LastOrDefault<GUIComponent>() as GUIListBox;
			if (specList != null)
			{
				return specList;
			}
			return new GUIListBox(new RectTransform(new Vector2(1f, 0.5f), A_0.mainList.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, null, null, true, false);
		}

		// Token: 0x06001749 RID: 5961 RVA: 0x000E2C84 File Offset: 0x000E0E84
		[CompilerGenerated]
		internal static TalentTree.TalentStages <UpdateTalentInfo>g__GetTalentState|41_0(Character character, Identifier talentIdentifier, IReadOnlyCollection<Identifier> selectedTalents)
		{
			bool unselectable = !TalentTree.IsViableTalentForCharacter(character, talentIdentifier, selectedTalents) || character.HasTalent(talentIdentifier);
			TalentTree.TalentStages stage = unselectable ? TalentTree.TalentStages.Locked : TalentTree.TalentStages.Available;
			if (unselectable)
			{
				stage = TalentTree.TalentStages.Locked;
			}
			if (character.HasTalent(talentIdentifier))
			{
				stage = TalentTree.TalentStages.Unlocked;
			}
			else if (selectedTalents.Contains(talentIdentifier))
			{
				stage = TalentTree.TalentStages.Highlighted;
			}
			return stage;
		}

		// Token: 0x0600174A RID: 5962 RVA: 0x000E2CCC File Offset: 0x000E0ECC
		[NullableContext(0)]
		[CompilerGenerated]
		internal static void <UpdateTalentInfo>g__ApplyTalentIconColor|41_1(TalentTree.TalentStages stage, [Nullable(1)] GUIComponent component, Option<Color> colorOverride)
		{
			Color color2;
			switch (stage)
			{
			case TalentTree.TalentStages.Invalid:
				color2 = TalentMenu.unselectableColor;
				break;
			case TalentTree.TalentStages.Locked:
				color2 = TalentMenu.unselectableColor;
				break;
			case TalentTree.TalentStages.Unlocked:
				color2 = TalentMenu.<UpdateTalentInfo>g__GetColorOrOverride|41_3(GUIStyle.Green, colorOverride);
				break;
			case TalentTree.TalentStages.Available:
				color2 = TalentMenu.<UpdateTalentInfo>g__GetColorOrOverride|41_3(TalentMenu.unselectedColor, colorOverride);
				break;
			case TalentTree.TalentStages.Highlighted:
				color2 = TalentMenu.<UpdateTalentInfo>g__GetColorOrOverride|41_3(GUIStyle.Orange, colorOverride);
				break;
			default:
				throw new ArgumentOutOfRangeException("stage", stage, null);
			}
			Color color = color2;
			component.Color = color;
			component.HoverColor = Color.Lerp(color, Color.White, 0.7f);
		}

		// Token: 0x0600174B RID: 5963 RVA: 0x000E2D6C File Offset: 0x000E0F6C
		[NullableContext(0)]
		[CompilerGenerated]
		internal static Color <UpdateTalentInfo>g__GetColorOrOverride|41_3(Color color, Option<Color> colorOverride)
		{
			Color overrideColor;
			if (!colorOverride.TryUnwrap(out overrideColor))
			{
				return color;
			}
			return overrideColor;
		}

		// Token: 0x0600174C RID: 5964 RVA: 0x000E2D88 File Offset: 0x000E0F88
		[CompilerGenerated]
		internal static TalentTree.TalentStages <UpdateTalentInfo>g__GetCollectiveTalentState|41_2(Character character, IReadOnlyCollection<TalentButton> buttons, IReadOnlyCollection<Identifier> selectedTalents)
		{
			HashSet<TalentTree.TalentStages> talentStages = new HashSet<TalentTree.TalentStages>();
			foreach (TalentButton button in buttons)
			{
				talentStages.Add(TalentMenu.<UpdateTalentInfo>g__GetTalentState|41_0(character, button.Identifier, selectedTalents));
			}
			TalentTree.TalentStages collectiveStage = talentStages.All((TalentTree.TalentStages stage) => stage == TalentTree.TalentStages.Locked) ? TalentTree.TalentStages.Locked : TalentTree.TalentStages.Available;
			foreach (TalentTree.TalentStages stage2 in talentStages)
			{
				if (stage2 == TalentTree.TalentStages.Highlighted)
				{
					collectiveStage = TalentTree.TalentStages.Highlighted;
					break;
				}
				if (stage2 == TalentTree.TalentStages.Unlocked)
				{
					collectiveStage = TalentTree.TalentStages.Unlocked;
					break;
				}
			}
			return collectiveStage;
		}

		// Token: 0x04000BC5 RID: 3013
		public const string ManageBotTalentsButtonUserData = "managebottalentsbutton";

		// Token: 0x04000BC6 RID: 3014
		[Nullable(2)]
		private Character character;

		// Token: 0x04000BC7 RID: 3015
		[Nullable(2)]
		private CharacterInfo characterInfo;

		// Token: 0x04000BC8 RID: 3016
		private static readonly Color unselectedColor = new Color(240, 255, 255, 225);

		// Token: 0x04000BC9 RID: 3017
		private static readonly Color unselectableColor = new Color(100, 100, 100, 225);

		// Token: 0x04000BCA RID: 3018
		private static readonly Color pressedColor = new Color(60, 60, 60, 225);

		// Token: 0x04000BCB RID: 3019
		private static readonly Color lockedColor = new Color(48, 48, 48, 255);

		// Token: 0x04000BCC RID: 3020
		private static readonly Color unlockedColor = new Color(24, 37, 31, 255);

		// Token: 0x04000BCD RID: 3021
		private static readonly Color availableColor = new Color(50, 47, 33, 255);

		// Token: 0x04000BCE RID: 3022
		private static readonly ImmutableDictionary<TalentTree.TalentStages, TalentTreeStyle> talentStageStyles;

		// Token: 0x04000BCF RID: 3023
		private readonly HashSet<TalentButton> talentButtons = new HashSet<TalentButton>();

		// Token: 0x04000BD0 RID: 3024
		private readonly HashSet<TalentShowCaseButton> talentShowCaseButtons = new HashSet<TalentShowCaseButton>();

		// Token: 0x04000BD1 RID: 3025
		private readonly HashSet<GUIComponent> showCaseTalentFrames = new HashSet<GUIComponent>();

		// Token: 0x04000BD2 RID: 3026
		private readonly HashSet<TalentCornerIcon> talentCornerIcons = new HashSet<TalentCornerIcon>();

		// Token: 0x04000BD3 RID: 3027
		private HashSet<Identifier> selectedTalents = new HashSet<Identifier>();

		// Token: 0x04000BD4 RID: 3028
		private readonly Queue<Identifier> showCaseClosureQueue = new Queue<Identifier>();

		// Token: 0x04000BD5 RID: 3029
		[Nullable(2)]
		private GUITextBlock nameBlock;

		// Token: 0x04000BD6 RID: 3030
		[Nullable(2)]
		private GUIButton renameButton;

		// Token: 0x04000BD7 RID: 3031
		[Nullable(2)]
		private GUIListBox skillListBox;

		// Token: 0x04000BD8 RID: 3032
		[Nullable(2)]
		private GUITextBlock talentPointText;

		// Token: 0x04000BD9 RID: 3033
		[Nullable(2)]
		private GUIProgressBar experienceBar;

		// Token: 0x04000BDA RID: 3034
		[Nullable(2)]
		private GUITextBlock experienceText;

		// Token: 0x04000BDB RID: 3035
		[Nullable(2)]
		private GUILayoutGroup skillLayout;

		// Token: 0x04000BDC RID: 3036
		[Nullable(2)]
		private GUIButton talentApplyButton;

		// Token: 0x04000BDD RID: 3037
		[Nullable(2)]
		private GUIButton talentResetButton;

		// Token: 0x04000BDE RID: 3038
		[Nullable(2)]
		private TalentMenu.StartAnimation startAnimation;

		// Token: 0x04000BDF RID: 3039
		[Nullable(2)]
		private GUIComponent talentMainArea;

		// Token: 0x04000BE0 RID: 3040
		private static readonly LocalizedString refundText;

		// Token: 0x04000BE1 RID: 3041
		private static readonly LocalizedString resetText;

		// Token: 0x02000A0C RID: 2572
		// (Invoke) Token: 0x060073D6 RID: 29654
		[NullableContext(0)]
		private delegate void StartAnimation(RectangleF start, RectangleF end, float duration);
	}
}
