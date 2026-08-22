using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200018F RID: 399
	internal class CheckPurchasedItemsAction : BinaryOptionAction
	{
		// Token: 0x170008B3 RID: 2227
		// (get) Token: 0x06001E65 RID: 7781 RVA: 0x000D592D File Offset: 0x000D3B2D
		// (set) Token: 0x06001E66 RID: 7782 RVA: 0x000D5935 File Offset: 0x000D3B35
		[Serialize(CheckPurchasedItemsAction.TransactionType.Purchased, IsPropertySaveable.Yes, "Do the items need to have been purchased or sold?", "", false)]
		public CheckPurchasedItemsAction.TransactionType Type { get; set; }

		// Token: 0x170008B4 RID: 2228
		// (get) Token: 0x06001E67 RID: 7783 RVA: 0x000D593E File Offset: 0x000D3B3E
		// (set) Token: 0x06001E68 RID: 7784 RVA: 0x000D5946 File Offset: 0x000D3B46
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item that must have been purchased or sold.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x170008B5 RID: 2229
		// (get) Token: 0x06001E69 RID: 7785 RVA: 0x000D594F File Offset: 0x000D3B4F
		// (set) Token: 0x06001E6A RID: 7786 RVA: 0x000D5957 File Offset: 0x000D3B57
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must have been purchased or sold.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x170008B6 RID: 2230
		// (get) Token: 0x06001E6B RID: 7787 RVA: 0x000D5960 File Offset: 0x000D3B60
		// (set) Token: 0x06001E6C RID: 7788 RVA: 0x000D5968 File Offset: 0x000D3B68
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching items that must have been purchased or sold.", "", false)]
		public int MinCount { get; set; }

		// Token: 0x06001E6D RID: 7789 RVA: 0x000D5971 File Offset: 0x000D3B71
		public CheckPurchasedItemsAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.MinCount = Math.Max(this.MinCount, 1);
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x000D5990 File Offset: 0x000D3B90
		protected override bool? DetermineSuccess()
		{
			Identifier identifier = this.ItemIdentifier;
			if (identifier.IsEmpty)
			{
				identifier = this.ItemTag;
				if (identifier.IsEmpty)
				{
					return new bool?(false);
				}
			}
			GameSession gameSession = GameMain.GameSession;
			CargoManager cargoManager2;
			if (gameSession == null)
			{
				cargoManager2 = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				cargoManager2 = ((campaign != null) ? campaign.CargoManager : null);
			}
			CargoManager cargoManager = cargoManager2;
			if (cargoManager == null)
			{
				return new bool?(false);
			}
			if (this.Type == CheckPurchasedItemsAction.TransactionType.Purchased)
			{
				int totalPurchased = 0;
				using (Dictionary<Identifier, List<PurchasedItem>>.Enumerator enumerator = cargoManager.PurchasedItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<Identifier, List<PurchasedItem>> keyValuePair = enumerator.Current;
						List<PurchasedItem> list;
						keyValuePair.Deconstruct(out identifier, out list);
						List<PurchasedItem> items = list;
						identifier = this.ItemIdentifier;
						if (!identifier.IsEmpty)
						{
							int num = totalPurchased;
							PurchasedItem purchasedItem = items.Find(delegate(PurchasedItem i)
							{
								Identifier itemPrefabIdentifier = i.ItemPrefabIdentifier;
								Identifier itemIdentifier = this.ItemIdentifier;
								return itemPrefabIdentifier == itemIdentifier;
							});
							totalPurchased = num + ((purchasedItem != null) ? purchasedItem.Quantity : 0);
						}
						else
						{
							identifier = this.ItemTag;
							if (!identifier.IsEmpty)
							{
								foreach (PurchasedItem item in items)
								{
									if (item.ItemPrefab.Tags.Contains(this.ItemTag))
									{
										totalPurchased += item.Quantity;
									}
								}
							}
						}
						if (totalPurchased >= this.MinCount)
						{
							return new bool?(true);
						}
					}
					goto IL_203;
				}
			}
			int totalSold = 0;
			foreach (KeyValuePair<Identifier, List<SoldItem>> keyValuePair2 in cargoManager.SoldItems)
			{
				List<SoldItem> list2;
				keyValuePair2.Deconstruct(out identifier, out list2);
				List<SoldItem> items2 = list2;
				identifier = this.ItemIdentifier;
				if (!identifier.IsEmpty)
				{
					totalSold += items2.Count(delegate(SoldItem i)
					{
						Prefab itemPrefab = i.ItemPrefab;
						Identifier itemIdentifier = this.ItemIdentifier;
						return itemPrefab.Identifier == itemIdentifier;
					});
				}
				else
				{
					identifier = this.ItemTag;
					if (!identifier.IsEmpty)
					{
						totalSold += items2.Count((SoldItem i) => i.ItemPrefab.Tags.Contains(this.ItemTag));
					}
				}
				if (totalSold >= this.MinCount)
				{
					return new bool?(true);
				}
			}
			IL_203:
			return new bool?(false);
		}

		// Token: 0x0200090C RID: 2316
		public enum TransactionType
		{
			// Token: 0x040031DF RID: 12767
			Purchased,
			// Token: 0x040031E0 RID: 12768
			Sold
		}
	}
}
