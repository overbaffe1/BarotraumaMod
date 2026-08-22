using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200020E RID: 526
	[NetworkSerialize(142)]
	internal readonly struct CircuitBoxTypeSelectionPair : INetSerializableStruct, IEquatable<CircuitBoxTypeSelectionPair>
	{
		// Token: 0x060035DF RID: 13791 RVA: 0x0021150B File Offset: 0x0020F70B
		public CircuitBoxTypeSelectionPair(CircuitBoxInputOutputNode.Type Type, Option<ushort> SelectedBy)
		{
			this.Type = Type;
			this.SelectedBy = SelectedBy;
		}

		// Token: 0x17000E5F RID: 3679
		// (get) Token: 0x060035E0 RID: 13792 RVA: 0x0021151B File Offset: 0x0020F71B
		// (set) Token: 0x060035E1 RID: 13793 RVA: 0x00211523 File Offset: 0x0020F723
		public CircuitBoxInputOutputNode.Type Type { get; set; }

		// Token: 0x17000E60 RID: 3680
		// (get) Token: 0x060035E2 RID: 13794 RVA: 0x0021152C File Offset: 0x0020F72C
		// (set) Token: 0x060035E3 RID: 13795 RVA: 0x00211534 File Offset: 0x0020F734
		public Option<ushort> SelectedBy { get; set; }

		// Token: 0x060035E4 RID: 13796 RVA: 0x00211540 File Offset: 0x0020F740
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxTypeSelectionPair");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060035E5 RID: 13797 RVA: 0x0021158C File Offset: 0x0020F78C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Type = ");
			builder.Append(this.Type.ToString());
			builder.Append(", SelectedBy = ");
			builder.Append(this.SelectedBy.ToString());
			return true;
		}

		// Token: 0x060035E6 RID: 13798 RVA: 0x002115E8 File Offset: 0x0020F7E8
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxTypeSelectionPair left, CircuitBoxTypeSelectionPair right)
		{
			return !(left == right);
		}

		// Token: 0x060035E7 RID: 13799 RVA: 0x002115F4 File Offset: 0x0020F7F4
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxTypeSelectionPair left, CircuitBoxTypeSelectionPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035E8 RID: 13800 RVA: 0x002115FE File Offset: 0x0020F7FE
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<SelectedBy>k__BackingField);
		}

		// Token: 0x060035E9 RID: 13801 RVA: 0x00211627 File Offset: 0x0020F827
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxTypeSelectionPair && this.Equals((CircuitBoxTypeSelectionPair)obj);
		}

		// Token: 0x060035EA RID: 13802 RVA: 0x0021163F File Offset: 0x0020F83F
		[CompilerGenerated]
		public bool Equals(CircuitBoxTypeSelectionPair other)
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<SelectedBy>k__BackingField, other.<SelectedBy>k__BackingField);
		}

		// Token: 0x060035EB RID: 13803 RVA: 0x00211671 File Offset: 0x0020F871
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxInputOutputNode.Type Type, out Option<ushort> SelectedBy)
		{
			Type = this.Type;
			SelectedBy = this.SelectedBy;
		}
	}
}
