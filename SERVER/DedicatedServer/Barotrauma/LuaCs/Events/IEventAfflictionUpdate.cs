using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000430 RID: 1072
	internal interface IEventAfflictionUpdate : IEvent<IEventAfflictionUpdate>, IEvent
	{
		// Token: 0x06003C77 RID: 15479
		void OnAfflictionUpdate(Affliction affliction, CharacterHealth characterHealth, Limb targetLimb, float deltaTime);

		// Token: 0x06003C78 RID: 15480 RVA: 0x0018CC0E File Offset: 0x0018AE0E
		private static IEventAfflictionUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventAfflictionUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D3D RID: 3389
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventAfflictionUpdate, IEvent<IEventAfflictionUpdate>, IEvent
		{
			// Token: 0x060066C1 RID: 26305 RVA: 0x0021ECAD File Offset: 0x0021CEAD
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066C2 RID: 26306 RVA: 0x0021ECB6 File Offset: 0x0021CEB6
			public void OnAfflictionUpdate(Affliction affliction, CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
			{
				this.LuaFuncs["OnAfflictionUpdate"](new object[]
				{
					affliction,
					characterHealth,
					targetLimb,
					deltaTime
				});
			}
		}
	}
}
