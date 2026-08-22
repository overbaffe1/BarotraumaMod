using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000182 RID: 386
	internal class AIObjectiveLoadItems : AIObjectiveLoop<Item>
	{
		// Token: 0x17000B99 RID: 2969
		// (get) Token: 0x06002D9B RID: 11675 RVA: 0x001EB8BE File Offset: 0x001E9ABE
		// (set) Token: 0x06002D9C RID: 11676 RVA: 0x001EB8C6 File Offset: 0x001E9AC6
		public override Identifier Identifier { get; set; } = "load items".ToIdentifier();

		// Token: 0x17000B9A RID: 2970
		// (get) Token: 0x06002D9D RID: 11677 RVA: 0x001EB8CF File Offset: 0x001E9ACF
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 20f;
			}
		}

		// Token: 0x17000B9B RID: 2971
		// (get) Token: 0x06002D9E RID: 11678 RVA: 0x001EB8D6 File Offset: 0x001E9AD6
		protected override bool ResetWhenClearingIgnoreList
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B9C RID: 2972
		// (get) Token: 0x06002D9F RID: 11679 RVA: 0x001EB8D9 File Offset: 0x001E9AD9
		private ImmutableArray<Identifier> TargetContainerTags { get; }

		// Token: 0x17000B9D RID: 2973
		// (get) Token: 0x06002DA0 RID: 11680 RVA: 0x001EB8E1 File Offset: 0x001E9AE1
		private List<Item> TargetContainers { get; } = new List<Item>();

		// Token: 0x17000B9E RID: 2974
		// (get) Token: 0x06002DA1 RID: 11681 RVA: 0x001EB8E9 File Offset: 0x001E9AE9
		private AIObjectiveLoadItems.ItemCondition TargetCondition { get; }

		// Token: 0x06002DA2 RID: 11682 RVA: 0x001EB8F4 File Offset: 0x001E9AF4
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

		// Token: 0x06002DA3 RID: 11683 RVA: 0x001EB9F0 File Offset: 0x001E9BF0
		protected override bool IsValidTarget(Item target)
		{
			return AIObjectiveLoadItems.IsValidTarget(target, this.character, null, new AIObjectiveLoadItems.ItemCondition?(this.TargetCondition)) && target.CurrentHull != null && target.CurrentHull.FireSources.Count <= 0 && !Character.CharacterList.Any((Character c) => c.CurrentHull == target.CurrentHull && !this.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c));
		}

		// Token: 0x06002DA4 RID: 11684 RVA: 0x001EBA7C File Offset: 0x001E9C7C
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

		// Token: 0x06002DA5 RID: 11685 RVA: 0x001EBB68 File Offset: 0x001E9D68
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

		// Token: 0x06002DA6 RID: 11686 RVA: 0x001EBBC0 File Offset: 0x001E9DC0
		protected override IEnumerable<Item> GetList()
		{
			return this.TargetContainers;
		}

		// Token: 0x06002DA7 RID: 11687 RVA: 0x001EBBC8 File Offset: 0x001E9DC8
		protected override AIObjective ObjectiveConstructor(Item target)
		{
			return new AIObjectiveLoadItem(target, this.TargetContainerTags, this.TargetCondition, this.Option, this.character, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06002DA8 RID: 11688 RVA: 0x001EBBF4 File Offset: 0x001E9DF4
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveLoadItems, Item>(this.character, target);
		}

		// Token: 0x06002DA9 RID: 11689 RVA: 0x001EBC04 File Offset: 0x001E9E04
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

		// Token: 0x02000E38 RID: 3640
		public enum ItemCondition
		{
			// Token: 0x040051BB RID: 20923
			Empty,
			// Token: 0x040051BC RID: 20924
			Full
		}
	}
}
