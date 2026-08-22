using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000442 RID: 1090
	internal interface IEventWifiSignalTransmitted : IEvent<IEventWifiSignalTransmitted>, IEvent
	{
		// Token: 0x06003C9B RID: 15515
		bool? OnWifiSignalTransmitted(WifiComponent wifiComponent, Signal signal, bool sentFromChat);

		// Token: 0x06003C9C RID: 15516 RVA: 0x0018CC9E File Offset: 0x0018AE9E
		private static IEventWifiSignalTransmitted GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventWifiSignalTransmitted.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4F RID: 3407
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventWifiSignalTransmitted, IEvent<IEventWifiSignalTransmitted>, IEvent
		{
			// Token: 0x060066E5 RID: 26341 RVA: 0x0021F29B File Offset: 0x0021D49B
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066E6 RID: 26342 RVA: 0x0021F2A4 File Offset: 0x0021D4A4
			public bool? OnWifiSignalTransmitted(WifiComponent wifiComponent, Signal signal, bool sentFromChat)
			{
				object result = this.LuaFuncs["OnWifiSignalTransmitted"](new object[]
				{
					wifiComponent,
					signal,
					sentFromChat
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null && dynValue.Type == DataType.Boolean)
				{
					return new bool?(dynValue.Boolean);
				}
				return null;
			}
		}
	}
}
