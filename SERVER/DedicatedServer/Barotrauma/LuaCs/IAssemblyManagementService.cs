using System;
using System.Reflection;
using FluentResults;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F8 RID: 1016
	public interface IAssemblyManagementService : IPluginManagementService, IReusableService, IService, IDisposable
	{
		// Token: 0x06003AC3 RID: 15043
		Result<Assembly> GetLoadedAssembly(OneOf<AssemblyName, string> assemblyName, in Guid[] excludedContexts);
	}
}
