using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C4 RID: 964
	internal class AbilityConditionStatusEffectIdentifier : AbilityConditionData
	{
		// Token: 0x06004622 RID: 17954 RVA: 0x0026B764 File Offset: 0x00269964
		public AbilityConditionStatusEffectIdentifier(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.effectIdentifier = conditionElement.GetAttributeString("effectidentifier", "").ToLowerInvariant();
		}

		// Token: 0x06004623 RID: 17955 RVA: 0x0026B78C File Offset: 0x0026998C
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

		// Token: 0x0400245D RID: 9309
		private string effectIdentifier;
	}
}
