using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002D0 RID: 720
	internal readonly struct Quad2D : IEquatable<Quad2D>
	{
		// Token: 0x06003087 RID: 12423 RVA: 0x0014D9D8 File Offset: 0x0014BBD8
		public Quad2D(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
		{
			this.A = A;
			this.B = B;
			this.C = C;
			this.D = D;
		}

		// Token: 0x17000DE4 RID: 3556
		// (get) Token: 0x06003088 RID: 12424 RVA: 0x0014D9F7 File Offset: 0x0014BBF7
		// (set) Token: 0x06003089 RID: 12425 RVA: 0x0014D9FF File Offset: 0x0014BBFF
		public Vector2 A { get; set; }

		// Token: 0x17000DE5 RID: 3557
		// (get) Token: 0x0600308A RID: 12426 RVA: 0x0014DA08 File Offset: 0x0014BC08
		// (set) Token: 0x0600308B RID: 12427 RVA: 0x0014DA10 File Offset: 0x0014BC10
		public Vector2 B { get; set; }

		// Token: 0x17000DE6 RID: 3558
		// (get) Token: 0x0600308C RID: 12428 RVA: 0x0014DA19 File Offset: 0x0014BC19
		// (set) Token: 0x0600308D RID: 12429 RVA: 0x0014DA21 File Offset: 0x0014BC21
		public Vector2 C { get; set; }

		// Token: 0x17000DE7 RID: 3559
		// (get) Token: 0x0600308E RID: 12430 RVA: 0x0014DA2A File Offset: 0x0014BC2A
		// (set) Token: 0x0600308F RID: 12431 RVA: 0x0014DA32 File Offset: 0x0014BC32
		public Vector2 D { get; set; }

		// Token: 0x17000DE8 RID: 3560
		// (get) Token: 0x06003090 RID: 12432 RVA: 0x0014DA3B File Offset: 0x0014BC3B
		public Vector2 Centroid
		{
			get
			{
				return (this.A + this.B + this.C + this.D) / 4f;
			}
		}

		// Token: 0x06003091 RID: 12433 RVA: 0x0014DA70 File Offset: 0x0014BC70
		public static Quad2D FromRectangle(RectangleF rectangle)
		{
			return new Quad2D(new ValueTuple<float, float>(rectangle.Left, rectangle.Top), new ValueTuple<float, float>(rectangle.Right, rectangle.Top), new ValueTuple<float, float>(rectangle.Right, rectangle.Bottom), new ValueTuple<float, float>(rectangle.Left, rectangle.Bottom));
		}

		// Token: 0x06003092 RID: 12434 RVA: 0x0014DAE4 File Offset: 0x0014BCE4
		public static Quad2D FromSubmarineRectangle(RectangleF rectangle)
		{
			return new Quad2D(new ValueTuple<float, float>(rectangle.X, rectangle.Y), new ValueTuple<float, float>(rectangle.X + rectangle.Width, rectangle.Y), new ValueTuple<float, float>(rectangle.X + rectangle.Width, rectangle.Y - rectangle.Height), new ValueTuple<float, float>(rectangle.X, rectangle.Y - rectangle.Height));
		}

		// Token: 0x06003093 RID: 12435 RVA: 0x0014DB6C File Offset: 0x0014BD6C
		public Quad2D Rotated(float radians)
		{
			return new Quad2D(MathUtils.RotatePointAroundTarget(this.A, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.B, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.C, this.Centroid, radians, true), MathUtils.RotatePointAroundTarget(this.D, this.Centroid, radians, true));
		}

		// Token: 0x17000DE9 RID: 3561
		// (get) Token: 0x06003094 RID: 12436 RVA: 0x0014DBCC File Offset: 0x0014BDCC
		public RectangleF BoundingAxisAlignedRectangle
		{
			get
			{
				Vector2 min = new ValueTuple<float, float>(Math.Min(this.A.X, Math.Min(this.B.X, Math.Min(this.C.X, this.D.X))), Math.Min(this.A.Y, Math.Min(this.B.Y, Math.Min(this.C.Y, this.D.Y))));
				Vector2 max = new ValueTuple<float, float>(Math.Max(this.A.X, Math.Max(this.B.X, Math.Max(this.C.X, this.D.X))), Math.Max(this.A.Y, Math.Max(this.B.Y, Math.Max(this.C.Y, this.D.Y))));
				return new RectangleF(min, max - min);
			}
		}

		// Token: 0x06003095 RID: 12437 RVA: 0x0014DCE8 File Offset: 0x0014BEE8
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

		// Token: 0x06003096 RID: 12438 RVA: 0x0014DD7C File Offset: 0x0014BF7C
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

		// Token: 0x06003097 RID: 12439 RVA: 0x0014DE1C File Offset: 0x0014C01C
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

		// Token: 0x06003098 RID: 12440 RVA: 0x0014DF7C File Offset: 0x0014C17C
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

		// Token: 0x06003099 RID: 12441 RVA: 0x0014DFC8 File Offset: 0x0014C1C8
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

		// Token: 0x0600309A RID: 12442 RVA: 0x0014E0C0 File Offset: 0x0014C2C0
		[CompilerGenerated]
		public static bool operator !=(Quad2D left, Quad2D right)
		{
			return !(left == right);
		}

		// Token: 0x0600309B RID: 12443 RVA: 0x0014E0CC File Offset: 0x0014C2CC
		[CompilerGenerated]
		public static bool operator ==(Quad2D left, Quad2D right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600309C RID: 12444 RVA: 0x0014E0D8 File Offset: 0x0014C2D8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2>.Default.GetHashCode(this.<A>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<B>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<C>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<D>k__BackingField);
		}

		// Token: 0x0600309D RID: 12445 RVA: 0x0014E13A File Offset: 0x0014C33A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Quad2D && this.Equals((Quad2D)obj);
		}

		// Token: 0x0600309E RID: 12446 RVA: 0x0014E154 File Offset: 0x0014C354
		[CompilerGenerated]
		public bool Equals(Quad2D other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<A>k__BackingField, other.<A>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<B>k__BackingField, other.<B>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<C>k__BackingField, other.<C>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<D>k__BackingField, other.<D>k__BackingField);
		}

		// Token: 0x0600309F RID: 12447 RVA: 0x0014E1C1 File Offset: 0x0014C3C1
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
