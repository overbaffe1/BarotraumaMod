using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000468 RID: 1128
	internal abstract class ClientPeer<TEndpoint> : ClientPeer where TEndpoint : Endpoint
	{
		// Token: 0x1700134F RID: 4943
		// (get) Token: 0x06004BD7 RID: 19415 RVA: 0x0029CD9A File Offset: 0x0029AF9A
		[Nullable(1)]
		public new TEndpoint ServerEndpoint
		{
			[NullableContext(1)]
			get
			{
				return this.ServerEndpoint as TEndpoint;
			}
		}

		// Token: 0x06004BD8 RID: 19416 RVA: 0x0029CDAC File Offset: 0x0029AFAC
		protected ClientPeer([Nullable(1)] TEndpoint serverEndpoint, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Endpoint> allServerEndpoints, ClientPeer.Callbacks callbacks, Option<int> ownerKey) : base(serverEndpoint, allServerEndpoints, callbacks, ownerKey)
		{
		}
	}
}
