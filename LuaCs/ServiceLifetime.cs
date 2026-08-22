using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000526 RID: 1318
	public enum ServiceLifetime
	{
		// Token: 0x04002C41 RID: 11329
		Transient,
		// Token: 0x04002C42 RID: 11330
		Singleton,
		// Token: 0x04002C43 RID: 11331
		PerThread,
		// Token: 0x04002C44 RID: 11332
		Invalid,
		// Token: 0x04002C45 RID: 11333
		Custom
	}
}
