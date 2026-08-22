using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000595 RID: 1429
	public interface ISettingRangeBase<T> : ISettingBase<T>, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x17001594 RID: 5524
		// (get) Token: 0x060056FD RID: 22269
		T MinValue { get; }

		// Token: 0x17001595 RID: 5525
		// (get) Token: 0x060056FE RID: 22270
		T MaxValue { get; }

		// Token: 0x17001596 RID: 5526
		// (get) Token: 0x060056FF RID: 22271
		int IncrementalSteps { get; }
	}
}
