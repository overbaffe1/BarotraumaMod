using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003B3 RID: 947
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class PipeConnection : NetworkConnection<PipeEndpoint>
	{
		// Token: 0x06003781 RID: 14209 RVA: 0x00175CF6 File Offset: 0x00173EF6
		public PipeConnection([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId) : base(new PipeEndpoint())
		{
			base.SetAccountInfo(new AccountInfo(accountId, Array.Empty<AccountId>()));
		}

		// Token: 0x06003782 RID: 14210 RVA: 0x00175D14 File Offset: 0x00173F14
		public override bool AddressMatches(NetworkConnection other)
		{
			return other is PipeConnection;
		}
	}
}
