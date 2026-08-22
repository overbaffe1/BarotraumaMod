using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030E RID: 782
	internal class AbilityConditionHasTalent : AbilityConditionDataless
	{
		// Token: 0x0600320D RID: 12813 RVA: 0x001542A4 File Offset: 0x001524A4
		public AbilityConditionHasTalent(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.talentIdentifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x0600320E RID: 12814 RVA: 0x001542C4 File Offset: 0x001524C4
		protected override bool MatchesConditionSpecific()
		{
			return this.character.HasTalent(this.talentIdentifier);
		}

		// Token: 0x040018B2 RID: 6322
		private readonly Identifier talentIdentifier;
	}
}
