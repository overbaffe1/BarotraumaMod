using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003B4 RID: 948
	internal sealed class SteamP2PConnection : P2PConnection<SteamP2PEndpoint>
	{
		// Token: 0x06003783 RID: 14211 RVA: 0x00175D1F File Offset: 0x00173F1F
		public SteamP2PConnection(SteamId steamId) : this(new SteamP2PEndpoint(steamId))
		{
		}

		// Token: 0x06003784 RID: 14212 RVA: 0x00175D2D File Offset: 0x00173F2D
		public SteamP2PConnection(SteamP2PEndpoint endpoint) : base(endpoint)
		{
		}

		// Token: 0x06003785 RID: 14213 RVA: 0x00175D36 File Offset: 0x00173F36
		public override bool AddressMatches(NetworkConnection other)
		{
			return base.Endpoint == other.Endpoint;
		}
	}
}
