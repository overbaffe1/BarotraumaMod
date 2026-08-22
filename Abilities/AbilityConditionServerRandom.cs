using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003E1 RID: 993
	internal class AbilityConditionServerRandom : AbilityConditionDataless
	{
		// Token: 0x17001209 RID: 4617
		// (get) Token: 0x06004663 RID: 18019 RVA: 0x0026C67D File Offset: 0x0026A87D
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06004664 RID: 18020 RVA: 0x0026C680 File Offset: 0x0026A880
		public AbilityConditionServerRandom(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.randomChance = conditionElement.GetAttributeFloat("randomchance", 1f);
		}

		// Token: 0x06004665 RID: 18021 RVA: 0x0026C6A0 File Offset: 0x0026A8A0
		protected override bool MatchesConditionSpecific()
		{
			return this.randomChance >= Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
		}

		// Token: 0x0400247B RID: 9339
		private readonly float randomChance;
	}
}
