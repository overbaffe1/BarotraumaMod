using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000447 RID: 1095
	public interface IEventRoundEnded : IEvent<IEventRoundEnded>, IEvent
	{
		// Token: 0x06003CA5 RID: 15525
		void OnRoundEnd();

		// Token: 0x06003CA6 RID: 15526 RVA: 0x0018CCC6 File Offset: 0x0018AEC6
		private static IEventRoundEnded GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventRoundEnded.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D54 RID: 3412
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventRoundEnded, IEvent<IEventRoundEnded>, IEvent
		{
			// Token: 0x060066EF RID: 26351 RVA: 0x0021F3BF File Offset: 0x0021D5BF
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066F0 RID: 26352 RVA: 0x0021F3C8 File Offset: 0x0021D5C8
			public void OnRoundEnd()
			{
				this.LuaFuncs["OnRoundEnd"](Array.Empty<object>());
			}
		}
	}
}
