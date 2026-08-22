using System;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200007F RID: 127
	internal class AIObjectiveMoveItem : AIObjective
	{
		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x060010F6 RID: 4342 RVA: 0x0009B07E File Offset: 0x0009927E
		// (set) Token: 0x060010F7 RID: 4343 RVA: 0x0009B086 File Offset: 0x00099286
		public override Identifier Identifier { get; set; } = "move item".ToIdentifier();

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x060010F8 RID: 4344 RVA: 0x0009B08F File Offset: 0x0009928F
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x060010F9 RID: 4345 RVA: 0x0009B092 File Offset: 0x00099292
		public AIObjectiveGetItem GetItemObjective
		{
			get
			{
				return this.getItemObjective;
			}
		}

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x060010FA RID: 4346 RVA: 0x0009B09A File Offset: 0x0009929A
		public AIObjectiveContainItem ContainObjective
		{
			get
			{
				return this.containObjective;
			}
		}

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x060010FB RID: 4347 RVA: 0x0009B0A2 File Offset: 0x000992A2
		public Item TargetItem
		{
			get
			{
				return this.targetItem;
			}
		}

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x060010FC RID: 4348 RVA: 0x0009B0AA File Offset: 0x000992AA
		public ItemContainer TargetContainer
		{
			get
			{
				return this.targetContainer;
			}
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x060010FD RID: 4349 RVA: 0x0009B0B2 File Offset: 0x000992B2
		// (set) Token: 0x060010FE RID: 4350 RVA: 0x0009B0BA File Offset: 0x000992BA
		public bool Equip { get; set; }

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x060010FF RID: 4351 RVA: 0x0009B0C3 File Offset: 0x000992C3
		// (set) Token: 0x06001100 RID: 4352 RVA: 0x0009B0CB File Offset: 0x000992CB
		public bool TakeWholeStack { get; set; }

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06001101 RID: 4353 RVA: 0x0009B0D4 File Offset: 0x000992D4
		// (set) Token: 0x06001102 RID: 4354 RVA: 0x0009B0DC File Offset: 0x000992DC
		public bool DropIfFails { get; set; } = true;

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x06001103 RID: 4355 RVA: 0x0009B0E5 File Offset: 0x000992E5
		// (set) Token: 0x06001104 RID: 4356 RVA: 0x0009B0ED File Offset: 0x000992ED
		public bool RemoveExistingWhenNecessary { get; set; }

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x06001105 RID: 4357 RVA: 0x0009B0F6 File Offset: 0x000992F6
		// (set) Token: 0x06001106 RID: 4358 RVA: 0x0009B0FE File Offset: 0x000992FE
		public Func<Item, bool> RemoveExistingPredicate { get; set; }

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x06001107 RID: 4359 RVA: 0x0009B107 File Offset: 0x00099307
		// (set) Token: 0x06001108 RID: 4360 RVA: 0x0009B10F File Offset: 0x0009930F
		public int? RemoveExistingMax { get; set; }

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06001109 RID: 4361 RVA: 0x0009B118 File Offset: 0x00099318
		// (set) Token: 0x0600110A RID: 4362 RVA: 0x0009B120 File Offset: 0x00099320
		public string AbandonGetItemDialogueIdentifier { get; set; }

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x0600110B RID: 4363 RVA: 0x0009B129 File Offset: 0x00099329
		// (set) Token: 0x0600110C RID: 4364 RVA: 0x0009B131 File Offset: 0x00099331
		public Func<bool> AbandonGetItemDialogueCondition { get; set; }

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x0600110D RID: 4365 RVA: 0x0009B13A File Offset: 0x0009933A
		// (set) Token: 0x0600110E RID: 4366 RVA: 0x0009B142 File Offset: 0x00099342
		public bool AllowToFindDivingGear { get; set; }

		// Token: 0x0600110F RID: 4367 RVA: 0x0009B14C File Offset: 0x0009934C
		public AIObjectiveMoveItem(Character character, Item targetItem, AIObjectiveManager objectiveManager, ItemContainer sourceContainer = null, ItemContainer targetContainer = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetItem = targetItem;
			this.sourceContainer = sourceContainer;
			this.targetContainer = targetContainer;
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x0009B19A File Offset: 0x0009939A
		public AIObjectiveMoveItem(Character character, Identifier itemIdentifier, AIObjectiveManager objectiveManager, ItemContainer sourceContainer, ItemContainer targetContainer = null, float priorityModifier = 1f) : this(character, new Identifier[]
		{
			itemIdentifier
		}, objectiveManager, sourceContainer, targetContainer, priorityModifier)
		{
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x0009B1B8 File Offset: 0x000993B8
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

		// Token: 0x06001112 RID: 4370 RVA: 0x0009B222 File Offset: 0x00099422
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x0009B22C File Offset: 0x0009942C
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

		// Token: 0x06001114 RID: 4372 RVA: 0x0009B39F File Offset: 0x0009959F
		public override void Reset()
		{
			base.Reset();
			this.getItemObjective = null;
			this.containObjective = null;
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x0009B3B5 File Offset: 0x000995B5
		protected override void OnAbandon()
		{
			base.OnAbandon();
			if (this.DropIfFails && this.targetItem != null && this.targetItem.IsOwnedBy(this.character))
			{
				this.targetItem.Drop(this.character, true, true);
			}
		}

		// Token: 0x0400080B RID: 2059
		public Func<Item, float> GetItemPriority;

		// Token: 0x0400080C RID: 2060
		private readonly Identifier[] itemIdentifiers;

		// Token: 0x0400080D RID: 2061
		private readonly ItemContainer sourceContainer;

		// Token: 0x0400080E RID: 2062
		private readonly ItemContainer targetContainer;

		// Token: 0x0400080F RID: 2063
		private readonly Item targetItem;

		// Token: 0x04000810 RID: 2064
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x04000811 RID: 2065
		private AIObjectiveContainItem containObjective;
	}
}
