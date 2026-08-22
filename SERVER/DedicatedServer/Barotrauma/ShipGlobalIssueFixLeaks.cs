using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000090 RID: 144
	internal class ShipGlobalIssueFixLeaks : ShipGlobalIssue
	{
		// Token: 0x0600129D RID: 4765 RVA: 0x000A42D2 File Offset: 0x000A24D2
		public ShipGlobalIssueFixLeaks(ShipCommandManager shipCommandManager) : base(shipCommandManager)
		{
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x000A42E8 File Offset: 0x000A24E8
		public override void CalculateGlobalIssue()
		{
			this.hullSeverities.Clear();
			foreach (Gap gap in Gap.GapList)
			{
				if (AIObjectiveFixLeaks.IsValidTarget(gap, this.shipCommandManager.character))
				{
					this.hullSeverities.Add(AIObjectiveFixLeaks.GetLeakSeverity(gap));
				}
			}
			float averagePercentage = 0f;
			if (this.hullSeverities.Any<float>())
			{
				this.hullSeverities.Sort();
				averagePercentage = this.hullSeverities.TakeLast(3).Average();
			}
			base.GlobalImportance = averagePercentage;
		}

		// Token: 0x040008D1 RID: 2257
		private readonly List<float> hullSeverities = new List<float>();
	}
}
