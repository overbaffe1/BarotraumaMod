using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000312 RID: 786
	internal class AbilityConditionInHull : AbilityConditionDataless
	{
		// Token: 0x06003216 RID: 12822 RVA: 0x00154454 File Offset: 0x00152654
		public AbilityConditionInHull(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003217 RID: 12823 RVA: 0x0015445E File Offset: 0x0015265E
		protected override bool MatchesConditionSpecific()
		{
			return this.character.CurrentHull != null;
		}
	}
}
