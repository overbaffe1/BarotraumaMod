using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FB RID: 1275
	internal struct Signal
	{
		// Token: 0x1700133F RID: 4927
		// (get) Token: 0x060047AC RID: 18348 RVA: 0x001C86EF File Offset: 0x001C68EF
		public double TimeSinceCreated
		{
			get
			{
				return Timing.TotalTimeUnpaused - this.CreationTime;
			}
		}

		// Token: 0x060047AD RID: 18349 RVA: 0x001C86FD File Offset: 0x001C68FD
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

		// Token: 0x060047AE RID: 18350 RVA: 0x001C8738 File Offset: 0x001C6938
		internal Signal WithStepsTaken(int stepsTaken)
		{
			Signal retVal = this;
			retVal.stepsTaken = stepsTaken;
			return retVal;
		}

		// Token: 0x060047AF RID: 18351 RVA: 0x001C8758 File Offset: 0x001C6958
		public static bool operator ==(Signal a, Signal b)
		{
			return a.value == b.value && a.stepsTaken == b.stepsTaken && a.sender == b.sender && a.source == b.source && MathUtils.NearlyEqual(a.power, b.power, 0.0001f) && MathUtils.NearlyEqual(a.strength, b.strength, 0.0001f);
		}

		// Token: 0x060047B0 RID: 18352 RVA: 0x001C87D2 File Offset: 0x001C69D2
		public static bool operator !=(Signal a, Signal b)
		{
			return !(a == b);
		}

		// Token: 0x0400229E RID: 8862
		public string value;

		// Token: 0x0400229F RID: 8863
		public int stepsTaken;

		// Token: 0x040022A0 RID: 8864
		public Character sender;

		// Token: 0x040022A1 RID: 8865
		public Item source;

		// Token: 0x040022A2 RID: 8866
		public float power;

		// Token: 0x040022A3 RID: 8867
		public float strength;

		// Token: 0x040022A4 RID: 8868
		public readonly double CreationTime;
	}
}
