using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D9 RID: 985
	internal class AbilityConditionInWater : AbilityConditionDataless
	{
		// Token: 0x06004652 RID: 18002 RVA: 0x0026C2EE File Offset: 0x0026A4EE
		public AbilityConditionInWater(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004653 RID: 18003 RVA: 0x0026C2F8 File Offset: 0x0026A4F8
		protected override bool MatchesConditionSpecific()
		{
			return this.character.InWater;
		}
	}
}
