using System;

namespace Voronoi2
{
	// Token: 0x02000008 RID: 8
	public class DoubleVector2
	{
		// Token: 0x0600003B RID: 59 RVA: 0x000039B8 File Offset: 0x00001BB8
		public DoubleVector2()
		{
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000039C0 File Offset: 0x00001BC0
		public DoubleVector2(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000039D6 File Offset: 0x00001BD6
		public void SetPoint(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000039E8 File Offset: 0x00001BE8
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
