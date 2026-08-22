using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044C RID: 1100
	internal interface IEventItemCreated : IEvent<IEventItemCreated>, IEvent
	{
		// Token: 0x06003CAF RID: 15535
		void OnItemCreated(Item item);

		// Token: 0x06003CB0 RID: 15536 RVA: 0x0018CCEE File Offset: 0x0018AEEE
		private static IEventItemCreated GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemCreated.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D59 RID: 3417
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemCreated, IEvent<IEventItemCreated>, IEvent
		{
			// Token: 0x060066F9 RID: 26361 RVA: 0x0021F4A4 File Offset: 0x0021D6A4
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066FA RID: 26362 RVA: 0x0021F4AD File Offset: 0x0021D6AD
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
