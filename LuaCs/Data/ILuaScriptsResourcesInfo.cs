using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000591 RID: 1425
	public interface ILuaScriptsResourcesInfo
	{
		// Token: 0x1700158B RID: 5515
		// (get) Token: 0x060056F0 RID: 22256
		ImmutableArray<ILuaScriptResourceInfo> LuaScripts { get; }
	}
}
