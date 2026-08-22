using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200051E RID: 1310
	public interface IPluginService : IReusableService, IService, IDisposable
	{
		// Token: 0x0600544D RID: 21581
		bool IsAssemblyLoaded(string friendlyName);

		// Token: 0x0600544E RID: 21582
		Result LoadAndInstanceTypes<T>(IEnumerable<IAssemblyResourceInfo> assemblyResourcesInfo, bool injectServices, out ImmutableArray<T> typeInstances) where T : class, IAssemblyPlugin;

		// Token: 0x0600544F RID: 21583
		Result<ImmutableArray<T>> GetLoadedPluginTypesInPackage<T>() where T : class, IAssemblyPlugin;

		// Token: 0x06005450 RID: 21584
		Result AdvancePluginStates(PluginRunState newState);

		// Token: 0x06005451 RID: 21585
		Result DisposePlugins();

		// Token: 0x06005452 RID: 21586
		PluginRunState GetPluginRunState();
	}
}
