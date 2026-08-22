using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000362 RID: 866
	internal class CharacterAbilityGroupInterval : CharacterAbilityGroup
	{
		// Token: 0x17000E48 RID: 3656
		// (get) Token: 0x06003321 RID: 13089 RVA: 0x00159760 File Offset: 0x00157960
		// (set) Token: 0x06003322 RID: 13090 RVA: 0x00159768 File Offset: 0x00157968
		public float TimeSinceLastUpdate { get; private set; }

		// Token: 0x06003323 RID: 13091 RVA: 0x00159771 File Offset: 0x00157971
		public CharacterAbilityGroupInterval(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup) : base(abilityEffectType, characterTalent, abilityElementGroup)
		{
			this.interval = abilityElementGroup.GetAttributeFloat("interval", 0f);
			this.effectDelay = abilityElementGroup.GetAttributeFloat("effectdelay", 0f);
		}

		// Token: 0x06003324 RID: 13092 RVA: 0x001597A8 File Offset: 0x001579A8
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

		// Token: 0x06003325 RID: 13093 RVA: 0x001598B4 File Offset: 0x00157AB4
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

		// Token: 0x0400195E RID: 6494
		private readonly float interval;

		// Token: 0x04001960 RID: 6496
		private readonly float effectDelay;

		// Token: 0x04001961 RID: 6497
		private float effectDelayTimer;
	}
}
