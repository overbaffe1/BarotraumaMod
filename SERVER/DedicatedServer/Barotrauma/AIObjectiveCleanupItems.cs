using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000068 RID: 104
	internal class AIObjectiveCleanupItems : AIObjectiveLoop<Item>
	{
		// Token: 0x170003CD RID: 973
		// (get) Token: 0x06000E55 RID: 3669 RVA: 0x0008AE91 File Offset: 0x00089091
		// (set) Token: 0x06000E56 RID: 3670 RVA: 0x0008AE99 File Offset: 0x00089099
		public override Identifier Identifier { get; set; } = "cleanup items".ToIdentifier();

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x06000E57 RID: 3671 RVA: 0x0008AEA2 File Offset: 0x000890A2
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06000E58 RID: 3672 RVA: 0x0008AEA5 File Offset: 0x000890A5
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06000E59 RID: 3673 RVA: 0x0008AEA8 File Offset: 0x000890A8
		protected override bool ForceOrderPriority
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06000E5A RID: 3674 RVA: 0x0008AEAB File Offset: 0x000890AB
		protected override int MaxTargets
		{
			get
			{
				return 100;
			}
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x0008AEB0 File Offset: 0x000890B0
		public AIObjectiveCleanupItems(Character character, AIObjectiveManager objectiveManager, Item prioritizedItem = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			if (prioritizedItem != null)
			{
				this.prioritizedItems.Add(prioritizedItem);
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x0008AEFC File Offset: 0x000890FC
		public AIObjectiveCleanupItems(Character character, AIObjectiveManager objectiveManager, IEnumerable<Item> prioritizedItems, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.prioritizedItems.AddRange(from i in prioritizedItems
			where i != null
			select i);
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x0008AF68 File Offset: 0x00089168
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

		// Token: 0x06000E5E RID: 3678 RVA: 0x0008AFE0 File Offset: 0x000891E0
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

		// Token: 0x06000E5F RID: 3679 RVA: 0x0008B0A4 File Offset: 0x000892A4
		protected override IEnumerable<Item> GetList()
		{
			return Item.CleanableItems;
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x0008B0AB File Offset: 0x000892AB
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveCleanupItem(item, this.character, this.objectiveManager, base.PriorityModifier)
			{
				IsPriority = this.prioritizedItems.Contains(item)
			};
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x0008B0D7 File Offset: 0x000892D7
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveCleanupItems, Item>(this.character, target);
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x0008B0E8 File Offset: 0x000892E8
		public static bool IsItemInsideValidSubmarine(Item item, Character character)
		{
			return item != null && !item.Removed && character != null && !character.Removed && item.CurrentHull != null && item.Submarine != null && item.Submarine.TeamID == character.TeamID && (character.Submarine == null || character.Submarine.IsConnectedTo(item.Submarine));
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x0008B158 File Offset: 0x00089358
		public static bool IsValidContainer(Item container, Character character)
		{
			return container.HasTag(Tags.AllowCleanup) && container.HasAccess(character) && container.ParentInventory == null && container.OwnInventory != null && container.OwnInventory.AllItems.Any<Item>() && container.GetComponent<ItemContainer>() != null && AIObjectiveCleanupItems.IsItemInsideValidSubmarine(container, character) && !container.IsClaimedByBallastFlora;
		}

		// Token: 0x06000E64 RID: 3684 RVA: 0x0008B1BC File Offset: 0x000893BC
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

		// Token: 0x06000E65 RID: 3685 RVA: 0x0008B32C File Offset: 0x0008952C
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

		// Token: 0x040006B4 RID: 1716
		public readonly List<Item> prioritizedItems = new List<Item>();
	}
}
