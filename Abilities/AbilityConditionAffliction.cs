using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B3 RID: 947
	internal class AbilityConditionAffliction : AbilityConditionData
	{
		// Token: 0x060045F4 RID: 17908 RVA: 0x0026A52F File Offset: 0x0026872F
		public AbilityConditionAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.afflictions = conditionElement.GetAttributeStringArray("afflictions", new string[0], true);
		}

		// Token: 0x060045F5 RID: 17909 RVA: 0x0026A554 File Offset: 0x00268754
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

		// Token: 0x04002442 RID: 9282
		private readonly string[] afflictions;
	}
}
