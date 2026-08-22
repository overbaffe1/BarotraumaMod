using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000108 RID: 264
	[NetworkSerialize(44)]
	internal readonly struct NetCircuitBoxHeader : INetSerializableStruct, IEquatable<NetCircuitBoxHeader>
	{
		// Token: 0x06001A24 RID: 6692 RVA: 0x000C959D File Offset: 0x000C779D
		public NetCircuitBoxHeader(CircuitBoxOpcode Opcode, ushort ItemID, byte ComponentIndex)
		{
			this.Opcode = Opcode;
			this.ItemID = ItemID;
			this.ComponentIndex = ComponentIndex;
		}

		// Token: 0x170007D2 RID: 2002
		// (get) Token: 0x06001A25 RID: 6693 RVA: 0x000C95B4 File Offset: 0x000C77B4
		// (set) Token: 0x06001A26 RID: 6694 RVA: 0x000C95BC File Offset: 0x000C77BC
		public CircuitBoxOpcode Opcode { get; set; }

		// Token: 0x170007D3 RID: 2003
		// (get) Token: 0x06001A27 RID: 6695 RVA: 0x000C95C5 File Offset: 0x000C77C5
		// (set) Token: 0x06001A28 RID: 6696 RVA: 0x000C95CD File Offset: 0x000C77CD
		public ushort ItemID { get; set; }

		// Token: 0x170007D4 RID: 2004
		// (get) Token: 0x06001A29 RID: 6697 RVA: 0x000C95D6 File Offset: 0x000C77D6
		// (set) Token: 0x06001A2A RID: 6698 RVA: 0x000C95DE File Offset: 0x000C77DE
		public byte ComponentIndex { get; set; }

		// Token: 0x06001A2B RID: 6699 RVA: 0x000C95E7 File Offset: 0x000C77E7
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBox> FindTarget()
		{
			return CircuitBox.FindCircuitBox(this.ItemID, this.ComponentIndex);
		}

		// Token: 0x06001A2C RID: 6700 RVA: 0x000C95FC File Offset: 0x000C77FC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetCircuitBoxHeader");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001A2D RID: 6701 RVA: 0x000C9648 File Offset: 0x000C7848
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Opcode = ");
			builder.Append(this.Opcode.ToString());
			builder.Append(", ItemID = ");
			builder.Append(this.ItemID.ToString());
			builder.Append(", ComponentIndex = ");
			builder.Append(this.ComponentIndex.ToString());
			return true;
		}

		// Token: 0x06001A2E RID: 6702 RVA: 0x000C96CB File Offset: 0x000C78CB
		[CompilerGenerated]
		public static bool operator !=(NetCircuitBoxHeader left, NetCircuitBoxHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06001A2F RID: 6703 RVA: 0x000C96D7 File Offset: 0x000C78D7
		[CompilerGenerated]
		public static bool operator ==(NetCircuitBoxHeader left, NetCircuitBoxHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A30 RID: 6704 RVA: 0x000C96E1 File Offset: 0x000C78E1
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<CircuitBoxOpcode>.Default.GetHashCode(this.<Opcode>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ItemID>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<ComponentIndex>k__BackingField);
		}

		// Token: 0x06001A31 RID: 6705 RVA: 0x000C9721 File Offset: 0x000C7921
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCircuitBoxHeader && this.Equals((NetCircuitBoxHeader)obj);
		}

		// Token: 0x06001A32 RID: 6706 RVA: 0x000C973C File Offset: 0x000C793C
		[CompilerGenerated]
		public bool Equals(NetCircuitBoxHeader other)
		{
			return EqualityComparer<CircuitBoxOpcode>.Default.Equals(this.<Opcode>k__BackingField, other.<Opcode>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ItemID>k__BackingField, other.<ItemID>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<ComponentIndex>k__BackingField, other.<ComponentIndex>k__BackingField);
		}

		// Token: 0x06001A33 RID: 6707 RVA: 0x000C9791 File Offset: 0x000C7991
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxOpcode Opcode, out ushort ItemID, out byte ComponentIndex)
		{
			Opcode = this.Opcode;
			ItemID = this.ItemID;
			ComponentIndex = this.ComponentIndex;
		}
	}
}
