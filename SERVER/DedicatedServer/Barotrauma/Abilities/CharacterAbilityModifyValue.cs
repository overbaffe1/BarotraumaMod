using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034B RID: 843
	internal class CharacterAbilityModifyValue : CharacterAbility
	{
		// Token: 0x17000E3C RID: 3644
		// (get) Token: 0x060032C6 RID: 12998 RVA: 0x001573D8 File Offset: 0x001555D8
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032C7 RID: 12999 RVA: 0x001573DC File Offset: 0x001555DC
		public CharacterAbilityModifyValue(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.addedValue = abilityElement.GetAttributeFloat("addedvalue", 0f);
			this.multiplyValue = abilityElement.GetAttributeFloat("multiplyvalue", 1f);
			if (MathUtils.NearlyEqual(this.addedValue, 0f, 0.0001f) && MathUtils.NearlyEqual(this.multiplyValue, 1f, 0.0001f))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityModifyValue");
				defaultInterpolatedStringHandler.AppendLiteral(" - added value is 0 and multiplier is 1, the ability will do nothing.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032C8 RID: 13000 RVA: 0x001574AC File Offset: 0x001556AC
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityValue abilityValue = abilityObject as IAbilityValue;
			if (abilityValue != null)
			{
				abilityValue.Value += this.addedValue;
				abilityValue.Value *= this.multiplyValue;
			}
		}

		// Token: 0x04001924 RID: 6436
		private readonly float addedValue;

		// Token: 0x04001925 RID: 6437
		private readonly float multiplyValue;
	}
}
