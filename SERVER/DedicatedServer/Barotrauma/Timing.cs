using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002AF RID: 687
	internal static class Timing
	{
		// Token: 0x17000DB6 RID: 3510
		// (get) Token: 0x06002F45 RID: 12101 RVA: 0x0013A8C4 File Offset: 0x00138AC4
		public static int FrameLimit
		{
			get
			{
				return GameSettings.CurrentConfig.Graphics.FrameLimit;
			}
		}

		// Token: 0x17000DB7 RID: 3511
		// (get) Token: 0x06002F46 RID: 12102 RVA: 0x0013A8D5 File Offset: 0x00138AD5
		// (set) Token: 0x06002F47 RID: 12103 RVA: 0x0013A8DC File Offset: 0x00138ADC
		public static double Alpha
		{
			get
			{
				return Timing.alpha;
			}
			set
			{
				Timing.alpha = Math.Min(Math.Max(value, 0.0), 1.0);
			}
		}

		// Token: 0x06002F48 RID: 12104 RVA: 0x0013A900 File Offset: 0x00138B00
		public static double Interpolate(double previous, double current)
		{
			return current * Timing.alpha + previous * (1.0 - Timing.alpha);
		}

		// Token: 0x06002F49 RID: 12105 RVA: 0x0013A91B File Offset: 0x00138B1B
		public static float Interpolate(float previous, float current)
		{
			return current * (float)Timing.alpha + previous * (1f - (float)Timing.alpha);
		}

		// Token: 0x06002F4A RID: 12106 RVA: 0x0013A934 File Offset: 0x00138B34
		public static float InterpolateRotation(float previous, float current)
		{
			if (MathUtils.NearlyEqual(previous, current, 0.02f))
			{
				return current;
			}
			float angleDiff = MathUtils.GetShortestAngle(previous, current);
			return previous + angleDiff * (float)Timing.alpha;
		}

		// Token: 0x06002F4B RID: 12107 RVA: 0x0013A963 File Offset: 0x00138B63
		public static Vector2 Interpolate(Vector2 previous, Vector2 current)
		{
			return new Vector2(Timing.Interpolate(previous.X, current.X), Timing.Interpolate(previous.Y, current.Y));
		}

		// Token: 0x040017A5 RID: 6053
		private static double alpha;

		// Token: 0x040017A6 RID: 6054
		public static double TotalTime;

		// Token: 0x040017A7 RID: 6055
		public static double TotalTimeUnpaused;

		// Token: 0x040017A8 RID: 6056
		public static double Accumulator;

		// Token: 0x040017A9 RID: 6057
		public const int FixedUpdateRate = 60;

		// Token: 0x040017AA RID: 6058
		public const double Step = 0.016666666666666666;

		// Token: 0x040017AB RID: 6059
		public static double AccumulatorMax = 0.25;
	}
}
