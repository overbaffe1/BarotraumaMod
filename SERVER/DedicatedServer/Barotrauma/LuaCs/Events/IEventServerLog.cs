using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000437 RID: 1079
	internal interface IEventServerLog : IEvent<IEventServerLog>, IEvent
	{
		// Token: 0x06003C85 RID: 15493
		void OnServerLog(string line, ServerLog.MessageType messageType);

		// Token: 0x06003C86 RID: 15494 RVA: 0x0018CC46 File Offset: 0x0018AE46
		private static IEventServerLog GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventServerLog.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D44 RID: 3396
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventServerLog, IEvent<IEventServerLog>, IEvent
		{
			// Token: 0x060066CF RID: 26319 RVA: 0x0021EE01 File Offset: 0x0021D001
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066D0 RID: 26320 RVA: 0x0021EE0A File Offset: 0x0021D00A
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
