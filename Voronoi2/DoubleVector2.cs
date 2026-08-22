using System;

namespace Voronoi2
{
	// Token: 0x02000008 RID: 8
	public class DoubleVector2
	{
		// Token: 0x0600003C RID: 60 RVA: 0x00003AA4 File Offset: 0x00001CA4
		public DoubleVector2()
		{
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003AAC File Offset: 0x00001CAC
		public DoubleVector2(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003AC2 File Offset: 0x00001CC2
		public void SetPoint(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003AD4 File Offset: 0x00001CD4
		public void Normalize()
		{
			double length = Math.Sqrt(this.X * this.X + this.Y * this.Y);
			this.X /= length;
			this.Y /= length;
		}

		// Token: 0x0400002B RID: 43
		public double X;

		// Token: 0x0400002C RID: 44
		public double Y;
	}
}
