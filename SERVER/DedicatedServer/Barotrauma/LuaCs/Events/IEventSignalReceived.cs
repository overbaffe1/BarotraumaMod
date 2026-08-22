using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044B RID: 1099
	internal interface IEventSignalReceived : IEvent<IEventSignalReceived>, IEvent
	{
		// Token: 0x06003CAD RID: 15533
		void OnSignalReceived(Signal signal, Connection connection);

		// Token: 0x06003CAE RID: 15534 RVA: 0x0018CCE6 File Offset: 0x0018AEE6
		private static IEventSignalReceived GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventSignalReceived.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D58 RID: 3416
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventSignalReceived, IEvent<IEventSignalReceived>, IEvent
		{
			// Token: 0x060066F7 RID: 26359 RVA: 0x0021F470 File Offset: 0x0021D670
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066F8 RID: 26360 RVA: 0x0021F479 File Offset: 0x0021D679
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
