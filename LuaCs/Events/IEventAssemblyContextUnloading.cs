using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200056C RID: 1388
	public interface IEventAssemblyContextUnloading : IEvent<IEventAssemblyContextUnloading>, IEvent
	{
		// Token: 0x060055DF RID: 21983
		void OnAssemblyUnloading(WeakReference<IAssemblyLoaderService> loaderService);
	}
}
