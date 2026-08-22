using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200039C RID: 924
	internal readonly struct Triangle2D : IEquatable<Triangle2D>
	{
		// Token: 0x0600451E RID: 17694 RVA: 0x00267C50 File Offset: 0x00265E50
		public Triangle2D(Vector2 A, Vector2 B, Vector2 C)
		{
			this.A = A;
			this.B = B;
			this.C = C;
		}

		// Token: 0x170011DF RID: 4575
		// (get) Token: 0x0600451F RID: 17695 RVA: 0x00267C67 File Offset: 0x00265E67
		// (set) Token: 0x06004520 RID: 17696 RVA: 0x00267C6F File Offset: 0x00265E6F
		public Vector2 A { get; set; }

		// Token: 0x170011E0 RID: 4576
		// (get) Token: 0x06004521 RID: 17697 RVA: 0x00267C78 File Offset: 0x00265E78
		// (set) Token: 0x06004522 RID: 17698 RVA: 0x00267C80 File Offset: 0x00265E80
		public Vector2 B { get; set; }

		// Token: 0x170011E1 RID: 4577
		// (get) Token: 0x06004523 RID: 17699 RVA: 0x00267C89 File Offset: 0x00265E89
		// (set) Token: 0x06004524 RID: 17700 RVA: 0x00267C91 File Offset: 0x00265E91
		public Vector2 C { get; set; }

		// Token: 0x06004525 RID: 17701 RVA: 0x00267C9C File Offset: 0x00265E9C
		public bool Contains(Vector2 point)
		{
			int halfPlaneAb = MathUtils.VectorOrientation(this.A, this.B, point);
			int halfPlaneBc = MathUtils.VectorOrientation(this.B, this.C, point);
			int halfPlaneCa = MathUtils.VectorOrientation(this.C, this.A, point);
			bool allNonNegative = halfPlaneAb >= 0 && halfPlaneBc >= 0 && halfPlaneCa >= 0;
			bool allNonPositive = halfPlaneAb <= 0 && halfPlaneBc <= 0 && halfPlaneCa <= 0;
			return allNonNegative || allNonPositive;
		}

		// Token: 0x06004526 RID: 17702 RVA: 0x00267D10 File Offset: 0x00265F10
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

		// Token: 0x06004527 RID: 17703 RVA: 0x00267D5C File Offset: 0x00265F5C
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

		// Token: 0x06004528 RID: 17704 RVA: 0x00267DDF File Offset: 0x00265FDF
		[CompilerGenerated]
		public static bool operator !=(Triangle2D left, Triangle2D right)
		{
			return !(left == right);
		}

		// Token: 0x06004529 RID: 17705 RVA: 0x00267DEB File Offset: 0x00265FEB
		[CompilerGenerated]
		public static bool operator ==(Triangle2D left, Triangle2D right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600452A RID: 17706 RVA: 0x00267DF5 File Offset: 0x00265FF5
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<Vector2>.Default.GetHashCode(this.<A>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<B>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<C>k__BackingField);
		}

		// Token: 0x0600452B RID: 17707 RVA: 0x00267E35 File Offset: 0x00266035
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Triangle2D && this.Equals((Triangle2D)obj);
		}

		// Token: 0x0600452C RID: 17708 RVA: 0x00267E50 File Offset: 0x00266050
		[CompilerGenerated]
		public bool Equals(Triangle2D other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<A>k__BackingField, other.<A>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<B>k__BackingField, other.<B>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<C>k__BackingField, other.<C>k__BackingField);
		}

		// Token: 0x0600452D RID: 17709 RVA: 0x00267EA5 File Offset: 0x002660A5
		[CompilerGenerated]
		public void Deconstruct(out Vector2 A, out Vector2 B, out Vector2 C)
		{
			A = this.A;
			B = this.B;
			C = this.C;
		}
	}
}
