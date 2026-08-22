using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030F RID: 783
	internal class AbilityConditionHasVelocity : AbilityConditionDataless
	{
		// Token: 0x0600320F RID: 12815 RVA: 0x001542D7 File Offset: 0x001524D7
		public AbilityConditionHasVelocity(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.velocity = conditionElement.GetAttributeFloat("velocity", 0f);
		}

		// Token: 0x06003210 RID: 12816 RVA: 0x001542F8 File Offset: 0x001524F8
		protected override bool MatchesConditionSpecific()
		{
			return this.character.AnimController.Collider.LinearVelocity.LengthSquared() > this.velocity * this.velocity;
		}

		// Token: 0x040018B3 RID: 6323
		private readonly float velocity;
	}
}
