using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C5 RID: 1221
	internal class AbilityItemContainer : AbilityObject, IAbilityItem
	{
		// Token: 0x060045E7 RID: 17895 RVA: 0x001BFE52 File Offset: 0x001BE052
		public AbilityItemContainer(Item item)
		{
			this.Item = item;
		}

		// Token: 0x170012BC RID: 4796
		// (get) Token: 0x060045E8 RID: 17896 RVA: 0x001BFE61 File Offset: 0x001BE061
		// (set) Token: 0x060045E9 RID: 17897 RVA: 0x001BFE69 File Offset: 0x001BE069
		public Item Item { get; set; }
	}
}
