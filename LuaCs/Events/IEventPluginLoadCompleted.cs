using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000568 RID: 1384
	public interface IEventPluginLoadCompleted : IEvent<IEventPluginLoadCompleted>, IEvent
	{
		// Token: 0x060055DB RID: 21979
		void OnLoadCompleted();
	}
}
