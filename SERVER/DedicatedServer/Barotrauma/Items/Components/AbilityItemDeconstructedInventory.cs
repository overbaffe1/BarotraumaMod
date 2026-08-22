using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004CB RID: 1227
	internal class AbilityItemDeconstructedInventory : AbilityObject, IAbilityItem, IAbilityItemPrefab
	{
		// Token: 0x0600460E RID: 17934 RVA: 0x001C0151 File Offset: 0x001BE351
		public AbilityItemDeconstructedInventory(ItemPrefab itemPrefab, Item item)
		{
			this.ItemPrefab = itemPrefab;
			this.Item = item;
		}

		// Token: 0x170012CB RID: 4811
		// (get) Token: 0x0600460F RID: 17935 RVA: 0x001C0167 File Offset: 0x001BE367
		// (set) Token: 0x06004610 RID: 17936 RVA: 0x001C016F File Offset: 0x001BE36F
		public ItemPrefab ItemPrefab { get; set; }

		// Token: 0x170012CC RID: 4812
		// (get) Token: 0x06004611 RID: 17937 RVA: 0x001C0178 File Offset: 0x001BE378
		// (set) Token: 0x06004612 RID: 17938 RVA: 0x001C0180 File Offset: 0x001BE380
		public Item Item { get; set; }
	}
}
