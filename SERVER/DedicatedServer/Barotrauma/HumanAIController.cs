using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005D RID: 93
	internal class HumanAIController : AIController
	{
		// Token: 0x17000376 RID: 886
		// (get) Token: 0x06000D16 RID: 3350 RVA: 0x0007F72B File Offset: 0x0007D92B
		// (set) Token: 0x06000D17 RID: 3351 RVA: 0x0007F733 File Offset: 0x0007D933
		public float SortTimer { get; set; }

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06000D18 RID: 3352 RVA: 0x0007F73C File Offset: 0x0007D93C
		// (set) Token: 0x06000D19 RID: 3353 RVA: 0x0007F744 File Offset: 0x0007D944
		public float Hearing { get; set; } = 1f;

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06000D1A RID: 3354 RVA: 0x0007F74D File Offset: 0x0007D94D
		// (set) Token: 0x06000D1B RID: 3355 RVA: 0x0007F755 File Offset: 0x0007D955
		public float ReportRange { get; set; } = float.PositiveInfinity;

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06000D1C RID: 3356 RVA: 0x0007F75E File Offset: 0x0007D95E
		// (set) Token: 0x06000D1D RID: 3357 RVA: 0x0007F766 File Offset: 0x0007D966
		public float FindWeaponsRange { get; set; } = float.PositiveInfinity;

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06000D1E RID: 3358 RVA: 0x0007F76F File Offset: 0x0007D96F
		// (set) Token: 0x06000D1F RID: 3359 RVA: 0x0007F777 File Offset: 0x0007D977
		public float AimSpeed
		{
			get
			{
				return this._aimSpeed;
			}
			set
			{
				this._aimSpeed = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06000D20 RID: 3360 RVA: 0x0007F78A File Offset: 0x0007D98A
		// (set) Token: 0x06000D21 RID: 3361 RVA: 0x0007F792 File Offset: 0x0007D992
		public float AimAccuracy
		{
			get
			{
				return this._aimAccuracy;
			}
			set
			{
				this._aimAccuracy = Math.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06000D22 RID: 3362 RVA: 0x0007F7AA File Offset: 0x0007D9AA
		// (set) Token: 0x06000D23 RID: 3363 RVA: 0x0007F7B2 File Offset: 0x0007D9B2
		public bool UseOutsideWaypoints { get; private set; }

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06000D24 RID: 3364 RVA: 0x0007F7BB File Offset: 0x0007D9BB
		public IndoorsSteeringManager PathSteering
		{
			get
			{
				return this.insideSteering as IndoorsSteeringManager;
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06000D25 RID: 3365 RVA: 0x0007F7C8 File Offset: 0x0007D9C8
		public HumanoidAnimController AnimController
		{
			get
			{
				return this.Character.AnimController as HumanoidAnimController;
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06000D26 RID: 3366 RVA: 0x0007F7DA File Offset: 0x0007D9DA
		public AIObjectiveManager ObjectiveManager
		{
			get
			{
				return this.objectiveManager;
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x06000D27 RID: 3367 RVA: 0x0007F7E2 File Offset: 0x0007D9E2
		// (set) Token: 0x06000D28 RID: 3368 RVA: 0x0007F7EA File Offset: 0x0007D9EA
		public float CurrentHullSafety { get; private set; } = 100f;

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x06000D29 RID: 3369 RVA: 0x0007F7F3 File Offset: 0x0007D9F3
		// (set) Token: 0x06000D2A RID: 3370 RVA: 0x0007F7FB File Offset: 0x0007D9FB
		public MentalStateManager MentalStateManager { get; private set; }

		// Token: 0x06000D2B RID: 3371 RVA: 0x0007F804 File Offset: 0x0007DA04
		public void InitMentalStateManager()
		{
			if (this.MentalStateManager == null)
			{
				this.MentalStateManager = new MentalStateManager(this.Character, this);
			}
			this.MentalStateManager.Active = true;
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x06000D2C RID: 3372 RVA: 0x0007F82C File Offset: 0x0007DA2C
		public override bool IsMentallyUnstable
		{
			get
			{
				MentalStateManager mentalStateManager = this.MentalStateManager;
				if (mentalStateManager != null)
				{
					MentalStateManager.MentalType currentMentalType = mentalStateManager.CurrentMentalType;
					if (currentMentalType - MentalStateManager.MentalType.Afraid <= 2)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x06000D2D RID: 3373 RVA: 0x0007F857 File Offset: 0x0007DA57
		// (set) Token: 0x06000D2E RID: 3374 RVA: 0x0007F85F File Offset: 0x0007DA5F
		public ShipCommandManager ShipCommandManager { get; private set; }

		// Token: 0x06000D2F RID: 3375 RVA: 0x0007F868 File Offset: 0x0007DA68
		public void InitShipCommandManager()
		{
			if (this.ShipCommandManager == null)
			{
				this.ShipCommandManager = new ShipCommandManager(this.Character);
			}
			this.ShipCommandManager.Active = true;
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x0007F890 File Offset: 0x0007DA90
		public HumanAIController(Character c) : base(c)
		{
			this.insideSteering = new IndoorsSteeringManager(this, true, false);
			this.outsideSteering = new SteeringManager(this);
			this.objectiveManager = new AIObjectiveManager(c);
			this.reactTimer = HumanAIController.GetReactionTime();
			this.SortTimer = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
			this.reportProblemsTimer = Rand.Range(0f, this.reportProblemsInterval, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x0007FA14 File Offset: 0x0007DC14
		public override void Update(float deltaTime)
		{
			if (HumanAIController.DisableCrewAI || this.Character.Removed)
			{
				return;
			}
			bool isIncapacitated = this.Character.IsIncapacitated;
			if (this.freezeAI && !isIncapacitated)
			{
				this.freezeAI = false;
			}
			if (isIncapacitated)
			{
				return;
			}
			this.wasDead = false;
			this.respondToAttackTimer -= deltaTime;
			if (this.respondToAttackTimer <= 0f)
			{
				foreach (KeyValuePair<Character, AttackResult> previousAttackResult in this.previousAttackResults)
				{
					this.RespondToAttack(previousAttackResult.Key, previousAttackResult.Value);
					if (this.previousHealAmounts.ContainsKey(previousAttackResult.Key))
					{
						this.previousHealAmounts[previousAttackResult.Key] = Math.Min(this.previousHealAmounts[previousAttackResult.Key] - 5f, 100f);
						if (this.previousHealAmounts[previousAttackResult.Key] <= 0f)
						{
							this.previousHealAmounts.Remove(previousAttackResult.Key);
						}
					}
				}
				this.previousAttackResults.Clear();
				this.respondToAttackTimer = 1f;
			}
			base.Update(deltaTime);
			foreach (KeyValuePair<Hull, HumanAIController.HullSafety> values in this.knownHulls)
			{
				HumanAIController.HullSafety hullSafety = values.Value;
				hullSafety.Update(deltaTime);
			}
			if (this.unreachableClearTimer > 0f)
			{
				this.unreachableClearTimer -= deltaTime;
			}
			else
			{
				this.unreachableClearTimer = 30f;
				this.UnreachableHulls.Clear();
				this.IgnoredItems.Clear();
			}
			bool isOutside = this.Character.Submarine == null;
			if (isOutside)
			{
				this.obstacleRaycastTimer -= deltaTime;
				if (this.obstacleRaycastTimer > 0f)
				{
					goto IL_496;
				}
				bool hasValidPath = base.HasValidPath(true, true, null);
				this.isBlocked = false;
				this.UseOutsideWaypoints = false;
				this.obstacleRaycastTimer = this.obstacleRaycastIntervalLong;
				AITarget selectedAiTarget = base.SelectedAiTarget;
				ISpatialEntity spatialEntity = (selectedAiTarget != null) ? selectedAiTarget.Entity : null;
				ISpatialEntity spatialEntity2;
				if ((spatialEntity2 = spatialEntity) == null)
				{
					AIObjectiveGoTo lastActiveObjective = this.ObjectiveManager.GetLastActiveObjective<AIObjectiveGoTo>();
					spatialEntity2 = ((lastActiveObjective != null) ? lastActiveObjective.Target : null);
				}
				ISpatialEntity spatialTarget = spatialEntity2;
				if (spatialTarget != null && (spatialTarget.Submarine == null || !this.<Update>g__IsCloseEnoughToTarget|97_0(2000f, false)))
				{
					IEnumerable<Body> ignoredBodies = null;
					Vector2 rayEnd = spatialTarget.SimPosition;
					Submarine targetSub = spatialTarget.Submarine;
					if (targetSub != null)
					{
						rayEnd += targetSub.SimPosition;
						ignoredBodies = targetSub.PhysicsBody.FarseerBody.ToEnumerable<Body>();
					}
					Body obstacle = Submarine.PickBody(base.SimPosition, rayEnd, ignoredBodies, new Category?(Category.Cat1 | Category.Cat8), true, null, false);
					this.isBlocked = (obstacle != null);
					bool useOutsideWaypoints;
					if (this.isBlocked)
					{
						Submarine sub = obstacle.UserData as Submarine;
						useOutsideWaypoints = (sub == null || sub.Info.IsRuin);
					}
					else
					{
						useOutsideWaypoints = false;
					}
					this.UseOutsideWaypoints = useOutsideWaypoints;
					bool resetPath = false;
					if (this.UseOutsideWaypoints)
					{
						bool flag;
						if (hasValidPath)
						{
							flag = base.HasValidPath(true, true, (WayPoint n) => n.Submarine != null || n.Ruin != null);
						}
						else
						{
							flag = false;
						}
						bool isUsingInsideWaypoints = flag;
						if (isUsingInsideWaypoints)
						{
							resetPath = true;
						}
					}
					else
					{
						bool flag2;
						if (hasValidPath)
						{
							flag2 = base.HasValidPath(true, true, (WayPoint n) => n.Submarine == null && n.Ruin == null);
						}
						else
						{
							flag2 = false;
						}
						bool isUsingOutsideWaypoints = flag2;
						if (isUsingOutsideWaypoints)
						{
							resetPath = true;
						}
					}
					if (resetPath)
					{
						this.PathSteering.ResetPath();
						goto IL_496;
					}
					goto IL_496;
				}
				else
				{
					if (!hasValidPath)
					{
						goto IL_496;
					}
					this.obstacleRaycastTimer = this.obstacleRaycastIntervalShort;
					if (Submarine.MainSub == null)
					{
						goto IL_496;
					}
					using (IEnumerator<Submarine> enumerator3 = Submarine.MainSub.GetConnectedSubs().GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							Submarine connectedSub = enumerator3.Current;
							if (connectedSub != Submarine.MainSub)
							{
								Vector2 rayStart = base.SimPosition - connectedSub.SimPosition;
								Vector2 dir = this.PathSteering.CurrentPath.CurrentNode.WorldPosition - base.WorldPosition;
								Vector2 rayEnd2 = rayStart + dir.ClampLength(this.Character.AnimController.Collider.GetLocalFront(null).Length() * 5f);
								if (Submarine.CheckVisibility(rayStart, rayEnd2, false, true, true, true, true, null) != null)
								{
									this.PathSteering.CurrentPath.Unreachable = true;
									break;
								}
							}
						}
						goto IL_496;
					}
				}
			}
			this.UseOutsideWaypoints = false;
			this.isBlocked = false;
			IL_496:
			if (isOutside || (this.Character.IsOnPlayerTeam && !this.Character.IsEscorted && !this.Character.IsOnFriendlyTeam(this.Character.Submarine.TeamID)))
			{
				this.enemyCheckTimer -= deltaTime;
				if (this.enemyCheckTimer < 0f)
				{
					this.SpotEnemies();
					this.enemyCheckTimer = this.enemyCheckInterval * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				}
			}
			bool useInsideSteering = !isOutside || this.isBlocked || base.HasValidPath(true, true, null) || this.<Update>g__IsCloseEnoughToTarget|97_0(this.steeringBuffer, true);
			if (useInsideSteering)
			{
				if (this.steeringManager != this.insideSteering)
				{
					this.insideSteering.Reset();
					this.PathSteering.ResetPath();
					this.steeringManager = this.insideSteering;
				}
				if (this.<Update>g__IsCloseEnoughToTarget|97_0(this.maxSteeringBuffer, true))
				{
					this.steeringBuffer += this.steeringBufferIncreaseSpeed * deltaTime;
				}
				else
				{
					this.steeringBuffer = this.minSteeringBuffer;
				}
			}
			else
			{
				if (this.steeringManager != this.outsideSteering)
				{
					this.outsideSteering.Reset();
					this.steeringManager = this.outsideSteering;
				}
				this.steeringBuffer = this.minSteeringBuffer;
			}
			this.steeringBuffer = Math.Clamp(this.steeringBuffer, this.minSteeringBuffer, this.maxSteeringBuffer);
			this.AnimController.Crouching = this.shouldCrouch;
			this.CheckCrouching(deltaTime);
			this.Character.ClearInputs();
			if (this.SortTimer > 0f)
			{
				this.SortTimer -= deltaTime;
			}
			else
			{
				this.objectiveManager.SortObjectives();
				this.SortTimer = 1f;
			}
			this.objectiveManager.UpdateObjectives(deltaTime);
			this.UpdateDragged(deltaTime);
			if (this.reportProblemsTimer > 0f)
			{
				this.reportProblemsTimer -= deltaTime;
			}
			if (this.reactTimer > 0f)
			{
				this.reactTimer -= deltaTime;
				if (this.findItemState != HumanAIController.FindItemState.None)
				{
					this.UnequipUnnecessaryItems();
				}
			}
			else
			{
				this.Character.UpdateTeam();
				if (this.Character.CurrentHull != null)
				{
					if (this.Character.IsOnPlayerTeam)
					{
						using (IEnumerator<Hull> enumerator4 = base.VisibleHulls.GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								Hull h = enumerator4.Current;
								HumanAIController.PropagateHullSafety(this.Character, h);
								this.dirtyHullSafetyCalculations.Remove(h);
							}
							goto IL_756;
						}
					}
					foreach (Hull h2 in base.VisibleHulls)
					{
						this.RefreshHullSafety(h2);
						this.dirtyHullSafetyCalculations.Remove(h2);
					}
					IL_756:
					foreach (Hull h3 in this.dirtyHullSafetyCalculations)
					{
						this.RefreshHullSafety(h3);
					}
				}
				this.dirtyHullSafetyCalculations.Clear();
				if (this.reportProblemsTimer <= 0f)
				{
					bool flag3;
					if (this.Character.Submarine != null && (this.Character.Submarine.TeamID == this.Character.TeamID || this.Character.Submarine.TeamID == this.Character.OriginalTeamID || this.Character.IsEscorted) && !this.Character.Submarine.Info.IsWreck)
					{
						this.ReportProblems();
					}
					else if (AIObjectiveRescueAll.IsValidTarget(this.Character, this.Character, out flag3))
					{
						HumanAIController.AddTargets<AIObjectiveRescueAll, Character>(this.Character, this.Character);
					}
					this.reportProblemsTimer = this.reportProblemsInterval;
				}
				this.SpeakAboutIssues();
				this.UnequipUnnecessaryItems();
				this.reactTimer = HumanAIController.GetReactionTime();
			}
			if (this.objectiveManager.CurrentObjective == null)
			{
				return;
			}
			this.objectiveManager.DoCurrentObjective(deltaTime);
			AIObjective currentObjective = this.objectiveManager.CurrentObjective;
			bool run = !currentObjective.ForceWalk && (currentObjective.ForceRun || this.objectiveManager.GetCurrentPriority() > 50f);
			AIObjectiveGoTo goTo = currentObjective as AIObjectiveGoTo;
			if (goTo != null)
			{
				run = goTo.ShouldRun(run);
			}
			if (this.Character.SelectedBy == null || run)
			{
				this.steeringManager.Update(this.Character.AnimController.GetCurrentSpeed(run && this.Character.CanRun));
			}
			bool ignorePlatforms = this.Character.AnimController.TargetMovement.Y < -0.5f && -this.Character.AnimController.TargetMovement.Y > Math.Abs(this.Character.AnimController.TargetMovement.X);
			if (this.steeringManager == this.insideSteering)
			{
				SteeringPath currPath = this.PathSteering.CurrentPath;
				if (currPath != null && currPath.CurrentNode != null && currPath.CurrentNode.SimPosition.Y < this.Character.AnimController.GetColliderBottom().Y)
				{
					float allowedJumpHeight = this.Character.AnimController.ImpactTolerance / 2f;
					float height = Math.Abs(currPath.CurrentNode.SimPosition.Y - this.Character.SimPosition.Y);
					ignorePlatforms = (height < allowedJumpHeight);
				}
				if (this.Character.IsClimbing && this.PathSteering.IsNextLadderSameAsCurrent)
				{
					this.Character.AnimController.TargetMovement = new Vector2(0f, (float)Math.Sign(this.Character.AnimController.TargetMovement.Y));
				}
			}
			this.Character.AnimController.IgnorePlatforms = ignorePlatforms;
			Vector2 targetMovement = this.AnimController.TargetMovement;
			if (!this.Character.AnimController.InWater)
			{
				targetMovement = new Vector2(this.Character.AnimController.TargetMovement.X, MathHelper.Clamp(this.Character.AnimController.TargetMovement.Y, -1f, 1f));
			}
			this.Character.AnimController.TargetMovement = this.Character.ApplyMovementLimits(targetMovement, this.AnimController.GetCurrentSpeed(run));
			this.flipTimer -= deltaTime;
			if (this.flipTimer <= 0f)
			{
				Direction newDir = this.Character.AnimController.TargetDir;
				if (this.Character.IsKeyDown(InputType.Aim))
				{
					float cursorDiffX = this.Character.CursorPosition.X - this.Character.Position.X;
					if (cursorDiffX > 10f)
					{
						newDir = Direction.Right;
					}
					else if (cursorDiffX < -10f)
					{
						newDir = Direction.Left;
					}
					if (this.Character.SelectedItem != null)
					{
						this.Character.SelectedItem.SecondaryUse(deltaTime, this.Character);
					}
				}
				else if (this.AutoFaceMovement && Math.Abs(this.Character.AnimController.TargetMovement.X) > 0.1f && !this.Character.AnimController.InWater)
				{
					newDir = ((this.Character.AnimController.TargetMovement.X > 0f) ? Direction.Right : Direction.Left);
				}
				if (newDir != this.Character.AnimController.TargetDir)
				{
					this.Character.AnimController.TargetDir = newDir;
					this.flipTimer = 0.5f;
				}
			}
			this.AutoFaceMovement = true;
			MentalStateManager mentalStateManager = this.MentalStateManager;
			if (mentalStateManager != null)
			{
				mentalStateManager.Update(deltaTime);
			}
			ShipCommandManager shipCommandManager = this.ShipCommandManager;
			if (shipCommandManager == null)
			{
				return;
			}
			shipCommandManager.Update(deltaTime);
		}

		// Token: 0x06000D32 RID: 3378 RVA: 0x000806B0 File Offset: 0x0007E8B0
		private void SpotEnemies()
		{
			if (this.objectiveManager.IsCurrentObjective<AIObjectiveCombat>())
			{
				return;
			}
			if (this.objectiveManager.HasActiveObjective<AIObjectiveCombat>())
			{
				return;
			}
			float closestDistance = 0f;
			Character closestEnemy = null;
			bool shouldActOffensively = this.ObjectiveManager.HasObjectiveOrOrder<AIObjectiveFightIntruders>();
			foreach (Character c in Character.CharacterList)
			{
				if (c.Submarine == this.Character.Submarine && !c.Removed && !c.IsDead && !c.IsIncapacitated && !c.InDetectable && !this.IsFriendly(c, false))
				{
					Vector2 toTarget = c.WorldPosition - base.WorldPosition;
					float dist = toTarget.LengthSquared();
					float maxDistance = (this.Character.Submarine == null) ? this.enemySpotDistanceOutside : this.enemySpotDistanceInside;
					if (dist <= maxDistance * maxDistance && !EnemyAIController.IsLatchedToSomeoneElse(c, this.Character))
					{
						Limb head = this.Character.AnimController.GetLimb(LimbType.Head, true, false, false);
						if (head != null)
						{
							float rotation = head.body.TransformedRotation;
							Vector2 forward = VectorExtensions.Forward(rotation, 1f);
							float angle = MathHelper.ToDegrees(toTarget.Angle(forward));
							if (angle <= 70f && this.Character.CanSeeTarget(c, null, false, false) && (dist < closestDistance || closestEnemy == null))
							{
								closestEnemy = c;
								closestDistance = dist;
							}
						}
					}
				}
			}
			if (closestEnemy != null)
			{
				this.AddCombatObjective(shouldActOffensively ? AIObjectiveCombat.CombatMode.Offensive : AIObjectiveCombat.CombatMode.Defensive, closestEnemy, 0f, null, null, null, false, false);
			}
		}

		// Token: 0x06000D33 RID: 3379 RVA: 0x00080870 File Offset: 0x0007EA70
		private void UnequipUnnecessaryItems()
		{
			if (this.Character.LockHands)
			{
				return;
			}
			if (this.ObjectiveManager.CurrentObjective == null)
			{
				return;
			}
			if (this.Character.CurrentHull == null)
			{
				return;
			}
			IEnumerable<Item> enumerable;
			bool shouldActOnSuffocation = this.Character.IsLowInOxygen && !this.Character.AnimController.HeadInWater && HumanAIController.HasDivingSuit(this.Character, 0f, false, true) && !HumanAIController.HasItem(this.Character, Tags.OxygenSource, out enumerable, default(Identifier), 1f, false, true, null);
			bool isCarrying = this.ObjectiveManager.HasActiveObjective<AIObjectiveContainItem>() || this.ObjectiveManager.HasActiveObjective<AIObjectiveMoveItem>();
			if (isCarrying)
			{
				if (this.findItemState != HumanAIController.FindItemState.OtherItem)
				{
					AIObjectiveMoveItem moveItemObjective = this.ObjectiveManager.GetLastActiveObjective<AIObjectiveMoveItem>();
					if (moveItemObjective != null && moveItemObjective.TargetItem != null && moveItemObjective.TargetItem.HasTag(Tags.HeavyDivingGear))
					{
						AIObjectiveGoTo gotoObjective = this.ObjectiveManager.GetActiveObjective() as AIObjectiveGoTo;
						if (gotoObjective != null && this.<UnequipUnnecessaryItems>g__NeedsDivingGearOnPath|99_0(gotoObjective))
						{
							gotoObjective.Abandon = true;
						}
					}
				}
				if (!shouldActOnSuffocation)
				{
					return;
				}
			}
			CharacterTeamType? characterTeamType;
			CharacterTeamType teamID;
			if (shouldActOnSuffocation || this.findItemState != HumanAIController.FindItemState.OtherItem)
			{
				bool flag;
				bool needsGear = this.NeedsDivingGear(this.Character.CurrentHull, out flag, null);
				if (!needsGear || shouldActOnSuffocation)
				{
					bool isCurrentObjectiveFindSafety = this.ObjectiveManager.IsCurrentObjective<AIObjectiveFindSafety>();
					bool flag2;
					if (!isCurrentObjectiveFindSafety && !this.Character.AnimController.InWater && !this.Character.AnimController.HeadInWater && !this.Character.IsClimbing && this.Character.Submarine != null && !this.Character.Submarine.Info.HasTag(SubmarineTag.Shuttle) && (Character.IsOnFriendlyTeam(this.Character.TeamID, this.Character.Submarine.TeamID) || this.Character.IsEscorted))
					{
						if (!this.ObjectiveManager.CurrentOrders.Any((Order o) => o.Objective.KeepDivingGearOnAlsoWhenInactive))
						{
							if (!this.ObjectiveManager.CurrentObjective.GetSubObjectivesRecursive(true).Any((AIObjective o) => o.KeepDivingGearOn) && this.Character.CurrentHull.OxygenPercentage >= 40f)
							{
								flag2 = this.Character.CurrentHull.IsWetRoom;
								goto IL_276;
							}
						}
					}
					flag2 = true;
					IL_276:
					bool shouldKeepTheGearOn = flag2;
					bool removeDivingSuit = !shouldKeepTheGearOn && !this.<UnequipUnnecessaryItems>g__IsOrderedToWait|99_4();
					if (shouldActOnSuffocation && this.Character.CurrentHull.Oxygen > 0f && (!isCurrentObjectiveFindSafety || this.Character.OxygenAvailable < 1f))
					{
						shouldKeepTheGearOn = false;
						removeDivingSuit = true;
					}
					bool takeMaskOff = !shouldKeepTheGearOn;
					if (!shouldKeepTheGearOn && !shouldActOnSuffocation)
					{
						if (this.ObjectiveManager.IsCurrentObjective<AIObjectiveIdle>())
						{
							removeDivingSuit = true;
							takeMaskOff = true;
						}
						else
						{
							bool removeSuit = false;
							bool removeMask = false;
							foreach (AIObjective objective in this.ObjectiveManager.CurrentObjective.GetSubObjectivesRecursive(true))
							{
								AIObjectiveGoTo gotoObjective2 = objective as AIObjectiveGoTo;
								if (gotoObjective2 != null)
								{
									if (this.<UnequipUnnecessaryItems>g__NeedsDivingGearOnPath|99_0(gotoObjective2))
									{
										removeDivingSuit = false;
										takeMaskOff = false;
										break;
									}
									if (gotoObjective2.Mimic)
									{
										bool targetHasDivingGear = HumanAIController.HasDivingGear(gotoObjective2.Target as Character, 0f, false);
										if (!removeSuit)
										{
											removeDivingSuit = !targetHasDivingGear;
											if (removeDivingSuit)
											{
												removeSuit = true;
											}
										}
										if (!removeMask)
										{
											takeMaskOff = !targetHasDivingGear;
											if (takeMaskOff)
											{
												removeMask = true;
											}
										}
									}
								}
							}
						}
					}
					if (removeDivingSuit)
					{
						Item divingSuit = this.Character.Inventory.FindEquippedItemByTag(Tags.HeavyDivingGear);
						if (divingSuit != null && !divingSuit.HasTag(Tags.DivingGearWearableIndoors) && divingSuit.IsInteractable(this.Character))
						{
							if (!shouldActOnSuffocation)
							{
								Submarine submarine = this.Character.Submarine;
								characterTeamType = ((submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null);
								teamID = this.Character.TeamID;
								if ((characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null) && this.ObjectiveManager.GetCurrentPriority() < 50f)
								{
									if (this.findItemState != HumanAIController.FindItemState.None && this.findItemState != HumanAIController.FindItemState.DivingSuit)
									{
										goto IL_534;
									}
									this.findItemState = HumanAIController.FindItemState.DivingSuit;
									Item targetContainer;
									if (!this.FindSuitableContainer(divingSuit, out targetContainer))
									{
										goto IL_534;
									}
									this.findItemState = HumanAIController.FindItemState.None;
									this.itemIndex = 0;
									if (targetContainer != null)
									{
										AIObjectiveMoveItem moveItemObjective2 = new AIObjectiveMoveItem(this.Character, divingSuit, this.ObjectiveManager, null, targetContainer.GetComponent<ItemContainer>(), 1f)
										{
											DropIfFails = false
										};
										moveItemObjective2.Abandoned += delegate()
										{
											this.ReequipUnequipped();
											this.IgnoredItems.Add(targetContainer);
										};
										moveItemObjective2.Completed += delegate()
										{
											base.ReequipUnequipped();
										};
										this.ObjectiveManager.CurrentObjective.AddSubObjective(moveItemObjective2, true);
										return;
									}
									divingSuit.Drop(this.Character, true, true);
									this.HandleRelocation(divingSuit);
									base.ReequipUnequipped();
									goto IL_534;
								}
							}
							divingSuit.Drop(this.Character, true, true);
							this.HandleRelocation(divingSuit);
							base.ReequipUnequipped();
						}
					}
					IL_534:
					if (takeMaskOff)
					{
						Item mask = this.Character.Inventory.FindEquippedItemByTag(Tags.LightDivingGear);
						if (mask != null)
						{
							if (!mask.AllowedSlots.Contains(InvSlotType.Any) || !this.Character.Inventory.TryPutItem(mask, this.Character, new List<InvSlotType>
							{
								InvSlotType.Any
							}, true, false, true))
							{
								Submarine submarine2 = this.Character.Submarine;
								characterTeamType = ((submarine2 != null) ? new CharacterTeamType?(submarine2.TeamID) : null);
								teamID = this.Character.TeamID;
								if (!(characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null) || this.ObjectiveManager.GetCurrentPriority() >= 50f)
								{
									mask.Drop(this.Character, true, true);
									this.HandleRelocation(mask);
									base.ReequipUnequipped();
								}
								else
								{
									HumanAIController.FindItemState findItemState = this.findItemState;
									flag = (findItemState == HumanAIController.FindItemState.None || findItemState == HumanAIController.FindItemState.DivingMask);
									if (flag)
									{
										this.findItemState = HumanAIController.FindItemState.DivingMask;
										Item targetContainer;
										if (this.FindSuitableContainer(mask, out targetContainer))
										{
											this.findItemState = HumanAIController.FindItemState.None;
											this.itemIndex = 0;
											if (targetContainer != null)
											{
												AIObjectiveMoveItem moveItemObjective3 = new AIObjectiveMoveItem(this.Character, mask, this.ObjectiveManager, null, targetContainer.GetComponent<ItemContainer>(), 1f);
												moveItemObjective3.Abandoned += delegate()
												{
													this.ReequipUnequipped();
													this.IgnoredItems.Add(targetContainer);
												};
												moveItemObjective3.Completed += base.ReequipUnequipped;
												this.ObjectiveManager.CurrentObjective.AddSubObjective(moveItemObjective3, true);
												return;
											}
											mask.Drop(this.Character, true, true);
											this.HandleRelocation(mask);
											base.ReequipUnequipped();
										}
									}
								}
							}
							else
							{
								base.ReequipUnequipped();
							}
						}
					}
				}
			}
			if (isCarrying)
			{
				return;
			}
			if (!this.ObjectiveManager.CurrentObjective.AllowAutomaticItemUnequipping || !this.ObjectiveManager.GetActiveObjective().AllowAutomaticItemUnequipping)
			{
				return;
			}
			Submarine submarine3 = this.Character.Submarine;
			characterTeamType = ((submarine3 != null) ? new CharacterTeamType?(submarine3.TeamID) : null);
			teamID = this.Character.TeamID;
			bool flag3 = characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null;
			bool flag4 = flag3;
			if (flag4)
			{
				HumanAIController.FindItemState findItemState = this.findItemState;
				bool flag = findItemState == HumanAIController.FindItemState.None || findItemState == HumanAIController.FindItemState.OtherItem;
				flag4 = flag;
			}
			if (flag4)
			{
				foreach (Item item in this.Character.HeldItems)
				{
					if (item != null && item.IsInteractable(this.Character) && item.UnequipAutomatically)
					{
						AIObjectiveOperateItem operateItem = this.ObjectiveManager.CurrentObjective as AIObjectiveOperateItem;
						if (operateItem != null)
						{
							if (operateItem.OperateTarget == item)
							{
								continue;
							}
							ItemComponent component = operateItem.Component;
							if (((component != null) ? component.Item : null) == item)
							{
								continue;
							}
						}
						if (!this.Character.TryPutItemInAnySlot(item) && !this.Character.TryPutItemInBag(item) && !item.HasTag(Tags.Weapon))
						{
							this.findItemState = HumanAIController.FindItemState.OtherItem;
							Item targetContainer;
							if (this.FindSuitableContainer(item, out targetContainer))
							{
								this.findItemState = HumanAIController.FindItemState.None;
								this.itemIndex = 0;
								if (targetContainer != null)
								{
									AIObjectiveMoveItem moveItemObjective4 = new AIObjectiveMoveItem(this.Character, item, this.ObjectiveManager, null, targetContainer.GetComponent<ItemContainer>(), 1f);
									moveItemObjective4.Abandoned += delegate()
									{
										this.ReequipUnequipped();
										this.IgnoredItems.Add(targetContainer);
									};
									this.ObjectiveManager.CurrentObjective.AddSubObjective(moveItemObjective4, true);
									break;
								}
								item.Drop(this.Character, true, true);
								this.HandleRelocation(item);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000D34 RID: 3380 RVA: 0x000811C0 File Offset: 0x0007F3C0
		public void HandleRelocation(Item item)
		{
			HumanAIController.<>c__DisplayClass101_0 CS$<>8__locals1 = new HumanAIController.<>c__DisplayClass101_0();
			CS$<>8__locals1.item = item;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.item.SpawnedInCurrentOutpost)
			{
				return;
			}
			if (CS$<>8__locals1.item.Submarine == null || Submarine.MainSub == null)
			{
				return;
			}
			if (!this.Character.IsOnPlayerTeam)
			{
				return;
			}
			if (CS$<>8__locals1.item.Submarine.TeamID == this.Character.TeamID)
			{
				return;
			}
			if (this.itemsToRelocate.Contains(CS$<>8__locals1.item))
			{
				return;
			}
			this.itemsToRelocate.Add(CS$<>8__locals1.item);
			DockingPort myPort;
			if (CS$<>8__locals1.item.Submarine.ConnectedDockingPorts.TryGetValue(Submarine.MainSub, out myPort))
			{
				myPort.OnUnDocked += CS$<>8__locals1.<HandleRelocation>g__Relocate|0;
			}
			CampaignMode campaign = GameMain.GameSession.Campaign;
			if (campaign != null)
			{
				campaign.BeforeLevelLoading += CS$<>8__locals1.<HandleRelocation>g__Relocate|0;
				campaign.OnSaveAndQuit += CS$<>8__locals1.<HandleRelocation>g__Relocate|0;
				campaign.ItemsRelocatedToMainSub = true;
			}
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x000812BF File Offset: 0x0007F4BF
		public bool FindSuitableContainer(Item containableItem, out Item suitableContainer)
		{
			return HumanAIController.FindSuitableContainer(this.Character, containableItem, this.IgnoredItems, ref this.itemIndex, out suitableContainer);
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x000812DC File Offset: 0x0007F4DC
		public static bool FindSuitableContainer(Character character, Item containableItem, List<Item> ignoredItems, ref int itemIndex, out Item suitableContainer)
		{
			suitableContainer = null;
			Item targetContainer;
			if (character.FindItem(ref itemIndex, out targetContainer, null, true, ignoredItems, null, null, delegate(Item i)
			{
				if (!i.HasAccess(character))
				{
					return 0f;
				}
				ItemContainer container = i.GetComponent<ItemContainer>();
				if (container == null)
				{
					return 0f;
				}
				if (!container.Inventory.CanBePut(containableItem))
				{
					return 0f;
				}
				Item rootContainer = container.Item.RootContainer ?? container.Item;
				if (rootContainer.GetComponent<Fabricator>() != null || rootContainer.GetComponent<Deconstructor>() != null)
				{
					return 0f;
				}
				bool isRestrictionsDefined;
				if (!container.ShouldBeContained(containableItem, out isRestrictionsDefined))
				{
					return 0f;
				}
				if (isRestrictionsDefined)
				{
					return 10f;
				}
				bool isPreferencesDefined;
				bool isSecondary;
				if (containableItem.IsContainerPreferred(container, out isPreferencesDefined, out isSecondary, false))
				{
					return (float)(isPreferencesDefined ? (isSecondary ? 2 : 5) : 1);
				}
				if (!isPreferencesDefined)
				{
					return 1f;
				}
				if (!container.Item.HasTag(Tags.FallbackLocker))
				{
					return 0f;
				}
				return 0.5f;
			}, 10000f, containableItem))
			{
				if (targetContainer != null)
				{
					HumanAIController humanAI = character.AIController as HumanAIController;
					if (humanAI != null)
					{
						if (humanAI.PathSteering.PathFinder.FindPath(character.SimPosition, targetContainer.SimPosition, character.Submarine, "FindSuitableContainer (" + character.DisplayName + ")", 0f, null, null, (PathNode node) => node.Waypoint.CurrentHull != null, true, 0f).Unreachable)
						{
							ignoredItems.Add(targetContainer);
							itemIndex = 0;
							return false;
						}
					}
				}
				suitableContainer = targetContainer;
				return true;
			}
			return false;
		}

		// Token: 0x06000D37 RID: 3383 RVA: 0x000813DC File Offset: 0x0007F5DC
		private void UpdateDragged(float deltaTime)
		{
			HumanPrefab humanPrefab = this.Character.HumanPrefab;
			if (humanPrefab != null && humanPrefab.AllowDraggingIndefinitely)
			{
				return;
			}
			if (this.Character.IsEscorted)
			{
				return;
			}
			if (this.Character.LockHands)
			{
				return;
			}
			if (this.Character.SelectedBy == null || !this.Character.SelectedBy.IsPlayer || this.Character.SelectedBy.TeamID == this.Character.TeamID)
			{
				this.refuseDraggingTimer -= deltaTime;
				return;
			}
			this.draggedTimer += deltaTime;
			if (this.draggedTimer > 10f || (this.refuseDraggingTimer > 0f && this.draggedTimer > 0.5f))
			{
				this.draggedTimer = 0f;
				this.refuseDraggingTimer = 30f;
				this.Character.SelectedBy.DeselectCharacter();
				Character character = this.Character;
				string value = TextManager.Get("dialogrefusedragging").Value;
				Identifier identifier = "refusedragging".ToIdentifier();
				character.Speak(value, null, 0.5f, identifier, 5f);
			}
		}

		// Token: 0x06000D38 RID: 3384 RVA: 0x000814FC File Offset: 0x0007F6FC
		protected void ReportProblems()
		{
			Order newOrder = null;
			Hull targetHull = null;
			bool speak = this.Character.SpeechImpediment < 100f && !this.Character.IsEscorted;
			if (this.Character.CurrentHull != null)
			{
				bool isFighting = this.ObjectiveManager.HasActiveObjective<AIObjectiveCombat>();
				bool isFleeing = this.ObjectiveManager.HasActiveObjective<AIObjectiveFindSafety>();
				foreach (Hull hull in base.VisibleHulls)
				{
					foreach (Character target in Character.CharacterList)
					{
						if (target.CurrentHull == hull && target.Enabled && !target.InDetectable && AIObjectiveFightIntruders.IsValidTarget(target, this.Character, false))
						{
							if (HumanAIController.AddTargets<AIObjectiveFightIntruders, Character>(this.Character, target) && newOrder == null)
							{
								OrderPrefab orderPrefab = OrderPrefab.Prefabs["reportintruders"];
								newOrder = new Order(orderPrefab, hull, null, this.Character, false);
								targetHull = hull;
								if (target.IsEscorted)
								{
									if (!this.Character.IsPrisoner && target.IsPrisoner)
									{
										LocalizedString msg = TextManager.GetWithVariables("orderdialog.prisonerescaped", new ValueTuple<string, LocalizedString, FormatCapitals>[]
										{
											new ValueTuple<string, LocalizedString, FormatCapitals>("[roomname]", targetHull.DisplayName, FormatCapitals.No)
										});
										this.Character.Speak(msg.Value, new ChatMessageType?(ChatMessageType.Order), 0f, default(Identifier), 0f);
										speak = false;
									}
									else if (!this.IsMentallyUnstable && target.AIController.IsMentallyUnstable)
									{
										LocalizedString msg2 = TextManager.GetWithVariables("orderdialog.mentalcase", new ValueTuple<string, LocalizedString, FormatCapitals>[]
										{
											new ValueTuple<string, LocalizedString, FormatCapitals>("[roomname]", targetHull.DisplayName, FormatCapitals.No)
										});
										this.Character.Speak(msg2.Value, new ChatMessageType?(ChatMessageType.Order), 0f, default(Identifier), 0f);
										speak = false;
									}
								}
							}
							if (this.Character.CombatAction == null && !isFighting)
							{
								this.AddCombatObjective(this.ObjectiveManager.HasObjectiveOrOrder<AIObjectiveFightIntruders>() ? AIObjectiveCombat.CombatMode.Offensive : AIObjectiveCombat.CombatMode.Defensive, target, 0f, null, null, null, false, false);
							}
						}
					}
					if (AIObjectiveExtinguishFires.IsValidTarget(hull, this.Character) && HumanAIController.AddTargets<AIObjectiveExtinguishFires, Hull>(this.Character, hull) && newOrder == null)
					{
						OrderPrefab orderPrefab2 = OrderPrefab.Prefabs["reportfire"];
						newOrder = new Order(orderPrefab2, hull, null, this.Character, false);
						targetHull = hull;
					}
					if (HumanAIController.IsBallastFloraNoticeable(this.Character, hull) && newOrder == null)
					{
						OrderPrefab orderPrefab3 = OrderPrefab.Prefabs["reportballastflora"];
						newOrder = new Order(orderPrefab3, hull, null, this.Character, false);
						targetHull = hull;
					}
					if (!isFighting)
					{
						foreach (Gap gap in hull.ConnectedGaps)
						{
							if (AIObjectiveFixLeaks.IsValidTarget(gap, this.Character) && HumanAIController.AddTargets<AIObjectiveFixLeaks, Gap>(this.Character, gap) && newOrder == null && !gap.IsRoomToRoom)
							{
								OrderPrefab orderPrefab4 = OrderPrefab.Prefabs["reportbreach"];
								newOrder = new Order(orderPrefab4, hull, null, this.Character, false);
								targetHull = hull;
							}
						}
						if (!isFleeing)
						{
							this.CheckForDraggedCorpses();
							foreach (Character target2 in Character.CharacterList)
							{
								bool flag;
								if (target2.CurrentHull == hull && AIObjectiveRescueAll.IsValidTarget(target2, this.Character, out flag) && HumanAIController.AddTargets<AIObjectiveRescueAll, Character>(this.Character, target2) && newOrder == null && (!this.Character.IsMedic || this.Character == target2) && !this.ObjectiveManager.HasActiveObjective<AIObjectiveRescue>())
								{
									OrderPrefab orderPrefab5 = OrderPrefab.Prefabs["requestfirstaid"];
									newOrder = new Order(orderPrefab5, hull, null, this.Character, false);
									targetHull = hull;
								}
							}
							foreach (Item item in Item.RepairableItems)
							{
								if (item.CurrentHull == hull && AIObjectiveRepairItems.IsValidTarget(item, this.Character))
								{
									if (item.Repairables.Any((Repairable r) => r.IsBelowRepairIconThreshold) && HumanAIController.AddTargets<AIObjectiveRepairItems, Item>(this.Character, item) && newOrder == null && !this.ObjectiveManager.HasActiveObjective<AIObjectiveRepairItem>())
									{
										OrderPrefab orderPrefab6 = OrderPrefab.Prefabs["reportbrokendevices"];
										OrderPrefab prefab = orderPrefab6;
										Entity targetEntity = hull;
										List<Repairable> repairables = item.Repairables;
										newOrder = new Order(prefab, targetEntity, (repairables != null) ? repairables.FirstOrDefault<Repairable>() : null, this.Character, false);
										targetHull = hull;
									}
								}
							}
						}
					}
				}
			}
			if (newOrder != null && speak)
			{
				Order order = newOrder;
				string empty = string.Empty;
				string text;
				if (targetHull == null)
				{
					text = null;
				}
				else
				{
					LocalizedString displayName = targetHull.DisplayName;
					text = ((displayName != null) ? displayName.Value : null);
				}
				string msg3 = order.GetChatMessage(empty, text ?? string.Empty, false, default(Identifier), true);
				if (this.Character.TeamID == CharacterTeamType.FriendlyNPC)
				{
					Character character = this.Character;
					string message = msg3;
					ChatMessageType? messageType = new ChatMessageType?(ChatMessageType.Default);
					float delay = 0f;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(newOrder.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendFormatted(((targetHull != null) ? targetHull.RoomName : null) ?? "null");
					character.Speak(message, messageType, delay, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), 60f);
					return;
				}
				if (this.Character.IsOnPlayerTeam)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.CrewManager : null) != null && GameMain.GameSession.CrewManager.AddOrder(newOrder, new float?(newOrder.FadeOutTime)))
					{
						this.Character.Speak(msg3, new ChatMessageType?(ChatMessageType.Order), 0f, default(Identifier), 0f);
						GameMain.Server.SendOrderChatMessage(new OrderChatMessage(newOrder.WithManualPriority(CharacterInfo.HighestManualOrderPriority).WithTargetEntity(targetHull).WithOrderGiver(this.Character), msg3, null, this.Character, true));
					}
				}
			}
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x00081BC0 File Offset: 0x0007FDC0
		public static bool IsBallastFloraNoticeable(Character character, Hull hull)
		{
			Func<BallastFloraBranch, bool> <>9__0;
			foreach (BallastFloraBehavior ballastFlora in BallastFloraBehavior.EntityList)
			{
				Hull parent = ballastFlora.Parent;
				if (((parent != null) ? parent.Submarine : null) == character.Submarine && ballastFlora.HasBrokenThrough)
				{
					IEnumerable<BallastFloraBranch> branches = ballastFlora.Branches;
					Func<BallastFloraBranch, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((BallastFloraBranch b) => !b.Removed && b.Health > 0f && b.CurrentHull == hull));
					}
					if (branches.Count(predicate) > 2)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x00081C6C File Offset: 0x0007FE6C
		public static void ReportProblem(Character reporter, Order order, Hull targetHull = null)
		{
			HumanAIController.<>c__DisplayClass115_0 CS$<>8__locals1;
			CS$<>8__locals1.reporter = reporter;
			CS$<>8__locals1.order = order;
			if (CS$<>8__locals1.reporter == null || CS$<>8__locals1.order == null)
			{
				return;
			}
			if (targetHull == null)
			{
				using (List<Hull>.Enumerator enumerator = CS$<>8__locals1.reporter.GetVisibleHulls().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Hull hull = enumerator.Current;
						HumanAIController.<ReportProblem>g__Report|115_0(hull, ref CS$<>8__locals1);
					}
					return;
				}
			}
			HumanAIController.<ReportProblem>g__Report|115_0(targetHull, ref CS$<>8__locals1);
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x00081CF4 File Offset: 0x0007FEF4
		private void SpeakAboutIssues()
		{
			if (!this.Character.IsOnPlayerTeam)
			{
				return;
			}
			if (this.Character.SpeechImpediment >= 100f)
			{
				return;
			}
			float minDelay = 0.5f;
			float maxDelay = 2f;
			if (this.Character.Oxygen < 30f)
			{
				string msgId = "DialogLowOxygen";
				Character character = this.Character;
				string value = TextManager.Get(msgId).Value;
				float delay = Rand.Range(minDelay, maxDelay, Rand.RandSync.Unsynced);
				Identifier identifier = msgId.ToIdentifier();
				character.Speak(value, null, delay, identifier, 30f);
			}
			if (this.Character.Bleeding > AfflictionPrefab.Bleeding.TreatmentThreshold && !this.Character.IsMedic)
			{
				string msgId2 = "DialogBleeding";
				Character character2 = this.Character;
				string value2 = TextManager.Get(msgId2).Value;
				float delay = Rand.Range(minDelay, maxDelay, Rand.RandSync.Unsynced);
				Identifier identifier = msgId2.ToIdentifier();
				character2.Speak(value2, null, delay, identifier, 30f);
			}
			if ((this.Character.CurrentHull == null || this.Character.CurrentHull.LethalPressure > 0f) && !this.Character.IsProtectedFromPressure)
			{
				if (this.Character.PressureProtection > 0f)
				{
					string msgId3 = "DialogInsufficientPressureProtection";
					Character character3 = this.Character;
					string value3 = TextManager.Get(msgId3).Value;
					float delay = Rand.Range(minDelay, maxDelay, Rand.RandSync.Unsynced);
					Identifier identifier = msgId3.ToIdentifier();
					character3.Speak(value3, null, delay, identifier, 30f);
					return;
				}
				Hull currentHull = this.Character.CurrentHull;
				if (((currentHull != null) ? currentHull.DisplayName : null) != null)
				{
					string msgId4 = "DialogPressure";
					Character character4 = this.Character;
					string value4 = TextManager.GetWithVariable(msgId4, "[roomname]", this.Character.CurrentHull.DisplayName, FormatCapitals.Yes).Value;
					float delay = Rand.Range(minDelay, maxDelay, Rand.RandSync.Unsynced);
					Identifier identifier = msgId4.ToIdentifier();
					character4.Speak(value4, null, delay, identifier, 30f);
				}
			}
		}

		// Token: 0x06000D3C RID: 3388 RVA: 0x00081EE8 File Offset: 0x000800E8
		private void CheckForDraggedCorpses()
		{
			if (this.Character.IsOnPlayerTeam)
			{
				return;
			}
			Submarine submarine = this.Character.Submarine;
			if (submarine != null)
			{
				SubmarineInfo info = submarine.Info;
				if (info != null && info.IsOutpost)
				{
					foreach (Character otherCharacter in Character.CharacterList)
					{
						if (otherCharacter.SelectedCharacter != null && otherCharacter.SelectedCharacter.IsDead && otherCharacter.SelectedCharacter.TeamID == this.Character.TeamID && !otherCharacter.IsPet && !otherCharacter.IsInstigator && this.Character.CanSeeTarget(otherCharacter, null, false, false))
						{
							string dialogTag = this.Character.IsSecurity ? "dialogdraggingcorpsereactionsecurity" : "dialogdraggingcorpsereaction";
							this.Character.Speak(TextManager.Get(dialogTag).Value, null, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), "dialogdraggingcorpsereaction".ToIdentifier(), 10f);
							this.AddCombatObjective(this.Character.IsSecurity ? AIObjectiveCombat.CombatMode.Arrest : AIObjectiveCombat.CombatMode.Retreat, otherCharacter, 0f, null, null, null, false, false);
							break;
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000D3D RID: 3389 RVA: 0x00082048 File Offset: 0x00080248
		public override void OnHealed(Character healer, float healAmount)
		{
			if (healer == null || healAmount <= 0f)
			{
				return;
			}
			if (this.previousHealAmounts.ContainsKey(healer))
			{
				Dictionary<Character, float> dictionary = this.previousHealAmounts;
				dictionary[healer] += healAmount;
				return;
			}
			this.previousHealAmounts.Add(healer, healAmount);
		}

		// Token: 0x06000D3E RID: 3390 RVA: 0x00082098 File Offset: 0x00080298
		public override void OnAttacked(Character attacker, AttackResult attackResult)
		{
			if (this.Character.IsDead && this.wasDead)
			{
				return;
			}
			if (this.Character.IsIncapacitated || this.Character.Stun > 0f)
			{
				this.RespondToAttack(attacker, attackResult);
				this.wasDead = this.Character.IsDead;
				return;
			}
			if (attacker == null || this.Character.IsPlayer)
			{
				this.RespondToAttack(attacker, attackResult);
				return;
			}
			if (this.previousAttackResults.ContainsKey(attacker))
			{
				if (attackResult.Afflictions != null)
				{
					using (List<Affliction>.Enumerator enumerator = attackResult.Afflictions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Affliction newAffliction = enumerator.Current;
							Affliction matchingAffliction = this.previousAttackResults[attacker].Afflictions.Find((Affliction a) => a.Prefab == newAffliction.Prefab && a.Source == newAffliction.Source);
							if (matchingAffliction == null)
							{
								this.previousAttackResults[attacker].Afflictions.Add(newAffliction);
							}
							else
							{
								matchingAffliction.Strength += newAffliction.Strength;
							}
						}
					}
				}
				this.previousAttackResults[attacker] = new AttackResult(this.previousAttackResults[attacker].Afflictions, this.previousAttackResults[attacker].HitLimb, null);
				return;
			}
			this.previousAttackResults.Add(attacker, attackResult);
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x00082214 File Offset: 0x00080414
		private void RespondToAttack(Character attacker, AttackResult attackResult)
		{
			HumanAIController.<>c__DisplayClass120_0 CS$<>8__locals1 = new HumanAIController.<>c__DisplayClass120_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.attacker = attacker;
			float healAmount = 0f;
			if (CS$<>8__locals1.attacker != null)
			{
				this.previousHealAmounts.TryGetValue(CS$<>8__locals1.attacker, out healAmount);
			}
			float realDamage = attackResult.Damage - healAmount;
			float totalDamage = realDamage;
			if (attackResult.Afflictions != null)
			{
				foreach (Affliction affliction in attackResult.Afflictions)
				{
					totalDamage -= affliction.Prefab.KarmaChangeOnApplied * affliction.Strength;
				}
			}
			if (totalDamage <= 0.01f)
			{
				return;
			}
			if (this.Character.IsBot && !this.freezeAI && !this.Character.IsDead && this.Character.IsIncapacitated)
			{
				this.objectiveManager.CreateAutonomousObjectives();
				this.objectiveManager.SortObjectives();
				this.freezeAI = true;
			}
			if (CS$<>8__locals1.attacker == null || CS$<>8__locals1.attacker.IsUnconscious || CS$<>8__locals1.attacker.Removed)
			{
				return;
			}
			bool sameTeam = CS$<>8__locals1.attacker.TeamID == this.Character.TeamID || (CS$<>8__locals1.attacker.TeamID == CharacterTeamType.Team1 && this.Character.IsEscorted);
			if (realDamage <= 0f && (CS$<>8__locals1.attacker.IsBot || sameTeam))
			{
				return;
			}
			if (CS$<>8__locals1.attacker.Submarine == null && this.Character.Submarine != null)
			{
				return;
			}
			CS$<>8__locals1.isAttackerInfected = false;
			CS$<>8__locals1.isAttackerFightingEnemy = false;
			CS$<>8__locals1.minorDamageThreshold = 5f;
			CS$<>8__locals1.majorDamageThreshold = 20f;
			if (sameTeam && !CS$<>8__locals1.attacker.IsInstigator)
			{
				CS$<>8__locals1.minorDamageThreshold = 10f;
				CS$<>8__locals1.majorDamageThreshold = 40f;
			}
			HumanAIController.<>c__DisplayClass120_0 CS$<>8__locals2 = CS$<>8__locals1;
			bool eitherIsMentallyUnstable;
			if (!this.IsMentallyUnstable)
			{
				AIController aicontroller = CS$<>8__locals1.attacker.AIController;
				eitherIsMentallyUnstable = (aicontroller != null && aicontroller.IsMentallyUnstable);
			}
			else
			{
				eitherIsMentallyUnstable = true;
			}
			CS$<>8__locals2.eitherIsMentallyUnstable = eitherIsMentallyUnstable;
			if (this.IsFriendly(CS$<>8__locals1.attacker, false))
			{
				if (CS$<>8__locals1.attacker.AnimController.Anim == Barotrauma.AnimController.Animation.CPR && CS$<>8__locals1.attacker.SelectedCharacter == this.Character)
				{
					return;
				}
				float cumulativeDamage = realDamage + this.Character.GetDamageDoneByAttacker(CS$<>8__locals1.attacker);
				bool isAccidental = CS$<>8__locals1.attacker.IsBot && !CS$<>8__locals1.eitherIsMentallyUnstable && CS$<>8__locals1.attacker.CombatAction == null;
				if (isAccidental)
				{
					if (CS$<>8__locals1.attacker.TeamID != this.Character.TeamID || (!this.Character.IsSecurity && cumulativeDamage > CS$<>8__locals1.minorDamageThreshold))
					{
						this.AddCombatObjective(AIObjectiveCombat.CombatMode.Retreat, CS$<>8__locals1.attacker, 0f, null, null, null, false, false);
						return;
					}
				}
				else
				{
					CS$<>8__locals1.isAttackerInfected = (CS$<>8__locals1.attacker.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.AlienInfectionType, true) > 0f);
					if ((CS$<>8__locals1.isAttackerInfected || cumulativeDamage > CS$<>8__locals1.minorDamageThreshold || totalDamage > CS$<>8__locals1.minorDamageThreshold) && (!CS$<>8__locals1.attacker.IsPlayer || this.Character.TeamID != CS$<>8__locals1.attacker.TeamID))
					{
						CS$<>8__locals1.<RespondToAttack>g__InformOtherNPCs|0(cumulativeDamage);
					}
					if (this.Character.IsBot)
					{
						AIObjectiveCombat.CombatMode combatMode = CS$<>8__locals1.<RespondToAttack>g__DetermineCombatMode|1(this.Character, cumulativeDamage, false);
						if (CS$<>8__locals1.attacker.IsPlayer && !this.Character.IsInstigator && !this.ObjectiveManager.IsCurrentObjective<AIObjectiveCombat>())
						{
							switch (combatMode)
							{
							case AIObjectiveCombat.CombatMode.Defensive:
							case AIObjectiveCombat.CombatMode.Retreat:
								if (this.Character.IsSecurity)
								{
									this.Character.Speak(TextManager.Get("dialogattackedbyfriendlysecurityresponse").Value, null, 0.5f, "attackedbyfriendlysecurityresponse".ToIdentifier(), 10f);
								}
								else
								{
									this.Character.Speak(TextManager.Get("DialogAttackedByFriendly").Value, null, 0.5f, "attackedbyfriendly".ToIdentifier(), 10f);
								}
								break;
							case AIObjectiveCombat.CombatMode.Offensive:
							case AIObjectiveCombat.CombatMode.Arrest:
								this.Character.Speak(TextManager.Get("dialogattackedbyfriendlysecurityarrest").Value, null, 0.5f, "attackedbyfriendlysecurityarrest".ToIdentifier(), 10f);
								break;
							case AIObjectiveCombat.CombatMode.None:
								if (this.Character.IsSecurity && realDamage > 1f)
								{
									this.Character.Speak(TextManager.Get("dialogattackedbyfriendlysecurityresponse").Value, null, 0.5f, "attackedbyfriendlysecurityresponse".ToIdentifier(), 10f);
								}
								break;
							}
						}
						this.AddCombatObjective(combatMode, CS$<>8__locals1.attacker, (realDamage > 1f) ? HumanAIController.GetReactionTime() : 0f, null, null, null, false, false);
					}
					if (!CS$<>8__locals1.isAttackerFightingEnemy)
					{
						GameSession gameSession = GameMain.GameSession;
						CampaignMode campaignMode = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
						if (campaignMode == null)
						{
							return;
						}
						campaignMode.OutpostNPCAttacked(this.Character, CS$<>8__locals1.attacker, attackResult);
						return;
					}
				}
			}
			else
			{
				if (this.Character.Submarine != null && this.Character.Submarine.GetConnectedSubs().Contains(CS$<>8__locals1.attacker.Submarine))
				{
					CS$<>8__locals1.<RespondToAttack>g__InformOtherNPCs|0(0f);
				}
				if (this.Character.IsBot)
				{
					this.AddCombatObjective(CS$<>8__locals1.<RespondToAttack>g__DetermineCombatMode|1(this.Character, 0f, false), CS$<>8__locals1.attacker, 0f, null, null, null, false, false);
				}
			}
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x000827A0 File Offset: 0x000809A0
		public void AddCombatObjective(AIObjectiveCombat.CombatMode mode, Character target, float delay = 0f, Func<AIObjective, bool> abortCondition = null, Action onAbort = null, Action onCompleted = null, bool allowHoldFire = false, bool speakWarnings = false)
		{
			HumanAIController.<>c__DisplayClass121_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.target = target;
			CS$<>8__locals1.mode = mode;
			CS$<>8__locals1.abortCondition = abortCondition;
			CS$<>8__locals1.allowHoldFire = allowHoldFire;
			CS$<>8__locals1.speakWarnings = speakWarnings;
			CS$<>8__locals1.onAbort = onAbort;
			CS$<>8__locals1.onCompleted = onCompleted;
			if (CS$<>8__locals1.mode == AIObjectiveCombat.CombatMode.None)
			{
				return;
			}
			if (this.Character.IsDead || this.Character.IsIncapacitated || this.Character.Removed)
			{
				return;
			}
			if (!this.Character.IsBot)
			{
				return;
			}
			AIObjectiveCombat combatObjective = this.ObjectiveManager.Objectives.FirstOrDefault((AIObjective o) => o is AIObjectiveCombat) as AIObjectiveCombat;
			if (combatObjective != null)
			{
				if (combatObjective.Mode == AIObjectiveCombat.CombatMode.Offensive && CS$<>8__locals1.mode != AIObjectiveCombat.CombatMode.Offensive)
				{
					return;
				}
				if (combatObjective.Mode != CS$<>8__locals1.mode || combatObjective.Enemy != CS$<>8__locals1.target || (combatObjective.Enemy == null && CS$<>8__locals1.target == null))
				{
					this.ObjectiveManager.Objectives.Remove(combatObjective);
					this.ObjectiveManager.AddObjective<AIObjectiveCombat>(this.<AddCombatObjective>g__CreateCombatObjective|121_0(ref CS$<>8__locals1));
					return;
				}
			}
			else
			{
				if (delay > 0f)
				{
					this.ObjectiveManager.AddObjective<AIObjectiveCombat>(this.<AddCombatObjective>g__CreateCombatObjective|121_0(ref CS$<>8__locals1), delay, null);
					return;
				}
				this.ObjectiveManager.AddObjective<AIObjectiveCombat>(this.<AddCombatObjective>g__CreateCombatObjective|121_0(ref CS$<>8__locals1));
			}
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x00082900 File Offset: 0x00080B00
		public void SetOrder(Order order, bool speak = true)
		{
			this.objectiveManager.SetOrder(order, speak);
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x00082910 File Offset: 0x00080B10
		public AIObjective SetForcedOrder(Order order)
		{
			AIObjective objective = this.ObjectiveManager.CreateObjective(order, 1f);
			if (order != null)
			{
				bool isDismissal = order.IsDismissal;
			}
			this.ObjectiveManager.SetForcedOrder(objective);
			return objective;
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x00082946 File Offset: 0x00080B46
		public void ClearForcedOrder()
		{
			this.ObjectiveManager.ClearForcedOrder();
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x00082953 File Offset: 0x00080B53
		public override void SelectTarget(AITarget target)
		{
			base.SelectedAiTarget = target;
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x0008295C File Offset: 0x00080B5C
		public override void Reset()
		{
			base.Reset();
			this.objectiveManager.SortObjectives();
			this.SortTimer = 1f;
			float waitDuration = HumanAIController.characterWaitOnSwitch;
			if (this.ObjectiveManager.IsCurrentObjective<AIObjectiveIdle>())
			{
				waitDuration *= 2f;
			}
			this.ObjectiveManager.WaitTimer = waitDuration;
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x000829AC File Offset: 0x00080BAC
		public override bool Escape(float deltaTime)
		{
			return base.UpdateEscape(deltaTime, false);
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x000829B8 File Offset: 0x00080BB8
		private void CheckCrouching(float deltaTime)
		{
			this.crouchRaycastTimer -= deltaTime;
			if (this.crouchRaycastTimer > 0f)
			{
				return;
			}
			this.crouchRaycastTimer = 1f;
			Vector2 startPos = this.Character.SimPosition;
			startPos.X += MathHelper.Clamp(this.Character.AnimController.TargetMovement.X, -1f, 1f);
			PhysicsBody mainCollider;
			if (!this.Character.AnimController.TryGetCollider(0, out mainCollider))
			{
				mainCollider = this.Character.AnimController.Collider;
			}
			float margin = 0.1f;
			if (this.shouldCrouch)
			{
				margin *= 2f;
			}
			float minCeilingDist = mainCollider.Height / 2f + mainCollider.Radius + margin;
			this.shouldCrouch = (Submarine.PickBody(startPos, startPos + Vector2.UnitY * minCeilingDist, null, new Category?(Category.Cat1), true, (Fixture fixture) => !(fixture.Body.UserData is Submarine), false) != null);
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x00082AC4 File Offset: 0x00080CC4
		public bool AllowCampaignInteraction()
		{
			if (this.Character == null || this.Character.Removed)
			{
				return false;
			}
			CampaignMode.InteractionType type = this.Character.CampaignInteractionType;
			if (type != CampaignMode.InteractionType.None && type != CampaignMode.InteractionType.Talk && type != CampaignMode.InteractionType.Examine)
			{
				if (this.Character.IsIncapacitated)
				{
					return false;
				}
				AIObjective currentObjective = this.ObjectiveManager.CurrentObjective;
				if (currentObjective is AIObjectiveCombat || currentObjective is AIObjectiveFindSafety || currentObjective is AIObjectiveExtinguishFires || currentObjective is AIObjectiveFightIntruders || currentObjective is AIObjectiveFixLeaks)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x00082B48 File Offset: 0x00080D48
		public bool NeedsDivingGear(Hull hull, out bool needsSuit, AIObjectiveManager objectiveManager = null)
		{
			needsSuit = false;
			bool needsAir = this.Character.NeedsAir && this.Character.CharacterHealth.OxygenLowResistance < 1f;
			if (hull != null && hull.WaterPercentage <= 90f && hull.LethalPressure <= 0f)
			{
				if (!hull.ConnectedGaps.Any((Gap gap) => !gap.IsRoomToRoom && gap.Open > 0.9f))
				{
					return (hull.WaterPercentage > 60f || (hull.IsWetRoom && hull.WaterPercentage > 10f) || hull.OxygenPercentage < 31f) && needsAir;
				}
			}
			if (!this.Character.IsImmuneToPressure)
			{
				bool flag;
				if (hull != null && hull.LethalPressure <= 0f)
				{
					AIObjectiveOperateItem operateItem = ((objectiveManager != null) ? objectiveManager.CurrentOrder : null) as AIObjectiveOperateItem;
					flag = (operateItem != null && operateItem.GetTarget().Item.CurrentHull == hull);
				}
				else
				{
					flag = true;
				}
				needsSuit = flag;
			}
			return needsAir | needsSuit;
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x00082C4D File Offset: 0x00080E4D
		public static bool HasDivingGear(Character character, float conditionPercentage = 0f, bool requireOxygenTank = true)
		{
			return HumanAIController.HasDivingSuit(character, conditionPercentage, requireOxygenTank, true) || HumanAIController.HasDivingMask(character, conditionPercentage, requireOxygenTank);
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x00082C64 File Offset: 0x00080E64
		public static bool HasDivingSuit(Character character, float conditionPercentage = 0f, bool requireOxygenTank = true, bool requireSuitablePressureProtection = true)
		{
			IEnumerable<Item> enumerable;
			return HumanAIController.HasItem(character, Tags.HeavyDivingGear, out enumerable, requireOxygenTank ? Tags.OxygenSource : Identifier.Empty, conditionPercentage, true, true, (Item item) => character.HasEquippedItem(item, new InvSlotType?(InvSlotType.InnerClothes | InvSlotType.OuterClothes), null) && (!requireSuitablePressureProtection || AIObjectiveFindDivingGear.IsSuitablePressureProtection(item, Tags.HeavyDivingGear, character)));
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x00082CB8 File Offset: 0x00080EB8
		public static bool HasDivingMask(Character character, float conditionPercentage = 0f, bool requireOxygenTank = true)
		{
			IEnumerable<Item> enumerable;
			return HumanAIController.HasItem(character, Tags.LightDivingGear, out enumerable, requireOxygenTank ? Tags.OxygenSource : Identifier.Empty, conditionPercentage, true, true, null);
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x00082CE8 File Offset: 0x00080EE8
		public static bool HasItem(Character character, Identifier tagOrIdentifier, out IEnumerable<Item> items, Identifier containedTag = default(Identifier), float conditionPercentage = 0f, bool requireEquipped = false, bool recursive = true, Func<Item, bool> predicate = null)
		{
			HumanAIController.matchingItems.Clear();
			items = HumanAIController.matchingItems;
			Character character2 = character;
			if (((character2 != null) ? character2.Inventory : null) == null)
			{
				return false;
			}
			HumanAIController.matchingItems = character.Inventory.FindAllItems((Item i) => (i.Prefab.Identifier == tagOrIdentifier || i.HasTag(tagOrIdentifier)) && i.ConditionPercentage >= conditionPercentage && (!requireEquipped || character.HasEquippedItem(i, null, null)) && (predicate == null || predicate(i)), recursive, HumanAIController.matchingItems);
			items = HumanAIController.matchingItems;
			Func<Item, bool> <>9__1;
			foreach (Item item in HumanAIController.matchingItems)
			{
				if (item != null)
				{
					if (containedTag.IsEmpty || item.OwnInventory == null)
					{
						return true;
					}
					int? suitableSlot = item.GetComponent<ItemContainer>().FindSuitableSubContainerIndex(containedTag);
					if (suitableSlot == null)
					{
						IEnumerable<Item> containedItems = item.ContainedItems;
						Func<Item, bool> predicate2;
						if ((predicate2 = <>9__1) == null)
						{
							predicate2 = (<>9__1 = ((Item it) => it.HasTag(containedTag) && it.ConditionPercentage > conditionPercentage));
						}
						return containedItems.Any(predicate2);
					}
					return item.ContainedItems.Any((Item it) => it.HasTag(containedTag) && it.ConditionPercentage > conditionPercentage && it.ParentInventory.IsInSlot(it, suitableSlot.Value));
				}
			}
			return false;
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x00082E80 File Offset: 0x00081080
		public static void StructureDamaged(Structure structure, float damageAmount, Character character)
		{
			if (character == null || damageAmount <= 0f)
			{
				return;
			}
			if (((structure != null) ? structure.Submarine : null) == null || !structure.Submarine.Info.IsOutpost || character.TeamID == structure.Submarine.TeamID)
			{
				return;
			}
			if (!structure.Prefab.IndestructibleInOutposts)
			{
				return;
			}
			bool someoneSpoke = false;
			float maxAccumulatedDamage = 0f;
			using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
			{
				Func<Character, float> <>9__1;
				while (enumerator.MoveNext())
				{
					Character otherCharacter = enumerator.Current;
					if (otherCharacter != character && otherCharacter.TeamID != character.TeamID && !otherCharacter.IsDead)
					{
						CharacterInfo info = otherCharacter.Info;
						if (((info != null) ? info.Job : null) != null)
						{
							HumanAIController otherHumanAI = otherCharacter.AIController as HumanAIController;
							if (otherHumanAI != null && Vector2.DistanceSquared(otherCharacter.WorldPosition, character.WorldPosition) <= 1000000f && otherCharacter.CanSeeTarget(character, null, true, false))
							{
								otherHumanAI.structureDamageAccumulator.TryAdd(character, 0f);
								float prevAccumulatedDamage = otherHumanAI.structureDamageAccumulator[character];
								Dictionary<Character, float> dictionary = otherHumanAI.structureDamageAccumulator;
								Character character2 = character;
								dictionary[character2] += MathHelper.Clamp(damageAmount, -0.083333336f, 0.083333336f);
								float accumulatedDamage = Math.Max(otherHumanAI.structureDamageAccumulator[character], maxAccumulatedDamage);
								maxAccumulatedDamage = Math.Max(accumulatedDamage, maxAccumulatedDamage);
								GameSession gameSession = GameMain.GameSession;
								bool flag;
								if (gameSession == null)
								{
									flag = (null != null);
								}
								else
								{
									CampaignMode campaign = gameSession.Campaign;
									if (campaign == null)
									{
										flag = (null != null);
									}
									else
									{
										Map map = campaign.Map;
										if (map == null)
										{
											flag = (null != null);
										}
										else
										{
											Location currentLocation = map.CurrentLocation;
											flag = (((currentLocation != null) ? currentLocation.Reputation : null) != null);
										}
									}
								}
								if (flag && character.IsPlayer)
								{
									float reputationLoss = damageAmount * 0.025f;
									GameMain.GameSession.Campaign.Map.CurrentLocation.Reputation.AddReputation(-reputationLoss, 10f);
								}
								if (!character.IsCriminal)
								{
									if (accumulatedDamage <= 5f)
									{
										break;
									}
									if (accumulatedDamage > 5f && prevAccumulatedDamage <= 5f && !someoneSpoke && !character.IsIncapacitated && character.Stun <= 0f)
									{
										if (accumulatedDamage < 20f)
										{
											AIObjectiveIdle idleObjective = otherHumanAI.ObjectiveManager.CurrentObjective as AIObjectiveIdle;
											if (idleObjective != null)
											{
												idleObjective.FaceTargetAndWait(character, 5f);
											}
										}
										otherCharacter.Speak(TextManager.Get("dialogdamagewallswarning").Value, null, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), "damageoutpostwalls".ToIdentifier(), 10f);
										someoneSpoke = true;
									}
								}
								if (character.IsCriminal || (accumulatedDamage > 20f && prevAccumulatedDamage <= 20f) || (accumulatedDamage > 50f && prevAccumulatedDamage <= 50f))
								{
									AIObjectiveCombat.CombatMode combatMode = (accumulatedDamage > 50f) ? AIObjectiveCombat.CombatMode.Offensive : AIObjectiveCombat.CombatMode.Arrest;
									if (combatMode == AIObjectiveCombat.CombatMode.Offensive)
									{
										character.IsCriminal = true;
										character.IsActingOffensively = true;
									}
									if (!HumanAIController.TriggerSecurity(otherHumanAI, character, combatMode))
									{
										IEnumerable<Character> characterList = Character.CharacterList;
										Func<Character, bool> predicate;
										Func<Character, bool> <>9__0;
										if ((predicate = <>9__0) == null)
										{
											predicate = (<>9__0 = ((Character c) => c.TeamID == otherCharacter.TeamID));
										}
										IEnumerable<Character> source = characterList.Where(predicate);
										Func<Character, float> keySelector;
										if ((keySelector = <>9__1) == null)
										{
											keySelector = (<>9__1 = ((Character c) => Vector2.DistanceSquared(character.WorldPosition, c.WorldPosition)));
										}
										foreach (Character security in source.OrderBy(keySelector))
										{
											if (!HumanAIController.TriggerSecurity(security.AIController as HumanAIController, character, combatMode))
											{
												return;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x00083304 File Offset: 0x00081504
		private static bool TriggerSecurity(HumanAIController humanAI, Character targetCharacter, AIObjectiveCombat.CombatMode combatMode)
		{
			if (humanAI == null)
			{
				return false;
			}
			if (!humanAI.Character.IsSecurity)
			{
				return false;
			}
			if (humanAI.ObjectiveManager.IsCurrentObjective<AIObjectiveCombat>())
			{
				return false;
			}
			humanAI.AddCombatObjective(combatMode, targetCharacter, HumanAIController.GetReactionTime(), null, null, delegate
			{
				foreach (Character anyCharacter in Character.CharacterList)
				{
					HumanAIController anyAI = anyCharacter.AIController as HumanAIController;
					if (anyAI != null)
					{
						Dictionary<Character, float> dictionary = anyAI.structureDamageAccumulator;
						if (dictionary != null)
						{
							dictionary.Remove(targetCharacter);
						}
					}
				}
			}, false, false);
			return true;
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x00083364 File Offset: 0x00081564
		public static void ItemTaken(Item item, Character thief)
		{
			HumanAIController.<>c__DisplayClass138_0 CS$<>8__locals1 = new HumanAIController.<>c__DisplayClass138_0();
			CS$<>8__locals1.thief = thief;
			CS$<>8__locals1.item = item;
			if (CS$<>8__locals1.item == null || CS$<>8__locals1.thief == null || CS$<>8__locals1.item.GetComponent<LevelResource>() != null)
			{
				return;
			}
			if (CS$<>8__locals1.thief.IsBot && CS$<>8__locals1.item.HasTag(AIObjectiveGetItem.AllowedItemsToTake))
			{
				return;
			}
			bool someoneSpoke = false;
			if (CS$<>8__locals1.item.Illegitimate)
			{
				Character itemOwner = CS$<>8__locals1.item.GetRootInventoryOwner() as Character;
				if (itemOwner != null && itemOwner != CS$<>8__locals1.thief && itemOwner.TeamID == CS$<>8__locals1.thief.TeamID)
				{
					CS$<>8__locals1.thief.IsCriminal = true;
				}
			}
			bool flag;
			if (!CS$<>8__locals1.item.Illegitimate)
			{
				ItemInventory ownInventory = CS$<>8__locals1.item.OwnInventory;
				object obj;
				if (ownInventory == null)
				{
					obj = null;
				}
				else
				{
					obj = ownInventory.FindItem((Item it) => it.Illegitimate, true);
				}
				flag = (obj != null);
			}
			else
			{
				flag = true;
			}
			bool foundIllegitimateItems = flag;
			if (foundIllegitimateItems && CS$<>8__locals1.thief.TeamID != CharacterTeamType.FriendlyNPC)
			{
				using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character otherCharacter = enumerator.Current;
						if (otherCharacter != CS$<>8__locals1.thief && otherCharacter.TeamID != CS$<>8__locals1.thief.TeamID && !otherCharacter.IsIncapacitated && otherCharacter.Stun <= 0f)
						{
							CharacterInfo info = otherCharacter.Info;
							if (((info != null) ? info.Job : null) != null)
							{
								HumanAIController otherHumanAI = otherCharacter.AIController as HumanAIController;
								if (otherHumanAI != null && !otherCharacter.IsEscorted && Vector2.DistanceSquared(otherCharacter.WorldPosition, CS$<>8__locals1.thief.WorldPosition) <= 1000000f && otherCharacter.CanSeeTarget(CS$<>8__locals1.thief, null, true, false))
								{
									if (CS$<>8__locals1.thief.Submarine != null)
									{
										List<Hull> connectedHulls = CS$<>8__locals1.thief.Submarine.GetHulls(true);
										if (CS$<>8__locals1.item.HasTag(Tags.FireExtinguisher))
										{
											if (connectedHulls.Any((Hull h) => h.FireSources.Any<FireSource>()))
											{
												continue;
											}
										}
										if (CS$<>8__locals1.item.HasTag(Tags.DivingGear))
										{
											IEnumerable<Hull> source = connectedHulls;
											Func<Hull, bool> predicate;
											if ((predicate = CS$<>8__locals1.<>9__3) == null)
											{
												predicate = (CS$<>8__locals1.<>9__3 = delegate(Hull h)
												{
													IEnumerable<Gap> connectedGaps = h.ConnectedGaps;
													Func<Gap, bool> predicate3;
													if ((predicate3 = CS$<>8__locals1.<>9__4) == null)
													{
														predicate3 = (CS$<>8__locals1.<>9__4 = ((Gap g) => AIObjectiveFixLeaks.IsValidTarget(g, CS$<>8__locals1.thief)));
													}
													return connectedGaps.Any(predicate3);
												});
											}
											if (source.Any(predicate))
											{
												continue;
											}
										}
									}
									if (!CS$<>8__locals1.item.HasTag(Tags.Handcuffs) || !CS$<>8__locals1.thief.HasEquippedItem(CS$<>8__locals1.item, null, null))
									{
										if (!CS$<>8__locals1.item.StolenDuringRound)
										{
											CS$<>8__locals1.item.StolenDuringRound = true;
											HumanAIController.ApplyStealingReputationLoss(CS$<>8__locals1.item);
										}
										if (!someoneSpoke)
										{
											otherCharacter.Speak(TextManager.Get("dialogstealwarning").Value, null, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), "thief".ToIdentifier(), 10f);
											someoneSpoke = true;
										}
										if (!CS$<>8__locals1.<ItemTaken>g__TriggerSecurity|1(otherHumanAI))
										{
											IEnumerable<Character> characterList = Character.CharacterList;
											Func<Character, bool> predicate2;
											Func<Character, bool> <>9__5;
											if ((predicate2 = <>9__5) == null)
											{
												predicate2 = (<>9__5 = ((Character c) => c.TeamID == otherCharacter.TeamID));
											}
											IEnumerable<Character> source2 = characterList.Where(predicate2);
											Func<Character, float> keySelector;
											if ((keySelector = CS$<>8__locals1.<>9__6) == null)
											{
												keySelector = (CS$<>8__locals1.<>9__6 = ((Character c) => Vector2.DistanceSquared(CS$<>8__locals1.thief.WorldPosition, c.WorldPosition)));
											}
											foreach (Character security in source2.OrderBy(keySelector))
											{
												if (CS$<>8__locals1.<ItemTaken>g__TriggerSecurity|1(security.AIController as HumanAIController))
												{
													break;
												}
											}
										}
									}
								}
							}
						}
					}
					return;
				}
			}
			ItemInventory ownInventory2 = CS$<>8__locals1.item.OwnInventory;
			Item item2;
			if (ownInventory2 == null)
			{
				item2 = null;
			}
			else
			{
				item2 = ownInventory2.FindItem((Item it) => it.Illegitimate, true);
			}
			Item foundItem = item2;
			if (foundItem != null)
			{
				HumanAIController.ItemTaken(foundItem, CS$<>8__locals1.thief);
			}
		}

		// Token: 0x06000D51 RID: 3409 RVA: 0x000837EC File Offset: 0x000819EC
		public static void ApplyStealingReputationLoss(Item item)
		{
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				GameSession gameSession = GameMain.GameSession;
				bool flag;
				if (gameSession == null)
				{
					flag = (null != null);
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					if (campaign == null)
					{
						flag = (null != null);
					}
					else
					{
						Map map = campaign.Map;
						flag = (((map != null) ? map.CurrentLocation : null) != null);
					}
				}
				if (flag)
				{
					float reputationLoss = MathHelper.Clamp((float)item.Prefab.GetMinPrice().GetValueOrDefault() * 0.0025f, 0.025f, 0.5f);
					Reputation reputation = GameMain.GameSession.Campaign.Map.CurrentLocation.Reputation;
					if (reputation == null)
					{
						return;
					}
					reputation.AddReputation(-reputationLoss, float.MaxValue);
				}
			}
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x0008388E File Offset: 0x00081A8E
		private static float GetReactionTime()
		{
			return 0.3f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x000838A8 File Offset: 0x00081AA8
		private static void PropagateHullSafety(Character character, Hull hull)
		{
			HumanAIController.DoForEachBot(character, delegate(HumanAIController humanAi)
			{
				humanAi.RefreshHullSafety(hull);
			}, float.PositiveInfinity);
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x000838D9 File Offset: 0x00081AD9
		public void AskToRecalculateHullSafety(Hull hull)
		{
			this.dirtyHullSafetyCalculations.Add(hull);
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x000838E8 File Offset: 0x00081AE8
		private void RefreshHullSafety(Hull hull)
		{
			IEnumerable<Hull> visibleHulls = this.dirtyHullSafetyCalculations.Contains(hull) ? hull.GetConnectedHulls(true, new int?(1), false) : null;
			float hullSafety = this.GetHullSafety(hull, this.Character, visibleHulls);
			if (hullSafety > 40f)
			{
				this.UnsafeHulls.Remove(hull);
				return;
			}
			this.UnsafeHulls.Add(hull);
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00083948 File Offset: 0x00081B48
		private static void RefreshTargets(Character character, Order order, Hull hull)
		{
			string a = order.Identifier.Value.ToLowerInvariant();
			if (!(a == "reportfire"))
			{
				if (!(a == "reportbreach"))
				{
					if (!(a == "reportbrokendevices"))
					{
						if (a == "reportintruders")
						{
							goto IL_123;
						}
						if (!(a == "requestfirstaid"))
						{
							return;
						}
						goto IL_171;
					}
				}
				else
				{
					using (List<Gap>.Enumerator enumerator = hull.ConnectedGaps.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Gap gap = enumerator.Current;
							if (AIObjectiveFixLeaks.IsValidTarget(gap, character))
							{
								HumanAIController.AddTargets<AIObjectiveFixLeaks, Gap>(character, gap);
							}
						}
						return;
					}
				}
				using (IEnumerator<Item> enumerator2 = Item.RepairableItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item item = enumerator2.Current;
						if (item.CurrentHull == hull && AIObjectiveRepairItems.IsValidTarget(item, character))
						{
							if (!item.Repairables.All((Repairable r) => r.IsBelowRepairThreshold))
							{
								HumanAIController.AddTargets<AIObjectiveRepairItems, Item>(character, item);
							}
						}
					}
					return;
				}
				IL_123:
				using (List<Character>.Enumerator enumerator3 = Character.CharacterList.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Character enemy = enumerator3.Current;
						if (enemy.CurrentHull == hull && AIObjectiveFightIntruders.IsValidTarget(enemy, character, false))
						{
							HumanAIController.AddTargets<AIObjectiveFightIntruders, Character>(character, enemy);
						}
					}
					return;
				}
				IL_171:
				foreach (Character c in Character.CharacterList)
				{
					bool flag;
					if (c.CurrentHull == hull && AIObjectiveRescueAll.IsValidTarget(c, character, out flag))
					{
						HumanAIController.AddTargets<AIObjectiveRescueAll, Character>(character, c);
					}
				}
				return;
			}
			HumanAIController.AddTargets<AIObjectiveExtinguishFires, Hull>(character, hull);
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x00083B4C File Offset: 0x00081D4C
		private static bool AddTargets<T1, T2>(Character caller, T2 target) where T1 : AIObjectiveLoop<T2>
		{
			bool targetAdded = false;
			Character caller2 = caller;
			Action<HumanAIController> action = delegate(HumanAIController humanAI)
			{
				if (caller != humanAI.Character && caller.SpeechImpediment >= 100f)
				{
					return;
				}
				T1 objective = humanAI.ObjectiveManager.GetObjective<T1>();
				if (objective != null && !targetAdded && objective.AddTarget(target))
				{
					targetAdded = true;
				}
			};
			HumanAIController humanAIController = caller.AIController as HumanAIController;
			HumanAIController.DoForEachBot(caller2, action, (humanAIController != null) ? humanAIController.ReportRange : float.PositiveInfinity);
			return targetAdded;
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x00083BB4 File Offset: 0x00081DB4
		public static void RemoveTargets<T1, T2>(Character caller, T2 target) where T1 : AIObjectiveLoop<T2>
		{
			HumanAIController.DoForEachBot(caller, delegate(HumanAIController humanAI)
			{
				T1 t = humanAI.ObjectiveManager.GetObjective<T1>();
				if (t == null)
				{
					return;
				}
				t.ReportedTargets.Remove(target);
			}, float.PositiveInfinity);
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x00083BE5 File Offset: 0x00081DE5
		private void StoreHullSafety(Hull hull, HumanAIController.HullSafety safety)
		{
			if (this.knownHulls.ContainsKey(hull))
			{
				this.knownHulls[hull] = safety;
				return;
			}
			this.knownHulls.Add(hull, safety);
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x00083C10 File Offset: 0x00081E10
		private float CalculateHullSafety(Hull hull, Character character, IEnumerable<Hull> visibleHulls = null)
		{
			bool isCurrentHull = character == this.Character && character.CurrentHull == hull;
			if (hull == null)
			{
				float hullSafety = (float)(character.IsProtectedFromPressure ? 0 : 100);
				if (isCurrentHull)
				{
					this.CurrentHullSafety = hullSafety;
				}
				return hullSafety;
			}
			if (isCurrentHull && visibleHulls == null)
			{
				visibleHulls = base.VisibleHulls;
			}
			AIObjective currentOrder = this.objectiveManager.CurrentOrder;
			bool ignoreFire = (currentOrder is AIObjectiveExtinguishFires && currentOrder.Priority > 0f) || this.objectiveManager.HasActiveObjective<AIObjectiveExtinguishFire>();
			bool ignoreOxygen = HumanAIController.HasDivingGear(character, 0f, true);
			bool ignoreEnemies = this.ObjectiveManager.HasObjectiveOrOrder<AIObjectiveFightIntruders>();
			float safety = HumanAIController.CalculateHullSafety(hull, visibleHulls, character, false, ignoreOxygen, ignoreFire, ignoreEnemies, false);
			if (isCurrentHull)
			{
				this.CurrentHullSafety = safety;
			}
			return safety;
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x00083CCA File Offset: 0x00081ECA
		public static float CalculateObjectiveHullSafety(Character character)
		{
			Hull currentHull = character.CurrentHull;
			AIController aicontroller = character.AIController;
			return HumanAIController.CalculateHullSafety(currentHull, ((aicontroller != null) ? aicontroller.VisibleHulls : null) ?? character.GetVisibleHulls(), character, false, false, false, false, true);
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x00083CFC File Offset: 0x00081EFC
		private static float CalculateHullSafety(Hull hull, IEnumerable<Hull> visibleHulls, Character character, bool ignoreWater = false, bool ignoreOxygen = false, bool ignoreFire = false, bool ignoreEnemies = false, bool ignorePressureProtection = false)
		{
			HumanAIController.<>c__DisplayClass150_0 CS$<>8__locals1 = new HumanAIController.<>c__DisplayClass150_0();
			CS$<>8__locals1.character = character;
			bool isProtectedFromPressure = CS$<>8__locals1.character.IsProtectedFromPressure;
			if (!ignorePressureProtection)
			{
				if (hull == null)
				{
					return (float)(isProtectedFromPressure ? 100 : 0);
				}
				if (hull.LethalPressure > 0f && !isProtectedFromPressure)
				{
					return 0f;
				}
			}
			else if (hull == null)
			{
				return 0f;
			}
			float oxygenFactor = ignoreOxygen ? 1f : MathHelper.Lerp(0.39f, 1f, MathUtils.InverseLerp(30f, 70f, hull.OxygenPercentage));
			float waterFactor = 1f;
			if (!ignoreWater && !isProtectedFromPressure)
			{
				if (visibleHulls != null)
				{
					float relativeWaterVolume = visibleHulls.Sum((Hull s) => s.WaterVolume) / visibleHulls.Sum((Hull s) => s.Volume);
					waterFactor = MathHelper.Lerp(1f, 0.2f, relativeWaterVolume);
				}
				else
				{
					float relativeWaterVolume2 = hull.WaterVolume / hull.Volume;
					waterFactor = MathHelper.Lerp(1f, 0.2f, relativeWaterVolume2);
				}
			}
			if (!ignoreOxygen && (!CS$<>8__locals1.character.NeedsOxygen || CS$<>8__locals1.character.CharacterHealth.OxygenLowResistance >= 1f))
			{
				oxygenFactor = 1f;
			}
			float fireFactor = 1f;
			if (!ignoreFire)
			{
				float fire = CS$<>8__locals1.<CalculateHullSafety>g__CalculateFire|3(hull) + hull.linkedTo.Sum((MapEntity e) => base.<CalculateHullSafety>g__CalculateFire|3(e as Hull));
				fireFactor = MathHelper.Lerp(1f, 0f, MathHelper.Clamp(fire, 0f, 1f));
			}
			float enemyFactor = 1f;
			if (!ignoreEnemies)
			{
				float enemyCount = 0f;
				foreach (Character c in Character.CharacterList)
				{
					float countModifier = 1f;
					if (c.CurrentHull != null)
					{
						if (visibleHulls == null)
						{
							if (c.CurrentHull != hull && !c.CurrentHull.linkedTo.Contains(hull))
							{
								continue;
							}
						}
						else
						{
							if (!visibleHulls.Contains(c.CurrentHull))
							{
								continue;
							}
							if (c.CurrentHull != hull && !c.CurrentHull.linkedTo.Contains(hull))
							{
								countModifier = 0.25f;
							}
						}
						if (HumanAIController.IsActive(c) && !HumanAIController.IsFriendly(CS$<>8__locals1.character, c, false, false) && !c.IsHandcuffed)
						{
							enemyCount += countModifier;
						}
					}
				}
				enemyFactor = MathHelper.Lerp(1f, 0f, MathHelper.Clamp(enemyCount * 0.9f, 0f, 1f));
			}
			float dangerousItemsFactor = 1f;
			foreach (Item item in Item.DangerousItems)
			{
				if (item.CurrentHull == hull)
				{
					dangerousItemsFactor = 0f;
					break;
				}
			}
			float safety = oxygenFactor * waterFactor * fireFactor * enemyFactor * dangerousItemsFactor;
			return MathHelper.Clamp(safety * 100f, 0f, 100f);
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x00084028 File Offset: 0x00082228
		public float GetHullSafety(Hull hull, Character character, IEnumerable<Hull> visibleHulls = null)
		{
			if (hull == null)
			{
				return this.CalculateHullSafety(null, character, visibleHulls);
			}
			HumanAIController.HullSafety hullSafety;
			if (!this.knownHulls.TryGetValue(hull, out hullSafety))
			{
				hullSafety = new HumanAIController.HullSafety(this.CalculateHullSafety(hull, character, visibleHulls));
				this.StoreHullSafety(hull, hullSafety);
			}
			else if (hullSafety.IsStale)
			{
				hullSafety.Reset(this.CalculateHullSafety(hull, character, visibleHulls));
			}
			return hullSafety.safety;
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x00084088 File Offset: 0x00082288
		public static float GetHullSafety(Hull hull, IEnumerable<Hull> visibleHulls, Character character, bool ignoreWater = false, bool ignoreOxygen = false, bool ignoreFire = false, bool ignoreEnemies = false)
		{
			if (hull == null)
			{
				return HumanAIController.CalculateHullSafety(null, visibleHulls, character, ignoreWater, ignoreOxygen, ignoreFire, ignoreEnemies, false);
			}
			HumanAIController controller = character.AIController as HumanAIController;
			if (controller != null)
			{
				HumanAIController.HullSafety hullSafety;
				if (!controller.knownHulls.TryGetValue(hull, out hullSafety))
				{
					hullSafety = new HumanAIController.HullSafety(HumanAIController.CalculateHullSafety(hull, visibleHulls, character, ignoreWater, ignoreOxygen, ignoreFire, ignoreEnemies, false));
					controller.StoreHullSafety(hull, hullSafety);
				}
				else if (hullSafety.IsStale)
				{
					hullSafety.Reset(HumanAIController.CalculateHullSafety(hull, visibleHulls, character, ignoreWater, ignoreOxygen, ignoreFire, ignoreEnemies, false));
				}
				return hullSafety.safety;
			}
			return HumanAIController.CalculateHullSafety(hull, visibleHulls, character, ignoreWater, ignoreOxygen, ignoreFire, ignoreEnemies, false);
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x00084120 File Offset: 0x00082320
		public static bool IsFriendly(Character me, Character other, bool onlySameTeam = false, bool ignoreHuskDisguising = false)
		{
			if (onlySameTeam)
			{
				ignoreHuskDisguising = true;
			}
			if (other.IsHusk && !ignoreHuskDisguising)
			{
				return me.IsDisguisedAsHusk;
			}
			if (other.IsPrisoner && me.IsPrisoner)
			{
				return true;
			}
			if (other.IsHostileEscortee && me.IsHostileEscortee)
			{
				return true;
			}
			bool sameTeam = me.TeamID == other.TeamID;
			if (!sameTeam && (onlySameTeam || !me.IsOnFriendlyTeam(other)))
			{
				return false;
			}
			if (other.IsPet)
			{
				return sameTeam || me.TeamID > CharacterTeamType.None;
			}
			if (!me.IsSameSpeciesOrGroup(other))
			{
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && (me.CampaignInteractionType == CampaignMode.InteractionType.None || CampaignMode.HostileFactionDisablesInteraction(me.CampaignInteractionType)) && ((me.TeamID == CharacterTeamType.FriendlyNPC && other.TeamID == CharacterTeamType.Team1) || (me.TeamID == CharacterTeamType.Team1 && other.TeamID == CharacterTeamType.FriendlyNPC)))
			{
				Character npc = (me.TeamID == CharacterTeamType.FriendlyNPC) ? me : other;
				HumanAIController npcAI = npc.AIController as HumanAIController;
				if (npcAI != null)
				{
					return !npcAI.IsInHostileFaction();
				}
			}
			return true;
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x0008422C File Offset: 0x0008242C
		public bool IsInHostileFaction()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign == null)
			{
				return false;
			}
			if (this.Character.IsEscorted)
			{
				return false;
			}
			Identifier npcFaction = this.Character.Faction;
			Map map = campaign.Map;
			Identifier? identifier;
			if (map == null)
			{
				identifier = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					identifier = null;
				}
				else
				{
					Faction faction = currentLocation.Faction;
					identifier = ((faction != null) ? new Identifier?(faction.Prefab.Identifier) : null);
				}
			}
			Identifier currentLocationFaction = identifier ?? Identifier.Empty;
			if (npcFaction.IsEmpty)
			{
				npcFaction = currentLocationFaction;
			}
			if (!currentLocationFaction.IsEmpty && npcFaction == currentLocationFaction)
			{
				Location currentLocation2 = campaign.CurrentLocation;
				if (currentLocation2 != null && currentLocation2.IsFactionHostile)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x0008430F File Offset: 0x0008250F
		public static bool IsActive(Character c)
		{
			return c != null && c.Enabled && !c.IsUnconscious;
		}

		// Token: 0x06000D62 RID: 3426 RVA: 0x00084328 File Offset: 0x00082528
		public static bool IsTrueForAllBotsInTheCrew(Character character, Func<HumanAIController, bool> predicate)
		{
			if (character == null)
			{
				return false;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsBotInTheCrew(character, c) && !predicate(c.AIController as HumanAIController))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x0008439C File Offset: 0x0008259C
		public static bool IsTrueForAnyBotInTheCrew(Character character, Func<HumanAIController, bool> predicate)
		{
			if (character == null)
			{
				return false;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsBotInTheCrew(character, c) && predicate(c.AIController as HumanAIController))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x00084410 File Offset: 0x00082610
		public static int CountBotsInTheCrew(Character character, Func<HumanAIController, bool> predicate = null)
		{
			if (character == null)
			{
				return 0;
			}
			int count = 0;
			foreach (Character other in Character.CharacterList)
			{
				if (HumanAIController.IsBotInTheCrew(character, other) && (predicate == null || predicate(other.AIController as HumanAIController)))
				{
					count++;
				}
			}
			return count;
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x00084488 File Offset: 0x00082688
		public bool IsTrueForAnyCrewMember(Func<Character, bool> predicate, bool onlyActive = true, bool onlyConnectedSubs = false)
		{
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsActive(c) && c.TeamID == this.Character.TeamID && (!onlyActive || !c.IsIncapacitated))
				{
					if (onlyConnectedSubs)
					{
						if (this.Character.Submarine == null)
						{
							if (c.Submarine != null)
							{
								return false;
							}
						}
						else if (c.Submarine != this.Character.Submarine && !this.Character.Submarine.GetConnectedSubs().Contains(c.Submarine))
						{
							return false;
						}
					}
					if (predicate(c))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x00084560 File Offset: 0x00082760
		private static void DoForEachBot(Character character, Action<HumanAIController> action, float range = float.PositiveInfinity)
		{
			if (character == null)
			{
				return;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsBotInTheCrew(character, c) && HumanAIController.CheckReportRange(character, c, range))
				{
					action(c.AIController as HumanAIController);
				}
			}
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x000845D4 File Offset: 0x000827D4
		private static bool CheckReportRange(Character character, Character target, float range)
		{
			if (float.IsPositiveInfinity(range))
			{
				return true;
			}
			if (character.CurrentHull == null || target.CurrentHull == null)
			{
				return Vector2.DistanceSquared(character.WorldPosition, target.WorldPosition) <= range * range;
			}
			return character.CurrentHull.GetApproximateDistance(character.Position, target.Position, target.CurrentHull, range, 2f, 0.5f) <= range;
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x00084643 File Offset: 0x00082843
		private static bool IsBotInTheCrew(Character self, Character other)
		{
			return HumanAIController.IsActive(other) && other.TeamID == self.TeamID && !other.IsIncapacitated && other.IsBot && other.AIController is HumanAIController;
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x0008467C File Offset: 0x0008287C
		public static bool IsItemTargetedBySomeone(ItemComponent target, CharacterTeamType team, out Character operatingCharacter)
		{
			operatingCharacter = null;
			if (((target != null) ? target.Item : null) == null)
			{
				return false;
			}
			float highestPriority = -1f;
			float highestPriorityModifier = -1f;
			foreach (Character c in Character.CharacterList)
			{
				if (c != null && !c.Removed && c.TeamID == team && !c.IsIncapacitated)
				{
					if (c.SelectedItem == target.Item)
					{
						operatingCharacter = c;
						return true;
					}
					HumanAIController humanAIController = c.AIController as HumanAIController;
					if (humanAIController != null)
					{
						AIObjectiveManager objectiveManager = humanAIController.ObjectiveManager;
						if (objectiveManager != null)
						{
							foreach (AIObjective objective in objectiveManager.Objectives)
							{
								AIObjectiveOperateItem operateObjective = objective as AIObjectiveOperateItem;
								if (operateObjective != null)
								{
									ItemComponent component = operateObjective.Component;
									if (((component != null) ? component.Item : null) == target.Item && operateObjective.Priority >= highestPriority && operateObjective.PriorityModifier >= highestPriorityModifier)
									{
										operatingCharacter = c;
										highestPriority = operateObjective.Priority;
										highestPriorityModifier = operateObjective.PriorityModifier;
									}
								}
							}
						}
					}
				}
			}
			return operatingCharacter != null;
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x00084800 File Offset: 0x00082A00
		public bool IsFriendly(Character other, bool onlySameTeam = false)
		{
			return HumanAIController.IsFriendly(this.Character, other, onlySameTeam, false);
		}

		// Token: 0x06000D6B RID: 3435 RVA: 0x00084810 File Offset: 0x00082A10
		public bool IsTrueForAnyBotInTheCrew(Func<HumanAIController, bool> predicate)
		{
			return HumanAIController.IsTrueForAnyBotInTheCrew(this.Character, predicate);
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x0008481E File Offset: 0x00082A1E
		public bool IsTrueForAllBotsInTheCrew(Func<HumanAIController, bool> predicate)
		{
			return HumanAIController.IsTrueForAllBotsInTheCrew(this.Character, predicate);
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x0008482C File Offset: 0x00082A2C
		public int CountBotsInTheCrew(Func<HumanAIController, bool> predicate = null)
		{
			return HumanAIController.CountBotsInTheCrew(this.Character, predicate);
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x00084850 File Offset: 0x00082A50
		[CompilerGenerated]
		private bool <Update>g__IsCloseEnoughToTarget|97_0(float threshold, bool targetSub = true)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			Entity target = (selectedAiTarget != null) ? selectedAiTarget.Entity : null;
			if (target == null)
			{
				return false;
			}
			if (targetSub)
			{
				Submarine sub = target.Submarine;
				if (sub == null)
				{
					return false;
				}
				target = sub;
				threshold += (float)(Math.Max(sub.Borders.Size.X, sub.Borders.Size.Y) / 2);
			}
			return Vector2.DistanceSquared(this.Character.WorldPosition, target.WorldPosition) < MathUtils.Pow2(threshold);
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x000848D8 File Offset: 0x00082AD8
		[CompilerGenerated]
		private bool <UnequipUnnecessaryItems>g__NeedsDivingGearOnPath|99_0(AIObjectiveGoTo gotoObjective)
		{
			bool insideSteering = base.SteeringManager == this.PathSteering && this.PathSteering.CurrentPath != null && !this.PathSteering.IsPathDirty;
			Hull targetHull = gotoObjective.GetTargetHull();
			bool flag;
			return (gotoObjective.Target != null && targetHull == null && !this.Character.IsImmuneToPressure) || this.NeedsDivingGear(targetHull, out flag, null) || (insideSteering && ((this.PathSteering.CurrentPath.HasOutdoorsNodes && !this.Character.IsImmuneToPressure) || this.PathSteering.CurrentPath.Nodes.Any(delegate(WayPoint n)
			{
				bool flag2;
				return this.NeedsDivingGear(n.CurrentHull, out flag2, null);
			})));
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x000849A4 File Offset: 0x00082BA4
		[CompilerGenerated]
		private bool <UnequipUnnecessaryItems>g__IsOrderedToWait|99_4()
		{
			if (this.Character.IsOnPlayerTeam)
			{
				AIObjectiveGoTo aiobjectiveGoTo = this.ObjectiveManager.CurrentOrder as AIObjectiveGoTo;
				return aiobjectiveGoTo != null && aiobjectiveGoTo.IsWaitOrder;
			}
			return false;
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x000849E4 File Offset: 0x00082BE4
		[CompilerGenerated]
		internal static void <ReportProblem>g__Report|115_0(Hull hull, ref HumanAIController.<>c__DisplayClass115_0 A_1)
		{
			HumanAIController.PropagateHullSafety(A_1.reporter, hull);
			HumanAIController.RefreshTargets(A_1.reporter, A_1.order, hull);
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x00084A04 File Offset: 0x00082C04
		[CompilerGenerated]
		private AIObjectiveCombat <AddCombatObjective>g__CreateCombatObjective|121_0(ref HumanAIController.<>c__DisplayClass121_0 A_1)
		{
			AIObjectiveCombat objective = new AIObjectiveCombat(this.Character, A_1.target, A_1.mode, this.objectiveManager, 1f, 10f)
			{
				AbortCondition = A_1.abortCondition,
				AllowHoldFire = A_1.allowHoldFire,
				SpeakWarnings = A_1.speakWarnings
			};
			if (A_1.onAbort != null)
			{
				objective.Abandoned += A_1.onAbort;
			}
			if (A_1.onCompleted != null)
			{
				objective.Completed += A_1.onCompleted;
			}
			return objective;
		}

		// Token: 0x04000604 RID: 1540
		public static bool DebugAI;

		// Token: 0x04000605 RID: 1541
		public static bool DisableCrewAI;

		// Token: 0x04000606 RID: 1542
		private readonly AIObjectiveManager objectiveManager;

		// Token: 0x04000608 RID: 1544
		private float crouchRaycastTimer;

		// Token: 0x04000609 RID: 1545
		private float reactTimer;

		// Token: 0x0400060A RID: 1546
		private float unreachableClearTimer;

		// Token: 0x0400060B RID: 1547
		private bool shouldCrouch;

		// Token: 0x0400060C RID: 1548
		public bool AutoFaceMovement = true;

		// Token: 0x0400060D RID: 1549
		private const float reactionTime = 0.3f;

		// Token: 0x0400060E RID: 1550
		private const float crouchRaycastInterval = 1f;

		// Token: 0x0400060F RID: 1551
		private const float sortObjectiveInterval = 1f;

		// Token: 0x04000610 RID: 1552
		private const float clearUnreachableInterval = 30f;

		// Token: 0x04000611 RID: 1553
		private float flipTimer;

		// Token: 0x04000612 RID: 1554
		private const float FlipInterval = 0.5f;

		// Token: 0x04000613 RID: 1555
		public const float HULL_SAFETY_THRESHOLD = 40f;

		// Token: 0x04000614 RID: 1556
		public const float HULL_LOW_OXYGEN_PERCENTAGE = 30f;

		// Token: 0x04000615 RID: 1557
		private static readonly float characterWaitOnSwitch = 5f;

		// Token: 0x04000616 RID: 1558
		public readonly HashSet<Hull> UnreachableHulls = new HashSet<Hull>();

		// Token: 0x04000617 RID: 1559
		public readonly HashSet<Hull> UnsafeHulls = new HashSet<Hull>();

		// Token: 0x04000618 RID: 1560
		public readonly List<Item> IgnoredItems = new List<Item>();

		// Token: 0x04000619 RID: 1561
		private readonly HashSet<Hull> dirtyHullSafetyCalculations = new HashSet<Hull>();

		// Token: 0x0400061A RID: 1562
		private float respondToAttackTimer;

		// Token: 0x0400061B RID: 1563
		private const float RespondToAttackInterval = 1f;

		// Token: 0x0400061C RID: 1564
		private bool wasDead;

		// Token: 0x0400061D RID: 1565
		private bool freezeAI;

		// Token: 0x0400061E RID: 1566
		private readonly float maxSteeringBuffer = 5000f;

		// Token: 0x0400061F RID: 1567
		private readonly float minSteeringBuffer = 500f;

		// Token: 0x04000620 RID: 1568
		private readonly float steeringBufferIncreaseSpeed = 100f;

		// Token: 0x04000621 RID: 1569
		private float steeringBuffer;

		// Token: 0x04000622 RID: 1570
		private readonly float obstacleRaycastIntervalShort = 1f;

		// Token: 0x04000623 RID: 1571
		private readonly float obstacleRaycastIntervalLong = 5f;

		// Token: 0x04000624 RID: 1572
		private float obstacleRaycastTimer;

		// Token: 0x04000625 RID: 1573
		private bool isBlocked;

		// Token: 0x04000626 RID: 1574
		private readonly float enemyCheckInterval = 0.2f;

		// Token: 0x04000627 RID: 1575
		private readonly float enemySpotDistanceOutside = 800f;

		// Token: 0x04000628 RID: 1576
		private readonly float enemySpotDistanceInside = 1000f;

		// Token: 0x04000629 RID: 1577
		private float enemyCheckTimer;

		// Token: 0x0400062A RID: 1578
		private readonly float reportProblemsInterval = 1f;

		// Token: 0x0400062B RID: 1579
		private float reportProblemsTimer;

		// Token: 0x0400062F RID: 1583
		private float _aimSpeed = 1f;

		// Token: 0x04000630 RID: 1584
		private float _aimAccuracy = 1f;

		// Token: 0x04000631 RID: 1585
		private readonly Dictionary<Character, AttackResult> previousAttackResults = new Dictionary<Character, AttackResult>();

		// Token: 0x04000632 RID: 1586
		private readonly Dictionary<Character, float> previousHealAmounts = new Dictionary<Character, float>();

		// Token: 0x04000633 RID: 1587
		private readonly SteeringManager outsideSteering;

		// Token: 0x04000634 RID: 1588
		private readonly SteeringManager insideSteering;

		// Token: 0x04000637 RID: 1591
		private readonly Dictionary<Character, float> structureDamageAccumulator = new Dictionary<Character, float>();

		// Token: 0x04000638 RID: 1592
		private readonly Dictionary<Hull, HumanAIController.HullSafety> knownHulls = new Dictionary<Hull, HumanAIController.HullSafety>();

		// Token: 0x0400063B RID: 1595
		private readonly HashSet<Item> itemsToRelocate = new HashSet<Item>();

		// Token: 0x0400063C RID: 1596
		private HumanAIController.FindItemState findItemState;

		// Token: 0x0400063D RID: 1597
		private int itemIndex;

		// Token: 0x0400063E RID: 1598
		private float draggedTimer;

		// Token: 0x0400063F RID: 1599
		private float refuseDraggingTimer;

		// Token: 0x04000640 RID: 1600
		private const float RefuseDraggingThresholdHigh = 10f;

		// Token: 0x04000641 RID: 1601
		private const float RefuseDraggingThresholdLow = 0.5f;

		// Token: 0x04000642 RID: 1602
		private const float RefuseDraggingDuration = 30f;

		// Token: 0x04000643 RID: 1603
		private static List<Item> matchingItems = new List<Item>();

		// Token: 0x0200077C RID: 1916
		private class HullSafety
		{
			// Token: 0x1700143C RID: 5180
			// (get) Token: 0x06005242 RID: 21058 RVA: 0x001EAEEB File Offset: 0x001E90EB
			public bool IsStale
			{
				get
				{
					return this.timer <= 0f;
				}
			}

			// Token: 0x06005243 RID: 21059 RVA: 0x001EAEFD File Offset: 0x001E90FD
			public HullSafety(float safety)
			{
				this.Reset(safety);
			}

			// Token: 0x06005244 RID: 21060 RVA: 0x001EAF0C File Offset: 0x001E910C
			public void Reset(float safety)
			{
				this.safety = safety;
				this.timer = 0.5f;
			}

			// Token: 0x06005245 RID: 21061 RVA: 0x001EAF20 File Offset: 0x001E9120
			public bool Update(float deltaTime)
			{
				this.timer = Math.Max(this.timer - deltaTime, 0f);
				return this.IsStale;
			}

			// Token: 0x04002D21 RID: 11553
			public float safety;

			// Token: 0x04002D22 RID: 11554
			public float timer;
		}

		// Token: 0x0200077D RID: 1917
		private enum FindItemState
		{
			// Token: 0x04002D24 RID: 11556
			None,
			// Token: 0x04002D25 RID: 11557
			DivingSuit,
			// Token: 0x04002D26 RID: 11558
			DivingMask,
			// Token: 0x04002D27 RID: 11559
			OtherItem
		}
	}
}
