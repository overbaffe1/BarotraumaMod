using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000206 RID: 518
	[NetworkSerialize(118)]
	internal readonly struct CircuitBoxRemoveLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveLabelEvent>
	{
		// Token: 0x06003569 RID: 13673 RVA: 0x00210573 File Offset: 0x0020E773
		public CircuitBoxRemoveLabelEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x17000E48 RID: 3656
		// (get) Token: 0x0600356A RID: 13674 RVA: 0x0021057C File Offset: 0x0020E77C
		// (set) Token: 0x0600356B RID: 13675 RVA: 0x00210584 File Offset: 0x0020E784
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x0600356C RID: 13676 RVA: 0x00210590 File Offset: 0x0020E790
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

		// Token: 0x0600356D RID: 13677 RVA: 0x002105DC File Offset: 0x0020E7DC
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x0600356E RID: 13678 RVA: 0x00210611 File Offset: 0x0020E811
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveLabelEvent left, CircuitBoxRemoveLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600356F RID: 13679 RVA: 0x0021061D File Offset: 0x0020E81D
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveLabelEvent left, CircuitBoxRemoveLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003570 RID: 13680 RVA: 0x00210627 File Offset: 0x0020E827
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x06003571 RID: 13681 RVA: 0x00210639 File Offset: 0x0020E839
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveLabelEvent && this.Equals((CircuitBoxRemoveLabelEvent)obj);
		}

		// Token: 0x06003572 RID: 13682 RVA: 0x00210651 File Offset: 0x0020E851
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveLabelEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x06003573 RID: 13683 RVA: 0x00210669 File Offset: 0x0020E869
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
