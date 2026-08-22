using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000413 RID: 1043
	public enum ServiceLifetime
	{
		// Token: 0x04001D5D RID: 7517
		Transient,
		// Token: 0x04001D5E RID: 7518
		Singleton,
		// Token: 0x04001D5F RID: 7519
		PerThread,
		// Token: 0x04001D60 RID: 7520
		Invalid,
		// Token: 0x04001D61 RID: 7521
		Custom
	}
}
