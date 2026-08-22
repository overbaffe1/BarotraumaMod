using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200056B RID: 1387
	public interface IEventAssemblyContextCreated : IEvent<IEventAssemblyContextCreated>, IEvent
	{
		// Token: 0x060055DE RID: 21982
		void OnAssemblyCreated(IAssemblyLoaderService loaderService);
	}
}
