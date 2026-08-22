using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000547 RID: 1351
	internal interface IEventClientControlHusk : IEvent<IEventClientControlHusk>, IEvent
	{
		// Token: 0x0600559A RID: 21914
		void OnClientControlHusk(Client client, Character husk);

		// Token: 0x0600559B RID: 21915 RVA: 0x002D13EE File Offset: 0x002CF5EE
		private static IEventClientControlHusk GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventClientControlHusk.LuaWrapper(luaFunc);
		}

		// Token: 0x02001360 RID: 4960
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventClientControlHusk, IEvent<IEventClientControlHusk>, IEvent
		{
			// Token: 0x0600975B RID: 38747 RVA: 0x003DB3A3 File Offset: 0x003D95A3
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600975C RID: 38748 RVA: 0x003DB3AC File Offset: 0x003D95AC
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
