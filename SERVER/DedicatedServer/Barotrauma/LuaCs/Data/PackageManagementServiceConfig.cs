using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000482 RID: 1154
	public class PackageManagementServiceConfig : IPackageManagementServiceConfig, IService, IDisposable
	{
		// Token: 0x06003DDE RID: 15838 RVA: 0x0018EB4A File Offset: 0x0018CD4A
		public void Dispose()
		{
		}

		// Token: 0x17001051 RID: 4177
		// (get) Token: 0x06003DDF RID: 15839 RVA: 0x0018EB4C File Offset: 0x0018CD4C
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17001052 RID: 4178
		// (get) Token: 0x06003DE0 RID: 15840 RVA: 0x0018EB4F File Offset: 0x0018CD4F
		public bool IsCsEnabled
		{
			get
			{
				return true;
			}
		}
	}
}
