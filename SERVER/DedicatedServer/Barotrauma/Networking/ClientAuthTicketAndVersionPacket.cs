using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003B9 RID: 953
	[NetworkSerialize(27, ArrayMaxSize = 65535)]
	internal struct ClientAuthTicketAndVersionPacket : INetSerializableStruct
	{
		// Token: 0x04001BDF RID: 7135
		[Nullable(1)]
		public string Name;

		// Token: 0x04001BE0 RID: 7136
		public Option<int> OwnerKey;

		// Token: 0x04001BE1 RID: 7137
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<AccountId> AccountId;

		// Token: 0x04001BE2 RID: 7138
		public Option<AuthenticationTicket> AuthTicket;

		// Token: 0x04001BE3 RID: 7139
		[Nullable(1)]
		public string GameVersion;

		// Token: 0x04001BE4 RID: 7140
		public Identifier Language;
	}
}
