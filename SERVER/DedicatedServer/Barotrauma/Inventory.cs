using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000039 RID: 57
	internal class Inventory : IClientSerializable, INetSerializable
	{
		// Token: 0x06000704 RID: 1796 RVA: 0x0004432C File Offset: 0x0004252C
		public void ServerEventRead(IReadMessage msg, Client sender)
		{
			Inventory.<>c__DisplayClass1_0 CS$<>8__locals1 = new Inventory.<>c__DisplayClass1_0();
			CS$<>8__locals1.sender = sender;
			CS$<>8__locals1.<>4__this = this;
			if (!this.receivedItemIds.TryGetValue(CS$<>8__locals1.sender, out CS$<>8__locals1.receivedItemIdsFromClient))
			{
				CS$<>8__locals1.receivedItemIdsFromClient = new List<ushort>[this.capacity];
				this.receivedItemIds.Add(CS$<>8__locals1.sender, CS$<>8__locals1.receivedItemIdsFromClient);
			}
			bool readyToApply;
			this.SharedRead(msg, CS$<>8__locals1.receivedItemIdsFromClient, out readyToApply);
			if (!readyToApply)
			{
				return;
			}
			if (CS$<>8__locals1.sender == null || CS$<>8__locals1.sender.Character == null)
			{
				return;
			}
			if (!CS$<>8__locals1.<ServerEventRead>g__IsInventoryAccessible|0())
			{
				CS$<>8__locals1.<ServerEventRead>g__CreateCorrectiveNetworkEvent|1();
				return;
			}
			CS$<>8__locals1.prevItems = new List<Item>(this.AllItems.Distinct<Item>());
			CS$<>8__locals1.prevItemInventories = new List<Inventory>
			{
				this
			};
			CS$<>8__locals1.itemAccessibility = CS$<>8__locals1.<ServerEventRead>g__GetItemAccessibility|2();
			CS$<>8__locals1.<ServerEventRead>g__HandleRemovedItems|3();
			CS$<>8__locals1.<ServerEventRead>g__HandleAddedItems|4();
			this.EnsureItemsInBothHands(CS$<>8__locals1.sender.Character);
			this.receivedItemIds.Remove(CS$<>8__locals1.sender);
			this.CreateNetworkEvent();
			foreach (Inventory prevInventory in CS$<>8__locals1.prevItemInventories.Distinct<Inventory>())
			{
				if (prevInventory != this && prevInventory != null)
				{
					prevInventory.CreateNetworkEvent();
				}
			}
			CS$<>8__locals1.<ServerEventRead>g__ServerLogAddedItems|5();
			CS$<>8__locals1.<ServerEventRead>g__ServerLogRemovedItems|6();
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x00044488 File Offset: 0x00042688
		private void EnsureItemsInBothHands(Character character)
		{
			Inventory.<>c__DisplayClass2_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.character = character;
			CharacterInventory charInv = this as CharacterInventory;
			if (charInv == null)
			{
				return;
			}
			int leftHandSlot = charInv.FindLimbSlot(InvSlotType.LeftHand);
			int rightHandSlot = charInv.FindLimbSlot(InvSlotType.RightHand);
			if (this.<EnsureItemsInBothHands>g__IsSlotIndexOutOfBound|2_1(leftHandSlot, ref CS$<>8__locals1) || this.<EnsureItemsInBothHands>g__IsSlotIndexOutOfBound|2_1(rightHandSlot, ref CS$<>8__locals1))
			{
				return;
			}
			this.<EnsureItemsInBothHands>g__TryPutInOppositeHandSlot|2_0(rightHandSlot, leftHandSlot, ref CS$<>8__locals1);
			this.<EnsureItemsInBothHands>g__TryPutInOppositeHandSlot|2_0(leftHandSlot, rightHandSlot, ref CS$<>8__locals1);
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000706 RID: 1798 RVA: 0x000444EB File Offset: 0x000426EB
		// (set) Token: 0x06000707 RID: 1799 RVA: 0x000444F3 File Offset: 0x000426F3
		public int ExtraStackSize
		{
			get
			{
				return this.extraStackSize;
			}
			set
			{
				this.extraStackSize = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000708 RID: 1800 RVA: 0x00044502 File Offset: 0x00042702
		public virtual IEnumerable<Item> AllItems
		{
			get
			{
				return this.GetAllItems(this is CharacterInventory);
			}
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x00044513 File Offset: 0x00042713
		public IEnumerable<Item> AllItemsMod
		{
			get
			{
				this.allItemsList.Clear();
				this.allItemsList.AddRange(this.AllItems);
				return this.allItemsList;
			}
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x00044537 File Offset: 0x00042737
		public int Capacity
		{
			get
			{
				return this.capacity;
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x0600070B RID: 1803 RVA: 0x0004453F File Offset: 0x0004273F
		public static bool IsDragAndDropGiveAllowed
		{
			get
			{
				return GameMain.NetworkMember == null || GameMain.NetworkMember.ServerSettings.AllowDragAndDropGive;
			}
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x0600070C RID: 1804 RVA: 0x00044559 File Offset: 0x00042759
		public int EmptySlotCount
		{
			get
			{
				return this.slots.Count((Inventory.ItemSlot i) => !i.Empty());
			}
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x00044588 File Offset: 0x00042788
		public Inventory(Entity owner, int capacity, int slotsPerRow = 5)
		{
			this.capacity = capacity;
			this.Owner = owner;
			this.slots = new Inventory.ItemSlot[capacity];
			for (int i = 0; i < capacity; i++)
			{
				this.slots[i] = new Inventory.ItemSlot(this);
			}
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x000445EC File Offset: 0x000427EC
		public IEnumerable<Item> GetAllItems(bool checkForDuplicates)
		{
			Inventory.<GetAllItems>d__28 <GetAllItems>d__ = new Inventory.<GetAllItems>d__28(-2);
			<GetAllItems>d__.<>4__this = this;
			<GetAllItems>d__.<>3__checkForDuplicates = checkForDuplicates;
			return <GetAllItems>d__;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00044604 File Offset: 0x00042804
		private void NotifyItemComponentsOfChange()
		{
			Item it = this.Owner as Item;
			if (it == null)
			{
				return;
			}
			foreach (ItemComponent c in it.Components)
			{
				c.OnInventoryChanged();
			}
			Inventory parentInventory = it.ParentInventory;
			if (parentInventory == null)
			{
				return;
			}
			parentInventory.NotifyItemComponentsOfChange();
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x00044678 File Offset: 0x00042878
		public bool Contains(Item item)
		{
			return this.slots.Any((Inventory.ItemSlot i) => i.Contains(item));
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000446AC File Offset: 0x000428AC
		public Item FirstOrDefault()
		{
			foreach (Inventory.ItemSlot itemSlot in this.slots)
			{
				Item item = itemSlot.FirstOrDefault();
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x000446E0 File Offset: 0x000428E0
		public Item LastOrDefault()
		{
			for (int i = this.slots.Length - 1; i >= 0; i--)
			{
				Item item = this.slots[i].LastOrDefault();
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x00044716 File Offset: 0x00042916
		private bool IsIndexInRange(int index)
		{
			return index >= 0 && index < this.slots.Length;
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00044729 File Offset: 0x00042929
		public Item GetItemAt(int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return null;
			}
			return this.slots[index].FirstOrDefault();
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x00044743 File Offset: 0x00042943
		public IEnumerable<Item> GetItemsAt(int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return Enumerable.Empty<Item>();
			}
			return this.slots[index].Items;
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x00044761 File Offset: 0x00042961
		public int GetItemStackSlotIndex(Item item, int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return -1;
			}
			return this.slots[index].Items.IndexOf(item);
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x00044784 File Offset: 0x00042984
		public int FindIndex(Item item)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x000447B8 File Offset: 0x000429B8
		public List<int> FindIndices(Item item)
		{
			List<int> indices = new List<int>();
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					indices.Add(i);
				}
			}
			return indices;
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x000447F4 File Offset: 0x000429F4
		public virtual bool ItemOwnsSelf(Item item)
		{
			if (this.Owner == null)
			{
				return false;
			}
			if (!(this.Owner is Item))
			{
				return false;
			}
			Item ownerItem = this.Owner as Item;
			return ownerItem == item || (ownerItem.ParentInventory != null && ownerItem.ParentInventory.ItemOwnsSelf(item));
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00044844 File Offset: 0x00042A44
		public virtual int FindAllowedSlot(Item item, bool ignoreCondition = false)
		{
			if (this.ItemOwnsSelf(item))
			{
				return -1;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					return -1;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (this.slots[j].CanBePut(item, ignoreCondition))
				{
					return j;
				}
			}
			return -1;
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x000448A4 File Offset: 0x00042AA4
		public bool CanBePut(Item item)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.CanBePutInSlot(item, i, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x000448D0 File Offset: 0x00042AD0
		public virtual bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return !this.ItemOwnsSelf(item) && this.IsIndexInRange(i) && this.slots[i].CanBePut(item, ignoreCondition);
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x000448F8 File Offset: 0x00042AF8
		public bool CanProbablyBePut(ItemPrefab itemPrefab, float? condition = null, int? quality = null)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.CanBePutInSlot(itemPrefab, i, condition, quality))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x00044925 File Offset: 0x00042B25
		public virtual bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition = null, int? quality = null)
		{
			return this.IsIndexInRange(i) && this.slots[i].CanProbablyBePut(itemPrefab, condition, quality);
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x00044944 File Offset: 0x00042B44
		public int HowManyCanBePut(ItemPrefab itemPrefab, float? condition = null)
		{
			int count = 0;
			for (int i = 0; i < this.capacity; i++)
			{
				count += this.HowManyCanBePut(itemPrefab, i, condition, false);
			}
			return count;
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x00044974 File Offset: 0x00042B74
		public virtual int HowManyCanBePut(ItemPrefab itemPrefab, int i, float? condition, bool ignoreItemsInSlot = false)
		{
			if (!this.IsIndexInRange(i))
			{
				return 0;
			}
			return this.slots[i].HowManyCanBePut(itemPrefab, null, condition, ignoreItemsInSlot);
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x000449AC File Offset: 0x00042BAC
		public virtual bool TryPutItem(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			int slot = this.FindAllowedSlot(item, ignoreCondition);
			if (slot < 0)
			{
				return false;
			}
			this.PutItem(item, slot, user, true, createNetworkEvent, triggerOnInsertedEffects);
			return true;
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x000449D8 File Offset: 0x00042BD8
		public virtual bool TryPutItem(Item item, int i, bool allowSwapping, bool allowCombine, Character user, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			if (!this.IsIndexInRange(i))
			{
				string thisItemStr = ((item != null) ? item.Prefab.Identifier.Value : null) ?? "null";
				string ownerStr = "null";
				Item ownerItem = this.Owner as Item;
				if (ownerItem != null)
				{
					ownerStr = ownerItem.Prefab.Identifier.Value;
				}
				else
				{
					Character ownerCharacter = this.Owner as Character;
					if (ownerCharacter != null)
					{
						ownerStr = ownerCharacter.SpeciesName.Value;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Inventory.TryPutItem failed: index was out of range (item: ");
				defaultInterpolatedStringHandler.AppendFormatted(thisItemStr);
				defaultInterpolatedStringHandler.AppendLiteral(", inventory: ");
				defaultInterpolatedStringHandler.AppendFormatted(ownerStr);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				GameAnalyticsManager.AddErrorEventOnce("Inventory.TryPutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			if (this.Owner == null)
			{
				return false;
			}
			if (this.slots[i].Any() && allowCombine && this.slots[i].First().Combine(item, user))
			{
				return this.slots[i].Any() || this.TryPutItem(item, i, allowSwapping, allowCombine, user, createNetworkEvent, ignoreCondition, triggerOnInsertedEffects);
			}
			if (this.CanBePutInSlot(item, i, ignoreCondition))
			{
				this.PutItem(item, i, user, true, createNetworkEvent, triggerOnInsertedEffects);
				return true;
			}
			if (this.slots[i].Any() && item.ParentInventory != null && allowSwapping)
			{
				Item itemInSlot = this.slots[i].First();
				if (itemInSlot.OwnInventory != null && !itemInSlot.OwnInventory.Contains(item))
				{
					ItemContainer component = itemInSlot.GetComponent<ItemContainer>();
					if (component != null && component.GetMaxStackSize(0) == 1 && itemInSlot.OwnInventory.TrySwapping(0, item, user, createNetworkEvent, false))
					{
						return true;
					}
				}
				return this.TrySwapping(i, item, user, createNetworkEvent, true) || this.TrySwapping(i, item, user, createNetworkEvent, false);
			}
			return false;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x00044BBC File Offset: 0x00042DBC
		protected virtual void PutItem(Item item, int i, Character user, bool removeItem = true, bool createNetworkEvent = true, bool triggerOnInsertedEffects = true)
		{
			if (!this.IsIndexInRange(i))
			{
				string errorMsg = "Inventory.PutItem failed: index was out of range(" + i.ToString() + ").\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("Inventory.PutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (this.Owner == null)
			{
				return;
			}
			Inventory prevInventory = item.ParentInventory;
			Inventory prevOwnerInventory = item.FindParentInventory((Inventory inv) => inv is CharacterInventory);
			if (createNetworkEvent)
			{
				this.CreateNetworkEvent();
				if (prevInventory != null && prevInventory != this)
				{
					prevInventory.syncItemsDelay = 1f;
				}
			}
			if (removeItem)
			{
				Character user2 = user;
				bool createNetworkEvent2;
				if (createNetworkEvent)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					createNetworkEvent2 = (networkMember != null && networkMember.IsServer);
				}
				else
				{
					createNetworkEvent2 = false;
				}
				item.Drop(user2, createNetworkEvent2, false);
				Inventory parentInventory = item.ParentInventory;
				if (parentInventory != null)
				{
					parentInventory.RemoveItem(item);
				}
			}
			this.slots[i].Add(item);
			item.ParentInventory = this;
			CharacterHUD.RecreateHudTextsIfControlling(user);
			if (item.body != null)
			{
				item.body.Enabled = false;
				item.body.BodyType = BodyType.Dynamic;
				item.SetTransform(item.SimPosition, 0f, false, true, null);
				item.body.UpdateDrawPosition(false);
			}
			CharacterInventory characterInventory = prevOwnerInventory as CharacterInventory;
			if (characterInventory != null && characterInventory != this && this.Owner == user)
			{
				GameServer server = GameMain.Server;
				Client client2;
				if (server == null)
				{
					client2 = null;
				}
				else
				{
					IReadOnlyList<Client> connectedClients = server.ConnectedClients;
					client2 = ((connectedClients != null) ? connectedClients.Find((Client cl) => cl.Character == user) : null);
				}
				Client client = client2;
				GameServer server2 = GameMain.Server;
				if (server2 != null)
				{
					server2.KarmaManager.OnItemTakenFromPlayer(characterInventory, client, item);
				}
			}
			if (this is CharacterInventory)
			{
				if (prevInventory != this && prevOwnerInventory != this)
				{
					HumanAIController.ItemTaken(item, user);
				}
			}
			else
			{
				CharacterInventory currentInventory = item.FindParentInventory((Inventory inv) => inv is CharacterInventory) as CharacterInventory;
				if (currentInventory != null && currentInventory != prevInventory)
				{
					HumanAIController.ItemTaken(item, user);
				}
			}
			this.NotifyItemComponentsOfChange();
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x00044DC8 File Offset: 0x00042FC8
		public bool IsEmpty()
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Any())
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x00044DF8 File Offset: 0x00042FF8
		public virtual bool IsFull(bool takeStacksIntoAccount = false)
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
					if (this.slots[i].Items.Count < item.Prefab.GetMaxStackSize(this))
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

		// Token: 0x06000726 RID: 1830 RVA: 0x00044E80 File Offset: 0x00043080
		protected bool TrySwapping(int index, Item item, Character user, bool createNetworkEvent, bool swapWholeStack)
		{
			Inventory.<>c__DisplayClass52_0 CS$<>8__locals1 = new Inventory.<>c__DisplayClass52_0();
			CS$<>8__locals1.user = user;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.index = index;
			CS$<>8__locals1.createNetworkEvent = createNetworkEvent;
			if (((item != null) ? item.ParentInventory : null) == null || !this.slots[CS$<>8__locals1.index].Any())
			{
				return false;
			}
			if (this.slots[CS$<>8__locals1.index].Items.Any((Item it) => !it.IsInteractable(CS$<>8__locals1.user)))
			{
				return false;
			}
			if (!this.AllowSwappingContainedItems)
			{
				return false;
			}
			CS$<>8__locals1.otherInventory = item.ParentInventory;
			bool otherIsEquipped = false;
			CS$<>8__locals1.otherIndex = -1;
			for (int i = 0; i < CS$<>8__locals1.otherInventory.slots.Length; i++)
			{
				if (CS$<>8__locals1.otherInventory.slots[i].Contains(item))
				{
					CharacterInventory characterInventory = CS$<>8__locals1.otherInventory as CharacterInventory;
					if (characterInventory != null)
					{
						if (characterInventory.SlotTypes[i] == InvSlotType.Any)
						{
							CS$<>8__locals1.otherIndex = i;
							break;
						}
						otherIsEquipped = true;
					}
				}
			}
			if (CS$<>8__locals1.otherIndex == -1)
			{
				CS$<>8__locals1.otherIndex = CS$<>8__locals1.otherInventory.FindIndex(item);
				if (CS$<>8__locals1.otherIndex == -1)
				{
					DebugConsole.ThrowError("Something went wrong when trying to swap items between inventory slots: couldn't find the source item from it's inventory.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return false;
				}
			}
			CS$<>8__locals1.existingItems = new List<Item>();
			if (swapWholeStack)
			{
				CS$<>8__locals1.existingItems.AddRange(this.slots[CS$<>8__locals1.index].Items);
				int j2;
				int j;
				for (j = 0; j < this.capacity; j = j2 + 1)
				{
					if (CS$<>8__locals1.existingItems.Any((Item existingItem) => CS$<>8__locals1.<>4__this.slots[j].Contains(existingItem)))
					{
						this.slots[j].RemoveAllItems();
					}
					j2 = j;
				}
			}
			else
			{
				CS$<>8__locals1.existingItems.Add(this.slots[CS$<>8__locals1.index].FirstOrDefault());
				int j2;
				int j;
				for (j = 0; j < this.capacity; j = j2 + 1)
				{
					if (CS$<>8__locals1.existingItems.Any((Item existingItem) => CS$<>8__locals1.<>4__this.slots[j].Contains(existingItem)))
					{
						this.slots[j].RemoveItem(CS$<>8__locals1.existingItems.First<Item>());
					}
					j2 = j;
				}
			}
			CS$<>8__locals1.stackedItems = new List<Item>();
			if (swapWholeStack)
			{
				for (int j3 = 0; j3 < CS$<>8__locals1.otherInventory.capacity; j3++)
				{
					if (CS$<>8__locals1.otherInventory.slots[j3].Contains(item) && !CS$<>8__locals1.stackedItems.Contains(item))
					{
						CS$<>8__locals1.stackedItems.AddRange(CS$<>8__locals1.otherInventory.slots[j3].Items);
						CS$<>8__locals1.otherInventory.slots[j3].RemoveAllItems();
					}
				}
			}
			else if (!CS$<>8__locals1.stackedItems.Contains(item))
			{
				CS$<>8__locals1.stackedItems.Add(item);
				CS$<>8__locals1.otherInventory.slots[CS$<>8__locals1.otherIndex].RemoveItem(item);
			}
			bool swapSuccessful;
			if (otherIsEquipped)
			{
				swapSuccessful = (CS$<>8__locals1.stackedItems.Distinct<Item>().All((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) && (CS$<>8__locals1.existingItems.All((Item existingItem) => CS$<>8__locals1.otherInventory.TryPutItem(existingItem, CS$<>8__locals1.otherIndex, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) || (CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.otherInventory.TryPutItem(CS$<>8__locals1.existingItems.First<Item>(), CS$<>8__locals1.user, CharacterInventory.AnySlot, CS$<>8__locals1.createNetworkEvent, false, true))));
			}
			else
			{
				swapSuccessful = ((CS$<>8__locals1.existingItems.All((Item existingItem) => CS$<>8__locals1.otherInventory.TryPutItem(existingItem, CS$<>8__locals1.otherIndex, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) || (CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.otherInventory.TryPutItem(CS$<>8__locals1.existingItems.First<Item>(), CS$<>8__locals1.user, CharacterInventory.AnySlot, CS$<>8__locals1.createNetworkEvent, false, true))) && CS$<>8__locals1.stackedItems.Distinct<Item>().All((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)));
				if (!swapSuccessful && CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.existingItems[0].AllowDroppingOnSwapWith(item))
				{
					Item container = CS$<>8__locals1.existingItems[0].Container;
					CharacterInventory characterInv = ((container != null) ? container.ParentInventory : null) as CharacterInventory;
					if (characterInv == null || !characterInv.TryPutItem(CS$<>8__locals1.existingItems[0], CS$<>8__locals1.user, new List<InvSlotType>
					{
						InvSlotType.Any
					}, true, false, true))
					{
						CS$<>8__locals1.existingItems[0].Drop(CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, true);
					}
					swapSuccessful = CS$<>8__locals1.stackedItems.Distinct<Item>().Any((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true));
				}
			}
			if (swapSuccessful)
			{
				return true;
			}
			if (swapWholeStack)
			{
				foreach (Item stackedItem2 in CS$<>8__locals1.stackedItems)
				{
					for (int k = 0; k < this.capacity; k++)
					{
						if (this.slots[k].Contains(stackedItem2))
						{
							this.slots[k].RemoveItem(stackedItem2);
						}
					}
				}
				using (List<Item>.Enumerator enumerator2 = CS$<>8__locals1.existingItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item existingItem2 = enumerator2.Current;
						for (int l = 0; l < CS$<>8__locals1.otherInventory.capacity; l++)
						{
							if (CS$<>8__locals1.otherInventory.slots[l].Contains(existingItem2))
							{
								CS$<>8__locals1.otherInventory.slots[l].RemoveItem(existingItem2);
							}
						}
					}
					goto IL_677;
				}
			}
			for (int m = 0; m < this.capacity; m++)
			{
				if (this.slots[m].Contains(item))
				{
					Inventory.ItemSlot itemSlot = this.slots[m];
					Func<Item, bool> predicate;
					if ((predicate = CS$<>8__locals1.<>9__9) == null)
					{
						predicate = (CS$<>8__locals1.<>9__9 = ((Item it) => CS$<>8__locals1.existingItems.Contains(it) || CS$<>8__locals1.stackedItems.Contains(it)));
					}
					itemSlot.RemoveWhere(predicate);
				}
			}
			for (int n = 0; n < CS$<>8__locals1.otherInventory.capacity; n++)
			{
				if (CS$<>8__locals1.otherInventory.slots[n].Contains(CS$<>8__locals1.existingItems.FirstOrDefault<Item>()))
				{
					Inventory.ItemSlot itemSlot2 = CS$<>8__locals1.otherInventory.slots[n];
					Func<Item, bool> predicate2;
					if ((predicate2 = CS$<>8__locals1.<>9__10) == null)
					{
						predicate2 = (CS$<>8__locals1.<>9__10 = ((Item it) => CS$<>8__locals1.existingItems.Contains(it) || CS$<>8__locals1.stackedItems.Contains(it)));
					}
					itemSlot2.RemoveWhere(predicate2);
				}
			}
			IL_677:
			if (otherIsEquipped)
			{
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.existingItems, this, CS$<>8__locals1.index);
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.stackedItems, CS$<>8__locals1.otherInventory, CS$<>8__locals1.otherIndex);
			}
			else
			{
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.stackedItems, CS$<>8__locals1.otherInventory, CS$<>8__locals1.otherIndex);
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.existingItems, this, CS$<>8__locals1.index);
			}
			if (CS$<>8__locals1.createNetworkEvent)
			{
				this.CreateNetworkEvent();
				CS$<>8__locals1.otherInventory.CreateNetworkEvent();
			}
			return false;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00045598 File Offset: 0x00043798
		public void CreateNetworkEvent()
		{
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			if (GameMain.NetworkMember.IsClient)
			{
				this.syncItemsDelay = 1f;
			}
			List<Range> slotRanges = new List<Range>();
			int startIndex = 0;
			int itemCount = 0;
			for (int i = 0; i < this.capacity; i++)
			{
				int count = this.slots[i].Items.Count;
				if (itemCount + count > 128 || i == this.capacity - 1)
				{
					slotRanges.Add(new Range(startIndex, i + 1));
					startIndex = i + 1;
					itemCount = 0;
				}
				itemCount += count;
			}
			foreach (Range slotRange in slotRanges)
			{
				this.CreateNetworkEvent(slotRange);
			}
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x00045674 File Offset: 0x00043874
		protected virtual void CreateNetworkEvent(Range slotRange)
		{
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00045678 File Offset: 0x00043878
		public Item FindItem(Func<Item, bool> predicate, bool recursive)
		{
			Item match = this.AllItems.FirstOrDefault(predicate);
			if (match == null && recursive)
			{
				foreach (Item item in this.AllItems)
				{
					if (((item != null) ? item.OwnInventory : null) != null)
					{
						match = item.OwnInventory.FindItem(predicate, true);
						if (match != null)
						{
							return match;
						}
					}
				}
				return match;
			}
			return match;
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x000456FC File Offset: 0x000438FC
		public List<Item> FindAllItems(Func<Item, bool> predicate = null, bool recursive = false, List<Item> list = null)
		{
			if (list == null)
			{
				list = new List<Item>();
			}
			foreach (Item item in this.AllItems)
			{
				if (predicate == null || predicate(item))
				{
					list.Add(item);
				}
				if (recursive)
				{
					ItemInventory ownInventory = item.OwnInventory;
					if (ownInventory != null)
					{
						ownInventory.FindAllItems(predicate, true, list);
					}
				}
			}
			return list;
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00045778 File Offset: 0x00043978
		public Item FindItemByTag(Identifier tag, bool recursive = false)
		{
			if (tag.IsEmpty)
			{
				return null;
			}
			return this.FindItem((Item i) => i.HasTag(tag), recursive);
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x000457B4 File Offset: 0x000439B4
		public Item FindItemByIdentifier(Identifier identifier, bool recursive = false)
		{
			if (identifier.IsEmpty)
			{
				return null;
			}
			return this.FindItem((Item i) => i.Prefab.Identifier == identifier, recursive);
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x000457F0 File Offset: 0x000439F0
		public virtual void RemoveItem(Item item)
		{
			if (item == null)
			{
				return;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					this.slots[i].RemoveItem(item);
					item.ParentInventory = null;
					CharacterHUD.RecreateHudTextsIfFocused(new Item[]
					{
						item
					});
				}
			}
			this.NotifyItemComponentsOfChange();
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x0004584C File Offset: 0x00043A4C
		public void ForceToSlot(Item item, int index)
		{
			this.slots[index].Add(item);
			item.ParentInventory = this;
			CharacterInventory characterInventory = this as CharacterInventory;
			Character character = ((characterInventory != null) ? characterInventory.Owner : null) as Character;
			bool equipped = character != null && character.HasEquippedItem(item, null, null);
			if (item.body != null && !equipped)
			{
				item.body.Enabled = false;
				item.body.BodyType = BodyType.Dynamic;
			}
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x000458C1 File Offset: 0x00043AC1
		public void ForceRemoveFromSlot(Item item, int index)
		{
			this.slots[index].RemoveItem(item);
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x000458D1 File Offset: 0x00043AD1
		public bool IsInSlot(Item item, int index)
		{
			return this.IsIndexInRange(index) && this.slots[index].Contains(item);
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x000458EC File Offset: 0x00043AEC
		public bool IsSlotEmpty(int index)
		{
			return this.IsIndexInRange(index) && this.slots[index].Empty();
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x00045908 File Offset: 0x00043B08
		public void SharedRead(IReadMessage msg, List<ushort>[] receivedItemIds, out bool readyToApply)
		{
			byte start = msg.ReadByte();
			byte end = msg.ReadByte();
			if (start == 0)
			{
				for (int i = 0; i < this.capacity; i++)
				{
					receivedItemIds[i] = null;
				}
			}
			for (int j = (int)start; j < (int)end; j++)
			{
				List<ushort> newItemIds = new List<ushort>();
				int itemCount = msg.ReadRangedInteger(0, 63);
				for (int k = 0; k < itemCount; k++)
				{
					newItemIds.Add(msg.ReadUInt16());
				}
				receivedItemIds[j] = newItemIds;
			}
			readyToApply = !receivedItemIds.Contains(null);
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x00045988 File Offset: 0x00043B88
		public void SharedWrite(IWriteMessage msg, Range slotRange)
		{
			int start = slotRange.Start.Value;
			int end = slotRange.End.Value;
			msg.WriteByte((byte)start);
			msg.WriteByte((byte)end);
			for (int i = start; i < end; i++)
			{
				msg.WriteRangedInteger(this.slots[i].Items.Count, 0, 63);
				for (int j = 0; j < Math.Min(this.slots[i].Items.Count, 63); j++)
				{
					Item item = this.slots[i].Items[j];
					msg.WriteUInt16((item != null) ? item.ID : 0);
				}
			}
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x00045A40 File Offset: 0x00043C40
		public void DeleteAllItems()
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Any())
				{
					foreach (Item item in this.slots[i].Items)
					{
						foreach (ItemContainer itemContainer in item.GetComponents<ItemContainer>())
						{
							itemContainer.Inventory.DeleteAllItems();
						}
					}
					this.slots[i].Items.ForEachMod(delegate(Item it)
					{
						it.Remove();
					});
					this.slots[i].RemoveAllItems();
				}
			}
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x00045B38 File Offset: 0x00043D38
		[CompilerGenerated]
		private void <EnsureItemsInBothHands>g__TryPutInOppositeHandSlot|2_0(int originalSlot, int otherHandSlot, ref Inventory.<>c__DisplayClass2_0 A_3)
		{
			foreach (Item it in this.slots[originalSlot].Items)
			{
				if (!it.AllowedSlots.None((InvSlotType s) => s.HasFlag(InvSlotType.RightHand | InvSlotType.LeftHand)) && !this.slots[otherHandSlot].Contains(it))
				{
					this.TryPutItem(it, otherHandSlot, true, true, A_3.character, false, false, true);
				}
			}
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x00045BD8 File Offset: 0x00043DD8
		[CompilerGenerated]
		private bool <EnsureItemsInBothHands>g__IsSlotIndexOutOfBound|2_1(int index, ref Inventory.<>c__DisplayClass2_0 A_2)
		{
			return index < 0 || index >= this.slots.Length;
		}

		// Token: 0x04000350 RID: 848
		private readonly Dictionary<Client, List<ushort>[]> receivedItemIds = new Dictionary<Client, List<ushort>[]>();

		// Token: 0x04000351 RID: 849
		public const int MaxPossibleStackSize = 63;

		// Token: 0x04000352 RID: 850
		public const int MaxItemsPerNetworkEvent = 128;

		// Token: 0x04000353 RID: 851
		public readonly Entity Owner;

		// Token: 0x04000354 RID: 852
		protected readonly int capacity;

		// Token: 0x04000355 RID: 853
		protected readonly Inventory.ItemSlot[] slots;

		// Token: 0x04000356 RID: 854
		public bool Locked;

		// Token: 0x04000357 RID: 855
		protected float syncItemsDelay;

		// Token: 0x04000358 RID: 856
		private int extraStackSize;

		// Token: 0x04000359 RID: 857
		private readonly List<Item> allItemsList = new List<Item>();

		// Token: 0x0400035A RID: 858
		public bool AllowSwappingContainedItems = true;

		// Token: 0x0200068A RID: 1674
		public class ItemSlot
		{
			// Token: 0x170013EF RID: 5103
			// (get) Token: 0x06004EBF RID: 20159 RVA: 0x001E22C7 File Offset: 0x001E04C7
			public IReadOnlyList<Item> Items
			{
				get
				{
					return this.items;
				}
			}

			// Token: 0x06004EC0 RID: 20160 RVA: 0x001E22CF File Offset: 0x001E04CF
			public ItemSlot(Inventory inventory)
			{
				this.inventory = inventory;
			}

			// Token: 0x06004EC1 RID: 20161 RVA: 0x001E22EC File Offset: 0x001E04EC
			public bool CanBePut(Item item, bool ignoreCondition = false)
			{
				if (item == null)
				{
					return false;
				}
				if (this.items.Count > 0)
				{
					if (!ignoreCondition)
					{
						if (item.IsFullCondition)
						{
							if (this.items.Any((Item it) => !it.IsFullCondition))
							{
								return false;
							}
						}
						else
						{
							if (!MathUtils.NearlyEqual(item.Condition, 0f, 0.0001f))
							{
								return false;
							}
							if (this.items.Any((Item it) => !MathUtils.NearlyEqual(it.Condition, 0f, 0.0001f)))
							{
								return false;
							}
						}
					}
					if (this.items[0].Quality != item.Quality)
					{
						return false;
					}
					if (this.items[0].Prefab.Identifier != item.Prefab.Identifier || this.items.Count + 1 > item.Prefab.GetMaxStackSize(this.inventory))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x06004EC2 RID: 20162 RVA: 0x001E23F8 File Offset: 0x001E05F8
			public bool CanProbablyBePut(ItemPrefab itemPrefab, float? condition = null, int? quality = null)
			{
				if (itemPrefab == null)
				{
					return false;
				}
				if (this.items.Count > 0)
				{
					if (condition != null)
					{
						if (MathUtils.NearlyEqual(condition.Value, 0f, 0.0001f))
						{
							if (this.items.Any((Item it) => it.Condition > 0f))
							{
								return false;
							}
						}
						else
						{
							if (!MathUtils.NearlyEqual(condition.Value, itemPrefab.Health, 0.0001f))
							{
								return false;
							}
							if (this.items.Any((Item it) => !it.IsFullCondition))
							{
								return false;
							}
						}
					}
					else if (this.items.Any((Item it) => !it.IsFullCondition))
					{
						return false;
					}
					if (quality != null && this.items[0].Quality != quality.Value)
					{
						return false;
					}
					if (this.items[0].Prefab.Identifier != itemPrefab.Identifier || this.items.Count + 1 > itemPrefab.GetMaxStackSize(this.inventory))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x06004EC3 RID: 20163 RVA: 0x001E254C File Offset: 0x001E074C
			public int HowManyCanBePut(ItemPrefab itemPrefab, int? maxStackSize = null, float? condition = null, bool ignoreItemsInSlot = false)
			{
				if (itemPrefab == null)
				{
					return 0;
				}
				int value = maxStackSize.GetValueOrDefault();
				if (maxStackSize == null)
				{
					value = itemPrefab.GetMaxStackSize(this.inventory);
					maxStackSize = new int?(value);
				}
				if (this.items.Count <= 0 || ignoreItemsInSlot)
				{
					return maxStackSize.Value;
				}
				if (condition != null)
				{
					if (MathUtils.NearlyEqual(condition.Value, 0f, 0.0001f))
					{
						if (this.items.Any((Item it) => it.Condition > 0f))
						{
							return 0;
						}
					}
					else
					{
						if (!MathUtils.NearlyEqual(condition.Value, itemPrefab.Health, 0.0001f))
						{
							return 0;
						}
						if (this.items.Any((Item it) => !it.IsFullCondition))
						{
							return 0;
						}
					}
				}
				else if (this.items.Any((Item it) => !it.IsFullCondition))
				{
					return 0;
				}
				if (this.items[0].Prefab.Identifier != itemPrefab.Identifier)
				{
					return 0;
				}
				return maxStackSize.Value - this.items.Count;
			}

			// Token: 0x06004EC4 RID: 20164 RVA: 0x001E26A8 File Offset: 0x001E08A8
			public void Add(Item item)
			{
				if (item == null)
				{
					throw new InvalidOperationException("Tried to add a null item to an inventory slot.");
				}
				if (this.items.Count > 0)
				{
					if (this.items[0].Prefab.Identifier != item.Prefab.Identifier)
					{
						throw new InvalidOperationException("Tried to stack different types of items.");
					}
					if (this.items.Count + 1 > item.Prefab.GetMaxStackSize(this.inventory))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Tried to add an item to a full inventory slot (stack already full, x");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.items.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.items.First<Item>().Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(").");
						throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				if (this.items.Contains(item))
				{
					return;
				}
				int index = 0;
				int i = 0;
				while (i < this.items.Count && this.items[i].Condition <= item.Condition)
				{
					index++;
					i++;
				}
				this.items.Insert(index, item);
			}

			// Token: 0x06004EC5 RID: 20165 RVA: 0x001E27E4 File Offset: 0x001E09E4
			public Item RemoveItem()
			{
				if (this.items.Count == 0)
				{
					return null;
				}
				Item item = this.items[0];
				this.items.RemoveAt(0);
				return item;
			}

			// Token: 0x06004EC6 RID: 20166 RVA: 0x001E281A File Offset: 0x001E0A1A
			public void RemoveItem(Item item)
			{
				this.items.Remove(item);
			}

			// Token: 0x06004EC7 RID: 20167 RVA: 0x001E2829 File Offset: 0x001E0A29
			public void RemoveAllItems()
			{
				this.items.Clear();
			}

			// Token: 0x06004EC8 RID: 20168 RVA: 0x001E2838 File Offset: 0x001E0A38
			public void RemoveWhere(Func<Item, bool> predicate)
			{
				this.items.RemoveAll((Item it) => predicate(it));
			}

			// Token: 0x06004EC9 RID: 20169 RVA: 0x001E286A File Offset: 0x001E0A6A
			public bool Any()
			{
				return this.items.Count > 0;
			}

			// Token: 0x06004ECA RID: 20170 RVA: 0x001E287A File Offset: 0x001E0A7A
			public bool Empty()
			{
				return this.items.Count == 0;
			}

			// Token: 0x06004ECB RID: 20171 RVA: 0x001E288A File Offset: 0x001E0A8A
			public Item First()
			{
				return this.items[0];
			}

			// Token: 0x06004ECC RID: 20172 RVA: 0x001E2898 File Offset: 0x001E0A98
			public Item FirstOrDefault()
			{
				return this.items.FirstOrDefault<Item>();
			}

			// Token: 0x06004ECD RID: 20173 RVA: 0x001E28A5 File Offset: 0x001E0AA5
			public Item LastOrDefault()
			{
				return this.items.LastOrDefault<Item>();
			}

			// Token: 0x06004ECE RID: 20174 RVA: 0x001E28B2 File Offset: 0x001E0AB2
			public bool Contains(Item item)
			{
				return this.items.Contains(item);
			}

			// Token: 0x040029D5 RID: 10709
			private readonly List<Item> items = new List<Item>(63);

			// Token: 0x040029D6 RID: 10710
			public bool HideIfEmpty;

			// Token: 0x040029D7 RID: 10711
			private readonly Inventory inventory;
		}
	}
}
