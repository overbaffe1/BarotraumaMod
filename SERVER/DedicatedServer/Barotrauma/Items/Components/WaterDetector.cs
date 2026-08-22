using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000502 RID: 1282
	internal class WaterDetector : ItemComponent
	{
		// Token: 0x1700134B RID: 4939
		// (get) Token: 0x060047D7 RID: 18391 RVA: 0x001C914A File Offset: 0x001C734A
		// (set) Token: 0x060047D8 RID: 18392 RVA: 0x001C9152 File Offset: 0x001C7352
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

		// Token: 0x1700134C RID: 4940
		// (get) Token: 0x060047D9 RID: 18393 RVA: 0x001C9161 File Offset: 0x001C7361
		// (set) Token: 0x060047DA RID: 18394 RVA: 0x001C916C File Offset: 0x001C736C
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

		// Token: 0x1700134D RID: 4941
		// (get) Token: 0x060047DB RID: 18395 RVA: 0x001C91CE File Offset: 0x001C73CE
		// (set) Token: 0x060047DC RID: 18396 RVA: 0x001C91D8 File Offset: 0x001C73D8
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

		// Token: 0x1700134E RID: 4942
		// (get) Token: 0x060047DD RID: 18397 RVA: 0x001C923A File Offset: 0x001C743A
		public bool WaterDetected
		{
			get
			{
				return this.isInWater;
			}
		}

		// Token: 0x1700134F RID: 4943
		// (get) Token: 0x060047DE RID: 18398 RVA: 0x001C9242 File Offset: 0x001C7442
		public int WaterPercentage
		{
			get
			{
				return WaterDetector.GetWaterPercentage(this.item.CurrentHull);
			}
		}

		// Token: 0x060047DF RID: 18399 RVA: 0x001C9254 File Offset: 0x001C7454
		public WaterDetector(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060047E0 RID: 18400 RVA: 0x001C9265 File Offset: 0x001C7465
		public static int GetWaterPercentage(Hull hull)
		{
			if (hull.WaterVolume / (float)hull.Rect.Width <= 1f)
			{
				return 0;
			}
			return MathHelper.Clamp((int)Math.Ceiling((double)hull.WaterPercentage), 0, 100);
		}

		// Token: 0x060047E1 RID: 18401 RVA: 0x001C9298 File Offset: 0x001C7498
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

		// Token: 0x040022BA RID: 8890
		private const float StateSwitchInterval = 1f;

		// Token: 0x040022BB RID: 8891
		private int prevSentWaterPercentageValue;

		// Token: 0x040022BC RID: 8892
		private string waterPercentageSignal;

		// Token: 0x040022BD RID: 8893
		private bool isInWater;

		// Token: 0x040022BE RID: 8894
		private float stateSwitchDelay;

		// Token: 0x040022BF RID: 8895
		private int maxOutputLength;

		// Token: 0x040022C0 RID: 8896
		private string output;

		// Token: 0x040022C1 RID: 8897
		private string falseOutput;
	}
}
