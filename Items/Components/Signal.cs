using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061E RID: 1566
	internal struct Signal
	{
		// Token: 0x1700196E RID: 6510
		// (get) Token: 0x0600647C RID: 25724 RVA: 0x003413AB File Offset: 0x0033F5AB
		public double TimeSinceCreated
		{
			get
			{
				return Timing.TotalTimeUnpaused - this.CreationTime;
			}
		}

		// Token: 0x0600647D RID: 25725 RVA: 0x003413B9 File Offset: 0x0033F5B9
		public Signal(string value, int stepsTaken = 0, Character sender = null, Item source = null, float power = 0f, float strength = 1f)
		{
			this.value = value;
			this.stepsTaken = stepsTaken;
			this.sender = sender;
			this.source = source;
			this.power = power;
			this.strength = strength;
			this.CreationTime = Timing.TotalTimeUnpaused;
		}

		// Token: 0x0600647E RID: 25726 RVA: 0x003413F4 File Offset: 0x0033F5F4
		internal Signal WithStepsTaken(int stepsTaken)
		{
			Signal retVal = this;
			retVal.stepsTaken = stepsTaken;
			return retVal;
		}

		// Token: 0x0600647F RID: 25727 RVA: 0x00341414 File Offset: 0x0033F614
		public static bool operator ==(Signal a, Signal b)
		{
			return a.value == b.value && a.stepsTaken == b.stepsTaken && a.sender == b.sender && a.source == b.source && MathUtils.NearlyEqual(a.power, b.power, 0.0001f) && MathUtils.NearlyEqual(a.strength, b.strength, 0.0001f);
		}

		// Token: 0x06006480 RID: 25728 RVA: 0x0034148E File Offset: 0x0033F68E
		public static bool operator !=(Signal a, Signal b)
		{
			return !(a == b);
		}

		// Token: 0x04003422 RID: 13346
		public string value;

		// Token: 0x04003423 RID: 13347
		public int stepsTaken;

		// Token: 0x04003424 RID: 13348
		public Character sender;

		// Token: 0x04003425 RID: 13349
		public Item source;

		// Token: 0x04003426 RID: 13350
		public float power;

		// Token: 0x04003427 RID: 13351
		public float strength;

		// Token: 0x04003428 RID: 13352
		public readonly double CreationTime;
	}
}
