using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using FarseerPhysics.Dynamics;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000436 RID: 1078
	internal interface IEventMeleeWeaponHandleImpact : IEvent<IEventMeleeWeaponHandleImpact>, IEvent
	{
		// Token: 0x06003C83 RID: 15491
		void OnMeleeWeaponHandleImpact(MeleeWeapon meleeWeapon, Body target);

		// Token: 0x06003C84 RID: 15492 RVA: 0x0018CC3E File Offset: 0x0018AE3E
		private static IEventMeleeWeaponHandleImpact GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventMeleeWeaponHandleImpact.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D43 RID: 3395
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventMeleeWeaponHandleImpact, IEvent<IEventMeleeWeaponHandleImpact>, IEvent
		{
			// Token: 0x060066CD RID: 26317 RVA: 0x0021EDD2 File Offset: 0x0021CFD2
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066CE RID: 26318 RVA: 0x0021EDDB File Offset: 0x0021CFDB
			public void OnMeleeWeaponHandleImpact(MeleeWeapon meleeWeapon, Body target)
			{
				this.LuaFuncs["OnMeleeWeaponHandleImpact"](new object[]
				{
					meleeWeapon,
					target
				});
			}
		}
	}
}
