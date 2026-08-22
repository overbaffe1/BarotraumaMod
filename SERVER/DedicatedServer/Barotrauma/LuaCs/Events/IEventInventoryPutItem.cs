using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000451 RID: 1105
	internal interface IEventInventoryPutItem : IEvent<IEventInventoryPutItem>, IEvent
	{
		// Token: 0x06003CB9 RID: 15545
		bool? OnInventoryPutItem(Inventory inventory, Item item, Character user, int i, bool removeItem);

		// Token: 0x06003CBA RID: 15546 RVA: 0x0018CD16 File Offset: 0x0018AF16
		private static IEventInventoryPutItem GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventInventoryPutItem.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5E RID: 3422
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventInventoryPutItem, IEvent<IEventInventoryPutItem>, IEvent
		{
			// Token: 0x06006703 RID: 26371 RVA: 0x0021F692 File Offset: 0x0021D892
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06006704 RID: 26372 RVA: 0x0021F69C File Offset: 0x0021D89C
			public bool? OnInventoryPutItem(Inventory inventory, Item item, Character user, int i, bool removeItem)
			{
				object result = this.LuaFuncs["OnInventoryPutItem"](new object[]
				{
					inventory,
					item,
					user,
					i,
					removeItem
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null && dynValue.Type == DataType.Boolean)
				{
					return new bool?(dynValue.Boolean);
				}
				return null;
			}
		}
	}
}
