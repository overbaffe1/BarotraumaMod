using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002FD RID: 765
	internal class AbilityConditionSkill : AbilityConditionData
	{
		// Token: 0x060031E5 RID: 12773 RVA: 0x00153876 File Offset: 0x00151A76
		public AbilityConditionSkill(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.skillIdentifier = conditionElement.GetAttributeString("skillidentifier", "").ToLowerInvariant();
		}

		// Token: 0x060031E6 RID: 12774 RVA: 0x0015389B File Offset: 0x00151A9B
		private bool MatchesConditionSpecific(Identifier skillIdentifier)
		{
			return this.skillIdentifier == skillIdentifier;
		}

		// Token: 0x060031E7 RID: 12775 RVA: 0x001538AC File Offset: 0x00151AAC
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

		// Token: 0x0400189B RID: 6299
		private readonly string skillIdentifier;
	}
}
