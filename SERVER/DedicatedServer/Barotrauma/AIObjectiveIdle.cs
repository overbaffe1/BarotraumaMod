using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000079 RID: 121
	internal class AIObjectiveIdle : AIObjective
	{
		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06001052 RID: 4178 RVA: 0x00096B4C File Offset: 0x00094D4C
		// (set) Token: 0x06001053 RID: 4179 RVA: 0x00096B54 File Offset: 0x00094D54
		public override Identifier Identifier { get; set; } = "idle".ToIdentifier();

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06001054 RID: 4180 RVA: 0x00096B5D File Offset: 0x00094D5D
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06001055 RID: 4181 RVA: 0x00096B60 File Offset: 0x00094D60
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06001056 RID: 4182 RVA: 0x00096B63 File Offset: 0x00094D63
		// (set) Token: 0x06001057 RID: 4183 RVA: 0x00096B6C File Offset: 0x00094D6C
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

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06001058 RID: 4184 RVA: 0x00096C24 File Offset: 0x00094E24
		// (set) Token: 0x06001059 RID: 4185 RVA: 0x00096C2C File Offset: 0x00094E2C
		public Hull TargetHull { get; set; }

		// Token: 0x0600105A RID: 4186 RVA: 0x00096C38 File Offset: 0x00094E38
		public AIObjectiveIdle(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Behavior = AIObjectiveIdle.BehaviorType.Passive;
			this.standStillTimer = Rand.Range(-10f, 10f, Rand.RandSync.Unsynced);
			this.walkDuration = Rand.Range(0f, 10f, Rand.RandSync.Unsynced);
			this.chairCheckTimer = Rand.Range(0f, 5f, Rand.RandSync.Unsynced);
			this.CalculatePriority(0f);
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x00096D22 File Offset: 0x00094F22
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x0600105C RID: 4188 RVA: 0x00096D25 File Offset: 0x00094F25
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x00096D28 File Offset: 0x00094F28
		public void CalculatePriority(float max = 0f)
		{
			base.Priority = 1f;
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x00096D35 File Offset: 0x00094F35
		protected override float GetPriority()
		{
			return base.Priority;
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x00096D3D File Offset: 0x00094F3D
		public override void Update(float deltaTime)
		{
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x00096D40 File Offset: 0x00094F40
		private void SetTargetTimerLow()
		{
			this.timerMargin += 0.5f;
			this.timerMargin = Math.Min(this.timerMargin, this.newTargetIntervalMin);
			this.newTargetTimer = Math.Max(this.newTargetTimer, this.timerMargin);
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x00096D8D File Offset: 0x00094F8D
		private void SetTargetTimerHigh()
		{
			this.newTargetTimer = Math.Max(this.newTargetTimer, this.newTargetIntervalMin);
			this.timerMargin = 0f;
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x00096DB4 File Offset: 0x00094FB4
		private void SetTargetTimerNormal()
		{
			this.newTargetTimer = ((this.currentTarget != null && this.character.AnimController.InWater) ? this.newTargetIntervalMin : Rand.Range(this.newTargetIntervalMin, this.newTargetIntervalMax, Rand.RandSync.Unsynced));
			this.timerMargin = 0f;
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x00096E06 File Offset: 0x00095006
		private bool IsSteeringFinished()
		{
			return base.PathSteering.CurrentPath != null && (base.PathSteering.CurrentPath.Finished || base.PathSteering.CurrentPath.Unreachable);
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x00096E3C File Offset: 0x0009503C
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

		// Token: 0x06001065 RID: 4197 RVA: 0x000972FC File Offset: 0x000954FC
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

		// Token: 0x06001066 RID: 4198 RVA: 0x000977E8 File Offset: 0x000959E8
		public void FaceTargetAndWait(ISpatialEntity target, float waitTime)
		{
			this.standStillTimer = waitTime;
			base.HumanAIController.FaceTarget(target);
			this.currentTarget = null;
			this.SetTargetTimerHigh();
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x0009780C File Offset: 0x00095A0C
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

		// Token: 0x06001068 RID: 4200 RVA: 0x00097AC4 File Offset: 0x00095CC4
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

		// Token: 0x06001069 RID: 4201 RVA: 0x00097C40 File Offset: 0x00095E40
		public static bool IsForbidden(Hull hull)
		{
			return hull == null || hull.AvoidStaying;
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x00097C50 File Offset: 0x00095E50
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

		// Token: 0x0600106B RID: 4203 RVA: 0x00097CD0 File Offset: 0x00095ED0
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

		// Token: 0x040007C0 RID: 1984
		private AIObjectiveIdle.BehaviorType behavior;

		// Token: 0x040007C1 RID: 1985
		private float newTargetIntervalMin;

		// Token: 0x040007C2 RID: 1986
		private float newTargetIntervalMax;

		// Token: 0x040007C3 RID: 1987
		private float standStillMin;

		// Token: 0x040007C4 RID: 1988
		private float standStillMax;

		// Token: 0x040007C5 RID: 1989
		private readonly float walkDurationMin = 5f;

		// Token: 0x040007C6 RID: 1990
		private readonly float walkDurationMax = 10f;

		// Token: 0x040007C8 RID: 1992
		private Hull currentTarget;

		// Token: 0x040007C9 RID: 1993
		private float newTargetTimer;

		// Token: 0x040007CA RID: 1994
		private bool searchingNewHull;

		// Token: 0x040007CB RID: 1995
		private float standStillTimer;

		// Token: 0x040007CC RID: 1996
		private float walkDuration;

		// Token: 0x040007CD RID: 1997
		private Character tooCloseCharacter;

		// Token: 0x040007CE RID: 1998
		private const float chairCheckInterval = 5f;

		// Token: 0x040007CF RID: 1999
		private float chairCheckTimer;

		// Token: 0x040007D0 RID: 2000
		private float autonomousObjectiveRetryTimer = 10f;

		// Token: 0x040007D1 RID: 2001
		private readonly List<Hull> targetHulls = new List<Hull>(20);

		// Token: 0x040007D2 RID: 2002
		private readonly List<float> hullWeights = new List<float>(20);

		// Token: 0x040007D3 RID: 2003
		public readonly HashSet<Identifier> PreferredOutpostModuleTypes = new HashSet<Identifier>();

		// Token: 0x040007D4 RID: 2004
		private float timerMargin;

		// Token: 0x040007D5 RID: 2005
		private readonly float checkItemsInterval = 1f;

		// Token: 0x040007D6 RID: 2006
		private float checkItemsTimer;

		// Token: 0x040007D7 RID: 2007
		private readonly List<Item> itemsToClean = new List<Item>();

		// Token: 0x040007D8 RID: 2008
		private readonly List<Item> ignoredItems = new List<Item>();

		// Token: 0x020007E0 RID: 2016
		public enum BehaviorType
		{
			// Token: 0x04002E39 RID: 11833
			Patrol,
			// Token: 0x04002E3A RID: 11834
			Passive,
			// Token: 0x04002E3B RID: 11835
			StayInHull,
			// Token: 0x04002E3C RID: 11836
			Active
		}
	}
}
