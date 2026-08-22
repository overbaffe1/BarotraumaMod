using System;
using Microsoft.Xna.Framework;

namespace Voronoi2
{
	// Token: 0x02000009 RID: 9
	public class Site
	{
		// Token: 0x0600003F RID: 63 RVA: 0x00003A32 File Offset: 0x00001C32
		public void SetPoint(Vector2 point)
		{
			this.Coord.SetPoint((double)point.X, (double)point.Y);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003A4D File Offset: 0x00001C4D
		public Site()
		{
			this.Coord = new DoubleVector2();
		}

		// Token: 0x0400002D RID: 45
		public DoubleVector2 Coord;

		// Token: 0x0400002E RID: 46
		public int SiteNbr;
	}
}
