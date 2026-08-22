using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000606 RID: 1542
	internal sealed class AbilityRepairable : AbilityObject, IAbilityItem
	{
		// Token: 0x17001946 RID: 6470
		// (get) Token: 0x060063E1 RID: 25569 RVA: 0x0033ECE4 File Offset: 0x0033CEE4
		// (set) Token: 0x060063E2 RID: 25570 RVA: 0x0033ECEC File Offset: 0x0033CEEC
		public Item Item { get; set; }

		// Token: 0x060063E3 RID: 25571 RVA: 0x0033ECF5 File Offset: 0x0033CEF5
		public AbilityRepairable(Item item)
		{
			this.Item = item;
		}
	}
}
