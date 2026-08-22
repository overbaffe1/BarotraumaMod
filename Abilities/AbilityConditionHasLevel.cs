using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D0 RID: 976
	internal sealed class AbilityConditionHasLevel : AbilityConditionDataless
	{
		// Token: 0x0600463D RID: 17981 RVA: 0x0026BD24 File Offset: 0x00269F24
		[NullableContext(1)]
		public AbilityConditionHasLevel(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			int match = conditionElement.GetAttributeInt("levelequals", 0);
			this.matchedLevel = ((match != 0) ? Option<int>.Some(match) : Option<int>.None());
			int min = conditionElement.GetAttributeInt("minlevel", 0);
			this.minLevel = ((min != 0) ? Option<int>.Some(min) : Option<int>.None());
			int max = conditionElement.GetAttributeInt("maxlevel", 0);
			this.maxLevel = ((max != 0) ? Option<int>.Some(max) : Option<int>.None());
			if (this.matchedLevel.IsNone() && this.minLevel.IsNone() && this.maxLevel.IsNone())
			{
				throw new Exception("AbilityConditionHasLevel must have either \"levelequals\", \"minlevel\" or \"maxlevel\" attribute.");
			}
		}

		// Token: 0x0600463E RID: 17982 RVA: 0x0026BDD4 File Offset: 0x00269FD4
		protected override bool MatchesConditionSpecific()
		{
			int currentLevel = this.character.Info.GetCurrentLevel();
			int match;
			if (this.matchedLevel.TryUnwrap(out match))
			{
				return currentLevel == match;
			}
			int min;
			if (this.minLevel.TryUnwrap(out min))
			{
				return currentLevel >= min;
			}
			int max;
			return this.maxLevel.TryUnwrap(out max) && currentLevel <= max;
		}

		// Token: 0x04002469 RID: 9321
		private readonly Option<int> matchedLevel;

		// Token: 0x0400246A RID: 9322
		private readonly Option<int> minLevel;

		// Token: 0x0400246B RID: 9323
		private readonly Option<int> maxLevel;
	}
}
