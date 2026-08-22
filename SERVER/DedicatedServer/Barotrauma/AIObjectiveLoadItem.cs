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
	// Token: 0x0200007B RID: 123
	internal class AIObjectiveLoadItem : AIObjective
	{
		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06001079 RID: 4217 RVA: 0x0009811E File Offset: 0x0009631E
		// (set) Token: 0x0600107A RID: 4218 RVA: 0x00098126 File Offset: 0x00096326
		public override Identifier Identifier { get; set; } = "load item".ToIdentifier();

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x0600107B RID: 4219 RVA: 0x0009812F File Offset: 0x0009632F
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x0600107C RID: 4220 RVA: 0x00098132 File Offset: 0x00096332
		private AIObjectiveLoadItems.ItemCondition TargetItemCondition { get; }

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x0600107D RID: 4221 RVA: 0x0009813A File Offset: 0x0009633A
		private Item Container { get; }

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x0600107E RID: 4222 RVA: 0x00098142 File Offset: 0x00096342
		private ItemContainer ItemContainer { get; }

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x0600107F RID: 4223 RVA: 0x0009814A File Offset: 0x0009634A
		private ImmutableArray<Identifier> TargetContainerTags { get; }

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06001080 RID: 4224 RVA: 0x00098152 File Offset: 0x00096352
		private ImmutableHashSet<Identifier> ValidContainableItemIdentifiers { get; }

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06001081 RID: 4225 RVA: 0x0009815A File Offset: 0x0009635A
		private static Dictionary<ItemPrefab, ImmutableHashSet<Identifier>> AllValidContainableItemIdentifiers { get; } = new Dictionary<ItemPrefab, ImmutableHashSet<Identifier>>();

		// Token: 0x06001082 RID: 4226 RVA: 0x00098164 File Offset: 0x00096364
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

		// Token: 0x06001083 RID: 4227 RVA: 0x0009825C File Offset: 0x0009645C
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

		// Token: 0x06001084 RID: 4228 RVA: 0x00098378 File Offset: 0x00096578
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

		// Token: 0x06001085 RID: 4229 RVA: 0x000985B4 File Offset: 0x000967B4
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

		// Token: 0x06001086 RID: 4230 RVA: 0x0009867C File Offset: 0x0009687C
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

		// Token: 0x06001087 RID: 4231 RVA: 0x000987F9 File Offset: 0x000969F9
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x00098801 File Offset: 0x00096A01
		public override void Reset()
		{
			base.Reset();
			this.moveItemObjective = null;
			this.itemIndex = 0;
		}

		// Token: 0x06001089 RID: 4233 RVA: 0x00098817 File Offset: 0x00096A17
		private void IgnoreTargetItem()
		{
			if (this.targetItem == null)
			{
				return;
			}
			this.ignoredItems.Add(this.targetItem);
			this.targetItem = null;
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x00098848 File Offset: 0x00096A48
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

		// Token: 0x0600108D RID: 4237 RVA: 0x000988BC File Offset: 0x00096ABC
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

		// Token: 0x040007E8 RID: 2024
		private int itemIndex;

		// Token: 0x040007E9 RID: 2025
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x040007EA RID: 2026
		private readonly HashSet<Item> ignoredItems = new HashSet<Item>();

		// Token: 0x040007EB RID: 2027
		private Item targetItem;

		// Token: 0x040007EC RID: 2028
		private readonly string abandonGetItemDialogueIdentifier = "dialogcannotfindloadable";

		// Token: 0x020007E6 RID: 2022
		private enum CheckStatus
		{
			// Token: 0x04002E48 RID: 11848
			Unfinished,
			// Token: 0x04002E49 RID: 11849
			Finished
		}
	}
}
