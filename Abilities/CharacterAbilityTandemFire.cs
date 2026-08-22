using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000423 RID: 1059
	internal class CharacterAbilityTandemFire : CharacterAbilityApplyStatusEffectsToNearestAlly
	{
		// Token: 0x06004739 RID: 18233 RVA: 0x002704D0 File Offset: 0x0026E6D0
		public CharacterAbilityTandemFire(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.tag = abilityElement.GetAttributeIdentifier("tag", Identifier.Empty);
		}

		// Token: 0x0600473A RID: 18234 RVA: 0x002704F0 File Offset: 0x0026E6F0
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

		// Token: 0x0600473B RID: 18235 RVA: 0x002705B4 File Offset: 0x0026E7B4
		[CompilerGenerated]
		internal static bool <ApplyEffect>g__SelectedItemHasTag|2_0(Character character, Identifier tag)
		{
			return (character.SelectedItem != null && character.SelectedItem.HasTag(tag)) || (character.SelectedSecondaryItem != null && character.SelectedSecondaryItem.HasTag(tag));
		}

		// Token: 0x0400250F RID: 9487
		private readonly Identifier tag;
	}
}
