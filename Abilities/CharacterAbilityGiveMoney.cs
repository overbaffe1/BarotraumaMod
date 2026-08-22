using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FF RID: 1023
	internal class CharacterAbilityGiveMoney : CharacterAbility
	{
		// Token: 0x17001222 RID: 4642
		// (get) Token: 0x060046CA RID: 18122 RVA: 0x0026DF9C File Offset: 0x0026C19C
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046CB RID: 18123 RVA: 0x0026DFA0 File Offset: 0x0026C1A0
		public CharacterAbilityGiveMoney(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			this.scalingStatIdentifier = abilityElement.GetAttributeIdentifier("scalingstatidentifier", Identifier.Empty);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", CharacterAbilityGiveMoney - amount of money set to 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060046CC RID: 18124 RVA: 0x0026E010 File Offset: 0x0026C210
		private void ApplyEffectSpecific(Character targetCharacter)
		{
			float multiplier = 1f;
			if (!this.scalingStatIdentifier.IsEmpty)
			{
				multiplier = 0f + base.Character.Info.GetSavedStatValue(StatTypes.None, this.scalingStatIdentifier);
			}
			int totalAmount = (int)(multiplier * (float)this.amount);
			targetCharacter.GiveMoney(totalAmount);
			GameAnalyticsManager.AddMoneyGainedEvent(totalAmount, GameAnalyticsManager.MoneySource.Ability, base.CharacterTalent.Prefab.Identifier.Value);
		}

		// Token: 0x060046CD RID: 18125 RVA: 0x0026E07C File Offset: 0x0026C27C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character targetCharacter = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (targetCharacter != null)
			{
				this.ApplyEffectSpecific(targetCharacter);
				return;
			}
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x060046CE RID: 18126 RVA: 0x0026E0B3 File Offset: 0x0026C2B3
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x040024AF RID: 9391
		private readonly int amount;

		// Token: 0x040024B0 RID: 9392
		private readonly Identifier scalingStatIdentifier;
	}
}
