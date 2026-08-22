using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000303 RID: 771
	internal class AbilityConditionCoauthor : AbilityConditionDataless
	{
		// Token: 0x060031F3 RID: 12787 RVA: 0x00153B5C File Offset: 0x00151D5C
		public AbilityConditionCoauthor(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.jobIdentifier = conditionElement.GetAttributeString("jobidentifier", string.Empty);
		}

		// Token: 0x060031F4 RID: 12788 RVA: 0x00153B7C File Offset: 0x00151D7C
		protected override bool MatchesConditionSpecific()
		{
			Character otherCharacter = this.character.SelectedCharacter;
			return otherCharacter != null && otherCharacter.HasJob(this.jobIdentifier) && this.character.SelectedBy == otherCharacter;
		}

		// Token: 0x040018A2 RID: 6306
		private readonly string jobIdentifier;
	}
}
