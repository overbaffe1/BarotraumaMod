using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044A RID: 1098
	public interface IEventDrawUpdate : IEvent<IEventDrawUpdate>, IEvent
	{
		// Token: 0x06003CAB RID: 15531
		void OnDrawUpdate(double deltaTime);

		// Token: 0x06003CAC RID: 15532 RVA: 0x0018CCDE File Offset: 0x0018AEDE
		private static IEventDrawUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventDrawUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D57 RID: 3415
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventDrawUpdate, IEvent<IEventDrawUpdate>, IEvent
		{
			// Token: 0x060066F5 RID: 26357 RVA: 0x0021F440 File Offset: 0x0021D640
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066F6 RID: 26358 RVA: 0x0021F449 File Offset: 0x0021D649
			public void OnDrawUpdate(double deltaTime)
			{
				this.LuaFuncs["OnDrawUpdate"](new object[]
				{
					deltaTime
				});
			}
		}
	}
}
