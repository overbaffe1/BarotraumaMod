using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200045C RID: 1116
	public interface IEventAssemblyContextUnloading : IEvent<IEventAssemblyContextUnloading>, IEvent
	{
		// Token: 0x06003CCA RID: 15562
		void OnAssemblyUnloading(WeakReference<IAssemblyLoaderService> loaderService);
	}
}
