using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044E RID: 1102
	internal interface IEventItemUse : IEvent<IEventItemUse>, IEvent
	{
		// Token: 0x06003CB3 RID: 15539
		bool? OnItemUsed(Item item, Character user, Limb targetLimb, Entity useTarget);

		// Token: 0x06003CB4 RID: 15540 RVA: 0x0018CCFE File Offset: 0x0018AEFE
		private static IEventItemUse GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemUse.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5B RID: 3419
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemUse, IEvent<IEventItemUse>, IEvent
		{
			// Token: 0x060066FD RID: 26365 RVA: 0x0021F4FA File Offset: 0x0021D6FA
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066FE RID: 26366 RVA: 0x0021F504 File Offset: 0x0021D704
			public bool? OnItemUsed(Item item, Character user, Limb targetLimb, Entity useTarget)
			{
				object result = this.LuaFuncs["OnItemUsed"](new object[]
				{
					item,
					user,
					targetLimb,
					useTarget
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
