using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000551 RID: 1361
	internal interface IEventCanUseVoiceRadio : IEvent<IEventCanUseVoiceRadio>, IEvent
	{
		// Token: 0x060055AE RID: 21934
		bool? OnCanUseVoiceRadio(Client sender, Client recipient);

		// Token: 0x060055AF RID: 21935 RVA: 0x002D143E File Offset: 0x002CF63E
		private static IEventCanUseVoiceRadio GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCanUseVoiceRadio.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136A RID: 4970
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCanUseVoiceRadio, IEvent<IEventCanUseVoiceRadio>, IEvent
		{
			// Token: 0x0600976F RID: 38767 RVA: 0x003DB760 File Offset: 0x003D9960
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009770 RID: 38768 RVA: 0x003DB76C File Offset: 0x003D996C
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
