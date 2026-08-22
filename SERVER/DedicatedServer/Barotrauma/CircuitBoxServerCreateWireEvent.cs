using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000118 RID: 280
	[NetworkSerialize(151)]
	internal readonly struct CircuitBoxServerCreateWireEvent : INetSerializableStruct, IEquatable<CircuitBoxServerCreateWireEvent>
	{
		// Token: 0x06001B1D RID: 6941 RVA: 0x000CB93C File Offset: 0x000C9B3C
		public CircuitBoxServerCreateWireEvent(CircuitBoxClientAddWireEvent Request, ushort WireId, Option<ushort> BackingItemId)
		{
			this.Request = Request;
			this.WireId = WireId;
			this.BackingItemId = BackingItemId;
		}

		// Token: 0x17000804 RID: 2052
		// (get) Token: 0x06001B1E RID: 6942 RVA: 0x000CB953 File Offset: 0x000C9B53
		// (set) Token: 0x06001B1F RID: 6943 RVA: 0x000CB95B File Offset: 0x000C9B5B
		public CircuitBoxClientAddWireEvent Request { get; set; }

		// Token: 0x17000805 RID: 2053
		// (get) Token: 0x06001B20 RID: 6944 RVA: 0x000CB964 File Offset: 0x000C9B64
		// (set) Token: 0x06001B21 RID: 6945 RVA: 0x000CB96C File Offset: 0x000C9B6C
		public ushort WireId { get; set; }

		// Token: 0x17000806 RID: 2054
		// (get) Token: 0x06001B22 RID: 6946 RVA: 0x000CB975 File Offset: 0x000C9B75
		// (set) Token: 0x06001B23 RID: 6947 RVA: 0x000CB97D File Offset: 0x000C9B7D
		public Option<ushort> BackingItemId { get; set; }

		// Token: 0x06001B24 RID: 6948 RVA: 0x000CB988 File Offset: 0x000C9B88
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxServerCreateWireEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B25 RID: 6949 RVA: 0x000CB9D4 File Offset: 0x000C9BD4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Request = ");
			builder.Append(this.Request.ToString());
			builder.Append(", WireId = ");
			builder.Append(this.WireId.ToString());
			builder.Append(", BackingItemId = ");
			builder.Append(this.BackingItemId.ToString());
			return true;
		}

		// Token: 0x06001B26 RID: 6950 RVA: 0x000CBA57 File Offset: 0x000C9C57
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerCreateWireEvent left, CircuitBoxServerCreateWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B27 RID: 6951 RVA: 0x000CBA63 File Offset: 0x000C9C63
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerCreateWireEvent left, CircuitBoxServerCreateWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B28 RID: 6952 RVA: 0x000CBA6D File Offset: 0x000C9C6D
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<CircuitBoxClientAddWireEvent>.Default.GetHashCode(this.<Request>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<WireId>k__BackingField)) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<BackingItemId>k__BackingField);
		}

		// Token: 0x06001B29 RID: 6953 RVA: 0x000CBAAD File Offset: 0x000C9CAD
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerCreateWireEvent && this.Equals((CircuitBoxServerCreateWireEvent)obj);
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x000CBAC8 File Offset: 0x000C9CC8
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerCreateWireEvent other)
		{
			return EqualityComparer<CircuitBoxClientAddWireEvent>.Default.Equals(this.<Request>k__BackingField, other.<Request>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<WireId>k__BackingField, other.<WireId>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<BackingItemId>k__BackingField, other.<BackingItemId>k__BackingField);
		}

		// Token: 0x06001B2B RID: 6955 RVA: 0x000CBB1D File Offset: 0x000C9D1D
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxClientAddWireEvent Request, out ushort WireId, out Option<ushort> BackingItemId)
		{
			Request = this.Request;
			WireId = this.WireId;
			BackingItemId = this.BackingItemId;
		}
	}
}
