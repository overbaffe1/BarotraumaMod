using System;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000331 RID: 817
	internal class CharacterAbilityApplyStatusEffectsToRandomAlly : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x17000E30 RID: 3632
		// (get) Token: 0x06003272 RID: 12914 RVA: 0x001557FC File Offset: 0x001539FC
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003273 RID: 12915 RVA: 0x00155800 File Offset: 0x00153A00
		public CharacterAbilityApplyStatusEffectsToRandomAlly(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.squaredMaxDistance = MathF.Pow(abilityElement.GetAttributeFloat("maxdistance", float.MaxValue), 2f);
			this.allowDifferentSub = abilityElement.GetAttributeBool("mustbeonsamesub", true);
			this.allowSelf = abilityElement.GetAttributeBool("allowself", true);
		}

		// Token: 0x06003274 RID: 12916 RVA: 0x00155859 File Offset: 0x00153A59
		protected override void ApplyEffect()
		{
			this.ApplyEffect(base.Character);
		}

		// Token: 0x06003275 RID: 12917 RVA: 0x00155868 File Offset: 0x00153A68
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

		// Token: 0x06003276 RID: 12918 RVA: 0x001558A0 File Offset: 0x00153AA0
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

		// Token: 0x040018D7 RID: 6359
		private readonly float squaredMaxDistance;

		// Token: 0x040018D8 RID: 6360
		private readonly bool allowDifferentSub;

		// Token: 0x040018D9 RID: 6361
		private readonly bool allowSelf;
	}
}
