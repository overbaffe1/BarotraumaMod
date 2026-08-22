using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B9 RID: 953
	internal abstract class AbilityConditionData : AbilityCondition
	{
		// Token: 0x06004604 RID: 17924 RVA: 0x0026ACE9 File Offset: 0x00268EE9
		public AbilityConditionData(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004605 RID: 17925 RVA: 0x0026ACF4 File Offset: 0x00268EF4
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

		// Token: 0x06004606 RID: 17926
		protected abstract bool MatchesConditionSpecific(AbilityObject abilityObject);

		// Token: 0x06004607 RID: 17927 RVA: 0x0026AD70 File Offset: 0x00268F70
		public override bool MatchesCondition()
		{
			DebugConsole.ThrowError("Used data-reliant ability condition in a state-based ability in talent " + this.characterTalent.DebugIdentifier + "! This is not allowed.", null, this.characterTalent.Prefab.ContentPackage, false, false);
			return false;
		}

		// Token: 0x06004608 RID: 17928 RVA: 0x0026ADA5 File Offset: 0x00268FA5
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
