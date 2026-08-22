using System;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200051C RID: 1308
	public interface IPackageManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x06005437 RID: 21559
		bool TryGetLoadedPackageByName(string name, out ContentPackage package);

		// Token: 0x06005438 RID: 21560
		Result LoadPackageInfo(ContentPackage package);

		// Token: 0x06005439 RID: 21561
		Result LoadPackagesInfo(ImmutableArray<ContentPackage> packages);

		// Token: 0x0600543A RID: 21562
		Result ExecuteLoadedPackages(ImmutableArray<ContentPackage> executionOrder, bool executeCsAssemblies);

		// Token: 0x0600543B RID: 21563
		Result SyncLoadedPackagesList(ImmutableArray<ContentPackage> packages);

		// Token: 0x0600543C RID: 21564
		Result StopRunningPackages();

		// Token: 0x0600543D RID: 21565
		Result UnloadPackage(ContentPackage package);

		// Token: 0x0600543E RID: 21566
		Result UnloadPackages(ImmutableArray<ContentPackage> packages);

		// Token: 0x0600543F RID: 21567
		Result UnloadAllPackages();

		// Token: 0x06005440 RID: 21568
		ImmutableArray<ContentPackage> GetAllLoadedPackages();

		// Token: 0x06005441 RID: 21569
		ImmutableArray<ContentPackage> GetLoadedUnrestrictedPackages();

		// Token: 0x06005442 RID: 21570
		bool IsPackageRunning(ContentPackage package);

		// Token: 0x06005443 RID: 21571
		bool IsAnyPackageLoaded();

		// Token: 0x06005444 RID: 21572
		bool IsAnyPackageRunning();

		// Token: 0x06005445 RID: 21573
		bool PackageContainsAnyRunnableResource(ContentPackage package);

		// Token: 0x06005446 RID: 21574
		Result<IModConfigInfo> GetModConfigForPackage(ContentPackage package);
	}
}
