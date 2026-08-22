using System;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200051D RID: 1309
	public interface IPluginManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x06005447 RID: 21575
		Result<ImmutableArray<Type>> GetImplementingTypes<T>(bool includeInterfaces = false, bool includeAbstractTypes = false, bool includeDefaultContext = true);

		// Token: 0x06005448 RID: 21576
		bool TryGetPackageForPlugin<TPlugin>(out ContentPackage ownerPackage);

		// Token: 0x06005449 RID: 21577
		Type GetType(string typeName, bool isByRefType = false, bool includeInterfaces = false, bool includeDefaultContext = true);

		// Token: 0x0600544A RID: 21578
		Result ActivatePluginInstances(ImmutableArray<ContentPackage> executionOrder, bool excludeAlreadyRunningPackages = true);

		// Token: 0x0600544B RID: 21579
		Result LoadAssemblyResources(ImmutableArray<IAssemblyResourceInfo> resources);

		// Token: 0x0600544C RID: 21580
		Result UnloadManagedAssemblies();
	}
}
