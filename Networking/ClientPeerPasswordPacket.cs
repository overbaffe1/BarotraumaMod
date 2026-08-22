using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004BD RID: 1213
	[NetworkSerialize(84, ArrayMaxSize = 255)]
	internal struct ClientPeerPasswordPacket : INetSerializableStruct
	{
		// Token: 0x040029E7 RID: 10727
		[Nullable(1)]
		public byte[] Password;
	}
}
