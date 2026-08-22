using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010C RID: 268
	[NetworkSerialize(115)]
	internal readonly struct CircuitBoxResizeLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxResizeLabelEvent>
	{
		// Token: 0x06001A6B RID: 6763 RVA: 0x000CA19C File Offset: 0x000C839C
		public CircuitBoxResizeLabelEvent(ushort ID, Vector2 Position, Vector2 Size)
		{
			this.ID = ID;
			this.Position = Position;
			this.Size = Size;
		}

		// Token: 0x170007E1 RID: 2017
		// (get) Token: 0x06001A6C RID: 6764 RVA: 0x000CA1B3 File Offset: 0x000C83B3
		// (set) Token: 0x06001A6D RID: 6765 RVA: 0x000CA1BB File Offset: 0x000C83BB
		public ushort ID { get; set; }

		// Token: 0x170007E2 RID: 2018
		// (get) Token: 0x06001A6E RID: 6766 RVA: 0x000CA1C4 File Offset: 0x000C83C4
		// (set) Token: 0x06001A6F RID: 6767 RVA: 0x000CA1CC File Offset: 0x000C83CC
		public Vector2 Position { get; set; }

		// Token: 0x170007E3 RID: 2019
		// (get) Token: 0x06001A70 RID: 6768 RVA: 0x000CA1D5 File Offset: 0x000C83D5
		// (set) Token: 0x06001A71 RID: 6769 RVA: 0x000CA1DD File Offset: 0x000C83DD
		public Vector2 Size { get; set; }

		// Token: 0x06001A72 RID: 6770 RVA: 0x000CA1E8 File Offset: 0x000C83E8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxResizeLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x000CA234 File Offset: 0x000C8434
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ID = ");
			builder.Append(this.ID.ToString());
			builder.Append(", Position = ");
			builder.Append(this.Position.ToString());
			builder.Append(", Size = ");
			builder.Append(this.Size.ToString());
			return true;
		}

		// Token: 0x06001A74 RID: 6772 RVA: 0x000CA2B7 File Offset: 0x000C84B7
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxResizeLabelEvent left, CircuitBoxResizeLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A75 RID: 6773 RVA: 0x000CA2C3 File Offset: 0x000C84C3
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxResizeLabelEvent left, CircuitBoxResizeLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A76 RID: 6774 RVA: 0x000CA2CD File Offset: 0x000C84CD
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Size>k__BackingField);
		}

		// Token: 0x06001A77 RID: 6775 RVA: 0x000CA30D File Offset: 0x000C850D
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxResizeLabelEvent && this.Equals((CircuitBoxResizeLabelEvent)obj);
		}

		// Token: 0x06001A78 RID: 6776 RVA: 0x000CA328 File Offset: 0x000C8528
		[CompilerGenerated]
		public bool Equals(CircuitBoxResizeLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Size>k__BackingField, other.<Size>k__BackingField);
		}

		// Token: 0x06001A79 RID: 6777 RVA: 0x000CA37D File Offset: 0x000C857D
		[CompilerGenerated]
		public void Deconstruct(out ushort ID, out Vector2 Position, out Vector2 Size)
		{
			ID = this.ID;
			Position = this.Position;
			Size = this.Size;
		}
	}
}
