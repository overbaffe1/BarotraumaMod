using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000467 RID: 1127
	public interface IDependencyInfo
	{
		// Token: 0x1700100B RID: 4107
		// (get) Token: 0x06003D54 RID: 15700
		ImmutableArray<Identifier> RequiredPackages { get; }

		// Token: 0x1700100C RID: 4108
		// (get) Token: 0x06003D55 RID: 15701
		ImmutableArray<Identifier> IncompatiblePackages { get; }
	}
}
