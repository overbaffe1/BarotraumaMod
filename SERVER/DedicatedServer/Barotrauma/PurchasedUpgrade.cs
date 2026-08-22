using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001EE RID: 494
	[NullableContext(1)]
	[Nullable(0)]
	internal class PurchasedUpgrade
	{
		// Token: 0x06002371 RID: 9073 RVA: 0x000EDDC6 File Offset: 0x000EBFC6
		public PurchasedUpgrade(UpgradePrefab upgradePrefab, UpgradeCategory category, int level = 1)
		{
			this.Category = category;
			this.Prefab = upgradePrefab;
			this.Level = level;
		}

		// Token: 0x06002372 RID: 9074 RVA: 0x000EDDE3 File Offset: 0x000EBFE3
		public void Deconstruct(out UpgradePrefab prefab, out UpgradeCategory category, out int level)
		{
			prefab = this.Prefab;
			category = this.Category;
			level = this.Level;
		}

		// Token: 0x04001120 RID: 4384
		public readonly UpgradeCategory Category;

		// Token: 0x04001121 RID: 4385
		public readonly UpgradePrefab Prefab;

		// Token: 0x04001122 RID: 4386
		public int Level;
	}
}
