using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200033F RID: 831
	internal class CharacterAbilityGiveTalentPoints : CharacterAbility
	{
		// Token: 0x060032A4 RID: 12964 RVA: 0x00156830 File Offset: 0x00154A30
		public CharacterAbilityGiveTalentPoints(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", amount of talent points to give is 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032A5 RID: 12965 RVA: 0x00156887 File Offset: 0x00154A87
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime && base.Character.Info != null)
			{
				base.Character.Info.AdditionalTalentPoints += this.amount;
			}
		}

		// Token: 0x04001904 RID: 6404
		private readonly int amount;
	}
}
