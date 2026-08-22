using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000446 RID: 1094
	public interface IEventRoundStarted : IEvent<IEventRoundStarted>, IEvent
	{
		// Token: 0x06003CA3 RID: 15523
		void OnRoundStart();

		// Token: 0x06003CA4 RID: 15524 RVA: 0x0018CCBE File Offset: 0x0018AEBE
		private static IEventRoundStarted GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundStarted.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D53 RID: 3411
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundStarted, IEvent<IEventRoundStarted>, IEvent
		{
			// Token: 0x060066ED RID: 26349 RVA: 0x0021F399 File Offset: 0x0021D599
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066EE RID: 26350 RVA: 0x0021F3A2 File Offset: 0x0021D5A2
			public void OnRoundStart()
			{
				this.LuaFuncs["OnRoundStart"](Array.Empty<object>());
			}
		}
	}
}
