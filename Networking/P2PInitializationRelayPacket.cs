using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004B9 RID: 1209
	[NetworkSerialize(52)]
	internal struct P2PInitializationRelayPacket : INetSerializableStruct
	{
		// Token: 0x040029DF RID: 10719
		public ulong LobbyID;

		// Token: 0x040029E0 RID: 10720
		public PeerPacketMessage Message;
	}
}
