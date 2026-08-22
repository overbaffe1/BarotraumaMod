using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F9 RID: 1017
	public interface IConfigService : IReusableService, IService, IDisposable, ILuaConfigService, ILuaService
	{
		// Token: 0x06003AC4 RID: 15044
		void RegisterSettingTypeInitializer<T>(string typeIdentifier, [TupleElementNames(new string[]
		{
			"ConfigService",
			"Info"
		})] Func<ValueTuple<IConfigService, IConfigInfo>, T> settingFactory) where T : class, ISettingBase;

		// Token: 0x06003AC5 RID: 15045
		Task<Result> LoadConfigsAsync(ImmutableArray<IConfigResourceInfo> configResources);

		// Token: 0x06003AC6 RID: 15046
		Task<Result> LoadConfigsProfilesAsync(ImmutableArray<IConfigResourceInfo> configProfileResources);

		// Token: 0x06003AC7 RID: 15047
		Result LoadSavedConfigsValues();

		// Token: 0x06003AC8 RID: 15048
		Result ApplyConfigProfile(ContentPackage package, string internalName);

		// Token: 0x06003AC9 RID: 15049
		Result DisposePackageData(ContentPackage package);

		// Token: 0x06003ACA RID: 15050
		Result DisposeAllPackageData();
	}
}
