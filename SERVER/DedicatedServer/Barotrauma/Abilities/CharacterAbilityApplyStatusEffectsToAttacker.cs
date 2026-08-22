using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032E RID: 814
	internal class CharacterAbilityApplyStatusEffectsToAttacker : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x0600326B RID: 12907 RVA: 0x00155678 File Offset: 0x00153878
		public CharacterAbilityApplyStatusEffectsToAttacker(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x0600326C RID: 12908 RVA: 0x00155684 File Offset: 0x00153884
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
