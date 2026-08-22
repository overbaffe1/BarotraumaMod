using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C3 RID: 963
	internal class AbilityConditionSkill : AbilityConditionData
	{
		// Token: 0x0600461F RID: 17951 RVA: 0x0026B6F6 File Offset: 0x002698F6
		public AbilityConditionSkill(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.skillIdentifier = conditionElement.GetAttributeString("skillidentifier", "").ToLowerInvariant();
		}

		// Token: 0x06004620 RID: 17952 RVA: 0x0026B71B File Offset: 0x0026991B
		private bool MatchesConditionSpecific(Identifier skillIdentifier)
		{
			return this.skillIdentifier == skillIdentifier;
		}

		// Token: 0x06004621 RID: 17953 RVA: 0x0026B72C File Offset: 0x0026992C
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilitySkillIdentifier abilitySkillIdentifier = abilityObject as IAbilitySkillIdentifier;
			if (abilitySkillIdentifier != null)
			{
				Identifier skillIdentifier = abilitySkillIdentifier.SkillIdentifier;
				return this.MatchesConditionSpecific(skillIdentifier);
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilitySkillIdentifier));
			return false;
		}

		// Token: 0x0400245C RID: 9308
		private readonly string skillIdentifier;
	}
}
