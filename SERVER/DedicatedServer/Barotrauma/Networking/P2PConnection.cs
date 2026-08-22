using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003B0 RID: 944
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class P2PConnection<[Nullable(0)] T> : P2PConnection where T : P2PEndpoint
	{
		// Token: 0x06003775 RID: 14197 RVA: 0x00175C71 File Offset: 0x00173E71
		protected P2PConnection(T endpoint) : base(endpoint)
		{
		}

		// Token: 0x17000F3C RID: 3900
		// (get) Token: 0x06003776 RID: 14198 RVA: 0x00175C7F File Offset: 0x00173E7F
		public new T Endpoint
		{
			get
			{
				return base.Endpoint as T;
			}
		}
	}
}
