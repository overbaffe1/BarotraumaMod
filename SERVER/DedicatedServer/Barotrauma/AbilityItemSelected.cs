using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B8 RID: 184
	internal class AbilityItemSelected : AbilityObject, IAbilityItem
	{
		// Token: 0x0600156B RID: 5483 RVA: 0x000B8D08 File Offset: 0x000B6F08
		public AbilityItemSelected(Item item)
		{
			this.Item = item;
		}

		// Token: 0x1700063B RID: 1595
		// (get) Token: 0x0600156C RID: 5484 RVA: 0x000B8D17 File Offset: 0x000B6F17
		// (set) Token: 0x0600156D RID: 5485 RVA: 0x000B8D1F File Offset: 0x000B6F1F
		public Item Item { get; set; }
	}
}
