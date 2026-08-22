using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200031B RID: 795
	internal class AbilityConditionServerRandom : AbilityConditionDataless
	{
		// Token: 0x17000E19 RID: 3609
		// (get) Token: 0x06003229 RID: 12841 RVA: 0x001547FD File Offset: 0x001529FD
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600322A RID: 12842 RVA: 0x00154800 File Offset: 0x00152A00
		public AbilityConditionServerRandom(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.randomChance = conditionElement.GetAttributeFloat("randomchance", 1f);
		}

		// Token: 0x0600322B RID: 12843 RVA: 0x00154820 File Offset: 0x00152A20
		protected override bool MatchesConditionSpecific()
		{
			return this.randomChance >= Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
		}

		// Token: 0x040018BA RID: 6330
		private readonly float randomChance;
	}
}
