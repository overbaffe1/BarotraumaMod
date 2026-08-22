using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000413 RID: 1043
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityReduceAffliction : CharacterAbility
	{
		// Token: 0x06004706 RID: 18182 RVA: 0x0026F568 File Offset: 0x0026D768
		public CharacterAbilityReduceAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionId = abilityElement.GetAttributeIdentifier("afflictionid", abilityElement.GetAttributeIdentifier("affliction", Identifier.Empty));
			this.amount = abilityElement.GetAttributeFloat("amount", 0f);
			if (this.afflictionId.IsEmpty)
			{
				DebugConsole.ThrowError("Error in CharacterAbilityReduceAffliction - affliction identifier not set.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06004707 RID: 18183 RVA: 0x0026F5D4 File Offset: 0x0026D7D4
		protected override void ApplyEffect()
		{
			this.ApplyEffectToCharacter(base.Character);
		}

		// Token: 0x06004708 RID: 18184 RVA: 0x0026F5E4 File Offset: 0x0026D7E4
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter characterData = abilityObject as IAbilityCharacter;
			if (characterData != null)
			{
				this.ApplyEffectToCharacter(characterData.Character);
			}
		}

		// Token: 0x06004709 RID: 18185 RVA: 0x0026F608 File Offset: 0x0026D808
		private void ApplyEffectToCharacter(Character character)
		{
			if (character != null)
			{
				CharacterHealth characterHealth = character.CharacterHealth;
				Identifier afflictionIdOrType = this.afflictionId;
				float num = this.amount;
				Character character2 = base.Character;
				characterHealth.ReduceAfflictionOnAllLimbs(afflictionIdOrType, num, null, character2);
			}
		}

		// Token: 0x0600470A RID: 18186 RVA: 0x0026F640 File Offset: 0x0026D840
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x040024E9 RID: 9449
		private readonly Identifier afflictionId;

		// Token: 0x040024EA RID: 9450
		private readonly float amount;
	}
}
