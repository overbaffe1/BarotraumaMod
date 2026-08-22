using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004A8 RID: 1192
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class EosP2PConnection : P2PConnection<EosP2PEndpoint>
	{
		// Token: 0x06004F39 RID: 20281 RVA: 0x002AE5DD File Offset: 0x002AC7DD
		public EosP2PConnection(EosP2PEndpoint endpoint) : base(endpoint)
		{
		}

		// Token: 0x06004F3A RID: 20282 RVA: 0x002AE5E6 File Offset: 0x002AC7E6
		public override bool AddressMatches(NetworkConnection other)
		{
			return base.Endpoint == other.Endpoint;
		}
	}
}
