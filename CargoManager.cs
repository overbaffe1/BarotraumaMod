using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000063 RID: 99
	internal class CargoManager
	{
		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x06000D8D RID: 3469 RVA: 0x0007D321 File Offset: 0x0007B521
		private List<CargoManager.SoldEntity> SoldEntities { get; } = new List<CargoManager.SoldEntity>();

		// Token: 0x06000D8E RID: 3470 RVA: 0x0007D32C File Offset: 0x0007B52C
		public IEnumerable<Item> GetSellableItems(Character character)
		{
			if (character == null)
			{
				return new List<Item>();
			}
			IEnumerable<CargoManager.SoldEntity> confirmedSoldEntities = this.GetConfirmedSoldEntities();
			return character.Inventory.FindAllItems(delegate(Item item)
			{
				if (!this.IsItemSellable(item, confirmedSoldEntities))
				{
					return false;
				}
				if (!item.AllowedSlots.All((InvSlotType s) => CargoManager.equipmentSlots.Contains(s)) && base.<GetSellableItems>g__IsInEquipmentSlot|1(item))
				{
					return false;
				}
				Item rootContainer = item.RootContainer;
				return rootContainer == null || !base.<GetSellableItems>g__IsInEquipmentSlot|1(rootContainer);
			}, true, null).Distinct<Item>();
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x0007D38A File Offset: 0x0007B58A
		private IEnumerable<CargoManager.SoldEntity> GetConfirmedSoldEntities()
		{
			return from se in this.SoldEntities
			where se.Status != CargoManager.SoldEntity.SellStatus.Unconfirmed
			select se;
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x0007D3B8 File Offset: 0x0007B5B8
		public void SetItemsInBuyCrate(Dictionary<Identifier, List<PurchasedItem>> items)
		{
			this.ItemsInBuyCrate.Clear();
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> entry in items)
			{
				this.ItemsInBuyCrate.Add(entry.Key, entry.Value);
			}
			NamedEvent<CargoManager> onItemsInBuyCrateChanged = this.OnItemsInBuyCrateChanged;
			if (onItemsInBuyCrateChanged == null)
			{
				return;
			}
			onItemsInBuyCrateChanged.Invoke(this);
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x0007D434 File Offset: 0x0007B634
		public void SetItemsInSubSellCrate(Dictionary<Identifier, List<PurchasedItem>> items)
		{
			this.ItemsInSellFromSubCrate.Clear();
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> entry in items)
			{
				this.ItemsInSellFromSubCrate.Add(entry.Key, entry.Value);
			}
			NamedEvent<CargoManager> onItemsInSellFromSubCrateChanged = this.OnItemsInSellFromSubCrateChanged;
			if (onItemsInSellFromSubCrateChanged == null)
			{
				return;
			}
			onItemsInSellFromSubCrateChanged.Invoke(this);
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0007D4B0 File Offset: 0x0007B6B0
		public void SetSoldItems(Dictionary<Identifier, List<SoldItem>> items)
		{
			if (this.SoldItems.Count == 0 && items.Count == 0)
			{
				return;
			}
			this.SoldItems.Clear();
			foreach (KeyValuePair<Identifier, List<SoldItem>> entry in items)
			{
				this.SoldItems.Add(entry.Key, entry.Value);
			}
			using (List<CargoManager.SoldEntity>.Enumerator enumerator2 = this.SoldEntities.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CargoManager.SoldEntity se = enumerator2.Current;
					if (se.Status != CargoManager.SoldEntity.SellStatus.Confirmed)
					{
						Func<SoldItem, bool> <>9__2;
						if (this.SoldItems.Any(delegate(KeyValuePair<Identifier, List<SoldItem>> si)
						{
							IEnumerable<SoldItem> value = si.Value;
							Func<SoldItem, bool> predicate;
							if ((predicate = <>9__2) == null)
							{
								predicate = (<>9__2 = ((SoldItem si) => CargoManager.<SetSoldItems>g__Match|8_0(si, se, true)));
							}
							return value.Any(predicate);
						}))
						{
							se.Status = CargoManager.SoldEntity.SellStatus.Confirmed;
						}
						else
						{
							se.Status = CargoManager.SoldEntity.SellStatus.Unconfirmed;
						}
					}
				}
			}
			foreach (List<SoldItem> soldItems in this.SoldItems.Values)
			{
				using (List<SoldItem>.Enumerator enumerator4 = soldItems.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						SoldItem si = enumerator4.Current;
						if (si.Origin == SoldItem.SellOrigin.Submarine)
						{
							CargoManager.SoldEntity soldEntityMatch = this.SoldEntities.FirstOrDefault((CargoManager.SoldEntity se) => se.Item == null && CargoManager.<SetSoldItems>g__Match|8_0(si, se, false));
							if (soldEntityMatch != null)
							{
								Item item = Entity.FindEntityByID(si.ID) as Item;
								if (item != null)
								{
									soldEntityMatch.SetItem(item);
									soldEntityMatch.Status = CargoManager.SoldEntity.SellStatus.Confirmed;
								}
							}
						}
					}
				}
			}
			NamedEvent<CargoManager> onSoldItemsChanged = this.OnSoldItemsChanged;
			if (onSoldItemsChanged == null)
			{
				return;
			}
			onSoldItemsChanged.Invoke(this);
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x0007D6B0 File Offset: 0x0007B8B0
		public void ModifyItemQuantityInSellCrate(Identifier storeIdentifier, ItemPrefab itemPrefab, int changeInQuantity)
		{
			PurchasedItem item = this.GetSellCrateItem(storeIdentifier, itemPrefab);
			if (item != null)
			{
				item.Quantity += changeInQuantity;
				if (item.Quantity < 1)
				{
					List<PurchasedItem> sellCrateItems = this.GetSellCrateItems(storeIdentifier, false);
					if (sellCrateItems != null)
					{
						sellCrateItems.Remove(item);
					}
				}
			}
			else if (changeInQuantity > 0)
			{
				this.GetSellCrateItems(storeIdentifier, true).Add(new PurchasedItem(itemPrefab, changeInQuantity));
			}
			NamedEvent<CargoManager> onItemsInSellCrateChanged = this.OnItemsInSellCrateChanged;
			if (onItemsInSellCrateChanged == null)
			{
				return;
			}
			onItemsInSellCrateChanged.Invoke(this);
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x0007D720 File Offset: 0x0007B920
		public void SellItems(Identifier storeIdentifier, List<PurchasedItem> itemsToSell, Store.StoreTab sellingMode)
		{
			IEnumerable<Item> sellableItems;
			try
			{
				IEnumerable<Item> enumerable;
				if (sellingMode != Store.StoreTab.Sell)
				{
					if (sellingMode != Store.StoreTab.SellSub)
					{
						throw new NotImplementedException();
					}
					enumerable = this.GetSellableItemsFromSub();
				}
				else
				{
					enumerable = this.GetSellableItems(Character.Controlled);
				}
				sellableItems = enumerable;
			}
			catch (NotImplementedException e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error selling items: unknown store tab type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Store.StoreTab>(sellingMode);
				defaultInterpolatedStringHandler.AppendLiteral("\".\n");
				defaultInterpolatedStringHandler.AppendFormatted(e.StackTrace.CleanupStackTrace());
				DebugConsole.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
				return;
			}
			bool canAddToRemoveQueue = this.campaign.IsSinglePlayer && Entity.Spawner != null;
			GameClient client = GameMain.Client;
			byte sellerId = (client != null) ? client.SessionId : 0;
			Dictionary<ItemPrefab, int> sellValues = this.GetSellValuesAtCurrentLocation(storeIdentifier, from i in itemsToSell
			select i.ItemPrefab);
			Location.StoreInfo store = this.Location.GetStore(storeIdentifier);
			if (store == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(61, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Error selling items at ");
				defaultInterpolatedStringHandler2.AppendFormatted<Location>(this.Location);
				defaultInterpolatedStringHandler2.AppendLiteral(": no store with identifier \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(storeIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" exists.\n");
				defaultInterpolatedStringHandler2.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.LogError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null);
				return;
			}
			List<SoldItem> storeSpecificSoldItems = this.GetSoldItems(storeIdentifier, true);
			using (List<PurchasedItem>.Enumerator enumerator = itemsToSell.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PurchasedItem item = enumerator.Current;
					int itemValue = item.Quantity * sellValues[item.ItemPrefab];
					if (store.Balance >= itemValue)
					{
						IEnumerable<Item> matchingItems = sellableItems.Where(delegate(Item i)
						{
							Prefab prefab = i.Prefab;
							Identifier itemPrefabIdentifier = item.ItemPrefabIdentifier;
							return prefab.Identifier == itemPrefabIdentifier;
						});
						int count = Math.Min(item.Quantity, matchingItems.Count<Item>());
						SoldItem.SellOrigin origin = (sellingMode == Store.StoreTab.Sell) ? SoldItem.SellOrigin.Character : SoldItem.SellOrigin.Submarine;
						if (origin == SoldItem.SellOrigin.Character || GameMain.IsSingleplayer)
						{
							for (int k = 0; k < count; k++)
							{
								Item matchingItem = matchingItems.ElementAt(k);
								storeSpecificSoldItems.Add(new SoldItem(matchingItem.Prefab, matchingItem.ID, canAddToRemoveQueue, sellerId, origin));
								this.SoldEntities.Add(new CargoManager.SoldEntity(matchingItem, this.campaign.IsSinglePlayer ? CargoManager.SoldEntity.SellStatus.Confirmed : CargoManager.SoldEntity.SellStatus.Local));
								if (canAddToRemoveQueue)
								{
									Entity.Spawner.AddItemToRemoveQueue(matchingItem);
								}
							}
						}
						else
						{
							for (int j = 0; j < count; j++)
							{
								storeSpecificSoldItems.Add(new SoldItem(item.ItemPrefab, 0, canAddToRemoveQueue, sellerId, origin));
								this.SoldEntities.Add(new CargoManager.SoldEntity(item.ItemPrefab, CargoManager.SoldEntity.SellStatus.Local));
							}
						}
						store.Balance -= itemValue;
						if (GameMain.IsSingleplayer)
						{
							this.campaign.Bank.Give(itemValue);
						}
						GameAnalyticsManager.AddMoneyGainedEvent(itemValue, GameAnalyticsManager.MoneySource.Store, item.ItemPrefab.Identifier.Value);
						List<PurchasedItem> sellCrate = (sellingMode == Store.StoreTab.Sell) ? this.GetSellCrateItems(storeIdentifier, false) : this.GetSubCrateItems(storeIdentifier, false);
						PurchasedItem itemToSell = (sellCrate != null) ? sellCrate.Find((PurchasedItem pi) => pi.ItemPrefab == item.ItemPrefab) : null;
						if (itemToSell != null)
						{
							itemToSell.Quantity -= item.Quantity;
							if (itemToSell.Quantity < 1)
							{
								sellCrate.Remove(itemToSell);
							}
						}
					}
				}
			}
			NamedEvent<CargoManager> onSoldItemsChanged = this.OnSoldItemsChanged;
			if (onSoldItemsChanged == null)
			{
				return;
			}
			onSoldItemsChanged.Invoke(this);
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x0007DAF4 File Offset: 0x0007BCF4
		public void ClearSoldItemsProjSpecific()
		{
			this.SoldItems.Clear();
			this.SoldEntities.Clear();
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x0007DB0C File Offset: 0x0007BD0C
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInBuyCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x06000D97 RID: 3479 RVA: 0x0007DB14 File Offset: 0x0007BD14
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInSellCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x06000D98 RID: 3480 RVA: 0x0007DB1C File Offset: 0x0007BD1C
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInSellFromSubCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x06000D99 RID: 3481 RVA: 0x0007DB24 File Offset: 0x0007BD24
		public Dictionary<Identifier, List<PurchasedItem>> PurchasedItems { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x0007DB2C File Offset: 0x0007BD2C
		public Dictionary<Identifier, List<SoldItem>> SoldItems { get; } = new Dictionary<Identifier, List<SoldItem>>();

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x06000D9B RID: 3483 RVA: 0x0007DB34 File Offset: 0x0007BD34
		private Location Location
		{
			get
			{
				CampaignMode campaignMode = this.campaign;
				if (campaignMode == null)
				{
					return null;
				}
				Map map = campaignMode.Map;
				if (map == null)
				{
					return null;
				}
				return map.CurrentLocation;
			}
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0007DB54 File Offset: 0x0007BD54
		public CargoManager(CampaignMode campaign)
		{
			this.campaign = campaign;
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0007DC00 File Offset: 0x0007BE00
		public static bool HasUnlockedStoreItem(ItemPrefab prefab)
		{
			foreach (Character character in GameSession.GetSessionCrewCharacters(CharacterType.Both))
			{
				if (character.HasStoreAccessForItem(prefab))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x0007DC5C File Offset: 0x0007BE5C
		private List<T> GetItems<T>(Identifier identifier, Dictionary<Identifier, List<T>> items, bool create = false)
		{
			List<T> storeSpecificItems;
			if (items.TryGetValue(identifier, out storeSpecificItems) && storeSpecificItems != null)
			{
				return storeSpecificItems;
			}
			if (create)
			{
				storeSpecificItems = new List<T>();
				items.Add(identifier, storeSpecificItems);
				return storeSpecificItems;
			}
			return new List<T>();
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x0007DC91 File Offset: 0x0007BE91
		public List<PurchasedItem> GetBuyCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInBuyCrate, create);
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x0007DCA1 File Offset: 0x0007BEA1
		public List<PurchasedItem> GetBuyCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetBuyCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x0007DCBC File Offset: 0x0007BEBC
		public PurchasedItem GetBuyCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> buyCrateItems = this.GetBuyCrateItems(identifier, false);
			if (buyCrateItems == null)
			{
				return null;
			}
			return buyCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x0007DCF5 File Offset: 0x0007BEF5
		public PurchasedItem GetBuyCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetBuyCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000DA3 RID: 3491 RVA: 0x0007DD0E File Offset: 0x0007BF0E
		public List<PurchasedItem> GetSellCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInSellCrate, create);
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x0007DD1E File Offset: 0x0007BF1E
		public List<PurchasedItem> GetSellCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSellCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x0007DD38 File Offset: 0x0007BF38
		public PurchasedItem GetSellCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> sellCrateItems = this.GetSellCrateItems(identifier, false);
			if (sellCrateItems == null)
			{
				return null;
			}
			return sellCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x06000DA6 RID: 3494 RVA: 0x0007DD71 File Offset: 0x0007BF71
		public PurchasedItem GetSellCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetSellCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000DA7 RID: 3495 RVA: 0x0007DD8A File Offset: 0x0007BF8A
		public List<PurchasedItem> GetSubCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInSellFromSubCrate, create);
		}

		// Token: 0x06000DA8 RID: 3496 RVA: 0x0007DD9A File Offset: 0x0007BF9A
		public List<PurchasedItem> GetSubCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSubCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x0007DDB4 File Offset: 0x0007BFB4
		public PurchasedItem GetSubCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> subCrateItems = this.GetSubCrateItems(identifier, false);
			if (subCrateItems == null)
			{
				return null;
			}
			return subCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x0007DDED File Offset: 0x0007BFED
		public PurchasedItem GetSubCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetSubCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x0007DE06 File Offset: 0x0007C006
		public List<PurchasedItem> GetPurchasedItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.PurchasedItems, create);
		}

		// Token: 0x06000DAC RID: 3500 RVA: 0x0007DE16 File Offset: 0x0007C016
		public List<PurchasedItem> GetPurchasedItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetPurchasedItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000DAD RID: 3501 RVA: 0x0007DE2F File Offset: 0x0007C02F
		public int GetPurchasedItemCount(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetPurchasedItemCount((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000DAE RID: 3502 RVA: 0x0007DE48 File Offset: 0x0007C048
		public int GetPurchasedItemCount(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> purchasedItems = this.GetPurchasedItems(identifier, false);
			if (purchasedItems == null)
			{
				return 0;
			}
			return (from i in purchasedItems
			where i.ItemPrefab == prefab
			select i).Sum((PurchasedItem it) => it.Quantity);
		}

		// Token: 0x06000DAF RID: 3503 RVA: 0x0007DEA5 File Offset: 0x0007C0A5
		public List<SoldItem> GetSoldItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<SoldItem>(identifier, this.SoldItems, create);
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x0007DEB5 File Offset: 0x0007C0B5
		public List<SoldItem> GetSoldItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSoldItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x0007DECE File Offset: 0x0007C0CE
		public void ClearItemsInBuyCrate()
		{
			this.ItemsInBuyCrate.Clear();
			NamedEvent<CargoManager> onItemsInBuyCrateChanged = this.OnItemsInBuyCrateChanged;
			if (onItemsInBuyCrateChanged == null)
			{
				return;
			}
			onItemsInBuyCrateChanged.Invoke(this);
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x0007DEEC File Offset: 0x0007C0EC
		public void ClearItemsInSellCrate()
		{
			this.ItemsInSellCrate.Clear();
			NamedEvent<CargoManager> onItemsInSellCrateChanged = this.OnItemsInSellCrateChanged;
			if (onItemsInSellCrateChanged == null)
			{
				return;
			}
			onItemsInSellCrateChanged.Invoke(this);
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x0007DF0A File Offset: 0x0007C10A
		public void ClearItemsInSellFromSubCrate()
		{
			this.ItemsInSellFromSubCrate.Clear();
			NamedEvent<CargoManager> onItemsInSellFromSubCrateChanged = this.OnItemsInSellFromSubCrateChanged;
			if (onItemsInSellFromSubCrateChanged == null)
			{
				return;
			}
			onItemsInSellFromSubCrateChanged.Invoke(this);
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0007DF28 File Offset: 0x0007C128
		public void SetPurchasedItems(Dictionary<Identifier, List<PurchasedItem>> purchasedItems)
		{
			if (purchasedItems.Count == 0 && this.PurchasedItems.Count == 0)
			{
				return;
			}
			this.PurchasedItems.Clear();
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> entry in purchasedItems)
			{
				this.PurchasedItems.Add(entry.Key, entry.Value);
			}
			NamedEvent<CargoManager> onPurchasedItemsChanged = this.OnPurchasedItemsChanged;
			if (onPurchasedItemsChanged == null)
			{
				return;
			}
			onPurchasedItemsChanged.Invoke(this);
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x0007DFBC File Offset: 0x0007C1BC
		public void ModifyItemQuantityInBuyCrate(Identifier storeIdentifier, ItemPrefab itemPrefab, int changeInQuantity, Client client = null)
		{
			PurchasedItem item = this.GetBuyCrateItem(storeIdentifier, itemPrefab);
			if (item != null)
			{
				item.Quantity += changeInQuantity;
				if (item.Quantity < 1)
				{
					this.GetBuyCrateItems(storeIdentifier, true).Remove(item);
				}
			}
			else if (changeInQuantity > 0)
			{
				this.GetBuyCrateItems(storeIdentifier, true).Add(new PurchasedItem(itemPrefab, changeInQuantity, client));
			}
			NamedEvent<CargoManager> onItemsInBuyCrateChanged = this.OnItemsInBuyCrateChanged;
			if (onItemsInBuyCrateChanged == null)
			{
				return;
			}
			onItemsInBuyCrateChanged.Invoke(this);
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x0007E028 File Offset: 0x0007C228
		public void ModifyItemQuantityInSubSellCrate(Identifier storeIdentifier, ItemPrefab itemPrefab, int changeInQuantity, Client client = null)
		{
			PurchasedItem item = this.GetSubCrateItem(storeIdentifier, itemPrefab);
			if (item != null)
			{
				item.Quantity += changeInQuantity;
				if (item.Quantity < 1)
				{
					List<PurchasedItem> subCrateItems = this.GetSubCrateItems(storeIdentifier, false);
					if (subCrateItems != null)
					{
						subCrateItems.Remove(item);
					}
				}
			}
			else if (changeInQuantity > 0)
			{
				this.GetSubCrateItems(storeIdentifier, true).Add(new PurchasedItem(itemPrefab, changeInQuantity, client));
			}
			NamedEvent<CargoManager> onItemsInSellFromSubCrateChanged = this.OnItemsInSellFromSubCrateChanged;
			if (onItemsInSellFromSubCrateChanged == null)
			{
				return;
			}
			onItemsInSellFromSubCrateChanged.Invoke(this);
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x0007E09C File Offset: 0x0007C29C
		public void PurchaseItems(Identifier storeIdentifier, List<PurchasedItem> itemsToPurchase, bool removeFromCrate, Client client = null)
		{
			Location location = this.Location;
			Location.StoreInfo store = (location != null) ? location.GetStore(storeIdentifier) : null;
			if (store == null)
			{
				return;
			}
			List<PurchasedItem> itemsPurchasedFromStore = this.GetPurchasedItems(storeIdentifier, true);
			Dictionary<ItemPrefab, int> buyValues = this.GetBuyValuesAtCurrentLocation(storeIdentifier, from i in itemsToPurchase
			select i.ItemPrefab);
			List<PurchasedItem> itemsInStoreCrate = this.GetBuyCrateItems(storeIdentifier, true);
			using (List<PurchasedItem>.Enumerator enumerator = itemsToPurchase.ToList<PurchasedItem>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PurchasedItem item = enumerator.Current;
					if (item.Quantity > 0)
					{
						int itemValue = item.Quantity * buyValues[item.ItemPrefab];
						if (!this.campaign.TryPurchase(client, itemValue))
						{
							itemsToPurchase.Remove(item);
						}
						else
						{
							PurchasedItem purchasedItem = itemsPurchasedFromStore.Find((PurchasedItem pi) => pi.ItemPrefab == item.ItemPrefab && pi.DeliverImmediately == item.DeliverImmediately);
							if (purchasedItem != null)
							{
								purchasedItem.Quantity += item.Quantity;
							}
							else
							{
								purchasedItem = new PurchasedItem(item.ItemPrefab, item.Quantity, client)
								{
									DeliverImmediately = item.DeliverImmediately
								};
								itemsPurchasedFromStore.Add(purchasedItem);
							}
							purchasedItem.Delivered = item.DeliverImmediately;
							if (GameMain.IsSingleplayer)
							{
								GameAnalyticsManager.AddMoneySpentEvent(itemValue, GameAnalyticsManager.MoneySink.Store, item.ItemPrefab.Identifier.Value);
							}
							store.Balance += itemValue;
						}
					}
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient)
			{
				Character targetCharacter = Character.Controlled;
				if (targetCharacter == null)
				{
					DebugConsole.ThrowError("Failed to deliver items directly to a character (not controlling a character).", null, null, false, false);
				}
				if (targetCharacter == null)
				{
					CargoManager.DeliverItemsToSub(from it in itemsToPurchase
					where it.DeliverImmediately
					select it, Submarine.MainSub, this, true);
				}
				else
				{
					CargoManager.DeliverItemsToCharacter(from it in itemsToPurchase
					where it.DeliverImmediately
					select it, targetCharacter, this);
				}
			}
			if (removeFromCrate)
			{
				using (List<PurchasedItem>.Enumerator enumerator2 = itemsToPurchase.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						PurchasedItem item = enumerator2.Current;
						PurchasedItem crateItem = itemsInStoreCrate.Find((PurchasedItem pi) => pi.ItemPrefab == item.ItemPrefab);
						if (crateItem != null)
						{
							crateItem.Quantity -= item.Quantity;
							if (crateItem.Quantity < 1)
							{
								itemsInStoreCrate.Remove(crateItem);
							}
						}
					}
				}
			}
			NamedEvent<CargoManager> onPurchasedItemsChanged = this.OnPurchasedItemsChanged;
			if (onPurchasedItemsChanged == null)
			{
				return;
			}
			onPurchasedItemsChanged.Invoke(this);
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x0007E3A8 File Offset: 0x0007C5A8
		public Dictionary<ItemPrefab, int> GetBuyValuesAtCurrentLocation(Identifier storeIdentifier, IEnumerable<ItemPrefab> items)
		{
			Dictionary<ItemPrefab, int> buyValues = new Dictionary<ItemPrefab, int>();
			Location location = this.Location;
			Location.StoreInfo store = (location != null) ? location.GetStore(storeIdentifier) : null;
			if (store == null)
			{
				return buyValues;
			}
			foreach (ItemPrefab item in items)
			{
				if (item != null && !buyValues.ContainsKey(item))
				{
					int buyValue = (store != null) ? store.GetAdjustedItemBuyPrice(item, null, true) : 0;
					buyValues.Add(item, buyValue);
				}
			}
			return buyValues;
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x0007E430 File Offset: 0x0007C630
		public Dictionary<ItemPrefab, int> GetSellValuesAtCurrentLocation(Identifier storeIdentifier, IEnumerable<ItemPrefab> items)
		{
			Dictionary<ItemPrefab, int> sellValues = new Dictionary<ItemPrefab, int>();
			Location location = this.Location;
			Location.StoreInfo store = (location != null) ? location.GetStore(storeIdentifier) : null;
			if (store == null)
			{
				return sellValues;
			}
			foreach (ItemPrefab item in items)
			{
				if (item != null && !sellValues.ContainsKey(item))
				{
					int sellValue = (store != null) ? store.GetAdjustedItemSellPrice(item, null, true) : 0;
					sellValues.Add(item, sellValue);
				}
			}
			return sellValues;
		}

		// Token: 0x06000DBA RID: 3514 RVA: 0x0007E4B8 File Offset: 0x0007C6B8
		public void CreatePurchasedItems()
		{
			this.purchasedIDCards.Clear();
			List<PurchasedItem> items = new List<PurchasedItem>();
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> storeSpecificItems in this.PurchasedItems)
			{
				items.AddRange(from it in storeSpecificItems.Value
				where !it.DeliverImmediately
				select it);
			}
			CargoManager.DeliverItemsToSub(items, Submarine.MainSub, this, true);
			this.PurchasedItems.Clear();
			NamedEvent<CargoManager> onPurchasedItemsChanged = this.OnPurchasedItemsChanged;
			if (onPurchasedItemsChanged == null)
			{
				return;
			}
			onPurchasedItemsChanged.Invoke(this);
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x06000DBB RID: 3515 RVA: 0x0007E570 File Offset: 0x0007C770
		private Dictionary<ItemPrefab, int> UndeterminedSoldEntities { get; } = new Dictionary<ItemPrefab, int>();

		// Token: 0x06000DBC RID: 3516 RVA: 0x0007E578 File Offset: 0x0007C778
		public IEnumerable<Item> GetSellableItemsFromSub()
		{
			if (Submarine.MainSub == null)
			{
				return new List<Item>();
			}
			IEnumerable<CargoManager.SoldEntity> confirmedSoldEntities = Enumerable.Empty<CargoManager.SoldEntity>();
			this.UndeterminedSoldEntities.Clear();
			confirmedSoldEntities = this.GetConfirmedSoldEntities();
			foreach (CargoManager.SoldEntity soldEntity in this.SoldEntities)
			{
				if (soldEntity.Item == null)
				{
					int count;
					if (this.UndeterminedSoldEntities.TryGetValue(soldEntity.ItemPrefab, out count))
					{
						this.UndeterminedSoldEntities[soldEntity.ItemPrefab] = count + 1;
					}
					else
					{
						this.UndeterminedSoldEntities.Add(soldEntity.ItemPrefab, 1);
					}
				}
			}
			return (from it in CargoManager.FindAllSellableItems()
			where this.IsItemSellable(it, confirmedSoldEntities)
			select it).ToList<Item>();
		}

		// Token: 0x06000DBD RID: 3517 RVA: 0x0007E660 File Offset: 0x0007C860
		public static IReadOnlyCollection<Item> FindAllItemsOnPlayerAndSub(Character character)
		{
			List<Item> allItems = new List<Item>();
			CharacterInventory inv = (character != null) ? character.Inventory : null;
			if (inv != null)
			{
				allItems.AddRange(inv.FindAllItems(null, true, null));
			}
			allItems.AddRange(CargoManager.FindAllSellableItems());
			return allItems;
		}

		// Token: 0x06000DBE RID: 3518 RVA: 0x0007E6A0 File Offset: 0x0007C8A0
		public static IEnumerable<Item> FindAllSellableItems()
		{
			if (Submarine.MainSub == null)
			{
				return Enumerable.Empty<Item>();
			}
			return Submarine.MainSub.GetItems(true).FindAll(delegate(Item item)
			{
				if (item.GetRootInventoryOwner() is Character)
				{
					return false;
				}
				if (!item.Components.All(delegate(ItemComponent c)
				{
					Holdable holdable = c as Holdable;
					return holdable == null || !holdable.Attachable || !holdable.Attached;
				}))
				{
					return false;
				}
				return item.Components.All(delegate(ItemComponent c)
				{
					Wire w = c as Wire;
					if (w != null)
					{
						return w.Connections.All((Connection c) => c == null);
					}
					return true;
				}) && CargoManager.<FindAllSellableItems>g__ItemAndAllContainersInteractable|73_2(item) && CargoManager.<FindAllSellableItems>g__AllContainersAllowSellingItems|73_1(item);
			}).Distinct<Item>();
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x0007E6F0 File Offset: 0x0007C8F0
		private bool IsItemSellable(Item item, IEnumerable<CargoManager.SoldEntity> confirmedItems)
		{
			if (item.Removed)
			{
				return false;
			}
			if (!item.Prefab.CanBeSold)
			{
				return false;
			}
			if (item.SpawnedInCurrentOutpost)
			{
				return false;
			}
			if (!item.Prefab.AllowSellingWhenBroken && item.ConditionPercentage < 90f)
			{
				return false;
			}
			if (confirmedItems != null && confirmedItems.Any((CargoManager.SoldEntity ci) => ci.Item == item))
			{
				return false;
			}
			int count;
			if (this.UndeterminedSoldEntities.TryGetValue(item.Prefab, out count))
			{
				int newCount = count - 1;
				if (newCount > 0)
				{
					this.UndeterminedSoldEntities[item.Prefab] = newCount;
				}
				else
				{
					this.UndeterminedSoldEntities.Remove(item.Prefab);
				}
				return false;
			}
			for (Item rootContainer = item.Container; rootContainer != null; rootContainer = rootContainer.Container)
			{
				ItemInventory ownInventory = rootContainer.OwnInventory;
				ItemContainer containerComponent = (ownInventory != null) ? ownInventory.Container : null;
				if (containerComponent != null)
				{
					if (!containerComponent.DrawInventory)
					{
						return false;
					}
					if (!containerComponent.IsAccessible())
					{
						return false;
					}
				}
			}
			ItemInventory ownInventory2 = item.OwnInventory;
			ItemContainer itemContainer = (ownInventory2 != null) ? ownInventory2.Container : null;
			if (itemContainer != null)
			{
				IEnumerable<Item> containedItems = item.ContainedItems;
				if (containedItems.None(null))
				{
					return true;
				}
				if (itemContainer.RemoveContainedItemsOnDeconstruct)
				{
					if (containedItems.All((Item it) => !it.Prefab.CanBeSold))
					{
						return true;
					}
				}
				if (confirmedItems != null && !containedItems.All((Item it) => confirmedItems.Any((CargoManager.SoldEntity ci) => ci.Item == it)))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x0007E8AF File Offset: 0x0007CAAF
		public static IEnumerable<Hull> FindCargoRooms(IEnumerable<Submarine> subs)
		{
			return subs.SelectMany((Submarine s) => CargoManager.FindCargoRooms(s));
		}

		// Token: 0x06000DC1 RID: 3521 RVA: 0x0007E8D8 File Offset: 0x0007CAD8
		public static IEnumerable<Hull> FindCargoRooms(Submarine sub)
		{
			return (from wp in WayPoint.WayPointList
			where wp.Submarine == sub && wp.SpawnType == SpawnType.Cargo
			select wp.CurrentHull).Distinct<Hull>();
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x0007E934 File Offset: 0x0007CB34
		public static IEnumerable<Item> FilterCargoCrates(IEnumerable<Item> items, Func<Item, bool> conditional = null)
		{
			return from it in items
			where it.HasTag(Tags.Crate) && !it.NonInteractable && !it.NonPlayerTeamInteractable && !it.IsHidden && !it.Removed && (conditional == null || conditional(it))
			select it;
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x0007E960 File Offset: 0x0007CB60
		public static IEnumerable<ItemContainer> FindReusableCargoContainers(IEnumerable<Submarine> subs, IEnumerable<Hull> cargoRooms = null)
		{
			return from it in CargoManager.FilterCargoCrates(Item.ItemList, (Item it) => subs.Contains(it.Submarine) && !it.HasTag(Tags.CargoMissionItem) && (cargoRooms == null || cargoRooms.Contains(it.CurrentHull)))
			select it.GetComponent<ItemContainer>() into c
			where c != null
			select c;
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x0007E9E0 File Offset: 0x0007CBE0
		public static ItemContainer GetOrCreateCargoContainerFor(ItemPrefab item, ISpatialEntity cargoRoomOrSpawnPoint, ref List<ItemContainer> availableContainers)
		{
			ItemContainer itemContainer = null;
			if (!string.IsNullOrEmpty(item.CargoContainerIdentifier))
			{
				itemContainer = availableContainers.Find((ItemContainer ac) => ac.Inventory.CanProbablyBePut(item, null, null) && (ac.Item.Prefab.Identifier == item.CargoContainerIdentifier || ac.Item.Prefab.Tags.Contains(item.CargoContainerIdentifier)));
				if (itemContainer == null)
				{
					ItemPrefab containerPrefab = ItemPrefab.Prefabs.Find((ItemPrefab ep) => ep.Identifier == item.CargoContainerIdentifier || (ep.Tags != null && ep.Tags.Contains(item.CargoContainerIdentifier)));
					if (containerPrefab == null)
					{
						DebugConsole.AddWarning("CargoManager: could not find the item prefab for container " + item.CargoContainerIdentifier + "!", null);
						return null;
					}
					Hull cargoRoom = cargoRoomOrSpawnPoint as Hull;
					Vector2 containerPosition = (cargoRoom != null) ? CargoManager.GetCargoPos(cargoRoom, containerPrefab) : cargoRoomOrSpawnPoint.Position;
					Item containerItem = new Item(containerPrefab, containerPosition, cargoRoomOrSpawnPoint.Submarine, 0, true);
					itemContainer = containerItem.GetComponent<ItemContainer>();
					if (itemContainer == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
						defaultInterpolatedStringHandler.AppendLiteral("CargoManager: No ItemContainer component found in ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(containerItem.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("!");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						return null;
					}
					if (!itemContainer.CanBeContained(item))
					{
						containerItem.Remove();
						return null;
					}
					availableContainers.Add(itemContainer);
				}
			}
			return itemContainer;
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x0007EB04 File Offset: 0x0007CD04
		public static void DeliverItemsToSub(IEnumerable<PurchasedItem> itemsToSpawn, Submarine sub, CargoManager cargoManager, bool showNotification = true)
		{
			if (!itemsToSpawn.Any<PurchasedItem>())
			{
				return;
			}
			WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, sub, false, null, false);
			if (wp == null)
			{
				DebugConsole.ThrowError("The submarine must have a waypoint marked as Cargo for bought items to be placed correctly!", null, null, false, false);
				return;
			}
			Hull cargoRoom = Hull.FindHull(wp.WorldPosition, null, true, true);
			if (cargoRoom == null)
			{
				DebugConsole.ThrowError("A waypoint marked as Cargo must be placed inside a room!", null, null, false, false);
				return;
			}
			bool flag;
			if (sub == Submarine.MainSub)
			{
				flag = itemsToSpawn.Any((PurchasedItem it) => !it.Delivered && it.Quantity > 0);
			}
			else
			{
				flag = false;
			}
			if (flag && showNotification)
			{
				new GUIMessageBox("", TextManager.GetWithVariable("CargoSpawnNotification", "[roomname]", cargoRoom.DisplayName, FormatCapitals.Yes), Array.Empty<LocalizedString>(), null, null, Alignment.TopLeft, GUIMessageBox.Type.InGame, "", null, "StoreShoppingCrateIcon", null, null, false);
			}
			IEnumerable<Submarine> connectedSubs = from s in sub.GetConnectedSubs()
			where s.Info.Type == SubmarineType.Player
			select s;
			List<ItemContainer> availableContainers = CargoManager.FindReusableCargoContainers(connectedSubs, CargoManager.FindCargoRooms(connectedSubs)).ToList<ItemContainer>();
			foreach (PurchasedItem pi in itemsToSpawn)
			{
				pi.Delivered = true;
				Vector2 position = CargoManager.GetCargoPos(cargoRoom, pi.ItemPrefab);
				for (int i = 0; i < pi.Quantity; i++)
				{
					Item item = new Item(pi.ItemPrefab, position, wp.Submarine, 0, true);
					ItemContainer itemContainer = CargoManager.GetOrCreateCargoContainerFor(pi.ItemPrefab, cargoRoom, ref availableContainers);
					if (itemContainer != null)
					{
						itemContainer.Inventory.TryPutItem(item, null, null, true, false, true);
					}
					CargoManager.ItemSpawned(pi, item, cargoManager);
					(((itemContainer != null) ? itemContainer.Item : null) ?? item).AssignCampaignInteractionType(CampaignMode.InteractionType.Cargo, null);
				}
			}
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x0007ECF4 File Offset: 0x0007CEF4
		public static void DeliverItemsToCharacter(IEnumerable<PurchasedItem> itemsToSpawn, Character character, CargoManager cargoManager)
		{
			if (!itemsToSpawn.Any<PurchasedItem>())
			{
				return;
			}
			foreach (PurchasedItem pi in itemsToSpawn)
			{
				pi.Delivered = true;
				for (int i = 0; i < pi.Quantity; i++)
				{
					Item item = new Item(pi.ItemPrefab, character.Position, character.Submarine, 0, true);
					Holdable component = item.GetComponent<Holdable>();
					if (component != null && component.Attached)
					{
						item.Drop(null, true, true);
					}
					if (!character.Inventory.TryPutItem(item, null, item.AllowedSlots, true, false, true))
					{
						foreach (Item containedItem in character.Inventory.AllItemsMod)
						{
							ItemInventory ownInventory = containedItem.OwnInventory;
							ItemContainer container = (ownInventory != null) ? ownInventory.Container : null;
							if (container != null && container.DrawInventory && container.IsAccessible() && containedItem.OwnInventory.TryPutItem(item, null, item.AllowedSlots, true, false, true))
							{
								break;
							}
						}
					}
					CargoManager.ItemSpawned(pi, item, cargoManager);
				}
			}
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x0007EE58 File Offset: 0x0007D058
		private static void ItemSpawned(PurchasedItem purchased, Item item, CargoManager cargoManager)
		{
			IdCard idCard = item.GetComponent<IdCard>();
			if (cargoManager != null && idCard != null && purchased.BuyerCharacterInfoIdentifier != 0)
			{
				if (purchased.DeliverImmediately)
				{
					CargoManager.InitPurchasedIDCard(purchased, idCard);
				}
				else
				{
					cargoManager.purchasedIDCards.Add(new ValueTuple<PurchasedItem, IdCard>(purchased, idCard));
				}
			}
			CargoManager.ItemSpawned(item);
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x0007EEA4 File Offset: 0x0007D0A4
		public static void ItemSpawned(Item item)
		{
			CharacterTeamType teamID = CharacterTeamType.Team1;
			Inventory parentInventory = item.ParentInventory;
			Character character = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
			if (character != null)
			{
				teamID = character.TeamID;
			}
			else
			{
				Submarine submarine;
				if ((submarine = item.Submarine) == null)
				{
					Item rootContainer = item.RootContainer;
					submarine = ((rootContainer != null) ? rootContainer.Submarine : null);
				}
				Submarine sub = submarine;
				if (sub != null)
				{
					teamID = sub.TeamID;
				}
			}
			foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
			{
				wifiComponent.TeamID = teamID;
			}
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x0007EF40 File Offset: 0x0007D140
		public void InitPurchasedIDCards()
		{
			foreach (ValueTuple<PurchasedItem, IdCard> valueTuple in this.purchasedIDCards)
			{
				PurchasedItem purchased = valueTuple.Item1;
				IdCard idCard = valueTuple.Item2;
				CargoManager.InitPurchasedIDCard(purchased, idCard);
			}
		}

		// Token: 0x06000DCA RID: 3530 RVA: 0x0007EFA0 File Offset: 0x0007D1A0
		private static void InitPurchasedIDCard(PurchasedItem purchased, IdCard idCard)
		{
			if (idCard != null && purchased.BuyerCharacterInfoIdentifier != 0)
			{
				Character owner = Character.CharacterList.Find(delegate(Character c)
				{
					CharacterInfo info = c.Info;
					int? num = (info != null) ? new int?(info.GetIdentifier()) : null;
					int buyerCharacterInfoIdentifier = purchased.BuyerCharacterInfoIdentifier;
					return num.GetValueOrDefault() == buyerCharacterInfoIdentifier & num != null;
				});
				if (((owner != null) ? owner.Info : null) != null)
				{
					WayPoint[] mainSubSpawnPoints = WayPoint.SelectCrewSpawnPoints(new List<CharacterInfo>
					{
						owner.Info
					}, Submarine.MainSub);
					idCard.Initialize(mainSubSpawnPoints.FirstOrDefault<WayPoint>(), owner);
				}
			}
		}

		// Token: 0x06000DCB RID: 3531 RVA: 0x0007F018 File Offset: 0x0007D218
		public static Vector2 GetCargoPos(Hull hull, ItemPrefab itemPrefab)
		{
			float floorPos = (float)(hull.Rect.Y - hull.Rect.Height);
			Vector2 position = new Vector2((hull.Rect.Width > 40) ? Rand.Range((float)hull.Rect.X + 20f, (float)hull.Rect.Right - 20f, Rand.RandSync.Unsynced) : ((float)hull.Rect.Center.X), floorPos);
			if (Submarine.PickBody(ConvertUnits.ToSimUnits(new Vector2(position.X, (float)(hull.Rect.Y - hull.Rect.Height / 2))), ConvertUnits.ToSimUnits(position), null, new Category?(Category.Cat1), true, null, false) != null)
			{
				float floorStructurePos = ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition.Y);
				if (floorStructurePos > floorPos)
				{
					floorPos = floorStructurePos;
				}
			}
			position.Y = floorPos + itemPrefab.Size.Y / 2f;
			return position;
		}

		// Token: 0x06000DCC RID: 3532 RVA: 0x0007F10C File Offset: 0x0007D30C
		public void SavePurchasedItems(XElement parentElement)
		{
			XElement itemsElement = new XElement("cargo");
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> storeSpecificItems in this.PurchasedItems)
			{
				foreach (PurchasedItem item in storeSpecificItems.Value)
				{
					if (((item != null) ? item.ItemPrefab : null) != null)
					{
						itemsElement.Add(new XElement("item", new object[]
						{
							new XAttribute("id", item.ItemPrefab.Identifier),
							new XAttribute("qty", item.Quantity),
							new XAttribute("storeid", storeSpecificItems.Key),
							new XAttribute("deliverimmediately", item.DeliverImmediately),
							new XAttribute("buyer", item.BuyerCharacterInfoIdentifier)
						}));
					}
				}
			}
			parentElement.Add(itemsElement);
		}

		// Token: 0x06000DCD RID: 3533 RVA: 0x0007F29C File Offset: 0x0007D49C
		public void LoadPurchasedItems(XElement element)
		{
			Dictionary<Identifier, List<PurchasedItem>> purchasedItems = new Dictionary<Identifier, List<PurchasedItem>>();
			if (element != null)
			{
				foreach (XElement itemElement in element.GetChildElements("item", StringComparison.OrdinalIgnoreCase))
				{
					string prefabId = itemElement.GetAttributeString("id", null);
					ItemPrefab prefab;
					if (!string.IsNullOrWhiteSpace(prefabId) && ItemPrefab.Prefabs.TryGet(prefabId.ToIdentifier(), out prefab))
					{
						int qty = itemElement.GetAttributeInt("qty", 0);
						Identifier storeId = itemElement.GetAttributeIdentifier("storeid", "merchant");
						bool deliverImmediately = itemElement.GetAttributeBool("deliverimmediately", false);
						int buyerId = itemElement.GetAttributeInt("buyer", 0);
						List<PurchasedItem> storeItems;
						if (!purchasedItems.TryGetValue(storeId, out storeItems))
						{
							storeItems = new List<PurchasedItem>();
							purchasedItems.Add(storeId, storeItems);
						}
						storeItems.Add(new PurchasedItem(prefab, qty, buyerId)
						{
							DeliverImmediately = deliverImmediately,
							Delivered = deliverImmediately
						});
					}
				}
			}
			this.SetPurchasedItems(purchasedItems);
		}

		// Token: 0x06000DCF RID: 3535 RVA: 0x0007F3FC File Offset: 0x0007D5FC
		[CompilerGenerated]
		internal static bool <SetSoldItems>g__Match|8_0(SoldItem soldItem, CargoManager.SoldEntity soldEntity, bool matchId)
		{
			return soldItem.ItemPrefab == soldEntity.ItemPrefab && (!matchId || (soldEntity.Item != null && soldItem.ID == soldEntity.Item.ID)) && (soldItem.Origin != SoldItem.SellOrigin.Character || GameMain.Client == null || soldItem.SellerID == GameMain.Client.SessionId);
		}

		// Token: 0x06000DD0 RID: 3536 RVA: 0x0007F460 File Offset: 0x0007D660
		[CompilerGenerated]
		internal static bool <FindAllSellableItems>g__AllContainersAllowSellingItems|73_1(Item item)
		{
			for (;;)
			{
				item = item.Container;
				if (item == null)
				{
					break;
				}
				if (item.HasTag(Tags.DontSellItems))
				{
					return false;
				}
				if (item.Components.Any((ItemComponent c) => c.DisallowSellingItemsFromContainer))
				{
					return false;
				}
				if (item == null)
				{
					return true;
				}
			}
			return true;
		}

		// Token: 0x06000DD1 RID: 3537 RVA: 0x0007F4BB File Offset: 0x0007D6BB
		[CompilerGenerated]
		internal static bool <FindAllSellableItems>g__ItemAndAllContainersInteractable|73_2(Item item)
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

		// Token: 0x0400070D RID: 1805
		private static readonly HashSet<InvSlotType> equipmentSlots = new HashSet<InvSlotType>
		{
			InvSlotType.Head,
			InvSlotType.InnerClothes,
			InvSlotType.OuterClothes,
			InvSlotType.Headset,
			InvSlotType.Card,
			InvSlotType.HealthInterface
		};

		// Token: 0x0400070E RID: 1806
		public const int MaxQuantity = 100;

		// Token: 0x04000714 RID: 1812
		private readonly CampaignMode campaign;

		// Token: 0x04000715 RID: 1813
		public readonly NamedEvent<CargoManager> OnItemsInBuyCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x04000716 RID: 1814
		public readonly NamedEvent<CargoManager> OnItemsInSellCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x04000717 RID: 1815
		public readonly NamedEvent<CargoManager> OnItemsInSellFromSubCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x04000718 RID: 1816
		public readonly NamedEvent<CargoManager> OnPurchasedItemsChanged = new NamedEvent<CargoManager>();

		// Token: 0x04000719 RID: 1817
		public readonly NamedEvent<CargoManager> OnSoldItemsChanged = new NamedEvent<CargoManager>();

		// Token: 0x0400071B RID: 1819
		[TupleElementNames(new string[]
		{
			"purchaseInfo",
			"idCard"
		})]
		private readonly List<ValueTuple<PurchasedItem, IdCard>> purchasedIDCards = new List<ValueTuple<PurchasedItem, IdCard>>();

		// Token: 0x0200083A RID: 2106
		private class SoldEntity
		{
			// Token: 0x17001A1A RID: 6682
			// (get) Token: 0x06006D32 RID: 27954 RVA: 0x00361D28 File Offset: 0x0035FF28
			// (set) Token: 0x06006D33 RID: 27955 RVA: 0x00361D30 File Offset: 0x0035FF30
			public Item Item { get; private set; }

			// Token: 0x17001A1B RID: 6683
			// (get) Token: 0x06006D34 RID: 27956 RVA: 0x00361D39 File Offset: 0x0035FF39
			public ItemPrefab ItemPrefab { get; }

			// Token: 0x17001A1C RID: 6684
			// (get) Token: 0x06006D35 RID: 27957 RVA: 0x00361D41 File Offset: 0x0035FF41
			// (set) Token: 0x06006D36 RID: 27958 RVA: 0x00361D49 File Offset: 0x0035FF49
			public CargoManager.SoldEntity.SellStatus Status { get; set; }

			// Token: 0x06006D37 RID: 27959 RVA: 0x00361D52 File Offset: 0x0035FF52
			public SoldEntity(Item item, CargoManager.SoldEntity.SellStatus status)
			{
				this.Item = item;
				this.ItemPrefab = ((item != null) ? item.Prefab : null);
				this.Status = status;
			}

			// Token: 0x06006D38 RID: 27960 RVA: 0x00361D7A File Offset: 0x0035FF7A
			public SoldEntity(ItemPrefab itemPrefab, CargoManager.SoldEntity.SellStatus status)
			{
				this.ItemPrefab = itemPrefab;
				this.Status = status;
			}

			// Token: 0x06006D39 RID: 27961 RVA: 0x00361D90 File Offset: 0x0035FF90
			public void SetItem(Item item)
			{
				if (this.Item != null)
				{
					DebugConsole.LogError("Trying to set SoldEntity.Item, but it's already set!\n" + Environment.StackTrace.CleanupStackTrace(), null, null);
					return;
				}
				this.Item = item;
			}

			// Token: 0x02001529 RID: 5417
			public enum SellStatus
			{
				// Token: 0x04006799 RID: 26521
				Confirmed,
				// Token: 0x0400679A RID: 26522
				Unconfirmed,
				// Token: 0x0400679B RID: 26523
				Local
			}
		}
	}
}
