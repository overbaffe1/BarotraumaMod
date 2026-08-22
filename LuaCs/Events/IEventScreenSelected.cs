using System;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200053D RID: 1341
	internal interface IEventScreenSelected : IEvent<IEventScreenSelected>, IEvent
	{
		// Token: 0x0600558A RID: 21898
		void OnScreenSelected(Screen screen);
	}
}
