using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x02000422 RID: 1058
	internal class CharacterAbilityRegenerateLoot : CharacterAbility
	{
		// Token: 0x06004737 RID: 18231 RVA: 0x002703FC File Offset: 0x0026E5FC
		public CharacterAbilityRegenerateLoot(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.randomChance = abilityElement.GetAttributeFloat("randomChance", 1f);
			this.randomChancePerItem = abilityElement.GetAttributeFloat("randomChancePerItem", 1f);
		}

		// Token: 0x06004738 RID: 18232 RVA: 0x00270454 File Offset: 0x0026E654
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityItem abilityItem = abilityObject as IAbilityItem;
			Item item = (abilityItem != null) ? abilityItem.Item : null;
			if (item == null)
			{
				return;
			}
			if (this.openedContainers.Contains(item))
			{
				return;
			}
			this.openedContainers.Add(item);
			if (this.randomChance < Rand.Range(0f, 1f, Rand.RandSync.Unsynced))
			{
				return;
			}
			ItemContainer itemContainer = item.GetComponent<ItemContainer>();
			if (itemContainer != null)
			{
				AutoItemPlacer.RegenerateLoot(item.Submarine, itemContainer, 1f - this.randomChancePerItem);
			}
		}

		// Token: 0x0400250C RID: 9484
		private readonly float randomChance;

		// Token: 0x0400250D RID: 9485
		private readonly float randomChancePerItem = 1f;

		// Token: 0x0400250E RID: 9486
		private readonly HashSet<Item> openedContainers = new HashSet<Item>();
	}
}
