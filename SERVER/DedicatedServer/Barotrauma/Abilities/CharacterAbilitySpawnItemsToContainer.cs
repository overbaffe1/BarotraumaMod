using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000352 RID: 850
	internal class CharacterAbilitySpawnItemsToContainer : CharacterAbility
	{
		// Token: 0x060032E1 RID: 13025 RVA: 0x00157BBC File Offset: 0x00155DBC
		public CharacterAbilitySpawnItemsToContainer(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statusEffects = CharacterAbilityGroup.ParseStatusEffects(base.CharacterTalent, abilityElement.GetChildElement("statuseffects"));
			this.randomChance = abilityElement.GetAttributeFloat("randomchance", 1f);
			this.oncePerContainer = abilityElement.GetAttributeBool("oncepercontainer", false);
		}

		// Token: 0x060032E2 RID: 13026 RVA: 0x00157C20 File Offset: 0x00155E20
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityItem abilityItem = abilityObject as IAbilityItem;
			Item item = (abilityItem != null) ? abilityItem.Item : null;
			if (item != null)
			{
				if (this.oncePerContainer)
				{
					if (this.openedContainers.Contains(item))
					{
						return;
					}
					this.openedContainers.Add(item);
				}
				if (this.randomChance < Rand.Range(0f, 1f, Rand.RandSync.Unsynced))
				{
					return;
				}
				using (List<StatusEffect>.Enumerator enumerator = this.statusEffects.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StatusEffect statusEffect = enumerator.Current;
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, item, item, null);
					}
					return;
				}
			}
			base.LogAbilityObjectMismatch();
		}

		// Token: 0x04001931 RID: 6449
		private readonly List<StatusEffect> statusEffects;

		// Token: 0x04001932 RID: 6450
		private readonly List<Item> openedContainers = new List<Item>();

		// Token: 0x04001933 RID: 6451
		private readonly float randomChance;

		// Token: 0x04001934 RID: 6452
		private readonly bool oncePerContainer;
	}
}
