using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000473 RID: 1139
	public interface IAssembliesResourcesInfo
	{
		// Token: 0x17001025 RID: 4133
		// (get) Token: 0x06003D80 RID: 15744
		ImmutableArray<IAssemblyResourceInfo> Assemblies { get; }
	}
}
