using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000216 RID: 534
	[NetworkSerialize(167)]
	internal readonly struct CircuitBoxInitializeStateFromServerEvent : INetSerializableStruct, IEquatable<CircuitBoxInitializeStateFromServerEvent>
	{
		// Token: 0x0600364F RID: 13903 RVA: 0x00212302 File Offset: 0x00210502
		public CircuitBoxInitializeStateFromServerEvent(ImmutableArray<CircuitBoxServerCreateComponentEvent> Components, ImmutableArray<CircuitBoxServerCreateWireEvent> Wires, ImmutableArray<CircuitBoxServerAddLabelEvent> Labels, ImmutableArray<CircuitBoxRenameConnectionLabelsEvent> LabelOverrides, Vector2 InputPos, Vector2 OutputPos)
		{
			this.Components = Components;
			this.Wires = Wires;
			this.Labels = Labels;
			this.LabelOverrides = LabelOverrides;
			this.InputPos = InputPos;
			this.OutputPos = OutputPos;
		}

		// Token: 0x17000E73 RID: 3699
		// (get) Token: 0x06003650 RID: 13904 RVA: 0x00212331 File Offset: 0x00210531
		// (set) Token: 0x06003651 RID: 13905 RVA: 0x00212339 File Offset: 0x00210539
		public ImmutableArray<CircuitBoxServerCreateComponentEvent> Components { get; set; }

		// Token: 0x17000E74 RID: 3700
		// (get) Token: 0x06003652 RID: 13906 RVA: 0x00212342 File Offset: 0x00210542
		// (set) Token: 0x06003653 RID: 13907 RVA: 0x0021234A File Offset: 0x0021054A
		public ImmutableArray<CircuitBoxServerCreateWireEvent> Wires { get; set; }

		// Token: 0x17000E75 RID: 3701
		// (get) Token: 0x06003654 RID: 13908 RVA: 0x00212353 File Offset: 0x00210553
		// (set) Token: 0x06003655 RID: 13909 RVA: 0x0021235B File Offset: 0x0021055B
		public ImmutableArray<CircuitBoxServerAddLabelEvent> Labels { get; set; }

		// Token: 0x17000E76 RID: 3702
		// (get) Token: 0x06003656 RID: 13910 RVA: 0x00212364 File Offset: 0x00210564
		// (set) Token: 0x06003657 RID: 13911 RVA: 0x0021236C File Offset: 0x0021056C
		public ImmutableArray<CircuitBoxRenameConnectionLabelsEvent> LabelOverrides { get; set; }

		// Token: 0x17000E77 RID: 3703
		// (get) Token: 0x06003658 RID: 13912 RVA: 0x00212375 File Offset: 0x00210575
		// (set) Token: 0x06003659 RID: 13913 RVA: 0x0021237D File Offset: 0x0021057D
		public Vector2 InputPos { get; set; }

		// Token: 0x17000E78 RID: 3704
		// (get) Token: 0x0600365A RID: 13914 RVA: 0x00212386 File Offset: 0x00210586
		// (set) Token: 0x0600365B RID: 13915 RVA: 0x0021238E File Offset: 0x0021058E
		public Vector2 OutputPos { get; set; }

		// Token: 0x0600365C RID: 13916 RVA: 0x00212398 File Offset: 0x00210598
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxInitializeStateFromServerEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600365D RID: 13917 RVA: 0x002123E4 File Offset: 0x002105E4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Components = ");
			builder.Append(this.Components.ToString());
			builder.Append(", Wires = ");
			builder.Append(this.Wires.ToString());
			builder.Append(", Labels = ");
			builder.Append(this.Labels.ToString());
			builder.Append(", LabelOverrides = ");
			builder.Append(this.LabelOverrides.ToString());
			builder.Append(", InputPos = ");
			builder.Append(this.InputPos.ToString());
			builder.Append(", OutputPos = ");
			builder.Append(this.OutputPos.ToString());
			return true;
		}

		// Token: 0x0600365E RID: 13918 RVA: 0x002124DE File Offset: 0x002106DE
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxInitializeStateFromServerEvent left, CircuitBoxInitializeStateFromServerEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600365F RID: 13919 RVA: 0x002124EA File Offset: 0x002106EA
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxInitializeStateFromServerEvent left, CircuitBoxInitializeStateFromServerEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003660 RID: 13920 RVA: 0x002124F4 File Offset: 0x002106F4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((EqualityComparer<ImmutableArray<CircuitBoxServerCreateComponentEvent>>.Default.GetHashCode(this.<Components>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxServerCreateWireEvent>>.Default.GetHashCode(this.<Wires>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxServerAddLabelEvent>>.Default.GetHashCode(this.<Labels>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxRenameConnectionLabelsEvent>>.Default.GetHashCode(this.<LabelOverrides>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<InputPos>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<OutputPos>k__BackingField);
		}

		// Token: 0x06003661 RID: 13921 RVA: 0x00212584 File Offset: 0x00210784
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxInitializeStateFromServerEvent && this.Equals((CircuitBoxInitializeStateFromServerEvent)obj);
		}

		// Token: 0x06003662 RID: 13922 RVA: 0x0021259C File Offset: 0x0021079C
		[CompilerGenerated]
		public bool Equals(CircuitBoxInitializeStateFromServerEvent other)
		{
			return EqualityComparer<ImmutableArray<CircuitBoxServerCreateComponentEvent>>.Default.Equals(this.<Components>k__BackingField, other.<Components>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxServerCreateWireEvent>>.Default.Equals(this.<Wires>k__BackingField, other.<Wires>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxServerAddLabelEvent>>.Default.Equals(this.<Labels>k__BackingField, other.<Labels>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxRenameConnectionLabelsEvent>>.Default.Equals(this.<LabelOverrides>k__BackingField, other.<LabelOverrides>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<InputPos>k__BackingField, other.<InputPos>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<OutputPos>k__BackingField, other.<OutputPos>k__BackingField);
		}

		// Token: 0x06003663 RID: 13923 RVA: 0x0021263C File Offset: 0x0021083C
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<CircuitBoxServerCreateComponentEvent> Components, out ImmutableArray<CircuitBoxServerCreateWireEvent> Wires, out ImmutableArray<CircuitBoxServerAddLabelEvent> Labels, out ImmutableArray<CircuitBoxRenameConnectionLabelsEvent> LabelOverrides, out Vector2 InputPos, out Vector2 OutputPos)
		{
			Components = this.Components;
			Wires = this.Wires;
			Labels = this.Labels;
			LabelOverrides = this.LabelOverrides;
			InputPos = this.InputPos;
			OutputPos = this.OutputPos;
		}
	}
}
