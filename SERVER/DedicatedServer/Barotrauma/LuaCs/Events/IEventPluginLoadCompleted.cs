using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000458 RID: 1112
	public interface IEventPluginLoadCompleted : IEvent<IEventPluginLoadCompleted>, IEvent
	{
		// Token: 0x06003CC6 RID: 15558
		void OnLoadCompleted();
	}
}
