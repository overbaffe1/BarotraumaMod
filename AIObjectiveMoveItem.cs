using System;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000185 RID: 389
	internal class AIObjectiveMoveItem : AIObjective
	{
		// Token: 0x17000BB6 RID: 2998
		// (get) Token: 0x06002DFE RID: 11774 RVA: 0x001EDF16 File Offset: 0x001EC116
		// (set) Token: 0x06002DFF RID: 11775 RVA: 0x001EDF1E File Offset: 0x001EC11E
		public override Identifier Identifier { get; set; } = "move item".ToIdentifier();

		// Token: 0x17000BB7 RID: 2999
		// (get) Token: 0x06002E00 RID: 11776 RVA: 0x001EDF27 File Offset: 0x001EC127
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BB8 RID: 3000
		// (get) Token: 0x06002E01 RID: 11777 RVA: 0x001EDF2A File Offset: 0x001EC12A
		public AIObjectiveGetItem GetItemObjective
		{
			get
			{
				return this.getItemObjective;
			}
		}

		// Token: 0x17000BB9 RID: 3001
		// (get) Token: 0x06002E02 RID: 11778 RVA: 0x001EDF32 File Offset: 0x001EC132
		public AIObjectiveContainItem ContainObjective
		{
			get
			{
				return this.containObjective;
			}
		}

		// Token: 0x17000BBA RID: 3002
		// (get) Token: 0x06002E03 RID: 11779 RVA: 0x001EDF3A File Offset: 0x001EC13A
		public Item TargetItem
		{
			get
			{
				return this.targetItem;
			}
		}

		// Token: 0x17000BBB RID: 3003
		// (get) Token: 0x06002E04 RID: 11780 RVA: 0x001EDF42 File Offset: 0x001EC142
		public ItemContainer TargetContainer
		{
			get
			{
				return this.targetContainer;
			}
		}

		// Token: 0x17000BBC RID: 3004
		// (get) Token: 0x06002E05 RID: 11781 RVA: 0x001EDF4A File Offset: 0x001EC14A
		// (set) Token: 0x06002E06 RID: 11782 RVA: 0x001EDF52 File Offset: 0x001EC152
		public bool Equip { get; set; }

		// Token: 0x17000BBD RID: 3005
		// (get) Token: 0x06002E07 RID: 11783 RVA: 0x001EDF5B File Offset: 0x001EC15B
		// (set) Token: 0x06002E08 RID: 11784 RVA: 0x001EDF63 File Offset: 0x001EC163
		public bool TakeWholeStack { get; set; }

		// Token: 0x17000BBE RID: 3006
		// (get) Token: 0x06002E09 RID: 11785 RVA: 0x001EDF6C File Offset: 0x001EC16C
		// (set) Token: 0x06002E0A RID: 11786 RVA: 0x001EDF74 File Offset: 0x001EC174
		public bool DropIfFails { get; set; } = true;

		// Token: 0x17000BBF RID: 3007
		// (get) Token: 0x06002E0B RID: 11787 RVA: 0x001EDF7D File Offset: 0x001EC17D
		// (set) Token: 0x06002E0C RID: 11788 RVA: 0x001EDF85 File Offset: 0x001EC185
		public bool RemoveExistingWhenNecessary { get; set; }

		// Token: 0x17000BC0 RID: 3008
		// (get) Token: 0x06002E0D RID: 11789 RVA: 0x001EDF8E File Offset: 0x001EC18E
		// (set) Token: 0x06002E0E RID: 11790 RVA: 0x001EDF96 File Offset: 0x001EC196
		public Func<Item, bool> RemoveExistingPredicate { get; set; }

		// Token: 0x17000BC1 RID: 3009
		// (get) Token: 0x06002E0F RID: 11791 RVA: 0x001EDF9F File Offset: 0x001EC19F
		// (set) Token: 0x06002E10 RID: 11792 RVA: 0x001EDFA7 File Offset: 0x001EC1A7
		public int? RemoveExistingMax { get; set; }

		// Token: 0x17000BC2 RID: 3010
		// (get) Token: 0x06002E11 RID: 11793 RVA: 0x001EDFB0 File Offset: 0x001EC1B0
		// (set) Token: 0x06002E12 RID: 11794 RVA: 0x001EDFB8 File Offset: 0x001EC1B8
		public string AbandonGetItemDialogueIdentifier { get; set; }

		// Token: 0x17000BC3 RID: 3011
		// (get) Token: 0x06002E13 RID: 11795 RVA: 0x001EDFC1 File Offset: 0x001EC1C1
		// (set) Token: 0x06002E14 RID: 11796 RVA: 0x001EDFC9 File Offset: 0x001EC1C9
		public Func<bool> AbandonGetItemDialogueCondition { get; set; }

		// Token: 0x17000BC4 RID: 3012
		// (get) Token: 0x06002E15 RID: 11797 RVA: 0x001EDFD2 File Offset: 0x001EC1D2
		// (set) Token: 0x06002E16 RID: 11798 RVA: 0x001EDFDA File Offset: 0x001EC1DA
		public bool AllowToFindDivingGear { get; set; }

		// Token: 0x06002E17 RID: 11799 RVA: 0x001EDFE4 File Offset: 0x001EC1E4
		public AIObjectiveMoveItem(Character character, Item targetItem, AIObjectiveManager objectiveManager, ItemContainer sourceContainer = null, ItemContainer targetContainer = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetItem = targetItem;
			this.sourceContainer = sourceContainer;
			this.targetContainer = targetContainer;
		}

		// Token: 0x06002E18 RID: 11800 RVA: 0x001EE032 File Offset: 0x001EC232
		public AIObjectiveMoveItem(Character character, Identifier itemIdentifier, AIObjectiveManager objectiveManager, ItemContainer sourceContainer, ItemContainer targetContainer = null, float priorityModifier = 1f) : this(character, new Identifier[]
		{
			itemIdentifier
		}, objectiveManager, sourceContainer, targetContainer, priorityModifier)
		{
		}

		// Token: 0x06002E19 RID: 11801 RVA: 0x001EE050 File Offset: 0x001EC250
		public AIObjectiveMoveItem(Character character, Identifier[] itemIdentifiers, AIObjectiveManager objectiveManager, ItemContainer sourceContainer, ItemContainer targetContainer = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.itemIdentifiers = itemIdentifiers;
			for (int i = 0; i < itemIdentifiers.Length; i++)
			{
				itemIdentifiers[i] = itemIdentifiers[i];
			}
			this.sourceContainer = sourceContainer;
			this.targetContainer = targetContainer;
		}

		// Token: 0x06002E1A RID: 11802 RVA: 0x001EE0BA File Offset: 0x001EC2BA
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06002E1B RID: 11803 RVA: 0x001EE0C4 File Offset: 0x001EC2C4
		protected override void Act(float deltaTime)
		{
			Item itemToMove = this.targetItem ?? this.sourceContainer.Inventory.FindItem((Item i) => this.itemIdentifiers.Any((Identifier id) => i.Prefab.Identifier == id || (i.HasTag(id) && !i.IgnoreByAI(this.character))), false);
			if (itemToMove == null)
			{
				base.Abandon = true;
				return;
			}
			if (itemToMove.IgnoreByAI(this.character))
			{
				base.Abandon = true;
				return;
			}
			if (this.targetContainer == null)
			{
				if (this.sourceContainer == null)
				{
					base.Abandon = true;
					return;
				}
				if (itemToMove.Container != this.sourceContainer.Item)
				{
					itemToMove.Drop(this.character, true, true);
					base.IsCompleted = true;
					return;
				}
			}
			else if (this.targetContainer.Inventory.Contains(itemToMove))
			{
				base.IsCompleted = true;
				return;
			}
			if (this.getItemObjective == null && !itemToMove.IsOwnedBy(this.character))
			{
				base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getItemObjective, () => new AIObjectiveGetItem(this.character, this.targetItem, this.objectiveManager, this.Equip, 1f)
				{
					CannotFindDialogueCondition = this.AbandonGetItemDialogueCondition,
					CannotFindDialogueIdentifierOverride = this.AbandonGetItemDialogueIdentifier,
					SpeakIfFails = (this.AbandonGetItemDialogueIdentifier != null),
					TakeWholeStack = this.TakeWholeStack,
					AllowToFindDivingGear = this.AllowToFindDivingGear
				}, null, delegate
				{
					this.Abandon = true;
				});
				return;
			}
			if (this.targetContainer != null)
			{
				base.TryAddSubObjective<AIObjectiveContainItem>(ref this.containObjective, delegate
				{
					AIObjectiveContainItem aiobjectiveContainItem = new AIObjectiveContainItem(this.character, itemToMove, this.targetContainer, this.objectiveManager, 1f);
					aiobjectiveContainItem.MoveWholeStack = this.TakeWholeStack;
					aiobjectiveContainItem.Equip = this.Equip;
					aiobjectiveContainItem.RemoveEmpty = false;
					aiobjectiveContainItem.RemoveExistingWhenNecessary = this.RemoveExistingWhenNecessary;
					aiobjectiveContainItem.RemoveExistingPredicate = this.RemoveExistingPredicate;
					aiobjectiveContainItem.RemoveMax = this.RemoveExistingMax;
					aiobjectiveContainItem.GetItemPriority = this.GetItemPriority;
					ItemContainer itemContainer = this.sourceContainer;
					aiobjectiveContainItem.ignoredContainerIdentifiers = ((itemContainer != null) ? itemContainer.Item.Prefab.Identifier.ToEnumerable<Identifier>().ToImmutableHashSet<Identifier>() : null);
					aiobjectiveContainItem.AllowToFindDivingGear = this.AllowToFindDivingGear;
					return aiobjectiveContainItem;
				}, delegate
				{
					this.IsCompleted = true;
				}, delegate
				{
					this.Abandon = true;
				});
				return;
			}
			itemToMove.Drop(this.character, true, true);
			base.IsCompleted = true;
		}

		// Token: 0x06002E1C RID: 11804 RVA: 0x001EE237 File Offset: 0x001EC437
		public override void Reset()
		{
			base.Reset();
			this.getItemObjective = null;
			this.containObjective = null;
		}

		// Token: 0x06002E1D RID: 11805 RVA: 0x001EE24D File Offset: 0x001EC44D
		protected override void OnAbandon()
		{
			base.OnAbandon();
			if (this.DropIfFails && this.targetItem != null && this.targetItem.IsOwnedBy(this.character))
			{
				this.targetItem.Drop(this.character, true, true);
			}
		}

		// Token: 0x04001805 RID: 6149
		public Func<Item, float> GetItemPriority;

		// Token: 0x04001806 RID: 6150
		private readonly Identifier[] itemIdentifiers;

		// Token: 0x04001807 RID: 6151
		private readonly ItemContainer sourceContainer;

		// Token: 0x04001808 RID: 6152
		private readonly ItemContainer targetContainer;

		// Token: 0x04001809 RID: 6153
		private readonly Item targetItem;

		// Token: 0x0400180A RID: 6154
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x0400180B RID: 6155
		private AIObjectiveContainItem containObjective;
	}
}
