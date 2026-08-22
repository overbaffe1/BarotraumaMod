using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055C RID: 1372
	public interface IEventDrawUpdate : IEvent<IEventDrawUpdate>, IEvent
	{
		// Token: 0x060055C4 RID: 21956
		void OnDrawUpdate(double deltaTime);

		// Token: 0x060055C5 RID: 21957 RVA: 0x002D1496 File Offset: 0x002CF696
		private static IEventDrawUpdate GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventDrawUpdate.LuaWrapper(luaFunc);
		}

		// Token: 0x02001375 RID: 4981
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventDrawUpdate, IEvent<IEventDrawUpdate>, IEvent
		{
			// Token: 0x06009785 RID: 38789 RVA: 0x003DBA40 File Offset: 0x003D9C40
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009786 RID: 38790 RVA: 0x003DBA49 File Offset: 0x003D9C49
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
