using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CC RID: 972
	internal abstract class AbilityConditionDataless : AbilityCondition
	{
		// Token: 0x06004633 RID: 17971 RVA: 0x0026BAE1 File Offset: 0x00269CE1
		public AbilityConditionDataless(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004634 RID: 17972
		protected abstract bool MatchesConditionSpecific();

		// Token: 0x06004635 RID: 17973 RVA: 0x0026BAEB File Offset: 0x00269CEB
		public override bool MatchesCondition()
		{
			if (!this.invert)
			{
				return this.MatchesConditionSpecific();
			}
			return !this.MatchesConditionSpecific();
		}

		// Token: 0x06004636 RID: 17974 RVA: 0x0026BB05 File Offset: 0x00269D05
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
