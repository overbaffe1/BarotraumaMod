using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000453 RID: 1107
	public interface IEventClientRawNetMessageReceived : IEvent<IEventClientRawNetMessageReceived>, IEvent
	{
		// Token: 0x06003CBD RID: 15549
		bool? OnReceivedClientNetMessage(IReadMessage netMessage, ClientPacketHeader clientPacketHeader, NetworkConnection sender);

		// Token: 0x06003CBE RID: 15550 RVA: 0x0018CD26 File Offset: 0x0018AF26
		private static IEventClientRawNetMessageReceived GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventClientRawNetMessageReceived.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D60 RID: 3424
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventClientRawNetMessageReceived, IEvent<IEventClientRawNetMessageReceived>, IEvent
		{
			// Token: 0x06006707 RID: 26375 RVA: 0x0021F789 File Offset: 0x0021D989
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06006708 RID: 26376 RVA: 0x0021F794 File Offset: 0x0021D994
			public bool? OnReceivedClientNetMessage(IReadMessage netMessage, ClientPacketHeader clientPacketHeader, NetworkConnection sender)
			{
				if (GameMain.Server == null)
				{
					return null;
				}
				Client client = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => c.Connection == sender);
				if (client == null)
				{
					return null;
				}
				object result = this.LuaFuncs["OnReceivedClientNetMessage"](new object[]
				{
					netMessage,
					clientPacketHeader,
					client
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
