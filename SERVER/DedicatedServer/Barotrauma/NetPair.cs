using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020002C7 RID: 711
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(4)]
	public readonly struct NetPair<[Nullable(2)] T, [Nullable(2)] U> : INetSerializableStruct, IEquatable<NetPair<T, U>>
	{
		// Token: 0x06003018 RID: 12312 RVA: 0x0014AEF6 File Offset: 0x001490F6
		public NetPair(T First, U Second)
		{
			this.First = First;
			this.Second = Second;
		}

		// Token: 0x17000DDD RID: 3549
		// (get) Token: 0x06003019 RID: 12313 RVA: 0x0014AF06 File Offset: 0x00149106
		// (set) Token: 0x0600301A RID: 12314 RVA: 0x0014AF0E File Offset: 0x0014910E
		public T First { get; set; }

		// Token: 0x17000DDE RID: 3550
		// (get) Token: 0x0600301B RID: 12315 RVA: 0x0014AF17 File Offset: 0x00149117
		// (set) Token: 0x0600301C RID: 12316 RVA: 0x0014AF1F File Offset: 0x0014911F
		public U Second { get; set; }

		// Token: 0x0600301D RID: 12317 RVA: 0x0014AF28 File Offset: 0x00149128
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

		// Token: 0x0600301E RID: 12318 RVA: 0x0014AF74 File Offset: 0x00149174
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

		// Token: 0x0600301F RID: 12319 RVA: 0x0014AFB3 File Offset: 0x001491B3
		[NullableContext(0)]
		[CompilerGenerated]
		public static bool operator !=(NetPair<T, U> left, NetPair<T, U> right)
		{
			return !(left == right);
		}

		// Token: 0x06003020 RID: 12320 RVA: 0x0014AFBF File Offset: 0x001491BF
		[NullableContext(0)]
		[CompilerGenerated]
		public static bool operator ==(NetPair<T, U> left, NetPair<T, U> right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003021 RID: 12321 RVA: 0x0014AFC9 File Offset: 0x001491C9
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode(this.<First>k__BackingField) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(this.<Second>k__BackingField);
		}

		// Token: 0x06003022 RID: 12322 RVA: 0x0014AFF2 File Offset: 0x001491F2
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetPair<T, U> && this.Equals((NetPair<T, U>)obj);
		}

		// Token: 0x06003023 RID: 12323 RVA: 0x0014B00A File Offset: 0x0014920A
		[NullableContext(0)]
		[CompilerGenerated]
		public bool Equals(NetPair<T, U> other)
		{
			return EqualityComparer<T>.Default.Equals(this.<First>k__BackingField, other.<First>k__BackingField) && EqualityComparer<U>.Default.Equals(this.<Second>k__BackingField, other.<Second>k__BackingField);
		}

		// Token: 0x06003024 RID: 12324 RVA: 0x0014B03C File Offset: 0x0014923C
		[CompilerGenerated]
		public void Deconstruct(out T First, out U Second)
		{
			First = this.First;
			Second = this.Second;
		}
	}
}
