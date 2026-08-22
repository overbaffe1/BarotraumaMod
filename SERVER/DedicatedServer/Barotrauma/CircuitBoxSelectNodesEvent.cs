using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000112 RID: 274
	[NetworkSerialize(133)]
	internal readonly struct CircuitBoxSelectNodesEvent : INetSerializableStruct, IEquatable<CircuitBoxSelectNodesEvent>
	{
		// Token: 0x06001ABF RID: 6847 RVA: 0x000CAC30 File Offset: 0x000C8E30
		public CircuitBoxSelectNodesEvent(ImmutableArray<ushort> TargetIDs, ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, ImmutableArray<ushort> LabelIDs, bool Overwrite, ushort CharacterID)
		{
			this.TargetIDs = TargetIDs;
			this.IOs = IOs;
			this.LabelIDs = LabelIDs;
			this.Overwrite = Overwrite;
			this.CharacterID = CharacterID;
		}

		// Token: 0x170007F0 RID: 2032
		// (get) Token: 0x06001AC0 RID: 6848 RVA: 0x000CAC57 File Offset: 0x000C8E57
		// (set) Token: 0x06001AC1 RID: 6849 RVA: 0x000CAC5F File Offset: 0x000C8E5F
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x170007F1 RID: 2033
		// (get) Token: 0x06001AC2 RID: 6850 RVA: 0x000CAC68 File Offset: 0x000C8E68
		// (set) Token: 0x06001AC3 RID: 6851 RVA: 0x000CAC70 File Offset: 0x000C8E70
		public ImmutableArray<CircuitBoxInputOutputNode.Type> IOs { get; set; }

		// Token: 0x170007F2 RID: 2034
		// (get) Token: 0x06001AC4 RID: 6852 RVA: 0x000CAC79 File Offset: 0x000C8E79
		// (set) Token: 0x06001AC5 RID: 6853 RVA: 0x000CAC81 File Offset: 0x000C8E81
		public ImmutableArray<ushort> LabelIDs { get; set; }

		// Token: 0x170007F3 RID: 2035
		// (get) Token: 0x06001AC6 RID: 6854 RVA: 0x000CAC8A File Offset: 0x000C8E8A
		// (set) Token: 0x06001AC7 RID: 6855 RVA: 0x000CAC92 File Offset: 0x000C8E92
		public bool Overwrite { get; set; }

		// Token: 0x170007F4 RID: 2036
		// (get) Token: 0x06001AC8 RID: 6856 RVA: 0x000CAC9B File Offset: 0x000C8E9B
		// (set) Token: 0x06001AC9 RID: 6857 RVA: 0x000CACA3 File Offset: 0x000C8EA3
		public ushort CharacterID { get; set; }

		// Token: 0x06001ACA RID: 6858 RVA: 0x000CACAC File Offset: 0x000C8EAC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxSelectNodesEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001ACB RID: 6859 RVA: 0x000CACF8 File Offset: 0x000C8EF8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			builder.Append(", IOs = ");
			builder.Append(this.IOs.ToString());
			builder.Append(", LabelIDs = ");
			builder.Append(this.LabelIDs.ToString());
			builder.Append(", Overwrite = ");
			builder.Append(this.Overwrite.ToString());
			builder.Append(", CharacterID = ");
			builder.Append(this.CharacterID.ToString());
			return true;
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x000CADC9 File Offset: 0x000C8FC9
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxSelectNodesEvent left, CircuitBoxSelectNodesEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x000CADD5 File Offset: 0x000C8FD5
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxSelectNodesEvent left, CircuitBoxSelectNodesEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x000CADE0 File Offset: 0x000C8FE0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.GetHashCode(this.<IOs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<LabelIDs>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Overwrite>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x06001ACF RID: 6863 RVA: 0x000CAE59 File Offset: 0x000C9059
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxSelectNodesEvent && this.Equals((CircuitBoxSelectNodesEvent)obj);
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x000CAE74 File Offset: 0x000C9074
		[CompilerGenerated]
		public bool Equals(CircuitBoxSelectNodesEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.Equals(this.<IOs>k__BackingField, other.<IOs>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<LabelIDs>k__BackingField, other.<LabelIDs>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Overwrite>k__BackingField, other.<Overwrite>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x06001AD1 RID: 6865 RVA: 0x000CAEF9 File Offset: 0x000C90F9
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs, out ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, out ImmutableArray<ushort> LabelIDs, out bool Overwrite, out ushort CharacterID)
		{
			TargetIDs = this.TargetIDs;
			IOs = this.IOs;
			LabelIDs = this.LabelIDs;
			Overwrite = this.Overwrite;
			CharacterID = this.CharacterID;
		}
	}
}
