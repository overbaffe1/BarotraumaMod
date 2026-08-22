using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F0 RID: 1520
	internal class AbilityRangedWeapon : AbilityObject, IAbilityItem
	{
		// Token: 0x06006383 RID: 25475 RVA: 0x0033D751 File Offset: 0x0033B951
		public AbilityRangedWeapon(Item item)
		{
			this.Item = item;
		}

		// Token: 0x1700192D RID: 6445
		// (get) Token: 0x06006384 RID: 25476 RVA: 0x0033D760 File Offset: 0x0033B960
		// (set) Token: 0x06006385 RID: 25477 RVA: 0x0033D768 File Offset: 0x0033B968
		public Item Item { get; set; }
	}
}
