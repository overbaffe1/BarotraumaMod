using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200039F RID: 927
	internal readonly struct SquareLine : IEquatable<SquareLine>
	{
		// Token: 0x06004534 RID: 17716 RVA: 0x00267F47 File Offset: 0x00266147
		public SquareLine(Vector2[] Points, SquareLine.LineType Type)
		{
			this.Points = Points;
			this.Type = Type;
		}

		// Token: 0x170011E4 RID: 4580
		// (get) Token: 0x06004535 RID: 17717 RVA: 0x00267F57 File Offset: 0x00266157
		// (set) Token: 0x06004536 RID: 17718 RVA: 0x00267F5F File Offset: 0x0026615F
		public Vector2[] Points { get; set; }

		// Token: 0x170011E5 RID: 4581
		// (get) Token: 0x06004537 RID: 17719 RVA: 0x00267F68 File Offset: 0x00266168
		// (set) Token: 0x06004538 RID: 17720 RVA: 0x00267F70 File Offset: 0x00266170
		public SquareLine.LineType Type { get; set; }

		// Token: 0x06004539 RID: 17721 RVA: 0x00267F7C File Offset: 0x0026617C
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

		// Token: 0x0600453A RID: 17722 RVA: 0x00267FC8 File Offset: 0x002661C8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Points = ");
			builder.Append(this.Points);
			builder.Append(", Type = ");
			builder.Append(this.Type.ToString());
			return true;
		}

		// Token: 0x0600453B RID: 17723 RVA: 0x00268016 File Offset: 0x00266216
		[CompilerGenerated]
		public static bool operator !=(SquareLine left, SquareLine right)
		{
			return !(left == right);
		}

		// Token: 0x0600453C RID: 17724 RVA: 0x00268022 File Offset: 0x00266222
		[CompilerGenerated]
		public static bool operator ==(SquareLine left, SquareLine right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600453D RID: 17725 RVA: 0x0026802C File Offset: 0x0026622C
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Vector2[]>.Default.GetHashCode(this.<Points>k__BackingField) * -1521134295 + EqualityComparer<SquareLine.LineType>.Default.GetHashCode(this.<Type>k__BackingField);
		}

		// Token: 0x0600453E RID: 17726 RVA: 0x00268055 File Offset: 0x00266255
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is SquareLine && this.Equals((SquareLine)obj);
		}

		// Token: 0x0600453F RID: 17727 RVA: 0x0026806D File Offset: 0x0026626D
		[CompilerGenerated]
		public bool Equals(SquareLine other)
		{
			return EqualityComparer<Vector2[]>.Default.Equals(this.<Points>k__BackingField, other.<Points>k__BackingField) && EqualityComparer<SquareLine.LineType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField);
		}

		// Token: 0x06004540 RID: 17728 RVA: 0x0026809F File Offset: 0x0026629F
		[CompilerGenerated]
		public void Deconstruct(out Vector2[] Points, out SquareLine.LineType Type)
		{
			Points = this.Points;
			Type = this.Type;
		}

		// Token: 0x020010C5 RID: 4293
		internal enum LineType
		{
			// Token: 0x040059A2 RID: 22946
			FourPointForwardsLine,
			// Token: 0x040059A3 RID: 22947
			SixPointBackwardsLine
		}
	}
}
