using System;
using System.Reflection;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200056D RID: 1389
	public interface IEventAssemblyUnloading : IEvent<IEventAssemblyUnloading>, IEvent
	{
		// Token: 0x060055E0 RID: 21984
		void OnAssemblyUnloading(Assembly assembly);
	}
}
