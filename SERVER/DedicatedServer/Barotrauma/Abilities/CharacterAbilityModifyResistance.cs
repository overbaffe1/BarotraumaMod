using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000346 RID: 838
	internal class CharacterAbilityModifyResistance : CharacterAbility
	{
		// Token: 0x17000E37 RID: 3639
		// (get) Token: 0x060032B6 RID: 12982 RVA: 0x00156E2F File Offset: 0x0015502F
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032B7 RID: 12983 RVA: 0x00156E34 File Offset: 0x00155034
		public CharacterAbilityModifyResistance(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.resistanceId = abilityElement.GetAttributeIdentifier("resistanceid", abilityElement.GetAttributeIdentifier("resistance", Identifier.Empty));
			this.multiplier = abilityElement.GetAttributeFloat("multiplier", 1f);
			if (this.resistanceId.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" - resistance identifier not set in ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityModifyResistance");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
			if (MathUtils.NearlyEqual(this.multiplier, 1f, 0.0001f))
			{
				DebugConsole.AddWarning("Possible error in talent " + base.CharacterTalent.DebugIdentifier + " - resistance set to 1, which will do nothing.", abilityElement.ContentPackage);
			}
		}

		// Token: 0x060032B8 RID: 12984 RVA: 0x00156F2C File Offset: 0x0015512C
		public override void UpdateCharacterAbility(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched != this.lastState)
			{
				TalentResistanceIdentifier identifier = new TalentResistanceIdentifier(this.resistanceId, base.CharacterTalent.Prefab.Identifier);
				if (conditionsMatched)
				{
					base.Character.ChangeAbilityResistance(identifier, this.multiplier);
				}
				else
				{
					base.Character.RemoveAbilityResistance(identifier);
				}
				this.lastState = conditionsMatched;
			}
		}

		// Token: 0x04001912 RID: 6418
		private readonly Identifier resistanceId;

		// Token: 0x04001913 RID: 6419
		private readonly float multiplier;

		// Token: 0x04001914 RID: 6420
		private bool lastState;
	}
}
