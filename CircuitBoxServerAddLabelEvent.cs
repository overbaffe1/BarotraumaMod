using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000204 RID: 516
	[NetworkSerialize(112)]
	internal readonly struct CircuitBoxServerAddLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxServerAddLabelEvent>
	{
		// Token: 0x06003545 RID: 13637 RVA: 0x0020FFE0 File Offset: 0x0020E1E0
		public CircuitBoxServerAddLabelEvent(ushort ID, Vector2 Position, Vector2 Size, Color Color, NetLimitedString Header, NetLimitedString Body)
		{
			this.ID = ID;
			this.Position = Position;
			this.Size = Size;
			this.Color = Color;
			this.Header = Header;
			this.Body = Body;
		}

		// Token: 0x17000E3F RID: 3647
		// (get) Token: 0x06003546 RID: 13638 RVA: 0x0021000F File Offset: 0x0020E20F
		// (set) Token: 0x06003547 RID: 13639 RVA: 0x00210017 File Offset: 0x0020E217
		public ushort ID { get; set; }

		// Token: 0x17000E40 RID: 3648
		// (get) Token: 0x06003548 RID: 13640 RVA: 0x00210020 File Offset: 0x0020E220
		// (set) Token: 0x06003549 RID: 13641 RVA: 0x00210028 File Offset: 0x0020E228
		public Vector2 Position { get; set; }

		// Token: 0x17000E41 RID: 3649
		// (get) Token: 0x0600354A RID: 13642 RVA: 0x00210031 File Offset: 0x0020E231
		// (set) Token: 0x0600354B RID: 13643 RVA: 0x00210039 File Offset: 0x0020E239
		public Vector2 Size { get; set; }

		// Token: 0x17000E42 RID: 3650
		// (get) Token: 0x0600354C RID: 13644 RVA: 0x00210042 File Offset: 0x0020E242
		// (set) Token: 0x0600354D RID: 13645 RVA: 0x0021004A File Offset: 0x0020E24A
		public Color Color { get; set; }

		// Token: 0x17000E43 RID: 3651
		// (get) Token: 0x0600354E RID: 13646 RVA: 0x00210053 File Offset: 0x0020E253
		// (set) Token: 0x0600354F RID: 13647 RVA: 0x0021005B File Offset: 0x0020E25B
		public NetLimitedString Header { get; set; }

		// Token: 0x17000E44 RID: 3652
		// (get) Token: 0x06003550 RID: 13648 RVA: 0x00210064 File Offset: 0x0020E264
		// (set) Token: 0x06003551 RID: 13649 RVA: 0x0021006C File Offset: 0x0020E26C
		public NetLimitedString Body { get; set; }

		// Token: 0x06003552 RID: 13650 RVA: 0x00210078 File Offset: 0x0020E278
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxServerAddLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003553 RID: 13651 RVA: 0x002100C4 File Offset: 0x0020E2C4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ID = ");
			builder.Append(this.ID.ToString());
			builder.Append(", Position = ");
			builder.Append(this.Position.ToString());
			builder.Append(", Size = ");
			builder.Append(this.Size.ToString());
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", Header = ");
			builder.Append(this.Header.ToString());
			builder.Append(", Body = ");
			builder.Append(this.Body.ToString());
			return true;
		}

		// Token: 0x06003554 RID: 13652 RVA: 0x002101BC File Offset: 0x0020E3BC
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerAddLabelEvent left, CircuitBoxServerAddLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003555 RID: 13653 RVA: 0x002101C8 File Offset: 0x0020E3C8
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerAddLabelEvent left, CircuitBoxServerAddLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003556 RID: 13654 RVA: 0x002101D4 File Offset: 0x0020E3D4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Size>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Header>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Body>k__BackingField);
		}

		// Token: 0x06003557 RID: 13655 RVA: 0x00210264 File Offset: 0x0020E464
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerAddLabelEvent && this.Equals((CircuitBoxServerAddLabelEvent)obj);
		}

		// Token: 0x06003558 RID: 13656 RVA: 0x0021027C File Offset: 0x0020E47C
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerAddLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Size>k__BackingField, other.<Size>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Header>k__BackingField, other.<Header>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Body>k__BackingField, other.<Body>k__BackingField);
		}

		// Token: 0x06003559 RID: 13657 RVA: 0x0021031C File Offset: 0x0020E51C
		[CompilerGenerated]
		public void Deconstruct(out ushort ID, out Vector2 Position, out Vector2 Size, out Color Color, out NetLimitedString Header, out NetLimitedString Body)
		{
			ID = this.ID;
			Position = this.Position;
			Size = this.Size;
			Color = this.Color;
			Header = this.Header;
			Body = this.Body;
		}
	}
}
