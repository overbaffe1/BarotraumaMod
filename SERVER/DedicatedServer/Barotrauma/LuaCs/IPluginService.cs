using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200040B RID: 1035
	public interface IPluginService : IReusableService, IService, IDisposable
	{
		// Token: 0x06003B31 RID: 15153
		bool IsAssemblyLoaded(string friendlyName);

		// Token: 0x06003B32 RID: 15154
		Result LoadAndInstanceTypes<T>(IEnumerable<IAssemblyResourceInfo> assemblyResourcesInfo, bool injectServices, out ImmutableArray<T> typeInstances) where T : class, IAssemblyPlugin;

		// Token: 0x06003B33 RID: 15155
		Result<ImmutableArray<T>> GetLoadedPluginTypesInPackage<T>() where T : class, IAssemblyPlugin;

		// Token: 0x06003B34 RID: 15156
		Result AdvancePluginStates(PluginRunState newState);

		// Token: 0x06003B35 RID: 15157
		Result DisposePlugins();

		// Token: 0x06003B36 RID: 15158
		PluginRunState GetPluginRunState();
	}
}
