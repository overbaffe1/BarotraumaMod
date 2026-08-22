using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002D8 RID: 728
	[NullableContext(1)]
	[Nullable(0)]
	internal class PurchasedUpgrade
	{
		// Token: 0x06003D17 RID: 15639 RVA: 0x0022D436 File Offset: 0x0022B636
		public PurchasedUpgrade(UpgradePrefab upgradePrefab, UpgradeCategory category, int level = 1)
		{
			this.Category = category;
			this.Prefab = upgradePrefab;
			this.Level = level;
		}

		// Token: 0x06003D18 RID: 15640 RVA: 0x0022D453 File Offset: 0x0022B653
		public void Deconstruct(out UpgradePrefab prefab, out UpgradeCategory category, out int level)
		{
			prefab = this.Prefab;
			category = this.Category;
			level = this.Level;
		}

		// Token: 0x04001FA9 RID: 8105
		public readonly UpgradeCategory Category;

		// Token: 0x04001FAA RID: 8106
		public readonly UpgradePrefab Prefab;

		// Token: 0x04001FAB RID: 8107
		public int Level;
	}
}
