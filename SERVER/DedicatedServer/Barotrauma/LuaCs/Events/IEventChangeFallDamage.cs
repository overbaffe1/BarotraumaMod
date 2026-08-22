using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043A RID: 1082
	internal interface IEventChangeFallDamage : IEvent<IEventChangeFallDamage>, IEvent
	{
		// Token: 0x06003C8B RID: 15499
		float? OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity);

		// Token: 0x06003C8C RID: 15500 RVA: 0x0018CC5E File Offset: 0x0018AE5E
		private static IEventChangeFallDamage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChangeFallDamage.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D47 RID: 3399
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChangeFallDamage, IEvent<IEventChangeFallDamage>, IEvent
		{
			// Token: 0x060066D5 RID: 26325 RVA: 0x0021EF1C File Offset: 0x0021D11C
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066D6 RID: 26326 RVA: 0x0021EF28 File Offset: 0x0021D128
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
