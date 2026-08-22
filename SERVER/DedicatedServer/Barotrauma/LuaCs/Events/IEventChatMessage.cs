using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000438 RID: 1080
	internal interface IEventChatMessage : IEvent<IEventChatMessage>, IEvent
	{
		// Token: 0x06003C87 RID: 15495
		bool? OnChatMessage(string messageText, Client sender, ChatMessageType type, ChatMessage message);

		// Token: 0x06003C88 RID: 15496 RVA: 0x0018CC4E File Offset: 0x0018AE4E
		private static IEventChatMessage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChatMessage.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D45 RID: 3397
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChatMessage, IEvent<IEventChatMessage>, IEvent
		{
			// Token: 0x060066D1 RID: 26321 RVA: 0x0021EE35 File Offset: 0x0021D035
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066D2 RID: 26322 RVA: 0x0021EE40 File Offset: 0x0021D040
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
