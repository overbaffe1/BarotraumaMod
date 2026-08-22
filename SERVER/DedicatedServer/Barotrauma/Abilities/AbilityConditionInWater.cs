using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000313 RID: 787
	internal class AbilityConditionInWater : AbilityConditionDataless
	{
		// Token: 0x06003218 RID: 12824 RVA: 0x0015446E File Offset: 0x0015266E
		public AbilityConditionInWater(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003219 RID: 12825 RVA: 0x00154478 File Offset: 0x00152678
		protected override bool MatchesConditionSpecific()
		{
			return this.character.InWater;
		}
	}
}
