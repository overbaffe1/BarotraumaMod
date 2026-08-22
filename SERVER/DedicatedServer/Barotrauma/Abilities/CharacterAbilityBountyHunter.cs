using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000357 RID: 855
	internal class CharacterAbilityBountyHunter : CharacterAbility
	{
		// Token: 0x060032F0 RID: 13040 RVA: 0x0015815F File Offset: 0x0015635F
		public CharacterAbilityBountyHunter(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.vitalityPercentage = abilityElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x060032F1 RID: 13041 RVA: 0x00158180 File Offset: 0x00156380
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character character = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (character != null)
			{
				int totalAmount = (int)(this.vitalityPercentage * character.MaxVitality);
				base.Character.GiveMoney(totalAmount);
				GameAnalyticsManager.AddMoneyGainedEvent(totalAmount, GameAnalyticsManager.MoneySource.Ability, base.CharacterTalent.Prefab.Identifier.Value);
			}
		}

		// Token: 0x04001940 RID: 6464
		private readonly float vitalityPercentage;
	}
}
