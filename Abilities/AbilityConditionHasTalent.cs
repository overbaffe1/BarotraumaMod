using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D4 RID: 980
	internal class AbilityConditionHasTalent : AbilityConditionDataless
	{
		// Token: 0x06004647 RID: 17991 RVA: 0x0026C124 File Offset: 0x0026A324
		public AbilityConditionHasTalent(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.talentIdentifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x06004648 RID: 17992 RVA: 0x0026C144 File Offset: 0x0026A344
		protected override bool MatchesConditionSpecific()
		{
			return this.character.HasTalent(this.talentIdentifier);
		}

		// Token: 0x04002473 RID: 9331
		private readonly Identifier talentIdentifier;
	}
}
