using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002D1 RID: 721
	internal readonly struct Triangle2D : IEquatable<Triangle2D>
	{
		// Token: 0x060030A0 RID: 12448 RVA: 0x0014E1F4 File Offset: 0x0014C3F4
		public Triangle2D(Vector2 A, Vector2 B, Vector2 C)
		{
			this.A = A;
			this.B = B;
			this.C = C;
		}

		// Token: 0x17000DEA RID: 3562
		// (get) Token: 0x060030A1 RID: 12449 RVA: 0x0014E20B File Offset: 0x0014C40B
		// (set) Token: 0x060030A2 RID: 12450 RVA: 0x0014E213 File Offset: 0x0014C413
		public Vector2 A { get; set; }

		// Token: 0x17000DEB RID: 3563
		// (get) Token: 0x060030A3 RID: 12451 RVA: 0x0014E21C File Offset: 0x0014C41C
		// (set) Token: 0x060030A4 RID: 12452 RVA: 0x0014E224 File Offset: 0x0014C424
		public Vector2 B { get; set; }

		// Token: 0x17000DEC RID: 3564
		// (get) Token: 0x060030A5 RID: 12453 RVA: 0x0014E22D File Offset: 0x0014C42D
		// (set) Token: 0x060030A6 RID: 12454 RVA: 0x0014E235 File Offset: 0x0014C435
		public Vector2 C { get; set; }

		// Token: 0x060030A7 RID: 12455 RVA: 0x0014E240 File Offset: 0x0014C440
		public bool Contains(Vector2 point)
		{
			int halfPlaneAb = MathUtils.VectorOrientation(this.A, this.B, point);
			int halfPlaneBc = MathUtils.VectorOrientation(this.B, this.C, point);
			int halfPlaneCa = MathUtils.VectorOrientation(this.C, this.A, point);
			bool allNonNegative = halfPlaneAb >= 0 && halfPlaneBc >= 0 && halfPlaneCa >= 0;
			bool allNonPositive = halfPlaneAb <= 0 && halfPlaneBc <= 0 && halfPlaneCa <= 0;
			return allNonNegative || allNonPositive;
		}

		// Token: 0x060030A8 RID: 12456 RVA: 0x0014E2B4 File Offset: 0x0014C4B4
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Triangle2D");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060030A9 RID: 12457 RVA: 0x0014E300 File Offset: 0x0014C500
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("A = ");
			builder.Append(this.A.ToString());
			builder.Append(", B = ");
			builder.Append(this.B.ToString());
			builder.Append(", C = ");
			builder.Append(this.C.ToString());
			return true;
		}

		// Token: 0x060030AA RID: 12458 RVA: 0x0014E383 File Offset: 0x0014C583
		[CompilerGenerated]
		public static bool operator !=(Triangle2D left, Triangle2D right)
		{
			return !(left == right);
		}

		// Token: 0x060030AB RID: 12459 RVA: 0x0014E38F File Offset: 0x0014C58F
		[CompilerGenerated]
		public static bool operator ==(Triangle2D left, Triangle2D right)
		{
			return left.Equals(right);
		}

		// Token: 0x060030AC RID: 12460 RVA: 0x0014E399 File Offset: 0x0014C599
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<Vector2>.Default.GetHashCode(this.<A>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<B>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<C>k__BackingField);
		}

		// Token: 0x060030AD RID: 12461 RVA: 0x0014E3D9 File Offset: 0x0014C5D9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Triangle2D && this.Equals((Triangle2D)obj);
		}

		// Token: 0x060030AE RID: 12462 RVA: 0x0014E3F4 File Offset: 0x0014C5F4
		[CompilerGenerated]
		public bool Equals(Triangle2D other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<A>k__BackingField, other.<A>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<B>k__BackingField, other.<B>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<C>k__BackingField, other.<C>k__BackingField);
		}

		// Token: 0x060030AF RID: 12463 RVA: 0x0014E449 File Offset: 0x0014C649
		[CompilerGenerated]
		public void Deconstruct(out Vector2 A, out Vector2 B, out Vector2 C)
		{
			A = this.A;
			B = this.B;
			C = this.C;
		}
	}
}
