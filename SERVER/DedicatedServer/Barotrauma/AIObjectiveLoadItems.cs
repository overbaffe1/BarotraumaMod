using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200007C RID: 124
	internal class AIObjectiveLoadItems : AIObjectiveLoop<Item>
	{
		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06001093 RID: 4243 RVA: 0x00098A2A File Offset: 0x00096C2A
		// (set) Token: 0x06001094 RID: 4244 RVA: 0x00098A32 File Offset: 0x00096C32
		public override Identifier Identifier { get; set; } = "load items".ToIdentifier();

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06001095 RID: 4245 RVA: 0x00098A3B File Offset: 0x00096C3B
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 20f;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06001096 RID: 4246 RVA: 0x00098A42 File Offset: 0x00096C42
		protected override bool ResetWhenClearingIgnoreList
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06001097 RID: 4247 RVA: 0x00098A45 File Offset: 0x00096C45
		private ImmutableArray<Identifier> TargetContainerTags { get; }

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06001098 RID: 4248 RVA: 0x00098A4D File Offset: 0x00096C4D
		private List<Item> TargetContainers { get; } = new List<Item>();

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06001099 RID: 4249 RVA: 0x00098A55 File Offset: 0x00096C55
		private AIObjectiveLoadItems.ItemCondition TargetCondition { get; }

		// Token: 0x0600109A RID: 4250 RVA: 0x00098A60 File Offset: 0x00096C60
		public AIObjectiveLoadItems(Character character, AIObjectiveManager objectiveManager, Identifier option, ImmutableArray<Identifier> containerTags, Item targetContainer = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, option)
		{
			if ((new ImmutableArray<Identifier>?(containerTags) == null || containerTags.None(null)) && targetContainer == null)
			{
				base.Abandon = true;
				return;
			}
			this.TargetContainerTags = containerTags.ToImmutableArray<Identifier>();
			if (targetContainer != null)
			{
				this.TargetContainers.Add(targetContainer);
			}
			else
			{
				foreach (Item item in Item.ItemList)
				{
					if (OrderPrefab.TargetItemsMatchItem(this.TargetContainerTags, item))
					{
						this.TargetContainers.Add(item);
					}
				}
			}
			this.TargetCondition = ((option == "turretammo") ? 0 : 1);
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x00098B5C File Offset: 0x00096D5C
		protected override bool IsValidTarget(Item target)
		{
			return AIObjectiveLoadItems.IsValidTarget(target, this.character, null, new AIObjectiveLoadItems.ItemCondition?(this.TargetCondition)) && target.CurrentHull != null && target.CurrentHull.FireSources.Count <= 0 && !Character.CharacterList.Any((Character c) => c.CurrentHull == target.CurrentHull && !this.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c));
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x00098BE8 File Offset: 0x00096DE8
		public static bool IsValidTarget(Item item, Character character, ImmutableArray<Identifier>? targetContainerTags = null, AIObjectiveLoadItems.ItemCondition? targetCondition = null)
		{
			if (item == null || item.Removed)
			{
				return false;
			}
			if (targetContainerTags != null && !OrderPrefab.TargetItemsMatchItem(targetContainerTags.Value, item))
			{
				return false;
			}
			ItemContainer container = item.GetComponent<ItemContainer>();
			if (container == null)
			{
				return false;
			}
			if (container.Inventory == null)
			{
				return false;
			}
			if (targetCondition != null && container.Inventory.IsFull(false) && container.Inventory.AllItems.None((Item i) => AIObjectiveLoadItems.ItemMatchesTargetCondition(i, targetCondition.Value)))
			{
				return false;
			}
			if (!AIObjectiveCleanupItems.IsItemInsideValidSubmarine(item, character))
			{
				return false;
			}
			Character owner = item.GetRootInventoryOwner() as Character;
			if (owner != null && owner != character)
			{
				return false;
			}
			if (item.IsClaimedByBallastFlora)
			{
				return false;
			}
			if (!item.HasAccess(character))
			{
				return false;
			}
			Powered component = item.GetComponent<Powered>();
			return component == null || component.PowerConsumption <= 0f || component.HasPower;
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x00098CD4 File Offset: 0x00096ED4
		public static bool ItemMatchesTargetCondition(Item item, AIObjectiveLoadItems.ItemCondition targetCondition)
		{
			if (item == null)
			{
				return false;
			}
			bool result;
			try
			{
				bool flag;
				if (targetCondition != AIObjectiveLoadItems.ItemCondition.Empty)
				{
					if (targetCondition != AIObjectiveLoadItems.ItemCondition.Full)
					{
						throw new NotImplementedException();
					}
					flag = item.IsFullCondition;
				}
				else
				{
					flag = (item.Condition <= 0.1f);
				}
				result = flag;
			}
			catch (NotImplementedException)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x00098D2C File Offset: 0x00096F2C
		protected override IEnumerable<Item> GetList()
		{
			return this.TargetContainers;
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x00098D34 File Offset: 0x00096F34
		protected override AIObjective ObjectiveConstructor(Item target)
		{
			return new AIObjectiveLoadItem(target, this.TargetContainerTags, this.TargetCondition, this.Option, this.character, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x060010A0 RID: 4256 RVA: 0x00098D60 File Offset: 0x00096F60
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveLoadItems, Item>(this.character, target);
		}

		// Token: 0x060010A1 RID: 4257 RVA: 0x00098D70 File Offset: 0x00096F70
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				float prio = this.objectiveManager.GetOrderPriority(this);
				if (this.subObjectives.All((AIObjective so) => so.SubObjectives.None(null) || so.Priority <= 0f))
				{
					base.ForceWalkTemporarily = true;
				}
				return prio;
			}
			return 49.5f;
		}

		// Token: 0x020007F0 RID: 2032
		public enum ItemCondition
		{
			// Token: 0x04002E5B RID: 11867
			Empty,
			// Token: 0x04002E5C RID: 11868
			Full
		}
	}
}
