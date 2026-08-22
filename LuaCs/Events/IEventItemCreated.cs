using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055E RID: 1374
	internal interface IEventItemCreated : IEvent<IEventItemCreated>, IEvent
	{
		// Token: 0x060055C8 RID: 21960
		void OnItemCreated(Item item);

		// Token: 0x060055C9 RID: 21961 RVA: 0x002D14A6 File Offset: 0x002CF6A6
		private static IEventItemCreated GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemCreated.LuaWrapper(luaFunc);
		}

		// Token: 0x02001377 RID: 4983
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemCreated, IEvent<IEventItemCreated>, IEvent
		{
			// Token: 0x06009789 RID: 38793 RVA: 0x003DBAA4 File Offset: 0x003D9CA4
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600978A RID: 38794 RVA: 0x003DBAAD File Offset: 0x003D9CAD
			public void OnItemCreated(Item item)
			{
				this.LuaFuncs["OnItemCreated"](new object[]
				{
					item
				});
			}
		}
	}
}
