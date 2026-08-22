using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FD RID: 509
	[NetworkSerialize(8)]
	internal readonly struct NetCircuitBoxCursorInfo : INetSerializableStruct, IEquatable<NetCircuitBoxCursorInfo>
	{
		// Token: 0x060034F6 RID: 13558 RVA: 0x0020F22A File Offset: 0x0020D42A
		public NetCircuitBoxCursorInfo([Nullable(1)] Vector2[] RecordedPositions, Option<Vector2> DragStart, Option<Identifier> HeldItem, ushort CharacterID = 0)
		{
			this.RecordedPositions = RecordedPositions;
			this.DragStart = DragStart;
			this.HeldItem = HeldItem;
			this.CharacterID = CharacterID;
		}

		// Token: 0x17000E30 RID: 3632
		// (get) Token: 0x060034F7 RID: 13559 RVA: 0x0020F249 File Offset: 0x0020D449
		// (set) Token: 0x060034F8 RID: 13560 RVA: 0x0020F251 File Offset: 0x0020D451
		[Nullable(1)]
		public Vector2[] RecordedPositions { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x17000E31 RID: 3633
		// (get) Token: 0x060034F9 RID: 13561 RVA: 0x0020F25A File Offset: 0x0020D45A
		// (set) Token: 0x060034FA RID: 13562 RVA: 0x0020F262 File Offset: 0x0020D462
		public Option<Vector2> DragStart { get; set; }

		// Token: 0x17000E32 RID: 3634
		// (get) Token: 0x060034FB RID: 13563 RVA: 0x0020F26B File Offset: 0x0020D46B
		// (set) Token: 0x060034FC RID: 13564 RVA: 0x0020F273 File Offset: 0x0020D473
		public Option<Identifier> HeldItem { get; set; }

		// Token: 0x17000E33 RID: 3635
		// (get) Token: 0x060034FD RID: 13565 RVA: 0x0020F27C File Offset: 0x0020D47C
		// (set) Token: 0x060034FE RID: 13566 RVA: 0x0020F284 File Offset: 0x0020D484
		public ushort CharacterID { get; set; }

		// Token: 0x060034FF RID: 13567 RVA: 0x0020F290 File Offset: 0x0020D490
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

		// Token: 0x06003500 RID: 13568 RVA: 0x0020F2DC File Offset: 0x0020D4DC
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

		// Token: 0x06003501 RID: 13569 RVA: 0x0020F378 File Offset: 0x0020D578
		[CompilerGenerated]
		public static bool operator !=(NetCircuitBoxCursorInfo left, NetCircuitBoxCursorInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003502 RID: 13570 RVA: 0x0020F384 File Offset: 0x0020D584
		[CompilerGenerated]
		public static bool operator ==(NetCircuitBoxCursorInfo left, NetCircuitBoxCursorInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003503 RID: 13571 RVA: 0x0020F390 File Offset: 0x0020D590
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Vector2[]>.Default.GetHashCode(this.<RecordedPositions>k__BackingField) * -1521134295 + EqualityComparer<Option<Vector2>>.Default.GetHashCode(this.<DragStart>k__BackingField)) * -1521134295 + EqualityComparer<Option<Identifier>>.Default.GetHashCode(this.<HeldItem>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x06003504 RID: 13572 RVA: 0x0020F3F2 File Offset: 0x0020D5F2
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCircuitBoxCursorInfo && this.Equals((NetCircuitBoxCursorInfo)obj);
		}

		// Token: 0x06003505 RID: 13573 RVA: 0x0020F40C File Offset: 0x0020D60C
		[CompilerGenerated]
		public bool Equals(NetCircuitBoxCursorInfo other)
		{
			return EqualityComparer<Vector2[]>.Default.Equals(this.<RecordedPositions>k__BackingField, other.<RecordedPositions>k__BackingField) && EqualityComparer<Option<Vector2>>.Default.Equals(this.<DragStart>k__BackingField, other.<DragStart>k__BackingField) && EqualityComparer<Option<Identifier>>.Default.Equals(this.<HeldItem>k__BackingField, other.<HeldItem>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x06003506 RID: 13574 RVA: 0x0020F479 File Offset: 0x0020D679
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
