using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004AE RID: 1198
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal abstract class P2PConnection : NetworkConnection<P2PEndpoint>
	{
		// Token: 0x06004F4C RID: 20300 RVA: 0x002AE7CD File Offset: 0x002AC9CD
		protected P2PConnection(P2PEndpoint endpoint) : base(endpoint)
		{
			this.Heartbeat();
		}

		// Token: 0x06004F4D RID: 20301 RVA: 0x002AE7DC File Offset: 0x002AC9DC
		public void Decay(float deltaTime)
		{
			this.Timeout -= (double)deltaTime;
		}

		// Token: 0x06004F4E RID: 20302 RVA: 0x002AE7ED File Offset: 0x002AC9ED
		public void Heartbeat()
		{
			this.Timeout = NetworkConnection.TimeoutThresholdNotInGame;
		}

		// Token: 0x040029D2 RID: 10706
		public double Timeout;
	}
}
