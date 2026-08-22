using System;

namespace Barotrauma
{
	// Token: 0x02000091 RID: 145
	internal class ShipIssueWorkerFixLeaks : ShipIssueWorkerGlobal
	{
		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x0600129F RID: 4767 RVA: 0x000A439C File Offset: 0x000A259C
		public override bool StopDuringEmergency
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060012A0 RID: 4768 RVA: 0x000A439F File Offset: 0x000A259F
		public ShipIssueWorkerFixLeaks(ShipCommandManager shipCommandManager, Order order, ShipGlobalIssueFixLeaks shipGlobalIssueFixLeaks) : base(shipCommandManager, order, shipGlobalIssueFixLeaks)
		{
		}
	}
}
