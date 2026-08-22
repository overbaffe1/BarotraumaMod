using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002FE RID: 766
	internal class AbilityConditionStatusEffectIdentifier : AbilityConditionData
	{
		// Token: 0x060031E8 RID: 12776 RVA: 0x001538E4 File Offset: 0x00151AE4
		public AbilityConditionStatusEffectIdentifier(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.effectIdentifier = conditionElement.GetAttributeString("effectidentifier", "").ToLowerInvariant();
		}

		// Token: 0x060031E9 RID: 12777 RVA: 0x0015390C File Offset: 0x00151B0C
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			StatusEffect.AbilityStatusEffectIdentifier abilityStatusEffectIdentifier = abilityObject as StatusEffect.AbilityStatusEffectIdentifier;
			if (abilityStatusEffectIdentifier != null)
			{
				Identifier identifier = abilityStatusEffectIdentifier.EffectIdentifier;
				return identifier == this.effectIdentifier;
			}
			base.LogAbilityConditionError(abilityObject, typeof(StatusEffect.AbilityStatusEffectIdentifier));
			return false;
		}

		// Token: 0x0400189C RID: 6300
		private string effectIdentifier;
	}
}
