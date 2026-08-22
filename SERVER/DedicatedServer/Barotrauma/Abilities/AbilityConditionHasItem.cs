using System;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x02000309 RID: 777
	internal class AbilityConditionHasItem : AbilityConditionDataless
	{
		// Token: 0x06003201 RID: 12801 RVA: 0x00153DB5 File Offset: 0x00151FB5
		public AbilityConditionHasItem(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tags = conditionElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
			this.requireAll = conditionElement.GetAttributeBool("requireall", false);
		}

		// Token: 0x06003202 RID: 12802 RVA: 0x00153DE8 File Offset: 0x00151FE8
		protected override bool MatchesConditionSpecific()
		{
			if (this.tags.None(null))
			{
				return this.character.GetEquippedItem(Identifier.Empty, null) != null;
			}
			if (this.requireAll)
			{
				foreach (Identifier tag in this.tags)
				{
					if (this.character.GetEquippedItem(tag, null) == null)
					{
						return false;
					}
				}
				return true;
			}
			foreach (Identifier tag2 in this.tags)
			{
				if (this.character.GetEquippedItem(tag2, null) != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040018A6 RID: 6310
		private readonly Identifier[] tags;

		// Token: 0x040018A7 RID: 6311
		private readonly bool requireAll;
	}
}
