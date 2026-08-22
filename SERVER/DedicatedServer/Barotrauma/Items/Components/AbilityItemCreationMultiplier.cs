using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004CA RID: 1226
	internal class AbilityItemCreationMultiplier : AbilityObject, IAbilityValue, IAbilityItemPrefab
	{
		// Token: 0x06004609 RID: 17929 RVA: 0x001C0119 File Offset: 0x001BE319
		public AbilityItemCreationMultiplier(ItemPrefab itemPrefab, float itemAmountMultiplier)
		{
			this.ItemPrefab = itemPrefab;
			this.Value = itemAmountMultiplier;
		}

		// Token: 0x170012C9 RID: 4809
		// (get) Token: 0x0600460A RID: 17930 RVA: 0x001C012F File Offset: 0x001BE32F
		// (set) Token: 0x0600460B RID: 17931 RVA: 0x001C0137 File Offset: 0x001BE337
		public ItemPrefab ItemPrefab { get; set; }

		// Token: 0x170012CA RID: 4810
		// (get) Token: 0x0600460C RID: 17932 RVA: 0x001C0140 File Offset: 0x001BE340
		// (set) Token: 0x0600460D RID: 17933 RVA: 0x001C0148 File Offset: 0x001BE348
		public float Value { get; set; }
	}
}
