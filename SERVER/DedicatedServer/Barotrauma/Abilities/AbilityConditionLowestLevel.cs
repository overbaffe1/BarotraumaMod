using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000315 RID: 789
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionLowestLevel : AbilityConditionCharacter
	{
		// Token: 0x0600321D RID: 12829 RVA: 0x001544E8 File Offset: 0x001526E8
		public AbilityConditionLowestLevel(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x0600321E RID: 12830 RVA: 0x001544F4 File Offset: 0x001526F4
		protected override bool MatchesCharacter(Character character)
		{
			int ownLevel = character.Info.GetCurrentLevel();
			foreach (Character otherCharacter in Character.GetFriendlyCrew(character))
			{
				if (otherCharacter != character && otherCharacter.Info.GetCurrentLevel() < ownLevel)
				{
					return false;
				}
			}
			return true;
		}
	}
}
