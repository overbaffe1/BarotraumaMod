using System;
using System.Collections.Generic;
using Barotrauma.Lights;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000152 RID: 338
	internal class CompareSegmentPointCW : IComparer<SegmentPoint>
	{
		// Token: 0x06002A17 RID: 10775 RVA: 0x001D1732 File Offset: 0x001CF932
		public CompareSegmentPointCW(Vector2 center)
		{
			this.center = center;
		}

		// Token: 0x06002A18 RID: 10776 RVA: 0x001D1741 File Offset: 0x001CF941
		public int Compare(SegmentPoint a, SegmentPoint b)
		{
			return -CompareCCW.Compare(a.WorldPos, b.WorldPos, this.center);
		}

		// Token: 0x04001606 RID: 5638
		private Vector2 center;
	}
}
