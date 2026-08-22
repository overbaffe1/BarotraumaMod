using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200038E RID: 910
	[NetworkSerialize(7)]
	public readonly struct NetCollection<T> : INetSerializableStruct, IEnumerable<!0>, IEnumerable, IEquatable<NetCollection<T>>
	{
		// Token: 0x06004475 RID: 17525 RVA: 0x002645FA File Offset: 0x002627FA
		public NetCollection(ImmutableArray<T> Array)
		{
			this.Array = Array;
		}

		// Token: 0x170011D0 RID: 4560
		// (get) Token: 0x06004476 RID: 17526 RVA: 0x00264603 File Offset: 0x00262803
		// (set) Token: 0x06004477 RID: 17527 RVA: 0x0026460B File Offset: 0x0026280B
		public ImmutableArray<T> Array { get; set; }

		// Token: 0x06004478 RID: 17528 RVA: 0x00264614 File Offset: 0x00262814
		public NetCollection(params T[] elements)
		{
			this = new NetCollection<T>(elements.ToImmutableArray<T>());
		}

		// Token: 0x06004479 RID: 17529 RVA: 0x00264627 File Offset: 0x00262827
		IEnumerator<T> IEnumerable<!0>.GetEnumerator()
		{
			return ((IEnumerable<!0>)this.Array).GetEnumerator();
		}

		// Token: 0x0600447A RID: 17530 RVA: 0x00264639 File Offset: 0x00262839
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.Array).GetEnumerator();
		}

		// Token: 0x0600447B RID: 17531 RVA: 0x0026464C File Offset: 0x0026284C
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

		// Token: 0x0600447C RID: 17532 RVA: 0x00264698 File Offset: 0x00262898
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Array = ");
			builder.Append(this.Array.ToString());
			return true;
		}

		// Token: 0x0600447D RID: 17533 RVA: 0x002646CD File Offset: 0x002628CD
		[CompilerGenerated]
		public static bool operator !=(NetCollection<T> left, NetCollection<T> right)
		{
			return !(left == right);
		}

		// Token: 0x0600447E RID: 17534 RVA: 0x002646D9 File Offset: 0x002628D9
		[CompilerGenerated]
		public static bool operator ==(NetCollection<T> left, NetCollection<T> right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600447F RID: 17535 RVA: 0x002646E3 File Offset: 0x002628E3
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<T>>.Default.GetHashCode(this.<Array>k__BackingField);
		}

		// Token: 0x06004480 RID: 17536 RVA: 0x002646F5 File Offset: 0x002628F5
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCollection<T> && this.Equals((NetCollection<T>)obj);
		}

		// Token: 0x06004481 RID: 17537 RVA: 0x0026470D File Offset: 0x0026290D
		[CompilerGenerated]
		public bool Equals(NetCollection<T> other)
		{
			return EqualityComparer<ImmutableArray<T>>.Default.Equals(this.<Array>k__BackingField, other.<Array>k__BackingField);
		}

		// Token: 0x06004482 RID: 17538 RVA: 0x00264725 File Offset: 0x00262925
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<T> Array)
		{
			Array = this.Array;
		}

		// Token: 0x040023E7 RID: 9191
		public static readonly NetCollection<T> Empty = new NetCollection<T>(ImmutableArray<T>.Empty);
	}
}
