using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Utils
{
	// Token: 0x020003AA RID: 938
	public struct CoordinateSpace2D
	{
		// Token: 0x170011F5 RID: 4597
		// (get) Token: 0x060045A1 RID: 17825 RVA: 0x00269368 File Offset: 0x00267568
		public Matrix LocalToCanonical
		{
			get
			{
				return new Matrix(this.I.X, this.I.Y, 0f, 0f, this.J.X, this.J.Y, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f) * Matrix.CreateTranslation(this.Origin.X, this.Origin.Y, 0f);
			}
		}

		// Token: 0x170011F6 RID: 4598
		// (get) Token: 0x060045A2 RID: 17826 RVA: 0x00269407 File Offset: 0x00267607
		public Matrix CanonicalToLocal
		{
			get
			{
				return Matrix.Invert(this.LocalToCanonical);
			}
		}

		// Token: 0x0400242A RID: 9258
		public static readonly CoordinateSpace2D CanonicalSpace = new CoordinateSpace2D
		{
			Origin = Vector2.Zero,
			I = Vector2.UnitX,
			J = Vector2.UnitY
		};

		// Token: 0x0400242B RID: 9259
		public Vector2 Origin;

		// Token: 0x0400242C RID: 9260
		public Vector2 I;

		// Token: 0x0400242D RID: 9261
		public Vector2 J;
	}
}
