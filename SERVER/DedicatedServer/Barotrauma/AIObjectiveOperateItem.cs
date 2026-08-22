using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000080 RID: 128
	internal class AIObjectiveOperateItem : AIObjective
	{
		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06001116 RID: 4374 RVA: 0x0009B3F3 File Offset: 0x000995F3
		// (set) Token: 0x06001117 RID: 4375 RVA: 0x0009B3FB File Offset: 0x000995FB
		public override Identifier Identifier { get; set; } = "operate item".ToIdentifier();

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06001118 RID: 4376 RVA: 0x0009B404 File Offset: 0x00099604
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

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06001119 RID: 4377 RVA: 0x0009B458 File Offset: 0x00099658
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x0600111A RID: 4378 RVA: 0x0009B45B File Offset: 0x0009965B
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x0600111B RID: 4379 RVA: 0x0009B45E File Offset: 0x0009965E
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x0600111C RID: 4380 RVA: 0x0009B461 File Offset: 0x00099661
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x0600111D RID: 4381 RVA: 0x0009B464 File Offset: 0x00099664
		public override bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				ItemComponent itemComponent = this.component;
				return itemComponent is Reactor || itemComponent is Turret;
			}
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x0600111E RID: 4382 RVA: 0x0009B48F File Offset: 0x0009968F
		// (set) Token: 0x0600111F RID: 4383 RVA: 0x0009B497 File Offset: 0x00099697
		public bool Override { get; set; } = true;

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06001120 RID: 4384 RVA: 0x0009B4A0 File Offset: 0x000996A0
		// (set) Token: 0x06001121 RID: 4385 RVA: 0x0009B4A8 File Offset: 0x000996A8
		public bool Repeat { get; set; }

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06001122 RID: 4386 RVA: 0x0009B4B1 File Offset: 0x000996B1
		public override bool CanBeCompleted
		{
			get
			{
				return base.CanBeCompleted && (!this.useController || this.controller != null);
			}
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x0009B4D0 File Offset: 0x000996D0
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

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06001124 RID: 4388 RVA: 0x0009B505 File Offset: 0x00099705
		public Entity OperateTarget
		{
			get
			{
				return this.operateTarget;
			}
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06001125 RID: 4389 RVA: 0x0009B50D File Offset: 0x0009970D
		public ItemComponent Component
		{
			get
			{
				return this.component;
			}
		}

		// Token: 0x06001126 RID: 4390 RVA: 0x0009B515 File Offset: 0x00099715
		public ItemComponent GetTarget()
		{
			if (!this.useController)
			{
				return this.component;
			}
			return this.controller;
		}

		// Token: 0x06001127 RID: 4391 RVA: 0x0009B52C File Offset: 0x0009972C
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

		// Token: 0x06001128 RID: 4392 RVA: 0x0009B904 File Offset: 0x00099B04
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

		// Token: 0x06001129 RID: 4393 RVA: 0x0009B9C0 File Offset: 0x00099BC0
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

		// Token: 0x0600112A RID: 4394 RVA: 0x0009BDF4 File Offset: 0x00099FF4
		protected override bool CheckObjectiveState()
		{
			return this.isDoneOperating && !this.Repeat;
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x0009BE09 File Offset: 0x0009A009
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.getItemObjective = null;
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x0009BE20 File Offset: 0x0009A020
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

		// Token: 0x0400081C RID: 2076
		private readonly ItemComponent component;

		// Token: 0x0400081D RID: 2077
		private readonly ItemComponent controller;

		// Token: 0x0400081E RID: 2078
		private readonly Entity operateTarget;

		// Token: 0x0400081F RID: 2079
		private readonly bool requireEquip;

		// Token: 0x04000820 RID: 2080
		private readonly bool useController;

		// Token: 0x04000821 RID: 2081
		private AIObjectiveGoTo goToObjective;

		// Token: 0x04000822 RID: 2082
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x04000823 RID: 2083
		public Func<PathNode, bool> EndNodeFilter;

		// Token: 0x04000826 RID: 2086
		public Func<bool> completionCondition;

		// Token: 0x04000827 RID: 2087
		private bool isDoneOperating;

		// Token: 0x04000828 RID: 2088
		public float? OverridePriority;
	}
}
