using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000549 RID: 1353
	internal interface IEventServerLog : IEvent<IEventServerLog>, IEvent
	{
		// Token: 0x0600559E RID: 21918
		void OnServerLog(string line, ServerLog.MessageType messageType);

		// Token: 0x0600559F RID: 21919 RVA: 0x002D13FE File Offset: 0x002CF5FE
		private static IEventServerLog GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventServerLog.LuaWrapper(luaFunc);
		}

		// Token: 0x02001362 RID: 4962
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventServerLog, IEvent<IEventServerLog>, IEvent
		{
			// Token: 0x0600975F RID: 38751 RVA: 0x003DB401 File Offset: 0x003D9601
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009760 RID: 38752 RVA: 0x003DB40A File Offset: 0x003D960A
			public void OnServerLog(string line, ServerLog.MessageType messageType)
			{
				this.LuaFuncs["OnServerLog"](new object[]
				{
					line,
					messageType
				});
			}
		}
	}
}
