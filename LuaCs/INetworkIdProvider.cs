using System;
using System.Diagnostics.CodeAnalysis;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F7 RID: 1271
	internal interface INetworkIdProvider : IService, IDisposable
	{
		// Token: 0x0600526D RID: 21101
		Guid GetNetworkIdForInstance([NotNull] IDataInfo instance);

		// Token: 0x0600526E RID: 21102
		Guid GetNetworkIdForInstance<TEntity>([NotNull] IDataInfo instance, TEntity attachedEntity) where TEntity : Entity;

		// Token: 0x0600526F RID: 21103
		Guid GetNetworkIdForInstance([NotNull] IDataInfo instance, [MaybeNull] ItemComponent attachedItemComponent);
	}
}
