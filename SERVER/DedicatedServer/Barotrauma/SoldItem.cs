using System;

namespace Barotrauma
{
	// Token: 0x020001D2 RID: 466
	internal class SoldItem
	{
		// Token: 0x170009D5 RID: 2517
		// (get) Token: 0x0600226E RID: 8814 RVA: 0x000E7DC8 File Offset: 0x000E5FC8
		public ItemPrefab ItemPrefab { get; }

		// Token: 0x170009D6 RID: 2518
		// (get) Token: 0x0600226F RID: 8815 RVA: 0x000E7DD0 File Offset: 0x000E5FD0
		// (set) Token: 0x06002270 RID: 8816 RVA: 0x000E7DD8 File Offset: 0x000E5FD8
		public ushort ID { get; private set; }

		// Token: 0x170009D7 RID: 2519
		// (get) Token: 0x06002271 RID: 8817 RVA: 0x000E7DE1 File Offset: 0x000E5FE1
		// (set) Token: 0x06002272 RID: 8818 RVA: 0x000E7DE9 File Offset: 0x000E5FE9
		public bool Removed { get; set; }

		// Token: 0x170009D8 RID: 2520
		// (get) Token: 0x06002273 RID: 8819 RVA: 0x000E7DF2 File Offset: 0x000E5FF2
		public byte SellerID { get; }

		// Token: 0x170009D9 RID: 2521
		// (get) Token: 0x06002274 RID: 8820 RVA: 0x000E7DFA File Offset: 0x000E5FFA
		public SoldItem.SellOrigin Origin { get; }

		// Token: 0x06002275 RID: 8821 RVA: 0x000E7E02 File Offset: 0x000E6002
		public SoldItem(ItemPrefab itemPrefab, ushort id, bool removed, byte sellerId, SoldItem.SellOrigin origin)
		{
			this.ItemPrefab = itemPrefab;
			this.ID = id;
			this.Removed = removed;
			this.SellerID = sellerId;
			this.Origin = origin;
		}

		// Token: 0x06002276 RID: 8822 RVA: 0x000E7E30 File Offset: 0x000E6030
		public void SetItemId(ushort id)
		{
			if (this.ID != 0)
			{
				DebugConsole.LogError("Error setting SoldItem.ID: ID has already been set and should not be changed.", null, null);
				return;
			}
			this.ID = id;
		}

		// Token: 0x02000985 RID: 2437
		public enum SellOrigin
		{
			// Token: 0x04003399 RID: 13209
			Character,
			// Token: 0x0400339A RID: 13210
			Submarine
		}
	}
}
