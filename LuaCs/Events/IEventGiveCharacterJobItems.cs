using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000543 RID: 1347
	internal interface IEventGiveCharacterJobItems : IEvent<IEventGiveCharacterJobItems>, IEvent
	{
		// Token: 0x06005592 RID: 21906
		void OnGiveCharacterJobItems(Character character, WayPoint spawnPoint, bool isPvPMode);

		// Token: 0x06005593 RID: 21907 RVA: 0x002D13CE File Offset: 0x002CF5CE
		private static IEventGiveCharacterJobItems GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventGiveCharacterJobItems.LuaWrapper(luaFunc);
		}

		// Token: 0x0200135C RID: 4956
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventGiveCharacterJobItems, IEvent<IEventGiveCharacterJobItems>, IEvent
		{
			// Token: 0x06009753 RID: 38739 RVA: 0x003DB2EA File Offset: 0x003D94EA
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009754 RID: 38740 RVA: 0x003DB2F3 File Offset: 0x003D94F3
			public void OnGiveCharacterJobItems(Character character, WayPoint spawnPoint, bool isPvPMode)
			{
				this.LuaFuncs["OnGiveCharacterJobItems"](new object[]
				{
					character,
					spawnPoint,
					isPvPMode
				});
			}
		}
	}
}
