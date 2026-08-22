using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061B RID: 1563
	internal class OxygenDetector : ItemComponent
	{
		// Token: 0x17001962 RID: 6498
		// (get) Token: 0x06006451 RID: 25681 RVA: 0x0034064B File Offset: 0x0033E84B
		// (set) Token: 0x06006452 RID: 25682 RVA: 0x00340653 File Offset: 0x0033E853
		public string OxygenSignal { get; private set; }

		// Token: 0x06006453 RID: 25683 RVA: 0x0034065C File Offset: 0x0033E85C
		public OxygenDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006454 RID: 25684 RVA: 0x00340670 File Offset: 0x0033E870
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.CurrentHull == null)
			{
				return;
			}
			int currOxygenPercentage = (int)this.item.CurrentHull.OxygenPercentage;
			if (this.prevSentOxygenValue != currOxygenPercentage || this.OxygenSignal == null)
			{
				this.prevSentOxygenValue = currOxygenPercentage;
				this.OxygenSignal = this.prevSentOxygenValue.ToString();
			}
			this.item.SendSignal(this.OxygenSignal, "signal_out");
			this.item.SendSignal((currOxygenPercentage <= 35) ? "1" : "0", "low_oxygen");
		}

		// Token: 0x04003401 RID: 13313
		public const int LowOxygenPercentage = 35;

		// Token: 0x04003402 RID: 13314
		private int prevSentOxygenValue;
	}
}
