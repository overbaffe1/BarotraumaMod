using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020002C3 RID: 707
	[NetworkSerialize(7)]
	public readonly struct NetCollection<T> : INetSerializableStruct, IEnumerable<T>, IEnumerable, IEquatable<NetCollection<T>>
	{
		// Token: 0x06002FF7 RID: 12279 RVA: 0x0014AB56 File Offset: 0x00148D56
		public NetCollection(ImmutableArray<T> Array)
		{
			this.Array = Array;
		}

		// Token: 0x17000DDB RID: 3547
		// (get) Token: 0x06002FF8 RID: 12280 RVA: 0x0014AB5F File Offset: 0x00148D5F
		// (set) Token: 0x06002FF9 RID: 12281 RVA: 0x0014AB67 File Offset: 0x00148D67
		public ImmutableArray<T> Array { get; set; }

		// Token: 0x06002FFA RID: 12282 RVA: 0x0014AB70 File Offset: 0x00148D70
		public NetCollection(params T[] elements)
		{
			this = new NetCollection<T>(elements.ToImmutableArray<T>());
		}

		// Token: 0x06002FFB RID: 12283 RVA: 0x0014AB83 File Offset: 0x00148D83
		IEnumerator<T> IEnumerable<!0>.GetEnumerator()
		{
			return ((IEnumerable<!0>)this.Array).GetEnumerator();
		}

		// Token: 0x06002FFC RID: 12284 RVA: 0x0014AB95 File Offset: 0x00148D95
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.Array).GetEnumerator();
		}

		// Token: 0x06002FFD RID: 12285 RVA: 0x0014ABA8 File Offset: 0x00148DA8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetCollection");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06002FFE RID: 12286 RVA: 0x0014ABF4 File Offset: 0x00148DF4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Array = ");
			builder.Append(this.Array.ToString());
			return true;
		}

		// Token: 0x06002FFF RID: 12287 RVA: 0x0014AC29 File Offset: 0x00148E29
		[CompilerGenerated]
		public static bool operator !=(NetCollection<T> left, NetCollection<T> right)
		{
			return !(left == right);
		}

		// Token: 0x06003000 RID: 12288 RVA: 0x0014AC35 File Offset: 0x00148E35
		[CompilerGenerated]
		public static bool operator ==(NetCollection<T> left, NetCollection<T> right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003001 RID: 12289 RVA: 0x0014AC3F File Offset: 0x00148E3F
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<T>>.Default.GetHashCode(this.<Array>k__BackingField);
		}

		// Token: 0x06003002 RID: 12290 RVA: 0x0014AC51 File Offset: 0x00148E51
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCollection<T> && this.Equals((NetCollection<T>)obj);
		}

		// Token: 0x06003003 RID: 12291 RVA: 0x0014AC69 File Offset: 0x00148E69
		[CompilerGenerated]
		public bool Equals(NetCollection<T> other)
		{
			return EqualityComparer<ImmutableArray<T>>.Default.Equals(this.<Array>k__BackingField, other.<Array>k__BackingField);
		}

		// Token: 0x06003004 RID: 12292 RVA: 0x0014AC81 File Offset: 0x00148E81
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<T> Array)
		{
			Array = this.Array;
		}

		// Token: 0x04001815 RID: 6165
		public static readonly NetCollection<T> Empty = new NetCollection<T>(ImmutableArray<T>.Empty);
	}
}
