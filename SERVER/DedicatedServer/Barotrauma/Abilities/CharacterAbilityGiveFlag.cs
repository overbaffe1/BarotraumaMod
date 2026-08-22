using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000336 RID: 822
	internal class CharacterAbilityGiveFlag : CharacterAbility
	{
		// Token: 0x06003287 RID: 12935 RVA: 0x00155E9D File Offset: 0x0015409D
		public CharacterAbilityGiveFlag(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.abilityFlag = CharacterAbilityGroup.ParseFlagType(abilityElement.GetAttributeString("flagtype", ""), base.CharacterTalent.DebugIdentifier);
		}

		// Token: 0x06003288 RID: 12936 RVA: 0x00155ECD File Offset: 0x001540CD
		public override void InitializeAbility(bool addingFirstTime)
		{
			base.Character.AddAbilityFlag(this.abilityFlag);
		}

		// Token: 0x040018E4 RID: 6372
		private readonly AbilityFlags abilityFlag;
	}
}
