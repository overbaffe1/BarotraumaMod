using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000211 RID: 529
	[NetworkSerialize(151)]
	internal readonly struct CircuitBoxServerCreateWireEvent : INetSerializableStruct, IEquatable<CircuitBoxServerCreateWireEvent>
	{
		// Token: 0x0600360C RID: 13836 RVA: 0x00211B10 File Offset: 0x0020FD10
		public CircuitBoxServerCreateWireEvent(CircuitBoxClientAddWireEvent Request, ushort WireId, Option<ushort> BackingItemId)
		{
			this.Request = Request;
			this.WireId = WireId;
			this.BackingItemId = BackingItemId;
		}

		// Token: 0x17000E68 RID: 3688
		// (get) Token: 0x0600360D RID: 13837 RVA: 0x00211B27 File Offset: 0x0020FD27
		// (set) Token: 0x0600360E RID: 13838 RVA: 0x00211B2F File Offset: 0x0020FD2F
		public CircuitBoxClientAddWireEvent Request { get; set; }

		// Token: 0x17000E69 RID: 3689
		// (get) Token: 0x0600360F RID: 13839 RVA: 0x00211B38 File Offset: 0x0020FD38
		// (set) Token: 0x06003610 RID: 13840 RVA: 0x00211B40 File Offset: 0x0020FD40
		public ushort WireId { get; set; }

		// Token: 0x17000E6A RID: 3690
		// (get) Token: 0x06003611 RID: 13841 RVA: 0x00211B49 File Offset: 0x0020FD49
		// (set) Token: 0x06003612 RID: 13842 RVA: 0x00211B51 File Offset: 0x0020FD51
		public Option<ushort> BackingItemId { get; set; }

		// Token: 0x06003613 RID: 13843 RVA: 0x00211B5C File Offset: 0x0020FD5C
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

		// Token: 0x06003614 RID: 13844 RVA: 0x00211BA8 File Offset: 0x0020FDA8
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

		// Token: 0x06003615 RID: 13845 RVA: 0x00211C2B File Offset: 0x0020FE2B
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerCreateWireEvent left, CircuitBoxServerCreateWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003616 RID: 13846 RVA: 0x00211C37 File Offset: 0x0020FE37
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerCreateWireEvent left, CircuitBoxServerCreateWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003617 RID: 13847 RVA: 0x00211C41 File Offset: 0x0020FE41
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<CircuitBoxClientAddWireEvent>.Default.GetHashCode(this.<Request>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<WireId>k__BackingField)) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<BackingItemId>k__BackingField);
		}

		// Token: 0x06003618 RID: 13848 RVA: 0x00211C81 File Offset: 0x0020FE81
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerCreateWireEvent && this.Equals((CircuitBoxServerCreateWireEvent)obj);
		}

		// Token: 0x06003619 RID: 13849 RVA: 0x00211C9C File Offset: 0x0020FE9C
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerCreateWireEvent other)
		{
			return EqualityComparer<CircuitBoxClientAddWireEvent>.Default.Equals(this.<Request>k__BackingField, other.<Request>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<WireId>k__BackingField, other.<WireId>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<BackingItemId>k__BackingField, other.<BackingItemId>k__BackingField);
		}

		// Token: 0x0600361A RID: 13850 RVA: 0x00211CF1 File Offset: 0x0020FEF1
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxClientAddWireEvent Request, out ushort WireId, out Option<ushort> BackingItemId)
		{
			Request = this.Request;
			WireId = this.WireId;
			BackingItemId = this.BackingItemId;
		}
	}
}
