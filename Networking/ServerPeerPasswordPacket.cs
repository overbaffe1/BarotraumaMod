using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004BE RID: 1214
	[NetworkSerialize(90)]
	internal struct ServerPeerPasswordPacket : INetSerializableStruct
	{
		// Token: 0x040029E8 RID: 10728
		public Option<int> Salt;

		// Token: 0x040029E9 RID: 10729
		public Option<int> RetriesLeft;
	}
}
