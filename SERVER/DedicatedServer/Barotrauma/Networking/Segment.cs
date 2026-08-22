using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003CF RID: 975
	[NetworkSerialize(79)]
	public readonly struct Segment<T> : INetSerializableStruct, IEquatable<Segment<T>> where T : struct
	{
		// Token: 0x06003817 RID: 14359 RVA: 0x0017786E File Offset: 0x00175A6E
		public Segment(T Identifier, int Pointer)
		{
			this.Identifier = Identifier;
			this.Pointer = Pointer;
		}

		// Token: 0x17000F63 RID: 3939
		// (get) Token: 0x06003818 RID: 14360 RVA: 0x0017787E File Offset: 0x00175A7E
		// (set) Token: 0x06003819 RID: 14361 RVA: 0x00177886 File Offset: 0x00175A86
		public T Identifier { get; set; }

		// Token: 0x17000F64 RID: 3940
		// (get) Token: 0x0600381A RID: 14362 RVA: 0x0017788F File Offset: 0x00175A8F
		// (set) Token: 0x0600381B RID: 14363 RVA: 0x00177897 File Offset: 0x00175A97
		public int Pointer { get; set; }

		// Token: 0x0600381C RID: 14364 RVA: 0x001778A0 File Offset: 0x00175AA0
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Segment");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600381D RID: 14365 RVA: 0x001778EC File Offset: 0x00175AEC
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Identifier = ");
			T identifier = this.Identifier;
			builder.Append(identifier.ToString());
			builder.Append(", Pointer = ");
			builder.Append(this.Pointer.ToString());
			return true;
		}

		// Token: 0x0600381E RID: 14366 RVA: 0x00177948 File Offset: 0x00175B48
		[CompilerGenerated]
		public static bool operator !=(Segment<T> left, Segment<T> right)
		{
			return !(left == right);
		}

		// Token: 0x0600381F RID: 14367 RVA: 0x00177954 File Offset: 0x00175B54
		[CompilerGenerated]
		public static bool operator ==(Segment<T> left, Segment<T> right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003820 RID: 14368 RVA: 0x0017795E File Offset: 0x00175B5E
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Pointer>k__BackingField);
		}

		// Token: 0x06003821 RID: 14369 RVA: 0x00177987 File Offset: 0x00175B87
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Segment<T> && this.Equals((Segment<T>)obj);
		}

		// Token: 0x06003822 RID: 14370 RVA: 0x0017799F File Offset: 0x00175B9F
		[CompilerGenerated]
		public bool Equals(Segment<T> other)
		{
			return EqualityComparer<T>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Pointer>k__BackingField, other.<Pointer>k__BackingField);
		}

		// Token: 0x06003823 RID: 14371 RVA: 0x001779D1 File Offset: 0x00175BD1
		[CompilerGenerated]
		public void Deconstruct(out T Identifier, out int Pointer)
		{
			Identifier = this.Identifier;
			Pointer = this.Pointer;
		}
	}
}
