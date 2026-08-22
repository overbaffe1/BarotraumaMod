using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F3 RID: 755
	internal abstract class AbilityConditionData : AbilityCondition
	{
		// Token: 0x060031CA RID: 12746 RVA: 0x00152E69 File Offset: 0x00151069
		public AbilityConditionData(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031CB RID: 12747 RVA: 0x00152E74 File Offset: 0x00151074
		protected void LogAbilityConditionError(AbilityObject abilityObject, Type expectedData)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(98, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Used data-reliant ability condition when data is incompatible! Expected ");
			defaultInterpolatedStringHandler.AppendFormatted<Type>(expectedData);
			defaultInterpolatedStringHandler.AppendLiteral(", but received ");
			defaultInterpolatedStringHandler.AppendFormatted<AbilityObject>(abilityObject);
			defaultInterpolatedStringHandler.AppendLiteral(" in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.characterTalent.DebugIdentifier);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.characterTalent.Prefab.ContentPackage, false, false);
		}

		// Token: 0x060031CC RID: 12748
		protected abstract bool MatchesConditionSpecific(AbilityObject abilityObject);

		// Token: 0x060031CD RID: 12749 RVA: 0x00152EF0 File Offset: 0x001510F0
		public override bool MatchesCondition()
		{
			DebugConsole.ThrowError("Used data-reliant ability condition in a state-based ability in talent " + this.characterTalent.DebugIdentifier + "! This is not allowed.", null, this.characterTalent.Prefab.ContentPackage, false, false);
			return false;
		}

		// Token: 0x060031CE RID: 12750 RVA: 0x00152F25 File Offset: 0x00151125
		public override bool MatchesCondition(AbilityObject abilityObject)
		{
			if (abilityObject == null)
			{
				return this.invert;
			}
			if (!this.invert)
			{
				return this.MatchesConditionSpecific(abilityObject);
			}
			return !this.MatchesConditionSpecific(abilityObject);
		}
	}
}
