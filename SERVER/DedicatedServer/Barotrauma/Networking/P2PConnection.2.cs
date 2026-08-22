using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003B1 RID: 945
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal abstract class P2PConnection : NetworkConnection<P2PEndpoint>
	{
		// Token: 0x06003777 RID: 14199 RVA: 0x00175C91 File Offset: 0x00173E91
		protected P2PConnection(P2PEndpoint endpoint) : base(endpoint)
		{
			this.Heartbeat();
		}

		// Token: 0x06003778 RID: 14200 RVA: 0x00175CA0 File Offset: 0x00173EA0
		public void Decay(float deltaTime)
		{
			this.Timeout -= (double)deltaTime;
		}

		// Token: 0x06003779 RID: 14201 RVA: 0x00175CB1 File Offset: 0x00173EB1
		public void Heartbeat()
		{
			this.Timeout = NetworkConnection.TimeoutThresholdNotInGame;
		}

		// Token: 0x04001BDB RID: 7131
		public double Timeout;
	}
}
