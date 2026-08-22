using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000566 RID: 1382
	public interface IEventServerConnected : IEvent<IEventServerConnected>, IEvent
	{
		// Token: 0x060055D8 RID: 21976
		void OnServerConnected();

		// Token: 0x060055D9 RID: 21977 RVA: 0x002D14E6 File Offset: 0x002CF6E6
		private static IEventServerConnected GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventServerConnected.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137F RID: 4991
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventServerConnected, IEvent<IEventServerConnected>, IEvent
		{
			// Token: 0x06009799 RID: 38809 RVA: 0x003DBDF2 File Offset: 0x003D9FF2
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600979A RID: 38810 RVA: 0x003DBDFB File Offset: 0x003D9FFB
			public void OnServerConnected()
			{
				this.LuaFuncs["OnServerConnected"](Array.Empty<object>());
			}
		}
	}
}
