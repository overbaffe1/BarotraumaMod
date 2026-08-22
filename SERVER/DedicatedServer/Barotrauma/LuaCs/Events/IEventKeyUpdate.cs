using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000444 RID: 1092
	public interface IEventKeyUpdate : IEvent<IEventKeyUpdate>, IEvent
	{
		// Token: 0x06003C9F RID: 15519
		void OnKeyUpdate(double deltaTime);

		// Token: 0x06003CA0 RID: 15520 RVA: 0x0018CCAE File Offset: 0x0018AEAE
		private static IEventKeyUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventKeyUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D51 RID: 3409
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventKeyUpdate, IEvent<IEventKeyUpdate>, IEvent
		{
			// Token: 0x060066E9 RID: 26345 RVA: 0x0021F343 File Offset: 0x0021D543
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066EA RID: 26346 RVA: 0x0021F34C File Offset: 0x0021D54C
			public void OnKeyUpdate(double deltaTime)
			{
				this.LuaFuncs["OnKeyUpdate"](new object[]
				{
					deltaTime
				});
			}
		}
	}
}
