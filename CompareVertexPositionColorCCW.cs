using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000153 RID: 339
	public class CompareVertexPositionColorCCW : IComparer<VertexPositionColor>
	{
		// Token: 0x06002A19 RID: 10777 RVA: 0x001D175B File Offset: 0x001CF95B
		public CompareVertexPositionColorCCW(Vector2 center)
		{
			this.center = center;
		}

		// Token: 0x06002A1A RID: 10778 RVA: 0x001D176C File Offset: 0x001CF96C
		public int Compare(VertexPositionColor a, VertexPositionColor b)
		{
			return -CompareCW.Compare(new Vector2(a.Position.X, a.Position.Y), new Vector2(b.Position.X, b.Position.Y), this.center);
		}

		// Token: 0x06002A1B RID: 10779 RVA: 0x001D17BB File Offset: 0x001CF9BB
		public static int Compare(VertexPositionColor a, VertexPositionColor b, Vector2 center)
		{
			return -CompareCW.Compare(new Vector2(a.Position.X, a.Position.Y), new Vector2(b.Position.X, b.Position.Y), center);
		}

		// Token: 0x04001607 RID: 5639
		private Vector2 center;
	}
}
