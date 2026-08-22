using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FC RID: 1020
	internal class CharacterAbilityGiveFlag : CharacterAbility
	{
		// Token: 0x060046C1 RID: 18113 RVA: 0x0026DD1D File Offset: 0x0026BF1D
		public CharacterAbilityGiveFlag(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.abilityFlag = CharacterAbilityGroup.ParseFlagType(abilityElement.GetAttributeString("flagtype", ""), base.CharacterTalent.DebugIdentifier);
		}

		// Token: 0x060046C2 RID: 18114 RVA: 0x0026DD4D File Offset: 0x0026BF4D
		public override void InitializeAbility(bool addingFirstTime)
		{
			base.Character.AddAbilityFlag(this.abilityFlag);
		}

		// Token: 0x040024A5 RID: 9381
		private readonly AbilityFlags abilityFlag;
	}
}
