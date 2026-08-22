using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200019C RID: 412
	internal class ShipIssueWorkerPowerUpReactor : ShipIssueWorkerItem
	{
		// Token: 0x06002FB6 RID: 12214 RVA: 0x001F7534 File Offset: 0x001F5734
		public ShipIssueWorkerPowerUpReactor(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x06002FB7 RID: 12215 RVA: 0x001F7540 File Offset: 0x001F5740
		public override void CalculateImportanceSpecific()
		{
			if (base.TargetItem.Condition <= 0f)
			{
				return;
			}
			Reactor reactor = base.TargetItemComponent as Reactor;
			if (reactor != null && -reactor.CurrPowerConsumption < 1E-45f)
			{
				base.Importance = 40f;
			}
		}
	}
}
