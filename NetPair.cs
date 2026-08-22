using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000392 RID: 914
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(4)]
	public readonly struct NetPair<[Nullable(2)] T, [Nullable(2)] U> : INetSerializableStruct, IEquatable<NetPair<T, U>>
	{
		// Token: 0x06004496 RID: 17558 RVA: 0x0026499A File Offset: 0x00262B9A
		public NetPair(T First, U Second)
		{
			this.First = First;
			this.Second = Second;
		}

		// Token: 0x170011D2 RID: 4562
		// (get) Token: 0x06004497 RID: 17559 RVA: 0x002649AA File Offset: 0x00262BAA
		// (set) Token: 0x06004498 RID: 17560 RVA: 0x002649B2 File Offset: 0x00262BB2
		public T First { get; set; }

		// Token: 0x170011D3 RID: 4563
		// (get) Token: 0x06004499 RID: 17561 RVA: 0x002649BB File Offset: 0x00262BBB
		// (set) Token: 0x0600449A RID: 17562 RVA: 0x002649C3 File Offset: 0x00262BC3
		public U Second { get; set; }

		// Token: 0x0600449B RID: 17563 RVA: 0x002649CC File Offset: 0x00262BCC
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetPair");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600449C RID: 17564 RVA: 0x00264A18 File Offset: 0x00262C18
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("First = ");
			builder.Append(this.First);
			builder.Append(", Second = ");
			builder.Append(this.Second);
			return true;
		}

		// Token: 0x0600449D RID: 17565 RVA: 0x00264A57 File Offset: 0x00262C57
		[NullableContext(0)]
		[CompilerGenerated]
		public static bool operator !=(NetPair<T, U> left, NetPair<T, U> right)
		{
			return !(left == right);
		}

		// Token: 0x0600449E RID: 17566 RVA: 0x00264A63 File Offset: 0x00262C63
		[NullableContext(0)]
		[CompilerGenerated]
		public static bool operator ==(NetPair<T, U> left, NetPair<T, U> right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600449F RID: 17567 RVA: 0x00264A6D File Offset: 0x00262C6D
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode(this.<First>k__BackingField) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(this.<Second>k__BackingField);
		}

		// Token: 0x060044A0 RID: 17568 RVA: 0x00264A96 File Offset: 0x00262C96
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetPair<T, U> && this.Equals((NetPair<T, U>)obj);
		}

		// Token: 0x060044A1 RID: 17569 RVA: 0x00264AAE File Offset: 0x00262CAE
		[NullableContext(0)]
		[CompilerGenerated]
		public bool Equals(NetPair<T, U> other)
		{
			return EqualityComparer<T>.Default.Equals(this.<First>k__BackingField, other.<First>k__BackingField) && EqualityComparer<U>.Default.Equals(this.<Second>k__BackingField, other.<Second>k__BackingField);
		}

		// Token: 0x060044A2 RID: 17570 RVA: 0x00264AE0 File Offset: 0x00262CE0
		[CompilerGenerated]
		public void Deconstruct(out T First, out U Second)
		{
			First = this.First;
			Second = this.Second;
		}
	}
}
