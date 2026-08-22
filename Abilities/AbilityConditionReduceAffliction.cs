using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C2 RID: 962
	internal class AbilityConditionReduceAffliction : AbilityConditionData
	{
		// Token: 0x0600461D RID: 17949 RVA: 0x0026B62E File Offset: 0x0026982E
		public AbilityConditionReduceAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.allowedTypes = conditionElement.GetAttributeStringArray("allowedtypes", Array.Empty<string>(), true);
			this.identifier = conditionElement.GetAttributeString("identifier", "");
		}

		// Token: 0x0600461E RID: 17950 RVA: 0x0026B668 File Offset: 0x00269868
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			AbilityConditionReduceAffliction.<>c__DisplayClass3_0 CS$<>8__locals1 = new AbilityConditionReduceAffliction.<>c__DisplayClass3_0();
			AbilityConditionReduceAffliction.<>c__DisplayClass3_0 CS$<>8__locals2 = CS$<>8__locals1;
			IAbilityAffliction abilityAffliction = abilityObject as IAbilityAffliction;
			CS$<>8__locals2.affliction = ((abilityAffliction != null) ? abilityAffliction.Affliction : null);
			if (CS$<>8__locals1.affliction != null)
			{
				return this.allowedTypes.Find((string c) => c == CS$<>8__locals1.affliction.Prefab.AfflictionType) != null && (string.IsNullOrEmpty(this.identifier) || !(CS$<>8__locals1.affliction.Prefab.Identifier != this.identifier));
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityAffliction));
			return false;
		}

		// Token: 0x0400245A RID: 9306
		private readonly string[] allowedTypes;

		// Token: 0x0400245B RID: 9307
		private readonly string identifier;
	}
}
