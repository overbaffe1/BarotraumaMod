using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C8 RID: 968
	internal sealed class AbilityConditionAllyNearby : AbilityConditionDataless
	{
		// Token: 0x0600462B RID: 17963 RVA: 0x0026B8EC File Offset: 0x00269AEC
		public AbilityConditionAllyNearby(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			string key = "truthywhen";
			AbilityConditionAllyNearby.NearbyCharacterTruthy nearbyCharacterTruthy = AbilityConditionAllyNearby.NearbyCharacterTruthy.OneCharacterMatches;
			this.truthyWhen = conditionElement.GetAttributeEnum<AbilityConditionAllyNearby.NearbyCharacterTruthy>(key, nearbyCharacterTruthy);
			this.distance = conditionElement.GetAttributeFloat("distance", 10f);
		}

		// Token: 0x0600462C RID: 17964 RVA: 0x0026B92C File Offset: 0x00269B2C
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

		// Token: 0x04002461 RID: 9313
		private readonly AbilityConditionAllyNearby.NearbyCharacterTruthy truthyWhen;

		// Token: 0x04002462 RID: 9314
		private readonly float distance;

		// Token: 0x020010E3 RID: 4323
		private enum NearbyCharacterTruthy
		{
			// Token: 0x040059EF RID: 23023
			OneCharacterMatches,
			// Token: 0x040059F0 RID: 23024
			NoCharacterMatches
		}
	}
}
