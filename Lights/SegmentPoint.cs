using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Lights
{
	// Token: 0x020004D0 RID: 1232
	internal struct SegmentPoint
	{
		// Token: 0x06005010 RID: 20496 RVA: 0x002B09F1 File Offset: 0x002AEBF1
		public SegmentPoint(Vector2 pos, ConvexHull convexHull)
		{
			this.Pos = pos;
			this.WorldPos = pos;
			this.ConvexHull = convexHull;
		}

		// Token: 0x06005011 RID: 20497 RVA: 0x002B0A08 File Offset: 0x002AEC08
		public override string ToString()
		{
			return this.Pos.ToString();
		}

		// Token: 0x04002A39 RID: 10809
		public Vector2 Pos;

		// Token: 0x04002A3A RID: 10810
		public Vector2 WorldPos;

		// Token: 0x04002A3B RID: 10811
		public ConvexHull ConvexHull;
	}
}
