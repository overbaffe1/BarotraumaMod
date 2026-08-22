using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042A RID: 1066
	internal interface IEventScreenSelected : IEvent<IEventScreenSelected>, IEvent
	{
		// Token: 0x06003C6F RID: 15471
		void OnScreenSelected(Screen screen);
	}
}
