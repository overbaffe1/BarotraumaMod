using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200011D RID: 285
	[NetworkSerialize(167)]
	internal readonly struct CircuitBoxInitializeStateFromServerEvent : INetSerializableStruct, IEquatable<CircuitBoxInitializeStateFromServerEvent>
	{
		// Token: 0x06001B60 RID: 7008 RVA: 0x000CC12E File Offset: 0x000CA32E
		public CircuitBoxInitializeStateFromServerEvent(ImmutableArray<CircuitBoxServerCreateComponentEvent> Components, ImmutableArray<CircuitBoxServerCreateWireEvent> Wires, ImmutableArray<CircuitBoxServerAddLabelEvent> Labels, ImmutableArray<CircuitBoxRenameConnectionLabelsEvent> LabelOverrides, Vector2 InputPos, Vector2 OutputPos)
		{
			this.Components = Components;
			this.Wires = Wires;
			this.Labels = Labels;
			this.LabelOverrides = LabelOverrides;
			this.InputPos = InputPos;
			this.OutputPos = OutputPos;
		}

		// Token: 0x1700080F RID: 2063
		// (get) Token: 0x06001B61 RID: 7009 RVA: 0x000CC15D File Offset: 0x000CA35D
		// (set) Token: 0x06001B62 RID: 7010 RVA: 0x000CC165 File Offset: 0x000CA365
		public ImmutableArray<CircuitBoxServerCreateComponentEvent> Components { get; set; }

		// Token: 0x17000810 RID: 2064
		// (get) Token: 0x06001B63 RID: 7011 RVA: 0x000CC16E File Offset: 0x000CA36E
		// (set) Token: 0x06001B64 RID: 7012 RVA: 0x000CC176 File Offset: 0x000CA376
		public ImmutableArray<CircuitBoxServerCreateWireEvent> Wires { get; set; }

		// Token: 0x17000811 RID: 2065
		// (get) Token: 0x06001B65 RID: 7013 RVA: 0x000CC17F File Offset: 0x000CA37F
		// (set) Token: 0x06001B66 RID: 7014 RVA: 0x000CC187 File Offset: 0x000CA387
		public ImmutableArray<CircuitBoxServerAddLabelEvent> Labels { get; set; }

		// Token: 0x17000812 RID: 2066
		// (get) Token: 0x06001B67 RID: 7015 RVA: 0x000CC190 File Offset: 0x000CA390
		// (set) Token: 0x06001B68 RID: 7016 RVA: 0x000CC198 File Offset: 0x000CA398
		public ImmutableArray<CircuitBoxRenameConnectionLabelsEvent> LabelOverrides { get; set; }

		// Token: 0x17000813 RID: 2067
		// (get) Token: 0x06001B69 RID: 7017 RVA: 0x000CC1A1 File Offset: 0x000CA3A1
		// (set) Token: 0x06001B6A RID: 7018 RVA: 0x000CC1A9 File Offset: 0x000CA3A9
		public Vector2 InputPos { get; set; }

		// Token: 0x17000814 RID: 2068
		// (get) Token: 0x06001B6B RID: 7019 RVA: 0x000CC1B2 File Offset: 0x000CA3B2
		// (set) Token: 0x06001B6C RID: 7020 RVA: 0x000CC1BA File Offset: 0x000CA3BA
		public Vector2 OutputPos { get; set; }

		// Token: 0x06001B6D RID: 7021 RVA: 0x000CC1C4 File Offset: 0x000CA3C4
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

		// Token: 0x06001B6E RID: 7022 RVA: 0x000CC210 File Offset: 0x000CA410
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

		// Token: 0x06001B6F RID: 7023 RVA: 0x000CC30A File Offset: 0x000CA50A
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxInitializeStateFromServerEvent left, CircuitBoxInitializeStateFromServerEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B70 RID: 7024 RVA: 0x000CC316 File Offset: 0x000CA516
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxInitializeStateFromServerEvent left, CircuitBoxInitializeStateFromServerEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B71 RID: 7025 RVA: 0x000CC320 File Offset: 0x000CA520
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((EqualityComparer<ImmutableArray<CircuitBoxServerCreateComponentEvent>>.Default.GetHashCode(this.<Components>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxServerCreateWireEvent>>.Default.GetHashCode(this.<Wires>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxServerAddLabelEvent>>.Default.GetHashCode(this.<Labels>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<CircuitBoxRenameConnectionLabelsEvent>>.Default.GetHashCode(this.<LabelOverrides>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<InputPos>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<OutputPos>k__BackingField);
		}

		// Token: 0x06001B72 RID: 7026 RVA: 0x000CC3B0 File Offset: 0x000CA5B0
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxInitializeStateFromServerEvent && this.Equals((CircuitBoxInitializeStateFromServerEvent)obj);
		}

		// Token: 0x06001B73 RID: 7027 RVA: 0x000CC3C8 File Offset: 0x000CA5C8
		[CompilerGenerated]
		public bool Equals(CircuitBoxInitializeStateFromServerEvent other)
		{
			return EqualityComparer<ImmutableArray<CircuitBoxServerCreateComponentEvent>>.Default.Equals(this.<Components>k__BackingField, other.<Components>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxServerCreateWireEvent>>.Default.Equals(this.<Wires>k__BackingField, other.<Wires>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxServerAddLabelEvent>>.Default.Equals(this.<Labels>k__BackingField, other.<Labels>k__BackingField) && EqualityComparer<ImmutableArray<CircuitBoxRenameConnectionLabelsEvent>>.Default.Equals(this.<LabelOverrides>k__BackingField, other.<LabelOverrides>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<InputPos>k__BackingField, other.<InputPos>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<OutputPos>k__BackingField, other.<OutputPos>k__BackingField);
		}

		// Token: 0x06001B74 RID: 7028 RVA: 0x000CC468 File Offset: 0x000CA668
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
