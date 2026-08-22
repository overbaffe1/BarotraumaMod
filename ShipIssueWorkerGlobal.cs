using System;

namespace Barotrauma
{
	// Token: 0x02000199 RID: 409
	internal abstract class ShipIssueWorkerGlobal : ShipIssueWorker
	{
		// Token: 0x06002FAD RID: 12205 RVA: 0x001F72AA File Offset: 0x001F54AA
		public ShipIssueWorkerGlobal(ShipCommandManager shipCommandManager, Order suggestedOrderPrefab, ShipGlobalIssue shipGlobalIssue) : base(shipCommandManager, suggestedOrderPrefab)
		{
			this.shipGlobalIssue = shipGlobalIssue;
		}

		// Token: 0x06002FAE RID: 12206 RVA: 0x001F72BB File Offset: 0x001F54BB
		public override void CalculateImportanceSpecific()
		{
			base.Importance = this.shipGlobalIssue.GlobalImportance;
		}

		// Token: 0x040018CE RID: 6350
		private readonly ShipGlobalIssue shipGlobalIssue;
	}
}
