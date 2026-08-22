using System;
using System.Collections.Generic;

namespace Voronoi2
{
	// Token: 0x0200000F RID: 15
	public class SiteSorterYX : IComparer<Site>
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00004110 File Offset: 0x00002310
		public int Compare(Site p1, Site p2)
		{
			DoubleVector2 s = p1.Coord;
			DoubleVector2 s2 = p2.Coord;
			if (s.Y < s2.Y)
			{
				return -1;
			}
			if (s.Y > s2.Y)
			{
				return 1;
			}
			if (s.X < s2.X)
			{
				return -1;
			}
			if (s.X > s2.X)
			{
				return 1;
			}
			return 0;
		}
	}
}
