using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003C0 RID: 960
	[NetworkSerialize(84, ArrayMaxSize = 255)]
	internal struct ClientPeerPasswordPacket : INetSerializableStruct
	{
		// Token: 0x04001BF0 RID: 7152
		[Nullable(1)]
		public byte[] Password;
	}
}
