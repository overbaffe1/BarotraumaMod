using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000459 RID: 1113
	public interface IEventPluginPreInitialize : IEvent<IEventPluginPreInitialize>, IEvent
	{
		// Token: 0x06003CC7 RID: 15559
		void PreInitPatching();
	}
}
