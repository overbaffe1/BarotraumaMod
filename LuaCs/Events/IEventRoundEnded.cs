using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000559 RID: 1369
	public interface IEventRoundEnded : IEvent<IEventRoundEnded>, IEvent
	{
		// Token: 0x060055BE RID: 21950
		void OnRoundEnd();

		// Token: 0x060055BF RID: 21951 RVA: 0x002D147E File Offset: 0x002CF67E
		private static IEventRoundEnded GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundEnded.LuaWrapper(luaFunc);
		}

		// Token: 0x02001372 RID: 4978
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundEnded, IEvent<IEventRoundEnded>, IEvent
		{
			// Token: 0x0600977F RID: 38783 RVA: 0x003DB9BF File Offset: 0x003D9BBF
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009780 RID: 38784 RVA: 0x003DB9C8 File Offset: 0x003D9BC8
			public void OnRoundEnd()
			{
				this.LuaFuncs["OnRoundEnd"](Array.Empty<object>());
			}
		}
	}
}
