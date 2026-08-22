using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using FarseerPhysics.Dynamics;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000548 RID: 1352
	internal interface IEventMeleeWeaponHandleImpact : IEvent<IEventMeleeWeaponHandleImpact>, IEvent
	{
		// Token: 0x0600559C RID: 21916
		void OnMeleeWeaponHandleImpact(MeleeWeapon meleeWeapon, Body target);

		// Token: 0x0600559D RID: 21917 RVA: 0x002D13F6 File Offset: 0x002CF5F6
		private static IEventMeleeWeaponHandleImpact GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventMeleeWeaponHandleImpact.LuaWrapper(luaFunc);
		}

		// Token: 0x02001361 RID: 4961
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventMeleeWeaponHandleImpact, IEvent<IEventMeleeWeaponHandleImpact>, IEvent
		{
			// Token: 0x0600975D RID: 38749 RVA: 0x003DB3D2 File Offset: 0x003D95D2
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600975E RID: 38750 RVA: 0x003DB3DB File Offset: 0x003D95DB
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
