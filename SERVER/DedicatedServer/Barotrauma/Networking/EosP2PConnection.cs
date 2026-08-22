using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003AB RID: 939
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class EosP2PConnection : P2PConnection<EosP2PEndpoint>
	{
		// Token: 0x06003764 RID: 14180 RVA: 0x00175AA1 File Offset: 0x00173CA1
		public EosP2PConnection(EosP2PEndpoint endpoint) : base(endpoint)
		{
		}

		// Token: 0x06003765 RID: 14181 RVA: 0x00175AAA File Offset: 0x00173CAA
		public override bool AddressMatches(NetworkConnection other)
		{
			return base.Endpoint == other.Endpoint;
		}
	}
}
