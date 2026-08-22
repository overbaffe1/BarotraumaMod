using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000339 RID: 825
	internal class CharacterAbilityGiveMoney : CharacterAbility
	{
		// Token: 0x17000E32 RID: 3634
		// (get) Token: 0x06003290 RID: 12944 RVA: 0x0015611C File Offset: 0x0015431C
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06003291 RID: 12945 RVA: 0x00156120 File Offset: 0x00154320
		public CharacterAbilityGiveMoney(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			this.scalingStatIdentifier = abilityElement.GetAttributeIdentifier("scalingstatidentifier", Identifier.Empty);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", CharacterAbilityGiveMoney - amount of money set to 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06003292 RID: 12946 RVA: 0x00156190 File Offset: 0x00154390
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

		// Token: 0x06003293 RID: 12947 RVA: 0x001561FC File Offset: 0x001543FC
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

		// Token: 0x06003294 RID: 12948 RVA: 0x00156233 File Offset: 0x00154433
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x040018EE RID: 6382
		private readonly int amount;

		// Token: 0x040018EF RID: 6383
		private readonly Identifier scalingStatIdentifier;
	}
}
