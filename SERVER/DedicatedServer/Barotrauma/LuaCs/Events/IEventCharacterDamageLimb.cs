using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000450 RID: 1104
	internal interface IEventCharacterDamageLimb : IEvent<IEventCharacterDamageLimb>, IEvent
	{
		// Token: 0x06003CB7 RID: 15543
		AttackResult? OnCharacterDamageLimb(Character character, Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker = null, float damageMultiplier = 1f, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false);

		// Token: 0x06003CB8 RID: 15544 RVA: 0x0018CD0E File Offset: 0x0018AF0E
		private static IEventCharacterDamageLimb GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterDamageLimb.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5D RID: 3421
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterDamageLimb, IEvent<IEventCharacterDamageLimb>, IEvent
		{
			// Token: 0x06006701 RID: 26369 RVA: 0x0021F5C9 File Offset: 0x0021D7C9
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06006702 RID: 26370 RVA: 0x0021F5D4 File Offset: 0x0021D7D4
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
