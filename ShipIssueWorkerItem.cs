using System;

namespace Barotrauma
{
	// Token: 0x0200019A RID: 410
	internal abstract class ShipIssueWorkerItem : ShipIssueWorker
	{
		// Token: 0x06002FAF RID: 12207 RVA: 0x001F72CE File Offset: 0x001F54CE
		public ShipIssueWorkerItem(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x06002FB0 RID: 12208 RVA: 0x001F72D8 File Offset: 0x001F54D8
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
