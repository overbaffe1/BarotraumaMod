using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004CA RID: 1226
	[NetworkSerialize(79)]
	public readonly struct Segment<T> : INetSerializableStruct, IEquatable<Segment<T>> where T : struct
	{
		// Token: 0x06004FE5 RID: 20453 RVA: 0x002AFF36 File Offset: 0x002AE136
		public Segment(T Identifier, int Pointer)
		{
			this.Identifier = Identifier;
			this.Pointer = Pointer;
		}

		// Token: 0x1700145D RID: 5213
		// (get) Token: 0x06004FE6 RID: 20454 RVA: 0x002AFF46 File Offset: 0x002AE146
		// (set) Token: 0x06004FE7 RID: 20455 RVA: 0x002AFF4E File Offset: 0x002AE14E
		public T Identifier { get; set; }

		// Token: 0x1700145E RID: 5214
		// (get) Token: 0x06004FE8 RID: 20456 RVA: 0x002AFF57 File Offset: 0x002AE157
		// (set) Token: 0x06004FE9 RID: 20457 RVA: 0x002AFF5F File Offset: 0x002AE15F
		public int Pointer { get; set; }

		// Token: 0x06004FEA RID: 20458 RVA: 0x002AFF68 File Offset: 0x002AE168
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

		// Token: 0x06004FEB RID: 20459 RVA: 0x002AFFB4 File Offset: 0x002AE1B4
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

		// Token: 0x06004FEC RID: 20460 RVA: 0x002B0010 File Offset: 0x002AE210
		[CompilerGenerated]
		public static bool operator !=(Segment<T> left, Segment<T> right)
		{
			return !(left == right);
		}

		// Token: 0x06004FED RID: 20461 RVA: 0x002B001C File Offset: 0x002AE21C
		[CompilerGenerated]
		public static bool operator ==(Segment<T> left, Segment<T> right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004FEE RID: 20462 RVA: 0x002B0026 File Offset: 0x002AE226
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Pointer>k__BackingField);
		}

		// Token: 0x06004FEF RID: 20463 RVA: 0x002B004F File Offset: 0x002AE24F
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is Segment<T> && this.Equals((Segment<T>)obj);
		}

		// Token: 0x06004FF0 RID: 20464 RVA: 0x002B0067 File Offset: 0x002AE267
		[CompilerGenerated]
		public bool Equals(Segment<T> other)
		{
			return EqualityComparer<T>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Pointer>k__BackingField, other.<Pointer>k__BackingField);
		}

		// Token: 0x06004FF1 RID: 20465 RVA: 0x002B0099 File Offset: 0x002AE299
		[CompilerGenerated]
		public void Deconstruct(out T Identifier, out int Pointer)
		{
			Identifier = this.Identifier;
			Pointer = this.Pointer;
		}
	}
}
