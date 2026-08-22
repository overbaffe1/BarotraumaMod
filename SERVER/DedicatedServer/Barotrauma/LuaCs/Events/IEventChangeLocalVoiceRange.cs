using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000440 RID: 1088
	internal interface IEventChangeLocalVoiceRange : IEvent<IEventChangeLocalVoiceRange>, IEvent
	{
		// Token: 0x06003C97 RID: 15511
		float? OnChangeLocalVoiceRange(Client sender, Client recipient);

		// Token: 0x06003C98 RID: 15512 RVA: 0x0018CC8E File Offset: 0x0018AE8E
		private static IEventChangeLocalVoiceRange GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventChangeLocalVoiceRange.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4D RID: 3405
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventChangeLocalVoiceRange, IEvent<IEventChangeLocalVoiceRange>, IEvent
		{
			// Token: 0x060066E1 RID: 26337 RVA: 0x0021F1C5 File Offset: 0x0021D3C5
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066E2 RID: 26338 RVA: 0x0021F1D0 File Offset: 0x0021D3D0
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
