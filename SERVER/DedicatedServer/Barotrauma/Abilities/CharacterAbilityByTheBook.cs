using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000358 RID: 856
	internal class CharacterAbilityByTheBook : CharacterAbility
	{
		// Token: 0x060032F2 RID: 13042 RVA: 0x001581DA File Offset: 0x001563DA
		public CharacterAbilityByTheBook(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.moneyAmount = abilityElement.GetAttributeInt("moneyamount", 0);
			this.experienceAmount = abilityElement.GetAttributeInt("experienceamount", 0);
			this.max = abilityElement.GetAttributeInt("max", 0);
		}

		// Token: 0x060032F3 RID: 13043 RVA: 0x0015821C File Offset: 0x0015641C
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

		// Token: 0x04001941 RID: 6465
		private readonly int moneyAmount;

		// Token: 0x04001942 RID: 6466
		private readonly int experienceAmount;

		// Token: 0x04001943 RID: 6467
		private readonly int max;
	}
}
