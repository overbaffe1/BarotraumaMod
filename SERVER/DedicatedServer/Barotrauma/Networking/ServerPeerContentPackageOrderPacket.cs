using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003BE RID: 958
	[NetworkSerialize(66, ArrayMaxSize = 65535)]
	internal struct ServerPeerContentPackageOrderPacket : INetSerializableStruct
	{
		// Token: 0x04001BEC RID: 7148
		[Nullable(1)]
		public string ServerName;

		// Token: 0x04001BED RID: 7149
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<ServerContentPackage> ContentPackages;

		// Token: 0x04001BEE RID: 7150
		public bool AllowModDownloads;
	}
}
