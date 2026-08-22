using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200045B RID: 1115
	public interface IEventAssemblyContextCreated : IEvent<IEventAssemblyContextCreated>, IEvent
	{
		// Token: 0x06003CC9 RID: 15561
		void OnAssemblyCreated(IAssemblyLoaderService loaderService);
	}
}
