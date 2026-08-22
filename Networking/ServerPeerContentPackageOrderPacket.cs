using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004BB RID: 1211
	[NetworkSerialize(66, ArrayMaxSize = 65535)]
	internal struct ServerPeerContentPackageOrderPacket : INetSerializableStruct
	{
		// Token: 0x040029E3 RID: 10723
		[Nullable(1)]
		public string ServerName;

		// Token: 0x040029E4 RID: 10724
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<ServerContentPackage> ContentPackages;

		// Token: 0x040029E5 RID: 10725
		public bool AllowModDownloads;
	}
}
