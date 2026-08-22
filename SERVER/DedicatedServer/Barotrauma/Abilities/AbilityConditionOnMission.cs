using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000318 RID: 792
	internal class AbilityConditionOnMission : AbilityConditionDataless
	{
		// Token: 0x06003223 RID: 12835 RVA: 0x0015475C File Offset: 0x0015295C
		public AbilityConditionOnMission(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06003224 RID: 12836 RVA: 0x00154766 File Offset: 0x00152966
		protected override bool MatchesConditionSpecific()
		{
			Level loaded = Level.Loaded;
			return loaded == null || loaded.Type != LevelData.LevelType.Outpost;
		}
	}
}
