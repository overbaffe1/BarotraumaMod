using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041E RID: 1054
	internal class CharacterAbilityByTheBook : CharacterAbility
	{
		// Token: 0x0600472C RID: 18220 RVA: 0x0027005A File Offset: 0x0026E25A
		public CharacterAbilityByTheBook(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.moneyAmount = abilityElement.GetAttributeInt("moneyamount", 0);
			this.experienceAmount = abilityElement.GetAttributeInt("experienceamount", 0);
			this.max = abilityElement.GetAttributeInt("max", 0);
		}

		// Token: 0x0600472D RID: 18221 RVA: 0x0027009C File Offset: 0x0026E29C
		protected override void ApplyEffect()
		{
			IEnumerable<Character> enemyCharacters = from c in Character.CharacterList
			where !base.Character.IsFriendly(c)
			select c;
			int timesGiven = 0;
			foreach (Character enemyCharacter in enemyCharacters)
			{
				if (enemyCharacter.IsHuman && enemyCharacter.Submarine != null && (Submarine.MainSub == null || enemyCharacter.Submarine == Submarine.MainSub) && !enemyCharacter.IsDead && enemyCharacter.LockHands)
				{
					base.Character.GiveMoney(this.moneyAmount);
					GameAnalyticsManager.AddMoneyGainedEvent(this.moneyAmount, GameAnalyticsManager.MoneySource.Ability, base.CharacterTalent.Prefab.Identifier.Value);
					foreach (Character character in Character.GetFriendlyCrew(base.Character))
					{
						character.Info.GiveExperience(this.experienceAmount);
					}
					timesGiven++;
					if (this.max > 0 && timesGiven >= this.max)
					{
						break;
					}
				}
			}
		}

		// Token: 0x04002502 RID: 9474
		private readonly int moneyAmount;

		// Token: 0x04002503 RID: 9475
		private readonly int experienceAmount;

		// Token: 0x04002504 RID: 9476
		private readonly int max;
	}
}
