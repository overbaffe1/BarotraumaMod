using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CA RID: 970
	internal sealed class AbilityConditionCrewMemberUnconscious : AbilityConditionDataless
	{
		// Token: 0x0600462F RID: 17967 RVA: 0x0026BA3B File Offset: 0x00269C3B
		[NullableContext(1)]
		public AbilityConditionCrewMemberUnconscious(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004630 RID: 17968 RVA: 0x0026BA48 File Offset: 0x00269C48
		protected override bool MatchesConditionSpecific()
		{
			foreach (Character c in Character.GetFriendlyCrew(this.character))
			{
				if (!c.IsDead && c.IsUnconscious)
				{
					return true;
				}
			}
			return false;
		}
	}
}
