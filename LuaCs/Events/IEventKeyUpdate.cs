using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000556 RID: 1366
	public interface IEventKeyUpdate : IEvent<IEventKeyUpdate>, IEvent
	{
		// Token: 0x060055B8 RID: 21944
		void OnKeyUpdate(double deltaTime);

		// Token: 0x060055B9 RID: 21945 RVA: 0x002D1466 File Offset: 0x002CF666
		private static IEventKeyUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventKeyUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136F RID: 4975
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventKeyUpdate, IEvent<IEventKeyUpdate>, IEvent
		{
			// Token: 0x06009779 RID: 38777 RVA: 0x003DB943 File Offset: 0x003D9B43
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600977A RID: 38778 RVA: 0x003DB94C File Offset: 0x003D9B4C
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
