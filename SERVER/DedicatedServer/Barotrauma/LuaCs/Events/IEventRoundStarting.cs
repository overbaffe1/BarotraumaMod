using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000445 RID: 1093
	public interface IEventRoundStarting : IEvent<IEventRoundStarting>, IEvent
	{
		// Token: 0x06003CA1 RID: 15521
		void OnRoundStarting();

		// Token: 0x06003CA2 RID: 15522 RVA: 0x0018CCB6 File Offset: 0x0018AEB6
		private static IEventRoundStarting GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundStarting.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D52 RID: 3410
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundStarting, IEvent<IEventRoundStarting>, IEvent
		{
			// Token: 0x060066EB RID: 26347 RVA: 0x0021F373 File Offset: 0x0021D573
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066EC RID: 26348 RVA: 0x0021F37C File Offset: 0x0021D57C
			public void OnRoundStarting()
			{
				this.LuaFuncs["OnRoundStarting"](Array.Empty<object>());
			}
		}
	}
}
