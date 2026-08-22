using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000332 RID: 818
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityApplyStatusEffectToNonHumans : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x06003277 RID: 12919 RVA: 0x001558EF File Offset: 0x00153AEF
		public CharacterAbilityApplyStatusEffectToNonHumans(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.maxDistance = abilityElement.GetAttributeFloat("maxdistance", float.MaxValue);
		}

		// Token: 0x06003278 RID: 12920 RVA: 0x00155910 File Offset: 0x00153B10
		protected override void ApplyEffect()
		{
			foreach (Character character in Character.CharacterList)
			{
				if (!character.IsHuman && (this.maxDistance >= 3.4028235E+38f || Vector2.DistanceSquared(character.WorldPosition, base.Character.WorldPosition) <= this.maxDistance * this.maxDistance))
				{
					base.ApplyEffectSpecific(character, null);
				}
			}
		}

		// Token: 0x06003279 RID: 12921 RVA: 0x001559A0 File Offset: 0x00153BA0
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x040018DA RID: 6362
		private readonly float maxDistance;
	}
}
