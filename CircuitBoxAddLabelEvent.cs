using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000203 RID: 515
	[NetworkSerialize(109)]
	internal readonly struct CircuitBoxAddLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxAddLabelEvent>
	{
		// Token: 0x06003534 RID: 13620 RVA: 0x0020FD53 File Offset: 0x0020DF53
		public CircuitBoxAddLabelEvent(Vector2 Position, Color Color, NetLimitedString Header, NetLimitedString Body)
		{
			this.Position = Position;
			this.Color = Color;
			this.Header = Header;
			this.Body = Body;
		}

		// Token: 0x17000E3B RID: 3643
		// (get) Token: 0x06003535 RID: 13621 RVA: 0x0020FD72 File Offset: 0x0020DF72
		// (set) Token: 0x06003536 RID: 13622 RVA: 0x0020FD7A File Offset: 0x0020DF7A
		public Vector2 Position { get; set; }

		// Token: 0x17000E3C RID: 3644
		// (get) Token: 0x06003537 RID: 13623 RVA: 0x0020FD83 File Offset: 0x0020DF83
		// (set) Token: 0x06003538 RID: 13624 RVA: 0x0020FD8B File Offset: 0x0020DF8B
		public Color Color { get; set; }

		// Token: 0x17000E3D RID: 3645
		// (get) Token: 0x06003539 RID: 13625 RVA: 0x0020FD94 File Offset: 0x0020DF94
		// (set) Token: 0x0600353A RID: 13626 RVA: 0x0020FD9C File Offset: 0x0020DF9C
		public NetLimitedString Header { get; set; }

		// Token: 0x17000E3E RID: 3646
		// (get) Token: 0x0600353B RID: 13627 RVA: 0x0020FDA5 File Offset: 0x0020DFA5
		// (set) Token: 0x0600353C RID: 13628 RVA: 0x0020FDAD File Offset: 0x0020DFAD
		public NetLimitedString Body { get; set; }

		// Token: 0x0600353D RID: 13629 RVA: 0x0020FDB8 File Offset: 0x0020DFB8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxAddLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600353E RID: 13630 RVA: 0x0020FE04 File Offset: 0x0020E004
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Position = ");
			builder.Append(this.Position.ToString());
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", Header = ");
			builder.Append(this.Header.ToString());
			builder.Append(", Body = ");
			builder.Append(this.Body.ToString());
			return true;
		}

		// Token: 0x0600353F RID: 13631 RVA: 0x0020FEAE File Offset: 0x0020E0AE
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxAddLabelEvent left, CircuitBoxAddLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003540 RID: 13632 RVA: 0x0020FEBA File Offset: 0x0020E0BA
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxAddLabelEvent left, CircuitBoxAddLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003541 RID: 13633 RVA: 0x0020FEC4 File Offset: 0x0020E0C4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Header>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Body>k__BackingField);
		}

		// Token: 0x06003542 RID: 13634 RVA: 0x0020FF26 File Offset: 0x0020E126
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxAddLabelEvent && this.Equals((CircuitBoxAddLabelEvent)obj);
		}

		// Token: 0x06003543 RID: 13635 RVA: 0x0020FF40 File Offset: 0x0020E140
		[CompilerGenerated]
		public bool Equals(CircuitBoxAddLabelEvent other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Header>k__BackingField, other.<Header>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Body>k__BackingField, other.<Body>k__BackingField);
		}

		// Token: 0x06003544 RID: 13636 RVA: 0x0020FFAD File Offset: 0x0020E1AD
		[CompilerGenerated]
		public void Deconstruct(out Vector2 Position, out Color Color, out NetLimitedString Header, out NetLimitedString Body)
		{
			Position = this.Position;
			Color = this.Color;
			Header = this.Header;
			Body = this.Body;
		}
	}
}
