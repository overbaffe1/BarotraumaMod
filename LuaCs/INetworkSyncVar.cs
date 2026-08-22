using System;
using System.Collections.Generic;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F8 RID: 1272
	public interface INetworkSyncVar : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x170014DF RID: 5343
		// (get) Token: 0x06005270 RID: 21104
		Guid InstanceId { get; }

		// Token: 0x06005271 RID: 21105
		void SetNetworkOwner(IEntityNetworkingService networkingService);

		// Token: 0x170014E0 RID: 5344
		// (get) Token: 0x06005272 RID: 21106
		NetSync SyncType { get; }

		// Token: 0x170014E1 RID: 5345
		// (get) Token: 0x06005273 RID: 21107
		ClientPermissions WritePermissions { get; }

		// Token: 0x06005274 RID: 21108
		void ReadNetMessage(IReadMessage message);

		// Token: 0x06005275 RID: 21109
		void WriteNetMessage(IWriteMessage message);
	}
}
