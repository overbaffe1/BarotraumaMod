using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004BC RID: 1212
	internal class AbilityItemPickingTime : AbilityObject, IAbilityValue, IAbilityItemPrefab
	{
		// Token: 0x060044F4 RID: 17652 RVA: 0x001B9A72 File Offset: 0x001B7C72
		public AbilityItemPickingTime(float pickingTime, ItemPrefab itemPrefab)
		{
			this.Value = pickingTime;
			this.ItemPrefab = itemPrefab;
		}

		// Token: 0x17001266 RID: 4710
		// (get) Token: 0x060044F5 RID: 17653 RVA: 0x001B9A88 File Offset: 0x001B7C88
		// (set) Token: 0x060044F6 RID: 17654 RVA: 0x001B9A90 File Offset: 0x001B7C90
		public float Value { get; set; }

		// Token: 0x17001267 RID: 4711
		// (get) Token: 0x060044F7 RID: 17655 RVA: 0x001B9A99 File Offset: 0x001B7C99
		// (set) Token: 0x060044F8 RID: 17656 RVA: 0x001B9AA1 File Offset: 0x001B7CA1
		public ItemPrefab ItemPrefab { get; set; }
	}
}
