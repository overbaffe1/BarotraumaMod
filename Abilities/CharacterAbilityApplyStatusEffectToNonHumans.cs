using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F8 RID: 1016
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityApplyStatusEffectToNonHumans : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x060046B1 RID: 18097 RVA: 0x0026D76F File Offset: 0x0026B96F
		public CharacterAbilityApplyStatusEffectToNonHumans(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.maxDistance = abilityElement.GetAttributeFloat("maxdistance", float.MaxValue);
		}

		// Token: 0x060046B2 RID: 18098 RVA: 0x0026D790 File Offset: 0x0026B990
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

		// Token: 0x060046B3 RID: 18099 RVA: 0x0026D820 File Offset: 0x0026BA20
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x0400249B RID: 9371
		private readonly float maxDistance;
	}
}
