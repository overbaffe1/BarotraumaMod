using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000119 RID: 281
	[NetworkSerialize(154)]
	internal readonly struct CircuitBoxRemoveWireEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveWireEvent>
	{
		// Token: 0x06001B2C RID: 6956 RVA: 0x000CBB3F File Offset: 0x000C9D3F
		public CircuitBoxRemoveWireEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x17000807 RID: 2055
		// (get) Token: 0x06001B2D RID: 6957 RVA: 0x000CBB48 File Offset: 0x000C9D48
		// (set) Token: 0x06001B2E RID: 6958 RVA: 0x000CBB50 File Offset: 0x000C9D50
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x06001B2F RID: 6959 RVA: 0x000CBB5C File Offset: 0x000C9D5C
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRemoveWireEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B30 RID: 6960 RVA: 0x000CBBA8 File Offset: 0x000C9DA8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x06001B31 RID: 6961 RVA: 0x000CBBDD File Offset: 0x000C9DDD
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveWireEvent left, CircuitBoxRemoveWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B32 RID: 6962 RVA: 0x000CBBE9 File Offset: 0x000C9DE9
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveWireEvent left, CircuitBoxRemoveWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B33 RID: 6963 RVA: 0x000CBBF3 File Offset: 0x000C9DF3
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001B34 RID: 6964 RVA: 0x000CBC05 File Offset: 0x000C9E05
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveWireEvent && this.Equals((CircuitBoxRemoveWireEvent)obj);
		}

		// Token: 0x06001B35 RID: 6965 RVA: 0x000CBC1D File Offset: 0x000C9E1D
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveWireEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001B36 RID: 6966 RVA: 0x000CBC35 File Offset: 0x000C9E35
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
