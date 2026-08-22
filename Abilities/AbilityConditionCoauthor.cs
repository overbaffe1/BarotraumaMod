using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C9 RID: 969
	internal class AbilityConditionCoauthor : AbilityConditionDataless
	{
		// Token: 0x0600462D RID: 17965 RVA: 0x0026B9DC File Offset: 0x00269BDC
		public AbilityConditionCoauthor(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.jobIdentifier = conditionElement.GetAttributeString("jobidentifier", string.Empty);
		}

		// Token: 0x0600462E RID: 17966 RVA: 0x0026B9FC File Offset: 0x00269BFC
		protected override bool MatchesConditionSpecific()
		{
			Character otherCharacter = this.character.SelectedCharacter;
			return otherCharacter != null && otherCharacter.HasJob(this.jobIdentifier) && this.character.SelectedBy == otherCharacter;
		}

		// Token: 0x04002463 RID: 9315
		private readonly string jobIdentifier;
	}
}
