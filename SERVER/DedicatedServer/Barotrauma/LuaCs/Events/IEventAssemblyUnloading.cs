using System;
using System.Reflection;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200045D RID: 1117
	public interface IEventAssemblyUnloading : IEvent<IEventAssemblyUnloading>, IEvent
	{
		// Token: 0x06003CCB RID: 15563
		void OnAssemblyUnloading(Assembly assembly);
	}
}
