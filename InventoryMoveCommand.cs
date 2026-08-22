using System;

namespace Barotrauma
{
	// Token: 0x02000149 RID: 329
	internal class InventoryMoveCommand : Command
	{
		// Token: 0x060029DE RID: 10718 RVA: 0x001D018A File Offset: 0x001CE38A
		public InventoryMoveCommand(Inventory oldInventory, Inventory newInventory, Item item, int oldSlot, int newSlot)
		{
			this.newInventory = newInventory;
			this.oldInventory = oldInventory;
			this.oldSlot = oldSlot;
			this.newSlot = newSlot;
			this.targetItem = item;
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x001D01B8 File Offset: 0x001CE3B8
		public override void Execute()
		{
			Item item = this.targetItem.GetReplacementOrThis() as Item;
			if (item != null)
			{
				Inventory inventory = this.newInventory;
				if (inventory == null)
				{
					return;
				}
				inventory.GetReplacementOrThis().TryPutItem(item, this.newSlot, true, false, null, false, false, true);
			}
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x001D01FC File Offset: 0x001CE3FC
		public override void UnExecute()
		{
			Item item = this.targetItem.GetReplacementOrThis() as Item;
			if (item != null)
			{
				Inventory inventory = this.oldInventory;
				if (inventory == null)
				{
					return;
				}
				inventory.GetReplacementOrThis().TryPutItem(item, this.oldSlot, true, false, null, false, false, true);
			}
		}

		// Token: 0x060029E1 RID: 10721 RVA: 0x001D0240 File Offset: 0x001CE440
		public override void Cleanup()
		{
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x001D0242 File Offset: 0x001CE442
		public override LocalizedString GetDescription()
		{
			return TextManager.GetWithVariable("Undo.MovedItem", "[item]", this.targetItem.Name, FormatCapitals.No);
		}

		// Token: 0x040015D7 RID: 5591
		private readonly Inventory oldInventory;

		// Token: 0x040015D8 RID: 5592
		private readonly Inventory newInventory;

		// Token: 0x040015D9 RID: 5593
		private readonly int oldSlot;

		// Token: 0x040015DA RID: 5594
		private readonly int newSlot;

		// Token: 0x040015DB RID: 5595
		private readonly Item targetItem;
	}
}
