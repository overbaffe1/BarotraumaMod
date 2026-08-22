using System;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200051B RID: 1307
	public interface IEntityNetworkingService
	{
		// Token: 0x06005432 RID: 21554
		Guid GetNetworkIdForInstance(INetworkSyncVar var);

		// Token: 0x06005433 RID: 21555
		void RegisterNetVar(INetworkSyncVar netVar);

		// Token: 0x06005434 RID: 21556
		void DeregisterNetVar(INetworkSyncVar netVar);

		// Token: 0x06005435 RID: 21557
		void SendNetVar(INetworkSyncVar netVar);

		// Token: 0x06005436 RID: 21558
		void SendNetVar(INetworkSyncVar netVar, NetworkConnection connection);
	}
}
