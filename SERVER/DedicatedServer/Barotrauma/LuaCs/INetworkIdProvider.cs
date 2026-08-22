using System;
using System.Diagnostics.CodeAnalysis;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E0 RID: 992
	internal interface INetworkIdProvider : IService, IDisposable
	{
		// Token: 0x06003918 RID: 14616
		Guid GetNetworkIdForInstance([NotNull] IDataInfo instance);

		// Token: 0x06003919 RID: 14617
		Guid GetNetworkIdForInstance<TEntity>([NotNull] IDataInfo instance, TEntity attachedEntity) where TEntity : Entity;

		// Token: 0x0600391A RID: 14618
		Guid GetNetworkIdForInstance([NotNull] IDataInfo instance, [MaybeNull] ItemComponent attachedItemComponent);
	}
}
