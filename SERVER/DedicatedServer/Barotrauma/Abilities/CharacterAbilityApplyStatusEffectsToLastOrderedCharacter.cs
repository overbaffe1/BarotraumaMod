using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032F RID: 815
	internal class CharacterAbilityApplyStatusEffectsToLastOrderedCharacter : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x0600326D RID: 12909 RVA: 0x001556AF File Offset: 0x001538AF
		public CharacterAbilityApplyStatusEffectsToLastOrderedCharacter(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x0600326E RID: 12910 RVA: 0x001556BC File Offset: 0x001538BC
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

		// Token: 0x0600326F RID: 12911 RVA: 0x00155725 File Offset: 0x00153925
		private bool IsViableTarget(Character targetCharacter)
		{
			return targetCharacter != null && !targetCharacter.Removed && targetCharacter != base.Character;
		}
	}
}
