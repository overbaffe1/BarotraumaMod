using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000282 RID: 642
	internal class CheckPurchasedItemsAction : BinaryOptionAction
	{
		// Token: 0x17000F07 RID: 3847
		// (get) Token: 0x0600392F RID: 14639 RVA: 0x0021AE29 File Offset: 0x00219029
		// (set) Token: 0x06003930 RID: 14640 RVA: 0x0021AE31 File Offset: 0x00219031
		[Serialize(CheckPurchasedItemsAction.TransactionType.Purchased, IsPropertySaveable.Yes, "Do the items need to have been purchased or sold?", "", false)]
		public CheckPurchasedItemsAction.TransactionType Type { get; set; }

		// Token: 0x17000F08 RID: 3848
		// (get) Token: 0x06003931 RID: 14641 RVA: 0x0021AE3A File Offset: 0x0021903A
		// (set) Token: 0x06003932 RID: 14642 RVA: 0x0021AE42 File Offset: 0x00219042
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item that must have been purchased or sold.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x17000F09 RID: 3849
		// (get) Token: 0x06003933 RID: 14643 RVA: 0x0021AE4B File Offset: 0x0021904B
		// (set) Token: 0x06003934 RID: 14644 RVA: 0x0021AE53 File Offset: 0x00219053
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must have been purchased or sold.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000F0A RID: 3850
		// (get) Token: 0x06003935 RID: 14645 RVA: 0x0021AE5C File Offset: 0x0021905C
		// (set) Token: 0x06003936 RID: 14646 RVA: 0x0021AE64 File Offset: 0x00219064
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching items that must have been purchased or sold.", "", false)]
		public int MinCount { get; set; }

		// Token: 0x06003937 RID: 14647 RVA: 0x0021AE6D File Offset: 0x0021906D
		public CheckPurchasedItemsAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.MinCount = Math.Max(this.MinCount, 1);
		}

		// Token: 0x06003938 RID: 14648 RVA: 0x0021AE8C File Offset: 0x0021908C
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

		// Token: 0x02000F1D RID: 3869
		public enum TransactionType
		{
			// Token: 0x040054AC RID: 21676
			Purchased,
			// Token: 0x040054AD RID: 21677
			Sold
		}
	}
}
