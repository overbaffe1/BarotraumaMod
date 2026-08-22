using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000117 RID: 279
	[NetworkSerialize(148)]
	internal readonly struct CircuitBoxClientAddWireEvent : INetSerializableStruct, IEquatable<CircuitBoxClientAddWireEvent>
	{
		// Token: 0x06001B0C RID: 6924 RVA: 0x000CB6B3 File Offset: 0x000C98B3
		public CircuitBoxClientAddWireEvent(Color Color, CircuitBoxConnectorIdentifier Start, CircuitBoxConnectorIdentifier End, uint SelectedWirePrefabIdentifier)
		{
			this.Color = Color;
			this.Start = Start;
			this.End = End;
			this.SelectedWirePrefabIdentifier = SelectedWirePrefabIdentifier;
		}

		// Token: 0x17000800 RID: 2048
		// (get) Token: 0x06001B0D RID: 6925 RVA: 0x000CB6D2 File Offset: 0x000C98D2
		// (set) Token: 0x06001B0E RID: 6926 RVA: 0x000CB6DA File Offset: 0x000C98DA
		public Color Color { get; set; }

		// Token: 0x17000801 RID: 2049
		// (get) Token: 0x06001B0F RID: 6927 RVA: 0x000CB6E3 File Offset: 0x000C98E3
		// (set) Token: 0x06001B10 RID: 6928 RVA: 0x000CB6EB File Offset: 0x000C98EB
		public CircuitBoxConnectorIdentifier Start { get; set; }

		// Token: 0x17000802 RID: 2050
		// (get) Token: 0x06001B11 RID: 6929 RVA: 0x000CB6F4 File Offset: 0x000C98F4
		// (set) Token: 0x06001B12 RID: 6930 RVA: 0x000CB6FC File Offset: 0x000C98FC
		public CircuitBoxConnectorIdentifier End { get; set; }

		// Token: 0x17000803 RID: 2051
		// (get) Token: 0x06001B13 RID: 6931 RVA: 0x000CB705 File Offset: 0x000C9905
		// (set) Token: 0x06001B14 RID: 6932 RVA: 0x000CB70D File Offset: 0x000C990D
		public uint SelectedWirePrefabIdentifier { get; set; }

		// Token: 0x06001B15 RID: 6933 RVA: 0x000CB718 File Offset: 0x000C9918
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxClientAddWireEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B16 RID: 6934 RVA: 0x000CB764 File Offset: 0x000C9964
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", Start = ");
			builder.Append(this.Start.ToString());
			builder.Append(", End = ");
			builder.Append(this.End.ToString());
			builder.Append(", SelectedWirePrefabIdentifier = ");
			builder.Append(this.SelectedWirePrefabIdentifier.ToString());
			return true;
		}

		// Token: 0x06001B17 RID: 6935 RVA: 0x000CB80E File Offset: 0x000C9A0E
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxClientAddWireEvent left, CircuitBoxClientAddWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B18 RID: 6936 RVA: 0x000CB81A File Offset: 0x000C9A1A
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxClientAddWireEvent left, CircuitBoxClientAddWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x000CB824 File Offset: 0x000C9A24
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<Start>k__BackingField)) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<End>k__BackingField)) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<SelectedWirePrefabIdentifier>k__BackingField);
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x000CB886 File Offset: 0x000C9A86
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxClientAddWireEvent && this.Equals((CircuitBoxClientAddWireEvent)obj);
		}

		// Token: 0x06001B1B RID: 6939 RVA: 0x000CB8A0 File Offset: 0x000C9AA0
		[CompilerGenerated]
		public bool Equals(CircuitBoxClientAddWireEvent other)
		{
			return EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<Start>k__BackingField, other.<Start>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<End>k__BackingField, other.<End>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<SelectedWirePrefabIdentifier>k__BackingField, other.<SelectedWirePrefabIdentifier>k__BackingField);
		}

		// Token: 0x06001B1C RID: 6940 RVA: 0x000CB90D File Offset: 0x000C9B0D
		[CompilerGenerated]
		public void Deconstruct(out Color Color, out CircuitBoxConnectorIdentifier Start, out CircuitBoxConnectorIdentifier End, out uint SelectedWirePrefabIdentifier)
		{
			Color = this.Color;
			Start = this.Start;
			End = this.End;
			SelectedWirePrefabIdentifier = this.SelectedWirePrefabIdentifier;
		}
	}
}
