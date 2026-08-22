using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Lights
{
	// Token: 0x020004CF RID: 1231
	internal class Segment
	{
		// Token: 0x0600500F RID: 20495 RVA: 0x002B090C File Offset: 0x002AEB0C
		public Segment(SegmentPoint start, SegmentPoint end, ConvexHull convexHull)
		{
			if (start.Pos.Y > end.Pos.Y)
			{
				SegmentPoint segmentPoint = start;
				start = end;
				end = segmentPoint;
			}
			this.Start = start;
			this.End = end;
			this.ConvexHull = convexHull;
			start.ConvexHull = convexHull;
			end.ConvexHull = convexHull;
			this.IsHorizontal = (Math.Abs(start.Pos.X - end.Pos.X) > Math.Abs(start.Pos.Y - end.Pos.Y));
			this.IsAxisAligned = (Math.Abs(start.Pos.X - end.Pos.X) < 0.1f || Math.Abs(start.Pos.Y - end.Pos.Y) < 0.001f);
		}

		// Token: 0x04002A33 RID: 10803
		public SegmentPoint Start;

		// Token: 0x04002A34 RID: 10804
		public SegmentPoint End;

		// Token: 0x04002A35 RID: 10805
		public ConvexHull ConvexHull;

		// Token: 0x04002A36 RID: 10806
		public bool IsHorizontal;

		// Token: 0x04002A37 RID: 10807
		public bool IsAxisAligned;

		// Token: 0x04002A38 RID: 10808
		public Vector2 SubmarineDrawPos;
	}
}
