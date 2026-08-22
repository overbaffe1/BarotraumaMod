using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BC RID: 1468
	internal class Fabricator : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001731 RID: 5937
		// (get) Token: 0x06005BEE RID: 23534 RVA: 0x002F100A File Offset: 0x002EF20A
		public GUIButton ActivateButton
		{
			get
			{
				return this.activateButton;
			}
		}

		// Token: 0x17001732 RID: 5938
		// (get) Token: 0x06005BEF RID: 23535 RVA: 0x002F1012 File Offset: 0x002EF212
		public FabricationRecipe SelectedItem
		{
			get
			{
				return this.selectedItem;
			}
		}

		// Token: 0x17001733 RID: 5939
		// (get) Token: 0x06005BF0 RID: 23536 RVA: 0x002F101A File Offset: 0x002EF21A
		public Identifier SelectedItemIdentifier
		{
			get
			{
				FabricationRecipe fabricationRecipe = this.SelectedItem;
				if (fabricationRecipe == null)
				{
					return Identifier.Empty;
				}
				return fabricationRecipe.TargetItem.Identifier;
			}
		}

		// Token: 0x17001734 RID: 5940
		// (get) Token: 0x06005BF1 RID: 23537 RVA: 0x002F1036 File Offset: 0x002EF236
		// (set) Token: 0x06005BF2 RID: 23538 RVA: 0x002F103E File Offset: 0x002EF23E
		[Serialize("FabricatorCreate", IsPropertySaveable.Yes, "", "", false)]
		public string CreateButtonText { get; set; }

		// Token: 0x17001735 RID: 5941
		// (get) Token: 0x06005BF3 RID: 23539 RVA: 0x002F1047 File Offset: 0x002EF247
		// (set) Token: 0x06005BF4 RID: 23540 RVA: 0x002F104F File Offset: 0x002EF24F
		[Serialize("vendingmachine.outofstock", IsPropertySaveable.Yes, "", "", false)]
		public string FabricationLimitReachedText { get; set; }

		// Token: 0x17001736 RID: 5942
		// (get) Token: 0x06005BF5 RID: 23541 RVA: 0x002F1058 File Offset: 0x002EF258
		// (set) Token: 0x06005BF6 RID: 23542 RVA: 0x002F1060 File Offset: 0x002EF260
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowSortByDropdown { get; set; }

		// Token: 0x17001737 RID: 5943
		// (get) Token: 0x06005BF7 RID: 23543 RVA: 0x002F1069 File Offset: 0x002EF269
		// (set) Token: 0x06005BF8 RID: 23544 RVA: 0x002F1071 File Offset: 0x002EF271
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowAvailableOnlyTickBox { get; set; }

		// Token: 0x17001738 RID: 5944
		// (get) Token: 0x06005BF9 RID: 23545 RVA: 0x002F107A File Offset: 0x002EF27A
		// (set) Token: 0x06005BFA RID: 23546 RVA: 0x002F1082 File Offset: 0x002EF282
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowCategoryButtons { get; set; }

		// Token: 0x17001739 RID: 5945
		// (get) Token: 0x06005BFB RID: 23547 RVA: 0x002F108B File Offset: 0x002EF28B
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005BFC RID: 23548 RVA: 0x002F108E File Offset: 0x002EF28E
		protected override void OnResolutionChanged()
		{
			if (base.GuiFrame != null)
			{
				this.InitInventoryUIs();
			}
		}

		// Token: 0x06005BFD RID: 23549 RVA: 0x002F10A0 File Offset: 0x002EF2A0
		protected override void CreateGUI()
		{
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.05f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = this.item.Prefab.Name;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text6, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock.TextAlignment = Alignment.Center;
			guitextBlock.AutoScaleVertical = true;
			GUILayoutGroup innerArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.95f), paddedFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true,
				CanBeFocused = true
			};
			List<MapEntityCategory> itemCategories = Enum.GetValues<MapEntityCategory>().ToList<MapEntityCategory>();
			itemCategories.Remove(MapEntityCategory.None);
			itemCategories.RemoveAll((MapEntityCategory c) => this.fabricationRecipes.None(delegate(KeyValuePair<uint, FabricationRecipe> f)
			{
				FabricationRecipe value = f.Value;
				ItemPrefab ti = (value != null) ? value.TargetItem : null;
				return ti != null && ti.Category.HasFlag(c);
			}));
			this.itemCategoryButtons.Clear();
			if (this.ShowCategoryButtons && itemCategories.Count > 2)
			{
				GUILayoutGroup categoryButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.05f, 1f), innerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					RelativeSpacing = 0.01f
				};
				int buttonSize = Math.Min(categoryButtonContainer.Rect.Width, categoryButtonContainer.Rect.Height / itemCategories.Count);
				GUIButton categoryButton = new GUIButton(new RectTransform(new Point(buttonSize), categoryButtonContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CategoryButton.All", null)
				{
					ToolTip = TextManager.Get("MapEntityCategory.All"),
					OnClicked = new GUIButton.OnClickedHandler(this.<CreateGUI>g__OnClickedCategoryButton|55_3)
				};
				this.itemCategoryButtons.Add(categoryButton);
				foreach (MapEntityCategory category in itemCategories)
				{
					categoryButton = new GUIButton(new RectTransform(new Point(buttonSize), categoryButtonContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CategoryButton." + category.ToString(), null)
					{
						ToolTip = TextManager.Get("MapEntityCategory." + category.ToString()),
						UserData = category,
						OnClicked = new GUIButton.OnClickedHandler(this.<CreateGUI>g__OnClickedCategoryButton|55_3)
					};
					this.itemCategoryButtons.Add(categoryButton);
				}
				using (List<GUIButton>.Enumerator enumerator2 = this.itemCategoryButtons.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GUIButton btn = enumerator2.Current;
						btn.RectTransform.SizeChanged += delegate()
						{
							List<UISprite> spriteList;
							if (btn.Frame.sprites == null || !btn.Frame.sprites.TryGetValue(GUIComponent.ComponentState.None, out spriteList))
							{
								return;
							}
							UISprite sprite = (spriteList != null) ? spriteList.First<UISprite>() : null;
							if (sprite == null)
							{
								return;
							}
							btn.RectTransform.NonScaledSize = new Point(btn.Rect.Width, (int)((float)btn.Rect.Width * ((float)sprite.Sprite.SourceRect.Height / (float)sprite.Sprite.SourceRect.Width)));
						};
					}
				}
			}
			GUILayoutGroup mainFrame = new GUILayoutGroup(new RectTransform(Vector2.One, innerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				RelativeSpacing = 0.02f,
				Stretch = true,
				CanBeFocused = true
			};
			GUIFrame topFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.8f), mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrameDark", null);
			GUILayoutGroup itemListFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), topFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUILayoutGroup paddedItemFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), itemListFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup filterArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), paddedItemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.03f,
				UserData = "filterarea"
			};
			RectTransform rectT2 = new RectTransform(new Vector2(0.4f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("serverlog.filter");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.CenterLeft, false, "", null);
			guitextBlock2.Padding = Vector4.Zero;
			guitextBlock2.AutoScaleVertical = true;
			this.itemFilterBox = new GUITextBox(new RectTransform(new Vector2(0.8f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, true, true)
			{
				OverflowClip = true
			};
			this.itemFilterBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.FilterEntities(this.selectedItemCategory, text);
				return true;
			};
			filterArea.RectTransform.MinSize = new Point(0, this.itemFilterBox.Rect.Height);
			filterArea.RectTransform.MaxSize = new Point(int.MaxValue, this.itemFilterBox.Rect.Height);
			GUILayoutGroup sortByArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), paddedItemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.03f,
				Visible = this.ShowSortByDropdown,
				IgnoreLayoutGroups = !this.ShowSortByDropdown
			};
			RectTransform rectT3 = new RectTransform(new Vector2(0.4f, 1f), sortByArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("campaignstore.sortby");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock3 = new GUITextBlock(rectT3, text3, null, subHeadingFont, Alignment.CenterLeft, false, "", null);
			guitextBlock3.Padding = Vector4.Zero;
			guitextBlock3.AutoScaleVertical = true;
			this.sortByDropdown = new GUIDropDown(new RectTransform(new Vector2(0.8f, 1f), sortByArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			foreach (Fabricator.SortBy sortBy in Enum.GetValues<Fabricator.SortBy>())
			{
				this.sortByDropdown.AddItem(TextManager.Get("fabricator.sortby." + sortBy.ToString()), sortBy, null, null, null);
			}
			this.sortByDropdown.Select(0);
			GUIDropDown guidropDown = this.sortByDropdown;
			guidropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object userdata)
			{
				this.FilterEntities(this.selectedItemCategory, this.itemFilterBox.Text);
				this.SortItems(Character.Controlled);
				return true;
			}));
			sortByArea.RectTransform.MinSize = new Point(0, this.sortByDropdown.Rect.Height);
			sortByArea.RectTransform.MaxSize = new Point(int.MaxValue, this.sortByDropdown.Rect.Height);
			GUILayoutGroup availableOnlyTickBoxArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), paddedItemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				Visible = this.ShowAvailableOnlyTickBox,
				IgnoreLayoutGroups = !this.ShowAvailableOnlyTickBox
			};
			RectTransform rectT4 = new RectTransform(new Vector2(0.4f, 1f), availableOnlyTickBoxArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("fabricator.onlyshowavailable");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock4 = new GUITextBlock(rectT4, text4, null, subHeadingFont, Alignment.CenterLeft, false, "", null);
			guitextBlock4.Padding = Vector4.Zero;
			guitextBlock4.AutoScaleVertical = true;
			this.availableOnlyTickBox = new GUITickBox(new RectTransform(new Vector2(1f), availableOnlyTickBoxArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), string.Empty, null, "")
			{
				ToolTip = TextManager.Get("fabricator.onlyshowavailable.tooltip")
			};
			GUITickBox guitickBox = this.availableOnlyTickBox;
			guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tickbox)
			{
				this.FilterEntities(this.selectedItemCategory, this.itemFilterBox.Text);
				return true;
			}));
			this.availableOnlyTickBox.RectTransform.MinSize = new Point(this.availableOnlyTickBox.Rect.Height);
			this.availableOnlyTickBox.RectTransform.IsFixedSize = true;
			availableOnlyTickBoxArea.RectTransform.MinSize = new Point(0, this.availableOnlyTickBox.Rect.Height);
			availableOnlyTickBoxArea.RectTransform.MaxSize = new Point(int.MaxValue, this.availableOnlyTickBox.Rect.Height);
			this.itemList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), paddedItemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent component, object userdata)
				{
					FabricationRecipe fabricationRecipe = userdata as FabricationRecipe;
					if (fabricationRecipe != null)
					{
						this.selectedItem = fabricationRecipe;
						this.SelectItem(Character.Controlled, this.selectedItem, null);
						return true;
					}
					return false;
				}
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), topFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUILayoutGroup outputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), topFrame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			this.paddedOutputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), outputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.outputTopArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), this.paddedOutputArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.outputSlot = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.4f), this.outputTopArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth), null, null);
			this.outputInventoryHolder = new GUIFrame(new RectTransform(new Vector2(1f, 1f), this.outputSlot.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			new GUICustomComponent(new RectTransform(Vector2.One, this.outputInventoryHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawOutputOverLay), null).CanBeFocused = false;
			this.selectedItemFrame = new GUIFrame(new RectTransform(new Vector2(0.6f, 1f), this.outputTopArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.selectedItemReqsFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.5f), this.paddedOutputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame bottomFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (this.inputContainer.Capacity > 0)
			{
				GUILayoutGroup separatorArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.15f), bottomFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.03f
				};
				RectTransform rectT5 = new RectTransform(Vector2.One, separatorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = TextManager.Get(new string[]
				{
					"fabricator.input",
					"uilabel.input"
				});
				subHeadingFont = GUIStyle.SubHeadingFont;
				GUITextBlock inputLabel = new GUITextBlock(rectT5, text5, null, subHeadingFont, Alignment.Left, false, "", null)
				{
					Padding = Vector4.Zero
				};
				inputLabel.RectTransform.Resize(new Point((int)inputLabel.Font.MeasureString(inputLabel.Text, false).X, inputLabel.RectTransform.Rect.Height), true);
				new GUIFrame(new RectTransform(Vector2.One, separatorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
				GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 1f), bottomFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft);
				this.inputInventoryHolder = new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				new GUICustomComponent(new RectTransform(Vector2.One, this.inputInventoryHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawInputOverLay), null).CanBeFocused = false;
				GUILayoutGroup buttonFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 0.9f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUILayoutGroup amountInputHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.4f), buttonFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true
				};
				new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), amountInputHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "1", null, null, Alignment.Center, false, "", null);
				this.amountInput = new GUIScrollBar(new RectTransform(new Vector2(0.7f, 1f), amountInputHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null)
				{
					OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
					{
						scrollBar.Step = 1f / Math.Max(scrollBar.Range.Y - 1f, 1f);
						this.AmountToFabricate = (int)MathF.Round(scrollBar.BarScrollValue);
						this.RefreshActivateButtonText();
						if (GameMain.Client != null)
						{
							this.pendingFabricatedItem = null;
							this.item.CreateClientEvent<Fabricator>(this);
						}
						return true;
					}
				};
				this.amountTextMax = new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), amountInputHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "1", null, null, Alignment.Center, false, "", null);
				this.activateButton = new GUIButton(new RectTransform(new Vector2(1f, 0.6f), buttonFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(this.CreateButtonText), Alignment.Center, "DeviceButton", null)
				{
					OnClicked = new GUIButton.OnClickedHandler(this.StartButtonClicked),
					UserData = this.selectedItem,
					Enabled = false
				};
				new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), buttonFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			}
			else
			{
				bottomFrame.RectTransform.RelativeSize = new Vector2(1f, 0.1f);
				this.activateButton = new GUIButton(new RectTransform(new Vector2(0.3f, 1f), bottomFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get(this.CreateButtonText), Alignment.Center, "DeviceButtonFixedSize", null)
				{
					OnClicked = new GUIButton.OnClickedHandler(this.StartButtonClicked),
					UserData = this.selectedItem,
					Enabled = false
				};
			}
			this.inSufficientPowerWarning = new GUITextBlock(new RectTransform(Vector2.One, this.activateButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("FabricatorNoPower"), new Color?(GUIStyle.Orange), null, Alignment.Center, true, "OuterGlow", new Color?(Color.Black))
			{
				HoverColor = Color.Black,
				IgnoreLayoutGroups = true,
				Visible = false,
				CanBeFocused = false
			};
			this.CreateRecipes();
			foreach (MapEntityCategory category2 in itemCategories)
			{
				GUITextBlock guitextBlock5 = new GUITextBlock(new RectTransform(new Vector2(1f, 0.15f), this.itemList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MapEntityCategory." + category2.ToString()), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null);
				guitextBlock5.CanBeFocused = false;
				guitextBlock5.UserData = category2;
				guitextBlock5.Visible = false;
			}
			this.requiresRecipeText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.15f), this.itemList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("fabricatorrequiresrecipe"), new Color?(Color.Red), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null)
			{
				AutoScaleHorizontal = true,
				CanBeFocused = false
			};
			this.nothingToShowText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.8f), this.itemList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("noitemsheader"), new Color?(GUIStyle.TextColorDim), null, Alignment.Center, false, "", null)
			{
				CanBeFocused = false,
				Visible = false
			};
			this.SortItems(Character.Controlled);
		}

		// Token: 0x06005BFE RID: 23550 RVA: 0x002F2730 File Offset: 0x002F0930
		private void RefreshActivateButtonText()
		{
			if (this.amountInput == null)
			{
				this.activateButton.Text = TextManager.Get(this.IsActive ? "FabricatorCancel" : this.CreateButtonText);
				return;
			}
			GUIButton guibutton = this.activateButton;
			string value;
			if (!this.IsActive)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get(this.CreateButtonText));
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.AmountToFabricate);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				value = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("FabricatorCancel"));
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.amountRemaining);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				value = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			guibutton.Text = value;
		}

		// Token: 0x06005BFF RID: 23551 RVA: 0x002F2814 File Offset: 0x002F0A14
		private void SetRecipeTooltip(GUIComponent component, FabricationRecipe recipe)
		{
			if (!recipe.RequiresRecipe)
			{
				component.ToolTip = RichString.Rich(recipe.TargetItem.Description, null);
				return;
			}
			RichString toolTip;
			if (!Fabricator.AnyOneHasRecipeForItem(Character.Controlled, recipe.TargetItem))
			{
				LocalizedString left = recipe.TargetItem.Description + "\n\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.Red.ToStringHex());
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("fabricatorrequiresrecipe"));
				defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
				toolTip = RichString.Rich(left + defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			else
			{
				LocalizedString left2 = recipe.TargetItem.Description + "\n\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler2.AppendFormatted(GUIStyle.Green.ToStringHex());
				defaultInterpolatedStringHandler2.AppendLiteral("‖");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("unlockedrecipe.true"));
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
				toolTip = RichString.Rich(left2 + defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			}
			component.ToolTip = toolTip;
		}

		// Token: 0x06005C00 RID: 23552 RVA: 0x002F2970 File Offset: 0x002F0B70
		private void InitInventoryUIs()
		{
			if (this.inputInventoryHolder != null)
			{
				this.inputContainer.AllowUIOverlap = true;
				this.inputContainer.Inventory.DrawWhenEquipped = true;
				this.inputContainer.Inventory.RectTransform = this.inputInventoryHolder.RectTransform;
			}
			this.outputContainer.AllowUIOverlap = true;
			this.outputContainer.Inventory.DrawWhenEquipped = true;
			this.outputContainer.Inventory.RectTransform = this.outputInventoryHolder.RectTransform;
		}

		// Token: 0x06005C01 RID: 23553 RVA: 0x002F29F8 File Offset: 0x002F0BF8
		private static RichString GetRecipeNameAndAmount(FabricationRecipe fabricationRecipe)
		{
			if (fabricationRecipe == null)
			{
				return "";
			}
			if (fabricationRecipe.Amount > 1)
			{
				return TextManager.GetWithVariables("fabricationrecipenamewithamount", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[name]", RichString.Rich(fabricationRecipe.DisplayName, null)),
					new ValueTuple<string, LocalizedString>("[amount]", fabricationRecipe.Amount.ToString())
				});
			}
			return RichString.Rich(fabricationRecipe.DisplayName, null);
		}

		// Token: 0x06005C02 RID: 23554 RVA: 0x002F2A84 File Offset: 0x002F0C84
		private void SortItems(Character character)
		{
			Fabricator.SortBy sortBy = (Fabricator.SortBy)this.sortByDropdown.SelectedData;
			this.itemList.Content.RectTransform.SortChildren(delegate(RectTransform c1, RectTransform c2)
			{
				FabricationRecipe item = c1.GUIComponent.UserData as FabricationRecipe;
				FabricationRecipe item2 = c2.GUIComponent.UserData as FabricationRecipe;
				if (item == null && item2 == null)
				{
					return 0;
				}
				if (item == null)
				{
					return -1;
				}
				if (item2 == null)
				{
					return 1;
				}
				bool missingRecipe = this.MissingRequiredRecipe(item, character);
				bool missingRecipe2 = this.MissingRequiredRecipe(item2, character);
				if (missingRecipe != missingRecipe2)
				{
					return missingRecipe.CompareTo(missingRecipe2);
				}
				switch (sortBy)
				{
				case Fabricator.SortBy.Category:
				{
					MapEntityCategory category = EnumExtensions.GetIndividualFlags<MapEntityCategory>(item.TargetItem.Category).FirstOrDefault<MapEntityCategory>();
					MapEntityCategory category2 = EnumExtensions.GetIndividualFlags<MapEntityCategory>(item2.TargetItem.Category).FirstOrDefault<MapEntityCategory>();
					if (category == category2)
					{
						return string.Compare(item.DisplayName.Value, item2.DisplayName.Value);
					}
					return category.CompareTo(category2);
				}
				case Fabricator.SortBy.Alphabetical:
					return string.Compare(item.DisplayName.Value, item2.DisplayName.Value);
				case Fabricator.SortBy.SkillRequirement:
				{
					float skillRequirement = item.RequiredSkills.Sum((Skill skill) => skill.Level);
					float skillRequirement2 = item2.RequiredSkills.Sum((Skill skill) => skill.Level);
					if (MathUtils.NearlyEqual(skillRequirement, skillRequirement2, 0.0001f))
					{
						return string.Compare(item.DisplayName.Value, item2.DisplayName.Value);
					}
					return skillRequirement.CompareTo(skillRequirement2);
				}
				case Fabricator.SortBy.Price:
				{
					PriceInfo defaultPrice = item.TargetItem.DefaultPrice;
					float itemValue = (float)((defaultPrice != null) ? defaultPrice.Price : 0);
					PriceInfo defaultPrice2 = item2.TargetItem.DefaultPrice;
					float itemValue2 = (float)((defaultPrice2 != null) ? defaultPrice2.Price : 0);
					if (MathUtils.NearlyEqual(itemValue, itemValue2, 0.0001f))
					{
						return string.Compare(item.DisplayName.Value, item2.DisplayName.Value);
					}
					return itemValue2.CompareTo(itemValue);
				}
				default:
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Sorting by ");
					defaultInterpolatedStringHandler.AppendFormatted<Fabricator.SortBy>(sortBy);
					defaultInterpolatedStringHandler.AppendLiteral(" has not been implemented.");
					throw new NotImplementedException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				}
			});
			if (sortBy == Fabricator.SortBy.Category)
			{
				foreach (GUIComponent categoryText in this.itemList.Content.Children.Where(delegate(GUIComponent c)
				{
					object userData = c.UserData;
					return ((userData != null) ? userData.GetType() : null) == typeof(MapEntityCategory);
				}).ToList<GUIComponent>())
				{
					categoryText.RectTransform.SetAsLastChild();
					MapEntityCategory category = (MapEntityCategory)categoryText.UserData;
					GUIComponent firstChildWithMatchingCategory = this.itemList.Content.Children.FirstOrDefault(delegate(GUIComponent c)
					{
						FabricationRecipe recipe = c.UserData as FabricationRecipe;
						return recipe != null && EnumExtensions.GetIndividualFlags<MapEntityCategory>(recipe.TargetItem.Category).FirstOrDefault<MapEntityCategory>() == category;
					});
					if (firstChildWithMatchingCategory != null)
					{
						categoryText.RectTransform.RepositionChildInHierarchy(this.itemList.Content.GetChildIndex(firstChildWithMatchingCategory));
						categoryText.Visible = true;
					}
					else
					{
						categoryText.Visible = false;
					}
				}
			}
			this.requiresRecipeText.RectTransform.SetAsLastChild();
			GUIComponent firstMissingRecipe = this.itemList.Content.Children.FirstOrDefault(delegate(GUIComponent c)
			{
				FabricationRecipe recipe = c.UserData as FabricationRecipe;
				return recipe != null && this.MissingRequiredRecipe(recipe, character);
			});
			if (firstMissingRecipe != null)
			{
				this.requiresRecipeText.RectTransform.RepositionChildInHierarchy(this.itemList.Content.GetChildIndex(firstMissingRecipe));
				this.requiresRecipeText.Visible = true;
			}
			else
			{
				this.requiresRecipeText.Visible = false;
			}
			this.HideEmptyItemListCategories();
		}

		// Token: 0x06005C03 RID: 23555 RVA: 0x002F2C50 File Offset: 0x002F0E50
		private void DrawInputOverLay(SpriteBatch spriteBatch, GUICustomComponent overlayComponent)
		{
			Fabricator.<>c__DisplayClass63_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			overlayComponent.RectTransform.SetAsLastChild();
			this.missingIngredientCounts.Clear();
			FabricationRecipe targetItem = this.fabricatedItem ?? this.selectedItem;
			if (targetItem != null)
			{
				foreach (FabricationRecipe.RequiredItem requiredItem in targetItem.RequiredItems)
				{
					if (this.missingIngredientCounts.ContainsKey(requiredItem))
					{
						Dictionary<FabricationRecipe.RequiredItem, int> dictionary = this.missingIngredientCounts;
						FabricationRecipe.RequiredItem key = requiredItem;
						dictionary[key] += requiredItem.Amount;
					}
					else
					{
						this.missingIngredientCounts[requiredItem] = requiredItem.Amount;
					}
				}
				using (IEnumerator<Item> enumerator2 = this.inputContainer.Inventory.AllItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item item = enumerator2.Current;
						FabricationRecipe.RequiredItem missingIngredient = this.missingIngredientCounts.Keys.FirstOrDefault((FabricationRecipe.RequiredItem mi) => mi.MatchesItem(item));
						if (missingIngredient != null)
						{
							if (this.missingIngredientCounts[missingIngredient] == 1)
							{
								this.missingIngredientCounts.Remove(missingIngredient);
							}
							else
							{
								Dictionary<FabricationRecipe.RequiredItem, int> dictionary2 = this.missingIngredientCounts;
								FabricationRecipe.RequiredItem key = missingIngredient;
								int num = dictionary2[key];
								dictionary2[key] = num - 1;
							}
						}
					}
				}
				if (this.ingredientHighlightTimer <= 0f)
				{
					foreach (Inventory inventory in this.linkedInventories)
					{
						if (inventory.visualSlots != null)
						{
							for (int k = 0; k < inventory.Capacity; k++)
							{
								if (inventory.visualSlots[k].HighlightTimer <= 0f)
								{
									Item availableItem = inventory.GetItemAt(k);
									if (availableItem != null)
									{
										if (this.missingIngredientCounts.Keys.Any((FabricationRecipe.RequiredItem it) => it.MatchesItem(availableItem)))
										{
											inventory.visualSlots[k].ShowBorderHighlight(GUIStyle.Green, 0.5f, 0.5f, 0.2f);
										}
										else if (availableItem.OwnInventory != null)
										{
											for (int j = 0; j < availableItem.OwnInventory.Capacity; j++)
											{
												Item availableContainedItem = availableItem.OwnInventory.GetItemAt(k);
												if (availableContainedItem != null && this.missingIngredientCounts.Keys.Any((FabricationRecipe.RequiredItem it) => it.MatchesItem(availableContainedItem)))
												{
													inventory.visualSlots[k].ShowBorderHighlight(GUIStyle.Green, 0.5f, 0.5f, 0.2f);
													break;
												}
											}
										}
									}
								}
							}
						}
					}
					this.ingredientHighlightTimer = 1f;
				}
				int slotIndex = 0;
				foreach (KeyValuePair<FabricationRecipe.RequiredItem, int> kvp in this.missingIngredientCounts)
				{
					Fabricator.<>c__DisplayClass63_4 CS$<>8__locals5 = new Fabricator.<>c__DisplayClass63_4();
					ItemInventory inventory2 = this.inputContainer.Inventory;
					if (((inventory2 != null) ? inventory2.visualSlots : null) == null)
					{
						break;
					}
					CS$<>8__locals5.requiredItem = kvp.Key;
					int missingCount = kvp.Value;
					while (slotIndex < this.inputContainer.Capacity && this.inputContainer.Inventory.GetItemAt(slotIndex) != null)
					{
						slotIndex++;
					}
					if (slotIndex >= this.inputContainer.Capacity)
					{
						break;
					}
					if (slotIndex < this.inputContainer.Capacity && this.inputContainer.Inventory.visualSlots[slotIndex].HighlightTimer <= 0f && this.availableIngredients.Any((KeyValuePair<Identifier, List<Item>> i) => i.Value.Any<Item>() && CS$<>8__locals5.requiredItem.MatchesItem(i.Value.First<Item>())))
					{
						this.inputContainer.Inventory.visualSlots[slotIndex].ShowBorderHighlight(GUIStyle.Green, 0.5f, 0.5f, 0.2f);
					}
					CS$<>8__locals5.slotRect = this.inputContainer.Inventory.visualSlots[slotIndex].Rect;
					ItemPrefab requiredItemPrefab = CS$<>8__locals5.requiredItem.FirstMatchingPrefab;
					ItemPrefab requiredItemToDisplay = CS$<>8__locals5.requiredItem.DefaultItem.IsEmpty ? null : CS$<>8__locals5.requiredItem.ItemPrefabs.FirstOrDefault((ItemPrefab p) => p.Identifier == CS$<>8__locals5.requiredItem.DefaultItem);
					float iconAlpha;
					if (requiredItemToDisplay == null && CS$<>8__locals5.requiredItem.ItemPrefabs.Multiple(null))
					{
						float iconCycleSpeed = 0.75f;
						float iconCycleT = (float)Timing.TotalTime * iconCycleSpeed;
						int iconIndex = (int)(iconCycleT % (float)CS$<>8__locals5.requiredItem.ItemPrefabs.Count<ItemPrefab>());
						requiredItemToDisplay = CS$<>8__locals5.requiredItem.ItemPrefabs.Skip(iconIndex).FirstOrDefault<ItemPrefab>();
						iconAlpha = Math.Min(Math.Abs(MathF.Sin(iconCycleT * 3.1415927f)) * 2f, 1f);
					}
					else
					{
						if (requiredItemToDisplay == null)
						{
							requiredItemToDisplay = CS$<>8__locals5.requiredItem.ItemPrefabs.FirstOrDefault<ItemPrefab>();
						}
						iconAlpha = 1f;
					}
					if (iconAlpha > 0f)
					{
						Sprite itemIcon = requiredItemToDisplay.InventoryIcon ?? requiredItemToDisplay.Sprite;
						itemIcon.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals5.slotRect.Center.ToVector2(), requiredItemToDisplay.InventoryIconColor * 0.3f * iconAlpha, 0f, Math.Min((float)CS$<>8__locals5.slotRect.Width * 0.9f / itemIcon.size.X, (float)CS$<>8__locals5.slotRect.Height * 0.9f / itemIcon.size.Y), SpriteEffects.None, null);
					}
					if (missingCount > 1)
					{
						Vector2 stackCountPos = new Vector2((float)CS$<>8__locals5.slotRect.Right, (float)CS$<>8__locals5.slotRect.Bottom);
						string stackCountText = "x" + missingCount.ToString();
						stackCountPos -= GUIStyle.SmallFont.MeasureString(stackCountText, false) + new Vector2(4f, 2f);
						GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, stackCountText, stackCountPos + Vector2.One, Color.Black, ForceUpperCase.Inherit, false);
						GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, stackCountText, stackCountPos, Color.White, ForceUpperCase.Inherit, false);
					}
					if (CS$<>8__locals5.requiredItem.UseCondition && CS$<>8__locals5.requiredItem.MinCondition < 1f)
					{
						CS$<>8__locals5.<DrawInputOverLay>g__DrawConditionBar|5(CS$<>8__locals1.spriteBatch, CS$<>8__locals5.requiredItem.MinCondition, ref CS$<>8__locals1);
					}
					else if (CS$<>8__locals5.requiredItem.MaxCondition < 1f)
					{
						CS$<>8__locals5.<DrawInputOverLay>g__DrawConditionBar|5(CS$<>8__locals1.spriteBatch, CS$<>8__locals5.requiredItem.MaxCondition, ref CS$<>8__locals1);
					}
					if (CS$<>8__locals5.slotRect.Contains(PlayerInput.MousePosition))
					{
						LocalizedString toolTipText = CS$<>8__locals5.requiredItem.OverrideHeader;
						if (CS$<>8__locals5.requiredItem.OverrideHeader.IsNullOrEmpty())
						{
							IEnumerable<LocalizedString> suitableIngredients = (from ip in (from ip in CS$<>8__locals5.requiredItem.ItemPrefabs
							where !ip.HideInMenus
							select ip).OrderBy(delegate(ItemPrefab ip)
							{
								PriceInfo defaultPrice = ip.DefaultPrice;
								if (defaultPrice == null)
								{
									return 0;
								}
								return defaultPrice.Price;
							})
							select ip.Name).Distinct<LocalizedString>();
							toolTipText = this.GetSuitableIngredientText(suitableIngredients);
						}
						if (CS$<>8__locals5.requiredItem.UseCondition && CS$<>8__locals5.requiredItem.MinCondition < 1f)
						{
							toolTipText += " " + ((int)Math.Round((double)(CS$<>8__locals5.requiredItem.MinCondition * 100f))).ToString() + "%";
						}
						else if (CS$<>8__locals5.requiredItem.MaxCondition < 1f)
						{
							if (CS$<>8__locals5.requiredItem.MaxCondition <= 0f)
							{
								toolTipText += " " + ((int)Math.Round((double)(CS$<>8__locals5.requiredItem.MaxCondition * 100f))).ToString() + "%";
							}
							else
							{
								toolTipText += " 0-" + ((int)Math.Round((double)(CS$<>8__locals5.requiredItem.MaxCondition * 100f))).ToString() + "%";
							}
						}
						else if (CS$<>8__locals5.requiredItem.MaxCondition <= 0f)
						{
							toolTipText = TextManager.GetWithVariable("displayname.emptyitem", "[itemname]", toolTipText, FormatCapitals.No);
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:");
						defaultInterpolatedStringHandler.AppendFormatted(Color.White.ToStringHex());
						defaultInterpolatedStringHandler.AppendLiteral("‖");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(toolTipText);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
						toolTipText = defaultInterpolatedStringHandler.ToStringAndClear();
						if (!CS$<>8__locals5.requiredItem.OverrideDescription.IsNullOrEmpty())
						{
							toolTipText += '\n' + CS$<>8__locals5.requiredItem.OverrideDescription;
						}
						else if (!requiredItemPrefab.Description.IsNullOrEmpty())
						{
							toolTipText += '\n' + requiredItemPrefab.Description;
						}
						this.tooltip = new Fabricator.ToolTip
						{
							TargetElement = CS$<>8__locals5.slotRect,
							Tooltip = toolTipText
						};
					}
					slotIndex++;
				}
			}
		}

		// Token: 0x06005C04 RID: 23556 RVA: 0x002F3664 File Offset: 0x002F1864
		private LocalizedString GetSuitableIngredientText(IEnumerable<LocalizedString> itemNameList)
		{
			int count = itemNameList.Count<LocalizedString>();
			if (count == 0)
			{
				return string.Empty;
			}
			if (count == 1)
			{
				return itemNameList.First<LocalizedString>();
			}
			if (count == 2)
			{
				return TextManager.GetWithVariables("DialogRequiredTreatmentOptionsLast", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[treatment1]", itemNameList.ElementAt(0)),
					new ValueTuple<string, LocalizedString>("[treatment2]", itemNameList.ElementAt(1))
				});
			}
			LocalizedString itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsFirst", new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[treatment1]", itemNameList.ElementAt(0)),
				new ValueTuple<string, LocalizedString>("[treatment2]", itemNameList.ElementAt(1))
			});
			bool isTruncated = false;
			int i;
			for (i = 2; i < count - 1; i++)
			{
				if (itemListStr.Length > 50)
				{
					isTruncated = true;
					break;
				}
				itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsFirst", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[treatment1]", itemListStr),
					new ValueTuple<string, LocalizedString>("[treatment2]", itemNameList.ElementAt(i))
				});
			}
			itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsLast", new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[treatment1]", itemListStr),
				new ValueTuple<string, LocalizedString>("[treatment2]", itemNameList.ElementAt(i))
			});
			if (isTruncated)
			{
				itemListStr += TextManager.Get("ellipsis");
			}
			return itemListStr;
		}

		// Token: 0x06005C05 RID: 23557 RVA: 0x002F37C0 File Offset: 0x002F19C0
		private void DrawOutputOverLay(SpriteBatch spriteBatch, GUICustomComponent overlayComponent)
		{
			overlayComponent.RectTransform.SetAsLastChild();
			FabricationRecipe targetItem = this.fabricatedItem ?? this.selectedItem;
			if (targetItem != null)
			{
				ItemInventory inventory = this.outputContainer.Inventory;
				if (((inventory != null) ? inventory.visualSlots : null) != null)
				{
					Rectangle slotRect = this.outputContainer.Inventory.visualSlots[0].Rect;
					if (this.fabricatedItem != null)
					{
						float clampedProgressState = Math.Clamp(this.progressState, 0f, 1f);
						GUI.DrawRectangle(spriteBatch, new Rectangle(slotRect.X, slotRect.Y + (int)((float)slotRect.Height * (1f - clampedProgressState)), slotRect.Width, (int)((float)slotRect.Height * clampedProgressState)), GUIStyle.Green * 0.5f, true, 0f, 1f);
					}
					if (this.outputContainer.Inventory.IsEmpty())
					{
						Sprite itemIcon = targetItem.TargetItem.InventoryIcon ?? targetItem.TargetItem.Sprite;
						Color iconColor = (itemIcon == targetItem.TargetItem.Sprite) ? targetItem.TargetItem.SpriteColor : targetItem.TargetItem.InventoryIconColor;
						itemIcon.Draw(spriteBatch, slotRect.Center.ToVector2(), Color.Lerp(iconColor, Color.TransparentBlack, 0.5f), 0f, Math.Min((float)slotRect.Width / itemIcon.size.X, (float)slotRect.Height / itemIcon.size.Y) * 0.9f, SpriteEffects.None, null);
					}
				}
			}
			if (this.tooltip != null)
			{
				GUIComponent.DrawToolTip(spriteBatch, RichString.Rich(this.tooltip.Tooltip, null), this.tooltip.TargetElement, Anchor.BottomCenter, Pivot.TopLeft);
				this.tooltip = null;
			}
		}

		// Token: 0x06005C06 RID: 23558 RVA: 0x002F3988 File Offset: 0x002F1B88
		private bool FilterEntities(MapEntityCategory? category, string filter)
		{
			GUITickBox guitickBox = this.availableOnlyTickBox;
			bool onlyShowAvailable = guitickBox != null && guitickBox.Selected;
			bool anyVisible = false;
			foreach (GUIComponent child in this.itemList.Content.Children)
			{
				FabricationRecipe recipe = child.UserData as FabricationRecipe;
				if (!(((recipe != null) ? recipe.DisplayName : null) == null))
				{
					if (recipe.HideForNonTraitors)
					{
						Character controlled = Character.Controlled;
						if (controlled == null || !controlled.IsTraitor)
						{
							child.Visible = false;
							continue;
						}
					}
					if (recipe.RequiresRecipe)
					{
						if (recipe.HideIfNoRecipe)
						{
							bool anyOneHasRecipe = Fabricator.AnyOneHasRecipeForItem(Character.Controlled, recipe.TargetItem);
							if (Character.Controlled != null && !anyOneHasRecipe)
							{
								child.Visible = false;
								continue;
							}
						}
						else
						{
							this.SetRecipeTooltip(child, recipe);
						}
					}
					child.Visible = ((string.IsNullOrWhiteSpace(filter) || recipe.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)) && (category == null || recipe.TargetItem.Category.HasFlag(category.Value)) && (!onlyShowAvailable || this.CanBeFabricated(recipe, this.availableIngredients, Character.Controlled)));
					if (child.Visible)
					{
						anyVisible = true;
					}
				}
			}
			foreach (GUIButton btn in this.itemCategoryButtons)
			{
				GUIComponent guicomponent = btn;
				MapEntityCategory? mapEntityCategory = (MapEntityCategory?)btn.UserData;
				MapEntityCategory? mapEntityCategory2 = this.selectedItemCategory;
				guicomponent.Selected = (mapEntityCategory.GetValueOrDefault() == mapEntityCategory2.GetValueOrDefault() & mapEntityCategory != null == (mapEntityCategory2 != null));
			}
			this.HideEmptyItemListCategories();
			this.nothingToShowText.Visible = !anyVisible;
			this.itemList.UserData = "itemlist";
			return true;
		}

		// Token: 0x06005C07 RID: 23559 RVA: 0x002F3BB8 File Offset: 0x002F1DB8
		private void HideEmptyItemListCategories()
		{
			bool visibleElementsChanged = false;
			bool recipeVisible = false;
			foreach (GUIComponent child in this.itemList.Content.Children.Reverse<GUIComponent>())
			{
				if (!(child.UserData is FabricationRecipe))
				{
					if (child.Enabled && child.Visible != recipeVisible)
					{
						child.Visible = recipeVisible;
						visibleElementsChanged = true;
					}
					recipeVisible = false;
				}
				else
				{
					recipeVisible |= child.Visible;
				}
			}
			Fabricator.SortBy sortBy = (Fabricator.SortBy)this.sortByDropdown.SelectedData;
			if (sortBy != Fabricator.SortBy.Category)
			{
				this.itemList.Content.Children.Where(delegate(GUIComponent c)
				{
					object userData = c.UserData;
					return ((userData != null) ? userData.GetType() : null) == typeof(MapEntityCategory);
				}).ForEach(delegate(GUIComponent c)
				{
					c.Visible = false;
				});
			}
			if (visibleElementsChanged)
			{
				this.itemList.UpdateScrollBarSize();
				this.itemList.BarScroll = 0f;
			}
		}

		// Token: 0x06005C08 RID: 23560 RVA: 0x002F3CDC File Offset: 0x002F1EDC
		public bool ClearFilter()
		{
			this.FilterEntities(this.selectedItemCategory, "");
			this.itemList.UpdateScrollBarSize();
			this.itemList.BarScroll = 0f;
			this.itemFilterBox.Text = "";
			return true;
		}

		// Token: 0x06005C09 RID: 23561 RVA: 0x002F3D1C File Offset: 0x002F1F1C
		private bool SelectItem(Character user, FabricationRecipe selectedItem, float? overrideRequiredTime = null)
		{
			this.selectedItem = selectedItem;
			this.displayingForCharacter = user;
			Option<float> overrideRequiredTime2;
			if (overrideRequiredTime != null)
			{
				overrideRequiredTime2 = Option.Some<float>(overrideRequiredTime.Value);
			}
			else
			{
				Option.UnspecifiedNone none = Option.None;
				overrideRequiredTime2 = none;
			}
			Fabricator.SelectedRecipe selectedRecipe = new Fabricator.SelectedRecipe(user, selectedItem, overrideRequiredTime2);
			this.LastSelectedRecipe = Option.Some<Fabricator.SelectedRecipe>(selectedRecipe);
			this.CreateSelectedItemUI(selectedRecipe);
			return true;
		}

		// Token: 0x06005C0A RID: 23562 RVA: 0x002F3D78 File Offset: 0x002F1F78
		private void CreateSelectedItemUI(Fabricator.SelectedRecipe recipe)
		{
			Fabricator.SelectedRecipe selectedRecipe2 = recipe;
			Character character;
			FabricationRecipe fabricationRecipe;
			Option<float> option;
			selectedRecipe2.Deconstruct(out character, out fabricationRecipe, out option);
			Character user = character;
			FabricationRecipe selectedRecipe = fabricationRecipe;
			Option<float> overrideRequiredTime = option;
			int max = Math.Max(selectedRecipe.TargetItem.GetMaxStackSize(this.outputContainer.Inventory) / selectedRecipe.Amount, 1);
			if (this.amountInput != null)
			{
				float prevBarScroll = this.amountInput.BarScroll;
				this.amountInput.Range = new Vector2(1f, (float)max);
				this.amountInput.BarScroll = prevBarScroll;
				this.amountTextMax.Text = max.ToString();
				this.amountInput.Enabled = (this.amountTextMax.Enabled = (max > 1));
				this.AmountToFabricate = Math.Min((int)this.amountInput.BarScrollValue, max);
			}
			this.RefreshActivateButtonText();
			this.selectedItemFrame.ClearChildren();
			this.selectedItemReqsFrame.ClearChildren();
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), this.selectedItemFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.03f,
				CanBeFocused = true
			};
			GUILayoutGroup paddedReqFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), this.selectedItemReqsFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.03f
			};
			LocalizedString itemName = Fabricator.GetRecipeNameAndAmount(selectedRecipe);
			LocalizedString name = itemName;
			Fabricator.QualityResult result = Fabricator.GetFabricatedItemQuality(selectedRecipe, user);
			float minimumQuality = (float)(selectedRecipe.Quality ?? result.Quality);
			LocalizedString qualityTooltip = string.Empty;
			if (result.HasRandomQualityRollChance)
			{
				float plusOnePercentage = result.TotalPlusOnePercentage;
				float plusTwoPercentage = result.TotalPlusTwoPercentage;
				string plusOnePercentageText = plusOnePercentage.ToString("F1", CultureInfo.InvariantCulture);
				string plusTwoPercentageText = plusTwoPercentage.ToString("F1", CultureInfo.InvariantCulture);
				int plusOneQuality = Math.Clamp(result.Quality + 1, 0, 3);
				int plusTwoQuality = Math.Clamp(result.Quality + 2, 0, 3);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("quality");
				defaultInterpolatedStringHandler.AppendFormatted<int>(plusOneQuality);
				LocalizedString plusOneQualityText = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("quality");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(plusTwoQuality);
				LocalizedString plusTwoQualityText = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
				string localizationTag = (plusTwoPercentage > 0f && plusOnePercentage > 0f && plusOneQuality != plusTwoQuality) ? "meetsbonusrequirementtwice" : "meetsbonusrequirement";
				ValueTuple<string, LocalizedString>[] variables = new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[chance]", plusOnePercentageText),
					new ValueTuple<string, LocalizedString>("[quality]", plusOneQualityText),
					new ValueTuple<string, LocalizedString>("[chance2]", plusTwoPercentageText),
					new ValueTuple<string, LocalizedString>("[quality2]", plusTwoQualityText)
				};
				if (MathUtils.NearlyEqual(plusOnePercentage, 0f, 0.0001f))
				{
					variables = new ValueTuple<string, LocalizedString>[]
					{
						new ValueTuple<string, LocalizedString>("[chance]", plusTwoPercentageText),
						new ValueTuple<string, LocalizedString>("[quality]", plusTwoQualityText)
					};
				}
				if (plusOneQuality == plusTwoQuality)
				{
					LocalizedString rawPercentage = result.PlusOnePercentage.ToString("F1", CultureInfo.InvariantCulture);
					variables = new ValueTuple<string, LocalizedString>[]
					{
						new ValueTuple<string, LocalizedString>("[chance]", rawPercentage),
						new ValueTuple<string, LocalizedString>("[quality]", plusOneQualityText)
					};
				}
				if (plusOnePercentage >= 100f)
				{
					minimumQuality = (float)plusOneQuality;
				}
				if (plusTwoPercentage >= 100f)
				{
					minimumQuality = (float)plusTwoQuality;
				}
				qualityTooltip = TextManager.GetWithVariables(localizationTag, variables);
			}
			if (minimumQuality > 0f || result.HasRandomQualityRollChance)
			{
				name = TextManager.GetWithVariable("itemname.quality" + ((int)minimumQuality).ToString(), "[itemname]", itemName + '\n', FormatCapitals.No).Fallback(TextManager.GetWithVariable("itemname.quality3", "[itemname]", itemName + '\n', FormatCapitals.No), true);
			}
			GUITextBlock nameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(name, null), new Color?(Color.Aqua), GUIStyle.SubHeadingFont, Alignment.TopLeft, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			if (result.HasRandomQualityRollChance)
			{
				GUIFrame iconLayout = new GUIFrame(new RectTransform(new Vector2(0.4f, 1f), this.selectedItemFrame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, null);
				GUIImage icon = GameSession.CreateNotificationIcon(iconLayout, true);
				icon.ToolTip = RichString.Rich(qualityTooltip, null);
				icon.Visible = (icon.CanBeFocused = true);
			}
			this.outputTopArea.RectTransform.MaxSize = new Point(int.MaxValue, this.outputInventoryHolder.Rect.Height);
			this.paddedOutputArea.Recalculate();
			nameBlock.Padding = new Vector4(0f, nameBlock.Padding.Y, (float)GUI.IntScale(5f), nameBlock.Padding.W);
			if (nameBlock.TextScale < 0.7f)
			{
				nameBlock.AutoScaleHorizontal = false;
				nameBlock.TextScale = 0.7f;
				nameBlock.Wrap = true;
				nameBlock.SetTextPos();
				nameBlock.RectTransform.MinSize = new Point(0, (int)(nameBlock.TextSize.Y * nameBlock.TextScale));
			}
			bool largeUI = base.GuiFrame.Rect.Height > GUI.IntScale(500f);
			if (largeUI)
			{
				paddedFrame.ChildAnchor = Anchor.CenterLeft;
			}
			if (!selectedRecipe.TargetItem.Description.IsNullOrEmpty())
			{
				RichString richDescription = RichString.Rich(selectedRecipe.TargetItem.Description, null);
				GUILayoutGroup descriptionParent = largeUI ? paddedReqFrame : paddedFrame;
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), descriptionParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = richDescription;
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock description = new GUITextBlock(rectT, text2, null, smallFont, Alignment.Left, true, "", null);
				if (!largeUI)
				{
					description.Padding = new Vector4(0f, description.Padding.Y, description.Padding.Z, description.Padding.W);
				}
				while (description.Rect.Height + nameBlock.Rect.Height > descriptionParent.Rect.Height / 2)
				{
					IReadOnlyList<LocalizedString> lines = description.WrappedText.Split(new char[]
					{
						'\n'
					});
					if (lines.Count <= 1)
					{
						break;
					}
					string newString = string.Join<LocalizedString>('\n', lines.Take(lines.Count - 1));
					if (newString.Length > 4)
					{
						description.Text = newString.Substring(0, newString.Length - 4) + "...";
					}
					else
					{
						description.Text = newString + "...";
					}
					description.CalculateHeightFromText(0, false);
					description.ToolTip = richDescription;
				}
				description.Text.RetrieveValue();
			}
			IEnumerable<Skill> inadequateSkills = Enumerable.Empty<Skill>();
			if (user != null)
			{
				inadequateSkills = from skill in selectedRecipe.RequiredSkills
				where (double)user.GetSkillLevel(skill.Identifier) < Math.Round((double)(skill.Level * this.SkillRequirementMultiplier))
				select skill;
			}
			if (selectedRecipe.RequiredSkills.Any<Skill>())
			{
				LocalizedString text = "";
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("FabricatorRequiredSkills"), new Color?(inadequateSkills.Any<Skill>() ? GUIStyle.Red : GUIStyle.Green), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null);
				guitextBlock.AutoScaleHorizontal = true;
				guitextBlock.ToolTip = TextManager.Get("fabricatorrequiredskills.tooltip");
				foreach (Skill skill2 in selectedRecipe.RequiredSkills)
				{
					text += TextManager.Get("SkillName." + skill2.Identifier.ToString()) + " " + TextManager.Get("Lvl").ToLower() + " " + Math.Round((double)(skill2.Level * this.SkillRequirementMultiplier));
					if (skill2 != selectedRecipe.RequiredSkills.Last<Skill>())
					{
						text += "\n";
					}
				}
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = text;
				GUIFont smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text3, null, smallFont, Alignment.Left, false, "", null);
			}
			float degreeOfSuccess = (user == null) ? 0f : this.FabricationDegreeOfSuccess(user, selectedRecipe.RequiredSkills);
			if (degreeOfSuccess > 0.5f)
			{
				degreeOfSuccess = 1f;
			}
			float time;
			float requiredTime = overrideRequiredTime.TryUnwrap(out time) ? time : ((user == null) ? selectedRecipe.RequiredTime : this.GetRequiredTime(selectedRecipe, user));
			if ((int)requiredTime > 0)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("FabricatorRequiredTime"), new Color?(ToolBox.GradientLerp(degreeOfSuccess, new Color[]
				{
					GUIStyle.Red,
					Color.Yellow,
					GUIStyle.Green
				})), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
				RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = ToolBox.SecondsToReadableTime(requiredTime);
				GUIFont smallFont = GUIStyle.SmallFont;
				this.requiredTimeBlock = new GUITextBlock(rectT3, text4, null, smallFont, Alignment.Left, false, "", null);
			}
			if (selectedRecipe.RequiredMoney > 0)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("subeditor.price"), new Color?(ToolBox.GradientLerp(degreeOfSuccess, new Color[]
				{
					GUIStyle.Red,
					Color.Yellow,
					GUIStyle.Green
				})), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = TextManager.FormatCurrency(this.SelectedItem.RequiredMoney, true);
				GUIFont smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT4, text5, null, smallFont, Alignment.Left, false, "", null);
			}
			if (selectedRecipe.RequiresRecipe && !Fabricator.AnyOneHasRecipeForItem(Character.Controlled, selectedRecipe.TargetItem))
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedReqFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("fabricatorrequiresrecipe"), new Color?(GUIStyle.Red), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
			}
		}

		// Token: 0x06005C0B RID: 23563 RVA: 0x002F4ADC File Offset: 0x002F2CDC
		public void HighlightRecipe(string identifier, Color color)
		{
			foreach (GUIComponent child in this.itemList.Content.Children)
			{
				FabricationRecipe recipe = child.UserData as FabricationRecipe;
				if (!(((recipe != null) ? recipe.DisplayName : null) == null) && recipe.TargetItem.Identifier == identifier)
				{
					if (child.FlashTimer > 0f)
					{
						break;
					}
					child.Flash(new Color?(color), 1.5f, false, false, null);
					for (int i = 0; i < child.CountChildren; i++)
					{
						GUIComponent grandChild = child.GetChild(i);
						if (!(grandChild is GUITextBlock))
						{
							grandChild.Flash(new Color?(color), 1.5f, false, false, null);
						}
					}
					break;
				}
			}
		}

		// Token: 0x06005C0C RID: 23564 RVA: 0x002F4BDC File Offset: 0x002F2DDC
		private bool StartButtonClicked(GUIButton button, object obj)
		{
			if (this.selectedItem == null)
			{
				return false;
			}
			if (this.fabricatedItem == null && !this.outputContainer.Inventory.CanProbablyBePut(this.selectedItem.TargetItem, new float?(this.selectedItem.OutCondition * this.selectedItem.TargetItem.Health), null))
			{
				this.outputSlot.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
				return false;
			}
			this.amountRemaining = this.AmountToFabricate;
			if (GameMain.Client != null)
			{
				this.pendingFabricatedItem = ((this.fabricatedItem != null) ? null : this.selectedItem);
				this.item.CreateClientEvent<Fabricator>(this);
			}
			else if (this.fabricatedItem == null)
			{
				this.StartFabricating(this.selectedItem, Character.Controlled, true);
			}
			else
			{
				this.CancelFabricating(Character.Controlled);
			}
			return true;
		}

		// Token: 0x06005C0D RID: 23565 RVA: 0x002F4CD0 File Offset: 0x002F2ED0
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.activateButton.Enabled = false;
			this.inSufficientPowerWarning.Visible = (this.IsActive && !this.hasPower);
			this.ingredientHighlightTimer -= deltaTime;
			if (!this.IsActive)
			{
				if (this.outputContainer != null && this.outputContainer.Inventory.AllItems.Any<Item>())
				{
					VisualSlot[] visualSlots = this.outputContainer.Inventory.visualSlots;
					if (visualSlots != null && visualSlots.Any<VisualSlot>() && visualSlots[0].HighlightTimer <= 0f)
					{
						visualSlots[0].ShowBorderHighlight(GUIStyle.Green, 0.5f, 0.5f, 0.5f);
					}
				}
				if (this.selectedItem != null && this.displayingForCharacter != character)
				{
					this.SelectItem(character, this.selectedItem, null);
				}
				if (this.refreshIngredientsTimer <= 0f)
				{
					this.RefreshAvailableIngredients();
					this.refreshIngredientsTimer = 1f;
				}
				this.refreshIngredientsTimer -= deltaTime;
			}
			if (character != null)
			{
				foreach (GUIComponent child in this.itemList.Content.Children)
				{
					FabricationRecipe recipe = child.UserData as FabricationRecipe;
					if (recipe != null && (recipe == this.selectedItem || (child.Rect.Y <= this.itemList.Rect.Bottom && child.Rect.Bottom >= this.itemList.Rect.Y)))
					{
						bool canBeFabricated = this.CanBeFabricated(recipe, this.availableIngredients, character);
						if (recipe == this.selectedItem)
						{
							this.activateButton.Enabled = canBeFabricated;
						}
						bool sufficientSkills = this.FabricationDegreeOfSuccess(character, recipe.RequiredSkills) >= 0.5f;
						Color baseColor = this.MissingRequiredRecipe(recipe, character) ? GUIStyle.Red : (sufficientSkills ? GUIStyle.TextColorNormal : GUIStyle.Orange);
						GUILayoutGroup childContainer = child.GetChild<GUILayoutGroup>();
						childContainer.GetChild<GUITextBlock>().TextColor = baseColor * (canBeFabricated ? 1f : 0.5f);
						GUIImage icon = childContainer.GetChild<GUIImage>();
						Color iconColor = (icon.Sprite == recipe.TargetItem.Sprite) ? recipe.TargetItem.SpriteColor : recipe.TargetItem.InventoryIconColor;
						childContainer.GetChild<GUIImage>().Color = iconColor * (canBeFabricated ? 1f : 0.5f);
						GUIComponent limitReachedText = child.FindChild("FabricationLimitReachedText", false);
						int amount;
						limitReachedText.Visible = (!canBeFabricated && this.fabricationLimits.TryGetValue(recipe.RecipeHash, out amount) && amount <= 0);
					}
				}
			}
		}

		// Token: 0x06005C0E RID: 23566 RVA: 0x002F4FD0 File Offset: 0x002F31D0
		public override void OnPlayerSkillsChanged()
		{
			this.RefreshSelectedItem();
		}

		// Token: 0x06005C0F RID: 23567 RVA: 0x002F4FD8 File Offset: 0x002F31D8
		public void RefreshSelectedItem()
		{
			Fabricator.SelectedRecipe lastSelected;
			if (!this.LastSelectedRecipe.TryUnwrap(out lastSelected))
			{
				return;
			}
			this.CreateSelectedItemUI(lastSelected);
		}

		// Token: 0x06005C10 RID: 23568 RVA: 0x002F4FFC File Offset: 0x002F31FC
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			FabricationRecipe fabricationRecipe = this.pendingFabricatedItem;
			uint recipeHash = (fabricationRecipe != null) ? fabricationRecipe.RecipeHash : 0U;
			msg.WriteUInt32(recipeHash);
			msg.WriteRangedInteger(this.AmountToFabricate, 1, 99);
		}

		// Token: 0x06005C11 RID: 23569 RVA: 0x002F5034 File Offset: 0x002F3234
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			Fabricator.FabricatorState newState = (Fabricator.FabricatorState)msg.ReadByte();
			int amountToFabricate = msg.ReadRangedInteger(0, 99);
			int amountRemaining = msg.ReadRangedInteger(0, 99);
			float newTimeUntilReady = msg.ReadSingle();
			uint recipeHash = msg.ReadUInt32();
			ushort userID = msg.ReadUInt16();
			Character user = Entity.FindEntityByID(userID) as Character;
			ushort reachedLimitCount = msg.ReadUInt16();
			for (int i = 0; i < (int)reachedLimitCount; i++)
			{
				this.fabricationLimits[msg.ReadUInt32()] = 0;
			}
			this.State = newState;
			if ((user != null && user != Character.Controlled) || this.State != Fabricator.FabricatorState.Stopped)
			{
				this.amountToFabricate = amountToFabricate;
			}
			this.amountRemaining = amountRemaining;
			if (newState == Fabricator.FabricatorState.Stopped || recipeHash == 0U)
			{
				this.CancelFabricating(null);
			}
			else if (newState == Fabricator.FabricatorState.Active || newState == Fabricator.FabricatorState.Paused)
			{
				if (this.fabricatedItem != null && this.fabricatedItem.RecipeHash == recipeHash)
				{
					return;
				}
				if (recipeHash == 0U)
				{
					return;
				}
				this.SelectItem(user, this.fabricationRecipes[recipeHash], null);
				this.StartFabricating(this.fabricationRecipes[recipeHash], user, true);
			}
			this.timeUntilReady = newTimeUntilReady;
		}

		// Token: 0x1700173A RID: 5946
		// (get) Token: 0x06005C12 RID: 23570 RVA: 0x002F5148 File Offset: 0x002F3348
		// (set) Token: 0x06005C13 RID: 23571 RVA: 0x002F5150 File Offset: 0x002F3350
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 1000f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float FabricationSpeed { get; set; }

		// Token: 0x1700173B RID: 5947
		// (get) Token: 0x06005C14 RID: 23572 RVA: 0x002F5159 File Offset: 0x002F3359
		// (set) Token: 0x06005C15 RID: 23573 RVA: 0x002F5161 File Offset: 0x002F3361
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillRequirementMultiplier { get; set; }

		// Token: 0x1700173C RID: 5948
		// (get) Token: 0x06005C16 RID: 23574 RVA: 0x002F516A File Offset: 0x002F336A
		// (set) Token: 0x06005C17 RID: 23575 RVA: 0x002F5172 File Offset: 0x002F3372
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int AmountToFabricate
		{
			get
			{
				return this.amountToFabricate;
			}
			set
			{
				this.amountToFabricate = MathHelper.Clamp(value, 1, 99);
			}
		}

		// Token: 0x1700173D RID: 5949
		// (get) Token: 0x06005C18 RID: 23576 RVA: 0x002F5183 File Offset: 0x002F3383
		// (set) Token: 0x06005C19 RID: 23577 RVA: 0x002F518B File Offset: 0x002F338B
		private Fabricator.FabricatorState State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state == value)
				{
					return;
				}
				this.state = value;
			}
		}

		// Token: 0x1700173E RID: 5950
		// (get) Token: 0x06005C1A RID: 23578 RVA: 0x002F519E File Offset: 0x002F339E
		public ItemContainer InputContainer
		{
			get
			{
				return this.inputContainer;
			}
		}

		// Token: 0x1700173F RID: 5951
		// (get) Token: 0x06005C1B RID: 23579 RVA: 0x002F51A6 File Offset: 0x002F33A6
		public ItemContainer OutputContainer
		{
			get
			{
				return this.outputContainer;
			}
		}

		// Token: 0x06005C1C RID: 23580 RVA: 0x002F51B0 File Offset: 0x002F33B0
		public Fabricator(Item item, ContentXElement element)
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastSelectedRecipe = none;
			this.availableIngredients = new Dictionary<Identifier, List<Item>>();
			this.fabricationLimits = new Dictionary<uint, int>();
			this.usedIngredients = new HashSet<Item>();
			this.ingredientFlexibilityCache = new Dictionary<ItemPrefab, int>();
			this.linkedInventories = new HashSet<Inventory>();
			base..ctor(item, element);
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("fabricableitem", StringComparison.OrdinalIgnoreCase))
				{
					DebugConsole.ThrowError("Error in item " + item.Name + "! Fabrication recipes should be defined in the craftable item's xml, not in the fabricator.", null, element.ContentPackage, false, false);
					break;
				}
			}
			Dictionary<uint, FabricationRecipe> fabricationRecipes = new Dictionary<uint, FabricationRecipe>();
			Func<Identifier, bool> <>9__0;
			foreach (ItemPrefab itemPrefab in ItemPrefab.Prefabs)
			{
				foreach (FabricationRecipe recipe in itemPrefab.FabricationRecipes.Values)
				{
					if (recipe.SuitableFabricatorIdentifiers.Length > 0)
					{
						ImmutableArray<Identifier> suitableFabricatorIdentifiers = recipe.SuitableFabricatorIdentifiers;
						Func<Identifier, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Identifier i) => item.Prefab.Identifier == i || item.HasTag(i)));
						}
						if (!suitableFabricatorIdentifiers.Any(predicate))
						{
							continue;
						}
					}
					ContentPackage packageToLog = itemPrefab.GetParentModPackageOrThisPackage();
					bool recipeInvalid = false;
					foreach (FabricationRecipe.RequiredItem requiredItem in recipe.RequiredItems)
					{
						if (requiredItem.ItemPrefabs.None(null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Error in the fabrication recipe for \"");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\". Could not find the ingredient \"");
							defaultInterpolatedStringHandler.AppendFormatted<FabricationRecipe.RequiredItem>(requiredItem);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, packageToLog, false, false);
							recipeInvalid = true;
						}
					}
					if (!recipeInvalid)
					{
						FabricationRecipe duplicateRecipe;
						if (fabricationRecipes.TryGetValue(recipe.RecipeHash, out duplicateRecipe))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(63, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Error in the fabrication recipe for \"");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(itemPrefab.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\". Duplicate recipe in \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(duplicateRecipe.TargetItem.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, packageToLog, false, false);
						}
						else
						{
							fabricationRecipes.Add(recipe.RecipeHash, recipe);
							if (recipe.FabricationLimitMax >= 0)
							{
								this.fabricationLimits.Add(recipe.RecipeHash, Rand.Range(recipe.FabricationLimitMin, recipe.FabricationLimitMax + 1, Rand.RandSync.Unsynced));
							}
						}
					}
				}
			}
			this.fabricationRecipes = fabricationRecipes.ToImmutableDictionary<uint, FabricationRecipe>();
			this.state = Fabricator.FabricatorState.Stopped;
		}

		// Token: 0x06005C1D RID: 23581 RVA: 0x002F550C File Offset: 0x002F370C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<ItemContainer> containers = this.item.GetComponents<ItemContainer>().ToList<ItemContainer>();
			if (containers.Count < 2)
			{
				DebugConsole.ThrowError("Error in item \"" + this.item.Name + "\": Fabricators must have two ItemContainer components!", null, null, false, false);
				return;
			}
			this.inputContainer = containers[0];
			this.outputContainer = containers[1];
			foreach (FabricationRecipe recipe in this.fabricationRecipes.Values)
			{
				if (recipe.RequiredItems.Length > this.inputContainer.Capacity)
				{
					DebugConsole.ThrowErrorLocalized("Error in item \"" + this.item.Name + "\": There's not enough room in the input inventory for the ingredients of \"" + recipe.TargetItem.Name + "\"!", null, null, false, false);
				}
			}
			this.OnItemLoadedProjSpecific();
		}

		// Token: 0x06005C1E RID: 23582 RVA: 0x002F561C File Offset: 0x002F381C
		private void OnItemLoadedProjSpecific()
		{
			this.CreateGUI();
			this.InitInventoryUIs();
		}

		// Token: 0x06005C1F RID: 23583 RVA: 0x002F562A File Offset: 0x002F382A
		public override bool Select(Character character)
		{
			this.SelectProjSpecific(character);
			return base.Select(character);
		}

		// Token: 0x06005C20 RID: 23584 RVA: 0x002F563C File Offset: 0x002F383C
		private void SelectProjSpecific(Character character)
		{
			if (character != Character.Controlled)
			{
				return;
			}
			List<GUIComponent> nonItems = (from c in this.itemList.Content.Children
			where !(c.UserData is FabricationRecipe)
			select c).ToList<GUIComponent>();
			nonItems.ForEach(delegate(GUIComponent i)
			{
				i.Visible = false;
			});
			this.SortItems(character);
			MapEntityCategory? category = this.selectedItemCategory;
			GUITextBox guitextBox = this.itemFilterBox;
			this.FilterEntities(category, ((guitextBox != null) ? guitextBox.Text : null) ?? string.Empty);
			this.HideEmptyItemListCategories();
		}

		// Token: 0x06005C21 RID: 23585 RVA: 0x002F56E6 File Offset: 0x002F38E6
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x06005C22 RID: 23586 RVA: 0x002F56EC File Offset: 0x002F38EC
		public void RemoveFabricationRecipes(IEnumerable<Identifier> allowedIdentifiers)
		{
			this.fabricationRecipes = (from kvp in this.fabricationRecipes
			where allowedIdentifiers.Contains(kvp.Value.TargetItemPrefabIdentifier)
			select kvp).ToImmutableDictionary<uint, FabricationRecipe>();
			this.CreateRecipes();
		}

		// Token: 0x06005C23 RID: 23587 RVA: 0x002F5730 File Offset: 0x002F3930
		private void CreateRecipes()
		{
			this.itemList.Content.RectTransform.ClearChildren();
			foreach (FabricationRecipe fi in this.fabricationRecipes.Values)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.itemList.Content.Rect.Width, (int)(40f * GUI.yScale)), this.itemList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
				{
					UserData = fi,
					HoverColor = Color.Gold * 0.2f,
					SelectedColor = Color.Gold * 0.5f
				};
				this.SetRecipeTooltip(frame, fi);
				GUILayoutGroup container = new GUILayoutGroup(new RectTransform(Vector2.One, frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					RelativeSpacing = 0.02f
				};
				Sprite itemIcon = fi.TargetItem.InventoryIcon ?? fi.TargetItem.Sprite;
				if (itemIcon != null)
				{
					GUIImage guiimage = new GUIImage(new RectTransform(new Point(frame.Rect.Height, frame.Rect.Height), container.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), itemIcon, true, null);
					guiimage.Color = ((itemIcon == fi.TargetItem.Sprite) ? fi.TargetItem.SpriteColor : fi.TargetItem.InventoryIconColor);
					guiimage.CanBeFocused = false;
				}
				RectTransform rectT = new RectTransform(new Vector2(0.85f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = RichString.Rich(Fabricator.GetRecipeNameAndAmount(fi), null);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.AutoScaleVertical = true;
				guitextBlock.CanBeFocused = false;
				RectTransform rectT2 = new RectTransform(new Vector2(0.85f, 1f), frame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get(this.FabricationLimitReachedText);
				smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock2 = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.BottomRight, false, "", null);
				guitextBlock2.UserData = "FabricationLimitReachedText";
				guitextBlock2.Visible = false;
			}
		}

		// Token: 0x06005C24 RID: 23588 RVA: 0x002F5A20 File Offset: 0x002F3C20
		private void StartFabricating(FabricationRecipe selectedItem, Character user, bool addToServerLog = true)
		{
			if (selectedItem == null)
			{
				return;
			}
			if (!this.outputContainer.Inventory.CanProbablyBePut(selectedItem.TargetItem, new float?(selectedItem.OutCondition * selectedItem.TargetItem.Health), null))
			{
				return;
			}
			this.IsActive = true;
			this.user = user;
			this.fabricatedItem = selectedItem;
			this.RefreshAvailableIngredients();
			this.itemList.Enabled = false;
			if (this.amountInput != null)
			{
				this.amountInput.Enabled = false;
			}
			this.RefreshActivateButtonText();
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				this.MoveIngredientsToInputContainer(selectedItem);
			}
			this.requiredTime = this.GetRequiredTime(this.fabricatedItem, user);
			this.timeUntilReady = this.requiredTime;
			this.inputContainer.Inventory.Locked = true;
			this.outputContainer.Inventory.Locked = true;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || networkMember.IsServer)
			{
				this.State = Fabricator.FabricatorState.Active;
			}
		}

		// Token: 0x06005C25 RID: 23589 RVA: 0x002F5B28 File Offset: 0x002F3D28
		private void CancelFabricating(Character user = null)
		{
			this.IsActive = false;
			this.user = null;
			this.currPowerConsumption = 0f;
			this.progressState = 0f;
			this.timeUntilReady = 0f;
			this.UpdateRequiredTimeProjSpecific();
			this.inputContainer.Inventory.Locked = false;
			this.outputContainer.Inventory.Locked = false;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || networkMember.IsServer)
			{
				this.State = Fabricator.FabricatorState.Stopped;
			}
			if (this.fabricatedItem == null)
			{
				return;
			}
			this.itemList.Enabled = true;
			if (this.amountInput != null)
			{
				this.amountInput.Enabled = this.amountTextMax.Enabled;
			}
			this.RefreshActivateButtonText();
			this.fabricatedItem = null;
		}

		// Token: 0x06005C26 RID: 23590 RVA: 0x002F5BE8 File Offset: 0x002F3DE8
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.refreshIngredientsTimer <= 0f)
			{
				this.RefreshAvailableIngredients();
				this.refreshIngredientsTimer = 1f;
			}
			this.refreshIngredientsTimer -= deltaTime;
			NetworkMember networkMember = GameMain.NetworkMember;
			bool isClient = networkMember != null && networkMember.IsClient;
			if (!isClient && (this.fabricatedItem == null || !this.CanBeFabricated(this.fabricatedItem, this.availableIngredients, this.user)))
			{
				this.CancelFabricating(null);
				return;
			}
			this.progressState = ((this.fabricatedItem == null) ? 0f : ((this.requiredTime - this.timeUntilReady) / this.requiredTime));
			if (isClient)
			{
				this.hasPower = (this.State != Fabricator.FabricatorState.Paused);
				if (!this.hasPower)
				{
					return;
				}
			}
			else
			{
				this.hasPower = this.HasPower;
				if (!this.hasPower)
				{
					this.State = Fabricator.FabricatorState.Paused;
					return;
				}
				this.State = Fabricator.FabricatorState.Active;
			}
			float tinkeringStrength = 0f;
			Repairable repairable = this.item.GetComponent<Repairable>();
			if (repairable != null)
			{
				repairable.LastActiveTime = (float)Timing.TotalTime + 10f;
				if (repairable.IsTinkering)
				{
					tinkeringStrength = repairable.TinkeringStrength;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float fabricationSpeedIncrease = 1f + tinkeringStrength * 2.5f;
			this.timeUntilReady -= deltaTime * fabricationSpeedIncrease * Math.Min((this.powerConsumption <= 0f) ? 1f : base.Voltage, 2f);
			this.UpdateRequiredTimeProjSpecific();
			if (this.timeUntilReady <= 0f)
			{
				this.Fabricate();
			}
		}

		// Token: 0x06005C27 RID: 23591 RVA: 0x002F5D77 File Offset: 0x002F3F77
		private Client GetUsingClient()
		{
			return null;
		}

		// Token: 0x06005C28 RID: 23592 RVA: 0x002F5D7C File Offset: 0x002F3F7C
		private void Fabricate()
		{
			Fabricator.<>c__DisplayClass133_0 CS$<>8__locals1 = new Fabricator.<>c__DisplayClass133_0();
			CS$<>8__locals1.<>4__this = this;
			this.RefreshAvailableIngredients();
			if (this.fabricatedItem == null || !this.CanBeFabricated(this.fabricatedItem, this.availableIngredients, this.user))
			{
				this.CancelFabricating(null);
				return;
			}
			if (this.fabricatedItem.RequiredMoney > 0)
			{
				if (this.user == null)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				if (!(((gameSession != null) ? gameSession.GameMode : null) is MultiPlayerCampaign))
				{
					GameSession gameSession2 = GameMain.GameSession;
					CampaignMode campaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
					if (campaign != null)
					{
						campaign.Bank.Deduct(this.fabricatedItem.RequiredMoney);
					}
				}
			}
			CS$<>8__locals1.ingredientsStolen = false;
			CS$<>8__locals1.ingredientsAllowStealing = true;
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				List<Item> chosenIngredients = new List<Item>();
				IEnumerable<Item> suitableIngredients = this.GetSortedSuitableIngredients();
				foreach (FabricationRecipe.RequiredItem requiredItem in this.fabricatedItem.RequiredItems)
				{
					for (int i = 0; i < requiredItem.Amount; i++)
					{
						foreach (Item suitableIngredient in suitableIngredients)
						{
							if (requiredItem.MatchesItem(suitableIngredient) && !chosenIngredients.Contains(suitableIngredient))
							{
								CS$<>8__locals1.ingredientsStolen |= suitableIngredient.StolenDuringRound;
								if (!suitableIngredient.AllowStealing)
								{
									CS$<>8__locals1.ingredientsAllowStealing = false;
								}
								if (requiredItem.UseCondition && suitableIngredient.ConditionPercentage - requiredItem.MinCondition * 100f > 0f)
								{
									suitableIngredient.Condition -= suitableIngredient.Prefab.Health * requiredItem.MinCondition;
									break;
								}
								if (suitableIngredient.OwnInventory != null)
								{
									foreach (Item containedItem in suitableIngredient.OwnInventory.AllItemsMod)
									{
										ItemContainer component = suitableIngredient.GetComponent<ItemContainer>();
										if (component != null && component.RemoveContainedItemsOnDeconstruct)
										{
											Entity.Spawner.AddItemToRemoveQueue(containedItem);
										}
										else
										{
											containedItem.Drop(null, true, true);
										}
									}
								}
								chosenIngredients.Add(suitableIngredient);
								break;
							}
						}
					}
				}
				Fabricator.AbilityFabricationItemIngredients fabricationIngredients = new Fabricator.AbilityFabricationItemIngredients(chosenIngredients);
				Character character2 = this.user;
				if (character2 != null)
				{
					character2.CheckTalents(AbilityEffectType.OnItemFabricatedIngredients, fabricationIngredients);
				}
				foreach (Item availableItem in fabricationIngredients.Items)
				{
					Entity.Spawner.AddItemToRemoveQueue(availableItem);
					this.inputContainer.Inventory.RemoveItem(availableItem);
				}
				int amountFittingContainer = this.outputContainer.Inventory.HowManyCanBePut(this.fabricatedItem.TargetItem, new float?(this.fabricatedItem.OutCondition * this.fabricatedItem.TargetItem.Health));
				Fabricator.AbilityFabricationItemAmount fabricationitemAmount = new Fabricator.AbilityFabricationItemAmount(this.fabricatedItem.TargetItem, (float)this.fabricatedItem.Amount);
				int quality = 0;
				if (this.fabricatedItem.Quality != null)
				{
					quality = this.fabricatedItem.Quality.Value;
				}
				else
				{
					Character character3 = this.user;
					if (((character3 != null) ? character3.Info : null) != null)
					{
						foreach (Character character in Character.GetFriendlyCrew(this.user))
						{
							character.CheckTalents(AbilityEffectType.OnAllyItemFabricatedAmount, fabricationitemAmount);
						}
						this.user.CheckTalents(AbilityEffectType.OnItemFabricatedAmount, fabricationitemAmount);
						quality = ((this.fabricatedItem.TargetItem.MaxStackSize > 1) ? Fabricator.GetFabricatedItemQuality(this.fabricatedItem, this.user).Quality : Fabricator.GetFabricatedItemQuality(this.fabricatedItem, this.user).RollQuality());
					}
				}
				int amount = (int)fabricationitemAmount.Value;
				if (this.fabricationLimits.ContainsKey(this.fabricatedItem.RecipeHash))
				{
					if (amount > this.fabricationLimits[this.fabricatedItem.RecipeHash])
					{
						amount = this.fabricationLimits[this.fabricatedItem.RecipeHash];
						this.fabricationLimits[this.fabricatedItem.RecipeHash] = 0;
					}
					else
					{
						Dictionary<uint, int> dictionary = this.fabricationLimits;
						uint recipeHash = this.fabricatedItem.RecipeHash;
						dictionary[recipeHash] -= amount;
					}
				}
				Character tempUser = this.user;
				for (int j = 0; j < amount; j++)
				{
					float outCondition = this.fabricatedItem.OutCondition;
					if (this.fabricatedItem.TargetItem.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
					{
						string str = "ItemFabricated:";
						GameSession gameSession3 = GameMain.GameSession;
						string text;
						if (gameSession3 == null)
						{
							text = null;
						}
						else
						{
							GameMode gameMode = gameSession3.GameMode;
							text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
						}
						GameAnalyticsManager.AddDesignEvent(str + (text ?? "none") + ":" + this.fabricatedItem.TargetItem.Identifier.ToString());
					}
					InvSlotType invSlot = this.fabricatedItem.MoveToSlot;
					if (j < amountFittingContainer)
					{
						Entity.Spawner.AddItemToSpawnQueue(this.fabricatedItem.TargetItem, this.outputContainer.Inventory, new float?(this.fabricatedItem.TargetItem.Health * outCondition), new int?(quality), delegate(Item spawnedItem)
						{
							CS$<>8__locals1.<Fabricate>g__onItemSpawned|0(spawnedItem, tempUser, invSlot);
							spawnedItem.Quality = quality;
							spawnedItem.StolenDuringRound = CS$<>8__locals1.ingredientsStolen;
							spawnedItem.AllowStealing = CS$<>8__locals1.ingredientsAllowStealing;
							spawnedItem.Condition = spawnedItem.MaxCondition * outCondition;
						}, true, false, InvSlotType.None);
					}
					else
					{
						Entity.Spawner.AddItemToSpawnQueue(this.fabricatedItem.TargetItem, this.item.Position, this.item.Submarine, new float?(this.fabricatedItem.TargetItem.Health * outCondition), new int?(quality), delegate(Item spawnedItem)
						{
							CS$<>8__locals1.<Fabricate>g__onItemSpawned|0(spawnedItem, tempUser, invSlot);
							spawnedItem.Quality = quality;
							spawnedItem.StolenDuringRound = CS$<>8__locals1.ingredientsStolen;
							spawnedItem.AllowStealing = CS$<>8__locals1.ingredientsAllowStealing;
							spawnedItem.Condition = spawnedItem.MaxCondition * outCondition;
						});
					}
				}
				Character character4 = this.user;
				if (((character4 != null) ? character4.Info : null) != null && !this.user.Removed)
				{
					foreach (Skill skill in this.fabricatedItem.RequiredSkills)
					{
						float addedSkill = skill.Level * SkillSettings.Current.SkillIncreasePerFabricatorRequiredSkill;
						Fabricator.AbilityFabricatorSkillGain addedSkillValue = new Fabricator.AbilityFabricatorSkillGain(skill.Identifier, addedSkill);
						this.user.CheckTalents(AbilityEffectType.OnItemFabricationSkillGain, addedSkillValue);
						this.user.Info.ApplySkillGain(skill.Identifier, addedSkillValue.Value, false, 2f, false);
					}
				}
				FabricationRecipe prevFabricatedItem = this.fabricatedItem;
				Character prevUser = this.user;
				this.CancelFabricating(null);
				this.amountRemaining--;
				if (this.amountRemaining > 0 && this.CanBeFabricated(prevFabricatedItem, this.availableIngredients, prevUser))
				{
					this.StartFabricating(prevFabricatedItem, prevUser, false);
				}
			}
		}

		// Token: 0x06005C29 RID: 23593 RVA: 0x002F653C File Offset: 0x002F473C
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			this.currPowerConsumption = base.PowerConsumption;
			Repairable component = this.item.GetComponent<Repairable>();
			if (component != null)
			{
				component.AdjustPowerConsumption(ref this.currPowerConsumption);
			}
			return this.currPowerConsumption;
		}

		// Token: 0x06005C2A RID: 23594 RVA: 0x002F658E File Offset: 0x002F478E
		public static float CalculateBonusRollPercentage(float skillLevel, float target)
		{
			return Math.Clamp((skillLevel - target) / (100f - target) * 100f, 0f, 100f);
		}

		// Token: 0x06005C2B RID: 23595 RVA: 0x002F65B0 File Offset: 0x002F47B0
		private static Fabricator.QualityResult GetFabricatedItemQuality(FabricationRecipe fabricatedItem, Character user)
		{
			if (((user != null) ? user.Info : null) == null)
			{
				return Fabricator.QualityResult.Empty;
			}
			ContentXElement childElement = fabricatedItem.TargetItem.ConfigElement.GetChildElement("Quality");
			ContentXElement contentXElement = null;
			if (childElement == contentXElement)
			{
				return Fabricator.QualityResult.Empty;
			}
			float floatQuality = 0f;
			floatQuality += user.GetStatValue(StatTypes.IncreaseFabricationQuality, false);
			foreach (Identifier tag in fabricatedItem.TargetItem.Tags)
			{
				floatQuality += user.Info.GetSavedStatValue(StatTypes.IncreaseFabricationQuality, tag);
			}
			if (!fabricatedItem.TargetItem.Tags.Contains(fabricatedItem.TargetItem.Identifier))
			{
				floatQuality += user.Info.GetSavedStatValue(StatTypes.IncreaseFabricationQuality, fabricatedItem.TargetItem.Identifier);
			}
			int quality = (int)floatQuality;
			Option.UnspecifiedNone none = Option.None;
			Option<float> plusOne = none;
			none = Option.None;
			Option<float> plusTwo = none;
			foreach (Skill skill in fabricatedItem.RequiredSkills)
			{
				float skillLevel = user.GetSkillLevel(skill.Identifier);
				if (skillLevel < 50f)
				{
					break;
				}
				float bonusChance = Fabricator.CalculateBonusRollPercentage(skillLevel, MathHelper.Lerp(skill.Level, 100f, 0.2f));
				plusOne = Fabricator.<GetFabricatedItemQuality>g__OverrideChanceIfLess|143_4(plusOne, bonusChance);
				if (skillLevel < 75f)
				{
					break;
				}
				float bonusChance2 = Fabricator.CalculateBonusRollPercentage(skillLevel, MathHelper.Lerp(skill.Level, 125f, 0.4f));
				plusTwo = Fabricator.<GetFabricatedItemQuality>g__OverrideChanceIfLess|143_4(plusTwo, bonusChance2);
			}
			bool hasRandomQuality = fabricatedItem.TargetItem.MaxStackSize <= 1;
			float PlusOnePercentage = plusOne.Match((float f) => f, () => 0f);
			float PlusTwoPercentage = plusTwo.Match((float f) => f, () => 0f);
			if (!hasRandomQuality && PlusOnePercentage > 0f)
			{
				quality++;
				if (PlusTwoPercentage > 0f)
				{
					quality++;
				}
			}
			return new Fabricator.QualityResult(quality, hasRandomQuality, PlusOnePercentage, PlusTwoPercentage);
		}

		// Token: 0x06005C2C RID: 23596 RVA: 0x002F6824 File Offset: 0x002F4A24
		private void UpdateRequiredTimeProjSpecific()
		{
			if (this.requiredTimeBlock == null)
			{
				return;
			}
			this.requiredTimeBlock.Text = ToolBox.SecondsToReadableTime((this.timeUntilReady > 0f) ? this.timeUntilReady : this.requiredTime);
		}

		// Token: 0x06005C2D RID: 23597 RVA: 0x002F6860 File Offset: 0x002F4A60
		private static bool AnyOneHasRecipeForItem(Character user, ItemPrefab item)
		{
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			CharacterType mustHaveRecipe = (gameMode != null && gameMode.IsSinglePlayer) ? CharacterType.Both : CharacterType.Bot;
			return (user != null && user.HasRecipeForItem(item.Identifier)) || GameSession.GetSessionCrewCharacters(mustHaveRecipe).Any((Character c) => c.HasRecipeForItem(item.Identifier));
		}

		// Token: 0x06005C2E RID: 23598 RVA: 0x002F68CB File Offset: 0x002F4ACB
		public bool MissingRequiredRecipe(FabricationRecipe fabricableItem, Character character)
		{
			if (fabricableItem.RequiresRecipe)
			{
				if (character == null)
				{
					return false;
				}
				if (!Fabricator.AnyOneHasRecipeForItem(character, fabricableItem.TargetItem))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06005C2F RID: 23599 RVA: 0x002F68EC File Offset: 0x002F4AEC
		private bool CanBeFabricated(FabricationRecipe fabricableItem, IReadOnlyDictionary<Identifier, List<Item>> availableIngredients, Character character)
		{
			if (fabricableItem == null)
			{
				return false;
			}
			if (this.MissingRequiredRecipe(fabricableItem, character))
			{
				return false;
			}
			if (fabricableItem.HideForNonTraitors && (character == null || !character.IsTraitor))
			{
				return false;
			}
			if (fabricableItem.RequiredMoney > 0)
			{
				GameSession gameSession = GameMain.GameSession;
				GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
				MultiPlayerCampaign mpCampaign = gameMode as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					CampaignMode campaign = gameMode as CampaignMode;
					if (campaign == null)
					{
						return false;
					}
					if (campaign.Bank.Balance < fabricableItem.RequiredMoney)
					{
						return false;
					}
				}
				else if (!mpCampaign.CanAfford(fabricableItem.RequiredMoney, this.GetUsingClient()))
				{
					return false;
				}
			}
			int amount;
			if (this.fabricationLimits.TryGetValue(fabricableItem.RecipeHash, out amount) && amount <= 0)
			{
				return false;
			}
			this.usedIngredients.Clear();
			this.ingredientFlexibilityCache.Clear();
			using (IEnumerator<ItemPrefab> enumerator = fabricableItem.RequiredItems.SelectMany((FabricationRecipe.RequiredItem r) => r.ItemPrefabs).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemPrefab prefab = enumerator.Current;
					this.ingredientFlexibilityCache[prefab] = fabricableItem.RequiredItems.Count((FabricationRecipe.RequiredItem r) => r.ItemPrefabs.Contains(prefab));
				}
			}
			return (from r in fabricableItem.RequiredItems
			orderby r.ItemPrefabs.Count<ItemPrefab>()
			select r).ThenByDescending((FabricationRecipe.RequiredItem requiredItem) => requiredItem.Amount).All(delegate(FabricationRecipe.RequiredItem requiredItem)
			{
				int availableItemsAmount = 0;
				foreach (ItemPrefab requiredPrefab in requiredItem.ItemPrefabs.OrderBy(new Func<ItemPrefab, int>(base.<CanBeFabricated>g__GetItemFlexibility|4)).ThenByDescending(new Func<ItemPrefab, int>(base.<CanBeFabricated>g__GetAvailableItemsCount|3)))
				{
					List<Item> availableItems;
					if (availableIngredients.TryGetValue(requiredPrefab.Identifier, out availableItems))
					{
						foreach (Item availableItem in availableItems)
						{
							if (!this.usedIngredients.Contains(availableItem))
							{
								if (requiredItem.IsConditionSuitable(availableItem.ConditionPercentage))
								{
									this.usedIngredients.Add(availableItem);
									availableItemsAmount++;
								}
								if (availableItemsAmount >= requiredItem.Amount)
								{
									return true;
								}
							}
						}
					}
				}
				return false;
			});
		}

		// Token: 0x06005C30 RID: 23600 RVA: 0x002F6ACC File Offset: 0x002F4CCC
		private float GetRequiredTime(FabricationRecipe fabricableItem, Character user)
		{
			float degreeOfSuccess = this.FabricationDegreeOfSuccess(user, fabricableItem.RequiredSkills);
			float t = (degreeOfSuccess < 0.5f) ? (degreeOfSuccess * degreeOfSuccess) : (degreeOfSuccess * 2f);
			float time = fabricableItem.RequiredTime / this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.FabricationSpeed, this.FabricationSpeed) / MathHelper.Clamp(t, 0.01f, 2f);
			CharacterInfo info = (user != null) ? user.Info : null;
			if (info != null)
			{
				ItemPrefab it = fabricableItem.TargetItem;
				if (it != null)
				{
					time /= 1f + it.Tags.Sum((Identifier tag) => info.GetSavedStatValue(StatTypes.FabricationSpeed, tag));
				}
			}
			return time;
		}

		// Token: 0x06005C31 RID: 23601 RVA: 0x002F6B7C File Offset: 0x002F4D7C
		public float FabricationDegreeOfSuccess(Character character, ImmutableArray<Skill> skills)
		{
			if (skills.Length == 0)
			{
				return 0.5f;
			}
			if (character == null)
			{
				return 0f;
			}
			float minDegreeOfSuccess = 1f;
			foreach (Skill skill in skills)
			{
				float characterLevel = character.GetSkillLevel(skill.Identifier);
				minDegreeOfSuccess = Math.Min(minDegreeOfSuccess, (characterLevel - skill.Level * this.SkillRequirementMultiplier + 100f) / 2f / 100f);
			}
			return minDegreeOfSuccess;
		}

		// Token: 0x06005C32 RID: 23602 RVA: 0x002F6BF8 File Offset: 0x002F4DF8
		public override float GetSkillMultiplier()
		{
			return this.SkillRequirementMultiplier;
		}

		// Token: 0x06005C33 RID: 23603 RVA: 0x002F6C00 File Offset: 0x002F4E00
		private void RefreshAvailableIngredients()
		{
			Character user = this.user;
			if (user == null)
			{
				user = Character.Controlled;
			}
			this.linkedInventories.Clear();
			List<Item> itemList = new List<Item>();
			itemList.AddRange(this.inputContainer.Inventory.AllItems);
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item linkedItem = linkedTo as Item;
				if (linkedItem != null)
				{
					ItemContainer itemContainer = linkedItem.GetComponent<ItemContainer>();
					if (itemContainer != null && (user == null || itemContainer.HasRequiredItems(user, false, null)))
					{
						Deconstructor deconstructor = linkedItem.GetComponent<Deconstructor>();
						if (deconstructor != null)
						{
							itemContainer = deconstructor.OutputContainer;
						}
						this.linkedInventories.Add(itemContainer.Inventory);
						itemList.AddRange(itemContainer.Inventory.AllItems);
					}
				}
			}
			for (int i = 0; i < itemList.Count; i++)
			{
				ItemContainer container = itemList[i].GetComponent<ItemContainer>();
				if (container != null)
				{
					itemList.AddRange(container.Inventory.AllItems);
				}
			}
			if (((user != null) ? user.Inventory : null) != null && user.SelectedItem == this.item)
			{
				itemList.AddRange(user.Inventory.AllItems);
				this.linkedInventories.Add(user.Inventory);
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.SelectedItem != null && c.Inventory != null && this.linkedInventories.Contains(c.SelectedItem.OwnInventory) && !this.linkedInventories.Contains(c.Inventory))
				{
					itemList.AddRange(c.Inventory.AllItems);
					this.linkedInventories.Add(c.Inventory);
				}
			}
			this.availableIngredients.Clear();
			foreach (Item item in itemList)
			{
				Identifier itemIdentifier = item.Prefab.Identifier;
				if (!this.availableIngredients.ContainsKey(itemIdentifier))
				{
					this.availableIngredients[itemIdentifier] = new List<Item>(itemList.Count);
				}
				this.availableIngredients[itemIdentifier].Add(item);
			}
			foreach (Identifier itemId in this.availableIngredients.Keys)
			{
				this.availableIngredients[itemId] = this.SortIngredients(this.availableIngredients[itemId]).ToList<Item>();
			}
		}

		// Token: 0x06005C34 RID: 23604 RVA: 0x002F6EF8 File Offset: 0x002F50F8
		private IEnumerable<Item> SortIngredients(IEnumerable<Item> items)
		{
			return items.OrderByDescending(new Func<Item, int>(this.<SortIngredients>g__getIngredientContainerPriority|155_3)).ThenBy(delegate(Item it)
			{
				PriceInfo defaultPrice = it.Prefab.DefaultPrice;
				if (defaultPrice == null)
				{
					return 0;
				}
				return defaultPrice.Price;
			}).ThenBy(delegate(Item it)
			{
				if (!MathUtils.IsValid(it.Condition))
				{
					return 0f;
				}
				return it.Condition;
			}).ThenByDescending(delegate(Item it)
			{
				Inventory parentInventory = it.ParentInventory;
				if (parentInventory == null)
				{
					return 0;
				}
				return parentInventory.FindIndex(it);
			});
		}

		// Token: 0x06005C35 RID: 23605 RVA: 0x002F6F84 File Offset: 0x002F5184
		private IEnumerable<Item> GetSortedSuitableIngredients()
		{
			List<Item> suitableIngredients = new List<Item>();
			ImmutableArray<FabricationRecipe.RequiredItem>.Enumerator enumerator = this.fabricatedItem.RequiredItems.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FabricationRecipe.RequiredItem requiredItem = enumerator.Current;
				Func<Item, bool> <>9__0;
				foreach (ItemPrefab requiredPrefab in requiredItem.ItemPrefabs)
				{
					if (this.availableIngredients.ContainsKey(requiredPrefab.Identifier))
					{
						List<Item> availableItems = this.availableIngredients[requiredPrefab.Identifier];
						List<Item> list = suitableIngredients;
						IEnumerable<Item> source = availableItems;
						Func<Item, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Item potentialItem) => requiredItem.IsConditionSuitable(potentialItem.ConditionPercentage)));
						}
						list.AddRange(source.Where(predicate));
					}
				}
			}
			return this.SortIngredients(suitableIngredients);
		}

		// Token: 0x06005C36 RID: 23606 RVA: 0x002F7068 File Offset: 0x002F5268
		private void MoveIngredientsToInputContainer(FabricationRecipe targetItem)
		{
			List<Item> chosenIngredients = new List<Item>();
			IEnumerable<Item> suitableIngredients = this.GetSortedSuitableIngredients();
			Func<Item, bool> <>9__0;
			foreach (FabricationRecipe.RequiredItem requiredItem in targetItem.RequiredItems)
			{
				for (int i = 0; i < requiredItem.Amount; i++)
				{
					foreach (Item suitableIngredient in suitableIngredients)
					{
						if (requiredItem.MatchesItem(suitableIngredient) && !chosenIngredients.Contains(suitableIngredient))
						{
							if (suitableIngredient.ParentInventory != this.inputContainer.Inventory)
							{
								if (!this.inputContainer.Inventory.CanBePut(suitableIngredient))
								{
									IEnumerable<Item> allItems = this.inputContainer.Inventory.AllItems;
									Func<Item, bool> predicate;
									if ((predicate = <>9__0) == null)
									{
										predicate = (<>9__0 = ((Item it) => !chosenIngredients.Contains(it)));
									}
									Item unneededItem = allItems.FirstOrDefault(predicate);
									if (unneededItem != null)
									{
										unneededItem.Drop(null, true, true);
									}
								}
								this.inputContainer.Inventory.TryPutItem(suitableIngredient, null, null, true, false, true);
							}
							chosenIngredients.Add(suitableIngredient);
							break;
						}
					}
				}
			}
			this.RefreshAvailableIngredients();
		}

		// Token: 0x06005C37 RID: 23607 RVA: 0x002F71C8 File Offset: 0x002F53C8
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			if (this.fabricatedItem != null)
			{
				componentElement.Add(new XAttribute("fabricateditemidentifier", this.fabricatedItem.TargetItem.Identifier));
				componentElement.Add(new XAttribute("savedtimeuntilready", this.timeUntilReady.ToString("G", CultureInfo.InvariantCulture)));
				componentElement.Add(new XAttribute("savedrequiredtime", this.requiredTime.ToString("G", CultureInfo.InvariantCulture)));
			}
			return componentElement;
		}

		// Token: 0x06005C38 RID: 23608 RVA: 0x002F7264 File Offset: 0x002F5464
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.savedFabricatedItem = componentElement.GetAttributeString("fabricateditemidentifier", "");
			this.savedTimeUntilReady = componentElement.GetAttributeFloat("savedtimeuntilready", 0f);
			this.savedRequiredTime = componentElement.GetAttributeFloat("savedrequiredtime", 0f);
		}

		// Token: 0x06005C39 RID: 23609 RVA: 0x002F72C0 File Offset: 0x002F54C0
		public override void OnMapLoaded()
		{
			if (string.IsNullOrEmpty(this.savedFabricatedItem))
			{
				return;
			}
			ItemContainer itemContainer = this.inputContainer;
			if (itemContainer != null)
			{
				itemContainer.OnMapLoaded();
			}
			ItemContainer itemContainer2 = this.outputContainer;
			if (itemContainer2 != null)
			{
				itemContainer2.OnMapLoaded();
			}
			FabricationRecipe recipe = this.fabricationRecipes.Values.FirstOrDefault((FabricationRecipe r) => r.TargetItem.Identifier == this.savedFabricatedItem);
			if (recipe == null)
			{
				DebugConsole.ThrowError("Error while loading a fabricator. Can't continue fabricating \"" + this.savedFabricatedItem + "\" (matching recipe not found).", null, null, false, false);
			}
			else
			{
				this.SelectItem(null, recipe, new float?(this.savedRequiredTime));
				this.StartFabricating(recipe, null, true);
				this.timeUntilReady = this.savedTimeUntilReady;
				this.requiredTime = this.savedRequiredTime;
			}
			this.savedFabricatedItem = null;
		}

		// Token: 0x06005C3A RID: 23610 RVA: 0x002F7379 File Offset: 0x002F5579
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.OnItemFabricated = null;
		}

		// Token: 0x06005C3C RID: 23612 RVA: 0x002F73BC File Offset: 0x002F55BC
		[CompilerGenerated]
		private bool <CreateGUI>g__OnClickedCategoryButton|55_3(GUIButton button, object userData)
		{
			MapEntityCategory? newCategory = (!button.Selected) ? ((MapEntityCategory?)userData) : null;
			if (newCategory != null)
			{
				this.itemFilterBox.Text = "";
			}
			this.selectedItemCategory = newCategory;
			this.FilterEntities(newCategory, this.itemFilterBox.Text);
			return true;
		}

		// Token: 0x06005C42 RID: 23618 RVA: 0x002F7510 File Offset: 0x002F5710
		[CompilerGenerated]
		internal static Option<float> <GetFabricatedItemQuality>g__OverrideChanceIfLess|143_4(Option<float> original, float bonusChance)
		{
			float originalChance;
			if (!original.TryUnwrap(out originalChance))
			{
				return Option.Some<float>(bonusChance);
			}
			if (originalChance <= bonusChance)
			{
				return original;
			}
			return Option.Some<float>(bonusChance);
		}

		// Token: 0x06005C43 RID: 23619 RVA: 0x002F753B File Offset: 0x002F573B
		[CompilerGenerated]
		private int <SortIngredients>g__getIngredientContainerPriority|155_3(Item item)
		{
			if (item.ParentInventory == this.InputContainer.Inventory)
			{
				return 3;
			}
			if (item.ParentInventory is CharacterInventory)
			{
				return 2;
			}
			return 1;
		}

		// Token: 0x04002ED9 RID: 11993
		private GUIListBox itemList;

		// Token: 0x04002EDA RID: 11994
		private GUIFrame selectedItemFrame;

		// Token: 0x04002EDB RID: 11995
		private GUIFrame selectedItemReqsFrame;

		// Token: 0x04002EDC RID: 11996
		private GUILayoutGroup outputTopArea;

		// Token: 0x04002EDD RID: 11997
		private GUILayoutGroup paddedOutputArea;

		// Token: 0x04002EDE RID: 11998
		private GUITextBlock amountTextMax;

		// Token: 0x04002EDF RID: 11999
		private GUIScrollBar amountInput;

		// Token: 0x04002EE0 RID: 12000
		private GUIButton activateButton;

		// Token: 0x04002EE1 RID: 12001
		private GUITextBox itemFilterBox;

		// Token: 0x04002EE2 RID: 12002
		private GUITickBox availableOnlyTickBox;

		// Token: 0x04002EE3 RID: 12003
		private GUIDropDown sortByDropdown;

		// Token: 0x04002EE4 RID: 12004
		private GUIComponent outputSlot;

		// Token: 0x04002EE5 RID: 12005
		private GUIComponent inputInventoryHolder;

		// Token: 0x04002EE6 RID: 12006
		private GUIComponent outputInventoryHolder;

		// Token: 0x04002EE7 RID: 12007
		private readonly List<GUIButton> itemCategoryButtons = new List<GUIButton>();

		// Token: 0x04002EE8 RID: 12008
		private MapEntityCategory? selectedItemCategory;

		// Token: 0x04002EE9 RID: 12009
		private GUITextBlock requiresRecipeText;

		// Token: 0x04002EEA RID: 12010
		private GUITextBlock nothingToShowText;

		// Token: 0x04002EEB RID: 12011
		private FabricationRecipe selectedItem;

		// Token: 0x04002EEC RID: 12012
		private Character displayingForCharacter;

		// Token: 0x04002EED RID: 12013
		private GUIComponent inSufficientPowerWarning;

		// Token: 0x04002EEE RID: 12014
		private FabricationRecipe pendingFabricatedItem;

		// Token: 0x04002EEF RID: 12015
		private Fabricator.ToolTip tooltip;

		// Token: 0x04002EF0 RID: 12016
		private GUITextBlock requiredTimeBlock;

		// Token: 0x04002EF6 RID: 12022
		private readonly Dictionary<FabricationRecipe.RequiredItem, int> missingIngredientCounts = new Dictionary<FabricationRecipe.RequiredItem, int>();

		// Token: 0x04002EF7 RID: 12023
		private float ingredientHighlightTimer;

		// Token: 0x04002EF8 RID: 12024
		private Option<Fabricator.SelectedRecipe> LastSelectedRecipe;

		// Token: 0x04002EF9 RID: 12025
		private ImmutableDictionary<uint, FabricationRecipe> fabricationRecipes;

		// Token: 0x04002EFA RID: 12026
		private const int MaxAmountToFabricate = 99;

		// Token: 0x04002EFB RID: 12027
		private FabricationRecipe fabricatedItem;

		// Token: 0x04002EFC RID: 12028
		private float timeUntilReady;

		// Token: 0x04002EFD RID: 12029
		private float requiredTime;

		// Token: 0x04002EFE RID: 12030
		private string savedFabricatedItem;

		// Token: 0x04002EFF RID: 12031
		private float savedTimeUntilReady;

		// Token: 0x04002F00 RID: 12032
		private float savedRequiredTime;

		// Token: 0x04002F01 RID: 12033
		private readonly Dictionary<Identifier, List<Item>> availableIngredients;

		// Token: 0x04002F02 RID: 12034
		private const float RefreshIngredientsInterval = 1f;

		// Token: 0x04002F03 RID: 12035
		private float refreshIngredientsTimer;

		// Token: 0x04002F04 RID: 12036
		private bool hasPower;

		// Token: 0x04002F05 RID: 12037
		private Character user;

		// Token: 0x04002F06 RID: 12038
		private ItemContainer inputContainer;

		// Token: 0x04002F07 RID: 12039
		private ItemContainer outputContainer;

		// Token: 0x04002F0A RID: 12042
		private int amountToFabricate;

		// Token: 0x04002F0B RID: 12043
		private int amountRemaining;

		// Token: 0x04002F0C RID: 12044
		private const float TinkeringSpeedIncrease = 2.5f;

		// Token: 0x04002F0D RID: 12045
		private Fabricator.FabricatorState state;

		// Token: 0x04002F0E RID: 12046
		private float progressState;

		// Token: 0x04002F0F RID: 12047
		private readonly Dictionary<uint, int> fabricationLimits;

		// Token: 0x04002F10 RID: 12048
		public Action<Item, Character> OnItemFabricated;

		// Token: 0x04002F11 RID: 12049
		public const int PlusOneQualityBonusThreshold = 50;

		// Token: 0x04002F12 RID: 12050
		public const int PlusTwoQualityBonusThreshold = 75;

		// Token: 0x04002F13 RID: 12051
		public const int PlusOneTarget = 100;

		// Token: 0x04002F14 RID: 12052
		public const int PlusTwoTarget = 125;

		// Token: 0x04002F15 RID: 12053
		public const float PlusOneLerp = 0.2f;

		// Token: 0x04002F16 RID: 12054
		public const float PlusTwoLerp = 0.4f;

		// Token: 0x04002F17 RID: 12055
		private readonly HashSet<Item> usedIngredients;

		// Token: 0x04002F18 RID: 12056
		private readonly Dictionary<ItemPrefab, int> ingredientFlexibilityCache;

		// Token: 0x04002F19 RID: 12057
		private readonly HashSet<Inventory> linkedInventories;

		// Token: 0x020013E8 RID: 5096
		private enum SortBy
		{
			// Token: 0x040063D0 RID: 25552
			Category,
			// Token: 0x040063D1 RID: 25553
			Alphabetical,
			// Token: 0x040063D2 RID: 25554
			SkillRequirement,
			// Token: 0x040063D3 RID: 25555
			Price
		}

		// Token: 0x020013E9 RID: 5097
		private class ToolTip
		{
			// Token: 0x040063D4 RID: 25556
			public Rectangle TargetElement;

			// Token: 0x040063D5 RID: 25557
			public LocalizedString Tooltip;
		}

		// Token: 0x020013EA RID: 5098
		private readonly struct SelectedRecipe : IEquatable<Fabricator.SelectedRecipe>
		{
			// Token: 0x060098F9 RID: 39161 RVA: 0x003DF246 File Offset: 0x003DD446
			public SelectedRecipe(Character User, FabricationRecipe SelectedItem, Option<float> OverrideRequiredTime)
			{
				this.User = User;
				this.SelectedItem = SelectedItem;
				this.OverrideRequiredTime = OverrideRequiredTime;
			}

			// Token: 0x17001D34 RID: 7476
			// (get) Token: 0x060098FA RID: 39162 RVA: 0x003DF25D File Offset: 0x003DD45D
			// (set) Token: 0x060098FB RID: 39163 RVA: 0x003DF265 File Offset: 0x003DD465
			public Character User { get; set; }

			// Token: 0x17001D35 RID: 7477
			// (get) Token: 0x060098FC RID: 39164 RVA: 0x003DF26E File Offset: 0x003DD46E
			// (set) Token: 0x060098FD RID: 39165 RVA: 0x003DF276 File Offset: 0x003DD476
			public FabricationRecipe SelectedItem { get; set; }

			// Token: 0x17001D36 RID: 7478
			// (get) Token: 0x060098FE RID: 39166 RVA: 0x003DF27F File Offset: 0x003DD47F
			// (set) Token: 0x060098FF RID: 39167 RVA: 0x003DF287 File Offset: 0x003DD487
			public Option<float> OverrideRequiredTime { get; set; }

			// Token: 0x06009900 RID: 39168 RVA: 0x003DF290 File Offset: 0x003DD490
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SelectedRecipe");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009901 RID: 39169 RVA: 0x003DF2DC File Offset: 0x003DD4DC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("User = ");
				builder.Append(this.User);
				builder.Append(", SelectedItem = ");
				builder.Append(this.SelectedItem);
				builder.Append(", OverrideRequiredTime = ");
				builder.Append(this.OverrideRequiredTime.ToString());
				return true;
			}

			// Token: 0x06009902 RID: 39170 RVA: 0x003DF343 File Offset: 0x003DD543
			[CompilerGenerated]
			public static bool operator !=(Fabricator.SelectedRecipe left, Fabricator.SelectedRecipe right)
			{
				return !(left == right);
			}

			// Token: 0x06009903 RID: 39171 RVA: 0x003DF34F File Offset: 0x003DD54F
			[CompilerGenerated]
			public static bool operator ==(Fabricator.SelectedRecipe left, Fabricator.SelectedRecipe right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009904 RID: 39172 RVA: 0x003DF359 File Offset: 0x003DD559
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Character>.Default.GetHashCode(this.<User>k__BackingField) * -1521134295 + EqualityComparer<FabricationRecipe>.Default.GetHashCode(this.<SelectedItem>k__BackingField)) * -1521134295 + EqualityComparer<Option<float>>.Default.GetHashCode(this.<OverrideRequiredTime>k__BackingField);
			}

			// Token: 0x06009905 RID: 39173 RVA: 0x003DF399 File Offset: 0x003DD599
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Fabricator.SelectedRecipe && this.Equals((Fabricator.SelectedRecipe)obj);
			}

			// Token: 0x06009906 RID: 39174 RVA: 0x003DF3B4 File Offset: 0x003DD5B4
			[CompilerGenerated]
			public bool Equals(Fabricator.SelectedRecipe other)
			{
				return EqualityComparer<Character>.Default.Equals(this.<User>k__BackingField, other.<User>k__BackingField) && EqualityComparer<FabricationRecipe>.Default.Equals(this.<SelectedItem>k__BackingField, other.<SelectedItem>k__BackingField) && EqualityComparer<Option<float>>.Default.Equals(this.<OverrideRequiredTime>k__BackingField, other.<OverrideRequiredTime>k__BackingField);
			}

			// Token: 0x06009907 RID: 39175 RVA: 0x003DF409 File Offset: 0x003DD609
			[CompilerGenerated]
			public void Deconstruct(out Character User, out FabricationRecipe SelectedItem, out Option<float> OverrideRequiredTime)
			{
				User = this.User;
				SelectedItem = this.SelectedItem;
				OverrideRequiredTime = this.OverrideRequiredTime;
			}
		}

		// Token: 0x020013EB RID: 5099
		private enum FabricatorState
		{
			// Token: 0x040063DA RID: 25562
			Active = 1,
			// Token: 0x040063DB RID: 25563
			Paused,
			// Token: 0x040063DC RID: 25564
			Stopped = 0
		}

		// Token: 0x020013EC RID: 5100
		public readonly struct QualityResult : IEquatable<Fabricator.QualityResult>
		{
			// Token: 0x06009908 RID: 39176 RVA: 0x003DF427 File Offset: 0x003DD627
			public QualityResult(int Quality, bool HasRandomQuality, float PlusOnePercentage, float PlusTwoPercentage)
			{
				this.Quality = Quality;
				this.HasRandomQuality = HasRandomQuality;
				this.PlusOnePercentage = PlusOnePercentage;
				this.PlusTwoPercentage = PlusTwoPercentage;
			}

			// Token: 0x17001D37 RID: 7479
			// (get) Token: 0x06009909 RID: 39177 RVA: 0x003DF446 File Offset: 0x003DD646
			// (set) Token: 0x0600990A RID: 39178 RVA: 0x003DF44E File Offset: 0x003DD64E
			public int Quality { get; set; }

			// Token: 0x17001D38 RID: 7480
			// (get) Token: 0x0600990B RID: 39179 RVA: 0x003DF457 File Offset: 0x003DD657
			// (set) Token: 0x0600990C RID: 39180 RVA: 0x003DF45F File Offset: 0x003DD65F
			public bool HasRandomQuality { get; set; }

			// Token: 0x17001D39 RID: 7481
			// (get) Token: 0x0600990D RID: 39181 RVA: 0x003DF468 File Offset: 0x003DD668
			// (set) Token: 0x0600990E RID: 39182 RVA: 0x003DF470 File Offset: 0x003DD670
			public float PlusOnePercentage { get; set; }

			// Token: 0x17001D3A RID: 7482
			// (get) Token: 0x0600990F RID: 39183 RVA: 0x003DF479 File Offset: 0x003DD679
			// (set) Token: 0x06009910 RID: 39184 RVA: 0x003DF481 File Offset: 0x003DD681
			public float PlusTwoPercentage { get; set; }

			// Token: 0x17001D3B RID: 7483
			// (get) Token: 0x06009911 RID: 39185 RVA: 0x003DF48A File Offset: 0x003DD68A
			public bool HasRandomQualityRollChance
			{
				get
				{
					return this.HasRandomQuality && (this.PlusOnePercentage > 0f || this.PlusTwoPercentage > 0f);
				}
			}

			// Token: 0x17001D3C RID: 7484
			// (get) Token: 0x06009912 RID: 39186 RVA: 0x003DF4B2 File Offset: 0x003DD6B2
			public float TotalPlusOnePercentage
			{
				get
				{
					return Math.Clamp(this.PlusOnePercentage * (100f - this.PlusTwoPercentage) / 100f, 0f, 100f);
				}
			}

			// Token: 0x17001D3D RID: 7485
			// (get) Token: 0x06009913 RID: 39187 RVA: 0x003DF4DC File Offset: 0x003DD6DC
			public float TotalPlusTwoPercentage
			{
				get
				{
					return Math.Clamp(this.PlusOnePercentage * this.PlusTwoPercentage / 100f, 0f, 100f);
				}
			}

			// Token: 0x06009914 RID: 39188 RVA: 0x003DF500 File Offset: 0x003DD700
			public int RollQuality()
			{
				int additionalQuality = 0;
				if (Fabricator.QualityResult.<RollQuality>g__Roll|24_0(this.PlusOnePercentage))
				{
					additionalQuality++;
					if (Fabricator.QualityResult.<RollQuality>g__Roll|24_0(this.PlusTwoPercentage))
					{
						additionalQuality++;
					}
				}
				return this.Quality + additionalQuality;
			}

			// Token: 0x06009915 RID: 39189 RVA: 0x003DF53C File Offset: 0x003DD73C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("QualityResult");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009916 RID: 39190 RVA: 0x003DF588 File Offset: 0x003DD788
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Quality = ");
				builder.Append(this.Quality.ToString());
				builder.Append(", HasRandomQuality = ");
				builder.Append(this.HasRandomQuality.ToString());
				builder.Append(", PlusOnePercentage = ");
				builder.Append(this.PlusOnePercentage.ToString());
				builder.Append(", PlusTwoPercentage = ");
				builder.Append(this.PlusTwoPercentage.ToString());
				builder.Append(", HasRandomQualityRollChance = ");
				builder.Append(this.HasRandomQualityRollChance.ToString());
				builder.Append(", TotalPlusOnePercentage = ");
				builder.Append(this.TotalPlusOnePercentage.ToString());
				builder.Append(", TotalPlusTwoPercentage = ");
				builder.Append(this.TotalPlusTwoPercentage.ToString());
				return true;
			}

			// Token: 0x06009917 RID: 39191 RVA: 0x003DF6A7 File Offset: 0x003DD8A7
			[CompilerGenerated]
			public static bool operator !=(Fabricator.QualityResult left, Fabricator.QualityResult right)
			{
				return !(left == right);
			}

			// Token: 0x06009918 RID: 39192 RVA: 0x003DF6B3 File Offset: 0x003DD8B3
			[CompilerGenerated]
			public static bool operator ==(Fabricator.QualityResult left, Fabricator.QualityResult right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009919 RID: 39193 RVA: 0x003DF6C0 File Offset: 0x003DD8C0
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<int>.Default.GetHashCode(this.<Quality>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<HasRandomQuality>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<PlusOnePercentage>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<PlusTwoPercentage>k__BackingField);
			}

			// Token: 0x0600991A RID: 39194 RVA: 0x003DF722 File Offset: 0x003DD922
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Fabricator.QualityResult && this.Equals((Fabricator.QualityResult)obj);
			}

			// Token: 0x0600991B RID: 39195 RVA: 0x003DF73C File Offset: 0x003DD93C
			[CompilerGenerated]
			public bool Equals(Fabricator.QualityResult other)
			{
				return EqualityComparer<int>.Default.Equals(this.<Quality>k__BackingField, other.<Quality>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<HasRandomQuality>k__BackingField, other.<HasRandomQuality>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<PlusOnePercentage>k__BackingField, other.<PlusOnePercentage>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<PlusTwoPercentage>k__BackingField, other.<PlusTwoPercentage>k__BackingField);
			}

			// Token: 0x0600991C RID: 39196 RVA: 0x003DF7A9 File Offset: 0x003DD9A9
			[CompilerGenerated]
			public void Deconstruct(out int Quality, out bool HasRandomQuality, out float PlusOnePercentage, out float PlusTwoPercentage)
			{
				Quality = this.Quality;
				HasRandomQuality = this.HasRandomQuality;
				PlusOnePercentage = this.PlusOnePercentage;
				PlusTwoPercentage = this.PlusTwoPercentage;
			}

			// Token: 0x0600991E RID: 39198 RVA: 0x003DF7E4 File Offset: 0x003DD9E4
			[CompilerGenerated]
			internal static bool <RollQuality>g__Roll|24_0(float percentage)
			{
				return percentage >= (float)Rand.Range(0, 100, Rand.RandSync.Unsynced);
			}

			// Token: 0x040063E1 RID: 25569
			public static readonly Fabricator.QualityResult Empty = new Fabricator.QualityResult(0, true, 0f, 0f);
		}

		// Token: 0x020013ED RID: 5101
		private class AbilityFabricatorSkillGain : AbilityObject, IAbilityValue, IAbilitySkillIdentifier
		{
			// Token: 0x0600991F RID: 39199 RVA: 0x003DF7F6 File Offset: 0x003DD9F6
			public AbilityFabricatorSkillGain(Identifier skillIdentifier, float skillAmount)
			{
				this.SkillIdentifier = skillIdentifier;
				this.Value = skillAmount;
			}

			// Token: 0x17001D3E RID: 7486
			// (get) Token: 0x06009920 RID: 39200 RVA: 0x003DF80C File Offset: 0x003DDA0C
			// (set) Token: 0x06009921 RID: 39201 RVA: 0x003DF814 File Offset: 0x003DDA14
			public float Value { get; set; }

			// Token: 0x17001D3F RID: 7487
			// (get) Token: 0x06009922 RID: 39202 RVA: 0x003DF81D File Offset: 0x003DDA1D
			// (set) Token: 0x06009923 RID: 39203 RVA: 0x003DF825 File Offset: 0x003DDA25
			public Identifier SkillIdentifier { get; set; }
		}

		// Token: 0x020013EE RID: 5102
		private class AbilityFabricationItemAmount : AbilityObject, IAbilityValue, IAbilityItemPrefab
		{
			// Token: 0x06009924 RID: 39204 RVA: 0x003DF82E File Offset: 0x003DDA2E
			public AbilityFabricationItemAmount(ItemPrefab itemPrefab, float itemAmount)
			{
				this.ItemPrefab = itemPrefab;
				this.Value = itemAmount;
			}

			// Token: 0x17001D40 RID: 7488
			// (get) Token: 0x06009925 RID: 39205 RVA: 0x003DF844 File Offset: 0x003DDA44
			// (set) Token: 0x06009926 RID: 39206 RVA: 0x003DF84C File Offset: 0x003DDA4C
			public float Value { get; set; }

			// Token: 0x17001D41 RID: 7489
			// (get) Token: 0x06009927 RID: 39207 RVA: 0x003DF855 File Offset: 0x003DDA55
			// (set) Token: 0x06009928 RID: 39208 RVA: 0x003DF85D File Offset: 0x003DDA5D
			public ItemPrefab ItemPrefab { get; set; }
		}

		// Token: 0x020013EF RID: 5103
		internal sealed class AbilityFabricationItemIngredients : AbilityObject
		{
			// Token: 0x17001D42 RID: 7490
			// (get) Token: 0x06009929 RID: 39209 RVA: 0x003DF866 File Offset: 0x003DDA66
			// (set) Token: 0x0600992A RID: 39210 RVA: 0x003DF86E File Offset: 0x003DDA6E
			public List<Item> Items { get; set; }

			// Token: 0x0600992B RID: 39211 RVA: 0x003DF877 File Offset: 0x003DDA77
			public AbilityFabricationItemIngredients(List<Item> items)
			{
				this.Items = items;
			}
		}
	}
}
