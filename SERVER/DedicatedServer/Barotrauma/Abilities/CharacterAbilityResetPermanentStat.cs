using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000350 RID: 848
	internal class CharacterAbilityResetPermanentStat : CharacterAbility
	{
		// Token: 0x17000E3E RID: 3646
		// (get) Token: 0x060032D7 RID: 13015 RVA: 0x001579D6 File Offset: 0x00155BD6
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000E3F RID: 3647
		// (get) Token: 0x060032D8 RID: 13016 RVA: 0x001579D9 File Offset: 0x00155BD9
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032D9 RID: 13017 RVA: 0x001579DC File Offset: 0x00155BDC
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

		// Token: 0x060032DA RID: 13018 RVA: 0x00157A75 File Offset: 0x00155C75
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x060032DB RID: 13019 RVA: 0x00157A7D File Offset: 0x00155C7D
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x060032DC RID: 13020 RVA: 0x00157A85 File Offset: 0x00155C85
		private void ApplyEffectSpecific()
		{
			Character character = base.Character;
			if (character == null)
			{
				return;
			}
			character.Info.ResetSavedStatValue(this.statIdentifier);
		}

		// Token: 0x0400192E RID: 6446
		private readonly Identifier statIdentifier;
	}
}
