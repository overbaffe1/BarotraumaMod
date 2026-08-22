using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020002ED RID: 749
	internal class AbilityConditionAffliction : AbilityConditionData
	{
		// Token: 0x060031BA RID: 12730 RVA: 0x001526AF File Offset: 0x001508AF
		public AbilityConditionAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.afflictions = conditionElement.GetAttributeStringArray("afflictions", new string[0], true);
		}

		// Token: 0x060031BB RID: 12731 RVA: 0x001526D4 File Offset: 0x001508D4
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilityAffliction abilityAffliction = abilityObject as IAbilityAffliction;
			if (abilityAffliction != null)
			{
				Affliction affliction = abilityAffliction.Affliction;
				if (affliction != null)
				{
					return this.afflictions.Any(delegate(string a)
					{
						Identifier identifier = affliction.Identifier;
						return a == identifier || a == affliction.Prefab.AfflictionType;
					});
				}
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityAttackResult));
			return false;
		}

		// Token: 0x04001881 RID: 6273
		private readonly string[] afflictions;
	}
}
