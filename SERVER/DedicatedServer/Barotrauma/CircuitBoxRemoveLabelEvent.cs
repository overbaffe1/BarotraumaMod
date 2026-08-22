using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200010D RID: 269
	[NetworkSerialize(118)]
	internal readonly struct CircuitBoxRemoveLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveLabelEvent>
	{
		// Token: 0x06001A7A RID: 6778 RVA: 0x000CA39F File Offset: 0x000C859F
		public CircuitBoxRemoveLabelEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x170007E4 RID: 2020
		// (get) Token: 0x06001A7B RID: 6779 RVA: 0x000CA3A8 File Offset: 0x000C85A8
		// (set) Token: 0x06001A7C RID: 6780 RVA: 0x000CA3B0 File Offset: 0x000C85B0
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x06001A7D RID: 6781 RVA: 0x000CA3BC File Offset: 0x000C85BC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRemoveLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001A7E RID: 6782 RVA: 0x000CA408 File Offset: 0x000C8608
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x06001A7F RID: 6783 RVA: 0x000CA43D File Offset: 0x000C863D
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveLabelEvent left, CircuitBoxRemoveLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A80 RID: 6784 RVA: 0x000CA449 File Offset: 0x000C8649
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveLabelEvent left, CircuitBoxRemoveLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A81 RID: 6785 RVA: 0x000CA453 File Offset: 0x000C8653
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001A82 RID: 6786 RVA: 0x000CA465 File Offset: 0x000C8665
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveLabelEvent && this.Equals((CircuitBoxRemoveLabelEvent)obj);
		}

		// Token: 0x06001A83 RID: 6787 RVA: 0x000CA47D File Offset: 0x000C867D
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveLabelEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x06001A84 RID: 6788 RVA: 0x000CA495 File Offset: 0x000C8695
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
