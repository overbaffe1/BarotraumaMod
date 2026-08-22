using System;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020003AC RID: 940
	internal sealed class LidgrenConnection : NetworkConnection<LidgrenEndpoint>
	{
		// Token: 0x06003766 RID: 14182 RVA: 0x00175ABD File Offset: 0x00173CBD
		public LidgrenConnection(NetConnection netConnection) : base(new LidgrenEndpoint(netConnection.RemoteEndPoint))
		{
			this.NetConnection = netConnection;
		}

		// Token: 0x06003767 RID: 14183 RVA: 0x00175AD8 File Offset: 0x00173CD8
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

		// Token: 0x04001BD3 RID: 7123
		public readonly NetConnection NetConnection;
	}
}
