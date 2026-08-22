using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004AD RID: 1197
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class P2PConnection<[Nullable(0)] T> : P2PConnection where T : P2PEndpoint
	{
		// Token: 0x06004F4A RID: 20298 RVA: 0x002AE7AD File Offset: 0x002AC9AD
		protected P2PConnection(T endpoint) : base(endpoint)
		{
		}

		// Token: 0x17001437 RID: 5175
		// (get) Token: 0x06004F4B RID: 20299 RVA: 0x002AE7BB File Offset: 0x002AC9BB
		public new T Endpoint
		{
			get
			{
				return base.Endpoint as T;
			}
		}
	}
}
