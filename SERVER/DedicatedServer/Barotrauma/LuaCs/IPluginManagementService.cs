using System;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200040A RID: 1034
	public interface IPluginManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x06003B2B RID: 15147
		Result<ImmutableArray<Type>> GetImplementingTypes<T>(bool includeInterfaces = false, bool includeAbstractTypes = false, bool includeDefaultContext = true);

		// Token: 0x06003B2C RID: 15148
		bool TryGetPackageForPlugin<TPlugin>(out ContentPackage ownerPackage);

		// Token: 0x06003B2D RID: 15149
		Type GetType(string typeName, bool isByRefType = false, bool includeInterfaces = false, bool includeDefaultContext = true);

		// Token: 0x06003B2E RID: 15150
		Result ActivatePluginInstances(ImmutableArray<ContentPackage> executionOrder, bool excludeAlreadyRunningPackages = true);

		// Token: 0x06003B2F RID: 15151
		Result LoadAssemblyResources(ImmutableArray<IAssemblyResourceInfo> resources);

		// Token: 0x06003B30 RID: 15152
		Result UnloadManagedAssemblies();
	}
}
