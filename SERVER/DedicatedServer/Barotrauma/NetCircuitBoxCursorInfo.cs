using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000102 RID: 258
	[NetworkSerialize(8)]
	internal readonly struct NetCircuitBoxCursorInfo : INetSerializableStruct, IEquatable<NetCircuitBoxCursorInfo>
	{
		// Token: 0x060019FA RID: 6650 RVA: 0x000C8ADE File Offset: 0x000C6CDE
		public NetCircuitBoxCursorInfo([Nullable(1)] Vector2[] RecordedPositions, Option<Vector2> DragStart, Option<Identifier> HeldItem, ushort CharacterID = 0)
		{
			this.RecordedPositions = RecordedPositions;
			this.DragStart = DragStart;
			this.HeldItem = HeldItem;
			this.CharacterID = CharacterID;
		}

		// Token: 0x170007C9 RID: 1993
		// (get) Token: 0x060019FB RID: 6651 RVA: 0x000C8AFD File Offset: 0x000C6CFD
		// (set) Token: 0x060019FC RID: 6652 RVA: 0x000C8B05 File Offset: 0x000C6D05
		[Nullable(1)]
		public Vector2[] RecordedPositions { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x170007CA RID: 1994
		// (get) Token: 0x060019FD RID: 6653 RVA: 0x000C8B0E File Offset: 0x000C6D0E
		// (set) Token: 0x060019FE RID: 6654 RVA: 0x000C8B16 File Offset: 0x000C6D16
		public Option<Vector2> DragStart { get; set; }

		// Token: 0x170007CB RID: 1995
		// (get) Token: 0x060019FF RID: 6655 RVA: 0x000C8B1F File Offset: 0x000C6D1F
		// (set) Token: 0x06001A00 RID: 6656 RVA: 0x000C8B27 File Offset: 0x000C6D27
		public Option<Identifier> HeldItem { get; set; }

		// Token: 0x170007CC RID: 1996
		// (get) Token: 0x06001A01 RID: 6657 RVA: 0x000C8B30 File Offset: 0x000C6D30
		// (set) Token: 0x06001A02 RID: 6658 RVA: 0x000C8B38 File Offset: 0x000C6D38
		public ushort CharacterID { get; set; }

		// Token: 0x06001A03 RID: 6659 RVA: 0x000C8B44 File Offset: 0x000C6D44
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetCircuitBoxCursorInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x000C8B90 File Offset: 0x000C6D90
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("RecordedPositions = ");
			builder.Append(this.RecordedPositions);
			builder.Append(", DragStart = ");
			builder.Append(this.DragStart.ToString());
			builder.Append(", HeldItem = ");
			builder.Append(this.HeldItem.ToString());
			builder.Append(", CharacterID = ");
			builder.Append(this.CharacterID.ToString());
			return true;
		}

		// Token: 0x06001A05 RID: 6661 RVA: 0x000C8C2C File Offset: 0x000C6E2C
		[CompilerGenerated]
		public static bool operator !=(NetCircuitBoxCursorInfo left, NetCircuitBoxCursorInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x000C8C38 File Offset: 0x000C6E38
		[CompilerGenerated]
		public static bool operator ==(NetCircuitBoxCursorInfo left, NetCircuitBoxCursorInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x000C8C44 File Offset: 0x000C6E44
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2[]>.Default.GetHashCode(this.<RecordedPositions>k__BackingField) * -1521134295 + EqualityComparer<Option<Vector2>>.Default.GetHashCode(this.<DragStart>k__BackingField)) * -1521134295 + EqualityComparer<Option<Identifier>>.Default.GetHashCode(this.<HeldItem>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x000C8CA6 File Offset: 0x000C6EA6
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCircuitBoxCursorInfo && this.Equals((NetCircuitBoxCursorInfo)obj);
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x000C8CC0 File Offset: 0x000C6EC0
		[CompilerGenerated]
		public bool Equals(NetCircuitBoxCursorInfo other)
		{
			return EqualityComparer<Vector2[]>.Default.Equals(this.<RecordedPositions>k__BackingField, other.<RecordedPositions>k__BackingField) && EqualityComparer<Option<Vector2>>.Default.Equals(this.<DragStart>k__BackingField, other.<DragStart>k__BackingField) && EqualityComparer<Option<Identifier>>.Default.Equals(this.<HeldItem>k__BackingField, other.<HeldItem>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x000C8D2D File Offset: 0x000C6F2D
		[CompilerGenerated]
		public void Deconstruct([Nullable(1)] out Vector2[] RecordedPositions, out Option<Vector2> DragStart, out Option<Identifier> HeldItem, out ushort CharacterID)
		{
			RecordedPositions = this.RecordedPositions;
			DragStart = this.DragStart;
			HeldItem = this.HeldItem;
			CharacterID = this.CharacterID;
		}
	}
}
