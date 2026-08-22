using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000314 RID: 788
	internal class AbilityConditionLevelsBehindHighest : AbilityConditionDataless
	{
		// Token: 0x0600321A RID: 12826 RVA: 0x00154485 File Offset: 0x00152685
		public AbilityConditionLevelsBehindHighest(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.levelsBehind = conditionElement.GetAttributeInt("levelsbehind", 0);
		}

		// Token: 0x0600321B RID: 12827 RVA: 0x001544A1 File Offset: 0x001526A1
		protected override bool MatchesConditionSpecific()
		{
			return Character.GetFriendlyCrew(this.character).Any((Character c) => c.Info.GetCurrentLevel() - this.character.Info.GetCurrentLevel() >= this.levelsBehind);
		}

		// Token: 0x040018B5 RID: 6325
		private readonly int levelsBehind;
	}
}
