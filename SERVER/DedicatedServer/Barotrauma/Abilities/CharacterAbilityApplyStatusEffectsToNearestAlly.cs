using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000330 RID: 816
	internal class CharacterAbilityApplyStatusEffectsToNearestAlly : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x06003270 RID: 12912 RVA: 0x00155740 File Offset: 0x00153940
		public CharacterAbilityApplyStatusEffectsToNearestAlly(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.squaredMaxDistance = MathF.Pow(abilityElement.GetAttributeFloat("maxdistance", float.MaxValue), 2f);
		}

		// Token: 0x06003271 RID: 12913 RVA: 0x0015576C File Offset: 0x0015396C
		protected override void ApplyEffect()
		{
			Character closestCharacter = null;
			float closestDistance = float.MaxValue;
			foreach (Character crewCharacter in Character.GetFriendlyCrew(base.Character))
			{
				if (crewCharacter != base.Character)
				{
					float tempDistance = Vector2.DistanceSquared(base.Character.WorldPosition, crewCharacter.WorldPosition);
					if (tempDistance < closestDistance)
					{
						closestCharacter = crewCharacter;
						closestDistance = tempDistance;
					}
				}
			}
			if (closestDistance < this.squaredMaxDistance)
			{
				base.ApplyEffectSpecific(closestCharacter, null);
			}
		}

		// Token: 0x040018D6 RID: 6358
		protected float squaredMaxDistance;
	}
}
