using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000443 RID: 1091
	internal interface IEventCharacterDeath : IEvent<IEventCharacterDeath>, IEvent
	{
		// Token: 0x06003C9D RID: 15517
		void OnCharacterDeath(Character character, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeathType);

		// Token: 0x06003C9E RID: 15518 RVA: 0x0018CCA6 File Offset: 0x0018AEA6
		private static IEventCharacterDeath GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterDeath.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D50 RID: 3408
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterDeath, IEvent<IEventCharacterDeath>, IEvent
		{
			// Token: 0x060066E7 RID: 26343 RVA: 0x0021F30B File Offset: 0x0021D50B
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066E8 RID: 26344 RVA: 0x0021F314 File Offset: 0x0021D514
			public void OnCharacterDeath(Character character, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeathType)
			{
				this.LuaFuncs["OnCharacterDeath"](new object[]
				{
					character,
					causeOfDeathAffliction,
					causeOfDeathType
				});
			}
		}
	}
}
