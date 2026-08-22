using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200017F RID: 383
	internal class AIObjectiveIdle : AIObjective
	{
		// Token: 0x17000B8A RID: 2954
		// (get) Token: 0x06002D5A RID: 11610 RVA: 0x001E99E0 File Offset: 0x001E7BE0
		// (set) Token: 0x06002D5B RID: 11611 RVA: 0x001E99E8 File Offset: 0x001E7BE8
		public override Identifier Identifier { get; set; } = "idle".ToIdentifier();

		// Token: 0x17000B8B RID: 2955
		// (get) Token: 0x06002D5C RID: 11612 RVA: 0x001E99F1 File Offset: 0x001E7BF1
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B8C RID: 2956
		// (get) Token: 0x06002D5D RID: 11613 RVA: 0x001E99F4 File Offset: 0x001E7BF4
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B8D RID: 2957
		// (get) Token: 0x06002D5E RID: 11614 RVA: 0x001E99F7 File Offset: 0x001E7BF7
		// (set) Token: 0x06002D5F RID: 11615 RVA: 0x001E9A00 File Offset: 0x001E7C00
		public AIObjectiveIdle.BehaviorType Behavior
		{
			get
			{
				return this.behavior;
			}
			set
			{
				this.behavior = value;
				switch (this.behavior)
				{
				case AIObjectiveIdle.BehaviorType.Patrol:
					this.newTargetIntervalMin = 15f;
					this.newTargetIntervalMax = 30f;
					this.standStillMin = 5f;
					this.standStillMax = 10f;
					return;
				case AIObjectiveIdle.BehaviorType.Passive:
				case AIObjectiveIdle.BehaviorType.StayInHull:
					this.newTargetIntervalMin = 60f;
					this.newTargetIntervalMax = 120f;
					this.standStillMin = 30f;
					this.standStillMax = 60f;
					return;
				case AIObjectiveIdle.BehaviorType.Active:
					this.newTargetIntervalMin = 40f;
					this.newTargetIntervalMax = 60f;
					this.standStillMin = 20f;
					this.standStillMax = 40f;
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x17000B8E RID: 2958
		// (get) Token: 0x06002D60 RID: 11616 RVA: 0x001E9AB8 File Offset: 0x001E7CB8
		// (set) Token: 0x06002D61 RID: 11617 RVA: 0x001E9AC0 File Offset: 0x001E7CC0
		public Hull TargetHull { get; set; }

		// Token: 0x06002D62 RID: 11618 RVA: 0x001E9ACC File Offset: 0x001E7CCC
		public AIObjectiveIdle(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Behavior = AIObjectiveIdle.BehaviorType.Passive;
			this.standStillTimer = Rand.Range(-10f, 10f, Rand.RandSync.Unsynced);
			this.walkDuration = Rand.Range(0f, 10f, Rand.RandSync.Unsynced);
			this.chairCheckTimer = Rand.Range(0f, 5f, Rand.RandSync.Unsynced);
			this.CalculatePriority(0f);
		}

		// Token: 0x06002D63 RID: 11619 RVA: 0x001E9BB6 File Offset: 0x001E7DB6
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x17000B8F RID: 2959
		// (get) Token: 0x06002D64 RID: 11620 RVA: 0x001E9BB9 File Offset: 0x001E7DB9
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002D65 RID: 11621 RVA: 0x001E9BBC File Offset: 0x001E7DBC
		public void CalculatePriority(float max = 0f)
		{
			base.Priority = 1f;
		}

		// Token: 0x06002D66 RID: 11622 RVA: 0x001E9BC9 File Offset: 0x001E7DC9
		protected override float GetPriority()
		{
			return base.Priority;
		}

		// Token: 0x06002D67 RID: 11623 RVA: 0x001E9BD1 File Offset: 0x001E7DD1
		public override void Update(float deltaTime)
		{
		}

		// Token: 0x06002D68 RID: 11624 RVA: 0x001E9BD4 File Offset: 0x001E7DD4
		private void SetTargetTimerLow()
		{
			this.timerMargin += 0.5f;
			this.timerMargin = Math.Min(this.timerMargin, this.newTargetIntervalMin);
			this.newTargetTimer = Math.Max(this.newTargetTimer, this.timerMargin);
		}

		// Token: 0x06002D69 RID: 11625 RVA: 0x001E9C21 File Offset: 0x001E7E21
		private void SetTargetTimerHigh()
		{
			this.newTargetTimer = Math.Max(this.newTargetTimer, this.newTargetIntervalMin);
			this.timerMargin = 0f;
		}

		// Token: 0x06002D6A RID: 11626 RVA: 0x001E9C48 File Offset: 0x001E7E48
		private void SetTargetTimerNormal()
		{
			this.newTargetTimer = ((this.currentTarget != null && this.character.AnimController.InWater) ? this.newTargetIntervalMin : Rand.Range(this.newTargetIntervalMin, this.newTargetIntervalMax, Rand.RandSync.Unsynced));
			this.timerMargin = 0f;
		}

		// Token: 0x06002D6B RID: 11627 RVA: 0x001E9C9A File Offset: 0x001E7E9A
		private bool IsSteeringFinished()
		{
			return base.PathSteering.CurrentPath != null && (base.PathSteering.CurrentPath.Finished || base.PathSteering.CurrentPath.Unreachable);
		}

		// Token: 0x06002D6C RID: 11628 RVA: 0x001E9CD0 File Offset: 0x001E7ED0
		protected override void Act(float deltaTime)
		{
			AIObjectiveIdle.<>c__DisplayClass47_0 CS$<>8__locals1 = new AIObjectiveIdle.<>c__DisplayClass47_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (base.PathSteering == null)
			{
				return;
			}
			if (this.objectiveManager.FailedAutonomousObjectives)
			{
				if (this.autonomousObjectiveRetryTimer > 0f)
				{
					this.autonomousObjectiveRetryTimer -= CS$<>8__locals1.deltaTime;
				}
				else
				{
					this.objectiveManager.CreateAutonomousObjectives();
				}
			}
			if (this.character.SelectedCharacter != null)
			{
				this.character.DeselectCharacter();
			}
			if (this.character.SelectedItem != null)
			{
				if (!this.character.SelectedItem.Prefab.AllowDeselectWhenIdling)
				{
					return;
				}
				this.character.SelectedItem = null;
			}
			if (!this.character.IsClimbing)
			{
				this.CleanupItems(CS$<>8__locals1.deltaTime);
			}
			if (this.behavior == AIObjectiveIdle.BehaviorType.StayInHull && this.TargetHull == null && this.character.CurrentHull != null && !AIObjectiveIdle.IsForbidden(this.character.CurrentHull))
			{
				this.TargetHull = this.character.CurrentHull;
			}
			bool currentTargetIsInvalid = this.currentTarget == null || AIObjectiveIdle.IsForbidden(this.currentTarget) || (base.PathSteering.CurrentPath != null && base.PathSteering.CurrentPath.Nodes.Any((WayPoint n) => CS$<>8__locals1.<>4__this.HumanAIController.UnsafeHulls.Contains(n.CurrentHull)));
			if (this.behavior == AIObjectiveIdle.BehaviorType.StayInHull && this.TargetHull != null && !currentTargetIsInvalid && !AIObjectiveIdle.IsForbidden(this.TargetHull))
			{
				if (!base.HumanAIController.UnsafeHulls.Contains(this.TargetHull))
				{
					this.currentTarget = this.TargetHull;
					CS$<>8__locals1.<Act>g__NavigateTo|1(this.currentTarget);
					return;
				}
				base.HumanAIController.AskToRecalculateHullSafety(this.TargetHull);
			}
			if (this.currentTarget != null && !currentTargetIsInvalid)
			{
				if (this.character.TeamID == CharacterTeamType.FriendlyNPC && !this.character.IsEscorted)
				{
					if (this.currentTarget.Submarine.TeamID != this.character.TeamID)
					{
						currentTargetIsInvalid = true;
					}
				}
				else if (this.currentTarget.Submarine != this.character.Submarine)
				{
					currentTargetIsInvalid = true;
				}
			}
			if (currentTargetIsInvalid || this.currentTarget == null || (AIObjectiveIdle.IsForbidden(this.character.CurrentHull) && this.IsSteeringFinished()))
			{
				if (this.newTargetTimer > this.timerMargin)
				{
					this.SetTargetTimerLow();
				}
			}
			else if (this.character.IsClimbing)
			{
				if (this.currentTarget == null)
				{
					this.SetTargetTimerLow();
				}
				else if (Math.Abs(this.character.AnimController.TargetMovement.Y) > 0.9f)
				{
					this.SetTargetTimerHigh();
				}
			}
			else if (this.character.AnimController.InWater && this.currentTarget == null)
			{
				this.SetTargetTimerLow();
			}
			if (this.newTargetTimer <= 0f)
			{
				if (!this.searchingNewHull)
				{
					this.searchingNewHull = true;
					this.FindTargetHulls();
				}
				else if (this.targetHulls.Any<Hull>())
				{
					this.currentTarget = ToolBox.SelectWeightedRandom<Hull>(this.targetHulls, this.hullWeights, Rand.RandSync.Unsynced);
					bool isInWrongSub = this.character.TeamID == CharacterTeamType.FriendlyNPC && !this.character.IsEscorted && this.character.Submarine.TeamID != this.character.TeamID;
					bool isCurrentHullAllowed = !isInWrongSub && !AIObjectiveIdle.IsForbidden(this.character.CurrentHull);
					Vector2 targetPos = this.character.GetRelativeSimPosition(this.currentTarget, null);
					SteeringPath path = base.PathSteering.PathFinder.FindPath(this.character.SimPosition, targetPos, this.character.Submarine, null, 0f, null, (PathNode node) => node.Waypoint.Stairs == null && node.Waypoint.CurrentHull == CS$<>8__locals1.<>4__this.currentTarget && node.Waypoint.Ladders == null && (!isCurrentHullAllowed || !AIObjectiveIdle.IsForbidden(node.Waypoint.CurrentHull)), (PathNode node) => node.Waypoint.CurrentHull != null && (node.Waypoint.CurrentHull == CS$<>8__locals1.<>4__this.character.CurrentHull || !CS$<>8__locals1.<>4__this.HumanAIController.UnsafeHulls.Contains(node.Waypoint.CurrentHull)), true, 0f);
					if (path.Unreachable)
					{
						int index = this.targetHulls.IndexOf(this.currentTarget);
						this.targetHulls.RemoveAt(index);
						this.hullWeights.RemoveAt(index);
						base.PathSteering.Reset();
						this.currentTarget = null;
						this.SetTargetTimerLow();
						return;
					}
					this.character.AIController.SelectTarget(this.currentTarget.AiTarget);
					base.PathSteering.SetPath(targetPos, path);
					this.SetTargetTimerNormal();
					this.searchingNewHull = false;
				}
				else
				{
					this.SetTargetTimerHigh();
					this.searchingNewHull = false;
				}
			}
			this.newTargetTimer -= CS$<>8__locals1.deltaTime;
			if (this.currentTarget == null || base.PathSteering.CurrentPath == null)
			{
				this.Wander(CS$<>8__locals1.deltaTime);
				return;
			}
			CS$<>8__locals1.<Act>g__NavigateTo|1(this.currentTarget);
		}

		// Token: 0x06002D6D RID: 11629 RVA: 0x001EA190 File Offset: 0x001E8390
		public void Wander(float deltaTime)
		{
			if (this.character.IsClimbing)
			{
				base.PathSteering.Reset();
				this.character.StopClimbing();
			}
			Hull currentHull = this.character.CurrentHull;
			if (!this.character.AnimController.InWater && currentHull != null)
			{
				this.standStillTimer -= deltaTime;
				if (this.standStillTimer > 0f)
				{
					this.walkDuration = Rand.Range(this.walkDurationMin, this.walkDurationMax, Rand.RandSync.Unsynced);
					if ((float)currentHull.Rect.Width > IndoorsSteeringManager.smallRoomSize / 2f && this.tooCloseCharacter == null)
					{
						foreach (Character c2 in Character.CharacterList)
						{
							if (c2 != this.character && c2.IsBot && c2.CurrentHull == currentHull)
							{
								HumanAIController humanAI = c2.AIController as HumanAIController;
								if (humanAI != null && Vector2.DistanceSquared(c2.WorldPosition, this.character.WorldPosition) <= 3600f)
								{
									AIObjectiveIdle idleObjective = humanAI.ObjectiveManager.CurrentObjective as AIObjectiveIdle;
									if (idleObjective != null && idleObjective.standStillTimer > 0f)
									{
										goto IL_171;
									}
									AIObjectiveGoTo gotoObjective = humanAI.ObjectiveManager.CurrentObjective as AIObjectiveGoTo;
									if (gotoObjective != null && gotoObjective.IsCloseEnough)
									{
										goto IL_171;
									}
									IL_1D2:
									base.HumanAIController.FaceTarget(c2);
									continue;
									IL_171:
									if (this.tooCloseCharacter != null && Math.Sign(this.tooCloseCharacter.WorldPosition.X - this.character.WorldPosition.X) != Math.Sign(c2.WorldPosition.X - this.character.WorldPosition.X))
									{
										this.tooCloseCharacter = null;
										break;
									}
									this.tooCloseCharacter = c2;
									goto IL_1D2;
								}
							}
						}
					}
					if (this.tooCloseCharacter != null && !this.tooCloseCharacter.Removed && Vector2.DistanceSquared(this.tooCloseCharacter.WorldPosition, this.character.WorldPosition) < 2500f)
					{
						Vector2 diff = this.character.WorldPosition - this.tooCloseCharacter.WorldPosition;
						if (diff.LengthSquared() < 0.0001f)
						{
							diff = Rand.Vector(1f, Rand.RandSync.Unsynced);
						}
						if (Math.Abs(diff.X) > 0f && (this.character.WorldPosition.X > (float)(currentHull.WorldRect.Right - 50) || this.character.WorldPosition.X < (float)(currentHull.WorldRect.Left + 50)))
						{
							this.tooCloseCharacter = null;
							base.PathSteering.Reset();
							this.standStillTimer = 0f;
							this.walkDuration = Math.Min(this.walkDuration, this.walkDurationMin);
							if (this.Behavior != AIObjectiveIdle.BehaviorType.StayInHull && (currentHull.Size.X < IndoorsSteeringManager.smallRoomSize || currentHull.Size.X < IndoorsSteeringManager.smallRoomSize / 2f * (float)Character.CharacterList.Count((Character c) => c.CurrentHull == currentHull)))
							{
								this.newTargetTimer = Math.Min(this.newTargetTimer, 1f);
								return;
							}
						}
						else
						{
							this.character.ReleaseSecondaryItem();
							base.PathSteering.SteeringManual(deltaTime, Vector2.Normalize(diff));
						}
						return;
					}
					base.PathSteering.Reset();
					this.tooCloseCharacter = null;
					this.chairCheckTimer -= deltaTime;
					if (this.chairCheckTimer <= 0f && this.character.SelectedSecondaryItem == null)
					{
						foreach (Item chair in Item.ChairItems)
						{
							if (chair.CurrentHull == currentHull && chair.ParentInventory == null)
							{
								PhysicsBody body = chair.body;
								if (body == null || !body.Enabled)
								{
									Controller controller = chair.GetComponent<Controller>();
									if (controller != null && controller.User == null)
									{
										chair.TryInteract(this.character, false, true, false);
									}
								}
							}
						}
						this.chairCheckTimer = 5f;
					}
					return;
				}
				else if (this.standStillTimer < -this.walkDuration)
				{
					this.standStillTimer = Rand.Range(this.standStillMin, this.standStillMax, Rand.RandSync.Unsynced);
				}
			}
			base.PathSteering.Wander(deltaTime, 150f, true);
		}

		// Token: 0x06002D6E RID: 11630 RVA: 0x001EA67C File Offset: 0x001E887C
		public void FaceTargetAndWait(ISpatialEntity target, float waitTime)
		{
			this.standStillTimer = waitTime;
			base.HumanAIController.FaceTarget(target);
			this.currentTarget = null;
			this.SetTargetTimerHigh();
		}

		// Token: 0x06002D6F RID: 11631 RVA: 0x001EA6A0 File Offset: 0x001E88A0
		private void FindTargetHulls()
		{
			this.targetHulls.Clear();
			this.hullWeights.Clear();
			foreach (Hull hull in Hull.HullList)
			{
				if (this.character.Submarine == null)
				{
					break;
				}
				if (!base.HumanAIController.UnsafeHulls.Contains(hull) && hull.Submarine != null && !hull.Submarine.Info.IsRuin && !hull.Submarine.Info.IsWreck)
				{
					if (this.character.TeamID == CharacterTeamType.FriendlyNPC && !this.character.IsEscorted)
					{
						if (hull.Submarine.TeamID != this.character.TeamID)
						{
							continue;
						}
					}
					else if (hull.Submarine.TeamID != this.character.Submarine.TeamID)
					{
						continue;
					}
					if (!AIObjectiveIdle.IsForbidden(hull) && this.character.Submarine.IsConnectedTo(hull.Submarine) && hull.RectWidth >= 200)
					{
						HumanoidAnimController animController = this.character.AnimController as HumanoidAnimController;
						if ((animController == null || hull.CeilingHeight >= ConvertUnits.ToDisplayUnits(animController.HeadPosition.Value)) && !this.targetHulls.Contains(hull))
						{
							this.targetHulls.Add(hull);
							float weight = (float)hull.RectWidth;
							float distanceFactor = base.GetDistanceFactor(hull.WorldPosition, 0f, 5f, 2500f, 1f);
							if (this.behavior == AIObjectiveIdle.BehaviorType.Patrol)
							{
								distanceFactor = 1f - distanceFactor;
							}
							float waterFactor = MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(0f, 100f, hull.WaterPercentage * 2f));
							weight *= distanceFactor * waterFactor;
							this.hullWeights.Add(weight);
						}
					}
				}
			}
			if (this.PreferredOutpostModuleTypes.Any<Identifier>() && this.character.CurrentHull != null)
			{
				for (int i = 0; i < this.targetHulls.Count; i++)
				{
					if (this.targetHulls[i].OutpostModuleTags.Any((Identifier t) => this.PreferredOutpostModuleTypes.Contains(t)))
					{
						List<float> list = this.hullWeights;
						int index = i;
						list[index] *= Rand.Range(10f, 100f, Rand.RandSync.Unsynced);
					}
				}
			}
		}

		// Token: 0x06002D70 RID: 11632 RVA: 0x001EA958 File Offset: 0x001E8B58
		private void CleanupItems(float deltaTime)
		{
			if (this.checkItemsTimer <= 0f)
			{
				this.checkItemsTimer = this.checkItemsInterval * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
				Submarine sub = this.character.Submarine;
				if (sub == null)
				{
					return;
				}
				if (sub.TeamID != this.character.TeamID)
				{
					return;
				}
				Hull currentHull = this.character.CurrentHull;
				if (currentHull == null)
				{
					return;
				}
				this.itemsToClean.Clear();
				foreach (Item item in Item.CleanableItems)
				{
					if (item.CurrentHull == currentHull && AIObjectiveCleanupItems.IsValidTarget(item, this.character, true, false, true, true) && !this.ignoredItems.Contains(item))
					{
						this.itemsToClean.Add(item);
					}
				}
				if (this.itemsToClean.Any<Item>())
				{
					Item targetItem = this.itemsToClean.MinBy((Item i) => Math.Abs(this.character.WorldPosition.X - i.WorldPosition.X));
					if (targetItem != null)
					{
						AIObjectiveCleanupItem cleanupObjective = new AIObjectiveCleanupItem(targetItem, this.character, this.objectiveManager, base.PriorityModifier);
						cleanupObjective.Abandoned += delegate()
						{
							this.ignoredItems.Add(targetItem);
						};
						this.subObjectives.Add(cleanupObjective);
						return;
					}
				}
			}
			else
			{
				this.checkItemsTimer -= deltaTime;
			}
		}

		// Token: 0x06002D71 RID: 11633 RVA: 0x001EAAD4 File Offset: 0x001E8CD4
		public static bool IsForbidden(Hull hull)
		{
			return hull == null || hull.AvoidStaying;
		}

		// Token: 0x06002D72 RID: 11634 RVA: 0x001EAAE4 File Offset: 0x001E8CE4
		public override void Reset()
		{
			base.Reset();
			this.currentTarget = null;
			this.searchingNewHull = false;
			this.tooCloseCharacter = null;
			this.targetHulls.Clear();
			this.hullWeights.Clear();
			this.checkItemsTimer = 0f;
			this.itemsToClean.Clear();
			this.ignoredItems.Clear();
			this.autonomousObjectiveRetryTimer = 10f;
			this.timerMargin = 0f;
			this.newTargetTimer = 0f;
		}

		// Token: 0x06002D73 RID: 11635 RVA: 0x001EAB64 File Offset: 0x001E8D64
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

		// Token: 0x040017BA RID: 6074
		private AIObjectiveIdle.BehaviorType behavior;

		// Token: 0x040017BB RID: 6075
		private float newTargetIntervalMin;

		// Token: 0x040017BC RID: 6076
		private float newTargetIntervalMax;

		// Token: 0x040017BD RID: 6077
		private float standStillMin;

		// Token: 0x040017BE RID: 6078
		private float standStillMax;

		// Token: 0x040017BF RID: 6079
		private readonly float walkDurationMin = 5f;

		// Token: 0x040017C0 RID: 6080
		private readonly float walkDurationMax = 10f;

		// Token: 0x040017C2 RID: 6082
		private Hull currentTarget;

		// Token: 0x040017C3 RID: 6083
		private float newTargetTimer;

		// Token: 0x040017C4 RID: 6084
		private bool searchingNewHull;

		// Token: 0x040017C5 RID: 6085
		private float standStillTimer;

		// Token: 0x040017C6 RID: 6086
		private float walkDuration;

		// Token: 0x040017C7 RID: 6087
		private Character tooCloseCharacter;

		// Token: 0x040017C8 RID: 6088
		private const float chairCheckInterval = 5f;

		// Token: 0x040017C9 RID: 6089
		private float chairCheckTimer;

		// Token: 0x040017CA RID: 6090
		private float autonomousObjectiveRetryTimer = 10f;

		// Token: 0x040017CB RID: 6091
		private readonly List<Hull> targetHulls = new List<Hull>(20);

		// Token: 0x040017CC RID: 6092
		private readonly List<float> hullWeights = new List<float>(20);

		// Token: 0x040017CD RID: 6093
		public readonly HashSet<Identifier> PreferredOutpostModuleTypes = new HashSet<Identifier>();

		// Token: 0x040017CE RID: 6094
		private float timerMargin;

		// Token: 0x040017CF RID: 6095
		private readonly float checkItemsInterval = 1f;

		// Token: 0x040017D0 RID: 6096
		private float checkItemsTimer;

		// Token: 0x040017D1 RID: 6097
		private readonly List<Item> itemsToClean = new List<Item>();

		// Token: 0x040017D2 RID: 6098
		private readonly List<Item> ignoredItems = new List<Item>();

		// Token: 0x02000E28 RID: 3624
		public enum BehaviorType
		{
			// Token: 0x04005199 RID: 20889
			Patrol,
			// Token: 0x0400519A RID: 20890
			Passive,
			// Token: 0x0400519B RID: 20891
			StayInHull,
			// Token: 0x0400519C RID: 20892
			Active
		}
	}
}
