using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041B RID: 1051
	internal class CharacterAbilityApprenticeship : CharacterAbility
	{
		// Token: 0x06004725 RID: 18213 RVA: 0x0026FE64 File Offset: 0x0026E064
		public CharacterAbilityApprenticeship(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.ignoreAbilitySkillGain = abilityElement.GetAttributeBool("ignoreabilityskillgain", true);
		}

		// Token: 0x06004726 RID: 18214 RVA: 0x0026FE80 File Offset: 0x0026E080
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

		// Token: 0x040024FD RID: 9469
		private readonly bool ignoreAbilitySkillGain;
	}
}
