using System;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200051A RID: 1306
	internal interface INetworkingService : IReusableService, IService, IDisposable, ILuaCsNetworking, ILuaCsShim, IEntityNetworkingService
	{
		// Token: 0x1700150E RID: 5390
		// (get) Token: 0x0600542B RID: 21547
		bool IsActive { get; }

		// Token: 0x1700150F RID: 5391
		// (get) Token: 0x0600542C RID: 21548
		bool IsSynchronized { get; }

		// Token: 0x0600542D RID: 21549
		IWriteMessage Start(string netId);

		// Token: 0x0600542E RID: 21550
		IWriteMessage Start(Guid netId);

		// Token: 0x0600542F RID: 21551
		void Receive(string netId, NetMessageReceived action);

		// Token: 0x06005430 RID: 21552
		void Receive(Guid netId, NetMessageReceived action);

		// Token: 0x06005431 RID: 21553
		void SendToServer(IWriteMessage netMessage, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable);
	}
}
