using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000558 RID: 1368
	public interface IEventRoundStarted : IEvent<IEventRoundStarted>, IEvent
	{
		// Token: 0x060055BC RID: 21948
		void OnRoundStart();

		// Token: 0x060055BD RID: 21949 RVA: 0x002D1476 File Offset: 0x002CF676
		private static IEventRoundStarted GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundStarted.LuaWrapper(luaFunc);
		}

		// Token: 0x02001371 RID: 4977
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundStarted, IEvent<IEventRoundStarted>, IEvent
		{
			// Token: 0x0600977D RID: 38781 RVA: 0x003DB999 File Offset: 0x003D9B99
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600977E RID: 38782 RVA: 0x003DB9A2 File Offset: 0x003D9BA2
			public void OnRoundStart()
			{
				this.LuaFuncs["OnRoundStart"](Array.Empty<object>());
			}
		}
	}
}
