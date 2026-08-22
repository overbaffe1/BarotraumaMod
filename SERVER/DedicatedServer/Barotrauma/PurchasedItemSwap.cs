using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001EF RID: 495
	[NullableContext(1)]
	[Nullable(0)]
	internal class PurchasedItemSwap
	{
		// Token: 0x06002373 RID: 9075 RVA: 0x000EDDFD File Offset: 0x000EBFFD
		public PurchasedItemSwap(Item itemToRemove, ItemPrefab itemToInstall)
		{
			this.ItemToRemove = itemToRemove;
			this.ItemToInstall = itemToInstall;
		}

		// Token: 0x04001123 RID: 4387
		public readonly Item ItemToRemove;

		// Token: 0x04001124 RID: 4388
		public readonly ItemPrefab ItemToInstall;
	}
}
