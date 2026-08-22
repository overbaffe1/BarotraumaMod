using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000099 RID: 153
	internal class ShipIssueWorkerSteer : ShipIssueWorkerItem
	{
		// Token: 0x060012B3 RID: 4787 RVA: 0x000A481B File Offset: 0x000A2A1B
		public ShipIssueWorkerSteer(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x000A4828 File Offset: 0x000A2A28
		public override void CalculateImportanceSpecific()
		{
			if (this.shipCommandManager.NavigationState == ShipCommandManager.NavigationStates.Inactive)
			{
				return;
			}
			Powered powered = base.TargetItemComponent as Powered;
			if (powered != null && !powered.HasPower)
			{
				return;
			}
			if (base.TargetItem.Condition <= 0f)
			{
				return;
			}
			base.Importance = 70f;
		}
	}
}
