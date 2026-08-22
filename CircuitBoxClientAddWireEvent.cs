using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000210 RID: 528
	[NetworkSerialize(148)]
	internal readonly struct CircuitBoxClientAddWireEvent : INetSerializableStruct, IEquatable<CircuitBoxClientAddWireEvent>
	{
		// Token: 0x060035FB RID: 13819 RVA: 0x00211887 File Offset: 0x0020FA87
		public CircuitBoxClientAddWireEvent(Color Color, CircuitBoxConnectorIdentifier Start, CircuitBoxConnectorIdentifier End, uint SelectedWirePrefabIdentifier)
		{
			this.Color = Color;
			this.Start = Start;
			this.End = End;
			this.SelectedWirePrefabIdentifier = SelectedWirePrefabIdentifier;
		}

		// Token: 0x17000E64 RID: 3684
		// (get) Token: 0x060035FC RID: 13820 RVA: 0x002118A6 File Offset: 0x0020FAA6
		// (set) Token: 0x060035FD RID: 13821 RVA: 0x002118AE File Offset: 0x0020FAAE
		public Color Color { get; set; }

		// Token: 0x17000E65 RID: 3685
		// (get) Token: 0x060035FE RID: 13822 RVA: 0x002118B7 File Offset: 0x0020FAB7
		// (set) Token: 0x060035FF RID: 13823 RVA: 0x002118BF File Offset: 0x0020FABF
		public CircuitBoxConnectorIdentifier Start { get; set; }

		// Token: 0x17000E66 RID: 3686
		// (get) Token: 0x06003600 RID: 13824 RVA: 0x002118C8 File Offset: 0x0020FAC8
		// (set) Token: 0x06003601 RID: 13825 RVA: 0x002118D0 File Offset: 0x0020FAD0
		public CircuitBoxConnectorIdentifier End { get; set; }

		// Token: 0x17000E67 RID: 3687
		// (get) Token: 0x06003602 RID: 13826 RVA: 0x002118D9 File Offset: 0x0020FAD9
		// (set) Token: 0x06003603 RID: 13827 RVA: 0x002118E1 File Offset: 0x0020FAE1
		public uint SelectedWirePrefabIdentifier { get; set; }

		// Token: 0x06003604 RID: 13828 RVA: 0x002118EC File Offset: 0x0020FAEC
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

		// Token: 0x06003605 RID: 13829 RVA: 0x00211938 File Offset: 0x0020FB38
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

		// Token: 0x06003606 RID: 13830 RVA: 0x002119E2 File Offset: 0x0020FBE2
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxClientAddWireEvent left, CircuitBoxClientAddWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003607 RID: 13831 RVA: 0x002119EE File Offset: 0x0020FBEE
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxClientAddWireEvent left, CircuitBoxClientAddWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003608 RID: 13832 RVA: 0x002119F8 File Offset: 0x0020FBF8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<Start>k__BackingField)) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<End>k__BackingField)) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<SelectedWirePrefabIdentifier>k__BackingField);
		}

		// Token: 0x06003609 RID: 13833 RVA: 0x00211A5A File Offset: 0x0020FC5A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxClientAddWireEvent && this.Equals((CircuitBoxClientAddWireEvent)obj);
		}

		// Token: 0x0600360A RID: 13834 RVA: 0x00211A74 File Offset: 0x0020FC74
		[CompilerGenerated]
		public bool Equals(CircuitBoxClientAddWireEvent other)
		{
			return EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<Start>k__BackingField, other.<Start>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<End>k__BackingField, other.<End>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<SelectedWirePrefabIdentifier>k__BackingField, other.<SelectedWirePrefabIdentifier>k__BackingField);
		}

		// Token: 0x0600360B RID: 13835 RVA: 0x00211AE1 File Offset: 0x0020FCE1
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
