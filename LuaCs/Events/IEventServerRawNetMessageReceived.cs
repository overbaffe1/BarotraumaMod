using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000565 RID: 1381
	public interface IEventServerRawNetMessageReceived : IEvent<IEventServerRawNetMessageReceived>, IEvent
	{
		// Token: 0x060055D6 RID: 21974
		bool? OnReceivedServerNetMessage(IReadMessage netMessage, ServerPacketHeader serverPacketHeader);

		// Token: 0x060055D7 RID: 21975 RVA: 0x002D14DE File Offset: 0x002CF6DE
		private static IEventServerRawNetMessageReceived GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventServerRawNetMessageReceived.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137E RID: 4990
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventServerRawNetMessageReceived, IEvent<IEventServerRawNetMessageReceived>, IEvent
		{
			// Token: 0x06009797 RID: 38807 RVA: 0x003DBD89 File Offset: 0x003D9F89
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009798 RID: 38808 RVA: 0x003DBD94 File Offset: 0x003D9F94
			public bool? OnReceivedServerNetMessage(IReadMessage netMessage, ServerPacketHeader serverPacketHeader)
			{
				object result = this.LuaFuncs["OnReceivedServerNetMessage"](new object[]
				{
					netMessage,
					serverPacketHeader
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
