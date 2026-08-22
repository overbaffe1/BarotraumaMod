using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000428 RID: 1064
	internal class CharacterAbilityGroupInterval : CharacterAbilityGroup
	{
		// Token: 0x17001238 RID: 4664
		// (get) Token: 0x0600475B RID: 18267 RVA: 0x002715E0 File Offset: 0x0026F7E0
		// (set) Token: 0x0600475C RID: 18268 RVA: 0x002715E8 File Offset: 0x0026F7E8
		public float TimeSinceLastUpdate { get; private set; }

		// Token: 0x0600475D RID: 18269 RVA: 0x002715F1 File Offset: 0x0026F7F1
		public CharacterAbilityGroupInterval(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup) : base(abilityEffectType, characterTalent, abilityElementGroup)
		{
			this.interval = abilityElementGroup.GetAttributeFloat("interval", 0f);
			this.effectDelay = abilityElementGroup.GetAttributeFloat("effectdelay", 0f);
		}

		// Token: 0x0600475E RID: 18270 RVA: 0x00271628 File Offset: 0x0026F828
		public void UpdateAbilityGroup(float deltaTime)
		{
			if (!base.IsActive)
			{
				return;
			}
			this.TimeSinceLastUpdate += deltaTime;
			if (this.TimeSinceLastUpdate < this.interval)
			{
				return;
			}
			bool conditionsMatched;
			if (this.AllConditionsMatched())
			{
				this.effectDelayTimer += this.TimeSinceLastUpdate;
				bool shouldApplyDelayedEffect = this.effectDelayTimer >= this.effectDelay;
				conditionsMatched = shouldApplyDelayedEffect;
			}
			else
			{
				this.effectDelayTimer = 0f;
				conditionsMatched = false;
			}
			bool hasFallbacks = this.fallbackAbilities.Count > 0;
			List<CharacterAbility> abilitiesToRun = (!conditionsMatched && hasFallbacks) ? this.fallbackAbilities : this.characterAbilities;
			if (hasFallbacks)
			{
				conditionsMatched = true;
			}
			foreach (CharacterAbility characterAbility in abilitiesToRun)
			{
				if (characterAbility.IsViable())
				{
					characterAbility.UpdateCharacterAbility(conditionsMatched, this.TimeSinceLastUpdate);
				}
			}
			if (conditionsMatched)
			{
				this.timesTriggered++;
			}
			this.TimeSinceLastUpdate = 0f;
		}

		// Token: 0x0600475F RID: 18271 RVA: 0x00271734 File Offset: 0x0026F934
		private bool AllConditionsMatched()
		{
			if (this.timesTriggered >= this.maxTriggerCount)
			{
				return false;
			}
			foreach (AbilityCondition abilityCondition in this.abilityConditions)
			{
				if (!abilityCondition.MatchesCondition())
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400251F RID: 9503
		private readonly float interval;

		// Token: 0x04002521 RID: 9505
		private readonly float effectDelay;

		// Token: 0x04002522 RID: 9506
		private float effectDelayTimer;
	}
}
