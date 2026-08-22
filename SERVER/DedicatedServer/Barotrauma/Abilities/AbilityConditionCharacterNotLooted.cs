using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F1 RID: 753
	internal sealed class AbilityConditionCharacterNotLooted : AbilityConditionCharacter
	{
		// Token: 0x060031C6 RID: 12742 RVA: 0x00152E17 File Offset: 0x00151017
		public AbilityConditionCharacterNotLooted(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.identifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x060031C7 RID: 12743 RVA: 0x00152E37 File Offset: 0x00151037
		protected override bool MatchesCharacter(Character character)
		{
			return character != null && !character.MarkedAsLooted.Contains(this.identifier);
		}

		// Token: 0x0400188D RID: 6285
		private readonly Identifier identifier;
	}
}
