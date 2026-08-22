using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DF RID: 991
	internal class AbilityConditionRagdolled : AbilityConditionDataless
	{
		// Token: 0x0600465F RID: 18015 RVA: 0x0026C5FE File Offset: 0x0026A7FE
		public AbilityConditionRagdolled(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004660 RID: 18016 RVA: 0x0026C608 File Offset: 0x0026A808
		protected override bool MatchesConditionSpecific()
		{
			return (this.character.IsRagdolled && !this.character.AnimController.IsHangingWithRope) || this.character.Stun > 0f || this.character.IsIncapacitated;
		}
	}
}
