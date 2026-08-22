using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000355 RID: 853
	internal class CharacterAbilityApprenticeship : CharacterAbility
	{
		// Token: 0x060032EB RID: 13035 RVA: 0x00157FE4 File Offset: 0x001561E4
		public CharacterAbilityApprenticeship(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.ignoreAbilitySkillGain = abilityElement.GetAttributeBool("ignoreabilityskillgain", true);
		}

		// Token: 0x060032EC RID: 13036 RVA: 0x00158000 File Offset: 0x00156200
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilitySkillGain abilitySkillGain = abilityObject as AbilitySkillGain;
			if (abilitySkillGain != null && abilitySkillGain.Character != base.Character)
			{
				if (this.ignoreAbilitySkillGain && abilitySkillGain.GainedFromAbility)
				{
					return;
				}
				CharacterInfo info = base.Character.Info;
				if (info == null)
				{
					return;
				}
				info.IncreaseSkillLevel(abilitySkillGain.SkillIdentifier, 1f, true, false);
			}
		}

		// Token: 0x0400193C RID: 6460
		private readonly bool ignoreAbilitySkillGain;
	}
}
