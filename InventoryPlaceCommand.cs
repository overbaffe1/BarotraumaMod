using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000147 RID: 327
	internal class InventoryPlaceCommand : Command
	{
		// Token: 0x060029CF RID: 10703 RVA: 0x001CFA64 File Offset: 0x001CDC64
		public InventoryPlaceCommand(Inventory inventory, List<Item> items, bool dropped)
		{
			this.Inventory = inventory;
			this.Receivers = (from item in items
			select new InventorySlotItem(inventory.FindIndex(item), item)).ToList<InventorySlotItem>();
			this.wasDropped = dropped;
		}

		// Token: 0x060029D0 RID: 10704 RVA: 0x001CFAB4 File Offset: 0x001CDCB4
		public override void Execute()
		{
			this.ContainUncontain(false);
		}

		// Token: 0x060029D1 RID: 10705 RVA: 0x001CFABD File Offset: 0x001CDCBD
		public override void UnExecute()
		{
			this.ContainUncontain(true);
		}

		// Token: 0x060029D2 RID: 10706 RVA: 0x001CFAC6 File Offset: 0x001CDCC6
		public override void Cleanup()
		{
			this.Receivers.Clear();
		}

		// Token: 0x060029D3 RID: 10707 RVA: 0x001CFAD4 File Offset: 0x001CDCD4
		private void ContainUncontain(bool drop)
		{
			if (this.wasDropped)
			{
				drop = !drop;
			}
			foreach (InventorySlotItem inventorySlotItem in this.Receivers)
			{
				int num;
				Item item2;
				inventorySlotItem.Deconstruct(out num, out item2);
				int slot = num;
				Item receiver = item2;
				Item item = (Item)receiver.GetReplacementOrThis();
				if (drop)
				{
					item.Drop(null, false, true);
				}
				else
				{
					this.Inventory.GetReplacementOrThis().TryPutItem(item, slot, false, false, null, false, false, true);
				}
			}
		}

		// Token: 0x060029D4 RID: 10708 RVA: 0x001CFB78 File Offset: 0x001CDD78
		public override LocalizedString GetDescription()
		{
			if (this.wasDropped)
			{
				return TextManager.GetWithVariable("Undo.DroppedItem", "[item]", this.Receivers.FirstOrDefault<InventorySlotItem>().Item.Name, FormatCapitals.No);
			}
			string container = "[ERROR]";
			Item item = this.Inventory.Owner as Item;
			if (item != null)
			{
				container = item.Name;
			}
			if (this.Receivers.Count <= 1)
			{
				return TextManager.GetWithVariables("Undo.ContainedItem", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[item]", this.Receivers.FirstOrDefault<InventorySlotItem>().Item.Name),
					new ValueTuple<string, string>("[container]", container)
				});
			}
			return TextManager.GetWithVariables("Undo.ContainedItemsMultiple", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[count]", this.Receivers.Count.ToString()),
				new ValueTuple<string, string>("[container]", container)
			});
		}

		// Token: 0x040015CE RID: 5582
		private readonly Inventory Inventory;

		// Token: 0x040015CF RID: 5583
		private readonly List<InventorySlotItem> Receivers;

		// Token: 0x040015D0 RID: 5584
		private readonly bool wasDropped;
	}
}
