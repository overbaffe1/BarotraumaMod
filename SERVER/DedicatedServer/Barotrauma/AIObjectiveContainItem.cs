using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200006A RID: 106
	internal class AIObjectiveContainItem : AIObjective
	{
		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06000EBD RID: 3773 RVA: 0x0008E7CE File Offset: 0x0008C9CE
		// (set) Token: 0x06000EBE RID: 3774 RVA: 0x0008E7D6 File Offset: 0x0008C9D6
		public override Identifier Identifier { get; set; } = "contain item".ToIdentifier();

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06000EBF RID: 3775 RVA: 0x0008E7DF File Offset: 0x0008C9DF
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06000EC0 RID: 3776 RVA: 0x0008E7E2 File Offset: 0x0008C9E2
		// (set) Token: 0x06000EC1 RID: 3777 RVA: 0x0008E7EA File Offset: 0x0008C9EA
		public Item ItemToContain { get; private set; }

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x0008E7F3 File Offset: 0x0008C9F3
		// (set) Token: 0x06000EC3 RID: 3779 RVA: 0x0008E7FB File Offset: 0x0008C9FB
		public bool AllowToFindDivingGear { get; set; } = true;

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06000EC4 RID: 3780 RVA: 0x0008E804 File Offset: 0x0008CA04
		// (set) Token: 0x06000EC5 RID: 3781 RVA: 0x0008E80C File Offset: 0x0008CA0C
		public bool AllowDangerousPressure { get; set; }

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06000EC6 RID: 3782 RVA: 0x0008E815 File Offset: 0x0008CA15
		// (set) Token: 0x06000EC7 RID: 3783 RVA: 0x0008E81D File Offset: 0x0008CA1D
		public float ConditionLevel { get; set; } = 1f;

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06000EC8 RID: 3784 RVA: 0x0008E826 File Offset: 0x0008CA26
		// (set) Token: 0x06000EC9 RID: 3785 RVA: 0x0008E82E File Offset: 0x0008CA2E
		public bool Equip { get; set; }

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06000ECA RID: 3786 RVA: 0x0008E837 File Offset: 0x0008CA37
		// (set) Token: 0x06000ECB RID: 3787 RVA: 0x0008E83F File Offset: 0x0008CA3F
		public bool RemoveEmpty { get; set; } = true;

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06000ECC RID: 3788 RVA: 0x0008E848 File Offset: 0x0008CA48
		// (set) Token: 0x06000ECD RID: 3789 RVA: 0x0008E850 File Offset: 0x0008CA50
		public bool RemoveExisting { get; set; }

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06000ECE RID: 3790 RVA: 0x0008E859 File Offset: 0x0008CA59
		// (set) Token: 0x06000ECF RID: 3791 RVA: 0x0008E861 File Offset: 0x0008CA61
		public bool RemoveExistingWhenNecessary { get; set; }

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x06000ED0 RID: 3792 RVA: 0x0008E86A File Offset: 0x0008CA6A
		// (set) Token: 0x06000ED1 RID: 3793 RVA: 0x0008E872 File Offset: 0x0008CA72
		public Func<Item, bool> RemoveExistingPredicate { get; set; }

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06000ED2 RID: 3794 RVA: 0x0008E87B File Offset: 0x0008CA7B
		// (set) Token: 0x06000ED3 RID: 3795 RVA: 0x0008E883 File Offset: 0x0008CA83
		public int? RemoveMax { get; set; }

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06000ED4 RID: 3796 RVA: 0x0008E88C File Offset: 0x0008CA8C
		// (set) Token: 0x06000ED5 RID: 3797 RVA: 0x0008E894 File Offset: 0x0008CA94
		public bool MoveWholeStack { get; set; }

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000ED6 RID: 3798 RVA: 0x0008E89D File Offset: 0x0008CA9D
		// (set) Token: 0x06000ED7 RID: 3799 RVA: 0x0008E8A5 File Offset: 0x0008CAA5
		public bool AllowStealing { get; set; }

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x0008E8AE File Offset: 0x0008CAAE
		// (set) Token: 0x06000ED9 RID: 3801 RVA: 0x0008E8B6 File Offset: 0x0008CAB6
		public int ItemCount
		{
			get
			{
				return this._itemCount;
			}
			set
			{
				this._itemCount = Math.Max(value, 1);
			}
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x0008E8C8 File Offset: 0x0008CAC8
		public AIObjectiveContainItem(Character character, Item item, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.container = container;
			this.item = item;
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x0008E939 File Offset: 0x0008CB39
		public AIObjectiveContainItem(Character character, Identifier itemIdentifier, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : this(character, itemIdentifier.ToEnumerable<Identifier>().ToImmutableHashSet<Identifier>(), container, objectiveManager, priorityModifier, spawnItemIfNotFound)
		{
		}

		// Token: 0x06000EDC RID: 3804 RVA: 0x0008E954 File Offset: 0x0008CB54
		public AIObjectiveContainItem(Character character, ImmutableHashSet<Identifier> itemIdentifiers, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.itemIdentifiers = itemIdentifiers;
			this.spawnItemIfNotFound = spawnItemIfNotFound;
			this.container = container;
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x0008E9D0 File Offset: 0x0008CBD0
		protected override bool CheckObjectiveState()
		{
			ItemContainer itemContainer = this.container;
			if (((itemContainer != null) ? itemContainer.Item : null) == null || !this.container.Item.HasAccess(this.character))
			{
				base.Abandon = true;
				return false;
			}
			if (this.item != null)
			{
				return this.container.Inventory.Contains(this.item);
			}
			return this.CountItems();
		}

		// Token: 0x06000EDE RID: 3806 RVA: 0x0008EA38 File Offset: 0x0008CC38
		private bool CountItems()
		{
			int containedItemCount = 0;
			foreach (Item it in this.container.Inventory.AllItems)
			{
				if (this.CheckItem(it) && this.IsInTargetSlot(it))
				{
					containedItemCount++;
				}
			}
			return containedItemCount >= this.ItemCount;
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x0008EAAC File Offset: 0x0008CCAC
		private bool CheckItem(Item item)
		{
			bool flag;
			return item.HasIdentifierOrTags(this.itemIdentifiers) && item.ConditionPercentage >= this.ConditionLevel && item.HasAccess(this.character) && this.container.ShouldBeContained(item, out flag);
		}

		// Token: 0x06000EE0 RID: 3808 RVA: 0x0008EAF4 File Offset: 0x0008CCF4
		protected override void Act(float deltaTime)
		{
			ItemContainer itemContainer = this.container;
			if (((itemContainer != null) ? itemContainer.Item : null) == null)
			{
				base.Abandon = true;
				return;
			}
			this.ItemToContain = (this.item ?? this.character.Inventory.FindItem(delegate(Item it)
			{
				if (!this.CheckItem(it))
				{
					return false;
				}
				if (it.Container != this.container.Item)
				{
					return true;
				}
				if (this.TargetSlot != null)
				{
					int num = it.Container.OwnInventory.FindIndex(it);
					int? targetSlot = this.TargetSlot;
					return !(num == targetSlot.GetValueOrDefault() & targetSlot != null);
				}
				return false;
			}, true));
			if (this.ItemToContain != null)
			{
				if (!this.character.CanInteractWith(this.ItemToContain, false))
				{
					base.Abandon = true;
					return;
				}
				if (!this.character.CanInteractWith(this.container.Item, false))
				{
					base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.container.Item, this.character, this.objectiveManager, false, this.AllowToFindDivingGear, 1f, 0f)
					{
						DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachTarget,
						TargetName = this.container.Item.Name,
						AbortCondition = delegate(AIObjective obj)
						{
							ItemContainer itemContainer2 = this.container;
							if (((itemContainer2 != null) ? itemContainer2.Item : null) != null && !this.container.Item.Removed && this.container.Item.HasAccess(this.character))
							{
								Item rootContainer = this.container.Item.RootContainer;
								bool? flag;
								if (rootContainer == null)
								{
									flag = null;
								}
								else
								{
									ItemInventory ownInventory = rootContainer.OwnInventory;
									flag = ((ownInventory != null) ? new bool?(ownInventory.Locked) : null);
								}
								bool? flag2 = flag;
								if (!flag2.GetValueOrDefault() && this.ItemToContain != null && !this.ItemToContain.Removed && this.ItemToContain.IsOwnedBy(this.character))
								{
									Character c = this.container.Item.GetRootInventoryOwner() as Character;
									return c != null && c != this.character;
								}
							}
							return true;
						},
						SpeakIfFails = !this.objectiveManager.IsCurrentOrder<AIObjectiveCleanupItems>(),
						endNodeFilter = ((PathNode n) => Vector2.DistanceSquared(n.Waypoint.WorldPosition, this.container.Item.WorldPosition) <= MathUtils.Pow2(150f))
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					}, delegate
					{
						base.Abandon = true;
					});
					return;
				}
				if (this.RemoveExisting || (this.RemoveExistingWhenNecessary && !AIObjectiveContainItem.<Act>g__CanBePut|75_7(this.container.Inventory, this.TargetSlot, this.ItemToContain)))
				{
					AIController humanAIController = base.HumanAIController;
					Item parentItem = this.container.Item;
					Func<Item, bool> removeExistingPredicate = this.RemoveExistingPredicate;
					bool avoidDroppingInSea = true;
					int? removeMax = this.RemoveMax;
					humanAIController.UnequipContainedItems(parentItem, removeExistingPredicate, avoidDroppingInSea, this.spawnItemIfNotFound, removeMax);
				}
				else if (this.RemoveEmpty)
				{
					base.HumanAIController.UnequipEmptyItems(this.container.Item, true, this.spawnItemIfNotFound);
				}
				Inventory originalInventory = this.ItemToContain.ParentInventory;
				List<int> slots = (originalInventory != null) ? originalInventory.FindIndices(this.ItemToContain) : null;
				if (this.<Act>g__TryPutItem|75_8(this.container.Inventory, this.TargetSlot, this.ItemToContain))
				{
					if (this.MoveWholeStack && slots != null)
					{
						foreach (int slot in slots)
						{
							foreach (Item item in originalInventory.GetItemsAt(slot).ToList<Item>())
							{
								this.<Act>g__TryPutItem|75_8(this.container.Inventory, this.TargetSlot, item);
							}
						}
					}
					base.IsCompleted = (this.item != null || this.CountItems());
					return;
				}
				if (this.ItemToContain.ParentInventory == this.character.Inventory && this.character.IsInFriendlySub)
				{
					this.ItemToContain.Drop(this.character, true, true);
				}
				base.Abandon = true;
				return;
			}
			else
			{
				if (this.character.Submarine == null)
				{
					base.Abandon = true;
					return;
				}
				base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getItemObjective, () => new AIObjectiveGetItem(this.character, this.itemIdentifiers, this.objectiveManager, this.Equip, this.checkInventory, 1f, this.spawnItemIfNotFound)
				{
					GetItemPriority = this.GetItemPriority,
					ignoredContainerIdentifiers = this.ignoredContainerIdentifiers,
					ignoredItems = this.containedItems,
					AllowToFindDivingGear = this.AllowToFindDivingGear,
					AllowDangerousPressure = this.AllowDangerousPressure,
					TargetCondition = this.ConditionLevel,
					ItemFilter = delegate(Item potentialItem)
					{
						bool flag;
						return (this.RemoveEmpty ? this.container.CanBeContained(potentialItem) : this.container.Inventory.CanBePut(potentialItem)) && this.container.ShouldBeContained(potentialItem, out flag);
					},
					ItemCount = this.ItemCount,
					TakeWholeStack = this.MoveWholeStack,
					ContainTarget = this.container,
					AllowStealing = this.AllowStealing
				}, delegate
				{
					AIObjectiveGetItem aiobjectiveGetItem = this.getItemObjective;
					if (((aiobjectiveGetItem != null) ? aiobjectiveGetItem.TargetItem : null) != null)
					{
						this.containedItems.Add(this.getItemObjective.TargetItem);
					}
					base.RemoveSubObjective<AIObjectiveGetItem>(ref this.getItemObjective);
				}, delegate
				{
					base.Abandon = true;
				});
				return;
			}
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x0008EDC8 File Offset: 0x0008CFC8
		public bool IsInTargetSlot(Item item)
		{
			if (this.TargetSlot == null)
			{
				return true;
			}
			ItemContainer itemContainer = this.container;
			ItemInventory inventory = (itemContainer != null) ? itemContainer.Inventory : null;
			return inventory != null && inventory.IsInSlot(item, this.TargetSlot.Value);
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x0008EE0E File Offset: 0x0008D00E
		public override void Reset()
		{
			base.Reset();
			this.getItemObjective = null;
			this.goToObjective = null;
			this.containedItems.Clear();
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x0008EE98 File Offset: 0x0008D098
		[CompilerGenerated]
		internal static bool <Act>g__CanBePut|75_7(Inventory inventory, int? targetSlot, Item itemToContain)
		{
			if (targetSlot != null)
			{
				return inventory.CanBePutInSlot(itemToContain, targetSlot.Value, false);
			}
			return inventory.CanBePut(itemToContain);
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x0008EEBC File Offset: 0x0008D0BC
		[CompilerGenerated]
		private bool <Act>g__TryPutItem|75_8(Inventory inventory, int? targetSlot, Item itemToContain)
		{
			if (targetSlot != null)
			{
				return inventory.TryPutItem(itemToContain, targetSlot.Value, false, false, this.character, true, false, true);
			}
			return inventory.TryPutItem(itemToContain, this.character, null, true, false, true);
		}

		// Token: 0x040006F1 RID: 1777
		public Func<Item, float> GetItemPriority;

		// Token: 0x040006F2 RID: 1778
		public ImmutableHashSet<Identifier> ignoredContainerIdentifiers;

		// Token: 0x040006F3 RID: 1779
		public bool checkInventory = true;

		// Token: 0x040006F4 RID: 1780
		private readonly bool spawnItemIfNotFound;

		// Token: 0x040006F5 RID: 1781
		public readonly ImmutableHashSet<Identifier> itemIdentifiers;

		// Token: 0x040006F6 RID: 1782
		public readonly ItemContainer container;

		// Token: 0x040006F7 RID: 1783
		private readonly Item item;

		// Token: 0x040006F9 RID: 1785
		public int? TargetSlot;

		// Token: 0x040006FA RID: 1786
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x040006FB RID: 1787
		private AIObjectiveGoTo goToObjective;

		// Token: 0x040006FC RID: 1788
		private readonly HashSet<Item> containedItems = new HashSet<Item>();

		// Token: 0x04000708 RID: 1800
		private int _itemCount = 1;
	}
}
