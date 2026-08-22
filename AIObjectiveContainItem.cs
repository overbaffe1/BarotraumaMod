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
	// Token: 0x02000170 RID: 368
	internal class AIObjectiveContainItem : AIObjective
	{
		// Token: 0x17000B01 RID: 2817
		// (get) Token: 0x06002BC5 RID: 11205 RVA: 0x001E1662 File Offset: 0x001DF862
		// (set) Token: 0x06002BC6 RID: 11206 RVA: 0x001E166A File Offset: 0x001DF86A
		public override Identifier Identifier { get; set; } = "contain item".ToIdentifier();

		// Token: 0x17000B02 RID: 2818
		// (get) Token: 0x06002BC7 RID: 11207 RVA: 0x001E1673 File Offset: 0x001DF873
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B03 RID: 2819
		// (get) Token: 0x06002BC8 RID: 11208 RVA: 0x001E1676 File Offset: 0x001DF876
		// (set) Token: 0x06002BC9 RID: 11209 RVA: 0x001E167E File Offset: 0x001DF87E
		public Item ItemToContain { get; private set; }

		// Token: 0x17000B04 RID: 2820
		// (get) Token: 0x06002BCA RID: 11210 RVA: 0x001E1687 File Offset: 0x001DF887
		// (set) Token: 0x06002BCB RID: 11211 RVA: 0x001E168F File Offset: 0x001DF88F
		public bool AllowToFindDivingGear { get; set; } = true;

		// Token: 0x17000B05 RID: 2821
		// (get) Token: 0x06002BCC RID: 11212 RVA: 0x001E1698 File Offset: 0x001DF898
		// (set) Token: 0x06002BCD RID: 11213 RVA: 0x001E16A0 File Offset: 0x001DF8A0
		public bool AllowDangerousPressure { get; set; }

		// Token: 0x17000B06 RID: 2822
		// (get) Token: 0x06002BCE RID: 11214 RVA: 0x001E16A9 File Offset: 0x001DF8A9
		// (set) Token: 0x06002BCF RID: 11215 RVA: 0x001E16B1 File Offset: 0x001DF8B1
		public float ConditionLevel { get; set; } = 1f;

		// Token: 0x17000B07 RID: 2823
		// (get) Token: 0x06002BD0 RID: 11216 RVA: 0x001E16BA File Offset: 0x001DF8BA
		// (set) Token: 0x06002BD1 RID: 11217 RVA: 0x001E16C2 File Offset: 0x001DF8C2
		public bool Equip { get; set; }

		// Token: 0x17000B08 RID: 2824
		// (get) Token: 0x06002BD2 RID: 11218 RVA: 0x001E16CB File Offset: 0x001DF8CB
		// (set) Token: 0x06002BD3 RID: 11219 RVA: 0x001E16D3 File Offset: 0x001DF8D3
		public bool RemoveEmpty { get; set; } = true;

		// Token: 0x17000B09 RID: 2825
		// (get) Token: 0x06002BD4 RID: 11220 RVA: 0x001E16DC File Offset: 0x001DF8DC
		// (set) Token: 0x06002BD5 RID: 11221 RVA: 0x001E16E4 File Offset: 0x001DF8E4
		public bool RemoveExisting { get; set; }

		// Token: 0x17000B0A RID: 2826
		// (get) Token: 0x06002BD6 RID: 11222 RVA: 0x001E16ED File Offset: 0x001DF8ED
		// (set) Token: 0x06002BD7 RID: 11223 RVA: 0x001E16F5 File Offset: 0x001DF8F5
		public bool RemoveExistingWhenNecessary { get; set; }

		// Token: 0x17000B0B RID: 2827
		// (get) Token: 0x06002BD8 RID: 11224 RVA: 0x001E16FE File Offset: 0x001DF8FE
		// (set) Token: 0x06002BD9 RID: 11225 RVA: 0x001E1706 File Offset: 0x001DF906
		public Func<Item, bool> RemoveExistingPredicate { get; set; }

		// Token: 0x17000B0C RID: 2828
		// (get) Token: 0x06002BDA RID: 11226 RVA: 0x001E170F File Offset: 0x001DF90F
		// (set) Token: 0x06002BDB RID: 11227 RVA: 0x001E1717 File Offset: 0x001DF917
		public int? RemoveMax { get; set; }

		// Token: 0x17000B0D RID: 2829
		// (get) Token: 0x06002BDC RID: 11228 RVA: 0x001E1720 File Offset: 0x001DF920
		// (set) Token: 0x06002BDD RID: 11229 RVA: 0x001E1728 File Offset: 0x001DF928
		public bool MoveWholeStack { get; set; }

		// Token: 0x17000B0E RID: 2830
		// (get) Token: 0x06002BDE RID: 11230 RVA: 0x001E1731 File Offset: 0x001DF931
		// (set) Token: 0x06002BDF RID: 11231 RVA: 0x001E1739 File Offset: 0x001DF939
		public bool AllowStealing { get; set; }

		// Token: 0x17000B0F RID: 2831
		// (get) Token: 0x06002BE0 RID: 11232 RVA: 0x001E1742 File Offset: 0x001DF942
		// (set) Token: 0x06002BE1 RID: 11233 RVA: 0x001E174A File Offset: 0x001DF94A
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

		// Token: 0x06002BE2 RID: 11234 RVA: 0x001E175C File Offset: 0x001DF95C
		public AIObjectiveContainItem(Character character, Item item, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.container = container;
			this.item = item;
		}

		// Token: 0x06002BE3 RID: 11235 RVA: 0x001E17CD File Offset: 0x001DF9CD
		public AIObjectiveContainItem(Character character, Identifier itemIdentifier, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : this(character, itemIdentifier.ToEnumerable<Identifier>().ToImmutableHashSet<Identifier>(), container, objectiveManager, priorityModifier, spawnItemIfNotFound)
		{
		}

		// Token: 0x06002BE4 RID: 11236 RVA: 0x001E17E8 File Offset: 0x001DF9E8
		public AIObjectiveContainItem(Character character, ImmutableHashSet<Identifier> itemIdentifiers, ItemContainer container, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.itemIdentifiers = itemIdentifiers;
			this.spawnItemIfNotFound = spawnItemIfNotFound;
			this.container = container;
		}

		// Token: 0x06002BE5 RID: 11237 RVA: 0x001E1864 File Offset: 0x001DFA64
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

		// Token: 0x06002BE6 RID: 11238 RVA: 0x001E18CC File Offset: 0x001DFACC
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

		// Token: 0x06002BE7 RID: 11239 RVA: 0x001E1940 File Offset: 0x001DFB40
		private bool CheckItem(Item item)
		{
			bool flag;
			return item.HasIdentifierOrTags(this.itemIdentifiers) && item.ConditionPercentage >= this.ConditionLevel && item.HasAccess(this.character) && this.container.ShouldBeContained(item, out flag);
		}

		// Token: 0x06002BE8 RID: 11240 RVA: 0x001E1988 File Offset: 0x001DFB88
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

		// Token: 0x06002BE9 RID: 11241 RVA: 0x001E1C5C File Offset: 0x001DFE5C
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

		// Token: 0x06002BEA RID: 11242 RVA: 0x001E1CA2 File Offset: 0x001DFEA2
		public override void Reset()
		{
			base.Reset();
			this.getItemObjective = null;
			this.goToObjective = null;
			this.containedItems.Clear();
		}

		// Token: 0x06002BEC RID: 11244 RVA: 0x001E1D2C File Offset: 0x001DFF2C
		[CompilerGenerated]
		internal static bool <Act>g__CanBePut|75_7(Inventory inventory, int? targetSlot, Item itemToContain)
		{
			if (targetSlot != null)
			{
				return inventory.CanBePutInSlot(itemToContain, targetSlot.Value, false);
			}
			return inventory.CanBePut(itemToContain);
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x001E1D50 File Offset: 0x001DFF50
		[CompilerGenerated]
		private bool <Act>g__TryPutItem|75_8(Inventory inventory, int? targetSlot, Item itemToContain)
		{
			if (targetSlot != null)
			{
				return inventory.TryPutItem(itemToContain, targetSlot.Value, false, false, this.character, true, false, true);
			}
			return inventory.TryPutItem(itemToContain, this.character, null, true, false, true);
		}

		// Token: 0x040016EB RID: 5867
		public Func<Item, float> GetItemPriority;

		// Token: 0x040016EC RID: 5868
		public ImmutableHashSet<Identifier> ignoredContainerIdentifiers;

		// Token: 0x040016ED RID: 5869
		public bool checkInventory = true;

		// Token: 0x040016EE RID: 5870
		private readonly bool spawnItemIfNotFound;

		// Token: 0x040016EF RID: 5871
		public readonly ImmutableHashSet<Identifier> itemIdentifiers;

		// Token: 0x040016F0 RID: 5872
		public readonly ItemContainer container;

		// Token: 0x040016F1 RID: 5873
		private readonly Item item;

		// Token: 0x040016F3 RID: 5875
		public int? TargetSlot;

		// Token: 0x040016F4 RID: 5876
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x040016F5 RID: 5877
		private AIObjectiveGoTo goToObjective;

		// Token: 0x040016F6 RID: 5878
		private readonly HashSet<Item> containedItems = new HashSet<Item>();

		// Token: 0x04001702 RID: 5890
		private int _itemCount = 1;
	}
}
