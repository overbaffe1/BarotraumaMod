using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003B8 RID: 952
	[NetworkSerialize(9)]
	internal struct PeerPacketHeaders : INetSerializableStruct
	{
		// Token: 0x0600378D RID: 14221 RVA: 0x00175E00 File Offset: 0x00174000
		public readonly void Deconstruct(out DeliveryMethod deliveryMethod, out PacketHeader packetHeader, out ConnectionInitialization? initialization)
		{
			deliveryMethod = this.DeliveryMethod;
			packetHeader = this.PacketHeader;
			initialization = this.Initialization;
		}

		// Token: 0x04001BDC RID: 7132
		public DeliveryMethod DeliveryMethod;

		// Token: 0x04001BDD RID: 7133
		public PacketHeader PacketHeader;

		// Token: 0x04001BDE RID: 7134
		public ConnectionInitialization? Initialization;
	}
}
