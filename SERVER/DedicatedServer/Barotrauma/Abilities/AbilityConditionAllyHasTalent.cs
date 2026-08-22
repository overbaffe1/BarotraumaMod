using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000301 RID: 769
	internal class AbilityConditionAllyHasTalent : AbilityConditionDataless
	{
		// Token: 0x060031EF RID: 12783 RVA: 0x001539E2 File Offset: 0x00151BE2
		public AbilityConditionAllyHasTalent(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.talentIdentifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x060031F0 RID: 12784 RVA: 0x00153A04 File Offset: 0x00151C04
		protected override bool MatchesConditionSpecific()
		{
			foreach (Character crewCharacter in Character.GetFriendlyCrew(this.characterTalent.Character))
			{
				if (crewCharacter.HasTalent(this.talentIdentifier))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400189F RID: 6303
		private readonly Identifier talentIdentifier;
	}
}
