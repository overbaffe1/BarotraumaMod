using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000416 RID: 1046
	internal class CharacterAbilityResetPermanentStat : CharacterAbility
	{
		// Token: 0x1700122E RID: 4654
		// (get) Token: 0x06004711 RID: 18193 RVA: 0x0026F856 File Offset: 0x0026DA56
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700122F RID: 4655
		// (get) Token: 0x06004712 RID: 18194 RVA: 0x0026F859 File Offset: 0x0026DA59
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004713 RID: 18195 RVA: 0x0026F85C File Offset: 0x0026DA5C
		public CharacterAbilityResetPermanentStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statIdentifier = abilityElement.GetAttributeIdentifier("statidentifier", Identifier.Empty);
			if (this.statIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityResetPermanentStat");
				defaultInterpolatedStringHandler.AppendLiteral(" - statIdentifier is empty.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06004714 RID: 18196 RVA: 0x0026F8F5 File Offset: 0x0026DAF5
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x06004715 RID: 18197 RVA: 0x0026F8FD File Offset: 0x0026DAFD
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x06004716 RID: 18198 RVA: 0x0026F905 File Offset: 0x0026DB05
		private void ApplyEffectSpecific()
		{
			Character character = base.Character;
			if (character == null)
			{
				return;
			}
			character.Info.ResetSavedStatValue(this.statIdentifier);
		}

		// Token: 0x040024EF RID: 9455
		private readonly Identifier statIdentifier;
	}
}
