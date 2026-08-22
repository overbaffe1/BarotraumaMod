using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200057C RID: 1404
	public interface IModConfigInfo : IAssembliesResourcesInfo, ILuaScriptsResourcesInfo, IConfigsResourcesInfo
	{
		// Token: 0x17001549 RID: 5449
		// (get) Token: 0x06005630 RID: 22064
		ImmutableArray<IStylesResourceInfo> Styles { get; }

		// Token: 0x1700154A RID: 5450
		// (get) Token: 0x06005631 RID: 22065
		ContentPackage Package { get; }
	}
}
