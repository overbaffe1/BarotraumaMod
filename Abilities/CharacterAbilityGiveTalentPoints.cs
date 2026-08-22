using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000405 RID: 1029
	internal class CharacterAbilityGiveTalentPoints : CharacterAbility
	{
		// Token: 0x060046DE RID: 18142 RVA: 0x0026E6B0 File Offset: 0x0026C8B0
		public CharacterAbilityGiveTalentPoints(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", amount of talent points to give is 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060046DF RID: 18143 RVA: 0x0026E707 File Offset: 0x0026C907
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime && base.Character.Info != null)
			{
				base.Character.Info.AdditionalTalentPoints += this.amount;
			}
		}

		// Token: 0x040024C5 RID: 9413
		private readonly int amount;
	}
}
