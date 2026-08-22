using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000115 RID: 277
	[NetworkSerialize(142)]
	internal readonly struct CircuitBoxTypeSelectionPair : INetSerializableStruct, IEquatable<CircuitBoxTypeSelectionPair>
	{
		// Token: 0x06001AF0 RID: 6896 RVA: 0x000CB337 File Offset: 0x000C9537
		public CircuitBoxTypeSelectionPair(CircuitBoxInputOutputNode.Type Type, Option<ushort> SelectedBy)
		{
			this.Type = Type;
			this.SelectedBy = SelectedBy;
		}

		// Token: 0x170007FB RID: 2043
		// (get) Token: 0x06001AF1 RID: 6897 RVA: 0x000CB347 File Offset: 0x000C9547
		// (set) Token: 0x06001AF2 RID: 6898 RVA: 0x000CB34F File Offset: 0x000C954F
		public CircuitBoxInputOutputNode.Type Type { get; set; }

		// Token: 0x170007FC RID: 2044
		// (get) Token: 0x06001AF3 RID: 6899 RVA: 0x000CB358 File Offset: 0x000C9558
		// (set) Token: 0x06001AF4 RID: 6900 RVA: 0x000CB360 File Offset: 0x000C9560
		public Option<ushort> SelectedBy { get; set; }

		// Token: 0x06001AF5 RID: 6901 RVA: 0x000CB36C File Offset: 0x000C956C
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

		// Token: 0x06001AF6 RID: 6902 RVA: 0x000CB3B8 File Offset: 0x000C95B8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Type = ");
			builder.Append(this.Type.ToString());
			builder.Append(", SelectedBy = ");
			builder.Append(this.SelectedBy.ToString());
			return true;
		}

		// Token: 0x06001AF7 RID: 6903 RVA: 0x000CB414 File Offset: 0x000C9614
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxTypeSelectionPair left, CircuitBoxTypeSelectionPair right)
		{
			return !(left == right);
		}

		// Token: 0x06001AF8 RID: 6904 RVA: 0x000CB420 File Offset: 0x000C9620
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxTypeSelectionPair left, CircuitBoxTypeSelectionPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001AF9 RID: 6905 RVA: 0x000CB42A File Offset: 0x000C962A
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<SelectedBy>k__BackingField);
		}

		// Token: 0x06001AFA RID: 6906 RVA: 0x000CB453 File Offset: 0x000C9653
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxTypeSelectionPair && this.Equals((CircuitBoxTypeSelectionPair)obj);
		}

		// Token: 0x06001AFB RID: 6907 RVA: 0x000CB46B File Offset: 0x000C966B
		[CompilerGenerated]
		public bool Equals(CircuitBoxTypeSelectionPair other)
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<SelectedBy>k__BackingField, other.<SelectedBy>k__BackingField);
		}

		// Token: 0x06001AFC RID: 6908 RVA: 0x000CB49D File Offset: 0x000C969D
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxInputOutputNode.Type Type, out Option<ushort> SelectedBy)
		{
			Type = this.Type;
			SelectedBy = this.SelectedBy;
		}
	}
}
