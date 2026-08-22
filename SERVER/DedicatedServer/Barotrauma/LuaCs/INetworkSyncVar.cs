using System;
using System.Collections.Generic;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E1 RID: 993
	public interface INetworkSyncVar : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x17000F95 RID: 3989
		// (get) Token: 0x0600391B RID: 14619
		Guid InstanceId { get; }

		// Token: 0x0600391C RID: 14620
		void SetNetworkOwner(IEntityNetworkingService networkingService);

		// Token: 0x17000F96 RID: 3990
		// (get) Token: 0x0600391D RID: 14621
		NetSync SyncType { get; }

		// Token: 0x17000F97 RID: 3991
		// (get) Token: 0x0600391E RID: 14622
		ClientPermissions WritePermissions { get; }

		// Token: 0x0600391F RID: 14623
		void ReadNetMessage(IReadMessage message);

		// Token: 0x06003920 RID: 14624
		void WriteNetMessage(IWriteMessage message);
	}
}
