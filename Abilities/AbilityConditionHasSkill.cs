using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D2 RID: 978
	internal class AbilityConditionHasSkill : AbilityConditionDataless
	{
		// Token: 0x06004641 RID: 17985 RVA: 0x0026BFD0 File Offset: 0x0026A1D0
		public AbilityConditionHasSkill(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.skillIdentifier = conditionElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
			this.minValue = conditionElement.GetAttributeFloat("minvalue", 0f);
		}

		// Token: 0x06004642 RID: 17986 RVA: 0x0026C006 File Offset: 0x0026A206
		protected override bool MatchesConditionSpecific()
		{
			return this.character.GetSkillLevel(this.skillIdentifier) >= this.minValue;
		}

		// Token: 0x04002470 RID: 9328
		private readonly Identifier skillIdentifier;

		// Token: 0x04002471 RID: 9329
		private readonly float minValue;
	}
}
