using System;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000409 RID: 1033
	public interface IPackageManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x06003B1B RID: 15131
		bool TryGetLoadedPackageByName(string name, out ContentPackage package);

		// Token: 0x06003B1C RID: 15132
		Result LoadPackageInfo(ContentPackage package);

		// Token: 0x06003B1D RID: 15133
		Result LoadPackagesInfo(ImmutableArray<ContentPackage> packages);

		// Token: 0x06003B1E RID: 15134
		Result ExecuteLoadedPackages(ImmutableArray<ContentPackage> executionOrder, bool executeCsAssemblies);

		// Token: 0x06003B1F RID: 15135
		Result SyncLoadedPackagesList(ImmutableArray<ContentPackage> packages);

		// Token: 0x06003B20 RID: 15136
		Result StopRunningPackages();

		// Token: 0x06003B21 RID: 15137
		Result UnloadPackage(ContentPackage package);

		// Token: 0x06003B22 RID: 15138
		Result UnloadPackages(ImmutableArray<ContentPackage> packages);

		// Token: 0x06003B23 RID: 15139
		Result UnloadAllPackages();

		// Token: 0x06003B24 RID: 15140
		ImmutableArray<ContentPackage> GetAllLoadedPackages();

		// Token: 0x06003B25 RID: 15141
		ImmutableArray<ContentPackage> GetLoadedUnrestrictedPackages();

		// Token: 0x06003B26 RID: 15142
		bool IsPackageRunning(ContentPackage package);

		// Token: 0x06003B27 RID: 15143
		bool IsAnyPackageLoaded();

		// Token: 0x06003B28 RID: 15144
		bool IsAnyPackageRunning();

		// Token: 0x06003B29 RID: 15145
		bool PackageContainsAnyRunnableResource(ContentPackage package);

		// Token: 0x06003B2A RID: 15146
		Result<IModConfigInfo> GetModConfigForPackage(ContentPackage package);
	}
}
