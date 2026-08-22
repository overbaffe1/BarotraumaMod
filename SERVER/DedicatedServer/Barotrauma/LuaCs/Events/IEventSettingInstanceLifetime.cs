using System;
using Barotrauma.LuaCs.Data;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200042E RID: 1070
	internal interface IEventSettingInstanceLifetime : IEvent<IEventSettingInstanceLifetime>, IEvent
	{
		// Token: 0x06003C73 RID: 15475
		void OnSettingInstanceCreated<T>(T configInstance) where T : ISettingBase;

		// Token: 0x06003C74 RID: 15476
		void OnSettingInstanceDisposed<T>(T configInstance) where T : ISettingBase;
	}
}
