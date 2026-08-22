using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044D RID: 1101
	internal interface IEventItemRemoved : IEvent<IEventItemRemoved>, IEvent
	{
		// Token: 0x06003CB1 RID: 15537
		void OnItemRemoved(Item item);

		// Token: 0x06003CB2 RID: 15538 RVA: 0x0018CCF6 File Offset: 0x0018AEF6
		private static IEventItemRemoved GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemRemoved.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5A RID: 3418
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemRemoved, IEvent<IEventItemRemoved>, IEvent
		{
			// Token: 0x060066FB RID: 26363 RVA: 0x0021F4CF File Offset: 0x0021D6CF
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066FC RID: 26364 RVA: 0x0021F4D8 File Offset: 0x0021D6D8
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
