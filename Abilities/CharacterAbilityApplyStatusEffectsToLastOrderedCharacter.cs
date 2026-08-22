using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F5 RID: 1013
	internal class CharacterAbilityApplyStatusEffectsToLastOrderedCharacter : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x060046A7 RID: 18087 RVA: 0x0026D52F File Offset: 0x0026B72F
		public CharacterAbilityApplyStatusEffectsToLastOrderedCharacter(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x060046A8 RID: 18088 RVA: 0x0026D53C File Offset: 0x0026B73C
		protected override void ApplyEffect()
		{
			if (this.IsViableTarget(base.Character.LastOrderedCharacter))
			{
				base.ApplyEffectSpecific(base.Character.LastOrderedCharacter, null);
			}
			if (base.Character.HasAbilityFlag(AbilityFlags.AllowSecondOrderedTarget) && this.IsViableTarget(base.Character.SecondLastOrderedCharacter))
			{
				base.ApplyEffectSpecific(base.Character.SecondLastOrderedCharacter, null);
			}
		}

		// Token: 0x060046A9 RID: 18089 RVA: 0x0026D5A5 File Offset: 0x0026B7A5
		private bool IsViableTarget(Character targetCharacter)
		{
			return targetCharacter != null && !targetCharacter.Removed && targetCharacter != base.Character;
		}
	}
}
