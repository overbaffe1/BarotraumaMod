using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000201 RID: 513
	[NetworkSerialize(44)]
	internal readonly struct NetCircuitBoxHeader : INetSerializableStruct, IEquatable<NetCircuitBoxHeader>
	{
		// Token: 0x06003513 RID: 13587 RVA: 0x0020F773 File Offset: 0x0020D973
		public NetCircuitBoxHeader(CircuitBoxOpcode Opcode, ushort ItemID, byte ComponentIndex)
		{
			this.Opcode = Opcode;
			this.ItemID = ItemID;
			this.ComponentIndex = ComponentIndex;
		}

		// Token: 0x17000E36 RID: 3638
		// (get) Token: 0x06003514 RID: 13588 RVA: 0x0020F78A File Offset: 0x0020D98A
		// (set) Token: 0x06003515 RID: 13589 RVA: 0x0020F792 File Offset: 0x0020D992
		public CircuitBoxOpcode Opcode { get; set; }

		// Token: 0x17000E37 RID: 3639
		// (get) Token: 0x06003516 RID: 13590 RVA: 0x0020F79B File Offset: 0x0020D99B
		// (set) Token: 0x06003517 RID: 13591 RVA: 0x0020F7A3 File Offset: 0x0020D9A3
		public ushort ItemID { get; set; }

		// Token: 0x17000E38 RID: 3640
		// (get) Token: 0x06003518 RID: 13592 RVA: 0x0020F7AC File Offset: 0x0020D9AC
		// (set) Token: 0x06003519 RID: 13593 RVA: 0x0020F7B4 File Offset: 0x0020D9B4
		public byte ComponentIndex { get; set; }

		// Token: 0x0600351A RID: 13594 RVA: 0x0020F7BD File Offset: 0x0020D9BD
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBox> FindTarget()
		{
			return CircuitBox.FindCircuitBox(this.ItemID, this.ComponentIndex);
		}

		// Token: 0x0600351B RID: 13595 RVA: 0x0020F7D0 File Offset: 0x0020D9D0
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

		// Token: 0x0600351C RID: 13596 RVA: 0x0020F81C File Offset: 0x0020DA1C
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

		// Token: 0x0600351D RID: 13597 RVA: 0x0020F89F File Offset: 0x0020DA9F
		[CompilerGenerated]
		public static bool operator !=(NetCircuitBoxHeader left, NetCircuitBoxHeader right)
		{
			return !(left == right);
		}

		// Token: 0x0600351E RID: 13598 RVA: 0x0020F8AB File Offset: 0x0020DAAB
		[CompilerGenerated]
		public static bool operator ==(NetCircuitBoxHeader left, NetCircuitBoxHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600351F RID: 13599 RVA: 0x0020F8B5 File Offset: 0x0020DAB5
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<CircuitBoxOpcode>.Default.GetHashCode(this.<Opcode>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ItemID>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<ComponentIndex>k__BackingField);
		}

		// Token: 0x06003520 RID: 13600 RVA: 0x0020F8F5 File Offset: 0x0020DAF5
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCircuitBoxHeader && this.Equals((NetCircuitBoxHeader)obj);
		}

		// Token: 0x06003521 RID: 13601 RVA: 0x0020F910 File Offset: 0x0020DB10
		[CompilerGenerated]
		public bool Equals(NetCircuitBoxHeader other)
		{
			return EqualityComparer<CircuitBoxOpcode>.Default.Equals(this.<Opcode>k__BackingField, other.<Opcode>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ItemID>k__BackingField, other.<ItemID>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<ComponentIndex>k__BackingField, other.<ComponentIndex>k__BackingField);
		}

		// Token: 0x06003522 RID: 13602 RVA: 0x0020F965 File Offset: 0x0020DB65
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxOpcode Opcode, out ushort ItemID, out byte ComponentIndex)
		{
			Opcode = this.Opcode;
			ItemID = this.ItemID;
			ComponentIndex = this.ComponentIndex;
		}
	}
}
