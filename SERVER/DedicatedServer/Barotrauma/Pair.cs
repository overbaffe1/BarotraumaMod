using System;

namespace Barotrauma
{
	// Token: 0x020002D3 RID: 723
	[Obsolete("Use named tuples instead.")]
	public class Pair<T1, T2>
	{
		// Token: 0x17000DED RID: 3565
		// (get) Token: 0x060030B1 RID: 12465 RVA: 0x0014E4B3 File Offset: 0x0014C6B3
		// (set) Token: 0x060030B2 RID: 12466 RVA: 0x0014E4BB File Offset: 0x0014C6BB
		public T1 First { get; set; }

		// Token: 0x17000DEE RID: 3566
		// (get) Token: 0x060030B3 RID: 12467 RVA: 0x0014E4C4 File Offset: 0x0014C6C4
		// (set) Token: 0x060030B4 RID: 12468 RVA: 0x0014E4CC File Offset: 0x0014C6CC
		public T2 Second { get; set; }

		// Token: 0x060030B5 RID: 12469 RVA: 0x0014E4D5 File Offset: 0x0014C6D5
		public Pair(T1 first, T2 second)
		{
			this.First = first;
			this.Second = second;
		}
	}
}
