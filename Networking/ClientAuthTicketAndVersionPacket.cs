using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004B6 RID: 1206
	[NetworkSerialize(27, ArrayMaxSize = 65535)]
	internal struct ClientAuthTicketAndVersionPacket : INetSerializableStruct
	{
		// Token: 0x040029D6 RID: 10710
		[Nullable(1)]
		public string Name;

		// Token: 0x040029D7 RID: 10711
		public Option<int> OwnerKey;

		// Token: 0x040029D8 RID: 10712
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<AccountId> AccountId;

		// Token: 0x040029D9 RID: 10713
		public Option<AuthenticationTicket> AuthTicket;

		// Token: 0x040029DA RID: 10714
		[Nullable(1)]
		public string GameVersion;

		// Token: 0x040029DB RID: 10715
		public Identifier Language;
	}
}
