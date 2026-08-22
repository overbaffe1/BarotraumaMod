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
	// Token: 0x02000181 RID: 385
	internal class AIObjectiveLoadItem : AIObjective
	{
		// Token: 0x17000B91 RID: 2961
		// (get) Token: 0x06002D81 RID: 11649 RVA: 0x001EAFB2 File Offset: 0x001E91B2
		// (set) Token: 0x06002D82 RID: 11650 RVA: 0x001EAFBA File Offset: 0x001E91BA
		public override Identifier Identifier { get; set; } = "load item".ToIdentifier();

		// Token: 0x17000B92 RID: 2962
		// (get) Token: 0x06002D83 RID: 11651 RVA: 0x001EAFC3 File Offset: 0x001E91C3
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B93 RID: 2963
		// (get) Token: 0x06002D84 RID: 11652 RVA: 0x001EAFC6 File Offset: 0x001E91C6
		private AIObjectiveLoadItems.ItemCondition TargetItemCondition { get; }

		// Token: 0x17000B94 RID: 2964
		// (get) Token: 0x06002D85 RID: 11653 RVA: 0x001EAFCE File Offset: 0x001E91CE
		private Item Container { get; }

		// Token: 0x17000B95 RID: 2965
		// (get) Token: 0x06002D86 RID: 11654 RVA: 0x001EAFD6 File Offset: 0x001E91D6
		private ItemContainer ItemContainer { get; }

		// Token: 0x17000B96 RID: 2966
		// (get) Token: 0x06002D87 RID: 11655 RVA: 0x001EAFDE File Offset: 0x001E91DE
		private ImmutableArray<Identifier> TargetContainerTags { get; }

		// Token: 0x17000B97 RID: 2967
		// (get) Token: 0x06002D88 RID: 11656 RVA: 0x001EAFE6 File Offset: 0x001E91E6
		private ImmutableHashSet<Identifier> ValidContainableItemIdentifiers { get; }

		// Token: 0x17000B98 RID: 2968
		// (get) Token: 0x06002D89 RID: 11657 RVA: 0x001EAFEE File Offset: 0x001E91EE
		private static Dictionary<ItemPrefab, ImmutableHashSet<Identifier>> AllValidContainableItemIdentifiers { get; } = new Dictionary<ItemPrefab, ImmutableHashSet<Identifier>>();

		// Token: 0x06002D8A RID: 11658 RVA: 0x001EAFF8 File Offset: 0x001E91F8
		public AIObjectiveLoadItem(Item container, ImmutableArray<Identifier> targetTags, AIObjectiveLoadItems.ItemCondition targetCondition, Identifier option, Character character, AIObjectiveManager objectiveManager, float priorityModifier) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Container = container;
			this.ItemContainer = ((container != null) ? container.GetComponent<ItemContainer>() : null);
			ItemContainer itemContainer = this.ItemContainer;
			if (((itemContainer != null) ? itemContainer.Inventory : null) == null)
			{
				base.Abandon = true;
				return;
			}
			this.TargetContainerTags = targetTags;
			this.TargetItemCondition = targetCondition;
			if (!option.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted(this.abandonGetItemDialogueIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(option);
				string optionSpecificDialogueIdentifier = defaultInterpolatedStringHandler.ToStringAndClear();
				if (TextManager.ContainsTag(optionSpecificDialogueIdentifier))
				{
					this.abandonGetItemDialogueIdentifier = optionSpecificDialogueIdentifier;
				}
			}
			this.ValidContainableItemIdentifiers = this.GetValidContainableItemIdentifiers();
			if (this.ValidContainableItemIdentifiers.None(null))
			{
				base.Abandon = true;
				return;
			}
		}

		// Token: 0x06002D8B RID: 11659 RVA: 0x001EB0F0 File Offset: 0x001E92F0
		private ImmutableHashSet<Identifier> GetValidContainableItemIdentifiers()
		{
			AIObjectiveLoadItem.<>c__DisplayClass31_0 CS$<>8__locals1 = new AIObjectiveLoadItem.<>c__DisplayClass31_0();
			CS$<>8__locals1.<>4__this = this;
			ImmutableHashSet<Identifier> existingIdentifiers;
			if (AIObjectiveLoadItem.AllValidContainableItemIdentifiers.TryGetValue(this.Container.Prefab, out existingIdentifiers))
			{
				return existingIdentifiers;
			}
			CS$<>8__locals1.useDefaultContainableItemIdentifiers = true;
			CS$<>8__locals1.potentialContainablePrefabs = MapEntityPrefab.List.Where(delegate(MapEntityPrefab mep)
			{
				ItemPrefab ip = mep as ItemPrefab;
				return ip != null && CS$<>8__locals1.<>4__this.ItemContainer.ContainableItemIdentifiers.Any((Identifier i) => i == ip.Identifier || ip.Tags.Contains(i));
			}).Cast<ItemPrefab>();
			CS$<>8__locals1.validContainableItemIdentifiers = new HashSet<Identifier>();
			foreach (ItemComponent component in this.Container.Components)
			{
				AIObjectiveLoadItem.<>c__DisplayClass31_2 CS$<>8__locals2;
				CS$<>8__locals2.component = component;
				if (CS$<>8__locals1.<GetValidContainableItemIdentifiers>g__CheckComponent|3(ref CS$<>8__locals2) == AIObjectiveLoadItem.CheckStatus.Finished)
				{
					break;
				}
			}
			ImmutableHashSet<Identifier> immutableHashSet;
			if (!CS$<>8__locals1.useDefaultContainableItemIdentifiers)
			{
				immutableHashSet = CS$<>8__locals1.validContainableItemIdentifiers.ToImmutableHashSet<Identifier>();
			}
			else
			{
				immutableHashSet = (from p in CS$<>8__locals1.potentialContainablePrefabs
				select p.Identifier).ToImmutableHashSet<Identifier>();
			}
			ImmutableHashSet<Identifier> identifiers = immutableHashSet;
			AIObjectiveLoadItem.AllValidContainableItemIdentifiers.Add(this.Container.Prefab, identifiers);
			return identifiers;
		}

		// Token: 0x06002D8C RID: 11660 RVA: 0x001EB20C File Offset: 0x001E940C
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			Item container = this.Container;
			Character character = this.character;
			AIObjectiveLoadItems.ItemCondition? targetCondition = new AIObjectiveLoadItems.ItemCondition?(this.TargetItemCondition);
			if (!AIObjectiveLoadItems.IsValidTarget(container, character, null, targetCondition))
			{
				base.Priority = 0f;
			}
			else if (this.targetItem == null)
			{
				base.Priority = 0f;
			}
			else
			{
				AIObjectiveLoadItem.<>c__DisplayClass32_0 CS$<>8__locals1;
				CS$<>8__locals1.dist = 0f;
				if (this.character.CurrentHull != this.targetItem.CurrentHull)
				{
					this.<GetPriority>g__AddDistance|32_0(this.character.WorldPosition, this.targetItem.WorldPosition, ref CS$<>8__locals1);
				}
				if (this.targetItem.CurrentHull != this.Container.CurrentHull)
				{
					this.<GetPriority>g__AddDistance|32_0(this.targetItem.WorldPosition, this.Container.WorldPosition, ref CS$<>8__locals1);
				}
				float distanceFactor = base.GetDistanceFactor(this.targetItem.WorldPosition, 0f, 5f, 5000f, 0.9f);
				bool hasContainable = this.character.HasItem(this.targetItem, false, null);
				float devotion = (base.CumulatedDevotion + (hasContainable ? (100f - this.MaxDevotion) : 0f)) / 100f;
				float max = 60f - (float)(hasContainable ? 1 : 2);
				base.Priority = MathHelper.Lerp(0f, max, MathHelper.Clamp(devotion + distanceFactor * base.PriorityModifier, 0f, 1f));
				if (this.moveItemObjective != null && this.targetItem.Container != this.Container)
				{
					if (!this.IsValidContainable(this.targetItem))
					{
						this.moveItemObjective.Abandon = true;
					}
					else if (!this.ItemContainer.Inventory.CanBePut(this.targetItem) && this.ItemContainer.Inventory.AllItems.None((Item i) => AIObjectiveLoadItems.ItemMatchesTargetCondition(i, this.TargetItemCondition)))
					{
						this.moveItemObjective.Abandon = true;
					}
				}
				if (this.ItemContainer.Inventory.IsFull(false))
				{
					base.Priority /= 4f;
				}
			}
			return base.Priority;
		}

		// Token: 0x06002D8D RID: 11661 RVA: 0x001EB448 File Offset: 0x001E9648
		protected override void Act(float deltaTime)
		{
			if (this.targetItem == null)
			{
				Item item;
				if (this.character.FindItem(ref this.itemIndex, out item, this.ValidContainableItemIdentifiers, false, null, null, new Func<Item, bool>(this.IsValidContainable), new Func<Item, float>(this.<Act>g__GetPriority|33_3), 10000f, null))
				{
					if (item == null)
					{
						base.Abandon = true;
					}
					this.targetItem = item;
				}
				this.objectiveManager.GetObjective<AIObjectiveIdle>().Wander(deltaTime);
				return;
			}
			if (this.moveItemObjective == null && !this.IsValidContainable(this.targetItem))
			{
				this.IgnoreTargetItem();
				this.Reset();
				return;
			}
			base.TryAddSubObjective<AIObjectiveMoveItem>(ref this.moveItemObjective, () => new AIObjectiveMoveItem(this.character, this.targetItem, this.objectiveManager, null, this.ItemContainer, base.PriorityModifier)
			{
				AbandonGetItemDialogueCondition = (() => this.IsValidContainable(this.targetItem)),
				AbandonGetItemDialogueIdentifier = this.abandonGetItemDialogueIdentifier,
				Equip = true,
				RemoveExistingWhenNecessary = true,
				RemoveExistingPredicate = ((Item i) => !this.ValidContainableItemIdentifiers.Contains(i.Prefab.Identifier) || AIObjectiveLoadItems.ItemMatchesTargetCondition(i, this.TargetItemCondition)),
				RemoveExistingMax = new int?(1),
				AllowToFindDivingGear = this.objectiveManager.HasOrder<AIObjectiveLoadItems>(null)
			}, delegate
			{
				base.IsCompleted = true;
				base.RemoveSubObjective<AIObjectiveMoveItem>(ref this.moveItemObjective);
			}, delegate
			{
				this.IgnoreTargetItem();
				this.Reset();
			});
		}

		// Token: 0x06002D8E RID: 11662 RVA: 0x001EB510 File Offset: 0x001E9710
		private bool IsValidContainable(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item.Removed)
			{
				return false;
			}
			if (!this.ValidContainableItemIdentifiers.Contains(item.Prefab.Identifier))
			{
				return false;
			}
			if (this.ignoredItems.Contains(item))
			{
				return false;
			}
			if (item.Illegitimate == this.character.IsOnPlayerTeam)
			{
				return false;
			}
			if ((item.SpawnedInCurrentOutpost && !item.AllowStealing) == this.character.IsOnPlayerTeam)
			{
				return false;
			}
			Character owner = item.GetRootInventoryOwner() as Character;
			if (owner != null && owner != this.character)
			{
				return false;
			}
			for (Item parentItem = item.Container; parentItem != null; parentItem = parentItem.Container)
			{
				if (parentItem.HasTag(Tags.DontTakeItems))
				{
					return false;
				}
			}
			if (!item.HasAccess(this.character))
			{
				return false;
			}
			if (!this.character.HasItem(item, false, null) && !base.CanEquip(item, false))
			{
				return false;
			}
			if (!this.ItemContainer.CanBeContained(item))
			{
				return false;
			}
			if (AIObjectiveLoadItems.ItemMatchesTargetCondition(item, this.TargetItemCondition))
			{
				return false;
			}
			if (this.TargetItemCondition == AIObjectiveLoadItems.ItemCondition.Full)
			{
				if (this.TargetItemCondition == AIObjectiveLoadItems.ItemCondition.Full && item.ConditionIncreasedRecently)
				{
					return false;
				}
				ItemInventory itemInventory = item.ParentInventory as ItemInventory;
				bool flag;
				bool isSecondary;
				if (itemInventory != null && item.IsContainerPreferred(itemInventory.Container, out flag, out isSecondary, true) && !isSecondary)
				{
					return false;
				}
			}
			return !AIObjectiveLoadItems.IsValidTarget(item.Container, this.character, new ImmutableArray<Identifier>?(this.TargetContainerTags), null);
		}

		// Token: 0x06002D8F RID: 11663 RVA: 0x001EB68D File Offset: 0x001E988D
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06002D90 RID: 11664 RVA: 0x001EB695 File Offset: 0x001E9895
		public override void Reset()
		{
			base.Reset();
			this.moveItemObjective = null;
			this.itemIndex = 0;
		}

		// Token: 0x06002D91 RID: 11665 RVA: 0x001EB6AB File Offset: 0x001E98AB
		private void IgnoreTargetItem()
		{
			if (this.targetItem == null)
			{
				return;
			}
			this.ignoredItems.Add(this.targetItem);
			this.targetItem = null;
		}

		// Token: 0x06002D93 RID: 11667 RVA: 0x001EB6DC File Offset: 0x001E98DC
		[CompilerGenerated]
		private void <GetPriority>g__AddDistance|32_0(Vector2 startPos, Vector2 targetPos, ref AIObjectiveLoadItem.<>c__DisplayClass32_0 A_3)
		{
			float yDist = Math.Abs(startPos.Y - targetPos.Y);
			if (yDist > 100f)
			{
				A_3.dist += yDist * 5f;
			}
			A_3.dist += Math.Abs(this.character.WorldPosition.X - targetPos.X);
		}

		// Token: 0x06002D95 RID: 11669 RVA: 0x001EB750 File Offset: 0x001E9950
		[CompilerGenerated]
		private float <Act>g__GetPriority|33_3(Item item)
		{
			float num;
			try
			{
				AIObjectiveLoadItems.ItemCondition targetItemCondition = this.TargetItemCondition;
				if (targetItemCondition != AIObjectiveLoadItems.ItemCondition.Empty)
				{
					if (targetItemCondition != AIObjectiveLoadItems.ItemCondition.Full)
					{
						throw new NotImplementedException();
					}
					num = MathUtils.InverseLerp(100f, 0f, item.ConditionPercentage);
				}
				else
				{
					num = MathUtils.InverseLerp(0f, 100f, item.ConditionPercentage);
				}
				float conditionBasedPriority = num;
				num = (this.ItemContainer.ContainsItemsWithSameIdentifier(item) ? conditionBasedPriority : (conditionBasedPriority / 2f));
			}
			catch (NotImplementedException)
			{
				num = 0f;
			}
			return num;
		}

		// Token: 0x040017E2 RID: 6114
		private int itemIndex;

		// Token: 0x040017E3 RID: 6115
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x040017E4 RID: 6116
		private readonly HashSet<Item> ignoredItems = new HashSet<Item>();

		// Token: 0x040017E5 RID: 6117
		private Item targetItem;

		// Token: 0x040017E6 RID: 6118
		private readonly string abandonGetItemDialogueIdentifier = "dialogcannotfindloadable";

		// Token: 0x02000E2E RID: 3630
		private enum CheckStatus
		{
			// Token: 0x040051A8 RID: 20904
			Unfinished,
			// Token: 0x040051A9 RID: 20905
			Finished
		}
	}
}
