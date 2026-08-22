using System;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050E RID: 1294
	public interface IEventService : IReusableService, IService, IDisposable, ILuaEventService, ILuaSafeEventService, ILuaService, ILuaCsHook, ILuaPatcher, ILuaCsShim
	{
		// Token: 0x060053EC RID: 21484
		Result Subscribe<T>(T subscriber) where T : class, IEvent<T>;

		// Token: 0x060053ED RID: 21485
		void Unsubscribe<T>(T subscriber) where T : class, IEvent;

		// Token: 0x060053EE RID: 21486
		void ClearAllEventSubscribers<T>() where T : class, IEvent;

		// Token: 0x060053EF RID: 21487
		void ClearAllSubscribers();

		// Token: 0x060053F0 RID: 21488
		Result PublishEvent<T>(Action<T> action) where T : class, IEvent<T>;

		// Token: 0x060053F1 RID: 21489
		void AddDispatcherEventService(IEventService eventService);

		// Token: 0x060053F2 RID: 21490
		void RemoveDispatcherEventService(IEventService eventService);
	}
}
