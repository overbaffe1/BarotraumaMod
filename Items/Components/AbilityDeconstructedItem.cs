using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F6 RID: 1526
	internal class AbilityDeconstructedItem : AbilityObject, IAbilityItem, IAbilityCharacter
	{
		// Token: 0x060063AA RID: 25514 RVA: 0x0033E235 File Offset: 0x0033C435
		public AbilityDeconstructedItem(Item item, Character character)
		{
			this.Item = item;
			this.Character = character;
		}

		// Token: 0x1700193B RID: 6459
		// (get) Token: 0x060063AB RID: 25515 RVA: 0x0033E24B File Offset: 0x0033C44B
		// (set) Token: 0x060063AC RID: 25516 RVA: 0x0033E253 File Offset: 0x0033C453
		public Item Item { get; set; }

		// Token: 0x1700193C RID: 6460
		// (get) Token: 0x060063AD RID: 25517 RVA: 0x0033E25C File Offset: 0x0033C45C
		// (set) Token: 0x060063AE RID: 25518 RVA: 0x0033E264 File Offset: 0x0033C464
		public Character Character { get; set; }
	}
}
