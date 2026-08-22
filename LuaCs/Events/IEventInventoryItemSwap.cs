using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000564 RID: 1380
	internal interface IEventInventoryItemSwap : IEvent<IEventInventoryItemSwap>, IEvent
	{
		// Token: 0x060055D4 RID: 21972
		bool? OnInventoryItemSwap(Inventory inventory, Item item, Character user, int i, bool swapWholeStack);

		// Token: 0x060055D5 RID: 21973 RVA: 0x002D14D6 File Offset: 0x002CF6D6
		private static IEventInventoryItemSwap GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventInventoryItemSwap.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137D RID: 4989
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventInventoryItemSwap, IEvent<IEventInventoryItemSwap>, IEvent
		{
			// Token: 0x06009795 RID: 38805 RVA: 0x003DBD0D File Offset: 0x003D9F0D
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009796 RID: 38806 RVA: 0x003DBD18 File Offset: 0x003D9F18
			public bool? OnInventoryItemSwap(Inventory inventory, Item item, Character user, int i, bool swapWholeStack)
			{
				object result = this.LuaFuncs["OnInventoryItemSwap"](new object[]
				{
					inventory,
					item,
					user,
					i,
					swapWholeStack
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
