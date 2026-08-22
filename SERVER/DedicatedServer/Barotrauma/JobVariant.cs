using System;

namespace Barotrauma
{
	// Token: 0x020000D2 RID: 210
	internal class JobVariant
	{
		// Token: 0x06001733 RID: 5939 RVA: 0x000C250A File Offset: 0x000C070A
		public JobVariant(JobPrefab prefab, int variant)
		{
			this.Prefab = prefab;
			this.Variant = variant;
		}

		// Token: 0x04000B32 RID: 2866
		public JobPrefab Prefab;

		// Token: 0x04000B33 RID: 2867
		public int Variant;
	}
}
