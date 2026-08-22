using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200039B RID: 923
	internal readonly struct Quad2D : IEquatable<Quad2D>
	{
		// Token: 0x06004505 RID: 17669 RVA: 0x00267434 File Offset: 0x00265634
		public Quad2D(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
		{
			this.A = A;
			this.B = B;
			this.C = C;
			this.D = D;
		}

		// Token: 0x170011D9 RID: 4569
		// (get) Token: 0x06004506 RID: 17670 RVA: 0x00267453 File Offset: 0x00265653
		// (set) Token: 0x06004507 RID: 17671 RVA: 0x0026745B File Offset: 0x0026565B
		public Vector2 A { get; set; }

		// Token: 0x170011DA RID: 4570
		// (get) Token: 0x06004508 RID: 17672 RVA: 0x00267464 File Offset: 0x00265664
		// (set) Token: 0x06004509 RID: 17673 RVA: 0x0026746C File Offset: 0x0026566C
		public Vector2 B { get; set; }

		// Token: 0x170011DB RID: 4571
		// (get) Token: 0x0600450A RID: 17674 RVA: 0x00267475 File Offset: 0x00265675
		// (set) Token: 0x0600450B RID: 17675 RVA: 0x0026747D File Offset: 0x0026567D
		public Vector2 C { get; set; }

		// Token: 0x170011DC RID: 4572
		// (get) Token: 0x0600450C RID: 17676 RVA: 0x00267486 File Offset: 0x00265686
		// (set) Token: 0x0600450D RID: 17677 RVA: 0x0026748E File Offset: 0x0026568E
		public Vector2 D { get; set; }

		// Token: 0x170011DD RID: 4573
		// (get) Token: 0x0600450E RID: 17678 RVA: 0x00267497 File Offset: 0x00265697
		public Vector2 Centroid
		{
			get
			{
				return (this.A + this.B + this.C + this.D) / 4f;
			}
		}

		// Token: 0x0600450F RID: 17679 RVA: 0x002674CC File Offset: 0x002656CC
		public static Quad2D FromRectangle(RectangleF rectangle)
		{
			return new Quad2D(new ValueTuple<float, float>(rectangle.Left, rectangle.Top), new ValueTuple<float, float>(rectangle.Right, rectangle.Top), new ValueTuple<float, float>(rectangle.Right, rectangle.Bottom), new ValueTuple<float, float>(rectangle.Left, rectangle.Bottom));
		}

		// Token: 0x06004510 RID: 17680 RVA: 0x00267540 File Offset: 0x00265740
		public static Quad2D FromSubmarineRectangle(RectangleF rectangle)
		{
			return new Quad2D(new ValueTuple<float, float>(rectangle.X, rectangle.Y), new ValueTuple<float, float>(rectangle.X + rectangle.Width, rectangle.Y), new ValueTuple<float, float>(rectangle.X + rectangle.Width, rectangle.Y - rectangle.Height), new ValueTuple<float, float>(rectangle.X, rectangle.Y - rectangle.Height));
		}

		// Token: 0x06004511 RID: 17681 RVA: 0x002675C8 File Offset: 0x002657C8
		public Quad2D Rotated(float radians)
		{
			return new Quad2D(MathUtils.RotatePointAroundTarget(this.A, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.B, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.C, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.D, this.Centroid, radians, true));
		}

		// Token: 0x170011DE RID: 4574
		// (get) Token: 0x06004512 RID: 17682 RVA: 0x00267628 File Offset: 0x00265828
		public RectangleF BoundingAxisAlignedRectangle
		{
			get
			{
				Vector2 min = new ValueTuple<float, float>(Math.Min(this.A.X, Math.Min(this.B.X, Math.Min(this.C.X, this.D.X))), Math.Min(this.A.Y, Math.Min(this.B.Y, Math.Min(this.C.Y, this.D.Y))));
				Vector2 max = new ValueTuple<float, float>(Math.Max(this.A.X, Math.Max(this.B.X, Math.Max(this.C.X, this.D.X))), Math.Max(this.A.Y, Math.Max(this.B.Y, Math.Max(this.C.Y, this.D.Y))));
				return new RectangleF(min, max - min);
			}
		}

		// Token: 0x06004513 RID: 17683 RVA: 0x00267744 File Offset: 0x00265944
		public unsafe bool TryGetEdges([TupleElementNames(new string[]
		{
			"A",
			"B"
		})] Span<ValueTuple<Vector2, Vector2>> outputSpan)
		{
			if (outputSpan.Length < 4)
			{
				return false;
			}
			*outputSpan[0] = new ValueTuple<Vector2, Vector2>(this.A, this.B);
			*outputSpan[1] = new ValueTuple<Vector2, Vector2>(this.B, this.C);
			*outputSpan[2] = new ValueTuple<Vector2, Vector2>(this.C, this.D);
			*outputSpan[3] = new ValueTuple<Vector2, Vector2>(this.D, this.A);
			return true;
		}

		// Token: 0x06004514 RID: 17684 RVA: 0x002677D8 File Offset: 0x002659D8
		public bool Contains(Vector2 point)
		{
			Triangle2D triangle2D = new Triangle2D(this.A, this.B, this.C);
			Triangle2D triangle2 = new Triangle2D(this.A, this.D, this.C);
			Triangle2D triangle3 = triangle2D;
			if (triangle3.Contains(this.D) || triangle2.Contains(this.B))
			{
				Triangle2D triangle2D2 = new Triangle2D(this.B, this.C, this.D);
				triangle2 = new Triangle2D(this.B, this.A, this.D);
				triangle3 = triangle2D2;
			}
			return triangle3.Contains(point) || triangle2.Contains(point);
		}

		// Token: 0x06004515 RID: 17685 RVA: 0x00267878 File Offset: 0x00265A78
		public unsafe bool Intersects(Quad2D other)
		{
			if (!this.BoundingAxisAlignedRectangle.Intersects(other.BoundingAxisAlignedRectangle))
			{
				return false;
			}
			if (this.Contains(other.A))
			{
				return true;
			}
			if (this.Contains(other.B))
			{
				return true;
			}
			if (this.Contains(other.C))
			{
				return true;
			}
			if (this.Contains(other.D))
			{
				return true;
			}
			if (other.Contains(this.A))
			{
				return true;
			}
			if (other.Contains(this.B))
			{
				return true;
			}
			if (other.Contains(this.C))
			{
				return true;
			}
			if (other.Contains(this.D))
			{
				return true;
			}
			Span<ValueTuple<Vector2, Vector2>> otherEdges;
			Span<ValueTuple<Vector2, Vector2>> span2;
			checked
			{
				Span<ValueTuple<Vector2, Vector2>> span = new Span<ValueTuple<Vector2, Vector2>>(stackalloc byte[unchecked((UIntPtr)4) * (UIntPtr)sizeof(ValueTuple<Vector2, Vector2>)], 4);
				Span<ValueTuple<Vector2, Vector2>> myEdges = span;
				this.TryGetEdges(myEdges);
				span = new Span<ValueTuple<Vector2, Vector2>>(stackalloc byte[unchecked((UIntPtr)4) * (UIntPtr)sizeof(ValueTuple<Vector2, Vector2>)], 4);
				otherEdges = span;
				other.TryGetEdges(otherEdges);
				span2 = myEdges;
			}
			for (int i = 0; i < span2.Length; i++)
			{
				ValueTuple<Vector2, Vector2> edge = *span2[i];
				Span<ValueTuple<Vector2, Vector2>> span3 = otherEdges;
				for (int j = 0; j < span3.Length; j++)
				{
					ValueTuple<Vector2, Vector2> otherEdge = *span3[j];
					if (MathUtils.LineSegmentsIntersect(edge.Item1, edge.Item2, otherEdge.Item1, otherEdge.Item2))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06004516 RID: 17686 RVA: 0x002679D8 File Offset: 0x00265BD8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Quad2D");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06004517 RID: 17687 RVA: 0x00267A24 File Offset: 0x00265C24
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("A = ");
			builder.Append(this.A.ToString());
			builder.Append(", B = ");
			builder.Append(this.B.ToString());
			builder.Append(", C = ");
			builder.Append(this.C.ToString());
			builder.Append(", D = ");
			builder.Append(this.D.ToString());
			builder.Append(", Centroid = ");
			builder.Append(this.Centroid.ToString());
			builder.Append(", BoundingAxisAlignedRectangle = ");
			builder.Append(this.BoundingAxisAlignedRectangle.ToString());
			return true;
		}

		// Token: 0x06004518 RID: 17688 RVA: 0x00267B1C File Offset: 0x00265D1C
		[CompilerGenerated]
		public static bool operator !=(Quad2D left, Quad2D right)
		{
			return !(left == right);
		}

		// Token: 0x06004519 RID: 17689 RVA: 0x00267B28 File Offset: 0x00265D28
		[CompilerGenerated]
		public static bool operator ==(Quad2D left, Quad2D right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600451A RID: 17690 RVA: 0x00267B34 File Offset: 0x00265D34
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2>.Default.GetHashCode(this.<A>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<B>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<C>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<D>k__BackingField);
		}

		// Token: 0x0600451B RID: 17691 RVA: 0x00267B96 File Offset: 0x00265D96
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Quad2D && this.Equals((Quad2D)obj);
		}

		// Token: 0x0600451C RID: 17692 RVA: 0x00267BB0 File Offset: 0x00265DB0
		[CompilerGenerated]
		public bool Equals(Quad2D other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<A>k__BackingField, other.<A>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<B>k__BackingField, other.<B>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<C>k__BackingField, other.<C>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<D>k__BackingField, other.<D>k__BackingField);
		}

		// Token: 0x0600451D RID: 17693 RVA: 0x00267C1D File Offset: 0x00265E1D
		[CompilerGenerated]
		public void Deconstruct(out Vector2 A, out Vector2 B, out Vector2 C, out Vector2 D)
		{
			A = this.A;
			B = this.B;
			C = this.C;
			D = this.D;
		}
	}
}
