using System;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000541 RID: 1345
	internal interface IEventSettingInstanceLifetime : IEvent<IEventSettingInstanceLifetime>, IEvent
	{
		// Token: 0x0600558E RID: 21902
		void OnSettingInstanceCreated<T>(T configInstance) where T : ISettingBase;

		// Token: 0x0600558F RID: 21903
		void OnSettingInstanceDisposed<T>(T configInstance) where T : ISettingBase;
	}
}
