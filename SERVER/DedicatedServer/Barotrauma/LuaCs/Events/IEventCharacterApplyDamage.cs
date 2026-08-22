using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043C RID: 1084
	internal interface IEventCharacterApplyDamage : IEvent<IEventCharacterApplyDamage>, IEvent
	{
		// Token: 0x06003C8F RID: 15503
		bool? OnCharacterApplyDamage(CharacterHealth characterHealth, AttackResult attackResult, Limb hitLimb, bool allowStacking);

		// Token: 0x06003C90 RID: 15504 RVA: 0x0018CC6E File Offset: 0x0018AE6E
		private static IEventCharacterApplyDamage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterApplyDamage.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D49 RID: 3401
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterApplyDamage, IEvent<IEventCharacterApplyDamage>, IEvent
		{
			// Token: 0x060066D9 RID: 26329 RVA: 0x0021F001 File Offset: 0x0021D201
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066DA RID: 26330 RVA: 0x0021F00C File Offset: 0x0021D20C
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
