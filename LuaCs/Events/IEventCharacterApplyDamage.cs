using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054E RID: 1358
	internal interface IEventCharacterApplyDamage : IEvent<IEventCharacterApplyDamage>, IEvent
	{
		// Token: 0x060055A8 RID: 21928
		bool? OnCharacterApplyDamage(CharacterHealth characterHealth, AttackResult attackResult, Limb hitLimb, bool allowStacking);

		// Token: 0x060055A9 RID: 21929 RVA: 0x002D1426 File Offset: 0x002CF626
		private static IEventCharacterApplyDamage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterApplyDamage.LuaWrapper(luaFunc);
		}

		// Token: 0x02001367 RID: 4967
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterApplyDamage, IEvent<IEventCharacterApplyDamage>, IEvent
		{
			// Token: 0x06009769 RID: 38761 RVA: 0x003DB601 File Offset: 0x003D9801
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600976A RID: 38762 RVA: 0x003DB60C File Offset: 0x003D980C
			public bool? OnCharacterApplyDamage(CharacterHealth characterHealth, AttackResult attackResult, Limb hitLimb, bool allowStacking)
			{
				object result = this.LuaFuncs["OnCharacterApplyDamage"](new object[]
				{
					characterHealth,
					attackResult,
					hitLimb,
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
