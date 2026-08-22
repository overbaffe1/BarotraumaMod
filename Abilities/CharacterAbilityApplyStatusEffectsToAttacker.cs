using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F4 RID: 1012
	internal class CharacterAbilityApplyStatusEffectsToAttacker : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x060046A5 RID: 18085 RVA: 0x0026D4F8 File Offset: 0x0026B6F8
		public CharacterAbilityApplyStatusEffectsToAttacker(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x060046A6 RID: 18086 RVA: 0x0026D504 File Offset: 0x0026B704
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilityAttackData abilityAttackData = abilityObject as AbilityAttackData;
			Character attacker = (abilityAttackData != null) ? abilityAttackData.Attacker : null;
			if (attacker != null)
			{
				base.ApplyEffectSpecific(attacker, null);
			}
		}
	}
}
