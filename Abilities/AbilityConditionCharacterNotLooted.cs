using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B7 RID: 951
	internal sealed class AbilityConditionCharacterNotLooted : AbilityConditionCharacter
	{
		// Token: 0x06004600 RID: 17920 RVA: 0x0026AC97 File Offset: 0x00268E97
		public AbilityConditionCharacterNotLooted(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.identifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x06004601 RID: 17921 RVA: 0x0026ACB7 File Offset: 0x00268EB7
		protected override bool MatchesCharacter(Character character)
		{
			return character != null && !character.MarkedAsLooted.Contains(this.identifier);
		}

		// Token: 0x0400244E RID: 9294
		private readonly Identifier identifier;
	}
}
