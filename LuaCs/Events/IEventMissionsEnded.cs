using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200055A RID: 1370
	internal interface IEventMissionsEnded : IEvent<IEventMissionsEnded>, IEvent
	{
		// Token: 0x060055C0 RID: 21952
		void OnMissionsEnded(IReadOnlyList<Mission> missions);

		// Token: 0x060055C1 RID: 21953 RVA: 0x002D1486 File Offset: 0x002CF686
		private static IEventMissionsEnded GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventMissionsEnded.LuaWrapper(luaFunc);
		}

		// Token: 0x02001373 RID: 4979
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventMissionsEnded, IEvent<IEventMissionsEnded>, IEvent
		{
			// Token: 0x06009781 RID: 38785 RVA: 0x003DB9E5 File Offset: 0x003D9BE5
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009782 RID: 38786 RVA: 0x003DB9EE File Offset: 0x003D9BEE
			public void OnMissionsEnded(IReadOnlyList<Mission> missions)
			{
				this.LuaFuncs["OnMissionsEnded"](new object[]
				{
					missions
				});
			}
		}
	}
}
