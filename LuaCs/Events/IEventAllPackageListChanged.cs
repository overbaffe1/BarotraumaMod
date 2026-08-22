using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200053E RID: 1342
	internal interface IEventAllPackageListChanged : IEvent<IEventAllPackageListChanged>, IEvent
	{
		// Token: 0x0600558B RID: 21899
		void OnAllPackageListChanged(IEnumerable<CorePackage> corePackages, IEnumerable<RegularPackage> regularPackages);
	}
}
