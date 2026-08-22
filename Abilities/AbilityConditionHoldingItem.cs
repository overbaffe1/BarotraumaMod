using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D6 RID: 982
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionHoldingItem : AbilityConditionDataless
	{
		// Token: 0x0600464B RID: 17995 RVA: 0x0026C1B1 File Offset: 0x0026A3B1
		public AbilityConditionHoldingItem(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tags = conditionElement.GetAttributeIdentifierImmutableHashSet("tags", ImmutableHashSet<Identifier>.Empty, true);
		}

		// Token: 0x0600464C RID: 17996 RVA: 0x0026C1D4 File Offset: 0x0026A3D4
		protected override bool MatchesConditionSpecific()
		{
			if (this.tags.Count == 0)
			{
				return AbilityConditionHoldingItem.<MatchesConditionSpecific>g__HasItemInHand|2_0(this.character, Identifier.Empty);
			}
			foreach (Identifier tag in this.tags)
			{
				if (AbilityConditionHoldingItem.<MatchesConditionSpecific>g__HasItemInHand|2_0(this.character, tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600464D RID: 17997 RVA: 0x0026C254 File Offset: 0x0026A454
		[CompilerGenerated]
		internal static bool <MatchesConditionSpecific>g__HasItemInHand|2_0(Character character, Identifier tagOrIdentifier)
		{
			return character.GetEquippedItem(tagOrIdentifier, new InvSlotType?(InvSlotType.RightHand)) != null || character.GetEquippedItem(tagOrIdentifier, new InvSlotType?(InvSlotType.LeftHand)) != null;
		}

		// Token: 0x04002475 RID: 9333
		private readonly ImmutableHashSet<Identifier> tags;
	}
}
