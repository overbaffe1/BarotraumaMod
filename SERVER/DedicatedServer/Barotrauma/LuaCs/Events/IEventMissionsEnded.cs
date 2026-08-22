using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000448 RID: 1096
	internal interface IEventMissionsEnded : IEvent<IEventMissionsEnded>, IEvent
	{
		// Token: 0x06003CA7 RID: 15527
		void OnMissionsEnded(IReadOnlyList<Mission> missions);

		// Token: 0x06003CA8 RID: 15528 RVA: 0x0018CCCE File Offset: 0x0018AECE
		private static IEventMissionsEnded GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventMissionsEnded.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D55 RID: 3413
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventMissionsEnded, IEvent<IEventMissionsEnded>, IEvent
		{
			// Token: 0x060066F1 RID: 26353 RVA: 0x0021F3E5 File Offset: 0x0021D5E5
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066F2 RID: 26354 RVA: 0x0021F3EE File Offset: 0x0021D5EE
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
