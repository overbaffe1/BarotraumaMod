using System;
using System.Collections.Generic;

namespace Barotrauma.Lights
{
	// Token: 0x020004CE RID: 1230
	internal class ConvexHullList
	{
		// Token: 0x0600500E RID: 20494 RVA: 0x002B08D9 File Offset: 0x002AEAD9
		public ConvexHullList(Submarine submarine)
		{
			this.Submarine = submarine;
		}

		// Token: 0x04002A2F RID: 10799
		public readonly Submarine Submarine;

		// Token: 0x04002A30 RID: 10800
		public HashSet<ConvexHull> IsHidden = new HashSet<ConvexHull>();

		// Token: 0x04002A31 RID: 10801
		public HashSet<ConvexHull> HasBeenVisible = new HashSet<ConvexHull>();

		// Token: 0x04002A32 RID: 10802
		public readonly List<ConvexHull> List = new List<ConvexHull>();
	}
}
