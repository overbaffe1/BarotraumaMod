using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041D RID: 1053
	internal class CharacterAbilityBountyHunter : CharacterAbility
	{
		// Token: 0x0600472A RID: 18218 RVA: 0x0026FFDF File Offset: 0x0026E1DF
		public CharacterAbilityBountyHunter(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.vitalityPercentage = abilityElement.GetAttributeFloat("vitalitypercentage", 0f);
		}

		// Token: 0x0600472B RID: 18219 RVA: 0x00270000 File Offset: 0x0026E200
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

		// Token: 0x04002501 RID: 9473
		private readonly float vitalityPercentage;
	}
}
