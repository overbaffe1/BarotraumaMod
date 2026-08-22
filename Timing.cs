using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200037B RID: 891
	internal static class Timing
	{
		// Token: 0x170011B9 RID: 4537
		// (get) Token: 0x060043DD RID: 17373 RVA: 0x00254E08 File Offset: 0x00253008
		public static int FrameLimit
		{
			get
			{
				return GameSettings.CurrentConfig.Graphics.FrameLimit;
			}
		}

		// Token: 0x170011BA RID: 4538
		// (get) Token: 0x060043DE RID: 17374 RVA: 0x00254E19 File Offset: 0x00253019
		// (set) Token: 0x060043DF RID: 17375 RVA: 0x00254E20 File Offset: 0x00253020
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

		// Token: 0x060043E0 RID: 17376 RVA: 0x00254E44 File Offset: 0x00253044
		public static double Interpolate(double previous, double current)
		{
			return current * Timing.alpha + previous * (1.0 - Timing.alpha);
		}

		// Token: 0x060043E1 RID: 17377 RVA: 0x00254E5F File Offset: 0x0025305F
		public static float Interpolate(float previous, float current)
		{
			return current * (float)Timing.alpha + previous * (1f - (float)Timing.alpha);
		}

		// Token: 0x060043E2 RID: 17378 RVA: 0x00254E78 File Offset: 0x00253078
		public static float InterpolateRotation(float previous, float current)
		{
			if (MathUtils.NearlyEqual(previous, current, 0.02f))
			{
				return current;
			}
			float angleDiff = MathUtils.GetShortestAngle(previous, current);
			return previous + angleDiff * (float)Timing.alpha;
		}

		// Token: 0x060043E3 RID: 17379 RVA: 0x00254EA7 File Offset: 0x002530A7
		public static Vector2 Interpolate(Vector2 previous, Vector2 current)
		{
			return new Vector2(Timing.Interpolate(previous.X, current.X), Timing.Interpolate(previous.Y, current.Y));
		}

		// Token: 0x04002388 RID: 9096
		private static double alpha;

		// Token: 0x04002389 RID: 9097
		public static double TotalTime;

		// Token: 0x0400238A RID: 9098
		public static double TotalTimeUnpaused;

		// Token: 0x0400238B RID: 9099
		public static double Accumulator;

		// Token: 0x0400238C RID: 9100
		public const int FixedUpdateRate = 60;

		// Token: 0x0400238D RID: 9101
		public const double Step = 0.016666666666666666;

		// Token: 0x0400238E RID: 9102
		public static double AccumulatorMax = 0.25;
	}
}
