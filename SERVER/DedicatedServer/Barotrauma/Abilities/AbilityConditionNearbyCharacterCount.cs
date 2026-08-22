using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000316 RID: 790
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionNearbyCharacterCount : AbilityConditionDataless
	{
		// Token: 0x0600321F RID: 12831 RVA: 0x00154560 File Offset: 0x00152760
		public AbilityConditionNearbyCharacterCount(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.distance = conditionElement.GetAttributeFloat("distance", 10f);
			this.count = conditionElement.GetAttributeInt("count", 1);
			this.targetTypes = base.ParseTargetTypes(conditionElement.GetAttributeStringArray("targettypes", Array.Empty<string>(), true)).ToImmutableHashSet<AbilityCondition.TargetType>();
		}

		// Token: 0x06003220 RID: 12832 RVA: 0x001545C0 File Offset: 0x001527C0
		protected override bool MatchesConditionSpecific()
		{
			int amountNeeded = this.count;
			foreach (Character otherCharacter in Character.CharacterList)
			{
				if (this.character.Submarine == otherCharacter.Submarine && base.IsViableTarget(this.targetTypes, otherCharacter) && Vector2.DistanceSquared(this.character.WorldPosition, otherCharacter.WorldPosition) < this.distance * this.distance)
				{
					amountNeeded--;
					if (amountNeeded <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x040018B6 RID: 6326
		private readonly float distance;

		// Token: 0x040018B7 RID: 6327
		private readonly int count;

		// Token: 0x040018B8 RID: 6328
		private readonly ImmutableHashSet<AbilityCondition.TargetType> targetTypes;
	}
}
