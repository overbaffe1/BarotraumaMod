using System;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F7 RID: 1015
	internal class CharacterAbilityApplyStatusEffectsToRandomAlly : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x17001220 RID: 4640
		// (get) Token: 0x060046AC RID: 18092 RVA: 0x0026D67C File Offset: 0x0026B87C
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060046AD RID: 18093 RVA: 0x0026D680 File Offset: 0x0026B880
		public CharacterAbilityApplyStatusEffectsToRandomAlly(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.squaredMaxDistance = MathF.Pow(abilityElement.GetAttributeFloat("maxdistance", float.MaxValue), 2f);
			this.allowDifferentSub = abilityElement.GetAttributeBool("mustbeonsamesub", true);
			this.allowSelf = abilityElement.GetAttributeBool("allowself", true);
		}

		// Token: 0x060046AE RID: 18094 RVA: 0x0026D6D9 File Offset: 0x0026B8D9
		protected override void ApplyEffect()
		{
			this.ApplyEffect(base.Character);
		}

		// Token: 0x060046AF RID: 18095 RVA: 0x0026D6E8 File Offset: 0x0026B8E8
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character targetCharacter = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (targetCharacter != null)
			{
				this.ApplyEffect(targetCharacter);
				return;
			}
			this.ApplyEffect(base.Character);
		}

		// Token: 0x060046B0 RID: 18096 RVA: 0x0026D720 File Offset: 0x0026B920
		private void ApplyEffect(Character thisCharacter)
		{
			Character chosenCharacter = Character.GetFriendlyCrew(thisCharacter).Where(delegate(Character c)
			{
				if ((this.allowSelf || c != thisCharacter) && (this.allowDifferentSub || c.Submarine == this.Character.Submarine))
				{
					float tempDistance = Vector2.DistanceSquared(thisCharacter.WorldPosition, c.WorldPosition);
					return tempDistance < this.squaredMaxDistance;
				}
				return false;
			}).GetRandomUnsynced<Character>();
			if (chosenCharacter == null)
			{
				return;
			}
			base.ApplyEffectSpecific(chosenCharacter, null);
		}

		// Token: 0x04002498 RID: 9368
		private readonly float squaredMaxDistance;

		// Token: 0x04002499 RID: 9369
		private readonly bool allowDifferentSub;

		// Token: 0x0400249A RID: 9370
		private readonly bool allowSelf;
	}
}
