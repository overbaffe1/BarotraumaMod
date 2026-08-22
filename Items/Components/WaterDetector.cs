using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000625 RID: 1573
	internal class WaterDetector : ItemComponent
	{
		// Token: 0x1700197A RID: 6522
		// (get) Token: 0x060064A7 RID: 25767 RVA: 0x00341E06 File Offset: 0x00340006
		// (set) Token: 0x060064A8 RID: 25768 RVA: 0x00341E0E File Offset: 0x0034000E
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

		// Token: 0x1700197B RID: 6523
		// (get) Token: 0x060064A9 RID: 25769 RVA: 0x00341E1D File Offset: 0x0034001D
		// (set) Token: 0x060064AA RID: 25770 RVA: 0x00341E28 File Offset: 0x00340028
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal the item sends out when it's underwater.", "", true)]
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

		// Token: 0x1700197C RID: 6524
		// (get) Token: 0x060064AB RID: 25771 RVA: 0x00341E8A File Offset: 0x0034008A
		// (set) Token: 0x060064AC RID: 25772 RVA: 0x00341E94 File Offset: 0x00340094
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal the item sends out when it's not underwater.", "", true)]
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

		// Token: 0x1700197D RID: 6525
		// (get) Token: 0x060064AD RID: 25773 RVA: 0x00341EF6 File Offset: 0x003400F6
		public bool WaterDetected
		{
			get
			{
				return this.isInWater;
			}
		}

		// Token: 0x1700197E RID: 6526
		// (get) Token: 0x060064AE RID: 25774 RVA: 0x00341EFE File Offset: 0x003400FE
		public int WaterPercentage
		{
			get
			{
				return WaterDetector.GetWaterPercentage(this.item.CurrentHull);
			}
		}

		// Token: 0x060064AF RID: 25775 RVA: 0x00341F10 File Offset: 0x00340110
		public WaterDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060064B0 RID: 25776 RVA: 0x00341F21 File Offset: 0x00340121
		public static int GetWaterPercentage(Hull hull)
		{
			if (hull.WaterVolume / (float)hull.Rect.Width <= 1f)
			{
				return 0;
			}
			return MathHelper.Clamp((int)Math.Ceiling((double)hull.WaterPercentage), 0, 100);
		}

		// Token: 0x060064B1 RID: 25777 RVA: 0x00341F54 File Offset: 0x00340154
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.stateSwitchDelay > 0f)
			{
				this.stateSwitchDelay -= deltaTime;
			}
			else
			{
				bool prevState = this.isInWater;
				this.isInWater = false;
				if (this.item.InWater)
				{
					this.isInWater = true;
				}
				else if (this.item.CurrentHull != null && WaterDetector.GetWaterPercentage(this.item.CurrentHull) > 0 && this.item.CurrentHull.Surface > (float)(this.item.Rect.Y - this.item.Rect.Height))
				{
					this.isInWater = true;
				}
				if (prevState != this.isInWater)
				{
					this.stateSwitchDelay = 1f;
				}
			}
			string signalOut = this.isInWater ? this.Output : this.FalseOutput;
			if (!string.IsNullOrEmpty(signalOut))
			{
				this.item.SendSignal(signalOut, "signal_out");
			}
			if (this.item.CurrentHull != null)
			{
				int waterPercentage = WaterDetector.GetWaterPercentage(this.item.CurrentHull);
				if (this.prevSentWaterPercentageValue != waterPercentage || this.waterPercentageSignal == null)
				{
					this.prevSentWaterPercentageValue = waterPercentage;
					this.waterPercentageSignal = this.prevSentWaterPercentageValue.ToString();
				}
				this.item.SendSignal(this.waterPercentageSignal, "water_%");
			}
			string highPressureOut = (this.item.CurrentHull == null || this.item.CurrentHull.LethalPressure > 5f) ? "1" : "0";
			this.item.SendSignal(highPressureOut, "high_pressure");
		}

		// Token: 0x0400343E RID: 13374
		private const float StateSwitchInterval = 1f;

		// Token: 0x0400343F RID: 13375
		private int prevSentWaterPercentageValue;

		// Token: 0x04003440 RID: 13376
		private string waterPercentageSignal;

		// Token: 0x04003441 RID: 13377
		private bool isInWater;

		// Token: 0x04003442 RID: 13378
		private float stateSwitchDelay;

		// Token: 0x04003443 RID: 13379
		private int maxOutputLength;

		// Token: 0x04003444 RID: 13380
		private string output;

		// Token: 0x04003445 RID: 13381
		private string falseOutput;
	}
}
