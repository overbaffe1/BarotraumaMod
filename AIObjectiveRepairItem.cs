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
	// Token: 0x02000189 RID: 393
	internal class AIObjectiveRepairItem : AIObjective
	{
		// Token: 0x17000BE1 RID: 3041
		// (get) Token: 0x06002E64 RID: 11876 RVA: 0x001EF677 File Offset: 0x001ED877
		// (set) Token: 0x06002E65 RID: 11877 RVA: 0x001EF67F File Offset: 0x001ED87F
		public override Identifier Identifier { get; set; } = "repair item".ToIdentifier();

		// Token: 0x17000BE2 RID: 3042
		// (get) Token: 0x06002E66 RID: 11878 RVA: 0x001EF688 File Offset: 0x001ED888
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

		// Token: 0x17000BE3 RID: 3043
		// (get) Token: 0x06002E67 RID: 11879 RVA: 0x001EF6EC File Offset: 0x001ED8EC
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BE4 RID: 3044
		// (get) Token: 0x06002E68 RID: 11880 RVA: 0x001EF6EF File Offset: 0x001ED8EF
		public override bool KeepDivingGearOn
		{
			get
			{
				Item item = this.Item;
				return ((item != null) ? item.CurrentHull : null) == null;
			}
		}

		// Token: 0x17000BE5 RID: 3045
		// (get) Token: 0x06002E69 RID: 11881 RVA: 0x001EF706 File Offset: 0x001ED906
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BE6 RID: 3046
		// (get) Token: 0x06002E6A RID: 11882 RVA: 0x001EF709 File Offset: 0x001ED909
		// (set) Token: 0x06002E6B RID: 11883 RVA: 0x001EF711 File Offset: 0x001ED911
		public Item Item { get; private set; }

		// Token: 0x06002E6C RID: 11884 RVA: 0x001EF71A File Offset: 0x001ED91A
		private bool IsRepairing()
		{
			return AIObjectiveRepairItem.IsRepairing(this.character, this.Item);
		}

		// Token: 0x06002E6D RID: 11885 RVA: 0x001EF730 File Offset: 0x001ED930
		public static bool IsRepairing(Character character, Item item)
		{
			return character.SelectedItem == item && item.Repairables.Any((Repairable r) => r.CurrentFixer == character);
		}

		// Token: 0x06002E6E RID: 11886 RVA: 0x001EF774 File Offset: 0x001ED974
		public AIObjectiveRepairItem(Character character, Item item, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool isPriority = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Item = item;
			this.isPriority = isPriority;
		}

		// Token: 0x06002E6F RID: 11887 RVA: 0x001EF7B4 File Offset: 0x001ED9B4
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

		// Token: 0x06002E70 RID: 11888 RVA: 0x001EF9F4 File Offset: 0x001EDBF4
		protected override bool CheckObjectiveState()
		{
			base.IsCompleted = this.Item.IsFullCondition;
			if (this.character.IsOnPlayerTeam && base.IsCompleted && this.IsRepairing())
			{
				this.character.Speak(TextManager.GetWithVariable("DialogItemRepaired", "[itemname]", this.Item.Name, FormatCapitals.Yes).Value, null, 0f, "itemrepaired".ToIdentifier(), 10f);
			}
			return base.IsCompleted;
		}

		// Token: 0x06002E71 RID: 11889 RVA: 0x001EFA84 File Offset: 0x001EDC84
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

		// Token: 0x06002E72 RID: 11890 RVA: 0x001EFFF0 File Offset: 0x001EE1F0
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

		// Token: 0x06002E73 RID: 11891 RVA: 0x001F0074 File Offset: 0x001EE274
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

		// Token: 0x06002E74 RID: 11892 RVA: 0x001F01A8 File Offset: 0x001EE3A8
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

		// Token: 0x06002E75 RID: 11893 RVA: 0x001F02F9 File Offset: 0x001EE4F9
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.refuelObjective = null;
			this.repairTool = null;
		}

		// Token: 0x04001835 RID: 6197
		private AIObjectiveGoTo goToObjective;

		// Token: 0x04001836 RID: 6198
		private AIObjectiveContainItem refuelObjective;

		// Token: 0x04001837 RID: 6199
		private RepairTool repairTool;

		// Token: 0x04001838 RID: 6200
		private const float WaitTimeBeforeRepair = 0.5f;

		// Token: 0x04001839 RID: 6201
		private float waitTimer;

		// Token: 0x0400183A RID: 6202
		private readonly bool isPriority;

		// Token: 0x0400183B RID: 6203
		private const float conditionCheckDelay = 1f;

		// Token: 0x0400183C RID: 6204
		private float conditionCheckTimer;

		// Token: 0x0400183D RID: 6205
		private float previousCondition;
	}
}
