using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003E2 RID: 994
	internal class AbilityConditionShipFlooded : AbilityConditionDataless
	{
		// Token: 0x06004666 RID: 18022 RVA: 0x0026C6BD File Offset: 0x0026A8BD
		public AbilityConditionShipFlooded(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.floodPercentage = conditionElement.GetAttributeFloat("floodpercentage", 0f);
		}

		// Token: 0x06004667 RID: 18023 RVA: 0x0026C6E0 File Offset: 0x0026A8E0
		protected override bool MatchesConditionSpecific()
		{
			if (!this.character.IsInFriendlySub)
			{
				return false;
			}
			float waterVolume = 0f;
			float totalVolume = 0f;
			foreach (Hull hull in Hull.HullList)
			{
				Submarine hullSubmarine = hull.Submarine;
				if (hullSubmarine != null && hullSubmarine == this.character.Submarine && hullSubmarine.TeamID == this.character.TeamID)
				{
					waterVolume += hull.WaterVolume;
					totalVolume += hull.Volume;
				}
			}
			return waterVolume / totalVolume > this.floodPercentage;
		}

		// Token: 0x0400247C RID: 9340
		private readonly float floodPercentage;
	}
}
