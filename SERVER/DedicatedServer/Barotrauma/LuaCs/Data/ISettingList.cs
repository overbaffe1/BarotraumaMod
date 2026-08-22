using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047A RID: 1146
	public interface ISettingList<T> : ISettingBase<T>, ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06003D9A RID: 15770
		bool TrySetValueByIndex(int index);

		// Token: 0x17001033 RID: 4147
		// (get) Token: 0x06003D9B RID: 15771
		IReadOnlyList<T> Options { get; }

		// Token: 0x17001034 RID: 4148
		// (get) Token: 0x06003D9C RID: 15772
		IReadOnlyList<string> StringOptions { get; }
	}
}
