using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000474 RID: 1140
	public interface ILuaScriptsResourcesInfo
	{
		// Token: 0x17001026 RID: 4134
		// (get) Token: 0x06003D81 RID: 15745
		ImmutableArray<ILuaScriptResourceInfo> LuaScripts { get; }
	}
}
