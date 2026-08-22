using System;
using System.Reflection;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200045A RID: 1114
	public interface IEventAssemblyLoaded : IEvent<IEventAssemblyLoaded>, IEvent
	{
		// Token: 0x06003CC8 RID: 15560
		void OnAssemblyLoaded(Assembly assembly);
	}
}
