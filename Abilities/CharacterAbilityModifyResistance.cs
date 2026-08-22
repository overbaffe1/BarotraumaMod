using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040C RID: 1036
	internal class CharacterAbilityModifyResistance : CharacterAbility
	{
		// Token: 0x17001227 RID: 4647
		// (get) Token: 0x060046F0 RID: 18160 RVA: 0x0026ECAF File Offset: 0x0026CEAF
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046F1 RID: 18161 RVA: 0x0026ECB4 File Offset: 0x0026CEB4
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

		// Token: 0x060046F2 RID: 18162 RVA: 0x0026EDAC File Offset: 0x0026CFAC
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

		// Token: 0x040024D3 RID: 9427
		private readonly Identifier resistanceId;

		// Token: 0x040024D4 RID: 9428
		private readonly float multiplier;

		// Token: 0x040024D5 RID: 9429
		private bool lastState;
	}
}
