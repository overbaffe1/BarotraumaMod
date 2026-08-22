using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000418 RID: 1048
	internal class CharacterAbilitySpawnItemsToContainer : CharacterAbility
	{
		// Token: 0x0600471B RID: 18203 RVA: 0x0026FA3C File Offset: 0x0026DC3C
		public CharacterAbilitySpawnItemsToContainer(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statusEffects = CharacterAbilityGroup.ParseStatusEffects(base.CharacterTalent, abilityElement.GetChildElement("statuseffects"));
			this.randomChance = abilityElement.GetAttributeFloat("randomchance", 1f);
			this.oncePerContainer = abilityElement.GetAttributeBool("oncepercontainer", false);
		}

		// Token: 0x0600471C RID: 18204 RVA: 0x0026FAA0 File Offset: 0x0026DCA0
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

		// Token: 0x040024F2 RID: 9458
		private readonly List<StatusEffect> statusEffects;

		// Token: 0x040024F3 RID: 9459
		private readonly List<Item> openedContainers = new List<Item>();

		// Token: 0x040024F4 RID: 9460
		private readonly float randomChance;

		// Token: 0x040024F5 RID: 9461
		private readonly bool oncePerContainer;
	}
}
