using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F5 RID: 757
	internal class AbilityConditionGeneHarvester : AbilityConditionData
	{
		// Token: 0x060031D1 RID: 12753 RVA: 0x00152FDD File Offset: 0x001511DD
		public AbilityConditionGeneHarvester(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031D2 RID: 12754 RVA: 0x00152FE8 File Offset: 0x001511E8
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			AbilityCharacterKill abilityCharacterKill = abilityObject as AbilityCharacterKill;
			if (abilityCharacterKill != null)
			{
				return abilityCharacterKill.Killer.Submarine == null || abilityCharacterKill.Killer.TeamID != abilityCharacterKill.Killer.Submarine.TeamID;
			}
			base.LogAbilityConditionError(abilityObject, typeof(AbilityCharacterKill));
			return false;
		}
	}
}
