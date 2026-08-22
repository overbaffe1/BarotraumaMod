using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054A RID: 1354
	internal interface IEventChatMessage : IEvent<IEventChatMessage>, IEvent
	{
		// Token: 0x060055A0 RID: 21920
		bool? OnChatMessage(string messageText, Client sender, ChatMessageType type, ChatMessage message);

		// Token: 0x060055A1 RID: 21921 RVA: 0x002D1406 File Offset: 0x002CF606
		private static IEventChatMessage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChatMessage.LuaWrapper(luaFunc);
		}

		// Token: 0x02001363 RID: 4963
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChatMessage, IEvent<IEventChatMessage>, IEvent
		{
			// Token: 0x06009761 RID: 38753 RVA: 0x003DB435 File Offset: 0x003D9635
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009762 RID: 38754 RVA: 0x003DB440 File Offset: 0x003D9640
			public bool? OnChatMessage(string messageText, Client sender, ChatMessageType type, ChatMessage message)
			{
				object result = this.LuaFuncs["OnChatMessage"](new object[]
				{
					messageText,
					sender,
					type,
					message
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
