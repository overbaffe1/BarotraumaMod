using System;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CF RID: 975
	internal class AbilityConditionHasItem : AbilityConditionDataless
	{
		// Token: 0x0600463B RID: 17979 RVA: 0x0026BC35 File Offset: 0x00269E35
		public AbilityConditionHasItem(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tags = conditionElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
			this.requireAll = conditionElement.GetAttributeBool("requireall", false);
		}

		// Token: 0x0600463C RID: 17980 RVA: 0x0026BC68 File Offset: 0x00269E68
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

		// Token: 0x04002467 RID: 9319
		private readonly Identifier[] tags;

		// Token: 0x04002468 RID: 9320
		private readonly bool requireAll;
	}
}
