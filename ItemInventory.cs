using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000C6 RID: 198
	internal class ItemInventory : Inventory
	{
		// Token: 0x060019E0 RID: 6624 RVA: 0x00106190 File Offset: 0x00104390
		protected override void ControlInput(Camera cam)
		{
			base.ControlInput(cam);
			cam.OffsetAmount = 0f;
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x001061A4 File Offset: 0x001043A4
		protected override void CalculateBackgroundFrame()
		{
			VisualSlot firstSlot = this.visualSlots.FirstOrDefault<VisualSlot>();
			if (firstSlot == null)
			{
				return;
			}
			Rectangle frame = firstSlot.Rect;
			frame.Location += firstSlot.DrawOffset.ToPoint();
			for (int i = 1; i < this.capacity; i++)
			{
				Rectangle slotRect = this.visualSlots[i].Rect;
				slotRect.Location += this.visualSlots[i].DrawOffset.ToPoint();
				frame = Rectangle.Union(frame, slotRect);
			}
			base.BackgroundFrame = new Rectangle(frame.X - (int)this.padding.X, frame.Y - (int)this.padding.Y, frame.Width + (int)(this.padding.X + this.padding.Z), frame.Height + (int)(this.padding.Y + this.padding.W));
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x001062A0 File Offset: 0x001044A0
		public override void Draw(SpriteBatch spriteBatch, bool subInventory = false)
		{
			if (this.visualSlots != null && this.visualSlots.Length != 0)
			{
				this.CalculateBackgroundFrame();
				if (this.container.InventoryBackSprite == null)
				{
					if (this.RectTransform == null)
					{
						GUI.DrawRectangle(spriteBatch, base.BackgroundFrame, Color.Black * 0.8f, true, 0f, 1f);
					}
				}
				else
				{
					this.container.InventoryBackSprite.Draw(spriteBatch, base.BackgroundFrame.Location.ToVector2(), Color.White, Vector2.Zero, 0f, new Vector2((float)base.BackgroundFrame.Width / this.container.InventoryBackSprite.size.X, (float)base.BackgroundFrame.Height / this.container.InventoryBackSprite.size.Y), SpriteEffects.None, null);
				}
				base.Draw(spriteBatch, subInventory);
				if (this.container.InventoryBottomSprite != null && !subInventory)
				{
					this.container.InventoryBottomSprite.Draw(spriteBatch, new Vector2((float)base.BackgroundFrame.Center.X, (float)base.BackgroundFrame.Bottom) + this.visualSlots[0].DrawOffset, 0f, Inventory.UIScale, SpriteEffects.None);
				}
				if (this.container.InventoryTopSprite != null && !subInventory)
				{
					this.container.InventoryTopSprite.Draw(spriteBatch, new Vector2((float)base.BackgroundFrame.Center.X, (float)base.BackgroundFrame.Y), 0f, Inventory.UIScale, SpriteEffects.None);
					return;
				}
			}
			else
			{
				base.Draw(spriteBatch, subInventory);
			}
		}

		// Token: 0x060019E3 RID: 6627 RVA: 0x0010645E File Offset: 0x0010465E
		public void ClientEventWrite(IWriteMessage msg, Item.InventoryStateEventData extraData)
		{
			base.SharedWrite(msg, extraData.SlotRange);
			this.syncItemsDelay = 1f;
		}

		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x060019E4 RID: 6628 RVA: 0x00106478 File Offset: 0x00104678
		public ItemContainer Container
		{
			get
			{
				return this.container;
			}
		}

		// Token: 0x060019E5 RID: 6629 RVA: 0x00106480 File Offset: 0x00104680
		public ItemInventory(Item owner, ItemContainer container, int capacity, int slotsPerRow = 5) : base(owner, capacity, slotsPerRow)
		{
			this.container = container;
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x00106494 File Offset: 0x00104694
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

		// Token: 0x060019E7 RID: 6631 RVA: 0x00106514 File Offset: 0x00104714
		public override bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return !this.ItemOwnsSelf(item) && i >= 0 && i < this.slots.Length && this.container.CanBeContained(item, i) && (item != null && this.slots[i].CanBePut(item, ignoreCondition)) && this.slots[i].Items.Count < this.container.GetMaxStackSize(i);
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x00106584 File Offset: 0x00104784
		public override bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition, int? quality = null)
		{
			return i >= 0 && i < this.slots.Length && this.container.CanBeContained(itemPrefab, i) && (itemPrefab != null && this.slots[i].CanProbablyBePut(itemPrefab, condition, quality)) && this.slots[i].Items.Count < this.container.GetMaxStackSize(i);
		}

		// Token: 0x060019E9 RID: 6633 RVA: 0x001065EC File Offset: 0x001047EC
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

		// Token: 0x060019EA RID: 6634 RVA: 0x00106650 File Offset: 0x00104850
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

		// Token: 0x060019EB RID: 6635 RVA: 0x001066E8 File Offset: 0x001048E8
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
			}
			return wasPut;
		}

		// Token: 0x060019EC RID: 6636 RVA: 0x00106778 File Offset: 0x00104978
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
			}
			return wasPut;
		}

		// Token: 0x060019ED RID: 6637 RVA: 0x00106818 File Offset: 0x00104A18
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

		// Token: 0x060019EE RID: 6638 RVA: 0x001069A2 File Offset: 0x00104BA2
		public override void RemoveItem(Item item)
		{
			base.RemoveItem(item);
			this.container.OnItemRemoved(item);
		}

		// Token: 0x04000D45 RID: 3397
		private readonly ItemContainer container;
	}
}
