using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000552 RID: 1362
	internal interface IEventChangeLocalVoiceRange : IEvent<IEventChangeLocalVoiceRange>, IEvent
	{
		// Token: 0x060055B0 RID: 21936
		float? OnChangeLocalVoiceRange(Client sender, Client recipient);

		// Token: 0x060055B1 RID: 21937 RVA: 0x002D1446 File Offset: 0x002CF646
		private static IEventChangeLocalVoiceRange GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChangeLocalVoiceRange.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136B RID: 4971
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChangeLocalVoiceRange, IEvent<IEventChangeLocalVoiceRange>, IEvent
		{
			// Token: 0x06009771 RID: 38769 RVA: 0x003DB7C5 File Offset: 0x003D99C5
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009772 RID: 38770 RVA: 0x003DB7D0 File Offset: 0x003D99D0
			public float? OnChangeLocalVoiceRange(Client sender, Client recipient)
			{
				object result = this.LuaFuncs["OnChangeLocalVoiceRange"](new object[]
				{
					sender,
					recipient
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null && dynValue.Type == DataType.Number)
				{
					return new float?((float)dynValue.Number);
				}
				return null;
			}
		}
	}
}
