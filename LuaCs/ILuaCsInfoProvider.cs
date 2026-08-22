using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000516 RID: 1302
	public interface ILuaCsInfoProvider : IService, IDisposable
	{
		// Token: 0x17001508 RID: 5384
		// (get) Token: 0x06005416 RID: 21526
		bool IsCsEnabled { get; }

		// Token: 0x17001509 RID: 5385
		// (get) Token: 0x06005417 RID: 21527
		bool HideUserNamesInLogs { get; }

		// Token: 0x1700150A RID: 5386
		// (get) Token: 0x06005418 RID: 21528
		bool UseCaching { get; }

		// Token: 0x1700150B RID: 5387
		// (get) Token: 0x06005419 RID: 21529
		RunState CurrentRunState { get; }

		// Token: 0x1700150C RID: 5388
		// (get) Token: 0x0600541A RID: 21530
		ContentPackage LuaCsForBarotraumaPackage { get; }
	}
}
