using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059D RID: 1437
	public interface IPackageManagementServiceConfig : IService, IDisposable
	{
		// Token: 0x170015B6 RID: 5558
		// (get) Token: 0x06005746 RID: 22342
		bool IsCsEnabled { get; }
	}
}
