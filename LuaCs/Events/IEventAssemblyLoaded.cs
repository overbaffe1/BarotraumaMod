using System;
using System.Reflection;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200056A RID: 1386
	public interface IEventAssemblyLoaded : IEvent<IEventAssemblyLoaded>, IEvent
	{
		// Token: 0x060055DD RID: 21981
		void OnAssemblyLoaded(Assembly assembly);
	}
}
