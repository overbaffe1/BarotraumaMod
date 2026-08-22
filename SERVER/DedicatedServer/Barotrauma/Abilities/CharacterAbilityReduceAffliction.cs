using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034D RID: 845
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityReduceAffliction : CharacterAbility
	{
		// Token: 0x060032CC RID: 13004 RVA: 0x001576E8 File Offset: 0x001558E8
		public CharacterAbilityReduceAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionId = abilityElement.GetAttributeIdentifier("afflictionid", abilityElement.GetAttributeIdentifier("affliction", Identifier.Empty));
			this.amount = abilityElement.GetAttributeFloat("amount", 0f);
			if (this.afflictionId.IsEmpty)
			{
				DebugConsole.ThrowError("Error in CharacterAbilityReduceAffliction - affliction identifier not set.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032CD RID: 13005 RVA: 0x00157754 File Offset: 0x00155954
		protected override void ApplyEffect()
		{
			this.ApplyEffectToCharacter(base.Character);
		}

		// Token: 0x060032CE RID: 13006 RVA: 0x00157764 File Offset: 0x00155964
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter characterData = abilityObject as IAbilityCharacter;
			if (characterData != null)
			{
				this.ApplyEffectToCharacter(characterData.Character);
			}
		}

		// Token: 0x060032CF RID: 13007 RVA: 0x00157788 File Offset: 0x00155988
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

		// Token: 0x060032D0 RID: 13008 RVA: 0x001577C0 File Offset: 0x001559C0
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x04001928 RID: 6440
		private readonly Identifier afflictionId;

		// Token: 0x04001929 RID: 6441
		private readonly float amount;
	}
}
