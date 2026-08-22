using System;

namespace Voronoi2
{
	// Token: 0x0200000B RID: 11
	public class Halfedge
	{
		// Token: 0x06000043 RID: 67 RVA: 0x00003B6C File Offset: 0x00001D6C
		public Halfedge()
		{
			this.PQnext = null;
		}

		// Token: 0x04000035 RID: 53
		public Halfedge ELleft;

		// Token: 0x04000036 RID: 54
		public Halfedge ELright;

		// Token: 0x04000037 RID: 55
		public Edge ELedge;

		// Token: 0x04000038 RID: 56
		public bool deleted;

		// Token: 0x04000039 RID: 57
		public int ELpm;

		// Token: 0x0400003A RID: 58
		public Site vertex;

		// Token: 0x0400003B RID: 59
		public double ystar;

		// Token: 0x0400003C RID: 60
		public Halfedge PQnext;
	}
}
