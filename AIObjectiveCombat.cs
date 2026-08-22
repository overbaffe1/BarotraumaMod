using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016F RID: 367
	internal class AIObjectiveCombat : AIObjective
	{
		// Token: 0x17000AEC RID: 2796
		// (get) Token: 0x06002B6E RID: 11118 RVA: 0x001DE21C File Offset: 0x001DC41C
		// (set) Token: 0x06002B6F RID: 11119 RVA: 0x001DE224 File Offset: 0x001DC424
		public override Identifier Identifier { get; set; } = "combat".ToIdentifier();

		// Token: 0x17000AED RID: 2797
		// (get) Token: 0x06002B70 RID: 11120 RVA: 0x001DE230 File Offset: 0x001DC430
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<AIObjectiveCombat.CombatMode>(this.Mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000AEE RID: 2798
		// (get) Token: 0x06002B71 RID: 11121 RVA: 0x001DE27F File Offset: 0x001DC47F
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AEF RID: 2799
		// (get) Token: 0x06002B72 RID: 11122 RVA: 0x001DE282 File Offset: 0x001DC482
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AF0 RID: 2800
		// (get) Token: 0x06002B73 RID: 11123 RVA: 0x001DE285 File Offset: 0x001DC485
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AF1 RID: 2801
		// (get) Token: 0x06002B74 RID: 11124 RVA: 0x001DE288 File Offset: 0x001DC488
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AF2 RID: 2802
		// (get) Token: 0x06002B75 RID: 11125 RVA: 0x001DE28B File Offset: 0x001DC48B
		private bool AllowCoolDown
		{
			get
			{
				return this.allowCooldown || !this.IsOffensiveOrArrest || this.Mode != this.initialMode || this.character.TeamID == this.Enemy.TeamID;
			}
		}

		// Token: 0x17000AF3 RID: 2803
		// (get) Token: 0x06002B76 RID: 11126 RVA: 0x001DE2C5 File Offset: 0x001DC4C5
		// (set) Token: 0x06002B77 RID: 11127 RVA: 0x001DE2CD File Offset: 0x001DC4CD
		public Character Enemy { get; private set; }

		// Token: 0x17000AF4 RID: 2804
		// (get) Token: 0x06002B78 RID: 11128 RVA: 0x001DE2D6 File Offset: 0x001DC4D6
		// (set) Token: 0x06002B79 RID: 11129 RVA: 0x001DE2DE File Offset: 0x001DC4DE
		public Item Weapon
		{
			get
			{
				return this._weapon;
			}
			set
			{
				this._weapon = value;
				this._weaponComponent = null;
			}
		}

		// Token: 0x17000AF5 RID: 2805
		// (get) Token: 0x06002B7A RID: 11130 RVA: 0x001DE2EE File Offset: 0x001DC4EE
		private ItemComponent WeaponComponent
		{
			get
			{
				if (this.Weapon == null)
				{
					return null;
				}
				return this._weaponComponent ?? AIObjectiveCombat.GetWeaponComponent(this.Weapon);
			}
		}

		// Token: 0x17000AF6 RID: 2806
		// (get) Token: 0x06002B7B RID: 11131 RVA: 0x001DE30F File Offset: 0x001DC50F
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AF7 RID: 2807
		// (get) Token: 0x06002B7C RID: 11132 RVA: 0x001DE312 File Offset: 0x001DC512
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AF8 RID: 2808
		// (get) Token: 0x06002B7D RID: 11133 RVA: 0x001DE315 File Offset: 0x001DC515
		// (set) Token: 0x06002B7E RID: 11134 RVA: 0x001DE31D File Offset: 0x001DC51D
		public float ArrestHoldFireTime { get; set; } = 10f;

		// Token: 0x17000AF9 RID: 2809
		// (get) Token: 0x06002B7F RID: 11135 RVA: 0x001DE326 File Offset: 0x001DC526
		// (set) Token: 0x06002B80 RID: 11136 RVA: 0x001DE32E File Offset: 0x001DC52E
		public AIObjectiveCombat.CombatMode Mode { get; private set; }

		// Token: 0x17000AFA RID: 2810
		// (get) Token: 0x06002B81 RID: 11137 RVA: 0x001DE338 File Offset: 0x001DC538
		private bool IsOffensiveOrArrest
		{
			get
			{
				AIObjectiveCombat.CombatMode combatMode = this.initialMode;
				return combatMode - AIObjectiveCombat.CombatMode.Offensive <= 1;
			}
		}

		// Token: 0x17000AFB RID: 2811
		// (get) Token: 0x06002B82 RID: 11138 RVA: 0x001DE35C File Offset: 0x001DC55C
		private bool TargetEliminated
		{
			get
			{
				return this.IsEnemyDisabled || (this.Enemy.IsUnconscious && this.Enemy.Params.Health.ConstantHealthRegeneration <= 0f) || (!this.character.IsInstigator && this.Enemy.IsHandcuffed && this.Enemy.IsKnockedDown);
			}
		}

		// Token: 0x17000AFC RID: 2812
		// (get) Token: 0x06002B83 RID: 11139 RVA: 0x001DE3C3 File Offset: 0x001DC5C3
		private bool IsEnemyDisabled
		{
			get
			{
				return this.Enemy == null || this.Enemy.Removed || this.Enemy.IsDead;
			}
		}

		// Token: 0x17000AFD RID: 2813
		// (get) Token: 0x06002B84 RID: 11140 RVA: 0x001DE3E7 File Offset: 0x001DC5E7
		private float AimSpeed
		{
			get
			{
				return base.HumanAIController.AimSpeed;
			}
		}

		// Token: 0x17000AFE RID: 2814
		// (get) Token: 0x06002B85 RID: 11141 RVA: 0x001DE3F4 File Offset: 0x001DC5F4
		private float AimAccuracy
		{
			get
			{
				return base.HumanAIController.AimAccuracy;
			}
		}

		// Token: 0x06002B86 RID: 11142 RVA: 0x001DE404 File Offset: 0x001DC604
		private bool IsEnemyClose(float margin)
		{
			if (this.Enemy == null)
			{
				return false;
			}
			Vector2 toEnemy = this.Enemy.WorldPosition - this.character.WorldPosition;
			if (this.character.CurrentHull != null && this.Enemy.CurrentHull != null && this.character.CurrentHull != this.Enemy.CurrentHull)
			{
				if (Math.Abs(toEnemy.Y) > 100f)
				{
					return false;
				}
				if (base.HumanAIController.VisibleHulls.Contains(this.Enemy.CurrentHull))
				{
					return Math.Abs(toEnemy.X) < margin;
				}
			}
			return Vector2.DistanceSquared(this.character.WorldPosition, this.Enemy.WorldPosition) < margin * margin;
		}

		// Token: 0x06002B87 RID: 11143 RVA: 0x001DE4CC File Offset: 0x001DC6CC
		public AIObjectiveCombat(Character character, Character enemy, AIObjectiveCombat.CombatMode mode, AIObjectiveManager objectiveManager, float priorityModifier = 1f, float coolDown = 10f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Enemy = enemy;
			this.coolDownTimer = coolDown;
			this.findSafety = objectiveManager.GetObjective<AIObjectiveFindSafety>();
			if (this.findSafety != null)
			{
				this.findSafety.Priority = 0f;
				base.HumanAIController.UnreachableHulls.Clear();
			}
			this.Mode = mode;
			this.initialMode = this.Mode;
			if (this.Enemy == null)
			{
				this.Mode = AIObjectiveCombat.CombatMode.Retreat;
			}
			this.spreadTimer = Rand.Range(-10f, 10f, Rand.RandSync.Unsynced);
			this.SetAimTimer(Rand.Range(1f, 1.5f, Rand.RandSync.Unsynced) / this.AimSpeed);
			base.HumanAIController.SortTimer = 0f;
		}

		// Token: 0x06002B88 RID: 11144 RVA: 0x001DE5C8 File Offset: 0x001DC7C8
		protected override float GetPriority()
		{
			if (this.TargetEliminated)
			{
				base.Priority = 0f;
				return base.Priority;
			}
			float xDist = Math.Abs(this.character.WorldPosition.X - this.Enemy.WorldPosition.X);
			float yDist = Math.Abs(this.character.WorldPosition.Y - this.Enemy.WorldPosition.Y);
			if (base.HumanAIController.VisibleHulls.Contains(this.Enemy.CurrentHull))
			{
				xDist /= 2f;
				yDist /= 2f;
			}
			float distanceFactor = MathUtils.InverseLerp(3000f, 0f, xDist + yDist * 5f);
			float devotion = base.CumulatedDevotion / 100f;
			float additionalPriority = MathHelper.Lerp(0f, 9f, Math.Clamp(devotion + distanceFactor, 0f, 1f));
			base.Priority = Math.Min((91f + additionalPriority) * base.PriorityModifier, 100f);
			if (base.Priority > 0f && EnemyAIController.IsLatchedToSomeoneElse(this.Enemy, this.character))
			{
				base.Priority = 0f;
			}
			return base.Priority;
		}

		// Token: 0x06002B89 RID: 11145 RVA: 0x001DE704 File Offset: 0x001DC904
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			this.isAimBlocked = false;
			this.ignoreWeaponTimer -= deltaTime;
			this.checkWeaponsTimer -= deltaTime;
			if (this.reloadTimer > 0f)
			{
				this.reloadTimer -= deltaTime;
			}
			if (this.ignoreWeaponTimer < 0f)
			{
				this.ignoredWeapons.Clear();
				this.ignoreWeaponTimer = 10f;
			}
			bool isFightingIntruders = this.objectiveManager.IsCurrentObjective<AIObjectiveFightIntruders>();
			if (this.findSafety != null && isFightingIntruders)
			{
				this.findSafety.Priority = 0f;
			}
			if (!this.AllowCoolDown && !this.character.IsOnPlayerTeam && !isFightingIntruders)
			{
				this.distanceTimer -= deltaTime;
				if (this.distanceTimer < 0f)
				{
					this.distanceTimer = 0.2f;
					this.sqrDistance = Vector2.DistanceSquared(this.character.WorldPosition, this.Enemy.WorldPosition);
				}
			}
		}

		// Token: 0x06002B8A RID: 11146 RVA: 0x001DE804 File Offset: 0x001DCA04
		protected override bool CheckObjectiveState()
		{
			Submarine submarine = this.character.Submarine;
			if (submarine != null)
			{
				SubmarineInfo info = submarine.Info;
				if (info != null && info.IsOutpost && this.character.IsOnFriendlyTeam(this.character.Submarine.TeamID) && this.character.Submarine == this.Enemy.Submarine)
				{
					if (this.character.TeamID == CharacterTeamType.FriendlyNPC && !this.character.IsSecurity)
					{
						this.allowCooldown = true;
						goto IL_299;
					}
					goto IL_299;
				}
			}
			if ((this.Enemy.Submarine == null && this.character.Submarine != null) || this.sqrDistance > 4000000f)
			{
				base.Abandon = true;
				if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.IsOffensiveOrArrest)
				{
					this.Enemy.IsCriminal = true;
				}
				return false;
			}
			if (this.Enemy.Submarine != null && this.character.Submarine != null && this.character.TeamID == CharacterTeamType.FriendlyNPC && this.Enemy.Submarine.TeamID != this.character.TeamID)
			{
				this.allowCooldown = true;
				if (this.character.Submarine.IsConnectedTo(this.Enemy.Submarine) && this.character.CanSeeTarget(this.Enemy, null, false, false))
				{
					this.allowCooldown = false;
					this.coolDownTimer = 10f;
				}
				else if (this.pathBackTimer <= 0f)
				{
					this.pathBackTimer = 1f;
					foreach (KeyValuePair<Submarine, DockingPort> keyValuePair in this.character.Submarine.ConnectedDockingPorts)
					{
						DockingPort dockingPort2;
						keyValuePair.Deconstruct(out submarine, out dockingPort2);
						Submarine sub = submarine;
						DockingPort dockingPort = dockingPort2;
						if (sub.TeamID == this.character.TeamID)
						{
							SteeringPath path = base.PathSteering.PathFinder.FindPath(this.character.SimPosition, this.character.GetRelativeSimPosition(dockingPort.Item, null), this.character.Submarine, null, 0f, null, null, (PathNode node) => node.Waypoint.CurrentHull != null, true, 0f);
							if (path.Unreachable)
							{
								this.allowCooldown = false;
								this.coolDownTimer = 10f;
							}
						}
					}
				}
				if (this.IsOffensiveOrArrest)
				{
					this.Enemy.IsCriminal = true;
				}
			}
			IL_299:
			return this.TargetEliminated || (this.AllowCoolDown && this.coolDownTimer <= 0f);
		}

		// Token: 0x06002B8B RID: 11147 RVA: 0x001DEAE0 File Offset: 0x001DCCE0
		protected override void Act(float deltaTime)
		{
			if (this.IsEnemyDisabled)
			{
				base.IsCompleted = true;
				return;
			}
			if (this.AllowCoolDown)
			{
				this.coolDownTimer -= deltaTime;
				if (this.pathBackTimer > 0f)
				{
					this.pathBackTimer -= deltaTime;
				}
			}
			if (this.standUpTimer > 0f)
			{
				this.standUpTimer -= deltaTime;
			}
			else
			{
				this.allowCrouching = true;
			}
			if (HumanAIController.DebugAI)
			{
				if (this.BlockedPositions == null)
				{
					this.BlockedPositions = new List<Vector2>();
				}
				this.BlockedPositions.Clear();
			}
			if (this.seekAmmunitionObjective == null && this.seekWeaponObjective == null)
			{
				if (this.Mode != AIObjectiveCombat.CombatMode.Retreat && this.TryArm())
				{
					this.OperateWeapon(deltaTime);
				}
				this.isMoving = false;
				if (this.seekAmmunitionObjective == null && this.seekWeaponObjective == null)
				{
					this.Move(deltaTime);
				}
			}
		}

		// Token: 0x06002B8C RID: 11148 RVA: 0x001DEBC0 File Offset: 0x001DCDC0
		private void Move(float deltaTime)
		{
			AIObjectiveCombat.<>c__DisplayClass101_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (this.Mode == AIObjectiveCombat.CombatMode.Retreat)
			{
				this.Retreat(CS$<>8__locals1.deltaTime);
				return;
			}
			if (this.character.IsOnPlayerTeam && !this.Enemy.IsPlayer)
			{
				AIObjectiveGoTo gotoObjective = this.objectiveManager.CurrentOrder as AIObjectiveGoTo;
				if (gotoObjective != null)
				{
					if (gotoObjective.IsWaitOrder && this.WeaponComponent is MeleeWeapon && this.IsEnemyClose(300f))
					{
						this.Engage(CS$<>8__locals1.deltaTime);
						return;
					}
					if (!gotoObjective.IsCloseEnough)
					{
						this.isMoving = true;
					}
					gotoObjective.FaceTargetOnCompleted = false;
					gotoObjective.ForceAct(CS$<>8__locals1.deltaTime);
					if (this.character.AnimController.InWater || !this.IsEnemyClose(300f))
					{
						return;
					}
					base.HumanAIController.FaceTarget(this.Enemy);
					base.HumanAIController.AutoFaceMovement = false;
					if (!gotoObjective.ShouldRun(true))
					{
						base.ForceWalkTemporarily = true;
						return;
					}
					return;
				}
			}
			AIObjectiveCombat.CombatMode mode = this.Mode;
			if (mode == AIObjectiveCombat.CombatMode.Defensive)
			{
				this.Retreat(CS$<>8__locals1.deltaTime);
				return;
			}
			if (mode == AIObjectiveCombat.CombatMode.Offensive)
			{
				if (this.hasValidRangedWeapon && this.IsEnemyClose(300f))
				{
					Hull currentHull = this.character.CurrentHull;
					bool backOff = currentHull != null;
					AIObjectiveCombat.<>c__DisplayClass101_1 CS$<>8__locals2;
					CS$<>8__locals2.escapeVel = Vector2.Zero;
					if (backOff)
					{
						int previousEnemyDir = 0;
						foreach (Character enemy in Character.CharacterList)
						{
							if (HumanAIController.IsActive(enemy) && !base.HumanAIController.IsFriendly(enemy, false) && !enemy.IsHandcuffed && enemy.CurrentHull != null && (currentHull == enemy.CurrentHull || currentHull.linkedTo.Contains(enemy.CurrentHull)))
							{
								Vector2 dir = this.character.Position - enemy.Position;
								int enemyDir = Math.Sign(dir.X);
								if (enemyDir == 0)
								{
									if (previousEnemyDir != 0)
									{
										continue;
									}
									enemyDir = ((Rand.Value(Rand.RandSync.Unsynced) > 0.5f) ? 1 : -1);
								}
								if (previousEnemyDir != 0 && enemyDir != previousEnemyDir)
								{
									backOff = false;
									break;
								}
								previousEnemyDir = enemyDir;
								float distMultiplier = MathHelper.Clamp(125f / Vector2.Distance(enemy.Position, this.character.Position), 0.1f, 10f);
								CS$<>8__locals2.escapeVel += new Vector2((float)enemyDir * distMultiplier, (!this.character.IsClimbing) ? 0f : ((float)Math.Sign(dir.Y) * distMultiplier));
							}
						}
						if (CS$<>8__locals2.escapeVel == Vector2.Zero)
						{
							backOff = false;
						}
						if (backOff)
						{
							float left = (float)(currentHull.Rect.X + 50);
							float right = (float)(currentHull.Rect.Right - 50);
							backOff = ((CS$<>8__locals2.escapeVel.X < 0f && this.character.Position.X > left) || (CS$<>8__locals2.escapeVel.X > 0f && this.character.Position.X < right));
						}
					}
					if (backOff)
					{
						this.<Move>g__BackOff|101_0(ref CS$<>8__locals1, ref CS$<>8__locals2);
						return;
					}
					this.Engage(CS$<>8__locals1.deltaTime);
					return;
				}
			}
			this.Engage(CS$<>8__locals1.deltaTime);
		}

		// Token: 0x06002B8D RID: 11149 RVA: 0x001DEF68 File Offset: 0x001DD168
		private bool TryArm()
		{
			if (this.character.LockHands || this.Enemy == null)
			{
				this.Weapon = null;
				base.RemoveSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective);
				return false;
			}
			bool isAllowedToSeekWeapons = this.character.IsHostileEscortee || this.character.IsPrisoner || (this.character.IsInFriendlySub && this.IsOffensiveOrArrest && !this.character.IsInstigator && !(this.objectiveManager.CurrentOrder is AIObjectiveGoTo));
			if (this.checkWeaponsTimer < 0f)
			{
				this.checkWeaponsTimer = 1f;
				HashSet<ItemComponent> allWeapons = this.FindWeaponsFromInventory();
				while (allWeapons.Any<ItemComponent>())
				{
					ItemComponent newWeaponComponent;
					this.Weapon = this.GetWeapon(allWeapons, out newWeaponComponent);
					this._weaponComponent = newWeaponComponent;
					if (this.Weapon == null)
					{
						break;
					}
					if (!this.character.Inventory.Contains(this.Weapon) || this.WeaponComponent == null)
					{
						allWeapons.RemoveWhere((ItemComponent weaponComponent) => weaponComponent.Item == this.Weapon);
						this.Weapon = null;
					}
					else
					{
						if (!this.WeaponComponent.IsEmpty(this.character))
						{
							break;
						}
						bool seekAmmo = isAllowedToSeekWeapons && this.seekAmmunitionObjective == null;
						if (seekAmmo)
						{
							seekAmmo = (this.Mode == AIObjectiveCombat.CombatMode.Arrest || !this.IsEnemyClose(300f));
						}
						if (this.Reload(seekAmmo) || this.seekAmmunitionObjective != null)
						{
							break;
						}
						allWeapons.RemoveWhere((ItemComponent weaponComponent) => weaponComponent.Item == this.Weapon);
						this.Weapon = null;
					}
				}
				if (this.Weapon == null)
				{
					this.Weapon = this.FindWeapon(out this._weaponComponent);
					if (this.Weapon != null && !this.<TryArm>g__CheckWeapon|102_0(true))
					{
						if (this.seekAmmunitionObjective != null)
						{
							return false;
						}
						this.Weapon = null;
					}
				}
				if (!isAllowedToSeekWeapons)
				{
					if (this.WeaponComponent == null)
					{
						this.SpeakNoWeapons();
						this.Mode = AIObjectiveCombat.CombatMode.Retreat;
					}
				}
				else if (this.seekAmmunitionObjective == null && (this.WeaponComponent == null || (this.WeaponComponent.CombatPriority < 30f && !this.IsEnemyClose(300f))))
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.followTargetObjective);
					base.TryAddSubObjective<AIObjectiveGetItem>(ref this.seekWeaponObjective, () => new AIObjectiveGetItem(this.character, "weapon".ToIdentifier(), this.objectiveManager, true, false, 1f, false)
					{
						AllowStealing = base.HumanAIController.IsMentallyUnstable,
						AbortCondition = ((AIObjective _) => this.IsEnemyClose(150f)),
						EvaluateCombatPriority = false,
						GetItemPriority = delegate(Item i)
						{
							if (this.Weapon != null && (i == this.Weapon || i.Prefab.Identifier == this.Weapon.Prefab.Identifier))
							{
								return 0f;
							}
							if (i.IsOwnedBy(this.character))
							{
								return 0f;
							}
							float priority = 0f;
							ItemComponent ic = AIObjectiveCombat.GetWeaponComponent(i);
							if (ic != null)
							{
								float num;
								priority = this.GetWeaponPriority(ic, false, true, out num) / 100f;
							}
							if (priority <= 0f)
							{
								return 0f;
							}
							Vector2 toItem = i.WorldPosition - this.character.WorldPosition;
							float range = base.HumanAIController.FindWeaponsRange;
							if (range > 0f && range < float.PositiveInfinity)
							{
								float yDiff = (Math.Abs(toItem.Y) > 100f) ? (toItem.Y * 2f) : 0f;
								Vector2 adjustedDiff = new Vector2(toItem.X, yDiff);
								if (adjustedDiff.LengthSquared() > MathUtils.Pow2(range))
								{
									return 0f;
								}
							}
							Vector2 toEnemy = this.Enemy.WorldPosition - this.character.WorldPosition;
							if (Math.Sign(toItem.X) == Math.Sign(toEnemy.X))
							{
								priority *= 0.5f;
							}
							if (i.CurrentHull != null && !base.HumanAIController.VisibleHulls.Contains(i.CurrentHull) && Math.Abs(toItem.Y) > 100f && Math.Abs(toEnemy.Y) > 100f && Math.Sign(toItem.Y) == Math.Sign(toEnemy.Y))
							{
								priority *= 0.75f;
							}
							return priority;
						}
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveGetItem>(ref this.seekWeaponObjective);
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveGetItem>(ref this.seekWeaponObjective);
						if (this.Weapon == null)
						{
							this.SpeakNoWeapons();
							this.Mode = AIObjectiveCombat.CombatMode.Retreat;
							return;
						}
						if (!this.objectiveManager.HasObjectiveOrOrder<AIObjectiveFightIntruders>())
						{
							this.Mode = AIObjectiveCombat.CombatMode.Defensive;
						}
					});
				}
			}
			else if (this.seekAmmunitionObjective == null && this.seekWeaponObjective == null && !this.<TryArm>g__CheckWeapon|102_0(false))
			{
				this.Weapon = null;
			}
			return this.Weapon != null;
		}

		// Token: 0x06002B8E RID: 11150 RVA: 0x001DF1F8 File Offset: 0x001DD3F8
		private void OperateWeapon(float deltaTime)
		{
			if (this.isMoving && this.character.IsClimbing)
			{
				this.ClearInputs();
				return;
			}
			AIObjectiveCombat.CombatMode mode = this.Mode;
			if (mode > AIObjectiveCombat.CombatMode.Arrest)
			{
				if (mode != AIObjectiveCombat.CombatMode.Retreat)
				{
					throw new NotImplementedException();
				}
			}
			else if (this.Equip())
			{
				this.Attack(deltaTime);
				return;
			}
		}

		// Token: 0x06002B8F RID: 11151 RVA: 0x001DF248 File Offset: 0x001DD448
		private Item FindWeapon(out ItemComponent weaponComponent)
		{
			return this.GetWeapon(this.FindWeaponsFromInventory(), out weaponComponent);
		}

		// Token: 0x06002B90 RID: 11152 RVA: 0x001DF257 File Offset: 0x001DD457
		private static ItemComponent GetWeaponComponent(Item item)
		{
			MeleeWeapon result;
			if ((result = item.GetComponent<MeleeWeapon>()) == null && (result = item.GetComponent<RangedWeapon>()) == null)
			{
				result = (item.GetComponent<RepairTool>() ?? item.GetComponent<Holdable>());
			}
			return result;
		}

		// Token: 0x06002B91 RID: 11153 RVA: 0x001DF280 File Offset: 0x001DD480
		private float GetWeaponPriority(ItemComponent weapon, bool prioritizeMelee, bool canSeekAmmo, out float lethalDmg)
		{
			lethalDmg = -1f;
			float priority = weapon.CombatPriority;
			if (priority <= 0f)
			{
				return 0f;
			}
			RepairTool repairTool = weapon as RepairTool;
			if (repairTool != null)
			{
				switch (repairTool.UsableIn)
				{
				case RepairTool.UseEnvironment.Air:
					if (this.character.InWater)
					{
						return 0f;
					}
					break;
				case RepairTool.UseEnvironment.Water:
					if (!this.character.InWater)
					{
						return 0f;
					}
					break;
				case RepairTool.UseEnvironment.None:
					return 0f;
				}
			}
			if (prioritizeMelee && weapon is MeleeWeapon)
			{
				priority *= 5f;
			}
			if (weapon.IsEmpty(this.character))
			{
				if (weapon is RangedWeapon && !canSeekAmmo)
				{
					return 0f;
				}
				Character character = this.character;
				Item weapon2 = this.Weapon;
				Func<InvSlotType, bool> func;
				if ((func = AIObjectiveCombat.<>O.<0>__IsHandSlotType) == null)
				{
					func = (AIObjectiveCombat.<>O.<0>__IsHandSlotType = new Func<InvSlotType, bool>(CharacterInventory.IsHandSlotType));
				}
				Func<InvSlotType, bool> predicate = func;
				if (character.HasEquippedItem(weapon2, null, predicate))
				{
					priority *= 0.75f;
				}
				else
				{
					priority *= 0.5f;
				}
			}
			if (this.Enemy.Params.Health.StunImmunity)
			{
				if (weapon.Item.HasTag(Tags.StunnerItem))
				{
					priority /= 2f;
				}
			}
			else if (this.Enemy.IsKnockedDown && this.Mode != AIObjectiveCombat.CombatMode.Arrest)
			{
				Attack attack = AIObjectiveCombat.GetAttackDefinition(weapon);
				if (attack != null)
				{
					lethalDmg = attack.GetTotalCharacterDamage();
					float max = lethalDmg + 1f;
					if (weapon.Item.HasTag(Tags.StunnerItem))
					{
						priority = max;
					}
					else
					{
						float stunDmg = this.ApproximateStunDamage(weapon, attack);
						float diff = stunDmg - lethalDmg;
						priority = Math.Clamp(priority - Math.Max(diff * 2f, 0f), 1f, max);
					}
				}
			}
			else if (this.Mode == AIObjectiveCombat.CombatMode.Arrest)
			{
				if (weapon.Item.HasTag(Tags.StunnerItem))
				{
					priority *= 5f;
				}
				else
				{
					Attack attack2 = AIObjectiveCombat.GetAttackDefinition(weapon);
					if (attack2 != null)
					{
						lethalDmg = attack2.GetTotalCharacterDamage();
						float stunDmg2 = this.ApproximateStunDamage(weapon, attack2);
						float diff2 = stunDmg2 - lethalDmg;
						if (diff2 < 0f)
						{
							priority /= 2f;
						}
					}
				}
			}
			else if (weapon is MeleeWeapon && weapon.Item.HasTag(Tags.StunnerItem) && (this.Enemy.Params.Health.StunImmunity || !AIObjectiveCombat.CanMeleeStunnerStun(weapon)))
			{
				Attack attack3 = AIObjectiveCombat.GetAttackDefinition(weapon);
				priority = ((attack3 != null) ? attack3.GetTotalCharacterDamage() : (priority / 2f));
			}
			float startPriority = priority;
			ImmutableArray<SkillRequirementHint> skillRequirementHints = weapon.Item.Prefab.SkillRequirementHints;
			if (new ImmutableArray<SkillRequirementHint>?(skillRequirementHints) != null)
			{
				foreach (SkillRequirementHint hint in skillRequirementHints)
				{
					float skillLevel = this.character.GetSkillLevel(hint.Skill);
					float targetLevel = hint.Level;
					priority = AIObjectiveCombat.<GetWeaponPriority>g__ReducePriority|106_0(priority, skillLevel, targetLevel);
				}
			}
			else
			{
				foreach (Skill skill in weapon.RequiredSkills)
				{
					float skillLevel2 = this.character.GetSkillLevel(skill.Identifier);
					float targetLevel2 = skill.Level * weapon.GetSkillMultiplier();
					priority = AIObjectiveCombat.<GetWeaponPriority>g__ReducePriority|106_0(priority, skillLevel2, targetLevel2);
				}
			}
			priority = Math.Max(priority, startPriority / 2f);
			return priority;
		}

		// Token: 0x06002B92 RID: 11154 RVA: 0x001DF5F8 File Offset: 0x001DD7F8
		private float ApproximateStunDamage(ItemComponent weapon, Attack attack)
		{
			IEnumerable<StatusEffect> statusEffects = from se in attack.StatusEffects
			where !se.HasConditions && se.type == ActionType.OnUse && se.HasRequiredItems(this.character)
			select se;
			List<StatusEffect> hitEffects;
			if (weapon.statusEffectLists != null && weapon.statusEffectLists.TryGetValue(ActionType.OnUse, out hitEffects))
			{
				statusEffects = statusEffects.Concat(hitEffects);
			}
			float afflictionsStun = attack.Afflictions.Keys.Sum(delegate(Affliction a)
			{
				Identifier identifier = a.Identifier;
				if (!(identifier == AfflictionPrefab.StunType))
				{
					return 0f;
				}
				return a.Strength;
			});
			float num;
			if (!statusEffects.None(null))
			{
				num = statusEffects.Max(delegate(StatusEffect se)
				{
					float stunAmount = 0f;
					Affliction stunAffliction = se.Afflictions.Find(delegate(Affliction a)
					{
						Identifier identifier = a.Identifier;
						return identifier == AfflictionPrefab.StunType;
					});
					if (stunAffliction != null)
					{
						stunAmount = stunAffliction.Strength;
					}
					return stunAmount;
				});
			}
			else
			{
				num = 0f;
			}
			float effectsStun = num;
			return attack.Stun + afflictionsStun + effectsStun;
		}

		// Token: 0x06002B93 RID: 11155 RVA: 0x001DF6B0 File Offset: 0x001DD8B0
		private static bool CanMeleeStunnerStun(ItemComponent weapon)
		{
			Identifier mobileBatteryTag = Tags.MobileBattery;
			IEnumerable<ItemComponent> containers = weapon.Item.Components.Where(delegate(ItemComponent ic)
			{
				ItemContainer container = ic as ItemContainer;
				return container != null && container.ContainableItemIdentifiers.Contains(mobileBatteryTag);
			});
			Func<Item, bool> <>9__2;
			return containers.None(null) || containers.Any(delegate(ItemComponent container)
			{
				ItemContainer itemContainer = container as ItemContainer;
				if (itemContainer == null)
				{
					return false;
				}
				IEnumerable<Item> allItems = itemContainer.Inventory.AllItems;
				Func<Item, bool> predicate;
				if ((predicate = <>9__2) == null)
				{
					predicate = (<>9__2 = ((Item i) => i != null && i.HasTag(mobileBatteryTag) && i.Condition > 0f));
				}
				return allItems.Any(predicate);
			});
		}

		// Token: 0x06002B94 RID: 11156 RVA: 0x001DF708 File Offset: 0x001DD908
		private Item GetWeapon(IEnumerable<ItemComponent> weaponList, out ItemComponent weaponComponent)
		{
			this.hasValidRangedWeapon = false;
			weaponComponent = null;
			float bestPriority = 0f;
			float lethalDmg = -1f;
			bool prioritizeMelee = this.IsEnemyClose(100f) || EnemyAIController.IsLatchedTo(this.Enemy, this.character);
			bool isCloseToEnemy = prioritizeMelee || this.IsEnemyClose(300f);
			foreach (ItemComponent weapon in weaponList)
			{
				float priority = this.GetWeaponPriority(weapon, prioritizeMelee, !isCloseToEnemy, out lethalDmg);
				bool flag = priority >= 30f;
				bool flag2 = flag;
				if (flag2)
				{
					bool flag3 = weapon is RangedWeapon || weapon is RepairTool;
					flag2 = flag3;
				}
				if (flag2)
				{
					this.hasValidRangedWeapon = true;
				}
				if (priority > bestPriority)
				{
					weaponComponent = weapon;
					bestPriority = priority;
				}
			}
			if (weaponComponent == null)
			{
				return null;
			}
			if (bestPriority < 1f)
			{
				return null;
			}
			if (this.Mode == AIObjectiveCombat.CombatMode.Arrest)
			{
				if (weaponComponent.Item.HasTag(Tags.StunnerItem))
				{
					this.isLethalWeapon = false;
				}
				else
				{
					if (lethalDmg < 0f)
					{
						lethalDmg = AIObjectiveCombat.GetLethalDamage(weaponComponent);
					}
					this.isLethalWeapon = (lethalDmg > 1f);
				}
				if (this.AllowHoldFire)
				{
					if (!this.hasAimed && this.holdFireTimer <= 0f)
					{
						this.holdFireTimer = this.ArrestHoldFireTime * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
					}
					else if (this.SpeakWarnings)
					{
						if (!this.lastWarningTriggered && this.holdFireTimer < this.ArrestHoldFireTime * 0.3f)
						{
							this.FriendlyGuardSpeak("dialogarrest.lastwarning".ToIdentifier(), 0f, 0f);
							this.lastWarningTriggered = true;
						}
						else if (!this.firstWarningTriggered && this.holdFireTimer < this.ArrestHoldFireTime * 0.8f)
						{
							this.FriendlyGuardSpeak("dialogarrest.firstwarning".ToIdentifier(), 0f, 0f);
							this.firstWarningTriggered = true;
						}
					}
				}
			}
			return weaponComponent.Item;
		}

		// Token: 0x06002B95 RID: 11157 RVA: 0x001DF918 File Offset: 0x001DDB18
		public static float GetLethalDamage(ItemComponent weapon)
		{
			float lethalDmg = 0f;
			Attack attack = AIObjectiveCombat.GetAttackDefinition(weapon);
			if (attack != null)
			{
				lethalDmg = attack.GetTotalCharacterDamage();
			}
			return lethalDmg;
		}

		// Token: 0x06002B96 RID: 11158 RVA: 0x001DF940 File Offset: 0x001DDB40
		private static Attack GetAttackDefinition(ItemComponent weapon)
		{
			MeleeWeapon meleeWeapon = weapon as MeleeWeapon;
			Attack result;
			if (meleeWeapon == null)
			{
				RangedWeapon rangedWeapon = weapon as RangedWeapon;
				if (rangedWeapon == null)
				{
					result = null;
				}
				else
				{
					Projectile projectile = rangedWeapon.FindProjectile(false);
					result = ((projectile != null) ? projectile.Attack : null);
				}
			}
			else
			{
				result = meleeWeapon.Attack;
			}
			return result;
		}

		// Token: 0x06002B97 RID: 11159 RVA: 0x001DF988 File Offset: 0x001DDB88
		private HashSet<ItemComponent> FindWeaponsFromInventory()
		{
			this.weapons.Clear();
			foreach (Item item in this.character.Inventory.AllItems)
			{
				this.GetWeapons(item, this.weapons);
				if (item.OwnInventory != null)
				{
					item.OwnInventory.AllItems.ForEach(delegate(Item i)
					{
						this.GetWeapons(i, this.weapons);
					});
				}
			}
			return this.weapons;
		}

		// Token: 0x06002B98 RID: 11160 RVA: 0x001DFA1C File Offset: 0x001DDC1C
		private void GetWeapons(Item item, ICollection<ItemComponent> weaponList)
		{
			if (item == null)
			{
				return;
			}
			foreach (ItemComponent component in item.Components)
			{
				if (!this.ignoredWeapons.Contains(component) && component.CombatPriority > 0f)
				{
					weaponList.Add(component);
				}
			}
		}

		// Token: 0x06002B99 RID: 11161 RVA: 0x001DFA90 File Offset: 0x001DDC90
		private void UnequipWeapon()
		{
			if (this.Weapon == null)
			{
				return;
			}
			if (this.character.LockHands)
			{
				return;
			}
			if (this.character.HeldItems.Contains(this.Weapon))
			{
				return;
			}
			this.character.Unequip(this.Weapon);
		}

		// Token: 0x06002B9A RID: 11162 RVA: 0x001DFAE0 File Offset: 0x001DDCE0
		private bool Equip()
		{
			if (this.character.LockHands)
			{
				return false;
			}
			if (this.WeaponComponent.IsEmpty(this.character))
			{
				return false;
			}
			Character character = this.character;
			Item weapon = this.Weapon;
			Func<InvSlotType, bool> func;
			if ((func = AIObjectiveCombat.<>O.<0>__IsHandSlotType) == null)
			{
				func = (AIObjectiveCombat.<>O.<0>__IsHandSlotType = new Func<InvSlotType, bool>(CharacterInventory.IsHandSlotType));
			}
			Func<InvSlotType, bool> predicate = func;
			if (!character.HasEquippedItem(weapon, null, predicate))
			{
				this.ClearInputs();
				this.Weapon.TryInteract(this.character, false, true, false);
				IEnumerable<InvSlotType> allowedSlots = this.Weapon.AllowedSlots;
				Func<InvSlotType, bool> predicate2;
				if ((predicate2 = AIObjectiveCombat.<>O.<0>__IsHandSlotType) == null)
				{
					predicate2 = (AIObjectiveCombat.<>O.<0>__IsHandSlotType = new Func<InvSlotType, bool>(CharacterInventory.IsHandSlotType));
				}
				IEnumerable<InvSlotType> slots = allowedSlots.Where(predicate2);
				bool successfullyEquipped = this.character.TryPutItem(this.Weapon, slots);
				ValueTuple<Item, Item> items;
				if (!successfullyEquipped && this.character.HasHandsFull(out items))
				{
					this.character.Unequip(items.Item1);
					this.character.Unequip(items.Item2);
					successfullyEquipped = this.character.TryPutItem(this.Weapon, slots);
				}
				if (!successfullyEquipped)
				{
					this.SpeakNoWeapons();
					this.Weapon = null;
					this.Mode = AIObjectiveCombat.CombatMode.Retreat;
					return false;
				}
				this.SetAimTimer(Rand.Range(0.2f, 0.4f, Rand.RandSync.Unsynced) / this.AimSpeed);
				this.SetReloadTime(this.WeaponComponent);
			}
			return true;
		}

		// Token: 0x06002B9B RID: 11163 RVA: 0x001DFC38 File Offset: 0x001DDE38
		private void Retreat(float deltaTime)
		{
			this.isMoving = true;
			if (!this.Enemy.IsHuman && !this.character.IsInFriendlySub)
			{
				this.PlayerCrewSpeak("dialogcombatretreating".ToIdentifier(), Rand.Range(0f, 1f, Rand.RandSync.Unsynced), 20f);
			}
			this.RemoveFollowTarget();
			base.RemoveSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective);
			if (this.retreatTarget != null && base.HumanAIController.VisibleHulls.Contains(this.Enemy.CurrentHull) && this.retreatTarget == this.character.CurrentHull)
			{
				this.retreatTarget = null;
			}
			if (this.retreatObjective != null && this.retreatObjective.Target != this.retreatTarget)
			{
				base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
			}
			if (this.character.Submarine == null && this.sqrDistance < MathUtils.Pow2(2000f))
			{
				base.SteeringManager.Reset();
				this.character.ReleaseSecondaryItem();
				base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.character.WorldPosition - this.Enemy.WorldPosition));
				base.SteeringManager.SteeringAvoid(deltaTime, 5f, 2f);
				return;
			}
			if (this.retreatTarget != null)
			{
				AIObjectiveGoTo aiobjectiveGoTo = this.retreatObjective;
				if (aiobjectiveGoTo == null || aiobjectiveGoTo.CanBeCompleted)
				{
					goto IL_1CE;
				}
			}
			if (this.findHullTimer > 0f)
			{
				this.findHullTimer -= deltaTime;
			}
			else
			{
				Hull potentialSafeHull;
				AIObjectiveFindSafety.HullSearchStatus hullSearchStatus = this.findSafety.FindBestHull(out potentialSafeHull, base.HumanAIController.VisibleHulls, this.character.TeamID != CharacterTeamType.FriendlyNPC);
				if (hullSearchStatus != AIObjectiveFindSafety.HullSearchStatus.Finished)
				{
					this.findSafety.UpdateSimpleEscape(deltaTime);
					return;
				}
				this.retreatTarget = potentialSafeHull;
				this.findHullTimer = 1f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
			}
			IL_1CE:
			if (this.retreatTarget != null && this.character.CurrentHull != this.retreatTarget)
			{
				base.TryAddSubObjective<AIObjectiveGoTo>(ref this.retreatObjective, () => new AIObjectiveGoTo(this.retreatTarget, this.character, this.objectiveManager, false, true, 1f, 0f)
				{
					UsePathingOutside = false,
					SpeakIfFails = false
				}, delegate
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
				}, delegate
				{
					if (this.Enemy != null && base.HumanAIController.VisibleHulls.Contains(this.Enemy.CurrentHull))
					{
						base.SteeringManager.Reset();
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
						return;
					}
					base.Abandon = true;
				});
			}
		}

		// Token: 0x06002B9C RID: 11164 RVA: 0x001DFE60 File Offset: 0x001DE060
		private void Engage(float deltaTime)
		{
			if (this.WeaponComponent == null)
			{
				this.RemoveFollowTarget();
				base.SteeringManager.Reset();
				return;
			}
			if (this.character.LockHands || this.Enemy == null)
			{
				this.Mode = AIObjectiveCombat.CombatMode.Retreat;
				base.SteeringManager.Reset();
				return;
			}
			this.retreatTarget = null;
			base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
			base.RemoveSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective);
			base.RemoveSubObjective<AIObjectiveGetItem>(ref this.seekWeaponObjective);
			if (this.character.Submarine == null)
			{
				MeleeWeapon meleeWeapon = this.WeaponComponent as MeleeWeapon;
				if (meleeWeapon != null)
				{
					if (this.sqrDistance > MathUtils.Pow2(meleeWeapon.Range))
					{
						this.isMoving = true;
						this.character.ReleaseSecondaryItem();
						base.SteeringManager.Reset();
						base.SteeringManager.SteeringSeek(this.character.GetRelativeSimPosition(this.Enemy, null), 10f);
						base.SteeringManager.SteeringAvoid(deltaTime, 5f, 15f);
						return;
					}
					base.SteeringManager.Reset();
					return;
				}
			}
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.character.Submarine != null && this.character.Submarine.TeamID != this.character.TeamID && !this.character.IsClimbing && !this.character.CanSeeTarget(this.Enemy, null, false, false))
			{
				base.SteeringManager.Reset();
				this.RemoveFollowTarget();
				return;
			}
			if (this.followTargetObjective != null && this.followTargetObjective.Target != this.Enemy)
			{
				this.RemoveFollowTarget();
			}
			base.TryAddSubObjective<AIObjectiveGoTo>(ref this.followTargetObjective, () => new AIObjectiveGoTo(this.Enemy, this.character, this.objectiveManager, true, true, 1f, 50f)
			{
				UsePathingOutside = false,
				IgnoreIfTargetDead = true,
				TargetName = this.Enemy.DisplayName,
				AlwaysUseEuclideanDistance = false,
				SpeakIfFails = false
			}, null, delegate
			{
				if (this.Enemy != null && base.HumanAIController.VisibleHulls.Contains(this.Enemy.CurrentHull))
				{
					base.SteeringManager.Reset();
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.followTargetObjective);
					return;
				}
				base.Abandon = true;
			});
			if (this.followTargetObjective == null)
			{
				return;
			}
			if (this.Mode == AIObjectiveCombat.CombatMode.Arrest && this.Enemy.IsKnockedDownOrRagdolled && !this.arrestingRegistered)
			{
				IEnumerable<Item> enumerable;
				if (!HumanAIController.HasItem(this.character, Tags.HandLockerItem, out enumerable, default(Identifier), 0f, false, true, null) && this.character.TeamID == CharacterTeamType.FriendlyNPC)
				{
					ItemPrefab prefab = ItemPrefab.Find(null, "handcuffs".ToIdentifier());
					if (prefab != null)
					{
						Entity.Spawner.AddItemToSpawnQueue(prefab, this.character.Inventory, null, null, delegate(Item i)
						{
							i.SpawnedInCurrentOutpost = true;
							i.AllowStealing = false;
						}, true, false, InvSlotType.None);
					}
				}
				this.arrestingRegistered = true;
				this.followTargetObjective.Completed += this.OnArrestTargetReached;
				this.followTargetObjective.CloseEnough = 100f;
			}
			if (!this.arrestingRegistered)
			{
				AIObjectiveGoTo aiobjectiveGoTo = this.followTargetObjective;
				ItemComponent weaponComponent = this.WeaponComponent;
				float closeEnough;
				if (!(weaponComponent is RangedWeapon))
				{
					MeleeWeapon mw = weaponComponent as MeleeWeapon;
					if (mw == null)
					{
						RepairTool rt = weaponComponent as RepairTool;
						if (rt == null)
						{
							closeEnough = 50f;
						}
						else
						{
							closeEnough = rt.Range;
						}
					}
					else
					{
						closeEnough = mw.Range;
					}
				}
				else
				{
					closeEnough = (this.isAimBlocked ? this.BlockedDistance : 1000f);
				}
				aiobjectiveGoTo.CloseEnough = closeEnough;
			}
			if (this.isAimBlocked)
			{
				base.ForceWalkTemporarily = true;
			}
			if (!this.followTargetObjective.IsCloseEnough)
			{
				this.isMoving = true;
			}
		}

		// Token: 0x06002B9D RID: 11165 RVA: 0x001E01C0 File Offset: 0x001DE3C0
		private void RemoveFollowTarget()
		{
			if (this.followTargetObjective != null)
			{
				if (this.arrestingRegistered)
				{
					this.followTargetObjective.Completed -= this.OnArrestTargetReached;
				}
				base.RemoveSubObjective<AIObjectiveGoTo>(ref this.followTargetObjective);
			}
			this.arrestingRegistered = false;
		}

		// Token: 0x06002B9E RID: 11166 RVA: 0x001E01FC File Offset: 0x001DE3FC
		private void OnArrestTargetReached()
		{
			if (!this.Enemy.IsKnockedDownOrRagdolled)
			{
				this.RemoveFollowTarget();
				return;
			}
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC)
			{
				foreach (Item item in this.Enemy.Inventory.AllItemsMod)
				{
					AIObjectiveFindThieves.MarkTargetAsInspected(this.character);
					bool confiscateItem = AIObjectiveCheckStolenItems.IsItemIllegitimate(this.Enemy, item);
					if (!confiscateItem && this.Enemy.IsActingOffensively)
					{
						bool flag;
						if (!item.HasTag(Tags.Weapon) && !item.HasTag(Tags.Poison))
						{
							ItemComponent weaponComponent = AIObjectiveCombat.GetWeaponComponent(item);
							flag = (weaponComponent != null && weaponComponent.CombatPriority > 0f);
						}
						else
						{
							flag = true;
						}
						confiscateItem = flag;
					}
					if (confiscateItem)
					{
						item.Drop(this.character, true, true);
						this.character.Inventory.TryPutItem(item, this.character, CharacterInventory.AnySlot, true, false, true);
					}
				}
			}
			IEnumerable<Item> matchingItems;
			if (!HumanAIController.HasItem(this.Enemy, Tags.HandLockerItem, out matchingItems, default(Identifier), 0f, false, true, null))
			{
				HumanAIController.HasItem(this.character, Tags.HandLockerItem, out matchingItems, default(Identifier), 0f, false, true, null);
			}
			if (matchingItems.Any<Item>() && !this.Enemy.IsUnconscious && this.Enemy.IsKnockedDownOrRagdolled && this.character.CanInteractWith(this.Enemy, 200f, true, false) && !this.Enemy.LockHands)
			{
				Item handCuffs = matchingItems.First<Item>();
				if (!base.HumanAIController.TakeItem(handCuffs, this.Enemy.Inventory, true, true, true, false, false, null) && this.objectiveManager.IsCurrentObjective<AIObjectiveFightIntruders>())
				{
					base.Abandon = true;
					return;
				}
				this.character.Speak(TextManager.Get("DialogTargetArrested").Value, null, 3f, "targetarrested".ToIdentifier(), 30f);
			}
			if (!this.objectiveManager.IsCurrentObjective<AIObjectiveFightIntruders>())
			{
				base.IsCompleted = true;
			}
		}

		// Token: 0x06002B9F RID: 11167 RVA: 0x001E0434 File Offset: 0x001DE634
		private void SeekAmmunition(ImmutableHashSet<Identifier> ammunitionIdentifiers)
		{
			this.retreatTarget = null;
			base.RemoveSubObjective<AIObjectiveGoTo>(ref this.retreatObjective);
			base.RemoveSubObjective<AIObjectiveGetItem>(ref this.seekWeaponObjective);
			this.RemoveFollowTarget();
			ItemContainer itemContainer = this.Weapon.GetComponent<ItemContainer>();
			base.TryAddSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective, () => new AIObjectiveContainItem(this.character, ammunitionIdentifiers, itemContainer, this.objectiveManager, 1f, !this.character.IsOnPlayerTeam && this.character.AIController.HasInfiniteItemSpawns(ammunitionIdentifiers))
			{
				ItemCount = itemContainer.MainContainerCapacity * itemContainer.MaxStackSize,
				checkInventory = false,
				MoveWholeStack = true
			}, delegate
			{
				this.RemoveSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective);
			}, delegate
			{
				this.SteeringManager.Reset();
				this.RemoveSubObjective<AIObjectiveContainItem>(ref this.seekAmmunitionObjective);
				this.ignoredWeapons.Add(this.WeaponComponent);
				this.Weapon = null;
			});
		}

		// Token: 0x06002BA0 RID: 11168 RVA: 0x001E04BC File Offset: 0x001DE6BC
		private bool Reload(bool seekAmmo)
		{
			if (this.WeaponComponent == null)
			{
				return false;
			}
			if (this.Weapon.OwnInventory == null)
			{
				return true;
			}
			if (!this.Weapon.IsInteractable(this.character))
			{
				return false;
			}
			base.HumanAIController.UnequipEmptyItems(this.Weapon, true, !this.character.IsOnPlayerTeam);
			ImmutableHashSet<Identifier> ammunitionIdentifiers = null;
			if (this.WeaponComponent.RequiredItems.ContainsKey(RelatedItem.RelationType.Contained))
			{
				using (List<RelatedItem>.Enumerator enumerator = this.WeaponComponent.RequiredItems[RelatedItem.RelationType.Contained].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RelatedItem requiredItem = enumerator.Current;
						if (!this.Weapon.OwnInventory.AllItems.Any((Item it) => it.Condition > 0f && requiredItem.MatchesItem(it)))
						{
							ammunitionIdentifiers = requiredItem.Identifiers;
							break;
						}
					}
					goto IL_10B;
				}
			}
			MeleeWeapon meleeWeapon = this.WeaponComponent as MeleeWeapon;
			if (meleeWeapon != null)
			{
				ammunitionIdentifiers = meleeWeapon.PreferredContainedItems;
			}
			IL_10B:
			if (ammunitionIdentifiers != null)
			{
				Item ammunition = this.character.Inventory.FindItem((Item i) => i.HasIdentifierOrTags(ammunitionIdentifiers) && i.Condition > 0f && !AIObjectiveCombat.<Reload>g__IsInsideHeadset|123_1(i) && i.IsInteractable(this.character), true);
				if (ammunition != null)
				{
					ItemContainer container = this.Weapon.GetComponent<ItemContainer>();
					if (container.Inventory.TryPutItem(ammunition, this.character, null, true, false, true))
					{
						this.ClearInputs();
						this.SetReloadTime(this.WeaponComponent);
					}
					else if (ammunition.ParentInventory == this.character.Inventory)
					{
						ammunition.Drop(this.character, true, true);
					}
				}
			}
			if (!this.WeaponComponent.IsEmpty(this.character))
			{
				return true;
			}
			if (this.IsOffensiveOrArrest && seekAmmo && ammunitionIdentifiers != null)
			{
				if (!this.Weapon.OwnInventory.Container.DrawInventory)
				{
					return false;
				}
				this.SeekAmmunition(ammunitionIdentifiers);
			}
			return false;
		}

		// Token: 0x17000AFF RID: 2815
		// (get) Token: 0x06002BA1 RID: 11169 RVA: 0x001E06C0 File Offset: 0x001DE8C0
		private float BlockedDistance
		{
			get
			{
				if (this._blockedDistance <= 0f)
				{
					this._blockedDistance = 300f * Rand.Range(1f, 1.3f, Rand.RandSync.Unsynced);
				}
				return this._blockedDistance;
			}
		}

		// Token: 0x06002BA2 RID: 11170 RVA: 0x001E06F4 File Offset: 0x001DE8F4
		private void Attack(float deltaTime)
		{
			this.character.CursorPosition = this.Enemy.WorldPosition;
			if (this.AimAccuracy < 1f)
			{
				this.spreadTimer += deltaTime * Rand.Range(0.01f, 1f, Rand.RandSync.Unsynced);
				float shake = Rand.Range(0.95f, 1.05f, Rand.RandSync.Unsynced);
				float offsetAmount = (1f - this.AimAccuracy) * Rand.Range(300f, 500f, Rand.RandSync.Unsynced);
				float distanceFactor = MathUtils.InverseLerp(0f, 1000000f, this.sqrDistance);
				float offset = (float)Math.Sin((double)(this.spreadTimer * shake)) * offsetAmount * distanceFactor;
				this.character.CursorPosition += new Vector2(0f, offset);
			}
			if (this.character.Submarine != null)
			{
				this.character.CursorPosition -= this.character.Submarine.Position;
			}
			this.visibilityCheckTimer -= deltaTime;
			if (this.visibilityCheckTimer <= 0f)
			{
				this.canSeeTarget = this.character.CanSeeTarget(this.Enemy, null, false, false);
				this.visibilityCheckTimer = 0.2f;
			}
			if (!this.canSeeTarget)
			{
				this.SetAimTimer(Rand.Range(0.2f, 0.4f, Rand.RandSync.Unsynced) / this.AimSpeed);
				return;
			}
			if (this.Weapon.RequireAimToUse)
			{
				this.character.SetInput(InputType.Aim, false, true);
			}
			this.hasAimed = true;
			if (this.AllowHoldFire && this.holdFireTimer > 0f)
			{
				this.holdFireTimer -= deltaTime;
				return;
			}
			if (this.aimTimer > 0f)
			{
				this.aimTimer -= deltaTime;
				return;
			}
			this.sqrDistance = Vector2.DistanceSquared(this.character.WorldPosition, this.Enemy.WorldPosition);
			this.distanceTimer = 0.2f;
			MeleeWeapon meleeWeapon = this.WeaponComponent as MeleeWeapon;
			if (meleeWeapon != null)
			{
				bool closeEnough = true;
				float sqrRange = meleeWeapon.Range * meleeWeapon.Range;
				if (this.character.AnimController.InWater)
				{
					if (this.sqrDistance > sqrRange)
					{
						closeEnough = false;
					}
				}
				else
				{
					float xDiff = Math.Abs(this.Enemy.WorldPosition.X - this.character.WorldPosition.X);
					if (xDiff > meleeWeapon.Range)
					{
						closeEnough = false;
					}
					float yDiff = Math.Abs(this.Enemy.WorldPosition.Y - this.character.WorldPosition.Y);
					if (yDiff > Math.Max(meleeWeapon.Range, 100f))
					{
						closeEnough = false;
					}
					if (closeEnough && this.Enemy.WorldPosition.Y < this.character.WorldPosition.Y && yDiff > 25f)
					{
						base.HumanAIController.AnimController.Crouch();
					}
				}
				if (this.reloadTimer > 0f)
				{
					return;
				}
				if (this.holdFireCondition != null && this.holdFireCondition())
				{
					return;
				}
				if (closeEnough)
				{
					this.UseWeapon(deltaTime);
					this.character.AIController.SteeringManager.Reset();
					return;
				}
				if (!this.character.IsFacing(this.Enemy.WorldPosition))
				{
					this.SetAimTimer(Rand.Range(1f, 1.5f, Rand.RandSync.Unsynced) / this.AimSpeed);
					return;
				}
			}
			else
			{
				RepairTool repairTool = this.WeaponComponent as RepairTool;
				if (repairTool != null)
				{
					float reach = AIObjectiveFixLeak.CalculateReach(repairTool, this.character);
					if (this.sqrDistance > reach * reach)
					{
						return;
					}
				}
				float aimFactor = 1.5707964f * (1f - this.AimAccuracy);
				if (VectorExtensions.Forward(this.Weapon.body.TransformedRotation, 1f).Angle(this.Enemy.WorldPosition - this.Weapon.WorldPosition) < 0.7853982f + aimFactor)
				{
					IEnumerable<Body> pickedBodies = Submarine.PickBodies(this.Weapon.SimPosition, Submarine.GetRelativeSimPosition(this.Weapon, this.Enemy, null), this.character.AnimController.LimbBodies, new Category?(Category.Cat2), true, null, false);
					foreach (Body body in pickedBodies)
					{
						Limb limb = body.UserData as Limb;
						if (limb != null)
						{
							Character target = limb.character;
							if (target != null && target != this.Enemy && base.HumanAIController.IsFriendly(target, false))
							{
								this.isAimBlocked = true;
								if (HumanAIController.DebugAI)
								{
									this.BlockedPositions.Add(ConvertUnits.ToDisplayUnits(body.Position));
								}
								this.allowCrouching = false;
								this.standUpTimer = 5f;
								return;
							}
						}
					}
					this.UseWeapon(deltaTime);
				}
			}
		}

		// Token: 0x06002BA3 RID: 11171 RVA: 0x001E0BEC File Offset: 0x001DEDEC
		private void UseWeapon(float deltaTime)
		{
			if (this.allowCrouching && !this.isMoving && !this.character.AnimController.InWater && !(this.WeaponComponent is MeleeWeapon))
			{
				base.HumanAIController.AnimController.Crouch();
			}
			this.character.SetInput(InputType.Shoot, false, true);
			this.Weapon.Use(deltaTime, this.character, null, null, null);
			this.SetReloadTime(this.WeaponComponent);
		}

		// Token: 0x06002BA4 RID: 11172 RVA: 0x001E0C68 File Offset: 0x001DEE68
		private float GetReloadTime(ItemComponent weaponComponent)
		{
			float reloadTime = 0f;
			RangedWeapon rangedWeapon = weaponComponent as RangedWeapon;
			if (rangedWeapon == null)
			{
				MeleeWeapon mw = weaponComponent as MeleeWeapon;
				if (mw != null)
				{
					reloadTime = mw.Reload;
				}
			}
			else if (rangedWeapon.ReloadTimer <= 0f && !rangedWeapon.HoldTrigger)
			{
				reloadTime = rangedWeapon.Reload;
			}
			return reloadTime;
		}

		// Token: 0x06002BA5 RID: 11173 RVA: 0x001E0CB8 File Offset: 0x001DEEB8
		private void SetReloadTime(ItemComponent weaponComponent)
		{
			float reloadTime = this.GetReloadTime(weaponComponent);
			this.reloadTimer = Math.Max(reloadTime, reloadTime * Rand.Range(1f, 1.25f, Rand.RandSync.Unsynced) / this.AimSpeed);
		}

		// Token: 0x06002BA6 RID: 11174 RVA: 0x001E0CF2 File Offset: 0x001DEEF2
		private void ClearInputs()
		{
			this.character.ClearInput(InputType.Aim);
			this.character.ClearInput(InputType.Shoot);
		}

		// Token: 0x17000B00 RID: 2816
		// (get) Token: 0x06002BA7 RID: 11175 RVA: 0x001E0D10 File Offset: 0x001DEF10
		private bool ShouldUnequipWeapon
		{
			get
			{
				return this.Weapon != null && this.character.Submarine != null && this.character.Submarine.TeamID == this.character.TeamID && Character.CharacterList.None((Character c) => c.Submarine == this.character.Submarine && HumanAIController.IsActive(c) && !HumanAIController.IsFriendly(this.character, c, false, false) && base.HumanAIController.VisibleHulls.Contains(c.CurrentHull));
			}
		}

		// Token: 0x06002BA8 RID: 11176 RVA: 0x001E0D68 File Offset: 0x001DEF68
		protected override void OnCompleted()
		{
			base.OnCompleted();
			if (this.Enemy != null)
			{
				AIObjectiveCombat.CombatMode mode = this.Mode;
				if (mode != AIObjectiveCombat.CombatMode.Offensive)
				{
					if (mode == AIObjectiveCombat.CombatMode.Arrest)
					{
						if (base.IsCompleted && !base.HumanAIController.IsTrueForAnyBotInTheCrew(delegate(HumanAIController bot)
						{
							if (bot != base.HumanAIController)
							{
								AIObjectiveCombat combatObj = bot.ObjectiveManager.CurrentObjective as AIObjectiveCombat;
								if (combatObj != null && combatObj.Mode == AIObjectiveCombat.CombatMode.Arrest && combatObj.Enemy == this.Enemy)
								{
									return true;
								}
							}
							AIObjective currentObjective = bot.ObjectiveManager.CurrentObjective;
							if (currentObjective is AIObjectiveGoTo)
							{
								AIObjectiveCombat combatObjective = currentObjective.SourceObjective as AIObjectiveCombat;
								if (combatObjective != null)
								{
									return combatObjective.Enemy == this.Enemy;
								}
							}
							return false;
						}))
						{
							this.RemoveFollowTarget();
							AIObjectiveGoTo approachArrestTarget = new AIObjectiveGoTo(this.Enemy, this.character, this.objectiveManager, false, false, 1f, 100f)
							{
								UsePathingOutside = false,
								IgnoreIfTargetDead = true,
								TargetName = this.Enemy.DisplayName,
								AlwaysUseEuclideanDistance = false,
								SpeakIfFails = false,
								SourceObjective = this
							};
							approachArrestTarget.Completed += this.OnArrestTargetReached;
							this.objectiveManager.AddObjective<AIObjectiveGoTo>(approachArrestTarget);
						}
					}
				}
				else if (this.Enemy.IsUnconscious && this.objectiveManager.HasObjectiveOrOrder<AIObjectiveFightIntruders>())
				{
					this.character.Speak(TextManager.Get("DialogTargetDown").Value, null, 3f, "targetdown".ToIdentifier(), 30f);
				}
			}
			if (this.ShouldUnequipWeapon)
			{
				this.UnequipWeapon();
			}
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager == null)
			{
				return;
			}
			steeringManager.Reset();
		}

		// Token: 0x06002BA9 RID: 11177 RVA: 0x001E0EBC File Offset: 0x001DF0BC
		protected override void OnAbandon()
		{
			base.OnAbandon();
			if (this.ShouldUnequipWeapon)
			{
				this.UnequipWeapon();
			}
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager == null)
			{
				return;
			}
			steeringManager.Reset();
		}

		// Token: 0x06002BAA RID: 11178 RVA: 0x001E0EE4 File Offset: 0x001DF0E4
		public override void OnDeselected()
		{
			base.OnDeselected();
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.IsOffensiveOrArrest && (!this.AllowHoldFire || (this.hasAimed && this.holdFireTimer <= 0f)))
			{
				this.Enemy.IsCriminal = true;
			}
		}

		// Token: 0x06002BAB RID: 11179 RVA: 0x001E0F38 File Offset: 0x001DF138
		public override void Reset()
		{
			base.Reset();
			this.hasAimed = false;
			this.holdFireTimer = 0f;
			this.pathBackTimer = 0f;
			this.standUpTimer = 0f;
			this.isLethalWeapon = false;
			this.canSeeTarget = false;
			this.seekWeaponObjective = null;
			this.seekAmmunitionObjective = null;
			this.retreatObjective = null;
			this.followTargetObjective = null;
			this.retreatTarget = null;
			this.firstWarningTriggered = false;
			this.lastWarningTriggered = false;
		}

		// Token: 0x06002BAC RID: 11180 RVA: 0x001E0FB2 File Offset: 0x001DF1B2
		private void SpeakNoWeapons()
		{
			if (!this.character.IsInFriendlySub)
			{
				this.PlayerCrewSpeak("dialogcombatnoweapons".ToIdentifier(), 0f, 30f);
			}
		}

		// Token: 0x06002BAD RID: 11181 RVA: 0x001E0FDB File Offset: 0x001DF1DB
		private void PlayerCrewSpeak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			if (this.character.IsOnPlayerTeam)
			{
				this.Speak(textIdentifier, delay, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06002BAE RID: 11182 RVA: 0x001E0FF3 File Offset: 0x001DF1F3
		private void FriendlyGuardSpeak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.character.IsSecurity)
			{
				this.Speak(textIdentifier, delay, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06002BAF RID: 11183 RVA: 0x001E101C File Offset: 0x001DF21C
		private void Speak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			LocalizedString msg = TextManager.Get(textIdentifier);
			if (!msg.IsNullOrEmpty())
			{
				this.character.Speak(msg.Value, null, delay, textIdentifier, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06002BB0 RID: 11184 RVA: 0x001E105C File Offset: 0x001DF25C
		private void SetAimTimer(float newTimer)
		{
			this.aimTimer = Math.Max(this.aimTimer, newTimer);
		}

		// Token: 0x06002BB1 RID: 11185 RVA: 0x001E1070 File Offset: 0x001DF270
		[CompilerGenerated]
		private void <Move>g__BackOff|101_0(ref AIObjectiveCombat.<>c__DisplayClass101_0 A_1, ref AIObjectiveCombat.<>c__DisplayClass101_1 A_2)
		{
			this.RemoveFollowTarget();
			this.isMoving = true;
			if (!this.IsEnemyClose(125f))
			{
				base.ForceWalkTemporarily = true;
			}
			base.HumanAIController.FaceTarget(this.Enemy);
			base.HumanAIController.AutoFaceMovement = false;
			this.character.ReleaseSecondaryItem();
			this.character.AIController.SteeringManager.SteeringManual(A_1.deltaTime, A_2.escapeVel);
		}

		// Token: 0x06002BB9 RID: 11193 RVA: 0x001E1378 File Offset: 0x001DF578
		[CompilerGenerated]
		private bool <TryArm>g__CheckWeapon|102_0(bool seekAmmo)
		{
			return this.character.Inventory.Contains(this.Weapon) && this.WeaponComponent != null && (!this.WeaponComponent.IsEmpty(this.character) || this.Reload(seekAmmo));
		}

		// Token: 0x06002BBA RID: 11194 RVA: 0x001E13C8 File Offset: 0x001DF5C8
		[CompilerGenerated]
		internal static float <GetWeaponPriority>g__ReducePriority|106_0(float prio, float skillLevel, float targetLevel)
		{
			float diff = targetLevel - skillLevel;
			if (diff > 0f)
			{
				prio -= diff;
			}
			return prio;
		}

		// Token: 0x06002BC2 RID: 11202 RVA: 0x001E1564 File Offset: 0x001DF764
		[CompilerGenerated]
		internal static bool <Reload>g__IsInsideHeadset|123_1(Item i)
		{
			Inventory parentInventory = i.ParentInventory;
			Item ownerItem = ((parentInventory != null) ? parentInventory.Owner : null) as Item;
			return ownerItem != null && ownerItem.HasTag(Tags.MobileRadio);
		}

		// Token: 0x040016B0 RID: 5808
		private readonly AIObjectiveCombat.CombatMode initialMode;

		// Token: 0x040016B1 RID: 5809
		private float checkWeaponsTimer;

		// Token: 0x040016B2 RID: 5810
		private const float CheckWeaponsInterval = 1f;

		// Token: 0x040016B3 RID: 5811
		private float ignoreWeaponTimer;

		// Token: 0x040016B4 RID: 5812
		private const float IgnoredWeaponsClearTime = 10f;

		// Token: 0x040016B5 RID: 5813
		private const float GoodWeaponPriority = 30f;

		// Token: 0x040016B6 RID: 5814
		private float holdFireTimer;

		// Token: 0x040016B7 RID: 5815
		private bool hasAimed;

		// Token: 0x040016B8 RID: 5816
		private bool isLethalWeapon;

		// Token: 0x040016B9 RID: 5817
		private bool allowCooldown;

		// Token: 0x040016BB RID: 5819
		private Item _weapon;

		// Token: 0x040016BC RID: 5820
		private ItemComponent _weaponComponent;

		// Token: 0x040016BD RID: 5821
		private bool hasValidRangedWeapon;

		// Token: 0x040016BE RID: 5822
		private readonly AIObjectiveFindSafety findSafety;

		// Token: 0x040016BF RID: 5823
		private readonly HashSet<ItemComponent> weapons = new HashSet<ItemComponent>();

		// Token: 0x040016C0 RID: 5824
		private readonly HashSet<ItemComponent> ignoredWeapons = new HashSet<ItemComponent>();

		// Token: 0x040016C1 RID: 5825
		private AIObjectiveContainItem seekAmmunitionObjective;

		// Token: 0x040016C2 RID: 5826
		private AIObjectiveGoTo retreatObjective;

		// Token: 0x040016C3 RID: 5827
		private AIObjectiveGoTo followTargetObjective;

		// Token: 0x040016C4 RID: 5828
		private AIObjectiveGetItem seekWeaponObjective;

		// Token: 0x040016C5 RID: 5829
		private Hull retreatTarget;

		// Token: 0x040016C6 RID: 5830
		private float coolDownTimer;

		// Token: 0x040016C7 RID: 5831
		private float pathBackTimer;

		// Token: 0x040016C8 RID: 5832
		private const float DefaultCoolDown = 10f;

		// Token: 0x040016C9 RID: 5833
		private const float PathBackCheckTime = 1f;

		// Token: 0x040016CA RID: 5834
		private float aimTimer;

		// Token: 0x040016CB RID: 5835
		private float reloadTimer;

		// Token: 0x040016CC RID: 5836
		private float spreadTimer;

		// Token: 0x040016CD RID: 5837
		private bool canSeeTarget;

		// Token: 0x040016CE RID: 5838
		private float visibilityCheckTimer;

		// Token: 0x040016CF RID: 5839
		private const float VisibilityCheckInterval = 0.2f;

		// Token: 0x040016D0 RID: 5840
		private float sqrDistance;

		// Token: 0x040016D1 RID: 5841
		private const float MaxDistance = 2000f;

		// Token: 0x040016D2 RID: 5842
		private const float DistanceCheckInterval = 0.2f;

		// Token: 0x040016D3 RID: 5843
		private float distanceTimer;

		// Token: 0x040016D4 RID: 5844
		private const float CloseDistance = 300f;

		// Token: 0x040016D5 RID: 5845
		private const float MeleeDistance = 125f;

		// Token: 0x040016D6 RID: 5846
		private const float TooCloseToShoot = 100f;

		// Token: 0x040016D7 RID: 5847
		private const float FloorHeightApproximate = 100f;

		// Token: 0x040016D8 RID: 5848
		public bool AllowHoldFire;

		// Token: 0x040016D9 RID: 5849
		public bool SpeakWarnings;

		// Token: 0x040016DA RID: 5850
		private bool firstWarningTriggered;

		// Token: 0x040016DB RID: 5851
		private bool lastWarningTriggered;

		// Token: 0x040016DD RID: 5853
		private const float ArrestTargetDistance = 100f;

		// Token: 0x040016DE RID: 5854
		private bool arrestingRegistered;

		// Token: 0x040016DF RID: 5855
		public Func<bool> holdFireCondition;

		// Token: 0x040016E1 RID: 5857
		private bool isMoving;

		// Token: 0x040016E2 RID: 5858
		private float findHullTimer;

		// Token: 0x040016E3 RID: 5859
		private const float findHullInterval = 1f;

		// Token: 0x040016E4 RID: 5860
		private bool isAimBlocked;

		// Token: 0x040016E5 RID: 5861
		private float _blockedDistance;

		// Token: 0x040016E6 RID: 5862
		public List<Vector2> BlockedPositions;

		// Token: 0x040016E7 RID: 5863
		private bool allowCrouching;

		// Token: 0x040016E8 RID: 5864
		private float standUpTimer;

		// Token: 0x040016E9 RID: 5865
		private const float StandUpCooldown = 5f;

		// Token: 0x02000DFE RID: 3582
		public enum CombatMode
		{
			// Token: 0x04005128 RID: 20776
			Defensive,
			// Token: 0x04005129 RID: 20777
			Offensive,
			// Token: 0x0400512A RID: 20778
			Arrest,
			// Token: 0x0400512B RID: 20779
			Retreat,
			// Token: 0x0400512C RID: 20780
			None
		}

		// Token: 0x02000DFF RID: 3583
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400512D RID: 20781
			public static Func<InvSlotType, bool> <0>__IsHandSlotType;
		}
	}
}
