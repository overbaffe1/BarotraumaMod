using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000454 RID: 1108
	internal interface IEventClientConnected : IEvent<IEventClientConnected>, IEvent
	{
		// Token: 0x06003CBF RID: 15551
		void OnClientConnected(Client client);

		// Token: 0x06003CC0 RID: 15552 RVA: 0x0018CD2E File Offset: 0x0018AF2E
		private static IEventClientConnected GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventClientConnected.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D61 RID: 3425
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventClientConnected, IEvent<IEventClientConnected>, IEvent
		{
			// Token: 0x06006709 RID: 26377 RVA: 0x0021F840 File Offset: 0x0021DA40
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600670A RID: 26378 RVA: 0x0021F849 File Offset: 0x0021DA49
			public void OnClientConnected(Client client)
			{
				this.LuaFuncs["OnClientConnected"](new object[]
				{
					client
				});
			}
		}
	}
}
