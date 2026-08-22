using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000478 RID: 1144
	public interface ISettingBase<T> : ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x1700102E RID: 4142
		// (get) Token: 0x06003D94 RID: 15764
		T Value { [return: NotNull] get; }

		// Token: 0x1700102F RID: 4143
		// (get) Token: 0x06003D95 RID: 15765
		T DefaultValue { [return: NotNull] get; }

		// Token: 0x06003D96 RID: 15766
		bool TrySetValue(T value);
	}
}
