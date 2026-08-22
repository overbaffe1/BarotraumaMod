using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003BF RID: 959
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(74, ArrayMaxSize = 65535)]
	internal struct PeerPacketMessage : INetSerializableStruct
	{
		// Token: 0x17000F46 RID: 3910
		// (get) Token: 0x060037B5 RID: 14261 RVA: 0x00176244 File Offset: 0x00174444
		public readonly int Length
		{
			get
			{
				return this.Buffer.Length;
			}
		}

		// Token: 0x060037B6 RID: 14262 RVA: 0x0017624E File Offset: 0x0017444E
		public readonly IReadMessage GetReadMessageUncompressed()
		{
			return new ReadWriteMessage(this.Buffer, 0, this.Length * 8, false);
		}

		// Token: 0x060037B7 RID: 14263 RVA: 0x00176265 File Offset: 0x00174465
		public readonly IReadMessage GetReadMessage(bool isCompressed, NetworkConnection conn)
		{
			return new ReadOnlyMessage(this.Buffer, isCompressed, 0, this.Length, conn);
		}

		// Token: 0x04001BEF RID: 7151
		public byte[] Buffer;
	}
}
