using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000205 RID: 517
	[NetworkSerialize(115)]
	internal readonly struct CircuitBoxResizeLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxResizeLabelEvent>
	{
		// Token: 0x0600355A RID: 13658 RVA: 0x00210370 File Offset: 0x0020E570
		public CircuitBoxResizeLabelEvent(ushort ID, Vector2 Position, Vector2 Size)
		{
			this.ID = ID;
			this.Position = Position;
			this.Size = Size;
		}

		// Token: 0x17000E45 RID: 3653
		// (get) Token: 0x0600355B RID: 13659 RVA: 0x00210387 File Offset: 0x0020E587
		// (set) Token: 0x0600355C RID: 13660 RVA: 0x0021038F File Offset: 0x0020E58F
		public ushort ID { get; set; }

		// Token: 0x17000E46 RID: 3654
		// (get) Token: 0x0600355D RID: 13661 RVA: 0x00210398 File Offset: 0x0020E598
		// (set) Token: 0x0600355E RID: 13662 RVA: 0x002103A0 File Offset: 0x0020E5A0
		public Vector2 Position { get; set; }

		// Token: 0x17000E47 RID: 3655
		// (get) Token: 0x0600355F RID: 13663 RVA: 0x002103A9 File Offset: 0x0020E5A9
		// (set) Token: 0x06003560 RID: 13664 RVA: 0x002103B1 File Offset: 0x0020E5B1
		public Vector2 Size { get; set; }

		// Token: 0x06003561 RID: 13665 RVA: 0x002103BC File Offset: 0x0020E5BC
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

		// Token: 0x06003562 RID: 13666 RVA: 0x00210408 File Offset: 0x0020E608
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

		// Token: 0x06003563 RID: 13667 RVA: 0x0021048B File Offset: 0x0020E68B
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxResizeLabelEvent left, CircuitBoxResizeLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003564 RID: 13668 RVA: 0x00210497 File Offset: 0x0020E697
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxResizeLabelEvent left, CircuitBoxResizeLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003565 RID: 13669 RVA: 0x002104A1 File Offset: 0x0020E6A1
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Size>k__BackingField);
		}

		// Token: 0x06003566 RID: 13670 RVA: 0x002104E1 File Offset: 0x0020E6E1
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxResizeLabelEvent && this.Equals((CircuitBoxResizeLabelEvent)obj);
		}

		// Token: 0x06003567 RID: 13671 RVA: 0x002104FC File Offset: 0x0020E6FC
		[CompilerGenerated]
		public bool Equals(CircuitBoxResizeLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Size>k__BackingField, other.<Size>k__BackingField);
		}

		// Token: 0x06003568 RID: 13672 RVA: 0x00210551 File Offset: 0x0020E751
		[CompilerGenerated]
		public void Deconstruct(out ushort ID, out Vector2 Position, out Vector2 Size)
		{
			ID = this.ID;
			Position = this.Position;
			Size = this.Size;
		}
	}
}
