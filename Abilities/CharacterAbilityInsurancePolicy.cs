using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041F RID: 1055
	internal class CharacterAbilityInsurancePolicy : CharacterAbility
	{
		// Token: 0x17001231 RID: 4657
		// (get) Token: 0x0600472F RID: 18223 RVA: 0x002701ED File Offset: 0x0026E3ED
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004730 RID: 18224 RVA: 0x002701F0 File Offset: 0x0026E3F0
		public CharacterAbilityInsurancePolicy(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.moneyPerMission = abilityElement.GetAttributeInt("moneypermission", 0);
		}

		// Token: 0x06004731 RID: 18225 RVA: 0x0027020C File Offset: 0x0026E40C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			Character character = base.Character;
			CharacterInfo info = (character != null) ? character.Info : null;
			if (info != null)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null)
				{
					int totalAmount = this.moneyPerMission * info.MissionsCompletedSinceDeath;
					campaign.Bank.Give(totalAmount);
					GameAnalyticsManager.AddMoneyGainedEvent(totalAmount, GameAnalyticsManager.MoneySource.Ability, base.CharacterTalent.Prefab.Identifier.Value);
				}
			}
		}

		// Token: 0x04002505 RID: 9477
		private readonly int moneyPerMission;
	}
}
