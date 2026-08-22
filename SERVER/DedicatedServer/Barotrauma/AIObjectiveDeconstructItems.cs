using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200006C RID: 108
	internal class AIObjectiveDeconstructItems : AIObjectiveLoop<Item>
	{
		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06000F01 RID: 3841 RVA: 0x0008F653 File Offset: 0x0008D853
		// (set) Token: 0x06000F02 RID: 3842 RVA: 0x0008F65B File Offset: 0x0008D85B
		public override Identifier Identifier { get; set; } = "deconstruct items".ToIdentifier();

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000F03 RID: 3843 RVA: 0x0008F664 File Offset: 0x0008D864
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x0008F66B File Offset: 0x0008D86B
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000F05 RID: 3845 RVA: 0x0008F66E File Offset: 0x0008D86E
		protected override int MaxTargets
		{
			get
			{
				return 10;
			}
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x0008F674 File Offset: 0x0008D874
		public AIObjectiveDeconstructItems(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x0008F6A4 File Offset: 0x0008D8A4
		public override void OnSelected()
		{
			base.OnSelected();
			if (!this.checkedDeconstructorExists)
			{
				if (this.character.Submarine == null || Item.ItemList.None((Item it) => it.GetComponent<Deconstructor>() != null && !it.IgnoreByAI(this.character) && it.IsInteractable(this.character) && this.character.Submarine.IsEntityFoundOnThisSub(it, true, true, true)))
				{
					Character character = this.character;
					string value = TextManager.Get("orderdialogself.deconstructitem.nodeconstructor").Value;
					Identifier identifier = "nodeconstructor".ToIdentifier();
					character.Speak(value, null, 5f, identifier, 30f);
					base.Abandon = true;
				}
				this.checkedDeconstructorExists = true;
			}
		}

		// Token: 0x06000F08 RID: 3848 RVA: 0x0008F72B File Offset: 0x0008D92B
		public override void Reset()
		{
			base.Reset();
			this.checkedDeconstructorExists = false;
		}

		// Token: 0x06000F09 RID: 3849 RVA: 0x0008F73A File Offset: 0x0008D93A
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				return this.objectiveManager.GetOrderPriority(this);
			}
			return 49.5f;
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x0008F770 File Offset: 0x0008D970
		protected override bool IsValidTarget(Item target)
		{
			if (target == null || target.Removed)
			{
				return false;
			}
			if (target.Prefab.DeconstructItems.Any<DeconstructItem>())
			{
				if (target.Prefab.DeconstructItems.All((DeconstructItem d) => d.RequiredOtherItem.Length != 0))
				{
					return false;
				}
			}
			if (!AIObjectiveDeconstructItems.IsValidTarget(target, this.character, true))
			{
				return base.Objectives.ContainsKey(target) && AIObjectiveCleanupItems.IsItemInsideValidSubmarine(target, this.character);
			}
			if (target.CurrentHull != null && target.CurrentHull.FireSources.Count > 0)
			{
				return false;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c != this.character && HumanAIController.IsActive(c))
				{
					if (c.CurrentHull == target.CurrentHull && !base.HumanAIController.IsFriendly(c, false))
					{
						return false;
					}
					if (c.TeamID == this.character.TeamID)
					{
						HumanAIController humanAi = c.AIController as HumanAIController;
						if (humanAi != null)
						{
							AIObjectiveDeconstructItem deconstruct = humanAi.ObjectiveManager.CurrentObjective as AIObjectiveDeconstructItem;
							if (deconstruct != null && deconstruct.Item == target)
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x0008F8D8 File Offset: 0x0008DAD8
		protected override IEnumerable<Item> GetList()
		{
			return Item.DeconstructItems;
		}

		// Token: 0x06000F0C RID: 3852 RVA: 0x0008F8DF File Offset: 0x0008DADF
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveDeconstructItem(item, this.character, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06000F0D RID: 3853 RVA: 0x0008F8F9 File Offset: 0x0008DAF9
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveDeconstructItems, Item>(this.character, target);
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x0008F907 File Offset: 0x0008DB07
		private static bool IsValidTarget(Item item, Character character, bool checkInventory)
		{
			return item != null && !item.Removed && (item.GetRootInventoryOwner() == character || AIObjectiveCleanupItems.IsValidTarget(item, character, checkInventory, true, false, false));
		}

		// Token: 0x06000F0F RID: 3855 RVA: 0x0008F92C File Offset: 0x0008DB2C
		public override void OnDeselected()
		{
			base.OnDeselected();
			foreach (AIObjective subObjective in base.SubObjectives)
			{
				AIObjectiveDeconstructItem deconstructObjective = subObjective as AIObjectiveDeconstructItem;
				if (deconstructObjective != null)
				{
					deconstructObjective.DropTarget();
				}
			}
		}

		// Token: 0x0400070F RID: 1807
		private bool checkedDeconstructorExists;
	}
}
