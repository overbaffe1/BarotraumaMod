using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002FF RID: 767
	internal class AbilityConditionAboveVitality : AbilityConditionDataless
	{
		// Token: 0x060031EA RID: 12778 RVA: 0x0015394A File Offset: 0x00151B4A
		public AbilityConditionAboveVitality(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.vitalityPercentage = conditionElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x060031EB RID: 12779 RVA: 0x0015396A File Offset: 0x00151B6A
		protected override bool MatchesConditionSpecific()
		{
			return this.character.Vitality / this.character.MaxVitality > this.vitalityPercentage;
		}

		// Token: 0x0400189D RID: 6301
		private readonly float vitalityPercentage;
	}
}
