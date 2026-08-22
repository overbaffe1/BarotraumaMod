using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000BD RID: 189
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class UpgradeStore
	{
		// Token: 0x170005D8 RID: 1496
		// (get) Token: 0x06001764 RID: 5988 RVA: 0x000E3786 File Offset: 0x000E1986
		[Nullable(2)]
		private CampaignMode Campaign
		{
			[NullableContext(2)]
			get
			{
				return this.campaignUI.Campaign;
			}
		}

		// Token: 0x170005D9 RID: 1497
		// (get) Token: 0x06001765 RID: 5989 RVA: 0x000E3793 File Offset: 0x000E1993
		private int PlayerBalance
		{
			get
			{
				CampaignMode campaign = this.Campaign;
				if (campaign == null)
				{
					return 0;
				}
				return campaign.GetBalance(null);
			}
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x000E37A8 File Offset: 0x000E19A8
		public UpgradeStore(CampaignUI campaignUI, GUIComponent parent)
		{
			UpgradeStore.WaitForServerUpdate = false;
			UpgradeStore.characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			this.campaignUI = campaignUI;
			GUIFrame upgradeFrame = new GUIFrame(UpgradeStore.rectT(1f, 1f, parent, Anchor.Center, ScaleBasis.Normal), "OuterGlow", new Color?(Color.Black * 0.7f))
			{
				CanBeFocused = false,
				UserData = "outerglow"
			};
			Vector2 relativeSize = new Vector2(0.13f, 0.13f);
			RectTransform canvas = GUI.Canvas;
			Anchor anchor = Anchor.TopLeft;
			Point? minSize = new Point?(new Point(250, 150));
			this.ItemInfoFrame = new GUIFrame(new RectTransform(relativeSize, canvas, anchor, null, minSize, null, ScaleBasis.Normal), "GUIToolTip", null)
			{
				CanBeFocused = false
			};
			this.CreateUI(upgradeFrame);
			if (this.Campaign == null)
			{
				return;
			}
			Identifier eventId = new Identifier("UpgradeStore");
			NamedEvent<UpgradeManager> onUpgradesChanged = this.Campaign.UpgradeManager.OnUpgradesChanged;
			if (onUpgradesChanged != null)
			{
				onUpgradesChanged.RegisterOverwriteExisting(eventId, delegate(UpgradeManager _)
				{
					this.RequestRefresh(false);
				});
			}
			this.Campaign.CargoManager.OnPurchasedItemsChanged.RegisterOverwriteExisting(eventId, delegate(CargoManager _)
			{
				this.RequestRefresh(false);
			});
			this.Campaign.CargoManager.OnSoldItemsChanged.RegisterOverwriteExisting(eventId, delegate(CargoManager _)
			{
				this.RequestRefresh(false);
			});
			this.Campaign.OnMoneyChanged.RegisterOverwriteExisting(eventId, delegate(WalletChangedEvent _)
			{
				this.RequestRefresh(false);
			});
		}

		// Token: 0x06001767 RID: 5991 RVA: 0x000E3955 File Offset: 0x000E1B55
		public void RequestRefresh(bool refreshUpgrades = false)
		{
			this.needsRefresh = true;
			if (refreshUpgrades)
			{
				this.SelectTab(UpgradeStore.UpgradeTab.Upgrade);
			}
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x000E3968 File Offset: 0x000E1B68
		private void RefreshAll()
		{
			UpgradeStore.characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			UpgradeStore.UpgradeTab upgradeTab = this.selectedUpgradeTab;
			if (upgradeTab != UpgradeStore.UpgradeTab.Upgrade)
			{
				if (upgradeTab == UpgradeStore.UpgradeTab.Repairs)
				{
					this.SelectTab(UpgradeStore.UpgradeTab.Repairs);
				}
			}
			else
			{
				this.RefreshUpgradeList();
				foreach (KeyValuePair<Item, GUIComponent> itemPreview in this.itemPreviews)
				{
					GUIImage image = itemPreview.Value as GUIImage;
					if (image != null && itemPreview.Key != null)
					{
						if (itemPreview.Key.PendingItemSwap == null)
						{
							image.Sprite = itemPreview.Key.Prefab.UpgradePreviewSprite;
						}
						else if (itemPreview.Key.PendingItemSwap.UpgradePreviewSprite != null)
						{
							image.Sprite = itemPreview.Key.PendingItemSwap.UpgradePreviewSprite;
						}
					}
				}
			}
			this.needsRefresh = false;
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x000E3A54 File Offset: 0x000E1C54
		private void RefreshUpgradeList()
		{
			if (this.Campaign == null)
			{
				return;
			}
			GUIComponent guicomponent = this.selectedUpgradeCategoryLayout;
			if (((guicomponent != null) ? guicomponent.Parent : null) != null)
			{
				GUIListBox listBox = this.selectedUpgradeCategoryLayout.FindChild("prefablist", true) as GUIListBox;
				if (listBox != null)
				{
					foreach (GUIComponent component in listBox.Content.Children)
					{
						object obj = component.UserData;
						if (obj is UpgradeStore.CategoryData)
						{
							UpgradeStore.CategoryData data = (UpgradeStore.CategoryData)obj;
							UpgradePrefab prefab = data.SinglePrefab;
							if (prefab != null)
							{
								UpgradeStore.UpdateUpgradeEntry(component, prefab, data.Category, this.Campaign);
							}
						}
					}
					if (this.customizeTabOpen && this.selectedUpgradeCategoryLayout != null && Submarine.MainSub != null && this.currentUpgradeCategory != null)
					{
						this.CreateSwappableItemList(listBox, this.currentUpgradeCategory, Submarine.MainSub);
						GUIButton guibutton = this.activeItemSwapSlideDown;
						object obj = (guibutton != null) ? guibutton.UserData : null;
						Item prevOpenedItem = obj as Item;
						if (prevOpenedItem != null)
						{
							GUIButton currentButton = listBox.FindChild((GUIComponent c) => c.UserData as Item == prevOpenedItem, true) as GUIButton;
							if (currentButton != null)
							{
								currentButton.OnClicked(currentButton, prevOpenedItem);
							}
						}
					}
				}
			}
			GUIListBox guilistBox = this.currentStoreLayout;
			if (((guilistBox != null) ? guilistBox.Parent : null) != null)
			{
				UpgradeStore.UpdateCategoryList(this.currentStoreLayout, this.Campaign, this.drawnSubmarine, this.applicableCategories);
			}
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x000E3BE8 File Offset: 0x000E1DE8
		public static void UpdateCategoryList(GUIListBox categoryList, CampaignMode campaign, [Nullable(2)] Submarine drawnSubmarine, IEnumerable<UpgradeCategory> applicableCategories)
		{
			List<Item> subItems = UpgradeStore.GetSubItems();
			foreach (GUIComponent component3 in categoryList.Content.Children)
			{
				object userData = component3.UserData;
				if (userData is UpgradeStore.CategoryData)
				{
					UpgradeStore.CategoryData data = (UpgradeStore.CategoryData)userData;
					GUIComponent indicators = component3.FindChild("indicators", true);
					if (indicators != null && data.Prefabs != null)
					{
						UpgradeStore.UpdateCategoryIndicators(indicators, component3, data.Prefabs, data.Category, campaign, drawnSubmarine, applicableCategories);
					}
					GUIComponent customizeButton = component3.FindChild("customizebutton", true);
					if (customizeButton != null)
					{
						customizeButton.Visible = UpgradeStore.HasSwappableItems(data.Category, subItems);
					}
				}
			}
			using (IEnumerator<UpgradeCategory> enumerator2 = (from c in UpgradeCategory.Categories
			orderby c.Name
			select c).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					UpgradeCategory category = enumerator2.Current;
					GUIComponent component2 = categoryList.Content.FindChild(delegate(GUIComponent c)
					{
						object userData2 = c.UserData;
						if (userData2 is UpgradeStore.CategoryData)
						{
							UpgradeStore.CategoryData categoryData = (UpgradeStore.CategoryData)userData2;
							return categoryData.Category == category;
						}
						return false;
					}, false);
					if (component2 != null)
					{
						component2.SetAsLastChild();
					}
				}
			}
			List<GUIComponent> lastChilds = (from component in categoryList.Content.Children
			where !component.Enabled
			select component).ToList<GUIComponent>();
			foreach (GUIComponent lastChild in lastChilds)
			{
				lastChild.SetAsLastChild();
			}
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x000E3DB4 File Offset: 0x000E1FB4
		private void CreateUI(GUIComponent parent)
		{
			this.selectedUpgradeTab = UpgradeStore.UpgradeTab.Upgrade;
			parent.ClearChildren();
			this.ItemInfoFrame.ClearChildren();
			GUILayoutGroup tooltipLayout = new GUILayoutGroup(UpgradeStore.rectT(0.95f, 0.95f, this.ItemInfoFrame, Anchor.Center, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = UpgradeStore.rectT(1f, 0f, tooltipLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text = string.Empty;
			GUIFont font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).UserData = "itemname";
			new GUITextBlock(UpgradeStore.rectT(1f, 0f, tooltipLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("UpgradeUITooltip.UpgradeListHeader"), null, null, Alignment.Left, false, "", null);
			GUIListBox guilistBox = new GUIListBox(UpgradeStore.rectT(1f, 0.5f, tooltipLayout, Anchor.TopLeft, ScaleBasis.Normal), false, null, null, true, false);
			guilistBox.ScrollBarVisible = false;
			guilistBox.AutoHideScrollBar = false;
			guilistBox.SmoothScroll = true;
			guilistBox.UserData = "upgradelist";
			new GUITextBlock(UpgradeStore.rectT(1f, 0f, tooltipLayout, Anchor.TopLeft, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null).UserData = "moreindicator";
			this.ItemInfoFrame.Children.ForEach(delegate(GUIComponent c)
			{
				c.CanBeFocused = false;
				c.Children.ForEach(delegate(GUIComponent c2)
				{
					c2.CanBeFocused = false;
				});
			});
			GUIFrame paddedLayout = new GUIFrame(UpgradeStore.rectT(0.95f, 0.95f, parent, Anchor.Center, ScaleBasis.Normal), null, null);
			this.mainStoreLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.9f, paddedLayout, Anchor.BottomLeft, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f
			};
			this.topHeaderLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.1f, paddedLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.storeLayout = new GUILayoutGroup(UpgradeStore.rectT(0.2f, 0.4f, this.mainStoreLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup leftLayout = new GUILayoutGroup(UpgradeStore.rectT(0.4f, 1f, this.topHeaderLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			GUILayoutGroup locationLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.5f, leftLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIImage submarineIcon = new GUIImage(UpgradeStore.rectT(new Point(locationLayout.Rect.Height, locationLayout.Rect.Height), locationLayout, Anchor.TopLeft), "SubmarineIcon", true);
			RectTransform rectT2 = UpgradeStore.rectT(1f - submarineIcon.RectTransform.RelativeSize.X, 1f, locationLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("UpgradeUI.Title");
			font = GUIStyle.LargeFont;
			GUITextBlock header = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			header.RectTransform.MaxSize = new Point((int)(header.TextSize.X + header.Padding.X + header.Padding.Z), int.MaxValue);
			RectTransform rectT3 = UpgradeStore.rectT(1f, 1f, locationLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("UpgradeUI.AllSubmarinesInfo");
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT3, text3, null, font, Alignment.Left, true, "", null);
			this.categoryButtonLayout = new GUILayoutGroup(UpgradeStore.rectT(0.4f, 0.3f, leftLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIButton upgradeButton = new GUIButton(UpgradeStore.rectT(0.5f, 1f, this.categoryButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("UICategory.Upgrades"), Alignment.Center, "GUITabButton", null)
			{
				UserData = UpgradeStore.UpgradeTab.Upgrade,
				Selected = (this.selectedUpgradeTab == UpgradeStore.UpgradeTab.Upgrade)
			};
			GUIButton repairButton = new GUIButton(UpgradeStore.rectT(0.5f, 1f, this.categoryButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("UICategory.Maintenance"), Alignment.Center, "GUITabButton", null)
			{
				UserData = UpgradeStore.UpgradeTab.Repairs,
				Selected = (this.selectedUpgradeTab == UpgradeStore.UpgradeTab.Repairs)
			};
			GUILayoutGroup rightLayout = new GUILayoutGroup(UpgradeStore.rectT(0.5f, 1f, this.topHeaderLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopRight);
			this.playerBalanceElement = CampaignUI.AddBalanceElement(rightLayout, new Vector2(1f, 0.8f));
			CampaignUI.PlayerBalanceElement? playerBalanceElement = this.playerBalanceElement;
			if (playerBalanceElement != null)
			{
				CampaignUI.PlayerBalanceElement balanceElement = playerBalanceElement.GetValueOrDefault();
				GUILayoutGroup totalBalanceContainer = balanceElement.TotalBalanceContainer;
				totalBalanceContainer.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(totalBalanceContainer.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent _)
				{
					this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
				}));
			}
			new GUIFrame(UpgradeStore.rectT(0.5f, 0.1f, rightLayout, Anchor.BottomRight, ScaleBasis.Normal), "HorizontalLine", null).IgnoreLayoutGroups = true;
			repairButton.OnClicked = (upgradeButton.OnClicked = delegate(GUIButton button, object o)
			{
				if (o is UpgradeStore.UpgradeTab)
				{
					UpgradeStore.UpgradeTab upgradeTab = (UpgradeStore.UpgradeTab)o;
					if (upgradeTab != this.selectedUpgradeTab || this.currentStoreLayout == null || this.currentStoreLayout.Parent != this.storeLayout)
					{
						this.selectedUpgradeTab = upgradeTab;
						this.SelectTab(this.selectedUpgradeTab);
						GUILayoutGroup guilayoutGroup = this.storeLayout;
						if (guilayoutGroup != null)
						{
							guilayoutGroup.Recalculate();
						}
					}
					repairButton.Selected = ((UpgradeStore.UpgradeTab)repairButton.UserData == this.selectedUpgradeTab);
					upgradeButton.Selected = ((UpgradeStore.UpgradeTab)upgradeButton.UserData == this.selectedUpgradeTab);
					return true;
				}
				return false;
			});
			RectTransform rectT4 = UpgradeStore.rectT(0.75f, 0.75f, this.mainStoreLayout, Anchor.BottomRight, ScaleBasis.Normal);
			Action<float, GUICustomComponent> onUpdate = new Action<float, GUICustomComponent>(this.UpdateSubmarinePreview);
			this.submarinePreviewComponent = new GUICustomComponent(rectT4, new Action<SpriteBatch, GUICustomComponent>(this.DrawSubmarine), onUpdate)
			{
				IgnoreLayoutGroups = true
			};
			this.SelectTab(UpgradeStore.UpgradeTab.Upgrade);
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Vector2(0.25f, 0.4f), this.mainStoreLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.52f * GUI.AspectRatioAdjustment, 0f)
			}, new Action<SpriteBatch, GUICustomComponent>(this.DrawItemSwapPreview), null);
			guicustomComponent.IgnoreLayoutGroups = true;
			guicustomComponent.CanBeFocused = true;
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				upgradeButton.TextBlock,
				repairButton.TextBlock
			});
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x000E43EC File Offset: 0x000E25EC
		private void DrawItemSwapPreview(SpriteBatch spriteBatch, GUICustomComponent component)
		{
			Item item;
			if (!this.customizeTabOpen)
			{
				item = (this.HoveredEntity as Item);
			}
			else
			{
				GUIButton guibutton = this.activeItemSwapSlideDown;
				item = ((((guibutton != null) ? guibutton.UserData : null) as Item) ?? (this.HoveredEntity as Item));
			}
			Item selectedItem = item;
			if (((selectedItem != null) ? selectedItem.Prefab.SwappableItem : null) == null)
			{
				return;
			}
			Sprite schematicsSprite = selectedItem.Prefab.SwappableItem.SchematicSprite;
			if (schematicsSprite == null)
			{
				return;
			}
			float schematicsScale = Math.Min((float)(component.Rect.Width / 2) / schematicsSprite.size.X, (float)component.Rect.Height / schematicsSprite.size.Y);
			Vector2 center = new Vector2((float)component.Rect.Center.X, (float)component.Rect.Center.Y);
			schematicsSprite.Draw(spriteBatch, new Vector2((float)component.Rect.X, center.Y), GUIStyle.Green, new Vector2(0f, schematicsSprite.size.Y / 2f), 0f, schematicsScale, SpriteEffects.None, null);
			GUIComponent guicomponent = this.selectedUpgradeCategoryLayout;
			GUIListBox swappableItemList = ((guicomponent != null) ? guicomponent.FindChild("prefablist", true) : null) as GUIListBox;
			GUIComponent guicomponent2;
			if (swappableItemList == null)
			{
				guicomponent2 = null;
			}
			else
			{
				guicomponent2 = swappableItemList.Content.FindChild((GUIComponent c) => c.UserData is ItemPrefab && c.IsParentOf(GUI.MouseOn, true), false);
			}
			GUIComponent highlightedElement = guicomponent2 ?? GUI.MouseOn;
			ItemPrefab swapTo = (((highlightedElement != null) ? highlightedElement.UserData : null) as ItemPrefab) ?? selectedItem.PendingItemSwap;
			if (((swapTo != null) ? swapTo.SwappableItem : null) == null)
			{
				return;
			}
			SwappableItem swappableItem = swapTo.SwappableItem;
			Sprite schematicsSprite2 = (swappableItem != null) ? swappableItem.SchematicSprite : null;
			if (schematicsSprite2 != null)
			{
				schematicsSprite2.Draw(spriteBatch, new Vector2((float)component.Rect.Right, center.Y), GUIStyle.Orange, new Vector2(schematicsSprite2.size.X, schematicsSprite2.size.Y / 2f), 0f, Math.Min((float)(component.Rect.Width / 2) / schematicsSprite2.size.X, (float)component.Rect.Height / schematicsSprite2.size.Y), SpriteEffects.None, null);
			}
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("GUIButtonToggleRight");
			Sprite arrowSprite = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
			if (arrowSprite != null)
			{
				arrowSprite.Draw(spriteBatch, center, 0f, GUI.Scale, SpriteEffects.None);
			}
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x000E4690 File Offset: 0x000E2890
		private void SelectTab(UpgradeStore.UpgradeTab tab)
		{
			if (this.currentStoreLayout != null)
			{
				GUILayoutGroup guilayoutGroup = this.storeLayout;
				if (guilayoutGroup != null)
				{
					guilayoutGroup.RemoveChild(this.currentStoreLayout);
				}
			}
			if (this.selectedUpgradeCategoryLayout != null)
			{
				GUILayoutGroup guilayoutGroup2 = this.mainStoreLayout;
				if (guilayoutGroup2 != null)
				{
					guilayoutGroup2.RemoveChild(this.selectedUpgradeCategoryLayout);
				}
			}
			if (tab == UpgradeStore.UpgradeTab.Upgrade)
			{
				this.CreateUpgradeTab();
				return;
			}
			if (tab != UpgradeStore.UpgradeTab.Repairs)
			{
				return;
			}
			this.CreateRepairsTab();
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x000E46F0 File Offset: 0x000E28F0
		private void CreateRepairsTab()
		{
			if (this.Campaign == null || this.storeLayout == null)
			{
				return;
			}
			this.highlightWalls = false;
			foreach (GUIComponent itemFrame in this.itemPreviews.Values)
			{
				itemFrame.OutlineColor = UpgradeStore.previewWhite;
			}
			this.currentStoreLayout = new GUIListBox(new RectTransform(new Vector2(1.2f, 1.5f), this.storeLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(256, 0)
			}, false, null, null, true, false)
			{
				AutoHideScrollBar = false,
				ScrollBarVisible = false,
				Spacing = 8
			};
			Location location = this.Campaign.Map.CurrentLocation;
			int hullRepairCost = CampaignMode.GetHullRepairCost();
			int itemRepairCost = CampaignMode.GetItemRepairCost();
			int shuttleRetrieveCost = 1000;
			if (location != null)
			{
				hullRepairCost = location.GetAdjustedMechanicalCost(hullRepairCost);
				itemRepairCost = location.GetAdjustedMechanicalCost(itemRepairCost);
				shuttleRetrieveCost = location.GetAdjustedMechanicalCost(shuttleRetrieveCost);
			}
			this.CreateRepairEntry(this.currentStoreLayout.Content, TextManager.Get("repairallwalls"), "RepairHullButton", hullRepairCost, delegate(GUIButton button, object o)
			{
				if (this.Campaign.PurchasedHullRepairs || hullRepairCost <= 0)
				{
					button.Enabled = false;
					return false;
				}
				if (this.PlayerBalance >= hullRepairCost)
				{
					LocalizedString body = TextManager.GetWithVariable("WallRepairs.PurchasePromptBody", "[amount]", hullRepairCost.ToString(), FormatCapitals.No);
					this.currectConfirmation = EventEditorScreen.AskForConfirmation(TextManager.Get("Upgrades.PurchasePromptTitle"), body, delegate
					{
						if (this.PlayerBalance >= hullRepairCost)
						{
							this.Campaign.TryPurchase(null, hullRepairCost);
							GameAnalyticsManager.AddMoneySpentEvent(hullRepairCost, GameAnalyticsManager.MoneySink.Service, "hullrepairs");
							this.Campaign.PurchasedHullRepairs = true;
							button.Enabled = false;
							this.SelectTab(UpgradeStore.UpgradeTab.Repairs);
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.SendCampaignState();
							}
						}
						else
						{
							button.Enabled = false;
						}
						return true;
					}, new GUISoundType?(GUISoundType.ConfirmTransaction));
					return true;
				}
				button.Enabled = false;
				return false;
			}, this.Campaign.PurchasedHullRepairs || !UpgradeStore.HasPermission || hullRepairCost <= 0, delegate(bool isHovered)
			{
				this.highlightWalls = isHovered;
				return true;
			}, false);
			this.CreateRepairEntry(this.currentStoreLayout.Content, TextManager.Get("repairallitems"), "RepairItemsButton", itemRepairCost, delegate(GUIButton button, object o)
			{
				if (this.PlayerBalance >= itemRepairCost && !this.Campaign.PurchasedItemRepairs && itemRepairCost > 0)
				{
					LocalizedString body = TextManager.GetWithVariable("ItemRepairs.PurchasePromptBody", "[amount]", itemRepairCost.ToString(), FormatCapitals.No);
					this.currectConfirmation = EventEditorScreen.AskForConfirmation(TextManager.Get("Upgrades.PurchasePromptTitle"), body, delegate
					{
						if (this.PlayerBalance >= itemRepairCost && !this.Campaign.PurchasedItemRepairs)
						{
							this.Campaign.TryPurchase(null, itemRepairCost);
							GameAnalyticsManager.AddMoneySpentEvent(hullRepairCost, GameAnalyticsManager.MoneySink.Service, "devicerepairs");
							this.Campaign.PurchasedItemRepairs = true;
							button.Enabled = false;
							this.SelectTab(UpgradeStore.UpgradeTab.Repairs);
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.SendCampaignState();
							}
						}
						else
						{
							button.Enabled = false;
						}
						return true;
					}, new GUISoundType?(GUISoundType.ConfirmTransaction));
					return true;
				}
				button.Enabled = false;
				return false;
			}, this.Campaign.PurchasedItemRepairs || !UpgradeStore.HasPermission || itemRepairCost <= 0, delegate(bool isHovered)
			{
				foreach (KeyValuePair<Item, GUIComponent> keyValuePair in this.itemPreviews)
				{
					Item item2;
					GUIComponent guicomponent;
					keyValuePair.Deconstruct(out item2, out guicomponent);
					Item item = item2;
					GUIComponent itemFrame2 = guicomponent;
					itemFrame2.OutlineColor = (itemFrame2.Color = ((isHovered && item.GetComponent<DockingPort>() == null) ? GUIStyle.Orange : UpgradeStore.previewWhite));
				}
				return true;
			}, false);
			GUIComponent content = this.currentStoreLayout.Content;
			LocalizedString title = TextManager.Get("replacelostshuttles");
			string imageStyle = "ReplaceShuttlesButton";
			int shuttleRetrieveCost2 = shuttleRetrieveCost;
			GUIButton.OnClickedHandler onPressed = delegate(GUIButton button, object o)
			{
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.SubmarineInfo : null) != null && GameMain.GameSession.SubmarineInfo.LeftBehindSubDockingPortOccupied)
				{
					new GUIMessageBox("", TextManager.Get("ReplaceShuttleDockingPortOccupied"), null, null, GUIMessageBox.Type.Default);
					return false;
				}
				if (this.PlayerBalance >= shuttleRetrieveCost && !this.Campaign.PurchasedLostShuttles)
				{
					LocalizedString body = TextManager.GetWithVariable("ReplaceLostShuttles.PurchasePromptBody", "[amount]", shuttleRetrieveCost.ToString(), FormatCapitals.No);
					this.currectConfirmation = EventEditorScreen.AskForConfirmation(TextManager.Get("Upgrades.PurchasePromptTitle"), body, delegate
					{
						if (this.PlayerBalance >= shuttleRetrieveCost && !this.Campaign.PurchasedLostShuttles)
						{
							this.Campaign.TryPurchase(null, shuttleRetrieveCost);
							GameAnalyticsManager.AddMoneySpentEvent(hullRepairCost, GameAnalyticsManager.MoneySink.Service, "retrieveshuttle");
							this.Campaign.PurchasedLostShuttles = true;
							button.Enabled = false;
							this.SelectTab(UpgradeStore.UpgradeTab.Repairs);
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.SendCampaignState();
							}
						}
						return true;
					}, new GUISoundType?(GUISoundType.ConfirmTransaction));
					return true;
				}
				button.Enabled = false;
				return false;
			};
			bool isDisabled;
			if (!this.Campaign.PurchasedLostShuttles && UpgradeStore.HasPermission)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.SubmarineInfo : null) != null)
				{
					isDisabled = !GameMain.GameSession.SubmarineInfo.SubsLeftBehind;
					goto IL_27A;
				}
			}
			isDisabled = true;
			IL_27A:
			this.CreateRepairEntry(content, title, imageStyle, shuttleRetrieveCost2, onPressed, isDisabled, delegate(bool isHovered)
			{
				if (!isHovered)
				{
					return false;
				}
				GameSession gameSession2 = GameMain.GameSession;
				SubmarineInfo subInfo = (gameSession2 != null) ? gameSession2.SubmarineInfo : null;
				if (subInfo == null)
				{
					return false;
				}
				foreach (KeyValuePair<Item, GUIComponent> keyValuePair in this.itemPreviews)
				{
					Item item2;
					GUIComponent guicomponent;
					keyValuePair.Deconstruct(out item2, out guicomponent);
					Item item = item2;
					GUIComponent itemFrame2 = guicomponent;
					if (subInfo.LeftBehindDockingPortIDs.Contains(item.ID))
					{
						itemFrame2.OutlineColor = (itemFrame2.Color = (subInfo.BlockedDockingPortIDs.Contains(item.ID) ? GUIStyle.Red : GUIStyle.Green));
					}
					else
					{
						itemFrame2.OutlineColor = (itemFrame2.Color = UpgradeStore.previewWhite);
					}
				}
				return true;
			}, true);
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x000E499C File Offset: 0x000E2B9C
		private void CreateRepairEntry(GUIComponent parent, LocalizedString title, string imageStyle, int price, GUIButton.OnClickedHandler onPressed, bool isDisabled, [Nullable(2)] Func<bool, bool> onHover = null, bool disableElement = false)
		{
			UpgradeStore.<>c__DisplayClass44_0 CS$<>8__locals1 = new UpgradeStore.<>c__DisplayClass44_0();
			CS$<>8__locals1.onHover = onHover;
			CS$<>8__locals1.frameChild = new GUIFrame(UpgradeStore.rectT(new Point(parent.Rect.Width, (int)(96f * GUI.Scale)), parent, Anchor.TopLeft), "UpgradeUIFrame", null);
			CS$<>8__locals1.frameChild.SelectedColor = CS$<>8__locals1.frameChild.Color;
			new GUICustomComponent(UpgradeStore.rectT(1f, 1f, CS$<>8__locals1.frameChild, Anchor.TopLeft, ScaleBasis.Normal), null, new Action<float, GUICustomComponent>(CS$<>8__locals1.<CreateRepairEntry>g__UpdateHover|0)).CanBeFocused = false;
			GUILayoutGroup contentLayout = new GUILayoutGroup(UpgradeStore.rectT(0.9f, 0.85f, CS$<>8__locals1.frameChild, Anchor.Center, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIFrame repairIcon = new GUIFrame(UpgradeStore.rectT(new Point(contentLayout.Rect.Height, contentLayout.Rect.Height), contentLayout, Anchor.TopLeft), imageStyle, null);
			GUILayoutGroup textLayout = new GUILayoutGroup(UpgradeStore.rectT(0.8f - repairIcon.RectTransform.RelativeSize.X, 1f, contentLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = UpgradeStore.rectT(1f, 0f, textLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text = title;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock.CanBeFocused = false;
			guitextBlock.AutoScaleHorizontal = true;
			new GUITextBlock(UpgradeStore.rectT(1f, 0f, textLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.FormatCurrency(price, true), null, null, Alignment.Left, false, "", null);
			GUILayoutGroup buyButtonLayout = new GUILayoutGroup(UpgradeStore.rectT(0.2f, 1f, contentLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.Center)
			{
				UserData = UpgradeStore.UpgradeStoreUserData.BuyButtonLayout
			};
			GUIButton guibutton = new GUIButton(UpgradeStore.rectT(0.7f, 0.5f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), string.Empty, Alignment.Center, "RepairBuyButton", null);
			guibutton.Enabled = (this.PlayerBalance >= price && !isDisabled);
			guibutton.OnClicked = onPressed;
			contentLayout.Recalculate();
			buyButtonLayout.Recalculate();
			if (disableElement)
			{
				CS$<>8__locals1.frameChild.Enabled = (this.PlayerBalance >= price && !isDisabled);
			}
			if (!UpgradeStore.HasPermission)
			{
				CS$<>8__locals1.frameChild.Enabled = false;
			}
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x000E4C08 File Offset: 0x000E2E08
		public static GUIListBox CreateUpgradeCategoryList(RectTransform rectTransform)
		{
			GUIListBox upgradeCategoryList = new GUIListBox(rectTransform, false, null, null, true, false)
			{
				AutoHideScrollBar = false,
				ScrollBarVisible = false,
				HideChildrenOutsideFrame = false,
				SmoothScroll = true,
				FadeElements = true,
				PadBottom = true,
				SelectTop = true,
				ClampScrollToElements = true,
				Spacing = 8,
				PlaySoundOnSelect = true
			};
			Dictionary<UpgradeCategory, List<UpgradePrefab>> upgrades = new Dictionary<UpgradeCategory, List<UpgradePrefab>>();
			foreach (UpgradeCategory category in from c in UpgradeCategory.Categories
			orderby c.Name
			select c)
			{
				foreach (UpgradePrefab prefab in UpgradePrefab.Prefabs.OrderBy((UpgradePrefab p) => p.Name))
				{
					if (prefab.UpgradeCategories.Contains(category))
					{
						if (upgrades.ContainsKey(category))
						{
							upgrades[category].Add(prefab);
						}
						else
						{
							upgrades.Add(category, new List<UpgradePrefab>
							{
								prefab
							});
						}
					}
				}
				if (!upgrades.ContainsKey(category) && UpgradeStore.HasSwappableItems(category, null))
				{
					upgrades.Add(category, new List<UpgradePrefab>());
				}
			}
			foreach (KeyValuePair<UpgradeCategory, List<UpgradePrefab>> keyValuePair in upgrades)
			{
				UpgradeCategory upgradeCategory;
				List<UpgradePrefab> list;
				keyValuePair.Deconstruct(out upgradeCategory, out list);
				UpgradeCategory category2 = upgradeCategory;
				List<UpgradePrefab> prefabs = list;
				GUIFrame frameChild = new GUIFrame(UpgradeStore.rectT(1f, 0.15f, upgradeCategoryList.Content, Anchor.TopLeft, ScaleBasis.Normal), "UpgradeUIFrame", null)
				{
					UserData = new UpgradeStore.CategoryData(category2, prefabs),
					GlowOnSelect = true
				};
				frameChild.DefaultColor = frameChild.Color;
				frameChild.Color = Color.Transparent;
				GUIButton weaponSwitchBg = new GUIButton(new RectTransform(new Vector2(0.65f), frameChild.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Smallest)
				{
					RelativeOffset = new Vector2(0.04f, 0f)
				}, Alignment.Center, "WeaponSwitchTab", null)
				{
					Visible = false,
					CanBeSelected = false,
					UserData = "customizebutton"
				};
				weaponSwitchBg.DefaultColor = (weaponSwitchBg.Frame.DefaultColor = weaponSwitchBg.Color);
				GUIImage weaponSwitchImg = new GUIImage(new RectTransform(new Vector2(0.7f), weaponSwitchBg.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "WeaponSwitchIcon", true)
				{
					CanBeFocused = false
				};
				weaponSwitchImg.DefaultColor = weaponSwitchImg.Color;
				GUILayoutGroup contentLayout = new GUILayoutGroup(UpgradeStore.rectT(0.9f, 0.85f, frameChild, Anchor.Center, ScaleBasis.Normal), false, Anchor.TopLeft);
				RectTransform rectT = UpgradeStore.rectT(1f, 1f, contentLayout, Anchor.TopLeft, ScaleBasis.Normal);
				RichString text = category2.Name;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				GUITextBlock itemCategoryLabel = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null)
				{
					CanBeFocused = false
				};
				GUILayoutGroup indicatorLayout = new GUILayoutGroup(UpgradeStore.rectT(0.5f, 0.25f, contentLayout, Anchor.BottomRight, ScaleBasis.Normal), true, Anchor.TopRight)
				{
					UserData = "indicators",
					IgnoreLayoutGroups = true,
					RelativeSpacing = 0.01f
				};
				foreach (UpgradePrefab prefab2 in prefabs)
				{
					GUIImage upgradeIndicator = new GUIImage(UpgradeStore.rectT(0.1f, 1f, indicatorLayout, Anchor.TopLeft, ScaleBasis.Normal), "UpgradeIndicator", true)
					{
						UserData = prefab2,
						CanBeFocused = false
					};
					upgradeIndicator.DefaultColor = upgradeIndicator.Color;
					upgradeIndicator.Color = Color.Transparent;
				}
				itemCategoryLabel.DefaultColor = itemCategoryLabel.TextColor;
				itemCategoryLabel.TextColor = Color.Transparent;
				contentLayout.Recalculate();
				indicatorLayout.Recalculate();
			}
			return upgradeCategoryList;
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x000E50BC File Offset: 0x000E32BC
		private void CreateUpgradeTab()
		{
			if (this.storeLayout == null || this.mainStoreLayout == null)
			{
				return;
			}
			this.currentStoreLayout = UpgradeStore.CreateUpgradeCategoryList(UpgradeStore.rectT(1f, 1.5f, this.storeLayout, Anchor.TopLeft, ScaleBasis.Normal));
			this.selectedUpgradeCategoryLayout = new GUIFrame(UpgradeStore.rectT(0.3f * GUI.AspectRatioAdjustment, 1f, this.mainStoreLayout, Anchor.TopLeft, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			this.RefreshUpgradeList();
			GUIListBox guilistBox = this.currentStoreLayout;
			guilistBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object userData)
			{
				if (!component.Enabled)
				{
					GUIComponent guicomponent = this.selectedUpgradeCategoryLayout;
					if (guicomponent != null)
					{
						guicomponent.ClearChildren();
					}
					using (Dictionary<Item, GUIComponent>.ValueCollection.Enumerator enumerator = this.itemPreviews.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							GUIComponent itemFrame = enumerator.Current;
							itemFrame.OutlineColor = (itemFrame.Color = UpgradeStore.previewWhite);
							itemFrame.Children.ForEach(delegate(GUIComponent c)
							{
								c.Color = itemFrame.Color;
							});
						}
					}
					return true;
				}
				if (userData is UpgradeStore.CategoryData)
				{
					UpgradeStore.CategoryData categoryData = (UpgradeStore.CategoryData)userData;
					Submarine sub = Submarine.MainSub;
					if (sub != null)
					{
						List<UpgradePrefab> prefabs = categoryData.Prefabs;
						if (prefabs != null)
						{
							this.TrySelectCategory(prefabs, categoryData.Category, sub);
						}
					}
				}
				GUIComponent guicomponent2 = this.selectedUpgradeCategoryLayout;
				GUIButton customizeCategoryButton = ((guicomponent2 != null) ? guicomponent2.FindChild("customizebutton", true) : null) as GUIButton;
				if (customizeCategoryButton != null)
				{
					customizeCategoryButton.OnClicked(customizeCategoryButton, customizeCategoryButton.UserData);
				}
				return true;
			}));
		}

		// Token: 0x06001772 RID: 6002 RVA: 0x000E5162 File Offset: 0x000E3362
		private void TrySelectCategory(List<UpgradePrefab> prefabs, UpgradeCategory category, Submarine submarine)
		{
			this.SelectUpgradeCategory(prefabs, category, submarine);
		}

		// Token: 0x06001773 RID: 6003 RVA: 0x000E5170 File Offset: 0x000E3370
		private static bool HasSwappableItems(UpgradeCategory category, [Nullable(new byte[]
		{
			2,
			1
		})] List<Item> subItems = null)
		{
			if (Submarine.MainSub == null)
			{
				return false;
			}
			if (subItems == null)
			{
				subItems = UpgradeStore.GetSubItems();
			}
			return subItems.Any((Item item) => UpgradeStore.HasSwappableItems(category, item));
		}

		// Token: 0x06001774 RID: 6004 RVA: 0x000E51B0 File Offset: 0x000E33B0
		private static bool HasSwappableItems(UpgradeCategory category, Item item)
		{
			return Submarine.MainSub != null && (item.Prefab.SwappableItem != null && !item.IsHidden && item.AllowSwapping && (item.Prefab.SwappableItem.CanBeBought || ItemPrefab.Prefabs.Any(delegate(ItemPrefab ip)
			{
				SwappableItem swappableItem = ip.SwappableItem;
				Identifier? identifier;
				Identifier? identifier2;
				if (swappableItem == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					identifier2 = new Identifier?(swappableItem.ReplacementOnUninstall);
				}
				identifier = identifier2;
				Identifier? identifier3 = new Identifier?(item.Prefab.Identifier);
				return identifier == identifier3;
			})) && Submarine.MainSub.IsEntityFoundOnThisSub(item, true, false, false)) && category.ItemTags.Any((Identifier t) => item.HasTag(t));
		}

		// Token: 0x06001775 RID: 6005 RVA: 0x000E525C File Offset: 0x000E345C
		private static List<Item> GetSubItems()
		{
			Submarine mainSub = Submarine.MainSub;
			return ((mainSub != null) ? mainSub.GetItems(true) : null) ?? new List<Item>();
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x000E527C File Offset: 0x000E347C
		private void SelectUpgradeCategory(List<UpgradePrefab> prefabs, UpgradeCategory category, Submarine submarine)
		{
			if (this.selectedUpgradeCategoryLayout == null)
			{
				return;
			}
			bool hasSwappableItems = UpgradeStore.HasSwappableItems(category, null);
			bool hasUpgradeModules = prefabs.Count > 0;
			this.customizeTabOpen = (!hasUpgradeModules && hasSwappableItems);
			GUIComponent[] categoryFrames = this.GetFrames(category);
			using (Dictionary<Item, GUIComponent>.ValueCollection.Enumerator enumerator = this.itemPreviews.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GUIComponent itemFrame = enumerator.Current;
					itemFrame.OutlineColor = (itemFrame.Color = (categoryFrames.Contains(itemFrame) ? GUIStyle.Orange : UpgradeStore.previewWhite));
					itemFrame.Children.ForEach(delegate(GUIComponent c)
					{
						c.Color = itemFrame.Color;
					});
				}
			}
			this.highlightWalls = category.IsWallUpgrade;
			this.selectedUpgradeCategoryLayout.ClearChildren();
			GUIFrame frame = new GUIFrame(UpgradeStore.rectT(1f, 0.4f, this.selectedUpgradeCategoryLayout, Anchor.TopLeft, ScaleBasis.Normal), "", null);
			GUIFrame paddedFrame = new GUIFrame(UpgradeStore.rectT(0.93f, 0.9f, frame, Anchor.Center, ScaleBasis.Normal), null, null);
			float listHeight = (hasSwappableItems && hasUpgradeModules) ? 0.9f : 1f;
			GUIListBox prefabList = new GUIListBox(UpgradeStore.rectT(1f, listHeight, paddedFrame, Anchor.BottomLeft, ScaleBasis.Normal), false, null, "", true, false)
			{
				UserData = "prefablist",
				AutoHideScrollBar = false,
				ScrollBarVisible = true
			};
			if (hasSwappableItems && hasUpgradeModules)
			{
				GUILayoutGroup buttonLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.1f, paddedFrame, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft);
				GUIButton customizeButton = new GUIButton(UpgradeStore.rectT(0.5f, 1f, buttonLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("uicategory.customize"), Alignment.Center, "GUITabButton", null)
				{
					UserData = "customizebutton"
				};
				new GUIImage(new RectTransform(new Vector2(1f, 0.75f), customizeButton.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Smallest)
				{
					RelativeOffset = new Vector2(0.015f, 0f)
				}, "WeaponSwitchIcon", true);
				customizeButton.TextBlock.RectTransform.RelativeSize = new Vector2(0.7f, 1f);
				GUIButton upgradeButton = new GUIButton(UpgradeStore.rectT(0.5f, 1f, buttonLayout, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("uicategory.upgrades"), Alignment.Center, "GUITabButton", null)
				{
					Selected = true
				};
				GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
				{
					upgradeButton.TextBlock,
					customizeButton.TextBlock
				});
				upgradeButton.OnClicked = delegate(GUIButton <p0>, object <p1>)
				{
					this.customizeTabOpen = false;
					customizeButton.Selected = false;
					upgradeButton.Selected = true;
					this.CreateUpgradePrefabList(prefabList, category, prefabs, submarine);
					GUIComponent[] categoryFrames2 = this.GetFrames(category);
					using (Dictionary<Item, GUIComponent>.ValueCollection.Enumerator enumerator2 = this.itemPreviews.Values.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							GUIComponent itemFrame = enumerator2.Current;
							itemFrame.OutlineColor = (itemFrame.Color = (categoryFrames2.Contains(itemFrame) ? GUIStyle.Orange : UpgradeStore.previewWhite));
							itemFrame.Children.ForEach(delegate(GUIComponent c)
							{
								c.Color = itemFrame.Color;
							});
						}
					}
					return true;
				};
				customizeButton.OnClicked = delegate(GUIButton <p0>, object <p1>)
				{
					this.customizeTabOpen = true;
					customizeButton.Selected = true;
					upgradeButton.Selected = false;
					this.CreateSwappableItemList(prefabList, category, submarine);
					return true;
				};
				return;
			}
			if (hasUpgradeModules)
			{
				this.CreateUpgradePrefabList(prefabList, category, prefabs, submarine);
				return;
			}
			if (hasSwappableItems)
			{
				this.CreateSwappableItemList(prefabList, category, submarine);
			}
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x000E561C File Offset: 0x000E381C
		private void CreateUpgradePrefabList(GUIListBox parent, UpgradeCategory category, List<UpgradePrefab> prefabs, Submarine submarine)
		{
			parent.Content.ClearChildren();
			List<Item> entitiesOnSub = null;
			if (!category.IsWallUpgrade)
			{
				entitiesOnSub = (from i in submarine.GetItems(true)
				where submarine.IsEntityFoundOnThisSub(i, true, false, false)
				select i).ToList<Item>();
			}
			foreach (UpgradePrefab prefab in prefabs)
			{
				if (prefab.GetMaxLevelForCurrentSub() != 0)
				{
					this.CreateUpgradeEntry(prefab, category, parent.Content, submarine, entitiesOnSub);
				}
			}
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x000E56C8 File Offset: 0x000E38C8
		private void CreateSwappableItemList(GUIListBox parent, UpgradeCategory category, Submarine submarine)
		{
			parent.Content.ClearChildren();
			this.currentUpgradeCategory = category;
			List<Item> entitiesOnSub = (from i in submarine.GetItems(true)
			where submarine.IsEntityFoundOnThisSub(i, true, false, false) && !i.IsHidden && i.AllowSwapping && i.Prefab.SwappableItem != null && category.ItemTags.Any((Identifier t) => i.HasTag(t))
			select i).ToList<Item>();
			foreach (Item item in entitiesOnSub)
			{
				this.CreateSwappableItemSlideDown(parent, item, entitiesOnSub, submarine);
			}
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x000E576C File Offset: 0x000E396C
		private void CreateSwappableItemSlideDown(GUIListBox parent, Item item, List<Item> swappableEntities, Submarine submarine)
		{
			UpgradeStore.<>c__DisplayClass55_0 CS$<>8__locals1 = new UpgradeStore.<>c__DisplayClass55_0();
			CS$<>8__locals1.item = item;
			CS$<>8__locals1.swappableEntities = swappableEntities;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.parent = parent;
			if (this.Campaign == null || submarine == null)
			{
				return;
			}
			IEnumerable<ItemPrefab> availableReplacements = MapEntityPrefab.List.Where(delegate(MapEntityPrefab p)
			{
				ItemPrefab itemPrefab = p as ItemPrefab;
				return itemPrefab != null && itemPrefab.SwappableItem != null && itemPrefab.SwappableItem.CanBeBought && itemPrefab.SwappableItem.SwapIdentifier.Equals(CS$<>8__locals1.item.Prefab.SwappableItem.SwapIdentifier, StringComparison.OrdinalIgnoreCase);
			}).Cast<ItemPrefab>();
			UpgradeStore.<>c__DisplayClass55_0 CS$<>8__locals2 = CS$<>8__locals1;
			ICollection<Item> linkedItems;
			if ((linkedItems = UpgradeManager.GetLinkedItemsToSwap(CS$<>8__locals1.item)) == null)
			{
				(linkedItems = new List<Item>()).Add(CS$<>8__locals1.item);
			}
			CS$<>8__locals2.linkedItems = linkedItems;
			if (CS$<>8__locals1.linkedItems.Min((Item it) => it.ID) < CS$<>8__locals1.item.ID)
			{
				return;
			}
			CS$<>8__locals1.currentOrPending = (CS$<>8__locals1.item.PendingItemSwap ?? CS$<>8__locals1.item.Prefab);
			LocalizedString name = CS$<>8__locals1.currentOrPending.Name;
			LocalizedString nameWithQuantity = "";
			if (CS$<>8__locals1.linkedItems.Count > 1)
			{
				using (IEnumerator<ItemPrefab> enumerator = (from it in CS$<>8__locals1.linkedItems
				select it.Prefab).Distinct<ItemPrefab>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ItemPrefab distinctItem = enumerator.Current;
						if (nameWithQuantity != string.Empty)
						{
							nameWithQuantity += ", ";
						}
						int count = CS$<>8__locals1.linkedItems.Count((Item it) => it.Prefab == distinctItem);
						nameWithQuantity += distinctItem.Name;
						if (count > 1)
						{
							nameWithQuantity += " " + TextManager.GetWithVariable("campaignstore.quantity", "[amount]", count.ToString(), FormatCapitals.No);
						}
					}
					goto IL_1E4;
				}
			}
			nameWithQuantity = name;
			IL_1E4:
			CS$<>8__locals1.isOpen = false;
			CS$<>8__locals1.toggleButton = new GUIButton(UpgradeStore.rectT(1f, 0.1f, CS$<>8__locals1.parent.Content, Anchor.TopLeft, ScaleBasis.Normal), string.Empty, Alignment.Center, "SlideDown", null)
			{
				UserData = CS$<>8__locals1.item
			};
			GUILayoutGroup buttonLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 1f, CS$<>8__locals1.toggleButton.Frame, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft);
			LocalizedString slotText = "";
			if (CS$<>8__locals1.linkedItems.Count > 1)
			{
				slotText = TextManager.GetWithVariable("weaponslot", "[number]", string.Join(", ", from it in CS$<>8__locals1.linkedItems
				select (CS$<>8__locals1.swappableEntities.IndexOf(it) + 1).ToString()), FormatCapitals.No);
			}
			else
			{
				slotText = TextManager.GetWithVariable("weaponslot", "[number]", (CS$<>8__locals1.swappableEntities.IndexOf(CS$<>8__locals1.item) + 1).ToString(), FormatCapitals.No);
			}
			RectTransform rectT = UpgradeStore.rectT(0.3f, 1f, buttonLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text2 = slotText;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, false, "", null);
			GUILayoutGroup group = new GUILayoutGroup(UpgradeStore.rectT(0.7f, 1f, buttonLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			LocalizedString title = (CS$<>8__locals1.item.PendingItemSwap != null) ? TextManager.GetWithVariable("upgrades.pendingitem", "[itemname]", name, FormatCapitals.No) : nameWithQuantity;
			RectTransform rectT2 = UpgradeStore.rectT(0.7f, 1f, group, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text3 = RichString.Rich(title, null);
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock text = new GUITextBlock(rectT2, text3, null, subHeadingFont, Alignment.Right, false, "", null)
			{
				TextColor = GUIStyle.Orange
			};
			CS$<>8__locals1.arrowImage = new GUIImage(UpgradeStore.rectT(0.5f, 1f, group, Anchor.TopLeft, ScaleBasis.BothHeight), "SlideDownArrow", true);
			group.Recalculate();
			if (text.TextSize.X > (float)text.Rect.Width)
			{
				text.ToolTip = text.Text;
				text.Text = ToolBox.LimitString(text.Text, text.Font, text.Rect.Width);
			}
			CS$<>8__locals1.frames = new List<GUIFrame>();
			if (CS$<>8__locals1.currentOrPending != null)
			{
				UpgradeStore.<>c__DisplayClass55_2 CS$<>8__locals4 = new UpgradeStore.<>c__DisplayClass55_2();
				CS$<>8__locals4.CS$<>8__locals1 = CS$<>8__locals1;
				bool flag;
				if (UpgradeStore.HasPermission)
				{
					if (CS$<>8__locals4.CS$<>8__locals1.item.PendingItemSwap == null)
					{
						SwappableItem swappableItem = CS$<>8__locals4.CS$<>8__locals1.currentOrPending.SwappableItem;
						flag = (swappableItem != null && !swappableItem.ReplacementOnUninstall.IsEmpty);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = false;
				}
				bool canUninstall = flag;
				UpgradeStore.<>c__DisplayClass55_2 CS$<>8__locals5 = CS$<>8__locals4;
				bool isUninstallPending;
				if (CS$<>8__locals4.CS$<>8__locals1.item.Prefab.SwappableItem != null)
				{
					ItemPrefab pendingItemSwap = CS$<>8__locals4.CS$<>8__locals1.item.PendingItemSwap;
					Identifier? identifier;
					Identifier? identifier2;
					if (pendingItemSwap == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						identifier2 = new Identifier?(pendingItemSwap.Identifier);
					}
					identifier = identifier2;
					Identifier? identifier3 = new Identifier?(CS$<>8__locals4.CS$<>8__locals1.item.Prefab.SwappableItem.ReplacementOnUninstall);
					isUninstallPending = (identifier == identifier3);
				}
				else
				{
					isUninstallPending = false;
				}
				CS$<>8__locals5.isUninstallPending = isUninstallPending;
				if (CS$<>8__locals4.isUninstallPending)
				{
					canUninstall = false;
				}
				CS$<>8__locals4.CS$<>8__locals1.frames.Add(UpgradeStore.CreateUpgradeEntry(UpgradeStore.rectT(1f, 0.35f, CS$<>8__locals4.CS$<>8__locals1.parent.Content, Anchor.TopLeft, ScaleBasis.Normal), CS$<>8__locals4.CS$<>8__locals1.currentOrPending.UpgradePreviewSprite, (CS$<>8__locals4.CS$<>8__locals1.item.PendingItemSwap != null) ? TextManager.GetWithVariable("upgrades.pendingitem", "[itemname]", name, FormatCapitals.No) : TextManager.GetWithVariable("upgrades.installeditem", "[itemname]", nameWithQuantity, FormatCapitals.No), CS$<>8__locals4.CS$<>8__locals1.currentOrPending.Description, 0, null, canUninstall, false, "WeaponUninstallButton", null, 0).Frame);
				if (canUninstall)
				{
					GUIButton refundButton = CS$<>8__locals4.CS$<>8__locals1.frames.Last<GUIFrame>().FindChild((GUIComponent c) => c is GUIButton, true) as GUIButton;
					if (refundButton != null)
					{
						refundButton.Enabled = true;
						GUIButton guibutton = refundButton;
						guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
						{
							string textTag = (CS$<>8__locals4.CS$<>8__locals1.item.PendingItemSwap != null) ? "upgrades.cancelitemswappromptbody" : "upgrades.itemuninstallpromptbody";
							if (CS$<>8__locals4.isUninstallPending)
							{
								textTag = "upgrades.cancelitemuninstallpromptbody";
							}
							LocalizedString promptBody = TextManager.GetWithVariable(textTag, "[itemtouninstall]", CS$<>8__locals4.isUninstallPending ? CS$<>8__locals4.CS$<>8__locals1.item.Name : CS$<>8__locals4.CS$<>8__locals1.currentOrPending.Name, FormatCapitals.No);
							UpgradeStore <>4__this = CS$<>8__locals4.CS$<>8__locals1.<>4__this;
							LocalizedString header = TextManager.Get("upgrades.refundprompttitle");
							LocalizedString body = promptBody;
							Func<bool> onConfirm;
							if ((onConfirm = CS$<>8__locals4.CS$<>8__locals1.<>9__8) == null)
							{
								onConfirm = (CS$<>8__locals4.CS$<>8__locals1.<>9__8 = delegate()
								{
									if (GameMain.NetworkMember != null)
									{
										UpgradeStore.WaitForServerUpdate = true;
									}
									CampaignMode campaign = CS$<>8__locals4.CS$<>8__locals1.<>4__this.Campaign;
									if (campaign != null)
									{
										campaign.UpgradeManager.CancelItemSwap(CS$<>8__locals4.CS$<>8__locals1.item, false, null);
									}
									GameClient client = GameMain.Client;
									if (client != null)
									{
										client.SendCampaignState();
									}
									return true;
								});
							}
							<>4__this.currectConfirmation = EventEditorScreen.AskForConfirmation(header, body, onConfirm, null);
							return true;
						}));
					}
				}
				GUIFrame dividerContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals4.CS$<>8__locals1.parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				new GUIFrame(new RectTransform(new Vector2(0.8f, 0.5f), dividerContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
				CS$<>8__locals4.CS$<>8__locals1.frames.Add(dividerContainer);
			}
			using (IEnumerator<ItemPrefab> enumerator2 = availableReplacements.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					UpgradeStore.<>c__DisplayClass55_3 CS$<>8__locals6 = new UpgradeStore.<>c__DisplayClass55_3();
					CS$<>8__locals6.CS$<>8__locals2 = CS$<>8__locals1;
					CS$<>8__locals6.replacement = enumerator2.Current;
					if (CS$<>8__locals6.replacement != CS$<>8__locals6.CS$<>8__locals2.currentOrPending)
					{
						bool isPurchased = CS$<>8__locals6.CS$<>8__locals2.item.AvailableSwaps.Contains(CS$<>8__locals6.replacement);
						int num;
						if (!isPurchased && CS$<>8__locals6.replacement != CS$<>8__locals6.CS$<>8__locals2.item.Prefab)
						{
							SwappableItem swappableItem2 = CS$<>8__locals6.replacement.SwappableItem;
							Map map = this.Campaign.Map;
							num = swappableItem2.GetPrice((map != null) ? map.CurrentLocation : null) * CS$<>8__locals6.CS$<>8__locals2.linkedItems.Count<Item>();
						}
						else
						{
							num = 0;
						}
						int price = num;
						CS$<>8__locals6.CS$<>8__locals2.frames.Add(UpgradeStore.CreateUpgradeEntry(UpgradeStore.rectT(1f, 0.35f, CS$<>8__locals6.CS$<>8__locals2.parent.Content, Anchor.TopLeft, ScaleBasis.Normal), CS$<>8__locals6.replacement.UpgradePreviewSprite, CS$<>8__locals6.replacement.Name, CS$<>8__locals6.replacement.Description, price, CS$<>8__locals6.replacement, true, false, isPurchased ? "WeaponInstallButton" : "StoreAddToCrateButton", null, 0).Frame);
						GUIButton buyButton = CS$<>8__locals6.CS$<>8__locals2.frames.Last<GUIFrame>().FindChild((GUIComponent c) => c is GUIButton, true) as GUIButton;
						if (buyButton != null)
						{
							if (UpgradeStore.HasPermission && this.PlayerBalance >= price)
							{
								buyButton.Enabled = true;
								GUIButton guibutton2 = buyButton;
								guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
								{
									string tag = isPurchased ? "upgrades.itemswappromptbody" : "upgrades.purchaseitemswappromptbody";
									ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
									array[0] = new ValueTuple<string, LocalizedString>("[itemtoinstall]", CS$<>8__locals6.replacement.Name);
									int num2 = 1;
									string item2 = "[amount]";
									SwappableItem swappableItem3 = CS$<>8__locals6.replacement.SwappableItem;
									CampaignMode campaign = CS$<>8__locals6.CS$<>8__locals2.<>4__this.Campaign;
									Location location;
									if (campaign == null)
									{
										location = null;
									}
									else
									{
										Map map2 = campaign.Map;
										location = ((map2 != null) ? map2.CurrentLocation : null);
									}
									array[num2] = new ValueTuple<string, LocalizedString>(item2, (swappableItem3.GetPrice(location) * CS$<>8__locals6.CS$<>8__locals2.linkedItems.Count<Item>()).ToString());
									LocalizedString promptBody = TextManager.GetWithVariables(tag, array);
									UpgradeStore <>4__this = CS$<>8__locals6.CS$<>8__locals2.<>4__this;
									LocalizedString header = TextManager.Get("Upgrades.PurchasePromptTitle");
									LocalizedString body = promptBody;
									Func<bool> onConfirm;
									if ((onConfirm = CS$<>8__locals6.<>9__11) == null)
									{
										onConfirm = (CS$<>8__locals6.<>9__11 = delegate()
										{
											if (GameMain.NetworkMember != null)
											{
												UpgradeStore.WaitForServerUpdate = true;
											}
											if (CS$<>8__locals6.CS$<>8__locals2.item.Prefab == CS$<>8__locals6.replacement && CS$<>8__locals6.CS$<>8__locals2.item.PendingItemSwap != null)
											{
												CampaignMode campaign2 = CS$<>8__locals6.CS$<>8__locals2.<>4__this.Campaign;
												if (campaign2 != null)
												{
													campaign2.UpgradeManager.CancelItemSwap(CS$<>8__locals6.CS$<>8__locals2.item, false, null);
												}
											}
											else
											{
												CampaignMode campaign3 = CS$<>8__locals6.CS$<>8__locals2.<>4__this.Campaign;
												if (campaign3 != null)
												{
													campaign3.UpgradeManager.PurchaseItemSwap(CS$<>8__locals6.CS$<>8__locals2.item, CS$<>8__locals6.replacement, false, null);
												}
											}
											GameClient client = GameMain.Client;
											if (client != null)
											{
												client.SendCampaignState();
											}
											return true;
										});
									}
									<>4__this.currectConfirmation = EventEditorScreen.AskForConfirmation(header, body, onConfirm, null);
									return true;
								}));
							}
							else
							{
								buyButton.Enabled = false;
							}
						}
					}
				}
			}
			foreach (GUIFrame frame in CS$<>8__locals1.frames)
			{
				frame.Visible = false;
			}
			CS$<>8__locals1.toggleButton.OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				if (CS$<>8__locals1.<>4__this.Campaign == null)
				{
					return false;
				}
				CS$<>8__locals1.isOpen = !CS$<>8__locals1.isOpen;
				CS$<>8__locals1.toggleButton.Selected = !CS$<>8__locals1.toggleButton.Selected;
				foreach (GUIFrame frame2 in CS$<>8__locals1.frames)
				{
					frame2.Visible = CS$<>8__locals1.toggleButton.Selected;
				}
				if (CS$<>8__locals1.toggleButton.Selected)
				{
					ICollection<Item> linkedItems2 = UpgradeManager.GetLinkedItemsToSwap(CS$<>8__locals1.item);
					foreach (KeyValuePair<Item, GUIComponent> itemPreview2 in CS$<>8__locals1.<>4__this.itemPreviews)
					{
						itemPreview2.Value.OutlineColor = (itemPreview2.Value.Color = (linkedItems2.Contains(itemPreview2.Key) ? GUIStyle.Orange : UpgradeStore.previewWhite));
					}
					using (IEnumerator<GUIComponent> enumerator6 = CS$<>8__locals1.toggleButton.Parent.Children.GetEnumerator())
					{
						while (enumerator6.MoveNext())
						{
							GUIComponent otherComponent = enumerator6.Current;
							if (otherComponent != CS$<>8__locals1.toggleButton && !CS$<>8__locals1.frames.Contains(otherComponent))
							{
								GUIButton otherButton = otherComponent as GUIButton;
								if (otherButton != null)
								{
									GUIComponent otherArrowImage = otherComponent.FindChild((GUIComponent c) => c is GUIImage, true);
									otherArrowImage.SpriteEffects = SpriteEffects.None;
									otherButton.Selected = false;
								}
								else
								{
									otherComponent.Visible = false;
								}
							}
						}
						goto IL_26E;
					}
				}
				using (Dictionary<Item, GUIComponent>.Enumerator enumerator7 = CS$<>8__locals1.<>4__this.itemPreviews.GetEnumerator())
				{
					while (enumerator7.MoveNext())
					{
						KeyValuePair<Item, GUIComponent> itemPreview = enumerator7.Current;
						GUIListBox guilistBox = CS$<>8__locals1.<>4__this.currentStoreLayout;
						object obj = (guilistBox != null) ? guilistBox.SelectedData : null;
						if (obj is UpgradeStore.CategoryData)
						{
							UpgradeStore.CategoryData categoryData = (UpgradeStore.CategoryData)obj;
							if (!categoryData.Category.ItemTags.Any((Identifier t) => itemPreview.Key.HasTag(t)))
							{
								continue;
							}
						}
						itemPreview.Value.OutlineColor = (itemPreview.Value.Color = GUIStyle.Orange);
					}
				}
				IL_26E:
				CS$<>8__locals1.<>4__this.activeItemSwapSlideDown = (CS$<>8__locals1.toggleButton.Selected ? CS$<>8__locals1.toggleButton : null);
				CS$<>8__locals1.arrowImage.SpriteEffects = (CS$<>8__locals1.toggleButton.Selected ? SpriteEffects.FlipVertically : SpriteEffects.None);
				CS$<>8__locals1.parent.RecalculateChildren();
				CS$<>8__locals1.parent.UpdateScrollBarSize();
				return true;
			};
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x000E6174 File Offset: 0x000E4374
		public static UpgradeStore.UpgradeFrame CreateUpgradeFrame(UpgradePrefab prefab, UpgradeCategory category, CampaignMode campaign, RectTransform rectTransform, bool addBuyButton = true)
		{
			UpgradePrice price2 = prefab.Price;
			int upgradeLevel = campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null);
			Map map = campaign.Map;
			int price = price2.GetBuyPrice(prefab, upgradeLevel, (map != null) ? map.CurrentLocation : null, UpgradeStore.characterList);
			return UpgradeStore.CreateUpgradeEntry(rectTransform, prefab.Sprite, prefab.Name, prefab.Description, price, new UpgradeStore.CategoryData(category, prefab), addBuyButton, true, "UpgradeBuyButton", prefab, campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null));
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x000E61F4 File Offset: 0x000E43F4
		public static UpgradeStore.UpgradeFrame CreateUpgradeEntry(RectTransform parent, Sprite sprite, LocalizedString title, LocalizedString body, int price, [Nullable(2)] object userData, bool addBuyButton = true, bool addProgressBar = true, string buttonStyle = "UpgradeBuyButton", [Nullable(2)] UpgradePrefab upgradePrefab = null, int currentLevel = 0)
		{
			float progressBarHeight = 0.25f;
			if (!addProgressBar)
			{
				progressBarHeight = 0f;
			}
			GUIFrame prefabFrame = new GUIFrame(parent, "ListBoxElement", null)
			{
				SelectedColor = Color.Transparent,
				UserData = userData
			};
			GUILayoutGroup mainLayout = new GUILayoutGroup(UpgradeStore.rectT(0.98f, 0.95f, prefabFrame, Anchor.Center, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup prefabLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, addBuyButton ? 0.65f : 1f, mainLayout, Anchor.Center, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup imageLayout = new GUILayoutGroup(UpgradeStore.rectT(new Point(prefabLayout.Rect.Height, prefabLayout.Rect.Height), prefabLayout, Anchor.TopLeft), false, Anchor.Center);
			GUIImage icon = new GUIImage(UpgradeStore.rectT(0.9f, 0.9f, imageLayout, Anchor.TopLeft, ScaleBasis.BothHeight), sprite, true, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup textLayout = new GUILayoutGroup(UpgradeStore.rectT(1f - imageLayout.RectTransform.RelativeSize.X, 1f, prefabLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = UpgradeStore.rectT(1f, 0.35f, textLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text = RichString.Rich(title, null);
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock name = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null)
			{
				AutoScaleHorizontal = true,
				AutoScaleVertical = true,
				Padding = Vector4.Zero
			};
			GUILayoutGroup descriptionLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.75f - progressBarHeight, textLayout, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT2 = UpgradeStore.rectT(1f, 1f, descriptionLayout, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text2 = body;
			font = GUIStyle.SmallFont;
			GUITextBlock description = new GUITextBlock(rectT2, text2, null, font, Alignment.TopLeft, true, "", null)
			{
				Padding = Vector4.Zero
			};
			GUILayoutGroup progressLayout = null;
			GUILayoutGroup buyButtonLayout = null;
			Option<UpgradeStore.BuyButtonFrame> buyButtonOption = Option<UpgradeStore.BuyButtonFrame>.None();
			Option<UpgradeStore.ProgressBarFrame> progressBarOption = Option<UpgradeStore.ProgressBarFrame>.None();
			if (addProgressBar)
			{
				progressLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.25f, textLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					UserData = UpgradeStore.UpgradeStoreUserData.ProgressBarLayout
				};
				RectTransform rectT3 = UpgradeStore.rectT(0.15f, 1f, progressLayout, Anchor.TopLeft, ScaleBasis.Normal);
				RichString text3 = string.Empty;
				font = GUIStyle.SmallFont;
				GUITextBlock progressText = new GUITextBlock(rectT3, text3, null, font, Alignment.Center, false, "", null)
				{
					Padding = Vector4.Zero
				};
				GUIProgressBar progressBar = new GUIProgressBar(UpgradeStore.rectT(0.85f, 0.75f, progressLayout, Anchor.TopLeft, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Orange), "", true);
				progressBarOption = Option.Some<UpgradeStore.ProgressBarFrame>(new UpgradeStore.ProgressBarFrame(progressText, progressBar));
			}
			if (addBuyButton)
			{
				LocalizedString formattedPrice = TextManager.FormatCurrency(Math.Abs(price), true);
				if (price < 0)
				{
					formattedPrice = "+" + formattedPrice;
				}
				buyButtonLayout = new GUILayoutGroup(UpgradeStore.rectT(1f, 0.35f, mainLayout, Anchor.TopLeft, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					UserData = UpgradeStore.UpgradeStoreUserData.BuyButtonLayout
				};
				GUIListBox materialCostList;
				if (upgradePrefab != null)
				{
					RectTransform rectT4 = UpgradeStore.rectT(imageLayout.RectTransform.RelativeSize.X, 1f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal);
					RichString text4 = "";
					font = GUIStyle.SubHeadingFont;
					GUITextBlock increaseText = new GUITextBlock(rectT4, text4, null, font, Alignment.Center, false, "", null)
					{
						UserData = UpgradeStore.UpgradeStoreUserData.IncreaseLabel
					};
					UpgradeStore.UpdateUpgradePercentageText(increaseText, upgradePrefab, currentLevel);
					materialCostList = new GUIListBox(UpgradeStore.rectT(0.65f - imageLayout.RectTransform.RelativeSize.X, 1f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), true, null, null, true, false);
				}
				else
				{
					materialCostList = new GUIListBox(UpgradeStore.rectT(0.65f, 1f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), true, null, null, true, false);
				}
				materialCostList.Visible = false;
				materialCostList.UserData = UpgradeStore.UpgradeStoreUserData.MaterialCostList;
				GUITextBlock priceText = new GUITextBlock(UpgradeStore.rectT(0.2f, 1f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), formattedPrice, null, null, Alignment.CenterRight, false, "", null)
				{
					UserData = UpgradeStore.UpgradeStoreUserData.PriceLabel,
					Visible = (userData is ItemPrefab)
				};
				if (price < 0)
				{
					priceText.TextColor = GUIStyle.Green;
				}
				else if (price == 0)
				{
					priceText.Text = string.Empty;
				}
				GUIButton buyButton = new GUIButton(UpgradeStore.rectT(0.15f, 1f, buyButtonLayout, Anchor.TopLeft, ScaleBasis.Normal), string.Empty, Alignment.Center, buttonStyle, null)
				{
					UserData = UpgradeStore.UpgradeStoreUserData.BuyButton,
					Enabled = false
				};
				buyButtonOption = Option.Some<UpgradeStore.BuyButtonFrame>(new UpgradeStore.BuyButtonFrame(buyButtonLayout, materialCostList, buyButton, priceText));
			}
			description.CalculateHeightFromText(0, false);
			int i = 100;
			while (i > 0 && description.Rect.Height > descriptionLayout.Rect.Height)
			{
				IReadOnlyList<LocalizedString> lines = description.WrappedText.Split(new char[]
				{
					'\n'
				});
				string newString = string.Join<LocalizedString>('\n', lines.Take(lines.Count - 1));
				if (0 >= newString.Length - 4)
				{
					break;
				}
				description.Text = newString.Substring(0, newString.Length - 4) + "...";
				description.CalculateHeightFromText(0, false);
				description.ToolTip = body;
				i--;
			}
			GUILayoutGroup group = parent.Parent.GUIComponent as GUILayoutGroup;
			if (group != null)
			{
				group.Recalculate();
			}
			descriptionLayout.Recalculate();
			prefabLayout.Recalculate();
			imageLayout.Recalculate();
			textLayout.Recalculate();
			if (progressLayout != null)
			{
				progressLayout.Recalculate();
			}
			if (buyButtonLayout != null)
			{
				buyButtonLayout.Recalculate();
			}
			return new UpgradeStore.UpgradeFrame(prefabFrame, icon, name, description, buyButtonOption, progressBarOption);
		}

		// Token: 0x0600177C RID: 6012 RVA: 0x000E67E8 File Offset: 0x000E49E8
		private static void UpdateUpgradePercentageText(GUITextBlock text, UpgradePrefab upgradePrefab, int currentLevel)
		{
			int maxLevel = upgradePrefab.GetMaxLevelForCurrentSub();
			float nextIncrease = upgradePrefab.IncreaseOnTooltip * (float)Math.Min(currentLevel + 1, maxLevel);
			if (nextIncrease != 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendFormatted<double>(Math.Round((double)nextIncrease, 1));
				defaultInterpolatedStringHandler.AppendLiteral(" %");
				text.Text = defaultInterpolatedStringHandler.ToStringAndClear();
				if (currentLevel == maxLevel)
				{
					text.TextColor = Color.Gray;
				}
			}
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x000E685C File Offset: 0x000E4A5C
		private void CreateUpgradeEntry(UpgradePrefab prefab, UpgradeCategory category, GUIComponent parent, Submarine submarine, [Nullable(new byte[]
		{
			2,
			1
		})] List<Item> itemsOnSubmarine)
		{
			GameSession gameSession = GameMain.GameSession;
			Submarine sub = ((gameSession != null) ? gameSession.Submarine : null) ?? Submarine.MainSub;
			if (this.Campaign == null || sub == null)
			{
				return;
			}
			UpgradeStore.UpgradeFrame prefabFrame = UpgradeStore.CreateUpgradeFrame(prefab, category, this.Campaign, UpgradeStore.rectT(1f, 0.4f, parent, Anchor.TopLeft, ScaleBasis.Normal), true);
			UpgradeStore.BuyButtonFrame buyButtonFrame;
			if (!prefabFrame.BuyButton.TryUnwrap(out buyButtonFrame))
			{
				return;
			}
			if (!prefab.IsApplicable(submarine.Info) || (itemsOnSubmarine != null && !itemsOnSubmarine.Any((Item it) => category.CanBeApplied(it, prefab))))
			{
				prefabFrame.Frame.Enabled = false;
				prefabFrame.Description.Enabled = false;
				prefabFrame.Name.Enabled = false;
				prefabFrame.Icon.Color = Color.Gray;
				buyButtonFrame.BuyButton.Enabled = false;
				buyButtonFrame.Layout.UserData = null;
			}
			GUIButton buyButton = buyButtonFrame.BuyButton;
			Func<bool> <>9__2;
			buyButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(buyButton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
			{
				string tag = "Upgrades.PurchasePromptBody";
				ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
				array[0] = new ValueTuple<string, LocalizedString>("[upgradename]", prefab.Name);
				int num = 1;
				string item = "[amount]";
				UpgradePrice price = prefab.Price;
				UpgradePrefab prefab2 = prefab;
				int upgradeLevel = this.Campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null);
				Map map = this.Campaign.Map;
				array[num] = new ValueTuple<string, LocalizedString>(item, price.GetBuyPrice(prefab2, upgradeLevel, (map != null) ? map.CurrentLocation : null, UpgradeStore.characterList).ToString());
				LocalizedString promptBody = TextManager.GetWithVariables(tag, array);
				UpgradeStore <>4__this = this;
				LocalizedString header = TextManager.Get("Upgrades.PurchasePromptTitle");
				LocalizedString body = promptBody;
				Func<bool> onConfirm;
				if ((onConfirm = <>9__2) == null)
				{
					onConfirm = (<>9__2 = delegate()
					{
						if (this.Campaign.UpgradeManager.TryPurchaseUpgrade(prefab, category, false, null))
						{
							if (GameMain.NetworkMember != null)
							{
								UpgradeStore.WaitForServerUpdate = true;
							}
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.SendCampaignState();
							}
						}
						return true;
					});
				}
				<>4__this.currectConfirmation = EventEditorScreen.AskForConfirmation(header, body, onConfirm, new GUISoundType?(GUISoundType.ConfirmTransaction));
				return true;
			}));
			UpgradeStore.UpdateUpgradeEntry(prefabFrame.Frame, prefab, category, this.Campaign);
		}

		// Token: 0x0600177E RID: 6014 RVA: 0x000E69B0 File Offset: 0x000E4BB0
		private void CreateItemTooltip(MapEntity entity)
		{
			int slotIndex = -1;
			Item swappableItem = entity as Item;
			if (swappableItem != null && swappableItem.Prefab.SwappableItem != null)
			{
				List<Item> entitiesOnSub = Submarine.MainSub.GetItems(true).Where(delegate(Item i)
				{
					if (i.Prefab.SwappableItem != null && Submarine.MainSub.IsEntityFoundOnThisSub(i, true, false, false))
					{
						string swapIdentifier = i.Prefab.SwappableItem.SwapIdentifier;
						SwappableItem swappableItem = swappableItem.Prefab.SwappableItem;
						return swapIdentifier == ((swappableItem != null) ? swappableItem.SwapIdentifier : null);
					}
					return false;
				}).ToList<Item>();
				slotIndex = entitiesOnSub.IndexOf(entity) + 1;
			}
			GUITextBlock itemName = this.ItemInfoFrame.FindChild("itemname", true) as GUITextBlock;
			GUIListBox upgradeList = this.ItemInfoFrame.FindChild("upgradelist", true) as GUIListBox;
			GUITextBlock moreIndicator = this.ItemInfoFrame.FindChild("moreindicator", true) as GUITextBlock;
			GUILayoutGroup layout = this.ItemInfoFrame.GetChild<GUILayoutGroup>();
			List<Upgrade> upgrades = entity.GetUpgrades();
			int upgradesCount = upgrades.Count;
			Item item = entity as Item;
			itemName.Text = (((item != null) ? item.Prefab.Name : null) ?? TextManager.Get("upgradecategory.walls"));
			if (slotIndex > -1)
			{
				itemName.Text = TextManager.GetWithVariables("weaponslotwithname", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[number]", slotIndex.ToString()),
					new ValueTuple<string, LocalizedString>("[weaponname]", itemName.Text)
				});
			}
			if (((item != null) ? item.PendingItemSwap : null) != null)
			{
				GUITextBlock guitextBlock = itemName;
				RichString text = itemName.Text;
				guitextBlock.Text = RichString.Rich(((text != null) ? text.ToString() : null) + "\n" + TextManager.GetWithVariable("upgrades.pendingitem", "[itemname]", item.PendingItemSwap.Name, FormatCapitals.No), null);
			}
			upgradeList.Content.ClearChildren();
			int j = 0;
			while (j < upgrades.Count && j < 4)
			{
				Upgrade upgrade = upgrades[j];
				GUITextBlock guitextBlock2 = new GUITextBlock(UpgradeStore.rectT(1f, 0.25f, upgradeList.Content, Anchor.TopLeft, ScaleBasis.Normal), UpgradeStore.<CreateItemTooltip>g__CreateListEntry|63_0(upgrade.Prefab.Name, upgrade.Level), null, null, Alignment.Left, false, "", null);
				guitextBlock2.AutoScaleHorizontal = true;
				guitextBlock2.UserData = Tuple.Create<int, UpgradePrefab>(upgrade.Level, upgrade.Prefab);
				j++;
			}
			CampaignMode campaign = this.Campaign;
			UpgradeManager upgradeManager = (campaign != null) ? campaign.UpgradeManager : null;
			if (upgradeManager == null)
			{
				return;
			}
			foreach (PurchasedUpgrade purchasedUpgrade in upgradeManager.PendingUpgrades)
			{
				UpgradePrefab upgradePrefab;
				UpgradeCategory upgradeCategory;
				int num;
				purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out num);
				UpgradePrefab prefab = upgradePrefab;
				UpgradeCategory category = upgradeCategory;
				int level = num;
				if ((item != null && category.CanBeApplied(item, prefab)) || (entity is Structure && category.IsWallUpgrade))
				{
					bool found = false;
					foreach (GUITextBlock textBlock in (from c in upgradeList.Content.Children
					where c is GUITextBlock
					select c).Cast<GUITextBlock>())
					{
						Tuple<int, UpgradePrefab> tuple = textBlock.UserData as Tuple<int, UpgradePrefab>;
						if (tuple != null && tuple.Item2 == prefab)
						{
							LocalizedString tooltip = UpgradeStore.<CreateItemTooltip>g__CreateListEntry|63_0(tuple.Item2.Name, level + tuple.Item1);
							textBlock.Text = tooltip;
							found = true;
							break;
						}
					}
					if (!found)
					{
						upgradesCount++;
						if (upgradeList.Content.CountChildren < 4)
						{
							new GUITextBlock(UpgradeStore.rectT(1f, 0.25f, upgradeList.Content, Anchor.TopLeft, ScaleBasis.Normal), UpgradeStore.<CreateItemTooltip>g__CreateListEntry|63_0(prefab.Name, level), null, null, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
						}
					}
				}
			}
			if (!upgradeList.Content.Children.Any<GUIComponent>())
			{
				new GUITextBlock(UpgradeStore.rectT(1f, 0.25f, upgradeList.Content, Anchor.TopLeft, ScaleBasis.Normal), TextManager.Get("UpgradeUITooltip.NoUpgradesElement"), null, null, Alignment.Left, false, "", null).AutoScaleHorizontal = true;
			}
			GUITextBlock guitextBlock3 = moreIndicator;
			LocalizedString lStr;
			if (upgradesCount <= 4)
			{
				lStr = string.Empty;
			}
			else
			{
				string tag = "upgradeuitooltip.moreindicator";
				string varName = "[amount]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(upgradesCount - 4);
				lStr = TextManager.GetWithVariable(tag, varName, defaultInterpolatedStringHandler.ToStringAndClear(), FormatCapitals.No);
			}
			guitextBlock3.Text = lStr;
			itemName.CalculateHeightFromText(0, false);
			moreIndicator.CalculateHeightFromText(0, false);
			layout.Recalculate();
		}

		// Token: 0x0600177F RID: 6015 RVA: 0x000E6EB0 File Offset: 0x000E50B0
		public static IEnumerable<UpgradeCategory> GetApplicableCategories(Submarine drawnSubmarine)
		{
			UpgradeStore.<GetApplicableCategories>d__64 <GetApplicableCategories>d__ = new UpgradeStore.<GetApplicableCategories>d__64(-2);
			<GetApplicableCategories>d__.<>3__drawnSubmarine = drawnSubmarine;
			return <GetApplicableCategories>d__;
		}

		// Token: 0x06001780 RID: 6016 RVA: 0x000E6EC0 File Offset: 0x000E50C0
		private void UpdateSubmarinePreview(float deltaTime, GUICustomComponent parent)
		{
			if (this.Campaign == null)
			{
				return;
			}
			if (!parent.Children.Any<GUIComponent>() || (Submarine.MainSub != null && Submarine.MainSub != this.drawnSubmarine) || GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y)
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					SubmarineInfo submarineInfo = gameSession.SubmarineInfo;
					if (submarineInfo != null)
					{
						submarineInfo.CheckSubsLeftBehind(null);
					}
				}
				this.drawnSubmarine = Submarine.MainSub;
				if (this.drawnSubmarine != null)
				{
					this.CreateSubmarinePreview(this.drawnSubmarine, parent);
					this.CreateHullBorderVerticies(this.drawnSubmarine, parent);
					this.applicableCategories.Clear();
					this.applicableCategories.AddRange(UpgradeStore.GetApplicableCategories(this.drawnSubmarine));
				}
				this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
				this.RefreshAll();
			}
			if (this.needsRefresh)
			{
				this.RefreshAll();
			}
			if (PlayerInput.KeyHit(Keys.Enter) && GUIMessageBox.MessageBoxes.Any<GUIComponent>())
			{
				for (int i = GUIMessageBox.MessageBoxes.Count - 1; i >= 0; i--)
				{
					GUIMessageBox msgBox = GUIMessageBox.MessageBoxes[i] as GUIMessageBox;
					if (msgBox != null && msgBox == this.currectConfirmation)
					{
						GUIButton firstButton = msgBox.Buttons.FirstOrDefault<GUIButton>();
						if (firstButton != null)
						{
							firstButton.OnClicked(firstButton, firstButton.UserData);
						}
					}
				}
			}
			bool found = false;
			using (Dictionary<Item, GUIComponent>.Enumerator enumerator = this.itemPreviews.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					UpgradeStore.<>c__DisplayClass65_0 CS$<>8__locals1 = new UpgradeStore.<>c__DisplayClass65_0();
					CS$<>8__locals1.<>4__this = this;
					KeyValuePair<Item, GUIComponent> keyValuePair = enumerator.Current;
					Item item;
					GUIComponent guicomponent;
					keyValuePair.Deconstruct(out item, out guicomponent);
					CS$<>8__locals1.item = item;
					GUIComponent frame = guicomponent;
					if (GUI.MouseOn == frame)
					{
						if (this.HoveredEntity != CS$<>8__locals1.item)
						{
							this.CreateItemTooltip(CS$<>8__locals1.item);
						}
						this.HoveredEntity = CS$<>8__locals1.item;
						if (PlayerInput.PrimaryMouseButtonClicked() && this.selectedUpgradeTab == UpgradeStore.UpgradeTab.Upgrade && this.currentStoreLayout != null)
						{
							if (this.customizeTabOpen)
							{
								if (this.selectedUpgradeCategoryLayout != null)
								{
									UpgradeStore.<>c__DisplayClass65_1 CS$<>8__locals2 = new UpgradeStore.<>c__DisplayClass65_1();
									CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
									UpgradeStore.<>c__DisplayClass65_1 CS$<>8__locals3 = CS$<>8__locals2;
									Item hoveredItem = this.HoveredEntity as Item;
									ICollection<Item> linkedItems;
									if (hoveredItem == null)
									{
										ICollection<Item> collection = new List<Item>();
										linkedItems = collection;
									}
									else
									{
										linkedItems = UpgradeManager.GetLinkedItemsToSwap(hoveredItem);
									}
									CS$<>8__locals3.linkedItems = linkedItems;
									GUIButton itemElement = this.selectedUpgradeCategoryLayout.FindChild(delegate(GUIComponent c)
									{
										Item item2 = c.UserData as Item;
										return item2 != null && (item2 == CS$<>8__locals2.CS$<>8__locals1.<>4__this.HoveredEntity || CS$<>8__locals2.linkedItems.Contains(item2));
									}, true) as GUIButton;
									if (itemElement != null)
									{
										if (!itemElement.Selected)
										{
											itemElement.OnClicked(itemElement, itemElement.UserData);
										}
										GUIComponent parent2 = itemElement.Parent;
										object obj;
										if (parent2 == null)
										{
											obj = null;
										}
										else
										{
											GUIComponent parent3 = parent2.Parent;
											obj = ((parent3 != null) ? parent3.Parent : null);
										}
										GUIListBox guilistBox = obj as GUIListBox;
										if (guilistBox != null)
										{
											guilistBox.ScrollToElement(itemElement, GUIListBox.PlaySelectSound.No);
										}
									}
									else
									{
										this.ScrollToCategory((UpgradeStore.CategoryData data) => data.Category.CanBeApplied(CS$<>8__locals2.CS$<>8__locals1.item, null), GUIListBox.PlaySelectSound.No);
									}
								}
							}
							else
							{
								this.ScrollToCategory((UpgradeStore.CategoryData data) => data.Category.CanBeApplied(CS$<>8__locals1.item, null), GUIListBox.PlaySelectSound.No);
							}
						}
						found = true;
						break;
					}
				}
			}
			if (!found)
			{
				bool isMouseOnStructure = false;
				if (GUI.MouseOn == this.submarinePreviewComponent || GUI.MouseOn == this.subPreviewFrame)
				{
					Structure firstStructure = this.submarineWalls.FirstOrDefault<Structure>();
					if (this.subHullVertices.Any((Vector2[] hullVertex) => ToolBox.PointIntersectsWithPolygon(PlayerInput.MousePosition, hullVertex, true)))
					{
						if (this.HoveredEntity != firstStructure && firstStructure != null)
						{
							this.CreateItemTooltip(firstStructure);
						}
						this.HoveredEntity = firstStructure;
						isMouseOnStructure = true;
						GUI.MouseCursor = CursorState.Hand;
						if (PlayerInput.PrimaryMouseButtonClicked() && this.selectedUpgradeTab == UpgradeStore.UpgradeTab.Upgrade && this.currentStoreLayout != null)
						{
							this.ScrollToCategory((UpgradeStore.CategoryData data) => data.Category.IsWallUpgrade, GUIListBox.PlaySelectSound.Yes);
						}
					}
				}
				if (!isMouseOnStructure)
				{
					this.HoveredEntity = null;
				}
			}
			this.ItemInfoFrame.RectTransform.ScreenSpaceOffset = (PlayerInput.MousePosition + new Vector2(20f, 20f)).ToPoint();
			if (this.ItemInfoFrame.Rect.Right > GameMain.GraphicsWidth)
			{
				this.ItemInfoFrame.RectTransform.ScreenSpaceOffset = (PlayerInput.MousePosition - new Vector2((float)(20 + this.ItemInfoFrame.Rect.Width), -20f)).ToPoint();
			}
		}

		// Token: 0x06001781 RID: 6017 RVA: 0x000E7344 File Offset: 0x000E5544
		private void CreateSubmarinePreview(Submarine submarine, GUIComponent parent)
		{
			UpgradeStore.<>c__DisplayClass66_0 CS$<>8__locals1 = new UpgradeStore.<>c__DisplayClass66_0();
			CS$<>8__locals1.submarine = submarine;
			if (this.mainStoreLayout == null)
			{
				return;
			}
			if (this.submarineInfoFrame != null && this.mainStoreLayout == this.submarineInfoFrame.Parent)
			{
				this.mainStoreLayout.RemoveChild(this.submarineInfoFrame);
			}
			parent.ClearChildren();
			this.submarineInfoFrame = new GUILayoutGroup(UpgradeStore.rectT(0.25f, 0.2f, this.mainStoreLayout, Anchor.TopRight, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				IgnoreLayoutGroups = true
			};
			RectTransform rectT = UpgradeStore.rectT(1f, 0f, this.submarineInfoFrame, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text = CS$<>8__locals1.submarine.Info.DisplayName;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Right, false, "", null);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			string tag = "submarineclass.classsuffixformat";
			string varName = "[type]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("submarineclass.");
			defaultInterpolatedStringHandler2.AppendFormatted<SubmarineClass>(CS$<>8__locals1.submarine.Info.SubmarineClass);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.GetWithVariable(tag, varName, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()), FormatCapitals.No));
			LocalizedString classText = defaultInterpolatedStringHandler.ToStringAndClear();
			RectTransform rectT2 = UpgradeStore.rectT(1f, 0.15f, this.submarineInfoFrame, Anchor.TopLeft, ScaleBasis.Normal);
			RichString text2 = classText;
			font = GUIStyle.Font;
			GUIComponent guicomponent = new GUITextBlock(rectT2, text2, null, font, Alignment.Right, false, "", null);
			LocalizedString left = TextManager.Get("submarineclass.description") + "\n\n";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("submarineclass.");
			defaultInterpolatedStringHandler3.AppendFormatted<SubmarineClass>(CS$<>8__locals1.submarine.Info.SubmarineClass);
			defaultInterpolatedStringHandler3.AppendLiteral(".description");
			guicomponent.ToolTip = left + TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			RectTransform rectT3 = UpgradeStore.rectT(1f, 0.15f, this.submarineInfoFrame, Anchor.TopLeft, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("submarinetier.");
			defaultInterpolatedStringHandler4.AppendFormatted<int>(CS$<>8__locals1.submarine.Info.Tier);
			RichString text3 = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
			font = GUIStyle.Font;
			new GUITextBlock(rectT3, text3, null, font, Alignment.Right, false, "", null).ToolTip = TextManager.Get("submarinetier.description");
			GUITextBlock description = new GUITextBlock(UpgradeStore.rectT(1f, 0f, this.submarineInfoFrame, Anchor.TopLeft, ScaleBasis.Normal), CS$<>8__locals1.submarine.Info.Description, null, null, Alignment.Right, true, "", null);
			this.submarineInfoFrame.RectTransform.ScreenSpaceOffset = new Point(0, (int)(16f * GUI.Scale));
			description.Padding = new Vector4(description.Padding.X, 24f * GUI.Scale, description.Padding.Z, description.Padding.W);
			List<Entity> pointsOfInterest = (from category in UpgradeCategory.Categories
			from item in CS$<>8__locals1.submarine.GetItems(true)
			where (category.CanBeApplied(item, null) || UpgradeStore.HasSwappableItems(category, item)) && item.IsPlayerTeamInteractable
			select item).Cast<Entity>().Distinct<Entity>().ToList<Entity>();
			UpgradeStore.<>c__DisplayClass66_0 CS$<>8__locals2 = CS$<>8__locals1;
			SubmarineInfo submarineInfo = GameMain.GameSession.SubmarineInfo;
			CS$<>8__locals2.ids = (((submarineInfo != null) ? submarineInfo.LeftBehindDockingPortIDs : null) ?? new List<ushort>());
			pointsOfInterest.AddRange(from item in CS$<>8__locals1.submarine.GetItems(true)
			where CS$<>8__locals1.ids.Contains(item.ID)
			select item);
			CS$<>8__locals1.submarine.CreateMiniMap(parent, pointsOfInterest, true);
			this.subPreviewFrame = parent.GetChild<GUIFrame>();
			Rectangle dockedBorders = CS$<>8__locals1.submarine.GetDockedBorders(true);
			GUIFrame hullContainer = parent.GetChild<GUIFrame>();
			if (hullContainer == null)
			{
				return;
			}
			this.itemPreviews.Clear();
			foreach (Entity entity in pointsOfInterest)
			{
				GUIComponent component = parent.FindChild(entity, true);
				if (component != null)
				{
					Item item2 = entity as Item;
					if (item2 != null)
					{
						Sprite icon = item2.Prefab.UpgradePreviewSprite;
						GUIComponent itemFrame;
						if (icon != null)
						{
							float spriteSize = 128f * item2.Prefab.UpgradePreviewScale;
							Point size = new Point((int)(spriteSize * item2.Scale / (float)dockedBorders.Width * (float)hullContainer.Rect.Width));
							itemFrame = new GUIImage(UpgradeStore.rectT(size, component, Anchor.Center), icon, true, null)
							{
								SelectedColor = GUIStyle.Orange,
								Color = UpgradeStore.previewWhite,
								HoverCursor = CursorState.Hand,
								SpriteEffects = ((item2.Rotation > 90f && item2.Rotation < 270f) ? SpriteEffects.FlipVertically : SpriteEffects.None)
							};
							if (item2.Prefab.SwappableItem != null)
							{
								GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.8f), itemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
								{
									RelativeOffset = new Vector2(-0.2f)
								}, "WeaponSwitchIcon.DropShadow", true);
								guiimage.SelectedColor = GUIStyle.Orange;
								guiimage.Color = UpgradeStore.previewWhite;
								guiimage.CanBeFocused = false;
							}
						}
						else
						{
							Point size2 = new Point((int)((float)item2.Rect.Width * item2.Scale / (float)dockedBorders.Width * (float)hullContainer.Rect.Width), (int)((float)item2.Rect.Height * item2.Scale / (float)dockedBorders.Height * (float)hullContainer.Rect.Height));
							itemFrame = new GUIFrame(UpgradeStore.rectT(size2, component, Anchor.Center), "ScanLines", null)
							{
								SelectedColor = GUIStyle.Orange,
								OutlineColor = UpgradeStore.previewWhite,
								Color = UpgradeStore.previewWhite,
								OutlineThickness = 2f,
								HoverCursor = CursorState.Hand
							};
						}
						if (!this.itemPreviews.ContainsKey(item2))
						{
							this.itemPreviews.Add(item2, itemFrame);
						}
					}
				}
			}
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x000E7A08 File Offset: 0x000E5C08
		private void CreateHullBorderVerticies(Submarine sub, GUIComponent parent)
		{
			this.submarineWalls = sub.GetWalls(true);
			if (sub.HullVertices == null)
			{
				return;
			}
			Rectangle dockedBorders = sub.GetDockedBorders(true);
			dockedBorders.Location += sub.WorldPosition.ToPoint();
			float scale = Math.Min((float)parent.Rect.Width / (float)dockedBorders.Width, (float)parent.Rect.Height / (float)dockedBorders.Height) * 0.9f;
			float displayScale = ConvertUnits.ToDisplayUnits(scale);
			Vector2 offset = (sub.WorldPosition - new Vector2((float)dockedBorders.Center.X, (float)(dockedBorders.Y - dockedBorders.Height / 2))) * scale;
			Vector2 center = parent.Rect.Center.ToVector2();
			this.subHullVertices = new Vector2[sub.HullVertices.Count][];
			for (int i = 0; i < sub.HullVertices.Count; i++)
			{
				Vector2 start = sub.HullVertices[i] * displayScale + offset;
				start.Y = -start.Y;
				Vector2 end = sub.HullVertices[(i + 1) % sub.HullVertices.Count] * displayScale + offset;
				end.Y = -end.Y;
				Vector2 edge = end - start;
				float length = edge.Length();
				float angle = (float)Math.Atan2((double)edge.Y, (double)edge.X);
				Matrix rotate = Matrix.CreateRotationZ(angle);
				this.subHullVertices[i] = new Vector2[]
				{
					center + start + Vector2.Transform(new Vector2(length, -10f), rotate),
					center + end + Vector2.Transform(new Vector2(-length, -10f), rotate),
					center + end + Vector2.Transform(new Vector2(-length, 10f), rotate),
					center + start + Vector2.Transform(new Vector2(length, 10f), rotate)
				};
			}
		}

		// Token: 0x06001783 RID: 6019 RVA: 0x000E7C5C File Offset: 0x000E5E5C
		private void DrawSubmarine(SpriteBatch spriteBatch, GUICustomComponent component)
		{
			foreach (Vector2[] hullVertex in this.subHullVertices)
			{
				Vector2 point = hullVertex[1] + (hullVertex[2] - hullVertex[1]) / 2f;
				Vector2 point2 = hullVertex[0] + (hullVertex[3] - hullVertex[0]) / 2f;
				GUI.DrawLine(spriteBatch, point, point2, this.highlightWalls ? (GUIStyle.Orange * 0.6f) : (Color.DarkCyan * 0.3f), 0f, 10f);
				if (GameMain.DebugDraw)
				{
					GUI.DrawRectangle(spriteBatch, hullVertex, Color.Red, 0f, 1f);
				}
			}
		}

		// Token: 0x06001784 RID: 6020 RVA: 0x000E7D38 File Offset: 0x000E5F38
		public static void UpdateUpgradeEntry(GUIComponent prefabFrame, UpgradePrefab prefab, UpgradeCategory category, CampaignMode campaign)
		{
			int currentLevel = campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null);
			int maxLevel = prefab.GetMaxLevelForCurrentSub();
			LocalizedString progressText = TextManager.GetWithVariables("upgrades.progressformat", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[level]", currentLevel.ToString()),
				new ValueTuple<string, string>("[maxlevel]", maxLevel.ToString())
			});
			GUIComponent progressParent = prefabFrame.FindChild(UpgradeStore.UpgradeStoreUserData.ProgressBarLayout, true);
			if (progressParent != null)
			{
				GUIProgressBar bar = progressParent.GetChild<GUIProgressBar>();
				if (bar != null)
				{
					bar.BarSize = (float)currentLevel / (float)maxLevel;
					bar.Color = ((currentLevel >= maxLevel) ? GUIStyle.Green : GUIStyle.Orange);
				}
				GUITextBlock block = progressParent.GetChild<GUITextBlock>();
				if (block != null)
				{
					block.Text = progressText;
				}
			}
			GUIComponent buttonParent = prefabFrame.FindChild(UpgradeStore.UpgradeStoreUserData.BuyButtonLayout, true);
			if (buttonParent == null)
			{
				return;
			}
			GUITextBlock priceLabel = (GUITextBlock)buttonParent.FindChild(UpgradeStore.UpgradeStoreUserData.PriceLabel, true);
			priceLabel.Visible = true;
			UpgradePrice price2 = prefab.Price;
			int upgradeLevel = campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null);
			Map map = campaign.Map;
			int price = price2.GetBuyPrice(prefab, upgradeLevel, (map != null) ? map.CurrentLocation : null, UpgradeStore.characterList);
			if (!UpgradeStore.WaitForServerUpdate)
			{
				priceLabel.Text = TextManager.FormatCurrency(price, true);
				if (currentLevel >= maxLevel)
				{
					priceLabel.Text = TextManager.Get("Upgrade.MaxedUpgrade");
				}
			}
			GUITextBlock increaseLabel = buttonParent.FindChild(UpgradeStore.UpgradeStoreUserData.IncreaseLabel, true) as GUITextBlock;
			if (increaseLabel != null && !UpgradeStore.WaitForServerUpdate)
			{
				UpgradeStore.UpdateUpgradePercentageText(increaseLabel, prefab, currentLevel);
			}
			bool isMax = currentLevel >= maxLevel;
			GUIButton button = buttonParent.FindChild(UpgradeStore.UpgradeStoreUserData.BuyButton, true) as GUIButton;
			if (button != null)
			{
				bool canBuy = !UpgradeStore.WaitForServerUpdate && UpgradeStore.HasPermission && !isMax && campaign.GetBalance(null) >= price && prefab.HasResourcesToUpgrade(Character.Controlled, currentLevel + 1);
				button.Enabled = canBuy;
			}
			GUIListBox itemList = prefabFrame.FindChild(UpgradeStore.UpgradeStoreUserData.MaterialCostList, true) as GUIListBox;
			if (itemList != null)
			{
				if (isMax)
				{
					itemList.Visible = false;
					return;
				}
				UpgradeStore.<UpdateUpgradeEntry>g__CreateMaterialCosts|69_0(itemList, prefab, currentLevel + 1);
			}
		}

		// Token: 0x06001785 RID: 6021 RVA: 0x000E7F4C File Offset: 0x000E614C
		private static void UpdateCategoryIndicators(GUIComponent indicators, GUIComponent parent, List<UpgradePrefab> prefabs, UpgradeCategory category, CampaignMode campaign, [Nullable(2)] Submarine drawnSubmarine, IEnumerable<UpgradeCategory> applicableCategories)
		{
			if (!category.IsWallUpgrade)
			{
				Submarine submarine = drawnSubmarine;
				if (((submarine != null) ? submarine.Info : null) != null)
				{
					if (UpgradePrefab.Prefabs.None((UpgradePrefab p) => p.UpgradeCategories.Contains(category) && p.GetMaxLevel(drawnSubmarine.Info) > 0) && !UpgradeStore.HasSwappableItems(category, null))
					{
						parent.ToolTip = TextManager.Get("upgradecategorynotapplicable");
						parent.Enabled = false;
						parent.SelectedColor = GUIStyle.Red * 0.5f;
					}
					else if (applicableCategories.Contains(category))
					{
						parent.Enabled = true;
						parent.SelectedColor = parent.Style.SelectedColor;
					}
					else
					{
						parent.Enabled = false;
						parent.SelectedColor = GUIStyle.Red * 0.5f;
					}
				}
			}
			foreach (GUIComponent component in indicators.Children)
			{
				UpgradeStore.<>c__DisplayClass70_1 CS$<>8__locals2;
				CS$<>8__locals2.image = (component as GUIImage);
				if (CS$<>8__locals2.image != null)
				{
					foreach (UpgradePrefab prefab in prefabs)
					{
						if (component.UserData == prefab)
						{
							int maxLevel = prefab.GetMaxLevelForCurrentSub();
							if (maxLevel == 0)
							{
								component.Visible = false;
							}
							else
							{
								Dictionary<Identifier, GUIComponentStyle> styles = GUIStyle.GetComponentStyle("upgradeindicator").ChildStyles;
								if (styles.ContainsKey("upgradeindicatoron") && styles.ContainsKey("upgradeindicatordim") && styles.ContainsKey("upgradeindicatoroff"))
								{
									GUIComponentStyle onStyle = styles["upgradeindicatoron".ToIdentifier()];
									GUIComponentStyle dimStyle = styles["upgradeindicatordim".ToIdentifier()];
									UpgradeStore.<>c__DisplayClass70_2 CS$<>8__locals3;
									CS$<>8__locals3.offStyle = styles["upgradeindicatoroff".ToIdentifier()];
									if (maxLevel == 0)
									{
										UpgradeStore.<UpdateCategoryIndicators>g__SetOff|70_1(ref CS$<>8__locals2, ref CS$<>8__locals3);
									}
									else if (campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null) >= maxLevel)
									{
										if (CS$<>8__locals2.image.Style != onStyle)
										{
											CS$<>8__locals2.image.ApplyStyle(onStyle);
										}
									}
									else if (campaign.UpgradeManager.GetUpgradeLevel(prefab, category, null) > 0)
									{
										if (CS$<>8__locals2.image.Style != dimStyle)
										{
											CS$<>8__locals2.image.ApplyStyle(dimStyle);
										}
									}
									else
									{
										UpgradeStore.<UpdateCategoryIndicators>g__SetOff|70_1(ref CS$<>8__locals2, ref CS$<>8__locals3);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06001786 RID: 6022 RVA: 0x000E821C File Offset: 0x000E641C
		private void ScrollToCategory(Predicate<UpgradeStore.CategoryData> predicate, GUIListBox.PlaySelectSound playSelectSound = GUIListBox.PlaySelectSound.No)
		{
			if (this.currentStoreLayout == null)
			{
				return;
			}
			UpgradeStore.CategoryData? mostAppropriateCategory = null;
			GUIComponent mostAppropriateChild = null;
			foreach (GUIComponent child in this.currentStoreLayout.Content.Children)
			{
				object userData = child.UserData;
				if (userData is UpgradeStore.CategoryData)
				{
					UpgradeStore.CategoryData data = (UpgradeStore.CategoryData)userData;
					if (predicate(data) && (mostAppropriateCategory == null || data.Category.ItemTags.Count<Identifier>() < mostAppropriateCategory.Value.Category.ItemTags.Count<Identifier>()))
					{
						mostAppropriateCategory = new UpgradeStore.CategoryData?(data);
						mostAppropriateChild = child;
					}
				}
			}
			if (mostAppropriateChild != null)
			{
				this.currentStoreLayout.ScrollToElement(mostAppropriateChild, playSelectSound);
			}
		}

		// Token: 0x06001787 RID: 6023 RVA: 0x000E82F0 File Offset: 0x000E64F0
		private GUIComponent[] GetFrames(UpgradeCategory category)
		{
			List<GUIComponent> frames = new List<GUIComponent>();
			foreach (KeyValuePair<Item, GUIComponent> keyValuePair in this.itemPreviews)
			{
				Item item2;
				GUIComponent guicomponent;
				keyValuePair.Deconstruct(out item2, out guicomponent);
				Item item = item2;
				GUIComponent guiFrame = guicomponent;
				if (category.CanBeApplied(item, null))
				{
					frames.Add(guiFrame);
				}
			}
			return frames.ToArray();
		}

		// Token: 0x170005DA RID: 1498
		// (get) Token: 0x06001788 RID: 6024 RVA: 0x000E836C File Offset: 0x000E656C
		private static bool HasPermission
		{
			get
			{
				return CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageCampaign);
			}
		}

		// Token: 0x06001789 RID: 6025 RVA: 0x000E8378 File Offset: 0x000E6578
		private static RectTransform rectT(float x, float y, GUIComponent parentComponent, Anchor anchor = Anchor.TopLeft, ScaleBasis scaleBasis = ScaleBasis.Normal)
		{
			return new RectTransform(new Vector2(x, y), parentComponent.RectTransform, anchor, null, null, null, scaleBasis);
		}

		// Token: 0x0600178A RID: 6026 RVA: 0x000E83B8 File Offset: 0x000E65B8
		private static RectTransform rectT(Point point, GUIComponent parentComponent, Anchor anchor = Anchor.TopLeft)
		{
			return new RectTransform(point, parentComponent.RectTransform, anchor, null, ScaleBasis.Normal, false);
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x000E8548 File Offset: 0x000E6748
		[CompilerGenerated]
		internal static LocalizedString <CreateItemTooltip>g__CreateListEntry|63_0(LocalizedString name, int level)
		{
			string tag = "upgradeuitooltip.upgradelistelement";
			ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
			array[0] = new ValueTuple<string, LocalizedString>("[upgradename]", name);
			int num = 1;
			string item = "[level]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<int>(level);
			array[num] = new ValueTuple<string, LocalizedString>(item, defaultInterpolatedStringHandler.ToStringAndClear());
			return TextManager.GetWithVariables(tag, array);
		}

		// Token: 0x06001792 RID: 6034 RVA: 0x000E85A8 File Offset: 0x000E67A8
		[CompilerGenerated]
		internal static void <UpdateUpgradeEntry>g__CreateMaterialCosts|69_0(GUIListBox list, UpgradePrefab upgradePrefab, int targetLevel)
		{
			list.Content.ClearChildren();
			IReadOnlyCollection<Item> allItems = CargoManager.FindAllItemsOnPlayerAndSub(Character.Controlled);
			ImmutableArray<ApplicableResourceCollection>.Enumerator enumerator = upgradePrefab.GetApplicableResources(targetLevel).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ApplicableResourceCollection collection = enumerator.Current;
				list.Visible = true;
				int length = collection.MatchingItems.Length;
				if (length != 0)
				{
					ItemPrefab defaultItemPrefab = collection.MatchingItems.First<ItemPrefab>();
					GUILayoutGroup wrapperLayout = new GUILayoutGroup(UpgradeStore.rectT(0.25f, 1f, list.Content, Anchor.TopLeft, ScaleBasis.Normal), false, Anchor.TopLeft);
					GUIFrame itemFrame = new GUIFrame(UpgradeStore.rectT(1f, 1f, wrapperLayout, Anchor.TopLeft, ScaleBasis.Normal), null, null)
					{
						ToolTip = defaultItemPrefab.Name
					};
					bool hasItems = collection.Cost.Amount <= allItems.Count(new Func<Item, bool>(collection.Cost.MatchesItem));
					Sprite icon = defaultItemPrefab.InventoryIcon ?? defaultItemPrefab.Sprite;
					Color iconColor = (defaultItemPrefab.InventoryIcon == null) ? defaultItemPrefab.SpriteColor : defaultItemPrefab.InventoryIconColor;
					GUIImage itemIcon = new GUIImage(new RectTransform(Vector2.One, itemFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest), icon, true, null)
					{
						Color = (hasItems ? iconColor : (iconColor * 0.9f)),
						CanBeFocused = false
					};
					RectTransform rectT = new RectTransform(new Vector2(0.5f, 0.5f), itemIcon.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
					defaultInterpolatedStringHandler.AppendFormatted<int>(collection.Count);
					RichString text = defaultInterpolatedStringHandler.ToStringAndClear();
					GUIFont font = GUIStyle.Font;
					GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.BottomRight, false, "", null);
					guitextBlock.Shadow = true;
					guitextBlock.CanBeFocused = false;
					guitextBlock.Padding = Vector4.Zero;
					guitextBlock.TextColor = (hasItems ? Color.White : GUIStyle.Red);
					if (length != 1)
					{
						float index = 0f;
						new GUICustomComponent(UpgradeStore.rectT(1f, 1f, itemFrame, Anchor.TopLeft, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
						{
							index += deltaTime / 3f;
							if (index > (float)length)
							{
								index = 0f;
							}
							ItemPrefab currentPrefab = collection.MatchingItems[(int)MathF.Floor(index)];
							Sprite icon2 = currentPrefab.InventoryIcon ?? currentPrefab.Sprite;
							Color iconColor2 = (currentPrefab.InventoryIcon == null) ? currentPrefab.SpriteColor : currentPrefab.InventoryIconColor;
							itemIcon.Sprite = icon2;
							itemIcon.Color = (hasItems ? iconColor2 : (iconColor2 * 0.9f));
							itemFrame.ToolTip = currentPrefab.Name;
						}).CanBeFocused = false;
					}
				}
			}
		}

		// Token: 0x06001793 RID: 6035 RVA: 0x000E887E File Offset: 0x000E6A7E
		[CompilerGenerated]
		internal static void <UpdateCategoryIndicators>g__SetOff|70_1(ref UpgradeStore.<>c__DisplayClass70_1 A_0, ref UpgradeStore.<>c__DisplayClass70_2 A_1)
		{
			if (A_0.image.Style == A_1.offStyle)
			{
				return;
			}
			A_0.image.ApplyStyle(A_1.offStyle);
		}

		// Token: 0x04000BED RID: 3053
		private readonly CampaignUI campaignUI;

		// Token: 0x04000BEE RID: 3054
		private UpgradeStore.UpgradeTab selectedUpgradeTab;

		// Token: 0x04000BEF RID: 3055
		[Nullable(2)]
		private GUIMessageBox currectConfirmation;

		// Token: 0x04000BF0 RID: 3056
		public readonly GUIFrame ItemInfoFrame;

		// Token: 0x04000BF1 RID: 3057
		[Nullable(2)]
		private GUIComponent selectedUpgradeCategoryLayout;

		// Token: 0x04000BF2 RID: 3058
		[Nullable(2)]
		private GUILayoutGroup topHeaderLayout;

		// Token: 0x04000BF3 RID: 3059
		[Nullable(2)]
		private GUILayoutGroup mainStoreLayout;

		// Token: 0x04000BF4 RID: 3060
		[Nullable(2)]
		private GUILayoutGroup storeLayout;

		// Token: 0x04000BF5 RID: 3061
		[Nullable(2)]
		private GUILayoutGroup categoryButtonLayout;

		// Token: 0x04000BF6 RID: 3062
		[Nullable(2)]
		private GUILayoutGroup submarineInfoFrame;

		// Token: 0x04000BF7 RID: 3063
		[Nullable(2)]
		private GUIListBox currentStoreLayout;

		// Token: 0x04000BF8 RID: 3064
		[Nullable(2)]
		private GUICustomComponent submarinePreviewComponent;

		// Token: 0x04000BF9 RID: 3065
		[Nullable(2)]
		private GUIFrame subPreviewFrame;

		// Token: 0x04000BFA RID: 3066
		[Nullable(2)]
		private Submarine drawnSubmarine;

		// Token: 0x04000BFB RID: 3067
		private readonly List<UpgradeCategory> applicableCategories = new List<UpgradeCategory>();

		// Token: 0x04000BFC RID: 3068
		private Vector2[][] subHullVertices = new Vector2[0][];

		// Token: 0x04000BFD RID: 3069
		private List<Structure> submarineWalls = new List<Structure>();

		// Token: 0x04000BFE RID: 3070
		[Nullable(2)]
		public MapEntity HoveredEntity;

		// Token: 0x04000BFF RID: 3071
		private bool highlightWalls;

		// Token: 0x04000C00 RID: 3072
		[Nullable(2)]
		private UpgradeCategory currentUpgradeCategory;

		// Token: 0x04000C01 RID: 3073
		[Nullable(2)]
		private GUIButton activeItemSwapSlideDown;

		// Token: 0x04000C02 RID: 3074
		private readonly Dictionary<Item, GUIComponent> itemPreviews = new Dictionary<Item, GUIComponent>();

		// Token: 0x04000C03 RID: 3075
		private static readonly Color previewWhite = Color.White * 0.5f;

		// Token: 0x04000C04 RID: 3076
		private Point screenResolution;

		// Token: 0x04000C05 RID: 3077
		private bool needsRefresh = true;

		// Token: 0x04000C06 RID: 3078
		private CampaignUI.PlayerBalanceElement? playerBalanceElement;

		// Token: 0x04000C07 RID: 3079
		private static ImmutableHashSet<Character> characterList = ImmutableHashSet<Character>.Empty;

		// Token: 0x04000C08 RID: 3080
		public static bool WaitForServerUpdate;

		// Token: 0x04000C09 RID: 3081
		private bool customizeTabOpen;

		// Token: 0x02000A1A RID: 2586
		[Nullable(0)]
		public readonly struct CategoryData
		{
			// Token: 0x060073FE RID: 29694 RVA: 0x003712D6 File Offset: 0x0036F4D6
			public CategoryData(UpgradeCategory category, List<UpgradePrefab> prefabs)
			{
				this.Category = category;
				this.Prefabs = prefabs;
				this.SinglePrefab = null;
			}

			// Token: 0x060073FF RID: 29695 RVA: 0x003712ED File Offset: 0x0036F4ED
			public CategoryData(UpgradeCategory category, UpgradePrefab prefab)
			{
				this.Category = category;
				this.SinglePrefab = prefab;
				this.Prefabs = null;
			}

			// Token: 0x04004338 RID: 17208
			public readonly UpgradeCategory Category;

			// Token: 0x04004339 RID: 17209
			[Nullable(new byte[]
			{
				2,
				1
			})]
			public readonly List<UpgradePrefab> Prefabs;

			// Token: 0x0400433A RID: 17210
			[Nullable(2)]
			public readonly UpgradePrefab SinglePrefab;
		}

		// Token: 0x02000A1B RID: 2587
		[NullableContext(0)]
		private enum UpgradeTab
		{
			// Token: 0x0400433C RID: 17212
			Upgrade,
			// Token: 0x0400433D RID: 17213
			Repairs
		}

		// Token: 0x02000A1C RID: 2588
		[NullableContext(0)]
		private enum UpgradeStoreUserData
		{
			// Token: 0x0400433F RID: 17215
			BuyButton,
			// Token: 0x04004340 RID: 17216
			BuyButtonLayout,
			// Token: 0x04004341 RID: 17217
			ProgressBarLayout,
			// Token: 0x04004342 RID: 17218
			IncreaseLabel,
			// Token: 0x04004343 RID: 17219
			PriceLabel,
			// Token: 0x04004344 RID: 17220
			MaterialCostList
		}

		// Token: 0x02000A1D RID: 2589
		[Nullable(0)]
		public readonly struct BuyButtonFrame : IEquatable<UpgradeStore.BuyButtonFrame>
		{
			// Token: 0x06007400 RID: 29696 RVA: 0x00371304 File Offset: 0x0036F504
			public BuyButtonFrame(GUILayoutGroup Layout, GUIListBox MaterialCostList, GUIButton BuyButton, GUITextBlock PriceText)
			{
				this.Layout = Layout;
				this.MaterialCostList = MaterialCostList;
				this.BuyButton = BuyButton;
				this.PriceText = PriceText;
			}

			// Token: 0x17001A6B RID: 6763
			// (get) Token: 0x06007401 RID: 29697 RVA: 0x00371323 File Offset: 0x0036F523
			// (set) Token: 0x06007402 RID: 29698 RVA: 0x0037132B File Offset: 0x0036F52B
			public GUILayoutGroup Layout { get; set; }

			// Token: 0x17001A6C RID: 6764
			// (get) Token: 0x06007403 RID: 29699 RVA: 0x00371334 File Offset: 0x0036F534
			// (set) Token: 0x06007404 RID: 29700 RVA: 0x0037133C File Offset: 0x0036F53C
			public GUIListBox MaterialCostList { get; set; }

			// Token: 0x17001A6D RID: 6765
			// (get) Token: 0x06007405 RID: 29701 RVA: 0x00371345 File Offset: 0x0036F545
			// (set) Token: 0x06007406 RID: 29702 RVA: 0x0037134D File Offset: 0x0036F54D
			public GUIButton BuyButton { get; set; }

			// Token: 0x17001A6E RID: 6766
			// (get) Token: 0x06007407 RID: 29703 RVA: 0x00371356 File Offset: 0x0036F556
			// (set) Token: 0x06007408 RID: 29704 RVA: 0x0037135E File Offset: 0x0036F55E
			public GUITextBlock PriceText { get; set; }

			// Token: 0x06007409 RID: 29705 RVA: 0x00371368 File Offset: 0x0036F568
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("BuyButtonFrame");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600740A RID: 29706 RVA: 0x003713B4 File Offset: 0x0036F5B4
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Layout = ");
				builder.Append(this.Layout);
				builder.Append(", MaterialCostList = ");
				builder.Append(this.MaterialCostList);
				builder.Append(", BuyButton = ");
				builder.Append(this.BuyButton);
				builder.Append(", PriceText = ");
				builder.Append(this.PriceText);
				return true;
			}

			// Token: 0x0600740B RID: 29707 RVA: 0x00371426 File Offset: 0x0036F626
			[CompilerGenerated]
			public static bool operator !=(UpgradeStore.BuyButtonFrame left, UpgradeStore.BuyButtonFrame right)
			{
				return !(left == right);
			}

			// Token: 0x0600740C RID: 29708 RVA: 0x00371432 File Offset: 0x0036F632
			[CompilerGenerated]
			public static bool operator ==(UpgradeStore.BuyButtonFrame left, UpgradeStore.BuyButtonFrame right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600740D RID: 29709 RVA: 0x0037143C File Offset: 0x0036F63C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<GUILayoutGroup>.Default.GetHashCode(this.<Layout>k__BackingField) * -1521134295 + EqualityComparer<GUIListBox>.Default.GetHashCode(this.<MaterialCostList>k__BackingField)) * -1521134295 + EqualityComparer<GUIButton>.Default.GetHashCode(this.<BuyButton>k__BackingField)) * -1521134295 + EqualityComparer<GUITextBlock>.Default.GetHashCode(this.<PriceText>k__BackingField);
			}

			// Token: 0x0600740E RID: 29710 RVA: 0x0037149E File Offset: 0x0036F69E
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is UpgradeStore.BuyButtonFrame && this.Equals((UpgradeStore.BuyButtonFrame)obj);
			}

			// Token: 0x0600740F RID: 29711 RVA: 0x003714B8 File Offset: 0x0036F6B8
			[CompilerGenerated]
			public bool Equals(UpgradeStore.BuyButtonFrame other)
			{
				return EqualityComparer<GUILayoutGroup>.Default.Equals(this.<Layout>k__BackingField, other.<Layout>k__BackingField) && EqualityComparer<GUIListBox>.Default.Equals(this.<MaterialCostList>k__BackingField, other.<MaterialCostList>k__BackingField) && EqualityComparer<GUIButton>.Default.Equals(this.<BuyButton>k__BackingField, other.<BuyButton>k__BackingField) && EqualityComparer<GUITextBlock>.Default.Equals(this.<PriceText>k__BackingField, other.<PriceText>k__BackingField);
			}

			// Token: 0x06007410 RID: 29712 RVA: 0x00371525 File Offset: 0x0036F725
			[CompilerGenerated]
			public void Deconstruct(out GUILayoutGroup Layout, out GUIListBox MaterialCostList, out GUIButton BuyButton, out GUITextBlock PriceText)
			{
				Layout = this.Layout;
				MaterialCostList = this.MaterialCostList;
				BuyButton = this.BuyButton;
				PriceText = this.PriceText;
			}
		}

		// Token: 0x02000A1E RID: 2590
		[Nullable(0)]
		public readonly struct ProgressBarFrame : IEquatable<UpgradeStore.ProgressBarFrame>
		{
			// Token: 0x06007411 RID: 29713 RVA: 0x00371548 File Offset: 0x0036F748
			public ProgressBarFrame(GUITextBlock ProgressText, GUIProgressBar ProgressBar)
			{
				this.ProgressText = ProgressText;
				this.ProgressBar = ProgressBar;
			}

			// Token: 0x17001A6F RID: 6767
			// (get) Token: 0x06007412 RID: 29714 RVA: 0x00371558 File Offset: 0x0036F758
			// (set) Token: 0x06007413 RID: 29715 RVA: 0x00371560 File Offset: 0x0036F760
			public GUITextBlock ProgressText { get; set; }

			// Token: 0x17001A70 RID: 6768
			// (get) Token: 0x06007414 RID: 29716 RVA: 0x00371569 File Offset: 0x0036F769
			// (set) Token: 0x06007415 RID: 29717 RVA: 0x00371571 File Offset: 0x0036F771
			public GUIProgressBar ProgressBar { get; set; }

			// Token: 0x06007416 RID: 29718 RVA: 0x0037157C File Offset: 0x0036F77C
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ProgressBarFrame");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06007417 RID: 29719 RVA: 0x003715C8 File Offset: 0x0036F7C8
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("ProgressText = ");
				builder.Append(this.ProgressText);
				builder.Append(", ProgressBar = ");
				builder.Append(this.ProgressBar);
				return true;
			}

			// Token: 0x06007418 RID: 29720 RVA: 0x003715FD File Offset: 0x0036F7FD
			[CompilerGenerated]
			public static bool operator !=(UpgradeStore.ProgressBarFrame left, UpgradeStore.ProgressBarFrame right)
			{
				return !(left == right);
			}

			// Token: 0x06007419 RID: 29721 RVA: 0x00371609 File Offset: 0x0036F809
			[CompilerGenerated]
			public static bool operator ==(UpgradeStore.ProgressBarFrame left, UpgradeStore.ProgressBarFrame right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600741A RID: 29722 RVA: 0x00371613 File Offset: 0x0036F813
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<GUITextBlock>.Default.GetHashCode(this.<ProgressText>k__BackingField) * -1521134295 + EqualityComparer<GUIProgressBar>.Default.GetHashCode(this.<ProgressBar>k__BackingField);
			}

			// Token: 0x0600741B RID: 29723 RVA: 0x0037163C File Offset: 0x0036F83C
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is UpgradeStore.ProgressBarFrame && this.Equals((UpgradeStore.ProgressBarFrame)obj);
			}

			// Token: 0x0600741C RID: 29724 RVA: 0x00371654 File Offset: 0x0036F854
			[CompilerGenerated]
			public bool Equals(UpgradeStore.ProgressBarFrame other)
			{
				return EqualityComparer<GUITextBlock>.Default.Equals(this.<ProgressText>k__BackingField, other.<ProgressText>k__BackingField) && EqualityComparer<GUIProgressBar>.Default.Equals(this.<ProgressBar>k__BackingField, other.<ProgressBar>k__BackingField);
			}

			// Token: 0x0600741D RID: 29725 RVA: 0x00371686 File Offset: 0x0036F886
			[CompilerGenerated]
			public void Deconstruct(out GUITextBlock ProgressText, out GUIProgressBar ProgressBar)
			{
				ProgressText = this.ProgressText;
				ProgressBar = this.ProgressBar;
			}
		}

		// Token: 0x02000A1F RID: 2591
		[Nullable(0)]
		public readonly struct UpgradeFrame : IEquatable<UpgradeStore.UpgradeFrame>
		{
			// Token: 0x0600741E RID: 29726 RVA: 0x00371698 File Offset: 0x0036F898
			public UpgradeFrame(GUIFrame Frame, GUIImage Icon, GUITextBlock Name, GUITextBlock Description, [Nullable(0)] Option<UpgradeStore.BuyButtonFrame> BuyButton, [Nullable(0)] Option<UpgradeStore.ProgressBarFrame> ProgressBar)
			{
				this.Frame = Frame;
				this.Icon = Icon;
				this.Name = Name;
				this.Description = Description;
				this.BuyButton = BuyButton;
				this.ProgressBar = ProgressBar;
			}

			// Token: 0x17001A71 RID: 6769
			// (get) Token: 0x0600741F RID: 29727 RVA: 0x003716C7 File Offset: 0x0036F8C7
			// (set) Token: 0x06007420 RID: 29728 RVA: 0x003716CF File Offset: 0x0036F8CF
			public GUIFrame Frame { get; set; }

			// Token: 0x17001A72 RID: 6770
			// (get) Token: 0x06007421 RID: 29729 RVA: 0x003716D8 File Offset: 0x0036F8D8
			// (set) Token: 0x06007422 RID: 29730 RVA: 0x003716E0 File Offset: 0x0036F8E0
			public GUIImage Icon { get; set; }

			// Token: 0x17001A73 RID: 6771
			// (get) Token: 0x06007423 RID: 29731 RVA: 0x003716E9 File Offset: 0x0036F8E9
			// (set) Token: 0x06007424 RID: 29732 RVA: 0x003716F1 File Offset: 0x0036F8F1
			public GUITextBlock Name { get; set; }

			// Token: 0x17001A74 RID: 6772
			// (get) Token: 0x06007425 RID: 29733 RVA: 0x003716FA File Offset: 0x0036F8FA
			// (set) Token: 0x06007426 RID: 29734 RVA: 0x00371702 File Offset: 0x0036F902
			public GUITextBlock Description { get; set; }

			// Token: 0x17001A75 RID: 6773
			// (get) Token: 0x06007427 RID: 29735 RVA: 0x0037170B File Offset: 0x0036F90B
			// (set) Token: 0x06007428 RID: 29736 RVA: 0x00371713 File Offset: 0x0036F913
			[Nullable(0)]
			public Option<UpgradeStore.BuyButtonFrame> BuyButton { [NullableContext(0)] get; [NullableContext(0)] set; }

			// Token: 0x17001A76 RID: 6774
			// (get) Token: 0x06007429 RID: 29737 RVA: 0x0037171C File Offset: 0x0036F91C
			// (set) Token: 0x0600742A RID: 29738 RVA: 0x00371724 File Offset: 0x0036F924
			[Nullable(0)]
			public Option<UpgradeStore.ProgressBarFrame> ProgressBar { [NullableContext(0)] get; [NullableContext(0)] set; }

			// Token: 0x0600742B RID: 29739 RVA: 0x00371730 File Offset: 0x0036F930
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("UpgradeFrame");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600742C RID: 29740 RVA: 0x0037177C File Offset: 0x0036F97C
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Frame = ");
				builder.Append(this.Frame);
				builder.Append(", Icon = ");
				builder.Append(this.Icon);
				builder.Append(", Name = ");
				builder.Append(this.Name);
				builder.Append(", Description = ");
				builder.Append(this.Description);
				builder.Append(", BuyButton = ");
				builder.Append(this.BuyButton.ToString());
				builder.Append(", ProgressBar = ");
				builder.Append(this.ProgressBar.ToString());
				return true;
			}

			// Token: 0x0600742D RID: 29741 RVA: 0x0037183C File Offset: 0x0036FA3C
			[CompilerGenerated]
			public static bool operator !=(UpgradeStore.UpgradeFrame left, UpgradeStore.UpgradeFrame right)
			{
				return !(left == right);
			}

			// Token: 0x0600742E RID: 29742 RVA: 0x00371848 File Offset: 0x0036FA48
			[CompilerGenerated]
			public static bool operator ==(UpgradeStore.UpgradeFrame left, UpgradeStore.UpgradeFrame right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600742F RID: 29743 RVA: 0x00371854 File Offset: 0x0036FA54
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((EqualityComparer<GUIFrame>.Default.GetHashCode(this.<Frame>k__BackingField) * -1521134295 + EqualityComparer<GUIImage>.Default.GetHashCode(this.<Icon>k__BackingField)) * -1521134295 + EqualityComparer<GUITextBlock>.Default.GetHashCode(this.<Name>k__BackingField)) * -1521134295 + EqualityComparer<GUITextBlock>.Default.GetHashCode(this.<Description>k__BackingField)) * -1521134295 + EqualityComparer<Option<UpgradeStore.BuyButtonFrame>>.Default.GetHashCode(this.<BuyButton>k__BackingField)) * -1521134295 + EqualityComparer<Option<UpgradeStore.ProgressBarFrame>>.Default.GetHashCode(this.<ProgressBar>k__BackingField);
			}

			// Token: 0x06007430 RID: 29744 RVA: 0x003718E4 File Offset: 0x0036FAE4
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is UpgradeStore.UpgradeFrame && this.Equals((UpgradeStore.UpgradeFrame)obj);
			}

			// Token: 0x06007431 RID: 29745 RVA: 0x003718FC File Offset: 0x0036FAFC
			[CompilerGenerated]
			public bool Equals(UpgradeStore.UpgradeFrame other)
			{
				return EqualityComparer<GUIFrame>.Default.Equals(this.<Frame>k__BackingField, other.<Frame>k__BackingField) && EqualityComparer<GUIImage>.Default.Equals(this.<Icon>k__BackingField, other.<Icon>k__BackingField) && EqualityComparer<GUITextBlock>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<GUITextBlock>.Default.Equals(this.<Description>k__BackingField, other.<Description>k__BackingField) && EqualityComparer<Option<UpgradeStore.BuyButtonFrame>>.Default.Equals(this.<BuyButton>k__BackingField, other.<BuyButton>k__BackingField) && EqualityComparer<Option<UpgradeStore.ProgressBarFrame>>.Default.Equals(this.<ProgressBar>k__BackingField, other.<ProgressBar>k__BackingField);
			}

			// Token: 0x06007432 RID: 29746 RVA: 0x00371999 File Offset: 0x0036FB99
			[CompilerGenerated]
			public void Deconstruct(out GUIFrame Frame, out GUIImage Icon, out GUITextBlock Name, out GUITextBlock Description, [Nullable(0)] out Option<UpgradeStore.BuyButtonFrame> BuyButton, [Nullable(0)] out Option<UpgradeStore.ProgressBarFrame> ProgressBar)
			{
				Frame = this.Frame;
				Icon = this.Icon;
				Name = this.Name;
				Description = this.Description;
				BuyButton = this.BuyButton;
				ProgressBar = this.ProgressBar;
			}
		}
	}
}
