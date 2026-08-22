using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042F RID: 1071
	internal interface IEventModifyChatMessage : IEvent<IEventModifyChatMessage>, IEvent
	{
		// Token: 0x06003C75 RID: 15477
		bool? OnModifyMessagePredicate(ChatMessage message, WifiComponent senderRadio);

		// Token: 0x06003C76 RID: 15478 RVA: 0x0018CC06 File Offset: 0x0018AE06
		private static IEventModifyChatMessage GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventModifyChatMessage.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D3C RID: 3388
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventModifyChatMessage, IEvent<IEventModifyChatMessage>, IEvent
		{
			// Token: 0x060066BF RID: 26303 RVA: 0x0021EC49 File Offset: 0x0021CE49
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066C0 RID: 26304 RVA: 0x0021EC54 File Offset: 0x0021CE54
			public bool? OnModifyMessagePredicate(ChatMessage message, WifiComponent senderRadio)
			{
				object result = this.LuaFuncs["OnModifyMessagePredicate"](new object[]
				{
					message,
					senderRadio
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
