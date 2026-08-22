using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000319 RID: 793
	internal class AbilityConditionRagdolled : AbilityConditionDataless
	{
		// Token: 0x06003225 RID: 12837 RVA: 0x0015477E File Offset: 0x0015297E
		public AbilityConditionRagdolled(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003226 RID: 12838 RVA: 0x00154788 File Offset: 0x00152988
		protected override bool MatchesConditionSpecific()
		{
			return (this.character.IsRagdolled && !this.character.AnimController.IsHangingWithRope) || this.character.Stun > 0f || this.character.IsIncapacitated;
		}
	}
}
