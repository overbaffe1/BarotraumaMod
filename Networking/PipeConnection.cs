using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004B0 RID: 1200
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class PipeConnection : NetworkConnection<PipeEndpoint>
	{
		// Token: 0x06004F56 RID: 20310 RVA: 0x002AE832 File Offset: 0x002ACA32
		public PipeConnection([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId) : base(new PipeEndpoint())
		{
			base.SetAccountInfo(new AccountInfo(accountId, Array.Empty<AccountId>()));
		}

		// Token: 0x06004F57 RID: 20311 RVA: 0x002AE850 File Offset: 0x002ACA50
		public override bool AddressMatches(NetworkConnection other)
		{
			return other is PipeConnection;
		}
	}
}
