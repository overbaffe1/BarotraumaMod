using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200020C RID: 524
	[NetworkSerialize(136)]
	internal readonly struct CircuitBoxServerUpdateSelection : INetSerializableStruct, IEquatable<CircuitBoxServerUpdateSelection>
	{
		// Token: 0x060035C1 RID: 13761 RVA: 0x00211105 File Offset: 0x0020F305
		public CircuitBoxServerUpdateSelection(ImmutableArray<CircuitBoxIdSelectionPair> ComponentIds, ImmutableArray<CircuitBoxIdSelectionPair> WireIds, ImmutableArray<CircuitBoxTypeSelectionPair> InputOutputs, ImmutableArray<CircuitBoxIdSelectionPair> LabelIds)
		{
			this.ComponentIds = ComponentIds;
			this.WireIds = WireIds;
			this.InputOutputs = InputOutputs;
			this.LabelIds = LabelIds;
		}

		// Token: 0x17000E59 RID: 3673
		// (get) Token: 0x060035C2 RID: 13762 RVA: 0x00211124 File Offset: 0x0020F324
		// (set) Token: 0x060035C3 RID: 13763 RVA: 0x0021112C File Offset: 0x0020F32C
		public ImmutableArray<CircuitBoxIdSelectionPair> ComponentIds { get; set; }

		// Token: 0x17000E5A RID: 3674
		// (get) Token: 0x060035C4 RID: 13764 RVA: 0x00211135 File Offset: 0x0020F335
		// (set) Token: 0x060035C5 RID: 13765 RVA: 0x0021113D File Offset: 0x0020F33D
		public ImmutableArray<CircuitBoxIdSelectionPair> WireIds { get; set; }

		// Token: 0x17000E5B RID: 3675
		// (get) Token: 0x060035C6 RID: 13766 RVA: 0x00211146 File Offset: 0x0020F346
		// (set) Token: 0x060035C7 RID: 13767 RVA: 0x0021114E File Offset: 0x0020F34E
		public ImmutableArray<CircuitBoxTypeSelectionPair> InputOutputs { get; set; }

		// Token: 0x17000E5C RID: 3676
		// (get) Token: 0x060035C8 RID: 13768 RVA: 0x00211157 File Offset: 0x0020F357
		// (set) Token: 0x060035C9 RID: 13769 RVA: 0x0021115F File Offset: 0x0020F35F
		public ImmutableArray<CircuitBoxIdSelectionPair> LabelIds { get; set; }

		// Token: 0x060035CA RID: 13770 RVA: 0x00211168 File Offset: 0x0020F368
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

		// Token: 0x060035CB RID: 13771 RVA: 0x002111B4 File Offset: 0x0020F3B4
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

		// Token: 0x060035CC RID: 13772 RVA: 0x0021125E File Offset: 0x0020F45E
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerUpdateSelection left, CircuitBoxServerUpdateSelection right)
		{
			return !(left == right);
		}

		// Token: 0x060035CD RID: 13773 RVA: 0x0021126A File Offset: 0x0020F46A
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerUpdateSelection left, CircuitBoxServerUpdateSelection right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035CE RID: 13774 RVA: 0x00211274 File Offset: 0x0020F474
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<ComponentIds>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<WireIds>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxTypeSelectionPair>>.Default.GetHashCode(this.<InputOutputs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.GetHashCode(this.<LabelIds>k__BackingField);
		}

		// Token: 0x060035CF RID: 13775 RVA: 0x002112D6 File Offset: 0x0020F4D6
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerUpdateSelection && this.Equals((CircuitBoxServerUpdateSelection)obj);
		}

		// Token: 0x060035D0 RID: 13776 RVA: 0x002112F0 File Offset: 0x0020F4F0
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerUpdateSelection other)
		{
			return EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<ComponentIds>k__BackingField, other.<ComponentIds>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<WireIds>k__BackingField, other.<WireIds>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxTypeSelectionPair>>.Default.Equals(this.<InputOutputs>k__BackingField, other.<InputOutputs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxIdSelectionPair>>.Default.Equals(this.<LabelIds>k__BackingField, other.<LabelIds>k__BackingField);
		}

		// Token: 0x060035D1 RID: 13777 RVA: 0x0021135D File Offset: 0x0020F55D
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
