using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000602 RID: 1538
	internal readonly struct PowerRange
	{
		// Token: 0x060063D6 RID: 25558 RVA: 0x0033EB78 File Offset: 0x0033CD78
		public PowerRange(float min, float max)
		{
			this = new PowerRange(min, max, 0f);
		}

		// Token: 0x060063D7 RID: 25559 RVA: 0x0033EB87 File Offset: 0x0033CD87
		public PowerRange(float min, float max, float reactorMaxOutput)
		{
			this.Min = min;
			this.Max = max;
			this.ReactorMaxOutput = reactorMaxOutput;
		}

		// Token: 0x060063D8 RID: 25560 RVA: 0x0033EB9E File Offset: 0x0033CD9E
		public static PowerRange operator +(PowerRange a, PowerRange b)
		{
			return new PowerRange(a.Min + b.Min, a.Max + b.Max, a.ReactorMaxOutput + b.ReactorMaxOutput);
		}

		// Token: 0x060063D9 RID: 25561 RVA: 0x0033EBCC File Offset: 0x0033CDCC
		public static PowerRange operator -(PowerRange a, PowerRange b)
		{
			return new PowerRange(a.Min - b.Min, a.Max - b.Max, a.ReactorMaxOutput - b.ReactorMaxOutput);
		}

		// Token: 0x040033CB RID: 13259
		public static readonly PowerRange Zero;

		// Token: 0x040033CC RID: 13260
		public readonly float Min;

		// Token: 0x040033CD RID: 13261
		public readonly float Max;

		// Token: 0x040033CE RID: 13262
		public readonly float ReactorMaxOutput;
	}
}
