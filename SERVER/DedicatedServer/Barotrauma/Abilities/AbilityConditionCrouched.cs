using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000305 RID: 773
	internal class AbilityConditionCrouched : AbilityConditionDataless
	{
		// Token: 0x060031F7 RID: 12791 RVA: 0x00153C2C File Offset: 0x00151E2C
		public AbilityConditionCrouched(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031F8 RID: 12792 RVA: 0x00153C38 File Offset: 0x00151E38
		protected override bool MatchesConditionSpecific()
		{
			HumanoidAnimController humanoidAnimController = this.character.AnimController as HumanoidAnimController;
			return humanoidAnimController != null && humanoidAnimController.Crouching;
		}
	}
}
