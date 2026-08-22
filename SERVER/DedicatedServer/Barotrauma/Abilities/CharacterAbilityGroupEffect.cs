using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000361 RID: 865
	internal class CharacterAbilityGroupEffect : CharacterAbilityGroup
	{
		// Token: 0x0600331D RID: 13085 RVA: 0x0015964D File Offset: 0x0015784D
		public CharacterAbilityGroupEffect(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup) : base(abilityEffectType, characterTalent, abilityElementGroup)
		{
		}

		// Token: 0x0600331E RID: 13086 RVA: 0x00159658 File Offset: 0x00157858
		public void CheckAbilityGroup(AbilityObject abilityObject)
		{
			if (!base.IsActive)
			{
				return;
			}
			if (this.IsOverTriggerCount)
			{
				return;
			}
			List<CharacterAbility> abilities = this.IsApplicable(abilityObject) ? this.characterAbilities : this.fallbackAbilities;
			foreach (CharacterAbility characterAbility in abilities)
			{
				if (characterAbility.IsViable())
				{
					characterAbility.ApplyAbilityEffect(abilityObject);
				}
			}
			if (abilities.Count > 0)
			{
				this.timesTriggered++;
			}
		}

		// Token: 0x17000E47 RID: 3655
		// (get) Token: 0x0600331F RID: 13087 RVA: 0x001596F0 File Offset: 0x001578F0
		private bool IsOverTriggerCount
		{
			get
			{
				return this.timesTriggered >= this.maxTriggerCount;
			}
		}

		// Token: 0x06003320 RID: 13088 RVA: 0x00159704 File Offset: 0x00157904
		private bool IsApplicable(AbilityObject abilityObject)
		{
			foreach (AbilityCondition abilityCondition in this.abilityConditions)
			{
				if (!abilityCondition.MatchesCondition(abilityObject))
				{
					return false;
				}
			}
			return true;
		}
	}
}
