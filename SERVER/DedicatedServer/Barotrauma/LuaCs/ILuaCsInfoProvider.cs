using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000403 RID: 1027
	public interface ILuaCsInfoProvider : IService, IDisposable
	{
		// Token: 0x17000FC1 RID: 4033
		// (get) Token: 0x06003AFA RID: 15098
		bool IsCsEnabled { get; }

		// Token: 0x17000FC2 RID: 4034
		// (get) Token: 0x06003AFB RID: 15099
		bool HideUserNamesInLogs { get; }

		// Token: 0x17000FC3 RID: 4035
		// (get) Token: 0x06003AFC RID: 15100
		bool UseCaching { get; }

		// Token: 0x17000FC4 RID: 4036
		// (get) Token: 0x06003AFD RID: 15101
		RunState CurrentRunState { get; }

		// Token: 0x17000FC5 RID: 4037
		// (get) Token: 0x06003AFE RID: 15102
		ContentPackage LuaCsForBarotraumaPackage { get; }
	}
}
