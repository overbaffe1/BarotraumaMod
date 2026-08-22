using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000449 RID: 1097
	public interface IEventUpdate : IEvent<IEventUpdate>, IEvent
	{
		// Token: 0x06003CA9 RID: 15529
		void OnUpdate(double fixedDeltaTime);

		// Token: 0x06003CAA RID: 15530 RVA: 0x0018CCD6 File Offset: 0x0018AED6
		private static IEventUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D56 RID: 3414
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventUpdate, IEvent<IEventUpdate>, IEvent
		{
			// Token: 0x060066F3 RID: 26355 RVA: 0x0021F410 File Offset: 0x0021D610
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066F4 RID: 26356 RVA: 0x0021F419 File Offset: 0x0021D619
			public void OnUpdate(double deltaTime)
			{
				this.LuaFuncs["OnUpdate"](new object[]
				{
					deltaTime
				});
			}
		}
	}
}
