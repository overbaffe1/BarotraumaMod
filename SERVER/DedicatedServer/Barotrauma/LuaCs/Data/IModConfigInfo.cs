using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200046D RID: 1133
	public interface IModConfigInfo : IAssembliesResourcesInfo, ILuaScriptsResourcesInfo, IConfigsResourcesInfo
	{
		// Token: 0x17001019 RID: 4121
		// (get) Token: 0x06003D65 RID: 15717
		ContentPackage Package { get; }
	}
}
