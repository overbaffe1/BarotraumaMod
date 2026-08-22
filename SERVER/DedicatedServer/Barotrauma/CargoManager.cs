using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002E RID: 46
	internal class CargoManager
	{
		// Token: 0x0600055E RID: 1374 RVA: 0x0003236C File Offset: 0x0003056C
		public void BuyBackSoldItems(Identifier storeIdentifier, List<SoldItem> itemsToBuy, Client client)
		{
			Location.StoreInfo store = this.Location.GetStore(storeIdentifier);
			if (store == null)
			{
				return;
			}
			List<SoldItem> storeSpecificItems = this.SoldItems.GetValueOrDefault(storeIdentifier);
			Dictionary<ItemPrefab, int> sellValues = this.GetSellValuesAtCurrentLocation(storeIdentifier, from i in itemsToBuy
			select i.ItemPrefab);
			foreach (SoldItem item in itemsToBuy)
			{
				int itemValue = sellValues[item.ItemPrefab];
				if (store.Balance >= itemValue && !item.Removed)
				{
					store.Balance += itemValue;
					this.campaign.TryPurchase(client, itemValue);
					storeSpecificItems.Remove(item);
				}
			}
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x00032448 File Offset: 0x00030648
		public void SellItems(Identifier storeIdentifier, List<SoldItem> itemsToSell, Client client)
		{
			Location.StoreInfo store = this.Location.GetStore(storeIdentifier);
			if (store == null)
			{
				return;
			}
			bool canAddToRemoveQueue = (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && Entity.Spawner != null;
			IEnumerable<Item> sellableItemsInSub = Enumerable.Empty<Item>();
			if (canAddToRemoveQueue)
			{
				if (itemsToSell.Any((SoldItem i) => i.Origin == SoldItem.SellOrigin.Submarine && i.ID == 0 && !i.Removed))
				{
					sellableItemsInSub = this.GetSellableItemsFromSub();
				}
			}
			List<SoldItem> itemsSoldAtStore = this.SoldItems.GetValueOrDefault(storeIdentifier);
			Dictionary<ItemPrefab, int> sellValues = this.GetSellValuesAtCurrentLocation(storeIdentifier, from i in itemsToSell
			select i.ItemPrefab);
			using (List<SoldItem>.Enumerator enumerator = itemsToSell.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SoldItem item = enumerator.Current;
					int itemValue = sellValues[item.ItemPrefab];
					if (store.Balance >= itemValue && !item.Removed)
					{
						if (item.Origin == SoldItem.SellOrigin.Submarine && item.ID == 0 && !item.Removed)
						{
							Item matchingItem = sellableItemsInSub.FirstOrDefault((Item i) => !i.Removed && i.Prefab == item.ItemPrefab && itemsToSell.None((SoldItem itemToSell) => itemToSell.ItemPrefab == i.Prefab && itemToSell.ID == i.ID));
							if (matchingItem == null)
							{
								continue;
							}
							item.SetItemId(matchingItem.ID);
						}
						if (!item.Removed && canAddToRemoveQueue)
						{
							Item entity = Entity.FindEntityByID(item.ID) as Item;
							if (entity != null)
							{
								item.Removed = true;
								Entity.Spawner.AddItemToRemoveQueue(entity);
							}
						}
						if (itemsSoldAtStore != null)
						{
							itemsSoldAtStore.Add(item);
						}
						store.Balance -= itemValue;
						this.campaign.GetWallet(client).Give(itemValue);
						GameAnalyticsManager.AddMoneyGainedEvent(itemValue, GameAnalyticsManager.MoneySource.Store, item.ItemPrefab.Identifier.Value);
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

		// Token: 0x06000560 RID: 1376 RVA: 0x000326B0 File Offset: 0x000308B0
		public void LogNewItemPurchases(Identifier storeIdentifier, List<PurchasedItem> newItems, Client client)
		{
			StringBuilder sb = new StringBuilder();
			int price = 0;
			Dictionary<ItemPrefab, int> buyValues = this.GetBuyValuesAtCurrentLocation(storeIdentifier, from i in newItems
			select i.ItemPrefab);
			foreach (PurchasedItem item in newItems)
			{
				int itemValue = item.Quantity * buyValues[item.ItemPrefab];
				GameAnalyticsManager.AddMoneySpentEvent(itemValue, GameAnalyticsManager.MoneySink.Store, item.ItemPrefab.Identifier.Value);
				StringBuilder stringBuilder = sb;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("\n - ");
				appendInterpolatedStringHandler.AppendFormatted<LocalizedString>(item.ItemPrefab.Name);
				appendInterpolatedStringHandler.AppendLiteral(" x");
				appendInterpolatedStringHandler.AppendFormatted<int>(item.Quantity);
				stringBuilder2.Append(ref appendInterpolatedStringHandler);
				price += itemValue;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 4);
			defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, ((client != null) ? client.Name : null) ?? "Unknown"));
			defaultInterpolatedStringHandler.AppendLiteral(" purchased ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(newItems.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" item(s) for ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.FormatCurrency(price, true));
			defaultInterpolatedStringHandler.AppendFormatted(sb.ToString());
			GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00032834 File Offset: 0x00030A34
		public void ClearSoldItemsProjSpecific()
		{
			this.SoldItems.Clear();
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000562 RID: 1378 RVA: 0x00032841 File Offset: 0x00030A41
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInBuyCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000563 RID: 1379 RVA: 0x00032849 File Offset: 0x00030A49
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInSellCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000564 RID: 1380 RVA: 0x00032851 File Offset: 0x00030A51
		public Dictionary<Identifier, List<PurchasedItem>> ItemsInSellFromSubCrate { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000565 RID: 1381 RVA: 0x00032859 File Offset: 0x00030A59
		public Dictionary<Identifier, List<PurchasedItem>> PurchasedItems { get; } = new Dictionary<Identifier, List<PurchasedItem>>();

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x00032861 File Offset: 0x00030A61
		public Dictionary<Identifier, List<SoldItem>> SoldItems { get; } = new Dictionary<Identifier, List<SoldItem>>();

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000567 RID: 1383 RVA: 0x00032869 File Offset: 0x00030A69
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

		// Token: 0x06000568 RID: 1384 RVA: 0x00032888 File Offset: 0x00030A88
		public CargoManager(CampaignMode campaign)
		{
			this.campaign = campaign;
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x00032928 File Offset: 0x00030B28
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

		// Token: 0x0600056A RID: 1386 RVA: 0x00032984 File Offset: 0x00030B84
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

		// Token: 0x0600056B RID: 1387 RVA: 0x000329B9 File Offset: 0x00030BB9
		public List<PurchasedItem> GetBuyCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInBuyCrate, create);
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x000329C9 File Offset: 0x00030BC9
		public List<PurchasedItem> GetBuyCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetBuyCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x000329E4 File Offset: 0x00030BE4
		public PurchasedItem GetBuyCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> buyCrateItems = this.GetBuyCrateItems(identifier, false);
			if (buyCrateItems == null)
			{
				return null;
			}
			return buyCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x00032A1D File Offset: 0x00030C1D
		public PurchasedItem GetBuyCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetBuyCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x00032A36 File Offset: 0x00030C36
		public List<PurchasedItem> GetSellCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInSellCrate, create);
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x00032A46 File Offset: 0x00030C46
		public List<PurchasedItem> GetSellCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSellCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x00032A60 File Offset: 0x00030C60
		public PurchasedItem GetSellCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> sellCrateItems = this.GetSellCrateItems(identifier, false);
			if (sellCrateItems == null)
			{
				return null;
			}
			return sellCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x00032A99 File Offset: 0x00030C99
		public PurchasedItem GetSellCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetSellCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x00032AB2 File Offset: 0x00030CB2
		public List<PurchasedItem> GetSubCrateItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.ItemsInSellFromSubCrate, create);
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x00032AC2 File Offset: 0x00030CC2
		public List<PurchasedItem> GetSubCrateItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSubCrateItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x00032ADC File Offset: 0x00030CDC
		public PurchasedItem GetSubCrateItem(Identifier identifier, ItemPrefab prefab)
		{
			List<PurchasedItem> subCrateItems = this.GetSubCrateItems(identifier, false);
			if (subCrateItems == null)
			{
				return null;
			}
			return subCrateItems.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == prefab);
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x00032B15 File Offset: 0x00030D15
		public PurchasedItem GetSubCrateItem(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetSubCrateItem((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x00032B2E File Offset: 0x00030D2E
		public List<PurchasedItem> GetPurchasedItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<PurchasedItem>(identifier, this.PurchasedItems, create);
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x00032B3E File Offset: 0x00030D3E
		public List<PurchasedItem> GetPurchasedItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetPurchasedItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x00032B57 File Offset: 0x00030D57
		public int GetPurchasedItemCount(Location.StoreInfo store, ItemPrefab prefab)
		{
			return this.GetPurchasedItemCount((store != null) ? store.Identifier : Identifier.Empty, prefab);
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x00032B70 File Offset: 0x00030D70
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

		// Token: 0x0600057B RID: 1403 RVA: 0x00032BCD File Offset: 0x00030DCD
		public List<SoldItem> GetSoldItems(Identifier identifier, bool create = false)
		{
			return this.GetItems<SoldItem>(identifier, this.SoldItems, create);
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x00032BDD File Offset: 0x00030DDD
		public List<SoldItem> GetSoldItems(Location.StoreInfo store, bool create = false)
		{
			return this.GetSoldItems((store != null) ? store.Identifier : Identifier.Empty, create);
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x00032BF6 File Offset: 0x00030DF6
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

		// Token: 0x0600057E RID: 1406 RVA: 0x00032C14 File Offset: 0x00030E14
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

		// Token: 0x0600057F RID: 1407 RVA: 0x00032C32 File Offset: 0x00030E32
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

		// Token: 0x06000580 RID: 1408 RVA: 0x00032C50 File Offset: 0x00030E50
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

		// Token: 0x06000581 RID: 1409 RVA: 0x00032CE4 File Offset: 0x00030EE4
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

		// Token: 0x06000582 RID: 1410 RVA: 0x00032D50 File Offset: 0x00030F50
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

		// Token: 0x06000583 RID: 1411 RVA: 0x00032DC4 File Offset: 0x00030FC4
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
				Character targetCharacter = (client != null) ? client.Character : null;
				if (targetCharacter == null)
				{
					DebugConsole.ThrowError("Failed to deliver items directly to a character (" + ((client == null) ? "client was null" : ("client " + client.Name + " is not controlling a character")) + ").", null, null, false, false);
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

		// Token: 0x06000584 RID: 1412 RVA: 0x00033108 File Offset: 0x00031308
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

		// Token: 0x06000585 RID: 1413 RVA: 0x00033190 File Offset: 0x00031390
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

		// Token: 0x06000586 RID: 1414 RVA: 0x00033218 File Offset: 0x00031418
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

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000587 RID: 1415 RVA: 0x000332D0 File Offset: 0x000314D0
		private Dictionary<ItemPrefab, int> UndeterminedSoldEntities { get; } = new Dictionary<ItemPrefab, int>();

		// Token: 0x06000588 RID: 1416 RVA: 0x000332D8 File Offset: 0x000314D8
		public IEnumerable<Item> GetSellableItemsFromSub()
		{
			if (Submarine.MainSub == null)
			{
				return new List<Item>();
			}
			IEnumerable<CargoManager.SoldEntity> confirmedSoldEntities = Enumerable.Empty<CargoManager.SoldEntity>();
			this.UndeterminedSoldEntities.Clear();
			return (from it in CargoManager.FindAllSellableItems()
			where this.IsItemSellable(it, confirmedSoldEntities)
			select it).ToList<Item>();
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x00033330 File Offset: 0x00031530
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

		// Token: 0x0600058A RID: 1418 RVA: 0x00033370 File Offset: 0x00031570
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
				}) && CargoManager.<FindAllSellableItems>g__ItemAndAllContainersInteractable|65_2(item) && CargoManager.<FindAllSellableItems>g__AllContainersAllowSellingItems|65_1(item);
			}).Distinct<Item>();
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x000333C0 File Offset: 0x000315C0
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

		// Token: 0x0600058C RID: 1420 RVA: 0x0003357F File Offset: 0x0003177F
		public static IEnumerable<Hull> FindCargoRooms(IEnumerable<Submarine> subs)
		{
			return subs.SelectMany((Submarine s) => CargoManager.FindCargoRooms(s));
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x000335A8 File Offset: 0x000317A8
		public static IEnumerable<Hull> FindCargoRooms(Submarine sub)
		{
			return (from wp in WayPoint.WayPointList
			where wp.Submarine == sub && wp.SpawnType == SpawnType.Cargo
			select wp.CurrentHull).Distinct<Hull>();
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x00033604 File Offset: 0x00031804
		public static IEnumerable<Item> FilterCargoCrates(IEnumerable<Item> items, Func<Item, bool> conditional = null)
		{
			return from it in items
			where it.HasTag(Tags.Crate) && !it.NonInteractable && !it.NonPlayerTeamInteractable && !it.IsHidden && !it.Removed && (conditional == null || conditional(it))
			select it;
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00033630 File Offset: 0x00031830
		public static IEnumerable<ItemContainer> FindReusableCargoContainers(IEnumerable<Submarine> subs, IEnumerable<Hull> cargoRooms = null)
		{
			return from it in CargoManager.FilterCargoCrates(Item.ItemList, (Item it) => subs.Contains(it.Submarine) && !it.HasTag(Tags.CargoMissionItem) && (cargoRooms == null || cargoRooms.Contains(it.CurrentHull)))
			select it.GetComponent<ItemContainer>() into c
			where c != null
			select c;
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x000336B0 File Offset: 0x000318B0
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
					if (GameMain.Server != null)
					{
						Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(itemContainer.Item));
					}
				}
			}
			return itemContainer;
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x000337F0 File Offset: 0x000319F0
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
				foreach (Client client in GameMain.Server.ConnectedClients)
				{
					if (client.TeamID == CharacterTeamType.None || client.TeamID == sub.TeamID)
					{
						ChatMessage msg = ChatMessage.Create("", TextManager.ContainsTag(cargoRoom.RoomName) ? ("CargoSpawnNotification~[roomname]=§" + cargoRoom.RoomName) : ("CargoSpawnNotification~[roomname]=" + cargoRoom.RoomName), ChatMessageType.ServerMessageBoxInGame, null, null, PlayerConnectionChangeType.None, null);
						msg.IconStyle = "StoreShoppingCrateIcon";
						GameMain.Server.SendDirectChatMessage(msg, client);
					}
				}
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
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
					}
					(((itemContainer != null) ? itemContainer.Item : null) ?? item).AssignCampaignInteractionType(CampaignMode.InteractionType.Cargo, null);
				}
			}
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x00033A6C File Offset: 0x00031C6C
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
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
					}
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

		// Token: 0x06000593 RID: 1427 RVA: 0x00033BE8 File Offset: 0x00031DE8
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

		// Token: 0x06000594 RID: 1428 RVA: 0x00033C34 File Offset: 0x00031E34
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

		// Token: 0x06000595 RID: 1429 RVA: 0x00033CD0 File Offset: 0x00031ED0
		public void InitPurchasedIDCards()
		{
			foreach (ValueTuple<PurchasedItem, IdCard> valueTuple in this.purchasedIDCards)
			{
				PurchasedItem purchased = valueTuple.Item1;
				IdCard idCard = valueTuple.Item2;
				CargoManager.InitPurchasedIDCard(purchased, idCard);
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x00033D30 File Offset: 0x00031F30
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

		// Token: 0x06000597 RID: 1431 RVA: 0x00033DA8 File Offset: 0x00031FA8
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

		// Token: 0x06000598 RID: 1432 RVA: 0x00033E9C File Offset: 0x0003209C
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

		// Token: 0x06000599 RID: 1433 RVA: 0x0003402C File Offset: 0x0003222C
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

		// Token: 0x0600059A RID: 1434 RVA: 0x00034138 File Offset: 0x00032338
		[CompilerGenerated]
		internal static bool <FindAllSellableItems>g__AllContainersAllowSellingItems|65_1(Item item)
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

		// Token: 0x0600059B RID: 1435 RVA: 0x00034193 File Offset: 0x00032393
		[CompilerGenerated]
		internal static bool <FindAllSellableItems>g__ItemAndAllContainersInteractable|65_2(Item item)
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

		// Token: 0x040002C1 RID: 705
		public const int MaxQuantity = 100;

		// Token: 0x040002C7 RID: 711
		private readonly CampaignMode campaign;

		// Token: 0x040002C8 RID: 712
		public readonly NamedEvent<CargoManager> OnItemsInBuyCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x040002C9 RID: 713
		public readonly NamedEvent<CargoManager> OnItemsInSellCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x040002CA RID: 714
		public readonly NamedEvent<CargoManager> OnItemsInSellFromSubCrateChanged = new NamedEvent<CargoManager>();

		// Token: 0x040002CB RID: 715
		public readonly NamedEvent<CargoManager> OnPurchasedItemsChanged = new NamedEvent<CargoManager>();

		// Token: 0x040002CC RID: 716
		public readonly NamedEvent<CargoManager> OnSoldItemsChanged = new NamedEvent<CargoManager>();

		// Token: 0x040002CE RID: 718
		[TupleElementNames(new string[]
		{
			"purchaseInfo",
			"idCard"
		})]
		private readonly List<ValueTuple<PurchasedItem, IdCard>> purchasedIDCards = new List<ValueTuple<PurchasedItem, IdCard>>();

		// Token: 0x02000612 RID: 1554
		private class SoldEntity
		{
			// Token: 0x170013D8 RID: 5080
			// (get) Token: 0x06004D15 RID: 19733 RVA: 0x001DED18 File Offset: 0x001DCF18
			// (set) Token: 0x06004D16 RID: 19734 RVA: 0x001DED20 File Offset: 0x001DCF20
			public Item Item { get; private set; }

			// Token: 0x170013D9 RID: 5081
			// (get) Token: 0x06004D17 RID: 19735 RVA: 0x001DED29 File Offset: 0x001DCF29
			public ItemPrefab ItemPrefab { get; }

			// Token: 0x170013DA RID: 5082
			// (get) Token: 0x06004D18 RID: 19736 RVA: 0x001DED31 File Offset: 0x001DCF31
			// (set) Token: 0x06004D19 RID: 19737 RVA: 0x001DED39 File Offset: 0x001DCF39
			public CargoManager.SoldEntity.SellStatus Status { get; set; }

			// Token: 0x06004D1A RID: 19738 RVA: 0x001DED42 File Offset: 0x001DCF42
			public SoldEntity(Item item, CargoManager.SoldEntity.SellStatus status)
			{
				this.Item = item;
				this.ItemPrefab = ((item != null) ? item.Prefab : null);
				this.Status = status;
			}

			// Token: 0x06004D1B RID: 19739 RVA: 0x001DED6A File Offset: 0x001DCF6A
			public SoldEntity(ItemPrefab itemPrefab, CargoManager.SoldEntity.SellStatus status)
			{
				this.ItemPrefab = itemPrefab;
				this.Status = status;
			}

			// Token: 0x06004D1C RID: 19740 RVA: 0x001DED80 File Offset: 0x001DCF80
			public void SetItem(Item item)
			{
				if (this.Item != null)
				{
					DebugConsole.LogError("Trying to set SoldEntity.Item, but it's already set!\n" + Environment.StackTrace.CleanupStackTrace(), null, null);
					return;
				}
				this.Item = item;
			}

			// Token: 0x02000E77 RID: 3703
			public enum SellStatus
			{
				// Token: 0x0400426A RID: 17002
				Confirmed,
				// Token: 0x0400426B RID: 17003
				Unconfirmed,
				// Token: 0x0400426C RID: 17004
				Local
			}
		}
	}
}
