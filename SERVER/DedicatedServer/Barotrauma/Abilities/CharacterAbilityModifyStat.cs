using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000347 RID: 839
	internal class CharacterAbilityModifyStat : CharacterAbility
	{
		// Token: 0x17000E38 RID: 3640
		// (get) Token: 0x060032B9 RID: 12985 RVA: 0x00156F89 File Offset: 0x00155189
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032BA RID: 12986 RVA: 0x00156F8C File Offset: 0x0015518C
		public CharacterAbilityModifyStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
		}

		// Token: 0x060032BB RID: 12987 RVA: 0x00156FDD File Offset: 0x001551DD
		public override void InitializeAbility(bool addingFirstTime)
		{
			this.VerifyState(true, 0f);
		}

		// Token: 0x060032BC RID: 12988 RVA: 0x00156FEB File Offset: 0x001551EB
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched != this.lastState)
			{
				base.Character.ChangeStat(this.statType, conditionsMatched ? this.value : (-this.value));
				this.lastState = conditionsMatched;
			}
		}

		// Token: 0x04001915 RID: 6421
		private readonly StatTypes statType;

		// Token: 0x04001916 RID: 6422
		private readonly float value;

		// Token: 0x04001917 RID: 6423
		private bool lastState;
	}
}
