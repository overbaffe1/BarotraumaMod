using System;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020004A9 RID: 1193
	internal sealed class LidgrenConnection : NetworkConnection<LidgrenEndpoint>
	{
		// Token: 0x06004F3B RID: 20283 RVA: 0x002AE5F9 File Offset: 0x002AC7F9
		public LidgrenConnection(NetConnection netConnection) : base(new LidgrenEndpoint(netConnection.RemoteEndPoint))
		{
			this.NetConnection = netConnection;
		}

		// Token: 0x06004F3C RID: 20284 RVA: 0x002AE614 File Offset: 0x002AC814
		public override bool AddressMatches(NetworkConnection other)
		{
			LidgrenConnection lidgrenConnection = other as LidgrenConnection;
			if (lidgrenConnection != null)
			{
				LidgrenEndpoint otherEndpoint = lidgrenConnection.Endpoint;
				if (otherEndpoint != null)
				{
					LidgrenEndpoint endpoint = base.Endpoint;
					if (endpoint != null)
					{
						return endpoint.Address == otherEndpoint.Address;
					}
				}
			}
			return false;
		}

		// Token: 0x040029CA RID: 10698
		public readonly NetConnection NetConnection;
	}
}
