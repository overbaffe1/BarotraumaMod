using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003E0 RID: 992
	internal class AbilityConditionRunning : AbilityConditionDataless
	{
		// Token: 0x06004661 RID: 18017 RVA: 0x0026C648 File Offset: 0x0026A848
		public AbilityConditionRunning(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004662 RID: 18018 RVA: 0x0026C654 File Offset: 0x0026A854
		protected override bool MatchesConditionSpecific()
		{
			HumanoidAnimController animController = this.character.AnimController as HumanoidAnimController;
			return animController != null && animController.IsMovingFast;
		}
	}
}
