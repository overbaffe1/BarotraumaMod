using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F6 RID: 1014
	internal class CharacterAbilityApplyStatusEffectsToNearestAlly : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x060046AA RID: 18090 RVA: 0x0026D5C0 File Offset: 0x0026B7C0
		public CharacterAbilityApplyStatusEffectsToNearestAlly(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.squaredMaxDistance = MathF.Pow(abilityElement.GetAttributeFloat("maxdistance", float.MaxValue), 2f);
		}

		// Token: 0x060046AB RID: 18091 RVA: 0x0026D5EC File Offset: 0x0026B7EC
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

		// Token: 0x04002497 RID: 9367
		protected float squaredMaxDistance;
	}
}
