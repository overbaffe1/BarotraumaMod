using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043F RID: 1087
	internal interface IEventCanUseVoiceRadio : IEvent<IEventCanUseVoiceRadio>, IEvent
	{
		// Token: 0x06003C95 RID: 15509
		bool? OnCanUseVoiceRadio(Client sender, Client recipient);

		// Token: 0x06003C96 RID: 15510 RVA: 0x0018CC86 File Offset: 0x0018AE86
		private static IEventCanUseVoiceRadio GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCanUseVoiceRadio.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4C RID: 3404
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCanUseVoiceRadio, IEvent<IEventCanUseVoiceRadio>, IEvent
		{
			// Token: 0x060066DF RID: 26335 RVA: 0x0021F160 File Offset: 0x0021D360
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066E0 RID: 26336 RVA: 0x0021F16C File Offset: 0x0021D36C
			public bool? OnCanUseVoiceRadio(Client sender, Client recipient)
			{
				object result = this.LuaFuncs["OnCanUseVoiceRadio"](new object[]
				{
					sender,
					recipient
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
