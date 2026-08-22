using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F8 RID: 1272
	internal class OxygenDetector : ItemComponent
	{
		// Token: 0x17001333 RID: 4915
		// (get) Token: 0x06004781 RID: 18305 RVA: 0x001C797F File Offset: 0x001C5B7F
		// (set) Token: 0x06004782 RID: 18306 RVA: 0x001C7987 File Offset: 0x001C5B87
		public string OxygenSignal { get; private set; }

		// Token: 0x06004783 RID: 18307 RVA: 0x001C7990 File Offset: 0x001C5B90
		public OxygenDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004784 RID: 18308 RVA: 0x001C79A4 File Offset: 0x001C5BA4
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

		// Token: 0x0400227D RID: 8829
		public const int LowOxygenPercentage = 35;

		// Token: 0x0400227E RID: 8830
		private int prevSentOxygenValue;
	}
}
