using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043D RID: 1085
	internal interface IEventCharacterApplyAffliction : IEvent<IEventCharacterApplyAffliction>, IEvent
	{
		// Token: 0x06003C91 RID: 15505
		bool? OnCharacterApplyAffliction(CharacterHealth characterHealth, CharacterHealth.LimbHealth limbHealth, Affliction newAffliction, bool allowStacking);

		// Token: 0x06003C92 RID: 15506 RVA: 0x0018CC76 File Offset: 0x0018AE76
		private static IEventCharacterApplyAffliction GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterApplyAffliction.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4A RID: 3402
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterApplyAffliction, IEvent<IEventCharacterApplyAffliction>, IEvent
		{
			// Token: 0x060066DB RID: 26331 RVA: 0x0021F078 File Offset: 0x0021D278
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066DC RID: 26332 RVA: 0x0021F084 File Offset: 0x0021D284
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
