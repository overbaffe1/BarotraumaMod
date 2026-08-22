using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002D9 RID: 729
	[NullableContext(1)]
	[Nullable(0)]
	internal class PurchasedItemSwap
	{
		// Token: 0x06003D19 RID: 15641 RVA: 0x0022D46D File Offset: 0x0022B66D
		public PurchasedItemSwap(Item itemToRemove, ItemPrefab itemToInstall)
		{
			this.ItemToRemove = itemToRemove;
			this.ItemToInstall = itemToInstall;
		}

		// Token: 0x04001FAC RID: 8108
		public readonly Item ItemToRemove;

		// Token: 0x04001FAD RID: 8109
		public readonly ItemPrefab ItemToInstall;
	}
}
