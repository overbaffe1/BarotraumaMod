using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000569 RID: 1385
	public interface IEventPluginPreInitialize : IEvent<IEventPluginPreInitialize>, IEvent
	{
		// Token: 0x060055DC RID: 21980
		void PreInitPatching();
	}
}
