using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040D RID: 1037
	internal class CharacterAbilityModifyStat : CharacterAbility
	{
		// Token: 0x17001228 RID: 4648
		// (get) Token: 0x060046F3 RID: 18163 RVA: 0x0026EE09 File Offset: 0x0026D009
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046F4 RID: 18164 RVA: 0x0026EE0C File Offset: 0x0026D00C
		public CharacterAbilityModifyStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
		}

		// Token: 0x060046F5 RID: 18165 RVA: 0x0026EE5D File Offset: 0x0026D05D
		public override void InitializeAbility(bool addingFirstTime)
		{
			this.VerifyState(true, 0f);
		}

		// Token: 0x060046F6 RID: 18166 RVA: 0x0026EE6B File Offset: 0x0026D06B
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched != this.lastState)
			{
				base.Character.ChangeStat(this.statType, conditionsMatched ? this.value : (-this.value));
				this.lastState = conditionsMatched;
			}
		}

		// Token: 0x040024D6 RID: 9430
		private readonly StatTypes statType;

		// Token: 0x040024D7 RID: 9431
		private readonly float value;

		// Token: 0x040024D8 RID: 9432
		private bool lastState;
	}
}
