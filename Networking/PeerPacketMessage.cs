using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004BC RID: 1212
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(74, ArrayMaxSize = 65535)]
	internal struct PeerPacketMessage : INetSerializableStruct
	{
		// Token: 0x17001441 RID: 5185
		// (get) Token: 0x06004F8A RID: 20362 RVA: 0x002AED80 File Offset: 0x002ACF80
		public readonly int Length
		{
			get
			{
				return this.Buffer.Length;
			}
		}

		// Token: 0x06004F8B RID: 20363 RVA: 0x002AED8A File Offset: 0x002ACF8A
		public readonly IReadMessage GetReadMessageUncompressed()
		{
			return new ReadWriteMessage(this.Buffer, 0, this.Length * 8, false);
		}

		// Token: 0x06004F8C RID: 20364 RVA: 0x002AEDA1 File Offset: 0x002ACFA1
		public readonly IReadMessage GetReadMessage(bool isCompressed, NetworkConnection conn)
		{
			return new ReadOnlyMessage(this.Buffer, isCompressed, 0, this.Length, conn);
		}

		// Token: 0x040029E6 RID: 10726
		public byte[] Buffer;
	}
}
