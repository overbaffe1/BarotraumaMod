using System;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000407 RID: 1031
	internal interface INetworkingService : IReusableService, IService, IDisposable, ILuaCsNetworking, ILuaCsShim, IEntityNetworkingService
	{
		// Token: 0x17000FC7 RID: 4039
		// (get) Token: 0x06003B0F RID: 15119
		bool IsActive { get; }

		// Token: 0x17000FC8 RID: 4040
		// (get) Token: 0x06003B10 RID: 15120
		bool IsSynchronized { get; }

		// Token: 0x06003B11 RID: 15121
		IWriteMessage Start(string netId);

		// Token: 0x06003B12 RID: 15122
		IWriteMessage Start(Guid netId);

		// Token: 0x06003B13 RID: 15123
		void Receive(string netId, NetMessageReceived action);

		// Token: 0x06003B14 RID: 15124
		void Receive(Guid netId, NetMessageReceived action);

		// Token: 0x06003B15 RID: 15125
		void SendToClient(IWriteMessage netMessage, NetworkConnection connection = null, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable);
	}
}
