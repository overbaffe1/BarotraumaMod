using System;
using Barotrauma.Abilities;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C9 RID: 1225
	internal class AbilityDeconstructedItem : AbilityObject, IAbilityItem, IAbilityCharacter
	{
		// Token: 0x06004604 RID: 17924 RVA: 0x001C00E1 File Offset: 0x001BE2E1
		public AbilityDeconstructedItem(Item item, Character character)
		{
			this.Item = item;
			this.Character = character;
		}

		// Token: 0x170012C7 RID: 4807
		// (get) Token: 0x06004605 RID: 17925 RVA: 0x001C00F7 File Offset: 0x001BE2F7
		// (set) Token: 0x06004606 RID: 17926 RVA: 0x001C00FF File Offset: 0x001BE2FF
		public Item Item { get; set; }

		// Token: 0x170012C8 RID: 4808
		// (get) Token: 0x06004607 RID: 17927 RVA: 0x001C0108 File Offset: 0x001BE308
		// (set) Token: 0x06004608 RID: 17928 RVA: 0x001C0110 File Offset: 0x001BE310
		public Character Character { get; set; }
	}
}
