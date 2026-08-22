using System;

namespace Voronoi2
{
	// Token: 0x0200000A RID: 10
	public class Edge
	{
		// Token: 0x06000042 RID: 66 RVA: 0x00003B4C File Offset: 0x00001D4C
		public Edge()
		{
			this.ep = new Site[2];
			this.reg = new Site[2];
		}

		// Token: 0x0400002F RID: 47
		public double a;

		// Token: 0x04000030 RID: 48
		public double b;

		// Token: 0x04000031 RID: 49
		public double c;

		// Token: 0x04000032 RID: 50
		public Site[] ep;

		// Token: 0x04000033 RID: 51
		public Site[] reg;

		// Token: 0x04000034 RID: 52
		public int edgenbr;
	}
}
