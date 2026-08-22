using System;

namespace Barotrauma
{
	// Token: 0x02000093 RID: 147
	internal abstract class ShipIssueWorkerGlobal : ShipIssueWorker
	{
		// Token: 0x060012A5 RID: 4773 RVA: 0x000A43CA File Offset: 0x000A25CA
		public ShipIssueWorkerGlobal(ShipCommandManager shipCommandManager, Order suggestedOrderPrefab, ShipGlobalIssue shipGlobalIssue) : base(shipCommandManager, suggestedOrderPrefab)
		{
			this.shipGlobalIssue = shipGlobalIssue;
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x000A43DB File Offset: 0x000A25DB
		public override void CalculateImportanceSpecific()
		{
			base.Importance = this.shipGlobalIssue.GlobalImportance;
		}

		// Token: 0x040008D4 RID: 2260
		private readonly ShipGlobalIssue shipGlobalIssue;
	}
}
