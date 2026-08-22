using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C5 RID: 965
	internal class AbilityConditionAboveVitality : AbilityConditionDataless
	{
		// Token: 0x06004624 RID: 17956 RVA: 0x0026B7CA File Offset: 0x002699CA
		public AbilityConditionAboveVitality(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.vitalityPercentage = conditionElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x06004625 RID: 17957 RVA: 0x0026B7EA File Offset: 0x002699EA
		protected override bool MatchesConditionSpecific()
		{
			return this.character.Vitality / this.character.MaxVitality > this.vitalityPercentage;
		}

		// Token: 0x0400245E RID: 9310
		private readonly float vitalityPercentage;
	}
}
