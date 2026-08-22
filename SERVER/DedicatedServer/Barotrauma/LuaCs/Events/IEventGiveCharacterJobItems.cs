using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000431 RID: 1073
	internal interface IEventGiveCharacterJobItems : IEvent<IEventGiveCharacterJobItems>, IEvent
	{
		// Token: 0x06003C79 RID: 15481
		void OnGiveCharacterJobItems(Character character, WayPoint spawnPoint, bool isPvPMode);

		// Token: 0x06003C7A RID: 15482 RVA: 0x0018CC16 File Offset: 0x0018AE16
		private static IEventGiveCharacterJobItems GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventGiveCharacterJobItems.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D3E RID: 3390
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventGiveCharacterJobItems, IEvent<IEventGiveCharacterJobItems>, IEvent
		{
			// Token: 0x060066C3 RID: 26307 RVA: 0x0021ECEA File Offset: 0x0021CEEA
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066C4 RID: 26308 RVA: 0x0021ECF3 File Offset: 0x0021CEF3
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
