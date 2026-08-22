using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003BC RID: 956
	[NetworkSerialize(52)]
	internal struct P2PInitializationRelayPacket : INetSerializableStruct
	{
		// Token: 0x04001BE8 RID: 7144
		public ulong LobbyID;

		// Token: 0x04001BE9 RID: 7145
		public PeerPacketMessage Message;
	}
}
