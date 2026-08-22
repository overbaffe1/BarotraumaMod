using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000EE RID: 238
	internal class SubmarineInfo : IDisposable
	{
		// Token: 0x0600221B RID: 8731 RVA: 0x00158B34 File Offset: 0x00156D34
		public void CreatePreviewWindow(GUIComponent parent)
		{
			GUIFrame content = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton previewButton = new GUIButton(new RectTransform(new Vector2(1f, 0.5f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				CanBeFocused = (this.SubmarineElement != null),
				OnClicked = delegate(GUIButton btn, object obj)
				{
					SubmarinePreview.Create(this);
					return false;
				}
			};
			Sprite sprite;
			if ((sprite = this.PreviewImage) == null)
			{
				SubmarineInfo submarineInfo = SubmarineInfo.savedSubmarines.Find((SubmarineInfo s) => s.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
				sprite = ((submarineInfo != null) ? submarineInfo.PreviewImage : null);
			}
			Sprite previewImage = sprite;
			if (previewImage == null)
			{
				new GUITextBlock(new RectTransform(Vector2.One, previewButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(SubmarineInfo.SavedSubmarines.Contains(this) ? "SubPreviewImageNotFound" : "SubNotDownloaded"), null, null, Alignment.Left, false, "", null);
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
			if (this.SubmarineElement != null)
			{
				GUIFrame guiframe = new GUIFrame(new RectTransform(Vector2.One * 0.12f, previewButton.RectTransform, Anchor.BottomRight, new Pivot?(Pivot.BottomRight), null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point((int)(0.03f * (float)previewButton.Rect.Height))
				}, "ExpandButton", new Color?(Color.White));
				guiframe.Color = Color.White;
				guiframe.HoverColor = Color.White;
				guiframe.PressedColor = Color.White;
			}
			GUIListBox descriptionBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.5f), content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				UserData = "descriptionbox",
				ScrollBarVisible = true,
				Spacing = 5,
				CurrentSelectMode = GUIListBox.SelectMode.None
			};
			GUIFont font = (parent.Rect.Width < 350) ? GUIStyle.SmallFont : GUIStyle.Font;
			this.CreateSpecsWindow(descriptionBox, font, true, true, true, false);
		}

		// Token: 0x0600221C RID: 8732 RVA: 0x00158EDC File Offset: 0x001570DC
		public void CreateSpecsWindow(GUIListBox parent, GUIFont font, bool includeTitle = true, bool includeClass = true, bool includeDescription = false, bool includeCrushDepth = false)
		{
			SubmarineInfo.<>c__DisplayClass2_0 CS$<>8__locals1 = new SubmarineInfo.<>c__DisplayClass2_0();
			float leftPanelWidth = 0.6f;
			float rightPanelWidth = 0.4f / leftPanelWidth;
			LocalizedString localizedString;
			if (this.HasTag(SubmarineTag.Shuttle))
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
				defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(this.SubmarineClass);
				array[num] = new ValueTuple<string, LocalizedString>(item, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
				int num2 = 1;
				string item2 = "[tier]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("submarinetier.");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Tier);
				array[num2] = new ValueTuple<string, LocalizedString>(item2, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
				localizedString = TextManager.GetWithVariables(tag, array);
			}
			LocalizedString className = localizedString;
			int classHeight = (int)GUIStyle.SubHeadingFont.MeasureString(className, false).Y;
			int leftPanelWidthInt = (int)((float)parent.Rect.Width * leftPanelWidth);
			CS$<>8__locals1.submarineNameText = null;
			if (includeTitle)
			{
				int nameHeight = (int)GUIStyle.LargeFont.MeasureString(this.DisplayName, true).Y;
				SubmarineInfo.<>c__DisplayClass2_0 CS$<>8__locals2 = CS$<>8__locals1;
				RectTransform rectT = new RectTransform(new Point(leftPanelWidthInt, nameHeight + HUDLayoutSettings.Padding / 2), parent.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text = this.DisplayName;
				GUIFont font2 = GUIStyle.LargeFont;
				CS$<>8__locals2.submarineNameText = new GUITextBlock(rectT, text, null, font2, Alignment.CenterLeft, false, "", null)
				{
					CanBeFocused = false
				};
				CS$<>8__locals1.submarineNameText.RectTransform.MinSize = new Point(0, (int)CS$<>8__locals1.submarineNameText.TextSize.Y);
			}
			if (includeClass)
			{
				RectTransform rectT2 = new RectTransform(new Point(leftPanelWidthInt, classHeight), parent.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text2 = className;
				GUIFont font2 = GUIStyle.SubHeadingFont;
				GUITextBlock guitextBlock = new GUITextBlock(rectT2, text2, null, font2, Alignment.CenterLeft, false, "", null);
				LocalizedString left = TextManager.Get("submarinetierandclass.description") + "\n\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(27, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("submarineclass.");
				defaultInterpolatedStringHandler3.AppendFormatted<SubmarineClass>(this.SubmarineClass);
				defaultInterpolatedStringHandler3.AppendLiteral(".description");
				guitextBlock.ToolTip = left + TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
				GUITextBlock submarineClassText = guitextBlock;
				submarineClassText.HoverColor = Color.Transparent;
				submarineClassText.RectTransform.MinSize = new Point(0, (int)submarineClassText.TextSize.Y);
			}
			if (this.Price > 0)
			{
				GUITextBlock priceText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("subeditor.price"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), priceText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), TextManager.GetWithVariable("currencyformat", "[credits]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.Price), FormatCapitals.No), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
			}
			Vector2 realWorldDimensions = this.Dimensions * Physics.DisplayToRealWorldRatio;
			if (realWorldDimensions != Vector2.Zero)
			{
				LocalizedString dimensionsStr = TextManager.GetWithVariables("DimensionsFormat", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[width]", ((int)realWorldDimensions.X).ToString()),
					new ValueTuple<string, string>("[height]", ((int)realWorldDimensions.Y).ToString())
				});
				GUITextBlock dimensionsText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Dimensions"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), dimensionsText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), dimensionsStr, null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				dimensionsText.RectTransform.MinSize = new Point(0, dimensionsText.Children.First<GUIComponent>().Rect.Height);
			}
			LocalizedString cargoCapacityStr = (this.CargoCapacity < 0) ? TextManager.Get("unknown") : TextManager.GetWithVariable("cargocapacityformat", "[cratecount]", this.CargoCapacity.ToString(), FormatCapitals.No);
			GUITextBlock cargoCapacityText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cargocapacity"), null, font, Alignment.TopLeft, true, "", null)
			{
				CanBeFocused = false
			};
			new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), cargoCapacityText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), cargoCapacityStr, null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
			cargoCapacityText.RectTransform.MinSize = new Point(0, cargoCapacityText.Children.First<GUIComponent>().Rect.Height);
			if (includeCrushDepth)
			{
				GUITextBlock crushDepthText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("CrushDepth"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), crushDepthText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), TextManager.GetWithVariable("meterformat", "[meters]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.GetSubCrushDepth()), FormatCapitals.No), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				crushDepthText.RectTransform.MinSize = new Point(0, crushDepthText.Children.First<GUIComponent>().Rect.Height);
			}
			if (this.RecommendedCrewSizeMax > 0)
			{
				GUITextBlock crewSizeText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RecommendedCrewSize"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), crewSizeText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), this.RecommendedCrewSizeMin.ToString() + " - " + this.RecommendedCrewSizeMax.ToString(), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				crewSizeText.RectTransform.MinSize = new Point(0, crewSizeText.Children.First<GUIComponent>().Rect.Height);
			}
			if (this.RecommendedCrewExperience != SubmarineInfo.CrewExperienceLevel.Unknown)
			{
				GUITextBlock crewExperienceText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RecommendedCrewExperience"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), crewExperienceText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), TextManager.Get(this.RecommendedCrewExperience.ToIdentifier<SubmarineInfo.CrewExperienceLevel>()), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				crewExperienceText.RectTransform.MinSize = new Point(0, crewExperienceText.Children.First<GUIComponent>().Rect.Height);
			}
			if (this.RequiredContentPackages.Any<string>())
			{
				GUITextBlock contentPackagesText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RequiredContentPackages"), null, font, Alignment.TopLeft, false, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), contentPackagesText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), string.Join(", ", this.RequiredContentPackages), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				contentPackagesText.RectTransform.MinSize = new Point(0, contentPackagesText.Children.First<GUIComponent>().Rect.Height);
			}
			if (!this.IsVanillaSubmarine() && this.GameVersion != null)
			{
				GUITextBlock versionText = new GUITextBlock(new RectTransform(new Vector2(leftPanelWidth, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverlistversion"), null, font, Alignment.TopLeft, true, "", null)
				{
					CanBeFocused = false
				};
				new GUITextBlock(new RectTransform(new Vector2(rightPanelWidth, 0f), versionText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), this.GameVersion.ToString(), null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
				versionText.RectTransform.MinSize = new Point(0, versionText.Children.First<GUIComponent>().Rect.Height);
			}
			if (CS$<>8__locals1.submarineNameText != null)
			{
				CS$<>8__locals1.submarineNameText.AutoScaleHorizontal = true;
			}
			CS$<>8__locals1.descBlock = null;
			if (includeDescription)
			{
				new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				if (!this.Description.IsNullOrEmpty())
				{
					RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get(new string[]
					{
						"SaveSubDialogDescription",
						"WorkshopItemDescription"
					});
					GUIFont font2 = GUIStyle.Font;
					GUITextBlock guitextBlock2 = new GUITextBlock(rectT3, text3, null, font2, Alignment.Left, true, "", null);
					guitextBlock2.CanBeFocused = false;
					guitextBlock2.ForceUpperCase = ForceUpperCase.Yes;
					CS$<>8__locals1.descBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.Description, null, font, Alignment.Left, true, "", null)
					{
						CanBeFocused = false
					};
				}
			}
			GUITextBlock.AutoScaleAndNormalize(from c in parent.Content.GetAllChildren<GUITextBlock>()
			where c != CS$<>8__locals1.submarineNameText && c != CS$<>8__locals1.descBlock
			select c, true, false, null);
			parent.ForceLayoutRecalculation();
		}

		// Token: 0x0600221D RID: 8733 RVA: 0x00159CFC File Offset: 0x00157EFC
		private float GetSubCrushDepth()
		{
			SubmarineInfo.PendingSubInfo pendingSubInfo = default(SubmarineInfo.PendingSubInfo);
			return SubmarineInfo.GetSubCrushDepth(this, ref pendingSubInfo);
		}

		// Token: 0x0600221E RID: 8734 RVA: 0x00159D1C File Offset: 0x00157F1C
		public static float GetSubCrushDepth(SubmarineInfo subInfo, ref SubmarineInfo.PendingSubInfo pendingSubInfo)
		{
			float subCrushDepth = 3500f;
			if (Submarine.MainSub != null && Submarine.MainSub.Info == subInfo)
			{
				subCrushDepth = Submarine.MainSub.RealWorldCrushDepth;
			}
			else if (subInfo != null)
			{
				if (pendingSubInfo.PendingSub != subInfo)
				{
					float realWorldCrushDepth;
					pendingSubInfo = new SubmarineInfo.PendingSubInfo(subInfo, subInfo.IsCrushDepthDefinedInStructures(out realWorldCrushDepth), realWorldCrushDepth);
				}
				subCrushDepth = pendingSubInfo.RealWorldCrushDepth;
			}
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (gameSession == null)
			{
				flag = (null != null);
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				flag = (((campaign != null) ? campaign.UpgradeManager : null) != null);
			}
			if (flag)
			{
				UpgradePrefab hullUpgradePrefab = UpgradePrefab.Find("increasewallhealth".ToIdentifier());
				if (hullUpgradePrefab != null)
				{
					int pendingLevel = GameMain.GameSession.Campaign.UpgradeManager.GetUpgradeLevel(hullUpgradePrefab, hullUpgradePrefab.UpgradeCategories.First<UpgradeCategory>(), subInfo);
					int currentLevel = 0;
					if (pendingSubInfo.PendingSub == null || pendingSubInfo.StructuresDefineRealWorldCrushDepth)
					{
						currentLevel = GameMain.GameSession.Campaign.UpgradeManager.GetRealUpgradeLevelForSub(hullUpgradePrefab, hullUpgradePrefab.UpgradeCategories.First<UpgradeCategory>(), subInfo);
					}
					if (pendingLevel > currentLevel)
					{
						ContentXElement sourceElement = hullUpgradePrefab.SourceElement;
						string text;
						if (sourceElement == null)
						{
							text = null;
						}
						else
						{
							ContentXElement childElement = sourceElement.GetChildElement("Structure");
							text = ((childElement != null) ? childElement.GetAttributeString("crushdepth", null) : null);
						}
						string updateValueStr = text;
						if (!string.IsNullOrEmpty(updateValueStr))
						{
							if (currentLevel > 0)
							{
								int upgradePercentage = UpgradePrefab.ParsePercentage(updateValueStr, Identifier.Empty, null, true);
								subCrushDepth /= 1f + (float)upgradePercentage / 100f * (float)currentLevel;
							}
							subCrushDepth = PropertyReference.CalculateUpgrade(subCrushDepth, pendingLevel, updateValueStr);
						}
					}
				}
			}
			return subCrushDepth;
		}

		// Token: 0x1700093F RID: 2367
		// (get) Token: 0x0600221F RID: 8735 RVA: 0x00159E77 File Offset: 0x00158077
		// (set) Token: 0x06002220 RID: 8736 RVA: 0x00159E7E File Offset: 0x0015807E
		public static HashSet<string> SubmarinePathsWithRemoteStorage { get; set; } = new HashSet<string>();

		// Token: 0x17000940 RID: 2368
		// (get) Token: 0x06002221 RID: 8737 RVA: 0x00159E86 File Offset: 0x00158086
		// (set) Token: 0x06002222 RID: 8738 RVA: 0x00159EB0 File Offset: 0x001580B0
		public bool SaveToRemoteStorage
		{
			get
			{
				return this.FilePath != null && SubmarineInfo.SubmarinePathsWithRemoteStorage.Contains(this.FilePath.CleanUpPathCrossPlatform(false, ""));
			}
			set
			{
				if (this.FilePath == null)
				{
					return;
				}
				if (value)
				{
					SubmarineInfo.SubmarinePathsWithRemoteStorage.Add(this.FilePath.CleanUpPathCrossPlatform(false, ""));
					return;
				}
				SubmarineInfo.SubmarinePathsWithRemoteStorage.Remove(this.FilePath.CleanUpPathCrossPlatform(false, ""));
			}
		}

		// Token: 0x17000941 RID: 2369
		// (get) Token: 0x06002223 RID: 8739 RVA: 0x00159F02 File Offset: 0x00158102
		public static IEnumerable<SubmarineInfo> SavedSubmarines
		{
			get
			{
				return SubmarineInfo.savedSubmarines;
			}
		}

		// Token: 0x17000942 RID: 2370
		// (get) Token: 0x06002224 RID: 8740 RVA: 0x00159F09 File Offset: 0x00158109
		// (set) Token: 0x06002225 RID: 8741 RVA: 0x00159F11 File Offset: 0x00158111
		public SubmarineTag Tags { get; private set; }

		// Token: 0x17000943 RID: 2371
		// (get) Token: 0x06002226 RID: 8742 RVA: 0x00159F1A File Offset: 0x0015811A
		// (set) Token: 0x06002227 RID: 8743 RVA: 0x00159F22 File Offset: 0x00158122
		public int Tier { get; set; }

		// Token: 0x17000944 RID: 2372
		// (get) Token: 0x06002228 RID: 8744 RVA: 0x00159F2B File Offset: 0x0015812B
		// (set) Token: 0x06002229 RID: 8745 RVA: 0x00159F33 File Offset: 0x00158133
		public int EqualityCheckVal { get; private set; }

		// Token: 0x17000945 RID: 2373
		// (get) Token: 0x0600222A RID: 8746 RVA: 0x00159F3C File Offset: 0x0015813C
		// (set) Token: 0x0600222B RID: 8747 RVA: 0x00159F44 File Offset: 0x00158144
		public string Name { get; set; }

		// Token: 0x17000946 RID: 2374
		// (get) Token: 0x0600222C RID: 8748 RVA: 0x00159F4D File Offset: 0x0015814D
		// (set) Token: 0x0600222D RID: 8749 RVA: 0x00159F55 File Offset: 0x00158155
		public LocalizedString DisplayName { get; set; }

		// Token: 0x17000947 RID: 2375
		// (get) Token: 0x0600222E RID: 8750 RVA: 0x00159F5E File Offset: 0x0015815E
		// (set) Token: 0x0600222F RID: 8751 RVA: 0x00159F66 File Offset: 0x00158166
		public LocalizedString Description { get; set; }

		// Token: 0x17000948 RID: 2376
		// (get) Token: 0x06002230 RID: 8752 RVA: 0x00159F6F File Offset: 0x0015816F
		// (set) Token: 0x06002231 RID: 8753 RVA: 0x00159F77 File Offset: 0x00158177
		public int Price { get; set; }

		// Token: 0x17000949 RID: 2377
		// (get) Token: 0x06002232 RID: 8754 RVA: 0x00159F80 File Offset: 0x00158180
		// (set) Token: 0x06002233 RID: 8755 RVA: 0x00159F88 File Offset: 0x00158188
		public bool InitialSuppliesSpawned { get; set; }

		// Token: 0x1700094A RID: 2378
		// (get) Token: 0x06002234 RID: 8756 RVA: 0x00159F91 File Offset: 0x00158191
		// (set) Token: 0x06002235 RID: 8757 RVA: 0x00159F99 File Offset: 0x00158199
		public bool NoItems { get; set; }

		// Token: 0x1700094B RID: 2379
		// (get) Token: 0x06002236 RID: 8758 RVA: 0x00159FA2 File Offset: 0x001581A2
		// (set) Token: 0x06002237 RID: 8759 RVA: 0x00159FAA File Offset: 0x001581AA
		public bool LowFuel { get; set; }

		// Token: 0x1700094C RID: 2380
		// (get) Token: 0x06002238 RID: 8760 RVA: 0x00159FB3 File Offset: 0x001581B3
		// (set) Token: 0x06002239 RID: 8761 RVA: 0x00159FBB File Offset: 0x001581BB
		public Version GameVersion { get; set; }

		// Token: 0x1700094D RID: 2381
		// (get) Token: 0x0600223A RID: 8762 RVA: 0x00159FC4 File Offset: 0x001581C4
		// (set) Token: 0x0600223B RID: 8763 RVA: 0x00159FCC File Offset: 0x001581CC
		public SubmarineType Type { get; set; }

		// Token: 0x1700094E RID: 2382
		// (get) Token: 0x0600223C RID: 8764 RVA: 0x00159FD5 File Offset: 0x001581D5
		// (set) Token: 0x0600223D RID: 8765 RVA: 0x00159FDD File Offset: 0x001581DD
		public bool IsManuallyOutfitted { get; set; }

		// Token: 0x1700094F RID: 2383
		// (get) Token: 0x0600223E RID: 8766 RVA: 0x00159FE6 File Offset: 0x001581E6
		// (set) Token: 0x0600223F RID: 8767 RVA: 0x00159FEE File Offset: 0x001581EE
		public OutpostModuleInfo OutpostModuleInfo { get; set; }

		// Token: 0x17000950 RID: 2384
		// (get) Token: 0x06002240 RID: 8768 RVA: 0x00159FF7 File Offset: 0x001581F7
		// (set) Token: 0x06002241 RID: 8769 RVA: 0x00159FFF File Offset: 0x001581FF
		public BeaconStationInfo BeaconStationInfo { get; set; }

		// Token: 0x17000951 RID: 2385
		// (get) Token: 0x06002242 RID: 8770 RVA: 0x0015A008 File Offset: 0x00158208
		// (set) Token: 0x06002243 RID: 8771 RVA: 0x0015A010 File Offset: 0x00158210
		public WreckInfo WreckInfo { get; set; }

		// Token: 0x17000952 RID: 2386
		// (get) Token: 0x06002244 RID: 8772 RVA: 0x0015A019 File Offset: 0x00158219
		// (set) Token: 0x06002245 RID: 8773 RVA: 0x0015A021 File Offset: 0x00158221
		public EnemySubmarineInfo EnemySubmarineInfo { get; set; }

		// Token: 0x17000953 RID: 2387
		// (get) Token: 0x06002246 RID: 8774 RVA: 0x0015A02A File Offset: 0x0015822A
		public ExtraSubmarineInfo GetExtraSubmarineInfo
		{
			get
			{
				return this.BeaconStationInfo ?? this.WreckInfo;
			}
		}

		// Token: 0x17000954 RID: 2388
		// (get) Token: 0x06002247 RID: 8775 RVA: 0x0015A03C File Offset: 0x0015823C
		// (set) Token: 0x06002248 RID: 8776 RVA: 0x0015A044 File Offset: 0x00158244
		public ImmutableHashSet<Identifier> OutpostTags { get; set; } = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x17000955 RID: 2389
		// (get) Token: 0x06002249 RID: 8777 RVA: 0x0015A04D File Offset: 0x0015824D
		// (set) Token: 0x0600224A RID: 8778 RVA: 0x0015A055 File Offset: 0x00158255
		public ImmutableHashSet<Identifier> TriggerOutpostMissionEvents { get; set; } = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x17000956 RID: 2390
		// (get) Token: 0x0600224B RID: 8779 RVA: 0x0015A060 File Offset: 0x00158260
		public bool IsOutpost
		{
			get
			{
				SubmarineType type = this.Type;
				return type - SubmarineType.Outpost <= 1;
			}
		}

		// Token: 0x17000957 RID: 2391
		// (get) Token: 0x0600224C RID: 8780 RVA: 0x0015A081 File Offset: 0x00158281
		public bool IsWreck
		{
			get
			{
				return this.Type == SubmarineType.Wreck;
			}
		}

		// Token: 0x17000958 RID: 2392
		// (get) Token: 0x0600224D RID: 8781 RVA: 0x0015A08C File Offset: 0x0015828C
		public bool IsBeacon
		{
			get
			{
				return this.Type == SubmarineType.BeaconStation;
			}
		}

		// Token: 0x17000959 RID: 2393
		// (get) Token: 0x0600224E RID: 8782 RVA: 0x0015A097 File Offset: 0x00158297
		public bool IsEnemySubmarine
		{
			get
			{
				return this.Type == SubmarineType.EnemySubmarine;
			}
		}

		// Token: 0x1700095A RID: 2394
		// (get) Token: 0x0600224F RID: 8783 RVA: 0x0015A0A2 File Offset: 0x001582A2
		public bool IsPlayer
		{
			get
			{
				return this.Type == SubmarineType.Player;
			}
		}

		// Token: 0x1700095B RID: 2395
		// (get) Token: 0x06002250 RID: 8784 RVA: 0x0015A0AD File Offset: 0x001582AD
		public bool IsRuin
		{
			get
			{
				return this.Type == SubmarineType.Ruin;
			}
		}

		// Token: 0x1700095C RID: 2396
		// (get) Token: 0x06002251 RID: 8785 RVA: 0x0015A0B8 File Offset: 0x001582B8
		public bool ShouldBeRuin
		{
			get
			{
				SubmarineType type = this.Type;
				bool flag = type == SubmarineType.OutpostModule || type == SubmarineType.Ruin;
				if (flag)
				{
					return this.OutpostModuleInfo.ModuleFlags.Any((Identifier f) => f.StartsWith("ruin"));
				}
				return false;
			}
		}

		// Token: 0x1700095D RID: 2397
		// (get) Token: 0x06002252 RID: 8786 RVA: 0x0015A10E File Offset: 0x0015830E
		public bool IsCampaignCompatible
		{
			get
			{
				return this.IsPlayer && !this.HasTag(SubmarineTag.Shuttle) && !this.HasTag(SubmarineTag.HideInMenus) && this.SubmarineClass > SubmarineClass.Undefined;
			}
		}

		// Token: 0x1700095E RID: 2398
		// (get) Token: 0x06002253 RID: 8787 RVA: 0x0015A135 File Offset: 0x00158335
		public bool IsCampaignCompatibleIgnoreClass
		{
			get
			{
				return this.IsPlayer && !this.HasTag(SubmarineTag.Shuttle) && !this.HasTag(SubmarineTag.HideInMenus);
			}
		}

		// Token: 0x1700095F RID: 2399
		// (get) Token: 0x06002254 RID: 8788 RVA: 0x0015A154 File Offset: 0x00158354
		public bool AllowPreviewImage
		{
			get
			{
				return this.Type == SubmarineType.Player;
			}
		}

		// Token: 0x17000960 RID: 2400
		// (get) Token: 0x06002255 RID: 8789 RVA: 0x0015A160 File Offset: 0x00158360
		public Md5Hash MD5Hash
		{
			get
			{
				if (this.hash == null)
				{
					if (this.hashTask == null)
					{
						XDocument doc = SubmarineInfo.OpenFile(this.FilePath);
						this.StartHashDocTask(doc);
					}
					this.hashTask.Wait();
					this.hashTask = null;
				}
				return this.hash;
			}
		}

		// Token: 0x17000961 RID: 2401
		// (get) Token: 0x06002256 RID: 8790 RVA: 0x0015A1AE File Offset: 0x001583AE
		public bool CalculatingHash
		{
			get
			{
				return this.hashTask != null && !this.hashTask.IsCompleted;
			}
		}

		// Token: 0x17000962 RID: 2402
		// (get) Token: 0x06002257 RID: 8791 RVA: 0x0015A1C8 File Offset: 0x001583C8
		// (set) Token: 0x06002258 RID: 8792 RVA: 0x0015A1D0 File Offset: 0x001583D0
		public Vector2 Dimensions { get; private set; }

		// Token: 0x17000963 RID: 2403
		// (get) Token: 0x06002259 RID: 8793 RVA: 0x0015A1D9 File Offset: 0x001583D9
		// (set) Token: 0x0600225A RID: 8794 RVA: 0x0015A1E1 File Offset: 0x001583E1
		public int CargoCapacity { get; private set; }

		// Token: 0x17000964 RID: 2404
		// (get) Token: 0x0600225B RID: 8795 RVA: 0x0015A1EA File Offset: 0x001583EA
		// (set) Token: 0x0600225C RID: 8796 RVA: 0x0015A1F2 File Offset: 0x001583F2
		public string FilePath { get; set; }

		// Token: 0x17000965 RID: 2405
		// (get) Token: 0x0600225D RID: 8797 RVA: 0x0015A1FB File Offset: 0x001583FB
		// (set) Token: 0x0600225E RID: 8798 RVA: 0x0015A219 File Offset: 0x00158419
		public XElement SubmarineElement
		{
			get
			{
				if (this.LazyLoad && this.submarineElement == null)
				{
					this.Reload();
				}
				return this.submarineElement;
			}
			private set
			{
				this.submarineElement = value;
			}
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x0015A222 File Offset: 0x00158422
		public override string ToString()
		{
			return "Barotrauma.SubmarineInfo (" + this.Name + ")";
		}

		// Token: 0x17000966 RID: 2406
		// (get) Token: 0x06002260 RID: 8800 RVA: 0x0015A239 File Offset: 0x00158439
		// (set) Token: 0x06002261 RID: 8801 RVA: 0x0015A241 File Offset: 0x00158441
		public bool IsFileCorrupted { get; private set; }

		// Token: 0x17000967 RID: 2407
		// (get) Token: 0x06002262 RID: 8802 RVA: 0x0015A24C File Offset: 0x0015844C
		// (set) Token: 0x06002263 RID: 8803 RVA: 0x0015A29C File Offset: 0x0015849C
		public bool RequiredContentPackagesInstalled
		{
			get
			{
				if (this.requiredContentPackagesInstalled != null)
				{
					return this.requiredContentPackagesInstalled.Value;
				}
				return this.RequiredContentPackages.All((string reqName) => ContentPackageManager.EnabledPackages.All.Any((ContentPackage contentPackage) => contentPackage.NameMatches(reqName)));
			}
			set
			{
				this.requiredContentPackagesInstalled = new bool?(value);
			}
		}

		// Token: 0x17000968 RID: 2408
		// (get) Token: 0x06002264 RID: 8804 RVA: 0x0015A2AA File Offset: 0x001584AA
		public bool SubsLeftBehind
		{
			get
			{
				if (this.subsLeftBehind != null)
				{
					return this.subsLeftBehind.Value;
				}
				this.CheckSubsLeftBehind(this.SubmarineElement);
				return this.subsLeftBehind.Value;
			}
		}

		// Token: 0x17000969 RID: 2409
		// (get) Token: 0x06002265 RID: 8805 RVA: 0x0015A2DC File Offset: 0x001584DC
		// (set) Token: 0x06002266 RID: 8806 RVA: 0x0015A2E4 File Offset: 0x001584E4
		public bool LeftBehindSubDockingPortOccupied { get; private set; }

		// Token: 0x1700096A RID: 2410
		// (get) Token: 0x06002267 RID: 8807 RVA: 0x0015A2ED File Offset: 0x001584ED
		// (set) Token: 0x06002268 RID: 8808 RVA: 0x0015A2F5 File Offset: 0x001584F5
		public HashSet<Identifier> LayersHiddenByDefault { get; private set; } = new HashSet<Identifier>();

		// Token: 0x06002269 RID: 8809 RVA: 0x0015A300 File Offset: 0x00158500
		public SubmarineInfo()
		{
			this.FilePath = null;
			this.DisplayName = TextManager.Get("UnspecifiedSubFileName");
			this.Name = this.DisplayName.Value;
			this.IsFileCorrupted = false;
			this.RequiredContentPackages = new HashSet<string>();
		}

		// Token: 0x0600226A RID: 8810 RVA: 0x0015A3A8 File Offset: 0x001585A8
		public SubmarineInfo(string filePath, string hash = "", XElement element = null, bool tryLoad = true, bool lazyLoad = false)
		{
			this.FilePath = filePath;
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				this.LastModifiedTime = File.GetLastWriteTime(filePath);
			}
			try
			{
				this.DisplayName = Path.GetFileNameWithoutExtension(filePath);
				this.Name = this.DisplayName.Value;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error loading submarine " + filePath + "!", e, null, false, false);
			}
			if (!string.IsNullOrWhiteSpace(hash))
			{
				this.hash = Md5Hash.StringAsHash(hash);
			}
			this.IsFileCorrupted = false;
			this.RequiredContentPackages = new HashSet<string>();
			if (element == null && tryLoad)
			{
				this.Reload();
			}
			else
			{
				this.SubmarineElement = element;
			}
			this.Name = (this.SubmarineElement.GetAttributeString("name", null) ?? this.Name);
			this.Init();
			if (lazyLoad)
			{
				this.LazyLoad = true;
				this.SubmarineElement = null;
			}
		}

		// Token: 0x0600226B RID: 8811 RVA: 0x0015A504 File Offset: 0x00158704
		public SubmarineInfo(Submarine sub) : this(sub.Info)
		{
			this.GameVersion = GameMain.Version;
			this.SubmarineElement = new XElement("Submarine");
			sub.SaveToXElement(this.SubmarineElement);
			this.Init();
		}

		// Token: 0x0600226C RID: 8812 RVA: 0x0015A544 File Offset: 0x00158744
		public SubmarineInfo(SubmarineInfo original)
		{
			this.Name = original.Name;
			this.DisplayName = original.DisplayName;
			this.Description = original.Description;
			this.Price = original.Price;
			this.InitialSuppliesSpawned = original.InitialSuppliesSpawned;
			this.NoItems = original.NoItems;
			this.LowFuel = original.LowFuel;
			this.GameVersion = original.GameVersion;
			this.Type = original.Type;
			this.SubmarineClass = original.SubmarineClass;
			this.hash = ((!string.IsNullOrEmpty(original.FilePath) && File.Exists(original.FilePath)) ? original.MD5Hash : null);
			this.Dimensions = original.Dimensions;
			this.CargoCapacity = original.CargoCapacity;
			this.FilePath = original.FilePath;
			this.RequiredContentPackages = new HashSet<string>(original.RequiredContentPackages);
			this.IsFileCorrupted = original.IsFileCorrupted;
			this.SubmarineElement = original.SubmarineElement;
			this.EqualityCheckVal = original.EqualityCheckVal;
			this.RecommendedCrewExperience = original.RecommendedCrewExperience;
			this.RecommendedCrewSizeMin = original.RecommendedCrewSizeMin;
			this.RecommendedCrewSizeMax = original.RecommendedCrewSizeMax;
			this.Tier = original.Tier;
			this.IsManuallyOutfitted = original.IsManuallyOutfitted;
			this.Tags = original.Tags;
			this.OutpostGenerationParams = original.OutpostGenerationParams;
			this.LayersHiddenByDefault = original.LayersHiddenByDefault;
			this.OutpostTags = original.OutpostTags;
			this.TriggerOutpostMissionEvents = original.TriggerOutpostMissionEvents;
			if (original.OutpostModuleInfo != null)
			{
				this.OutpostModuleInfo = new OutpostModuleInfo(original.OutpostModuleInfo);
			}
			else if (original.BeaconStationInfo != null)
			{
				this.BeaconStationInfo = new BeaconStationInfo(original.BeaconStationInfo);
			}
			else if (original.EnemySubmarineInfo != null)
			{
				this.EnemySubmarineInfo = new EnemySubmarineInfo(original.EnemySubmarineInfo);
			}
			else if (original.WreckInfo != null)
			{
				this.WreckInfo = new WreckInfo(original.WreckInfo);
			}
			this.PreviewImage = ((original.PreviewImage != null) ? new Sprite(original.PreviewImage) : null);
		}

		// Token: 0x0600226D RID: 8813 RVA: 0x0015A7AC File Offset: 0x001589AC
		public void Reload()
		{
			XDocument doc = null;
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				Exception e;
				doc = SubmarineInfo.OpenFile(this.FilePath, out e);
				if ((e != null && !(e is IOException)) || doc != null || i == maxLoadRetries || !File.Exists(this.FilePath))
				{
					break;
				}
				DebugConsole.NewMessage("Opening submarine file \"" + this.FilePath + "\" failed, retrying in 250 ms...", null, false);
				Thread.Sleep(250);
			}
			if (((doc != null) ? doc.Root : null) == null)
			{
				this.IsFileCorrupted = true;
				return;
			}
			if (this.hash == null)
			{
				this.StartHashDocTask(doc);
			}
			this.SubmarineElement = doc.Root;
		}

		// Token: 0x0600226E RID: 8814 RVA: 0x0015A85C File Offset: 0x00158A5C
		private void Init()
		{
			this.DisplayName = TextManager.Get("Submarine.Name." + this.Name).Fallback(this.Name, true);
			this.Description = TextManager.Get("Submarine.Description." + this.Name).Fallback(this.SubmarineElement.GetAttributeString("description", ""), true);
			this.EqualityCheckVal = this.SubmarineElement.GetAttributeInt("checkval", 0);
			this.Price = this.SubmarineElement.GetAttributeInt("price", 1000);
			this.InitialSuppliesSpawned = this.SubmarineElement.GetAttributeBool("initialsuppliesspawned", false);
			this.NoItems = this.SubmarineElement.GetAttributeBool("noitems", false);
			this.LowFuel = this.SubmarineElement.GetAttributeBool("lowfuel", false);
			this.IsManuallyOutfitted = this.SubmarineElement.GetAttributeBool("ismanuallyoutfitted", false);
			this.GameVersion = new Version(this.SubmarineElement.GetAttributeString("gameversion", "0.0.0.0"));
			SubmarineTag tags;
			if (Enum.TryParse<SubmarineTag>(this.SubmarineElement.GetAttributeString("tags", ""), out tags))
			{
				this.Tags = tags;
			}
			this.Dimensions = this.SubmarineElement.GetAttributeVector2("dimensions", Vector2.Zero);
			this.CargoCapacity = this.SubmarineElement.GetAttributeInt("cargocapacity", -1);
			this.RecommendedCrewSizeMin = this.SubmarineElement.GetAttributeInt("recommendedcrewsizemin", 0);
			this.RecommendedCrewSizeMax = this.SubmarineElement.GetAttributeInt("recommendedcrewsizemax", 0);
			Identifier recommendedCrewExperience = this.SubmarineElement.GetAttributeIdentifier("recommendedcrewexperience", SubmarineInfo.CrewExperienceLevel.Unknown.ToIdentifier<SubmarineInfo.CrewExperienceLevel>());
			foreach (Identifier hiddenLayer in this.SubmarineElement.GetAttributeIdentifierArray("layerhiddenbydefault", Array.Empty<Identifier>(), true))
			{
				this.LayersHiddenByDefault.Add(hiddenLayer);
			}
			if (recommendedCrewExperience == "Beginner")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceLow;
			}
			else if (recommendedCrewExperience == "Intermediate")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceMid;
			}
			else if (recommendedCrewExperience == "Experienced")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceHigh;
			}
			else
			{
				Enum.TryParse<SubmarineInfo.CrewExperienceLevel>(recommendedCrewExperience.Value, true, out this.RecommendedCrewExperience);
			}
			this.Tier = this.SubmarineElement.GetAttributeInt("tier", SubmarineInfo.GetDefaultTier(this.Price));
			this.OutpostTags = this.SubmarineElement.GetAttributeIdentifierImmutableHashSet("OutpostTags", ImmutableHashSet<Identifier>.Empty, true);
			this.TriggerOutpostMissionEvents = this.SubmarineElement.GetAttributeIdentifierImmutableHashSet("TriggerOutpostMissionEvents", ImmutableHashSet<Identifier>.Empty, true);
			if (this.GameVersion < new Version(1, 8, 0, 0) && this.OutpostTags.Contains("PvPOutpost"))
			{
				this.TriggerOutpostMissionEvents = this.TriggerOutpostMissionEvents.Add("deathmatchweapondrop".ToIdentifier());
			}
			XElement xelement = this.SubmarineElement;
			SubmarineType type;
			if (((xelement != null) ? xelement.Attribute("type") : null) != null && Enum.TryParse<SubmarineType>(this.SubmarineElement.GetAttributeString("type", ""), out type))
			{
				this.Type = type;
				if (this.Type == SubmarineType.OutpostModule)
				{
					this.OutpostModuleInfo = new OutpostModuleInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.BeaconStation)
				{
					this.BeaconStationInfo = new BeaconStationInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.EnemySubmarine)
				{
					this.EnemySubmarineInfo = new EnemySubmarineInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.Wreck)
				{
					this.WreckInfo = new WreckInfo(this, this.SubmarineElement);
				}
			}
			if (this.Type == SubmarineType.Player)
			{
				XElement xelement2 = this.SubmarineElement;
				if (((xelement2 != null) ? xelement2.Attribute("class") : null) != null)
				{
					string classStr = this.SubmarineElement.GetAttributeString("class", "Undefined");
					SubmarineClass submarineClass;
					if (classStr == "DeepDiver")
					{
						this.SubmarineClass = SubmarineClass.Scout;
					}
					else if (Enum.TryParse<SubmarineClass>(classStr, out submarineClass))
					{
						this.SubmarineClass = submarineClass;
					}
				}
			}
			else
			{
				this.SubmarineClass = SubmarineClass.Undefined;
			}
			this.RequiredContentPackages.Clear();
			string[] contentPackageNames = this.SubmarineElement.GetAttributeStringArray("requiredcontentpackages", Array.Empty<string>(), true, false);
			foreach (string contentPackageName in contentPackageNames)
			{
				this.RequiredContentPackages.Add(contentPackageName);
			}
			this.InitProjectSpecific();
		}

		// Token: 0x0600226F RID: 8815 RVA: 0x0015ACD4 File Offset: 0x00158ED4
		private void InitProjectSpecific()
		{
			string previewImageData = this.SubmarineElement.GetAttributeString("previewimage", "");
			if (!string.IsNullOrEmpty(previewImageData))
			{
				try
				{
					using (MemoryStream mem = new MemoryStream(Convert.FromBase64String(previewImageData)))
					{
						Texture2D texture = TextureLoader.FromStream(mem, this.FilePath, false, false, null);
						if (texture == null)
						{
							throw new Exception("PreviewImage texture returned null");
						}
						this.PreviewImage = new Sprite(texture, null, null, 0f, this.FilePath);
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Loading the preview image of the submarine \"" + this.Name + "\" failed. The file may be corrupted.", e, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("Submarine..ctor:PreviewImageLoadingFailed", GameAnalyticsManager.ErrorSeverity.Error, "Loading the preview image of the submarine \"" + this.Name + "\" failed. The file may be corrupted.");
					this.PreviewImage = null;
				}
			}
		}

		// Token: 0x06002270 RID: 8816 RVA: 0x0015ADCC File Offset: 0x00158FCC
		public void Dispose()
		{
			Sprite previewImage = this.PreviewImage;
			if (previewImage != null)
			{
				previewImage.Remove();
			}
			this.PreviewImage = null;
			if (SubmarineInfo.savedSubmarines.Contains(this))
			{
				SubmarineInfo.savedSubmarines.Remove(this);
			}
		}

		// Token: 0x06002271 RID: 8817 RVA: 0x0015ADFF File Offset: 0x00158FFF
		public void UnloadSubmarineElement()
		{
			this.SubmarineElement = null;
		}

		// Token: 0x06002272 RID: 8818 RVA: 0x0015AE08 File Offset: 0x00159008
		public bool IsVanillaSubmarine()
		{
			if (this.FilePath == null)
			{
				return false;
			}
			ContentPackage vanilla = GameMain.VanillaContent;
			if (vanilla != null)
			{
				IEnumerable<BaseSubFile> vanillaSubs = vanilla.GetFiles<BaseSubFile>();
				string pathToCompare = this.FilePath.CleanUpPath();
				if (vanillaSubs.Any((BaseSubFile sub) => sub.Path == pathToCompare))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002273 RID: 8819 RVA: 0x0015AE60 File Offset: 0x00159060
		public void StartHashDocTask(XDocument doc)
		{
			if (this.hash != null)
			{
				return;
			}
			if (this.hashTask != null)
			{
				return;
			}
			this.hashTask = new Task(delegate()
			{
				this.hash = Md5Hash.CalculateForString(doc.ToString(), Md5Hash.StringHashOptions.IgnoreWhitespace);
			});
			this.hashTask.Start();
		}

		// Token: 0x06002274 RID: 8820 RVA: 0x0015AEBB File Offset: 0x001590BB
		public bool HasTag(SubmarineTag tag)
		{
			return this.Tags.HasFlag(tag);
		}

		// Token: 0x06002275 RID: 8821 RVA: 0x0015AED3 File Offset: 0x001590D3
		public void AddTag(SubmarineTag tag)
		{
			if (this.Tags.HasFlag(tag))
			{
				return;
			}
			this.Tags |= tag;
		}

		// Token: 0x06002276 RID: 8822 RVA: 0x0015AEFC File Offset: 0x001590FC
		public void RemoveTag(SubmarineTag tag)
		{
			if (!this.Tags.HasFlag(tag))
			{
				return;
			}
			this.Tags &= ~tag;
		}

		// Token: 0x06002277 RID: 8823 RVA: 0x0015AF28 File Offset: 0x00159128
		public void CheckSubsLeftBehind(XElement element = null)
		{
			if (element == null)
			{
				element = this.SubmarineElement;
			}
			this.subsLeftBehind = new bool?(false);
			this.LeftBehindSubDockingPortOccupied = false;
			this.LeftBehindDockingPortIDs.Clear();
			this.BlockedDockingPortIDs.Clear();
			foreach (XElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("linkedsubmarine", StringComparison.OrdinalIgnoreCase) && subElement.Attribute("location") != null)
				{
					this.subsLeftBehind = new bool?(true);
					ushort targetDockingPortID = (ushort)subElement.GetAttributeInt("originallinkedto", 0);
					this.LeftBehindDockingPortIDs.Add(targetDockingPortID);
					XElement targetPortElement = (targetDockingPortID == 0) ? null : element.Elements().FirstOrDefault((XElement e) => e.GetAttributeInt("ID", 0) == (int)targetDockingPortID);
					if (targetPortElement != null && targetPortElement.GetAttributeIntArray("linked", Array.Empty<int>()).Length != 0)
					{
						this.BlockedDockingPortIDs.Add(targetDockingPortID);
						this.LeftBehindSubDockingPortOccupied = true;
					}
				}
			}
		}

		// Token: 0x06002278 RID: 8824 RVA: 0x0015B060 File Offset: 0x00159260
		public bool IsCrushDepthDefinedInStructures(out float realWorldCrushDepth)
		{
			if (this.SubmarineElement == null)
			{
				realWorldCrushDepth = 3500f;
				return false;
			}
			bool structureCrushDepthsDefined = false;
			realWorldCrushDepth = float.PositiveInfinity;
			foreach (XElement structureElement in this.SubmarineElement.GetChildElements("structure", StringComparison.OrdinalIgnoreCase))
			{
				XAttribute xattribute = structureElement.Attribute("name");
				string name = ((xattribute != null) ? xattribute.Value : null) ?? "";
				Identifier identifier = structureElement.GetAttributeIdentifier("identifier", "");
				StructurePrefab structurePrefab = Structure.FindPrefab(name, identifier);
				if (structurePrefab != null && structurePrefab.Body)
				{
					if (!structureCrushDepthsDefined && structureElement.Attribute("crushdepth") != null)
					{
						structureCrushDepthsDefined = true;
					}
					float structureCrushDepth = structureElement.GetAttributeFloat("crushdepth", float.PositiveInfinity);
					realWorldCrushDepth = Math.Min(structureCrushDepth, realWorldCrushDepth);
				}
			}
			if (!structureCrushDepthsDefined)
			{
				realWorldCrushDepth = 3500f;
			}
			return structureCrushDepthsDefined;
		}

		// Token: 0x06002279 RID: 8825 RVA: 0x0015B160 File Offset: 0x00159360
		public void AddOutpostNPCIdentifierOrTag(Character npc, Identifier idOrTag)
		{
			if (!this.OutpostNPCs.ContainsKey(idOrTag))
			{
				this.OutpostNPCs.Add(idOrTag, new List<Character>());
			}
			this.OutpostNPCs[idOrTag].Add(npc);
		}

		// Token: 0x0600227A RID: 8826 RVA: 0x0015B194 File Offset: 0x00159394
		public void SaveAs(string filePath, MemoryStream previewImage = null)
		{
			XName name = this.SubmarineElement.Name;
			object[] array = new object[2];
			array[0] = from a in this.SubmarineElement.Attributes()
			where !string.Equals(a.Name.LocalName, "previewimage", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(a.Name.LocalName, "name", StringComparison.InvariantCultureIgnoreCase)
			select a;
			array[1] = this.SubmarineElement.Elements();
			XElement newElement = new XElement(name, array);
			if (this.Type == SubmarineType.OutpostModule)
			{
				this.OutpostModuleInfo.Save(newElement);
				this.OutpostModuleInfo = new OutpostModuleInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.BeaconStation)
			{
				this.BeaconStationInfo.Save(newElement);
				this.BeaconStationInfo = new BeaconStationInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.EnemySubmarine)
			{
				this.EnemySubmarineInfo.Save(newElement);
				this.EnemySubmarineInfo = new EnemySubmarineInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.Wreck)
			{
				this.WreckInfo.Save(newElement);
				this.WreckInfo = new WreckInfo(this, newElement);
			}
			XDocument doc = new XDocument(new object[]
			{
				newElement
			});
			doc.Root.Add(new XAttribute("name", this.Name));
			if (previewImage != null && this.AllowPreviewImage)
			{
				doc.Root.Add(new XAttribute("previewimage", Convert.ToBase64String(previewImage.ToArray())));
			}
			SaveUtil.CompressStringToFile(filePath, doc.ToString());
		}

		// Token: 0x0600227B RID: 8827 RVA: 0x0015B2F2 File Offset: 0x001594F2
		public static void AddToSavedSubs(SubmarineInfo subInfo)
		{
			SubmarineInfo.savedSubmarines.Add(subInfo);
		}

		// Token: 0x0600227C RID: 8828 RVA: 0x0015B300 File Offset: 0x00159500
		public static void RemoveSavedSub(string filePath)
		{
			string fullPath = Path.GetFullPath(filePath);
			for (int i = SubmarineInfo.savedSubmarines.Count - 1; i >= 0; i--)
			{
				if (Path.GetFullPath(SubmarineInfo.savedSubmarines[i].FilePath) == fullPath)
				{
					SubmarineInfo.savedSubmarines[i].Dispose();
				}
			}
		}

		// Token: 0x0600227D RID: 8829 RVA: 0x0015B358 File Offset: 0x00159558
		public static void RefreshSavedSub(string filePath)
		{
			SubmarineInfo.RemoveSavedSub(filePath);
			if (File.Exists(filePath))
			{
				SubmarineInfo subInfo = new SubmarineInfo(filePath, "", null, true, true);
				if (!subInfo.IsFileCorrupted)
				{
					SubmarineInfo.savedSubmarines.Add(subInfo);
				}
				SubmarineInfo.savedSubmarines = (from s in SubmarineInfo.savedSubmarines
				orderby s.FilePath ?? ""
				select s).ToList<SubmarineInfo>();
			}
		}

		// Token: 0x0600227E RID: 8830 RVA: 0x0015B3C8 File Offset: 0x001595C8
		public static void RefreshSavedSubs()
		{
			IEnumerable<BaseSubFile> contentPackageSubs = ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage c) => c.GetFiles<BaseSubFile>());
			int i = SubmarineInfo.savedSubmarines.Count - 1;
			while (i >= 0)
			{
				if (!File.Exists(SubmarineInfo.savedSubmarines[i].FilePath))
				{
					goto IL_E2;
				}
				bool isDownloadedSub = Path.GetFullPath(Path.GetDirectoryName(SubmarineInfo.savedSubmarines[i].FilePath)) == Path.GetFullPath(SaveUtil.SubmarineDownloadFolder);
				bool isInContentPackage = contentPackageSubs.Any((BaseSubFile f) => f.Path == SubmarineInfo.savedSubmarines[i].FilePath);
				if (!isDownloadedSub && (!(SubmarineInfo.savedSubmarines[i].LastModifiedTime == File.GetLastWriteTime(SubmarineInfo.savedSubmarines[i].FilePath)) || !isInContentPackage))
				{
					goto IL_E2;
				}
				IL_F7:
				int j = i;
				i = j - 1;
				continue;
				IL_E2:
				SubmarineInfo.savedSubmarines[i].Dispose();
				goto IL_F7;
			}
			List<string> filePaths = new List<string>();
			using (IEnumerator<BaseSubFile> enumerator = contentPackageSubs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BaseSubFile subFile = enumerator.Current;
					if (File.Exists(subFile.Path.Value) && !filePaths.Any((string fp) => fp == subFile.Path))
					{
						filePaths.Add(subFile.Path.Value);
					}
				}
			}
			filePaths.RemoveAll((string p) => SubmarineInfo.savedSubmarines.Any((SubmarineInfo sub) => sub.FilePath == p));
			foreach (string path in filePaths)
			{
				SubmarineInfo subInfo = new SubmarineInfo(path, "", null, true, false);
				if (!subInfo.IsFileCorrupted)
				{
					SubmarineInfo.savedSubmarines.Add(subInfo);
				}
			}
		}

		// Token: 0x0600227F RID: 8831 RVA: 0x0015B600 File Offset: 0x00159800
		public static XDocument OpenFile(string file)
		{
			Exception ex;
			return SubmarineInfo.OpenFile(file, out ex);
		}

		// Token: 0x06002280 RID: 8832 RVA: 0x0015B618 File Offset: 0x00159818
		public static XDocument OpenFile(string file, out Exception exception)
		{
			XDocument doc = null;
			string extension = "";
			exception = null;
			try
			{
				extension = Path.GetExtension(file);
			}
			catch
			{
				file += ".sub";
			}
			if (string.IsNullOrWhiteSpace(extension))
			{
				extension = ".sub";
				file += ".sub";
			}
			if (extension == ".sub")
			{
				Stream stream;
				try
				{
					stream = SaveUtil.DecompressFileToStream(file);
				}
				catch (FileNotFoundException e)
				{
					exception = e;
					DebugConsole.ThrowError("Loading submarine \"" + file + "\" failed! (File not found) " + Environment.StackTrace.CleanupStackTrace(), e, null, false, false);
					return null;
				}
				catch (Exception e2)
				{
					exception = e2;
					DebugConsole.ThrowError("Loading submarine \"" + file + "\" failed!", e2, null, false, false);
					return null;
				}
				try
				{
					stream.Position = 0L;
					using (XmlReader reader = XMLExtensions.CreateReader(stream, ""))
					{
						doc = XDocument.Load(reader);
					}
					stream.Close();
					stream.Dispose();
					return doc;
				}
				catch (Exception e3)
				{
					exception = e3;
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Loading submarine \"",
						file,
						"\" failed! (",
						e3.Message,
						")"
					}), null, null, false, false);
					return null;
				}
			}
			if (extension == ".xml")
			{
				try
				{
					ToolBox.IsProperFilenameCase(file);
					using (FileStream stream2 = File.Open(file, FileMode.Open, FileAccess.Read, null, true))
					{
						using (XmlReader reader2 = XMLExtensions.CreateReader(stream2, ""))
						{
							doc = XDocument.Load(reader2);
						}
					}
					return doc;
				}
				catch (Exception e4)
				{
					exception = e4;
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Loading submarine \"",
						file,
						"\" failed! (",
						e4.Message,
						")"
					}), null, null, false, false);
					return null;
				}
			}
			DebugConsole.ThrowError("Couldn't load submarine \"" + file + "! (Unrecognized file extension)", null, null, false, false);
			return null;
		}

		// Token: 0x06002281 RID: 8833 RVA: 0x0015B884 File Offset: 0x00159A84
		public int GetPrice(Location location = null, ImmutableHashSet<Character> characterList = null)
		{
			if (location == null)
			{
				GameSession gameSession = GameMain.GameSession;
				Location location2;
				if (gameSession == null)
				{
					location2 = null;
				}
				else
				{
					CampaignMode campaign2 = gameSession.Campaign;
					if (campaign2 == null)
					{
						location2 = null;
					}
					else
					{
						Map map = campaign2.Map;
						location2 = ((map != null) ? map.CurrentLocation : null);
					}
				}
				Location currentLocation = location2;
				if (currentLocation == null)
				{
					return this.Price;
				}
				location = currentLocation;
			}
			if (characterList == null)
			{
				characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			}
			float price = (float)this.Price;
			GameSession gameSession2 = GameMain.GameSession;
			CampaignMode campaign = (gameSession2 != null) ? gameSession2.Campaign : null;
			if (campaign != null)
			{
				price *= campaign.Settings.ShipyardPriceMultiplier;
			}
			if (characterList.Any<Character>())
			{
				Faction faction = location.Faction;
				if (faction != null && Faction.GetPlayerAffiliationStatus(faction) == FactionAffiliation.Positive)
				{
					price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplierAffiliated, true));
				}
				price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplier, true));
			}
			return (int)price;
		}

		// Token: 0x06002282 RID: 8834 RVA: 0x0015B97B File Offset: 0x00159B7B
		public static int GetDefaultTier(int price)
		{
			if (price > 20000)
			{
				return 3;
			}
			if (price <= 10000)
			{
				return 1;
			}
			return 2;
		}

		// Token: 0x04001134 RID: 4404
		public Sprite PreviewImage;

		// Token: 0x04001136 RID: 4406
		private static List<SubmarineInfo> savedSubmarines = new List<SubmarineInfo>();

		// Token: 0x04001137 RID: 4407
		private Task hashTask;

		// Token: 0x04001138 RID: 4408
		private Md5Hash hash;

		// Token: 0x04001139 RID: 4409
		public readonly DateTime LastModifiedTime;

		// Token: 0x0400113B RID: 4411
		public int RecommendedCrewSizeMin = 1;

		// Token: 0x0400113C RID: 4412
		public int RecommendedCrewSizeMax = 2;

		// Token: 0x0400113D RID: 4413
		public SubmarineInfo.CrewExperienceLevel RecommendedCrewExperience;

		// Token: 0x04001140 RID: 4416
		public HashSet<string> RequiredContentPackages = new HashSet<string>();

		// Token: 0x04001141 RID: 4417
		public const int MaxNameLength = 30;

		// Token: 0x04001142 RID: 4418
		public const int MaxDescriptionLength = 500;

		// Token: 0x0400114D RID: 4429
		public SubmarineClass SubmarineClass;

		// Token: 0x04001157 RID: 4439
		public bool IsFromRemoteStorage;

		// Token: 0x04001158 RID: 4440
		public readonly bool LazyLoad;

		// Token: 0x04001159 RID: 4441
		private XElement submarineElement;

		// Token: 0x0400115B RID: 4443
		private bool? requiredContentPackagesInstalled;

		// Token: 0x0400115C RID: 4444
		private bool? subsLeftBehind;

		// Token: 0x0400115D RID: 4445
		public readonly List<ushort> LeftBehindDockingPortIDs = new List<ushort>();

		// Token: 0x0400115E RID: 4446
		public readonly List<ushort> BlockedDockingPortIDs = new List<ushort>();

		// Token: 0x04001160 RID: 4448
		public OutpostGenerationParams OutpostGenerationParams;

		// Token: 0x04001161 RID: 4449
		public readonly Dictionary<Identifier, List<Character>> OutpostNPCs = new Dictionary<Identifier, List<Character>>();

		// Token: 0x04001163 RID: 4451
		public const int HighestTier = 3;

		// Token: 0x02000BAD RID: 2989
		public readonly struct PendingSubInfo : IEquatable<SubmarineInfo.PendingSubInfo>
		{
			// Token: 0x06007980 RID: 31104 RVA: 0x0037ED87 File Offset: 0x0037CF87
			public PendingSubInfo(SubmarineInfo PendingSub = null, bool StructuresDefineRealWorldCrushDepth = false, float RealWorldCrushDepth = 3500f)
			{
				this.PendingSub = PendingSub;
				this.StructuresDefineRealWorldCrushDepth = StructuresDefineRealWorldCrushDepth;
				this.RealWorldCrushDepth = RealWorldCrushDepth;
			}

			// Token: 0x17001AC0 RID: 6848
			// (get) Token: 0x06007981 RID: 31105 RVA: 0x0037ED9E File Offset: 0x0037CF9E
			// (set) Token: 0x06007982 RID: 31106 RVA: 0x0037EDA6 File Offset: 0x0037CFA6
			public SubmarineInfo PendingSub { get; set; }

			// Token: 0x17001AC1 RID: 6849
			// (get) Token: 0x06007983 RID: 31107 RVA: 0x0037EDAF File Offset: 0x0037CFAF
			// (set) Token: 0x06007984 RID: 31108 RVA: 0x0037EDB7 File Offset: 0x0037CFB7
			public bool StructuresDefineRealWorldCrushDepth { get; set; }

			// Token: 0x17001AC2 RID: 6850
			// (get) Token: 0x06007985 RID: 31109 RVA: 0x0037EDC0 File Offset: 0x0037CFC0
			// (set) Token: 0x06007986 RID: 31110 RVA: 0x0037EDC8 File Offset: 0x0037CFC8
			public float RealWorldCrushDepth { get; set; }

			// Token: 0x06007987 RID: 31111 RVA: 0x0037EDD4 File Offset: 0x0037CFD4
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("PendingSubInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06007988 RID: 31112 RVA: 0x0037EE20 File Offset: 0x0037D020
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("PendingSub = ");
				builder.Append(this.PendingSub);
				builder.Append(", StructuresDefineRealWorldCrushDepth = ");
				builder.Append(this.StructuresDefineRealWorldCrushDepth.ToString());
				builder.Append(", RealWorldCrushDepth = ");
				builder.Append(this.RealWorldCrushDepth.ToString());
				return true;
			}

			// Token: 0x06007989 RID: 31113 RVA: 0x0037EE95 File Offset: 0x0037D095
			[CompilerGenerated]
			public static bool operator !=(SubmarineInfo.PendingSubInfo left, SubmarineInfo.PendingSubInfo right)
			{
				return !(left == right);
			}

			// Token: 0x0600798A RID: 31114 RVA: 0x0037EEA1 File Offset: 0x0037D0A1
			[CompilerGenerated]
			public static bool operator ==(SubmarineInfo.PendingSubInfo left, SubmarineInfo.PendingSubInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600798B RID: 31115 RVA: 0x0037EEAB File Offset: 0x0037D0AB
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<SubmarineInfo>.Default.GetHashCode(this.<PendingSub>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<StructuresDefineRealWorldCrushDepth>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<RealWorldCrushDepth>k__BackingField);
			}

			// Token: 0x0600798C RID: 31116 RVA: 0x0037EEEB File Offset: 0x0037D0EB
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SubmarineInfo.PendingSubInfo && this.Equals((SubmarineInfo.PendingSubInfo)obj);
			}

			// Token: 0x0600798D RID: 31117 RVA: 0x0037EF04 File Offset: 0x0037D104
			[CompilerGenerated]
			public bool Equals(SubmarineInfo.PendingSubInfo other)
			{
				return EqualityComparer<SubmarineInfo>.Default.Equals(this.<PendingSub>k__BackingField, other.<PendingSub>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<StructuresDefineRealWorldCrushDepth>k__BackingField, other.<StructuresDefineRealWorldCrushDepth>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<RealWorldCrushDepth>k__BackingField, other.<RealWorldCrushDepth>k__BackingField);
			}

			// Token: 0x0600798E RID: 31118 RVA: 0x0037EF59 File Offset: 0x0037D159
			[CompilerGenerated]
			public void Deconstruct(out SubmarineInfo PendingSub, out bool StructuresDefineRealWorldCrushDepth, out float RealWorldCrushDepth)
			{
				PendingSub = this.PendingSub;
				StructuresDefineRealWorldCrushDepth = this.StructuresDefineRealWorldCrushDepth;
				RealWorldCrushDepth = this.RealWorldCrushDepth;
			}
		}

		// Token: 0x02000BAE RID: 2990
		public enum CrewExperienceLevel
		{
			// Token: 0x0400488F RID: 18575
			Unknown,
			// Token: 0x04004890 RID: 18576
			CrewExperienceLow,
			// Token: 0x04004891 RID: 18577
			CrewExperienceMid,
			// Token: 0x04004892 RID: 18578
			CrewExperienceHigh
		}
	}
}
