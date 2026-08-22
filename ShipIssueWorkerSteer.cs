using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200019F RID: 415
	internal class ShipIssueWorkerSteer : ShipIssueWorkerItem
	{
		// Token: 0x06002FBB RID: 12219 RVA: 0x001F76FB File Offset: 0x001F58FB
		public ShipIssueWorkerSteer(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x06002FBC RID: 12220 RVA: 0x001F7708 File Offset: 0x001F5908
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
