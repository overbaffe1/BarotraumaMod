using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000554 RID: 1364
	internal interface IEventWifiSignalTransmitted : IEvent<IEventWifiSignalTransmitted>, IEvent
	{
		// Token: 0x060055B4 RID: 21940
		bool? OnWifiSignalTransmitted(WifiComponent wifiComponent, Signal signal, bool sentFromChat);

		// Token: 0x060055B5 RID: 21941 RVA: 0x002D1456 File Offset: 0x002CF656
		private static IEventWifiSignalTransmitted GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventWifiSignalTransmitted.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136D RID: 4973
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventWifiSignalTransmitted, IEvent<IEventWifiSignalTransmitted>, IEvent
		{
			// Token: 0x06009775 RID: 38773 RVA: 0x003DB89B File Offset: 0x003D9A9B
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009776 RID: 38774 RVA: 0x003DB8A4 File Offset: 0x003D9AA4
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
