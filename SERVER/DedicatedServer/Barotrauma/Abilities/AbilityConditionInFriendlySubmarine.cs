using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000311 RID: 785
	internal class AbilityConditionInFriendlySubmarine : AbilityConditionDataless
	{
		// Token: 0x06003214 RID: 12820 RVA: 0x001543FA File Offset: 0x001525FA
		public AbilityConditionInFriendlySubmarine(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003215 RID: 12821 RVA: 0x00154404 File Offset: 0x00152604
		protected override bool MatchesConditionSpecific()
		{
			Submarine submarine = this.character.Submarine;
			CharacterTeamType? characterTeamType = (submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null;
			CharacterTeamType teamID = this.character.TeamID;
			return characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null;
		}
	}
}
