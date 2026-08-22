using System;

namespace Barotrauma
{
	// Token: 0x02000143 RID: 323
	internal readonly struct InventorySlotItem
	{
		// Token: 0x060029B9 RID: 10681 RVA: 0x001CEE4E File Offset: 0x001CD04E
		public InventorySlotItem(int slot, Item item)
		{
			this.Slot = slot;
			this.Item = item;
		}

		// Token: 0x060029BA RID: 10682 RVA: 0x001CEE5E File Offset: 0x001CD05E
		public void Deconstruct(out int slot, out Item item)
		{
			slot = this.Slot;
			item = this.Item;
		}

		// Token: 0x040015C2 RID: 5570
		public readonly int Slot;

		// Token: 0x040015C3 RID: 5571
		public readonly Item Item;
	}
}
