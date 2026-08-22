using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200053F RID: 1343
	internal interface IEventEnabledPackageListChanged : IEvent<IEventEnabledPackageListChanged>, IEvent
	{
		// Token: 0x0600558C RID: 21900
		void OnEnabledPackageListChanged(CorePackage package, IEnumerable<RegularPackage> regularPackages);
	}
}
