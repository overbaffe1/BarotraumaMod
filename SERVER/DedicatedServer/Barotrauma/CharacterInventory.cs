using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000038 RID: 56
	internal class CharacterInventory : Inventory
	{
		// Token: 0x060006E7 RID: 1767 RVA: 0x00043023 File Offset: 0x00041223
		public void ServerEventWrite(IWriteMessage msg, Client c, Character.InventoryStateEventData inventoryData)
		{
			base.SharedWrite(msg, inventoryData.SlotRange);
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x060006E8 RID: 1768 RVA: 0x00043032 File Offset: 0x00041232
		public InvSlotType[] SlotTypes { get; }

		// Token: 0x060006E9 RID: 1769 RVA: 0x0004303A File Offset: 0x0004123A
		public static bool IsHandSlotType(InvSlotType s)
		{
			return s.HasFlag(InvSlotType.LeftHand) || s.HasFlag(InvSlotType.RightHand);
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x060006EA RID: 1770 RVA: 0x00043062 File Offset: 0x00041262
		// (set) Token: 0x060006EB RID: 1771 RVA: 0x0004306A File Offset: 0x0004126A
		public bool AccessibleWhenAlive { get; private set; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x060006EC RID: 1772 RVA: 0x00043073 File Offset: 0x00041273
		// (set) Token: 0x060006ED RID: 1773 RVA: 0x0004307B File Offset: 0x0004127B
		public bool AccessibleByOwner { get; private set; }

		// Token: 0x060006EE RID: 1774 RVA: 0x00043084 File Offset: 0x00041284
		private static string[] ParseSlotTypes(ContentXElement element)
		{
			string slotString = element.GetAttributeString("slots", null);
			if (slotString != null)
			{
				return slotString.Split(',', StringSplitOptions.None);
			}
			return Array.Empty<string>();
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x000430B0 File Offset: 0x000412B0
		public CharacterInventory(ContentXElement element, Character character, bool spawnInitialItems) : base(character, CharacterInventory.ParseSlotTypes(element).Length, 5)
		{
			CharacterInventory <>4__this = this;
			this.character = character;
			this.IsEquipped = new bool[this.capacity];
			this.SlotTypes = new InvSlotType[this.capacity];
			this.AccessibleWhenAlive = element.GetAttributeBool("accessiblewhenalive", character.Info != null);
			this.AccessibleByOwner = element.GetAttributeBool("accessiblebyowner", this.AccessibleWhenAlive);
			string[] slotTypeNames = CharacterInventory.ParseSlotTypes(element);
			for (int i = 0; i < this.capacity; i++)
			{
				InvSlotType parsedSlotType = InvSlotType.Any;
				slotTypeNames[i] = slotTypeNames[i].Trim();
				if (!Enum.TryParse<InvSlotType>(slotTypeNames[i], out parsedSlotType))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in the inventory config of \"",
						character.SpeciesName.ToString(),
						"\" - ",
						slotTypeNames[i],
						" is not a valid inventory slot type."
					}), null, element.ContentPackage, false, false);
				}
				this.SlotTypes[i] = parsedSlotType;
				InvSlotType invSlotType = this.SlotTypes[i];
				if (invSlotType == InvSlotType.RightHand || invSlotType == InvSlotType.LeftHand)
				{
					this.slots[i].HideIfEmpty = true;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				InvSlotType slotType = this.SlotTypes[j];
				List<Inventory.ItemSlot> slotList;
				if (!this.slotsByType.TryGetValue(slotType, out slotList))
				{
					slotList = new List<Inventory.ItemSlot>();
					this.slotsByType[this.SlotTypes[j]] = slotList;
				}
				slotList.Add(this.slots[j]);
			}
			IEnumerable<ContentXElement> itemElements = from e in element.Elements()
			where e.Name.ToString().Equals("item", StringComparison.OrdinalIgnoreCase)
			select e;
			int itemCount = itemElements.Count<ContentXElement>();
			if (itemCount > this.capacity)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral("\" is configured to spawn with more items than it has inventory capacity for.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (!spawnInitialItems)
			{
				return;
			}
			Action<Item> <>9__1;
			foreach (ContentXElement subElement in itemElements)
			{
				string itemIdentifier = subElement.GetAttributeString("identifier", "");
				ItemPrefab itemPrefab;
				if (!ItemPrefab.Prefabs.TryGet(itemIdentifier, out itemPrefab))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in character inventory \"",
						character.SpeciesName.ToString(),
						"\" - item \"",
						itemIdentifier,
						"\" not found."
					}), null, element.ContentPackage, false, false);
				}
				else
				{
					string slotString = subElement.GetAttributeString("slot", "None");
					InvSlotType s;
					InvSlotType slot = Enum.TryParse<InvSlotType>(slotString, true, out s) ? s : InvSlotType.None;
					bool forceToSlot = subElement.GetAttributeBool("forcetoslot", false);
					int amount = subElement.GetAttributeInt("amount", 1);
					for (int k = 0; k < amount; k++)
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner != null)
						{
							ItemPrefab itemPrefab2 = itemPrefab;
							bool ignoreLimbSlots = forceToSlot;
							InvSlotType slot2 = slot;
							float? condition = null;
							int? quality = null;
							Action<Item> onSpawned;
							if ((onSpawned = <>9__1) == null)
							{
								onSpawned = (<>9__1 = delegate(Item item)
								{
									if (item != null && item.ParentInventory != <>4__this)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(59, 2);
										defaultInterpolatedStringHandler2.AppendLiteral("Failed to spawn the initial item \"");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(item.Prefab.Identifier);
										defaultInterpolatedStringHandler2.AppendLiteral("\" in the inventory of \"");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(character.SpeciesName);
										defaultInterpolatedStringHandler2.AppendLiteral("\".");
										string errorMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
										DebugConsole.ThrowError(errorMsg, null, element.ContentPackage, false, false);
										GameAnalyticsManager.AddErrorEventOnce("CharacterInventory:FailedToSpawnInitialItem", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
										return;
									}
									if (!character.Enabled)
									{
										foreach (Item heldItem in character.HeldItems)
										{
											if (item.body != null)
											{
												item.body.Enabled = false;
											}
										}
									}
								});
							}
							spawner.AddItemToSpawnQueue(itemPrefab2, this, condition, quality, onSpawned, true, ignoreLimbSlots, slot2);
						}
					}
				}
			}
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x00043494 File Offset: 0x00041694
		public Item FindEquippedItemByTag(Identifier tag)
		{
			if (tag.IsEmpty)
			{
				return null;
			}
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.SlotTypes[i] != InvSlotType.Any)
				{
					Item item = this.slots[i].FirstOrDefault();
					if (item != null && item.HasTag(tag))
					{
						return item;
					}
				}
			}
			return null;
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x000434E8 File Offset: 0x000416E8
		public int FindLimbSlot(InvSlotType limbSlot)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.SlotTypes[i] == limbSlot)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x00043518 File Offset: 0x00041718
		public Item GetItemInLimbSlot(InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				return slotList.First<Inventory.ItemSlot>().FirstOrDefault();
			}
			return null;
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x00043542 File Offset: 0x00041742
		public IEnumerable<Item> GetItemsInLimbSlot(InvSlotType limbSlot)
		{
			CharacterInventory.<GetItemsInLimbSlot>d__25 <GetItemsInLimbSlot>d__ = new CharacterInventory.<GetItemsInLimbSlot>d__25(-2);
			<GetItemsInLimbSlot>d__.<>4__this = this;
			<GetItemsInLimbSlot>d__.<>3__limbSlot = limbSlot;
			return <GetItemsInLimbSlot>d__;
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0004355C File Offset: 0x0004175C
		public bool IsInLimbSlot(Item item, InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (limbSlot == (InvSlotType.RightHand | InvSlotType.LeftHand))
			{
				if (this.GetItemsInLimbSlot(InvSlotType.RightHand).Contains(item) && this.GetItemsInLimbSlot(InvSlotType.LeftHand).Contains(item))
				{
					return true;
				}
			}
			else if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				foreach (Inventory.ItemSlot slot in slotList)
				{
					if (slot.Contains(item))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x000435E8 File Offset: 0x000417E8
		public bool IsSlotEmpty(InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				foreach (Inventory.ItemSlot slot in slotList)
				{
					if (slot.Empty())
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00043650 File Offset: 0x00041850
		public bool CanBePut(Item item, InvSlotType slotType)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (slotType.HasFlag(this.SlotTypes[i]) && this.CanBePutInSlot(item, i, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x00043698 File Offset: 0x00041898
		public override bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return base.CanBePutInSlot(item, i, ignoreCondition) && item.AllowedSlots.Any((InvSlotType s) => s.HasFlag(this.SlotTypes[i])) && (this.SlotTypes[i] == InvSlotType.Any || this.slots[i].Items.Count < 1);
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x00043710 File Offset: 0x00041910
		public override bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition, int? quality = null)
		{
			return base.CanBePutInSlot(itemPrefab, i, condition, quality) && (this.SlotTypes[i] == InvSlotType.Any || this.slots[i].Items.Count < 1);
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x00043743 File Offset: 0x00041943
		public override void RemoveItem(Item item)
		{
			this.RemoveItem(item, false);
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x00043750 File Offset: 0x00041950
		public void RemoveItem(Item item, bool tryEquipFromSameStack)
		{
			if (!base.Contains(item))
			{
				return;
			}
			bool wasEquipped = this.character.HasEquippedItem(item, null, null);
			List<int> indices = base.FindIndices(item);
			base.RemoveItem(item);
			CharacterHUD.RecreateHudTextsIfControlling(this.character);
			if (tryEquipFromSameStack && wasEquipped)
			{
				int limbSlot = indices.Find((int j) => this.SlotTypes[j] != InvSlotType.Any);
				foreach (int i in indices)
				{
					Item itemInSameSlot = base.GetItemAt(i);
					if (itemInSameSlot != null)
					{
						if (this.TryPutItem(itemInSameSlot, limbSlot, false, false, this.character, true, false, true))
						{
							break;
						}
						break;
					}
				}
			}
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x00043814 File Offset: 0x00041A14
		public bool TryPutItemWithAutoEquipCheck(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true)
		{
			if (item.AllowedSlots.Contains(InvSlotType.Any))
			{
				Wearable wearable = item.GetComponent<Wearable>();
				if (wearable != null && !wearable.AutoEquipWhenFull && !this.IsAnySlotAvailable(item))
				{
					return false;
				}
			}
			if (allowedSlots != null && allowedSlots.Any<InvSlotType>() && !allowedSlots.Contains(InvSlotType.Any))
			{
				bool allSlotsTaken = true;
				foreach (InvSlotType allowedSlot in allowedSlots)
				{
					if (allowedSlot == (InvSlotType.RightHand | InvSlotType.LeftHand))
					{
						int rightHandSlot = this.FindLimbSlot(InvSlotType.RightHand);
						int leftHandSlot = this.FindLimbSlot(InvSlotType.LeftHand);
						if (rightHandSlot > -1 && this.slots[rightHandSlot].CanBePut(item, false) && leftHandSlot > -1 && this.slots[leftHandSlot].CanBePut(item, false))
						{
							allSlotsTaken = false;
							break;
						}
					}
					else
					{
						int slot = this.FindLimbSlot(allowedSlot);
						if (slot > -1 && this.slots[slot].CanBePut(item, false))
						{
							allSlotsTaken = false;
							break;
						}
					}
				}
				if (allSlotsTaken)
				{
					int slot2 = this.FindLimbSlot(allowedSlots.First<InvSlotType>());
					if (slot2 > -1 && this.slots[slot2].Items.Any((Item it) => it != item) && this.slots[slot2].First().AllowDroppingOnSwapWith(item))
					{
						foreach (Item existingItem in this.slots[slot2].Items.ToList<Item>())
						{
							if (existingItem.IsInteractable(this.character))
							{
								existingItem.Drop(user, true, true);
								Inventory parentInventory = existingItem.ParentInventory;
								if (parentInventory != null)
								{
									parentInventory.RemoveItem(existingItem);
								}
							}
						}
					}
				}
			}
			return this.TryPutItem(item, user, allowedSlots, createNetworkEvent, false, true);
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x00043A24 File Offset: 0x00041C24
		public override bool TryPutItem(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			CharacterInventory.<>c__DisplayClass34_0 CS$<>8__locals1 = new CharacterInventory.<>c__DisplayClass34_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.item = item;
			if (allowedSlots == null || !allowedSlots.Any<InvSlotType>())
			{
				return false;
			}
			if (CS$<>8__locals1.item == null)
			{
				return false;
			}
			if (CS$<>8__locals1.item.Removed)
			{
				DebugConsole.ThrowError("Tried to put a removed item (" + CS$<>8__locals1.item.Name + ") in an inventory.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return false;
			}
			if (CS$<>8__locals1.item.GetComponent<Pickable>() == null || CS$<>8__locals1.item.AllowedSlots.None(null))
			{
				return false;
			}
			int currentSlot = -1;
			bool inWrongSlot = false;
			bool inSuitableSlot = false;
			int slotIndex;
			int num;
			for (slotIndex = 0; slotIndex < this.capacity; slotIndex = num + 1)
			{
				if (this.slots[slotIndex].Contains(CS$<>8__locals1.item))
				{
					currentSlot = slotIndex;
					InvSlotType firstMatchingSlotType = allowedSlots.FirstOrDefault((InvSlotType slot) => slot.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[slotIndex]));
					if (firstMatchingSlotType == InvSlotType.None)
					{
						inWrongSlot = true;
						break;
					}
					inSuitableSlot = true;
					IEnumerable<InvSlotType> individualFlags = EnumExtensions.GetIndividualFlags<InvSlotType>(firstMatchingSlotType);
					foreach (InvSlotType flag in individualFlags)
					{
						if (flag != InvSlotType.None && !this.IsInLimbSlot(CS$<>8__locals1.item, flag))
						{
							inSuitableSlot = false;
							break;
						}
					}
				}
				num = slotIndex;
			}
			if (inSuitableSlot && !inWrongSlot)
			{
				return true;
			}
			if (allowedSlots.Contains(InvSlotType.Any) && CS$<>8__locals1.item.AllowedSlots.Contains(InvSlotType.Any))
			{
				int freeIndex = this.GetFreeAnySlot(CS$<>8__locals1.item, inWrongSlot);
				if (freeIndex > -1)
				{
					this.PutItem(CS$<>8__locals1.item, freeIndex, user, true, createNetworkEvent, true);
					CS$<>8__locals1.item.Unequip(this.character);
					return true;
				}
			}
			int placedInSlot = -1;
			Func<InvSlotType, int> keySelector;
			if ((keySelector = CS$<>8__locals1.<>9__1) == null)
			{
				keySelector = (CS$<>8__locals1.<>9__1 = ((InvSlotType slotType) => (!CS$<>8__locals1.<>4__this.IsSlotEmpty(slotType)) ? 1 : 0));
			}
			foreach (InvSlotType allowedSlot in allowedSlots.OrderBy(keySelector))
			{
				if ((!allowedSlot.HasFlag(InvSlotType.RightHand) || this.character.AnimController.GetLimb(LimbType.RightHand, true, false, false) != null) && (!allowedSlot.HasFlag(InvSlotType.LeftHand) || this.character.AnimController.GetLimb(LimbType.LeftHand, true, false, false) != null))
				{
					bool free = true;
					int i;
					for (i = 0; i < this.capacity; i = num + 1)
					{
						if (allowedSlot.HasFlag(this.SlotTypes[i]) && CS$<>8__locals1.item.AllowedSlots.Any((InvSlotType s) => s.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[i])))
						{
							IEnumerable<Item> items = this.slots[i].Items;
							Func<Item, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__3) == null)
							{
								predicate = (CS$<>8__locals1.<>9__3 = ((Item it) => it != CS$<>8__locals1.item));
							}
							if (items.Any(predicate) && (!this.slots[i].First().AllowedSlots.Contains(InvSlotType.Any) || !this.TryPutItem(this.slots[i].FirstOrDefault(), this.character, new List<InvSlotType>
							{
								InvSlotType.Any
							}, true, ignoreCondition, true)))
							{
								free = false;
							}
						}
						num = i;
					}
					if (free)
					{
						int i;
						Func<InvSlotType, bool> <>9__5;
						for (i = 0; i < this.capacity; i = num + 1)
						{
							if (allowedSlot.HasFlag(this.SlotTypes[i]) && CS$<>8__locals1.item.GetComponents<Pickable>().Any(delegate(Pickable p)
							{
								IEnumerable<InvSlotType> allowedSlots2 = p.AllowedSlots;
								Func<InvSlotType, bool> predicate2;
								if ((predicate2 = <>9__5) == null)
								{
									predicate2 = (<>9__5 = ((InvSlotType s) => s.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[i])));
								}
								return allowedSlots2.Any(predicate2);
							}) && this.slots[i].Empty())
							{
								bool removeFromOtherSlots = CS$<>8__locals1.item.ParentInventory != this;
								if (placedInSlot == -1 && inWrongSlot && (!this.slots[i].HideIfEmpty || this.SlotTypes[currentSlot] != InvSlotType.Any))
								{
									removeFromOtherSlots = true;
								}
								this.PutItem(CS$<>8__locals1.item, i, user, removeFromOtherSlots, createNetworkEvent, true);
								CS$<>8__locals1.item.Equip(this.character);
								placedInSlot = i;
							}
							num = i;
						}
						if (placedInSlot > -1)
						{
							break;
						}
					}
				}
			}
			return placedInSlot > -1;
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x00043F50 File Offset: 0x00042150
		public bool IsAnySlotAvailable(Item item)
		{
			return this.GetFreeAnySlot(item, false) > -1;
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x00043F60 File Offset: 0x00042160
		private int GetFreeAnySlot(Item item, bool inWrongSlot)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.SlotTypes[i] == InvSlotType.Any && !this.slots[i].Empty() && this.CanBePutInSlot(item, i, false))
				{
					return i;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (this.SlotTypes[j] == InvSlotType.Any && this.slots[j].Contains(item))
				{
					return j;
				}
			}
			for (int k = 0; k < this.capacity; k++)
			{
				if (this.SlotTypes[k] == InvSlotType.Any && this.CanBePutInSlot(item, k, false))
				{
					return k;
				}
			}
			Func<Item, bool> <>9__0;
			for (int l = 0; l < this.capacity; l++)
			{
				if (this.SlotTypes[l] == InvSlotType.Any)
				{
					if (inWrongSlot)
					{
						if (this.slots[l].Any())
						{
							IEnumerable<Item> items = this.slots[l].Items;
							Func<Item, bool> predicate;
							if ((predicate = <>9__0) == null)
							{
								predicate = (<>9__0 = ((Item it) => it != item));
							}
							if (items.Any(predicate))
							{
								goto IL_11B;
							}
						}
					}
					else if (!this.CanBePutInSlot(item, l, false))
					{
						goto IL_11B;
					}
					return l;
				}
				IL_11B:;
			}
			return -1;
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0004409C File Offset: 0x0004229C
		public override bool TryPutItem(Item item, int index, bool allowSwapping, bool allowCombine, Character user, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			if (index < 0 || index >= this.slots.Length)
			{
				string errorMsg = "CharacterInventory.TryPutItem failed: index was out of range(" + index.ToString() + ").\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("CharacterInventory.TryPutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			if (this.slots[index].Any())
			{
				return !this.slots[index].Contains(item) && base.TryPutItem(item, index, allowSwapping, allowCombine, user, createNetworkEvent, ignoreCondition, true);
			}
			if (this.SlotTypes[index] != InvSlotType.Any)
			{
				InvSlotType placeToSlots = InvSlotType.None;
				bool slotsFree = true;
				foreach (Pickable pickable in item.GetComponents<Pickable>())
				{
					foreach (InvSlotType allowedSlot in pickable.AllowedSlots)
					{
						if (allowedSlot.HasFlag(this.SlotTypes[index]))
						{
							for (int i = 0; i < this.capacity; i++)
							{
								if (allowedSlot.HasFlag(this.SlotTypes[i]) && this.slots[i].Any() && !this.slots[i].Contains(item))
								{
									slotsFree = false;
									break;
								}
								placeToSlots = allowedSlot;
							}
						}
					}
				}
				return slotsFree && this.TryPutItem(item, user, new List<InvSlotType>
				{
					placeToSlots
				}, createNetworkEvent, ignoreCondition, true);
			}
			if (!item.GetComponents<Pickable>().Any((Pickable p) => p.AllowedSlots.Contains(InvSlotType.Any)))
			{
				return false;
			}
			if (this.slots[index].Any())
			{
				return this.slots[index].Contains(item);
			}
			this.PutItem(item, index, user, true, createNetworkEvent, true);
			return true;
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0004429C File Offset: 0x0004249C
		protected override void PutItem(Item item, int i, Character user, bool removeItem = true, bool createNetworkEvent = true, bool triggerOnInsertedEffects = true)
		{
			base.PutItem(item, i, user, removeItem, createNetworkEvent, triggerOnInsertedEffects);
			CharacterHUD.RecreateHudTextsIfControlling(this.character);
			if (item.CampaignInteractionType == CampaignMode.InteractionType.Cargo)
			{
				item.AssignCampaignInteractionType(CampaignMode.InteractionType.None, null);
			}
			item.Equipper = user;
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x000442D1 File Offset: 0x000424D1
		protected override void CreateNetworkEvent(Range slotRange)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this.character, new Character.InventoryStateEventData(slotRange));
		}

		// Token: 0x04000348 RID: 840
		private readonly Character character;

		// Token: 0x0400034A RID: 842
		private readonly Dictionary<InvSlotType, List<Inventory.ItemSlot>> slotsByType = new Dictionary<InvSlotType, List<Inventory.ItemSlot>>();

		// Token: 0x0400034B RID: 843
		public static readonly List<InvSlotType> AnySlot = new List<InvSlotType>
		{
			InvSlotType.Any
		};

		// Token: 0x0400034C RID: 844
		public static readonly List<InvSlotType> BagSlot = new List<InvSlotType>
		{
			InvSlotType.Bag
		};

		// Token: 0x0400034D RID: 845
		protected bool[] IsEquipped;

		// Token: 0x0200067F RID: 1663
		public enum AccessLevel
		{
			// Token: 0x040029B3 RID: 10675
			OnlyIfIncapacitated,
			// Token: 0x040029B4 RID: 10676
			AllowBotsAndPets,
			// Token: 0x040029B5 RID: 10677
			AllowFriendly
		}
	}
}
