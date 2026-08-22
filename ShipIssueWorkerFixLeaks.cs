using System;

namespace Barotrauma
{
	// Token: 0x02000197 RID: 407
	internal class ShipIssueWorkerFixLeaks : ShipIssueWorkerGlobal
	{
		// Token: 0x17000C4C RID: 3148
		// (get) Token: 0x06002FA7 RID: 12199 RVA: 0x001F727C File Offset: 0x001F547C
		public override bool StopDuringEmergency
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002FA8 RID: 12200 RVA: 0x001F727F File Offset: 0x001F547F
		public ShipIssueWorkerFixLeaks(ShipCommandManager shipCommandManager, Order order, ShipGlobalIssueFixLeaks shipGlobalIssueFixLeaks) : base(shipCommandManager, order, shipGlobalIssueFixLeaks)
		{
		}
	}
}
