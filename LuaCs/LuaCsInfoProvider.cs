using System;
using System.Linq;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000501 RID: 1281
	public sealed class LuaCsInfoProvider : ILuaCsInfoProvider, IService, IDisposable
	{
		// Token: 0x060052F5 RID: 21237 RVA: 0x002C5FEC File Offset: 0x002C41EC
		public void Dispose()
		{
		}

		// Token: 0x170014F1 RID: 5361
		// (get) Token: 0x060052F6 RID: 21238 RVA: 0x002C5FEE File Offset: 0x002C41EE
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170014F2 RID: 5362
		// (get) Token: 0x060052F7 RID: 21239 RVA: 0x002C5FF1 File Offset: 0x002C41F1
		public bool IsCsEnabled
		{
			get
			{
				return LuaCsSetup.Instance.IsCsEnabled;
			}
		}

		// Token: 0x170014F3 RID: 5363
		// (get) Token: 0x060052F8 RID: 21240 RVA: 0x002C5FFD File Offset: 0x002C41FD
		public bool HideUserNamesInLogs
		{
			get
			{
				return LuaCsSetup.Instance.HideUserNamesInLogs;
			}
		}

		// Token: 0x170014F4 RID: 5364
		// (get) Token: 0x060052F9 RID: 21241 RVA: 0x002C6009 File Offset: 0x002C4209
		public bool UseCaching
		{
			get
			{
				return LuaCsSetup.Instance.UseCaching;
			}
		}

		// Token: 0x170014F5 RID: 5365
		// (get) Token: 0x060052FA RID: 21242 RVA: 0x002C6015 File Offset: 0x002C4215
		public RunState CurrentRunState
		{
			get
			{
				return LuaCsSetup.Instance.CurrentRunState;
			}
		}

		// Token: 0x170014F6 RID: 5366
		// (get) Token: 0x060052FB RID: 21243 RVA: 0x002C6024 File Offset: 0x002C4224
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
