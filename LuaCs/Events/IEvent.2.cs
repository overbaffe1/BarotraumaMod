using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200053C RID: 1340
	public interface IEvent<out T> : IEvent where T : class, IEvent<T>
	{
		// Token: 0x06005589 RID: 21897 RVA: 0x002D13A6 File Offset: 0x002CF5A6
		public virtual static T GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			throw new InvalidOperationException("Lua runners forbidden for  " + typeof(T).Name);
		}
	}
}
