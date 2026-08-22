using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000306 RID: 774
	internal abstract class AbilityConditionDataless : AbilityCondition
	{
		// Token: 0x060031F9 RID: 12793 RVA: 0x00153C61 File Offset: 0x00151E61
		public AbilityConditionDataless(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031FA RID: 12794
		protected abstract bool MatchesConditionSpecific();

		// Token: 0x060031FB RID: 12795 RVA: 0x00153C6B File Offset: 0x00151E6B
		public override bool MatchesCondition()
		{
			if (!this.invert)
			{
				return this.MatchesConditionSpecific();
			}
			return !this.MatchesConditionSpecific();
		}

		// Token: 0x060031FC RID: 12796 RVA: 0x00153C85 File Offset: 0x00151E85
		public override bool MatchesCondition(AbilityObject abilityObject)
		{
			if (!this.invert)
			{
				return this.MatchesConditionSpecific();
			}
			return !this.MatchesConditionSpecific();
		}
	}
}
