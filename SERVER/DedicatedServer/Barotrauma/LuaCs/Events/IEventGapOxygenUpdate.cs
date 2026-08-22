using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043B RID: 1083
	internal interface IEventGapOxygenUpdate : IEvent<IEventGapOxygenUpdate>, IEvent
	{
		// Token: 0x06003C8D RID: 15501
		bool? OnGapOxygenUpdate(Gap gap, Hull hull1, Hull hull2);

		// Token: 0x06003C8E RID: 15502 RVA: 0x0018CC66 File Offset: 0x0018AE66
		private static IEventGapOxygenUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventGapOxygenUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D48 RID: 3400
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventGapOxygenUpdate, IEvent<IEventGapOxygenUpdate>, IEvent
		{
			// Token: 0x060066D7 RID: 26327 RVA: 0x0021EF9A File Offset: 0x0021D19A
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066D8 RID: 26328 RVA: 0x0021EFA4 File Offset: 0x0021D1A4
			public bool? OnGapOxygenUpdate(Gap gap, Hull hull1, Hull hull2)
			{
				object result = this.LuaFuncs["OnGapOxygenUpdate"](new object[]
				{
					gap,
					hull1,
					hull2
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
