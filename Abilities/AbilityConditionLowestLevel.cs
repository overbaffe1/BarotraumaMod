using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DB RID: 987
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionLowestLevel : AbilityConditionCharacter
	{
		// Token: 0x06004657 RID: 18007 RVA: 0x0026C368 File Offset: 0x0026A568
		public AbilityConditionLowestLevel(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004658 RID: 18008 RVA: 0x0026C374 File Offset: 0x0026A574
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
