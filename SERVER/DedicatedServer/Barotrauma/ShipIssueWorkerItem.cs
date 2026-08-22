using System;

namespace Barotrauma
{
	// Token: 0x02000094 RID: 148
	internal abstract class ShipIssueWorkerItem : ShipIssueWorker
	{
		// Token: 0x060012A7 RID: 4775 RVA: 0x000A43EE File Offset: 0x000A25EE
		public ShipIssueWorkerItem(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x060012A8 RID: 4776 RVA: 0x000A43F8 File Offset: 0x000A25F8
		protected override bool IsIssueViable()
		{
			if (base.TargetItemComponent == null)
			{
				DebugConsole.ThrowError("TargetItemComponent was null in " + ((this != null) ? this.ToString() : null), null, null, false, false);
				return false;
			}
			if (base.TargetItem == null)
			{
				DebugConsole.ThrowError("TargetItem was null in " + ((this != null) ? this.ToString() : null), null, null, false, false);
				return false;
			}
			return true;
		}
	}
}
