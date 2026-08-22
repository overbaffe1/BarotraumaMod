using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200003B RID: 59
	internal class ItemInventory : Inventory
	{
		// Token: 0x0600086B RID: 2155 RVA: 0x0004FBEC File Offset: 0x0004DDEC
		public void ServerEventWrite(IWriteMessage msg, Client c, Item.InventoryStateEventData inventoryData)
		{
			base.SharedWrite(msg, inventoryData.SlotRange);
		}

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x0600086C RID: 2156 RVA: 0x0004FBFB File Offset: 0x0004DDFB
		public ItemContainer Container
		{
			get
			{
				return this.container;
			}
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x0004FC03 File Offset: 0x0004DE03
		public ItemInventory(Item owner, ItemContainer container, int capacity, int slotsPerRow = 5) : base(owner, capacity, slotsPerRow)
		{
			this.container = container;
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x0004FC18 File Offset: 0x0004DE18
		public override int FindAllowedSlot(Item item, bool ignoreCondition = false)
		{
			if (this.ItemOwnsSelf(item))
			{
				return -1;
			}
			if (base.Contains(item))
			{
				return -1;
			}
			if (!this.container.CanBeContained(item))
			{
				return -1;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Any() && this.CanBePutInSlot(item, i, ignoreCondition))
				{
					return i;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (this.CanBePutInSlot(item, j, ignoreCondition))
				{
					return j;
				}
			}
			return -1;
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x0004FC98 File Offset: 0x0004DE98
		public override bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return !this.ItemOwnsSelf(item) && i >= 0 && i < this.slots.Length && this.container.CanBeContained(item, i) && (item != null && this.slots[i].CanBePut(item, ignoreCondition)) && this.slots[i].Items.Count < this.container.GetMaxStackSize(i);
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x0004FD08 File Offset: 0x0004DF08
		public override bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition, int? quality = null)
		{
			return i >= 0 && i < this.slots.Length && this.container.CanBeContained(itemPrefab, i) && (itemPrefab != null && this.slots[i].CanProbablyBePut(itemPrefab, condition, quality)) && this.slots[i].Items.Count < this.container.GetMaxStackSize(i);
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x0004FD70 File Offset: 0x0004DF70
		public override int HowManyCanBePut(ItemPrefab itemPrefab, int i, float? condition, bool ignoreItemsInSlot = false)
		{
			if (itemPrefab == null)
			{
				return 0;
			}
			if (i < 0 || i >= this.slots.Length)
			{
				return 0;
			}
			if (!this.container.CanBeContained(itemPrefab, i))
			{
				return 0;
			}
			return this.slots[i].HowManyCanBePut(itemPrefab, new int?(Math.Min(itemPrefab.GetMaxStackSize(this), this.container.GetMaxStackSize(i))), condition, ignoreItemsInSlot);
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x0004FDD4 File Offset: 0x0004DFD4
		public override bool IsFull(bool takeStacksIntoAccount = false)
		{
			if (takeStacksIntoAccount)
			{
				for (int i = 0; i < this.capacity; i++)
				{
					if (!this.slots[i].Any())
					{
						return false;
					}
					Item item = this.slots[i].FirstOrDefault();
					if (this.slots[i].Items.Count < Math.Min(item.Prefab.GetMaxStackSize(this), this.container.GetMaxStackSize(i)))
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < this.capacity; j++)
				{
					if (!this.slots[j].Any())
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x0004FE6C File Offset: 0x0004E06C
		public override bool TryPutItem(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			bool wasPut = base.TryPutItem(item, user, allowedSlots, createNetworkEvent, ignoreCondition, triggerOnInsertedEffects);
			if (wasPut)
			{
				foreach (Character c in Character.CharacterList)
				{
					if (c.HeldItems.Contains(item))
					{
						item.Unequip(c);
						break;
					}
				}
				this.container.IsActive = true;
				this.container.OnItemContained(item, true);
				GameServer server = GameMain.Server;
				if (server != null)
				{
					KarmaManager karmaManager = server.KarmaManager;
					if (karmaManager != null)
					{
						karmaManager.OnItemContained(item, this.container.Item, user);
					}
				}
			}
			return wasPut;
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x0004FF28 File Offset: 0x0004E128
		public override bool TryPutItem(Item item, int i, bool allowSwapping, bool allowCombine, Character user, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			bool wasPut = base.TryPutItem(item, i, allowSwapping, allowCombine, user, createNetworkEvent, ignoreCondition, triggerOnInsertedEffects);
			if (wasPut && item.ParentInventory == this)
			{
				foreach (Character c in Character.CharacterList)
				{
					if (c.HeldItems.Contains(item))
					{
						item.Unequip(c);
						break;
					}
				}
				this.container.IsActive = true;
				this.container.OnItemContained(item, triggerOnInsertedEffects);
				GameServer server = GameMain.Server;
				if (server != null)
				{
					KarmaManager karmaManager = server.KarmaManager;
					if (karmaManager != null)
					{
						karmaManager.OnItemContained(item, this.container.Item, user);
					}
				}
			}
			return wasPut;
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x0004FFF4 File Offset: 0x0004E1F4
		protected override void CreateNetworkEvent(Range slotRange)
		{
			if (!Item.ItemList.Contains(this.container.Item))
			{
				string errorMsg = "Attempted to create a network event for an item (" + this.container.Item.Name + ") that hasn't been fully initialized yet.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("ItemInventory.CreateServerEvent:EventForUninitializedItem" + this.container.Item.Name + this.container.Item.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (!this.container.Item.Components.Contains(this.container))
			{
				string str = "Creating a network event for the item \"";
				Item item = this.container.Item;
				DebugConsole.Log(str + ((item != null) ? item.ToString() : null) + "\" failed, ItemContainer not found in components");
				return;
			}
			if (slotRange.Start.Value < 0 || slotRange.End.Value > this.capacity)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error when creating an inventory event: invalid slot range (");
				defaultInterpolatedStringHandler.AppendFormatted<Range>(slotRange);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace, null, null, false, false);
				return;
			}
			if (GameMain.NetworkMember != null)
			{
				if (GameMain.NetworkMember.IsClient)
				{
					this.syncItemsDelay = 1f;
				}
				GameMain.NetworkMember.CreateEntityEvent(this.Owner as INetSerializable, new Item.InventoryStateEventData(this.container, slotRange));
			}
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x0005017E File Offset: 0x0004E37E
		public override void RemoveItem(Item item)
		{
			base.RemoveItem(item);
			this.container.OnItemRemoved(item);
		}

		// Token: 0x040003D5 RID: 981
		private readonly ItemContainer container;
	}
}
