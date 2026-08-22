using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D5 RID: 981
	internal class AbilityConditionHasVelocity : AbilityConditionDataless
	{
		// Token: 0x06004649 RID: 17993 RVA: 0x0026C157 File Offset: 0x0026A357
		public AbilityConditionHasVelocity(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.velocity = conditionElement.GetAttributeFloat("velocity", 0f);
		}

		// Token: 0x0600464A RID: 17994 RVA: 0x0026C178 File Offset: 0x0026A378
		protected override bool MatchesConditionSpecific()
		{
			return this.character.AnimController.Collider.LinearVelocity.LengthSquared() > this.velocity * this.velocity;
		}

		// Token: 0x04002474 RID: 9332
		private readonly float velocity;
	}
}
