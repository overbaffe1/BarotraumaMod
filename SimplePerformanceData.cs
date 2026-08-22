using System;

namespace Barotrauma
{
	// Token: 0x020002FB RID: 763
	public class SimplePerformanceData : IPerformanceData
	{
		// Token: 0x1700105F RID: 4191
		// (get) Token: 0x06003E28 RID: 15912 RVA: 0x00232A92 File Offset: 0x00230C92
		public string Identifier { get; }

		// Token: 0x17001060 RID: 4192
		// (get) Token: 0x06003E29 RID: 15913 RVA: 0x00232A9A File Offset: 0x00230C9A
		public long ElapsedTicks { get; }

		// Token: 0x06003E2A RID: 15914 RVA: 0x00232AA2 File Offset: 0x00230CA2
		public SimplePerformanceData(string identifier, long elapsedTicks)
		{
			this.Identifier = identifier;
			this.ElapsedTicks = elapsedTicks;
		}
	}
}
