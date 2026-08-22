using System;

namespace Barotrauma
{
	// Token: 0x02000212 RID: 530
	public interface IPerformanceData
	{
		// Token: 0x17000AC5 RID: 2757
		// (get) Token: 0x0600253F RID: 9535
		string Identifier { get; }

		// Token: 0x17000AC6 RID: 2758
		// (get) Token: 0x06002540 RID: 9536
		long ElapsedTicks { get; }
	}
}
