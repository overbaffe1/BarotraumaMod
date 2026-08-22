using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000196 RID: 406
	internal class ShipGlobalIssueFixLeaks : ShipGlobalIssue
	{
		// Token: 0x06002FA5 RID: 12197 RVA: 0x001F71B2 File Offset: 0x001F53B2
		public ShipGlobalIssueFixLeaks(ShipCommandManager shipCommandManager) : base(shipCommandManager)
		{
		}

		// Token: 0x06002FA6 RID: 12198 RVA: 0x001F71C8 File Offset: 0x001F53C8
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

		// Token: 0x040018CB RID: 6347
		private readonly List<float> hullSeverities = new List<float>();
	}
}
