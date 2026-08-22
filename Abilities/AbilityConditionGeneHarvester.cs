using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BB RID: 955
	internal class AbilityConditionGeneHarvester : AbilityConditionData
	{
		// Token: 0x0600460B RID: 17931 RVA: 0x0026AE5D File Offset: 0x0026905D
		public AbilityConditionGeneHarvester(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x0600460C RID: 17932 RVA: 0x0026AE68 File Offset: 0x00269068
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
