using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C6 RID: 966
	internal class AbilityConditionAlliesAboveVitality : AbilityConditionDataless
	{
		// Token: 0x06004626 RID: 17958 RVA: 0x0026B80B File Offset: 0x00269A0B
		public AbilityConditionAlliesAboveVitality(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.vitalityPercentage = conditionElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x06004627 RID: 17959 RVA: 0x0026B82B File Offset: 0x00269A2B
		protected override bool MatchesConditionSpecific()
		{
			return Character.GetFriendlyCrew(this.character).All((Character c) => c.HealthPercentage / 100f >= this.vitalityPercentage);
		}

		// Token: 0x0400245F RID: 9311
		private readonly float vitalityPercentage;
	}
}
