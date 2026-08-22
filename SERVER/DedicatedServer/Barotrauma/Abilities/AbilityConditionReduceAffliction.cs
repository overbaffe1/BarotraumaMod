using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002FC RID: 764
	internal class AbilityConditionReduceAffliction : AbilityConditionData
	{
		// Token: 0x060031E3 RID: 12771 RVA: 0x001537AE File Offset: 0x001519AE
		public AbilityConditionReduceAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.allowedTypes = conditionElement.GetAttributeStringArray("allowedtypes", Array.Empty<string>(), true);
			this.identifier = conditionElement.GetAttributeString("identifier", "");
		}

		// Token: 0x060031E4 RID: 12772 RVA: 0x001537E8 File Offset: 0x001519E8
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

		// Token: 0x04001899 RID: 6297
		private readonly string[] allowedTypes;

		// Token: 0x0400189A RID: 6298
		private readonly string identifier;
	}
}
