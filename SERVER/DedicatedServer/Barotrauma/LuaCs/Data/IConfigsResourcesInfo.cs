using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000475 RID: 1141
	public interface IConfigsResourcesInfo
	{
		// Token: 0x17001027 RID: 4135
		// (get) Token: 0x06003D82 RID: 15746
		ImmutableArray<IConfigResourceInfo> Configs { get; }
	}
}
