using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000B5 RID: 181
	internal class SubmarineSelection
	{
		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x060016B5 RID: 5813 RVA: 0x000D6899 File Offset: 0x000D4A99
		// (set) Token: 0x060016B6 RID: 5814 RVA: 0x000D68A1 File Offset: 0x000D4AA1
		private bool TransferItemsOnSwitch
		{
			get
			{
				return this.transferItemsOnSwitch;
			}
			set
			{
				this.transferItemsOnSwitch = value;
				if (this.transferItemsTickBox != null)
				{
					this.transferItemsTickBox.Selected = value;
				}
			}
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x000D68C0 File Offset: 0x000D4AC0
		public SubmarineSelection(bool transfer, Action closeAction, RectTransform parent)
		{
			if (GameMain.GameSession.Campaign == null)
			{
				return;
			}
			this.transferService = transfer;
			this.purchaseService = !transfer;
			this.parent = parent;
			this.closeAction = closeAction;
			this.subsToShow = new List<SubmarineInfo>();
			if (GameMain.Client == null)
			{
				this.messageBoxOptions = new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("Cancel")
				};
			}
			else
			{
				this.messageBoxOptions = new LocalizedString[]
				{
					TextManager.Get("Yes") + " " + TextManager.Get("initiatevoting"),
					TextManager.Get("Cancel")
				};
			}
			Submarine mainSub = Submarine.MainSub;
			if (((mainSub != null) ? mainSub.Info : null) == null)
			{
				return;
			}
			this.Initialize();
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x000D69B4 File Offset: 0x000D4BB4
		private void Initialize()
		{
			this.initialized = true;
			this.selectedSubText = TextManager.Get("selectedsub");
			this.switchText = TextManager.Get("switchtosubmarinebutton");
			this.purchaseAndSwitchText = TextManager.Get("purchaseandswitch");
			this.purchaseOnlyText = TextManager.Get("purchase");
			this.currencyName = TextManager.Get("credit").Value.ToLowerInvariant();
			this.UpdateSubmarines();
			this.missingPreviewText = TextManager.Get("SubPreviewImageNotFound");
			this.CreateGUI();
		}

		// Token: 0x060016B9 RID: 5817 RVA: 0x000D6A44 File Offset: 0x000D4C44
		private void CreateGUI()
		{
			this.createdForResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.GuiFrame = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.7f), this.parent, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.02f)
			}, "", null);
			this.selectionIndicatorThickness = HUDLayoutSettings.Padding / 2;
			GUIFrame background = new GUIFrame(new RectTransform(this.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, this.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "", new Color?(Color.Black * 0.9f))
			{
				CanBeFocused = false
			};
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Point(background.Rect.Width - HUDLayoutSettings.Padding * 4, background.Rect.Height - HUDLayoutSettings.Padding * 4), background.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = (int)((float)HUDLayoutSettings.Padding * 1.5f)
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.transferService ? TextManager.Get("switchsubmarineheader") : TextManager.GetWithVariable("outpostshipyard", "[location]", GameMain.GameSession.Map.CurrentLocation.DisplayName, FormatCapitals.No);
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock header = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			header.CalculateHeightFromText(0, true);
			this.playerBalanceElement = CampaignUI.AddBalanceElement(header, new Vector2(1f, 1.5f));
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup submarineContentGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = HUDLayoutSettings.Padding,
				Stretch = true
			};
			this.submarineHorizontalGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), submarineContentGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				IsHorizontal = true,
				AbsoluteSpacing = HUDLayoutSettings.Padding,
				Stretch = true
			};
			this.submarineControlsGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), submarineContentGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopCenter);
			GUILayoutGroup infoFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				IsHorizontal = true,
				Stretch = true,
				AbsoluteSpacing = HUDLayoutSettings.Padding
			};
			new GUIFrame(new RectTransform(Vector2.One, infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(new Color(8, 13, 19))).IgnoreLayoutGroups = true;
			this.listBackground = new GUIImage(new RectTransform(new Vector2(0.59f, 1f), infoFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, true)
			{
				IgnoreLayoutGroups = true
			};
			GUIListBox guilistBox = new GUIListBox(new RectTransform(Vector2.One, infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.IgnoreLayoutGroups = true;
			guilistBox.CanBeFocused = false;
			this.specsFrame = new GUIListBox(new RectTransform(new Vector2(0.39f, 1f), infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				CurrentSelectMode = GUIListBox.SelectMode.None,
				Spacing = GUI.IntScale(5f),
				Padding = new Vector4((float)HUDLayoutSettings.Padding / 2f, (float)HUDLayoutSettings.Padding, 0f, 0f)
			};
			new GUIFrame(new RectTransform(new Vector2(0.02f, 0.8f), infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, "VerticalLine", null);
			GUIListBox descriptionFrame = new GUIListBox(new RectTransform(new Vector2(0.59f, 1f), infoFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				Padding = new Vector4((float)HUDLayoutSettings.Padding / 2f, (float)HUDLayoutSettings.Padding * 1.5f, (float)HUDLayoutSettings.Padding * 1.5f, (float)HUDLayoutSettings.Padding / 2f)
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), descriptionFrame.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = string.Empty;
			font = GUIStyle.Font;
			this.descriptionTextBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, true, "", null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup bottomContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.CenterRight)
			{
				IsHorizontal = true,
				AbsoluteSpacing = HUDLayoutSettings.Padding
			};
			float transferInfoFrameWidth = 1f;
			if (this.closeAction != null)
			{
				GUIButton closeButton = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "GUIButtonFreeScale", null)
				{
					OnClicked = delegate(GUIButton button, object userData)
					{
						this.closeAction();
						return true;
					}
				};
				transferInfoFrameWidth -= closeButton.RectTransform.RelativeSize.X;
			}
			if (this.purchaseService)
			{
				this.confirmButtonAlt = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.purchaseOnlyText, Alignment.Center, "GUIButtonFreeScale", null);
				transferInfoFrameWidth -= this.confirmButtonAlt.RectTransform.RelativeSize.X;
			}
			this.confirmButton = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.purchaseService ? this.purchaseAndSwitchText : this.switchText, Alignment.Center, "GUIButtonFreeScale", null);
			this.SetConfirmButtonState(false);
			transferInfoFrameWidth -= this.confirmButton.RectTransform.RelativeSize.X;
			GUIFrame transferInfoFrame = new GUIFrame(new RectTransform(new Vector2(transferInfoFrameWidth, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			this.transferItemsTickBox = new GUITickBox(new RectTransform(new Vector2(0.2f, 1f), transferInfoFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("transferitems"), GUIStyle.SubHeadingFont, "")
			{
				Selected = this.TransferItemsOnSwitch,
				Visible = false,
				OnSelected = ((GUITickBox tb) => this.transferItemsOnSwitch = tb.Selected)
			};
			this.transferItemsTickBox.RectTransform.Resize(new Point(Math.Min((int)this.transferItemsTickBox.ContentWidth, transferInfoFrame.Rect.Width), this.transferItemsTickBox.Rect.Height), true);
			this.itemTransferInfoBlock = new GUITextBlock(new RectTransform(Vector2.One, transferInfoFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null, null, Alignment.Left, true, "", null)
			{
				TextAlignment = Alignment.CenterRight,
				Visible = false
			};
			this.pageIndicatorHolder = new GUIFrame(new RectTransform(new Vector2(1f, 1.5f), this.submarineControlsGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.pageIndicator = GUIStyle.GetComponentStyle("GUIPageIndicator").GetDefaultSprite();
			this.UpdatePaging();
			for (int i = 0; i < this.submarineDisplays.Length; i++)
			{
				SubmarineSelection.SubmarineDisplayContent submarineDisplayElement = new SubmarineSelection.SubmarineDisplayContent
				{
					background = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f), this.submarineHorizontalGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(new Color(8, 13, 19)))
				};
				submarineDisplayElement.submarineImage = new GUIImage(new RectTransform(new Vector2(0.8f, 1f), submarineDisplayElement.background.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, true);
				submarineDisplayElement.middleTextBlock = new GUITextBlock(new RectTransform(new Vector2(0.8f, 1f), submarineDisplayElement.background.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Center, false, "", null);
				RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
				rectTransform.AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding);
				RichString text3 = string.Empty;
				font = GUIStyle.SubHeadingFont;
				submarineDisplayElement.submarineName = new GUITextBlock(rectTransform, text3, null, font, Alignment.Center, false, "", null);
				RectTransform rectTransform2 = new RectTransform(new Vector2(1f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
				rectTransform2.AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding);
				RichString text4 = string.Empty;
				font = GUIStyle.SubHeadingFont;
				submarineDisplayElement.submarineFee = new GUITextBlock(rectTransform2, text4, null, font, Alignment.Center, false, "", null);
				submarineDisplayElement.selectSubmarineButton = new GUIButton(new RectTransform(Vector2.One, submarineDisplayElement.background.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null);
				submarineDisplayElement.previewButton = new GUIButton(new RectTransform(Vector2.One * 0.12f, submarineDisplayElement.background.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point((int)(0.03f * (float)background.Rect.Height))
				}, Alignment.Center, "ExpandButton", null)
				{
					Color = Color.White,
					HoverColor = Color.White,
					PressedColor = Color.White
				};
				submarineDisplayElement.submarineClass = new GUITextBlock(new RectTransform(new Vector2(1f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding + (int)GUIStyle.Font.MeasureString(submarineDisplayElement.submarineName.Text, false).Y)
				}, string.Empty, null, null, Alignment.Left, false, "", null);
				submarineDisplayElement.submarineTier = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding + (int)GUIStyle.Font.MeasureString(submarineDisplayElement.submarineName.Text, false).Y)
				}, string.Empty, null, null, Alignment.Right, false, "", null);
				this.submarineDisplays[i] = submarineDisplayElement;
			}
			this.selectedSubmarineIndicator = new GUICustomComponent(new RectTransform(Point.Zero, this.submarineHorizontalGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				this.DrawSubmarineIndicator(sb, component.Rect);
			}, null)
			{
				IgnoreLayoutGroups = true,
				CanBeFocused = false
			};
		}

		// Token: 0x060016BA RID: 5818 RVA: 0x000D79C4 File Offset: 0x000D5BC4
		private void UpdatePaging()
		{
			if (this.pageIndicatorHolder == null)
			{
				return;
			}
			this.pageIndicatorHolder.ClearChildren();
			if (this.currentPage > this.pageCount)
			{
				this.currentPage = this.pageCount;
			}
			if (this.pageCount < 2)
			{
				return;
			}
			this.browseLeftButton = new GUIButton(new RectTransform(new Vector2(1.15f, 1.15f), this.pageIndicatorHolder.RectTransform, Anchor.CenterLeft, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(-HUDLayoutSettings.Padding * 3, 0)
			}, string.Empty, Alignment.Center, "GUIButtonToggleLeft", null)
			{
				IgnoreLayoutGroups = true,
				OnClicked = delegate(GUIButton button, object userData)
				{
					this.ChangePage(-1);
					return true;
				}
			};
			Point indicatorSize = new Point(GUI.IntScale((float)this.pageIndicator.SourceRect.Width * 1.5f), GUI.IntScale((float)this.pageIndicator.SourceRect.Height * 1.5f));
			this.pageIndicatorHolder.RectTransform.NonScaledSize = new Point(this.pageCount * indicatorSize.X + HUDLayoutSettings.Padding * (this.pageCount - 1), this.pageIndicatorHolder.RectTransform.NonScaledSize.Y);
			int xPos = 0;
			int yPos = this.pageIndicatorHolder.Rect.Height / 2 - indicatorSize.Y / 2;
			this.pageIndicators = new GUIImage[this.pageCount];
			for (int i = 0; i < this.pageCount; i++)
			{
				this.pageIndicators[i] = new GUIImage(new RectTransform(indicatorSize, this.pageIndicatorHolder.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(xPos, yPos)
				}, this.pageIndicator, true, null);
				xPos += indicatorSize.X + HUDLayoutSettings.Padding;
			}
			for (int j = 0; j < this.pageIndicators.Length; j++)
			{
				this.pageIndicators[j].Color = ((j == this.currentPage - 1) ? Color.White : Color.Gray);
			}
			this.browseRightButton = new GUIButton(new RectTransform(new Vector2(1.15f, 1.15f), this.pageIndicatorHolder.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(-HUDLayoutSettings.Padding * 3, 0)
			}, string.Empty, Alignment.Center, "GUIButtonToggleRight", null)
			{
				IgnoreLayoutGroups = true,
				OnClicked = delegate(GUIButton button, object userData)
				{
					this.ChangePage(1);
					return true;
				}
			};
			this.browseLeftButton.Enabled = (this.currentPage > 1);
			this.browseRightButton.Enabled = (this.currentPage < this.pageCount);
		}

		// Token: 0x060016BB RID: 5819 RVA: 0x000D7CAD File Offset: 0x000D5EAD
		private void DrawSubmarineIndicator(SpriteBatch spriteBatch, Rectangle area)
		{
			if (area == Rectangle.Empty)
			{
				return;
			}
			GUI.DrawRectangle(spriteBatch, area, SubmarineSelection.indicatorColor, false, 0f, (float)this.selectionIndicatorThickness);
		}

		// Token: 0x060016BC RID: 5820 RVA: 0x000D7CD8 File Offset: 0x000D5ED8
		public void Update()
		{
			if (SubmarineSelection.ContentRefreshRequired)
			{
				this.RefreshSubmarineDisplay(true, false);
			}
			else
			{
				this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
			}
			if (PlayerInput.KeyHit(Keys.Left))
			{
				this.SelectSubmarine(this.subsToShow.IndexOf(this.selectedSubmarine), -1);
				return;
			}
			if (PlayerInput.KeyHit(Keys.Right))
			{
				this.SelectSubmarine(this.subsToShow.IndexOf(this.selectedSubmarine), 1);
			}
		}

		// Token: 0x060016BD RID: 5821 RVA: 0x000D7D4C File Offset: 0x000D5F4C
		public void RefreshSubmarineDisplay(bool updateSubs, bool setTransferOptionToTrue = false)
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			if (GameMain.GraphicsWidth != this.createdForResolution.X || GameMain.GraphicsHeight != this.createdForResolution.Y)
			{
				this.CreateGUI();
			}
			else
			{
				this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
			}
			if (setTransferOptionToTrue)
			{
				this.TransferItemsOnSwitch = true;
			}
			if (updateSubs)
			{
				this.UpdateSubmarines();
			}
			if (this.pageIndicators != null)
			{
				for (int i = 0; i < this.pageIndicators.Length; i++)
				{
					this.pageIndicators[i].Color = ((i == this.currentPage - 1) ? Color.White : Color.Gray);
				}
			}
			int submarineIndex = (this.currentPage - 1) * 4;
			for (int j = 0; j < this.submarineDisplays.Length; j++)
			{
				SubmarineInfo subToDisplay = this.GetSubToDisplay(submarineIndex);
				if (subToDisplay == null)
				{
					this.submarineDisplays[j].submarineImage.Sprite = null;
					this.submarineDisplays[j].submarineName.Text = string.Empty;
					this.submarineDisplays[j].submarineFee.Text = string.Empty;
					this.submarineDisplays[j].submarineClass.Text = string.Empty;
					this.submarineDisplays[j].submarineTier.Text = string.Empty;
					this.submarineDisplays[j].selectSubmarineButton.Enabled = false;
					this.submarineDisplays[j].selectSubmarineButton.OnClicked = null;
					this.submarineDisplays[j].displayedSubmarine = null;
					this.submarineDisplays[j].middleTextBlock.AutoDraw = false;
					this.submarineDisplays[j].previewButton.Visible = false;
				}
				else
				{
					this.submarineDisplays[j].displayedSubmarine = subToDisplay;
					Sprite previewImage = this.GetPreviewImage(subToDisplay);
					if (previewImage != null)
					{
						this.submarineDisplays[j].submarineImage.Sprite = previewImage;
						this.submarineDisplays[j].middleTextBlock.AutoDraw = false;
					}
					else
					{
						this.submarineDisplays[j].submarineImage.Sprite = null;
						this.submarineDisplays[j].middleTextBlock.Text = this.missingPreviewText;
						this.submarineDisplays[j].middleTextBlock.AutoDraw = true;
					}
					this.submarineDisplays[j].selectSubmarineButton.Enabled = true;
					int index = j;
					this.submarineDisplays[j].selectSubmarineButton.OnClicked = delegate(GUIButton button, object userData)
					{
						this.SelectSubmarine(subToDisplay, this.submarineDisplays[index].background.Rect);
						return true;
					};
					this.submarineDisplays[j].submarineName.Text = subToDisplay.DisplayName;
					GUITextBlock submarineClass = this.submarineDisplays[j].submarineClass;
					string tag = "submarineclass.classsuffixformat";
					string varName = "[type]";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
					defaultInterpolatedStringHandler.AppendLiteral("submarineclass.");
					defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(subToDisplay.SubmarineClass);
					submarineClass.Text = TextManager.GetWithVariable(tag, varName, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), FormatCapitals.No);
					GUIComponent submarineClass2 = this.submarineDisplays[j].submarineClass;
					LocalizedString left = TextManager.Get("submarineclass.description") + "\n\n";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("submarineclass.");
					defaultInterpolatedStringHandler2.AppendFormatted<SubmarineClass>(subToDisplay.SubmarineClass);
					defaultInterpolatedStringHandler2.AppendLiteral(".description");
					submarineClass2.ToolTip = left + TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
					GUITextBlock submarineTier = this.submarineDisplays[j].submarineTier;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(14, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("submarinetier.");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(subToDisplay.Tier);
					submarineTier.Text = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
					this.submarineDisplays[j].submarineTier.ToolTip = TextManager.Get("submarinetier.description");
					if (!GameMain.GameSession.IsSubmarineOwned(subToDisplay))
					{
						LocalizedString amountString = TextManager.FormatCurrency(subToDisplay.GetPrice(null, null), true);
						this.submarineDisplays[j].submarineFee.Text = TextManager.GetWithVariable("price", "[amount]", amountString, FormatCapitals.No);
					}
					else if (subToDisplay.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name)
					{
						this.submarineDisplays[j].submarineFee.Text = string.Empty;
					}
					else
					{
						this.submarineDisplays[j].submarineFee.Text = this.selectedSubText;
					}
					if (this.transferService && subToDisplay.Name == SubmarineSelection.CurrentOrPendingSubmarine().Name && updateSubs)
					{
						if (this.selectedSubmarine == null)
						{
							CoroutineManager.StartCoroutine(this.SelectOwnSubmarineWithDelay(subToDisplay, this.submarineDisplays[j]), "");
						}
						else
						{
							this.SelectSubmarine(subToDisplay, this.submarineDisplays[j].background.Rect);
						}
					}
					else if ((!this.transferService && this.selectedSubmarine == null) || (!this.transferService && GameMain.GameSession.IsSubmarineOwned(this.selectedSubmarine)) || subToDisplay == this.selectedSubmarine)
					{
						this.SelectSubmarine(subToDisplay, this.submarineDisplays[j].background.Rect);
					}
					this.submarineDisplays[j].previewButton.Visible = true;
					this.submarineDisplays[j].previewButton.OnClicked = delegate(GUIButton btn, object obj)
					{
						SubmarinePreview.Create(subToDisplay);
						return false;
					};
				}
				submarineIndex++;
			}
			if (this.subsToShow.Count == 0)
			{
				this.SelectSubmarine(null, Rectangle.Empty);
				return;
			}
			this.UpdateItemTransferInfoFrame();
		}

		// Token: 0x060016BE RID: 5822 RVA: 0x000D83F8 File Offset: 0x000D65F8
		private void UpdateSubmarines()
		{
			this.subsToShow.Clear();
			if (this.transferService)
			{
				this.subsToShow.AddRange(GameMain.GameSession.OwnedSubmarines);
				List<SubmarineInfo> list = this.subsToShow;
				Comparison<SubmarineInfo> comparison;
				if ((comparison = SubmarineSelection.<>O.<0>__ComparePrice) == null)
				{
					comparison = (SubmarineSelection.<>O.<0>__ComparePrice = new Comparison<SubmarineInfo>(SubmarineSelection.<UpdateSubmarines>g__ComparePrice|51_0));
				}
				list.Sort(comparison);
				string currentSubName = SubmarineSelection.CurrentOrPendingSubmarine().Name;
				int currentIndex = this.subsToShow.FindIndex((SubmarineInfo s) => s.Name == currentSubName);
				if (currentIndex != -1)
				{
					this.currentPage = (int)Math.Ceiling((double)((float)(currentIndex + 1) / 4f));
				}
			}
			else
			{
				SubmarineSelection.<>c__DisplayClass51_1 CS$<>8__locals2 = new SubmarineSelection.<>c__DisplayClass51_1();
				List<SubmarineInfo> list2 = this.subsToShow;
				IEnumerable<SubmarineInfo> source;
				if (GameMain.Client != null)
				{
					IEnumerable<SubmarineInfo> campaignSubs = MultiPlayerCampaign.GetCampaignSubs();
					source = campaignSubs;
				}
				else
				{
					source = SubmarineInfo.SavedSubmarines;
				}
				list2.AddRange(from s in source
				where s.IsCampaignCompatible && !GameMain.GameSession.OwnedSubmarines.Any((SubmarineInfo os) => os.Name == s.Name)
				select s);
				SubmarineSelection.<>c__DisplayClass51_1 CS$<>8__locals3 = CS$<>8__locals2;
				CampaignMode campaign = GameMain.GameSession.Campaign;
				Location currentLocation;
				if (campaign == null)
				{
					currentLocation = null;
				}
				else
				{
					Map map = campaign.Map;
					currentLocation = ((map != null) ? map.CurrentLocation : null);
				}
				CS$<>8__locals3.currentLocation = currentLocation;
				if (CS$<>8__locals2.currentLocation != null)
				{
					this.subsToShow.RemoveAll((SubmarineInfo sub) => !CS$<>8__locals2.currentLocation.IsSubmarineAvailable(sub));
				}
				List<SubmarineInfo> list3 = this.subsToShow;
				Comparison<SubmarineInfo> comparison2;
				if ((comparison2 = SubmarineSelection.<>O.<0>__ComparePrice) == null)
				{
					comparison2 = (SubmarineSelection.<>O.<0>__ComparePrice = new Comparison<SubmarineInfo>(SubmarineSelection.<UpdateSubmarines>g__ComparePrice|51_0));
				}
				list3.Sort(comparison2);
			}
			if (this.transferService)
			{
				this.SetConfirmButtonState(this.selectedSubmarine != null && this.selectedSubmarine.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name);
			}
			this.pageCount = Math.Max(1, (int)Math.Ceiling((double)((float)this.subsToShow.Count / 4f)));
			this.UpdatePaging();
			SubmarineSelection.ContentRefreshRequired = false;
		}

		// Token: 0x060016BF RID: 5823 RVA: 0x000D85C3 File Offset: 0x000D67C3
		private SubmarineInfo GetSubToDisplay(int index)
		{
			if (this.subsToShow.Count <= index || index < 0)
			{
				return null;
			}
			return this.subsToShow[index];
		}

		// Token: 0x060016C0 RID: 5824 RVA: 0x000D85E8 File Offset: 0x000D67E8
		private Sprite GetPreviewImage(SubmarineInfo info)
		{
			Sprite preview = info.PreviewImage;
			if (preview == null)
			{
				SubmarineInfo potentialMatch = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.EqualityCheckVal == info.EqualityCheckVal);
				preview = ((potentialMatch != null) ? potentialMatch.PreviewImage : null);
				if (preview == null)
				{
					potentialMatch = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == info.Name);
					preview = ((potentialMatch != null) ? potentialMatch.PreviewImage : null);
				}
			}
			return preview;
		}

		// Token: 0x060016C1 RID: 5825 RVA: 0x000D865D File Offset: 0x000D685D
		private IEnumerable<CoroutineStatus> SelectOwnSubmarineWithDelay(SubmarineInfo info, SubmarineSelection.SubmarineDisplayContent display)
		{
			SubmarineSelection.<SelectOwnSubmarineWithDelay>d__54 <SelectOwnSubmarineWithDelay>d__ = new SubmarineSelection.<SelectOwnSubmarineWithDelay>d__54(-2);
			<SelectOwnSubmarineWithDelay>d__.<>4__this = this;
			<SelectOwnSubmarineWithDelay>d__.<>3__info = info;
			<SelectOwnSubmarineWithDelay>d__.<>3__display = display;
			return <SelectOwnSubmarineWithDelay>d__;
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x000D867C File Offset: 0x000D687C
		private void SelectSubmarine(int index, int direction)
		{
			SubmarineInfo nextSub = this.GetSubToDisplay(index + direction);
			if (nextSub == null)
			{
				return;
			}
			for (int i = 0; i < this.submarineDisplays.Length; i++)
			{
				if (this.submarineDisplays[i].displayedSubmarine == nextSub)
				{
					this.SelectSubmarine(nextSub, this.submarineDisplays[i].background.Rect);
					return;
				}
			}
			this.ChangePage(direction);
			for (int j = 0; j < this.submarineDisplays.Length; j++)
			{
				if (this.submarineDisplays[j].displayedSubmarine == nextSub)
				{
					this.SelectSubmarine(nextSub, this.submarineDisplays[j].background.Rect);
					return;
				}
			}
		}

		// Token: 0x060016C3 RID: 5827 RVA: 0x000D8728 File Offset: 0x000D6928
		private void SelectSubmarine(SubmarineInfo info, Rectangle backgroundRect)
		{
			if (this.selectedSubmarine == info)
			{
				return;
			}
			this.specsFrame.Content.ClearChildren();
			this.selectedSubmarine = info;
			if (info != null)
			{
				bool owned = GameMain.GameSession.IsSubmarineOwned(info);
				if (owned)
				{
					this.confirmButton.Text = this.switchText;
					this.confirmButton.OnClicked = delegate(GUIButton button, object userData)
					{
						this.ShowTransferPrompt();
						return true;
					};
				}
				else
				{
					this.confirmButton.Text = this.purchaseAndSwitchText;
					this.confirmButton.OnClicked = delegate(GUIButton button, object userData)
					{
						this.ShowBuyPrompt(false);
						return true;
					};
					this.confirmButtonAlt.Text = this.purchaseOnlyText;
					this.confirmButtonAlt.OnClicked = delegate(GUIButton button, object userData)
					{
						this.ShowBuyPrompt(true);
						return true;
					};
				}
				this.SetConfirmButtonState(this.selectedSubmarine.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name);
				this.selectedSubmarineIndicator.RectTransform.NonScaledSize = backgroundRect.Size;
				this.selectedSubmarineIndicator.RectTransform.AbsoluteOffset = new Point(backgroundRect.Left - this.submarineHorizontalGroup.Rect.Left, 0);
				Sprite previewImage = this.GetPreviewImage(info);
				this.listBackground.Sprite = previewImage;
				this.listBackground.SetCrop(true, true);
				GUIFont font = GUIStyle.Font;
				info.CreateSpecsWindow(this.specsFrame, font, true, true, false, true);
				this.descriptionTextBlock.Text = info.Description;
				this.descriptionTextBlock.CalculateHeightFromText(0, false);
			}
			else
			{
				this.listBackground.Sprite = null;
				this.listBackground.SetCrop(false, true);
				this.descriptionTextBlock.Text = string.Empty;
				this.selectedSubmarineIndicator.RectTransform.NonScaledSize = Point.Zero;
				this.SetConfirmButtonState(false);
			}
			this.UpdateItemTransferInfoFrame();
		}

		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x060016C4 RID: 5828 RVA: 0x000D88FA File Offset: 0x000D6AFA
		private bool IsSelectedSubCurrentSub
		{
			get
			{
				Submarine mainSub = Submarine.MainSub;
				string a;
				if (mainSub == null)
				{
					a = null;
				}
				else
				{
					SubmarineInfo info = mainSub.Info;
					a = ((info != null) ? info.Name : null);
				}
				SubmarineInfo submarineInfo = this.selectedSubmarine;
				return a == ((submarineInfo != null) ? submarineInfo.Name : null);
			}
		}

		// Token: 0x060016C5 RID: 5829 RVA: 0x000D8930 File Offset: 0x000D6B30
		private void UpdateItemTransferInfoFrame()
		{
			if (this.selectedSubmarine == null)
			{
				this.transferItemsTickBox.Visible = false;
				this.itemTransferInfoBlock.Visible = false;
				return;
			}
			if (this.IsSelectedSubCurrentSub)
			{
				this.TransferItemsOnSwitch = false;
				this.transferItemsTickBox.Visible = false;
				this.itemTransferInfoBlock.Visible = this.confirmButton.Enabled;
				this.itemTransferInfoBlock.Text = TextManager.Get("switchingbacktocurrentsub");
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			string a;
			if (gameSession == null)
			{
				a = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign == null)
				{
					a = null;
				}
				else
				{
					SubmarineInfo pendingSubmarineSwitch = campaign.PendingSubmarineSwitch;
					a = ((pendingSubmarineSwitch != null) ? pendingSubmarineSwitch.Name : null);
				}
			}
			if (a == this.selectedSubmarine.Name)
			{
				this.transferItemsTickBox.Visible = false;
				this.itemTransferInfoBlock.Visible = true;
				this.itemTransferInfoBlock.Text = (GameMain.GameSession.Campaign.TransferItemsOnSubSwitch ? TextManager.Get("itemtransferenabledreminder") : TextManager.Get("itemtransferdisabledreminder"));
				return;
			}
			this.transferItemsTickBox.Selected = this.TransferItemsOnSwitch;
			this.transferItemsTickBox.Visible = true;
			this.itemTransferInfoBlock.Visible = false;
		}

		// Token: 0x060016C6 RID: 5830 RVA: 0x000D8A61 File Offset: 0x000D6C61
		private void SetConfirmButtonState(bool state)
		{
			if (this.confirmButtonAlt != null)
			{
				this.confirmButtonAlt.Enabled = state;
			}
			if (this.confirmButton != null)
			{
				this.confirmButton.Enabled = state;
			}
		}

		// Token: 0x060016C7 RID: 5831 RVA: 0x000D8A8C File Offset: 0x000D6C8C
		public static SubmarineInfo CurrentOrPendingSubmarine()
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
				flag = (((campaign != null) ? campaign.PendingSubmarineSwitch : null) != null);
			}
			if (flag)
			{
				return GameMain.GameSession.Campaign.PendingSubmarineSwitch;
			}
			Submarine mainSub = Submarine.MainSub;
			if (mainSub == null)
			{
				return null;
			}
			return mainSub.Info;
		}

		// Token: 0x060016C8 RID: 5832 RVA: 0x000D8AD8 File Offset: 0x000D6CD8
		private void ChangePage(int pageChangeDirection)
		{
			this.SelectSubmarine(null, Rectangle.Empty);
			if (pageChangeDirection < 0 && this.currentPage > 1)
			{
				this.currentPage--;
			}
			if (pageChangeDirection > 0 && this.currentPage < this.pageCount)
			{
				this.currentPage++;
			}
			this.browseLeftButton.Enabled = (this.currentPage > 1);
			this.browseRightButton.Enabled = (this.currentPage < this.pageCount);
			this.RefreshSubmarineDisplay(false, false);
		}

		// Token: 0x060016C9 RID: 5833 RVA: 0x000D8B64 File Offset: 0x000D6D64
		private void ShowTransferPrompt()
		{
			LocalizedString text = TextManager.GetWithVariables("switchsubmarinetext", new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[submarinename1]", SubmarineSelection.CurrentOrPendingSubmarine().DisplayName),
				new ValueTuple<string, LocalizedString>("[submarinename2]", this.selectedSubmarine.DisplayName)
			});
			text += this.GetItemTransferText();
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("switchsubmarineheader"), text, this.messageBoxOptions, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = delegate(GUIButton applyButton, object obj)
			{
				if (!this.TransferItemsOnSwitch && !this.IsSelectedSubCurrentSub)
				{
					if (this.selectedSubmarine.NoItems)
					{
						return base.<ShowTransferPrompt>g__ShowConfirmationPopup|1(TextManager.Get("noitemsheader"), TextManager.Get("noitemswarning"));
					}
					if (!GameMain.GameSession.IsSubmarineOwned(this.selectedSubmarine) && !this.selectedSubmarine.IsManuallyOutfitted)
					{
						ValueTuple<LocalizedString, LocalizedString> itemTransferWarningText = this.GetItemTransferWarningText();
						LocalizedString header = itemTransferWarningText.Item1;
						LocalizedString body = itemTransferWarningText.Item2;
						return base.<ShowTransferPrompt>g__ShowConfirmationPopup|1(header, body);
					}
					if (this.selectedSubmarine.LowFuel)
					{
						return base.<ShowTransferPrompt>g__ShowConfirmationPopup|1(TextManager.Get("lowfuelheader"), TextManager.Get("lowfuelwarning"));
					}
				}
				return base.<ShowTransferPrompt>g__Confirm|2();
			};
			GUIButton guibutton = msgBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x000D8C9C File Offset: 0x000D6E9C
		[return: TupleElementNames(new string[]
		{
			"header",
			"body"
		})]
		private ValueTuple<LocalizedString, LocalizedString> GetItemTransferWarningText()
		{
			LocalizedString header = TextManager.Get("itemtransferheader").Fallback(TextManager.Get("lowfuelheader"), false);
			LocalizedString body = TextManager.Get("itemtransferwarning").Fallback(TextManager.Get("lowfuelwarning"), false);
			return new ValueTuple<LocalizedString, LocalizedString>(header, body);
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x000D8CE8 File Offset: 0x000D6EE8
		private void ShowBuyPrompt(bool purchaseOnly)
		{
			int price = this.selectedSubmarine.GetPrice(null, null);
			if (!GameMain.GameSession.Campaign.CanAfford(price, null))
			{
				new GUIMessageBox(TextManager.Get("purchasesubmarineheader"), TextManager.GetWithVariables("notenoughmoneyforpurchasetext", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[currencyname]", this.currencyName),
					new ValueTuple<string, LocalizedString>("[submarinename]", this.selectedSubmarine.DisplayName)
				}), null, null, GUIMessageBox.Type.Default);
				return;
			}
			GUIMessageBox msgBox;
			if (!purchaseOnly)
			{
				LocalizedString text = TextManager.GetWithVariables("purchaseandswitchsubmarinetext", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[submarinename1]", this.selectedSubmarine.DisplayName),
					new ValueTuple<string, LocalizedString>("[amount]", price.ToString()),
					new ValueTuple<string, LocalizedString>("[currencyname]", this.currencyName),
					new ValueTuple<string, LocalizedString>("[submarinename2]", SubmarineSelection.CurrentOrPendingSubmarine().DisplayName)
				});
				text += this.GetItemTransferText();
				msgBox = new GUIMessageBox(TextManager.Get("purchaseandswitchsubmarineheader"), text, this.messageBoxOptions, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton applyButton, object obj)
				{
					if (!this.TransferItemsOnSwitch && !this.IsSelectedSubCurrentSub)
					{
						if (this.selectedSubmarine.NoItems)
						{
							base.<ShowBuyPrompt>g__ShowConfirmationPopup|2(TextManager.Get("noitemsheader"), TextManager.Get("noitemswarning"));
							return false;
						}
						if (!GameMain.GameSession.IsSubmarineOwned(this.selectedSubmarine) && !this.selectedSubmarine.IsManuallyOutfitted)
						{
							ValueTuple<LocalizedString, LocalizedString> itemTransferWarningText = this.GetItemTransferWarningText();
							LocalizedString header = itemTransferWarningText.Item1;
							LocalizedString body = itemTransferWarningText.Item2;
							base.<ShowBuyPrompt>g__ShowConfirmationPopup|2(header, body);
							return false;
						}
						if (this.selectedSubmarine.LowFuel)
						{
							base.<ShowBuyPrompt>g__ShowConfirmationPopup|2(TextManager.Get("lowfuelheader"), TextManager.Get("lowfuelwarning"));
							return false;
						}
					}
					return base.<ShowBuyPrompt>g__Confirm|3();
				};
			}
			else
			{
				msgBox = new GUIMessageBox(TextManager.Get("purchasesubmarineheader"), TextManager.GetWithVariables("purchasesubmarinetext", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[submarinename]", this.selectedSubmarine.DisplayName),
					new ValueTuple<string, LocalizedString>("[amount]", price.ToString()),
					new ValueTuple<string, LocalizedString>("[currencyname]", this.currencyName)
				}) + '\n' + TextManager.Get("submarineswitchinstruction"), this.messageBoxOptions, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton applyButton, object obj)
				{
					if (GameMain.Client == null)
					{
						GameMain.GameSession.TryPurchaseSubmarine(this.selectedSubmarine, null);
						this.RefreshSubmarineDisplay(true, false);
					}
					else
					{
						GameMain.Client.InitiateSubmarineChange(this.selectedSubmarine, false, VoteType.PurchaseSub);
					}
					return true;
				};
			}
			msgBox.Buttons[0].ClickSound = GUISoundType.ConfirmTransaction;
			GUIButton guibutton = msgBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
		}

		// Token: 0x060016CC RID: 5836 RVA: 0x000D8FE8 File Offset: 0x000D71E8
		private LocalizedString GetItemTransferText()
		{
			Submarine mainSub = Submarine.MainSub;
			string a;
			if (mainSub == null)
			{
				a = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				a = ((info != null) ? info.Name : null);
			}
			if (a == this.selectedSubmarine.Name)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("\n\n");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("switchingbacktocurrentsub"));
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			LocalizedString s = "\n\n" + TextManager.Get(this.TransferItemsOnSwitch ? "itemswillbetransferred" : "itemswontbetransferred");
			LocalizedString left = s;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 1);
			defaultInterpolatedStringHandler2.AppendLiteral(" ");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("toggleitemtransferprompt"));
			return left + defaultInterpolatedStringHandler2.ToStringAndClear();
		}

		// Token: 0x060016D3 RID: 5843 RVA: 0x000D9128 File Offset: 0x000D7328
		[CompilerGenerated]
		internal static int <UpdateSubmarines>g__ComparePrice|51_0(SubmarineInfo x, SubmarineInfo y)
		{
			return x.Price.CompareTo(y.Price) * 100 + x.Name.CompareTo(y.Name);
		}

		// Token: 0x04000B67 RID: 2919
		private const int submarinesPerPage = 4;

		// Token: 0x04000B68 RID: 2920
		private int currentPage = 1;

		// Token: 0x04000B69 RID: 2921
		private int pageCount;

		// Token: 0x04000B6A RID: 2922
		private readonly bool transferService;

		// Token: 0x04000B6B RID: 2923
		private readonly bool purchaseService;

		// Token: 0x04000B6C RID: 2924
		private bool initialized;

		// Token: 0x04000B6D RID: 2925
		public GUIFrame GuiFrame;

		// Token: 0x04000B6E RID: 2926
		private GUIFrame pageIndicatorHolder;

		// Token: 0x04000B6F RID: 2927
		private GUICustomComponent selectedSubmarineIndicator;

		// Token: 0x04000B70 RID: 2928
		private GUILayoutGroup submarineHorizontalGroup;

		// Token: 0x04000B71 RID: 2929
		private GUILayoutGroup submarineControlsGroup;

		// Token: 0x04000B72 RID: 2930
		private GUIButton browseLeftButton;

		// Token: 0x04000B73 RID: 2931
		private GUIButton browseRightButton;

		// Token: 0x04000B74 RID: 2932
		private GUIButton confirmButton;

		// Token: 0x04000B75 RID: 2933
		private GUIButton confirmButtonAlt;

		// Token: 0x04000B76 RID: 2934
		private GUIListBox specsFrame;

		// Token: 0x04000B77 RID: 2935
		private GUIImage[] pageIndicators;

		// Token: 0x04000B78 RID: 2936
		private GUITextBlock descriptionTextBlock;

		// Token: 0x04000B79 RID: 2937
		private int selectionIndicatorThickness;

		// Token: 0x04000B7A RID: 2938
		private GUIImage listBackground;

		// Token: 0x04000B7B RID: 2939
		private GUITickBox transferItemsTickBox;

		// Token: 0x04000B7C RID: 2940
		private GUITextBlock itemTransferInfoBlock;

		// Token: 0x04000B7D RID: 2941
		private readonly List<SubmarineInfo> subsToShow;

		// Token: 0x04000B7E RID: 2942
		private readonly SubmarineSelection.SubmarineDisplayContent[] submarineDisplays = new SubmarineSelection.SubmarineDisplayContent[4];

		// Token: 0x04000B7F RID: 2943
		private SubmarineInfo selectedSubmarine;

		// Token: 0x04000B80 RID: 2944
		private LocalizedString purchaseAndSwitchText;

		// Token: 0x04000B81 RID: 2945
		private LocalizedString purchaseOnlyText;

		// Token: 0x04000B82 RID: 2946
		private LocalizedString selectedSubText;

		// Token: 0x04000B83 RID: 2947
		private LocalizedString switchText;

		// Token: 0x04000B84 RID: 2948
		private LocalizedString missingPreviewText;

		// Token: 0x04000B85 RID: 2949
		private LocalizedString currencyName;

		// Token: 0x04000B86 RID: 2950
		private readonly RectTransform parent;

		// Token: 0x04000B87 RID: 2951
		private readonly Action closeAction;

		// Token: 0x04000B88 RID: 2952
		private Sprite pageIndicator;

		// Token: 0x04000B89 RID: 2953
		private readonly LocalizedString[] messageBoxOptions;

		// Token: 0x04000B8A RID: 2954
		public static bool ContentRefreshRequired = false;

		// Token: 0x04000B8B RID: 2955
		private static readonly Color indicatorColor = new Color(112, 149, 129);

		// Token: 0x04000B8C RID: 2956
		private Point createdForResolution;

		// Token: 0x04000B8D RID: 2957
		private CampaignUI.PlayerBalanceElement? playerBalanceElement;

		// Token: 0x04000B8E RID: 2958
		private bool transferItemsOnSwitch = true;

		// Token: 0x020009E6 RID: 2534
		private struct SubmarineDisplayContent
		{
			// Token: 0x0400428E RID: 17038
			public GUIFrame background;

			// Token: 0x0400428F RID: 17039
			public GUIImage submarineImage;

			// Token: 0x04004290 RID: 17040
			public SubmarineInfo displayedSubmarine;

			// Token: 0x04004291 RID: 17041
			public GUITextBlock submarineName;

			// Token: 0x04004292 RID: 17042
			public GUITextBlock submarineClass;

			// Token: 0x04004293 RID: 17043
			public GUITextBlock submarineTier;

			// Token: 0x04004294 RID: 17044
			public GUITextBlock submarineFee;

			// Token: 0x04004295 RID: 17045
			public GUIButton selectSubmarineButton;

			// Token: 0x04004296 RID: 17046
			public GUITextBlock middleTextBlock;

			// Token: 0x04004297 RID: 17047
			public GUIButton previewButton;
		}

		// Token: 0x020009E7 RID: 2535
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004298 RID: 17048
			public static Comparison<SubmarineInfo> <0>__ComparePrice;
		}
	}
}
