using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004B1 RID: 1201
	internal sealed class SteamP2PConnection : P2PConnection<SteamP2PEndpoint>
	{
		// Token: 0x06004F58 RID: 20312 RVA: 0x002AE85B File Offset: 0x002ACA5B
		public SteamP2PConnection(SteamId steamId) : this(new SteamP2PEndpoint(steamId))
		{
		}

		// Token: 0x06004F59 RID: 20313 RVA: 0x002AE869 File Offset: 0x002ACA69
		public SteamP2PConnection(SteamP2PEndpoint endpoint) : base(endpoint)
		{
		}

		// Token: 0x06004F5A RID: 20314 RVA: 0x002AE872 File Offset: 0x002ACA72
		public override bool AddressMatches(NetworkConnection other)
		{
			return base.Endpoint == other.Endpoint;
		}
	}
}
