using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DA RID: 986
	internal class AbilityConditionLevelsBehindHighest : AbilityConditionDataless
	{
		// Token: 0x06004654 RID: 18004 RVA: 0x0026C305 File Offset: 0x0026A505
		public AbilityConditionLevelsBehindHighest(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.levelsBehind = conditionElement.GetAttributeInt("levelsbehind", 0);
		}

		// Token: 0x06004655 RID: 18005 RVA: 0x0026C321 File Offset: 0x0026A521
		protected override bool MatchesConditionSpecific()
		{
			return Character.GetFriendlyCrew(this.character).Any((Character c) => c.Info.GetCurrentLevel() - this.character.Info.GetCurrentLevel() >= this.levelsBehind);
		}

		// Token: 0x04002476 RID: 9334
		private readonly int levelsBehind;
	}
}
