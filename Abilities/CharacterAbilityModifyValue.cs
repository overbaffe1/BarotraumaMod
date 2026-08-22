using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000411 RID: 1041
	internal class CharacterAbilityModifyValue : CharacterAbility
	{
		// Token: 0x1700122C RID: 4652
		// (get) Token: 0x06004700 RID: 18176 RVA: 0x0026F258 File Offset: 0x0026D458
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004701 RID: 18177 RVA: 0x0026F25C File Offset: 0x0026D45C
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

		// Token: 0x06004702 RID: 18178 RVA: 0x0026F32C File Offset: 0x0026D52C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityValue abilityValue = abilityObject as IAbilityValue;
			if (abilityValue != null)
			{
				abilityValue.Value += this.addedValue;
				abilityValue.Value *= this.multiplyValue;
			}
		}

		// Token: 0x040024E5 RID: 9445
		private readonly float addedValue;

		// Token: 0x040024E6 RID: 9446
		private readonly float multiplyValue;
	}
}
