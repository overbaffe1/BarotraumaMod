using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000590 RID: 1424
	public interface IAssembliesResourcesInfo
	{
		// Token: 0x1700158A RID: 5514
		// (get) Token: 0x060056EF RID: 22255
		ImmutableArray<IAssemblyResourceInfo> Assemblies { get; }
	}
}
