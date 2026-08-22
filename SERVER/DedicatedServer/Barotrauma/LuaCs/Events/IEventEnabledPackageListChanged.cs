using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042C RID: 1068
	internal interface IEventEnabledPackageListChanged : IEvent<IEventEnabledPackageListChanged>, IEvent
	{
		// Token: 0x06003C71 RID: 15473
		void OnEnabledPackageListChanged(CorePackage package, IEnumerable<RegularPackage> regularPackages);
	}
}
