using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200020B RID: 523
	[NetworkSerialize(133)]
	internal readonly struct CircuitBoxSelectNodesEvent : INetSerializableStruct, IEquatable<CircuitBoxSelectNodesEvent>
	{
		// Token: 0x060035AE RID: 13742 RVA: 0x00210E04 File Offset: 0x0020F004
		public CircuitBoxSelectNodesEvent(ImmutableArray<ushort> TargetIDs, ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, ImmutableArray<ushort> LabelIDs, bool Overwrite, ushort CharacterID)
		{
			this.TargetIDs = TargetIDs;
			this.IOs = IOs;
			this.LabelIDs = LabelIDs;
			this.Overwrite = Overwrite;
			this.CharacterID = CharacterID;
		}

		// Token: 0x17000E54 RID: 3668
		// (get) Token: 0x060035AF RID: 13743 RVA: 0x00210E2B File Offset: 0x0020F02B
		// (set) Token: 0x060035B0 RID: 13744 RVA: 0x00210E33 File Offset: 0x0020F033
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x17000E55 RID: 3669
		// (get) Token: 0x060035B1 RID: 13745 RVA: 0x00210E3C File Offset: 0x0020F03C
		// (set) Token: 0x060035B2 RID: 13746 RVA: 0x00210E44 File Offset: 0x0020F044
		public ImmutableArray<CircuitBoxInputOutputNode.Type> IOs { get; set; }

		// Token: 0x17000E56 RID: 3670
		// (get) Token: 0x060035B3 RID: 13747 RVA: 0x00210E4D File Offset: 0x0020F04D
		// (set) Token: 0x060035B4 RID: 13748 RVA: 0x00210E55 File Offset: 0x0020F055
		public ImmutableArray<ushort> LabelIDs { get; set; }

		// Token: 0x17000E57 RID: 3671
		// (get) Token: 0x060035B5 RID: 13749 RVA: 0x00210E5E File Offset: 0x0020F05E
		// (set) Token: 0x060035B6 RID: 13750 RVA: 0x00210E66 File Offset: 0x0020F066
		public bool Overwrite { get; set; }

		// Token: 0x17000E58 RID: 3672
		// (get) Token: 0x060035B7 RID: 13751 RVA: 0x00210E6F File Offset: 0x0020F06F
		// (set) Token: 0x060035B8 RID: 13752 RVA: 0x00210E77 File Offset: 0x0020F077
		public ushort CharacterID { get; set; }

		// Token: 0x060035B9 RID: 13753 RVA: 0x00210E80 File Offset: 0x0020F080
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

		// Token: 0x060035BA RID: 13754 RVA: 0x00210ECC File Offset: 0x0020F0CC
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

		// Token: 0x060035BB RID: 13755 RVA: 0x00210F9D File Offset: 0x0020F19D
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxSelectNodesEvent left, CircuitBoxSelectNodesEvent right)
		{
			return !(left == right);
		}

		// Token: 0x060035BC RID: 13756 RVA: 0x00210FA9 File Offset: 0x0020F1A9
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxSelectNodesEvent left, CircuitBoxSelectNodesEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035BD RID: 13757 RVA: 0x00210FB4 File Offset: 0x0020F1B4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.GetHashCode(this.<IOs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<LabelIDs>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Overwrite>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x060035BE RID: 13758 RVA: 0x0021102D File Offset: 0x0020F22D
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxSelectNodesEvent && this.Equals((CircuitBoxSelectNodesEvent)obj);
		}

		// Token: 0x060035BF RID: 13759 RVA: 0x00211048 File Offset: 0x0020F248
		[CompilerGenerated]
		public bool Equals(CircuitBoxSelectNodesEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.Equals(this.<IOs>k__BackingField, other.<IOs>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<LabelIDs>k__BackingField, other.<LabelIDs>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Overwrite>k__BackingField, other.<Overwrite>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x060035C0 RID: 13760 RVA: 0x002110CD File Offset: 0x0020F2CD
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
