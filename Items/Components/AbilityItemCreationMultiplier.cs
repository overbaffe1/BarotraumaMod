using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F7 RID: 1527
	internal class AbilityItemCreationMultiplier : AbilityObject, IAbilityValue, IAbilityItemPrefab
	{
		// Token: 0x060063AF RID: 25519 RVA: 0x0033E26D File Offset: 0x0033C46D
		public AbilityItemCreationMultiplier(ItemPrefab itemPrefab, float itemAmountMultiplier)
		{
			this.ItemPrefab = itemPrefab;
			this.Value = itemAmountMultiplier;
		}

		// Token: 0x1700193D RID: 6461
		// (get) Token: 0x060063B0 RID: 25520 RVA: 0x0033E283 File Offset: 0x0033C483
		// (set) Token: 0x060063B1 RID: 25521 RVA: 0x0033E28B File Offset: 0x0033C48B
		public ItemPrefab ItemPrefab { get; set; }

		// Token: 0x1700193E RID: 6462
		// (get) Token: 0x060063B2 RID: 25522 RVA: 0x0033E294 File Offset: 0x0033C494
		// (set) Token: 0x060063B3 RID: 25523 RVA: 0x0033E29C File Offset: 0x0033C49C
		public float Value { get; set; }
	}
}
