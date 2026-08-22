using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200016E RID: 366
	internal class AIObjectiveCleanupItems : AIObjectiveLoop<Item>
	{
		// Token: 0x17000AE7 RID: 2791
		// (get) Token: 0x06002B5D RID: 11101 RVA: 0x001DDD25 File Offset: 0x001DBF25
		// (set) Token: 0x06002B5E RID: 11102 RVA: 0x001DDD2D File Offset: 0x001DBF2D
		public override Identifier Identifier { get; set; } = "cleanup items".ToIdentifier();

		// Token: 0x17000AE8 RID: 2792
		// (get) Token: 0x06002B5F RID: 11103 RVA: 0x001DDD36 File Offset: 0x001DBF36
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AE9 RID: 2793
		// (get) Token: 0x06002B60 RID: 11104 RVA: 0x001DDD39 File Offset: 0x001DBF39
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AEA RID: 2794
		// (get) Token: 0x06002B61 RID: 11105 RVA: 0x001DDD3C File Offset: 0x001DBF3C
		protected override bool ForceOrderPriority
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AEB RID: 2795
		// (get) Token: 0x06002B62 RID: 11106 RVA: 0x001DDD3F File Offset: 0x001DBF3F
		protected override int MaxTargets
		{
			get
			{
				return 100;
			}
		}

		// Token: 0x06002B63 RID: 11107 RVA: 0x001DDD44 File Offset: 0x001DBF44
		public AIObjectiveCleanupItems(Character character, AIObjectiveManager objectiveManager, Item prioritizedItem = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			if (prioritizedItem != null)
			{
				this.prioritizedItems.Add(prioritizedItem);
			}
		}

		// Token: 0x06002B64 RID: 11108 RVA: 0x001DDD90 File Offset: 0x001DBF90
		public AIObjectiveCleanupItems(Character character, AIObjectiveManager objectiveManager, IEnumerable<Item> prioritizedItems, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.prioritizedItems.AddRange(from i in prioritizedItems
			where i != null
			select i);
		}

		// Token: 0x06002B65 RID: 11109 RVA: 0x001DDDFC File Offset: 0x001DBFFC
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				float prio = this.objectiveManager.GetOrderPriority(this);
				if (this.subObjectives.All((AIObjective so) => so.SubObjectives.None(null)))
				{
					base.ForceWalkTemporarily = true;
				}
				return prio;
			}
			return 49.5f;
		}

		// Token: 0x06002B66 RID: 11110 RVA: 0x001DDE74 File Offset: 0x001DC074
		protected override bool IsValidTarget(Item target)
		{
			if (!AIObjectiveCleanupItems.IsValidTarget(target, this.character, true, true, true, true))
			{
				return base.Objectives.ContainsKey(target) && AIObjectiveCleanupItems.IsItemInsideValidSubmarine(target, this.character);
			}
			if (target.CurrentHull.FireSources.Count > 0)
			{
				return false;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c != this.character && HumanAIController.IsActive(c) && c.CurrentHull == target.CurrentHull && !base.HumanAIController.IsFriendly(c, false))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002B67 RID: 11111 RVA: 0x001DDF38 File Offset: 0x001DC138
		protected override IEnumerable<Item> GetList()
		{
			return Item.CleanableItems;
		}

		// Token: 0x06002B68 RID: 11112 RVA: 0x001DDF3F File Offset: 0x001DC13F
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveCleanupItem(item, this.character, this.objectiveManager, base.PriorityModifier)
			{
				IsPriority = this.prioritizedItems.Contains(item)
			};
		}

		// Token: 0x06002B69 RID: 11113 RVA: 0x001DDF6B File Offset: 0x001DC16B
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveCleanupItems, Item>(this.character, target);
		}

		// Token: 0x06002B6A RID: 11114 RVA: 0x001DDF7C File Offset: 0x001DC17C
		public static bool IsItemInsideValidSubmarine(Item item, Character character)
		{
			return item != null && !item.Removed && character != null && !character.Removed && item.CurrentHull != null && item.Submarine != null && item.Submarine.TeamID == character.TeamID && (character.Submarine == null || character.Submarine.IsConnectedTo(item.Submarine));
		}

		// Token: 0x06002B6B RID: 11115 RVA: 0x001DDFEC File Offset: 0x001DC1EC
		public static bool IsValidContainer(Item container, Character character)
		{
			return container.HasTag(Tags.AllowCleanup) && container.HasAccess(character) && container.ParentInventory == null && container.OwnInventory != null && container.OwnInventory.AllItems.Any<Item>() && container.GetComponent<ItemContainer>() != null && AIObjectiveCleanupItems.IsItemInsideValidSubmarine(container, character) && !container.IsClaimedByBallastFlora;
		}

		// Token: 0x06002B6C RID: 11116 RVA: 0x001DE050 File Offset: 0x001DC250
		public static bool IsValidTarget(Item item, Character character, bool checkInventory, bool allowUnloading = true, bool requireValidContainer = true, bool ignoreItemsMarkedForDeconstruction = true)
		{
			if (item == null)
			{
				return false;
			}
			if (item.GetComponents<Pickable>().None((Pickable c) => !(c is Door) && c.CanBePicked))
			{
				return false;
			}
			if (item.DontCleanUp)
			{
				return false;
			}
			if (item.Illegitimate == character.IsOnPlayerTeam)
			{
				return false;
			}
			if (item.ParentInventory != null)
			{
				if (item.Container == null)
				{
					return false;
				}
				if (!allowUnloading)
				{
					return false;
				}
				if (requireValidContainer && !AIObjectiveCleanupItems.IsValidContainer(item.Container, character))
				{
					return false;
				}
			}
			if (ignoreItemsMarkedForDeconstruction && Item.DeconstructItems.Contains(item))
			{
				return false;
			}
			if (!item.HasAccess(character))
			{
				return false;
			}
			if (character != null && !AIObjectiveCleanupItems.IsItemInsideValidSubmarine(item, character))
			{
				return false;
			}
			if (item.HasBallastFloraInHull)
			{
				return false;
			}
			if ((double)item.LastEatenTime > Timing.TotalTimeUnpaused - 1.0)
			{
				return false;
			}
			Wire wire = item.GetComponent<Wire>();
			if (wire != null)
			{
				if (wire.Connections.Any((Connection c) => c != null))
				{
					return false;
				}
			}
			else
			{
				ConnectionPanel connectionPanel = item.GetComponent<ConnectionPanel>();
				if (connectionPanel != null)
				{
					if (connectionPanel.Connections.Any((Connection c) => c.Wires.Count > 0))
					{
						return false;
					}
				}
			}
			Rope component = item.GetComponent<Rope>();
			return (component == null || !component.IsActive || component.Snapped) && (!checkInventory || AIObjective.CanPutInInventory(character, item, false));
		}

		// Token: 0x06002B6D RID: 11117 RVA: 0x001DE1C0 File Offset: 0x001DC3C0
		public override void OnDeselected()
		{
			base.OnDeselected();
			foreach (AIObjective subObjective in base.SubObjectives)
			{
				AIObjectiveCleanupItem cleanUpObjective = subObjective as AIObjectiveCleanupItem;
				if (cleanUpObjective != null)
				{
					cleanUpObjective.DropTarget();
				}
			}
		}

		// Token: 0x040016AE RID: 5806
		public readonly List<Item> prioritizedItems = new List<Item>();
	}
}
