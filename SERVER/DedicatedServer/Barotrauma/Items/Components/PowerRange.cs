using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D9 RID: 1241
	internal readonly struct PowerRange
	{
		// Token: 0x0600466E RID: 18030 RVA: 0x001C1F6A File Offset: 0x001C016A
		public PowerRange(float min, float max)
		{
			this = new PowerRange(min, max, 0f);
		}

		// Token: 0x0600466F RID: 18031 RVA: 0x001C1F79 File Offset: 0x001C0179
		public PowerRange(float min, float max, float reactorMaxOutput)
		{
			this.Min = min;
			this.Max = max;
			this.ReactorMaxOutput = reactorMaxOutput;
		}

		// Token: 0x06004670 RID: 18032 RVA: 0x001C1F90 File Offset: 0x001C0190
		public static PowerRange operator +(PowerRange a, PowerRange b)
		{
			return new PowerRange(a.Min + b.Min, a.Max + b.Max, a.ReactorMaxOutput + b.ReactorMaxOutput);
		}

		// Token: 0x06004671 RID: 18033 RVA: 0x001C1FBE File Offset: 0x001C01BE
		public static PowerRange operator -(PowerRange a, PowerRange b)
		{
			return new PowerRange(a.Min - b.Min, a.Max - b.Max, a.ReactorMaxOutput - b.ReactorMaxOutput);
		}

		// Token: 0x040021F5 RID: 8693
		public static readonly PowerRange Zero;

		// Token: 0x040021F6 RID: 8694
		public readonly float Min;

		// Token: 0x040021F7 RID: 8695
		public readonly float Max;

		// Token: 0x040021F8 RID: 8696
		public readonly float ReactorMaxOutput;
	}
}
