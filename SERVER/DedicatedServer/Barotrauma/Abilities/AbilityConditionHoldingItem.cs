using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000310 RID: 784
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionHoldingItem : AbilityConditionDataless
	{
		// Token: 0x06003211 RID: 12817 RVA: 0x00154331 File Offset: 0x00152531
		public AbilityConditionHoldingItem(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.tags = conditionElement.GetAttributeIdentifierImmutableHashSet("tags", ImmutableHashSet<Identifier>.Empty, true);
		}

		// Token: 0x06003212 RID: 12818 RVA: 0x00154354 File Offset: 0x00152554
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

		// Token: 0x06003213 RID: 12819 RVA: 0x001543D4 File Offset: 0x001525D4
		[CompilerGenerated]
		internal static bool <MatchesConditionSpecific>g__HasItemInHand|2_0(Character character, Identifier tagOrIdentifier)
		{
			return character.GetEquippedItem(tagOrIdentifier, new InvSlotType?(InvSlotType.RightHand)) != null || character.GetEquippedItem(tagOrIdentifier, new InvSlotType?(InvSlotType.LeftHand)) != null;
		}

		// Token: 0x040018B4 RID: 6324
		private readonly ImmutableHashSet<Identifier> tags;
	}
}
