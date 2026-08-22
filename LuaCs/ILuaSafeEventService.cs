using System;
using System.Collections.Generic;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200052C RID: 1324
	public interface ILuaSafeEventService : ILuaService, IService, IDisposable, ILuaCsHook, ILuaPatcher, IReusableService, ILuaCsShim
	{
		// Token: 0x060054A1 RID: 21665
		void Subscribe<T>(string identifier, IDictionary<string, LuaCsFunc> callbacks) where T : class, IEvent<T>;

		// Token: 0x060054A2 RID: 21666
		void Unsubscribe(string eventName, string identifier);

		// Token: 0x060054A3 RID: 21667
		void PublishLuaEvent<T>(LuaCsFunc subscriberRunner) where T : class, IEvent<T>;

		// Token: 0x060054A4 RID: 21668
		Result RegisterLuaEventAlias<T>(string luaEventName, string targetMethod) where T : class, IEvent<T>;
	}
}
