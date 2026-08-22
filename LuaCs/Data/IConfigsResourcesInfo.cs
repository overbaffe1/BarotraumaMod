using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000592 RID: 1426
	public interface IConfigsResourcesInfo
	{
		// Token: 0x1700158C RID: 5516
		// (get) Token: 0x060056F1 RID: 22257
		ImmutableArray<IConfigResourceInfo> Configs { get; }
	}
}
