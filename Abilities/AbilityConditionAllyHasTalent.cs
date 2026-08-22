using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C7 RID: 967
	internal class AbilityConditionAllyHasTalent : AbilityConditionDataless
	{
		// Token: 0x06004629 RID: 17961 RVA: 0x0026B862 File Offset: 0x00269A62
		public AbilityConditionAllyHasTalent(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.talentIdentifier = conditionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x0600462A RID: 17962 RVA: 0x0026B884 File Offset: 0x00269A84
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

		// Token: 0x04002460 RID: 9312
		private readonly Identifier talentIdentifier;
	}
}
