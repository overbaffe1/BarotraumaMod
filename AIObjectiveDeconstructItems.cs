using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000172 RID: 370
	internal class AIObjectiveDeconstructItems : AIObjectiveLoop<Item>
	{
		// Token: 0x17000B13 RID: 2835
		// (get) Token: 0x06002C09 RID: 11273 RVA: 0x001E24E7 File Offset: 0x001E06E7
		// (set) Token: 0x06002C0A RID: 11274 RVA: 0x001E24EF File Offset: 0x001E06EF
		public override Identifier Identifier { get; set; } = "deconstruct items".ToIdentifier();

		// Token: 0x17000B14 RID: 2836
		// (get) Token: 0x06002C0B RID: 11275 RVA: 0x001E24F8 File Offset: 0x001E06F8
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x17000B15 RID: 2837
		// (get) Token: 0x06002C0C RID: 11276 RVA: 0x001E24FF File Offset: 0x001E06FF
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B16 RID: 2838
		// (get) Token: 0x06002C0D RID: 11277 RVA: 0x001E2502 File Offset: 0x001E0702
		protected override int MaxTargets
		{
			get
			{
				return 10;
			}
		}

		// Token: 0x06002C0E RID: 11278 RVA: 0x001E2508 File Offset: 0x001E0708
		public AIObjectiveDeconstructItems(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06002C0F RID: 11279 RVA: 0x001E2538 File Offset: 0x001E0738
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

		// Token: 0x06002C10 RID: 11280 RVA: 0x001E25BF File Offset: 0x001E07BF
		public override void Reset()
		{
			base.Reset();
			this.checkedDeconstructorExists = false;
		}

		// Token: 0x06002C11 RID: 11281 RVA: 0x001E25CE File Offset: 0x001E07CE
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

		// Token: 0x06002C12 RID: 11282 RVA: 0x001E2604 File Offset: 0x001E0804
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

		// Token: 0x06002C13 RID: 11283 RVA: 0x001E276C File Offset: 0x001E096C
		protected override IEnumerable<Item> GetList()
		{
			return Item.DeconstructItems;
		}

		// Token: 0x06002C14 RID: 11284 RVA: 0x001E2773 File Offset: 0x001E0973
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveDeconstructItem(item, this.character, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06002C15 RID: 11285 RVA: 0x001E278D File Offset: 0x001E098D
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveDeconstructItems, Item>(this.character, target);
		}

		// Token: 0x06002C16 RID: 11286 RVA: 0x001E279B File Offset: 0x001E099B
		private static bool IsValidTarget(Item item, Character character, bool checkInventory)
		{
			return item != null && !item.Removed && (item.GetRootInventoryOwner() == character || AIObjectiveCleanupItems.IsValidTarget(item, character, checkInventory, true, false, false));
		}

		// Token: 0x06002C17 RID: 11287 RVA: 0x001E27C0 File Offset: 0x001E09C0
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

		// Token: 0x04001709 RID: 5897
		private bool checkedDeconstructorExists;
	}
}
