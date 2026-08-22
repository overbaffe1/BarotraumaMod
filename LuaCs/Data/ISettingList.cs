using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000596 RID: 1430
	public interface ISettingList<T> : ISettingBase<T>, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06005700 RID: 22272
		bool TrySetValueByIndex(int index);

		// Token: 0x17001597 RID: 5527
		// (get) Token: 0x06005701 RID: 22273
		IReadOnlyList<T> Options { get; }

		// Token: 0x17001598 RID: 5528
		// (get) Token: 0x06005702 RID: 22274
		IReadOnlyList<string> StringOptions { get; }
	}
}
