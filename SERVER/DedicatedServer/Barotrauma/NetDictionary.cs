using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020002C4 RID: 708
	[NetworkSerialize(20)]
	public readonly struct NetDictionary<[Nullable(1)] T, [Nullable(2)] U> : INetSerializableStruct, IEquatable<NetDictionary<T, U>>
	{
		// Token: 0x06003006 RID: 12294 RVA: 0x0014ACA0 File Offset: 0x00148EA0
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

		// Token: 0x17000DDC RID: 3548
		// (get) Token: 0x06003007 RID: 12295 RVA: 0x0014ACA9 File Offset: 0x00148EA9
		// (set) Token: 0x06003008 RID: 12296 RVA: 0x0014ACB1 File Offset: 0x00148EB1
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

		// Token: 0x06003009 RID: 12297 RVA: 0x0014ACBC File Offset: 0x00148EBC
		[NullableContext(1)]
		public Dictionary<T, U> ToDictionary()
		{
			return this.Pairs.ToDictionary((NetPair<T, U> pair) => pair.First, (NetPair<T, U> pair) => pair.Second);
		}

		// Token: 0x0600300A RID: 12298 RVA: 0x0014AD14 File Offset: 0x00148F14
		[NullableContext(1)]
		public ImmutableDictionary<T, U> ToImmutableDictionary()
		{
			return this.Pairs.ToImmutableDictionary((NetPair<T, U> pair) => pair.First, (NetPair<T, U> pair) => pair.Second);
		}

		// Token: 0x0600300B RID: 12299 RVA: 0x0014AD70 File Offset: 0x00148F70
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

		// Token: 0x0600300C RID: 12300 RVA: 0x0014ADBC File Offset: 0x00148FBC
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Pairs = ");
			builder.Append(this.Pairs.ToString());
			return true;
		}

		// Token: 0x0600300D RID: 12301 RVA: 0x0014ADF1 File Offset: 0x00148FF1
		[CompilerGenerated]
		public static bool operator !=(NetDictionary<T, U> left, NetDictionary<T, U> right)
		{
			return !(left == right);
		}

		// Token: 0x0600300E RID: 12302 RVA: 0x0014ADFD File Offset: 0x00148FFD
		[CompilerGenerated]
		public static bool operator ==(NetDictionary<T, U> left, NetDictionary<T, U> right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600300F RID: 12303 RVA: 0x0014AE07 File Offset: 0x00149007
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<NetPair<T, U>>>.Default.GetHashCode(this.<Pairs>k__BackingField);
		}

		// Token: 0x06003010 RID: 12304 RVA: 0x0014AE19 File Offset: 0x00149019
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetDictionary<T, U> && this.Equals((NetDictionary<T, U>)obj);
		}

		// Token: 0x06003011 RID: 12305 RVA: 0x0014AE31 File Offset: 0x00149031
		[CompilerGenerated]
		public bool Equals(NetDictionary<T, U> other)
		{
			return EqualityComparer<ImmutableArray<NetPair<T, U>>>.Default.Equals(this.<Pairs>k__BackingField, other.<Pairs>k__BackingField);
		}

		// Token: 0x06003012 RID: 12306 RVA: 0x0014AE49 File Offset: 0x00149049
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
