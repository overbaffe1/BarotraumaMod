using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200020A RID: 522
	[NetworkSerialize(130)]
	internal readonly struct CircuitBoxMoveComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxMoveComponentEvent>
	{
		// Token: 0x0600359D RID: 13725 RVA: 0x00210B77 File Offset: 0x0020ED77
		public CircuitBoxMoveComponentEvent(ImmutableArray<ushort> TargetIDs, ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, ImmutableArray<ushort> LabelIDs, Vector2 MoveAmount)
		{
			this.TargetIDs = TargetIDs;
			this.IOs = IOs;
			this.LabelIDs = LabelIDs;
			this.MoveAmount = MoveAmount;
		}

		// Token: 0x17000E50 RID: 3664
		// (get) Token: 0x0600359E RID: 13726 RVA: 0x00210B96 File Offset: 0x0020ED96
		// (set) Token: 0x0600359F RID: 13727 RVA: 0x00210B9E File Offset: 0x0020ED9E
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x17000E51 RID: 3665
		// (get) Token: 0x060035A0 RID: 13728 RVA: 0x00210BA7 File Offset: 0x0020EDA7
		// (set) Token: 0x060035A1 RID: 13729 RVA: 0x00210BAF File Offset: 0x0020EDAF
		public ImmutableArray<CircuitBoxInputOutputNode.Type> IOs { get; set; }

		// Token: 0x17000E52 RID: 3666
		// (get) Token: 0x060035A2 RID: 13730 RVA: 0x00210BB8 File Offset: 0x0020EDB8
		// (set) Token: 0x060035A3 RID: 13731 RVA: 0x00210BC0 File Offset: 0x0020EDC0
		public ImmutableArray<ushort> LabelIDs { get; set; }

		// Token: 0x17000E53 RID: 3667
		// (get) Token: 0x060035A4 RID: 13732 RVA: 0x00210BC9 File Offset: 0x0020EDC9
		// (set) Token: 0x060035A5 RID: 13733 RVA: 0x00210BD1 File Offset: 0x0020EDD1
		public Vector2 MoveAmount { get; set; }

		// Token: 0x060035A6 RID: 13734 RVA: 0x00210BDC File Offset: 0x0020EDDC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxMoveComponentEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060035A7 RID: 13735 RVA: 0x00210C28 File Offset: 0x0020EE28
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			builder.Append(", IOs = ");
			builder.Append(this.IOs.ToString());
			builder.Append(", LabelIDs = ");
			builder.Append(this.LabelIDs.ToString());
			builder.Append(", MoveAmount = ");
			builder.Append(this.MoveAmount.ToString());
			return true;
		}

		// Token: 0x060035A8 RID: 13736 RVA: 0x00210CD2 File Offset: 0x0020EED2
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxMoveComponentEvent left, CircuitBoxMoveComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x060035A9 RID: 13737 RVA: 0x00210CDE File Offset: 0x0020EEDE
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxMoveComponentEvent left, CircuitBoxMoveComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035AA RID: 13738 RVA: 0x00210CE8 File Offset: 0x0020EEE8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.GetHashCode(this.<IOs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<LabelIDs>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<MoveAmount>k__BackingField);
		}

		// Token: 0x060035AB RID: 13739 RVA: 0x00210D4A File Offset: 0x0020EF4A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxMoveComponentEvent && this.Equals((CircuitBoxMoveComponentEvent)obj);
		}

		// Token: 0x060035AC RID: 13740 RVA: 0x00210D64 File Offset: 0x0020EF64
		[CompilerGenerated]
		public bool Equals(CircuitBoxMoveComponentEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.Equals(this.<IOs>k__BackingField, other.<IOs>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<LabelIDs>k__BackingField, other.<LabelIDs>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<MoveAmount>k__BackingField, other.<MoveAmount>k__BackingField);
		}

		// Token: 0x060035AD RID: 13741 RVA: 0x00210DD1 File Offset: 0x0020EFD1
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs, out ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, out ImmutableArray<ushort> LabelIDs, out Vector2 MoveAmount)
		{
			TargetIDs = this.TargetIDs;
			IOs = this.IOs;
			LabelIDs = this.LabelIDs;
			MoveAmount = this.MoveAmount;
		}
	}
}
