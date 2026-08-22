using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200031C RID: 796
	internal class AbilityConditionShipFlooded : AbilityConditionDataless
	{
		// Token: 0x0600322C RID: 12844 RVA: 0x0015483D File Offset: 0x00152A3D
		public AbilityConditionShipFlooded(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.floodPercentage = conditionElement.GetAttributeFloat("floodpercentage", 0f);
		}

		// Token: 0x0600322D RID: 12845 RVA: 0x00154860 File Offset: 0x00152A60
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

		// Token: 0x040018BB RID: 6331
		private readonly float floodPercentage;
	}
}
