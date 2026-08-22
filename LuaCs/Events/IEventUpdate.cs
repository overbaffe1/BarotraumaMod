using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055B RID: 1371
	public interface IEventUpdate : IEvent<IEventUpdate>, IEvent
	{
		// Token: 0x060055C2 RID: 21954
		void OnUpdate(double fixedDeltaTime);

		// Token: 0x060055C3 RID: 21955 RVA: 0x002D148E File Offset: 0x002CF68E
		private static IEventUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02001374 RID: 4980
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventUpdate, IEvent<IEventUpdate>, IEvent
		{
			// Token: 0x06009783 RID: 38787 RVA: 0x003DBA10 File Offset: 0x003D9C10
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009784 RID: 38788 RVA: 0x003DBA19 File Offset: 0x003D9C19
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
