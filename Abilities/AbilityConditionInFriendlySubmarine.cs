using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D7 RID: 983
	internal class AbilityConditionInFriendlySubmarine : AbilityConditionDataless
	{
		// Token: 0x0600464E RID: 17998 RVA: 0x0026C27A File Offset: 0x0026A47A
		public AbilityConditionInFriendlySubmarine(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x0600464F RID: 17999 RVA: 0x0026C284 File Offset: 0x0026A484
		protected override bool MatchesConditionSpecific()
		{
			Submarine submarine = this.character.Submarine;
			CharacterTeamType? characterTeamType = (submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null;
			CharacterTeamType teamID = this.character.TeamID;
			return characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null;
		}
	}
}
