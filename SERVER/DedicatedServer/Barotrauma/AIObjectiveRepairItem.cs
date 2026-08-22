using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000083 RID: 131
	internal class AIObjectiveRepairItem : AIObjective
	{
		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x0600115C RID: 4444 RVA: 0x0009C7DF File Offset: 0x0009A9DF
		// (set) Token: 0x0600115D RID: 4445 RVA: 0x0009C7E7 File Offset: 0x0009A9E7
		public override Identifier Identifier { get; set; } = "repair item".ToIdentifier();

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x0600115E RID: 4446 RVA: 0x0009C7F0 File Offset: 0x0009A9F0
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				Item item = this.Item;
				defaultInterpolatedStringHandler.AppendFormatted(((item != null) ? item.Name : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x0600115F RID: 4447 RVA: 0x0009C854 File Offset: 0x0009AA54
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x06001160 RID: 4448 RVA: 0x0009C857 File Offset: 0x0009AA57
		public override bool KeepDivingGearOn
		{
			get
			{
				Item item = this.Item;
				return ((item != null) ? item.CurrentHull : null) == null;
			}
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x06001161 RID: 4449 RVA: 0x0009C86E File Offset: 0x0009AA6E
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x06001162 RID: 4450 RVA: 0x0009C871 File Offset: 0x0009AA71
		// (set) Token: 0x06001163 RID: 4451 RVA: 0x0009C879 File Offset: 0x0009AA79
		public Item Item { get; private set; }

		// Token: 0x06001164 RID: 4452 RVA: 0x0009C882 File Offset: 0x0009AA82
		private bool IsRepairing()
		{
			return AIObjectiveRepairItem.IsRepairing(this.character, this.Item);
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x0009C898 File Offset: 0x0009AA98
		public static bool IsRepairing(Character character, Item item)
		{
			return character.SelectedItem == item && item.Repairables.Any((Repairable r) => r.CurrentFixer == character);
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x0009C8DC File Offset: 0x0009AADC
		public AIObjectiveRepairItem(Character character, Item item, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool isPriority = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Item = item;
			this.isPriority = isPriority;
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x0009C91C File Offset: 0x0009AB1C
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
			}
			if (this.Item.IgnoreByAI(this.character))
			{
				base.Abandon = true;
			}
			if (base.Abandon)
			{
				if (this.IsRepairing())
				{
					this.Item.Repairables.ForEach(delegate(Repairable r)
					{
						r.StopRepairing(this.character);
					});
				}
				return base.Priority;
			}
			if (AIObjectiveRepairItems.IsItemRepairedByAnother(this.character, this.Item))
			{
				base.Priority = 0f;
				base.IsCompleted = true;
			}
			else if (this.Item.IsClaimedByBallastFlora)
			{
				base.Priority = 0f;
			}
			else
			{
				float distanceFactor = 1f;
				if (!this.isPriority && this.Item.CurrentHull != this.character.CurrentHull)
				{
					distanceFactor = base.GetDistanceFactor(this.Item.WorldPosition, 0.25f, 5f, 4000f, 1f);
				}
				float requiredSuccessFactor = this.objectiveManager.HasOrder<AIObjectiveRepairItems>(null) ? 0f : 0.4f;
				float severity = this.isPriority ? 1f : (AIObjectiveRepairItems.GetTargetPriority(this.Item, this.character, requiredSuccessFactor) / 100f);
				bool isSelected = this.IsRepairing();
				float selectedBonus = isSelected ? (100f - this.MaxDevotion) : 0f;
				float devotion = (base.CumulatedDevotion + selectedBonus) / 100f;
				float reduction = (float)(this.isPriority ? 1 : (isSelected ? 2 : 3));
				float max = 60f - reduction;
				float highestWeight = -1f;
				foreach (Identifier tag in this.Item.Prefab.Tags)
				{
					float weight;
					if (JobPrefab.ItemRepairPriorities.TryGetValue(tag, out weight) && weight > highestWeight)
					{
						highestWeight = weight;
					}
				}
				if (highestWeight == -1f)
				{
					highestWeight = 1f;
				}
				base.Priority = MathHelper.Lerp(0f, max, MathHelper.Clamp(devotion + severity * distanceFactor * highestWeight * base.PriorityModifier, 0f, 1f));
			}
			return base.Priority;
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x0009CB5C File Offset: 0x0009AD5C
		protected override bool CheckObjectiveState()
		{
			base.IsCompleted = this.Item.IsFullCondition;
			if (this.character.IsOnPlayerTeam && base.IsCompleted && this.IsRepairing())
			{
				this.character.Speak(TextManager.GetWithVariable("DialogItemRepaired", "[itemname]", this.Item.Name, FormatCapitals.Yes).Value, null, 0f, "itemrepaired".ToIdentifier(), 10f);
			}
			return base.IsCompleted;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x0009CBEC File Offset: 0x0009ADEC
		protected override void Act(float deltaTime)
		{
			if (this.subObjectives.Any<AIObjective>())
			{
				return;
			}
			foreach (Repairable repairable in this.Item.Repairables)
			{
				if (!repairable.HasRequiredItems(this.character, false, null))
				{
					using (Dictionary<RelatedItem.RelationType, List<RelatedItem>>.Enumerator enumerator2 = repairable.RequiredItems.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp = enumerator2.Current;
							foreach (RelatedItem requiredItem2 in kvp.Value)
							{
								AIObjectiveGetItem getItemObjective = new AIObjectiveGetItem(this.character, requiredItem2.Identifiers, this.objectiveManager, true, true, 1f, false)
								{
									AllowVariants = requiredItem2.AllowVariants
								};
								if (this.objectiveManager.IsCurrentOrder<AIObjectiveRepairItems>() && this.character.IsOnPlayerTeam)
								{
									getItemObjective.Abandoned += delegate()
									{
										this.character.Speak(TextManager.Get("dialogcannotfindrequireditemtorepair").Value, null, 0f, "dialogcannotfindrequireditemtorepair".ToIdentifier(), 10f);
									};
								}
								this.subObjectives.Add(getItemObjective);
							}
						}
						return;
					}
				}
			}
			if (this.repairTool == null)
			{
				this.FindRepairTool();
			}
			List<RelatedItem> requiredItems;
			if (this.repairTool != null && this.repairTool.RequiredItems.TryGetValue(RelatedItem.RelationType.Contained, out requiredItems))
			{
				if (this.repairTool.Item.OwnInventory == null)
				{
					base.Abandon = true;
					return;
				}
				RelatedItem item = null;
				Item fuel = null;
				using (List<RelatedItem>.Enumerator enumerator4 = requiredItems.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						RelatedItem requiredItem = enumerator4.Current;
						item = requiredItem;
						fuel = this.repairTool.Item.OwnInventory.AllItems.FirstOrDefault((Item it) => it.Condition > 0f && requiredItem.MatchesItem(it));
						if (fuel != null)
						{
							break;
						}
					}
				}
				if (fuel == null)
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					base.TryAddSubObjective<AIObjectiveContainItem>(ref this.refuelObjective, () => new AIObjectiveContainItem(this.character, item.Identifiers, this.repairTool.Item.GetComponent<ItemContainer>(), this.objectiveManager, 1f, this.character.TeamID == CharacterTeamType.FriendlyNPC)
					{
						RemoveExisting = true
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveContainItem>(ref this.refuelObjective);
					}, delegate
					{
						base.Abandon = true;
					});
					return;
				}
			}
			float num;
			if (this.character.CanInteractWith(this.Item, out num, false))
			{
				this.waitTimer += deltaTime;
				if (this.character.IsClimbing && this.Item.WorldPosition.Y > this.character.WorldPosition.Y + ConvertUnits.ToDisplayUnits(this.character.AnimController.ArmLength))
				{
					this.character.AIController.SteeringManager.SteeringManual(deltaTime, Vector2.UnitY);
				}
				if (this.waitTimer < 0.5f)
				{
					return;
				}
				base.HumanAIController.FaceTarget(this.Item);
				bool repairThroughRepairInterface = false;
				using (List<Repairable>.Enumerator enumerator5 = this.Item.Repairables.GetEnumerator())
				{
					if (enumerator5.MoveNext())
					{
						Repairable repairable2 = enumerator5.Current;
						if (repairable2.CurrentFixer != null && repairable2.CurrentFixer != this.character)
						{
							base.Abandon = (repairable2.CurrentFixer.IsPlayer || repairable2.DegreeOfSuccess(this.character) < repairable2.DegreeOfSuccess(repairable2.CurrentFixer));
						}
						if (!base.Abandon)
						{
							if (this.character.SelectedItem != this.Item)
							{
								if (this.Item.TryInteract(this.character, false, false, true) || this.Item.TryInteract(this.character, false, true, false))
								{
									this.character.SelectedItem = this.Item;
									repairThroughRepairInterface = true;
								}
								else
								{
									base.Abandon = true;
								}
							}
							this.CheckPreviousCondition(deltaTime);
						}
						if (base.Abandon)
						{
							if (this.character.IsOnPlayerTeam && this.IsRepairing())
							{
								this.character.Speak(TextManager.GetWithVariable("DialogCannotRepair", "[itemname]", this.Item.Name, FormatCapitals.Yes).Value, null, 0f, "cannotrepair".ToIdentifier(), 10f);
							}
							repairable2.StopRepairing(this.character);
						}
						else if (repairable2.CurrentFixer != this.character)
						{
							repairable2.StartRepairing(this.character, Repairable.FixActions.Repair);
						}
						else
						{
							repairThroughRepairInterface = true;
						}
					}
				}
				if (!repairThroughRepairInterface && this.repairTool != null && !base.Abandon)
				{
					this.OperateRepairTool(deltaTime);
					return;
				}
			}
			else
			{
				this.waitTimer = 0f;
				base.RemoveSubObjective<AIObjectiveContainItem>(ref this.refuelObjective);
				base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, delegate
				{
					AIObjectiveGoTo objective = new AIObjectiveGoTo(this.Item, this.character, this.objectiveManager, false, true, 1f, 0f)
					{
						DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachTarget,
						TargetName = this.Item.Name,
						SpeakCannotReachCondition = (() => this.isPriority)
					};
					if (this.repairTool != null)
					{
						objective.CloseEnough = AIObjectiveFixLeak.CalculateReach(this.repairTool, this.character);
					}
					return objective;
				}, null, delegate
				{
					base.Abandon = true;
					if (this.character.IsOnPlayerTeam && this.IsRepairing())
					{
						this.character.Speak(TextManager.GetWithVariable("DialogCannotRepair", "[itemname]", this.Item.Name, FormatCapitals.Yes).Value, null, 0f, "cannotrepair".ToIdentifier(), 10f);
					}
				});
			}
		}

		// Token: 0x0600116A RID: 4458 RVA: 0x0009D158 File Offset: 0x0009B358
		private void CheckPreviousCondition(float deltaTime)
		{
			if (this.Item == null || this.Item.Removed)
			{
				return;
			}
			this.conditionCheckTimer -= deltaTime;
			if (this.conditionCheckTimer > 0f)
			{
				return;
			}
			this.conditionCheckTimer = 1f;
			if (this.previousCondition > -1f && this.Item.Condition < this.previousCondition)
			{
				base.Abandon = true;
				return;
			}
			this.previousCondition = this.Item.Condition;
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x0009D1DC File Offset: 0x0009B3DC
		private void FindRepairTool()
		{
			foreach (Repairable repairable in this.Item.Repairables)
			{
				foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in repairable.RequiredItems)
				{
					foreach (RelatedItem requiredItem in kvp.Value)
					{
						foreach (Item item in this.character.Inventory.AllItems)
						{
							if (requiredItem.MatchesItem(item))
							{
								this.repairTool = item.GetComponent<RepairTool>();
							}
						}
					}
				}
			}
		}

		// Token: 0x0600116C RID: 4460 RVA: 0x0009D310 File Offset: 0x0009B510
		private void OperateRepairTool(float deltaTime)
		{
			this.character.CursorPosition = this.Item.WorldPosition;
			if (this.character.Submarine != null)
			{
				this.character.CursorPosition -= this.character.Submarine.Position;
			}
			if (this.repairTool.Item.RequireAimToUse)
			{
				this.character.SetInput(InputType.Aim, false, true);
			}
			Vector2 fromToolToTarget = this.Item.Position - this.repairTool.Item.Position;
			if (fromToolToTarget.LengthSquared() < MathUtils.Pow(this.repairTool.Range / 2f, 2f))
			{
				this.character.AIController.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.character.SimPosition - this.Item.SimPosition) / 2f);
			}
			else
			{
				this.character.AIController.SteeringManager.Reset();
			}
			if (VectorExtensions.Forward(this.repairTool.Item.body.TransformedRotation, 1f).Angle(fromToolToTarget) < 0.7853982f)
			{
				this.repairTool.Use(deltaTime, this.character);
			}
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x0009D461 File Offset: 0x0009B661
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.refuelObjective = null;
			this.repairTool = null;
		}

		// Token: 0x0400083B RID: 2107
		private AIObjectiveGoTo goToObjective;

		// Token: 0x0400083C RID: 2108
		private AIObjectiveContainItem refuelObjective;

		// Token: 0x0400083D RID: 2109
		private RepairTool repairTool;

		// Token: 0x0400083E RID: 2110
		private const float WaitTimeBeforeRepair = 0.5f;

		// Token: 0x0400083F RID: 2111
		private float waitTimer;

		// Token: 0x04000840 RID: 2112
		private readonly bool isPriority;

		// Token: 0x04000841 RID: 2113
		private const float conditionCheckDelay = 1f;

		// Token: 0x04000842 RID: 2114
		private float conditionCheckTimer;

		// Token: 0x04000843 RID: 2115
		private float previousCondition;
	}
}
