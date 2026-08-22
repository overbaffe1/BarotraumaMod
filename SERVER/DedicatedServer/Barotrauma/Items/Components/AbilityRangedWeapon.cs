using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004BF RID: 1215
	internal class AbilityRangedWeapon : AbilityObject, IAbilityItem
	{
		// Token: 0x0600452D RID: 17709 RVA: 0x001BA86E File Offset: 0x001B8A6E
		public AbilityRangedWeapon(Item item)
		{
			this.Item = item;
		}

		// Token: 0x1700127C RID: 4732
		// (get) Token: 0x0600452E RID: 17710 RVA: 0x001BA87D File Offset: 0x001B8A7D
		// (set) Token: 0x0600452F RID: 17711 RVA: 0x001BA885 File Offset: 0x001B8A85
		public Item Item { get; set; }
	}
}
