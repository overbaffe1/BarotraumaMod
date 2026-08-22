using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200020D RID: 525
	[NetworkSerialize(139)]
	internal readonly struct CircuitBoxIdSelectionPair : INetSerializableStruct, IEquatable<CircuitBoxIdSelectionPair>
	{
		// Token: 0x060035D2 RID: 13778 RVA: 0x00211390 File Offset: 0x0020F590
		public CircuitBoxIdSelectionPair(ushort ID, Option<ushort> SelectedBy)
		{
			this.ID = ID;
			this.SelectedBy = SelectedBy;
		}

		// Token: 0x17000E5D RID: 3677
		// (get) Token: 0x060035D3 RID: 13779 RVA: 0x002113A0 File Offset: 0x0020F5A0
		// (set) Token: 0x060035D4 RID: 13780 RVA: 0x002113A8 File Offset: 0x0020F5A8
		public ushort ID { get; set; }

		// Token: 0x17000E5E RID: 3678
		// (get) Token: 0x060035D5 RID: 13781 RVA: 0x002113B1 File Offset: 0x0020F5B1
		// (set) Token: 0x060035D6 RID: 13782 RVA: 0x002113B9 File Offset: 0x0020F5B9
		public Option<ushort> SelectedBy { get; set; }

		// Token: 0x060035D7 RID: 13783 RVA: 0x002113C4 File Offset: 0x0020F5C4
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxIdSelectionPair");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060035D8 RID: 13784 RVA: 0x00211410 File Offset: 0x0020F610
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ID = ");
			builder.Append(this.ID.ToString());
			builder.Append(", SelectedBy = ");
			builder.Append(this.SelectedBy.ToString());
			return true;
		}

		// Token: 0x060035D9 RID: 13785 RVA: 0x0021146C File Offset: 0x0020F66C
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxIdSelectionPair left, CircuitBoxIdSelectionPair right)
		{
			return !(left == right);
		}

		// Token: 0x060035DA RID: 13786 RVA: 0x00211478 File Offset: 0x0020F678
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxIdSelectionPair left, CircuitBoxIdSelectionPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035DB RID: 13787 RVA: 0x00211482 File Offset: 0x0020F682
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<SelectedBy>k__BackingField);
		}

		// Token: 0x060035DC RID: 13788 RVA: 0x002114AB File Offset: 0x0020F6AB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxIdSelectionPair && this.Equals((CircuitBoxIdSelectionPair)obj);
		}

		// Token: 0x060035DD RID: 13789 RVA: 0x002114C3 File Offset: 0x0020F6C3
		[CompilerGenerated]
		public bool Equals(CircuitBoxIdSelectionPair other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<SelectedBy>k__BackingField, other.<SelectedBy>k__BackingField);
		}

		// Token: 0x060035DE RID: 13790 RVA: 0x002114F5 File Offset: 0x0020F6F5
		[CompilerGenerated]
		public void Deconstruct(out ushort ID, out Option<ushort> SelectedBy)
		{
			ID = this.ID;
			SelectedBy = this.SelectedBy;
		}
	}
}
