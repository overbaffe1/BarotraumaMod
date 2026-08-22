using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055D RID: 1373
	internal interface IEventSignalReceived : IEvent<IEventSignalReceived>, IEvent
	{
		// Token: 0x060055C6 RID: 21958
		void OnSignalReceived(Signal signal, Connection connection);

		// Token: 0x060055C7 RID: 21959 RVA: 0x002D149E File Offset: 0x002CF69E
		private static IEventSignalReceived GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventSignalReceived.LuaWrapper(luaFunc);
		}

		// Token: 0x02001376 RID: 4982
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventSignalReceived, IEvent<IEventSignalReceived>, IEvent
		{
			// Token: 0x06009787 RID: 38791 RVA: 0x003DBA70 File Offset: 0x003D9C70
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009788 RID: 38792 RVA: 0x003DBA79 File Offset: 0x003D9C79
			public void OnSignalReceived(Signal signal, Connection connection)
			{
				this.LuaFuncs["OnSignalReceived"](new object[]
				{
					signal,
					connection
				});
			}
		}
	}
}
