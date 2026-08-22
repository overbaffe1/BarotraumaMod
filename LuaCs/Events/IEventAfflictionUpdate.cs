using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000542 RID: 1346
	internal interface IEventAfflictionUpdate : IEvent<IEventAfflictionUpdate>, IEvent
	{
		// Token: 0x06005590 RID: 21904
		void OnAfflictionUpdate(Affliction affliction, CharacterHealth characterHealth, Limb targetLimb, float deltaTime);

		// Token: 0x06005591 RID: 21905 RVA: 0x002D13C6 File Offset: 0x002CF5C6
		private static IEventAfflictionUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventAfflictionUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x0200135B RID: 4955
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventAfflictionUpdate, IEvent<IEventAfflictionUpdate>, IEvent
		{
			// Token: 0x06009751 RID: 38737 RVA: 0x003DB2AD File Offset: 0x003D94AD
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009752 RID: 38738 RVA: 0x003DB2B6 File Offset: 0x003D94B6
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
