using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054F RID: 1359
	internal interface IEventCharacterApplyAffliction : IEvent<IEventCharacterApplyAffliction>, IEvent
	{
		// Token: 0x060055AA RID: 21930
		bool? OnCharacterApplyAffliction(CharacterHealth characterHealth, CharacterHealth.LimbHealth limbHealth, Affliction newAffliction, bool allowStacking);

		// Token: 0x060055AB RID: 21931 RVA: 0x002D142E File Offset: 0x002CF62E
		private static IEventCharacterApplyAffliction GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterApplyAffliction.LuaWrapper(luaFunc);
		}

		// Token: 0x02001368 RID: 4968
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterApplyAffliction, IEvent<IEventCharacterApplyAffliction>, IEvent
		{
			// Token: 0x0600976B RID: 38763 RVA: 0x003DB678 File Offset: 0x003D9878
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600976C RID: 38764 RVA: 0x003DB684 File Offset: 0x003D9884
			public bool? OnCharacterApplyAffliction(CharacterHealth characterHealth, CharacterHealth.LimbHealth limbHealth, Affliction newAffliction, bool allowStacking)
			{
				object result = this.LuaFuncs["OnCharacterApplyAffliction"](new object[]
				{
					characterHealth,
					limbHealth,
					newAffliction,
					allowStacking
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
