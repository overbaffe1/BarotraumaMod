using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000302 RID: 770
	internal sealed class AbilityConditionAllyNearby : AbilityConditionDataless
	{
		// Token: 0x060031F1 RID: 12785 RVA: 0x00153A6C File Offset: 0x00151C6C
		public AbilityConditionAllyNearby(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			string key = "truthywhen";
			AbilityConditionAllyNearby.NearbyCharacterTruthy nearbyCharacterTruthy = AbilityConditionAllyNearby.NearbyCharacterTruthy.OneCharacterMatches;
			this.truthyWhen = conditionElement.GetAttributeEnum<AbilityConditionAllyNearby.NearbyCharacterTruthy>(key, nearbyCharacterTruthy);
			this.distance = conditionElement.GetAttributeFloat("distance", 10f);
		}

		// Token: 0x060031F2 RID: 12786 RVA: 0x00153AAC File Offset: 0x00151CAC
		protected override bool MatchesConditionSpecific()
		{
			AbilityConditionAllyNearby.NearbyCharacterTruthy nearbyCharacterTruthy = this.truthyWhen;
			bool flag;
			if (nearbyCharacterTruthy != AbilityConditionAllyNearby.NearbyCharacterTruthy.OneCharacterMatches)
			{
				if (nearbyCharacterTruthy != AbilityConditionAllyNearby.NearbyCharacterTruthy.NoCharacterMatches)
				{
					throw new ArgumentOutOfRangeException("truthyWhen");
				}
				flag = false;
			}
			else
			{
				flag = true;
			}
			bool trueCondition = flag;
			foreach (Character ally in Character.GetFriendlyCrew(this.character))
			{
				if (ally != this.character)
				{
					float distanceToCharacter = Vector2.DistanceSquared(ally.WorldPosition, this.character.WorldPosition);
					if (distanceToCharacter < this.distance * this.distance)
					{
						return trueCondition;
					}
				}
			}
			return !trueCondition;
		}

		// Token: 0x040018A0 RID: 6304
		private readonly AbilityConditionAllyNearby.NearbyCharacterTruthy truthyWhen;

		// Token: 0x040018A1 RID: 6305
		private readonly float distance;

		// Token: 0x02000B9A RID: 2970
		private enum NearbyCharacterTruthy
		{
			// Token: 0x040039DF RID: 14815
			OneCharacterMatches,
			// Token: 0x040039E0 RID: 14816
			NoCharacterMatches
		}
	}
}
