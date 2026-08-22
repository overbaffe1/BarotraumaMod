using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059E RID: 1438
	public class PackageManagementServiceConfig : IPackageManagementServiceConfig, IService, IDisposable
	{
		// Token: 0x06005747 RID: 22343 RVA: 0x002D3B6E File Offset: 0x002D1D6E
		public void Dispose()
		{
		}

		// Token: 0x170015B7 RID: 5559
		// (get) Token: 0x06005748 RID: 22344 RVA: 0x002D3B70 File Offset: 0x002D1D70
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170015B8 RID: 5560
		// (get) Token: 0x06005749 RID: 22345 RVA: 0x002D3B73 File Offset: 0x002D1D73
		public bool IsCsEnabled
		{
			get
			{
				return true;
			}
		}
	}
}
