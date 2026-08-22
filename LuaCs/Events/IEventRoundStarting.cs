using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000557 RID: 1367
	public interface IEventRoundStarting : IEvent<IEventRoundStarting>, IEvent
	{
		// Token: 0x060055BA RID: 21946
		void OnRoundStarting();

		// Token: 0x060055BB RID: 21947 RVA: 0x002D146E File Offset: 0x002CF66E
		private static IEventRoundStarting GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundStarting.LuaWrapper(luaFunc);
		}

		// Token: 0x02001370 RID: 4976
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundStarting, IEvent<IEventRoundStarting>, IEvent
		{
			// Token: 0x0600977B RID: 38779 RVA: 0x003DB973 File Offset: 0x003D9B73
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600977C RID: 38780 RVA: 0x003DB97C File Offset: 0x003D9B7C
			public void OnRoundStarting()
			{
				this.LuaFuncs["OnRoundStarting"](Array.Empty<object>());
			}
		}
	}
}
