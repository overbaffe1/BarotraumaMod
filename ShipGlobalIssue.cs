using System;

namespace Barotrauma
{
	// Token: 0x02000198 RID: 408
	internal abstract class ShipGlobalIssue
	{
		// Token: 0x17000C4D RID: 3149
		// (get) Token: 0x06002FA9 RID: 12201 RVA: 0x001F728A File Offset: 0x001F548A
		// (set) Token: 0x06002FAA RID: 12202 RVA: 0x001F7292 File Offset: 0x001F5492
		public float GlobalImportance { get; set; }

		// Token: 0x06002FAB RID: 12203 RVA: 0x001F729B File Offset: 0x001F549B
		public ShipGlobalIssue(ShipCommandManager shipCommandManager)
		{
			this.shipCommandManager = shipCommandManager;
		}

		// Token: 0x06002FAC RID: 12204
		public abstract void CalculateGlobalIssue();

		// Token: 0x040018CD RID: 6349
		protected ShipCommandManager shipCommandManager;
	}
}
