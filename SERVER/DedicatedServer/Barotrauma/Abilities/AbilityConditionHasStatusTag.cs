using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030D RID: 781
	internal class AbilityConditionHasStatusTag : AbilityConditionDataless
	{
		// Token: 0x06003209 RID: 12809 RVA: 0x001541A4 File Offset: 0x001523A4
		public AbilityConditionHasStatusTag(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tag = conditionElement.GetAttributeIdentifier("tag", Identifier.Empty);
			if (this.tag.IsEmpty)
			{
				DebugConsole.AddWarning("Error in talent \"" + characterTalent.Prefab.OriginalName + "\" - tag not defined in AbilityConditionHasStatusTag.", characterTalent.Prefab.ContentPackage);
			}
		}

		// Token: 0x0600320A RID: 12810 RVA: 0x00154208 File Offset: 0x00152408
		protected override bool MatchesConditionSpecific()
		{
			return !this.tag.IsEmpty && (StatusEffect.DurationList.Any((DurationListElement d) => d.Targets.Contains(this.character) && d.Parent.HasTag(this.tag)) || DelayedEffect.DelayList.Any((DelayedListElement d) => d.Targets.Contains(this.character) && d.Parent.HasTag(this.tag)));
		}

		// Token: 0x040018B1 RID: 6321
		private readonly Identifier tag;
	}
}
