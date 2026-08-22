using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DC RID: 988
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionNearbyCharacterCount : AbilityConditionDataless
	{
		// Token: 0x06004659 RID: 18009 RVA: 0x0026C3E0 File Offset: 0x0026A5E0
		public AbilityConditionNearbyCharacterCount(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.distance = conditionElement.GetAttributeFloat("distance", 10f);
			this.count = conditionElement.GetAttributeInt("count", 1);
			this.targetTypes = base.ParseTargetTypes(conditionElement.GetAttributeStringArray("targettypes", Array.Empty<string>(), true)).ToImmutableHashSet<AbilityCondition.TargetType>();
		}

		// Token: 0x0600465A RID: 18010 RVA: 0x0026C440 File Offset: 0x0026A640
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

		// Token: 0x04002477 RID: 9335
		private readonly float distance;

		// Token: 0x04002478 RID: 9336
		private readonly int count;

		// Token: 0x04002479 RID: 9337
		private readonly ImmutableHashSet<AbilityCondition.TargetType> targetTypes;
	}
}
