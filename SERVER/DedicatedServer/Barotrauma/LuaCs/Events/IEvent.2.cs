using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000429 RID: 1065
	public interface IEvent<out T> : IEvent where T : class, IEvent<T>
	{
		// Token: 0x06003C6E RID: 15470 RVA: 0x0018CBE6 File Offset: 0x0018ADE6
		public virtual static T GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			throw new InvalidOperationException("Lua runners forbidden for  " + typeof(T).Name);
		}
	}
}
