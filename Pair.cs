using System;

namespace Barotrauma
{
	// Token: 0x0200039E RID: 926
	[Obsolete("Use named tuples instead.")]
	public class Pair<T1, T2>
	{
		// Token: 0x170011E2 RID: 4578
		// (get) Token: 0x0600452F RID: 17711 RVA: 0x00267F0F File Offset: 0x0026610F
		// (set) Token: 0x06004530 RID: 17712 RVA: 0x00267F17 File Offset: 0x00266117
		public T1 First { get; set; }

		// Token: 0x170011E3 RID: 4579
		// (get) Token: 0x06004531 RID: 17713 RVA: 0x00267F20 File Offset: 0x00266120
		// (set) Token: 0x06004532 RID: 17714 RVA: 0x00267F28 File Offset: 0x00266128
		public T2 Second { get; set; }

		// Token: 0x06004533 RID: 17715 RVA: 0x00267F31 File Offset: 0x00266131
		public Pair(T1 first, T2 second)
		{
			this.First = first;
			this.Second = second;
		}
	}
}
