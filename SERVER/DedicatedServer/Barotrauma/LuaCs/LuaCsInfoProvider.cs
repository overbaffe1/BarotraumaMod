using System;
using System.Linq;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003EC RID: 1004
	public sealed class LuaCsInfoProvider : ILuaCsInfoProvider, IService, IDisposable
	{
		// Token: 0x060039C2 RID: 14786 RVA: 0x00181540 File Offset: 0x0017F740
		public void Dispose()
		{
		}

		// Token: 0x17000FA9 RID: 4009
		// (get) Token: 0x060039C3 RID: 14787 RVA: 0x00181542 File Offset: 0x0017F742
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000FAA RID: 4010
		// (get) Token: 0x060039C4 RID: 14788 RVA: 0x00181545 File Offset: 0x0017F745
		public bool IsCsEnabled
		{
			get
			{
				return LuaCsSetup.Instance.IsCsEnabled;
			}
		}

		// Token: 0x17000FAB RID: 4011
		// (get) Token: 0x060039C5 RID: 14789 RVA: 0x00181551 File Offset: 0x0017F751
		public bool HideUserNamesInLogs
		{
			get
			{
				return LuaCsSetup.Instance.HideUserNamesInLogs;
			}
		}

		// Token: 0x17000FAC RID: 4012
		// (get) Token: 0x060039C6 RID: 14790 RVA: 0x0018155D File Offset: 0x0017F75D
		public bool UseCaching
		{
			get
			{
				return LuaCsSetup.Instance.UseCaching;
			}
		}

		// Token: 0x17000FAD RID: 4013
		// (get) Token: 0x060039C7 RID: 14791 RVA: 0x00181569 File Offset: 0x0017F769
		public RunState CurrentRunState
		{
			get
			{
				return LuaCsSetup.Instance.CurrentRunState;
			}
		}

		// Token: 0x17000FAE RID: 4014
		// (get) Token: 0x060039C8 RID: 14792 RVA: 0x00181578 File Offset: 0x0017F778
		public ContentPackage LuaCsForBarotraumaPackage
		{
			get
			{
				RegularPackage result;
				if ((result = ContentPackageManager.EnabledPackages.Regular.FirstOrDefault((RegularPackage cp) => cp.NameMatches("LuaCsForBarotrauma"), null)) == null)
				{
					if ((result = ContentPackageManager.LocalPackages.FirstOrDefault((ContentPackage cp) => cp.NameMatches("LuaCsForBarotrauma"))) == null)
					{
						result = ContentPackageManager.WorkshopPackages.FirstOrDefault((ContentPackage cp) => cp.NameMatches("LuaCsForBarotrauma"));
					}
				}
				return result;
			}
		}
	}
}
