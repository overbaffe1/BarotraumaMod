using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000555 RID: 1365
	internal interface IEventCharacterDeath : IEvent<IEventCharacterDeath>, IEvent
	{
		// Token: 0x060055B6 RID: 21942
		void OnCharacterDeath(Character character, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeathType);

		// Token: 0x060055B7 RID: 21943 RVA: 0x002D145E File Offset: 0x002CF65E
		private static IEventCharacterDeath GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterDeath.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136E RID: 4974
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterDeath, IEvent<IEventCharacterDeath>, IEvent
		{
			// Token: 0x06009777 RID: 38775 RVA: 0x003DB90B File Offset: 0x003D9B0B
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009778 RID: 38776 RVA: 0x003DB914 File Offset: 0x003D9B14
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
