using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000563 RID: 1379
	internal interface IEventInventoryPutItem : IEvent<IEventInventoryPutItem>, IEvent
	{
		// Token: 0x060055D2 RID: 21970
		bool? OnInventoryPutItem(Inventory inventory, Item item, Character user, int i, bool removeItem);

		// Token: 0x060055D3 RID: 21971 RVA: 0x002D14CE File Offset: 0x002CF6CE
		private static IEventInventoryPutItem GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventInventoryPutItem.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137C RID: 4988
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventInventoryPutItem, IEvent<IEventInventoryPutItem>, IEvent
		{
			// Token: 0x06009793 RID: 38803 RVA: 0x003DBC92 File Offset: 0x003D9E92
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009794 RID: 38804 RVA: 0x003DBC9C File Offset: 0x003D9E9C
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
