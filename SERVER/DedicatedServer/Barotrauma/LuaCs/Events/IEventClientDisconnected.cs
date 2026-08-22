using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000455 RID: 1109
	internal interface IEventClientDisconnected : IEvent<IEventClientDisconnected>, IEvent
	{
		// Token: 0x06003CC1 RID: 15553
		void OnClientDisconnected(Client client);

		// Token: 0x06003CC2 RID: 15554 RVA: 0x0018CD36 File Offset: 0x0018AF36
		private static IEventClientDisconnected GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventClientDisconnected.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D62 RID: 3426
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventClientDisconnected, IEvent<IEventClientDisconnected>, IEvent
		{
			// Token: 0x0600670B RID: 26379 RVA: 0x0021F86B File Offset: 0x0021DA6B
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600670C RID: 26380 RVA: 0x0021F874 File Offset: 0x0021DA74
			public void OnClientDisconnected(Client client)
			{
				this.LuaFuncs["OnClientDisconnected"](new object[]
				{
					client
				});
			}
		}
	}
}
