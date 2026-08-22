using System;

namespace Barotrauma
{
	// Token: 0x020001D0 RID: 464
	internal class JobVariant
	{
		// Token: 0x06003270 RID: 12912 RVA: 0x0020990A File Offset: 0x00207B0A
		public JobVariant(JobPrefab prefab, int variant)
		{
			this.Prefab = prefab;
			this.Variant = variant;
		}

		// Token: 0x04001A76 RID: 6774
		public JobPrefab Prefab;

		// Token: 0x04001A77 RID: 6775
		public int Variant;
	}
}
