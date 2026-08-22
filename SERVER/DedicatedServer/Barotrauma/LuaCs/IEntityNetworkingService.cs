using System;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000408 RID: 1032
	public interface IEntityNetworkingService
	{
		// Token: 0x06003B16 RID: 15126
		Guid GetNetworkIdForInstance(INetworkSyncVar var);

		// Token: 0x06003B17 RID: 15127
		void RegisterNetVar(INetworkSyncVar netVar);

		// Token: 0x06003B18 RID: 15128
		void DeregisterNetVar(INetworkSyncVar netVar);

		// Token: 0x06003B19 RID: 15129
		void SendNetVar(INetworkSyncVar netVar);

		// Token: 0x06003B1A RID: 15130
		void SendNetVar(INetworkSyncVar netVar, NetworkConnection connection);
	}
}
