using System;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FB RID: 1019
	public interface IEventService : IReusableService, IService, IDisposable, ILuaEventService, ILuaSafeEventService, ILuaService, ILuaCsHook, ILuaPatcher, ILuaCsShim
	{
		// Token: 0x06003AD0 RID: 15056
		Result Subscribe<T>(T subscriber) where T : class, IEvent<T>;

		// Token: 0x06003AD1 RID: 15057
		void Unsubscribe<T>(T subscriber) where T : class, IEvent;

		// Token: 0x06003AD2 RID: 15058
		void ClearAllEventSubscribers<T>() where T : class, IEvent;

		// Token: 0x06003AD3 RID: 15059
		void ClearAllSubscribers();

		// Token: 0x06003AD4 RID: 15060
		Result PublishEvent<T>(Action<T> action) where T : class, IEvent<T>;

		// Token: 0x06003AD5 RID: 15061
		void AddDispatcherEventService(IEventService eventService);

		// Token: 0x06003AD6 RID: 15062
		void RemoveDispatcherEventService(IEventService eventService);
	}
}
