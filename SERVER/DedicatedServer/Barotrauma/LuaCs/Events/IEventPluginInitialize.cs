using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000457 RID: 1111
	public interface IEventPluginInitialize : IEvent<IEventPluginInitialize>, IEvent
	{
		// Token: 0x06003CC5 RID: 15557
		void Initialize();
	}
}
