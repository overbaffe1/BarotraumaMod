using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E1 RID: 1249
	internal sealed class AbilityRepairable : AbilityObject, IAbilityItem
	{
		// Token: 0x17001300 RID: 4864
		// (get) Token: 0x060046D2 RID: 18130 RVA: 0x001C464C File Offset: 0x001C284C
		// (set) Token: 0x060046D3 RID: 18131 RVA: 0x001C4654 File Offset: 0x001C2854
		public Item Item { get; set; }

		// Token: 0x060046D4 RID: 18132 RVA: 0x001C465D File Offset: 0x001C285D
		public AbilityRepairable(Item item)
		{
			this.Item = item;
		}
	}
}
