using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003C1 RID: 961
	[NetworkSerialize(90)]
	internal struct ServerPeerPasswordPacket : INetSerializableStruct
	{
		// Token: 0x04001BF1 RID: 7153
		public Option<int> Salt;

		// Token: 0x04001BF2 RID: 7154
		public Option<int> RetriesLeft;
	}
}
