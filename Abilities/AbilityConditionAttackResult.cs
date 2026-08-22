using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B5 RID: 949
	internal class AbilityConditionAttackResult : AbilityConditionData
	{
		// Token: 0x060045F9 RID: 17913 RVA: 0x0026A990 File Offset: 0x00268B90
		public AbilityConditionAttackResult(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.targetTypes = base.ParseTargetTypes(conditionElement.GetAttributeStringArray("targettypes", Array.Empty<string>(), false));
			this.afflictions = conditionElement.GetAttributeIdentifierArray("afflictions", Array.Empty<Identifier>(), true);
		}

		// Token: 0x060045FA RID: 17914 RVA: 0x0026A9D0 File Offset: 0x00268BD0
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilityAttackResult abilityAttackResult = abilityObject as IAbilityAttackResult;
			AttackResult? attackResult2 = (abilityAttackResult != null) ? new AttackResult?(abilityAttackResult.AttackResult) : null;
			if (attackResult2 != null)
			{
				AttackResult attackResult = attackResult2.GetValueOrDefault();
				IEnumerable<AbilityCondition.TargetType> enumerable = this.targetTypes;
				Limb hitLimb = attackResult.HitLimb;
				return base.IsViableTarget(enumerable, (hitLimb != null) ? hitLimb.character : null) && (!this.afflictions.Any<Identifier>() || (attackResult.Afflictions != null && this.afflictions.Any((Identifier a) => (from c in attackResult.Afflictions
				select c.Identifier).Contains(a))));
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityAttackResult));
			return false;
		}

		// Token: 0x04002449 RID: 9289
		private readonly List<AbilityCondition.TargetType> targetTypes;

		// Token: 0x0400244A RID: 9290
		private readonly Identifier[] afflictions;
	}
}
