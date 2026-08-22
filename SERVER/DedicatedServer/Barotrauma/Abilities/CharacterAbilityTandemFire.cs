using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x0200035D RID: 861
	internal class CharacterAbilityTandemFire : CharacterAbilityApplyStatusEffectsToNearestAlly
	{
		// Token: 0x060032FF RID: 13055 RVA: 0x00158650 File Offset: 0x00156850
		public CharacterAbilityTandemFire(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.tag = abilityElement.GetAttributeIdentifier("tag", Identifier.Empty);
		}

		// Token: 0x06003300 RID: 13056 RVA: 0x00158670 File Offset: 0x00156870
		protected override void ApplyEffect()
		{
			if (!CharacterAbilityTandemFire.<ApplyEffect>g__SelectedItemHasTag|2_0(base.Character, this.tag))
			{
				return;
			}
			Character closestCharacter = null;
			float closestDistance = this.squaredMaxDistance;
			foreach (Character crewCharacter in Character.GetFriendlyCrew(base.Character))
			{
				if (crewCharacter != base.Character)
				{
					float tempDistance = Vector2.DistanceSquared(base.Character.WorldPosition, crewCharacter.WorldPosition);
					if (tempDistance < closestDistance && CharacterAbilityTandemFire.<ApplyEffect>g__SelectedItemHasTag|2_0(crewCharacter, this.tag))
					{
						closestCharacter = crewCharacter;
						closestDistance = tempDistance;
					}
				}
			}
			if (closestCharacter == null)
			{
				return;
			}
			if (closestDistance < this.squaredMaxDistance)
			{
				base.ApplyEffectSpecific(base.Character, null);
				base.ApplyEffectSpecific(closestCharacter, null);
			}
		}

		// Token: 0x06003301 RID: 13057 RVA: 0x00158734 File Offset: 0x00156934
		[CompilerGenerated]
		internal static bool <ApplyEffect>g__SelectedItemHasTag|2_0(Character character, Identifier tag)
		{
			return (character.SelectedItem != null && character.SelectedItem.HasTag(tag)) || (character.SelectedSecondaryItem != null && character.SelectedSecondaryItem.HasTag(tag));
		}

		// Token: 0x0400194E RID: 6478
		private readonly Identifier tag;
	}
}
