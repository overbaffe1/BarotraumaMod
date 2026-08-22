using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000479 RID: 1145
	public interface ISettingRangeBase<T> : ISettingBase<T>, ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x17001030 RID: 4144
		// (get) Token: 0x06003D97 RID: 15767
		T MinValue { get; }

		// Token: 0x17001031 RID: 4145
		// (get) Token: 0x06003D98 RID: 15768
		T MaxValue { get; }

		// Token: 0x17001032 RID: 4146
		// (get) Token: 0x06003D99 RID: 15769
		int IncrementalSteps { get; }
	}
}
