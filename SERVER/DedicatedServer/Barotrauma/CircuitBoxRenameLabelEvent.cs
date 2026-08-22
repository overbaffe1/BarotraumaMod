using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200011A RID: 282
	[NetworkSerialize(157)]
	internal readonly struct CircuitBoxRenameLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxRenameLabelEvent>
	{
		// Token: 0x06001B37 RID: 6967 RVA: 0x000CBC43 File Offset: 0x000C9E43
		public CircuitBoxRenameLabelEvent(ushort LabelId, Color Color, NetLimitedString NewHeader, NetLimitedString NewBody)
		{
			this.LabelId = LabelId;
			this.Color = Color;
			this.NewHeader = NewHeader;
			this.NewBody = NewBody;
		}

		// Token: 0x17000808 RID: 2056
		// (get) Token: 0x06001B38 RID: 6968 RVA: 0x000CBC62 File Offset: 0x000C9E62
		// (set) Token: 0x06001B39 RID: 6969 RVA: 0x000CBC6A File Offset: 0x000C9E6A
		public ushort LabelId { get; set; }

		// Token: 0x17000809 RID: 2057
		// (get) Token: 0x06001B3A RID: 6970 RVA: 0x000CBC73 File Offset: 0x000C9E73
		// (set) Token: 0x06001B3B RID: 6971 RVA: 0x000CBC7B File Offset: 0x000C9E7B
		public Color Color { get; set; }

		// Token: 0x1700080A RID: 2058
		// (get) Token: 0x06001B3C RID: 6972 RVA: 0x000CBC84 File Offset: 0x000C9E84
		// (set) Token: 0x06001B3D RID: 6973 RVA: 0x000CBC8C File Offset: 0x000C9E8C
		public NetLimitedString NewHeader { get; set; }

		// Token: 0x1700080B RID: 2059
		// (get) Token: 0x06001B3E RID: 6974 RVA: 0x000CBC95 File Offset: 0x000C9E95
		// (set) Token: 0x06001B3F RID: 6975 RVA: 0x000CBC9D File Offset: 0x000C9E9D
		public NetLimitedString NewBody { get; set; }

		// Token: 0x06001B40 RID: 6976 RVA: 0x000CBCA8 File Offset: 0x000C9EA8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRenameLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x000CBCF4 File Offset: 0x000C9EF4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("LabelId = ");
			builder.Append(this.LabelId.ToString());
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", NewHeader = ");
			builder.Append(this.NewHeader.ToString());
			builder.Append(", NewBody = ");
			builder.Append(this.NewBody.ToString());
			return true;
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x000CBD9E File Offset: 0x000C9F9E
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRenameLabelEvent left, CircuitBoxRenameLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x000CBDAA File Offset: 0x000C9FAA
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRenameLabelEvent left, CircuitBoxRenameLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B44 RID: 6980 RVA: 0x000CBDB4 File Offset: 0x000C9FB4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ushort>.Default.GetHashCode(this.<LabelId>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<NewHeader>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<NewBody>k__BackingField);
		}

		// Token: 0x06001B45 RID: 6981 RVA: 0x000CBE16 File Offset: 0x000CA016
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRenameLabelEvent && this.Equals((CircuitBoxRenameLabelEvent)obj);
		}

		// Token: 0x06001B46 RID: 6982 RVA: 0x000CBE30 File Offset: 0x000CA030
		[CompilerGenerated]
		public bool Equals(CircuitBoxRenameLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<LabelId>k__BackingField, other.<LabelId>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<NewHeader>k__BackingField, other.<NewHeader>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<NewBody>k__BackingField, other.<NewBody>k__BackingField);
		}

		// Token: 0x06001B47 RID: 6983 RVA: 0x000CBE9D File Offset: 0x000CA09D
		[CompilerGenerated]
		public void Deconstruct(out ushort LabelId, out Color Color, out NetLimitedString NewHeader, out NetLimitedString NewBody)
		{
			LabelId = this.LabelId;
			Color = this.Color;
			NewHeader = this.NewHeader;
			NewBody = this.NewBody;
		}
	}
}
