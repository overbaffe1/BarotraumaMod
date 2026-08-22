using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000110 RID: 272
	[NetworkSerialize(127)]
	internal readonly struct CircuitBoxRemoveComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveComponentEvent>
	{
		// Token: 0x06001AA3 RID: 6819 RVA: 0x000CA8A0 File Offset: 0x000C8AA0
		public CircuitBoxRemoveComponentEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x170007EB RID: 2027
		// (get) Token: 0x06001AA4 RID: 6820 RVA: 0x000CA8A9 File Offset: 0x000C8AA9
		// (set) Token: 0x06001AA5 RID: 6821 RVA: 0x000CA8B1 File Offset: 0x000C8AB1
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x06001AA6 RID: 6822 RVA: 0x000CA8BC File Offset: 0x000C8ABC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRemoveComponentEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001AA7 RID: 6823 RVA: 0x000CA908 File Offset: 0x000C8B08
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x06001AA8 RID: 6824 RVA: 0x000CA93D File Offset: 0x000C8B3D
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveComponentEvent left, CircuitBoxRemoveComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001AA9 RID: 6825 RVA: 0x000CA949 File Offset: 0x000C8B49
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveComponentEvent left, CircuitBoxRemoveComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001AAA RID: 6826 RVA: 0x000CA953 File Offset: 0x000C8B53
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001AAB RID: 6827 RVA: 0x000CA965 File Offset: 0x000C8B65
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveComponentEvent && this.Equals((CircuitBoxRemoveComponentEvent)obj);
		}

		// Token: 0x06001AAC RID: 6828 RVA: 0x000CA97D File Offset: 0x000C8B7D
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveComponentEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001AAD RID: 6829 RVA: 0x000CA995 File Offset: 0x000C8B95
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
