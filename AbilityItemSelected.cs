using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001BA RID: 442
	internal class AbilityItemSelected : AbilityObject, IAbilityItem
	{
		// Token: 0x0600312E RID: 12590 RVA: 0x00203B1C File Offset: 0x00201D1C
		public AbilityItemSelected(Item item)
		{
			this.Item = item;
		}

		// Token: 0x17000CE1 RID: 3297
		// (get) Token: 0x0600312F RID: 12591 RVA: 0x00203B2B File Offset: 0x00201D2B
		// (set) Token: 0x06003130 RID: 12592 RVA: 0x00203B33 File Offset: 0x00201D33
		public Item Item { get; set; }
	}
}
