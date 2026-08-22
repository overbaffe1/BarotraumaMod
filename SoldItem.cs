using System;

namespace Barotrauma
{
	// Token: 0x020002C0 RID: 704
	internal class SoldItem
	{
		// Token: 0x17000FDE RID: 4062
		// (get) Token: 0x06003C72 RID: 15474 RVA: 0x0022B120 File Offset: 0x00229320
		public ItemPrefab ItemPrefab { get; }

		// Token: 0x17000FDF RID: 4063
		// (get) Token: 0x06003C73 RID: 15475 RVA: 0x0022B128 File Offset: 0x00229328
		// (set) Token: 0x06003C74 RID: 15476 RVA: 0x0022B130 File Offset: 0x00229330
		public ushort ID { get; private set; }

		// Token: 0x17000FE0 RID: 4064
		// (get) Token: 0x06003C75 RID: 15477 RVA: 0x0022B139 File Offset: 0x00229339
		// (set) Token: 0x06003C76 RID: 15478 RVA: 0x0022B141 File Offset: 0x00229341
		public bool Removed { get; set; }

		// Token: 0x17000FE1 RID: 4065
		// (get) Token: 0x06003C77 RID: 15479 RVA: 0x0022B14A File Offset: 0x0022934A
		public byte SellerID { get; }

		// Token: 0x17000FE2 RID: 4066
		// (get) Token: 0x06003C78 RID: 15480 RVA: 0x0022B152 File Offset: 0x00229352
		public SoldItem.SellOrigin Origin { get; }

		// Token: 0x06003C79 RID: 15481 RVA: 0x0022B15A File Offset: 0x0022935A
		public SoldItem(ItemPrefab itemPrefab, ushort id, bool removed, byte sellerId, SoldItem.SellOrigin origin)
		{
			this.ItemPrefab = itemPrefab;
			this.ID = id;
			this.Removed = removed;
			this.SellerID = sellerId;
			this.Origin = origin;
		}

		// Token: 0x06003C7A RID: 15482 RVA: 0x0022B188 File Offset: 0x00229388
		public void SetItemId(ushort id)
		{
			if (this.ID != 0)
			{
				DebugConsole.LogError("Error setting SoldItem.ID: ID has already been set and should not be changed.", null, null);
				return;
			}
			this.ID = id;
		}

		// Token: 0x02000F75 RID: 3957
		public enum SellOrigin
		{
			// Token: 0x040055AF RID: 21935
			Character,
			// Token: 0x040055B0 RID: 21936
			Submarine
		}
	}
}
