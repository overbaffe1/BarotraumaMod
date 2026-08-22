using System;

namespace Barotrauma
{
	// Token: 0x020002FA RID: 762
	public interface IPerformanceData
	{
		// Token: 0x1700105D RID: 4189
		// (get) Token: 0x06003E26 RID: 15910
		string Identifier { get; }

		// Token: 0x1700105E RID: 4190
		// (get) Token: 0x06003E27 RID: 15911
		long ElapsedTicks { get; }
	}
}
