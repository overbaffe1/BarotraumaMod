using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010B RID: 267
	[NetworkSerialize(112)]
	internal readonly struct CircuitBoxServerAddLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxServerAddLabelEvent>
	{
		// Token: 0x06001A56 RID: 6742 RVA: 0x000C9E0C File Offset: 0x000C800C
		public CircuitBoxServerAddLabelEvent(ushort ID, Vector2 Position, Vector2 Size, Color Color, NetLimitedString Header, NetLimitedString Body)
		{
			this.ID = ID;
			this.Position = Position;
			this.Size = Size;
			this.Color = Color;
			this.Header = Header;
			this.Body = Body;
		}

		// Token: 0x170007DB RID: 2011
		// (get) Token: 0x06001A57 RID: 6743 RVA: 0x000C9E3B File Offset: 0x000C803B
		// (set) Token: 0x06001A58 RID: 6744 RVA: 0x000C9E43 File Offset: 0x000C8043
		public ushort ID { get; set; }

		// Token: 0x170007DC RID: 2012
		// (get) Token: 0x06001A59 RID: 6745 RVA: 0x000C9E4C File Offset: 0x000C804C
		// (set) Token: 0x06001A5A RID: 6746 RVA: 0x000C9E54 File Offset: 0x000C8054
		public Vector2 Position { get; set; }

		// Token: 0x170007DD RID: 2013
		// (get) Token: 0x06001A5B RID: 6747 RVA: 0x000C9E5D File Offset: 0x000C805D
		// (set) Token: 0x06001A5C RID: 6748 RVA: 0x000C9E65 File Offset: 0x000C8065
		public Vector2 Size { get; set; }

		// Token: 0x170007DE RID: 2014
		// (get) Token: 0x06001A5D RID: 6749 RVA: 0x000C9E6E File Offset: 0x000C806E
		// (set) Token: 0x06001A5E RID: 6750 RVA: 0x000C9E76 File Offset: 0x000C8076
		public Color Color { get; set; }

		// Token: 0x170007DF RID: 2015
		// (get) Token: 0x06001A5F RID: 6751 RVA: 0x000C9E7F File Offset: 0x000C807F
		// (set) Token: 0x06001A60 RID: 6752 RVA: 0x000C9E87 File Offset: 0x000C8087
		public NetLimitedString Header { get; set; }

		// Token: 0x170007E0 RID: 2016
		// (get) Token: 0x06001A61 RID: 6753 RVA: 0x000C9E90 File Offset: 0x000C8090
		// (set) Token: 0x06001A62 RID: 6754 RVA: 0x000C9E98 File Offset: 0x000C8098
		public NetLimitedString Body { get; set; }

		// Token: 0x06001A63 RID: 6755 RVA: 0x000C9EA4 File Offset: 0x000C80A4
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

		// Token: 0x06001A64 RID: 6756 RVA: 0x000C9EF0 File Offset: 0x000C80F0
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

		// Token: 0x06001A65 RID: 6757 RVA: 0x000C9FE8 File Offset: 0x000C81E8
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerAddLabelEvent left, CircuitBoxServerAddLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A66 RID: 6758 RVA: 0x000C9FF4 File Offset: 0x000C81F4
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerAddLabelEvent left, CircuitBoxServerAddLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A67 RID: 6759 RVA: 0x000CA000 File Offset: 0x000C8200
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Size>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Header>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Body>k__BackingField);
		}

		// Token: 0x06001A68 RID: 6760 RVA: 0x000CA090 File Offset: 0x000C8290
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerAddLabelEvent && this.Equals((CircuitBoxServerAddLabelEvent)obj);
		}

		// Token: 0x06001A69 RID: 6761 RVA: 0x000CA0A8 File Offset: 0x000C82A8
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerAddLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Size>k__BackingField, other.<Size>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Header>k__BackingField, other.<Header>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Body>k__BackingField, other.<Body>k__BackingField);
		}

		// Token: 0x06001A6A RID: 6762 RVA: 0x000CA148 File Offset: 0x000C8348
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
