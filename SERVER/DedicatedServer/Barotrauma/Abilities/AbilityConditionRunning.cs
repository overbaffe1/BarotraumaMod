using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200031A RID: 794
	internal class AbilityConditionRunning : AbilityConditionDataless
	{
		// Token: 0x06003227 RID: 12839 RVA: 0x001547C8 File Offset: 0x001529C8
		public AbilityConditionRunning(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003228 RID: 12840 RVA: 0x001547D4 File Offset: 0x001529D4
		protected override bool MatchesConditionSpecific()
		{
			HumanoidAnimController animController = this.character.AnimController as HumanoidAnimController;
			return animController != null && animController.IsMovingFast;
		}
	}
}
