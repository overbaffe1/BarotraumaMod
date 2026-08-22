using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CB RID: 971
	internal class AbilityConditionCrouched : AbilityConditionDataless
	{
		// Token: 0x06004631 RID: 17969 RVA: 0x0026BAAC File Offset: 0x00269CAC
		public AbilityConditionCrouched(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004632 RID: 17970 RVA: 0x0026BAB8 File Offset: 0x00269CB8
		protected override bool MatchesConditionSpecific()
		{
			HumanoidAnimController humanoidAnimController = this.character.AnimController as HumanoidAnimController;
			return humanoidAnimController != null && humanoidAnimController.Crouching;
		}
	}
}
