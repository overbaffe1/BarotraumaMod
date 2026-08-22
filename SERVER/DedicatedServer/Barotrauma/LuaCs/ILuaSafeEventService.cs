using System;
using System.Collections.Generic;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000419 RID: 1049
	public interface ILuaSafeEventService : ILuaService, IService, IDisposable, ILuaCsHook, ILuaPatcher, IReusableService, ILuaCsShim
	{
		// Token: 0x06003B85 RID: 15237
		void Subscribe<T>(string identifier, IDictionary<string, LuaCsFunc> callbacks) where T : class, IEvent<T>;

		// Token: 0x06003B86 RID: 15238
		void Unsubscribe(string eventName, string identifier);

		// Token: 0x06003B87 RID: 15239
		void PublishLuaEvent<T>(LuaCsFunc subscriberRunner) where T : class, IEvent<T>;

		// Token: 0x06003B88 RID: 15240
		Result RegisterLuaEventAlias<T>(string luaEventName, string targetMethod) where T : class, IEvent<T>;
	}
}
