using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000562 RID: 1378
	internal interface IEventCharacterDamageLimb : IEvent<IEventCharacterDamageLimb>, IEvent
	{
		// Token: 0x060055D0 RID: 21968
		AttackResult? OnCharacterDamageLimb(Character character, Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker = null, float damageMultiplier = 1f, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false);

		// Token: 0x060055D1 RID: 21969 RVA: 0x002D14C6 File Offset: 0x002CF6C6
		private static IEventCharacterDamageLimb GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterDamageLimb.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137B RID: 4987
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterDamageLimb, IEvent<IEventCharacterDamageLimb>, IEvent
		{
			// Token: 0x06009791 RID: 38801 RVA: 0x003DBBC9 File Offset: 0x003D9DC9
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009792 RID: 38802 RVA: 0x003DBBD4 File Offset: 0x003D9DD4
			public AttackResult? OnCharacterDamageLimb(Character character, Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker = null, float damageMultiplier = 1f, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false)
			{
				object result = this.LuaFuncs["OnCharacterDamageLimb"](new object[]
				{
					character,
					worldPosition,
					hitLimb,
					afflictions,
					stun,
					playSound,
					attackImpulse,
					attacker,
					damageMultiplier,
					allowStacking,
					penetration,
					shouldImplode
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null)
				{
					result = dynValue.ToObject();
				}
				if (result is AttackResult)
				{
					AttackResult attackResult = (AttackResult)result;
					return new AttackResult?(attackResult);
				}
				return null;
			}
		}
	}
}
