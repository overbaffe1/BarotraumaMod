using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004B5 RID: 1205
	[NetworkSerialize(9)]
	internal struct PeerPacketHeaders : INetSerializableStruct
	{
		// Token: 0x06004F62 RID: 20322 RVA: 0x002AE93C File Offset: 0x002ACB3C
		public readonly void Deconstruct(out DeliveryMethod deliveryMethod, out PacketHeader packetHeader, out ConnectionInitialization? initialization)
		{
			deliveryMethod = this.DeliveryMethod;
			packetHeader = this.PacketHeader;
			initialization = this.Initialization;
		}

		// Token: 0x040029D3 RID: 10707
		public DeliveryMethod DeliveryMethod;

		// Token: 0x040029D4 RID: 10708
		public PacketHeader PacketHeader;

		// Token: 0x040029D5 RID: 10709
		public ConnectionInitialization? Initialization;
	}
}
