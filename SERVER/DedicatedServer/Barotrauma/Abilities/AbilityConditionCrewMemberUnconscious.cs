using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000304 RID: 772
	internal sealed class AbilityConditionCrewMemberUnconscious : AbilityConditionDataless
	{
		// Token: 0x060031F5 RID: 12789 RVA: 0x00153BBB File Offset: 0x00151DBB
		[NullableContext(1)]
		public AbilityConditionCrewMemberUnconscious(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031F6 RID: 12790 RVA: 0x00153BC8 File Offset: 0x00151DC8
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
