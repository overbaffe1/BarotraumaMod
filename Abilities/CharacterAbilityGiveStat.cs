using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000404 RID: 1028
	internal class CharacterAbilityGiveStat : CharacterAbility
	{
		// Token: 0x060046DC RID: 18140 RVA: 0x0026E644 File Offset: 0x0026C844
		public CharacterAbilityGiveStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
		}

		// Token: 0x060046DD RID: 18141 RVA: 0x0026E695 File Offset: 0x0026C895
		public override void InitializeAbility(bool addingFirstTime)
		{
			base.Character.ChangeStat(this.statType, this.value);
		}

		// Token: 0x040024C3 RID: 9411
		private readonly StatTypes statType;

		// Token: 0x040024C4 RID: 9412
		private readonly float value;
	}
}
