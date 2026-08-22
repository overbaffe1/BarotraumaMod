using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F8 RID: 1528
	internal class AbilityItemDeconstructedInventory : AbilityObject, IAbilityItem, IAbilityItemPrefab
	{
		// Token: 0x060063B4 RID: 25524 RVA: 0x0033E2A5 File Offset: 0x0033C4A5
		public AbilityItemDeconstructedInventory(ItemPrefab itemPrefab, Item item)
		{
			this.ItemPrefab = itemPrefab;
			this.Item = item;
		}

		// Token: 0x1700193F RID: 6463
		// (get) Token: 0x060063B5 RID: 25525 RVA: 0x0033E2BB File Offset: 0x0033C4BB
		// (set) Token: 0x060063B6 RID: 25526 RVA: 0x0033E2C3 File Offset: 0x0033C4C3
		public ItemPrefab ItemPrefab { get; set; }

		// Token: 0x17001940 RID: 6464
		// (get) Token: 0x060063B7 RID: 25527 RVA: 0x0033E2CC File Offset: 0x0033C4CC
		// (set) Token: 0x060063B8 RID: 25528 RVA: 0x0033E2D4 File Offset: 0x0033C4D4
		public Item Item { get; set; }
	}
}
