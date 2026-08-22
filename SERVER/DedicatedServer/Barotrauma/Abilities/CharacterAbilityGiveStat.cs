using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200033E RID: 830
	internal class CharacterAbilityGiveStat : CharacterAbility
	{
		// Token: 0x060032A2 RID: 12962 RVA: 0x001567C4 File Offset: 0x001549C4
		public CharacterAbilityGiveStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
		}

		// Token: 0x060032A3 RID: 12963 RVA: 0x00156815 File Offset: 0x00154A15
		public override void InitializeAbility(bool addingFirstTime)
		{
			base.Character.ChangeStat(this.statType, this.value);
		}

		// Token: 0x04001902 RID: 6402
		private readonly StatTypes statType;

		// Token: 0x04001903 RID: 6403
		private readonly float value;
	}
}
