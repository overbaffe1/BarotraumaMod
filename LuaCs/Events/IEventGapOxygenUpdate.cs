using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054D RID: 1357
	internal interface IEventGapOxygenUpdate : IEvent<IEventGapOxygenUpdate>, IEvent
	{
		// Token: 0x060055A6 RID: 21926
		bool? OnGapOxygenUpdate(Gap gap, Hull hull1, Hull hull2);

		// Token: 0x060055A7 RID: 21927 RVA: 0x002D141E File Offset: 0x002CF61E
		private static IEventGapOxygenUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventGapOxygenUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02001366 RID: 4966
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventGapOxygenUpdate, IEvent<IEventGapOxygenUpdate>, IEvent
		{
			// Token: 0x06009767 RID: 38759 RVA: 0x003DB59A File Offset: 0x003D979A
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009768 RID: 38760 RVA: 0x003DB5A4 File Offset: 0x003D97A4
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
