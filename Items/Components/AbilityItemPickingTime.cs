using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005EE RID: 1518
	internal class AbilityItemPickingTime : AbilityObject, IAbilityValue, IAbilityItemPrefab
	{
		// Token: 0x06006372 RID: 25458 RVA: 0x0033D40A File Offset: 0x0033B60A
		public AbilityItemPickingTime(float pickingTime, ItemPrefab itemPrefab)
		{
			this.Value = pickingTime;
			this.ItemPrefab = itemPrefab;
		}

		// Token: 0x17001927 RID: 6439
		// (get) Token: 0x06006373 RID: 25459 RVA: 0x0033D420 File Offset: 0x0033B620
		// (set) Token: 0x06006374 RID: 25460 RVA: 0x0033D428 File Offset: 0x0033B628
		public float Value { get; set; }

		// Token: 0x17001928 RID: 6440
		// (get) Token: 0x06006375 RID: 25461 RVA: 0x0033D431 File Offset: 0x0033B631
		// (set) Token: 0x06006376 RID: 25462 RVA: 0x0033D439 File Offset: 0x0033B639
		public ItemPrefab ItemPrefab { get; set; }
	}
}
