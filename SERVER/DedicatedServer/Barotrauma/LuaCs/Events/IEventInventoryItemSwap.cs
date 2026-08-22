using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000452 RID: 1106
	internal interface IEventInventoryItemSwap : IEvent<IEventInventoryItemSwap>, IEvent
	{
		// Token: 0x06003CBB RID: 15547
		bool? OnInventoryItemSwap(Inventory inventory, Item item, Character user, int i, bool swapWholeStack);

		// Token: 0x06003CBC RID: 15548 RVA: 0x0018CD1E File Offset: 0x0018AF1E
		private static IEventInventoryItemSwap GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventInventoryItemSwap.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5F RID: 3423
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventInventoryItemSwap, IEvent<IEventInventoryItemSwap>, IEvent
		{
			// Token: 0x06006705 RID: 26373 RVA: 0x0021F70D File Offset: 0x0021D90D
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06006706 RID: 26374 RVA: 0x0021F718 File Offset: 0x0021D918
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
