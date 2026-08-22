using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000096 RID: 150
	internal class ShipIssueWorkerPowerUpReactor : ShipIssueWorkerItem
	{
		// Token: 0x060012AE RID: 4782 RVA: 0x000A4654 File Offset: 0x000A2854
		public ShipIssueWorkerPowerUpReactor(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x060012AF RID: 4783 RVA: 0x000A4660 File Offset: 0x000A2860
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
