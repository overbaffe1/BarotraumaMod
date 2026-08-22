using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000427 RID: 1063
	internal class CharacterAbilityGroupEffect : CharacterAbilityGroup
	{
		// Token: 0x06004757 RID: 18263 RVA: 0x002714CD File Offset: 0x0026F6CD
		public CharacterAbilityGroupEffect(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup) : base(abilityEffectType, characterTalent, abilityElementGroup)
		{
		}

		// Token: 0x06004758 RID: 18264 RVA: 0x002714D8 File Offset: 0x0026F6D8
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

		// Token: 0x17001237 RID: 4663
		// (get) Token: 0x06004759 RID: 18265 RVA: 0x00271570 File Offset: 0x0026F770
		private bool IsOverTriggerCount
		{
			get
			{
				return this.timesTriggered >= this.maxTriggerCount;
			}
		}

		// Token: 0x0600475A RID: 18266 RVA: 0x00271584 File Offset: 0x0026F784
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
