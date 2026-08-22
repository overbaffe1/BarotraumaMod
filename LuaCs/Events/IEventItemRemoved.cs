using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055F RID: 1375
	internal interface IEventItemRemoved : IEvent<IEventItemRemoved>, IEvent
	{
		// Token: 0x060055CA RID: 21962
		void OnItemRemoved(Item item);

		// Token: 0x060055CB RID: 21963 RVA: 0x002D14AE File Offset: 0x002CF6AE
		private static IEventItemRemoved GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemRemoved.LuaWrapper(luaFunc);
		}

		// Token: 0x02001378 RID: 4984
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemRemoved, IEvent<IEventItemRemoved>, IEvent
		{
			// Token: 0x0600978B RID: 38795 RVA: 0x003DBACF File Offset: 0x003D9CCF
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600978C RID: 38796 RVA: 0x003DBAD8 File Offset: 0x003D9CD8
			public void OnItemRemoved(Item item)
			{
				this.LuaFuncs["OnItemRemoved"](new object[]
				{
					item
				});
			}
		}
	}
}
