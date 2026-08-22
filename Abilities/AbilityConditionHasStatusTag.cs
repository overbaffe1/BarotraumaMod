using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D3 RID: 979
	internal class AbilityConditionHasStatusTag : AbilityConditionDataless
	{
		// Token: 0x06004643 RID: 17987 RVA: 0x0026C024 File Offset: 0x0026A224
		public AbilityConditionHasStatusTag(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tag = conditionElement.GetAttributeIdentifier("tag", Identifier.Empty);
			if (this.tag.IsEmpty)
			{
				DebugConsole.AddWarning("Error in talent \"" + characterTalent.Prefab.OriginalName + "\" - tag not defined in AbilityConditionHasStatusTag.", characterTalent.Prefab.ContentPackage);
			}
		}

		// Token: 0x06004644 RID: 17988 RVA: 0x0026C088 File Offset: 0x0026A288
		protected override bool MatchesConditionSpecific()
		{
			return !this.tag.IsEmpty && (StatusEffect.DurationList.Any((DurationListElement d) => d.Targets.Contains(this.character) && d.Parent.HasTag(this.tag)) || DelayedEffect.DelayList.Any((DelayedListElement d) => d.Targets.Contains(this.character) && d.Parent.HasTag(this.tag)));
		}

		// Token: 0x04002472 RID: 9330
		private readonly Identifier tag;
	}
}
