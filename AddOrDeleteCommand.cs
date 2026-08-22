using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000146 RID: 326
	internal class AddOrDeleteCommand : Command
	{
		// Token: 0x060029C6 RID: 10694 RVA: 0x001CF06C File Offset: 0x001CD26C
		public AddOrDeleteCommand(List<MapEntity> receivers, bool wasDeleted, bool handleInventoryBehavior = true)
		{
			this.WasDeleted = wasDeleted;
			this.Receivers = new List<MapEntity>(receivers);
			try
			{
				foreach (MapEntity receiver in receivers)
				{
					Item it = receiver as Item;
					if (it != null && it.ParentInventory != null)
					{
						this.PreviousInventories.Add(new InventorySlotItem(it.ParentInventory.FindIndex(it), it), it.ParentInventory);
					}
				}
				List<MapEntity> clonedTargets = MapEntity.Clone(receivers);
				List<MapEntity> itemsToDelete = new List<MapEntity>();
				foreach (MapEntity receiver2 in this.Receivers)
				{
					Item it2 = receiver2 as Item;
					if (it2 != null)
					{
						foreach (CircuitBox cb in it2.GetComponents<CircuitBox>())
						{
							this.CircuitBoxData.Add(cb.Save(new XElement("root")));
						}
						foreach (ItemContainer component in it2.GetComponents<ItemContainer>())
						{
							if (component.Inventory != null)
							{
								itemsToDelete.AddRange(from item in component.Inventory.AllItems
								where !item.Removed
								select item);
							}
						}
					}
				}
				if (itemsToDelete.Any<MapEntity>() && handleInventoryBehavior)
				{
					this.ContainedItemsCommand.Add(new AddOrDeleteCommand(itemsToDelete, wasDeleted, true));
					if (wasDeleted)
					{
						foreach (MapEntity item2 in itemsToDelete)
						{
							if (item2 != null && !item2.Removed)
							{
								item2.Remove();
							}
						}
					}
				}
				foreach (MapEntity clone in clonedTargets)
				{
					clone.ShallowRemove();
					Item it3 = clone as Item;
					if (it3 != null)
					{
						foreach (ItemContainer container in it3.GetComponents<ItemContainer>())
						{
							ItemInventory inventory = container.Inventory;
							if (inventory != null)
							{
								inventory.DeleteAllItems();
							}
						}
					}
				}
				this.CloneList = clonedTargets;
			}
			catch (Exception e)
			{
				this.Receivers = new List<MapEntity>();
				this.CloneList = new List<MapEntity>();
				DebugConsole.ThrowError("Could not store object", e, null, false, false);
			}
		}

		// Token: 0x060029C7 RID: 10695 RVA: 0x001CF408 File Offset: 0x001CD608
		public override void Execute()
		{
			this.Process(true);
		}

		// Token: 0x060029C8 RID: 10696 RVA: 0x001CF411 File Offset: 0x001CD611
		public override void UnExecute()
		{
			this.Process(false);
		}

		// Token: 0x060029C9 RID: 10697 RVA: 0x001CF41C File Offset: 0x001CD61C
		private void Process(bool redo)
		{
			ImmutableArray<Item> items = this.DeleteUndelete(redo);
			foreach (AddOrDeleteCommand cmd in this.ContainedItemsCommand)
			{
				cmd.Process(redo);
			}
			this.ApplyCircuitBoxDataIfAny(items);
		}

		// Token: 0x060029CA RID: 10698 RVA: 0x001CF480 File Offset: 0x001CD680
		private void ApplyCircuitBoxDataIfAny(ImmutableArray<Item> items)
		{
			int cbIndex = 0;
			foreach (Item newItem in items)
			{
				foreach (ItemComponent component in newItem.Components)
				{
					CircuitBox cb = component as CircuitBox;
					if (cb != null)
					{
						if (cbIndex < 0 || cbIndex >= this.CircuitBoxData.Count)
						{
							DebugConsole.ThrowError("Unable to restore wiring in circuit box, index out of range.", null, null, false, false);
						}
						else
						{
							XElement cbData = this.CircuitBoxData[cbIndex];
							cbIndex++;
							cb.LoadFromXML(new ContentXElement(null, cbData));
						}
					}
				}
			}
		}

		// Token: 0x060029CB RID: 10699 RVA: 0x001CF540 File Offset: 0x001CD740
		public override void Cleanup()
		{
			foreach (MapEntity entity in this.CloneList)
			{
				if (!entity.Removed)
				{
					entity.Remove();
				}
			}
			List<MapEntity> cloneList = this.CloneList;
			if (cloneList != null)
			{
				cloneList.Clear();
			}
			this.Receivers.Clear();
			Dictionary<InventorySlotItem, Inventory> previousInventories = this.PreviousInventories;
			if (previousInventories != null)
			{
				previousInventories.Clear();
			}
			List<AddOrDeleteCommand> containedItemsCommand = this.ContainedItemsCommand;
			if (containedItemsCommand != null)
			{
				containedItemsCommand.ForEach(delegate(AddOrDeleteCommand cmd)
				{
					cmd.Cleanup();
				});
			}
			this.CircuitBoxData.Clear();
		}

		// Token: 0x060029CC RID: 10700 RVA: 0x001CF604 File Offset: 0x001CD804
		private ImmutableArray<Item> DeleteUndelete(bool redo)
		{
			bool wasDeleted = this.WasDeleted;
			if (redo)
			{
				wasDeleted = !wasDeleted;
			}
			ImmutableArray<Item>.Builder builder = ImmutableArray.CreateBuilder<Item>();
			if (wasDeleted)
			{
				List<MapEntity> clones = MapEntity.Clone(this.CloneList);
				int length = Math.Min(this.Receivers.Count, clones.Count);
				for (int i = 0; i < length; i++)
				{
					MapEntity clone = clones[i];
					MapEntity receiver = this.Receivers[i];
					Item item = receiver.GetReplacementOrThis() as Item;
					if (item != null)
					{
						Item cloneItem = clone as Item;
						if (cloneItem != null)
						{
							builder.Add(cloneItem);
							foreach (ItemComponent ic in item.Components)
							{
								int index = item.GetComponentIndex(ic);
								ItemComponent component = cloneItem.Components.ElementAtOrDefault(index);
								ItemComponent itemComponent = component;
								if (itemComponent != null)
								{
									ItemContainer newContainer = itemComponent as ItemContainer;
									if (newContainer != null)
									{
										ItemInventory inventory2 = newContainer.Inventory;
										if (inventory2 != null)
										{
											ItemContainer itemContainer = ic as ItemContainer;
											if (itemContainer != null && itemContainer.Inventory != null)
											{
												itemContainer.Inventory.GetReplacementOrThis().ReplacedBy = newContainer.Inventory;
											}
										}
									}
									ic.GetReplacementOrThis().ReplacedBy = component;
								}
							}
						}
					}
					receiver.GetReplacementOrThis().ReplacedBy = clone;
				}
				for (int j = 0; j < length; j++)
				{
					MapEntity clone2 = clones[j];
					MapEntity receiver2 = this.Receivers[j];
					Item it = clone2 as Item;
					if (it != null)
					{
						foreach (KeyValuePair<InventorySlotItem, Inventory> keyValuePair in this.PreviousInventories)
						{
							InventorySlotItem inventorySlotItem;
							Inventory inventory3;
							keyValuePair.Deconstruct(out inventorySlotItem, out inventory3);
							InventorySlotItem slotRef = inventorySlotItem;
							Inventory inventory = inventory3;
							if (slotRef.Item == receiver2)
							{
								inventory.GetReplacementOrThis().TryPutItem(it, slotRef.Slot, false, false, null, false, false, true);
							}
						}
					}
				}
				foreach (MapEntity clone3 in clones)
				{
					clone3.Submarine = Submarine.MainSub;
				}
				return builder.ToImmutable();
			}
			foreach (MapEntity t in this.Receivers)
			{
				MapEntity receiver3 = t.GetReplacementOrThis();
				if (!receiver3.Removed)
				{
					receiver3.Remove();
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x060029CD RID: 10701 RVA: 0x001CF8D4 File Offset: 0x001CDAD4
		public void MergeInto(AddOrDeleteCommand master)
		{
			master.Receivers.AddRange(this.Receivers);
			master.CloneList.AddRange(this.CloneList);
			master.ContainedItemsCommand.AddRange(this.ContainedItemsCommand);
			foreach (KeyValuePair<InventorySlotItem, Inventory> keyValuePair in this.PreviousInventories)
			{
				InventorySlotItem inventorySlotItem;
				Inventory inventory;
				keyValuePair.Deconstruct(out inventorySlotItem, out inventory);
				InventorySlotItem slot = inventorySlotItem;
				Inventory item = inventory;
				master.PreviousInventories.Add(slot, item);
			}
		}

		// Token: 0x060029CE RID: 10702 RVA: 0x001CF974 File Offset: 0x001CDB74
		public override LocalizedString GetDescription()
		{
			if (this.WasDeleted)
			{
				if (this.Receivers.Count <= 1)
				{
					string tag = "Undo.RemovedItem";
					string varName = "[item]";
					MapEntity mapEntity = this.Receivers.FirstOrDefault<MapEntity>();
					return TextManager.GetWithVariable(tag, varName, ((mapEntity != null) ? mapEntity.Name : null) ?? "null", FormatCapitals.No);
				}
				return TextManager.GetWithVariable("Undo.RemovedItemsMultiple", "[count]", this.Receivers.Count.ToString(), FormatCapitals.No);
			}
			else
			{
				if (this.Receivers.Count <= 1)
				{
					string tag2 = "Undo.AddedItem";
					string varName2 = "[item]";
					MapEntity mapEntity2 = this.Receivers.FirstOrDefault<MapEntity>();
					return TextManager.GetWithVariable(tag2, varName2, ((mapEntity2 != null) ? mapEntity2.Name : null) ?? "null", FormatCapitals.No);
				}
				return TextManager.GetWithVariable("Undo.AddedItemsMultiple", "[count]", this.Receivers.Count.ToString(), FormatCapitals.No);
			}
		}

		// Token: 0x040015C8 RID: 5576
		private readonly Dictionary<InventorySlotItem, Inventory> PreviousInventories = new Dictionary<InventorySlotItem, Inventory>();

		// Token: 0x040015C9 RID: 5577
		public readonly List<MapEntity> Receivers;

		// Token: 0x040015CA RID: 5578
		private readonly List<MapEntity> CloneList;

		// Token: 0x040015CB RID: 5579
		private readonly bool WasDeleted;

		// Token: 0x040015CC RID: 5580
		private readonly List<AddOrDeleteCommand> ContainedItemsCommand = new List<AddOrDeleteCommand>();

		// Token: 0x040015CD RID: 5581
		private readonly List<XElement> CircuitBoxData = new List<XElement>();
	}
}
