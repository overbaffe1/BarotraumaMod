using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000113 RID: 275
	[NetworkSerialize(136)]
	internal readonly struct CircuitBoxServerUpdateSelection : INetSerializableStruct, IEquatable<CircuitBoxServerUpdateSelection>
	{
		// Token: 0x06001AD2 RID: 6866 RVA: 0x000CAF31 File Offset: 0x000C9131
		public CircuitBoxServerUpdateSelection(ImmutableArray<CircuitBoxIdSelectionPair> ComponentIds, ImmutableArray<CircuitBoxIdSelectionPair> WireIds, ImmutableArray<CircuitBoxTypeSelectionPair> InputOutputs, ImmutableArray<CircuitBoxIdSelectionPair> LabelIds)
		{
			this.ComponentIds = ComponentIds;
			this.WireIds = WireIds;
			this.InputOutputs = InputOutputs;
			this.LabelIds = LabelIds;
		}

		// Token: 0x170007F5 RID: 2037
		// (get) Token: 0x06001AD3 RID: 6867 RVA: 0x000CAF50 File Offset: 0x000C9150
		// (set) Token: 0x06001AD4 RID: 6868 RVA: 0x000CAF58 File Offset: 0x000C9158
		public ImmutableArray<CircuitBoxIdSelectionPair> ComponentIds { get; set; }

		// Token: 0x170007F6 RID: 2038
		// (get) Token: 0x06001AD5 RID: 6869 RVA: 0x000CAF61 File Offset: 0x000C9161
		// (set) Token: 0x06001AD6 RID: 6870 RVA: 0x000CAF69 File Offset: 0x000C9169
		public ImmutableArray<CircuitBoxIdSelectionPair> WireIds { get; set; }

		// Token: 0x170007F7 RID: 2039
		// (get) Token: 0x06001AD7 RID: 6871 RVA: 0x000CAF72 File Offset: 0x000C9172
		// (set) Token: 0x06001AD8 RID: 6872 RVA: 0x000CAF7A File Offset: 0x000C917A
		public ImmutableArray<CircuitBoxTypeSelectionPair> InputOutputs { get; set; }

		// Token: 0x170007F8 RID: 2040
		// (get) Token: 0x06001AD9 RID: 6873 RVA: 0x000CAF83 File Offset: 0x000C9183
		// (set) Token: 0x06001ADA RID: 6874 RVA: 0x000CAF8B File Offset: 0x000C918B
		public ImmutableArray<CircuitBoxIdSelectionPair> LabelIds { get; set; }

		// Token: 0x06001ADB RID: 6875 RVA: 0x000CAF94 File Offset: 0x000C9194
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxServerUpdateSelection");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001ADC RID: 6876 RVA: 0x000CAFE0 File Offset: 0x000C91E0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ComponentIds = ");
			builder.Append(this.ComponentIds.ToString());
			builder.Append(", WireIds = ");
			builder.Append(this.WireIds.ToString());
			builder.Append(", InputOutputs = ");
			builder.Append(this.InputOutputs.ToString());
			builder.Append(", LabelIds = ");
			builder.Append(this.LabelIds.ToString());
			return true;
		}

		// Token: 0x06001ADD RID: 6877 RVA: 0x000CB08A File Offset: 0x000C928A
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerUpdateSelection left, CircuitBoxServerUpdateSelection right)
		{
			return !(left == right);
		}

		// Token: 0x06001ADE RID: 6878 RVA: 0x000CB096 File Offset: 0x000C9296
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerUpdateSelection left, CircuitBoxServerUpdateSelection right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001ADF RID: 6879 RVA: 0x000CB0A0 File Offset: 0x000C92A0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<ComponentIds>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<WireIds>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxTypeSelectionPair>>.Default.GetHashCode(this.<InputOutputs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<LabelIds>k__BackingField);
		}

		// Token: 0x06001AE0 RID: 6880 RVA: 0x000CB102 File Offset: 0x000C9302
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerUpdateSelection && this.Equals((CircuitBoxServerUpdateSelection)obj);
		}

		// Token: 0x06001AE1 RID: 6881 RVA: 0x000CB11C File Offset: 0x000C931C
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerUpdateSelection other)
		{
			return EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<ComponentIds>k__BackingField, other.<ComponentIds>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<WireIds>k__BackingField, other.<WireIds>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxTypeSelectionPair>>.Default.Equals(this.<InputOutputs>k__BackingField, other.<InputOutputs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<LabelIds>k__BackingField, other.<LabelIds>k__BackingField);
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x000CB189 File Offset: 0x000C9389
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<CircuitBoxIdSelectionPair> ComponentIds, out ImmutableArray<CircuitBoxIdSelectionPair> WireIds, out ImmutableArray<CircuitBoxTypeSelectionPair> InputOutputs, out ImmutableArray<CircuitBoxIdSelectionPair> LabelIds)
		{
			ComponentIds = this.ComponentIds;
			WireIds = this.WireIds;
			InputOutputs = this.InputOutputs;
			LabelIds = this.LabelIds;
		}
	}
}
