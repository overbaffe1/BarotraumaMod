using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042B RID: 1067
	internal interface IEventAllPackageListChanged : IEvent<IEventAllPackageListChanged>, IEvent
	{
		// Token: 0x06003C70 RID: 15472
		void OnAllPackageListChanged(IEnumerable<CorePackage> corePackages, IEnumerable<RegularPackage> regularPackages);
	}
}
