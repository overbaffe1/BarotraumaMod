using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200033D RID: 829
	internal class CharacterAbilityGiveResistance : CharacterAbility
	{
		// Token: 0x060032A0 RID: 12960 RVA: 0x001566E0 File Offset: 0x001548E0
		public CharacterAbilityGiveResistance(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.resistanceId = abilityElement.GetAttributeIdentifier("resistanceid", abilityElement.GetAttributeIdentifier("resistance", Identifier.Empty));
			this.multiplier = abilityElement.GetAttributeFloat("multiplier", 1f);
			if (this.resistanceId.IsEmpty)
			{
				DebugConsole.ThrowError("Error in CharacterAbilityGiveResistance - resistance identifier not set.", null, abilityElement.ContentPackage, false, false);
			}
			if (MathUtils.NearlyEqual(this.multiplier, 1f, 0.0001f))
			{
				DebugConsole.AddWarning("Possible error in talent " + base.CharacterTalent.DebugIdentifier + " - multiplier set to 1, which will do nothing.", abilityElement.ContentPackage);
			}
		}

		// Token: 0x060032A1 RID: 12961 RVA: 0x00156788 File Offset: 0x00154988
		public override void InitializeAbility(bool addingFirstTime)
		{
			TalentResistanceIdentifier identifier = new TalentResistanceIdentifier(this.resistanceId, base.CharacterTalent.Prefab.Identifier);
			base.Character.ChangeAbilityResistance(identifier, this.multiplier);
		}

		// Token: 0x04001900 RID: 6400
		private readonly Identifier resistanceId;

		// Token: 0x04001901 RID: 6401
		private readonly float multiplier;
	}
}
