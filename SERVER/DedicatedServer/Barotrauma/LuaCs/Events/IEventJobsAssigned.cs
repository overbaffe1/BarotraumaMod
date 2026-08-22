using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000456 RID: 1110
	internal interface IEventJobsAssigned : IEvent<IEventJobsAssigned>, IEvent
	{
		// Token: 0x06003CC3 RID: 15555
		void OnJobsAssigned(IReadOnlyList<Client> unassignedClients);

		// Token: 0x06003CC4 RID: 15556 RVA: 0x0018CD3E File Offset: 0x0018AF3E
		private static IEventJobsAssigned GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventJobsAssigned.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D63 RID: 3427
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventJobsAssigned, IEvent<IEventJobsAssigned>, IEvent
		{
			// Token: 0x0600670D RID: 26381 RVA: 0x0021F896 File Offset: 0x0021DA96
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600670E RID: 26382 RVA: 0x0021F89F File Offset: 0x0021DA9F
			public void OnJobsAssigned(IReadOnlyList<Client> unassignedClients)
			{
				this.LuaFuncs["OnJobsAssigned"](new object[]
				{
					unassignedClients
				});
			}
		}
	}
}
