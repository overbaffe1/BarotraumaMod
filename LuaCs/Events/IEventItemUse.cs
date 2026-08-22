using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000560 RID: 1376
	internal interface IEventItemUse : IEvent<IEventItemUse>, IEvent
	{
		// Token: 0x060055CC RID: 21964
		bool? OnItemUsed(Item item, Character user, Limb targetLimb, Entity useTarget);

		// Token: 0x060055CD RID: 21965 RVA: 0x002D14B6 File Offset: 0x002CF6B6
		private static IEventItemUse GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemUse.LuaWrapper(luaFunc);
		}

		// Token: 0x02001379 RID: 4985
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemUse, IEvent<IEventItemUse>, IEvent
		{
			// Token: 0x0600978D RID: 38797 RVA: 0x003DBAFA File Offset: 0x003D9CFA
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600978E RID: 38798 RVA: 0x003DBB04 File Offset: 0x003D9D04
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
