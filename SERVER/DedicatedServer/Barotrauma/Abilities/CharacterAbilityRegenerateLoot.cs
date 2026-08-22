using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x0200035C RID: 860
	internal class CharacterAbilityRegenerateLoot : CharacterAbility
	{
		// Token: 0x060032FD RID: 13053 RVA: 0x0015857C File Offset: 0x0015677C
		public CharacterAbilityRegenerateLoot(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.randomChance = abilityElement.GetAttributeFloat("randomChance", 1f);
			this.randomChancePerItem = abilityElement.GetAttributeFloat("randomChancePerItem", 1f);
		}

		// Token: 0x060032FE RID: 13054 RVA: 0x001585D4 File Offset: 0x001567D4
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

		// Token: 0x0400194B RID: 6475
		private readonly float randomChance;

		// Token: 0x0400194C RID: 6476
		private readonly float randomChancePerItem = 1f;

		// Token: 0x0400194D RID: 6477
		private readonly HashSet<Item> openedContainers = new HashSet<Item>();
	}
}
