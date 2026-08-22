using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030A RID: 778
	internal sealed class AbilityConditionHasLevel : AbilityConditionDataless
	{
		// Token: 0x06003203 RID: 12803 RVA: 0x00153EA4 File Offset: 0x001520A4
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

		// Token: 0x06003204 RID: 12804 RVA: 0x00153F54 File Offset: 0x00152154
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

		// Token: 0x040018A8 RID: 6312
		private readonly Option<int> matchedLevel;

		// Token: 0x040018A9 RID: 6313
		private readonly Option<int> minLevel;

		// Token: 0x040018AA RID: 6314
		private readonly Option<int> maxLevel;
	}
}
