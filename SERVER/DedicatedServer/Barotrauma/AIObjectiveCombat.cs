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
	// Token: 0x02000069 RID: 105
	internal class AIObjectiveCombat : AIObjective
	{
		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x06000E66 RID: 3686 RVA: 0x0008B388 File Offset: 0x00089588
		// (set) Token: 0x06000E67 RID: 3687 RVA: 0x0008B390 File Offset: 0x00089590
		public override Identifier Identifier { get; set; } = "combat".ToIdentifier();

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x06000E68 RID: 3688 RVA: 0x0008B39C File Offset: 0x0008959C
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

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06000E69 RID: 3689 RVA: 0x0008B3EB File Offset: 0x000895EB
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x0008B3EE File Offset: 0x000895EE
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x06000E6B RID: 3691 RVA: 0x0008B3F1 File Offset: 0x000895F1
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x06000E6C RID: 3692 RVA: 0x0008B3F4 File Offset: 0x000895F4
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x06000E6D RID: 3693 RVA: 0x0008B3F7 File Offset: 0x000895F7
		private bool AllowCoolDown
		{
			get
			{
				return this.allowCooldown || !this.IsOffensiveOrArrest || this.Mode != this.initialMode || this.character.TeamID == this.Enemy.TeamID;
			}
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06000E6E RID: 3694 RVA: 0x0008B431 File Offset: 0x00089631
		// (set) Token: 0x06000E6F RID: 3695 RVA: 0x0008B439 File Offset: 0x00089639
		public Character Enemy { get; private set; }

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06000E70 RID: 3696 RVA: 0x0008B442 File Offset: 0x00089642
		// (set) Token: 0x06000E71 RID: 3697 RVA: 0x0008B44A File Offset: 0x0008964A
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

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06000E72 RID: 3698 RVA: 0x0008B45A File Offset: 0x0008965A
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

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06000E73 RID: 3699 RVA: 0x0008B47B File Offset: 0x0008967B
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06000E74 RID: 3700 RVA: 0x0008B47E File Offset: 0x0008967E
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06000E75 RID: 3701 RVA: 0x0008B481 File Offset: 0x00089681
		// (set) Token: 0x06000E76 RID: 3702 RVA: 0x0008B489 File Offset: 0x00089689
		public float ArrestHoldFireTime { get; set; } = 10f;

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06000E77 RID: 3703 RVA: 0x0008B492 File Offset: 0x00089692
		// (set) Token: 0x06000E78 RID: 3704 RVA: 0x0008B49A File Offset: 0x0008969A
		public AIObjectiveCombat.CombatMode Mode { get; private set; }

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06000E79 RID: 3705 RVA: 0x0008B4A4 File Offset: 0x000896A4
		private bool IsOffensiveOrArrest
		{
			get
			{
				AIObjectiveCombat.CombatMode combatMode = this.initialMode;
				return combatMode - AIObjectiveCombat.CombatMode.Offensive <= 1;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06000E7A RID: 3706 RVA: 0x0008B4C8 File Offset: 0x000896C8
		private bool TargetEliminated
		{
			get
			{
				return this.IsEnemyDisabled || (this.Enemy.IsUnconscious && this.Enemy.Params.Health.ConstantHealthRegeneration <= 0f) || (!this.character.IsInstigator && this.Enemy.IsHandcuffed && this.Enemy.IsKnockedDown);
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06000E7B RID: 3707 RVA: 0x0008B52F File Offset: 0x0008972F
		private bool IsEnemyDisabled
		{
			get
			{
				return this.Enemy == null || this.Enemy.Removed || this.Enemy.IsDead;
			}
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06000E7C RID: 3708 RVA: 0x0008B553 File Offset: 0x00089753
		private float AimSpeed
		{
			get
			{
				return base.HumanAIController.AimSpeed;
			}
		}

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06000E7D RID: 3709 RVA: 0x0008B560 File Offset: 0x00089760
		private float AimAccuracy
		{
			get
			{
				return base.HumanAIController.AimAccuracy;
			}
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x0008B570 File Offset: 0x00089770
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

		// Token: 0x06000E7F RID: 3711 RVA: 0x0008B638 File Offset: 0x00089838
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

		// Token: 0x06000E80 RID: 3712 RVA: 0x0008B734 File Offset: 0x00089934
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

		// Token: 0x06000E81 RID: 3713 RVA: 0x0008B870 File Offset: 0x00089A70
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

		// Token: 0x06000E82 RID: 3714 RVA: 0x0008B970 File Offset: 0x00089B70
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

		// Token: 0x06000E83 RID: 3715 RVA: 0x0008BC4C File Offset: 0x00089E4C
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

		// Token: 0x06000E84 RID: 3716 RVA: 0x0008BD2C File Offset: 0x00089F2C
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

		// Token: 0x06000E85 RID: 3717 RVA: 0x0008C0D4 File Offset: 0x0008A2D4
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

		// Token: 0x06000E86 RID: 3718 RVA: 0x0008C364 File Offset: 0x0008A564
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

		// Token: 0x06000E87 RID: 3719 RVA: 0x0008C3B4 File Offset: 0x0008A5B4
		private Item FindWeapon(out ItemComponent weaponComponent)
		{
			return this.GetWeapon(this.FindWeaponsFromInventory(), out weaponComponent);
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x0008C3C3 File Offset: 0x0008A5C3
		private static ItemComponent GetWeaponComponent(Item item)
		{
			MeleeWeapon result;
			if ((result = item.GetComponent<MeleeWeapon>()) == null && (result = item.GetComponent<RangedWeapon>()) == null)
			{
				result = (item.GetComponent<RepairTool>() ?? item.GetComponent<Holdable>());
			}
			return result;
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x0008C3EC File Offset: 0x0008A5EC
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

		// Token: 0x06000E8A RID: 3722 RVA: 0x0008C764 File Offset: 0x0008A964
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

		// Token: 0x06000E8B RID: 3723 RVA: 0x0008C81C File Offset: 0x0008AA1C
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

		// Token: 0x06000E8C RID: 3724 RVA: 0x0008C874 File Offset: 0x0008AA74
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

		// Token: 0x06000E8D RID: 3725 RVA: 0x0008CA84 File Offset: 0x0008AC84
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

		// Token: 0x06000E8E RID: 3726 RVA: 0x0008CAAC File Offset: 0x0008ACAC
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

		// Token: 0x06000E8F RID: 3727 RVA: 0x0008CAF4 File Offset: 0x0008ACF4
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

		// Token: 0x06000E90 RID: 3728 RVA: 0x0008CB88 File Offset: 0x0008AD88
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

		// Token: 0x06000E91 RID: 3729 RVA: 0x0008CBFC File Offset: 0x0008ADFC
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

		// Token: 0x06000E92 RID: 3730 RVA: 0x0008CC4C File Offset: 0x0008AE4C
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

		// Token: 0x06000E93 RID: 3731 RVA: 0x0008CDA4 File Offset: 0x0008AFA4
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

		// Token: 0x06000E94 RID: 3732 RVA: 0x0008CFCC File Offset: 0x0008B1CC
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

		// Token: 0x06000E95 RID: 3733 RVA: 0x0008D32C File Offset: 0x0008B52C
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

		// Token: 0x06000E96 RID: 3734 RVA: 0x0008D368 File Offset: 0x0008B568
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

		// Token: 0x06000E97 RID: 3735 RVA: 0x0008D5A0 File Offset: 0x0008B7A0
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

		// Token: 0x06000E98 RID: 3736 RVA: 0x0008D628 File Offset: 0x0008B828
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

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06000E99 RID: 3737 RVA: 0x0008D82C File Offset: 0x0008BA2C
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

		// Token: 0x06000E9A RID: 3738 RVA: 0x0008D860 File Offset: 0x0008BA60
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

		// Token: 0x06000E9B RID: 3739 RVA: 0x0008DD58 File Offset: 0x0008BF58
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

		// Token: 0x06000E9C RID: 3740 RVA: 0x0008DDD4 File Offset: 0x0008BFD4
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

		// Token: 0x06000E9D RID: 3741 RVA: 0x0008DE24 File Offset: 0x0008C024
		private void SetReloadTime(ItemComponent weaponComponent)
		{
			float reloadTime = this.GetReloadTime(weaponComponent);
			this.reloadTimer = Math.Max(reloadTime, reloadTime * Rand.Range(1f, 1.25f, Rand.RandSync.Unsynced) / this.AimSpeed);
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x0008DE5E File Offset: 0x0008C05E
		private void ClearInputs()
		{
			this.character.ClearInput(InputType.Aim);
			this.character.ClearInput(InputType.Shoot);
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06000E9F RID: 3743 RVA: 0x0008DE7C File Offset: 0x0008C07C
		private bool ShouldUnequipWeapon
		{
			get
			{
				return this.Weapon != null && this.character.Submarine != null && this.character.Submarine.TeamID == this.character.TeamID && Character.CharacterList.None((Character c) => c.Submarine == this.character.Submarine && HumanAIController.IsActive(c) && !HumanAIController.IsFriendly(this.character, c, false, false) && base.HumanAIController.VisibleHulls.Contains(c.CurrentHull));
			}
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x0008DED4 File Offset: 0x0008C0D4
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

		// Token: 0x06000EA1 RID: 3745 RVA: 0x0008E028 File Offset: 0x0008C228
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

		// Token: 0x06000EA2 RID: 3746 RVA: 0x0008E050 File Offset: 0x0008C250
		public override void OnDeselected()
		{
			base.OnDeselected();
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.IsOffensiveOrArrest && (!this.AllowHoldFire || (this.hasAimed && this.holdFireTimer <= 0f)))
			{
				this.Enemy.IsCriminal = true;
			}
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x0008E0A4 File Offset: 0x0008C2A4
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

		// Token: 0x06000EA4 RID: 3748 RVA: 0x0008E11E File Offset: 0x0008C31E
		private void SpeakNoWeapons()
		{
			if (!this.character.IsInFriendlySub)
			{
				this.PlayerCrewSpeak("dialogcombatnoweapons".ToIdentifier(), 0f, 30f);
			}
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x0008E147 File Offset: 0x0008C347
		private void PlayerCrewSpeak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			if (this.character.IsOnPlayerTeam)
			{
				this.Speak(textIdentifier, delay, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06000EA6 RID: 3750 RVA: 0x0008E15F File Offset: 0x0008C35F
		private void FriendlyGuardSpeak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && this.character.IsSecurity)
			{
				this.Speak(textIdentifier, delay, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x0008E188 File Offset: 0x0008C388
		private void Speak(Identifier textIdentifier, float delay, float minDurationBetweenSimilar)
		{
			LocalizedString msg = TextManager.Get(textIdentifier);
			if (!msg.IsNullOrEmpty())
			{
				this.character.Speak(msg.Value, null, delay, textIdentifier, minDurationBetweenSimilar);
			}
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x0008E1C8 File Offset: 0x0008C3C8
		private void SetAimTimer(float newTimer)
		{
			this.aimTimer = Math.Max(this.aimTimer, newTimer);
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x0008E1DC File Offset: 0x0008C3DC
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

		// Token: 0x06000EB1 RID: 3761 RVA: 0x0008E4E4 File Offset: 0x0008C6E4
		[CompilerGenerated]
		private bool <TryArm>g__CheckWeapon|102_0(bool seekAmmo)
		{
			return this.character.Inventory.Contains(this.Weapon) && this.WeaponComponent != null && (!this.WeaponComponent.IsEmpty(this.character) || this.Reload(seekAmmo));
		}

		// Token: 0x06000EB2 RID: 3762 RVA: 0x0008E534 File Offset: 0x0008C734
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

		// Token: 0x06000EBA RID: 3770 RVA: 0x0008E6D0 File Offset: 0x0008C8D0
		[CompilerGenerated]
		internal static bool <Reload>g__IsInsideHeadset|123_1(Item i)
		{
			Inventory parentInventory = i.ParentInventory;
			Item ownerItem = ((parentInventory != null) ? parentInventory.Owner : null) as Item;
			return ownerItem != null && ownerItem.HasTag(Tags.MobileRadio);
		}

		// Token: 0x040006B6 RID: 1718
		private readonly AIObjectiveCombat.CombatMode initialMode;

		// Token: 0x040006B7 RID: 1719
		private float checkWeaponsTimer;

		// Token: 0x040006B8 RID: 1720
		private const float CheckWeaponsInterval = 1f;

		// Token: 0x040006B9 RID: 1721
		private float ignoreWeaponTimer;

		// Token: 0x040006BA RID: 1722
		private const float IgnoredWeaponsClearTime = 10f;

		// Token: 0x040006BB RID: 1723
		private const float GoodWeaponPriority = 30f;

		// Token: 0x040006BC RID: 1724
		private float holdFireTimer;

		// Token: 0x040006BD RID: 1725
		private bool hasAimed;

		// Token: 0x040006BE RID: 1726
		private bool isLethalWeapon;

		// Token: 0x040006BF RID: 1727
		private bool allowCooldown;

		// Token: 0x040006C1 RID: 1729
		private Item _weapon;

		// Token: 0x040006C2 RID: 1730
		private ItemComponent _weaponComponent;

		// Token: 0x040006C3 RID: 1731
		private bool hasValidRangedWeapon;

		// Token: 0x040006C4 RID: 1732
		private readonly AIObjectiveFindSafety findSafety;

		// Token: 0x040006C5 RID: 1733
		private readonly HashSet<ItemComponent> weapons = new HashSet<ItemComponent>();

		// Token: 0x040006C6 RID: 1734
		private readonly HashSet<ItemComponent> ignoredWeapons = new HashSet<ItemComponent>();

		// Token: 0x040006C7 RID: 1735
		private AIObjectiveContainItem seekAmmunitionObjective;

		// Token: 0x040006C8 RID: 1736
		private AIObjectiveGoTo retreatObjective;

		// Token: 0x040006C9 RID: 1737
		private AIObjectiveGoTo followTargetObjective;

		// Token: 0x040006CA RID: 1738
		private AIObjectiveGetItem seekWeaponObjective;

		// Token: 0x040006CB RID: 1739
		private Hull retreatTarget;

		// Token: 0x040006CC RID: 1740
		private float coolDownTimer;

		// Token: 0x040006CD RID: 1741
		private float pathBackTimer;

		// Token: 0x040006CE RID: 1742
		private const float DefaultCoolDown = 10f;

		// Token: 0x040006CF RID: 1743
		private const float PathBackCheckTime = 1f;

		// Token: 0x040006D0 RID: 1744
		private float aimTimer;

		// Token: 0x040006D1 RID: 1745
		private float reloadTimer;

		// Token: 0x040006D2 RID: 1746
		private float spreadTimer;

		// Token: 0x040006D3 RID: 1747
		private bool canSeeTarget;

		// Token: 0x040006D4 RID: 1748
		private float visibilityCheckTimer;

		// Token: 0x040006D5 RID: 1749
		private const float VisibilityCheckInterval = 0.2f;

		// Token: 0x040006D6 RID: 1750
		private float sqrDistance;

		// Token: 0x040006D7 RID: 1751
		private const float MaxDistance = 2000f;

		// Token: 0x040006D8 RID: 1752
		private const float DistanceCheckInterval = 0.2f;

		// Token: 0x040006D9 RID: 1753
		private float distanceTimer;

		// Token: 0x040006DA RID: 1754
		private const float CloseDistance = 300f;

		// Token: 0x040006DB RID: 1755
		private const float MeleeDistance = 125f;

		// Token: 0x040006DC RID: 1756
		private const float TooCloseToShoot = 100f;

		// Token: 0x040006DD RID: 1757
		private const float FloorHeightApproximate = 100f;

		// Token: 0x040006DE RID: 1758
		public bool AllowHoldFire;

		// Token: 0x040006DF RID: 1759
		public bool SpeakWarnings;

		// Token: 0x040006E0 RID: 1760
		private bool firstWarningTriggered;

		// Token: 0x040006E1 RID: 1761
		private bool lastWarningTriggered;

		// Token: 0x040006E3 RID: 1763
		private const float ArrestTargetDistance = 100f;

		// Token: 0x040006E4 RID: 1764
		private bool arrestingRegistered;

		// Token: 0x040006E5 RID: 1765
		public Func<bool> holdFireCondition;

		// Token: 0x040006E7 RID: 1767
		private bool isMoving;

		// Token: 0x040006E8 RID: 1768
		private float findHullTimer;

		// Token: 0x040006E9 RID: 1769
		private const float findHullInterval = 1f;

		// Token: 0x040006EA RID: 1770
		private bool isAimBlocked;

		// Token: 0x040006EB RID: 1771
		private float _blockedDistance;

		// Token: 0x040006EC RID: 1772
		public List<Vector2> BlockedPositions;

		// Token: 0x040006ED RID: 1773
		private bool allowCrouching;

		// Token: 0x040006EE RID: 1774
		private float standUpTimer;

		// Token: 0x040006EF RID: 1775
		private const float StandUpCooldown = 5f;

		// Token: 0x020007B6 RID: 1974
		public enum CombatMode
		{
			// Token: 0x04002DC8 RID: 11720
			Defensive,
			// Token: 0x04002DC9 RID: 11721
			Offensive,
			// Token: 0x04002DCA RID: 11722
			Arrest,
			// Token: 0x04002DCB RID: 11723
			Retreat,
			// Token: 0x04002DCC RID: 11724
			None
		}

		// Token: 0x020007B7 RID: 1975
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002DCD RID: 11725
			public static Func<InvSlotType, bool> <0>__IsHandSlotType;
		}
	}
}
