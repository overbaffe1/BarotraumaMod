using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020002EF RID: 751
	internal class AbilityConditionAttackResult : AbilityConditionData
	{
		// Token: 0x060031BF RID: 12735 RVA: 0x00152B10 File Offset: 0x00150D10
		public AbilityConditionAttackResult(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.targetTypes = base.ParseTargetTypes(conditionElement.GetAttributeStringArray("targettypes", Array.Empty<string>(), false));
			this.afflictions = conditionElement.GetAttributeIdentifierArray("afflictions", Array.Empty<Identifier>(), true);
		}

		// Token: 0x060031C0 RID: 12736 RVA: 0x00152B50 File Offset: 0x00150D50
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

		// Token: 0x04001888 RID: 6280
		private readonly List<AbilityCondition.TargetType> targetTypes;

		// Token: 0x04001889 RID: 6281
		private readonly Identifier[] afflictions;
	}
}
