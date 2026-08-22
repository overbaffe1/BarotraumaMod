using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Utils
{
	// Token: 0x020002E0 RID: 736
	public struct CoordinateSpace2D
	{
		// Token: 0x17000E01 RID: 3585
		// (get) Token: 0x06003155 RID: 12629 RVA: 0x00150EF4 File Offset: 0x0014F0F4
		public Matrix LocalToCanonical
		{
			get
			{
				return new Matrix(this.I.X, this.I.Y, 0f, 0f, this.J.X, this.J.Y, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f) * Matrix.CreateTranslation(this.Origin.X, this.Origin.Y, 0f);
			}
		}

		// Token: 0x17000E02 RID: 3586
		// (get) Token: 0x06003156 RID: 12630 RVA: 0x00150F93 File Offset: 0x0014F193
		public Matrix CanonicalToLocal
		{
			get
			{
				return Matrix.Invert(this.LocalToCanonical);
			}
		}

		// Token: 0x0400185C RID: 6236
		public static readonly CoordinateSpace2D CanonicalSpace = new CoordinateSpace2D
		{
			Origin = Vector2.Zero,
			I = Vector2.UnitX,
			J = Vector2.UnitY
		};

		// Token: 0x0400185D RID: 6237
		public Vector2 Origin;

		// Token: 0x0400185E RID: 6238
		public Vector2 I;

		// Token: 0x0400185F RID: 6239
		public Vector2 J;
	}
}
