using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000111 RID: 273
	[NetworkSerialize(130)]
	internal readonly struct CircuitBoxMoveComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxMoveComponentEvent>
	{
		// Token: 0x06001AAE RID: 6830 RVA: 0x000CA9A3 File Offset: 0x000C8BA3
		public CircuitBoxMoveComponentEvent(ImmutableArray<ushort> TargetIDs, ImmutableArray<CircuitBoxInputOutputNode.Type> IOs, ImmutableArray<ushort> LabelIDs, Vector2 MoveAmount)
		{
			this.TargetIDs = TargetIDs;
			this.IOs = IOs;
			this.LabelIDs = LabelIDs;
			this.MoveAmount = MoveAmount;
		}

		// Token: 0x170007EC RID: 2028
		// (get) Token: 0x06001AAF RID: 6831 RVA: 0x000CA9C2 File Offset: 0x000C8BC2
		// (set) Token: 0x06001AB0 RID: 6832 RVA: 0x000CA9CA File Offset: 0x000C8BCA
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x170007ED RID: 2029
		// (get) Token: 0x06001AB1 RID: 6833 RVA: 0x000CA9D3 File Offset: 0x000C8BD3
		// (set) Token: 0x06001AB2 RID: 6834 RVA: 0x000CA9DB File Offset: 0x000C8BDB
		public ImmutableArray<CircuitBoxInputOutputNode.Type> IOs { get; set; }

		// Token: 0x170007EE RID: 2030
		// (get) Token: 0x06001AB3 RID: 6835 RVA: 0x000CA9E4 File Offset: 0x000C8BE4
		// (set) Token: 0x06001AB4 RID: 6836 RVA: 0x000CA9EC File Offset: 0x000C8BEC
		public ImmutableArray<ushort> LabelIDs { get; set; }

		// Token: 0x170007EF RID: 2031
		// (get) Token: 0x06001AB5 RID: 6837 RVA: 0x000CA9F5 File Offset: 0x000C8BF5
		// (set) Token: 0x06001AB6 RID: 6838 RVA: 0x000CA9FD File Offset: 0x000C8BFD
		public Vector2 MoveAmount { get; set; }

		// Token: 0x06001AB7 RID: 6839 RVA: 0x000CAA08 File Offset: 0x000C8C08
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

		// Token: 0x06001AB8 RID: 6840 RVA: 0x000CAA54 File Offset: 0x000C8C54
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

		// Token: 0x06001AB9 RID: 6841 RVA: 0x000CAAFE File Offset: 0x000C8CFE
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxMoveComponentEvent left, CircuitBoxMoveComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x000CAB0A File Offset: 0x000C8D0A
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxMoveComponentEvent left, CircuitBoxMoveComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x000CAB14 File Offset: 0x000C8D14
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.GetHashCode(this.<IOs>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<LabelIDs>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<MoveAmount>k__BackingField);
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x000CAB76 File Offset: 0x000C8D76
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxMoveComponentEvent && this.Equals((CircuitBoxMoveComponentEvent)obj);
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x000CAB90 File Offset: 0x000C8D90
		[CompilerGenerated]
		public bool Equals(CircuitBoxMoveComponentEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxInputOutputNode.Type>>.Default.Equals(this.<IOs>k__BackingField, other.<IOs>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<LabelIDs>k__BackingField, other.<LabelIDs>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<MoveAmount>k__BackingField, other.<MoveAmount>k__BackingField);
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x000CABFD File Offset: 0x000C8DFD
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
