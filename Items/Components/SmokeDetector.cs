using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000620 RID: 1568
	internal class SmokeDetector : ItemComponent
	{
		// Token: 0x17001973 RID: 6515
		// (get) Token: 0x0600648B RID: 25739 RVA: 0x00341642 File Offset: 0x0033F842
		// (set) Token: 0x0600648C RID: 25740 RVA: 0x0034164A File Offset: 0x0033F84A
		public bool FireInRange { get; private set; }

		// Token: 0x17001974 RID: 6516
		// (get) Token: 0x0600648D RID: 25741 RVA: 0x00341653 File Offset: 0x0033F853
		// (set) Token: 0x0600648E RID: 25742 RVA: 0x0034165B File Offset: 0x0033F85B
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the output strings. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x17001975 RID: 6517
		// (get) Token: 0x0600648F RID: 25743 RVA: 0x0034166A File Offset: 0x0033F86A
		// (set) Token: 0x06006490 RID: 25744 RVA: 0x00341674 File Offset: 0x0033F874
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal the item outputs when it has detected a fire.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.output = value;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x17001976 RID: 6518
		// (get) Token: 0x06006491 RID: 25745 RVA: 0x003416D6 File Offset: 0x0033F8D6
		// (set) Token: 0x06006492 RID: 25746 RVA: 0x003416E0 File Offset: 0x0033F8E0
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal the item outputs when it has not detected a fire.", "", true)]
		public string FalseOutput
		{
			get
			{
				return this.falseOutput;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.falseOutput = value;
				if (this.falseOutput.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.falseOutput = this.falseOutput.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x06006493 RID: 25747 RVA: 0x00341742 File Offset: 0x0033F942
		public SmokeDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006494 RID: 25748 RVA: 0x00341754 File Offset: 0x0033F954
		private bool IsFireInRange()
		{
			if (this.item.CurrentHull == null || this.item.InWater)
			{
				return false;
			}
			IEnumerable<Hull> connectedHulls = this.item.CurrentHull.GetConnectedHulls(true, new int?(10), true);
			foreach (Hull hull in connectedHulls)
			{
				foreach (FireSource fireSource in hull.FireSources)
				{
					if (fireSource.IsInDamageRange(this.item.WorldPosition, Math.Max(fireSource.DamageRange * 2f, 500f)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06006495 RID: 25749 RVA: 0x0034183C File Offset: 0x0033FA3C
		public override void Update(float deltaTime, Camera cam)
		{
			this.fireCheckTimer -= deltaTime;
			if (this.fireCheckTimer <= 0f)
			{
				this.FireInRange = this.IsFireInRange();
				this.fireCheckTimer = 1f;
			}
			string signalOut = this.FireInRange ? this.Output : this.FalseOutput;
			if (!string.IsNullOrEmpty(signalOut))
			{
				this.item.SendSignal(signalOut, "signal_out");
			}
		}

		// Token: 0x0400342D RID: 13357
		private const float FireCheckInterval = 1f;

		// Token: 0x0400342E RID: 13358
		private float fireCheckTimer;

		// Token: 0x04003430 RID: 13360
		private int maxOutputLength;

		// Token: 0x04003431 RID: 13361
		private string output;

		// Token: 0x04003432 RID: 13362
		private string falseOutput;
	}
}
