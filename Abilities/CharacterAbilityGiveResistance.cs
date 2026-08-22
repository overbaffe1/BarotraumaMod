using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000403 RID: 1027
	internal class CharacterAbilityGiveResistance : CharacterAbility
	{
		// Token: 0x060046DA RID: 18138 RVA: 0x0026E560 File Offset: 0x0026C760
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

		// Token: 0x060046DB RID: 18139 RVA: 0x0026E608 File Offset: 0x0026C808
		public override void InitializeAbility(bool addingFirstTime)
		{
			TalentResistanceIdentifier identifier = new TalentResistanceIdentifier(this.resistanceId, base.CharacterTalent.Prefab.Identifier);
			base.Character.ChangeAbilityResistance(identifier, this.multiplier);
		}

		// Token: 0x040024C1 RID: 9409
		private readonly Identifier resistanceId;

		// Token: 0x040024C2 RID: 9410
		private readonly float multiplier;
	}
}
