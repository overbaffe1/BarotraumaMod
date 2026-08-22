using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000435 RID: 1077
	internal interface IEventClientControlHusk : IEvent<IEventClientControlHusk>, IEvent
	{
		// Token: 0x06003C81 RID: 15489
		void OnClientControlHusk(Client client, Character husk);

		// Token: 0x06003C82 RID: 15490 RVA: 0x0018CC36 File Offset: 0x0018AE36
		private static IEventClientControlHusk GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventClientControlHusk.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D42 RID: 3394
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventClientControlHusk, IEvent<IEventClientControlHusk>, IEvent
		{
			// Token: 0x060066CB RID: 26315 RVA: 0x0021EDA3 File Offset: 0x0021CFA3
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066CC RID: 26316 RVA: 0x0021EDAC File Offset: 0x0021CFAC
			public void OnClientControlHusk(Client client, Character husk)
			{
				this.LuaFuncs["OnClientControlHusk"](new object[]
				{
					client,
					husk
				});
			}
		}
	}
}
