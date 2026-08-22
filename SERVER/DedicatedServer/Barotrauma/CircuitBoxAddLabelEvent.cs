using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010A RID: 266
	[NetworkSerialize(109)]
	internal readonly struct CircuitBoxAddLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxAddLabelEvent>
	{
		// Token: 0x06001A45 RID: 6725 RVA: 0x000C9B7F File Offset: 0x000C7D7F
		public CircuitBoxAddLabelEvent(Vector2 Position, Color Color, NetLimitedString Header, NetLimitedString Body)
		{
			this.Position = Position;
			this.Color = Color;
			this.Header = Header;
			this.Body = Body;
		}

		// Token: 0x170007D7 RID: 2007
		// (get) Token: 0x06001A46 RID: 6726 RVA: 0x000C9B9E File Offset: 0x000C7D9E
		// (set) Token: 0x06001A47 RID: 6727 RVA: 0x000C9BA6 File Offset: 0x000C7DA6
		public Vector2 Position { get; set; }

		// Token: 0x170007D8 RID: 2008
		// (get) Token: 0x06001A48 RID: 6728 RVA: 0x000C9BAF File Offset: 0x000C7DAF
		// (set) Token: 0x06001A49 RID: 6729 RVA: 0x000C9BB7 File Offset: 0x000C7DB7
		public Color Color { get; set; }

		// Token: 0x170007D9 RID: 2009
		// (get) Token: 0x06001A4A RID: 6730 RVA: 0x000C9BC0 File Offset: 0x000C7DC0
		// (set) Token: 0x06001A4B RID: 6731 RVA: 0x000C9BC8 File Offset: 0x000C7DC8
		public NetLimitedString Header { get; set; }

		// Token: 0x170007DA RID: 2010
		// (get) Token: 0x06001A4C RID: 6732 RVA: 0x000C9BD1 File Offset: 0x000C7DD1
		// (set) Token: 0x06001A4D RID: 6733 RVA: 0x000C9BD9 File Offset: 0x000C7DD9
		public NetLimitedString Body { get; set; }

		// Token: 0x06001A4E RID: 6734 RVA: 0x000C9BE4 File Offset: 0x000C7DE4
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

		// Token: 0x06001A4F RID: 6735 RVA: 0x000C9C30 File Offset: 0x000C7E30
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

		// Token: 0x06001A50 RID: 6736 RVA: 0x000C9CDA File Offset: 0x000C7EDA
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxAddLabelEvent left, CircuitBoxAddLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A51 RID: 6737 RVA: 0x000C9CE6 File Offset: 0x000C7EE6
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxAddLabelEvent left, CircuitBoxAddLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A52 RID: 6738 RVA: 0x000C9CF0 File Offset: 0x000C7EF0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Header>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<Body>k__BackingField);
		}

		// Token: 0x06001A53 RID: 6739 RVA: 0x000C9D52 File Offset: 0x000C7F52
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxAddLabelEvent && this.Equals((CircuitBoxAddLabelEvent)obj);
		}

		// Token: 0x06001A54 RID: 6740 RVA: 0x000C9D6C File Offset: 0x000C7F6C
		[CompilerGenerated]
		public bool Equals(CircuitBoxAddLabelEvent other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Header>k__BackingField, other.<Header>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<Body>k__BackingField, other.<Body>k__BackingField);
		}

		// Token: 0x06001A55 RID: 6741 RVA: 0x000C9DD9 File Offset: 0x000C7FD9
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
