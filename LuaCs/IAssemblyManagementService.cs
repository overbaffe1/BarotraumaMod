using System;
using System.Reflection;
using FluentResults;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050C RID: 1292
	public interface IAssemblyManagementService : IPluginManagementService, IReusableService, IService, IDisposable
	{
		// Token: 0x060053E7 RID: 21479
		Result<Assembly> GetLoadedAssembly(OneOf<AssemblyName, string> assemblyName, in Guid[] excludedContexts);
	}
}
