using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DE RID: 990
	internal class AbilityConditionOnMission : AbilityConditionDataless
	{
		// Token: 0x0600465D RID: 18013 RVA: 0x0026C5DC File Offset: 0x0026A7DC
		public AbilityConditionOnMission(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x0600465E RID: 18014 RVA: 0x0026C5E6 File Offset: 0x0026A7E6
		protected override bool MatchesConditionSpecific()
		{
			Level loaded = Level.Loaded;
			return loaded == null || loaded.Type != LevelData.LevelType.Outpost;
		}
	}
}
