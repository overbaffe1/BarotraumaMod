using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F3 RID: 1523
	internal class AbilityItemContainer : AbilityObject, IAbilityItem
	{
		// Token: 0x06006392 RID: 25490 RVA: 0x0033DF34 File Offset: 0x0033C134
		public AbilityItemContainer(Item item)
		{
			this.Item = item;
		}

		// Token: 0x17001931 RID: 6449
		// (get) Token: 0x06006393 RID: 25491 RVA: 0x0033DF43 File Offset: 0x0033C143
		// (set) Token: 0x06006394 RID: 25492 RVA: 0x0033DF4B File Offset: 0x0033C14B
		public Item Item { get; set; }
	}
}
