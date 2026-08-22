using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000186 RID: 390
	internal class AIObjectiveOperateItem : AIObjective
	{
		// Token: 0x17000BC5 RID: 3013
		// (get) Token: 0x06002E1E RID: 11806 RVA: 0x001EE28B File Offset: 0x001EC48B
		// (set) Token: 0x06002E1F RID: 11807 RVA: 0x001EE293 File Offset: 0x001EC493
		public override Identifier Identifier { get; set; } = "operate item".ToIdentifier();

		// Token: 0x17000BC6 RID: 3014
		// (get) Token: 0x06002E20 RID: 11808 RVA: 0x001EE29C File Offset: 0x001EC49C
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(this.component.Name);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000BC7 RID: 3015
		// (get) Token: 0x06002E21 RID: 11809 RVA: 0x001EE2F0 File Offset: 0x001EC4F0
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BC8 RID: 3016
		// (get) Token: 0x06002E22 RID: 11810 RVA: 0x001EE2F3 File Offset: 0x001EC4F3
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BC9 RID: 3017
		// (get) Token: 0x06002E23 RID: 11811 RVA: 0x001EE2F6 File Offset: 0x001EC4F6
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BCA RID: 3018
		// (get) Token: 0x06002E24 RID: 11812 RVA: 0x001EE2F9 File Offset: 0x001EC4F9
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BCB RID: 3019
		// (get) Token: 0x06002E25 RID: 11813 RVA: 0x001EE2FC File Offset: 0x001EC4FC
		public override bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				ItemComponent itemComponent = this.component;
				return itemComponent is Reactor || itemComponent is Turret;
			}
		}

		// Token: 0x17000BCC RID: 3020
		// (get) Token: 0x06002E26 RID: 11814 RVA: 0x001EE327 File Offset: 0x001EC527
		// (set) Token: 0x06002E27 RID: 11815 RVA: 0x001EE32F File Offset: 0x001EC52F
		public bool Override { get; set; } = true;

		// Token: 0x17000BCD RID: 3021
		// (get) Token: 0x06002E28 RID: 11816 RVA: 0x001EE338 File Offset: 0x001EC538
		// (set) Token: 0x06002E29 RID: 11817 RVA: 0x001EE340 File Offset: 0x001EC540
		public bool Repeat { get; set; }

		// Token: 0x17000BCE RID: 3022
		// (get) Token: 0x06002E2A RID: 11818 RVA: 0x001EE349 File Offset: 0x001EC549
		public override bool CanBeCompleted
		{
			get
			{
				return base.CanBeCompleted && (!this.useController || this.controller != null);
			}
		}

		// Token: 0x06002E2B RID: 11819 RVA: 0x001EE368 File Offset: 0x001EC568
		public override bool IsDuplicate<T>(T otherObjective)
		{
			if (base.IsDuplicate<T>(otherObjective))
			{
				AIObjectiveOperateItem operateObjective = otherObjective as AIObjectiveOperateItem;
				if (operateObjective != null)
				{
					return operateObjective.component == this.component;
				}
			}
			return false;
		}

		// Token: 0x17000BCF RID: 3023
		// (get) Token: 0x06002E2C RID: 11820 RVA: 0x001EE39D File Offset: 0x001EC59D
		public Entity OperateTarget
		{
			get
			{
				return this.operateTarget;
			}
		}

		// Token: 0x17000BD0 RID: 3024
		// (get) Token: 0x06002E2D RID: 11821 RVA: 0x001EE3A5 File Offset: 0x001EC5A5
		public ItemComponent Component
		{
			get
			{
				return this.component;
			}
		}

		// Token: 0x06002E2E RID: 11822 RVA: 0x001EE3AD File Offset: 0x001EC5AD
		public ItemComponent GetTarget()
		{
			if (!this.useController)
			{
				return this.component;
			}
			return this.controller;
		}

		// Token: 0x06002E2F RID: 11823 RVA: 0x001EE3C4 File Offset: 0x001EC5C4
		protected override float GetPriority()
		{
			bool isOrder = this.objectiveManager.IsOrder(this);
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			if (!isOrder && this.component.Item.ConditionPercentage <= 0f)
			{
				base.Priority = 0f;
			}
			else
			{
				AIObjectiveOperateItem.<>c__DisplayClass43_0 CS$<>8__locals1 = new AIObjectiveOperateItem.<>c__DisplayClass43_0();
				CS$<>8__locals1.<>4__this = this;
				if (this.OverridePriority != null)
				{
					base.Priority = this.OverridePriority.Value;
				}
				else if (isOrder)
				{
					base.Priority = this.objectiveManager.GetOrderPriority(this);
				}
				CS$<>8__locals1.target = this.GetTarget();
				ItemComponent target = CS$<>8__locals1.target;
				Item targetItem = (target != null) ? target.Item : null;
				if (targetItem == null)
				{
					base.Abandon = true;
					base.Priority = 0f;
					return base.Priority;
				}
				if (targetItem.IsClaimedByBallastFlora)
				{
					base.Priority = 0f;
					return base.Priority;
				}
				Hull targetHull = targetItem.CurrentHull;
				if (base.HumanAIController.UnsafeHulls.Contains(targetHull))
				{
					base.Priority = 0f;
					if (isOrder && this == this.objectiveManager.CurrentObjective && this.character.IsOnPlayerTeam)
					{
						Character character = this.character;
						string value3 = TextManager.GetWithVariable("dialogoperatetargetroomisunsafe", "[item]", targetItem.Name, FormatCapitals.No).Value;
						Identifier identifier = "dialogoperatetargetroomisunsafe".ToIdentifier();
						character.Speak(value3, null, 1f, identifier, 5f);
					}
					return base.Priority;
				}
				Reactor reactor = this.component.Item.GetComponent<Reactor>();
				if (reactor != null)
				{
					if (!isOrder && reactor.LastUserWasPlayer && this.character.IsOnPlayerTeam)
					{
						base.Priority = 0f;
						return base.Priority;
					}
					string a = this.Option.Value.ToLowerInvariant();
					if (!(a == "shutdown"))
					{
						if (a == "powerup")
						{
							if (CS$<>8__locals1.<GetPriority>g__IsAnotherOrderTargetingSameItem|1(this.objectiveManager.ForcedOrder) || this.objectiveManager.CurrentOrders.Any((Order o) => base.<GetPriority>g__IsAnotherOrderTargetingSameItem|1(o.Objective)))
							{
								base.Priority = 0f;
								return base.Priority;
							}
						}
					}
					else if (!reactor.PowerOn)
					{
						base.Priority = 0f;
						return base.Priority;
					}
				}
				else if (!isOrder)
				{
					Steering steering = this.component.Item.GetComponent<Steering>();
					if (steering != null && (steering.AutoPilot || base.HumanAIController.IsTrueForAnyCrewMember((Character c) => c != this.character && c.IsCaptain, true, true)))
					{
						base.Priority = 0f;
						return base.Priority;
					}
				}
				if (targetItem.CurrentHull == null || (targetItem.Submarine != this.character.Submarine && !isOrder) || this.IsItemOperatedByAnother(CS$<>8__locals1.target) || this.component.Item.IgnoreByAI(this.character) || (this.useController && this.controller.Item.IgnoreByAI(this.character)))
				{
					base.Priority = 0f;
				}
				else if (isOrder)
				{
					float max = this.objectiveManager.GetOrderPriority(this);
					float value = base.CumulatedDevotion + max * base.PriorityModifier;
					base.Priority = MathHelper.Clamp(value, 0f, max);
				}
				else if (this.OverridePriority == null)
				{
					float value2 = base.CumulatedDevotion + 60f * base.PriorityModifier;
					if (reactor != null && reactor.PowerOn && reactor.FissionRate > 1f && reactor.AutoTemp && this.Option == "powerup")
					{
						value2 = 0f;
					}
					base.Priority = MathHelper.Clamp(value2, 0f, 59f);
				}
			}
			return base.Priority;
		}

		// Token: 0x06002E30 RID: 11824 RVA: 0x001EE79C File Offset: 0x001EC99C
		public AIObjectiveOperateItem(ItemComponent item, Character character, AIObjectiveManager objectiveManager, Identifier option, bool requireEquip, Entity operateTarget = null, bool useController = false, ItemComponent controller = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, option)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item", "Attempted to create an AIObjectiveOperateItem with a null target.");
			}
			this.component = item;
			this.requireEquip = requireEquip;
			this.operateTarget = operateTarget;
			this.useController = useController;
			if (useController)
			{
				ItemComponent itemComponent = controller;
				if (controller == null)
				{
					Item item2 = this.component.Item;
					itemComponent = ((item2 != null) ? item2.FindController(null) : null);
				}
				this.controller = itemComponent;
			}
			ItemComponent target = this.GetTarget();
			if (target == null)
			{
				base.Abandon = true;
				return;
			}
			if (!target.Item.IsInteractable(character))
			{
				base.Abandon = true;
			}
		}

		// Token: 0x06002E31 RID: 11825 RVA: 0x001EE858 File Offset: 0x001ECA58
		protected override void Act(float deltaTime)
		{
			if (this.character.LockHands)
			{
				base.Abandon = true;
				return;
			}
			ItemComponent target = this.GetTarget();
			if (this.useController && this.controller == null)
			{
				if (this.character.IsOnPlayerTeam)
				{
					Character character = this.character;
					string value = TextManager.GetWithVariable("DialogCantFindController", "[item]", this.component.Item.Name, FormatCapitals.No).Value;
					Identifier identifier = "cantfindcontroller".ToIdentifier();
					character.Speak(value, null, 2f, identifier, 30f);
				}
				base.Abandon = true;
				return;
			}
			if (this.operateTarget != null && base.HumanAIController.IsTrueForAnyBotInTheCrew(delegate(HumanAIController other)
			{
				if (other != this.HumanAIController)
				{
					AIObjectiveOperateItem operateObjective = other.ObjectiveManager.GetActiveObjective() as AIObjectiveOperateItem;
					if (operateObjective != null)
					{
						return operateObjective.operateTarget == this.operateTarget;
					}
				}
				return false;
			}))
			{
				base.Abandon = true;
				return;
			}
			this.character.SelectedCharacter = null;
			if (target.CanBeSelected)
			{
				float num;
				if (this.character.IsClimbing || !this.character.CanInteractWith(target.Item, out num, false))
				{
					base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(target.Item, this.character, this.objectiveManager, false, true, 1f, 50f)
					{
						DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachTarget,
						TargetName = target.Item.Name,
						ForceWalkPermanently = this.ForceWalk,
						endNodeFilter = (this.EndNodeFilter ?? AIObjectiveGetItem.CreateEndNodeFilter(target.Item))
					}, delegate
					{
						this.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					}, delegate
					{
						this.Abandon = true;
					});
					return;
				}
				Controller controller = target.Item.GetComponent<Controller>();
				if (controller == null || !controller.ControlCharacterPose)
				{
					base.HumanAIController.FaceTarget(target.Item);
				}
				else
				{
					base.HumanAIController.SteeringManager.Reset();
				}
				if (this.character.SelectedItem != target.Item && this.character.SelectedSecondaryItem != target.Item)
				{
					target.Item.TryInteract(this.character, false, true, false);
				}
				if (this.component.CrewAIOperate(deltaTime, this.character, this))
				{
					this.isDoneOperating = (this.completionCondition == null || this.completionCondition());
					return;
				}
			}
			else
			{
				if (this.component.Item.GetComponent<Pickable>() == null)
				{
					base.Abandon = true;
					return;
				}
				if (!this.character.Inventory.Contains(this.component.Item))
				{
					base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getItemObjective, () => new AIObjectiveGetItem(this.character, this.component.Item, this.objectiveManager, true, 1f), delegate
					{
						this.RemoveSubObjective<AIObjectiveGetItem>(ref this.getItemObjective);
					}, delegate
					{
						this.Abandon = true;
					});
					return;
				}
				if (this.requireEquip && !this.character.HasEquippedItem(this.component.Item, null, null))
				{
					Holdable holdable = this.component.Item.GetComponent<Holdable>();
					if (holdable == null)
					{
						return;
					}
					int j;
					int i;
					for (i = 0; i < this.character.Inventory.Capacity; i = j + 1)
					{
						if (this.character.Inventory.SlotTypes[i] != InvSlotType.Any && holdable.AllowedSlots.Any((InvSlotType s) => s.HasFlag(this.character.Inventory.SlotTypes[i])))
						{
							Item existingItem = this.character.Inventory.GetItemAt(i);
							if (existingItem != null && (!existingItem.AllowedSlots.Contains(InvSlotType.Any) || !this.character.Inventory.TryPutItem(existingItem, this.character, new List<InvSlotType>
							{
								InvSlotType.Any
							}, true, false, true)))
							{
								existingItem.Drop(this.character, true, true);
							}
							if (this.character.Inventory.TryPutItem(this.component.Item, i, true, false, this.character, true, false, true))
							{
								this.component.Item.Equip(this.character);
								return;
							}
						}
						j = i;
					}
					return;
				}
				else if (this.component.CrewAIOperate(deltaTime, this.character, this))
				{
					this.isDoneOperating = (this.completionCondition == null || this.completionCondition());
				}
			}
		}

		// Token: 0x06002E32 RID: 11826 RVA: 0x001EEC8C File Offset: 0x001ECE8C
		protected override bool CheckObjectiveState()
		{
			return this.isDoneOperating && !this.Repeat;
		}

		// Token: 0x06002E33 RID: 11827 RVA: 0x001EECA1 File Offset: 0x001ECEA1
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.getItemObjective = null;
		}

		// Token: 0x06002E34 RID: 11828 RVA: 0x001EECB8 File Offset: 0x001ECEB8
		private bool IsItemOperatedByAnother(ItemComponent target)
		{
			AIObjectiveOperateItem.<>c__DisplayClass48_0 CS$<>8__locals1 = new AIObjectiveOperateItem.<>c__DisplayClass48_0();
			CS$<>8__locals1.target = target;
			ItemComponent target2 = CS$<>8__locals1.target;
			if (((target2 != null) ? target2.Item : null) == null)
			{
				return false;
			}
			bool isOrdered = CS$<>8__locals1.<IsItemOperatedByAnother>g__IsOrderedToOperateTarget|0(base.HumanAIController);
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsActive(c) && c != this.character && c.TeamID == this.character.TeamID)
				{
					if (c.IsPlayer)
					{
						if (c.SelectedItem == CS$<>8__locals1.target.Item)
						{
							return true;
						}
					}
					else
					{
						HumanAIController otherAI = c.AIController as HumanAIController;
						if (otherAI != null)
						{
							IEnumerable<AIObjective> objectives = otherAI.ObjectiveManager.Objectives;
							Func<AIObjective, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__2) == null)
							{
								predicate = (CS$<>8__locals1.<>9__2 = delegate(AIObjective o)
								{
									AIObjectiveOperateItem operateObjective = o as AIObjectiveOperateItem;
									return operateObjective != null && operateObjective.Component.Item == CS$<>8__locals1.target.Item;
								});
							}
							if (!objectives.None(predicate))
							{
								bool isOtherCharacterOrdered = CS$<>8__locals1.<IsItemOperatedByAnother>g__IsOrderedToOperateTarget|0(otherAI);
								if (!isOrdered)
								{
									if (isOtherCharacterOrdered)
									{
										return true;
									}
								}
								else if (!isOtherCharacterOrdered)
								{
									continue;
								}
								if (CS$<>8__locals1.<IsItemOperatedByAnother>g__IsOperatingTarget|1(otherAI))
								{
									if (CS$<>8__locals1.target is Steering)
									{
										if (this.character.GetSkillLevel(Tags.HelmSkill) <= c.GetSkillLevel(Tags.HelmSkill))
										{
											return true;
										}
									}
									else if (CS$<>8__locals1.target.DegreeOfSuccess(this.character) <= CS$<>8__locals1.target.DegreeOfSuccess(c))
									{
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x04001816 RID: 6166
		private readonly ItemComponent component;

		// Token: 0x04001817 RID: 6167
		private readonly ItemComponent controller;

		// Token: 0x04001818 RID: 6168
		private readonly Entity operateTarget;

		// Token: 0x04001819 RID: 6169
		private readonly bool requireEquip;

		// Token: 0x0400181A RID: 6170
		private readonly bool useController;

		// Token: 0x0400181B RID: 6171
		private AIObjectiveGoTo goToObjective;

		// Token: 0x0400181C RID: 6172
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x0400181D RID: 6173
		public Func<PathNode, bool> EndNodeFilter;

		// Token: 0x04001820 RID: 6176
		public Func<bool> completionCondition;

		// Token: 0x04001821 RID: 6177
		private bool isDoneOperating;

		// Token: 0x04001822 RID: 6178
		public float? OverridePriority;
	}
}
