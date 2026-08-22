using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000300 RID: 768
	internal class AbilityConditionAlliesAboveVitality : AbilityConditionDataless
	{
		// Token: 0x060031EC RID: 12780 RVA: 0x0015398B File Offset: 0x00151B8B
		public AbilityConditionAlliesAboveVitality(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.vitalityPercentage = conditionElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x060031ED RID: 12781 RVA: 0x001539AB File Offset: 0x00151BAB
		protected override bool MatchesConditionSpecific()
		{
			return Character.GetFriendlyCrew(this.character).All((Character c) => c.HealthPercentage / 100f >= this.vitalityPercentage);
		}

		// Token: 0x0400189E RID: 6302
		private readonly float vitalityPercentage;
	}
}
