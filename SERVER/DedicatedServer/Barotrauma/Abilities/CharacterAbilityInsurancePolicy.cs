using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000359 RID: 857
	internal class CharacterAbilityInsurancePolicy : CharacterAbility
	{
		// Token: 0x17000E41 RID: 3649
		// (get) Token: 0x060032F5 RID: 13045 RVA: 0x0015836D File Offset: 0x0015656D
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032F6 RID: 13046 RVA: 0x00158370 File Offset: 0x00156570
		public CharacterAbilityInsurancePolicy(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.moneyPerMission = abilityElement.GetAttributeInt("moneypermission", 0);
		}

		// Token: 0x060032F7 RID: 13047 RVA: 0x0015838C File Offset: 0x0015658C
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

		// Token: 0x04001944 RID: 6468
		private readonly int moneyPerMission;
	}
}
