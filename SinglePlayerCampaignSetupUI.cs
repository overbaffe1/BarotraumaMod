using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000102 RID: 258
	internal sealed class SinglePlayerCampaignSetupUI : CampaignSetupUI
	{
		// Token: 0x170009CD RID: 2509
		// (get) Token: 0x0600242F RID: 9263 RVA: 0x0016BE5A File Offset: 0x0016A05A
		// (set) Token: 0x06002430 RID: 9264 RVA: 0x0016BE62 File Offset: 0x0016A062
		public CharacterInfo.AppearanceCustomizationMenu[] CharacterMenus { get; private set; }

		// Token: 0x06002431 RID: 9265 RVA: 0x0016BE6B File Offset: 0x0016A06B
		public SinglePlayerCampaignSetupUI(GUIComponent newGameContainer, GUIComponent loadGameContainer) : base(newGameContainer, loadGameContainer)
		{
			this.CreateNewGameMenu();
		}

		// Token: 0x06002432 RID: 9266 RVA: 0x0016BE7C File Offset: 0x0016A07C
		public void Update()
		{
			float targetScroll = (float)this.currentPage / ((float)this.pageContainer.Content.CountChildren - 1f);
			this.pageContainer.BarScroll = MathHelper.Lerp(this.pageContainer.BarScroll, targetScroll, 0.2f);
			if (MathUtils.NearlyEqual(this.pageContainer.BarScroll, targetScroll, 0.001f))
			{
				this.pageContainer.BarScroll = targetScroll;
			}
			for (int i = 0; i < this.CharacterMenus.Length; i++)
			{
				CharacterInfo.AppearanceCustomizationMenu appearanceCustomizationMenu = this.CharacterMenus[i];
				if (appearanceCustomizationMenu != null)
				{
					appearanceCustomizationMenu.Update();
				}
			}
			this.pageContainer.HoverCursor = CursorState.Default;
			this.pageContainer.Content.HoverCursor = CursorState.Default;
		}

		// Token: 0x06002433 RID: 9267 RVA: 0x0016BF34 File Offset: 0x0016A134
		public void SetPage(int pageIndex)
		{
			this.currentPage = pageIndex;
			int i;
			Action<GUIComponent> <>9__1;
			int j;
			for (i = 0; i < this.pageContainer.Content.CountChildren; i = j + 1)
			{
				GUIComponent child = this.pageContainer.Content.GetChild(i);
				child.CanBeFocused = (i == this.currentPage);
				IEnumerable<GUIComponent> allChildren = child.GetAllChildren();
				Action<GUIComponent> action;
				if ((action = <>9__1) == null)
				{
					action = (<>9__1 = delegate(GUIComponent c)
					{
						GUIDropDown dd = c as GUIDropDown;
						if (dd != null)
						{
							dd.Dropped = false;
						}
						c.CanBeFocused = (i == this.currentPage);
					});
				}
				allChildren.ForEach(action);
				j = i;
			}
			GUIListBox previewListBox = this.subPreviewContainer.GetAllChildren<GUIListBox>().FirstOrDefault<GUIListBox>();
			if (previewListBox != null)
			{
				IEnumerable<GUIComponent> allChildren2 = previewListBox.GetAllChildren();
				if (allChildren2 == null)
				{
					return;
				}
				allChildren2.ForEach(delegate(GUIComponent c)
				{
					c.CanBeFocused = false;
				});
			}
		}

		// Token: 0x06002434 RID: 9268 RVA: 0x0016C020 File Offset: 0x0016A220
		private void CreateNewGameMenu()
		{
			this.pageContainer = new GUIListBox(new RectTransform(Vector2.One, this.newGameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, null, true, false)
			{
				ScrollBarEnabled = false,
				ScrollBarVisible = false,
				AllowArrowKeyScroll = false,
				HoverCursor = CursorState.Default
			};
			this.CreateFirstPage(this.<CreateNewGameMenu>g__createPageLayout|13_0());
			this.CreateSecondPage(this.<CreateNewGameMenu>g__createPageLayout|13_0());
			this.pageContainer.RecalculateChildren();
			this.pageContainer.GetAllChildren().ForEach(delegate(GUIComponent c)
			{
				c.ClampMouseRectToParent = true;
			});
			this.pageContainer.GetAllChildren<GUIDropDown>().ForEach(delegate(GUIDropDown dd)
			{
				dd.ListBox.ClampMouseRectToParent = false;
				dd.ListBox.Content.ClampMouseRectToParent = false;
			});
			this.SetPage(0);
		}

		// Token: 0x06002435 RID: 9269 RVA: 0x0016C11C File Offset: 0x0016A31C
		private void CreateFirstPage(GUILayoutGroup firstPageLayout)
		{
			SinglePlayerCampaignSetupUI.<>c__DisplayClass14_0 CS$<>8__locals1 = new SinglePlayerCampaignSetupUI.<>c__DisplayClass14_0();
			CS$<>8__locals1.<>4__this = this;
			firstPageLayout.RelativeSpacing = 0.02f;
			GUILayoutGroup columnContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), firstPageLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(Vector2.One, columnContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.015f
			};
			GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(new Vector2(1.5f, 1f), columnContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.015f
			};
			columnContainer.Recalculate();
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.02f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, 20);
			RichString text7 = TextManager.Get("SaveName");
			GUIFont font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectTransform, text7, null, font, Alignment.Left, false, "", null);
			GUITextBox guitextBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.05f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, string.Empty, null, null, Alignment.Left, false, "", null, false, true);
			guitextBox.textFilterFunction = ((string str) => ToolBox.RemoveInvalidFileNameChars(str));
			this.saveNameBox = guitextBox;
			RectTransform rectTransform2 = new RectTransform(new Vector2(1f, 0.02f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform2.MinSize = new Point(0, 20);
			RichString text2 = TextManager.Get("MapSeed");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectTransform2, text2, null, font, Alignment.Left, false, "", null);
			this.seedBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.05f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, ToolBox.RandomSeed(8), null, null, Alignment.Left, false, "", null, false, true);
			RectTransform rectTransform3 = new RectTransform(new Vector2(1f, 0.02f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform3.MinSize = new Point(0, 20);
			RichString text3 = TextManager.Get("SelectedSub");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectTransform3, text3, null, font, Alignment.Left, false, "", null);
			GUIDropDown moddedDropdown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.02f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", 3, "", false, false, Alignment.CenterLeft, 1f);
			moddedDropdown.AddItem(TextManager.Get("clientpermission.all"), CampaignSetupUI.CategoryFilter.All, null, null, null);
			moddedDropdown.AddItem(TextManager.Get("servertag.modded.false"), CampaignSetupUI.CategoryFilter.Vanilla, null, null, null);
			moddedDropdown.AddItem(TextManager.Get("customrank"), CampaignSetupUI.CategoryFilter.Custom, null, null, null);
			moddedDropdown.Select(0);
			GUILayoutGroup filterContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.subList = new GUIListBox(new RectTransform(new Vector2(1f, 0.65f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				ScrollBarVisible = true
			};
			SinglePlayerCampaignSetupUI.<>c__DisplayClass14_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(new Vector2(0.001f, 1f), filterContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("serverlog.filter");
			font = GUIStyle.Font;
			CS$<>8__locals2.searchTitle = new GUITextBlock(rectT, text4, null, font, Alignment.CenterLeft, false, "", null);
			SinglePlayerCampaignSetupUI.<>c__DisplayClass14_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 1f), filterContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			string text5 = "";
			font = GUIStyle.Font;
			CS$<>8__locals3.searchBox = new GUITextBox(rectT2, text5, null, font, Alignment.Left, false, "", null, true, true);
			filterContainer.RectTransform.MinSize = CS$<>8__locals1.searchBox.RectTransform.MinSize;
			CS$<>8__locals1.searchBox.OnSelected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = false;
			};
			CS$<>8__locals1.searchBox.OnDeselected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = true;
			};
			CS$<>8__locals1.searchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				CS$<>8__locals1.<>4__this.FilterSubs(CS$<>8__locals1.<>4__this.subList, text);
				return true;
			};
			moddedDropdown.OnSelected = delegate(GUIComponent component, object data)
			{
				CS$<>8__locals1.searchBox.Text = string.Empty;
				CS$<>8__locals1.<>4__this.subFilter = (CampaignSetupUI.CategoryFilter)data;
				CS$<>8__locals1.<>4__this.UpdateSubList(SubmarineInfo.SavedSubmarines);
				return true;
			};
			this.subList.OnSelected = new GUIListBox.OnSelectedHandler(this.OnSubSelected);
			this.subPreviewContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup firstPageButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.08f), firstPageLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.025f
			};
			RectTransform rectT3 = new RectTransform(new Vector2(0.3f, 1f), firstPageButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = "";
			font = GUIStyle.Font;
			base.InitialMoneyText = new GUITextBlock(rectT3, text6, new Color?(GUIStyle.Green), font, Alignment.CenterLeft, false, "", null)
			{
				TextGetter = delegate()
				{
					int initialMoney = CampaignSettings.CurrentSettings.InitialMoney;
					SubmarineInfo subInfo = CS$<>8__locals1.<>4__this.subList.SelectedData as SubmarineInfo;
					if (subInfo != null)
					{
						initialMoney -= subInfo.Price;
					}
					initialMoney = Math.Max(initialMoney, 0);
					return TextManager.GetWithVariable("campaignstartingmoney", "[money]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", initialMoney), FormatCapitals.No);
				}
			};
			base.CampaignCustomizeButton = new GUIButton(new RectTransform(new Vector2(0.25f, 1f), firstPageButtonContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SettingsButton"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton tb, object userdata)
				{
					SinglePlayerCampaignSetupUI <>4__this = CS$<>8__locals1.<>4__this;
					CampaignSettings currentSettings = CampaignSettings.CurrentSettings;
					Action<CampaignSettings> onClosed;
					if ((onClosed = CS$<>8__locals1.<>9__7) == null)
					{
						onClosed = (CS$<>8__locals1.<>9__7 = delegate(CampaignSettings settings)
						{
							CampaignSettings prevSettings = CampaignSettings.CurrentSettings;
							CampaignSettings.CurrentSettings = settings;
							if (prevSettings.InitialMoney != settings.InitialMoney)
							{
								object selectedData = CS$<>8__locals1.<>4__this.subList.SelectedData;
								CS$<>8__locals1.<>4__this.UpdateSubList(SubmarineInfo.SavedSubmarines);
								SubmarineInfo selectedSub = selectedData as SubmarineInfo;
								if (selectedSub != null && selectedSub.Price <= CampaignSettings.CurrentSettings.InitialMoney)
								{
									CS$<>8__locals1.<>4__this.subList.Select(selectedData, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
								}
							}
						});
					}
					<>4__this.CreateCustomizeWindow(currentSettings, onClosed);
					return true;
				}
			};
			this.nextButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), firstPageButtonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Next"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object userData)
				{
					CS$<>8__locals1.<>4__this.SetPage(1);
					return false;
				}
			};
			columnContainer.Recalculate();
			leftColumn.Recalculate();
			rightColumn.Recalculate();
		}

		// Token: 0x06002436 RID: 9270 RVA: 0x0016C9FC File Offset: 0x0016ABFC
		private void CreateSecondPage(GUILayoutGroup secondPageLayout)
		{
			secondPageLayout.RelativeSpacing = 0.01f;
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.04f), secondPageLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("Crew");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.TopLeft, false, "", null);
			this.characterInfoColumns = new GUIListBox(new RectTransform(new Vector2(1f, 0.86f), secondPageLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, "", true, false);
			GUILayoutGroup secondPageButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.08f), secondPageLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.2f
			};
			new GUIButton(new RectTransform(new Vector2(0.4f, 1f), secondPageButtonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Back"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userData)
			{
				this.SetPage(0);
				return false;
			};
			base.StartButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), secondPageButtonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("StartCampaignButton"), Alignment.Center, "", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(this.FinishSetup)
			};
		}

		// Token: 0x06002437 RID: 9271 RVA: 0x0016CC04 File Offset: 0x0016AE04
		public void RandomizeCrew()
		{
			List<ValueTuple<CharacterInfo, JobPrefab>> characterInfos = new List<ValueTuple<CharacterInfo, JobPrefab>>();
			foreach (JobPrefab jobPrefab in JobPrefab.Prefabs)
			{
				for (int i = 0; i < jobPrefab.InitialCount; i++)
				{
					int variant = Rand.Range(0, jobPrefab.Variants, Rand.RandSync.Unsynced);
					characterInfos.Add(new ValueTuple<CharacterInfo, JobPrefab>(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab, variant, Rand.RandSync.Unsynced, default(Identifier)), jobPrefab));
				}
			}
			if (characterInfos.Count == 0)
			{
				DebugConsole.ThrowError("No starting crew found! If you're using mods, it may be that the mods have overridden the vanilla jobs without specifying which types of characters the starting crew should consist of. If you're the developer of the mod, ensure that you've set the InitialCount properties for the custom jobs.", null, null, false, false);
				DebugConsole.AddWarning("Choosing the first available jobs as the starting crew...", null);
				foreach (JobPrefab jobPrefab2 in JobPrefab.Prefabs)
				{
					int variant2 = Rand.Range(0, jobPrefab2.Variants, Rand.RandSync.Unsynced);
					characterInfos.Add(new ValueTuple<CharacterInfo, JobPrefab>(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab2, variant2, Rand.RandSync.Unsynced, default(Identifier)), jobPrefab2));
					if (characterInfos.Count >= 3)
					{
						break;
					}
				}
			}
			characterInfos.Sort(([TupleElementNames(new string[]
			{
				"Info",
				"Job"
			})] ValueTuple<CharacterInfo, JobPrefab> a, [TupleElementNames(new string[]
			{
				"Info",
				"Job"
			})] ValueTuple<CharacterInfo, JobPrefab> b) => Math.Sign(a.Item2.CampaignSetupUIOrder - b.Item2.CampaignSetupUIOrder));
			this.characterInfoColumns.ClearChildren();
			CharacterInfo.AppearanceCustomizationMenu[] characterMenus = this.CharacterMenus;
			if (characterMenus != null)
			{
				characterMenus.ForEach(delegate(CharacterInfo.AppearanceCustomizationMenu m)
				{
					m.Dispose();
				});
			}
			this.CharacterMenus = new CharacterInfo.AppearanceCustomizationMenu[characterInfos.Count];
			for (int j = 0; j < characterInfos.Count; j++)
			{
				GUILayoutGroup subLayout = new GUILayoutGroup(new RectTransform(new Vector2(Math.Max(1f / (float)characterInfos.Count, 0.33f), 1f), this.characterInfoColumns.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				ValueTuple<CharacterInfo, JobPrefab> valueTuple = characterInfos[j];
				CharacterInfo characterInfo = valueTuple.Item1;
				JobPrefab job = valueTuple.Item2;
				characterInfo.CreateIcon(new RectTransform(new Vector2(1f, 0.275f), subLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal));
				GUIFrame jobTextContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), subLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				GUITextBlock jobText = new GUITextBlock(new RectTransform(Vector2.One, jobTextContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), job.Name, new Color?(job.UIColor), null, Alignment.Left, false, "", null);
				GUITextBox characterName = new GUITextBox(new RectTransform(new Vector2(1f, 0.1f), subLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
				{
					Text = characterInfo.Name,
					UserData = "random"
				};
				characterName.OnDeselected += delegate(GUITextBox sender, Keys key)
				{
					if (string.IsNullOrWhiteSpace(sender.Text))
					{
						characterInfo.Name = characterInfo.GetRandomName(Rand.RandSync.Unsynced);
						sender.UserData = "random";
					}
					else
					{
						characterInfo.Rename(sender.Text);
						sender.UserData = "user";
					}
					sender.Text = characterInfo.Name;
				};
				GUITextBox characterName2 = characterName;
				characterName2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(characterName2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox sender, string text)
				{
					sender.Deselect();
					return false;
				}));
				GUIFrame customizationFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.6f), subLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				this.CharacterMenus[j] = new CharacterInfo.AppearanceCustomizationMenu(characterInfo, customizationFrame, false)
				{
					OnHeadSwitch = delegate(CharacterInfo.AppearanceCustomizationMenu menu)
					{
						string ud = characterName.UserData as string;
						if (ud != null && ud == "random")
						{
							characterInfo.Name = characterInfo.GetRandomName(Rand.RandSync.Unsynced);
							characterName.Text = characterInfo.Name;
							characterName.UserData = "random";
						}
						SinglePlayerCampaignSetupUI.StealRandomizeButton(menu, jobTextContainer);
					}
				};
				SinglePlayerCampaignSetupUI.StealRandomizeButton(this.CharacterMenus[j], jobTextContainer);
			}
		}

		// Token: 0x06002438 RID: 9272 RVA: 0x0016D0CC File Offset: 0x0016B2CC
		private void CreateCustomizeWindow(CampaignSettings prevSettings, Action<CampaignSettings> onClosed = null)
		{
			base.CampaignCustomizeSettings = new GUIMessageBox("", "", new LocalizedString[]
			{
				TextManager.Get("OK")
			}, new Vector2?(new Vector2(0.25f, 0.5f)), new Point?(new Point(450, 350)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup campaignSettingContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.8f), base.CampaignCustomizeSettings.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			CampaignSetupUI.CampaignSettingElements elements = CampaignSetupUI.CreateCampaignSettingList(campaignSettingContent, prevSettings, true);
			GUIButton guibutton = base.CampaignCustomizeSettings.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
			{
				Action<CampaignSettings> onClosed2 = onClosed;
				if (onClosed2 != null)
				{
					onClosed2(elements.CreateSettings());
				}
				GameSettings.SaveCurrentConfig();
				return this.CampaignCustomizeSettings.Close(button, o);
			}));
		}

		// Token: 0x06002439 RID: 9273 RVA: 0x0016D1E4 File Offset: 0x0016B3E4
		private static void StealRandomizeButton(CharacterInfo.AppearanceCustomizationMenu menu, GUIComponent parent)
		{
			GUIButton randomizeButton = menu.RandomizeButton;
			GUIButton oldButton = parent.GetChild<GUIButton>();
			parent.RemoveChild(oldButton);
			randomizeButton.RectTransform.Parent = parent.RectTransform;
			randomizeButton.RectTransform.RelativeSize = Vector2.One * 1.3f;
		}

		// Token: 0x0600243A RID: 9274 RVA: 0x0016D234 File Offset: 0x0016B434
		private bool FinishSetup(GUIButton btn, object userdata)
		{
			if (string.IsNullOrWhiteSpace(this.saveNameBox.Text))
			{
				this.saveNameBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
				return false;
			}
			SubmarineInfo selectedSub = null;
			if (!(this.subList.SelectedData is SubmarineInfo))
			{
				return false;
			}
			selectedSub = (this.subList.SelectedData as SubmarineInfo);
			if (selectedSub.SubmarineClass == SubmarineClass.Undefined)
			{
				new GUIMessageBox(TextManager.Get("error"), TextManager.Get("undefinedsubmarineselected"), null, null, GUIMessageBox.Type.Default);
				return false;
			}
			if (string.IsNullOrEmpty(selectedSub.MD5Hash.StringRepresentation))
			{
				((GUITextBlock)this.subList.SelectedComponent).TextColor = Color.DarkRed * 0.8f;
				this.subList.SelectedComponent.CanBeFocused = false;
				this.subList.Deselect();
				return false;
			}
			string savePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Singleplayer, this.saveNameBox.Text);
			bool hasRequiredContentPackages = selectedSub.RequiredContentPackagesInstalled;
			CampaignSettings settings = CampaignSettings.CurrentSettings;
			if (selectedSub.HasTag(SubmarineTag.Shuttle) || !hasRequiredContentPackages)
			{
				if (!hasRequiredContentPackages)
				{
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ContentPackageMismatch"), TextManager.GetWithVariable("ContentPackageMismatchWarning", "[requiredcontentpackages]", string.Join(", ", selectedSub.RequiredContentPackages), FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
					GUIButton guibutton = msgBox.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object obj)
					{
						if (GUIMessageBox.MessageBoxes.Count == 0)
						{
							Action<SubmarineInfo, string, string, CampaignSettings> startNewGame2 = this.StartNewGame;
							if (startNewGame2 != null)
							{
								startNewGame2(selectedSub, savePath, this.seedBox.Text, settings);
							}
						}
						return true;
					}));
					msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
				}
				if (selectedSub.HasTag(SubmarineTag.Shuttle))
				{
					GUIMessageBox msgBox2 = new GUIMessageBox(TextManager.Get("ShuttleSelected"), TextManager.Get("ShuttleWarning"), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					msgBox2.Buttons[0].OnClicked = delegate(GUIButton button, object obj)
					{
						Action<SubmarineInfo, string, string, CampaignSettings> startNewGame2 = this.StartNewGame;
						if (startNewGame2 != null)
						{
							startNewGame2(selectedSub, savePath, this.seedBox.Text, settings);
						}
						return true;
					};
					GUIButton guibutton2 = msgBox2.Buttons[0];
					guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox2.Close));
					msgBox2.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox2.Close);
					return false;
				}
			}
			else
			{
				Action<SubmarineInfo, string, string, CampaignSettings> startNewGame = this.StartNewGame;
				if (startNewGame != null)
				{
					startNewGame(selectedSub, savePath, this.seedBox.Text, settings);
				}
			}
			return true;
		}

		// Token: 0x0600243B RID: 9275 RVA: 0x0016D5A4 File Offset: 0x0016B7A4
		public void RandomizeSeed()
		{
			this.seedBox.Text = ToolBox.RandomSeed(8);
		}

		// Token: 0x0600243C RID: 9276 RVA: 0x0016D5B8 File Offset: 0x0016B7B8
		private void FilterSubs(GUIListBox subList, string filter)
		{
			foreach (GUIComponent child in subList.Content.Children)
			{
				SubmarineInfo sub = child.UserData as SubmarineInfo;
				if (sub == null)
				{
					break;
				}
				child.Visible = (string.IsNullOrEmpty(filter) || sub.DisplayName.Contains(filter.ToLower(), StringComparison.OrdinalIgnoreCase));
			}
		}

		// Token: 0x0600243D RID: 9277 RVA: 0x0016D638 File Offset: 0x0016B838
		private bool OnSubSelected(GUIComponent component, object obj)
		{
			if (this.subPreviewContainer == null)
			{
				return false;
			}
			GUILayoutGroup guilayoutGroup = this.subPreviewContainer.Parent as GUILayoutGroup;
			if (guilayoutGroup != null)
			{
				guilayoutGroup.Recalculate();
			}
			this.subPreviewContainer.ClearChildren();
			SubmarineInfo sub = obj as SubmarineInfo;
			if (sub == null)
			{
				return true;
			}
			if (sub.Price > CampaignSettings.CurrentSettings.InitialMoney && !GameMain.DebugDraw)
			{
				this.SetPage(0);
				this.nextButton.Enabled = false;
				return false;
			}
			this.nextButton.Enabled = true;
			sub.CreatePreviewWindow(this.subPreviewContainer);
			return true;
		}

		// Token: 0x0600243E RID: 9278 RVA: 0x0016D6C8 File Offset: 0x0016B8C8
		public void CreateDefaultSaveName()
		{
			string savePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Singleplayer, "Save_Default");
			this.saveNameBox.Text = Path.GetFileNameWithoutExtension(savePath);
		}

		// Token: 0x0600243F RID: 9279 RVA: 0x0016D6F4 File Offset: 0x0016B8F4
		public void UpdateSubList(IEnumerable<SubmarineInfo> submarines)
		{
			List<SubmarineInfo> subsToShow;
			if (this.subFilter != CampaignSetupUI.CategoryFilter.All)
			{
				subsToShow = (from s in submarines
				where s.IsCampaignCompatibleIgnoreClass && s.IsVanillaSubmarine() == (this.subFilter == CampaignSetupUI.CategoryFilter.Vanilla)
				select s).ToList<SubmarineInfo>();
			}
			else
			{
				string downloadFolder = Path.GetFullPath(SaveUtil.SubmarineDownloadFolder);
				subsToShow = (from s in submarines
				where s.IsCampaignCompatibleIgnoreClass && Path.GetDirectoryName(Path.GetFullPath(s.FilePath)) != downloadFolder
				select s).ToList<SubmarineInfo>();
			}
			subsToShow.Sort(delegate(SubmarineInfo s1, SubmarineInfo s2)
			{
				int p = s1.Price;
				if (!s1.IsCampaignCompatible)
				{
					p += 100000;
				}
				int p2 = s2.Price;
				if (!s2.IsCampaignCompatible)
				{
					p2 += 100000;
				}
				return p.CompareTo(p2) * 100 + s1.Name.CompareTo(s2.Name);
			});
			this.subList.ClearChildren();
			foreach (SubmarineInfo sub in subsToShow)
			{
				GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.15f), this.subList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 30)
				}, ToolBox.LimitString(sub.DisplayName.Value, GUIStyle.Font, this.subList.Rect.Width - 65), null, null, Alignment.Left, false, "ListBoxElement", null)
				{
					ToolTip = sub.Description,
					UserData = sub
				};
				if (!sub.RequiredContentPackagesInstalled)
				{
					textBlock.TextColor = Color.Lerp(textBlock.TextColor, Color.DarkRed, 0.5f);
					textBlock.ToolTip = TextManager.Get("ContentPackageMismatch") + "\n\n" + textBlock.ToolTip.SanitizedString;
				}
				GUILayoutGroup infoContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				RectTransform rectT = new RectTransform(new Vector2(1f, 0.5f), infoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.GetWithVariable("currencyformat", "[credits]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", sub.Price), FormatCapitals.No);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.BottomRight, false, "", null);
				guitextBlock.TextColor = ((sub.Price > CampaignSettings.CurrentSettings.InitialMoney) ? GUIStyle.Red : (textBlock.TextColor * 0.8f));
				guitextBlock.ToolTip = textBlock.ToolTip;
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.5f), infoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("submarineclass.");
				defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(sub.SubmarineClass);
				RichString text2 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock2 = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.TopRight, false, "", null);
				guitextBlock2.TextColor = textBlock.TextColor * 0.8f;
				guitextBlock2.ToolTip = textBlock.ToolTip;
				if (!GameMain.DebugDraw && (sub.Price > CampaignSettings.CurrentSettings.InitialMoney || !sub.IsCampaignCompatible))
				{
					textBlock.CanBeFocused = false;
					textBlock.TextColor *= 0.5f;
				}
			}
			if (SubmarineInfo.SavedSubmarines.Any<SubmarineInfo>())
			{
				List<SubmarineInfo> validSubs = (from s in subsToShow
				where s.IsCampaignCompatible && s.Price <= CampaignSettings.CurrentSettings.InitialMoney
				select s).ToList<SubmarineInfo>();
				if (validSubs.Count > 0)
				{
					this.subList.Select(validSubs[Rand.Int(validSubs.Count, Rand.RandSync.Unsynced)], GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
				}
			}
		}

		// Token: 0x06002440 RID: 9280 RVA: 0x0016DB60 File Offset: 0x0016BD60
		public override void CreateLoadMenu(IEnumerable<CampaignMode.SaveInfo> saveFiles = null)
		{
			List<CampaignMode.SaveInfo> prevSaveFiles = this.prevSaveFiles;
			if (prevSaveFiles != null)
			{
				prevSaveFiles.Clear();
			}
			this.prevSaveFiles = null;
			this.loadGameContainer.ClearChildren();
			if (saveFiles == null)
			{
				saveFiles = SaveUtil.GetSaveFiles(SaveUtil.SaveType.Singleplayer, true, false);
			}
			GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), this.loadGameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.03f
			};
			base.CreateSaveFilteringHeader(leftColumn);
			this.saveList = new GUIListBox(new RectTransform(Vector2.One, leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = new GUIListBox.OnSelectedHandler(this.SelectSaveFile)
			};
			new GUIButton(new RectTransform(new Vector2(0.6f, 0.08f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("showinfolder"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				string saveFolder = SaveUtil.GetSaveFolder(SaveUtil.SaveType.Singleplayer);
				try
				{
					ToolBox.OpenFileWithShell(saveFolder);
				}
				catch (Exception e)
				{
					new GUIMessageBox(TextManager.Get("error"), TextManager.GetWithVariables("showinfoldererror", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[folder]", saveFolder),
						new ValueTuple<string, string>("[errormessage]", e.Message)
					}), null, null, GUIMessageBox.Type.Default);
				}
				return true;
			};
			foreach (CampaignMode.SaveInfo saveInfo in saveFiles)
			{
				GUIComponent saveFrame = base.CreateSaveElement(saveInfo);
				if (saveFrame != null)
				{
					XElement docRoot = SaveUtil.ExtractGameSessionRootElementFromSaveFile(saveInfo.FilePath, true);
					if (docRoot == null)
					{
						DebugConsole.ThrowError("Error loading save file \"" + saveInfo.FilePath + "\". The file may be corrupted.", null, null, false, false);
						saveFrame.GetChild<GUITextBlock>().TextColor = GUIStyle.Red;
					}
					else if (docRoot.GetAttributeBool("ismultiplayer", false))
					{
						this.saveList.Content.RemoveChild(saveFrame);
					}
					else if (!SaveUtil.IsSaveFileCompatible(docRoot))
					{
						saveFrame.GetChild<GUITextBlock>().TextColor = GUIStyle.Red;
						saveFrame.ToolTip = TextManager.Get("campaignmode.incompatiblesave");
					}
				}
			}
			base.SortSaveList(CampaignSetupUI.SaveSortingType.LastPlayedDescending);
			this.loadGameButton = new GUIButton(new RectTransform(new Vector2(0.45f, 0.12f), this.loadGameContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadButton"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object obj)
				{
					object selectedData = this.saveList.SelectedData;
					if (!(selectedData is CampaignMode.SaveInfo))
					{
						return false;
					}
					CampaignMode.SaveInfo saveInfo2 = (CampaignMode.SaveInfo)selectedData;
					if (string.IsNullOrWhiteSpace(saveInfo2.FilePath))
					{
						return false;
					}
					CampaignSetupUI.LoadGameDelegate loadGame = this.LoadGame;
					if (loadGame != null)
					{
						string filePath = saveInfo2.FilePath;
						Option.UnspecifiedNone none = Option.None;
						loadGame(filePath, none);
					}
					return true;
				},
				Enabled = false
			};
		}

		// Token: 0x06002441 RID: 9281 RVA: 0x0016DE4C File Offset: 0x0016C04C
		private bool SelectSaveFile(GUIComponent component, object obj)
		{
			if (!(obj is CampaignMode.SaveInfo))
			{
				return true;
			}
			CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)obj;
			string fileName = saveInfo.FilePath;
			XElement docRoot = SaveUtil.ExtractGameSessionRootElementFromSaveFile(fileName, true);
			if (docRoot == null)
			{
				DebugConsole.ThrowError("Error loading save file \"" + fileName + "\". The file may be corrupted.", null, null, false, false);
				return false;
			}
			this.loadGameButton.Enabled = SaveUtil.IsSaveFileCompatible(docRoot);
			this.RemoveSaveFrame();
			string subName = saveInfo.SubmarineName;
			LocalizedString saveTime = (from t in saveInfo.SaveTime
			select t.ToLocalUserString()).Fallback(TextManager.Get("Unknown"));
			string mapseed = docRoot.GetAttributeString("mapseed", "unknown");
			Identifier locationNameIdentifier = docRoot.GetAttributeIdentifier("currentlocation", Identifier.Empty);
			int locationNameFormatIndex = docRoot.GetAttributeInt("currentlocationnameformatindex", -1);
			Identifier locationType = docRoot.GetAttributeIdentifier("locationtype", Identifier.Empty);
			LevelData.LevelType levelType = docRoot.GetAttributeEnum("nextleveltype", LevelData.LevelType.LocationConnection);
			LocalizedString locationName = (locationType.IsEmpty || locationNameIdentifier.IsEmpty) ? LocalizedString.EmptyString : Location.GetName(locationType, locationNameFormatIndex, locationNameIdentifier);
			GUIFrame saveFileFrame = new GUIFrame(new RectTransform(new Vector2(0.45f, 0.6f), this.loadGameContainer.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, "InnerFrame", null)
			{
				UserData = "savefileframe"
			};
			RectTransform rectTransform = new RectTransform(new Vector2(0.9f, 0.2f), saveFileFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0f, 0.05f);
			RichString text = Path.GetFileNameWithoutExtension(fileName);
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock titleText = new GUITextBlock(rectTransform, text, null, font, Alignment.Center, false, "", null);
			titleText.Text = ToolBox.LimitString(titleText.Text, titleText.Font, titleText.Rect.Width);
			GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.5f), saveFileFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, false, Anchor.TopLeft);
			if (!locationName.IsNullOrEmpty())
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = locationName;
				font = GUIStyle.SmallFont;
				new GUITextBlock(rectT, text2, null, font, Alignment.Left, false, "", null);
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("savestate.");
				defaultInterpolatedStringHandler.AppendFormatted<LevelData.LevelType>(levelType);
				RichString text3 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				font = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text3, null, font, Alignment.Left, false, "", null);
				new GUIFrame(new RectTransform(new Vector2(0f, 0.05f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			}
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("Submarine"));
			defaultInterpolatedStringHandler2.AppendLiteral(" : ");
			defaultInterpolatedStringHandler2.AppendFormatted(subName);
			RichString text4 = defaultInterpolatedStringHandler2.ToStringAndClear();
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT3, text4, null, font, Alignment.Left, false, "", null);
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.Get("LastSaved"));
			defaultInterpolatedStringHandler3.AppendLiteral(" : ");
			defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(saveTime);
			RichString text5 = defaultInterpolatedStringHandler3.ToStringAndClear();
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT4, text5, null, font, Alignment.Left, false, "", null);
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(TextManager.Get("MapSeed"));
			defaultInterpolatedStringHandler4.AppendLiteral(" : ");
			defaultInterpolatedStringHandler4.AppendFormatted(mapseed);
			RichString text6 = defaultInterpolatedStringHandler4.ToStringAndClear();
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT5, text6, null, font, Alignment.Left, false, "", null);
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.85f, 0.15f), saveFileFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Delete"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.UserData = saveInfo;
			guibutton.OnClicked = new GUIButton.OnClickedHandler(base.DeleteSave);
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("rollbackbutton"), Alignment.Center, "GUIButtonSmall", null);
			guibutton2.UserData = saveInfo;
			guibutton2.ToolTip = TextManager.Get("backuptooltip");
			guibutton2.OnClicked = new GUIButton.OnClickedHandler(this.ViewBackupMenu);
			return true;
		}

		// Token: 0x06002442 RID: 9282 RVA: 0x0016E5A4 File Offset: 0x0016C7A4
		private bool ViewBackupMenu(GUIButton btn, object obj)
		{
			if (obj is CampaignMode.SaveInfo)
			{
				CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)obj;
				ImmutableArray<SaveUtil.BackupIndexData> indexData = SaveUtil.GetIndexData(saveInfo.FilePath);
				base.CreateBackupMenu(indexData, delegate(SaveUtil.BackupIndexData index)
				{
					this.LoadGame(saveInfo.FilePath, Option.Some<uint>(index.Index));
				});
				return true;
			}
			return false;
		}

		// Token: 0x06002443 RID: 9283 RVA: 0x0016E600 File Offset: 0x0016C800
		private void RemoveSaveFrame()
		{
			GUIComponent prevFrame = null;
			foreach (GUIComponent child in this.loadGameContainer.Children)
			{
				if (!(child.UserData as string != "savefileframe"))
				{
					prevFrame = child;
					break;
				}
			}
			this.loadGameContainer.RemoveChild(prevFrame);
		}

		// Token: 0x06002444 RID: 9284 RVA: 0x0016E674 File Offset: 0x0016C874
		[CompilerGenerated]
		private GUILayoutGroup <CreateNewGameMenu>g__createPageLayout|13_0()
		{
			GUIFrame containerItem = new GUIFrame(new RectTransform(Vector2.One, this.pageContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			return new GUILayoutGroup(new RectTransform(Vector2.One * 0.95f, containerItem.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
		}

		// Token: 0x0400120C RID: 4620
		private GUIListBox subList;

		// Token: 0x0400120D RID: 4621
		protected GUILayoutGroup subPreviewContainer;

		// Token: 0x0400120F RID: 4623
		private GUIButton nextButton;

		// Token: 0x04001210 RID: 4624
		private GUIListBox characterInfoColumns;

		// Token: 0x04001211 RID: 4625
		private int currentPage;

		// Token: 0x04001212 RID: 4626
		private GUIListBox pageContainer;
	}
}
