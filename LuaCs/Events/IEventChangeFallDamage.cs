using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054C RID: 1356
	internal interface IEventChangeFallDamage : IEvent<IEventChangeFallDamage>, IEvent
	{
		// Token: 0x060055A4 RID: 21924
		float? OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity);

		// Token: 0x060055A5 RID: 21925 RVA: 0x002D1416 File Offset: 0x002CF616
		private static IEventChangeFallDamage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChangeFallDamage.LuaWrapper(luaFunc);
		}

		// Token: 0x02001365 RID: 4965
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChangeFallDamage, IEvent<IEventChangeFallDamage>, IEvent
		{
			// Token: 0x06009765 RID: 38757 RVA: 0x003DB51C File Offset: 0x003D971C
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009766 RID: 38758 RVA: 0x003DB528 File Offset: 0x003D9728
			public float? OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity)
			{
				object result = this.LuaFuncs["OnChangeFallDamage"](new object[]
				{
					impactDamage,
					character,
					impactPos,
					velocity
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null && dynValue.Type == DataType.Number)
				{
					return new float?((float)dynValue.Number);
				}
				return null;
			}
		}
	}
}
