using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000B4 RID: 180
	internal class Store
	{
		// Token: 0x170005B7 RID: 1463
		// (get) Token: 0x06001651 RID: 5713 RVA: 0x000CFFA2 File Offset: 0x000CE1A2
		private Dictionary<ItemPrefab, Store.ItemQuantity> OwnedItems { get; } = new Dictionary<ItemPrefab, Store.ItemQuantity>();

		// Token: 0x170005B8 RID: 1464
		// (get) Token: 0x06001652 RID: 5714 RVA: 0x000CFFAA File Offset: 0x000CE1AA
		// (set) Token: 0x06001653 RID: 5715 RVA: 0x000CFFB2 File Offset: 0x000CE1B2
		private Location.StoreInfo ActiveStore { get; set; }

		// Token: 0x170005B9 RID: 1465
		// (get) Token: 0x06001654 RID: 5716 RVA: 0x000CFFBB File Offset: 0x000CE1BB
		private CargoManager CargoManager
		{
			get
			{
				return this.campaignUI.Campaign.CargoManager;
			}
		}

		// Token: 0x170005BA RID: 1466
		// (get) Token: 0x06001655 RID: 5717 RVA: 0x000CFFCD File Offset: 0x000CE1CD
		private Location CurrentLocation
		{
			get
			{
				Map map = this.campaignUI.Campaign.Map;
				if (map == null)
				{
					return null;
				}
				return map.CurrentLocation;
			}
		}

		// Token: 0x170005BB RID: 1467
		// (get) Token: 0x06001656 RID: 5718 RVA: 0x000CFFEA File Offset: 0x000CE1EA
		private int Balance
		{
			get
			{
				return this.campaignUI.Campaign.GetBalance(null);
			}
		}

		// Token: 0x170005BC RID: 1468
		// (get) Token: 0x06001657 RID: 5719 RVA: 0x000D0000 File Offset: 0x000CE200
		private bool IsBuying
		{
			get
			{
				bool result;
				switch (this.activeTab)
				{
				case Store.StoreTab.Buy:
					result = true;
					break;
				case Store.StoreTab.Sell:
					result = false;
					break;
				case Store.StoreTab.SellSub:
					result = false;
					break;
				default:
					throw new NotImplementedException();
				}
				return result;
			}
		}

		// Token: 0x170005BD RID: 1469
		// (get) Token: 0x06001658 RID: 5720 RVA: 0x000D003C File Offset: 0x000CE23C
		private GUIListBox ActiveShoppingCrateList
		{
			get
			{
				GUIListBox result;
				switch (this.activeTab)
				{
				case Store.StoreTab.Buy:
					result = this.shoppingCrateBuyList;
					break;
				case Store.StoreTab.Sell:
					result = this.shoppingCrateSellList;
					break;
				case Store.StoreTab.SellSub:
					result = this.shoppingCrateSellFromSubList;
					break;
				default:
					throw new NotImplementedException();
				}
				return result;
			}
		}

		// Token: 0x170005BE RID: 1470
		// (get) Token: 0x06001659 RID: 5721 RVA: 0x000D0086 File Offset: 0x000CE286
		// (set) Token: 0x0600165A RID: 5722 RVA: 0x000D008E File Offset: 0x000CE28E
		private bool HasBuyPermissions
		{
			get
			{
				return Store.HasPermissionToUseTab(Store.StoreTab.Buy);
			}
			set
			{
				this.hadBuyPermissions = value;
			}
		}

		// Token: 0x170005BF RID: 1471
		// (get) Token: 0x0600165B RID: 5723 RVA: 0x000D0097 File Offset: 0x000CE297
		// (set) Token: 0x0600165C RID: 5724 RVA: 0x000D009F File Offset: 0x000CE29F
		private bool HasSellInventoryPermissions
		{
			get
			{
				return Store.HasPermissionToUseTab(Store.StoreTab.Sell);
			}
			set
			{
				this.hadSellInventoryPermissions = value;
			}
		}

		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x0600165D RID: 5725 RVA: 0x000D00A8 File Offset: 0x000CE2A8
		// (set) Token: 0x0600165E RID: 5726 RVA: 0x000D00B0 File Offset: 0x000CE2B0
		private bool HasSellSubPermissions
		{
			get
			{
				return Store.HasPermissionToUseTab(Store.StoreTab.SellSub);
			}
			set
			{
				this.hadSellSubPermissions = value;
			}
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x000D00BC File Offset: 0x000CE2BC
		private static bool HasPermissionToUseTab(Store.StoreTab tab)
		{
			bool result;
			switch (tab)
			{
			case Store.StoreTab.Buy:
				result = true;
				break;
			case Store.StoreTab.Sell:
				result = CampaignMode.AllowedToManageCampaign(ClientPermissions.SellInventoryItems);
				break;
			case Store.StoreTab.SellSub:
				result = CampaignMode.AllowedToManageCampaign(ClientPermissions.SellSubItems);
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x000D00FE File Offset: 0x000CE2FE
		private void UpdatePermissions()
		{
			this.HasBuyPermissions = Store.HasPermissionToUseTab(Store.StoreTab.Buy);
			this.HasSellInventoryPermissions = Store.HasPermissionToUseTab(Store.StoreTab.Sell);
			this.HasSellSubPermissions = Store.HasPermissionToUseTab(Store.StoreTab.SellSub);
		}

		// Token: 0x06001661 RID: 5729 RVA: 0x000D0124 File Offset: 0x000CE324
		private bool HasTabPermissions(Store.StoreTab tab)
		{
			bool result;
			switch (tab)
			{
			case Store.StoreTab.Buy:
				result = this.HasBuyPermissions;
				break;
			case Store.StoreTab.Sell:
				result = this.HasSellInventoryPermissions;
				break;
			case Store.StoreTab.SellSub:
				result = this.HasSellSubPermissions;
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06001662 RID: 5730 RVA: 0x000D0163 File Offset: 0x000CE363
		private bool HasActiveTabPermissions()
		{
			return this.HasTabPermissions(this.activeTab);
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x000D0174 File Offset: 0x000CE374
		private bool HavePermissionsChanged(Store.StoreTab tab)
		{
			bool flag;
			switch (tab)
			{
			case Store.StoreTab.Buy:
				flag = this.hadBuyPermissions;
				break;
			case Store.StoreTab.Sell:
				flag = this.hadSellInventoryPermissions;
				break;
			case Store.StoreTab.SellSub:
				flag = this.hadSellSubPermissions;
				break;
			default:
				flag = false;
				break;
			}
			bool hadTabPermissions = flag;
			return hadTabPermissions != this.HasTabPermissions(tab);
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x000D01C4 File Offset: 0x000CE3C4
		public Store(CampaignUI campaignUI, GUIComponent parentComponent)
		{
			this.campaignUI = campaignUI;
			this.parentComponent = parentComponent;
			this.UpdatePermissions();
			this.CreateUI();
			Identifier refreshStoreId = new Identifier("RefreshStore");
			campaignUI.Campaign.Map.OnLocationChanged.RegisterOverwriteExisting(refreshStoreId, delegate(Map.LocationChangeInfo locationChangeInfo)
			{
				this.UpdateLocation(locationChangeInfo.PrevLocation, locationChangeInfo.NewLocation);
			});
			Location currentLocation = this.CurrentLocation;
			if (currentLocation != null)
			{
				Reputation reputation = currentLocation.Reputation;
				if (reputation != null)
				{
					reputation.OnReputationValueChanged.RegisterOverwriteExisting(refreshStoreId, delegate(Reputation _)
					{
						this.needsRefresh = true;
					});
				}
			}
			CargoManager cargoManager = campaignUI.Campaign.CargoManager;
			cargoManager.OnItemsInBuyCrateChanged.RegisterOverwriteExisting(refreshStoreId, delegate(CargoManager _)
			{
				this.needsBuyingRefresh = true;
			});
			cargoManager.OnPurchasedItemsChanged.RegisterOverwriteExisting(refreshStoreId, delegate(CargoManager _)
			{
				this.needsRefresh = true;
			});
			cargoManager.OnItemsInSellCrateChanged.RegisterOverwriteExisting(refreshStoreId, delegate(CargoManager _)
			{
				this.needsSellingRefresh = true;
			});
			cargoManager.OnSoldItemsChanged.RegisterOverwriteExisting(refreshStoreId, delegate(CargoManager _)
			{
				this.needsItemsToSellRefresh = true;
				this.needsItemsToSellFromSubRefresh = true;
				this.needsRefresh = true;
			});
			cargoManager.OnItemsInSellFromSubCrateChanged.RegisterOverwriteExisting(refreshStoreId, delegate(CargoManager _)
			{
				this.needsSellingFromSubRefresh = true;
			});
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x000D032C File Offset: 0x000CE52C
		public void SelectStore(Character merchant)
		{
			Identifier storeIdentifier = (merchant != null) ? merchant.MerchantIdentifier : Identifier.Empty;
			Location currentLocation = this.CurrentLocation;
			if (((currentLocation != null) ? currentLocation.Stores : null) != null)
			{
				if (!storeIdentifier.IsEmpty)
				{
					Location.StoreInfo store = this.CurrentLocation.GetStore(storeIdentifier);
					if (store != null)
					{
						this.ActiveStore = store;
						if (this.storeNameBlock != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
							defaultInterpolatedStringHandler.AppendLiteral("storename.");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(store.Identifier);
							LocalizedString storeName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
							if (storeName.IsNullOrEmpty())
							{
								storeName = TextManager.Get("store");
							}
							this.storeNameBlock.SetRichText(storeName);
						}
						this.ActiveStore.SetMerchantFaction(merchant.Faction);
						goto IL_293;
					}
				}
				this.ActiveStore = null;
				string errorId;
				string msg;
				if (storeIdentifier.IsEmpty)
				{
					errorId = "Store.SelectStore:IdentifierEmpty";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(47, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Error selecting store at ");
					defaultInterpolatedStringHandler2.AppendFormatted<Location>(this.CurrentLocation);
					defaultInterpolatedStringHandler2.AppendLiteral(": identifier is empty.");
					msg = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				else
				{
					errorId = "Store.SelectStore:StoreDoesntExist";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(102, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Error selecting store with identifier \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(storeIdentifier);
					defaultInterpolatedStringHandler3.AppendLiteral("\" at ");
					defaultInterpolatedStringHandler3.AppendFormatted<Location>(this.CurrentLocation);
					defaultInterpolatedStringHandler3.AppendLiteral(": store with the identifier doesn't exist at the location.");
					msg = defaultInterpolatedStringHandler3.ToStringAndClear();
				}
				DebugConsole.LogError(msg, null, null);
				GameAnalyticsManager.AddErrorEventOnce(errorId, GameAnalyticsManager.ErrorSeverity.Error, msg);
			}
			else
			{
				this.ActiveStore = null;
				string errorId2 = "";
				string msg2 = "";
				if (this.campaignUI.Campaign.Map == null)
				{
					errorId2 = "Store.SelectStore:MapNull";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(54, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("Error selecting store with identifier \"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(storeIdentifier);
					defaultInterpolatedStringHandler4.AppendLiteral("\": Map is null.");
					msg2 = defaultInterpolatedStringHandler4.ToStringAndClear();
				}
				else if (this.CurrentLocation == null)
				{
					errorId2 = "Store.SelectStore:CurrentLocationNull";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(66, 1);
					defaultInterpolatedStringHandler5.AppendLiteral("Error selecting store with identifier \"");
					defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(storeIdentifier);
					defaultInterpolatedStringHandler5.AppendLiteral("\": CurrentLocation is null.");
					msg2 = defaultInterpolatedStringHandler5.ToStringAndClear();
				}
				else if (this.CurrentLocation.Stores == null)
				{
					errorId2 = "Store.SelectStore:StoresNull";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(73, 1);
					defaultInterpolatedStringHandler6.AppendLiteral("Error selecting store with identifier \"");
					defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(storeIdentifier);
					defaultInterpolatedStringHandler6.AppendLiteral("\": CurrentLocation.Stores is null.");
					msg2 = defaultInterpolatedStringHandler6.ToStringAndClear();
				}
				if (!msg2.IsNullOrEmpty())
				{
					DebugConsole.LogError(msg2, null, null);
					GameAnalyticsManager.AddErrorEventOnce(errorId2, GameAnalyticsManager.ErrorSeverity.Error, msg2);
				}
			}
			IL_293:
			this.RefreshItemsToSell();
			this.Refresh(true);
		}

		// Token: 0x06001666 RID: 5734 RVA: 0x000D05D9 File Offset: 0x000CE7D9
		public void Refresh(bool updateOwned = true)
		{
			this.UpdatePermissions();
			if (updateOwned)
			{
				this.UpdateOwnedItems();
			}
			this.RefreshBuying(false);
			this.RefreshSelling(false);
			this.RefreshSellingFromSub(false, true);
			this.SetConfirmButtonBehavior();
			this.needsRefresh = false;
		}

		// Token: 0x06001667 RID: 5735 RVA: 0x000D0610 File Offset: 0x000CE810
		private void RefreshBuying(bool updateOwned = true)
		{
			if (updateOwned)
			{
				this.UpdateOwnedItems();
			}
			this.RefreshShoppingCrateBuyList();
			this.RefreshStoreBuyList();
			bool hasPermissions = this.HasTabPermissions(Store.StoreTab.Buy);
			this.storeBuyList.Enabled = hasPermissions;
			this.shoppingCrateBuyList.Enabled = hasPermissions;
			this.needsBuyingRefresh = false;
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x000D065C File Offset: 0x000CE85C
		private void RefreshSelling(bool updateOwned = true)
		{
			if (updateOwned)
			{
				this.UpdateOwnedItems();
			}
			this.RefreshShoppingCrateSellList();
			this.RefreshStoreSellList();
			bool hasPermissions = this.HasTabPermissions(Store.StoreTab.Sell);
			this.storeSellList.Enabled = hasPermissions;
			this.shoppingCrateSellList.Enabled = hasPermissions;
			this.needsSellingRefresh = false;
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x000D06A8 File Offset: 0x000CE8A8
		private void RefreshSellingFromSub(bool updateOwned = true, bool updateItemsToSellFromSub = true)
		{
			if (updateOwned)
			{
				this.UpdateOwnedItems();
			}
			if (updateItemsToSellFromSub)
			{
				this.RefreshItemsToSellFromSub();
			}
			this.RefreshShoppingCrateSellFromSubList();
			this.RefreshStoreSellFromSubList();
			bool hasPermissions = this.HasTabPermissions(Store.StoreTab.SellSub);
			this.storeSellFromSubList.Enabled = hasPermissions;
			this.shoppingCrateSellFromSubList.Enabled = hasPermissions;
			this.needsSellingFromSubRefresh = false;
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x000D06FC File Offset: 0x000CE8FC
		private void CreateUI()
		{
			GUIComponent glowChild = this.parentComponent.FindChild((GUIComponent c) => c.UserData as string == "glow", false);
			if (glowChild != null)
			{
				this.parentComponent.RemoveChild(glowChild);
			}
			GUIComponent containerChild = this.parentComponent.FindChild((GUIComponent c) => c.UserData as string == "container", false);
			if (containerChild != null)
			{
				this.parentComponent.RemoveChild(containerChild);
			}
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1.25f, 1.25f), this.parentComponent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlow", new Color?(Color.Black * 0.7f));
			guiframe.CanBeFocused = false;
			guiframe.UserData = "glow";
			GUIFrame guiframe2 = new GUIFrame(new RectTransform(new Vector2(0.95f), this.parentComponent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			guiframe2.CanBeFocused = false;
			guiframe2.UserData = "container";
			int panelMaxWidth = (int)(GUI.xScale * (float)((GUI.HorizontalAspectRatio < 1.4f) ? 650 : 560));
			GUILayoutGroup storeContent = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 1f), this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Store).RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Store).Rect.Height - HUDLayoutSettings.ButtonAreaTop.Bottom)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUILayoutGroup headerGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.06785714f), storeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.005f
			};
			float imageWidth = (float)headerGroup.Rect.Height / (float)headerGroup.Rect.Width;
			new GUIImage(new RectTransform(new Vector2(imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "StoreTradingIcon", GUIImage.ScalingMode.None);
			RectTransform rectT = new RectTransform(new Vector2(1f - imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("store");
			GUIFont guifont = GUIStyle.LargeFont;
			this.storeNameBlock = new GUITextBlock(rectT, text2, null, guifont, Alignment.Left, false, "", null)
			{
				CanBeFocused = false,
				ForceUpperCase = ForceUpperCase.Yes
			};
			GUILayoutGroup balanceAndValueGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05357143f), storeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.005f
			};
			GUILayoutGroup merchantBalanceContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), balanceAndValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.005f
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.5f), merchantBalanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("campaignstore.storebalance");
			guifont = GUIStyle.Font;
			Color? textColor = null;
			GUIFont font = guifont;
			Alignment textAlignment = Alignment.BottomLeft;
			bool wrap = false;
			string style = "";
			Color? color = null;
			GUITextBlock guitextBlock = new GUITextBlock(rectT2, text3, textColor, font, textAlignment, wrap, style, color);
			guitextBlock.AutoScaleVertical = true;
			guitextBlock.ForceUpperCase = ForceUpperCase.Yes;
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0.5f), merchantBalanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = "";
			color = new Color?(Color.White);
			guifont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT3, text4, null, guifont, Alignment.Left, false, "", color);
			guitextBlock2.AutoScaleVertical = true;
			guitextBlock2.TextScale = 1.1f;
			guitextBlock2.TextGetter = (() => this.GetMerchantBalanceText());
			GUILayoutGroup reputationEffectContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), balanceAndValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				RelativeSpacing = 0.005f,
				ToolTip = TextManager.Get("campaignstore.reputationtooltip")
			};
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.5f), reputationEffectContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("reputationmodifier");
			guifont = GUIStyle.Font;
			GUITextBlock guitextBlock3 = new GUITextBlock(rectT4, text5, null, guifont, Alignment.BottomLeft, false, "", null);
			guitextBlock3.AutoScaleVertical = true;
			guitextBlock3.CanBeFocused = false;
			guitextBlock3.ForceUpperCase = ForceUpperCase.Yes;
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0.5f), reputationEffectContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = "";
			guifont = GUIStyle.SubHeadingFont;
			this.reputationEffectBlock = new GUITextBlock(rectT5, text6, null, guifont, Alignment.Left, false, "", null)
			{
				AutoScaleVertical = true,
				CanBeFocused = false,
				TextScale = 1.1f,
				TextGetter = delegate()
				{
					if (this.ActiveStore != null)
					{
						Color textColor2 = GUIStyle.ColorReputationNeutral;
						string sign = "";
						int reputationModifier = (int)MathF.Round((this.ActiveStore.GetReputationModifier(this.activeTab == Store.StoreTab.Buy) - 1f) * 100f);
						if (reputationModifier > 0)
						{
							textColor2 = (this.IsBuying ? GUIStyle.ColorReputationLow : GUIStyle.ColorReputationHigh);
							sign = "+";
						}
						else if (reputationModifier < 0)
						{
							textColor2 = (this.IsBuying ? GUIStyle.ColorReputationHigh : GUIStyle.ColorReputationLow);
						}
						this.reputationEffectBlock.TextColor = textColor2;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler.AppendFormatted(sign);
						defaultInterpolatedStringHandler.AppendFormatted<int>(reputationModifier);
						defaultInterpolatedStringHandler.AppendLiteral("%");
						return defaultInterpolatedStringHandler.ToStringAndClear();
					}
					return "";
				}
			};
			GUIFrame modeButtonFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.028571429f), storeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup modeButtonContainer = new GUILayoutGroup(new RectTransform(Vector2.One, modeButtonFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			Array tabs = Enum.GetValues(typeof(Store.StoreTab));
			this.storeTabButtons.Clear();
			this.tabSortingMethods.Clear();
			foreach (object obj in tabs)
			{
				Store.StoreTab tab = (Store.StoreTab)obj;
				LocalizedString localizedString;
				if (tab == Store.StoreTab.SellSub)
				{
					localizedString = TextManager.Get("submarine");
				}
				else
				{
					localizedString = TextManager.Get("campaignstoretab." + tab.ToString());
				}
				LocalizedString text11 = localizedString;
				GUIButton tabButton = new GUIButton(new RectTransform(new Vector2(1f / (float)(tabs.Length + 1), 1f), modeButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text11, Alignment.Center, "GUITabButton", null)
				{
					UserData = tab,
					OnClicked = delegate(GUIButton button, object userData)
					{
						this.ChangeStoreTab((Store.StoreTab)userData);
						return true;
					}
				};
				this.storeTabButtons.Add(tabButton);
				this.tabSortingMethods.Add(tab, Store.SortingMethod.AlphabeticalAsc);
			}
			GUILayoutGroup storeInventoryContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), new GUIFrame(new RectTransform(new Vector2(1f, 0.84999996f), storeContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null).RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			this.categoryButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.08f, 1f), storeInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f
			};
			List<MapEntityCategory> itemCategories = Enum.GetValues(typeof(MapEntityCategory)).Cast<MapEntityCategory>().ToList<MapEntityCategory>();
			itemCategories.Remove(MapEntityCategory.None);
			itemCategories.RemoveAll((MapEntityCategory c) => !ItemPrefab.Prefabs.Any((ItemPrefab ep) => ep.Category.HasFlag(c) && ep.CanBeBought));
			this.itemCategoryButtons.Clear();
			GUIButton categoryButton = new GUIButton(new RectTransform(new Point(this.categoryButtonContainer.Rect.Width, this.categoryButtonContainer.Rect.Width), this.categoryButtonContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CategoryButton.All", null)
			{
				ToolTip = TextManager.Get("MapEntityCategory.All"),
				OnClicked = new GUIButton.OnClickedHandler(this.<CreateUI>g__OnClickedCategoryButton|85_3)
			};
			this.itemCategoryButtons.Add(categoryButton);
			foreach (MapEntityCategory category in itemCategories)
			{
				categoryButton = new GUIButton(new RectTransform(new Point(this.categoryButtonContainer.Rect.Width, this.categoryButtonContainer.Rect.Width), this.categoryButtonContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CategoryButton." + category.ToString(), null)
				{
					ToolTip = TextManager.Get("MapEntityCategory." + category.ToString()),
					UserData = category,
					OnClicked = new GUIButton.OnClickedHandler(this.<CreateUI>g__OnClickedCategoryButton|85_3)
				};
				this.itemCategoryButtons.Add(categoryButton);
			}
			using (List<GUIButton>.Enumerator enumerator3 = this.itemCategoryButtons.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					GUIButton btn = enumerator3.Current;
					btn.RectTransform.SizeChanged += delegate()
					{
						if (btn.Frame.sprites == null)
						{
							return;
						}
						UISprite sprite = btn.Frame.sprites[GUIComponent.ComponentState.None].First<UISprite>();
						btn.RectTransform.NonScaledSize = new Point(btn.Rect.Width, (int)((float)btn.Rect.Width * ((float)sprite.Sprite.SourceRect.Height / (float)sprite.Sprite.SourceRect.Width)));
					};
				}
			}
			GUILayoutGroup sortFilterListContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.92f, 1f), storeInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			GUILayoutGroup sortFilterGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.08f), sortFilterListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			GUILayoutGroup sortGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), sortFilterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), sortGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.sortby"), null, null, Alignment.Left, false, "", null);
			this.sortingDropDown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.5f), sortGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.sortby"), 3, "", false, false, Alignment.CenterLeft, 1f)
			{
				OnSelected = delegate(GUIComponent child, object userData)
				{
					this.SortActiveTabItems((Store.SortingMethod)userData);
					return true;
				}
			};
			string tag = "sortingmethod.";
			this.sortingDropDown.AddItem(TextManager.Get(tag + Store.SortingMethod.AlphabeticalAsc.ToString()), Store.SortingMethod.AlphabeticalAsc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + Store.SortingMethod.PriceAsc.ToString()), Store.SortingMethod.PriceAsc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + Store.SortingMethod.PriceDesc.ToString()), Store.SortingMethod.PriceDesc, null, null, null);
			GUILayoutGroup filterGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 1f), sortFilterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), filterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverlog.filter"), null, null, Alignment.Left, false, "", null);
			this.searchBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.5f), filterGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, true, true);
			this.searchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.FilterStoreItems(null, text);
				return true;
			};
			GUIFrame storeItemListContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.92f), sortFilterListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.tabLists.Clear();
			this.storeBuyList = new GUIListBox(new RectTransform(Vector2.One, storeItemListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false,
				Visible = false
			};
			GUIListBox parentList = this.storeBuyList;
			Location currentLocation = this.CurrentLocation;
			this.storeDailySpecialsGroup = this.CreateDealsGroup(parentList, (currentLocation != null) ? currentLocation.DailySpecialsCount : 1);
			this.tabLists.Add(Store.StoreTab.Buy, this.storeBuyList);
			this.storeSellList = new GUIListBox(new RectTransform(Vector2.One, storeItemListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false,
				Visible = false
			};
			GUIListBox parentList2 = this.storeSellList;
			Location currentLocation2 = this.CurrentLocation;
			this.storeRequestedGoodGroup = this.CreateDealsGroup(parentList2, (currentLocation2 != null) ? currentLocation2.RequestedGoodsCount : 1);
			this.tabLists.Add(Store.StoreTab.Sell, this.storeSellList);
			this.storeSellFromSubList = new GUIListBox(new RectTransform(Vector2.One, storeItemListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false,
				Visible = false
			};
			GUIListBox parentList3 = this.storeSellFromSubList;
			Location currentLocation3 = this.CurrentLocation;
			this.storeRequestedSubGoodGroup = this.CreateDealsGroup(parentList3, (currentLocation3 != null) ? currentLocation3.RequestedGoodsCount : 1);
			this.tabLists.Add(Store.StoreTab.SellSub, this.storeSellFromSubList);
			GUILayoutGroup shoppingCrateContent = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 1f), this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Store).RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Store).Rect.Height - HUDLayoutSettings.ButtonAreaTop.Bottom)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			headerGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05357143f), shoppingCrateContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopRight)
			{
				RelativeSpacing = 0.005f
			};
			imageWidth = (float)headerGroup.Rect.Height / (float)headerGroup.Rect.Width;
			new GUIImage(new RectTransform(new Vector2(imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "StoreShoppingCrateIcon", GUIImage.ScalingMode.None);
			RectTransform rectT6 = new RectTransform(new Vector2(1f - imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text7 = TextManager.Get("campaignstore.shoppingcrate");
			guifont = GUIStyle.LargeFont;
			GUITextBlock guitextBlock4 = new GUITextBlock(rectT6, text7, null, guifont, Alignment.Right, false, "", null);
			guitextBlock4.CanBeFocused = false;
			guitextBlock4.ForceUpperCase = ForceUpperCase.Yes;
			this.playerBalanceElement = CampaignUI.AddBalanceElement(shoppingCrateContent, new Vector2(1f, 0.05357143f));
			GUIFrame dividerFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.042857144f), shoppingCrateContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIImage(new RectTransform(Vector2.One, dividerFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "HorizontalLine", GUIImage.ScalingMode.None);
			GUILayoutGroup shoppingCrateInventoryContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), new GUIFrame(new RectTransform(new Vector2(1f, 0.84999996f), shoppingCrateContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null).RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			GUIFrame shoppingCrateListContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.8f), shoppingCrateInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.shoppingCrateBuyList = new GUIListBox(new RectTransform(Vector2.One, shoppingCrateListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = false,
				KeepSpaceForScrollBar = true
			};
			this.shoppingCrateSellList = new GUIListBox(new RectTransform(Vector2.One, shoppingCrateListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = false,
				KeepSpaceForScrollBar = true
			};
			this.shoppingCrateSellFromSubList = new GUIListBox(new RectTransform(Vector2.One, shoppingCrateListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = false,
				KeepSpaceForScrollBar = true
			};
			GUILayoutGroup relevantBalanceContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), shoppingCrateInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT7 = new RectTransform(new Vector2(0.5f, 1f), relevantBalanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text8 = "";
			guifont = GUIStyle.Font;
			this.relevantBalanceName = new GUITextBlock(rectT7, text8, null, guifont, Alignment.Left, false, "", null)
			{
				CanBeFocused = false
			};
			GUITextBlock guitextBlock5 = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), relevantBalanceContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(Color.White), GUIStyle.SubHeadingFont, Alignment.Right, false, "", null);
			guitextBlock5.CanBeFocused = false;
			guitextBlock5.TextScale = 1.1f;
			guitextBlock5.TextGetter = delegate()
			{
				if (!this.IsBuying)
				{
					return this.GetMerchantBalanceText();
				}
				return CampaignUI.GetTotalBalance();
			};
			GUILayoutGroup totalContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), shoppingCrateInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT8 = new RectTransform(new Vector2(0.5f, 1f), totalContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text9 = TextManager.Get("campaignstore.total");
			guifont = GUIStyle.Font;
			new GUITextBlock(rectT8, text9, null, guifont, Alignment.Left, false, "", null).CanBeFocused = false;
			RectTransform rectT9 = new RectTransform(new Vector2(0.5f, 1f), totalContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text10 = "";
			guifont = GUIStyle.SubHeadingFont;
			this.shoppingCrateTotal = new GUITextBlock(rectT9, text10, null, guifont, Alignment.Right, false, "", null)
			{
				CanBeFocused = false,
				TextScale = 1.1f
			};
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), shoppingCrateInventoryContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopRight);
			this.confirmButton = new GUIButton(new RectTransform(new Vector2(0.35f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "", null)
			{
				ForceUpperCase = ForceUpperCase.Yes
			};
			this.SetConfirmButtonBehavior();
			this.clearAllButton = new GUIButton(new RectTransform(new Vector2(0.35f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.clearall"), Alignment.Center, "", null)
			{
				ClickSound = GUISoundType.Cart,
				Enabled = this.HasActiveTabPermissions(),
				ForceUpperCase = ForceUpperCase.Yes,
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (!this.HasActiveTabPermissions())
					{
						return false;
					}
					List<PurchasedItem> list;
					switch (this.activeTab)
					{
					case Store.StoreTab.Buy:
						list = new List<PurchasedItem>(this.CargoManager.GetBuyCrateItems(this.ActiveStore, false));
						break;
					case Store.StoreTab.Sell:
						list = new List<PurchasedItem>(this.CargoManager.GetSellCrateItems(this.ActiveStore, false));
						break;
					case Store.StoreTab.SellSub:
						list = new List<PurchasedItem>(this.CargoManager.GetSubCrateItems(this.ActiveStore, false));
						break;
					default:
						throw new NotImplementedException();
					}
					List<PurchasedItem> itemsToRemove = list;
					itemsToRemove.ForEach(delegate(PurchasedItem i)
					{
						this.ClearFromShoppingCrate(i);
					});
					return true;
				}
			};
			this.ChangeStoreTab(this.activeTab);
			this.resolutionWhenCreated = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x0600166B RID: 5739 RVA: 0x000D20CC File Offset: 0x000D02CC
		private LocalizedString GetMerchantBalanceText()
		{
			Location.StoreInfo activeStore = this.ActiveStore;
			return TextManager.FormatCurrency((activeStore != null) ? activeStore.Balance : 0, true);
		}

		// Token: 0x0600166C RID: 5740 RVA: 0x000D20E8 File Offset: 0x000D02E8
		private GUILayoutGroup CreateDealsGroup(GUIListBox parentList, int elementCount)
		{
			elementCount++;
			int elementHeight = (int)(GUI.yScale * 80f);
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parentList.Content.Rect.Width, elementCount * elementHeight + 3), parentList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
			{
				UserData = "deals"
			};
			GUILayoutGroup dealsGroup = new GUILayoutGroup(new RectTransform(Vector2.One, frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			GUILayoutGroup dealsHeader = new GUILayoutGroup(new RectTransform(new Point((int)(0.95f * (float)parentList.Content.Rect.Width), elementHeight), dealsGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft)
			{
				UserData = "header"
			};
			float iconWidth = 0.9f * (float)dealsHeader.Rect.Height / (float)dealsHeader.Rect.Width;
			GUIImage dealsIcon = new GUIImage(new RectTransform(new Vector2(iconWidth, 0.9f), dealsHeader.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "StoreDealIcon", true);
			LocalizedString text = TextManager.Get((parentList == this.storeBuyList) ? "campaignstore.dailyspecials" : "campaignstore.requestedgoods");
			RectTransform rectT = new RectTransform(new Vector2(1f - iconWidth, 0.9f), dealsHeader.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = text;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUITextBlock dealsText = new GUITextBlock(rectT, text2, null, largeFont, Alignment.Left, false, "", null);
			this.storeSpecialColor = dealsIcon.Color;
			dealsText.TextColor = this.storeSpecialColor;
			GUIImage divider = new GUIImage(new RectTransform(new Point(dealsGroup.Rect.Width, 3), dealsGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "HorizontalLine", GUIImage.ScalingMode.None)
			{
				UserData = "divider"
			};
			frame.CanBeFocused = (dealsGroup.CanBeFocused = (dealsHeader.CanBeFocused = (dealsIcon.CanBeFocused = (dealsText.CanBeFocused = (divider.CanBeFocused = false)))));
			return dealsGroup;
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x000D2368 File Offset: 0x000D0568
		private void UpdateLocation(Location prevLocation, Location newLocation)
		{
			if (prevLocation == newLocation)
			{
				return;
			}
			if (((prevLocation != null) ? prevLocation.Reputation : null) != null)
			{
				prevLocation.Reputation.OnReputationValueChanged.Dispose();
			}
			if (ItemPrefab.Prefabs.Any((ItemPrefab p) => p.CanBeBoughtFrom(newLocation)))
			{
				this.selectedItemCategory = null;
				this.searchBox.Text = "";
				this.ChangeStoreTab(Store.StoreTab.Buy);
				Location newLocation2 = newLocation;
				if (((newLocation2 != null) ? newLocation2.Reputation : null) != null)
				{
					newLocation.Reputation.OnReputationValueChanged.RegisterOverwriteExisting("RefreshStore".ToIdentifier(), delegate(Reputation _)
					{
						base.<UpdateLocation>g__SetNeedsRefresh|2();
					});
				}
			}
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x000D242C File Offset: 0x000D062C
		private void UpdateCategoryButtons()
		{
			List<PurchasedItem> list;
			switch (this.activeTab)
			{
			case Store.StoreTab.Buy:
			{
				Location.StoreInfo activeStore = this.ActiveStore;
				list = ((activeStore != null) ? activeStore.Stock : null);
				break;
			}
			case Store.StoreTab.Sell:
				list = this.itemsToSell;
				break;
			case Store.StoreTab.SellSub:
				list = this.itemsToSellFromSub;
				break;
			default:
				list = null;
				break;
			}
			IEnumerable<PurchasedItem> enumerable = list;
			IEnumerable<PurchasedItem> tabItems = enumerable ?? Enumerable.Empty<PurchasedItem>();
			foreach (GUIButton button in this.itemCategoryButtons)
			{
				object userData = button.UserData;
				if (userData is MapEntityCategory)
				{
					MapEntityCategory category = (MapEntityCategory)userData;
					bool isButtonEnabled = false;
					foreach (PurchasedItem item in tabItems)
					{
						if (item.ItemPrefab.Category.HasFlag(category))
						{
							isButtonEnabled = true;
							break;
						}
					}
					button.Enabled = isButtonEnabled;
				}
			}
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x000D2550 File Offset: 0x000D0750
		private void ChangeStoreTab(Store.StoreTab tab)
		{
			this.activeTab = tab;
			foreach (GUIButton tabButton in this.storeTabButtons)
			{
				tabButton.Selected = ((Store.StoreTab)tabButton.UserData == this.activeTab);
			}
			this.sortingDropDown.SelectItem(this.tabSortingMethods[tab]);
			this.relevantBalanceName.Text = (this.IsBuying ? TextManager.Get("campaignstore.balance") : TextManager.Get("campaignstore.storebalance"));
			this.UpdateCategoryButtons();
			this.SetShoppingCrateTotalText();
			this.SetClearAllButtonStatus();
			this.SetConfirmButtonBehavior();
			this.SetConfirmButtonStatus();
			this.FilterStoreItems();
			switch (tab)
			{
			case Store.StoreTab.Buy:
				this.storeSellList.Visible = false;
				if (this.storeSellFromSubList != null)
				{
					this.storeSellFromSubList.Visible = false;
				}
				this.storeBuyList.Visible = true;
				this.shoppingCrateSellList.Visible = false;
				if (this.shoppingCrateSellFromSubList != null)
				{
					this.shoppingCrateSellFromSubList.Visible = false;
				}
				this.shoppingCrateBuyList.Visible = true;
				return;
			case Store.StoreTab.Sell:
				this.storeBuyList.Visible = false;
				if (this.storeSellFromSubList != null)
				{
					this.storeSellFromSubList.Visible = false;
				}
				this.storeSellList.Visible = true;
				this.shoppingCrateBuyList.Visible = false;
				if (this.shoppingCrateSellFromSubList != null)
				{
					this.shoppingCrateSellFromSubList.Visible = false;
				}
				this.shoppingCrateSellList.Visible = true;
				return;
			case Store.StoreTab.SellSub:
				this.storeBuyList.Visible = false;
				this.storeSellList.Visible = false;
				if (this.storeSellFromSubList != null)
				{
					this.storeSellFromSubList.Visible = true;
				}
				this.shoppingCrateBuyList.Visible = false;
				this.shoppingCrateSellList.Visible = false;
				if (this.shoppingCrateSellFromSubList != null)
				{
					this.shoppingCrateSellFromSubList.Visible = true;
				}
				return;
			default:
				return;
			}
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x000D2748 File Offset: 0x000D0948
		private void FilterStoreItems(MapEntityCategory? category, string filter)
		{
			this.selectedItemCategory = category;
			GUIListBox list = this.tabLists[this.activeTab];
			filter = ((filter != null) ? filter.ToLower() : null);
			foreach (GUIComponent child in list.Content.Children)
			{
				PurchasedItem item = child.UserData as PurchasedItem;
				LocalizedString left;
				if (item == null)
				{
					left = null;
				}
				else
				{
					ItemPrefab itemPrefab = item.ItemPrefab;
					left = ((itemPrefab != null) ? itemPrefab.Name : null);
				}
				if (!(left == null))
				{
					child.Visible = ((this.IsBuying || item.Quantity > 0) && (category == null || item.ItemPrefab.Category.HasFlag(category.Value)) && (string.IsNullOrEmpty(filter) || item.ItemPrefab.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)));
				}
			}
			foreach (GUIButton btn in this.itemCategoryButtons)
			{
				GUIComponent guicomponent = btn;
				MapEntityCategory? mapEntityCategory = (MapEntityCategory?)btn.UserData;
				MapEntityCategory? mapEntityCategory2 = this.selectedItemCategory;
				guicomponent.Selected = (mapEntityCategory.GetValueOrDefault() == mapEntityCategory2.GetValueOrDefault() & mapEntityCategory != null == (mapEntityCategory2 != null));
			}
			list.UpdateScrollBarSize();
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x000D28D0 File Offset: 0x000D0AD0
		private void FilterStoreItems()
		{
			MapEntityCategory? category = string.IsNullOrEmpty(this.searchBox.Text) ? this.selectedItemCategory : null;
			this.FilterStoreItems(category, this.searchBox.Text);
		}

		// Token: 0x06001672 RID: 5746 RVA: 0x000D2913 File Offset: 0x000D0B13
		private static float GetReputationRequirement(PriceInfo priceInfo, Identifier faction)
		{
			return priceInfo.MinReputation.GetValueOrDefault(faction);
		}

		// Token: 0x06001673 RID: 5747 RVA: 0x000D2924 File Offset: 0x000D0B24
		private static bool ReputationRequirementsMet(PriceInfo priceInfo, Identifier faction)
		{
			if (priceInfo.MinReputation.None(null))
			{
				return true;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			float requirement;
			return campaign != null && priceInfo.MinReputation.TryGetValue(faction, out requirement) && MathF.Round(campaign.GetReputation(faction)) >= requirement;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x000D297C File Offset: 0x000D0B7C
		private void RefreshStoreBuyList()
		{
			Store.<>c__DisplayClass98_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float prevBuyListScroll = this.storeBuyList.BarScroll;
			float prevShoppingCrateScroll = this.shoppingCrateBuyList.BarScroll;
			Location.StoreInfo activeStore = this.ActiveStore;
			int num;
			if (activeStore == null)
			{
				num = 0;
			}
			else
			{
				num = activeStore.DailySpecials.Count((ItemPrefab s) => s.CanCharacterBuy());
			}
			int dailySpecialCount = num;
			if ((this.ActiveStore == null && this.storeDailySpecialsGroup != null) || this.storeDailySpecialsGroup != null != this.ActiveStore.DailySpecials.Any<ItemPrefab>() || dailySpecialCount != this.prevDailySpecialCount)
			{
				GUIComponent guicomponent = this.storeBuyList;
				GUILayoutGroup guilayoutGroup = this.storeDailySpecialsGroup;
				guicomponent.RemoveChild((guilayoutGroup != null) ? guilayoutGroup.Parent : null);
				if (this.ActiveStore != null && (this.storeDailySpecialsGroup == null || dailySpecialCount != this.prevDailySpecialCount))
				{
					this.storeDailySpecialsGroup = this.CreateDealsGroup(this.storeBuyList, dailySpecialCount);
					this.storeDailySpecialsGroup.Parent.SetAsFirstChild();
				}
				else
				{
					this.storeDailySpecialsGroup = null;
				}
				this.storeBuyList.RecalculateChildren();
				this.prevDailySpecialCount = dailySpecialCount;
			}
			CS$<>8__locals1.hasPermissions = this.HasTabPermissions(Store.StoreTab.Buy);
			CS$<>8__locals1.existingItemFrames = new HashSet<GUIComponent>();
			if (this.ActiveStore != null)
			{
				foreach (PurchasedItem item in this.ActiveStore.Stock)
				{
					this.<RefreshStoreBuyList>g__CreateOrUpdateItemFrame|98_1(item.ItemPrefab, item.Quantity, ref CS$<>8__locals1);
				}
				using (List<ItemPrefab>.Enumerator enumerator2 = this.ActiveStore.DailySpecials.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ItemPrefab itemPrefab = enumerator2.Current;
						if (!this.ActiveStore.Stock.Any((PurchasedItem pi) => pi.ItemPrefab == itemPrefab))
						{
							this.<RefreshStoreBuyList>g__CreateOrUpdateItemFrame|98_1(itemPrefab, 0, ref CS$<>8__locals1);
						}
					}
				}
			}
			List<GUIComponent> removedItemFrames = (from c in this.storeBuyList.Content.Children
			where c.UserData is PurchasedItem
			select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>();
			if (this.storeDailySpecialsGroup != null)
			{
				removedItemFrames.AddRange((from c in this.storeDailySpecialsGroup.Children
				where c.UserData is PurchasedItem
				select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>());
			}
			removedItemFrames.ForEach(delegate(GUIComponent f)
			{
				f.RectTransform.Parent = null;
			});
			if (this.activeTab == Store.StoreTab.Buy)
			{
				this.UpdateCategoryButtons();
				this.FilterStoreItems();
			}
			this.SortItems(Store.StoreTab.Buy);
			this.storeBuyList.BarScroll = prevBuyListScroll;
			this.shoppingCrateBuyList.BarScroll = prevShoppingCrateScroll;
		}

		// Token: 0x06001675 RID: 5749 RVA: 0x000D2C74 File Offset: 0x000D0E74
		private void RefreshStoreSellList()
		{
			Store.<>c__DisplayClass99_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float prevSellListScroll = this.storeSellList.BarScroll;
			float prevShoppingCrateScroll = this.shoppingCrateSellList.BarScroll;
			Location.StoreInfo activeStore = this.ActiveStore;
			int requestedGoodsCount = (activeStore != null) ? activeStore.RequestedGoods.Count : 0;
			if ((this.ActiveStore == null && this.storeRequestedGoodGroup != null) || this.storeRequestedGoodGroup != null != this.ActiveStore.RequestedGoods.Any<ItemPrefab>() || requestedGoodsCount != this.prevRequestedGoodsCount)
			{
				GUIComponent guicomponent = this.storeSellList;
				GUILayoutGroup guilayoutGroup = this.storeRequestedGoodGroup;
				guicomponent.RemoveChild((guilayoutGroup != null) ? guilayoutGroup.Parent : null);
				if (this.ActiveStore != null && (this.storeRequestedGoodGroup == null || requestedGoodsCount != this.prevRequestedGoodsCount))
				{
					this.storeRequestedGoodGroup = this.CreateDealsGroup(this.storeSellList, requestedGoodsCount);
					this.storeRequestedGoodGroup.Parent.SetAsFirstChild();
				}
				else
				{
					this.storeRequestedGoodGroup = null;
				}
				this.storeSellList.RecalculateChildren();
				this.prevRequestedGoodsCount = requestedGoodsCount;
			}
			CS$<>8__locals1.hasPermissions = this.HasTabPermissions(Store.StoreTab.Sell);
			CS$<>8__locals1.existingItemFrames = new HashSet<GUIComponent>();
			if (this.ActiveStore != null)
			{
				foreach (PurchasedItem item in this.itemsToSell)
				{
					this.<RefreshStoreSellList>g__CreateOrUpdateItemFrame|99_0(item.ItemPrefab, item.Quantity, ref CS$<>8__locals1);
				}
				using (List<ItemPrefab>.Enumerator enumerator2 = this.ActiveStore.RequestedGoods.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ItemPrefab requestedGood = enumerator2.Current;
						if (!this.itemsToSell.Any((PurchasedItem pi) => pi.ItemPrefab == requestedGood))
						{
							this.<RefreshStoreSellList>g__CreateOrUpdateItemFrame|99_0(requestedGood, 0, ref CS$<>8__locals1);
						}
					}
				}
			}
			List<GUIComponent> removedItemFrames = (from c in this.storeSellList.Content.Children
			where c.UserData is PurchasedItem
			select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>();
			if (this.storeRequestedGoodGroup != null)
			{
				removedItemFrames.AddRange((from c in this.storeRequestedGoodGroup.Children
				where c.UserData is PurchasedItem
				select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>());
			}
			removedItemFrames.ForEach(delegate(GUIComponent f)
			{
				f.RectTransform.Parent = null;
			});
			if (this.activeTab == Store.StoreTab.Sell)
			{
				this.UpdateCategoryButtons();
				this.FilterStoreItems();
			}
			this.SortItems(Store.StoreTab.Sell);
			this.storeSellList.BarScroll = prevSellListScroll;
			this.shoppingCrateSellList.BarScroll = prevShoppingCrateScroll;
		}

		// Token: 0x06001676 RID: 5750 RVA: 0x000D2F44 File Offset: 0x000D1144
		private void RefreshStoreSellFromSubList()
		{
			Store.<>c__DisplayClass100_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float prevSellListScroll = this.storeSellFromSubList.BarScroll;
			float prevShoppingCrateScroll = this.shoppingCrateSellFromSubList.BarScroll;
			Location.StoreInfo activeStore = this.ActiveStore;
			int requestedGoodsCount = (activeStore != null) ? activeStore.RequestedGoods.Count : 0;
			if ((this.ActiveStore == null && this.storeRequestedSubGoodGroup != null) || this.storeRequestedSubGoodGroup != null != this.ActiveStore.RequestedGoods.Any<ItemPrefab>() || requestedGoodsCount != this.prevSubRequestedGoodsCount)
			{
				GUIComponent guicomponent = this.storeSellFromSubList;
				GUILayoutGroup guilayoutGroup = this.storeRequestedSubGoodGroup;
				guicomponent.RemoveChild((guilayoutGroup != null) ? guilayoutGroup.Parent : null);
				if (this.ActiveStore != null && (this.storeRequestedSubGoodGroup == null || requestedGoodsCount != this.prevSubRequestedGoodsCount))
				{
					this.storeRequestedSubGoodGroup = this.CreateDealsGroup(this.storeSellFromSubList, requestedGoodsCount);
					this.storeRequestedSubGoodGroup.Parent.SetAsFirstChild();
				}
				else
				{
					this.storeRequestedSubGoodGroup = null;
				}
				this.storeSellFromSubList.RecalculateChildren();
				this.prevSubRequestedGoodsCount = requestedGoodsCount;
			}
			CS$<>8__locals1.hasPermissions = this.HasSellSubPermissions;
			CS$<>8__locals1.existingItemFrames = new HashSet<GUIComponent>();
			if (this.ActiveStore != null)
			{
				foreach (PurchasedItem item in this.itemsToSellFromSub)
				{
					this.<RefreshStoreSellFromSubList>g__CreateOrUpdateItemFrame|100_0(item.ItemPrefab, item.Quantity, ref CS$<>8__locals1);
				}
				using (List<ItemPrefab>.Enumerator enumerator2 = this.ActiveStore.RequestedGoods.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ItemPrefab requestedGood = enumerator2.Current;
						if (!this.itemsToSellFromSub.Any((PurchasedItem pi) => pi.ItemPrefab == requestedGood))
						{
							this.<RefreshStoreSellFromSubList>g__CreateOrUpdateItemFrame|100_0(requestedGood, 0, ref CS$<>8__locals1);
						}
					}
				}
			}
			List<GUIComponent> removedItemFrames = (from c in this.storeSellFromSubList.Content.Children
			where c.UserData is PurchasedItem
			select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>();
			if (this.storeRequestedSubGoodGroup != null)
			{
				removedItemFrames.AddRange((from c in this.storeRequestedSubGoodGroup.Children
				where c.UserData is PurchasedItem
				select c).Except(CS$<>8__locals1.existingItemFrames).ToList<GUIComponent>());
			}
			removedItemFrames.ForEach(delegate(GUIComponent f)
			{
				f.RectTransform.Parent = null;
			});
			if (this.activeTab == Store.StoreTab.SellSub)
			{
				this.UpdateCategoryButtons();
				this.FilterStoreItems();
			}
			this.SortItems(Store.StoreTab.SellSub);
			this.storeSellFromSubList.BarScroll = prevSellListScroll;
			this.shoppingCrateSellFromSubList.BarScroll = prevShoppingCrateScroll;
		}

		// Token: 0x06001677 RID: 5751 RVA: 0x000D3214 File Offset: 0x000D1414
		private void SetPriceGetters(GUIComponent itemFrame, bool buying)
		{
			if (itemFrame != null)
			{
				object userData = itemFrame.UserData;
				PurchasedItem pi = userData as PurchasedItem;
				if (pi != null)
				{
					GUITextBlock undiscountedPriceBlock = itemFrame.FindChild("undiscountedprice", true) as GUITextBlock;
					if (undiscountedPriceBlock != null)
					{
						if (buying)
						{
							undiscountedPriceBlock.TextGetter = delegate()
							{
								Location.StoreInfo activeStore = this.ActiveStore;
								return TextManager.FormatCurrency((activeStore != null) ? activeStore.GetAdjustedItemBuyPrice(pi.ItemPrefab, null, false) : 0, true);
							};
						}
						else
						{
							undiscountedPriceBlock.TextGetter = delegate()
							{
								Location.StoreInfo activeStore = this.ActiveStore;
								return TextManager.FormatCurrency((activeStore != null) ? activeStore.GetAdjustedItemSellPrice(pi.ItemPrefab, null, false) : 0, true);
							};
						}
					}
					GUITextBlock priceBlock = itemFrame.FindChild("price", true) as GUITextBlock;
					if (priceBlock != null)
					{
						if (buying)
						{
							priceBlock.TextGetter = delegate()
							{
								Location.StoreInfo activeStore = this.ActiveStore;
								return TextManager.FormatCurrency((activeStore != null) ? activeStore.GetAdjustedItemBuyPrice(pi.ItemPrefab, null, true) : 0, true);
							};
							return;
						}
						priceBlock.TextGetter = delegate()
						{
							Location.StoreInfo activeStore = this.ActiveStore;
							return TextManager.FormatCurrency((activeStore != null) ? activeStore.GetAdjustedItemSellPrice(pi.ItemPrefab, null, true) : 0, true);
						};
					}
					return;
				}
			}
		}

		// Token: 0x06001678 RID: 5752 RVA: 0x000D32C8 File Offset: 0x000D14C8
		public void RefreshItemsToSell()
		{
			this.itemsToSell.Clear();
			if (this.ActiveStore == null)
			{
				return;
			}
			IEnumerable<Item> playerItems = this.CargoManager.GetSellableItems(Character.Controlled);
			using (IEnumerator<Item> enumerator = playerItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item playerItem = enumerator.Current;
					PurchasedItem item = this.itemsToSell.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == playerItem.Prefab);
					if (item != null)
					{
						item.Quantity++;
					}
					else if (playerItem.Prefab.GetPriceInfo(this.ActiveStore) != null)
					{
						this.itemsToSell.Add(new PurchasedItem(playerItem.Prefab, 1));
					}
				}
			}
			List<PurchasedItem> itemsInCrate = new List<PurchasedItem>(this.CargoManager.GetSellCrateItems(this.ActiveStore, false));
			using (List<PurchasedItem>.Enumerator enumerator2 = itemsInCrate.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					PurchasedItem crateItem = enumerator2.Current;
					PurchasedItem playerItem2 = this.itemsToSell.Find((PurchasedItem i) => i.ItemPrefab == crateItem.ItemPrefab);
					int playerItemQuantity = (playerItem2 != null) ? playerItem2.Quantity : 0;
					if (crateItem.Quantity > playerItemQuantity)
					{
						this.CargoManager.ModifyItemQuantityInSellCrate(this.ActiveStore.Identifier, crateItem.ItemPrefab, playerItemQuantity - crateItem.Quantity);
					}
				}
			}
			this.needsItemsToSellRefresh = false;
		}

		// Token: 0x06001679 RID: 5753 RVA: 0x000D3470 File Offset: 0x000D1670
		public void RefreshItemsToSellFromSub()
		{
			this.itemsToSellFromSub.Clear();
			if (this.ActiveStore == null)
			{
				return;
			}
			IEnumerable<Item> subItems = this.CargoManager.GetSellableItemsFromSub();
			using (IEnumerator<Item> enumerator = subItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item subItem = enumerator.Current;
					PurchasedItem item = this.itemsToSellFromSub.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == subItem.Prefab);
					if (item != null)
					{
						item.Quantity++;
					}
					else if (subItem.Prefab.GetPriceInfo(this.ActiveStore) != null)
					{
						this.itemsToSellFromSub.Add(new PurchasedItem(subItem.Prefab, 1));
					}
				}
			}
			List<PurchasedItem> itemsInCrate = new List<PurchasedItem>(this.CargoManager.GetSubCrateItems(this.ActiveStore, false));
			using (List<PurchasedItem>.Enumerator enumerator2 = itemsInCrate.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					PurchasedItem crateItem = enumerator2.Current;
					PurchasedItem subItem2 = this.itemsToSellFromSub.Find((PurchasedItem i) => i.ItemPrefab == crateItem.ItemPrefab);
					int subItemQuantity = (subItem2 != null) ? subItem2.Quantity : 0;
					if (crateItem.Quantity > subItemQuantity)
					{
						this.CargoManager.ModifyItemQuantityInSubSellCrate(this.ActiveStore.Identifier, crateItem.ItemPrefab, subItemQuantity - crateItem.Quantity, null);
					}
				}
			}
			this.sellableItemsFromSubUpdateTimer = 0f;
			this.needsItemsToSellFromSubRefresh = false;
		}

		// Token: 0x0600167A RID: 5754 RVA: 0x000D3620 File Offset: 0x000D1820
		private void RefreshShoppingCrateList(IEnumerable<PurchasedItem> items, GUIListBox listBox, Store.StoreTab tab)
		{
			bool hasPermissions = this.HasTabPermissions(tab);
			HashSet<GUIComponent> existingItemFrames = new HashSet<GUIComponent>();
			int totalPrice = 0;
			if (this.ActiveStore != null)
			{
				using (List<PurchasedItem>.Enumerator enumerator = items.ToList<PurchasedItem>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PurchasedItem item = enumerator.Current;
						PriceInfo priceInfo = item.ItemPrefab.GetPriceInfo(this.ActiveStore);
						if (priceInfo != null)
						{
							GUIComponent itemFrame = listBox.Content.FindChild(delegate(GUIComponent c)
							{
								PurchasedItem pi = c.UserData as PurchasedItem;
								return pi != null && pi.ItemPrefab.Identifier == item.ItemPrefab.Identifier;
							}, false);
							GUINumberInput numInput;
							if (itemFrame == null)
							{
								itemFrame = this.CreateItemFrame(item, listBox, tab, !hasPermissions);
								numInput = (itemFrame.FindChild((GUIComponent c) => c is GUINumberInput, true) as GUINumberInput);
							}
							else
							{
								itemFrame.UserData = item;
								numInput = (itemFrame.FindChild((GUIComponent c) => c is GUINumberInput, true) as GUINumberInput);
								if (numInput != null)
								{
									numInput.UserData = item;
									numInput.Enabled = hasPermissions;
									numInput.MaxValueInt = new int?(this.GetMaxAvailable(item.ItemPrefab, tab));
								}
								this.SetOwnedText(itemFrame, null);
								this.SetItemFrameStatus(itemFrame, hasPermissions);
							}
							existingItemFrames.Add(itemFrame);
							this.suppressBuySell = true;
							if (numInput != null)
							{
								if (numInput.IntValue != item.Quantity)
								{
									itemFrame.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
								numInput.IntValue = item.Quantity;
							}
							this.suppressBuySell = false;
							try
							{
								int num;
								switch (tab)
								{
								case Store.StoreTab.Buy:
									num = this.ActiveStore.GetAdjustedItemBuyPrice(item.ItemPrefab, priceInfo, true);
									break;
								case Store.StoreTab.Sell:
									num = this.ActiveStore.GetAdjustedItemSellPrice(item.ItemPrefab, priceInfo, true);
									break;
								case Store.StoreTab.SellSub:
									num = this.ActiveStore.GetAdjustedItemSellPrice(item.ItemPrefab, priceInfo, true);
									break;
								default:
									throw new NotImplementedException();
								}
								int price = num;
								totalPrice += item.Quantity * price;
							}
							catch (NotImplementedException e)
							{
								DebugConsole.LogError("Error getting item price: Uknown store tab type. " + e.StackTrace.CleanupStackTrace(), null, null);
							}
						}
					}
				}
			}
			List<GUIComponent> removedItemFrames = listBox.Content.Children.Except(existingItemFrames).ToList<GUIComponent>();
			removedItemFrames.ForEach(delegate(GUIComponent f)
			{
				listBox.Content.RemoveChild(f);
			});
			this.SortItems(listBox, Store.SortingMethod.CategoryAsc);
			listBox.UpdateScrollBarSize();
			switch (tab)
			{
			case Store.StoreTab.Buy:
				this.buyTotal = totalPrice;
				break;
			case Store.StoreTab.Sell:
				this.sellTotal = totalPrice;
				break;
			case Store.StoreTab.SellSub:
				this.sellFromSubTotal = totalPrice;
				break;
			}
			if (this.activeTab == tab)
			{
				this.SetShoppingCrateTotalText();
			}
			this.SetClearAllButtonStatus();
			this.SetConfirmButtonStatus();
		}

		// Token: 0x0600167B RID: 5755 RVA: 0x000D3998 File Offset: 0x000D1B98
		private void RefreshShoppingCrateBuyList()
		{
			this.RefreshShoppingCrateList(this.CargoManager.GetBuyCrateItems(this.ActiveStore, false), this.shoppingCrateBuyList, Store.StoreTab.Buy);
		}

		// Token: 0x0600167C RID: 5756 RVA: 0x000D39B9 File Offset: 0x000D1BB9
		private void RefreshShoppingCrateSellList()
		{
			this.RefreshShoppingCrateList(this.CargoManager.GetSellCrateItems(this.ActiveStore, false), this.shoppingCrateSellList, Store.StoreTab.Sell);
		}

		// Token: 0x0600167D RID: 5757 RVA: 0x000D39DA File Offset: 0x000D1BDA
		private void RefreshShoppingCrateSellFromSubList()
		{
			this.RefreshShoppingCrateList(this.CargoManager.GetSubCrateItems(this.ActiveStore, false), this.shoppingCrateSellFromSubList, Store.StoreTab.SellSub);
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x000D39FC File Offset: 0x000D1BFC
		private void SortItems(GUIListBox list, Store.SortingMethod sortingMethod)
		{
			Store.<>c__DisplayClass108_0 CS$<>8__locals1 = new Store.<>c__DisplayClass108_0();
			CS$<>8__locals1.sortingMethod = sortingMethod;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.list = list;
			if (this.CurrentLocation == null || this.ActiveStore == null)
			{
				return;
			}
			if (CS$<>8__locals1.sortingMethod == Store.SortingMethod.AlphabeticalAsc || CS$<>8__locals1.sortingMethod == Store.SortingMethod.AlphabeticalDesc)
			{
				CS$<>8__locals1.list.Content.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByName|3));
				GUILayoutGroup specialsGroup = CS$<>8__locals1.<SortItems>g__GetSpecialsGroup|0();
				if (specialsGroup != null)
				{
					specialsGroup.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByName|3));
					specialsGroup.Recalculate();
					return;
				}
			}
			else if (CS$<>8__locals1.sortingMethod == Store.SortingMethod.PriceAsc || CS$<>8__locals1.sortingMethod == Store.SortingMethod.PriceDesc)
			{
				this.SortItems(CS$<>8__locals1.list, Store.SortingMethod.AlphabeticalAsc);
				if (CS$<>8__locals1.list != this.storeBuyList && CS$<>8__locals1.list != this.shoppingCrateBuyList)
				{
					CS$<>8__locals1.list.Content.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareBySellPrice|4));
					GUILayoutGroup specialsGroup2 = CS$<>8__locals1.<SortItems>g__GetSpecialsGroup|0();
					if (specialsGroup2 != null)
					{
						specialsGroup2.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareBySellPrice|4));
						specialsGroup2.Recalculate();
						return;
					}
				}
				else
				{
					CS$<>8__locals1.list.Content.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByBuyPrice|5));
					GUILayoutGroup specialsGroup3 = CS$<>8__locals1.<SortItems>g__GetSpecialsGroup|0();
					if (specialsGroup3 != null)
					{
						specialsGroup3.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByBuyPrice|5));
						specialsGroup3.Recalculate();
						return;
					}
				}
			}
			else if (CS$<>8__locals1.sortingMethod == Store.SortingMethod.CategoryAsc)
			{
				this.SortItems(CS$<>8__locals1.list, Store.SortingMethod.AlphabeticalAsc);
				CS$<>8__locals1.list.Content.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByCategory|6));
				GUILayoutGroup specialsGroup4 = CS$<>8__locals1.<SortItems>g__GetSpecialsGroup|0();
				if (specialsGroup4 != null)
				{
					specialsGroup4.RectTransform.SortChildren(new Comparison<RectTransform>(CS$<>8__locals1.<SortItems>g__CompareByCategory|6));
					specialsGroup4.Recalculate();
				}
			}
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x000D3BCA File Offset: 0x000D1DCA
		private void SortItems(Store.StoreTab tab, Store.SortingMethod sortingMethod)
		{
			this.tabSortingMethods[tab] = sortingMethod;
			this.SortItems(this.tabLists[tab], sortingMethod);
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x000D3BEC File Offset: 0x000D1DEC
		private void SortItems(Store.StoreTab tab)
		{
			this.SortItems(tab, this.tabSortingMethods[tab]);
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x000D3C01 File Offset: 0x000D1E01
		private void SortActiveTabItems(Store.SortingMethod sortingMethod)
		{
			this.SortItems(this.activeTab, sortingMethod);
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x000D3C10 File Offset: 0x000D1E10
		private GUIComponent CreateItemFrame(PurchasedItem pi, GUIComponent parentComponent, Store.StoreTab containingTab, bool forceDisable = false)
		{
			GUIListBox parentListBox = parentComponent as GUIListBox;
			int width;
			RectTransform parent;
			if (parentListBox != null)
			{
				width = parentListBox.Content.Rect.Width;
				parent = parentListBox.Content.RectTransform;
			}
			else
			{
				width = parentComponent.Rect.Width;
				parent = parentComponent.RectTransform;
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(width, (int)(GUI.yScale * 80f)), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
			{
				UserData = pi
			};
			GUILayoutGroup mainGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 1f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			float nameAndIconRelativeWidth = 0.635f;
			float iconRelativeWidth = 0f;
			float priceAndButtonRelativeWidth = 1f - nameAndIconRelativeWidth;
			Sprite itemIcon = pi.ItemPrefab.InventoryIcon ?? pi.ItemPrefab.Sprite;
			if (itemIcon != null)
			{
				iconRelativeWidth = 0.9f * (float)mainGroup.Rect.Height / (float)mainGroup.Rect.Width;
				GUIImage img = new GUIImage(new RectTransform(new Vector2(iconRelativeWidth, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), itemIcon, true, null)
				{
					CanBeFocused = false,
					Color = ((itemIcon == pi.ItemPrefab.InventoryIcon) ? pi.ItemPrefab.InventoryIconColor : pi.ItemPrefab.SpriteColor) * (forceDisable ? 0.5f : 1f),
					UserData = "icon"
				};
				img.RectTransform.MaxSize = img.Rect.Size;
			}
			GUIFrame nameAndQuantityFrame = new GUIFrame(new RectTransform(new Vector2(nameAndIconRelativeWidth - iconRelativeWidth, 1f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup nameAndQuantityGroup = new GUILayoutGroup(new RectTransform(Vector2.One, nameAndQuantityFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false,
				Stretch = true
			};
			bool isSellingRelatedList = containingTab > Store.StoreTab.Buy;
			bool locationHasDealOnItem = isSellingRelatedList ? this.ActiveStore.RequestedGoods.Contains(pi.ItemPrefab) : this.ActiveStore.DailySpecials.Contains(pi.ItemPrefab);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.4f), nameAndQuantityGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = RichString.Rich(pi.ItemPrefab.Name, null);
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock nameBlock = new GUITextBlock(rectT, text, null, font, Alignment.BottomLeft, false, "", null)
			{
				CanBeFocused = false,
				Shadow = locationHasDealOnItem,
				TextColor = Color.White * (forceDisable ? 0.5f : 1f),
				TextScale = 0.85f,
				UserData = "name"
			};
			if (locationHasDealOnItem)
			{
				float relativeWidth = 0.9f * (float)nameAndQuantityFrame.Rect.Height / (float)nameAndQuantityFrame.Rect.Width;
				Vector2 dealIconSize = new Vector2(relativeWidth, 0.9f) * 0.5f;
				GUIImage dealIcon = new GUIImage(new RectTransform(dealIconSize, nameAndQuantityFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point((int)nameBlock.Padding.X, 0)
				}, "StoreDealIcon", true)
				{
					CanBeFocused = false,
					UserData = "StoreDealIcon"
				};
				Color dealIconColor = dealIcon.Color;
				if (forceDisable)
				{
					dealIconColor.A = 0;
				}
				dealIcon.Color = dealIconColor;
				dealIcon.SetAsFirstChild();
			}
			bool isParentOnLeftSideOfInterface = parentComponent == this.storeBuyList || parentComponent == this.storeDailySpecialsGroup || parentComponent == this.storeSellList || parentComponent == this.storeRequestedGoodGroup || parentComponent == this.storeSellFromSubList || parentComponent == this.storeRequestedSubGoodGroup;
			GUILayoutGroup shoppingCrateAmountGroup = null;
			GUINumberInput amountInput = null;
			if (isParentOnLeftSideOfInterface)
			{
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.3f), nameAndQuantityGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = Store.CreateQuantityLabelText(containingTab, pi.Quantity);
				font = GUIStyle.Font;
				GUITextBlock guitextBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.BottomLeft, false, "", null);
				guitextBlock.CanBeFocused = false;
				guitextBlock.Shadow = locationHasDealOnItem;
				guitextBlock.TextColor = Color.White * (forceDisable ? 0.5f : 1f);
				guitextBlock.TextScale = 0.85f;
				guitextBlock.UserData = "quantitylabel";
			}
			else
			{
				float relativePadding = nameBlock.Padding.X / (float)nameBlock.Rect.Width;
				shoppingCrateAmountGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f - relativePadding, 0.6f), nameAndQuantityGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(relativePadding, 0f)
				}, true, Anchor.TopLeft)
				{
					RelativeSpacing = 0.02f
				};
				amountInput = new GUINumberInput(new RectTransform(new Vector2(0.4f, 1f), shoppingCrateAmountGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					MinValueInt = new int?(0),
					MaxValueInt = new int?(this.GetMaxAvailable(pi.ItemPrefab, containingTab)),
					UserData = pi,
					IntValue = pi.Quantity
				};
				amountInput.Enabled = !forceDisable;
				amountInput.TextBox.OnSelected += delegate(GUITextBox sender, Keys key)
				{
					this.suppressBuySell = true;
				};
				amountInput.TextBox.OnDeselected += delegate(GUITextBox sender, Keys key)
				{
					this.suppressBuySell = false;
					GUINumberInput.OnValueChangedHandler onValueChanged = amountInput.OnValueChanged;
					if (onValueChanged == null)
					{
						return;
					}
					onValueChanged(amountInput);
				};
				GUINumberInput amountInput3 = amountInput;
				amountInput3.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(amountInput3.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numberInput)
				{
					if (this.suppressBuySell)
					{
						return;
					}
					PurchasedItem purchasedItem = numberInput.UserData as PurchasedItem;
					if (!this.HasActiveTabPermissions())
					{
						numberInput.IntValue = purchasedItem.Quantity;
						return;
					}
					this.AddToShoppingCrate(purchasedItem, numberInput.IntValue - purchasedItem.Quantity);
				}));
				frame.HoverColor = (frame.SelectedColor = Color.Transparent);
			}
			RectTransform rectTransform = (shoppingCrateAmountGroup == null) ? new RectTransform(new Vector2(1f, 0.3f), nameAndQuantityGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal) : new RectTransform(new Vector2(0.6f, 1f), shoppingCrateAmountGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RectTransform rectT3 = rectTransform;
			RichString text3 = string.Empty;
			font = GUIStyle.Font;
			Alignment textAlignment = (shoppingCrateAmountGroup == null) ? Alignment.TopLeft : Alignment.CenterLeft;
			GUITextBlock ownedLabel = new GUITextBlock(rectT3, text3, null, font, textAlignment, false, "", null)
			{
				CanBeFocused = false,
				Shadow = locationHasDealOnItem,
				TextColor = Color.White * (forceDisable ? 0.5f : 1f),
				TextScale = 0.85f,
				UserData = "owned"
			};
			this.SetOwnedText(frame, ownedLabel);
			if (shoppingCrateAmountGroup != null)
			{
				shoppingCrateAmountGroup.Recalculate();
			}
			float buttonRelativeWidth = 0.9f * (float)mainGroup.Rect.Height / (float)mainGroup.Rect.Width;
			GUIFrame priceFrame = new GUIFrame(new RectTransform(new Vector2(priceAndButtonRelativeWidth - buttonRelativeWidth, 1f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.5f), priceFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text4 = "0 MK";
			font = GUIStyle.SubHeadingFont;
			GUITextBlock priceBlock = new GUITextBlock(rectT4, text4, null, font, Alignment.Right, false, "", null)
			{
				CanBeFocused = false,
				TextColor = (locationHasDealOnItem ? this.storeSpecialColor : Color.White),
				UserData = "price"
			};
			priceBlock.Color *= (forceDisable ? 0.5f : 1f);
			priceBlock.CalculateHeightFromText(0, false);
			if (locationHasDealOnItem)
			{
				RectTransform rectTransform2 = new RectTransform(new Vector2(1f, 0.25f), priceFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
				rectTransform2.AbsoluteOffset = new Point(0, priceBlock.RectTransform.ScaledSize.Y);
				RichString text5 = "";
				font = GUIStyle.SmallFont;
				GUITextBlock guitextBlock2 = new GUITextBlock(rectTransform2, text5, null, font, Alignment.Center, false, "", null);
				guitextBlock2.CanBeFocused = false;
				guitextBlock2.Strikethrough = new GUITextBlock.StrikethroughSettings(new Color?(priceBlock.TextColor), 1, 1);
				guitextBlock2.TextColor = priceBlock.TextColor;
				guitextBlock2.UserData = "undiscountedprice";
			}
			this.SetPriceGetters(frame, !isSellingRelatedList);
			if (isParentOnLeftSideOfInterface)
			{
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(buttonRelativeWidth, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "StoreAddToCrateButton", null);
				guibutton.ClickSound = GUISoundType.Cart;
				guibutton.Enabled = (!forceDisable && pi.Quantity > 0);
				guibutton.ForceUpperCase = ForceUpperCase.Yes;
				guibutton.UserData = "addbutton";
				guibutton.OnClicked = ((GUIButton button, object userData) => this.AddToShoppingCrate(pi, 1));
			}
			else
			{
				GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(buttonRelativeWidth, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "StoreRemoveFromCrateButton", null);
				guibutton2.ClickSound = GUISoundType.Cart;
				guibutton2.Enabled = !forceDisable;
				guibutton2.ForceUpperCase = ForceUpperCase.Yes;
				guibutton2.UserData = "removebutton";
				guibutton2.OnClicked = ((GUIButton button, object userData) => this.ClearFromShoppingCrate(pi));
			}
			if (parentListBox != null)
			{
				parentListBox.RecalculateChildren();
			}
			else
			{
				GUILayoutGroup parentLayoutGroup = parentComponent as GUILayoutGroup;
				if (parentLayoutGroup != null)
				{
					parentLayoutGroup.Recalculate();
				}
			}
			mainGroup.Recalculate();
			mainGroup.RectTransform.RecalculateChildren(true, true);
			GUINumberInput amountInput2 = amountInput;
			if (amountInput2 != null)
			{
				amountInput2.LayoutGroup.Recalculate();
			}
			nameBlock.Text = ToolBox.LimitString(nameBlock.Text.SanitizedString, nameBlock.Font, nameBlock.Rect.Width);
			mainGroup.RectTransform.Children.ForEach(delegate(RectTransform c)
			{
				c.IsFixedSize = true;
			});
			return frame;
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x000D4870 File Offset: 0x000D2A70
		private void UpdateOwnedItems()
		{
			this.OwnedItems.Clear();
			if (this.ActiveStore == null)
			{
				return;
			}
			Submarine mainSub = Submarine.MainSub;
			List<Item> subItems = (mainSub != null) ? mainSub.GetItems(true) : null;
			if (subItems != null)
			{
				foreach (Item subItem in subItems)
				{
					if (subItem.Components.All(delegate(ItemComponent c)
					{
						Holdable h = c as Holdable;
						return h == null || !h.Attachable || !h.Attached;
					}))
					{
						if (subItem.Components.All(delegate(ItemComponent c)
						{
							Wire w = c as Wire;
							if (w != null)
							{
								return w.Connections.All((Connection c) => c == null);
							}
							return true;
						}) && Store.<UpdateOwnedItems>g__ItemAndAllContainersInteractable|113_2(subItem))
						{
							Entity rootInventoryOwner2 = subItem.GetRootInventoryOwner();
							if (!(rootInventoryOwner2 is Character))
							{
								this.<UpdateOwnedItems>g__AddOwnedItem|113_3(subItem);
							}
						}
					}
				}
			}
			foreach (Item item in Item.ItemList)
			{
				if (item != null && !item.Removed)
				{
					Entity rootInventoryOwner = item.GetRootInventoryOwner();
					bool ownedByCrewMember = GameMain.GameSession.CrewManager.GetCharacters().Any((Character c) => c == rootInventoryOwner);
					if (ownedByCrewMember)
					{
						this.<UpdateOwnedItems>g__AddOwnedItem|113_3(item);
					}
				}
			}
			CargoManager cargoManager = this.CargoManager;
			if (cargoManager != null)
			{
				(from pi in cargoManager.GetPurchasedItems(this.ActiveStore, false)
				where !pi.DeliverImmediately
				select pi).ForEach(delegate(PurchasedItem pi)
				{
					this.<UpdateOwnedItems>g__AddNonEmptyOwnedItems|113_4(pi);
				});
			}
			this.ownedItemsUpdateTimer = 0f;
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x000D4A44 File Offset: 0x000D2C44
		private void SetItemFrameStatus(GUIComponent itemFrame, bool enabled)
		{
			float full = 1f;
			float dim = 0.7f;
			float alpha = enabled ? full : dim;
			PurchasedItem pi = ((itemFrame != null) ? itemFrame.UserData : null) as PurchasedItem;
			if (pi == null)
			{
				return;
			}
			if (pi.IsStoreComponentEnabled != null && pi.IsStoreComponentEnabled.Value == enabled)
			{
				return;
			}
			GUIImage icon = itemFrame.FindChild("icon", true) as GUIImage;
			if (icon != null)
			{
				ItemPrefab itemPrefab = pi.ItemPrefab;
				if (((itemPrefab != null) ? itemPrefab.InventoryIcon : null) != null)
				{
					icon.Color = pi.ItemPrefab.InventoryIconColor * alpha;
				}
				else
				{
					ItemPrefab itemPrefab2 = pi.ItemPrefab;
					if (((itemPrefab2 != null) ? itemPrefab2.Sprite : null) != null)
					{
						icon.Color = pi.ItemPrefab.SpriteColor * alpha;
					}
				}
			}
			Color color = Color.White * alpha;
			GUITextBlock name = itemFrame.FindChild("name", true) as GUITextBlock;
			if (name != null)
			{
				name.TextColor = color;
			}
			GUITextBlock qty = itemFrame.FindChild("quantitylabel", true) as GUITextBlock;
			if (qty != null)
			{
				qty.TextColor = color;
			}
			else
			{
				GUINumberInput numberInput = itemFrame.FindChild((GUIComponent c) => c is GUINumberInput, true) as GUINumberInput;
				if (numberInput != null)
				{
					numberInput.Enabled = enabled;
				}
			}
			GUITextBlock ownedBlock = itemFrame.FindChild("owned", true) as GUITextBlock;
			if (ownedBlock != null)
			{
				ownedBlock.TextColor = color;
			}
			bool isDiscounted = false;
			GUITextBlock undiscountedPriceBlock = itemFrame.FindChild("undiscountedprice", true) as GUITextBlock;
			if (undiscountedPriceBlock != null)
			{
				undiscountedPriceBlock.TextColor = color;
				undiscountedPriceBlock.Strikethrough.Color = color;
				isDiscounted = true;
			}
			GUITextBlock priceBlock = itemFrame.FindChild("price", true) as GUITextBlock;
			if (priceBlock != null)
			{
				priceBlock.TextColor = (isDiscounted ? (this.storeSpecialColor * alpha) : color);
			}
			GUIButton addButton = itemFrame.FindChild("addbutton", true) as GUIButton;
			if (addButton != null)
			{
				addButton.Enabled = enabled;
			}
			else
			{
				GUIButton removeButton = itemFrame.FindChild("removebutton", true) as GUIButton;
				if (removeButton != null)
				{
					removeButton.Enabled = enabled;
				}
			}
			GUIImage dealIcon = itemFrame.FindChild("StoreDealIcon", true) as GUIImage;
			if (dealIcon != null)
			{
				dealIcon.Color *= alpha;
			}
			pi.IsStoreComponentEnabled = new bool?(enabled);
			itemFrame.UserData = pi;
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x000D4CA8 File Offset: 0x000D2EA8
		private static void SetQuantityLabelText(Store.StoreTab mode, GUIComponent itemFrame)
		{
			GUITextBlock label = ((itemFrame != null) ? itemFrame.FindChild("quantitylabel", true) : null) as GUITextBlock;
			if (label != null)
			{
				label.Text = Store.CreateQuantityLabelText(mode, (itemFrame.UserData as PurchasedItem).Quantity);
			}
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x000D4CF4 File Offset: 0x000D2EF4
		private static LocalizedString CreateQuantityLabelText(Store.StoreTab mode, int quantity)
		{
			try
			{
				string text;
				switch (mode)
				{
				case Store.StoreTab.Buy:
					text = "campaignstore.instock";
					break;
				case Store.StoreTab.Sell:
					text = "campaignstore.ownedinventory";
					break;
				case Store.StoreTab.SellSub:
					text = "campaignstore.ownedsub";
					break;
				default:
					throw new NotImplementedException();
				}
				string textTag = text;
				return TextManager.GetWithVariable(textTag, "[amount]", quantity.ToString(), FormatCapitals.No);
			}
			catch (NotImplementedException e)
			{
				string errorMsg = "Error creating a store quantity label text: unknown store tab.\n" + e.StackTrace.CleanupStackTrace();
				DebugConsole.AddWarning(errorMsg, null);
			}
			return string.Empty;
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x000D4D90 File Offset: 0x000D2F90
		private void SetOwnedText(GUIComponent itemComponent, GUITextBlock ownedLabel = null)
		{
			if (ownedLabel == null)
			{
				ownedLabel = (((itemComponent != null) ? itemComponent.FindChild("owned", true) : null) as GUITextBlock);
			}
			if (itemComponent == null && ownedLabel == null)
			{
				return;
			}
			PurchasedItem purchasedItem = ((itemComponent != null) ? itemComponent.UserData : null) as PurchasedItem;
			Store.ItemQuantity itemQuantity = null;
			LocalizedString ownedLabelText = string.Empty;
			if (purchasedItem != null && this.OwnedItems.TryGetValue(purchasedItem.ItemPrefab, out itemQuantity) && itemQuantity.Total > 0)
			{
				if (itemQuantity.AllNonEmpty)
				{
					ownedLabelText = TextManager.GetWithVariable("campaignstore.owned", "[amount]", itemQuantity.Total.ToString(), FormatCapitals.No);
				}
				else
				{
					ownedLabelText = TextManager.GetWithVariables("campaignstore.ownedspecific", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[nonempty]", itemQuantity.NonEmpty.ToString()),
						new ValueTuple<string, string>("[total]", itemQuantity.Total.ToString())
					});
				}
			}
			if (itemComponent != null)
			{
				LocalizedString toolTip = string.Empty;
				if (purchasedItem.ItemPrefab != null)
				{
					toolTip = purchasedItem.ItemPrefab.GetTooltip(Character.Controlled);
					if (itemQuantity != null)
					{
						if (itemQuantity.AllNonEmpty)
						{
							LocalizedString left = toolTip;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
							defaultInterpolatedStringHandler.AppendLiteral("\n\n");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(ownedLabelText);
							toolTip = left + defaultInterpolatedStringHandler.ToStringAndClear();
						}
						else
						{
							LocalizedString left2 = toolTip;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("\n\n");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.GetWithVariable("campaignstore.ownednonempty", "[amount]", itemQuantity.NonEmpty.ToString(), FormatCapitals.No));
							toolTip = left2 + defaultInterpolatedStringHandler2.ToStringAndClear();
							LocalizedString left3 = toolTip;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("\n");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.GetWithVariable("campaignstore.ownedtotal", "[amount]", itemQuantity.Total.ToString(), FormatCapitals.No));
							toolTip = left3 + defaultInterpolatedStringHandler3.ToStringAndClear();
						}
					}
					PriceInfo priceInfo = purchasedItem.ItemPrefab.GetPriceInfo(this.ActiveStore);
					GameSession gameSession = GameMain.GameSession;
					CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
					if (priceInfo != null && campaign != null)
					{
						Identifier faction = this.ActiveStore.GetMerchantOrLocationFactionIdentifier();
						float requiredReputation = Store.GetReputationRequirement(priceInfo, faction);
						if (requiredReputation > 0f)
						{
							LocalizedString repStr = TextManager.GetWithVariables("campaignstore.reputationrequired", new ValueTuple<string, string>[]
							{
								new ValueTuple<string, string>("[amount]", ((int)requiredReputation).ToString()),
								new ValueTuple<string, string>("[faction]", TextManager.Get("faction." + faction.ToString()).Value)
							});
							Color color = (MathF.Round(campaign.GetReputation(faction)) < requiredReputation) ? GUIStyle.Orange : GUIStyle.Green;
							LocalizedString left4 = toolTip;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(20, 2);
							defaultInterpolatedStringHandler4.AppendLiteral("\n‖color:");
							defaultInterpolatedStringHandler4.AppendFormatted(color.ToStringHex());
							defaultInterpolatedStringHandler4.AppendLiteral("‖");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(repStr);
							defaultInterpolatedStringHandler4.AppendLiteral("‖color:end‖");
							toolTip = left4 + defaultInterpolatedStringHandler4.ToStringAndClear();
						}
					}
				}
				itemComponent.ToolTip = RichString.Rich(toolTip, null);
			}
			if (ownedLabel != null)
			{
				ownedLabel.Text = ownedLabelText;
			}
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x000D50F4 File Offset: 0x000D32F4
		private int GetMaxAvailable(ItemPrefab itemPrefab, Store.StoreTab mode)
		{
			List<PurchasedItem> list = null;
			try
			{
				List<PurchasedItem> list2;
				switch (mode)
				{
				case Store.StoreTab.Buy:
				{
					Location.StoreInfo activeStore = this.ActiveStore;
					list2 = ((activeStore != null) ? activeStore.Stock : null);
					break;
				}
				case Store.StoreTab.Sell:
					list2 = this.itemsToSell;
					break;
				case Store.StoreTab.SellSub:
					list2 = this.itemsToSellFromSub;
					break;
				default:
					throw new NotImplementedException();
				}
				list = list2;
			}
			catch (NotImplementedException e)
			{
				DebugConsole.LogError("Error getting item availability: Unknown store tab type. " + e.StackTrace.CleanupStackTrace(), null, null);
			}
			if (list != null)
			{
				PurchasedItem item = list.Find((PurchasedItem i) => i.ItemPrefab == itemPrefab);
				if (item != null)
				{
					if (mode == Store.StoreTab.Buy)
					{
						return Math.Max(item.Quantity - this.CargoManager.GetPurchasedItemCount(this.ActiveStore, item.ItemPrefab), 0);
					}
					return item.Quantity;
				}
			}
			return 0;
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x000D51D8 File Offset: 0x000D33D8
		private bool ModifyBuyQuantity(PurchasedItem item, int quantity)
		{
			if (((item != null) ? item.ItemPrefab : null) == null)
			{
				return false;
			}
			if (!this.HasBuyPermissions)
			{
				return false;
			}
			if (quantity > 0)
			{
				PurchasedItem crateItem = this.CargoManager.GetBuyCrateItem(this.ActiveStore, item.ItemPrefab);
				if (crateItem != null && crateItem.Quantity >= 100)
				{
					return false;
				}
				int totalQuantityToBuy = (crateItem != null) ? (crateItem.Quantity + quantity) : quantity;
				if (totalQuantityToBuy > this.GetMaxAvailable(item.ItemPrefab, Store.StoreTab.Buy))
				{
					return false;
				}
			}
			this.CargoManager.ModifyItemQuantityInBuyCrate(this.ActiveStore.Identifier, item.ItemPrefab, quantity, null);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.SendCampaignState();
			}
			return true;
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x000D527C File Offset: 0x000D347C
		private bool ModifySellQuantity(PurchasedItem item, int quantity)
		{
			if (((item != null) ? item.ItemPrefab : null) == null)
			{
				return false;
			}
			if (!this.HasSellInventoryPermissions)
			{
				return false;
			}
			if (quantity > 0)
			{
				PurchasedItem itemToSell = this.CargoManager.GetSellCrateItem(this.ActiveStore, item.ItemPrefab);
				int totalQuantityToSell = (itemToSell != null) ? (itemToSell.Quantity + quantity) : quantity;
				if (totalQuantityToSell > this.GetMaxAvailable(item.ItemPrefab, Store.StoreTab.Sell))
				{
					return false;
				}
			}
			this.CargoManager.ModifyItemQuantityInSellCrate(this.ActiveStore.Identifier, item.ItemPrefab, quantity);
			return true;
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x000D5300 File Offset: 0x000D3500
		private bool ModifySellFromSubQuantity(PurchasedItem item, int quantity)
		{
			if (((item != null) ? item.ItemPrefab : null) == null)
			{
				return false;
			}
			if (!this.HasSellSubPermissions)
			{
				return false;
			}
			if (quantity > 0)
			{
				PurchasedItem itemToSell = this.CargoManager.GetSubCrateItem(this.ActiveStore, item.ItemPrefab);
				int totalQuantityToSell = (itemToSell != null) ? (itemToSell.Quantity + quantity) : quantity;
				if (totalQuantityToSell > this.GetMaxAvailable(item.ItemPrefab, Store.StoreTab.SellSub))
				{
					return false;
				}
			}
			this.CargoManager.ModifyItemQuantityInSubSellCrate(this.ActiveStore.Identifier, item.ItemPrefab, quantity, null);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.SendCampaignState();
			}
			return true;
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x000D5394 File Offset: 0x000D3594
		private bool AddToShoppingCrate(PurchasedItem item, int quantity = 1)
		{
			if (item == null)
			{
				return false;
			}
			bool result;
			try
			{
				bool flag;
				switch (this.activeTab)
				{
				case Store.StoreTab.Buy:
					flag = this.ModifyBuyQuantity(item, quantity);
					break;
				case Store.StoreTab.Sell:
					flag = this.ModifySellQuantity(item, quantity);
					break;
				case Store.StoreTab.SellSub:
					flag = this.ModifySellFromSubQuantity(item, quantity);
					break;
				default:
					throw new NotImplementedException();
				}
				result = flag;
			}
			catch (NotImplementedException e)
			{
				DebugConsole.LogError("Error adding an item to the shopping crate: Uknown store tab type. " + e.StackTrace.CleanupStackTrace(), null, null);
				result = false;
			}
			return result;
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x000D5428 File Offset: 0x000D3628
		private bool ClearFromShoppingCrate(PurchasedItem item)
		{
			if (item == null)
			{
				return false;
			}
			bool result;
			try
			{
				bool flag;
				switch (this.activeTab)
				{
				case Store.StoreTab.Buy:
					flag = this.ModifyBuyQuantity(item, -item.Quantity);
					break;
				case Store.StoreTab.Sell:
					flag = this.ModifySellQuantity(item, -item.Quantity);
					break;
				case Store.StoreTab.SellSub:
					flag = this.ModifySellFromSubQuantity(item, -item.Quantity);
					break;
				default:
					throw new NotImplementedException();
				}
				result = flag;
			}
			catch (NotImplementedException e)
			{
				DebugConsole.LogError("Error clearing the shopping crate: Uknown store tab type. " + e.StackTrace.CleanupStackTrace(), null, null);
				result = false;
			}
			return result;
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x000D54D0 File Offset: 0x000D36D0
		private bool BuyItems()
		{
			Store.<>c__DisplayClass124_0 CS$<>8__locals1 = new Store.<>c__DisplayClass124_0();
			CS$<>8__locals1.<>4__this = this;
			if (!this.HasBuyPermissions)
			{
				return false;
			}
			CS$<>8__locals1.itemsToPurchase = new List<PurchasedItem>(this.CargoManager.GetBuyCrateItems(this.ActiveStore, false));
			List<PurchasedItem> itemsToRemove = new List<PurchasedItem>();
			int totalPrice = 0;
			foreach (PurchasedItem item in CS$<>8__locals1.itemsToPurchase)
			{
				if (item != null)
				{
					PriceInfo priceInfo;
					if (item.ItemPrefab == null || !item.ItemPrefab.CanBeBoughtFrom(this.ActiveStore, out priceInfo))
					{
						itemsToRemove.Add(item);
					}
					else if (item.ItemPrefab.DefaultPrice.RequiresUnlock && !CargoManager.HasUnlockedStoreItem(item.ItemPrefab))
					{
						itemsToRemove.Add(item);
					}
					else
					{
						totalPrice += item.Quantity * this.ActiveStore.GetAdjustedItemBuyPrice(item.ItemPrefab, priceInfo, true);
					}
				}
			}
			itemsToRemove.ForEach(delegate(PurchasedItem i)
			{
				CS$<>8__locals1.itemsToPurchase.Remove(i);
			});
			if (CS$<>8__locals1.itemsToPurchase.None(null) || this.Balance < totalPrice)
			{
				return false;
			}
			if (CampaignMode.AllowImmediateItemDelivery())
			{
				this.deliveryPrompt = new GUIMessageBox(TextManager.Get("newsupplies"), TextManager.Get("suppliespurchased.deliverymethod"), new LocalizedString[]
				{
					TextManager.Get("suppliespurchased.deliverymethod.deliverimmediately"),
					TextManager.Get("suppliespurchased.deliverymethod.delivertosub")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				this.deliveryPrompt.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
				{
					base.<BuyItems>g__ConfirmPurchase|3(true);
					GUIMessageBox guimessageBox = CS$<>8__locals1.<>4__this.deliveryPrompt;
					if (guimessageBox != null)
					{
						guimessageBox.Close();
					}
					return true;
				};
				this.deliveryPrompt.Buttons[1].OnClicked = delegate(GUIButton btn, object userdata)
				{
					base.<BuyItems>g__ConfirmPurchase|3(false);
					GUIMessageBox guimessageBox = CS$<>8__locals1.<>4__this.deliveryPrompt;
					if (guimessageBox != null)
					{
						guimessageBox.Close();
					}
					return true;
				};
			}
			else
			{
				CS$<>8__locals1.<BuyItems>g__ConfirmPurchase|3(false);
			}
			return false;
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x000D56C8 File Offset: 0x000D38C8
		public void OnDeselected()
		{
			GUIMessageBox guimessageBox = this.deliveryPrompt;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			this.deliveryPrompt = null;
		}

		// Token: 0x06001690 RID: 5776 RVA: 0x000D56E4 File Offset: 0x000D38E4
		private bool SellItems()
		{
			if (!this.HasActiveTabPermissions())
			{
				return false;
			}
			List<PurchasedItem> itemsToSell;
			try
			{
				Store.StoreTab storeTab = this.activeTab;
				List<PurchasedItem> list;
				if (storeTab != Store.StoreTab.Sell)
				{
					if (storeTab != Store.StoreTab.SellSub)
					{
						throw new NotImplementedException();
					}
					list = new List<PurchasedItem>(this.CargoManager.GetSubCrateItems(this.ActiveStore, false));
				}
				else
				{
					list = new List<PurchasedItem>(this.CargoManager.GetSellCrateItems(this.ActiveStore, false));
				}
				itemsToSell = list;
			}
			catch (NotImplementedException e)
			{
				DebugConsole.LogError("Error confirming the store transaction: Unknown store tab type. " + e.StackTrace.CleanupStackTrace(), null, null);
				return false;
			}
			List<PurchasedItem> itemsToRemove = new List<PurchasedItem>();
			int totalValue = 0;
			foreach (PurchasedItem item in itemsToSell)
			{
				PriceInfo priceInfo2;
				if (item == null)
				{
					priceInfo2 = null;
				}
				else
				{
					ItemPrefab itemPrefab = item.ItemPrefab;
					priceInfo2 = ((itemPrefab != null) ? itemPrefab.GetPriceInfo(this.ActiveStore) : null);
				}
				PriceInfo priceInfo = priceInfo2;
				if (priceInfo != null)
				{
					totalValue += item.Quantity * this.ActiveStore.GetAdjustedItemSellPrice(item.ItemPrefab, priceInfo, true);
				}
				else
				{
					itemsToRemove.Add(item);
				}
			}
			itemsToRemove.ForEach(delegate(PurchasedItem i)
			{
				itemsToSell.Remove(i);
			});
			if (itemsToSell.None(null) || totalValue > this.ActiveStore.Balance)
			{
				return false;
			}
			this.CargoManager.SellItems(this.ActiveStore.Identifier, itemsToSell, this.activeTab);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				client.SendCampaignState();
			}
			return false;
		}

		// Token: 0x06001691 RID: 5777 RVA: 0x000D5894 File Offset: 0x000D3A94
		private void SetShoppingCrateTotalText()
		{
			if (this.ActiveStore == null)
			{
				this.shoppingCrateTotal.Text = TextManager.FormatCurrency(0, true);
				this.shoppingCrateTotal.TextColor = Color.White;
				return;
			}
			if (this.IsBuying)
			{
				this.shoppingCrateTotal.Text = TextManager.FormatCurrency(this.buyTotal, true);
				this.shoppingCrateTotal.TextColor = ((this.Balance < this.buyTotal) ? Color.Red : Color.White);
				return;
			}
			Store.StoreTab storeTab = this.activeTab;
			int num;
			if (storeTab != Store.StoreTab.Sell)
			{
				if (storeTab != Store.StoreTab.SellSub)
				{
					throw new NotImplementedException();
				}
				num = this.sellFromSubTotal;
			}
			else
			{
				num = this.sellTotal;
			}
			int total = num;
			this.shoppingCrateTotal.Text = TextManager.FormatCurrency(total, true);
			this.shoppingCrateTotal.TextColor = ((this.CurrentLocation != null && total > this.ActiveStore.Balance) ? Color.Red : Color.White);
		}

		// Token: 0x06001692 RID: 5778 RVA: 0x000D598C File Offset: 0x000D3B8C
		private void SetConfirmButtonBehavior()
		{
			if (this.ActiveStore == null)
			{
				this.confirmButton.OnClicked = null;
				return;
			}
			if (this.IsBuying)
			{
				this.confirmButton.ClickSound = GUISoundType.ConfirmTransaction;
				this.confirmButton.Text = TextManager.Get("CampaignStore.Purchase");
				this.confirmButton.OnClicked = ((GUIButton b, object o) => this.BuyItems());
				return;
			}
			this.confirmButton.ClickSound = GUISoundType.Select;
			this.confirmButton.Text = TextManager.Get("CampaignStoreTab.Sell");
			this.confirmButton.OnClicked = delegate(GUIButton b, object o)
			{
				GUIMessageBox confirmDialog = new GUIMessageBox(TextManager.Get("FireWarningHeader"), TextManager.Get("CampaignStore.SellWarningText"), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				confirmDialog.Buttons[0].ClickSound = GUISoundType.ConfirmTransaction;
				confirmDialog.Buttons[0].OnClicked = ((GUIButton b, object o) => this.SellItems());
				GUIButton guibutton = confirmDialog.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(confirmDialog.Close));
				confirmDialog.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(confirmDialog.Close);
				return true;
			};
		}

		// Token: 0x06001693 RID: 5779 RVA: 0x000D5A28 File Offset: 0x000D3C28
		private void SetConfirmButtonStatus()
		{
			GUIButton guibutton = this.confirmButton;
			bool flag = this.ActiveStore != null && this.HasActiveTabPermissions() && this.ActiveShoppingCrateList.Content.RectTransform.Children.Any<RectTransform>();
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3;
				switch (this.activeTab)
				{
				case Store.StoreTab.Buy:
					flag3 = (this.Balance >= this.buyTotal);
					break;
				case Store.StoreTab.Sell:
					flag3 = (this.CurrentLocation != null && this.sellTotal <= this.ActiveStore.Balance);
					break;
				case Store.StoreTab.SellSub:
					flag3 = (this.CurrentLocation != null && this.sellFromSubTotal <= this.ActiveStore.Balance);
					break;
				default:
					flag3 = false;
					break;
				}
				flag2 = flag3;
			}
			guibutton.Enabled = flag2;
			this.confirmButton.Visible = (this.ActiveStore != null);
		}

		// Token: 0x06001694 RID: 5780 RVA: 0x000D5B07 File Offset: 0x000D3D07
		private void SetClearAllButtonStatus()
		{
			this.clearAllButton.Enabled = (this.HasActiveTabPermissions() && this.ActiveShoppingCrateList.Content.RectTransform.Children.Any<RectTransform>());
		}

		// Token: 0x06001695 RID: 5781 RVA: 0x000D5B3C File Offset: 0x000D3D3C
		public void Update(float deltaTime)
		{
			this.updateStopwatch.Restart();
			if (GameMain.DevMode && PlayerInput.KeyDown(Keys.D0))
			{
				this.CreateUI();
				this.needsRefresh = true;
			}
			if (GameMain.GraphicsWidth != this.resolutionWhenCreated.X || GameMain.GraphicsHeight != this.resolutionWhenCreated.Y)
			{
				this.CreateUI();
				this.needsRefresh = true;
			}
			else
			{
				this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
				this.ownedItemsUpdateTimer += deltaTime;
				if (this.ownedItemsUpdateTimer >= 1.5f)
				{
					bool checkForRefresh = !this.needsItemsToSellRefresh || !this.needsRefresh;
					Dictionary<ItemPrefab, Store.ItemQuantity> prevOwnedItems = checkForRefresh ? new Dictionary<ItemPrefab, Store.ItemQuantity>(this.OwnedItems) : null;
					this.UpdateOwnedItems();
					if (checkForRefresh)
					{
						bool flag;
						if (this.OwnedItems.Count == prevOwnedItems.Count)
						{
							if (this.OwnedItems.Values.Sum((Store.ItemQuantity v) => v.Total) == prevOwnedItems.Values.Sum((Store.ItemQuantity v) => v.Total) && !this.OwnedItems.Any(delegate(KeyValuePair<ItemPrefab, Store.ItemQuantity> kvp)
							{
								Store.ItemQuantity v;
								return !prevOwnedItems.TryGetValue(kvp.Key, out v) || kvp.Value.Total != v.Total;
							}))
							{
								flag = prevOwnedItems.Any((KeyValuePair<ItemPrefab, Store.ItemQuantity> kvp) => !this.OwnedItems.ContainsKey(kvp.Key));
								goto IL_179;
							}
						}
						flag = true;
						IL_179:
						bool refresh = flag;
						if (refresh)
						{
							this.needsItemsToSellRefresh = true;
							this.needsRefresh = true;
						}
					}
				}
				this.sellableItemsFromSubUpdateTimer += deltaTime;
				if (this.sellableItemsFromSubUpdateTimer >= 1.5f)
				{
					bool checkForRefresh2 = !this.needsRefresh;
					List<PurchasedItem> prevSubItems = checkForRefresh2 ? new List<PurchasedItem>(this.itemsToSellFromSub) : null;
					this.RefreshItemsToSellFromSub();
					if (checkForRefresh2)
					{
						bool flag2;
						if (this.itemsToSellFromSub.Count == prevSubItems.Count)
						{
							if (this.itemsToSellFromSub.Sum((PurchasedItem i) => i.Quantity) == prevSubItems.Sum((PurchasedItem i) => i.Quantity) && !this.itemsToSellFromSub.Any(delegate(PurchasedItem i)
							{
								PurchasedItem prev2 = prevSubItems.FirstOrDefault((PurchasedItem prev) => prev.ItemPrefab == i.ItemPrefab);
								return prev2 == null || i.Quantity != prev2.Quantity;
							}))
							{
								flag2 = prevSubItems.Any((PurchasedItem prev) => this.itemsToSellFromSub.None((PurchasedItem i) => i.ItemPrefab == prev.ItemPrefab));
								goto IL_284;
							}
						}
						flag2 = true;
						IL_284:
						this.needsRefresh = flag2;
					}
				}
			}
			if (this.activeTab == Store.StoreTab.Buy)
			{
				int currBalance = this.Balance;
				if (this.prevBalance != currBalance)
				{
					this.needsBuyingRefresh = true;
					this.prevBalance = currBalance;
				}
			}
			if (this.ActiveStore != null)
			{
				if (this.needsItemsToSellRefresh)
				{
					this.RefreshItemsToSell();
				}
				if (this.needsItemsToSellFromSubRefresh)
				{
					this.RefreshItemsToSellFromSub();
				}
				if (this.needsRefresh)
				{
					this.Refresh(this.ownedItemsUpdateTimer > 0f);
				}
				if (this.needsBuyingRefresh || this.HavePermissionsChanged(Store.StoreTab.Buy))
				{
					this.RefreshBuying(this.ownedItemsUpdateTimer > 0f);
				}
				if (this.needsSellingRefresh || this.HavePermissionsChanged(Store.StoreTab.Sell))
				{
					this.RefreshSelling(this.ownedItemsUpdateTimer > 0f);
				}
				if (this.needsSellingFromSubRefresh || this.HavePermissionsChanged(Store.StoreTab.SellSub))
				{
					this.RefreshSellingFromSub(this.ownedItemsUpdateTimer > 0f, this.sellableItemsFromSubUpdateTimer > 0f);
				}
			}
			this.updateStopwatch.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:GameSession:Store", this.updateStopwatch.ElapsedTicks);
		}

		// Token: 0x060016A0 RID: 5792 RVA: 0x000D6028 File Offset: 0x000D4228
		[CompilerGenerated]
		private bool <CreateUI>g__OnClickedCategoryButton|85_3(GUIButton button, object userData)
		{
			MapEntityCategory? newCategory = (!button.Selected) ? ((MapEntityCategory?)userData) : null;
			if (newCategory != null)
			{
				this.searchBox.Text = "";
			}
			MapEntityCategory? mapEntityCategory = newCategory;
			MapEntityCategory? mapEntityCategory2 = this.selectedItemCategory;
			if (!(mapEntityCategory.GetValueOrDefault() == mapEntityCategory2.GetValueOrDefault() & mapEntityCategory != null == (mapEntityCategory2 != null)))
			{
				this.tabLists[this.activeTab].ScrollBar.BarScroll = 0f;
			}
			this.FilterStoreItems(newCategory, this.searchBox.Text);
			return true;
		}

		// Token: 0x060016A6 RID: 5798 RVA: 0x000D61B4 File Offset: 0x000D43B4
		[CompilerGenerated]
		private void <RefreshStoreBuyList>g__CreateOrUpdateItemFrame|98_1(ItemPrefab itemPrefab, int quantity, ref Store.<>c__DisplayClass98_0 A_3)
		{
			PriceInfo priceInfo;
			if (itemPrefab.CanBeBoughtFrom(this.ActiveStore, out priceInfo) && itemPrefab.CanCharacterBuy())
			{
				bool isDailySpecial = this.ActiveStore.DailySpecials.Contains(itemPrefab);
				GUIComponent itemFrame = isDailySpecial ? this.storeDailySpecialsGroup.FindChild(delegate(GUIComponent c)
				{
					PurchasedItem pi = c.UserData as PurchasedItem;
					return pi != null && pi.ItemPrefab == itemPrefab;
				}, false) : this.storeBuyList.Content.FindChild(delegate(GUIComponent c)
				{
					PurchasedItem pi = c.UserData as PurchasedItem;
					return pi != null && pi.ItemPrefab == itemPrefab;
				}, false);
				quantity = Math.Max(quantity - this.CargoManager.GetPurchasedItemCount(this.ActiveStore, itemPrefab), 0);
				PurchasedItem buyCrateItem = this.CargoManager.GetBuyCrateItem(this.ActiveStore, itemPrefab);
				if (buyCrateItem != null)
				{
					quantity = Math.Max(quantity - buyCrateItem.Quantity, 0);
				}
				if (itemFrame == null)
				{
					GUIComponent parentComponent = isDailySpecial ? this.storeDailySpecialsGroup : this.storeBuyList;
					itemFrame = this.CreateItemFrame(new PurchasedItem(itemPrefab, quantity), parentComponent, Store.StoreTab.Buy, !A_3.hasPermissions);
				}
				else
				{
					(itemFrame.UserData as PurchasedItem).Quantity = quantity;
					Store.SetQuantityLabelText(Store.StoreTab.Buy, itemFrame);
					this.SetOwnedText(itemFrame, null);
					this.SetPriceGetters(itemFrame, true);
				}
				this.SetItemFrameStatus(itemFrame, A_3.hasPermissions && quantity > 0 && Store.ReputationRequirementsMet(priceInfo, this.ActiveStore.GetMerchantOrLocationFactionIdentifier()));
				A_3.existingItemFrames.Add(itemFrame);
			}
		}

		// Token: 0x060016A7 RID: 5799 RVA: 0x000D632C File Offset: 0x000D452C
		[CompilerGenerated]
		private void <RefreshStoreSellList>g__CreateOrUpdateItemFrame|99_0(ItemPrefab itemPrefab, int itemQuantity, ref Store.<>c__DisplayClass99_0 A_3)
		{
			if (itemPrefab.GetPriceInfo(this.ActiveStore) == null)
			{
				return;
			}
			bool isRequestedGood = this.ActiveStore.RequestedGoods.Contains(itemPrefab);
			GUIComponent itemFrame = isRequestedGood ? this.storeRequestedGoodGroup.FindChild(delegate(GUIComponent c)
			{
				PurchasedItem pi = c.UserData as PurchasedItem;
				return pi != null && pi.ItemPrefab == itemPrefab;
			}, false) : this.storeSellList.Content.FindChild(delegate(GUIComponent c)
			{
				PurchasedItem pi = c.UserData as PurchasedItem;
				return pi != null && pi.ItemPrefab == itemPrefab;
			}, false);
			PurchasedItem sellCrateItem = this.CargoManager.GetSellCrateItem(this.ActiveStore, itemPrefab);
			if (sellCrateItem != null)
			{
				itemQuantity = Math.Max(itemQuantity - sellCrateItem.Quantity, 0);
			}
			if (itemFrame == null)
			{
				GUIComponent parentComponent = isRequestedGood ? this.storeRequestedGoodGroup : this.storeSellList;
				itemFrame = this.CreateItemFrame(new PurchasedItem(itemPrefab, itemQuantity), parentComponent, Store.StoreTab.Sell, !A_3.hasPermissions);
			}
			else
			{
				(itemFrame.UserData as PurchasedItem).Quantity = itemQuantity;
				Store.SetQuantityLabelText(Store.StoreTab.Sell, itemFrame);
				this.SetOwnedText(itemFrame, null);
				this.SetPriceGetters(itemFrame, false);
			}
			this.SetItemFrameStatus(itemFrame, A_3.hasPermissions && itemQuantity > 0);
			if (itemQuantity < 1 && !isRequestedGood)
			{
				itemFrame.Visible = false;
			}
			A_3.existingItemFrames.Add(itemFrame);
		}

		// Token: 0x060016A8 RID: 5800 RVA: 0x000D646C File Offset: 0x000D466C
		[CompilerGenerated]
		private void <RefreshStoreSellFromSubList>g__CreateOrUpdateItemFrame|100_0(ItemPrefab itemPrefab, int itemQuantity, ref Store.<>c__DisplayClass100_0 A_3)
		{
			if (itemPrefab.GetPriceInfo(this.ActiveStore) == null)
			{
				return;
			}
			bool isRequestedGood = this.ActiveStore.RequestedGoods.Contains(itemPrefab);
			GUIComponent itemFrame = isRequestedGood ? this.storeRequestedSubGoodGroup.FindChild(delegate(GUIComponent c)
			{
				PurchasedItem pi = c.UserData as PurchasedItem;
				return pi != null && pi.ItemPrefab == itemPrefab;
			}, false) : this.storeSellFromSubList.Content.FindChild(delegate(GUIComponent c)
			{
				PurchasedItem pi = c.UserData as PurchasedItem;
				return pi != null && pi.ItemPrefab == itemPrefab;
			}, false);
			PurchasedItem subCrateItem = this.CargoManager.GetSubCrateItem(this.ActiveStore, itemPrefab);
			if (subCrateItem != null)
			{
				itemQuantity = Math.Max(itemQuantity - subCrateItem.Quantity, 0);
			}
			if (itemFrame == null)
			{
				GUIComponent parentComponent = isRequestedGood ? this.storeRequestedSubGoodGroup : this.storeSellFromSubList;
				itemFrame = this.CreateItemFrame(new PurchasedItem(itemPrefab, itemQuantity), parentComponent, Store.StoreTab.SellSub, !A_3.hasPermissions);
			}
			else
			{
				(itemFrame.UserData as PurchasedItem).Quantity = itemQuantity;
				Store.SetQuantityLabelText(Store.StoreTab.SellSub, itemFrame);
				this.SetOwnedText(itemFrame, null);
				this.SetPriceGetters(itemFrame, false);
			}
			this.SetItemFrameStatus(itemFrame, A_3.hasPermissions && itemQuantity > 0);
			if (itemQuantity < 1 && !isRequestedGood)
			{
				itemFrame.Visible = false;
			}
			A_3.existingItemFrames.Add(itemFrame);
		}

		// Token: 0x060016A9 RID: 5801 RVA: 0x000D65AC File Offset: 0x000D47AC
		[CompilerGenerated]
		internal static int <SortItems>g__CompareByElement|108_2(RectTransform x, RectTransform y)
		{
			if (Store.<SortItems>g__ShouldBeOnTop|108_7(x) || Store.<SortItems>g__ShouldBeOnBottom|108_8(y))
			{
				return -1;
			}
			if (Store.<SortItems>g__ShouldBeOnBottom|108_8(x) || Store.<SortItems>g__ShouldBeOnTop|108_7(y))
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x060016AA RID: 5802 RVA: 0x000D65D4 File Offset: 0x000D47D4
		[CompilerGenerated]
		internal static bool <SortItems>g__ShouldBeOnTop|108_7(RectTransform rt)
		{
			string id = rt.GUIComponent.UserData as string;
			return id != null && (id == "deals" || id == "header");
		}

		// Token: 0x060016AB RID: 5803 RVA: 0x000D6614 File Offset: 0x000D4814
		[CompilerGenerated]
		internal static bool <SortItems>g__ShouldBeOnBottom|108_8(RectTransform rt)
		{
			string id = rt.GUIComponent.UserData as string;
			return id != null && id == "divider";
		}

		// Token: 0x060016AD RID: 5805 RVA: 0x000D664B File Offset: 0x000D484B
		[CompilerGenerated]
		internal static bool <UpdateOwnedItems>g__ItemAndAllContainersInteractable|113_2(Item item)
		{
			while (item.IsPlayerTeamInteractable)
			{
				item = item.Container;
				if (item == null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060016AE RID: 5806 RVA: 0x000D6664 File Offset: 0x000D4864
		[CompilerGenerated]
		private void <UpdateOwnedItems>g__AddOwnedItem|113_3(Item item)
		{
			PriceInfo priceInfo = (item != null) ? item.Prefab.GetPriceInfo(this.ActiveStore) : null;
			if (priceInfo == null)
			{
				return;
			}
			bool isNonEmpty = !priceInfo.DisplayNonEmpty || item.ConditionPercentage > 5f;
			Store.ItemQuantity itemQuantity;
			if (this.OwnedItems.TryGetValue(item.Prefab, out itemQuantity))
			{
				this.OwnedItems[item.Prefab].Add(1, isNonEmpty);
				return;
			}
			this.OwnedItems.Add(item.Prefab, new Store.ItemQuantity(1, isNonEmpty));
		}

		// Token: 0x060016AF RID: 5807 RVA: 0x000D66EC File Offset: 0x000D48EC
		[CompilerGenerated]
		private void <UpdateOwnedItems>g__AddNonEmptyOwnedItems|113_4(PurchasedItem purchasedItem)
		{
			if (purchasedItem == null)
			{
				return;
			}
			Store.ItemQuantity itemQuantity;
			if (this.OwnedItems.TryGetValue(purchasedItem.ItemPrefab, out itemQuantity))
			{
				this.OwnedItems[purchasedItem.ItemPrefab].Add(purchasedItem.Quantity, true);
				return;
			}
			this.OwnedItems.Add(purchasedItem.ItemPrefab, new Store.ItemQuantity(purchasedItem.Quantity, true));
		}

		// Token: 0x04000B30 RID: 2864
		private readonly CampaignUI campaignUI;

		// Token: 0x04000B31 RID: 2865
		private readonly GUIComponent parentComponent;

		// Token: 0x04000B32 RID: 2866
		private readonly List<GUIButton> storeTabButtons = new List<GUIButton>();

		// Token: 0x04000B33 RID: 2867
		private readonly List<GUIButton> itemCategoryButtons = new List<GUIButton>();

		// Token: 0x04000B34 RID: 2868
		private readonly Dictionary<Store.StoreTab, GUIListBox> tabLists = new Dictionary<Store.StoreTab, GUIListBox>();

		// Token: 0x04000B35 RID: 2869
		private readonly Dictionary<Store.StoreTab, Store.SortingMethod> tabSortingMethods = new Dictionary<Store.StoreTab, Store.SortingMethod>();

		// Token: 0x04000B36 RID: 2870
		private readonly List<PurchasedItem> itemsToSell = new List<PurchasedItem>();

		// Token: 0x04000B37 RID: 2871
		private readonly List<PurchasedItem> itemsToSellFromSub = new List<PurchasedItem>();

		// Token: 0x04000B38 RID: 2872
		private GUIMessageBox deliveryPrompt;

		// Token: 0x04000B39 RID: 2873
		private Store.StoreTab activeTab;

		// Token: 0x04000B3A RID: 2874
		private MapEntityCategory? selectedItemCategory;

		// Token: 0x04000B3B RID: 2875
		private bool suppressBuySell;

		// Token: 0x04000B3C RID: 2876
		private int buyTotal;

		// Token: 0x04000B3D RID: 2877
		private int sellTotal;

		// Token: 0x04000B3E RID: 2878
		private int sellFromSubTotal;

		// Token: 0x04000B3F RID: 2879
		private GUITextBlock storeNameBlock;

		// Token: 0x04000B40 RID: 2880
		private GUITextBlock reputationEffectBlock;

		// Token: 0x04000B41 RID: 2881
		private GUIDropDown sortingDropDown;

		// Token: 0x04000B42 RID: 2882
		private GUITextBox searchBox;

		// Token: 0x04000B43 RID: 2883
		private GUILayoutGroup categoryButtonContainer;

		// Token: 0x04000B44 RID: 2884
		private GUIListBox storeBuyList;

		// Token: 0x04000B45 RID: 2885
		private GUIListBox storeSellList;

		// Token: 0x04000B46 RID: 2886
		private GUIListBox storeSellFromSubList;

		// Token: 0x04000B47 RID: 2887
		private GUILayoutGroup storeDailySpecialsGroup;

		// Token: 0x04000B48 RID: 2888
		private GUILayoutGroup storeRequestedGoodGroup;

		// Token: 0x04000B49 RID: 2889
		private GUILayoutGroup storeRequestedSubGoodGroup;

		// Token: 0x04000B4A RID: 2890
		private Color storeSpecialColor;

		// Token: 0x04000B4B RID: 2891
		private GUIListBox shoppingCrateBuyList;

		// Token: 0x04000B4C RID: 2892
		private GUIListBox shoppingCrateSellList;

		// Token: 0x04000B4D RID: 2893
		private GUIListBox shoppingCrateSellFromSubList;

		// Token: 0x04000B4E RID: 2894
		private GUITextBlock relevantBalanceName;

		// Token: 0x04000B4F RID: 2895
		private GUITextBlock shoppingCrateTotal;

		// Token: 0x04000B50 RID: 2896
		private GUIButton clearAllButton;

		// Token: 0x04000B51 RID: 2897
		private GUIButton confirmButton;

		// Token: 0x04000B52 RID: 2898
		private bool needsRefresh;

		// Token: 0x04000B53 RID: 2899
		private bool needsBuyingRefresh;

		// Token: 0x04000B54 RID: 2900
		private bool needsSellingRefresh;

		// Token: 0x04000B55 RID: 2901
		private bool needsItemsToSellRefresh;

		// Token: 0x04000B56 RID: 2902
		private bool needsSellingFromSubRefresh;

		// Token: 0x04000B57 RID: 2903
		private bool needsItemsToSellFromSubRefresh;

		// Token: 0x04000B58 RID: 2904
		private Point resolutionWhenCreated;

		// Token: 0x04000B59 RID: 2905
		private CampaignUI.PlayerBalanceElement? playerBalanceElement;

		// Token: 0x04000B5C RID: 2908
		private bool hadBuyPermissions;

		// Token: 0x04000B5D RID: 2909
		private bool hadSellInventoryPermissions;

		// Token: 0x04000B5E RID: 2910
		private bool hadSellSubPermissions;

		// Token: 0x04000B5F RID: 2911
		private int prevDailySpecialCount;

		// Token: 0x04000B60 RID: 2912
		private int prevRequestedGoodsCount;

		// Token: 0x04000B61 RID: 2913
		private int prevSubRequestedGoodsCount;

		// Token: 0x04000B62 RID: 2914
		private int prevBalance;

		// Token: 0x04000B63 RID: 2915
		private float ownedItemsUpdateTimer;

		// Token: 0x04000B64 RID: 2916
		private float sellableItemsFromSubUpdateTimer;

		// Token: 0x04000B65 RID: 2917
		private const float timerUpdateInterval = 1.5f;

		// Token: 0x04000B66 RID: 2918
		private readonly Stopwatch updateStopwatch = new Stopwatch();

		// Token: 0x020009C4 RID: 2500
		private class ItemQuantity
		{
			// Token: 0x17001A66 RID: 6758
			// (get) Token: 0x060072DE RID: 29406 RVA: 0x0036DDEC File Offset: 0x0036BFEC
			// (set) Token: 0x060072DF RID: 29407 RVA: 0x0036DDF4 File Offset: 0x0036BFF4
			public int Total { get; private set; }

			// Token: 0x17001A67 RID: 6759
			// (get) Token: 0x060072E0 RID: 29408 RVA: 0x0036DDFD File Offset: 0x0036BFFD
			// (set) Token: 0x060072E1 RID: 29409 RVA: 0x0036DE05 File Offset: 0x0036C005
			public int NonEmpty { get; private set; }

			// Token: 0x17001A68 RID: 6760
			// (get) Token: 0x060072E2 RID: 29410 RVA: 0x0036DE0E File Offset: 0x0036C00E
			public bool AllNonEmpty
			{
				get
				{
					return this.NonEmpty == this.Total;
				}
			}

			// Token: 0x060072E3 RID: 29411 RVA: 0x0036DE1E File Offset: 0x0036C01E
			public ItemQuantity(int total, bool areNonEmpty = true)
			{
				this.Total = total;
				this.NonEmpty = (areNonEmpty ? total : 0);
			}

			// Token: 0x060072E4 RID: 29412 RVA: 0x0036DE3A File Offset: 0x0036C03A
			public void Add(int amount, bool areNonEmpty)
			{
				this.Total += amount;
				if (areNonEmpty)
				{
					this.NonEmpty += amount;
				}
			}
		}

		// Token: 0x020009C5 RID: 2501
		public enum StoreTab
		{
			// Token: 0x04004240 RID: 16960
			Buy,
			// Token: 0x04004241 RID: 16961
			Sell,
			// Token: 0x04004242 RID: 16962
			SellSub
		}

		// Token: 0x020009C6 RID: 2502
		private enum SortingMethod
		{
			// Token: 0x04004244 RID: 16964
			AlphabeticalAsc,
			// Token: 0x04004245 RID: 16965
			AlphabeticalDesc,
			// Token: 0x04004246 RID: 16966
			PriceAsc,
			// Token: 0x04004247 RID: 16967
			PriceDesc,
			// Token: 0x04004248 RID: 16968
			CategoryAsc
		}
	}
}
