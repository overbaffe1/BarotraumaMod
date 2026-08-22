using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000114 RID: 276
	[NetworkSerialize(139)]
	internal readonly struct CircuitBoxIdSelectionPair : INetSerializableStruct, IEquatable<CircuitBoxIdSelectionPair>
	{
		// Token: 0x06001AE3 RID: 6883 RVA: 0x000CB1BC File Offset: 0x000C93BC
		public CircuitBoxIdSelectionPair(ushort ID, Option<ushort> SelectedBy)
		{
			this.ID = ID;
			this.SelectedBy = SelectedBy;
		}

		// Token: 0x170007F9 RID: 2041
		// (get) Token: 0x06001AE4 RID: 6884 RVA: 0x000CB1CC File Offset: 0x000C93CC
		// (set) Token: 0x06001AE5 RID: 6885 RVA: 0x000CB1D4 File Offset: 0x000C93D4
		public ushort ID { get; set; }

		// Token: 0x170007FA RID: 2042
		// (get) Token: 0x06001AE6 RID: 6886 RVA: 0x000CB1DD File Offset: 0x000C93DD
		// (set) Token: 0x06001AE7 RID: 6887 RVA: 0x000CB1E5 File Offset: 0x000C93E5
		public Option<ushort> SelectedBy { get; set; }

		// Token: 0x06001AE8 RID: 6888 RVA: 0x000CB1F0 File Offset: 0x000C93F0
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

		// Token: 0x06001AE9 RID: 6889 RVA: 0x000CB23C File Offset: 0x000C943C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ID = ");
			builder.Append(this.ID.ToString());
			builder.Append(", SelectedBy = ");
			builder.Append(this.SelectedBy.ToString());
			return true;
		}

		// Token: 0x06001AEA RID: 6890 RVA: 0x000CB298 File Offset: 0x000C9498
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxIdSelectionPair left, CircuitBoxIdSelectionPair right)
		{
			return !(left == right);
		}

		// Token: 0x06001AEB RID: 6891 RVA: 0x000CB2A4 File Offset: 0x000C94A4
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxIdSelectionPair left, CircuitBoxIdSelectionPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001AEC RID: 6892 RVA: 0x000CB2AE File Offset: 0x000C94AE
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<SelectedBy>k__BackingField);
		}

		// Token: 0x06001AED RID: 6893 RVA: 0x000CB2D7 File Offset: 0x000C94D7
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxIdSelectionPair && this.Equals((CircuitBoxIdSelectionPair)obj);
		}

		// Token: 0x06001AEE RID: 6894 RVA: 0x000CB2EF File Offset: 0x000C94EF
		[CompilerGenerated]
		public bool Equals(CircuitBoxIdSelectionPair other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<SelectedBy>k__BackingField, other.<SelectedBy>k__BackingField);
		}

		// Token: 0x06001AEF RID: 6895 RVA: 0x000CB321 File Offset: 0x000C9521
		[CompilerGenerated]
		public void Deconstruct(out ushort ID, out Option<ushort> SelectedBy)
		{
			ID = this.ID;
			SelectedBy = this.SelectedBy;
		}
	}
}
