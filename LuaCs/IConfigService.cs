using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004EA RID: 1258
	public interface IConfigService : IReusableService, IService, IDisposable, ILuaConfigService, ILuaService
	{
		// Token: 0x06005213 RID: 21011
		ImmutableArray<ISettingBase> GetDisplayableConfigs();

		// Token: 0x06005214 RID: 21012
		void RegisterSettingTypeInitializer<T>(string typeIdentifier, [TupleElementNames(new string[]
		{
			"ConfigService",
			"Info"
		})] Func<ValueTuple<IConfigService, IConfigInfo>, T> settingFactory) where T : class, ISettingBase;

		// Token: 0x06005215 RID: 21013
		Task<Result> LoadConfigsAsync(ImmutableArray<IConfigResourceInfo> configResources);

		// Token: 0x06005216 RID: 21014
		Task<Result> LoadConfigsProfilesAsync(ImmutableArray<IConfigResourceInfo> configProfileResources);

		// Token: 0x06005217 RID: 21015
		Result LoadSavedConfigsValues();

		// Token: 0x06005218 RID: 21016
		Result ApplyConfigProfile(ContentPackage package, string internalName);

		// Token: 0x06005219 RID: 21017
		Result DisposePackageData(ContentPackage package);

		// Token: 0x0600521A RID: 21018
		Result DisposeAllPackageData();
	}
}
