using System;
using System.Collections.Immutable;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000586 RID: 1414
	public interface IDependencyInfo
	{
		// Token: 0x17001575 RID: 5493
		// (get) Token: 0x060056C8 RID: 22216
		ImmutableArray<Identifier> RequiredPackages { get; }

		// Token: 0x17001576 RID: 5494
		// (get) Token: 0x060056C9 RID: 22217
		ImmutableArray<Identifier> IncompatiblePackages { get; }
	}
}
