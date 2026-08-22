using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042D RID: 1069
	internal interface IEventReloadAllPackages : IEvent<IEventReloadAllPackages>, IEvent
	{
		// Token: 0x06003C72 RID: 15474
		void OnReloadAllPackages();
	}
}
