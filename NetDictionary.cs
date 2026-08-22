using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200038F RID: 911
	[NetworkSerialize(20)]
	public readonly struct NetDictionary<[Nullable(1)] T, [Nullable(2)] U> : INetSerializableStruct, IEquatable<NetDictionary<T, U>>
	{
		// Token: 0x06004484 RID: 17540 RVA: 0x00264744 File Offset: 0x00262944
		public NetDictionary([Nullable(new byte[]
		{
			0,
			0,
			1,
			1
		})] ImmutableArray<NetPair<T, U>> Pairs)
		{
			this.Pairs = Pairs;
		}

		// Token: 0x170011D1 RID: 4561
		// (get) Token: 0x06004485 RID: 17541 RVA: 0x0026474D File Offset: 0x0026294D
		// (set) Token: 0x06004486 RID: 17542 RVA: 0x00264755 File Offset: 0x00262955
		[Nullable(new byte[]
		{
			0,
			0,
			1,
			1
		})]
		public ImmutableArray<NetPair<T, U>> Pairs { [return: Nullable(new byte[]
		{
			0,
			0,
			1,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			0,
			1,
			1
		})] set; }

		// Token: 0x06004487 RID: 17543 RVA: 0x00264760 File Offset: 0x00262960
		[NullableContext(1)]
		public Dictionary<T, U> ToDictionary()
		{
			return this.Pairs.ToDictionary((NetPair<T, U> pair) => pair.First, (NetPair<T, U> pair) => pair.Second);
		}

		// Token: 0x06004488 RID: 17544 RVA: 0x002647B8 File Offset: 0x002629B8
		[NullableContext(1)]
		public ImmutableDictionary<T, U> ToImmutableDictionary()
		{
			return this.Pairs.ToImmutableDictionary((NetPair<T, U> pair) => pair.First, (NetPair<T, U> pair) => pair.Second);
		}

		// Token: 0x06004489 RID: 17545 RVA: 0x00264814 File Offset: 0x00262A14
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetDictionary");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600448A RID: 17546 RVA: 0x00264860 File Offset: 0x00262A60
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Pairs = ");
			builder.Append(this.Pairs.ToString());
			return true;
		}

		// Token: 0x0600448B RID: 17547 RVA: 0x00264895 File Offset: 0x00262A95
		[CompilerGenerated]
		public static bool operator !=(NetDictionary<T, U> left, NetDictionary<T, U> right)
		{
			return !(left == right);
		}

		// Token: 0x0600448C RID: 17548 RVA: 0x002648A1 File Offset: 0x00262AA1
		[CompilerGenerated]
		public static bool operator ==(NetDictionary<T, U> left, NetDictionary<T, U> right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600448D RID: 17549 RVA: 0x002648AB File Offset: 0x00262AAB
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<NetPair<T, U>>>.Default.GetHashCode(this.<Pairs>k__BackingField);
		}

		// Token: 0x0600448E RID: 17550 RVA: 0x002648BD File Offset: 0x00262ABD
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetDictionary<T, U> && this.Equals((NetDictionary<T, U>)obj);
		}

		// Token: 0x0600448F RID: 17551 RVA: 0x002648D5 File Offset: 0x00262AD5
		[CompilerGenerated]
		public bool Equals(NetDictionary<T, U> other)
		{
			return EqualityComparer<ImmutableArray<NetPair<T, U>>>.Default.Equals(this.<Pairs>k__BackingField, other.<Pairs>k__BackingField);
		}

		// Token: 0x06004490 RID: 17552 RVA: 0x002648ED File Offset: 0x00262AED
		[CompilerGenerated]
		public void Deconstruct([Nullable(new byte[]
		{
			0,
			0,
			1,
			1
		})] out ImmutableArray<NetPair<T, U>> Pairs)
		{
			Pairs = this.Pairs;
		}
	}
}
