using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000100 RID: 256
	internal abstract class CampaignSetupUI
	{
		// Token: 0x170009C8 RID: 2504
		// (get) Token: 0x0600240D RID: 9229 RVA: 0x00168BA0 File Offset: 0x00166DA0
		// (set) Token: 0x0600240E RID: 9230 RVA: 0x00168BA8 File Offset: 0x00166DA8
		public GUIButton StartButton { get; protected set; }

		// Token: 0x170009C9 RID: 2505
		// (get) Token: 0x0600240F RID: 9231 RVA: 0x00168BB1 File Offset: 0x00166DB1
		// (set) Token: 0x06002410 RID: 9232 RVA: 0x00168BB9 File Offset: 0x00166DB9
		public GUITextBlock InitialMoneyText { get; protected set; }

		// Token: 0x170009CA RID: 2506
		// (get) Token: 0x06002411 RID: 9233 RVA: 0x00168BC2 File Offset: 0x00166DC2
		// (set) Token: 0x06002412 RID: 9234 RVA: 0x00168BCA File Offset: 0x00166DCA
		public GUIButton CampaignCustomizeButton { get; set; }

		// Token: 0x170009CB RID: 2507
		// (get) Token: 0x06002413 RID: 9235 RVA: 0x00168BD3 File Offset: 0x00166DD3
		// (set) Token: 0x06002414 RID: 9236 RVA: 0x00168BDB File Offset: 0x00166DDB
		public GUIMessageBox CampaignCustomizeSettings { get; set; }

		// Token: 0x06002415 RID: 9237 RVA: 0x00168BE4 File Offset: 0x00166DE4
		public CampaignSetupUI(GUIComponent newGameContainer, GUIComponent loadGameContainer)
		{
			this.newGameContainer = newGameContainer;
			this.loadGameContainer = loadGameContainer;
		}

		// Token: 0x06002416 RID: 9238 RVA: 0x00168BFC File Offset: 0x00166DFC
		protected GUIComponent CreateSaveElement(CampaignMode.SaveInfo saveInfo)
		{
			if (string.IsNullOrEmpty(saveInfo.FilePath))
			{
				DebugConsole.AddWarning("Error when updating campaign load menu: path to a save file was empty.\n" + Environment.StackTrace, null);
				return null;
			}
			GUIFrame saveFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), this.saveList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 45)
			}, "ListBoxElement", null)
			{
				UserData = saveInfo
			};
			GUITextBlock nameText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), saveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Path.GetFileNameWithoutExtension(saveInfo.FilePath), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null)
			{
				CanBeFocused = false
			};
			LocalizedString errorMsg;
			if (new ImmutableArray<string>?(saveInfo.EnabledContentPackageNames) != null && saveInfo.EnabledContentPackageNames.Any<string>() && !GameSession.IsCompatibleWithEnabledContentPackages(saveInfo.EnabledContentPackageNames, out errorMsg))
			{
				nameText.TextColor = GUIStyle.Red;
				saveFrame.ToolTip = string.Join("\n", new object[]
				{
					errorMsg,
					TextManager.Get("campaignmode.contentpackagemismatchwarning")
				});
			}
			if (this.prevSaveFiles == null)
			{
				this.prevSaveFiles = new List<CampaignMode.SaveInfo>();
			}
			this.prevSaveFiles.Add(saveInfo);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.5f), saveFrame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal);
			RichString text = saveInfo.SubmarineName;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
			guitextBlock.CanBeFocused = false;
			guitextBlock.UserData = saveInfo.FilePath;
			string saveTimeStr = string.Empty;
			SerializableDateTime time;
			if (saveInfo.SaveTime.TryUnwrap(out time))
			{
				saveTimeStr = time.ToLocalUserString();
			}
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 1f), saveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = saveTimeStr;
			smallFont = GUIStyle.SmallFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Right, false, "", null);
			guitextBlock2.CanBeFocused = false;
			guitextBlock2.UserData = saveInfo.FilePath;
			return saveFrame;
		}

		// Token: 0x06002417 RID: 9239 RVA: 0x00168ED8 File Offset: 0x001670D8
		protected void SortSaveList(CampaignSetupUI.SaveSortingType sortingType = CampaignSetupUI.SaveSortingType.LastPlayedDescending)
		{
			GUIListBox guilistBox = this.saveList;
			if (guilistBox == null)
			{
				return;
			}
			guilistBox.Content.RectTransform.SortChildren(delegate(RectTransform rect1, RectTransform rect2)
			{
				object userData = rect1.GUIComponent.UserData;
				if (userData is CampaignMode.SaveInfo)
				{
					CampaignMode.SaveInfo file = (CampaignMode.SaveInfo)userData;
					userData = rect2.GUIComponent.UserData;
					if (userData is CampaignMode.SaveInfo)
					{
						CampaignMode.SaveInfo file2 = (CampaignMode.SaveInfo)userData;
						SerializableDateTime file1WriteTime;
						SerializableDateTime file2WriteTime;
						if (!file.SaveTime.TryUnwrap(out file1WriteTime) || !file2.SaveTime.TryUnwrap(out file2WriteTime))
						{
							return 0;
						}
						int result;
						switch (sortingType)
						{
						case CampaignSetupUI.SaveSortingType.LastPlayedDescending:
							result = file2WriteTime.CompareTo(file1WriteTime);
							break;
						case CampaignSetupUI.SaveSortingType.LastPlayedAscending:
							result = file1WriteTime.CompareTo(file2WriteTime);
							break;
						case CampaignSetupUI.SaveSortingType.NameDescending:
							result = string.Compare(Path.GetFileNameWithoutExtension(file.FilePath), Path.GetFileNameWithoutExtension(file2.FilePath), StringComparison.OrdinalIgnoreCase);
							break;
						case CampaignSetupUI.SaveSortingType.NameAscending:
							result = string.Compare(Path.GetFileNameWithoutExtension(file2.FilePath), Path.GetFileNameWithoutExtension(file.FilePath), StringComparison.OrdinalIgnoreCase);
							break;
						default:
							result = 0;
							break;
						}
						return result;
					}
				}
				return 0;
			});
		}

		// Token: 0x06002418 RID: 9240 RVA: 0x00168F18 File Offset: 0x00167118
		protected void CreateSaveFilteringHeader(GUIComponent parent)
		{
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(Vector2.UnitX, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUI.CreateFilterBox(new RectTransform(new Vector2(0.6f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)).OnTextChanged += delegate(GUITextBox _, string filterText)
			{
				filterText = filterText.Trim();
				foreach (GUIComponent saveElement in this.saveList.Content.Children)
				{
					object userData = saveElement.UserData;
					if (userData is CampaignMode.SaveInfo)
					{
						CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)userData;
						saveElement.Visible = (filterText.IsNullOrEmpty() || Path.GetFileNameWithoutExtension(saveInfo.FilePath).Contains(filterText, StringComparison.OrdinalIgnoreCase));
					}
				}
				return true;
			};
			CampaignSetupUI.SaveSortingType[] sortingTypes = Enum.GetValues<CampaignSetupUI.SaveSortingType>();
			GUIDropDown dropDown = new GUIDropDown(new RectTransform(new Vector2(0.4f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, sortingTypes.Length, "", false, false, Alignment.CenterLeft, 1f)
			{
				OnSelected = delegate(GUIComponent _, object data)
				{
					this.SortSaveList((CampaignSetupUI.SaveSortingType)data);
					return true;
				}
			};
			foreach (CampaignSetupUI.SaveSortingType sortingType in sortingTypes)
			{
				GUIDropDown guidropDown = dropDown;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler.AppendLiteral("SaveSortingType.");
				defaultInterpolatedStringHandler.AppendFormatted<CampaignSetupUI.SaveSortingType>(sortingType);
				guidropDown.AddItem(TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), sortingType, null, null, null);
			}
			dropDown.SelectItem(CampaignSetupUI.SaveSortingType.LastPlayedDescending);
			container.RectTransform.MinSize = new ValueTuple<int, int>(0, container.Children.Max((GUIComponent child) => child.Rect.Size.Y));
		}

		// Token: 0x06002419 RID: 9241 RVA: 0x001690D4 File Offset: 0x001672D4
		protected static CampaignSetupUI.CampaignSettingElements CreateCampaignSettingList(GUIComponent parent, CampaignSettings prevSettings, bool isSinglePlayer)
		{
			CampaignSetupUI.<>c__DisplayClass37_0 CS$<>8__locals1 = new CampaignSetupUI.<>c__DisplayClass37_0();
			CS$<>8__locals1.prevSettings = prevSettings;
			CS$<>8__locals1.isSinglePlayer = isSinglePlayer;
			CS$<>8__locals1.loadingPreset = false;
			GUILayoutGroup presetDropdownLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.14f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), presetDropdownLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignsettingpreset"), null, null, Alignment.Left, false, "", null);
			CS$<>8__locals1.presetDropdown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), presetDropdownLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, CampaignModePresets.List.Length + 1, "", false, false, Alignment.CenterLeft, 1f);
			CS$<>8__locals1.presetDropdown.AddItem(TextManager.Get("karmapreset.custom"), null, null, null, null);
			CS$<>8__locals1.presetDropdown.Select(0);
			presetDropdownLayout.RectTransform.MinSize = new Point(0, CS$<>8__locals1.presetDropdown.Rect.Height);
			foreach (CampaignSettings settings in CampaignModePresets.List)
			{
				string name = settings.PresetName;
				CS$<>8__locals1.presetDropdown.AddItem(TextManager.Get("preset." + name).Fallback(name, true), settings, null, null, null);
				if (settings.PresetName.Equals(CS$<>8__locals1.prevSettings.PresetName, StringComparison.OrdinalIgnoreCase))
				{
					CS$<>8__locals1.presetDropdown.SelectItem(settings);
				}
			}
			CampaignSetupUI.SettingValue<string> presetValue = new CampaignSetupUI.SettingValue<string>(delegate()
			{
				CampaignSettings settings2 = CS$<>8__locals1.presetDropdown.SelectedData as CampaignSettings;
				if (settings2 == null)
				{
					return string.Empty;
				}
				return settings2.PresetName;
			}, delegate(string _)
			{
			});
			GUIListBox settingsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.86f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = GUI.IntScale(5f)
			};
			NetLobbyScreen.CreateSubHeader("campaignsettingcategories.general", settingsList.Content, null);
			CampaignSetupUI.<>c__DisplayClass37_0 CS$<>8__locals2 = CS$<>8__locals1;
			CampaignSetupUI.SettingValue<bool> tutorialEnabled;
			if (!CS$<>8__locals1.isSinglePlayer)
			{
				tutorialEnabled = new CampaignSetupUI.SettingValue<bool>(() => false, delegate(bool _)
				{
				});
			}
			else
			{
				tutorialEnabled = CampaignSetupUI.<CreateCampaignSettingList>g__CreateTickbox|37_11(settingsList.Content, TextManager.Get("CampaignOption.EnableTutorial"), TextManager.Get("campaignoption.enabletutorial.tooltip"), CS$<>8__locals1.prevSettings.TutorialEnabled, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			}
			CS$<>8__locals2.tutorialEnabled = tutorialEnabled;
			CS$<>8__locals1.radiationEnabled = CampaignSetupUI.<CreateCampaignSettingList>g__CreateTickbox|37_11(settingsList.Content, TextManager.Get("CampaignOption.EnableRadiation"), TextManager.Get("campaignoption.enableradiation.tooltip"), CS$<>8__locals1.prevSettings.RadiationEnabled, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			NetLobbyScreen.CreateSubHeader("campaignsettingcategories.resources", settingsList.Content, null);
			ImmutableArray<CampaignSetupUI.SettingCarouselElement<Identifier>> startingSetOptions = (from s in StartItemSet.Sets
			orderby s.Order
			select s).Select(delegate(StartItemSet set)
			{
				Identifier identifier = set.Identifier;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler.AppendLiteral("startitemset.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(set.Identifier);
				return new CampaignSetupUI.SettingCarouselElement<Identifier>(identifier, defaultInterpolatedStringHandler.ToStringAndClear(), false);
			}).ToImmutableArray<CampaignSetupUI.SettingCarouselElement<Identifier>>();
			CampaignSetupUI.SettingCarouselElement<Identifier> prevStartingSet = startingSetOptions.FirstOrNull(delegate(CampaignSetupUI.SettingCarouselElement<Identifier> element)
			{
				Identifier startItemSet = CS$<>8__locals1.prevSettings.StartItemSet;
				return element.Value == startItemSet;
			}) ?? startingSetOptions[1];
			CS$<>8__locals1.startingSetInput = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSelectionCarousel|37_10<Identifier>(settingsList.Content, TextManager.Get("startitemset"), TextManager.Get("startitemsettooltip"), prevStartingSet, 0.14f, startingSetOptions, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			ImmutableArray<CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption>> fundOptions = ImmutableArray.Create<CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption>>(new CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption>(StartingBalanceAmountOption.Low, "startingfunds.low", false), new CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption>(StartingBalanceAmountOption.Medium, "startingfunds.medium", false), new CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption>(StartingBalanceAmountOption.High, "startingfunds.high", false));
			CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption> prevStartingFund = fundOptions.FirstOrNull((CampaignSetupUI.SettingCarouselElement<StartingBalanceAmountOption> element) => element.Value == CS$<>8__locals1.prevSettings.StartingBalanceAmount) ?? fundOptions[1];
			CS$<>8__locals1.startingFundsInput = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSelectionCarousel|37_10<StartingBalanceAmountOption>(settingsList.Content, TextManager.Get("startingfundsdescription"), TextManager.Get("startingfundstooltip"), prevStartingFund, 0.14f, fundOptions, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CS$<>8__locals1.maxMissionCountInput = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIIntegerInputCarousel|37_8(settingsList.Content, TextManager.Get("maxmissioncount"), TextManager.Get("maxmissioncounttooltip"), CS$<>8__locals1.prevSettings.MaxMissionCount, 1, 1, 10, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings rewardMultiplierSettings = CampaignSettings.GetMultiplierSettings("MissionRewardMultiplier");
			CS$<>8__locals1.rewardMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.missionrewardmultiplier"), TextManager.Get("campaignoption.missionrewardmultiplier.tooltip"), CS$<>8__locals1.prevSettings.MissionRewardMultiplier, rewardMultiplierSettings.Step, rewardMultiplierSettings.Min, rewardMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings experienceMultiplierSettings = CampaignSettings.GetMultiplierSettings("ExperienceRewardMultiplier");
			CS$<>8__locals1.experienceMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.experiencerewardmultiplier"), TextManager.Get("campaignoption.experiencerewardmultiplier.tooltip"), CS$<>8__locals1.prevSettings.ExperienceRewardMultiplier, experienceMultiplierSettings.Step, experienceMultiplierSettings.Min, experienceMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings shopPriceMultiplierSettings = CampaignSettings.GetMultiplierSettings("ShopPriceMultiplier");
			CS$<>8__locals1.shopPriceMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.shoppricemultiplier"), TextManager.Get("campaignoption.shoppricemultiplier.tooltip"), CS$<>8__locals1.prevSettings.ShopPriceMultiplier, shopPriceMultiplierSettings.Step, shopPriceMultiplierSettings.Min, shopPriceMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings shipyardPriceMultiplierSettings = CampaignSettings.GetMultiplierSettings("ShipyardPriceMultiplier");
			CS$<>8__locals1.shipyardPriceMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.shipyardpricemultiplier"), TextManager.Get("campaignoption.shipyardpricemultiplier.tooltip"), CS$<>8__locals1.prevSettings.ShipyardPriceMultiplier, shipyardPriceMultiplierSettings.Step, shipyardPriceMultiplierSettings.Min, shipyardPriceMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			NetLobbyScreen.CreateSubHeader("campaignsettingcategories.hazards", settingsList.Content, null);
			ImmutableArray<CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>> hostilityOptions = ImmutableArray.Create<CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>>(new CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>(WorldHostilityOption.Low, "worldhostility.low", false), new CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>(WorldHostilityOption.Medium, "worldhostility.medium", false), new CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>(WorldHostilityOption.High, "worldhostility.high", false), new CampaignSetupUI.SettingCarouselElement<WorldHostilityOption>(WorldHostilityOption.Hellish, "worldhostility.hellish", true));
			CampaignSetupUI.SettingCarouselElement<WorldHostilityOption> prevHostility = hostilityOptions.FirstOrNull((CampaignSetupUI.SettingCarouselElement<WorldHostilityOption> element) => element.Value == CS$<>8__locals1.prevSettings.WorldHostility) ?? hostilityOptions[1];
			CS$<>8__locals1.hostilityInput = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSelectionCarousel|37_10<WorldHostilityOption>(settingsList.Content, TextManager.Get("worldhostility"), TextManager.Get("worldhostility.tooltip"), prevHostility, 0.14f, hostilityOptions, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings crewVitalityMultiplierSettings = CampaignSettings.GetMultiplierSettings("CrewVitalityMultiplier");
			CS$<>8__locals1.crewVitalityMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.maxvitalitymultipliercrew"), TextManager.Get("campaignoption.maxvitalitymultipliercrew.tooltip"), CS$<>8__locals1.prevSettings.CrewVitalityMultiplier, crewVitalityMultiplierSettings.Step, crewVitalityMultiplierSettings.Min, crewVitalityMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings nonCrewVitalityMultiplierSettings = CampaignSettings.GetMultiplierSettings("NonCrewVitalityMultiplier");
			CS$<>8__locals1.nonCrewVitalityMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.maxvitalitymultipliernoncrew"), TextManager.Get("campaignoption.maxvitalitymultipliernoncrew.tooltip"), CS$<>8__locals1.prevSettings.NonCrewVitalityMultiplier, nonCrewVitalityMultiplierSettings.Step, nonCrewVitalityMultiplierSettings.Min, nonCrewVitalityMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings oxygenSourceMultiplierSettings = CampaignSettings.GetMultiplierSettings("OxygenMultiplier");
			CS$<>8__locals1.oxygenMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.oxygensourcemultiplier"), TextManager.Get("campaignoption.oxygensourcemultiplier.tooltip"), CS$<>8__locals1.prevSettings.OxygenMultiplier, oxygenSourceMultiplierSettings.Step, oxygenSourceMultiplierSettings.Min, oxygenSourceMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings reactorFuelMultiplierSettings = CampaignSettings.GetMultiplierSettings("FuelMultiplier");
			CS$<>8__locals1.fuelMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.reactorfuelmultiplier"), TextManager.Get("campaignoption.reactorfuelmultiplier.tooltip"), CS$<>8__locals1.prevSettings.FuelMultiplier, reactorFuelMultiplierSettings.Step, reactorFuelMultiplierSettings.Min, reactorFuelMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CampaignSettings.MultiplierSettings repairFailMultiplierSettings = CampaignSettings.GetMultiplierSettings("RepairFailMultiplier");
			CS$<>8__locals1.repairFailMultiplier = CampaignSetupUI.<CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(settingsList.Content, TextManager.Get("campaignoption.repairfailmultiplier"), TextManager.Get("campaignoption.repairfailmultiplier.tooltip"), CS$<>8__locals1.prevSettings.RepairFailMultiplier, repairFailMultiplierSettings.Step, repairFailMultiplierSettings.Min, repairFailMultiplierSettings.Max, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			ImmutableArray<CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>> patdownProbabilityPresets = ImmutableArray.Create<CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>>(new CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>(PatdownProbabilityOption.Off, "probability.off", false), new CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>(PatdownProbabilityOption.Low, "probability.low", false), new CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>(PatdownProbabilityOption.Medium, "probability.medium", false), new CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption>(PatdownProbabilityOption.High, "probability.high", false));
			CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption> prevPatdownProbability = patdownProbabilityPresets.FirstOrNull((CampaignSetupUI.SettingCarouselElement<PatdownProbabilityOption> element) => element.Value == CS$<>8__locals1.prevSettings.PatdownProbability) ?? patdownProbabilityPresets[1];
			CS$<>8__locals1.patdownProbability = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSelectionCarousel|37_10<PatdownProbabilityOption>(settingsList.Content, TextManager.Get("campaignoption.patdownprobability"), TextManager.Get("campaignoption.patdownprobability.tooltip"), prevPatdownProbability, 0.14f, patdownProbabilityPresets, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CS$<>8__locals1.huskWarning = CampaignSetupUI.<CreateCampaignSettingList>g__CreateTickbox|37_11(settingsList.Content, TextManager.Get("campaignoption.showhuskwarning"), TextManager.Get("campaignoption.showhuskwarning.tooltip"), CS$<>8__locals1.prevSettings.ShowHuskWarning, 0.14f, new Action(CS$<>8__locals1.<CreateCampaignSettingList>g__OnValuesChanged|7));
			CS$<>8__locals1.presetDropdown.OnSelected = delegate(GUIComponent _, object o)
			{
				CampaignSettings settings2 = o as CampaignSettings;
				if (settings2 == null)
				{
					return false;
				}
				CS$<>8__locals1.loadingPreset = true;
				CS$<>8__locals1.tutorialEnabled.SetValue(CS$<>8__locals1.isSinglePlayer && settings2.TutorialEnabled);
				CS$<>8__locals1.radiationEnabled.SetValue(settings2.RadiationEnabled);
				CS$<>8__locals1.maxMissionCountInput.SetValue(settings2.MaxMissionCount);
				CS$<>8__locals1.startingFundsInput.SetValue(settings2.StartingBalanceAmount);
				CS$<>8__locals1.hostilityInput.SetValue(settings2.WorldHostility);
				CS$<>8__locals1.startingSetInput.SetValue(settings2.StartItemSet);
				CS$<>8__locals1.crewVitalityMultiplier.SetValue(settings2.CrewVitalityMultiplier);
				CS$<>8__locals1.nonCrewVitalityMultiplier.SetValue(settings2.NonCrewVitalityMultiplier);
				CS$<>8__locals1.oxygenMultiplier.SetValue(settings2.OxygenMultiplier);
				CS$<>8__locals1.fuelMultiplier.SetValue(settings2.FuelMultiplier);
				CS$<>8__locals1.rewardMultiplier.SetValue(settings2.MissionRewardMultiplier);
				CS$<>8__locals1.experienceMultiplier.SetValue(settings2.ExperienceRewardMultiplier);
				CS$<>8__locals1.shopPriceMultiplier.SetValue(settings2.ShopPriceMultiplier);
				CS$<>8__locals1.shipyardPriceMultiplier.SetValue(settings2.ShipyardPriceMultiplier);
				CS$<>8__locals1.repairFailMultiplier.SetValue(settings2.RepairFailMultiplier);
				CS$<>8__locals1.patdownProbability.SetValue(settings2.PatdownProbability);
				CS$<>8__locals1.huskWarning.SetValue(settings2.ShowHuskWarning);
				CS$<>8__locals1.loadingPreset = false;
				return true;
			};
			return new CampaignSetupUI.CampaignSettingElements
			{
				SelectedPreset = presetValue,
				TutorialEnabled = CS$<>8__locals1.tutorialEnabled,
				RadiationEnabled = CS$<>8__locals1.radiationEnabled,
				MaxMissionCount = CS$<>8__locals1.maxMissionCountInput,
				StartingFunds = CS$<>8__locals1.startingFundsInput,
				WorldHostility = CS$<>8__locals1.hostilityInput,
				StartItemSet = CS$<>8__locals1.startingSetInput,
				CrewVitalityMultiplier = CS$<>8__locals1.crewVitalityMultiplier,
				NonCrewVitalityMultiplier = CS$<>8__locals1.nonCrewVitalityMultiplier,
				OxygenMultiplier = CS$<>8__locals1.oxygenMultiplier,
				FuelMultiplier = CS$<>8__locals1.fuelMultiplier,
				MissionRewardMultiplier = CS$<>8__locals1.rewardMultiplier,
				ExperienceRewardMultiplier = CS$<>8__locals1.experienceMultiplier,
				ShopPriceMultiplier = CS$<>8__locals1.shopPriceMultiplier,
				ShipyardPriceMultiplier = CS$<>8__locals1.shipyardPriceMultiplier,
				RepairFailMultiplier = CS$<>8__locals1.repairFailMultiplier,
				PatdownProbability = CS$<>8__locals1.patdownProbability,
				ShowHuskWarning = CS$<>8__locals1.huskWarning
			};
		}

		// Token: 0x0600241A RID: 9242
		public abstract void CreateLoadMenu(IEnumerable<CampaignMode.SaveInfo> saveFiles = null);

		// Token: 0x0600241B RID: 9243 RVA: 0x00169C3C File Offset: 0x00167E3C
		protected bool DeleteSave(GUIButton button, object obj)
		{
			if (obj is CampaignMode.SaveInfo)
			{
				CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)obj;
				LocalizedString header = TextManager.Get("deletedialoglabel");
				LocalizedString body = TextManager.GetWithVariable("deletedialogquestion", "[file]", Path.GetFileNameWithoutExtension(saveInfo.FilePath), FormatCapitals.No);
				Predicate<CampaignMode.SaveInfo> <>9__1;
				EventEditorScreen.AskForConfirmation(header, body, delegate
				{
					SaveUtil.DeleteSave(saveInfo.FilePath);
					List<CampaignMode.SaveInfo> list = this.prevSaveFiles;
					if (list != null)
					{
						Predicate<CampaignMode.SaveInfo> match;
						if ((match = <>9__1) == null)
						{
							match = (<>9__1 = ((CampaignMode.SaveInfo s) => s.FilePath == saveInfo.FilePath));
						}
						list.RemoveAll(match);
					}
					this.CreateLoadMenu(this.prevSaveFiles.ToList<CampaignMode.SaveInfo>());
					return true;
				}, null);
				return true;
			}
			return false;
		}

		// Token: 0x0600241C RID: 9244 RVA: 0x00169CC0 File Offset: 0x00167EC0
		protected void CreateBackupMenu(IEnumerable<SaveUtil.BackupIndexData> indexData, Action<SaveUtil.BackupIndexData> loadBackup)
		{
			GUIMessageBox backupPopup = new GUIMessageBox("", "", new LocalizedString[]
			{
				TextManager.Get("Load"),
				TextManager.Get("Cancel")
			}, new Vector2?(new Vector2(0.3f, 0.5f)), new Point?(new Point(500, 500)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup campaignSettingContent = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.8f), backupPopup.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIListBox backupList = new GUIListBox(new RectTransform(Vector2.One, campaignSettingContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			bool isIronman = serverSettings != null && serverSettings.IronmanModeActive;
			if (!indexData.Any<SaveUtil.BackupIndexData>() || isIronman)
			{
				LocalizedString errorMsg = isIronman ? TextManager.Get("ironmanmodebackupdisclaimer") : TextManager.Get("nobackups");
				RectTransform rectT = new RectTransform(Vector2.One, campaignSettingContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = errorMsg;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				GUITextBlock errorBlock = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null)
				{
					TextColor = GUIStyle.Red,
					IgnoreLayoutGroups = true
				};
				if (errorBlock.Font.MeasureString(errorMsg, false).X > (float)campaignSettingContent.Rect.Width)
				{
					errorBlock.Wrap = true;
					errorBlock.SetTextPos();
				}
			}
			if (!isIronman)
			{
				foreach (SaveUtil.BackupIndexData data in from i in indexData
				orderby i.SaveTime descending
				select i)
				{
					GUIFrame indexFrame = new GUIFrame(new RectTransform(new Vector2(1f, 1f / (float)SaveUtil.MaxBackupCount), backupList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
					{
						UserData = data
					};
					GUILayoutGroup indexLayout = new GUILayoutGroup(new RectTransform(Vector2.One, indexFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						RelativeSpacing = 0.05f,
						Stretch = true
					};
					GUILayoutGroup leftLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.8f), indexLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
					{
						RelativeSpacing = 0.05f,
						Stretch = true
					};
					LocalizedString locationName = (data.LocationType.IsEmpty || data.LocationNameIdentifier.IsEmpty) ? TextManager.Get("unknown") : Location.GetName(data.LocationType, data.LocationNameFormatIndex, data.LocationNameIdentifier);
					GUITextBlock locationNameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), leftLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), locationName, null, null, Alignment.CenterLeft, false, "", null)
					{
						TextColor = Color.White
					};
					RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.5f), leftLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler.AppendLiteral("savestate.");
					defaultInterpolatedStringHandler.AppendFormatted<LevelData.LevelType>(data.LevelType);
					new GUITextBlock(rectT2, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), null, null, Alignment.CenterLeft, false, "", null);
					GUILayoutGroup rightLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.8f), indexLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
					{
						RelativeSpacing = 0.05f,
						Stretch = true
					};
					SerializableDateTime utcNow = SerializableDateTime.UtcNow;
					SerializableDateTime saveTime = data.SaveTime;
					TimeSpan difference = utcNow - saveTime;
					double totalMinutes = difference.TotalMinutes;
					LocalizedString localizedString;
					if (totalMinutes >= 1.0)
					{
						if (totalMinutes <= 60.0)
						{
							localizedString = TextManager.GetWithVariable("subeditor.saveageminutes", "[minutes]", difference.Minutes.ToString(), FormatCapitals.No);
						}
						else
						{
							localizedString = TextManager.GetWithVariable("saveagehours", "[hours]", ((int)Math.Floor(difference.TotalHours)).ToString(), FormatCapitals.No);
						}
					}
					else
					{
						localizedString = TextManager.Get("subeditor.savedjustnow");
					}
					LocalizedString timeFormat = localizedString;
					new GUITextBlock(new RectTransform(Vector2.One, rightLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), timeFormat, null, null, Alignment.CenterRight, false, "", null);
					locationNameBlock.Text = ToolBox.LimitString(locationName, locationNameBlock.Font, locationNameBlock.Rect.Width);
				}
			}
			backupList.AfterSelected = delegate(GUIComponent selected, object _)
			{
				backupPopup.Buttons[0].Enabled = true;
				return true;
			};
			GUIButton guibutton = backupPopup.Buttons[1];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
			{
				backupPopup.Close();
				return true;
			}));
			backupPopup.Buttons[0].Enabled = false;
			GUIButton guibutton2 = backupPopup.Buttons[0];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
			{
				GUIComponent selectedComponent = backupList.SelectedComponent;
				object obj = (selectedComponent != null) ? selectedComponent.UserData : null;
				if (obj is SaveUtil.BackupIndexData)
				{
					SaveUtil.BackupIndexData selectedIndexData = (SaveUtil.BackupIndexData)obj;
					backupPopup.Close();
					Action<SaveUtil.BackupIndexData> loadBackup2 = loadBackup;
					if (loadBackup2 != null)
					{
						loadBackup2(selectedIndexData);
					}
					return true;
				}
				return false;
			}));
		}

		// Token: 0x0600241F RID: 9247 RVA: 0x0016A464 File Offset: 0x00168664
		[CompilerGenerated]
		internal static CampaignSetupUI.SettingValue<int> <CreateCampaignSettingList>g__CreateGUIIntegerInputCarousel|37_8(GUIComponent parent, LocalizedString description, LocalizedString tooltip, int defaultValue, int valueStep, int minValue, int maxValue, float verticalSize, Action onChanged)
		{
			CampaignSetupUI.<>c__DisplayClass37_1 CS$<>8__locals1 = new CampaignSetupUI.<>c__DisplayClass37_1();
			CS$<>8__locals1.onChanged = onChanged;
			GUILayoutGroup inputContainer = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSettingBase|37_12(parent, description, tooltip, 0.55f, verticalSize);
			GUIButton minusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIMinusButton", null);
			RectTransform numberInputRect = new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			GUIButton plusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIPlusButton", null);
			CampaignSetupUI.<>c__DisplayClass37_1 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = numberInputRect;
			NumberType inputType = NumberType.Int;
			string style = "GUITextBox";
			Alignment textAlignment = Alignment.Center;
			ValueTuple<GUIButton, GUIButton>? customPlusMinusButtons = new ValueTuple<GUIButton, GUIButton>?(new ValueTuple<GUIButton, GUIButton>(plusButton, minusButton));
			CS$<>8__locals2.numberInput = new GUINumberInput(rectT, inputType, style, textAlignment, null, GUINumberInput.ButtonVisibility.ForceVisible, customPlusMinusButtons)
			{
				IntValue = defaultValue,
				MinValueInt = new int?(minValue),
				MaxValueInt = new int?(maxValue),
				ValueStep = (float)valueStep,
				ToolTip = tooltip
			};
			inputContainer.RectTransform.Parent.MinSize = new Point(0, CS$<>8__locals1.numberInput.RectTransform.MinSize.Y);
			GUINumberInput numberInput = CS$<>8__locals1.numberInput;
			numberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(numberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput _)
			{
				CS$<>8__locals1.onChanged();
			}));
			return new CampaignSetupUI.SettingValue<int>(() => CS$<>8__locals1.numberInput.IntValue, delegate(int i)
			{
				CS$<>8__locals1.numberInput.IntValue = i;
			});
		}

		// Token: 0x06002420 RID: 9248 RVA: 0x0016A624 File Offset: 0x00168824
		[CompilerGenerated]
		internal static CampaignSetupUI.SettingValue<float> <CreateCampaignSettingList>g__CreateGUIFloatInputCarousel|37_9(GUIComponent parent, LocalizedString description, LocalizedString tooltip, float defaultValue, float valueStep, float minValue, float maxValue, float verticalSize, Action onChanged)
		{
			CampaignSetupUI.<>c__DisplayClass37_2 CS$<>8__locals1 = new CampaignSetupUI.<>c__DisplayClass37_2();
			CS$<>8__locals1.onChanged = onChanged;
			GUILayoutGroup inputContainer = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSettingBase|37_12(parent, description, tooltip, 0.55f, verticalSize);
			GUIButton minusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIMinusButton", null);
			RectTransform numberInputRect = new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			GUIButton plusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIPlusButton", null);
			CampaignSetupUI.<>c__DisplayClass37_2 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = numberInputRect;
			NumberType inputType = NumberType.Float;
			string style = "GUITextBox";
			Alignment textAlignment = Alignment.Center;
			ValueTuple<GUIButton, GUIButton>? customPlusMinusButtons = new ValueTuple<GUIButton, GUIButton>?(new ValueTuple<GUIButton, GUIButton>(plusButton, minusButton));
			CS$<>8__locals2.numberInput = new GUINumberInput(rectT, inputType, style, textAlignment, null, GUINumberInput.ButtonVisibility.ForceVisible, customPlusMinusButtons)
			{
				FloatValue = defaultValue,
				MinValueFloat = new float?(minValue),
				MaxValueFloat = new float?(maxValue),
				ValueStep = valueStep,
				ToolTip = tooltip
			};
			CS$<>8__locals1.numberInput.RectTransform.Parent.MinSize = new Point(0, CS$<>8__locals1.numberInput.RectTransform.MinSize.Y);
			GUINumberInput numberInput = CS$<>8__locals1.numberInput;
			numberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(numberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput _)
			{
				CS$<>8__locals1.onChanged();
			}));
			return new CampaignSetupUI.SettingValue<float>(() => CS$<>8__locals1.numberInput.FloatValue, delegate(float i)
			{
				CS$<>8__locals1.numberInput.FloatValue = (float)Math.Round((double)i, 1);
			});
		}

		// Token: 0x06002421 RID: 9249 RVA: 0x0016A7E8 File Offset: 0x001689E8
		[CompilerGenerated]
		internal static CampaignSetupUI.SettingValue<T> <CreateCampaignSettingList>g__CreateSelectionCarousel|37_10<T>(GUIComponent parent, LocalizedString description, LocalizedString tooltip, CampaignSetupUI.SettingCarouselElement<T> defaultValue, float verticalSize, ImmutableArray<CampaignSetupUI.SettingCarouselElement<T>> options, Action onChanged)
		{
			CampaignSetupUI.<>c__DisplayClass37_3<T> CS$<>8__locals1 = new CampaignSetupUI.<>c__DisplayClass37_3<T>();
			CS$<>8__locals1.options = options;
			CS$<>8__locals1.onChanged = onChanged;
			GUILayoutGroup inputContainer = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSettingBase|37_12(parent, description, tooltip, 0.55f, verticalSize);
			GUIButton minusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIButtonToggleLeft", null)
			{
				UserData = -1
			};
			GUIFrame inputFrame = new GUIFrame(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			CS$<>8__locals1.numberInput = new GUINumberInput(new RectTransform(Vector2.One, inputFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), NumberType.Int, "GUITextBox", Alignment.Center, null, GUINumberInput.ButtonVisibility.ForceHidden, null)
			{
				IntValue = CS$<>8__locals1.options.IndexOf(defaultValue),
				MinValueInt = new int?(0),
				MaxValueInt = new int?(CS$<>8__locals1.options.Length),
				Visible = false,
				ToolTip = tooltip
			};
			inputContainer.RectTransform.Parent.MinSize = new Point(0, CS$<>8__locals1.numberInput.RectTransform.MinSize.Y);
			CS$<>8__locals1.inputLabel = new GUITextBox(new RectTransform(Vector2.One, inputFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), defaultValue.Label.Value, null, null, Alignment.Center, false, "", null, false, false)
			{
				CanBeFocused = false
			};
			GUIButton plusButton = new GUIButton(new RectTransform(Vector2.One, inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIButtonToggleRight", null)
			{
				UserData = 1
			};
			minusButton.OnClicked = (plusButton.OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<CreateCampaignSettingList>g__ChangeValue|23));
			GUINumberInput numberInput = CS$<>8__locals1.numberInput;
			numberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(numberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput _)
			{
				CS$<>8__locals1.onChanged();
			}));
			return new CampaignSetupUI.SettingValue<T>(() => CS$<>8__locals1.options[CS$<>8__locals1.numberInput.IntValue].Value, delegate(T t)
			{
				base.<CreateCampaignSettingList>g__SetValue|24(CS$<>8__locals1.options.IndexOf((CampaignSetupUI.SettingCarouselElement<T> e) => object.Equals(e.Value, t)));
			});
		}

		// Token: 0x06002422 RID: 9250 RVA: 0x0016AA9C File Offset: 0x00168C9C
		[CompilerGenerated]
		internal static CampaignSetupUI.SettingValue<bool> <CreateCampaignSettingList>g__CreateTickbox|37_11(GUIComponent parent, LocalizedString description, LocalizedString tooltip, bool defaultValue, float verticalSize, Action onChanged)
		{
			GUILayoutGroup inputContainer = CampaignSetupUI.<CreateCampaignSettingList>g__CreateSettingBase|37_12(parent, description, tooltip, 0.625f, verticalSize);
			GUILayoutGroup tickboxContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.375f, 1f), inputContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUITickBox tickBox = new GUITickBox(new RectTransform(Vector2.One, tickboxContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, "")
			{
				Selected = defaultValue,
				ToolTip = tooltip
			};
			tickBox.Box.IgnoreLayoutGroups = true;
			tickBox.Box.RectTransform.SetPosition(Anchor.CenterLeft, null);
			inputContainer.RectTransform.Parent.MinSize = new Point(0, tickBox.RectTransform.MinSize.Y);
			GUITickBox tickBox2 = tickBox;
			tickBox2.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(tickBox2.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox _)
			{
				onChanged();
				return true;
			}));
			return new CampaignSetupUI.SettingValue<bool>(() => tickBox.Selected, delegate(bool b)
			{
				tickBox.Selected = b;
			});
		}

		// Token: 0x06002423 RID: 9251 RVA: 0x0016AC04 File Offset: 0x00168E04
		[CompilerGenerated]
		internal static GUILayoutGroup <CreateCampaignSettingList>g__CreateSettingBase|37_12(GUIComponent parent, LocalizedString description, LocalizedString tooltip, float horizontalSize, float verticalSize)
		{
			GUILayoutGroup settingHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, verticalSize), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			RectTransform rectT = new RectTransform(new Vector2(horizontalSize, 1f), settingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = description;
			GUIFont font = (parent.Rect.Width < 320) ? GUIStyle.SmallFont : GUIStyle.Font;
			GUITextBlock descriptionBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, true, "", null)
			{
				ToolTip = tooltip
			};
			GUILayoutGroup inputContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f - horizontalSize, 0.8f), settingHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			inputContainer.RectTransform.IsFixedSize = true;
			settingHolder.RectTransform.MinSize = new Point(0, (int)descriptionBlock.TextSize.Y);
			return inputContainer;
		}

		// Token: 0x040011F8 RID: 4600
		private const CampaignSetupUI.SaveSortingType DefaultSaveSortingType = CampaignSetupUI.SaveSortingType.LastPlayedDescending;

		// Token: 0x040011F9 RID: 4601
		protected readonly GUIComponent newGameContainer;

		// Token: 0x040011FA RID: 4602
		protected readonly GUIComponent loadGameContainer;

		// Token: 0x040011FB RID: 4603
		protected GUIListBox saveList;

		// Token: 0x040011FC RID: 4604
		protected GUITextBox saveNameBox;

		// Token: 0x040011FD RID: 4605
		protected GUITextBox seedBox;

		// Token: 0x040011FE RID: 4606
		protected GUIButton loadGameButton;

		// Token: 0x040011FF RID: 4607
		public Action<SubmarineInfo, string, string, CampaignSettings> StartNewGame;

		// Token: 0x04001200 RID: 4608
		public CampaignSetupUI.LoadGameDelegate LoadGame;

		// Token: 0x04001201 RID: 4609
		protected CampaignSetupUI.CategoryFilter subFilter;

		// Token: 0x04001206 RID: 4614
		protected List<CampaignMode.SaveInfo> prevSaveFiles;

		// Token: 0x02000BF6 RID: 3062
		protected enum SaveSortingType
		{
			// Token: 0x04004963 RID: 18787
			LastPlayedDescending,
			// Token: 0x04004964 RID: 18788
			LastPlayedAscending,
			// Token: 0x04004965 RID: 18789
			NameDescending,
			// Token: 0x04004966 RID: 18790
			NameAscending
		}

		// Token: 0x02000BF7 RID: 3063
		// (Invoke) Token: 0x06007A69 RID: 31337
		public delegate void LoadGameDelegate(string loadPath, Option<uint> backupIndex);

		// Token: 0x02000BF8 RID: 3064
		protected enum CategoryFilter
		{
			// Token: 0x04004968 RID: 18792
			All,
			// Token: 0x04004969 RID: 18793
			Vanilla,
			// Token: 0x0400496A RID: 18794
			Custom
		}

		// Token: 0x02000BF9 RID: 3065
		public struct CampaignSettingElements
		{
			// Token: 0x06007A6C RID: 31340 RVA: 0x003814D4 File Offset: 0x0037F6D4
			public readonly CampaignSettings CreateSettings()
			{
				return new CampaignSettings(null)
				{
					PresetName = this.SelectedPreset.GetValue(),
					TutorialEnabled = this.TutorialEnabled.GetValue(),
					RadiationEnabled = this.RadiationEnabled.GetValue(),
					MaxMissionCount = this.MaxMissionCount.GetValue(),
					StartingBalanceAmount = this.StartingFunds.GetValue(),
					WorldHostility = this.WorldHostility.GetValue(),
					StartItemSet = this.StartItemSet.GetValue(),
					CrewVitalityMultiplier = this.CrewVitalityMultiplier.GetValue(),
					NonCrewVitalityMultiplier = this.NonCrewVitalityMultiplier.GetValue(),
					OxygenMultiplier = this.OxygenMultiplier.GetValue(),
					FuelMultiplier = this.FuelMultiplier.GetValue(),
					MissionRewardMultiplier = this.MissionRewardMultiplier.GetValue(),
					ExperienceRewardMultiplier = this.ExperienceRewardMultiplier.GetValue(),
					ShopPriceMultiplier = this.ShopPriceMultiplier.GetValue(),
					ShipyardPriceMultiplier = this.ShipyardPriceMultiplier.GetValue(),
					RepairFailMultiplier = this.RepairFailMultiplier.GetValue(),
					PatdownProbability = this.PatdownProbability.GetValue(),
					ShowHuskWarning = this.ShowHuskWarning.GetValue()
				};
			}

			// Token: 0x0400496B RID: 18795
			public CampaignSetupUI.SettingValue<string> SelectedPreset;

			// Token: 0x0400496C RID: 18796
			public CampaignSetupUI.SettingValue<bool> TutorialEnabled;

			// Token: 0x0400496D RID: 18797
			public CampaignSetupUI.SettingValue<bool> RadiationEnabled;

			// Token: 0x0400496E RID: 18798
			public CampaignSetupUI.SettingValue<int> MaxMissionCount;

			// Token: 0x0400496F RID: 18799
			public CampaignSetupUI.SettingValue<StartingBalanceAmountOption> StartingFunds;

			// Token: 0x04004970 RID: 18800
			public CampaignSetupUI.SettingValue<WorldHostilityOption> WorldHostility;

			// Token: 0x04004971 RID: 18801
			public CampaignSetupUI.SettingValue<Identifier> StartItemSet;

			// Token: 0x04004972 RID: 18802
			public CampaignSetupUI.SettingValue<float> CrewVitalityMultiplier;

			// Token: 0x04004973 RID: 18803
			public CampaignSetupUI.SettingValue<float> NonCrewVitalityMultiplier;

			// Token: 0x04004974 RID: 18804
			public CampaignSetupUI.SettingValue<float> OxygenMultiplier;

			// Token: 0x04004975 RID: 18805
			public CampaignSetupUI.SettingValue<float> FuelMultiplier;

			// Token: 0x04004976 RID: 18806
			public CampaignSetupUI.SettingValue<float> MissionRewardMultiplier;

			// Token: 0x04004977 RID: 18807
			public CampaignSetupUI.SettingValue<float> ExperienceRewardMultiplier;

			// Token: 0x04004978 RID: 18808
			public CampaignSetupUI.SettingValue<float> ShopPriceMultiplier;

			// Token: 0x04004979 RID: 18809
			public CampaignSetupUI.SettingValue<float> ShipyardPriceMultiplier;

			// Token: 0x0400497A RID: 18810
			public CampaignSetupUI.SettingValue<float> RepairFailMultiplier;

			// Token: 0x0400497B RID: 18811
			public CampaignSetupUI.SettingValue<PatdownProbabilityOption> PatdownProbability;

			// Token: 0x0400497C RID: 18812
			public CampaignSetupUI.SettingValue<bool> ShowHuskWarning;
		}

		// Token: 0x02000BFA RID: 3066
		public readonly struct SettingValue<T>
		{
			// Token: 0x06007A6D RID: 31341 RVA: 0x00381619 File Offset: 0x0037F819
			public T GetValue()
			{
				return this.getter();
			}

			// Token: 0x06007A6E RID: 31342 RVA: 0x00381626 File Offset: 0x0037F826
			public void SetValue(T value)
			{
				this.setter(value);
			}

			// Token: 0x06007A6F RID: 31343 RVA: 0x00381634 File Offset: 0x0037F834
			public SettingValue(Func<T> get, Action<T> set)
			{
				this.getter = get;
				this.setter = set;
			}

			// Token: 0x0400497D RID: 18813
			private readonly Func<T> getter;

			// Token: 0x0400497E RID: 18814
			private readonly Action<T> setter;
		}

		// Token: 0x02000BFB RID: 3067
		private readonly struct SettingCarouselElement<T>
		{
			// Token: 0x06007A70 RID: 31344 RVA: 0x00381644 File Offset: 0x0037F844
			public SettingCarouselElement(T value, string label, bool isHidden = false)
			{
				this.Value = value;
				this.Label = TextManager.Get(label).Fallback(label, true);
				this.IsHidden = isHidden;
			}

			// Token: 0x0400497F RID: 18815
			public readonly LocalizedString Label;

			// Token: 0x04004980 RID: 18816
			public readonly T Value;

			// Token: 0x04004981 RID: 18817
			public readonly bool IsHidden;
		}
	}
}
