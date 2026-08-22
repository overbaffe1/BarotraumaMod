using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D8 RID: 984
	internal class AbilityConditionInHull : AbilityConditionDataless
	{
		// Token: 0x06004650 RID: 18000 RVA: 0x0026C2D4 File Offset: 0x0026A4D4
		public AbilityConditionInHull(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004651 RID: 18001 RVA: 0x0026C2DE File Offset: 0x0026A4DE
		protected override bool MatchesConditionSpecific()
		{
			return this.character.CurrentHull != null;
		}
	}
}
