using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C4 RID: 1220
	internal class ItemContainer : ItemComponent, IDrawableComponent
	{
		// Token: 0x17001299 RID: 4761
		// (get) Token: 0x0600457B RID: 17787 RVA: 0x001BD424 File Offset: 0x001BB624
		// (set) Token: 0x0600457C RID: 17788 RVA: 0x001BD42C File Offset: 0x001BB62C
		[Serialize(5, IsPropertySaveable.No, "How many items can be contained inside this item.", "", false)]
		public int Capacity
		{
			get
			{
				return this.capacity;
			}
			private set
			{
				this.capacity = Math.Max(value, 0);
				this.MainContainerCapacity = value;
			}
		}

		// Token: 0x1700129A RID: 4762
		// (get) Token: 0x0600457D RID: 17789 RVA: 0x001BD442 File Offset: 0x001BB642
		// (set) Token: 0x0600457E RID: 17790 RVA: 0x001BD44A File Offset: 0x001BB64A
		public int MainContainerCapacity { get; private set; }

		// Token: 0x1700129B RID: 4763
		// (get) Token: 0x0600457F RID: 17791 RVA: 0x001BD453 File Offset: 0x001BB653
		// (set) Token: 0x06004580 RID: 17792 RVA: 0x001BD45B File Offset: 0x001BB65B
		[Serialize(64, IsPropertySaveable.No, "How many items can be stacked in one slot. Does not increase the maximum stack size of the items themselves, e.g. a stack of bullets could have a maximum size of 8 but the number of bullets in a specific weapon could be restricted to 6.", "", false)]
		public int MaxStackSize
		{
			get
			{
				return this.maxStackSize;
			}
			set
			{
				this.maxStackSize = Math.Max(value, 1);
			}
		}

		// Token: 0x1700129C RID: 4764
		// (get) Token: 0x06004581 RID: 17793 RVA: 0x001BD46A File Offset: 0x001BB66A
		// (set) Token: 0x06004582 RID: 17794 RVA: 0x001BD472 File Offset: 0x001BB672
		[Serialize(true, IsPropertySaveable.No, "Should the items contained inside this item be hidden. If set to false, you should use the ItemPos and ItemInterval properties to determine where the items get rendered.", "", false)]
		public bool HideItems
		{
			get
			{
				return this.hideItems;
			}
			set
			{
				this.hideItems = value;
				base.Drawable = !this.hideItems;
			}
		}

		// Token: 0x1700129D RID: 4765
		// (get) Token: 0x06004583 RID: 17795 RVA: 0x001BD48A File Offset: 0x001BB68A
		// (set) Token: 0x06004584 RID: 17796 RVA: 0x001BD492 File Offset: 0x001BB692
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position where the contained items get drawn at (offset from the upper left corner of the sprite in pixels).", "", false)]
		public Vector2 ItemPos { get; set; }

		// Token: 0x1700129E RID: 4766
		// (get) Token: 0x06004585 RID: 17797 RVA: 0x001BD49B File Offset: 0x001BB69B
		// (set) Token: 0x06004586 RID: 17798 RVA: 0x001BD4A3 File Offset: 0x001BB6A3
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The interval at which the contained items are spaced apart from each other (in pixels).", "", false)]
		public Vector2 ItemInterval { get; set; }

		// Token: 0x1700129F RID: 4767
		// (get) Token: 0x06004587 RID: 17799 RVA: 0x001BD4AC File Offset: 0x001BB6AC
		// (set) Token: 0x06004588 RID: 17800 RVA: 0x001BD4B4 File Offset: 0x001BB6B4
		[Serialize(100, IsPropertySaveable.No, "How many items are placed in a row before starting a new row.", "", false)]
		public int ItemsPerRow { get; set; }

		// Token: 0x170012A0 RID: 4768
		// (get) Token: 0x06004589 RID: 17801 RVA: 0x001BD4BD File Offset: 0x001BB6BD
		// (set) Token: 0x0600458A RID: 17802 RVA: 0x001BD4C5 File Offset: 0x001BB6C5
		[Serialize(false, IsPropertySaveable.No, "Should items be drawn based on their position within the inventory?", "", false)]
		public bool ItemsUseInventoryPlacement { get; set; }

		// Token: 0x170012A1 RID: 4769
		// (get) Token: 0x0600458B RID: 17803 RVA: 0x001BD4CE File Offset: 0x001BB6CE
		// (set) Token: 0x0600458C RID: 17804 RVA: 0x001BD4D6 File Offset: 0x001BB6D6
		[Serialize(true, IsPropertySaveable.No, "Should the inventory of this item be visible when the item is selected. Note that this does not prevent dragging and dropping items to the item.", "", false)]
		public bool DrawInventory { get; set; }

		// Token: 0x170012A2 RID: 4770
		// (get) Token: 0x0600458D RID: 17805 RVA: 0x001BD4DF File Offset: 0x001BB6DF
		// (set) Token: 0x0600458E RID: 17806 RVA: 0x001BD4E7 File Offset: 0x001BB6E7
		[Serialize(true, IsPropertySaveable.No, "Allow dragging and dropping items to deposit items into this inventory.", "", false)]
		public bool AllowDragAndDrop { get; set; }

		// Token: 0x170012A3 RID: 4771
		// (get) Token: 0x0600458F RID: 17807 RVA: 0x001BD4F0 File Offset: 0x001BB6F0
		// (set) Token: 0x06004590 RID: 17808 RVA: 0x001BD4F8 File Offset: 0x001BB6F8
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool AllowSwappingContainedItems { get; set; }

		// Token: 0x170012A4 RID: 4772
		// (get) Token: 0x06004591 RID: 17809 RVA: 0x001BD501 File Offset: 0x001BB701
		// (set) Token: 0x06004592 RID: 17810 RVA: 0x001BD509 File Offset: 0x001BB709
		[Serialize(true, IsPropertySaveable.No, "Should a button that allows sorting the items alphabetically be shown in the container's UI panel?", "", false)]
		public bool ShowSortButton { get; set; }

		// Token: 0x170012A5 RID: 4773
		// (get) Token: 0x06004593 RID: 17811 RVA: 0x001BD512 File Offset: 0x001BB712
		// (set) Token: 0x06004594 RID: 17812 RVA: 0x001BD51A File Offset: 0x001BB71A
		[Serialize(true, IsPropertySaveable.No, "Should a button that merges items into stacks be shown in the container's UI panel?", "", false)]
		public bool ShowMergeButton { get; set; }

		// Token: 0x170012A6 RID: 4774
		// (get) Token: 0x06004595 RID: 17813 RVA: 0x001BD523 File Offset: 0x001BB723
		// (set) Token: 0x06004596 RID: 17814 RVA: 0x001BD52B File Offset: 0x001BB72B
		[Serialize(true, IsPropertySaveable.Yes, "When this item is equipped, and you 'quick use' (double click / equip button) another equippable item, should the game attempt to move that item inside this one?", "", false)]
		public bool QuickUseMovesItemsInside { get; set; }

		// Token: 0x170012A7 RID: 4775
		// (get) Token: 0x06004597 RID: 17815 RVA: 0x001BD534 File Offset: 0x001BB734
		// (set) Token: 0x06004598 RID: 17816 RVA: 0x001BD53C File Offset: 0x001BB73C
		[Serialize(false, IsPropertySaveable.No, "If set to true, interacting with this item will make the character interact with the contained item(s), automatically picking them up if they can be picked up.", "", false)]
		public bool AutoInteractWithContained { get; set; }

		// Token: 0x170012A8 RID: 4776
		// (get) Token: 0x06004599 RID: 17817 RVA: 0x001BD545 File Offset: 0x001BB745
		// (set) Token: 0x0600459A RID: 17818 RVA: 0x001BD557 File Offset: 0x001BB757
		[Serialize("", IsPropertySaveable.Yes, "Interacting with this container will autointeract with contained items that have one of these tags. Only valid if AutoInteractWithContained is set to true.", "", false)]
		public string AutoInteractWithContainedTags
		{
			get
			{
				return this.autoInteractWithContainedTags.ConvertToString(",");
			}
			set
			{
				this.autoInteractWithContainedTags = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			}
		}

		// Token: 0x170012A9 RID: 4777
		// (get) Token: 0x0600459B RID: 17819 RVA: 0x001BD56F File Offset: 0x001BB76F
		// (set) Token: 0x0600459C RID: 17820 RVA: 0x001BD577 File Offset: 0x001BB777
		[Serialize(true, IsPropertySaveable.No, "Is the container accessible in general.", "", false)]
		public bool AllowAccess { get; set; }

		// Token: 0x170012AA RID: 4778
		// (get) Token: 0x0600459D RID: 17821 RVA: 0x001BD580 File Offset: 0x001BB780
		// (set) Token: 0x0600459E RID: 17822 RVA: 0x001BD588 File Offset: 0x001BB788
		[Serialize(false, IsPropertySaveable.No, "Is the container only accessible when it's broken. Doesn't apply to editors.", "", false)]
		public bool AccessOnlyWhenBroken { get; set; }

		// Token: 0x170012AB RID: 4779
		// (get) Token: 0x0600459F RID: 17823 RVA: 0x001BD591 File Offset: 0x001BB791
		// (set) Token: 0x060045A0 RID: 17824 RVA: 0x001BD599 File Offset: 0x001BB799
		[Serialize(true, IsPropertySaveable.No, "Is the container accessible when dropped.", "", false)]
		public bool AllowAccessWhenDropped { get; set; }

		// Token: 0x170012AC RID: 4780
		// (get) Token: 0x060045A1 RID: 17825 RVA: 0x001BD5A2 File Offset: 0x001BB7A2
		// (set) Token: 0x060045A2 RID: 17826 RVA: 0x001BD5AA File Offset: 0x001BB7AA
		[Serialize(5, IsPropertySaveable.No, "How many inventory slots the inventory has per row.", "", false)]
		public int SlotsPerRow { get; set; }

		// Token: 0x170012AD RID: 4781
		// (get) Token: 0x060045A3 RID: 17827 RVA: 0x001BD5B3 File Offset: 0x001BB7B3
		// (set) Token: 0x060045A4 RID: 17828 RVA: 0x001BD5C8 File Offset: 0x001BB7C8
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Define items (by identifiers or tags) that bots should place inside this container. If empty, no restrictions are applied.", "", false)]
		public string ContainableRestrictions
		{
			get
			{
				return string.Join<Identifier>(",", this.containableRestrictions);
			}
			set
			{
				this.containableRestrictions.Clear();
				if (!value.IsNullOrEmpty())
				{
					foreach (string str in value.Split(',', StringSplitOptions.None))
					{
						if (!str.IsNullOrWhiteSpace())
						{
							this.containableRestrictions.Add(str.ToIdentifier());
						}
					}
				}
			}
		}

		// Token: 0x170012AE RID: 4782
		// (get) Token: 0x060045A5 RID: 17829 RVA: 0x001BD61E File Offset: 0x001BB81E
		// (set) Token: 0x060045A6 RID: 17830 RVA: 0x001BD626 File Offset: 0x001BB826
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should this container be automatically filled with items?", "", false)]
		public bool AutoFill { get; set; }

		// Token: 0x170012AF RID: 4783
		// (get) Token: 0x060045A7 RID: 17831 RVA: 0x001BD62F File Offset: 0x001BB82F
		// (set) Token: 0x060045A8 RID: 17832 RVA: 0x001BD63C File Offset: 0x001BB83C
		[Serialize(0f, IsPropertySaveable.No, "The rotation in which the contained sprites are drawn (in degrees).", "", false)]
		public float ItemRotation
		{
			get
			{
				return MathHelper.ToDegrees(this.itemRotation);
			}
			set
			{
				this.itemRotation = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170012B0 RID: 4784
		// (get) Token: 0x060045A9 RID: 17833 RVA: 0x001BD64A File Offset: 0x001BB84A
		// (set) Token: 0x060045AA RID: 17834 RVA: 0x001BD652 File Offset: 0x001BB852
		[Serialize("", IsPropertySaveable.No, "Specify an item for the container to spawn with.", "", false)]
		public string SpawnWithId { get; set; }

		// Token: 0x170012B1 RID: 4785
		// (get) Token: 0x060045AB RID: 17835 RVA: 0x001BD65B File Offset: 0x001BB85B
		// (set) Token: 0x060045AC RID: 17836 RVA: 0x001BD663 File Offset: 0x001BB863
		[Serialize(false, IsPropertySaveable.No, "Should the items configured using SpawnWithId spawn if this item is broken.", "", false)]
		public bool SpawnWithIdWhenBroken { get; set; }

		// Token: 0x170012B2 RID: 4786
		// (get) Token: 0x060045AD RID: 17837 RVA: 0x001BD66C File Offset: 0x001BB86C
		// (set) Token: 0x060045AE RID: 17838 RVA: 0x001BD674 File Offset: 0x001BB874
		[Serialize(false, IsPropertySaveable.No, "Should the items be injected into the user.", "", false)]
		public bool AutoInject { get; set; }

		// Token: 0x170012B3 RID: 4787
		// (get) Token: 0x060045AF RID: 17839 RVA: 0x001BD67D File Offset: 0x001BB87D
		// (set) Token: 0x060045B0 RID: 17840 RVA: 0x001BD685 File Offset: 0x001BB885
		[Serialize(0.5f, IsPropertySaveable.No, "The health threshold that the user must reach in order to activate the autoinjection.", "", false)]
		public float AutoInjectThreshold { get; set; }

		// Token: 0x170012B4 RID: 4788
		// (get) Token: 0x060045B1 RID: 17841 RVA: 0x001BD68E File Offset: 0x001BB88E
		// (set) Token: 0x060045B2 RID: 17842 RVA: 0x001BD696 File Offset: 0x001BB896
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RemoveContainedItemsOnDeconstruct { get; set; }

		// Token: 0x170012B5 RID: 4789
		// (get) Token: 0x060045B3 RID: 17843 RVA: 0x001BD69F File Offset: 0x001BB89F
		// (set) Token: 0x060045B4 RID: 17844 RVA: 0x001BD6AC File Offset: 0x001BB8AC
		public bool Locked
		{
			get
			{
				return this.Inventory.Locked;
			}
			set
			{
				this.Inventory.Locked = value;
			}
		}

		// Token: 0x170012B6 RID: 4790
		// (get) Token: 0x060045B5 RID: 17845 RVA: 0x001BD6BA File Offset: 0x001BB8BA
		public int ContainedItemCount
		{
			get
			{
				return this.Inventory.AllItems.Count<Item>();
			}
		}

		// Token: 0x170012B7 RID: 4791
		// (get) Token: 0x060045B6 RID: 17846 RVA: 0x001BD6CC File Offset: 0x001BB8CC
		public int ContainedNonBrokenItemCount
		{
			get
			{
				return this.Inventory.AllItems.Count((Item it) => it.Condition > 0f);
			}
		}

		// Token: 0x170012B8 RID: 4792
		// (get) Token: 0x060045B7 RID: 17847 RVA: 0x001BD6FD File Offset: 0x001BB8FD
		// (set) Token: 0x060045B8 RID: 17848 RVA: 0x001BD70A File Offset: 0x001BB90A
		public int ExtraStackSize
		{
			get
			{
				return this.Inventory.ExtraStackSize;
			}
			set
			{
				this.Inventory.ExtraStackSize = value;
			}
		}

		// Token: 0x060045B9 RID: 17849 RVA: 0x001BD718 File Offset: 0x001BB918
		public bool ShouldBeContained(string[] identifiersOrTags, out bool isRestrictionsDefined)
		{
			isRestrictionsDefined = this.containableRestrictions.Any<Identifier>();
			return !this.slotRestrictions.None((ItemContainer.SlotRestrictions s) => s.MatchesItem(this.item)) && (!isRestrictionsDefined || identifiersOrTags.Any((string id) => this.containableRestrictions.Any((Identifier r) => r == id)));
		}

		// Token: 0x060045BA RID: 17850 RVA: 0x001BD76C File Offset: 0x001BB96C
		public bool ShouldBeContained(Item item, out bool isRestrictionsDefined)
		{
			isRestrictionsDefined = this.containableRestrictions.Any<Identifier>();
			return !this.slotRestrictions.None((ItemContainer.SlotRestrictions s) => s.MatchesItem(item)) && (!isRestrictionsDefined || this.containableRestrictions.Any((Identifier id) => item.Prefab.Identifier == id || item.HasTag(id)));
		}

		// Token: 0x170012B9 RID: 4793
		// (get) Token: 0x060045BB RID: 17851 RVA: 0x001BD7D0 File Offset: 0x001BB9D0
		public ImmutableHashSet<Identifier> ContainableItemIdentifiers
		{
			get
			{
				return this.containableItemIdentifiers;
			}
		}

		// Token: 0x170012BA RID: 4794
		// (get) Token: 0x060045BC RID: 17852 RVA: 0x001BD7D8 File Offset: 0x001BB9D8
		public List<RelatedItem> ContainableItems { get; }

		// Token: 0x170012BB RID: 4795
		// (get) Token: 0x060045BD RID: 17853 RVA: 0x001BD7E0 File Offset: 0x001BB9E0
		public List<RelatedItem> AllSubContainableItems { get; }

		// Token: 0x060045BE RID: 17854 RVA: 0x001BD7E8 File Offset: 0x001BB9E8
		public ItemContainer(Item item, ContentXElement element) : base(item, element)
		{
			int totalCapacity = this.capacity;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "containable"))
				{
					if (a == "subcontainer")
					{
						totalCapacity += subElement.GetAttributeInt("capacity", 1);
						this.HasSubContainers = true;
					}
				}
				else
				{
					RelatedItem containable = RelatedItem.Load(subElement, false, item.Name);
					if (containable == null)
					{
						string str = "Error in item config \"";
						ContentPath configFilePath = item.ConfigFilePath;
						DebugConsole.ThrowError(str + ((configFilePath != null) ? configFilePath.ToString() : null) + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
					}
					else
					{
						if (this.ContainableItems == null)
						{
							this.ContainableItems = new List<RelatedItem>();
						}
						this.ContainableItems.Add(containable);
					}
				}
			}
			this.Inventory = new ItemInventory(item, this, totalCapacity, this.SlotsPerRow);
			this.ExtraStackSize = element.GetAttributeInt("ExtraStackSize", 0);
			List<ItemContainer.SlotRestrictions> newSlotRestrictions = new List<ItemContainer.SlotRestrictions>(totalCapacity);
			for (int i = 0; i < this.capacity; i++)
			{
				newSlotRestrictions.Add(new ItemContainer.SlotRestrictions(this.maxStackSize, this.ContainableItems, false));
			}
			int subContainerIndex = this.capacity;
			foreach (ContentXElement subElement2 in element.Elements())
			{
				if (!(subElement2.Name.ToString().ToLowerInvariant() != "subcontainer"))
				{
					int subCapacity = subElement2.GetAttributeInt("capacity", 1);
					int subMaxStackSize = subElement2.GetAttributeInt("maxstacksize", this.maxStackSize);
					bool autoInject = subElement2.GetAttributeBool("autoinject", false);
					this.subContainersCanAutoInject = (this.subContainersCanAutoInject || autoInject);
					List<RelatedItem> subContainableItems = new List<RelatedItem>();
					foreach (ContentXElement subSubElement in subElement2.Elements())
					{
						if (!(subSubElement.Name.ToString().ToLowerInvariant() != "containable"))
						{
							RelatedItem containable2 = RelatedItem.Load(subSubElement, false, item.Name);
							if (containable2 == null)
							{
								string str2 = "Error in item config \"";
								ContentPath configFilePath2 = item.ConfigFilePath;
								DebugConsole.ThrowError(str2 + ((configFilePath2 != null) ? configFilePath2.ToString() : null) + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
							}
							else
							{
								subContainableItems.Add(containable2);
								if (this.AllSubContainableItems == null)
								{
									this.AllSubContainableItems = new List<RelatedItem>();
								}
								this.AllSubContainableItems.Add(containable2);
							}
						}
					}
					for (int j = subContainerIndex; j < subContainerIndex + subCapacity; j++)
					{
						newSlotRestrictions.Add(new ItemContainer.SlotRestrictions(subMaxStackSize, subContainableItems, autoInject));
					}
					subContainerIndex += subCapacity;
				}
			}
			this.capacity = totalCapacity;
			this.slotRestrictions = newSlotRestrictions.ToImmutableArray<ItemContainer.SlotRestrictions>();
		}

		// Token: 0x060045BF RID: 17855 RVA: 0x001BDB98 File Offset: 0x001BBD98
		public void ReloadContainableRestrictions(ContentXElement element)
		{
			int containableIndex = 0;
			foreach (ContentXElement subElement in element.GetChildElements("containable"))
			{
				RelatedItem containable = RelatedItem.Load(subElement, false, this.item.Name);
				if (containable == null)
				{
					DebugConsole.ThrowError("Error when loading containable restrictions for \"" + this.item.Name + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
				}
				else
				{
					this.ContainableItems[containableIndex] = containable;
					containableIndex++;
					if (containableIndex >= this.ContainableItems.Count)
					{
						break;
					}
				}
			}
			for (int i = 0; i < this.capacity; i++)
			{
				this.slotRestrictions[i].ContainableItems = this.ContainableItems;
			}
		}

		// Token: 0x060045C0 RID: 17856 RVA: 0x001BDC74 File Offset: 0x001BBE74
		public int GetMaxStackSize(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= this.capacity)
			{
				return 0;
			}
			return this.slotRestrictions[slotIndex].MaxStackSize;
		}

		// Token: 0x060045C1 RID: 17857 RVA: 0x001BDC98 File Offset: 0x001BBE98
		public void OnItemContained(Item containedItem, bool triggerOnInsertedEffects = true)
		{
			int index = this.Inventory.FindIndex(containedItem);
			RelatedItem relatedItem = null;
			if (index >= 0 && index < this.slotRestrictions.Length && this.slotRestrictions[index].ContainableItems != null)
			{
				this.activeContainedItems.RemoveAll((ItemContainer.ActiveContainedItem i) => i.Item == containedItem);
				foreach (RelatedItem containableItem in this.slotRestrictions[index].ContainableItems)
				{
					if (containableItem.MatchesItem(containedItem))
					{
						if (relatedItem == null)
						{
							relatedItem = containableItem;
						}
						foreach (StatusEffect effect in containableItem.StatusEffects)
						{
							ItemContainer.ActiveContainedItem activeContainedItem = new ItemContainer.ActiveContainedItem(containedItem, effect, containableItem.ExcludeBroken, containableItem.ExcludeFullCondition, containableItem.BlameEquipperForDeath);
							this.activeContainedItems.Add(activeContainedItem);
							if (triggerOnInsertedEffects && this.ShouldApplyEffects(activeContainedItem))
							{
								Submarine submarine = this.item.Submarine;
								if ((submarine == null || !submarine.Loading) && !this.initializingLoadedItems && !containedItem.OnInsertedEffectsApplied)
								{
									activeContainedItem.StatusEffect.Apply(ActionType.OnInserted, 1f, this.item, this.targets, null);
								}
							}
						}
						if (triggerOnInsertedEffects)
						{
							containedItem.OnInsertedEffectsApplied = true;
						}
					}
				}
			}
			ItemContainer.ContainedItem containedItemInfo = new ItemContainer.ContainedItem(containedItem, relatedItem != null && relatedItem.Hide, (relatedItem != null) ? relatedItem.ItemPos : null, (relatedItem != null) ? relatedItem.Rotation : 0f);
			this.containedItems.RemoveAll((ItemContainer.ContainedItem d) => d.Item == containedItem);
			if (this.hideItems)
			{
				this.containedItems.Add(containedItemInfo);
			}
			else
			{
				int containedIndex = 0;
				while (containedIndex < this.containedItems.Count && index > this.Inventory.FindIndex(this.containedItems[containedIndex].Item))
				{
					containedIndex++;
				}
				this.containedItems.Insert(containedIndex, containedItemInfo);
			}
			if (this.item.GetComponent<Planter>() != null)
			{
				string str = "MicroInteraction:";
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				GameAnalyticsManager.AddDesignEvent(str + (text ?? "null") + ":GardeningPlanted:" + containedItem.Prefab.Identifier.ToString());
			}
			bool isActive;
			if (!this.hasSignalConnections && this.activeContainedItems.Count <= 0)
			{
				isActive = this.Inventory.AllItems.Any((Item it) => it.body != null);
			}
			else
			{
				isActive = true;
			}
			this.IsActive = isActive;
			if (this.IsActive)
			{
				Character owner = this.item.GetRootInventoryOwner() as Character;
				if (owner != null)
				{
					if (owner.HasEquippedItem(this.item, null, (InvSlotType slot) => slot.HasFlag(InvSlotType.LeftHand) || slot.HasFlag(InvSlotType.RightHand)))
					{
						this.SetContainedActive(true);
					}
				}
			}
			if (containedItem.FlippedX != this.item.FlippedX)
			{
				containedItem.FlipX(false, false);
			}
			if (containedItem.FlippedY != this.item.FlippedY)
			{
				containedItem.FlipY(false, false);
			}
			this.item.SetContainedItemPositions();
			CharacterHUD.RecreateHudTextsIfFocused(new Item[]
			{
				this.item,
				containedItem
			});
			this.OnContainedItemsChanged.Invoke(this);
		}

		// Token: 0x060045C2 RID: 17858 RVA: 0x001BE0C4 File Offset: 0x001BC2C4
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			this.SetContainedItemPositions();
		}

		// Token: 0x060045C3 RID: 17859 RVA: 0x001BE0CC File Offset: 0x001BC2CC
		public void OnItemRemoved(Item containedItem)
		{
			foreach (ItemContainer.ActiveContainedItem activeContainedItem in this.activeContainedItems)
			{
				if (activeContainedItem.Item == containedItem && this.ShouldApplyEffects(activeContainedItem))
				{
					activeContainedItem.StatusEffect.Apply(ActionType.OnRemoved, 1f, this.item, this.targets, null);
				}
			}
			containedItem.OnInsertedEffectsApplied = false;
			this.activeContainedItems.RemoveAll((ItemContainer.ActiveContainedItem i) => i.Item == containedItem);
			this.containedItems.RemoveAll((ItemContainer.ContainedItem i) => i.Item == containedItem);
			this.item.SetContainedItemPositions();
			bool isActive;
			if (!this.hasSignalConnections && this.activeContainedItems.Count <= 0)
			{
				isActive = this.Inventory.AllItems.Any((Item it) => it.body != null);
			}
			else
			{
				isActive = true;
			}
			this.IsActive = isActive;
			CharacterHUD.RecreateHudTextsIfFocused(new Item[]
			{
				this.item,
				containedItem
			});
			this.OnContainedItemsChanged.Invoke(this);
		}

		// Token: 0x060045C4 RID: 17860 RVA: 0x001BE224 File Offset: 0x001BC424
		public bool BlameEquipperForDeath()
		{
			return this.activeContainedItems.Any((ItemContainer.ActiveContainedItem c) => c.BlameEquipperForDeath);
		}

		// Token: 0x060045C5 RID: 17861 RVA: 0x001BE250 File Offset: 0x001BC450
		public bool CanBeContained(Item item)
		{
			if (!this.AllowAccessWhenDropped)
			{
				PhysicsBody body = this.item.body;
				if (body != null && body.Enabled)
				{
					return false;
				}
			}
			return this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(item));
		}

		// Token: 0x060045C6 RID: 17862 RVA: 0x001BE2A4 File Offset: 0x001BC4A4
		public bool CanBeContained(Item item, int index)
		{
			if (index < 0 || index >= this.capacity)
			{
				return false;
			}
			if (!this.AllowAccessWhenDropped)
			{
				PhysicsBody body = this.item.body;
				if (body != null && body.Enabled)
				{
					return false;
				}
			}
			return this.slotRestrictions[index].MatchesItem(item);
		}

		// Token: 0x060045C7 RID: 17863 RVA: 0x001BE2F4 File Offset: 0x001BC4F4
		public bool CanBeContained(ItemPrefab itemPrefab)
		{
			return this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(itemPrefab));
		}

		// Token: 0x060045C8 RID: 17864 RVA: 0x001BE325 File Offset: 0x001BC525
		public bool CanBeContained(ItemPrefab itemPrefab, int index)
		{
			return index >= 0 && index < this.capacity && this.slotRestrictions[index].MatchesItem(itemPrefab);
		}

		// Token: 0x060045C9 RID: 17865 RVA: 0x001BE348 File Offset: 0x001BC548
		public bool ContainsItemsWithSameIdentifier(Item item)
		{
			if (item == null)
			{
				return false;
			}
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				if (containedItem.Prefab.Identifier == item.Prefab.Identifier)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060045CA RID: 17866 RVA: 0x001BE3BC File Offset: 0x001BC5BC
		public override void FlipX(bool relativeToSub)
		{
			base.FlipX(relativeToSub);
			if (this.HideItems)
			{
				return;
			}
			if (this.item.body == null)
			{
				return;
			}
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				if (containedItem.body != null && containedItem.body.Enabled && containedItem.body.Dir != this.item.body.Dir)
				{
					containedItem.FlipX(relativeToSub, false);
				}
			}
		}

		// Token: 0x060045CB RID: 17867 RVA: 0x001BE460 File Offset: 0x001BC660
		public override void Update(float deltaTime, Camera cam)
		{
			if (!string.IsNullOrEmpty(this.SpawnWithId) && !this.alwaysContainedItemsSpawned)
			{
				this.SpawnAlwaysContainedItems();
				this.alwaysContainedItemsSpawned = true;
			}
			if (this.hasSignalConnections)
			{
				float totalConditionValue = 0f;
				float totalConditionPercentage = 0f;
				int totalItems = 0;
				foreach (Item item in this.Inventory.AllItems)
				{
					if (!MathUtils.NearlyEqual(item.Condition, 0f, 0.0001f))
					{
						totalConditionValue += item.Condition;
						totalConditionPercentage += item.ConditionPercentage;
						totalItems++;
					}
				}
				if (!MathUtils.NearlyEqual(totalConditionValue, this.prevTotalConditionValue, 0.0001f))
				{
					this.totalConditionValueString = ((int)totalConditionValue).ToString(CultureInfo.InvariantCulture);
					this.prevTotalConditionValue = totalConditionValue;
				}
				if (!MathUtils.NearlyEqual(totalConditionPercentage, this.prevTotalConditionPercentage, 0.0001f))
				{
					this.totalConditionPercentageString = ((int)totalConditionPercentage).ToString(CultureInfo.InvariantCulture);
					this.prevTotalConditionPercentage = totalConditionPercentage;
				}
				if (totalItems != this.prevTotalItems)
				{
					this.totalItemsString = totalItems.ToString(CultureInfo.InvariantCulture);
					this.prevTotalItems = totalItems;
				}
				this.item.SendSignal(this.totalConditionValueString, "contained_conditions");
				this.item.SendSignal(this.totalConditionPercentageString, "contained_conditions_percentage");
				this.item.SendSignal(this.totalItemsString, "contained_items");
			}
			CharacterInventory ownerInventory = this.item.ParentInventory as CharacterInventory;
			if (ownerInventory != null)
			{
				this.SetContainedItemPositionsIfNeeded();
				if (this.AutoInject || this.subContainersCanAutoInject)
				{
					this.autoInjectCooldown -= deltaTime;
					if (this.autoInjectCooldown <= 0f)
					{
						Entity entity = (ownerInventory != null) ? ownerInventory.Owner : null;
						Character ownerCharacter = entity as Character;
						if (ownerCharacter != null && !ownerCharacter.IsDead && ownerCharacter.HealthPercentage / 100f <= this.AutoInjectThreshold && ownerCharacter.HasEquippedItem(this.item, null, null))
						{
							if (this.AutoInject)
							{
								this.Inventory.AllItemsMod.ForEach(delegate(Item i)
								{
									base.<Update>g__Inject|1(i);
								});
							}
							else
							{
								Action<Item> <>9__2;
								for (int j = 0; j < this.slotRestrictions.Length; j++)
								{
									if (this.slotRestrictions[j].AutoInject)
									{
										IEnumerable<Item> itemsAt = this.Inventory.GetItemsAt(j);
										Action<Item> action;
										if ((action = <>9__2) == null)
										{
											action = (<>9__2 = delegate(Item i)
											{
												base.<Update>g__Inject|1(i);
											});
										}
										itemsAt.ForEachMod(action);
									}
								}
							}
							this.autoInjectCooldown = 1f;
						}
					}
				}
			}
			else if (this.item.body != null && this.item.body.Enabled)
			{
				if (this.item.body.FarseerBody.Awake)
				{
					this.SetContainedItemPositionsIfNeeded();
				}
			}
			else if (!this.hasSignalConnections && this.activeContainedItems.Count == 0)
			{
				this.IsActive = false;
				return;
			}
			foreach (ItemContainer.ActiveContainedItem activeContainedItem in this.activeContainedItems)
			{
				if (this.ShouldApplyEffects(activeContainedItem))
				{
					StatusEffect effect = activeContainedItem.StatusEffect;
					effect.Apply(ActionType.OnActive, deltaTime, this.item, this.targets, null);
					effect.Apply(ActionType.OnContaining, deltaTime, this.item, this.targets, null);
					Wearable component = this.item.GetComponent<Wearable>();
					if (component != null && component.IsActive)
					{
						effect.Apply(ActionType.OnWearing, deltaTime, this.item, this.targets, null);
					}
				}
			}
		}

		// Token: 0x060045CC RID: 17868 RVA: 0x001BE870 File Offset: 0x001BCA70
		private bool ShouldApplyEffects(ItemContainer.ActiveContainedItem activeContainedItem)
		{
			Item contained = activeContainedItem.Item;
			if (activeContainedItem.ExcludeBroken && contained.Condition <= 0f)
			{
				return false;
			}
			if (activeContainedItem.ExcludeFullCondition && contained.IsFullCondition)
			{
				return false;
			}
			StatusEffect effect = activeContainedItem.StatusEffect;
			this.targets.Clear();
			if (effect.HasTargetType(StatusEffect.TargetType.This))
			{
				this.targets.AddRange(this.item.AllPropertyObjects);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Contained))
			{
				this.targets.AddRange(contained.AllPropertyObjects);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Character))
			{
				Inventory parentInventory = this.item.ParentInventory;
				Character character = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
				if (character != null)
				{
					this.targets.Add(character);
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
			{
				effect.AddNearbyTargets(this.item.WorldPosition, this.targets);
			}
			return true;
		}

		// Token: 0x060045CD RID: 17869 RVA: 0x001BE960 File Offset: 0x001BCB60
		private void SetContainedItemPositionsIfNeeded()
		{
			if (Vector2.DistanceSquared(this.prevContainedItemRefreshPosition, this.item.Position) <= 10f)
			{
				float num = this.prevContainedItemRefreshRotation;
				PhysicsBody body = this.item.body;
				if (Math.Abs((num - ((body != null) ? new float?(body.Rotation) : null)) ?? this.item.RotationRad) <= 0.01f)
				{
					return;
				}
			}
			this.SetContainedItemPositions();
			this.prevContainedItemRefreshPosition = this.item.Position;
			PhysicsBody body2 = this.item.body;
			this.prevContainedItemRefreshRotation = ((body2 != null) ? body2.Rotation : this.item.RotationRad);
		}

		// Token: 0x060045CE RID: 17870 RVA: 0x001BEA40 File Offset: 0x001BCC40
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			if (this.IsActive)
			{
				this.Update(deltaTime, cam);
			}
		}

		// Token: 0x060045CF RID: 17871 RVA: 0x001BEA52 File Offset: 0x001BCC52
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			return this.IsAccessible() && base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x060045D0 RID: 17872 RVA: 0x001BEA68 File Offset: 0x001BCC68
		public bool IsAccessible()
		{
			if (!this.AllowAccess)
			{
				return false;
			}
			if (this.AccessOnlyWhenBroken)
			{
				Screen selected = Screen.Selected;
				return (selected != null && selected.IsEditor) || this.item.Condition <= 0f;
			}
			return true;
		}

		// Token: 0x060045D1 RID: 17873 RVA: 0x001BEAB4 File Offset: 0x001BCCB4
		public override bool Select(Character character)
		{
			if (this.item.Container != null)
			{
				return false;
			}
			if (!this.IsAccessible())
			{
				return false;
			}
			if (this.AutoInteractWithContained && character.SelectedItem == null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					foreach (Item contained in this.Inventory.AllItems)
					{
						if (this.CanAutoInteractWithContained(contained) && contained.TryInteract(character, false, false, false))
						{
							character.FocusedItem = contained;
							return false;
						}
					}
				}
			}
			AbilityItemContainer abilityItem = new AbilityItemContainer(this.item);
			character.CheckTalents(AbilityEffectType.OnOpenItemContainer, abilityItem);
			Inventory parentInventory = this.item.ParentInventory;
			return ((parentInventory != null) ? parentInventory.Owner : null) != character && base.Select(character);
		}

		// Token: 0x060045D2 RID: 17874 RVA: 0x001BEB98 File Offset: 0x001BCD98
		public override bool Pick(Character picker)
		{
			if (!this.IsAccessible())
			{
				return false;
			}
			if (this.AutoInteractWithContained)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					foreach (Item contained in this.Inventory.AllItems)
					{
						if (this.CanAutoInteractWithContained(contained) && contained.TryInteract(picker, false, false, false))
						{
							picker.FocusedItem = contained;
							return true;
						}
					}
				}
			}
			this.IsActive = true;
			return picker != null;
		}

		// Token: 0x060045D3 RID: 17875 RVA: 0x001BEC34 File Offset: 0x001BCE34
		public override bool Combine(Item item, Character user)
		{
			if (!this.AllowDragAndDrop && user != null)
			{
				return false;
			}
			if (!this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(item)))
			{
				return false;
			}
			if (user != null && !user.CanAccessInventory(this.Inventory, CharacterInventory.AccessLevel.AllowBotsAndPets))
			{
				return false;
			}
			if (base.Item.GetComponent<GeneticMaterial>() != null)
			{
				return false;
			}
			if (this.Inventory.TryPutItem(item, user, null, true, false, true))
			{
				this.IsActive = true;
				if (this.hideItems && item.body != null)
				{
					item.body.Enabled = false;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060045D4 RID: 17876 RVA: 0x001BECE1 File Offset: 0x001BCEE1
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.IsActive = true;
			this.SetContainedActive(false);
		}

		// Token: 0x060045D5 RID: 17877 RVA: 0x001BECF4 File Offset: 0x001BCEF4
		public override void Equip(Character character)
		{
			this.IsActive = true;
			if (character != null)
			{
				if (character.HasEquippedItem(this.item, null, (InvSlotType slot) => slot.HasFlag(InvSlotType.LeftHand) || slot.HasFlag(InvSlotType.RightHand)))
				{
					this.SetContainedActive(true);
					return;
				}
			}
			this.SetContainedActive(false);
		}

		// Token: 0x060045D6 RID: 17878 RVA: 0x001BED50 File Offset: 0x001BCF50
		private bool CanAutoInteractWithContained(Item containedItem)
		{
			return this.AutoInteractWithContained && (this.autoInteractWithContainedTags.None(null) || this.autoInteractWithContainedTags.Any((Identifier t) => containedItem.HasTag(t)));
		}

		// Token: 0x060045D7 RID: 17879 RVA: 0x001BED9C File Offset: 0x001BCF9C
		private void SetContainedActive(bool active)
		{
			if (this.ContainableItems != null)
			{
				if (this.ContainableItems.Any((RelatedItem c) => c.SetActive))
				{
					goto IL_69;
				}
			}
			if (this.AllSubContainableItems != null)
			{
				if (this.AllSubContainableItems.Any((RelatedItem c) => c.SetActive))
				{
					goto IL_69;
				}
			}
			return;
			IL_69:
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				RelatedItem containableItem = this.FindContainableItem(containedItem);
				if (containableItem != null && containableItem.SetActive)
				{
					foreach (ItemComponent ic in containedItem.Components)
					{
						ic.IsActive = active;
					}
					if (containedItem.body != null)
					{
						containedItem.body.Enabled = active;
						if (active)
						{
							containedItem.body.PhysEnabled = false;
						}
					}
				}
			}
			if (active)
			{
				this.FlipX(false);
			}
		}

		// Token: 0x060045D8 RID: 17880 RVA: 0x001BEED4 File Offset: 0x001BD0D4
		private RelatedItem FindContainableItem(Item item)
		{
			int index = this.Inventory.FindIndex(item);
			if (index == -1)
			{
				return null;
			}
			ItemContainer.SlotRestrictions slotRestrictions = this.slotRestrictions[index];
			if (slotRestrictions == null)
			{
				return null;
			}
			List<RelatedItem> containableItems = slotRestrictions.ContainableItems;
			if (containableItems == null)
			{
				return null;
			}
			return containableItems.FirstOrDefault((RelatedItem ci) => ci.MatchesItem(item));
		}

		// Token: 0x060045D9 RID: 17881 RVA: 0x001BEF34 File Offset: 0x001BD134
		public int? FindSuitableSubContainerIndex(Identifier itemTagOrIdentifier)
		{
			for (int i = 0; i < this.slotRestrictions.Length; i++)
			{
				if (this.slotRestrictions[i].MatchesItem(itemTagOrIdentifier))
				{
					return new int?(i);
				}
			}
			return null;
		}

		// Token: 0x060045DA RID: 17882 RVA: 0x001BEF7C File Offset: 0x001BD17C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if ((name == "activate" || name == "use" || name == "trigger_in") && signal.value != "0")
			{
				this.item.Use(1f, signal.sender, null, null, null);
			}
		}

		// Token: 0x060045DB RID: 17883 RVA: 0x001BEFE4 File Offset: 0x001BD1E4
		public void SetContainedItemPositions()
		{
			if (this.containedItems.Count == 0)
			{
				return;
			}
			Item rootContainer = this.item.RootContainer;
			PhysicsBody rootBody = ((rootContainer != null) ? rootContainer.body : null) ?? this.item.body;
			Vector2 transformedItemIntervalHorizontal;
			Vector2 transformedItemIntervalVertical;
			bool flippedX;
			bool flippedY;
			Vector2 transformedItemPos = this.GetContainedPosition(false, out transformedItemIntervalHorizontal, out transformedItemIntervalVertical, out flippedX, out flippedY);
			int i = 0;
			Vector2 currentItemPos = transformedItemPos;
			foreach (ItemContainer.ContainedItem contained in this.containedItems)
			{
				Vector2 itemPos = currentItemPos;
				if (contained.ItemPos != null)
				{
					Vector2 pos = contained.ItemPos.Value;
					if (this.item.body != null)
					{
						Matrix transform = Matrix.CreateRotationZ(this.item.body.Rotation);
						pos.X *= rootBody.Dir;
						itemPos = Vector2.Transform(pos, transform) + this.item.body.Position;
					}
					else
					{
						itemPos = pos;
						if (flippedX)
						{
							itemPos.X = -itemPos.X;
							itemPos.X += (float)this.item.Rect.Width;
						}
						if (flippedY)
						{
							itemPos.Y = -itemPos.Y;
							itemPos.Y -= (float)this.item.Rect.Height;
						}
						itemPos += new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y);
						if (Math.Abs(this.item.RotationRad) > 0.01f)
						{
							Matrix transform2 = Matrix.CreateRotationZ(this.item.RotationRad);
							itemPos = Vector2.Transform(itemPos - this.item.Position, transform2) + this.item.Position;
						}
					}
				}
				if (contained.Item.body != null)
				{
					try
					{
						Vector2 simPos = ConvertUnits.ToSimUnits(itemPos);
						float rotation = this.itemRotation;
						if (contained.Rotation != 0f)
						{
							rotation = MathHelper.ToRadians(contained.Rotation);
						}
						if (this.item.body != null)
						{
							rotation *= rootBody.Dir;
							rotation += this.item.body.Rotation;
						}
						else
						{
							if (flippedX ^ flippedY)
							{
								rotation = -rotation;
							}
							rotation += -this.item.RotationRad;
						}
						contained.Item.body.FarseerBody.SetTransformIgnoreContacts(ref simPos, rotation);
						contained.Item.body.UpdateDrawPosition(false);
					}
					catch (Exception e)
					{
						DebugConsole.Log("SetTransformIgnoreContacts threw an exception in SetContainedItemPositions (" + e.Message + ")\n" + e.StackTrace.CleanupStackTrace());
						GameAnalyticsManager.AddErrorEventOnce("ItemContainer.SetContainedItemPositions.InvalidPosition:" + contained.Item.Name, GameAnalyticsManager.ErrorSeverity.Error, "SetTransformIgnoreContacts threw an exception in SetContainedItemPositions (" + e.Message + ")\n" + e.StackTrace.CleanupStackTrace());
					}
					contained.Item.body.Submarine = this.item.Submarine;
				}
				contained.Item.Rect = new Rectangle((int)(itemPos.X - (float)contained.Item.Rect.Width / 2f), (int)(itemPos.Y + (float)contained.Item.Rect.Height / 2f), contained.Item.Rect.Width, contained.Item.Rect.Height);
				contained.Item.Submarine = this.item.Submarine;
				contained.Item.CurrentHull = this.item.CurrentHull;
				contained.Item.SetContainedItemPositions();
				foreach (LightComponent lightComponent in contained.Item.GetComponents<LightComponent>())
				{
					lightComponent.SetLightSourceTransform();
				}
				i++;
				if (Math.Abs(this.ItemInterval.X) > 0.001f && Math.Abs(this.ItemInterval.Y) > 0.001f)
				{
					currentItemPos += transformedItemIntervalHorizontal;
					if (i % this.ItemsPerRow == 0)
					{
						currentItemPos = transformedItemPos;
						currentItemPos += transformedItemIntervalVertical * (float)(i / this.ItemsPerRow);
					}
				}
				else
				{
					currentItemPos += transformedItemIntervalHorizontal + transformedItemIntervalVertical;
				}
			}
		}

		// Token: 0x060045DC RID: 17884 RVA: 0x001BF4CC File Offset: 0x001BD6CC
		private Vector2 GetContainedPosition(bool drawPosition, out Vector2 transformedItemIntervalHorizontal, out Vector2 transformedItemIntervalVertical, out bool flippedX, out bool flippedY)
		{
			Vector2 transformedItemPos = this.ItemPos * this.item.Scale;
			Vector2 transformedItemInterval = this.ItemInterval * this.item.Scale;
			transformedItemIntervalHorizontal = new Vector2(transformedItemInterval.X, 0f);
			transformedItemIntervalVertical = new Vector2(0f, transformedItemInterval.Y);
			if (this.item.RootContainer != null)
			{
				flippedX = (this.item.RootContainer.FlippedX && this.item.RootContainer.Prefab.CanSpriteFlipX);
				flippedY = (this.item.RootContainer.FlippedY && this.item.RootContainer.Prefab.CanSpriteFlipY);
			}
			else
			{
				flippedX = (this.item.FlippedX && this.item.Prefab.CanSpriteFlipX);
				flippedY = (this.item.FlippedY && this.item.Prefab.CanSpriteFlipY);
			}
			Item rootContainer = this.item.RootContainer;
			PhysicsBody rootBody = ((rootContainer != null) ? rootContainer.body : null) ?? this.item.body;
			bool bodyFlipped = rootBody != null && rootBody.Dir == -1f;
			if (this.ItemPos == Vector2.Zero && this.ItemInterval == Vector2.Zero && !drawPosition)
			{
				transformedItemPos = this.item.Position;
			}
			else if (this.item.body == null)
			{
				if (flippedX)
				{
					transformedItemPos.X = -transformedItemPos.X;
					transformedItemPos.X += (float)this.item.Rect.Width;
					transformedItemInterval.X = -transformedItemInterval.X;
					transformedItemIntervalHorizontal.X = -transformedItemIntervalHorizontal.X;
				}
				if (flippedY)
				{
					transformedItemPos.Y = -transformedItemPos.Y;
					transformedItemPos.Y -= (float)this.item.Rect.Height;
					transformedItemInterval.Y = -transformedItemInterval.Y;
					transformedItemIntervalVertical.Y = -transformedItemIntervalVertical.Y;
				}
				transformedItemPos += new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y);
				if (drawPosition && this.item.Submarine != null)
				{
					transformedItemPos += this.item.Submarine.DrawPosition;
				}
				if (Math.Abs(this.item.RotationRad) > 0.01f)
				{
					Matrix transform = Matrix.CreateRotationZ(-this.item.RotationRad);
					transformedItemPos = (drawPosition ? (Vector2.Transform(transformedItemPos - this.item.DrawPosition, transform) + this.item.DrawPosition) : (Vector2.Transform(transformedItemPos - this.item.Position, transform) + this.item.Position));
					transformedItemIntervalVertical = Vector2.Transform(transformedItemIntervalVertical, transform);
					transformedItemIntervalHorizontal = Vector2.Transform(transformedItemIntervalHorizontal, transform);
				}
			}
			else
			{
				Holdable component = this.item.GetComponent<Holdable>();
				if (component != null && component.Attachable)
				{
					transformedItemPos -= this.item.Rect.Size.FlipY().ToVector2() / 2f;
				}
				Matrix transform2 = Matrix.CreateRotationZ(drawPosition ? this.item.body.DrawRotation : this.item.body.Rotation);
				if (bodyFlipped)
				{
					transformedItemPos.X = -transformedItemPos.X;
					transformedItemInterval.X = -transformedItemInterval.X;
					transformedItemIntervalHorizontal.X = -transformedItemIntervalHorizontal.X;
				}
				transformedItemPos = Vector2.Transform(transformedItemPos, transform2);
				transformedItemIntervalVertical = Vector2.Transform(transformedItemIntervalVertical, transform2);
				transformedItemIntervalHorizontal = Vector2.Transform(transformedItemIntervalHorizontal, transform2);
				transformedItemPos += (drawPosition ? this.item.body.DrawPosition : this.item.body.Position);
			}
			return transformedItemPos;
		}

		// Token: 0x060045DD RID: 17885 RVA: 0x001BF900 File Offset: 0x001BDB00
		public override void OnItemLoaded()
		{
			this.Inventory.AllowSwappingContainedItems = this.AllowSwappingContainedItems;
			this.containableItemIdentifiers = this.slotRestrictions.SelectMany(delegate(ItemContainer.SlotRestrictions s)
			{
				List<RelatedItem> containableItems = s.ContainableItems;
				IEnumerable<Identifier> enumerable;
				if (containableItems == null)
				{
					enumerable = null;
				}
				else
				{
					enumerable = containableItems.SelectMany((RelatedItem ri) => ri.Identifiers);
				}
				return enumerable ?? Enumerable.Empty<Identifier>();
			}).ToImmutableHashSet<Identifier>();
			List<Connection> connections = this.item.Connections;
			bool flag;
			if (connections == null)
			{
				flag = false;
			}
			else
			{
				flag = connections.Any(delegate(Connection c)
				{
					string name = c.Name;
					return name == "contained_conditions" || name == "contained_conditions_percentage" || name == "contained_items";
				});
			}
			this.hasSignalConnections = flag;
			if (this.item.Submarine == null || !this.item.Submarine.Loading)
			{
				this.SpawnAlwaysContainedItems();
			}
		}

		// Token: 0x060045DE RID: 17886 RVA: 0x001BF9BC File Offset: 0x001BDBBC
		public override void OnMapLoaded()
		{
			if (this.itemIds != null)
			{
				this.initializingLoadedItems = true;
				ushort i = 0;
				while ((int)i < this.itemIds.Length)
				{
					if ((int)i >= this.Inventory.Capacity)
					{
						this.Inventory.TryPutItem(this.item, null, null, false, false, true);
					}
					else
					{
						foreach (ushort id in this.itemIds[(int)i])
						{
							Item item = Entity.FindEntityByID(id) as Item;
							if (item != null)
							{
								this.Inventory.TryPutItem(item, (int)i, false, false, null, false, true, true);
							}
						}
					}
					i += 1;
				}
				this.initializingLoadedItems = false;
				this.itemIds = null;
			}
			Submarine submarine = this.item.Submarine;
			if (((submarine != null) ? submarine.Info : null) != null && (this.item.Submarine.Info.IsOutpost || this.item.Submarine.Info.IsRuin))
			{
				if (this.SpawnWithId.Length > 0)
				{
					this.IsActive = true;
				}
			}
			else
			{
				this.SpawnAlwaysContainedItems();
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x060045DF RID: 17887 RVA: 0x001BFAF8 File Offset: 0x001BDCF8
		private void SpawnAlwaysContainedItems()
		{
			if (this.SpawnWithId.Length > 0 && (this.item.Condition > 0f || this.SpawnWithIdWhenBroken))
			{
				string[] splitIds = this.SpawnWithId.Split(',', StringSplitOptions.None);
				string[] array = splitIds;
				for (int i = 0; i < array.Length; i++)
				{
					string id = array[i];
					ItemPrefab prefab = ItemPrefab.Prefabs.Find((ItemPrefab m) => m.Identifier == id);
					if (prefab != null && this.Inventory != null && this.Inventory.CanProbablyBePut(prefab, null, null))
					{
						if (!false && (Entity.Spawner == null || Entity.Spawner.Removed) && GameMain.NetworkMember == null)
						{
							Item spawnedItem = new Item(prefab, Vector2.Zero, null, 0, true);
							this.Inventory.TryPutItem(spawnedItem, null, spawnedItem.AllowedSlots, false, false, true);
							this.alwaysContainedItemsSpawned = true;
						}
						else
						{
							this.IsActive = true;
							EntitySpawner spawner = Entity.Spawner;
							if (spawner != null)
							{
								spawner.AddItemToSpawnQueue(prefab, this.Inventory, null, null, delegate(Item item)
								{
									this.alwaysContainedItemsSpawned = true;
								}, false, false, InvSlotType.None);
							}
						}
					}
				}
			}
		}

		// Token: 0x060045E0 RID: 17888 RVA: 0x001BFC4D File Offset: 0x001BDE4D
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x060045E1 RID: 17889 RVA: 0x001BFC4F File Offset: 0x001BDE4F
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (!Submarine.Unloading)
			{
				this.Inventory.AllItemsMod.ForEach(delegate(Item it)
				{
					it.Drop(null, true, true);
				});
			}
		}

		// Token: 0x060045E2 RID: 17890 RVA: 0x001BFC90 File Offset: 0x001BDE90
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			string containedString = componentElement.GetAttributeString("contained", "");
			string[] itemIdStrings = containedString.Split(',', StringSplitOptions.None);
			this.itemIds = new List<ushort>[itemIdStrings.Length];
			for (int i = 0; i < itemIdStrings.Length; i++)
			{
				List<ushort>[] array = this.itemIds;
				int num = i;
				if (array[num] == null)
				{
					array[num] = new List<ushort>();
				}
				foreach (string idStr in itemIdStrings[i].Split(';', StringSplitOptions.None))
				{
					int id;
					if (int.TryParse(idStr, out id))
					{
						this.itemIds[i].Add(idRemap.GetOffsetId(id));
					}
				}
			}
			this.ExtraStackSize = componentElement.GetAttributeInt("ExtraStackSize", 0);
		}

		// Token: 0x060045E3 RID: 17891 RVA: 0x001BFD54 File Offset: 0x001BDF54
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			string[] itemIdStrings = new string[this.Inventory.Capacity];
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				IEnumerable<Item> items = this.Inventory.GetItemsAt(i);
				itemIdStrings[i] = string.Join<string>(';', from it in items
				select it.ID.ToString());
			}
			componentElement.Add(new XAttribute("contained", string.Join(',', itemIdStrings)));
			componentElement.Add(new XAttribute("ExtraStackSize", this.ExtraStackSize));
			return componentElement;
		}

		// Token: 0x04002167 RID: 8551
		public readonly NamedEvent<ItemContainer> OnContainedItemsChanged = new NamedEvent<ItemContainer>();

		// Token: 0x04002168 RID: 8552
		private bool alwaysContainedItemsSpawned;

		// Token: 0x04002169 RID: 8553
		public readonly ItemInventory Inventory;

		// Token: 0x0400216A RID: 8554
		private readonly List<ItemContainer.ActiveContainedItem> activeContainedItems = new List<ItemContainer.ActiveContainedItem>();

		// Token: 0x0400216B RID: 8555
		private readonly List<ItemContainer.ContainedItem> containedItems = new List<ItemContainer.ContainedItem>();

		// Token: 0x0400216C RID: 8556
		private List<ushort>[] itemIds;

		// Token: 0x0400216D RID: 8557
		private int capacity;

		// Token: 0x0400216F RID: 8559
		private int maxStackSize;

		// Token: 0x04002170 RID: 8560
		private bool hideItems;

		// Token: 0x0400217C RID: 8572
		private ImmutableHashSet<Identifier> autoInteractWithContainedTags = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x04002181 RID: 8577
		private readonly HashSet<Identifier> containableRestrictions = new HashSet<Identifier>();

		// Token: 0x04002183 RID: 8579
		private float itemRotation;

		// Token: 0x04002189 RID: 8585
		private readonly ImmutableArray<ItemContainer.SlotRestrictions> slotRestrictions;

		// Token: 0x0400218A RID: 8586
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x0400218B RID: 8587
		private float prevContainedItemRefreshRotation;

		// Token: 0x0400218C RID: 8588
		private Vector2 prevContainedItemRefreshPosition;

		// Token: 0x0400218D RID: 8589
		private float autoInjectCooldown = 1f;

		// Token: 0x0400218E RID: 8590
		private const float AutoInjectInterval = 1f;

		// Token: 0x0400218F RID: 8591
		private bool subContainersCanAutoInject;

		// Token: 0x04002190 RID: 8592
		private ImmutableHashSet<Identifier> containableItemIdentifiers;

		// Token: 0x04002193 RID: 8595
		public readonly bool HasSubContainers;

		// Token: 0x04002194 RID: 8596
		public bool hasSignalConnections;

		// Token: 0x04002195 RID: 8597
		private string totalConditionValueString = "";

		// Token: 0x04002196 RID: 8598
		private string totalConditionPercentageString = "";

		// Token: 0x04002197 RID: 8599
		private string totalItemsString = "";

		// Token: 0x04002198 RID: 8600
		private float prevTotalConditionValue;

		// Token: 0x04002199 RID: 8601
		private float prevTotalConditionPercentage;

		// Token: 0x0400219A RID: 8602
		private int prevTotalItems;

		// Token: 0x0400219B RID: 8603
		private bool initializingLoadedItems;

		// Token: 0x02000E10 RID: 3600
		private readonly struct ActiveContainedItem : IEquatable<ItemContainer.ActiveContainedItem>
		{
			// Token: 0x06006938 RID: 26936 RVA: 0x002243ED File Offset: 0x002225ED
			public ActiveContainedItem(Item Item, StatusEffect StatusEffect, bool ExcludeBroken, bool ExcludeFullCondition, bool BlameEquipperForDeath)
			{
				this.Item = Item;
				this.StatusEffect = StatusEffect;
				this.ExcludeBroken = ExcludeBroken;
				this.ExcludeFullCondition = ExcludeFullCondition;
				this.BlameEquipperForDeath = BlameEquipperForDeath;
			}

			// Token: 0x17001697 RID: 5783
			// (get) Token: 0x06006939 RID: 26937 RVA: 0x00224414 File Offset: 0x00222614
			// (set) Token: 0x0600693A RID: 26938 RVA: 0x0022441C File Offset: 0x0022261C
			public Item Item { get; set; }

			// Token: 0x17001698 RID: 5784
			// (get) Token: 0x0600693B RID: 26939 RVA: 0x00224425 File Offset: 0x00222625
			// (set) Token: 0x0600693C RID: 26940 RVA: 0x0022442D File Offset: 0x0022262D
			public StatusEffect StatusEffect { get; set; }

			// Token: 0x17001699 RID: 5785
			// (get) Token: 0x0600693D RID: 26941 RVA: 0x00224436 File Offset: 0x00222636
			// (set) Token: 0x0600693E RID: 26942 RVA: 0x0022443E File Offset: 0x0022263E
			public bool ExcludeBroken { get; set; }

			// Token: 0x1700169A RID: 5786
			// (get) Token: 0x0600693F RID: 26943 RVA: 0x00224447 File Offset: 0x00222647
			// (set) Token: 0x06006940 RID: 26944 RVA: 0x0022444F File Offset: 0x0022264F
			public bool ExcludeFullCondition { get; set; }

			// Token: 0x1700169B RID: 5787
			// (get) Token: 0x06006941 RID: 26945 RVA: 0x00224458 File Offset: 0x00222658
			// (set) Token: 0x06006942 RID: 26946 RVA: 0x00224460 File Offset: 0x00222660
			public bool BlameEquipperForDeath { get; set; }

			// Token: 0x06006943 RID: 26947 RVA: 0x0022446C File Offset: 0x0022266C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ActiveContainedItem");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006944 RID: 26948 RVA: 0x002244B8 File Offset: 0x002226B8
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", StatusEffect = ");
				builder.Append(this.StatusEffect);
				builder.Append(", ExcludeBroken = ");
				builder.Append(this.ExcludeBroken.ToString());
				builder.Append(", ExcludeFullCondition = ");
				builder.Append(this.ExcludeFullCondition.ToString());
				builder.Append(", BlameEquipperForDeath = ");
				builder.Append(this.BlameEquipperForDeath.ToString());
				return true;
			}

			// Token: 0x06006945 RID: 26949 RVA: 0x0022456D File Offset: 0x0022276D
			[CompilerGenerated]
			public static bool operator !=(ItemContainer.ActiveContainedItem left, ItemContainer.ActiveContainedItem right)
			{
				return !(left == right);
			}

			// Token: 0x06006946 RID: 26950 RVA: 0x00224579 File Offset: 0x00222779
			[CompilerGenerated]
			public static bool operator ==(ItemContainer.ActiveContainedItem left, ItemContainer.ActiveContainedItem right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006947 RID: 26951 RVA: 0x00224584 File Offset: 0x00222784
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<StatusEffect>.Default.GetHashCode(this.<StatusEffect>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ExcludeBroken>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ExcludeFullCondition>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<BlameEquipperForDeath>k__BackingField);
			}

			// Token: 0x06006948 RID: 26952 RVA: 0x002245FD File Offset: 0x002227FD
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemContainer.ActiveContainedItem && this.Equals((ItemContainer.ActiveContainedItem)obj);
			}

			// Token: 0x06006949 RID: 26953 RVA: 0x00224618 File Offset: 0x00222818
			[CompilerGenerated]
			public bool Equals(ItemContainer.ActiveContainedItem other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<StatusEffect>.Default.Equals(this.<StatusEffect>k__BackingField, other.<StatusEffect>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ExcludeBroken>k__BackingField, other.<ExcludeBroken>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ExcludeFullCondition>k__BackingField, other.<ExcludeFullCondition>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<BlameEquipperForDeath>k__BackingField, other.<BlameEquipperForDeath>k__BackingField);
			}

			// Token: 0x0600694A RID: 26954 RVA: 0x0022469D File Offset: 0x0022289D
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out StatusEffect StatusEffect, out bool ExcludeBroken, out bool ExcludeFullCondition, out bool BlameEquipperForDeath)
			{
				Item = this.Item;
				StatusEffect = this.StatusEffect;
				ExcludeBroken = this.ExcludeBroken;
				ExcludeFullCondition = this.ExcludeFullCondition;
				BlameEquipperForDeath = this.BlameEquipperForDeath;
			}
		}

		// Token: 0x02000E11 RID: 3601
		private readonly struct ContainedItem : IEquatable<ItemContainer.ContainedItem>
		{
			// Token: 0x0600694B RID: 26955 RVA: 0x002246C9 File Offset: 0x002228C9
			public ContainedItem(Item Item, bool Hide, Vector2? ItemPos, float Rotation)
			{
				this.Item = Item;
				this.Hide = Hide;
				this.ItemPos = ItemPos;
				this.Rotation = Rotation;
			}

			// Token: 0x1700169C RID: 5788
			// (get) Token: 0x0600694C RID: 26956 RVA: 0x002246E8 File Offset: 0x002228E8
			// (set) Token: 0x0600694D RID: 26957 RVA: 0x002246F0 File Offset: 0x002228F0
			public Item Item { get; set; }

			// Token: 0x1700169D RID: 5789
			// (get) Token: 0x0600694E RID: 26958 RVA: 0x002246F9 File Offset: 0x002228F9
			// (set) Token: 0x0600694F RID: 26959 RVA: 0x00224701 File Offset: 0x00222901
			public bool Hide { get; set; }

			// Token: 0x1700169E RID: 5790
			// (get) Token: 0x06006950 RID: 26960 RVA: 0x0022470A File Offset: 0x0022290A
			// (set) Token: 0x06006951 RID: 26961 RVA: 0x00224712 File Offset: 0x00222912
			public Vector2? ItemPos { get; set; }

			// Token: 0x1700169F RID: 5791
			// (get) Token: 0x06006952 RID: 26962 RVA: 0x0022471B File Offset: 0x0022291B
			// (set) Token: 0x06006953 RID: 26963 RVA: 0x00224723 File Offset: 0x00222923
			public float Rotation { get; set; }

			// Token: 0x06006954 RID: 26964 RVA: 0x0022472C File Offset: 0x0022292C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ContainedItem");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006955 RID: 26965 RVA: 0x00224778 File Offset: 0x00222978
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", Hide = ");
				builder.Append(this.Hide.ToString());
				builder.Append(", ItemPos = ");
				builder.Append(this.ItemPos.ToString());
				builder.Append(", Rotation = ");
				builder.Append(this.Rotation.ToString());
				return true;
			}

			// Token: 0x06006956 RID: 26966 RVA: 0x00224814 File Offset: 0x00222A14
			[CompilerGenerated]
			public static bool operator !=(ItemContainer.ContainedItem left, ItemContainer.ContainedItem right)
			{
				return !(left == right);
			}

			// Token: 0x06006957 RID: 26967 RVA: 0x00224820 File Offset: 0x00222A20
			[CompilerGenerated]
			public static bool operator ==(ItemContainer.ContainedItem left, ItemContainer.ContainedItem right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006958 RID: 26968 RVA: 0x0022482C File Offset: 0x00222A2C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Hide>k__BackingField)) * -1521134295 + EqualityComparer<Vector2?>.Default.GetHashCode(this.<ItemPos>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Rotation>k__BackingField);
			}

			// Token: 0x06006959 RID: 26969 RVA: 0x0022488E File Offset: 0x00222A8E
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemContainer.ContainedItem && this.Equals((ItemContainer.ContainedItem)obj);
			}

			// Token: 0x0600695A RID: 26970 RVA: 0x002248A8 File Offset: 0x00222AA8
			[CompilerGenerated]
			public bool Equals(ItemContainer.ContainedItem other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Hide>k__BackingField, other.<Hide>k__BackingField) && EqualityComparer<Vector2?>.Default.Equals(this.<ItemPos>k__BackingField, other.<ItemPos>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Rotation>k__BackingField, other.<Rotation>k__BackingField);
			}

			// Token: 0x0600695B RID: 26971 RVA: 0x00224915 File Offset: 0x00222B15
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out bool Hide, out Vector2? ItemPos, out float Rotation)
			{
				Item = this.Item;
				Hide = this.Hide;
				ItemPos = this.ItemPos;
				Rotation = this.Rotation;
			}
		}

		// Token: 0x02000E12 RID: 3602
		private class SlotRestrictions
		{
			// Token: 0x0600695C RID: 26972 RVA: 0x0022493C File Offset: 0x00222B3C
			public SlotRestrictions(int maxStackSize, List<RelatedItem> containableItems, bool autoInject)
			{
				this.MaxStackSize = maxStackSize;
				this.ContainableItems = containableItems;
				this.AutoInject = autoInject;
			}

			// Token: 0x0600695D RID: 26973 RVA: 0x0022495C File Offset: 0x00222B5C
			public bool MatchesItem(Item item)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.MatchesItem(item));
			}

			// Token: 0x0600695E RID: 26974 RVA: 0x002249A4 File Offset: 0x00222BA4
			public bool MatchesItem(ItemPrefab itemPrefab)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.MatchesItem(itemPrefab));
			}

			// Token: 0x0600695F RID: 26975 RVA: 0x002249EC File Offset: 0x00222BEC
			public bool MatchesItem(Identifier identifierOrTag)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.Identifiers.Contains(identifierOrTag) && !c.ExcludedIdentifiers.Contains(identifierOrTag));
			}

			// Token: 0x040041A9 RID: 16809
			public int MaxStackSize;

			// Token: 0x040041AA RID: 16810
			public List<RelatedItem> ContainableItems;

			// Token: 0x040041AB RID: 16811
			public readonly bool AutoInject;
		}
	}
}
