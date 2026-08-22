using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030C RID: 780
	internal class AbilityConditionHasSkill : AbilityConditionDataless
	{
		// Token: 0x06003207 RID: 12807 RVA: 0x00154150 File Offset: 0x00152350
		public AbilityConditionHasSkill(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.skillIdentifier = conditionElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
			this.minValue = conditionElement.GetAttributeFloat("minvalue", 0f);
		}

		// Token: 0x06003208 RID: 12808 RVA: 0x00154186 File Offset: 0x00152386
		protected override bool MatchesConditionSpecific()
		{
			return this.character.GetSkillLevel(this.skillIdentifier) >= this.minValue;
		}

		// Token: 0x040018AF RID: 6319
		private readonly Identifier skillIdentifier;

		// Token: 0x040018B0 RID: 6320
		private readonly float minValue;
	}
}
