using System;

namespace Barotrauma
{
	// Token: 0x02000213 RID: 531
	public class SimplePerformanceData : IPerformanceData
	{
		// Token: 0x17000AC7 RID: 2759
		// (get) Token: 0x06002541 RID: 9537 RVA: 0x000F5105 File Offset: 0x000F3305
		public string Identifier { get; }

		// Token: 0x17000AC8 RID: 2760
		// (get) Token: 0x06002542 RID: 9538 RVA: 0x000F510D File Offset: 0x000F330D
		public long ElapsedTicks { get; }

		// Token: 0x06002543 RID: 9539 RVA: 0x000F5115 File Offset: 0x000F3315
		public SimplePerformanceData(string identifier, long elapsedTicks)
		{
			this.Identifier = identifier;
			this.ElapsedTicks = elapsedTicks;
		}
	}
}
