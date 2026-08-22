using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002D4 RID: 724
	internal readonly struct SquareLine : IEquatable<SquareLine>
	{
		// Token: 0x060030B6 RID: 12470 RVA: 0x0014E4EB File Offset: 0x0014C6EB
		public SquareLine(Vector2[] Points, SquareLine.LineType Type)
		{
			this.Points = Points;
			this.Type = Type;
		}

		// Token: 0x17000DEF RID: 3567
		// (get) Token: 0x060030B7 RID: 12471 RVA: 0x0014E4FB File Offset: 0x0014C6FB
		// (set) Token: 0x060030B8 RID: 12472 RVA: 0x0014E503 File Offset: 0x0014C703
		public Vector2[] Points { get; set; }

		// Token: 0x17000DF0 RID: 3568
		// (get) Token: 0x060030B9 RID: 12473 RVA: 0x0014E50C File Offset: 0x0014C70C
		// (set) Token: 0x060030BA RID: 12474 RVA: 0x0014E514 File Offset: 0x0014C714
		public SquareLine.LineType Type { get; set; }

		// Token: 0x060030BB RID: 12475 RVA: 0x0014E520 File Offset: 0x0014C720
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SquareLine");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060030BC RID: 12476 RVA: 0x0014E56C File Offset: 0x0014C76C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Points = ");
			builder.Append(this.Points);
			builder.Append(", Type = ");
			builder.Append(this.Type.ToString());
			return true;
		}

		// Token: 0x060030BD RID: 12477 RVA: 0x0014E5BA File Offset: 0x0014C7BA
		[CompilerGenerated]
		public static bool operator !=(SquareLine left, SquareLine right)
		{
			return !(left == right);
		}

		// Token: 0x060030BE RID: 12478 RVA: 0x0014E5C6 File Offset: 0x0014C7C6
		[CompilerGenerated]
		public static bool operator ==(SquareLine left, SquareLine right)
		{
			return left.Equals(right);
		}

		// Token: 0x060030BF RID: 12479 RVA: 0x0014E5D0 File Offset: 0x0014C7D0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Vector2[]>.Default.GetHashCode(this.<Points>k__BackingField) * -1521134295 + EqualityComparer<SquareLine.LineType>.Default.GetHashCode(this.<Type>k__BackingField);
		}

		// Token: 0x060030C0 RID: 12480 RVA: 0x0014E5F9 File Offset: 0x0014C7F9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is SquareLine && this.Equals((SquareLine)obj);
		}

		// Token: 0x060030C1 RID: 12481 RVA: 0x0014E611 File Offset: 0x0014C811
		[CompilerGenerated]
		public bool Equals(SquareLine other)
		{
			return EqualityComparer<Vector2[]>.Default.Equals(this.<Points>k__BackingField, other.<Points>k__BackingField) && EqualityComparer<SquareLine.LineType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField);
		}

		// Token: 0x060030C2 RID: 12482 RVA: 0x0014E643 File Offset: 0x0014C843
		[CompilerGenerated]
		public void Deconstruct(out Vector2[] Points, out SquareLine.LineType Type)
		{
			Points = this.Points;
			Type = this.Type;
		}

		// Token: 0x02000B74 RID: 2932
		internal enum LineType
		{
			// Token: 0x04003982 RID: 14722
			FourPointForwardsLine,
			// Token: 0x04003983 RID: 14723
			SixPointBackwardsLine
		}
	}
}
