using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000594 RID: 1428
	public interface ISettingBase<T> : ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x17001592 RID: 5522
		// (get) Token: 0x060056FA RID: 22266
		T Value { [return: NotNull] get; }

		// Token: 0x17001593 RID: 5523
		// (get) Token: 0x060056FB RID: 22267
		T DefaultValue { [return: NotNull] get; }

		// Token: 0x060056FC RID: 22268
		bool TrySetValue(T value);
	}
}
