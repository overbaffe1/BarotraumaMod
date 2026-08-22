using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000020 RID: 32
	internal class EnemyAIController : AIController
	{
		// Token: 0x060001A2 RID: 418 RVA: 0x00009434 File Offset: 0x00007634
		public override void DebugDraw(SpriteBatch spriteBatch)
		{
			if (!this.Character.IsUnconscious && this.Character.Enabled && this.Enabled)
			{
				Screen selected = Screen.Selected;
				Camera camera = (selected != null) ? selected.Cam : null;
				if (camera == null || camera.Zoom >= 0.4f)
				{
					Vector2 pos = this.Character.DrawPosition;
					pos.Y = -pos.Y;
					if (this.State == AIState.Idle && this.PreviousState == AIState.Attack)
					{
						AITarget target = this._selectedAiTarget ?? this._lastAiTarget;
						if (target != null && target.Entity != null)
						{
							AITargetMemory memory = this.GetTargetMemory(target, false, false);
							if (memory != null)
							{
								Vector2 targetPos = memory.Location;
								targetPos.Y = -targetPos.Y;
								GUI.DrawLine(spriteBatch, pos, targetPos, Color.White * 0.5f, 0f, 4f);
								Vector2 pos2 = pos - Vector2.UnitY * 60f;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
								defaultInterpolatedStringHandler.AppendFormatted<Entity>(target.Entity);
								defaultInterpolatedStringHandler.AppendLiteral(" (");
								defaultInterpolatedStringHandler.AppendFormatted(memory.Priority.FormatZeroDecimal());
								defaultInterpolatedStringHandler.AppendLiteral(")");
								GUI.DrawString(spriteBatch, pos2, defaultInterpolatedStringHandler.ToStringAndClear(), Color.White, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
							}
						}
					}
					else
					{
						AITarget selectedAiTarget = base.SelectedAiTarget;
						if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) != null && this.AttackLimb != null)
						{
							Vector2 targetPos2 = base.SelectedAiTarget.Entity.DrawPosition;
							if (this.State == AIState.Attack)
							{
								targetPos2 = this.attackWorldPos;
							}
							targetPos2.Y = -targetPos2.Y;
							Vector2 attackLimbPos = this.AttackLimb.DrawPosition;
							attackLimbPos.Y = -attackLimbPos.Y;
							GUI.DrawLine(spriteBatch, attackLimbPos, targetPos2, GUIStyle.Red * 0.75f, 0f, 4f);
							if (this.wallTarget != null && !this.IsCoolDownRunning)
							{
								Vector2 wallTargetPos = this.wallTarget.Position;
								if (this.wallTarget.Structure.Submarine != null)
								{
									wallTargetPos += this.wallTarget.Structure.Submarine.DrawPosition;
								}
								wallTargetPos.Y = -wallTargetPos.Y;
								GUI.DrawRectangle(spriteBatch, wallTargetPos - new Vector2(10f, 10f), new Vector2(20f, 20f), Color.Orange, false, 0f, 1f);
								GUI.DrawLine(spriteBatch, attackLimbPos, wallTargetPos, Color.Orange * 0.75f, 0f, 5f);
							}
							Vector2 pos3 = pos - Vector2.UnitY * 60f;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(0, 1);
							defaultInterpolatedStringHandler2.AppendFormatted<Entity>(base.SelectedAiTarget.Entity);
							GUI.DrawString(spriteBatch, pos3, defaultInterpolatedStringHandler2.ToStringAndClear(), GUIStyle.Red, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
							Vector2 pos4 = pos - Vector2.UnitY * 40f;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(11, 3);
							defaultInterpolatedStringHandler3.AppendFormatted(this.targetValue.FormatZeroDecimal());
							defaultInterpolatedStringHandler3.AppendLiteral(" (M: ");
							AITargetMemory aitargetMemory = this.CurrentTargetMemory;
							defaultInterpolatedStringHandler3.AppendFormatted((aitargetMemory != null) ? aitargetMemory.Priority.FormatZeroDecimal() : null);
							defaultInterpolatedStringHandler3.AppendLiteral(", P: ");
							CharacterParams.TargetParams targetParams = this.CurrentTargetingParams;
							defaultInterpolatedStringHandler3.AppendFormatted((targetParams != null) ? targetParams.Priority.FormatZeroDecimal() : null);
							defaultInterpolatedStringHandler3.AppendLiteral(")");
							GUI.DrawString(spriteBatch, pos4, defaultInterpolatedStringHandler3.ToStringAndClear(), GUIStyle.Red, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
						}
					}
					Color stateColor = Color.White;
					switch (this.State)
					{
					case AIState.Attack:
						stateColor = (this.IsCoolDownRunning ? Color.Orange : GUIStyle.Red);
						break;
					case AIState.Escape:
						stateColor = Color.LightBlue;
						break;
					case AIState.Eat:
						stateColor = Color.Brown;
						break;
					case AIState.Flee:
						stateColor = Color.White;
						break;
					}
					GUI.DrawString(spriteBatch, pos - Vector2.UnitY * 80f, this.State.ToString(), stateColor, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
					if (this.State == AIState.Attack && this.currentTargetingParams != null && this.currentTargetingParams.AttackPattern == AttackPattern.Circle)
					{
						GUI.DrawString(spriteBatch, pos - Vector2.UnitY * 100f, this.CirclePhase.ToString(), stateColor, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
					}
					if (this.LatchOntoAI != null && (this.State == AIState.Idle || this.LatchOntoAI.IsAttachedToSub))
					{
						foreach (Joint attachJoint in this.LatchOntoAI.AttachJoints)
						{
							GUI.DrawLine(spriteBatch, ConvertUnits.ToDisplayUnits(new Vector2(attachJoint.WorldAnchorA.X, -attachJoint.WorldAnchorA.Y)), ConvertUnits.ToDisplayUnits(new Vector2(attachJoint.WorldAnchorB.X, -attachJoint.WorldAnchorB.Y)), GUIStyle.Green, 0f, 4f);
						}
						if (this.LatchOntoAI.AttachPos != null)
						{
							GUI.DrawLine(spriteBatch, pos, ConvertUnits.ToDisplayUnits(new Vector2(this.LatchOntoAI.AttachPos.Value.X, -this.LatchOntoAI.AttachPos.Value.Y)), GUIStyle.Green, 0f, 3f);
						}
					}
					IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
					if (pathSteering != null)
					{
						SteeringPath path = pathSteering.CurrentPath;
						if (path != null)
						{
							if (path.CurrentNode != null)
							{
								GUI.DrawLine(spriteBatch, pos, new Vector2(path.CurrentNode.DrawPosition.X, -path.CurrentNode.DrawPosition.Y), Color.DarkViolet, 0f, 3f);
								GUI.DrawString(spriteBatch, pos - new Vector2(0f, 100f), "Path cost: " + path.Cost.FormatZeroDecimal(), Color.White, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
							}
							for (int i = 1; i < path.Nodes.Count; i++)
							{
								WayPoint previousNode = path.Nodes[i - 1];
								WayPoint currentNode = path.Nodes[i];
								GUI.DrawLine(spriteBatch, new Vector2(currentNode.DrawPosition.X, -currentNode.DrawPosition.Y), new Vector2(previousNode.DrawPosition.X, -previousNode.DrawPosition.Y), GUIStyle.Red * 0.5f, 0f, 3f);
								GUIStyle.SmallFont.DrawString(spriteBatch, currentNode.ID.ToString(), new Vector2(currentNode.DrawPosition.X - 10f, -currentNode.DrawPosition.Y - 30f), GUIStyle.Red, ForceUpperCase.Inherit, false);
							}
						}
					}
					else if (this.steeringManager.AvoidDir.LengthSquared() > 0.0001f)
					{
						Vector2 hitPos = ConvertUnits.ToDisplayUnits(this.steeringManager.AvoidRayCastHitPosition);
						hitPos.Y = -hitPos.Y;
						GUI.DrawLine(spriteBatch, hitPos, hitPos + new Vector2(this.steeringManager.AvoidDir.X, -this.steeringManager.AvoidDir.Y) * 100f, GUIStyle.Red, 0f, 5f);
					}
					GUI.DrawLine(spriteBatch, pos, pos + ConvertUnits.ToDisplayUnits(new Vector2(base.Steering.X, -base.Steering.Y)), Color.Blue, 0f, 4f);
					GUI.DrawLine(spriteBatch, pos, pos + ConvertUnits.ToDisplayUnits(new Vector2(this.Character.AnimController.TargetMovement.X, -this.Character.AnimController.TargetMovement.Y)), Color.SteelBlue, 0f, 2f);
					return;
				}
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060001A3 RID: 419 RVA: 0x00009CF0 File Offset: 0x00007EF0
		// (set) Token: 0x060001A4 RID: 420 RVA: 0x00009CF8 File Offset: 0x00007EF8
		public AIState State
		{
			get
			{
				return this._state;
			}
			set
			{
				if (this._state == value)
				{
					return;
				}
				if (this._state == AIState.PlayDead && value == AIState.Idle)
				{
					return;
				}
				this.PreviousState = this._state;
				this.OnStateChanged(this._state, value);
				this._state = value;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x00009D32 File Offset: 0x00007F32
		// (set) Token: 0x060001A6 RID: 422 RVA: 0x00009D3A File Offset: 0x00007F3A
		public AIState PreviousState { get; private set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x00009D43 File Offset: 0x00007F43
		private IndoorsSteeringManager PathSteering
		{
			get
			{
				return this.insideSteering as IndoorsSteeringManager;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x00009D50 File Offset: 0x00007F50
		private bool IsAttackRunning
		{
			get
			{
				return this.AttackLimb != null && this.AttackLimb.attack.IsRunning;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00009D6C File Offset: 0x00007F6C
		private bool IsCoolDownRunning
		{
			get
			{
				return (this.AttackLimb != null && this.AttackLimb.attack.CoolDownTimer > 0f) || (this._previousAttackLimb != null && this._previousAttackLimb.attack.CoolDownTimer > 0f);
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060001AA RID: 426 RVA: 0x00009DBB File Offset: 0x00007FBB
		public float CombatStrength
		{
			get
			{
				return this.AIParams.CombatStrength;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060001AB RID: 427 RVA: 0x00009DC8 File Offset: 0x00007FC8
		private float Sight
		{
			get
			{
				return this.GetPerceptionRange(this.AIParams.Sight);
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060001AC RID: 428 RVA: 0x00009DDB File Offset: 0x00007FDB
		private float Hearing
		{
			get
			{
				return this.GetPerceptionRange(this.AIParams.Hearing);
			}
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00009DF0 File Offset: 0x00007FF0
		private float GetPerceptionRange(float range)
		{
			AIState aistate = this.State;
			bool flag = aistate == AIState.PlayDead || aistate == AIState.Hiding;
			if (flag)
			{
				return 0.2f;
			}
			aistate = this.PreviousState;
			flag = (aistate == AIState.PlayDead || aistate == AIState.Hiding);
			if (flag)
			{
				return range * 1.5f;
			}
			return range;
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060001AE RID: 430 RVA: 0x00009E40 File Offset: 0x00008040
		private float FleeHealthThreshold
		{
			get
			{
				return this.AIParams.FleeHealthThreshold;
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060001AF RID: 431 RVA: 0x00009E4D File Offset: 0x0000804D
		private bool IsAggressiveBoarder
		{
			get
			{
				return this.AIParams.AggressiveBoarding;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x00009E5A File Offset: 0x0000805A
		private FishAnimController FishAnimController
		{
			get
			{
				return this.Character.AnimController as FishAnimController;
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001B1 RID: 433 RVA: 0x00009E6C File Offset: 0x0000806C
		// (set) Token: 0x060001B2 RID: 434 RVA: 0x00009E74 File Offset: 0x00008074
		public Limb AttackLimb
		{
			get
			{
				return this._attackLimb;
			}
			private set
			{
				if (this._attackLimb != value)
				{
					this._previousAttackLimb = this._attackLimb;
					if (this._previousAttackLimb != null)
					{
						this.Character.DeselectCharacter();
						if (this._previousAttackLimb.attack.SnapRopeOnNewAttack)
						{
							Rope attachedRope = this._previousAttackLimb.AttachedRope;
							if (attachedRope != null)
							{
								attachedRope.Snap();
							}
						}
					}
				}
				else if (this._attackLimb != null && this._attackLimb.attack.CoolDownTimer <= 0f)
				{
					this.Character.DeselectCharacter();
					if (this._attackLimb.attack.SnapRopeOnNewAttack)
					{
						Rope attachedRope2 = this._attackLimb.AttachedRope;
						if (attachedRope2 != null)
						{
							attachedRope2.Snap();
						}
					}
				}
				this._attackLimb = value;
				this.attackVector = null;
				this.Reverse = (this._attackLimb != null && this._attackLimb.attack.Reverse);
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x00009F5C File Offset: 0x0000815C
		// (set) Token: 0x060001B4 RID: 436 RVA: 0x00009F8A File Offset: 0x0000818A
		public Attack ActiveAttack
		{
			get
			{
				if (this._activeAttack == null)
				{
					return null;
				}
				if (this.lastAttackUpdateTime <= Timing.TotalTime - (double)this._activeAttack.Duration)
				{
					return null;
				}
				return this._activeAttack;
			}
			private set
			{
				this._activeAttack = value;
				this.lastAttackUpdateTime = Timing.TotalTime;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060001B5 RID: 437 RVA: 0x00009F9E File Offset: 0x0000819E
		public AITargetMemory CurrentTargetMemory
		{
			get
			{
				return this.currentTargetMemory;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x00009FA6 File Offset: 0x000081A6
		public bool CanAttackDoors
		{
			get
			{
				return this.canAttackDoors;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001B7 RID: 439 RVA: 0x00009FAE File Offset: 0x000081AE
		public float PriorityFearIncrement
		{
			get
			{
				return this.priorityFearIncreasement;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060001B8 RID: 440 RVA: 0x00009FB6 File Offset: 0x000081B6
		// (set) Token: 0x060001B9 RID: 441 RVA: 0x00009FBE File Offset: 0x000081BE
		public LatchOntoAI LatchOntoAI { get; private set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060001BA RID: 442 RVA: 0x00009FC7 File Offset: 0x000081C7
		// (set) Token: 0x060001BB RID: 443 RVA: 0x00009FCF File Offset: 0x000081CF
		public SwarmBehavior SwarmBehavior { get; private set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00009FD8 File Offset: 0x000081D8
		// (set) Token: 0x060001BD RID: 445 RVA: 0x00009FE0 File Offset: 0x000081E0
		public PetBehavior PetBehavior { get; private set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00009FE9 File Offset: 0x000081E9
		public CharacterParams.TargetParams CurrentTargetingParams
		{
			get
			{
				return this.currentTargetingParams;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00009FF1 File Offset: 0x000081F1
		public bool AttackHumans
		{
			get
			{
				return this.GetTargetParams(Tags.Human).Any(delegate(CharacterParams.TargetParams tp)
				{
					if (tp != null)
					{
						float priority = tp.Priority;
						if (priority > 0f)
						{
							AIState state = tp.State;
							if (state == AIState.Attack || state == AIState.Aggressive)
							{
								return true;
							}
						}
					}
					return false;
				});
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x0000A022 File Offset: 0x00008222
		public bool AttackRooms
		{
			get
			{
				return this.GetTargetParams(Tags.Room).Any(delegate(CharacterParams.TargetParams tp)
				{
					if (tp != null)
					{
						float priority = tp.Priority;
						if (priority > 0f)
						{
							AIState state = tp.State;
							if (state == AIState.Attack || state == AIState.Aggressive)
							{
								return true;
							}
						}
					}
					return false;
				});
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x0000A054 File Offset: 0x00008254
		public override CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null && latchOntoAI.IsAttachedToSub)
				{
					return CanEnterSubmarine.False;
				}
				return this.Character.AnimController.CanEnterSubmarine;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x0000A088 File Offset: 0x00008288
		public override bool CanFlip
		{
			get
			{
				return !this.Reverse && (this.State != AIState.Eat || this.Character.SelectedCharacter == null) && (this.LatchOntoAI == null || !this.LatchOntoAI.IsAttachedToSub) && (this.Character.CurrentHull == null || !this.Character.AnimController.InWater || Math.Min(this.Character.CurrentHull.Size.X, this.Character.CurrentHull.Size.Y) > ConvertUnits.ToDisplayUnits(Math.Max(this.colliderLength, this.colliderWidth)));
			}
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000A138 File Offset: 0x00008338
		public void SetUnattackableSubmarines(Submarine submarine, bool includeOwnSub = true, bool includeConnectedSubs = true, bool clearExisting = true)
		{
			EnemyAIController.<>c__DisplayClass109_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.includeConnectedSubs = includeConnectedSubs;
			if (clearExisting)
			{
				this.unattackableSubmarines.Clear();
			}
			if (submarine != null)
			{
				this.<SetUnattackableSubmarines>g__AddSubs|109_0(submarine, ref CS$<>8__locals1);
			}
			if (includeOwnSub)
			{
				Submarine ownSub = this.Character.Submarine;
				if (ownSub != null && ownSub != submarine)
				{
					this.<SetUnattackableSubmarines>g__AddSubs|109_0(ownSub, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000A190 File Offset: 0x00008390
		public static bool IsTargetBeingChasedBy(Character target, Character character)
		{
			EnemyAIController enemyAI = ((character != null) ? character.AIController : null) as EnemyAIController;
			bool flag;
			if (enemyAI != null)
			{
				AITarget selectedAiTarget = enemyAI.SelectedAiTarget;
				flag = (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == target);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			bool flag3 = flag2;
			if (flag3)
			{
				AIState state = enemyAI.State;
				bool flag4 = state == AIState.Attack || state == AIState.Aggressive;
				flag3 = flag4;
			}
			return flag3;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000A1EB File Offset: 0x000083EB
		public bool IsBeingChasedBy(Character c)
		{
			return EnemyAIController.IsTargetBeingChasedBy(this.Character, c);
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x0000A1F9 File Offset: 0x000083F9
		private bool IsBeingChased
		{
			get
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				return this.IsBeingChasedBy(((selectedAiTarget != null) ? selectedAiTarget.Entity : null) as Character);
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000A218 File Offset: 0x00008418
		private static bool IsTargetInPlayerTeam(AITarget target)
		{
			bool flag;
			if (target == null)
			{
				flag = (null != null);
			}
			else
			{
				Entity entity = target.Entity;
				flag = (((entity != null) ? entity.Submarine : null) != null);
			}
			if (!flag || !target.Entity.Submarine.Info.IsPlayer)
			{
				Character character = ((target != null) ? target.Entity : null) as Character;
				return character != null && character.IsOnPlayerTeam;
			}
			return true;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000A278 File Offset: 0x00008478
		private bool IsAttackingOwner(Character other)
		{
			if (this.PetBehavior != null && this.PetBehavior.Owner != null && !other.IsUnconscious && !other.IsHandcuffed)
			{
				HumanAIController humanAI = other.AIController as HumanAIController;
				if (humanAI != null)
				{
					AIObjectiveCombat combat = humanAI.ObjectiveManager.CurrentObjective as AIObjectiveCombat;
					if (combat != null && combat.Enemy != null)
					{
						return combat.Enemy == this.PetBehavior.Owner;
					}
				}
			}
			return false;
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x0000A2EA File Offset: 0x000084EA
		// (set) Token: 0x060001CA RID: 458 RVA: 0x0000A2F2 File Offset: 0x000084F2
		public bool Reverse
		{
			get
			{
				return this.reverse;
			}
			private set
			{
				this.reverse = value;
				if (this.FishAnimController != null)
				{
					this.FishAnimController.Reverse = this.reverse;
				}
			}
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000A314 File Offset: 0x00008514
		public EnemyAIController(Character c, string seed) : base(c)
		{
			if (c.IsHuman)
			{
				throw new Exception("Tried to create an enemy ai controller for human!");
			}
			ContentXElement mainElement = c.Params.OriginalElement.IsOverride() ? c.Params.OriginalElement.FirstElement() : c.Params.OriginalElement;
			this.targetMemories = new Dictionary<AITarget, AITargetMemory>();
			this.steeringManager = this.outsideSteering;
			bool targetOutposts;
			if (Level.Loaded == null || Level.Loaded.Type != LevelData.LevelType.Outpost)
			{
				Submarine mainSub = Submarine.MainSub;
				if (mainSub != null)
				{
					SubmarineInfo info = mainSub.Info;
					if (info != null)
					{
						targetOutposts = (info.Type == SubmarineType.Outpost);
						goto IL_1D3;
					}
				}
				targetOutposts = false;
			}
			else
			{
				targetOutposts = true;
			}
			IL_1D3:
			this.TargetOutposts = targetOutposts;
			List<XElement> aiElements = new List<XElement>();
			List<float> aiCommonness = new List<float>();
			foreach (ContentXElement element in mainElement.Elements())
			{
				if (element.Name.ToString().Equals("ai", StringComparison.OrdinalIgnoreCase))
				{
					aiElements.Add(element);
					aiCommonness.Add(element.GetAttributeFloat("commonness", 1f));
				}
			}
			if (aiElements.Count == 0)
			{
				string str = "Error in file \"";
				ContentPath path = c.Params.File.Path;
				string error = str + ((path != null) ? path.ToString() : null) + "\" - no AI element found.";
				Exception e = null;
				CharacterPrefab prefab = c.Prefab;
				DebugConsole.ThrowError(error, e, (prefab != null) ? prefab.ContentPackage : null, false, false);
				this.outsideSteering = new SteeringManager(this);
				this.insideSteering = new IndoorsSteeringManager(this, false, false);
				return;
			}
			MTRandom random = new MTRandom(ToolBox.StringToInt(seed));
			XElement aiElement = (aiElements.Count == 1) ? aiElements[0] : ToolBox.SelectWeightedRandom<XElement>(aiElements, aiCommonness, random);
			foreach (XElement subElement in aiElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "chooserandom")
				{
					IEnumerable<XElement> subElements = subElement.Elements();
					if (subElements.Any<XElement>())
					{
						this.<.ctor>g__LoadSubElement|124_0(subElements.ToArray<XElement>().GetRandom(random));
					}
				}
				else
				{
					this.<.ctor>g__LoadSubElement|124_0(subElement);
				}
			}
			if (this.PetBehavior == null)
			{
				Identifier group = this.Character.Group;
				if (!(group == "human"))
				{
					goto IL_38F;
				}
			}
			this.Character.TeamID = CharacterTeamType.FriendlyNPC;
			IL_38F:
			this.ReevaluateAttacks();
			this.outsideSteering = new SteeringManager(this);
			this.insideSteering = new IndoorsSteeringManager(this, this.AIParams.CanOpenDoors, this.canAttackDoors);
			this.steeringManager = this.outsideSteering;
			this.State = AIState.Idle;
			this.requiredHoleCount = (int)Math.Ceiling((double)(ConvertUnits.ToDisplayUnits(this.colliderWidth) / 96f));
			this.myBodies = (from l in this.Character.AnimController.Limbs
			select l.body.FarseerBody).ToList<Body>();
			this.myBodies.Add(this.Character.AnimController.Collider.FarseerBody);
			if (this.AIParams.PlayDeadProbability > 0f)
			{
				this.Character.EvaluatePlayDeadProbability(null);
			}
			CreatureMetrics.UnlockInEditor(this.Character.SpeciesName);
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060001CC RID: 460 RVA: 0x0000A7C4 File Offset: 0x000089C4
		public CharacterParams.AIParams AIParams
		{
			get
			{
				if (this._aiParams == null)
				{
					this._aiParams = this.Character.Params.AI;
					if (this._aiParams == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
						defaultInterpolatedStringHandler.AppendLiteral("No AI Params defined for ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Character.SpeciesName);
						defaultInterpolatedStringHandler.AppendLiteral(". AI disabled.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Character.Prefab.ContentPackage, false, false);
						this.Enabled = false;
						this._aiParams = new CharacterParams.AIParams(null, this.Character.Params);
					}
				}
				return this._aiParams;
			}
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000A871 File Offset: 0x00008A71
		private IEnumerable<CharacterParams.TargetParams> GetTargetParams(Identifier targetTag)
		{
			return this.AIParams.GetTargets(targetTag);
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000A87F File Offset: 0x00008A7F
		private IEnumerable<CharacterParams.TargetParams> GetTargetParams(IEnumerable<Identifier> targetingTags)
		{
			EnemyAIController.<GetTargetParams>d__129 <GetTargetParams>d__ = new EnemyAIController.<GetTargetParams>d__129(-2);
			<GetTargetParams>d__.<>4__this = this;
			<GetTargetParams>d__.<>3__targetingTags = targetingTags;
			return <GetTargetParams>d__;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000A898 File Offset: 0x00008A98
		private IEnumerable<Identifier> GetTargetingTags(AITarget aiTarget)
		{
			this._targetingTags.Clear();
			if (((aiTarget != null) ? aiTarget.Entity : null) == null)
			{
				return this._targetingTags;
			}
			Character targetCharacter = aiTarget.Entity as Character;
			if (targetCharacter != null)
			{
				CharacterParams.TargetParams tp;
				CharacterParams.TargetParams tP;
				if (targetCharacter.IsDead)
				{
					this._targetingTags.Add(Tags.Dead);
				}
				else if (this.AIParams.TryGetHighestPriorityTarget(targetCharacter.CharacterHealth.GetActiveAfflictionTags(), out tp) && tp.Threshold >= this.Character.GetDamageDoneByAttacker(targetCharacter))
				{
					this._targetingTags.Add(tp.Tag);
				}
				else if (this.PetBehavior != null && aiTarget.Entity == this.PetBehavior.Owner)
				{
					this._targetingTags.Add(Tags.Owner);
				}
				else if (this.PetBehavior != null && (!this.Character.IsOnFriendlyTeam(targetCharacter) || this.IsAttackingOwner(targetCharacter)))
				{
					this._targetingTags.Add(Tags.Hostile);
				}
				else if (this.AIParams.TryGetHighestPriorityTarget(targetCharacter, out tP))
				{
					this._targetingTags.Add(tP.Tag);
				}
				else
				{
					EnemyAIController enemy = targetCharacter.AIController as EnemyAIController;
					if (enemy != null)
					{
						if (enemy.PetBehavior != null && (this.PetBehavior != null || this.AIParams.HasTag(Tags.Pet)))
						{
							this._targetingTags.Add(Tags.Pet);
						}
						else if (targetCharacter.IsHusk && this.AIParams.HasTag(Tags.Husk))
						{
							this._targetingTags.Add(Tags.Husk);
						}
						else if (!this.Character.IsSameSpeciesOrGroup(targetCharacter))
						{
							if (enemy.CombatStrength > this.CombatStrength)
							{
								this._targetingTags.Add(Tags.Stronger);
							}
							else if (enemy.CombatStrength < this.CombatStrength)
							{
								this._targetingTags.Add(Tags.Weaker);
							}
							else
							{
								this._targetingTags.Add(Tags.Equal);
							}
						}
					}
				}
			}
			else
			{
				Item targetItem = aiTarget.Entity as Item;
				if (targetItem != null)
				{
					foreach (CharacterParams.TargetParams prio in this.AIParams.Targets)
					{
						if (targetItem.HasTag(prio.Tag))
						{
							this._targetingTags.Add(prio.Tag);
						}
					}
					if (this._targetingTags.None(null))
					{
						if (targetItem.GetComponent<Sonar>() != null)
						{
							this._targetingTags.Add(Tags.Sonar);
						}
						if (targetItem.GetComponent<Door>() != null)
						{
							this._targetingTags.Add(Tags.Door);
						}
					}
				}
				else if (aiTarget.Entity is Structure)
				{
					this._targetingTags.Add(Tags.Wall);
				}
				else if (aiTarget.Entity is Hull)
				{
					this._targetingTags.Add(Tags.Room);
				}
			}
			return this._targetingTags;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000ABA4 File Offset: 0x00008DA4
		public override void SelectTarget(AITarget target)
		{
			this.SelectTarget(target, 100f);
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000ABB2 File Offset: 0x00008DB2
		public void SelectTarget(AITarget target, float priority)
		{
			base.SelectedAiTarget = target;
			this.currentTargetMemory = this.GetTargetMemory(target, true, false);
			this.currentTargetMemory.Priority = priority;
			this.ignoredTargets.Remove(target);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000ABE4 File Offset: 0x00008DE4
		private void ReleaseDragTargets()
		{
			Limb attackLimb = this.AttackLimb;
			if (attackLimb != null)
			{
				Rope attachedRope = attackLimb.AttachedRope;
				if (attachedRope != null)
				{
					attachedRope.Snap();
				}
			}
			if (this.Character.Params.CanInteract && this.Character.Inventory != null)
			{
				this.Character.HeldItems.ForEach(delegate(Item i)
				{
					Holdable component = i.GetComponent<Holdable>();
					if (component == null)
					{
						return;
					}
					Rope rope = component.GetRope();
					if (rope == null)
					{
						return;
					}
					rope.Snap();
				});
			}
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000AC5B File Offset: 0x00008E5B
		public void EvaluatePlayDeadProbability(float? probability = null)
		{
			if (probability != null)
			{
				this.AIParams.PlayDeadProbability = probability.Value;
			}
			this.Character.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= this.AIParams.PlayDeadProbability);
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000AC9C File Offset: 0x00008E9C
		public override void Update(float deltaTime)
		{
			if (EnemyAIController.DisableEnemyAI)
			{
				return;
			}
			base.Update(deltaTime);
			this.UpdateTriggers(deltaTime);
			this.Character.ClearInputs();
			base.IsTryingToSteerThroughGap = false;
			this.Reverse = false;
			this.Character.UpdateTeam();
			this.HandleLaddersAndPlatforms(deltaTime);
			if (Math.Abs(this.Character.AnimController.movement.X) > 0.1f && !this.Character.AnimController.InWater && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer || Character.Controlled == this.Character))
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) != null || base.EscapeTarget != null)
				{
					AITarget selectedAiTarget2 = base.SelectedAiTarget;
					Entity t = ((selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null) ?? base.EscapeTarget;
					float referencePos = (Vector2.DistanceSquared(this.Character.WorldPosition, t.WorldPosition) > 10000f && base.HasValidPath(true, true, null)) ? this.PathSteering.CurrentPath.CurrentNode.WorldPosition.X : t.WorldPosition.X;
					this.Character.AnimController.TargetDir = ((this.Character.WorldPosition.X < referencePos) ? Direction.Right : Direction.Left);
				}
				else
				{
					this.Character.AnimController.TargetDir = ((this.Character.AnimController.movement.X > 0f) ? Direction.Right : Direction.Left);
				}
			}
			if (this.isStateChanged && (this.State == AIState.Idle || this.State == AIState.Patrol))
			{
				this.stateResetTimer -= deltaTime;
				if (this.stateResetTimer <= 0f)
				{
					this.ResetOriginalState();
				}
			}
			if (this.targetIgnoreTimer > 0f)
			{
				this.targetIgnoreTimer -= deltaTime;
			}
			else
			{
				this.ignoredTargets.Clear();
				this.targetIgnoreTimer = this.targetIgnoreTime;
			}
			this.avoidTimer -= deltaTime;
			if (this.avoidTimer < 0f)
			{
				this.avoidTimer = 0f;
			}
			this.UpdateCurrentMemoryLocation();
			if (this.updateMemoriesTimer > 0f)
			{
				this.updateMemoriesTimer -= deltaTime;
			}
			else
			{
				this.FadeMemories(this.updateMemoriesInverval);
				this.updateMemoriesTimer = this.updateMemoriesInverval;
			}
			if (Math.Max(this.Character.HealthPercentage, 0f) < this.FleeHealthThreshold && base.SelectedAiTarget != null)
			{
				Character target = base.SelectedAiTarget.Entity as Character;
				if (target == null)
				{
					Item targetItem = base.SelectedAiTarget.Entity as Item;
					if (targetItem != null)
					{
						target = EnemyAIController.GetOwner(targetItem);
					}
				}
				bool shouldFlee = false;
				if (target != null)
				{
					shouldFlee = ((target.IsHuman && this.CanPerceive(base.SelectedAiTarget, -1f, -1f, false)) || this.IsBeingChasedBy(target));
				}
				this.State = (shouldFlee ? AIState.Flee : AIState.Idle);
				this.wallTarget = null;
				if (this.State != AIState.Flee)
				{
					base.SelectedAiTarget = null;
					this._lastAiTarget = null;
				}
			}
			else
			{
				if (EnemyAIController.TargetingRestrictions != this.previousTargetingRestrictions)
				{
					this.previousTargetingRestrictions = EnemyAIController.TargetingRestrictions;
					this.updateTargetsTimer = 0f;
					base.SelectedAiTarget = null;
				}
				if (this.updateTargetsTimer > 0f)
				{
					this.updateTargetsTimer -= deltaTime;
				}
				else if (this.avoidTimer <= 0f || (this.activeTriggers.Any<KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams>>() && this.returnTimer <= 0f))
				{
					this.UpdateTargets();
				}
			}
			if (this.Character.Params.UsePathFinding && this.AIParams.UsePathFindingToGetInside && this.AIParams.CanOpenDoors)
			{
				if (this.Character.Submarine != null || (base.HasValidPath(true, true, null) && this.<Update>g__IsCloseEnoughToTargetSub|137_0(this.maxSteeringBuffer)) || this.<Update>g__IsCloseEnoughToTargetSub|137_0(this.steeringBuffer))
				{
					if (this.steeringManager != this.insideSteering)
					{
						this.insideSteering.Reset();
					}
					this.steeringManager = this.insideSteering;
					this.steeringBuffer += this.steeringBufferIncreaseSpeed * deltaTime;
				}
				else
				{
					if (this.steeringManager != this.outsideSteering)
					{
						this.outsideSteering.Reset();
					}
					this.steeringManager = this.outsideSteering;
					this.steeringBuffer = this.minSteeringBuffer;
				}
				this.steeringBuffer = Math.Clamp(this.steeringBuffer, this.minSteeringBuffer, this.maxSteeringBuffer);
			}
			else if (this.Character.Submarine != null && this.Character.Params.UsePathFinding)
			{
				if (this.steeringManager != this.insideSteering)
				{
					this.insideSteering.Reset();
				}
				this.steeringManager = this.insideSteering;
			}
			else
			{
				if (this.steeringManager != this.outsideSteering)
				{
					this.outsideSteering.Reset();
				}
				this.steeringManager = this.outsideSteering;
			}
			bool useSteeringLengthAsMovementSpeed = this.State == AIState.Idle && this.Character.AnimController.InWater;
			bool run = false;
			switch (this.State)
			{
			case AIState.Idle:
				this.UpdateIdle(deltaTime, true);
				break;
			case AIState.Attack:
				run = (!this.IsCoolDownRunning || (this.AttackLimb != null && this.AttackLimb.attack.FullSpeedAfterAttack));
				this.UpdateAttack(deltaTime);
				break;
			case AIState.Escape:
			case AIState.Flee:
				run = true;
				this.Escape(deltaTime);
				break;
			case AIState.Eat:
				this.UpdateEating(deltaTime);
				break;
			case AIState.Avoid:
			case AIState.Aggressive:
			case AIState.PassiveAggressive:
			{
				AITarget selectedAiTarget3 = base.SelectedAiTarget;
				if (((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				float squaredDistance = Vector2.DistanceSquared(base.WorldPosition, base.SelectedAiTarget.WorldPosition);
				Limb attackLimb = this.AttackLimb ?? this.GetAttackLimb(base.SelectedAiTarget.WorldPosition, null);
				if (attackLimb != null && (double)squaredDistance <= Math.Pow((double)attackLimb.attack.Range, 2.0))
				{
					run = true;
					if (this.State == AIState.Avoid)
					{
						this.Escape(deltaTime);
					}
					else
					{
						this.UpdateAttack(deltaTime);
					}
				}
				else
				{
					bool isBeingChased = this.IsBeingChased;
					float num;
					if (!isBeingChased)
					{
						CharacterParams.TargetParams targetParams = this.currentTargetingParams;
						if (targetParams != null && targetParams.ReactDistance > 0f)
						{
							num = this.currentTargetingParams.ReactDistance;
							goto IL_6DC;
						}
					}
					num = this.GetPerceivingRange(base.SelectedAiTarget);
					IL_6DC:
					float reactDistance = num;
					if ((double)squaredDistance <= Math.Pow((double)reactDistance, 2.0))
					{
						float halfReactDistance = reactDistance / 2f;
						CharacterParams.TargetParams targetParams = this.currentTargetingParams;
						float attackDistance = (targetParams != null && targetParams.AttackDistance > 0f) ? this.currentTargetingParams.AttackDistance : halfReactDistance;
						if (this.State == AIState.Aggressive || (this.State == AIState.PassiveAggressive && (double)squaredDistance < Math.Pow((double)attackDistance, 2.0)))
						{
							run = true;
							this.UpdateAttack(deltaTime);
						}
						else
						{
							run = (isBeingChased || (double)squaredDistance < Math.Pow((double)halfReactDistance, 2.0));
							this.State = AIState.Escape;
							this.avoidTimer = this.AIParams.AvoidTime * 0.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
						}
					}
					else
					{
						this.UpdateIdle(deltaTime, true);
					}
				}
				break;
			}
			case AIState.Protect:
			case AIState.Follow:
			case AIState.FleeTo:
			case AIState.HideTo:
			case AIState.Hiding:
			{
				AITarget selectedAiTarget4 = base.SelectedAiTarget;
				if (((selectedAiTarget4 != null) ? selectedAiTarget4.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				if (this.State == AIState.Protect)
				{
					EnemyAIController.<>c__DisplayClass137_0 CS$<>8__locals1 = new EnemyAIController.<>c__DisplayClass137_0();
					CS$<>8__locals1.<>4__this = this;
					Entity entity = base.SelectedAiTarget.Entity;
					CS$<>8__locals1.targetCharacter = (entity as Character);
					if (CS$<>8__locals1.targetCharacter != null)
					{
						Character.Attacker attacker2 = CS$<>8__locals1.targetCharacter.LastAttackers.LastOrDefault(new Func<Character.Attacker, bool>(CS$<>8__locals1.<Update>g__ShouldRetaliate|1));
						Character attacker = (attacker2 != null) ? attacker2.Character : null;
						if (((attacker != null) ? attacker.AiTarget : null) != null)
						{
							this.ChangeTargetState(attacker, AIState.Attack, new float?(this.currentTargetingParams.Priority * 2f));
							this.SelectTarget(attacker.AiTarget);
							this.State = AIState.Attack;
							this.UpdateWallTarget(this.requiredHoleCount);
							return;
						}
					}
				}
				float distX = Math.Abs(base.WorldPosition.X - base.SelectedAiTarget.WorldPosition.X);
				float distY = Math.Abs(base.WorldPosition.Y - base.SelectedAiTarget.WorldPosition.Y);
				if (this.Character.Submarine != null && distY > 50f)
				{
					Character targetC = base.SelectedAiTarget.Entity as Character;
					if (targetC != null && !base.VisibleHulls.Contains(targetC.CurrentHull))
					{
						distY *= 3f;
					}
				}
				float dist = distX + distY;
				float reactDist = this.GetPerceivingRange(base.SelectedAiTarget);
				Vector2 offset = Vector2.Zero;
				if (this.currentTargetingParams != null)
				{
					if (this.currentTargetingParams.ReactDistance > 0f)
					{
						reactDist = this.currentTargetingParams.ReactDistance;
					}
					offset = this.currentTargetingParams.Offset;
				}
				if (offset != Vector2.Zero)
				{
					reactDist += offset.Length();
				}
				if (dist > reactDist + this.movementMargin)
				{
					AIState state = this.State;
					bool flag = state == AIState.FleeTo || state - AIState.HideTo <= 1;
					this.movementMargin = (flag ? 0f : reactDist);
					if (this.State == AIState.Hiding)
					{
						this.State = AIState.HideTo;
					}
					run = true;
					this.UpdateFollow(deltaTime);
				}
				else
				{
					if (this.State == AIState.HideTo)
					{
						this.State = AIState.Hiding;
					}
					this.movementMargin = MathHelper.Clamp(this.movementMargin -= deltaTime, 0f, reactDist);
					AIState state = this.State;
					bool flag = state == AIState.FleeTo || state == AIState.Hiding;
					if (flag)
					{
						base.SteeringManager.Reset();
						this.Character.AnimController.TargetMovement = Vector2.Zero;
						if (this.Character.AnimController.InWater)
						{
							float force = this.Character.AnimController.Collider.Mass / 10f;
							this.Character.AnimController.Collider.MoveToPos(base.SelectedAiTarget.Entity.SimPosition + ConvertUnits.ToSimUnits(offset), force, null);
							Item item = base.SelectedAiTarget.Entity as Item;
							if (item != null)
							{
								float rotation = item.Rotation;
								this.Character.AnimController.Collider.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.SteerTorque, true);
								Limb mainLimb = this.Character.AnimController.MainLimb;
								if (mainLimb.type == LimbType.Head)
								{
									mainLimb.body.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.HeadTorque, true);
								}
								else
								{
									mainLimb.body.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.TorsoTorque, true);
								}
							}
							if (this.disableTailCoroutine == null)
							{
								Item i = base.SelectedAiTarget.Entity as Item;
								if (i != null && i.HasTag(Tags.GuardianShelter) && !CoroutineManager.IsCoroutineRunning(this.disableTailCoroutine))
								{
									this.disableTailCoroutine = CoroutineManager.Invoke(delegate
									{
										Character character = this.Character;
										if (character != null && !character.Removed)
										{
											this.Character.AnimController.HideAndDisable(LimbType.Tail, 0f, false);
										}
									}, 1f);
								}
							}
							this.Character.AnimController.ApplyPose(new Vector2(0f, -1f), new Vector2(0f, -1f), new Vector2(0f, -1f), new Vector2(0f, -1f), 1f);
						}
					}
					else
					{
						this.UpdateIdle(deltaTime, true);
					}
				}
				break;
			}
			case AIState.Observe:
			{
				AITarget selectedAiTarget5 = base.SelectedAiTarget;
				if (((selectedAiTarget5 != null) ? selectedAiTarget5.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				run = false;
				float sqrDist = Vector2.DistanceSquared(base.WorldPosition, base.SelectedAiTarget.WorldPosition);
				CharacterParams.TargetParams targetParams = this.currentTargetingParams;
				float reactDist = (targetParams != null && targetParams.ReactDistance > 0f) ? this.currentTargetingParams.ReactDistance : this.GetPerceivingRange(base.SelectedAiTarget);
				float halfReactDist = reactDist / 2f;
				targetParams = this.currentTargetingParams;
				float attackDist = (targetParams != null && targetParams.AttackDistance > 0f) ? this.currentTargetingParams.AttackDistance : halfReactDist;
				if ((double)sqrDist > Math.Pow((double)reactDist, 2.0))
				{
					this.UpdateIdle(deltaTime, true);
				}
				else if ((double)sqrDist < Math.Pow((double)(attackDist + this.movementMargin), 2.0))
				{
					this.movementMargin = attackDist;
					base.SteeringManager.Reset();
					if (this.Character.AnimController.InWater)
					{
						useSteeringLengthAsMovementSpeed = true;
						Vector2 dir = Vector2.Normalize(base.SelectedAiTarget.WorldPosition - this.Character.WorldPosition);
						if ((double)sqrDist < Math.Pow((double)(attackDist * 0.75f), 2.0))
						{
							dir = -dir;
							useSteeringLengthAsMovementSpeed = false;
							this.Reverse = true;
							run = true;
						}
						base.SteeringManager.SteeringManual(deltaTime, dir * 0.2f);
					}
					else
					{
						base.FaceTarget(base.SelectedAiTarget.Entity);
					}
					this.observeTimer -= deltaTime;
					if (this.observeTimer < 0f)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						this.State = AIState.Idle;
						base.ResetAITarget();
					}
				}
				else
				{
					run = ((double)sqrDist > Math.Pow((double)(attackDist * 2f), 2.0));
					this.movementMargin = MathHelper.Clamp(this.movementMargin -= deltaTime, 0f, attackDist);
					this.UpdateFollow(deltaTime);
				}
				break;
			}
			case AIState.Freeze:
				base.SteeringManager.Reset();
				break;
			case AIState.Patrol:
				this.UpdatePatrol(deltaTime, true);
				break;
			case AIState.PlayDead:
				this.Character.IsRagdolled = true;
				break;
			default:
				throw new NotImplementedException();
			}
			if (!this.Character.AnimController.SimplePhysicsEnabled)
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.Update(this, deltaTime);
				}
			}
			base.IsSteeringThroughGap = false;
			if (this.SwarmBehavior != null)
			{
				this.SwarmBehavior.IsActive = (this.SwarmBehavior.ForceActive || (this.State == AIState.Idle && this.Character.CurrentHull == null));
				this.SwarmBehavior.Refresh();
				this.SwarmBehavior.UpdateSteering(deltaTime);
			}
			this.SteerInsideLevel(deltaTime);
			float speed = this.Character.AnimController.GetCurrentSpeed(run && this.Character.CanRun);
			this.steeringManager.Update(Math.Max(speed, 1f));
			float movementSpeed = useSteeringLengthAsMovementSpeed ? base.Steering.Length() : speed;
			this.Character.AnimController.TargetMovement = this.Character.ApplyMovementLimits(base.Steering, movementSpeed);
			if (this.Character.CurrentHull != null && this.Character.AnimController.InWater)
			{
				this.Character.AnimController.TargetMovement = this.Character.AnimController.TargetMovement.ClampLength(5f);
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000BC64 File Offset: 0x00009E64
		private void HandleLaddersAndPlatforms(float deltaTime)
		{
			bool ignorePlatforms = this.Character.AnimController.TargetMovement.Y < -0.5f && -this.Character.AnimController.TargetMovement.Y > Math.Abs(this.Character.AnimController.TargetMovement.X);
			if (this.steeringManager == this.insideSteering)
			{
				SteeringPath currPath = this.PathSteering.CurrentPath;
				if (currPath != null)
				{
					WayPoint currentNode = currPath.CurrentNode;
					if (currentNode != null)
					{
						Vector2 colliderBottom = this.Character.AnimController.GetColliderBottom();
						if (this.Character.Submarine != currentNode.Submarine)
						{
							colliderBottom = Submarine.GetRelativeSimPosition(colliderBottom, currentNode.Submarine, this.Character.Submarine);
						}
						if (currentNode.SimPosition.Y < colliderBottom.Y)
						{
							float allowedJumpHeight = this.Character.AnimController.ImpactTolerance / 2f;
							Vector2 diff = currentNode.WorldPosition - this.Character.WorldPosition;
							float height = ConvertUnits.ToSimUnits(Math.Abs(diff.Y));
							ignorePlatforms = (height < allowedJumpHeight);
							if (ignorePlatforms && !this.Character.CanClimb && this.PathSteering.IsCurrentNodeLadder && ConvertUnits.ToSimUnits(Math.Abs(diff.X)) < this.Character.AnimController.Collider.GetMaxExtent())
							{
								if (this.lastDroppingTime < Timing.TotalTime - 5.0)
								{
									this.Character.IsRagdolled = true;
									this.Character.SetInput(InputType.Ragdoll, false, true);
									this.droppingTimer += deltaTime;
									if (this.droppingTimer > 1f)
									{
										this.lastDroppingTime = Timing.TotalTime;
									}
								}
								else
								{
									this.droppingTimer = 0f;
								}
							}
						}
					}
				}
				if (this.Character.IsClimbing && this.PathSteering.IsNextLadderSameAsCurrent)
				{
					this.Character.AnimController.TargetMovement = new Vector2(0f, (float)Math.Sign(this.Character.AnimController.TargetMovement.Y));
				}
			}
			this.Character.AnimController.IgnorePlatforms = ignorePlatforms;
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000BEA4 File Offset: 0x0000A0A4
		private void UpdateIdle(float deltaTime, bool followLastTarget = true)
		{
			if (this.Character.AllowPlayDead && this.Character.Submarine != null)
			{
				if (this.playDeadTimer > 0f)
				{
					this.playDeadTimer -= deltaTime;
				}
				else
				{
					this.State = AIState.PlayDead;
				}
			}
			else if (this.AIParams.PatrolFlooded || this.AIParams.PatrolDry)
			{
				this.State = AIState.Patrol;
			}
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (pathSteering == null && Level.Loaded != null && Level.Loaded.GetRealWorldDepth(base.WorldPosition.Y) > this.Character.CharacterHealth.CrushDepth * 0.75f)
			{
				base.SteeringManager.SteeringManual(deltaTime, Vector2.UnitY);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
				return;
			}
			if (followLastTarget)
			{
				AITarget target = base.SelectedAiTarget ?? this._lastAiTarget;
				Entity entity = (target != null) ? target.Entity : null;
				if (entity != null && !entity.Removed && this.PreviousState == AIState.Attack && this.Character.CurrentHull == null)
				{
					Limb previousAttackLimb = this._previousAttackLimb;
					if (((previousAttackLimb != null) ? previousAttackLimb.attack : null) != null)
					{
						Limb previousAttackLimb2 = this._previousAttackLimb;
						Attack previousAttack = (previousAttackLimb2 != null) ? previousAttackLimb2.attack : null;
						if (previousAttack == null || (previousAttack.AfterAttack == AIBehaviorAfterAttack.FallBack && previousAttack.CoolDownTimer > 0f))
						{
							goto IL_1F3;
						}
					}
					AITargetMemory memory = this.GetTargetMemory(target, false, false);
					if (memory != null)
					{
						Vector2 location = memory.Location;
						float dist = Vector2.DistanceSquared(base.WorldPosition, location);
						Vector2 vector;
						if (dist >= 2500f && this.IsPositionInsideAllowedZone(base.WorldPosition, out vector))
						{
							base.SteeringManager.SteeringSeek(this.Character.GetRelativeSimPosition(target.Entity, new Vector2?(location)), 5f);
							base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
							return;
						}
						base.ResetAITarget();
					}
					else
					{
						base.ResetAITarget();
					}
				}
			}
			IL_1F3:
			if (!this.Character.IsClimbing)
			{
				if (pathSteering != null && !this.Character.AnimController.InWater)
				{
					pathSteering.Wander(deltaTime, Math.Max(ConvertUnits.ToDisplayUnits(this.colliderLength), 100f), false);
					return;
				}
				this.steeringManager.SteeringWander(1f, true);
				if (this.Character.AnimController.InWater)
				{
					base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
				}
			}
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000C120 File Offset: 0x0000A320
		private void UpdatePatrol(float deltaTime, bool followLastTarget = true)
		{
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				if (this.patrolTarget == null || base.IsCurrentPathUnreachable || base.IsCurrentPathFinished)
				{
					this.newPatrolTargetTimer = Math.Min(this.newPatrolTargetTimer, this.newPatrolTargetIntervalMin);
				}
				if (this.newPatrolTargetTimer > 0f)
				{
					this.newPatrolTargetTimer -= deltaTime;
				}
				else if (!this.searchingNewHull)
				{
					this.searchingNewHull = true;
					this.FindTargetHulls();
				}
				else if (this.targetHulls.Any<Hull>())
				{
					this.patrolTarget = ToolBox.SelectWeightedRandom<Hull>(this.targetHulls, this.hullWeights, Rand.RandSync.Unsynced);
					SteeringPath path = this.PathSteering.PathFinder.FindPath(this.Character.SimPosition, this.patrolTarget.SimPosition, this.Character.Submarine, null, this.minGapSize * 1.5f, null, null, (PathNode n) => this.<UpdatePatrol>g__PatrolNodeFilter|152_0(n), true, 0f);
					if (path.Unreachable)
					{
						int index = this.targetHulls.IndexOf(this.patrolTarget);
						this.targetHulls.RemoveAt(index);
						this.hullWeights.RemoveAt(index);
						this.PathSteering.Reset();
						this.patrolTarget = null;
						this.patrolTimerMargin += 0.5f;
						this.patrolTimerMargin = Math.Min(this.patrolTimerMargin, this.newPatrolTargetIntervalMin);
						this.newPatrolTargetTimer = Math.Min(this.newPatrolTargetIntervalMin, this.patrolTimerMargin);
					}
					else
					{
						this.PathSteering.SetPath(this.patrolTarget.SimPosition, path);
						this.patrolTimerMargin = 0f;
						this.newPatrolTargetTimer = this.newPatrolTargetIntervalMax * Rand.Range(0.5f, 1.5f, Rand.RandSync.Unsynced);
						this.searchingNewHull = false;
					}
				}
				else
				{
					this.newPatrolTargetTimer = this.newPatrolTargetIntervalMax;
					this.searchingNewHull = false;
				}
				if (this.patrolTarget != null)
				{
					SteeringPath currentPath = pathSteering.CurrentPath;
					if (currentPath != null && !currentPath.Finished && !currentPath.Unreachable)
					{
						pathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(this.patrolTarget, null), 1f, this.minGapSize * 1.5f, null, null, new Func<PathNode, bool>(this.<UpdatePatrol>g__PatrolNodeFilter|152_0), true, 0f);
						return;
					}
				}
			}
			this.UpdateIdle(deltaTime, followLastTarget);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000C378 File Offset: 0x0000A578
		private void FindTargetHulls()
		{
			if (this.Character.Submarine == null)
			{
				return;
			}
			if (this.Character.CurrentHull == null)
			{
				return;
			}
			this.targetHulls.Clear();
			this.hullWeights.Clear();
			float hullMinSize = ConvertUnits.ToDisplayUnits(Math.Max(this.colliderLength, this.colliderWidth) * 2f);
			bool checkWaterLevel = !this.AIParams.PatrolFlooded || !this.AIParams.PatrolDry;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine != null && hull.Submarine.TeamID == this.Character.Submarine.TeamID && this.Character.Submarine.IsConnectedTo(hull.Submarine) && (float)hull.RectWidth >= hullMinSize && (float)hull.RectHeight >= hullMinSize && (!checkWaterLevel || ((!this.AIParams.PatrolDry || hull.WaterPercentage <= 50f) && (!this.AIParams.PatrolFlooded || hull.WaterPercentage >= 80f))) && (!this.AIParams.PatrolDry || hull.WaterPercentage >= 80f || Math.Abs(this.Character.CurrentHull.WorldPosition.Y - hull.WorldPosition.Y) <= this.Character.CurrentHull.CeilingHeight / 2f) && !this.targetHulls.Contains(hull))
				{
					this.targetHulls.Add(hull);
					float weight = hull.Size.Combine();
					float dist = Vector2.Distance(this.Character.WorldPosition, hull.WorldPosition);
					float optimal = 1000f;
					float max = 3000f;
					float distanceFactor = (dist > optimal) ? MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(optimal, max, dist)) : MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, optimal, dist));
					float waterFactor = 1f;
					if (checkWaterLevel)
					{
						waterFactor = (this.AIParams.PatrolDry ? MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(0f, 100f, hull.WaterPercentage)) : MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 100f, hull.WaterPercentage)));
					}
					weight *= distanceFactor * waterFactor;
					this.hullWeights.Add(weight);
				}
			}
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000C648 File Offset: 0x0000A848
		private bool IsSameTarget(AITarget target, AITarget otherTarget)
		{
			return ((target != null) ? target.Entity : null) == ((otherTarget != null) ? otherTarget.Entity : null) || (EnemyAIController.<IsSameTarget>g__IsItemInCharacterInventory|158_0(target, otherTarget) || EnemyAIController.<IsSameTarget>g__IsItemInCharacterInventory|158_0(otherTarget, target));
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000C67C File Offset: 0x0000A87C
		private void UpdateAttack(float deltaTime)
		{
			EnemyAIController.<>c__DisplayClass159_0 CS$<>8__locals1 = new EnemyAIController.<>c__DisplayClass159_0();
			CS$<>8__locals1.<>4__this = this;
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null || base.SelectedAiTarget.Entity.Removed || this.currentTargetingParams == null)
			{
				this.State = AIState.Idle;
				return;
			}
			if (this.Character.IsAttachedToController())
			{
				return;
			}
			this.attackWorldPos = base.SelectedAiTarget.WorldPosition;
			this.attackSimPos = base.SelectedAiTarget.SimPosition;
			Item item = base.SelectedAiTarget.Entity as Item;
			if (item != null)
			{
				Character owner = EnemyAIController.GetOwner(item);
				if (owner != null)
				{
					if (this.Character.IsFriendly(owner) || owner.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
					{
						base.ResetAITarget();
						this.State = AIState.Idle;
						return;
					}
					base.SelectedAiTarget = owner.AiTarget;
				}
			}
			if (this.wallTarget != null)
			{
				this.attackWorldPos = this.wallTarget.Position;
				if (this.wallTarget.Structure.Submarine != null)
				{
					this.attackWorldPos += this.wallTarget.Structure.Submarine.Position;
				}
				this.attackSimPos = ((this.Character.Submarine == this.wallTarget.Structure.Submarine) ? this.wallTarget.Position : this.attackWorldPos);
				this.attackSimPos = ConvertUnits.ToSimUnits(this.attackSimPos);
			}
			else
			{
				this.attackSimPos = this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null);
			}
			if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True)
			{
				if (this.TrySteerThroughGaps(deltaTime))
				{
					return;
				}
			}
			else
			{
				Structure w = base.SelectedAiTarget.Entity as Structure;
				if (w != null && this.wallTarget == null)
				{
					bool isBroken = true;
					for (int i = 0; i < w.Sections.Length; i++)
					{
						if (!w.SectionBodyDisabled(i))
						{
							isBroken = false;
							Vector2 sectionPos = w.SectionPosition(i, true);
							this.attackWorldPos = sectionPos;
							this.attackSimPos = ConvertUnits.ToSimUnits(this.attackWorldPos);
							break;
						}
					}
					if (isBroken)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						this.State = AIState.Idle;
						base.ResetAITarget();
						return;
					}
				}
			}
			this.attackLimbSelectionTimer -= deltaTime;
			if (this.AttackLimb == null || this.attackLimbSelectionTimer <= 0f)
			{
				this.attackLimbSelectionTimer = this.attackLimbSelectionInterval * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
				if (!this.IsAttackRunning && !this.IsCoolDownRunning)
				{
					this.AttackLimb = this.GetAttackLimb(this.attackWorldPos, null);
				}
			}
			CS$<>8__locals1.targetCharacter = (base.SelectedAiTarget.Entity as Character);
			IDamageable damageable;
			if (this.wallTarget == null)
			{
				damageable = (base.SelectedAiTarget.Entity as IDamageable);
			}
			else
			{
				IDamageable structure = this.wallTarget.Structure;
				damageable = structure;
			}
			IDamageable damageTarget = damageable;
			CS$<>8__locals1.canAttack = !this.Character.IsClimbing;
			bool pursue = false;
			if (this.IsCoolDownRunning && (this._previousAttackLimb == null || this.AttackLimb == null || this.AttackLimb.attack.CoolDownTimer > 0f))
			{
				Limb currentAttackLimb = this.AttackLimb ?? this._previousAttackLimb;
				if (currentAttackLimb.attack.CoolDownTimer >= currentAttackLimb.attack.CoolDown + currentAttackLimb.attack.CurrentRandomCoolDown - currentAttackLimb.attack.AfterAttackDelay)
				{
					return;
				}
				currentAttackLimb.attack.AfterAttackTimer += deltaTime;
				AIBehaviorAfterAttack activeBehavior = (currentAttackLimb.attack.AfterAttackSecondaryDelay > 0f && currentAttackLimb.attack.AfterAttackTimer > currentAttackLimb.attack.AfterAttackSecondaryDelay) ? currentAttackLimb.attack.AfterAttackSecondary : currentAttackLimb.attack.AfterAttack;
				switch (activeBehavior)
				{
				case AIBehaviorAfterAttack.FallBackUntilCanAttack:
				case AIBehaviorAfterAttack.FollowThroughUntilCanAttack:
				case AIBehaviorAfterAttack.ReverseUntilCanAttack:
				{
					if (activeBehavior == AIBehaviorAfterAttack.ReverseUntilCanAttack)
					{
						this.Reverse = true;
					}
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					Limb newLimb = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
					if (newLimb != null)
					{
						this.AttackLimb = newLimb;
						goto IL_699;
					}
					this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
					return;
				}
				case AIBehaviorAfterAttack.PursueIfCanAttack:
				case AIBehaviorAfterAttack.Pursue:
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						if (activeBehavior == AIBehaviorAfterAttack.Pursue)
						{
							CS$<>8__locals1.canAttack = false;
							pursue = true;
							goto IL_699;
						}
						this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
						return;
					}
					else
					{
						if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
						{
							CS$<>8__locals1.canAttack = false;
							goto IL_699;
						}
						if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
						{
							CS$<>8__locals1.canAttack = false;
							if (activeBehavior == AIBehaviorAfterAttack.PursueIfCanAttack)
							{
								this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
								return;
							}
							this.AttackLimb = null;
							goto IL_699;
						}
						else
						{
							Limb newLimb2 = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
							if (newLimb2 != null)
							{
								this.AttackLimb = newLimb2;
								goto IL_699;
							}
							if (activeBehavior == AIBehaviorAfterAttack.Pursue)
							{
								CS$<>8__locals1.canAttack = false;
								pursue = true;
								goto IL_699;
							}
							this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
							return;
						}
					}
					break;
				case AIBehaviorAfterAttack.Eat:
					if (currentAttackLimb.IsSevered)
					{
						this.ReleaseEatingTarget();
						return;
					}
					this.UpdateEating(deltaTime);
					return;
				case AIBehaviorAfterAttack.FollowThrough:
					this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
					return;
				case AIBehaviorAfterAttack.FollowThroughWithoutObstacleAvoidance:
					this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, false);
					return;
				case AIBehaviorAfterAttack.IdleUntilCanAttack:
				{
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					Limb newLimb3 = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
					if (newLimb3 != null)
					{
						this.AttackLimb = newLimb3;
						goto IL_699;
					}
					this.UpdateIdle(deltaTime, false);
					return;
				}
				}
				if (activeBehavior == AIBehaviorAfterAttack.Reverse)
				{
					this.Reverse = true;
				}
				this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
				return;
			}
			else
			{
				this.attackVector = null;
			}
			IL_699:
			if (CS$<>8__locals1.canAttack)
			{
				Limb attackLimb2 = this.AttackLimb;
				if (attackLimb2 != null && attackLimb2.IsSevered)
				{
					this.AttackLimb = null;
				}
				if (this.AttackLimb != null)
				{
					Limb attackLimb3 = this.AttackLimb;
					IEnumerable<AttackContext> attackContexts = this.Character.GetAttackContexts();
					AITarget selectedAiTarget2 = base.SelectedAiTarget;
					if (this.IsValidAttack(attackLimb3, attackContexts, (selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null))
					{
						goto IL_706;
					}
				}
				this.AttackLimb = this.GetAttackLimb(this.attackWorldPos, null);
				IL_706:
				CS$<>8__locals1.canAttack = (this.AttackLimb != null && this.AttackLimb.attack.CoolDownTimer <= 0f);
			}
			if (!this.AIParams.CanOpenDoors && !this.Character.AnimController.SimplePhysicsEnabled && base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null && (!this.canAttackDoors || !this.canAttackWalls || !this.AIParams.TargetOuterWalls) && this.wallTarget == null && Vector2.DistanceSquared(this.Character.WorldPosition, this.attackWorldPos) < 4000000f)
			{
				Vector2 rayStart = base.SimPosition;
				if (this.Character.Submarine == null)
				{
					rayStart -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				Vector2 dir = base.SelectedAiTarget.WorldPosition - base.WorldPosition;
				Vector2 rayEnd = rayStart + dir.ClampLength(this.Character.AnimController.Collider.GetLocalFront(null).Length() * 2f);
				Body closestBody = Submarine.CheckVisibility(rayStart, rayEnd, false, true, true, true, true, null);
				if (Submarine.LastPickedFraction != 1f && closestBody != null)
				{
					if (!this.AIParams.TargetOuterWalls || !this.canAttackWalls)
					{
						Structure s = closestBody.UserData as Structure;
						if (s != null && s.Submarine != null)
						{
							goto IL_8CC;
						}
					}
					if (this.canAttackDoors)
					{
						goto IL_8E6;
					}
					Item j = closestBody.UserData as Item;
					if (j == null || j.Submarine == null || j.GetComponent<Door>() == null)
					{
						goto IL_8E6;
					}
					IL_8CC:
					this.State = AIState.Idle;
					this.IgnoreTarget(base.SelectedAiTarget);
					base.ResetAITarget();
					return;
				}
			}
			IL_8E6:
			float distance = 0f;
			Limb attackTargetLimb = null;
			if (CS$<>8__locals1.canAttack)
			{
				if (!this.Character.AnimController.SimplePhysicsEnabled && this.wallTarget == null && CS$<>8__locals1.targetCharacter != null)
				{
					LimbType targetLimbType = this.AttackLimb.Params.Attack.Attack.TargetLimbType;
					attackTargetLimb = this.GetTargetLimb(this.AttackLimb, CS$<>8__locals1.targetCharacter, targetLimbType);
					if (attackTargetLimb == null)
					{
						this.State = AIState.Idle;
						this.IgnoreTarget(base.SelectedAiTarget);
						base.ResetAITarget();
						return;
					}
					this.attackWorldPos = attackTargetLimb.WorldPosition;
					this.attackSimPos = this.Character.GetRelativeSimPosition(attackTargetLimb, null);
				}
				Vector2 attackLimbPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.WorldPosition : this.AttackLimb.WorldPosition;
				Vector2 toTarget = this.attackWorldPos - attackLimbPos;
				EnemyAIController.<>c__DisplayClass159_1 CS$<>8__locals2;
				CS$<>8__locals2.toTargetOffset = toTarget;
				if (this.Character.Submarine == null)
				{
					if (this.wallTarget != null)
					{
						if (this.wallTarget.Structure.Submarine != null)
						{
							Vector2 margin = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(this.wallTarget.Structure.Submarine.Velocity, ref CS$<>8__locals2);
							CS$<>8__locals2.toTargetOffset += margin;
						}
					}
					else if (CS$<>8__locals1.targetCharacter != null)
					{
						Vector2 margin2 = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(CS$<>8__locals1.targetCharacter.AnimController.Collider.LinearVelocity, ref CS$<>8__locals2);
						CS$<>8__locals2.toTargetOffset += margin2;
					}
					else
					{
						Entity entity = base.SelectedAiTarget.Entity;
						MapEntity e = entity as MapEntity;
						if (e != null && entity.Submarine != null)
						{
							Vector2 margin3 = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(e.Submarine.Velocity, ref CS$<>8__locals2);
							CS$<>8__locals2.toTargetOffset += margin3;
						}
					}
				}
				distance = CS$<>8__locals2.toTargetOffset.Length();
				if (CS$<>8__locals1.canAttack)
				{
					CS$<>8__locals1.canAttack = (distance < this.AttackLimb.attack.Range);
				}
				if (CS$<>8__locals1.canAttack && !this.Character.InWater && this.Character.AnimController.CanWalk && !this.Character.IsFacing(this.attackWorldPos))
				{
					CS$<>8__locals1.canAttack = false;
				}
				if (CS$<>8__locals1.canAttack)
				{
					this.reachTimer = 0f;
					if (this.IsAggressiveBoarder)
					{
						Item k = base.SelectedAiTarget.Entity as Item;
						if (k != null)
						{
							Door component = k.GetComponent<Door>();
							if (component != null && component.CanBeTraversed)
							{
								CS$<>8__locals1.canAttack = false;
							}
						}
					}
				}
				else if (this.currentTargetingParams.AttackPattern == AttackPattern.Straight && distance < this.AttackLimb.attack.Range * 5f)
				{
					Vector2 targetVelocity = Vector2.Zero;
					Submarine targetSub = base.SelectedAiTarget.Entity.Submarine;
					if (targetSub != null)
					{
						targetVelocity = targetSub.Velocity;
					}
					else if (CS$<>8__locals1.targetCharacter != null)
					{
						targetVelocity = CS$<>8__locals1.targetCharacter.AnimController.Collider.LinearVelocity;
					}
					else
					{
						Item l = base.SelectedAiTarget.Entity as Item;
						if (l != null && l.body != null)
						{
							targetVelocity = l.body.LinearVelocity;
						}
					}
					float mySpeed = this.Character.AnimController.Collider.LinearVelocity.LengthSquared();
					float targetSpeed = targetVelocity.LengthSquared();
					if (mySpeed < 0.1f || mySpeed > targetSpeed)
					{
						this.reachTimer += deltaTime;
						if (this.reachTimer > 10f)
						{
							this.reachTimer = 0f;
							this.IgnoreTarget(base.SelectedAiTarget);
							this.State = AIState.Idle;
							base.ResetAITarget();
							return;
						}
					}
				}
				HumanoidAnimController humanoidAnimController = this.Character.AnimController as HumanoidAnimController;
				if (humanoidAnimController != null && distance < this.AttackLimb.attack.Range * 2f)
				{
					Character targetCharacter = CS$<>8__locals1.targetCharacter;
					Hull targetHull = (targetCharacter != null) ? targetCharacter.CurrentHull : null;
					if (targetHull != null && targetHull == this.Character.CurrentHull && toTarget.Y < 0f && Math.Abs(toTarget.Y) > this.AttackLimb.attack.Range / 2f && Math.Abs(toTarget.X) <= this.AttackLimb.attack.Range)
					{
						humanoidAnimController.Crouch();
					}
				}
				if (CS$<>8__locals1.canAttack)
				{
					if (this.AttackLimb.attack.Ranged)
					{
						float offset = this.AttackLimb.Params.GetSpriteOrientation() - 1.5707964f;
						Vector2 forward = VectorExtensions.Forward(this.AttackLimb.body.TransformedRotation - offset * this.Character.AnimController.Dir, 1f);
						float angle = forward.Angle(toTarget);
						CS$<>8__locals1.canAttack = (angle < MathHelper.ToRadians(this.AttackLimb.attack.RequiredAngle));
						if (CS$<>8__locals1.canAttack && this.AttackLimb.attack.AvoidFriendlyFire)
						{
							CS$<>8__locals1.canAttack = !CS$<>8__locals1.<UpdateAttack>g__IsBlocked|2(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null));
						}
					}
					else if (this.wallTarget == null && this.Character.CurrentHull != null && CS$<>8__locals1.targetCharacter != null)
					{
						CS$<>8__locals1.canAttack = (Submarine.PickBody(base.SimPosition, this.attackSimPos, null, new Category?(Category.Cat1), true, null, false) == null);
					}
				}
			}
			CS$<>8__locals1.steeringLimb = ((CS$<>8__locals1.canAttack && !this.AttackLimb.attack.Ranged) ? this.AttackLimb : null);
			bool updateSteering = true;
			if (CS$<>8__locals1.steeringLimb == null)
			{
				CS$<>8__locals1.steeringLimb = (this.Character.AnimController.GetLimb(LimbType.Head, true, false, false) ?? this.Character.AnimController.GetLimb(LimbType.Torso, true, false, false));
			}
			if (CS$<>8__locals1.steeringLimb == null)
			{
				this.State = AIState.Idle;
				return;
			}
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (this.AttackLimb != null && this.AttackLimb.attack.Retreat)
			{
				this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
			}
			else if (pathSteering != null)
			{
				if (this.canAttackDoors && base.HasValidPath(true, true, null) && (CS$<>8__locals1.targetCharacter == null || CS$<>8__locals1.targetCharacter.CurrentHull != this.Character.CurrentHull))
				{
					WayPoint currentNode = pathSteering.CurrentPath.CurrentNode;
					Door door2;
					if ((door2 = ((currentNode != null) ? currentNode.ConnectedDoor : null)) == null)
					{
						WayPoint nextNode = pathSteering.CurrentPath.NextNode;
						door2 = ((nextNode != null) ? nextNode.ConnectedDoor : null);
					}
					Door door = door2;
					if (door != null && !door.CanBeTraversed && (!this.Character.IsInFriendlySub || !door.HasAccess(this.Character)) && door.Item.AiTarget != null && base.SelectedAiTarget != door.Item.AiTarget)
					{
						this.SelectTarget(door.Item.AiTarget, this.currentTargetMemory.Priority);
						this.State = AIState.Attack;
						this.AttackLimb = null;
						return;
					}
				}
				float max = 300f;
				float margin4 = (this.AttackLimb != null) ? Math.Min(this.AttackLimb.attack.Range * 0.9f, max) : max;
				if ((!CS$<>8__locals1.canAttack || distance > margin4) && !base.IsTryingToSteerThroughGap)
				{
					bool useManualSteering = false;
					if (this.Character.CurrentHull != null && this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin && (this.Character.AnimController.InWater || pursue || !this.Character.AnimController.CanWalk) && CS$<>8__locals1.targetCharacter != null && base.VisibleHulls.Contains(CS$<>8__locals1.targetCharacter.CurrentHull) && this.CanSeeTarget(CS$<>8__locals1.targetCharacter))
					{
						useManualSteering = true;
					}
					if (useManualSteering)
					{
						Vector2 myPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.SimPosition : CS$<>8__locals1.steeringLimb.SimPosition;
						base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.attackSimPos - myPos));
					}
					else
					{
						Func<PathNode, bool> nodeFilter = null;
						float outsideNodePenalty = 0f;
						if (this.Character.CurrentHull != null && this.Character.IsInPlayerSub)
						{
							outsideNodePenalty = 50f;
						}
						pathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 2f, this.minGapSize, (PathNode n) => n.Waypoint.CurrentHull == null == (CS$<>8__locals1.<>4__this.Character.CurrentHull == null), null, nodeFilter, true, outsideNodePenalty);
						if (pathSteering.CurrentPath != null)
						{
							if (pathSteering.IsPathDirty)
							{
								Hull hull = this.Character.CurrentHull;
								if (hull != null)
								{
									if (hull.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.Open >= 1f && g.ConnectedDoor != null))
									{
										base.SteeringManager.Reset();
									}
								}
								base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
							}
							else if (pathSteering.CurrentPath.Unreachable)
							{
								this.State = AIState.Idle;
								this.IgnoreTarget(base.SelectedAiTarget);
								base.ResetAITarget();
								return;
							}
						}
					}
				}
				else if (!base.IsTryingToSteerThroughGap)
				{
					if (this.AttackLimb.attack.Ranged)
					{
						float dir2 = this.Character.AnimController.Dir;
						if ((dir2 > 0f && this.attackWorldPos.X > this.AttackLimb.WorldPosition.X + margin4) || (dir2 < 0f && this.attackWorldPos.X < this.AttackLimb.WorldPosition.X - margin4))
						{
							base.SteeringManager.Reset();
						}
						else
						{
							this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
						}
					}
					else
					{
						base.SteeringManager.Reset();
					}
				}
				else
				{
					base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
				}
			}
			else
			{
				Vector2 steerPos = this.attackSimPos;
				if (!this.Character.AnimController.SimplePhysicsEnabled)
				{
					Vector2 offset2 = this.Character.SimPosition - CS$<>8__locals1.steeringLimb.SimPosition;
					steerPos += offset2;
				}
				if (this.Character.CurrentHull == null)
				{
					AttackPattern attackPattern = this.currentTargetingParams.AttackPattern;
					if (attackPattern != AttackPattern.Sweep)
					{
						if (attackPattern == AttackPattern.Circle)
						{
							if (!this.IsCoolDownRunning && (!this.IsAttackRunning || this.CirclePhase == CirclePhase.Strike) && this.currentTargetingParams != null)
							{
								Entity entity2 = base.SelectedAiTarget.Entity;
								EnemyAIController.<>c__DisplayClass159_2 CS$<>8__locals3;
								CS$<>8__locals3.targetSub = ((entity2 != null) ? entity2.Submarine : null);
								ISpatialEntity spatialTarget = CS$<>8__locals3.targetSub ?? base.SelectedAiTarget.Entity;
								float targetSize = 0f;
								if (!this.currentTargetingParams.IgnoreTargetSize)
								{
									targetSize = ((CS$<>8__locals3.targetSub != null) ? ((float)(Math.Max(CS$<>8__locals3.targetSub.Borders.Width, CS$<>8__locals3.targetSub.Borders.Height) / 2)) : ((CS$<>8__locals1.targetCharacter != null) ? ConvertUnits.ToDisplayUnits(CS$<>8__locals1.targetCharacter.AnimController.Collider.GetSize().X) : 100f));
								}
								float sqrDistToTarget = Vector2.DistanceSquared(base.WorldPosition, spatialTarget.WorldPosition);
								bool isProgressive = this.AIParams.MaxAggression - this.AIParams.StartAggression > 0f;
								switch (this.CirclePhase)
								{
								case CirclePhase.Start:
								{
									this.currentAttackIntensity = MathUtils.InverseLerp(this.AIParams.StartAggression, this.AIParams.MaxAggression, CS$<>8__locals1.<UpdateAttack>g__ClampIntensity|10(this.aggressionIntensity));
									this.inverseDir = false;
									this.circleDir = CS$<>8__locals1.<UpdateAttack>g__GetDirFromHeadingInRadius|7();
									this.circleRotation = 0f;
									this.strikeTimer = 0f;
									this.blockCheckTimer = 0f;
									this.breakCircling = false;
									float minFallBackDistance = this.currentTargetingParams.CircleStartDistance * 0.5f;
									float maxFallBackDistance = this.currentTargetingParams.CircleStartDistance;
									float maxRandomOffset = this.currentTargetingParams.CircleMaxRandomOffset;
									if (isProgressive)
									{
										float intensity = CS$<>8__locals1.<UpdateAttack>g__ClampIntensity|10(this.currentAttackIntensity);
										float minRotationSpeed = 0.01f * this.currentTargetingParams.CircleRotationSpeed;
										float maxRotationSpeed = 0.5f * this.currentTargetingParams.CircleRotationSpeed;
										this.circleRotationSpeed = MathHelper.Lerp(minRotationSpeed, maxRotationSpeed, intensity);
										this.circleFallbackDistance = MathHelper.Lerp(maxFallBackDistance, minFallBackDistance, intensity);
										this.circleOffset = Rand.Vector(MathHelper.Lerp(maxRandomOffset, 0f, intensity), Rand.RandSync.Unsynced);
									}
									else
									{
										this.circleRotationSpeed = this.currentTargetingParams.CircleRotationSpeed;
										this.circleFallbackDistance = maxFallBackDistance;
										this.circleOffset = Rand.Vector(maxRandomOffset, Rand.RandSync.Unsynced);
									}
									this.circleRotationSpeed *= Rand.Range(1f - this.currentTargetingParams.CircleRandomRotationFactor, 1f + this.currentTargetingParams.CircleRandomRotationFactor, Rand.RandSync.Unsynced);
									this.aggressionIntensity = Math.Clamp(this.aggressionIntensity, this.AIParams.StartAggression, this.AIParams.MaxAggression);
									CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									if (CS$<>8__locals3.targetSub != null && CS$<>8__locals3.targetSub.Borders.Width < 1000)
									{
										Limb attackLimb4 = this.AttackLimb;
										Attack attack2 = (attackLimb4 != null) ? attackLimb4.attack : null;
										if (attack2 != null && !attack2.Ranged)
										{
											this.breakCircling = true;
											this.CirclePhase = CirclePhase.CloseIn;
											break;
										}
									}
									if (sqrDistToTarget > MathUtils.Pow2(targetSize + this.currentTargetingParams.CircleStartDistance))
									{
										this.CirclePhase = CirclePhase.CloseIn;
									}
									else if (sqrDistToTarget < MathUtils.Pow2(targetSize + this.circleFallbackDistance))
									{
										this.CirclePhase = CirclePhase.FallBack;
									}
									else
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									break;
								}
								case CirclePhase.CloseIn:
								{
									Vector2 targetVelocity2 = CS$<>8__locals1.<UpdateAttack>g__GetTargetVelocity|8(ref CS$<>8__locals3);
									float targetDistance = this.currentTargetingParams.IgnoreTargetSize ? (this.currentTargetingParams.CircleStartDistance * 0.9f) : (targetSize + this.currentTargetingParams.CircleStartDistance / 2f);
									if (this.AttackLimb != null && distance > 0f && distance < this.AttackLimb.attack.Range * CS$<>8__locals1.<UpdateAttack>g__GetStrikeDistanceMultiplier|6(targetVelocity2))
									{
										this.strikeTimer = this.AttackLimb.attack.CoolDown;
										this.CirclePhase = CirclePhase.Strike;
									}
									else if (!this.breakCircling && sqrDistToTarget <= MathUtils.Pow2(targetDistance) && targetVelocity2.LengthSquared() <= MathUtils.Pow2(CS$<>8__locals1.<UpdateAttack>g__GetTargetMaxSpeed|9(ref CS$<>8__locals3)))
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									break;
								}
								case CirclePhase.FallBack:
									updateSteering = false;
									if (!this.UpdateFallBack(this.attackWorldPos, deltaTime, false, true, true) || sqrDistToTarget > MathUtils.Pow2(targetSize + this.circleFallbackDistance))
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									else
									{
										CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									}
									break;
								case CirclePhase.Advance:
								{
									Vector2 targetVel = CS$<>8__locals1.<UpdateAttack>g__GetTargetVelocity|8(ref CS$<>8__locals3);
									Attack attack2;
									if (this.breakCircling || targetVel.LengthSquared() > MathUtils.Pow2(CS$<>8__locals1.<UpdateAttack>g__GetTargetMaxSpeed|9(ref CS$<>8__locals3)))
									{
										this.CirclePhase = CirclePhase.CloseIn;
									}
									else if (sqrDistToTarget > MathUtils.Pow2(targetSize + this.currentTargetingParams.CircleStartDistance * 1.2f))
									{
										if (this.currentTargetingParams.DynamicCircleRotationSpeed && this.circleRotationSpeed < 100f)
										{
											this.circleRotationSpeed *= 1f + deltaTime;
										}
										else
										{
											this.CirclePhase = CirclePhase.CloseIn;
										}
									}
									else
									{
										float rotationStep = this.circleRotationSpeed * deltaTime * this.circleDir;
										if (isProgressive)
										{
											this.circleRotation += rotationStep;
										}
										else
										{
											this.circleRotation = rotationStep;
										}
										Vector2 targetPos = this.attackSimPos + this.circleOffset;
										float targetDist = targetSize;
										if (targetDist <= 0f)
										{
											targetDist = this.circleFallbackDistance;
										}
										if (CS$<>8__locals3.targetSub != null)
										{
											Limb attackLimb5 = this.AttackLimb;
											attack2 = ((attackLimb5 != null) ? attackLimb5.attack : null);
											if (attack2 != null && attack2.Ranged)
											{
												targetDist += this.circleFallbackDistance / 2f;
											}
										}
										if (Vector2.DistanceSquared(base.SimPosition, targetPos) < ConvertUnits.ToSimUnits(targetDist))
										{
											if (CS$<>8__locals1.canAttack)
											{
												Limb attackLimb6 = this.AttackLimb;
												attack2 = ((attackLimb6 != null) ? attackLimb6.attack : null);
												if (attack2 != null && !attack2.Ranged && sqrDistToTarget < MathUtils.Pow2(targetSize + this.circleFallbackDistance))
												{
													this.CirclePhase = CirclePhase.Strike;
													this.strikeTimer = this.AttackLimb.attack.CoolDown;
													break;
												}
											}
											this.CirclePhase = CirclePhase.Start;
											break;
										}
										steerPos = MathUtils.RotatePointAroundTarget(base.SimPosition, targetPos, this.circleRotation, true);
										if (this.IsBlocked(deltaTime, steerPos, Category.Cat8))
										{
											if (!this.inverseDir)
											{
												this.circleDir = -this.circleDir;
												this.inverseDir = true;
											}
											else if (this.circleRotationSpeed < 1f)
											{
												this.circleRotationSpeed *= 1f + deltaTime;
											}
											else if (this.circleOffset.LengthSquared() > 0.1f)
											{
												this.circleOffset = Vector2.Zero;
											}
											else
											{
												Limb attackLimb7 = this.AttackLimb;
												attack2 = ((attackLimb7 != null) ? attackLimb7.attack : null);
												this.breakCircling = (attack2 != null && !attack2.Ranged);
												if (!this.breakCircling)
												{
													this.CirclePhase = CirclePhase.FallBack;
												}
											}
										}
									}
									Limb attackLimb8 = this.AttackLimb;
									attack2 = ((attackLimb8 != null) ? attackLimb8.attack : null);
									if (attack2 != null && !attack2.Ranged)
									{
										CS$<>8__locals1.canAttack = false;
										float requiredDistMultiplier = CS$<>8__locals1.<UpdateAttack>g__GetStrikeDistanceMultiplier|6(targetVel);
										if (distance > 0f && distance < this.AttackLimb.attack.Range * requiredDistMultiplier && CS$<>8__locals1.<UpdateAttack>g__IsFacing|5(MathHelper.Lerp(0.5f, 0.9f, this.currentAttackIntensity)))
										{
											this.strikeTimer = this.AttackLimb.attack.CoolDown;
											this.CirclePhase = CirclePhase.Strike;
										}
									}
									break;
								}
								case CirclePhase.Strike:
									this.strikeTimer -= deltaTime;
									steerPos = base.SimPosition + base.Steering;
									if (this.strikeTimer <= 0f)
									{
										this.CirclePhase = CirclePhase.Start;
										this.aggressionIntensity += this.AIParams.AggressionCumulation;
									}
									break;
								}
							}
						}
					}
					else if (this.currentTargetingParams.SweepDistance > 0f)
					{
						if (distance <= 0f)
						{
							distance = (this.attackWorldPos - base.WorldPosition).Length();
						}
						float amplitude = MathHelper.Lerp(0f, this.currentTargetingParams.SweepStrength, MathUtils.InverseLerp(this.currentTargetingParams.SweepDistance, 0f, distance));
						if (amplitude > 0f)
						{
							this.sweepTimer += deltaTime * this.currentTargetingParams.SweepSpeed;
							float sin = (float)Math.Sin((double)this.sweepTimer) * amplitude;
							steerPos = MathUtils.RotatePointAroundTarget(this.attackSimPos, base.SimPosition, sin, true);
						}
						else
						{
							this.sweepTimer = Rand.Range(-1000f, 1000f, Rand.RandSync.Unsynced) * this.currentTargetingParams.SweepSpeed;
						}
					}
				}
				if (updateSteering)
				{
					if (this.currentTargetingParams.AttackPattern == AttackPattern.Straight)
					{
						Limb attackLimb = this.AttackLimb;
						if (attackLimb != null && attackLimb.attack.Ranged)
						{
							bool advance = (!CS$<>8__locals1.canAttack && this.Character.CurrentHull == null) || distance > attackLimb.attack.Range * 0.9f;
							bool fallBack = CS$<>8__locals1.canAttack && distance < Math.Min(250f, attackLimb.attack.Range * 0.25f);
							if (fallBack)
							{
								this.Reverse = true;
								this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
								goto IL_1E1F;
							}
							if (advance)
							{
								base.SteeringManager.SteeringSeek(steerPos, 10f);
								goto IL_1E1F;
							}
							if (this.Character.CurrentHull == null && !CS$<>8__locals1.canAttack)
							{
								base.SteeringManager.SteeringWander(1f, true);
								base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
								goto IL_1E1F;
							}
							base.SteeringManager.Reset();
							base.FaceTarget(base.SelectedAiTarget.Entity);
							goto IL_1E1F;
						}
					}
					if (!CS$<>8__locals1.canAttack || distance > Math.Min(this.AttackLimb.attack.Range * 0.9f, 100f))
					{
						if (pathSteering != null)
						{
							pathSteering.SteeringSeek(steerPos, 10f, this.minGapSize, null, null, null, true, 0f);
						}
						else
						{
							base.SteeringManager.SteeringSeek(steerPos, 10f);
						}
					}
					IL_1E1F:
					if (this.Character.CurrentHull == null)
					{
						AITarget selectedAiTarget3 = base.SelectedAiTarget;
						Character c = ((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) as Character;
						if ((c != null && c.Submarine == null) || distance == 0f || distance > ConvertUnits.ToDisplayUnits(this.avoidLookAheadDistance * 2f) || (this.AttackLimb != null && this.AttackLimb.attack.Ranged))
						{
							base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 30f);
						}
					}
				}
			}
			EnemyAIController.WallTarget wallTarget = this.wallTarget;
			Structure structure2;
			if ((structure2 = ((wallTarget != null) ? wallTarget.Structure : null)) == null)
			{
				AITarget selectedAiTarget4 = base.SelectedAiTarget;
				structure2 = ((selectedAiTarget4 != null) ? selectedAiTarget4.Entity : null);
			}
			Entity targetEntity = structure2;
			Limb attackLimb9 = this.AttackLimb;
			Attack attack = (attackLimb9 != null) ? attackLimb9.attack : null;
			if (attack != null && attack.Ranged)
			{
				Attack attack3 = attack;
				ISpatialEntity spatialEntity = attackTargetLimb;
				this.AimRangedAttack(attack3, spatialEntity ?? targetEntity);
			}
			if (CS$<>8__locals1.canAttack)
			{
				if (!this.UpdateLimbAttack(deltaTime, this.attackSimPos, damageTarget, distance, attackTargetLimb))
				{
					this.IgnoreTarget(base.SelectedAiTarget);
					return;
				}
			}
			else if (this.IsAttackRunning)
			{
				this.AttackLimb.attack.ResetAttackTimer();
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000E5CC File Offset: 0x0000C7CC
		public void AimRangedAttack(Attack attack, ISpatialEntity targetEntity)
		{
			if (attack == null || !attack.Ranged)
			{
				return;
			}
			Entity entity = targetEntity as Entity;
			if (entity != null && entity.Removed)
			{
				return;
			}
			this.Character.SetInput(InputType.Aim, false, true);
			if (attack.AimRotationTorque <= 0f)
			{
				return;
			}
			Limb limb = this.GetLimbToRotate(attack);
			if (limb != null)
			{
				Vector2 toTarget = targetEntity.WorldPosition - limb.WorldPosition;
				float offset = limb.Params.GetSpriteOrientation() - 1.5707964f;
				limb.body.SuppressSmoothRotationCalls = false;
				float angle = MathUtils.VectorToAngle(toTarget);
				limb.body.SmoothRotate(angle + offset, attack.AimRotationTorque, true);
				limb.body.SuppressSmoothRotationCalls = true;
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x0000E67C File Offset: 0x0000C87C
		private bool IsValidAttack(Limb attackingLimb, IEnumerable<AttackContext> currentContexts, Entity target)
		{
			if (attackingLimb == null)
			{
				return false;
			}
			if (target == null)
			{
				return false;
			}
			Attack attack = attackingLimb.attack;
			if (attack == null)
			{
				return false;
			}
			if (attack.CoolDownTimer > 0f)
			{
				return false;
			}
			if (!attack.IsValidContext(currentContexts))
			{
				return false;
			}
			if (!attack.IsValidTarget(target))
			{
				return false;
			}
			if (!attackingLimb.attack.Ranged)
			{
				if (!(target is Item))
				{
					if (!(target is Structure))
					{
						goto IL_91;
					}
					if (attackingLimb.attack.StructureDamage > 0f)
					{
						goto IL_91;
					}
				}
				else if (attackingLimb.attack.ItemDamage > 0f)
				{
					goto IL_91;
				}
				return false;
			}
			IL_91:
			ISerializableEntity se = target as ISerializableEntity;
			if (se != null && se is Character && attack.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se)))
			{
				return false;
			}
			if (attack.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this.Character)))
			{
				return false;
			}
			if (attack.Ranged)
			{
				Vector2 attackLimbPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.WorldPosition : attackingLimb.WorldPosition;
				Vector2 toTarget = this.attackWorldPos - attackLimbPos;
				if (attack.MinRange > 0f && toTarget.LengthSquared() < MathUtils.Pow2(attack.MinRange))
				{
					return false;
				}
				float offset = attackingLimb.Params.GetSpriteOrientation() - 1.5707964f;
				Vector2 forward = VectorExtensions.Forward(attackingLimb.body.TransformedRotation - offset * this.Character.AnimController.Dir, 1f);
				float angle = MathHelper.ToDegrees(forward.Angle(toTarget));
				if (angle > attack.RequiredAngle)
				{
					return false;
				}
			}
			if (attack.RootForceWorldEnd.LengthSquared() > 1f)
			{
				Character targetCharacter = target as Character;
				if (targetCharacter == null)
				{
					Item targetItem = target as Item;
					if (targetItem == null)
					{
						goto IL_211;
					}
					if (this.Character.CurrentHull == targetItem.CurrentHull)
					{
						goto IL_211;
					}
				}
				else if (this.Character.CurrentHull == targetCharacter.CurrentHull && !targetCharacter.IsKnockedDownOrRagdolled)
				{
					goto IL_211;
				}
				return false;
				IL_211:
				float verticalDistance = Math.Abs(this.attackWorldPos.Y - this.Character.WorldPosition.Y);
				if (verticalDistance > 50f)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000E8CC File Offset: 0x0000CACC
		private Limb GetAttackLimb(Vector2 attackWorldPos, Limb ignoredLimb = null)
		{
			IEnumerable<AttackContext> currentContexts = this.Character.GetAttackContexts();
			Entity entity;
			if (this.wallTarget == null)
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				entity = ((selectedAiTarget != null) ? selectedAiTarget.Entity : null);
			}
			else
			{
				entity = this.wallTarget.Structure;
			}
			Entity target = entity;
			if (target == null)
			{
				return null;
			}
			Limb selectedLimb = null;
			float currentPriority = -1f;
			foreach (Limb limb in this.Character.AnimController.Limbs)
			{
				if (limb != ignoredLimb && !limb.IsSevered && !limb.IsStuck && this.IsValidAttack(limb, currentContexts, target))
				{
					if (this.AIParams.RandomAttack)
					{
						this.attackLimbs.Add(limb);
						this.weights.Add(limb.attack.Priority);
					}
					else
					{
						float priority = this.<GetAttackLimb>g__CalculatePriority|164_0(limb, attackWorldPos);
						if (priority > currentPriority)
						{
							currentPriority = priority;
							selectedLimb = limb;
						}
					}
				}
			}
			if (this.AIParams.RandomAttack)
			{
				selectedLimb = ToolBox.SelectWeightedRandom<Limb>(this.attackLimbs, this.weights, Rand.RandSync.Unsynced);
				this.attackLimbs.Clear();
				this.weights.Clear();
			}
			return selectedLimb;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000E9EC File Offset: 0x0000CBEC
		public override void OnAttacked(Character attacker, AttackResult attackResult)
		{
			float reactionTime = Rand.Range(0.1f, 0.3f, Rand.RandSync.Unsynced);
			this.updateTargetsTimer = Math.Min(this.updateTargetsTimer, reactionTime);
			bool wasLatched = this.IsLatchedOnSub;
			this.Character.AnimController.ReleaseStuckLimbs();
			if (attackResult.Damage > 0f)
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.DeattachFromBody(true, 1f);
				}
			}
			if (((attacker != null) ? attacker.AiTarget : null) == null || attacker.Removed || attacker.IsDead)
			{
				return;
			}
			AITargetMemory targetMemory = this.GetTargetMemory(attacker.AiTarget, true, true);
			targetMemory.Priority += EnemyAIController.GetRelativeDamage(attackResult.Damage, this.Character.Vitality) * this.AIParams.AggressionHurt;
			if (attackResult.Damage >= this.AIParams.DamageThreshold)
			{
				this.ReleaseDragTargets();
			}
			bool isFriendly = this.Character.IsFriendly(attacker);
			if (wasLatched)
			{
				this.State = AIState.Escape;
				this.avoidTimer = this.AIParams.AvoidTime * 0.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				if (!isFriendly)
				{
					this.SelectTarget(attacker.AiTarget);
				}
				return;
			}
			if (this.State == AIState.Flee)
			{
				if (!isFriendly)
				{
					this.SelectTarget(attacker.AiTarget);
				}
				return;
			}
			if (!isFriendly && attackResult.Damage > 0f)
			{
				bool canAttack = (attacker.Submarine == this.Character.Submarine && this.canAttackCharacters) || (attacker.Submarine != null && this.canAttackWalls);
				if (canAttack)
				{
					AIState state = this.State;
					if (state != AIState.PlayDead)
					{
						if (state != AIState.Hiding)
						{
							goto IL_1C2;
						}
					}
					else if (Rand.Value(Rand.RandSync.Unsynced) < 0.5f)
					{
						return;
					}
					this.SelectTarget(attacker.AiTarget);
					this.State = AIState.Attack;
				}
				IL_1C2:
				IEnumerable<CharacterParams.TargetParams> targetingParams;
				if (this.AIParams.AttackWhenProvoked && canAttack)
				{
					if (this.ignoredTargets.Contains(attacker.AiTarget))
					{
						this.ignoredTargets.Remove(attacker.AiTarget);
					}
					if (attacker.IsHusk)
					{
						this.ChangeTargetState(Tags.Husk, AIState.Attack, new float?((float)100));
					}
					else
					{
						this.ChangeTargetState(attacker, AIState.Attack, new float?((float)100));
					}
				}
				else if (!this.AIParams.HasTag(attacker.SpeciesName))
				{
					if (attacker.IsHusk)
					{
						this.ChangeTargetState(Tags.Husk, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
					}
					else
					{
						EnemyAIController enemyAI = attacker.AIController as EnemyAIController;
						if (enemyAI != null)
						{
							if (enemyAI.CombatStrength > this.CombatStrength)
							{
								if (!this.AIParams.HasTag("stronger"))
								{
									this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
								}
							}
							else if (enemyAI.CombatStrength < this.CombatStrength)
							{
								if (!this.AIParams.HasTag("weaker"))
								{
									this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
								}
							}
							else if (!this.AIParams.HasTag("equal"))
							{
								this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
							}
						}
						else
						{
							this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
						}
					}
				}
				else if (canAttack && attacker.IsHuman && this.AIParams.TryGetTargets(attacker, out targetingParams))
				{
					this.tempParamsList.Clear();
					this.tempParamsList.AddRange(targetingParams);
					foreach (CharacterParams.TargetParams tp in this.tempParamsList)
					{
						AIState state2 = tp.State;
						bool flag = state2 - AIState.Aggressive <= 1;
						if (flag)
						{
							this.ChangeTargetState(attacker, AIState.Attack, new float?((float)100));
						}
					}
				}
			}
			bool retaliate = !isFriendly && base.SelectedAiTarget != attacker.AiTarget && attacker.Submarine == this.Character.Submarine;
			bool avoidGunFire = this.AIParams.AvoidGunfire && attacker.Submarine != this.Character.Submarine;
			if (this.State == AIState.Attack && (this.IsAttackRunning || this.IsCoolDownRunning))
			{
				retaliate = false;
				if (this.IsAttackRunning)
				{
					avoidGunFire = false;
				}
			}
			if (retaliate)
			{
				foreach (Limb limb in this.Character.AnimController.Limbs)
				{
					if (limb.attack != null)
					{
						limb.attack.CoolDownTimer *= reactionTime;
					}
				}
			}
			else if (avoidGunFire && attackResult.Damage >= this.AIParams.DamageThreshold)
			{
				this.State = AIState.Escape;
				this.avoidTimer = this.AIParams.AvoidTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				this.SelectTarget(attacker.AiTarget);
			}
			if (Math.Max(this.Character.HealthPercentage, 0f) < this.FleeHealthThreshold)
			{
				this.State = AIState.Flee;
				this.avoidTimer = this.AIParams.MinFleeTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				this.SelectTarget(attacker.AiTarget);
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000EF54 File Offset: 0x0000D154
		private Item GetEquippedItem(Limb limb)
		{
			EnemyAIController.<>c__DisplayClass166_0 CS$<>8__locals1;
			CS$<>8__locals1.limb = limb;
			InvSlotType slot = EnemyAIController.<GetEquippedItem>g__GetInvSlotForLimb|166_0(ref CS$<>8__locals1);
			if (slot != InvSlotType.None)
			{
				return this.Character.Inventory.GetItemInLimbSlot(slot);
			}
			return null;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000EF87 File Offset: 0x0000D187
		private static float GetRelativeDamage(float dmg, float vitality)
		{
			return dmg / Math.Max(vitality, 1f);
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000EF98 File Offset: 0x0000D198
		private bool UpdateLimbAttack(float deltaTime, Vector2 attackSimPos, IDamageable damageTarget, float distance = -1f, Limb targetLimb = null)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null)
			{
				return false;
			}
			Limb attackLimb = this.AttackLimb;
			if (((attackLimb != null) ? attackLimb.attack : null) == null)
			{
				return false;
			}
			ISpatialEntity spatialEntity2;
			if (this.wallTarget == null)
			{
				ISpatialEntity spatialEntity = base.SelectedAiTarget.Entity;
				spatialEntity2 = spatialEntity;
			}
			else
			{
				ISpatialEntity spatialEntity = this.wallTarget.Structure;
				spatialEntity2 = spatialEntity;
			}
			ISpatialEntity spatialTarget = spatialEntity2;
			if (spatialTarget == null)
			{
				return false;
			}
			this.ActiveAttack = this.AttackLimb.attack;
			if (this.wallTarget != null)
			{
				AITarget aiTarget = this.wallTarget.Structure.AiTarget;
				if (aiTarget != null && base.SelectedAiTarget != aiTarget)
				{
					this.SelectTarget(aiTarget, this.GetTargetMemory(base.SelectedAiTarget, true, false).Priority);
					this.State = AIState.Attack;
					this.AttackLimb = null;
					return true;
				}
			}
			if (damageTarget == null)
			{
				return false;
			}
			this.ActiveAttack = this.AttackLimb.attack;
			if (this.ActiveAttack.Ranged && this.ActiveAttack.RequiredAngleToShoot > 0f)
			{
				Limb referenceLimb = this.GetLimbToRotate(this.ActiveAttack);
				if (referenceLimb != null)
				{
					Vector2 toTarget = this.attackWorldPos - referenceLimb.WorldPosition;
					float offset = referenceLimb.Params.GetSpriteOrientation() - 1.5707964f;
					Vector2 forward = VectorExtensions.Forward(referenceLimb.body.TransformedRotation - offset * referenceLimb.Dir, 1f);
					float angle = MathHelper.ToDegrees(forward.Angle(toTarget));
					if (angle > this.ActiveAttack.RequiredAngleToShoot)
					{
						return true;
					}
				}
			}
			if (this.Character.Params.CanInteract && this.Character.Inventory != null)
			{
				Item item = this.GetEquippedItem(this.AttackLimb);
				if (item != null)
				{
					if (item.RequireAimToUse && !this.Aim(deltaTime, spatialTarget, item))
					{
						return true;
					}
					this.Character.SetInput(item.IsShootable ? InputType.Shoot : InputType.Use, false, true);
					item.Use(deltaTime, this.Character, null, null, null);
				}
			}
			this.Character.SetInput(InputType.Attack, true, true);
			if (!this.ActiveAttack.IsRunning)
			{
				this.Character.PlaySound(CharacterSound.SoundType.Attack, 1f, 3f);
			}
			AttackResult attackResult;
			if (this.AttackLimb.UpdateAttack(deltaTime, attackSimPos, damageTarget, out attackResult, distance, targetLimb))
			{
				if (this.ActiveAttack.CoolDownTimer > 0f)
				{
					this.SetAimTimer(Math.Min(this.ActiveAttack.CoolDown, 1.5f));
				}
				if (this.LatchOntoAI != null)
				{
					Character targetCharacter = base.SelectedAiTarget.Entity as Character;
					if (targetCharacter != null)
					{
						this.LatchOntoAI.SetAttachTarget(targetCharacter);
					}
				}
				if (!this.ActiveAttack.Ranged)
				{
					if (damageTarget.Health <= 0f || attackResult.Damage <= 0f)
					{
						this.currentTargetMemory.Priority -= Math.Max(this.currentTargetMemory.Priority / 2f, 1f);
						return this.currentTargetMemory.Priority > 1f;
					}
					float greed = this.AIParams.AggressionGreed;
					if (!(damageTarget is Character))
					{
						greed /= 2f;
					}
					this.currentTargetMemory.Priority += EnemyAIController.GetRelativeDamage(attackResult.Damage, damageTarget.Health) * greed;
				}
			}
			return true;
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000F2DB File Offset: 0x0000D4DB
		private bool CanSeeTarget(ISpatialEntity target)
		{
			if (Timing.TotalTime > this.lastVisibilityCheckTime + 0.20000000298023224)
			{
				this.canSeeTarget = this.Character.CanSeeTarget(target, null, false, false);
				this.lastVisibilityCheckTime = Timing.TotalTime;
			}
			return this.canSeeTarget;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x0000F31C File Offset: 0x0000D51C
		private bool Aim(float deltaTime, ISpatialEntity target, Item weapon)
		{
			if (target == null || weapon == null)
			{
				return false;
			}
			if (this.AttackLimb == null)
			{
				return false;
			}
			Vector2 toTarget = target.WorldPosition - weapon.WorldPosition;
			float dist = toTarget.Length();
			this.Character.CursorPosition = target.WorldPosition;
			if (this.AttackLimb.attack.SwayAmount > 0f)
			{
				this.sinTime += deltaTime * this.AttackLimb.attack.SwayFrequency;
				this.Character.CursorPosition += VectorExtensions.Forward(weapon.body.TransformedRotation + (float)Math.Sin((double)this.sinTime) / 2f, dist / 2f * this.AttackLimb.attack.SwayAmount);
			}
			if (this.Character.Submarine != null)
			{
				this.Character.CursorPosition -= this.Character.Submarine.Position;
			}
			if (!this.CanSeeTarget(target))
			{
				this.SetAimTimer(1.5f);
				return false;
			}
			this.Character.SetInput(InputType.Aim, false, true);
			if (this.aimTimer > 0f)
			{
				this.aimTimer -= deltaTime;
				return false;
			}
			float angle = VectorExtensions.Forward(weapon.body.TransformedRotation, 1f).Angle(toTarget);
			float minDistance = 300f;
			float distanceFactor = MathHelper.Lerp(1f, 0.1f, MathUtils.InverseLerp(minDistance, 1000f, dist));
			float margin = 0.7853982f * distanceFactor;
			if (angle < margin || dist < minDistance)
			{
				Category collisionCategories = Category.Cat1 | Category.Cat2 | Category.Cat6 | Category.Cat8;
				Body pickedBody = Submarine.PickBody(weapon.SimPosition, this.Character.GetRelativeSimPosition(target, null), this.myBodies, new Category?(collisionCategories), true, null, true);
				if (pickedBody != null)
				{
					if (target is MapEntity)
					{
						Submarine sub = pickedBody.UserData as Submarine;
						if (sub != null && sub == target.Submarine)
						{
							return true;
						}
						if (target == pickedBody.UserData)
						{
							return true;
						}
					}
					Character t = null;
					Character c = pickedBody.UserData as Character;
					if (c != null)
					{
						t = c;
					}
					else
					{
						Limb limb = pickedBody.UserData as Limb;
						if (limb != null)
						{
							t = limb.character;
						}
					}
					if (t != null && (t == target || !this.Character.IsFriendly(t) || this.IsAttackingOwner(t)))
					{
						return true;
					}
					Item item = pickedBody.UserData as Item;
					if (item != null && item.Prefab.DamagedByProjectiles)
					{
						return true;
					}
					Holdable holdable = pickedBody.UserData as Holdable;
					if (holdable != null && holdable.Item.Prefab.DamagedByProjectiles)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x0000F5CE File Offset: 0x0000D7CE
		private void SetAimTimer(float timer = 1.5f)
		{
			this.aimTimer = timer * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x0000F5E8 File Offset: 0x0000D7E8
		private bool IsBlocked(float deltaTime, Vector2 steerPos, Category collisionCategory = Category.Cat8)
		{
			this.blockCheckTimer -= deltaTime;
			if (this.blockCheckTimer <= 0f)
			{
				this.blockCheckTimer = this.blockCheckInterval;
				this.isBlocked = Submarine.PickBodies(base.SimPosition, steerPos, null, new Category?(collisionCategory), true, null, false).Any<Body>();
			}
			return this.isBlocked;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000F644 File Offset: 0x0000D844
		private bool UpdateFallBack(Vector2 attackWorldPos, float deltaTime, bool followThrough, bool checkBlocking = false, bool avoidObstacles = true)
		{
			if (this.attackVector == null)
			{
				this.attackVector = new Vector2?(attackWorldPos - base.WorldPosition);
			}
			Vector2 dir = Vector2.Normalize(followThrough ? this.attackVector.Value : (-this.attackVector.Value));
			if (!MathUtils.IsValid(dir))
			{
				dir = Vector2.UnitY;
			}
			this.steeringManager.SteeringManual(deltaTime, dir);
			if (this.Character.AnimController.InWater && !this.Reverse && avoidObstacles)
			{
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
			return !checkBlocking || !this.IsBlocked(deltaTime, base.SimPosition + dir * (this.avoidLookAheadDistance / 2f), Category.Cat8);
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000F724 File Offset: 0x0000D924
		private Limb GetLimbToRotate(Attack attack)
		{
			Limb limb = this.AttackLimb;
			if (attack.RotationLimbIndex > -1 && attack.RotationLimbIndex < this.Character.AnimController.Limbs.Length)
			{
				limb = this.Character.AnimController.Limbs[attack.RotationLimbIndex];
			}
			return limb;
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000F774 File Offset: 0x0000D974
		private void UpdateEating(float deltaTime)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
			{
				this.ReleaseEatingTarget();
				return;
			}
			Entity entity = base.SelectedAiTarget.Entity;
			bool flag = entity is Character || entity is Item;
			if (flag)
			{
				Limb mouthLimb = this.Character.AnimController.GetLimb(LimbType.Head, true, false, false);
				if (mouthLimb == null)
				{
					DebugConsole.ThrowError("Character \"" + this.Character.SpeciesName.ToString() + "\" failed to eat a target (No head limb found)", null, this.Character.Prefab.ContentPackage, false, false);
					this.IgnoreTarget(base.SelectedAiTarget);
					this.ReleaseEatingTarget();
					base.ResetAITarget();
					return;
				}
				Vector2 mouthPos = this.Character.AnimController.SimplePhysicsEnabled ? base.SimPosition : (this.Character.AnimController.GetMouthPosition() ?? Vector2.Zero);
				Vector2 attackSimPosition = this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null);
				Vector2 limbDiff = attackSimPosition - mouthPos;
				float extent = Math.Max(mouthLimb.body.GetMaxExtent(), 2f);
				bool tooFar = this.Character.InWater ? (limbDiff.LengthSquared() > extent * extent) : (limbDiff.X > extent);
				if (tooFar)
				{
					this.steeringManager.SteeringSeek(attackSimPosition - (mouthPos - base.SimPosition), 2f);
					if (this.Character.InWater)
					{
						base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
						return;
					}
				}
				else
				{
					Character targetCharacter = base.SelectedAiTarget.Entity as Character;
					if (targetCharacter != null)
					{
						this.Character.SelectCharacter(targetCharacter);
					}
					else
					{
						Item item = base.SelectedAiTarget.Entity as Item;
						if (item != null && !item.Removed && item.body != null)
						{
							float itemBodyExtent = item.body.GetMaxExtent() * 2f;
							if (Math.Abs(limbDiff.X) < itemBodyExtent && Math.Abs(limbDiff.Y) < this.Character.AnimController.Collider.GetMaxExtent() + this.Character.AnimController.ColliderHeightFromFloor)
							{
								Vector2 velocity = limbDiff;
								if (limbDiff.LengthSquared() > 0.01f)
								{
									velocity = Vector2.Normalize(velocity);
								}
								item.body.LinearVelocity *= 0.9f;
								item.body.LinearVelocity -= velocity * 0.25f;
								bool wasBroken = item.Condition <= 0f;
								item.LastEatenTime = (float)Timing.TotalTimeUnpaused;
								item.AddDamage(this.Character, item.WorldPosition, new Attack(0f, 0f, 0f, 0f, 0.02f * this.Character.Params.EatingSpeed, 0f), Vector2.Zero, deltaTime, true);
								this.Character.ApplyStatusEffects(ActionType.OnEating, deltaTime);
								if (item.Condition <= 0f)
								{
									if (!wasBroken)
									{
										PetBehavior petBehavior = this.PetBehavior;
										if (petBehavior != null)
										{
											petBehavior.OnEat(item);
										}
									}
									Entity.Spawner.AddItemToRemoveQueue(item);
								}
							}
						}
					}
					this.steeringManager.SteeringManual(deltaTime, Vector2.Normalize(limbDiff) * 3f);
					if (this.Character.AnimController.OnGround || this.Character.InWater)
					{
						this.Character.AnimController.Collider.ApplyForce(limbDiff * mouthLimb.Mass * 50f, 10f);
						return;
					}
				}
			}
			else
			{
				this.IgnoreTarget(base.SelectedAiTarget);
				this.ReleaseEatingTarget();
				base.ResetAITarget();
			}
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000FB90 File Offset: 0x0000DD90
		private void ReleaseEatingTarget()
		{
			this.State = AIState.Idle;
			this.Character.DeselectCharacter();
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000FBA4 File Offset: 0x0000DDA4
		private void UpdateFollow(float deltaTime)
		{
			if (base.SelectedAiTarget == null || base.SelectedAiTarget.Entity == null || base.SelectedAiTarget.Entity.Removed)
			{
				this.State = AIState.Idle;
				return;
			}
			if (this.Character.CurrentHull != null && this.steeringManager == this.insideSteering)
			{
				if ((this.Character.AnimController.InWater || !this.Character.AnimController.CanWalk) && this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin)
				{
					Character c = base.SelectedAiTarget.Entity as Character;
					if (c != null && base.VisibleHulls.Contains(c.CurrentHull))
					{
						base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
						goto IL_161;
					}
				}
				this.PathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 2f, this.minGapSize, null, null, null, true, 0f);
			}
			else
			{
				base.SteeringManager.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 5f);
			}
			IL_161:
			IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				if (!pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Unreachable)
				{
					this.State = AIState.Idle;
					this.IgnoreTarget(base.SelectedAiTarget);
					return;
				}
			}
			else if (this.Character.AnimController.InWater)
			{
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000FD7C File Offset: 0x0000DF7C
		public static bool IsLatchedTo(Character target, Character character)
		{
			EnemyAIController enemyAI = target.AIController as EnemyAIController;
			return enemyAI != null && enemyAI.LatchOntoAI != null && enemyAI.LatchOntoAI.IsAttached && enemyAI.LatchOntoAI.TargetCharacter == character;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000FDC0 File Offset: 0x0000DFC0
		public static bool IsLatchedToSomeoneElse(Character target, Character character)
		{
			EnemyAIController enemyAI = target.AIController as EnemyAIController;
			return enemyAI != null && enemyAI.LatchOntoAI != null && (enemyAI.LatchOntoAI.IsAttached && enemyAI.LatchOntoAI.TargetCharacter != null) && enemyAI.LatchOntoAI.TargetCharacter != character;
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060001ED RID: 493 RVA: 0x0000FE13 File Offset: 0x0000E013
		private bool IsLatchedOnSub
		{
			get
			{
				return this.LatchOntoAI != null && this.LatchOntoAI.IsAttachedToSub;
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000FE2C File Offset: 0x0000E02C
		public void UpdateTargets()
		{
			this.targetValue = 0f;
			AITarget newTarget = null;
			CharacterParams.TargetParams selectedTargetParams = null;
			AITargetMemory targetMemory = null;
			bool isAnyTargetClose = false;
			bool isBeingChased = this.IsBeingChased;
			bool isCharacterInside = this.Character.CurrentHull != null;
			bool tryToGetInside = this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True || (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.Partial && this.IsAggressiveBoarder);
			using (List<AITarget>.Enumerator enumerator = AITarget.List.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AITarget aiTarget = enumerator.Current;
					if (!aiTarget.ShouldBeIgnored() && !this.ignoredTargets.Contains(aiTarget) && aiTarget.Type != AITarget.TargetType.HumanOnly)
					{
						if (!this.TargetOutposts)
						{
							GameSession gameSession = GameMain.GameSession;
							if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode) && aiTarget.Entity.Submarine != null && aiTarget.Entity.Submarine.Info.IsOutpost)
							{
								continue;
							}
						}
						Character targetCharacter = aiTarget.Entity as Character;
						if (targetCharacter != this.Character)
						{
							if (EnemyAIController.TargetingRestrictions.HasFlag(EnemyTargetingRestrictions.PlayerCharacters))
							{
								if (targetCharacter != null && targetCharacter.IsPlayer)
								{
									continue;
								}
								Item item = aiTarget.Entity as Item;
								if (item != null)
								{
									Character character = item.GetRootInventoryOwner() as Character;
									if (character != null && character.IsPlayer)
									{
										continue;
									}
								}
							}
							if (EnemyAIController.TargetingRestrictions.HasFlag(EnemyTargetingRestrictions.PlayerSubmarines))
							{
								Submarine submarine = aiTarget.Entity.Submarine;
								SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
								if (submarineInfo != null && submarineInfo.IsPlayer)
								{
									continue;
								}
							}
							IEnumerable<Identifier> targetingTags = this.GetTargetingTags(aiTarget);
							Door door = null;
							if (targetCharacter != null)
							{
								if (targetCharacter.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
								{
									continue;
								}
								if (this.AIParams.Targets.None(null) && this.Character.IsFriendly(targetCharacter))
								{
									continue;
								}
							}
							else
							{
								if (aiTarget.Entity.Submarine != null)
								{
									if (!aiTarget.Entity.Submarine.Info.IsWreck && !aiTarget.Entity.Submarine.Info.IsBeacon)
									{
										Entity entity = aiTarget.Entity;
										if (entity is Structure || entity is Hull)
										{
											goto IL_2B4;
										}
										Item item3 = entity as Item;
										if (item3 != null && item3.body == null)
										{
											goto IL_2B4;
										}
										bool flag = false;
										IL_2BC:
										if ((!flag || !this.unattackableSubmarines.Contains(aiTarget.Entity.Submarine)) && (aiTarget.Entity.Submarine.Info.OutpostGenerationParams == null || aiTarget.Entity.Submarine.Info.OutpostGenerationParams.ForceToEndLocationIndex <= -1))
										{
											goto IL_324;
										}
										continue;
										IL_2B4:
										flag = true;
										goto IL_2BC;
									}
									continue;
								}
								IL_324:
								Hull hull = aiTarget.Entity as Hull;
								if (hull != null)
								{
									if (this.Character.CurrentHull != null || hull.Submarine == null)
									{
										continue;
									}
									if (hull.Submarine.Info.IsRuin)
									{
										continue;
									}
								}
								else
								{
									Item item2 = aiTarget.Entity as Item;
									if (item2 != null)
									{
										door = item2.GetComponent<Door>();
										bool targetingFromOutsideToInside = item2.CurrentHull != null && !isCharacterInside;
										if (targetingFromOutsideToInside && ((door != null && !this.canAttackDoors && !this.AIParams.CanOpenDoors) || !this.canAttackWalls))
										{
											continue;
										}
										if (door == null && targetingFromOutsideToInside)
										{
											Submarine submarine2 = item2.Submarine;
											SubmarineInfo submarineInfo = (submarine2 != null) ? submarine2.Info : null;
											if (submarineInfo != null && submarineInfo.IsRuin)
											{
												continue;
											}
										}
										else if (targetingTags.Contains(Tags.Nasonov) && (item2.Submarine == null || !item2.Submarine.Info.IsPlayer) && item2.ParentInventory == null)
										{
											continue;
										}
										if (this.Character.CurrentHull != null && targetingTags.Contains(Tags.Decoy))
										{
											continue;
										}
									}
									else
									{
										Structure s = aiTarget.Entity as Structure;
										if (s != null)
										{
											if (!s.HasBody || s.IsPlatform || s.Submarine == null || s.Submarine.Info.IsRuin)
											{
												continue;
											}
											bool isInnerWall = s.Prefab.Tags.Contains("inner");
											if ((isInnerWall && !isCharacterInside) || (!tryToGetInside && base.IsWallDisabled(s)))
											{
												continue;
											}
										}
									}
								}
								if (door != null)
								{
									if (door.Item.Submarine == null)
									{
										continue;
									}
									Gap linkedGap = door.LinkedGap;
									bool isOutdoor = linkedGap != null && linkedGap.FlowTargetHull != null && !linkedGap.IsRoomToRoom;
									if (this.Character.CurrentHull == null && !isOutdoor)
									{
										continue;
									}
									if (!door.CanBeTraversed)
									{
										if (!this.canAttackDoors)
										{
											continue;
										}
									}
									else if (this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.True)
									{
										continue;
									}
								}
								else
								{
									IDamageable damageable = aiTarget.Entity as IDamageable;
									if (damageable != null && damageable.Health <= 0f)
									{
										continue;
									}
								}
							}
							if (!targetingTags.None(null))
							{
								CharacterParams.TargetParams matchingTargetParams = null;
								foreach (CharacterParams.TargetParams targetParams in this.GetTargetParams(targetingTags))
								{
									if ((matchingTargetParams == null || matchingTargetParams.Priority <= targetParams.Priority) && (!targetParams.IgnoreInside || this.Character.CurrentHull == null) && (!targetParams.IgnoreOutside || this.Character.CurrentHull != null) && (!targetParams.IgnoreIncapacitated || targetCharacter == null || !targetCharacter.IsIncapacitated) && (!targetParams.IgnoreTargetInside || aiTarget.Entity.Submarine == null) && (!targetParams.IgnoreTargetOutside || aiTarget.Entity.Submarine != null))
									{
										Entity entity = aiTarget.Entity;
										ISerializableEntity se = entity as ISerializableEntity;
										if ((se == null || !targetParams.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se))) && !targetParams.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this.Character)))
										{
											if (targetParams.IgnoreIfNotInSameSub)
											{
												if (aiTarget.Entity.Submarine != this.Character.Submarine)
												{
													continue;
												}
												Hull hull2;
												if (targetCharacter == null)
												{
													Item it = aiTarget.Entity as Item;
													hull2 = ((it != null) ? it.CurrentHull : null);
												}
												else
												{
													hull2 = targetCharacter.CurrentHull;
												}
												Hull targetHull = hull2;
												if (targetHull == null != (this.Character.CurrentHull == null))
												{
													continue;
												}
											}
											AIState state = targetParams.State;
											bool flag = state == AIState.Eat || state == AIState.Observe;
											if (!flag || targetCharacter == null || targetCharacter.Submarine == this.Character.Submarine)
											{
												Item targetItem = aiTarget.Entity as Item;
												if (targetItem != null)
												{
													if (targetParams.IgnoreContained && targetItem.ParentInventory != null)
													{
														continue;
													}
													AIState state2 = targetParams.State;
													if (state2 != AIState.Attack && state2 != AIState.Aggressive)
													{
														if (state2 == AIState.FleeTo)
														{
															float healthThreshold = targetParams.Threshold;
															if (targetParams.ThresholdMin > 0f && targetParams.ThresholdMax > 0f)
															{
																healthThreshold = ((this.currentTargetingParams == targetParams && this.State == AIState.FleeTo) ? targetParams.ThresholdMax : targetParams.ThresholdMin);
															}
															if (this.Character.HealthPercentage > healthThreshold)
															{
																continue;
															}
														}
													}
													else if (!this.canAttackItems)
													{
														continue;
													}
													if (targetItem.HasTag(Tags.GuardianShelter))
													{
														bool ignore = false;
														foreach (Character otherCharacter in Character.CharacterList)
														{
															if (otherCharacter != this.Character)
															{
																AIController aicontroller = otherCharacter.AIController;
																if (((aicontroller != null) ? aicontroller.SelectedAiTarget : null) == aiTarget && this.Character.IsFriendly(otherCharacter))
																{
																	ignore = true;
																	break;
																}
															}
														}
														if (ignore)
														{
															continue;
														}
													}
												}
												matchingTargetParams = targetParams;
											}
										}
									}
								}
								if (matchingTargetParams != null)
								{
									float valueModifier = 1f;
									if (targetCharacter != null)
									{
										if (targetCharacter.AIController is EnemyAIController)
										{
											Identifier tag = matchingTargetParams.Tag;
											bool flag2 = tag == Tags.Stronger;
											bool flag3 = flag2;
											if (flag3)
											{
												AIState state = this.State;
												bool flag = state == AIState.Escape || state - AIState.Flee <= 1;
												flag3 = flag;
											}
											if (flag3)
											{
												if (base.SelectedAiTarget == aiTarget)
												{
													valueModifier *= 2f;
												}
												if (this.IsBeingChasedBy(targetCharacter))
												{
													valueModifier *= 2f;
												}
												if (this.Character.CurrentHull != null && !base.VisibleHulls.Contains(targetCharacter.CurrentHull))
												{
													valueModifier /= 2f;
												}
											}
										}
									}
									else
									{
										Structure s2 = aiTarget.Entity as Structure;
										if (s2 != null)
										{
											bool isInnerWall2 = s2.Prefab.Tags.Contains("inner");
											valueModifier = 200f / s2.MaxHealth;
											for (int i = 0; i < s2.Sections.Length; i++)
											{
												WallSection section = s2.Sections[i];
												if (section.gap != null)
												{
													bool leadsInside = !section.gap.IsRoomToRoom && section.gap.FlowTargetHull != null;
													if (tryToGetInside)
													{
														if (!isCharacterInside)
														{
															if (this.CanPassThroughHole(s2, i))
															{
																valueModifier *= (leadsInside ? (this.IsAggressiveBoarder ? 5f : 1f) : 0f);
															}
															else if (this.IsAggressiveBoarder && leadsInside && this.canAttackWalls)
															{
																valueModifier *= 1f + section.gap.Open;
															}
														}
														else if (this.IsAggressiveBoarder)
														{
															if (!isInnerWall2)
															{
																valueModifier = 0f;
																break;
															}
															if (this.CanPassThroughHole(s2, i))
															{
																valueModifier *= (isInnerWall2 ? 0.5f : 0f);
															}
															else
															{
																if (!this.canAttackWalls)
																{
																	valueModifier = 0f;
																	break;
																}
																valueModifier = 0.1f;
															}
														}
														else
														{
															if (!this.canAttackWalls)
															{
																valueModifier = 0f;
																break;
															}
															valueModifier *= 1f - section.gap.Open * 0.25f;
															valueModifier = Math.Max(valueModifier, 0.1f);
														}
													}
													else
													{
														if (isInnerWall2 || !this.canAttackWalls)
														{
															valueModifier = 0f;
															break;
														}
														if (this.IsAggressiveBoarder)
														{
															valueModifier *= 1f + section.gap.Open;
														}
													}
													valueModifier = Math.Clamp(valueModifier, 0f, 5f);
												}
											}
										}
										if (door != null)
										{
											if (this.IsAggressiveBoarder)
											{
												if (this.Character.CurrentHull == null)
												{
													if (door.CanBeTraversed)
													{
														valueModifier = 5f;
													}
													else if (door.LinkedGap != null)
													{
														valueModifier = 1f + door.LinkedGap.Open * 4f;
													}
												}
												else
												{
													bool isOpen = door.CanBeTraversed;
													Gap linkedGap = door.LinkedGap;
													bool isOutdoor2 = linkedGap != null && linkedGap.FlowTargetHull != null && !linkedGap.IsRoomToRoom;
													valueModifier = (float)((isOpen || isOutdoor2) ? 0 : 1);
												}
											}
										}
										else
										{
											IDamageable damageable = aiTarget.Entity as IDamageable;
											if (damageable != null && damageable.Health <= 0f)
											{
												continue;
											}
										}
									}
									if (matchingTargetParams.State == AIState.Eat && this.Character.Params.Health.HealthRegenerationWhenEating > 0f && !this.Character.IsPet)
									{
										valueModifier *= MathHelper.Lerp(1f, 0.1f, this.Character.HealthPercentage / 100f);
									}
									valueModifier *= matchingTargetParams.Priority;
									if (valueModifier != 0f)
									{
										Identifier tag = matchingTargetParams.Tag;
										if (tag != Tags.Decoy)
										{
											if (this.SwarmBehavior != null && this.SwarmBehavior.Members.Any<AICharacter>())
											{
												using (List<AICharacter>.Enumerator enumerator4 = this.SwarmBehavior.Members.GetEnumerator())
												{
													while (enumerator4.MoveNext())
													{
														AICharacter otherCharacter2 = enumerator4.Current;
														if (otherCharacter2 != this.Character)
														{
															AIController aicontroller2 = otherCharacter2.AIController;
															if (((aicontroller2 != null) ? aicontroller2.SelectedAiTarget : null) == aiTarget)
															{
																valueModifier /= 2f;
															}
														}
													}
													goto IL_DC2;
												}
											}
											foreach (Character otherCharacter3 in Character.CharacterList)
											{
												if (otherCharacter3 != this.Character)
												{
													AIController aicontroller3 = otherCharacter3.AIController;
													if (((aicontroller3 != null) ? aicontroller3.SelectedAiTarget : null) == aiTarget && this.Character.IsFriendly(otherCharacter3))
													{
														valueModifier /= 2f;
													}
												}
											}
										}
										IL_DC2:
										if (aiTarget.IsWithinSector(base.WorldPosition))
										{
											Vector2 toTarget = aiTarget.WorldPosition - this.Character.WorldPosition;
											float dist = toTarget.Length();
											float nonModifiedDist = dist;
											if (this.targetMemories.ContainsKey(aiTarget))
											{
												dist *= 0.9f;
											}
											if (matchingTargetParams.PerceptionDistanceMultiplier > 0f)
											{
												dist /= matchingTargetParams.PerceptionDistanceMultiplier;
											}
											if ((matchingTargetParams.MaxPerceptionDistance <= 0f || dist * dist <= matchingTargetParams.MaxPerceptionDistance * matchingTargetParams.MaxPerceptionDistance) && (this.State != AIState.PlayDead || targetCharacter != null))
											{
												AITarget aiTarget3 = aiTarget;
												float dist2 = dist;
												bool flag4 = base.SelectedAiTarget != aiTarget;
												bool flag5 = flag4;
												if (!flag5)
												{
													AIState state = this.State;
													bool flag = state == AIState.PlayDead || state == AIState.Hiding;
													flag5 = flag;
												}
												if (this.CanPerceive(aiTarget3, dist2, -1f, flag5))
												{
													if (base.SelectedAiTarget == aiTarget)
													{
														if (this.Character.Submarine == null)
														{
															Entity entity = aiTarget.Entity;
															ISpatialEntity spatialEntity = entity;
															if (spatialEntity != null && ((ISpatialEntity)entity).Submarine != null)
															{
																tag = matchingTargetParams.Tag;
																if (!(tag == Tags.Door))
																{
																	Identifier tag2 = matchingTargetParams.Tag;
																	if (!(tag2 == Tags.Wall))
																	{
																		goto IL_106F;
																	}
																}
																Vector2 rayStart = this.Character.SimPosition;
																Vector2 rayEnd = aiTarget.SimPosition + spatialEntity.Submarine.SimPosition;
																Body closestBody = Submarine.PickBody(rayStart, rayEnd, null, new Category?(Category.Cat1 | Category.Cat8), true, null, true);
																if (closestBody != null)
																{
																	ISpatialEntity hit = closestBody.UserData as ISpatialEntity;
																	if (hit != null)
																	{
																		Vector2 hitPos = hit.SimPosition;
																		if (closestBody.UserData is Submarine)
																		{
																			hitPos = Submarine.LastPickedPosition;
																		}
																		else if (hit.Submarine != null)
																		{
																			hitPos += hit.Submarine.SimPosition;
																		}
																		float subHalfWidth = (float)spatialEntity.Submarine.Borders.Width / 2f;
																		float subHalfHeight = (float)spatialEntity.Submarine.Borders.Height / 2f;
																		Vector2 diff = ConvertUnits.ToDisplayUnits(rayEnd - hitPos);
																		bool isOtherSideOfTheSub = Math.Abs(diff.X) > subHalfWidth || Math.Abs(diff.Y) > subHalfHeight;
																		if (isOtherSideOfTheSub)
																		{
																			this.IgnoreTarget(aiTarget);
																			base.ResetAITarget();
																			continue;
																		}
																	}
																}
															}
														}
														IL_106F:
														valueModifier *= 1.1f;
													}
													if (!isBeingChased)
													{
														AIState state = matchingTargetParams.State;
														bool flag = state - AIState.Avoid <= 2;
														if (flag)
														{
															float reactDistance = matchingTargetParams.ReactDistance;
															if (reactDistance > 0f && reactDistance < dist)
															{
																continue;
															}
														}
													}
													dist = Math.Max(dist, 100f);
													targetMemory = this.GetTargetMemory(aiTarget, true, base.SelectedAiTarget != aiTarget);
													if (this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin && this.Character.CurrentHull != null)
													{
														float diff2 = Math.Abs(toTarget.Y) - this.Character.CurrentHull.Size.Y;
														if (diff2 > 0f)
														{
															dist *= MathHelper.Clamp(diff2 / 100f, 2f, 3f);
														}
													}
													if (this.Character.Submarine == null)
													{
														Entity entity2 = aiTarget.Entity;
														if (((entity2 != null) ? entity2.Submarine : null) != null && targetCharacter == null)
														{
															bool prioritizeSubCenter = matchingTargetParams.PrioritizeSubCenter;
															bool flag6 = prioritizeSubCenter;
															if (!flag6)
															{
																AttackPattern attackPattern = matchingTargetParams.AttackPattern;
																bool flag = attackPattern - AttackPattern.Sweep <= 1;
																flag6 = flag;
															}
															if (flag6 && !isAnyTargetClose)
															{
																if (Submarine.MainSubs.Contains(aiTarget.Entity.Submarine))
																{
																	float horizontalDistanceToSubCenter = Math.Abs(aiTarget.WorldPosition.X - aiTarget.Entity.Submarine.WorldPosition.X);
																	dist *= MathHelper.Lerp(1f, 5f, MathUtils.InverseLerp(0f, 10000f, horizontalDistanceToSubCenter));
																}
																else if (matchingTargetParams.AttackPattern == AttackPattern.Circle)
																{
																	dist *= 5f;
																}
															}
														}
													}
													if (targetCharacter != null && this.Character.CurrentHull != null && this.Character.CurrentHull == targetCharacter.CurrentHull)
													{
														dist /= 2f;
													}
													AIState state3 = matchingTargetParams.State;
													if (state3 != AIState.Escape && state3 != AIState.Avoid && (matchingTargetParams.State != AIState.Attack || this.State != matchingTargetParams.State || base.SelectedAiTarget != aiTarget))
													{
														AITarget aiTarget2 = aiTarget;
														bool flag7;
														if (aiTarget2 == null)
														{
															flag7 = (null != null);
														}
														else
														{
															Entity entity3 = aiTarget2.Entity;
															flag7 = (((entity3 != null) ? entity3.Submarine : null) != null);
														}
														Vector2 vector;
														if ((!flag7 || aiTarget.Entity.Submarine != this.Character.Submarine) && !this.IsPositionInsideAllowedZone(aiTarget.WorldPosition, out vector))
														{
															bool isTargetInPlayerTeam = EnemyAIController.IsTargetInPlayerTeam(aiTarget);
															if (this.Character.LastAttackers.None((Character.Attacker a) => a.Damage > 0f && a.Character != null && (a.Character == aiTarget.Entity || (a.Character.IsOnPlayerTeam & isTargetInPlayerTeam))))
															{
																continue;
															}
														}
													}
													valueModifier *= targetMemory.Priority / MathF.Sqrt(dist);
													if (valueModifier > this.targetValue)
													{
														Item j = aiTarget.Entity as Item;
														if (j != null)
														{
															Character owner = EnemyAIController.GetOwner(j);
															if (owner == this.Character)
															{
																continue;
															}
															if (owner != null)
															{
																if ((owner.AiTarget != null && this.ignoredTargets.Contains(owner.AiTarget)) || this.Character.IsFriendly(owner) || owner.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
																{
																	continue;
																}
																if (this.GetTargetParams(this.GetTargetingTags(owner.AiTarget)).Any((CharacterParams.TargetParams t) => t.State == AIState.Idle))
																{
																	continue;
																}
															}
														}
														if (targetCharacter != null)
														{
															if (this.Character.CurrentHull != null && targetCharacter.CurrentHull != this.Character.CurrentHull)
															{
																AIState state = matchingTargetParams.State;
																bool flag = state == AIState.Eat || state == AIState.Observe;
																bool flag8 = flag;
																bool flag9 = flag8;
																if (!flag9)
																{
																	AIState state4 = matchingTargetParams.State;
																	bool flag10 = state4 == AIState.Protect || state4 == AIState.Follow;
																	flag9 = (flag10 && (!this.Character.CanClimb || !this.Character.CanInteract || !this.AIParams.CanOpenDoors || !this.Character.Params.UsePathFinding));
																}
																if (flag9 && !base.VisibleHulls.Contains(targetCharacter.CurrentHull))
																{
																	continue;
																}
															}
															if (targetCharacter.Submarine != this.Character.Submarine || targetCharacter.CurrentHull == null != (this.Character.CurrentHull == null))
															{
																if (targetCharacter.Submarine != null)
																{
																	if (this.Character.Submarine != null && !targetCharacter.Submarine.IsConnectedTo(this.Character.Submarine))
																	{
																		continue;
																	}
																	valueModifier *= 0.5f;
																}
																else if (this.Character.CurrentHull != null)
																{
																	AITarget selectedAiTarget = base.SelectedAiTarget;
																	if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) != targetCharacter)
																	{
																		continue;
																	}
																}
															}
															else if (targetCharacter.Submarine == null && this.Character.Submarine == null && dist > Math.Clamp(ConvertUnits.ToDisplayUnits(this.colliderLength) * 10f, 1000f, 5000f) && Submarine.PickBodies(base.SimPosition, targetCharacter.SimPosition, null, new Category?(Category.Cat8), true, null, false).Any<Body>())
															{
																continue;
															}
														}
														newTarget = aiTarget;
														selectedTargetParams = matchingTargetParams;
														this.targetValue = valueModifier;
														if (!isAnyTargetClose)
														{
															isAnyTargetClose = (ConvertUnits.ToDisplayUnits(this.colliderLength) > nonModifiedDist);
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
				}
			}
			this.currentTargetingParams = selectedTargetParams;
			this.currentTargetMemory = targetMemory;
			CharacterParams.TargetParams targetParams2 = this.currentTargetingParams;
			this.State = ((targetParams2 != null) ? targetParams2.State : AIState.Idle);
			base.SelectedAiTarget = newTarget;
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			bool flag11 = latchOntoAI == null || !latchOntoAI.IsAttached || this.wallTarget != null;
			bool flag12 = flag11;
			if (flag12)
			{
				AIState state = this.State;
				bool flag = state == AIState.Attack || state - AIState.Aggressive <= 1;
				flag12 = flag;
			}
			if (flag12)
			{
				this.UpdateWallTarget(this.requiredHoleCount);
			}
			this.updateTargetsTimer = this.updateTargetsInterval * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00011594 File Offset: 0x0000F794
		private void UpdateWallTarget(int requiredHoleCount)
		{
			EnemyAIController.<>c__DisplayClass195_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.requiredHoleCount = requiredHoleCount;
			this.wallTarget = null;
			if (base.SelectedAiTarget == null)
			{
				return;
			}
			if (base.SelectedAiTarget.Entity == null)
			{
				return;
			}
			if (!this.canAttackWalls)
			{
				return;
			}
			if (base.HasValidPath(true, true, null))
			{
				return;
			}
			this.wallHits.Clear();
			CS$<>8__locals1.wall = null;
			Vector2 refPos = (this.AttackLimb != null) ? this.AttackLimb.SimPosition : base.SimPosition;
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Target))
			{
				Vector2 rayStart = refPos;
				Vector2 rayEnd = base.SelectedAiTarget.SimPosition;
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayEnd -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|195_0(rayStart, rayEnd, ref CS$<>8__locals1);
			}
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Heading))
			{
				Vector2 rayStart2 = refPos;
				Vector2 rayEnd2 = rayStart2 + VectorExtensions.Forward(this.Character.AnimController.Collider.Rotation + 1.5707964f, this.avoidLookAheadDistance * 5f);
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart2 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
					rayEnd2 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayStart2 -= this.Character.Submarine.SimPosition;
					rayEnd2 -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|195_0(rayStart2, rayEnd2, ref CS$<>8__locals1);
			}
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Steering))
			{
				Vector2 rayStart3 = refPos;
				Vector2 rayEnd3 = rayStart3 + base.Steering * 5f;
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart3 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
					rayEnd3 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayStart3 -= this.Character.Submarine.SimPosition;
					rayEnd3 -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|195_0(rayStart3, rayEnd3, ref CS$<>8__locals1);
			}
			if (this.wallHits.Any<ValueTuple<Body, int, Vector2>>())
			{
				float targetDistance = ConvertUnits.ToSimUnits(base.SelectedAiTarget.WorldPosition - ((this.AttackLimb != null) ? this.AttackLimb.WorldPosition : base.WorldPosition)).LengthSquared();
				Body closestBody = null;
				float closestDistance = 0f;
				int sectionIndex = -1;
				Vector2 sectionPos = Vector2.Zero;
				foreach (ValueTuple<Body, int, Vector2> valueTuple in this.wallHits)
				{
					Body body = valueTuple.Item1;
					int index = valueTuple.Item2;
					Vector2 sectionPosition = valueTuple.Item3;
					Structure structure = body.UserData as Structure;
					float distance = Vector2.DistanceSquared(refPos, Submarine.GetRelativeSimPosition(ConvertUnits.ToSimUnits(sectionPosition), this.Character.Submarine, structure.Submarine));
					if (distance <= targetDistance && (closestBody == null || closestDistance == 0f || distance < closestDistance))
					{
						closestBody = body;
						closestDistance = distance;
						CS$<>8__locals1.wall = structure;
						sectionPos = sectionPosition;
						sectionIndex = index;
					}
				}
				if (closestBody == null || sectionIndex == -1)
				{
					return;
				}
				Vector2 attachTargetNormal;
				if (CS$<>8__locals1.wall.IsHorizontal)
				{
					attachTargetNormal = new Vector2(0f, (float)Math.Sign(base.WorldPosition.Y - CS$<>8__locals1.wall.WorldPosition.Y));
					sectionPos.Y += ((CS$<>8__locals1.wall.BodyHeight <= 0f) ? ((float)CS$<>8__locals1.wall.Rect.Height) : CS$<>8__locals1.wall.BodyHeight) / 2f * attachTargetNormal.Y;
				}
				else
				{
					attachTargetNormal = new Vector2((float)Math.Sign(base.WorldPosition.X - CS$<>8__locals1.wall.WorldPosition.X), 0f);
					sectionPos.X += ((CS$<>8__locals1.wall.BodyWidth <= 0f) ? ((float)CS$<>8__locals1.wall.Rect.Width) : CS$<>8__locals1.wall.BodyWidth) / 2f * attachTargetNormal.X;
				}
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.SetAttachTarget(CS$<>8__locals1.wall, ConvertUnits.ToSimUnits(sectionPos), attachTargetNormal);
				}
				if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True || (!CS$<>8__locals1.wall.SectionBodyDisabled(sectionIndex) && !base.IsWallDisabled(CS$<>8__locals1.wall)))
				{
					if (!CS$<>8__locals1.wall.NoAITarget || this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.True)
					{
						this.wallTarget = new EnemyAIController.WallTarget(sectionPos, CS$<>8__locals1.wall, sectionIndex);
						return;
					}
					Item i = base.SelectedAiTarget.Entity as Item;
					if (i == null || i.GetComponent<Door>() == null)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						base.ResetAITarget();
						return;
					}
				}
				else
				{
					this.IgnoreTarget(base.SelectedAiTarget);
					base.ResetAITarget();
				}
			}
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00011BC8 File Offset: 0x0000FDC8
		private bool TrySteerThroughGaps(float deltaTime)
		{
			if (this.wallTarget != null && this.wallTarget.SectionIndex > -1 && base.CanPassThroughHole(this.wallTarget.Structure, this.wallTarget.SectionIndex, this.requiredHoleCount))
			{
				WallSection section = this.wallTarget.Structure.GetSection(this.wallTarget.SectionIndex);
				Vector2 targetPos = this.wallTarget.Structure.SectionPosition(this.wallTarget.SectionIndex, true);
				return ((section != null) ? section.gap : null) != null && this.SteerThroughGap(this.wallTarget.Structure, section, targetPos, deltaTime);
			}
			if (base.SelectedAiTarget != null)
			{
				Structure wall = base.SelectedAiTarget.Entity as Structure;
				if (wall != null)
				{
					for (int i = 0; i < wall.Sections.Length; i++)
					{
						WallSection section2 = wall.Sections[i];
						if (base.CanPassThroughHole(wall, i, this.requiredHoleCount) && ((section2 != null) ? section2.gap : null) != null)
						{
							return this.SteerThroughGap(wall, section2, wall.SectionPosition(i, true), deltaTime);
						}
					}
				}
				else
				{
					Item j = base.SelectedAiTarget.Entity as Item;
					if (j != null)
					{
						Door door = j.GetComponent<Door>();
						bool flag;
						if (door == null)
						{
							flag = (null != null);
						}
						else
						{
							Gap linkedGap = door.LinkedGap;
							flag = (((linkedGap != null) ? linkedGap.FlowTargetHull : null) != null);
						}
						if (flag && !door.LinkedGap.IsRoomToRoom && door.CanBeTraversed && (this.Character.AnimController.CanWalk || door.LinkedGap.FlowTargetHull.WaterPercentage > 25f) && door.LinkedGap.Size > ConvertUnits.ToDisplayUnits(this.colliderWidth))
						{
							float maxDistance = Math.Max(ConvertUnits.ToDisplayUnits(this.colliderLength), 100f);
							return this.SteerThroughGap(door.LinkedGap, door.LinkedGap.FlowTargetHull.WorldPosition, deltaTime, maxDistance);
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00011DC0 File Offset: 0x0000FFC0
		private AITargetMemory GetTargetMemory(AITarget target, bool addIfNotFound = false, bool keepAlive = false)
		{
			AITargetMemory memory;
			if (!this.targetMemories.TryGetValue(target, out memory) && addIfNotFound)
			{
				memory = new AITargetMemory(target, 10f);
				this.targetMemories.Add(target, memory);
			}
			if (keepAlive)
			{
				memory.Priority = Math.Max(memory.Priority, 10f);
			}
			return memory;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00011E14 File Offset: 0x00010014
		private void UpdateCurrentMemoryLocation()
		{
			if (this._selectedAiTarget != null)
			{
				if (this._selectedAiTarget.Entity == null || this._selectedAiTarget.Entity.Removed)
				{
					this._selectedAiTarget = null;
					return;
				}
				if (this.CanPerceive(this._selectedAiTarget, -1f, -1f, false))
				{
					AITargetMemory memory = this.GetTargetMemory(this._selectedAiTarget, false, false);
					if (memory != null)
					{
						memory.Location = this._selectedAiTarget.WorldPosition;
					}
				}
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00011E8C File Offset: 0x0001008C
		private void FadeMemories(float deltaTime)
		{
			this.removals.Clear();
			foreach (KeyValuePair<AITarget, AITargetMemory> kvp in this.targetMemories)
			{
				AITarget target = kvp.Key;
				AITargetMemory memory = kvp.Value;
				float fadeTime = this.memoryFadeTime;
				if (target == base.SelectedAiTarget)
				{
					fadeTime = 0f;
				}
				else if (target == this._lastAiTarget)
				{
					fadeTime /= 2f;
				}
				memory.Priority -= fadeTime * deltaTime;
				if (memory.Priority <= 1f || target.Entity == null || target.Entity.Removed || !AITarget.List.Contains(target))
				{
					this.removals.Add(target);
				}
			}
			this.removals.ForEach(delegate(AITarget r)
			{
				this.targetMemories.Remove(r);
			});
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00011F8C File Offset: 0x0001018C
		public void IgnoreTarget(AITarget target)
		{
			if (target == null)
			{
				return;
			}
			this.ignoredTargets.Add(target);
			this.targetIgnoreTimer = this.targetIgnoreTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00011FBC File Offset: 0x000101BC
		public void LaunchTrigger(StatusEffect.AITrigger trigger)
		{
			if (trigger.IsTriggered)
			{
				return;
			}
			if (this.activeTriggers.ContainsKey(trigger))
			{
				return;
			}
			if (this.activeTriggers.ContainsValue(this.currentTargetingParams))
			{
				if (!trigger.AllowToOverride)
				{
					return;
				}
				KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> existingTrigger = this.activeTriggers.FirstOrDefault((KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> kvp) => kvp.Value == this.currentTargetingParams && kvp.Key.AllowToBeOverridden);
				if (existingTrigger.Key == null)
				{
					return;
				}
				this.activeTriggers.Remove(existingTrigger.Key);
			}
			trigger.Launch();
			this.activeTriggers.Add(trigger, this.currentTargetingParams);
			this.ChangeParams(this.currentTargetingParams, trigger.State, null);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00012068 File Offset: 0x00010268
		private void UpdateTriggers(float deltaTime)
		{
			foreach (KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> triggerObject in this.activeTriggers)
			{
				StatusEffect.AITrigger trigger = triggerObject.Key;
				if (!trigger.IsPermanent)
				{
					trigger.UpdateTimer(deltaTime);
					if (!trigger.IsActive)
					{
						trigger.Reset();
						this.ResetParams(triggerObject.Value);
						this.inactiveTriggers.Add(trigger);
					}
				}
			}
			foreach (StatusEffect.AITrigger trigger2 in this.inactiveTriggers)
			{
				this.activeTriggers.Remove(trigger2);
			}
			this.inactiveTriggers.Clear();
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00012148 File Offset: 0x00010348
		private bool TryResetOriginalState(Identifier tag)
		{
			if (!this.modifiedParams.ContainsKey(tag))
			{
				return false;
			}
			IEnumerable<CharacterParams.TargetParams> matchingParams;
			if (this.AIParams.TryGetTargets(tag, out matchingParams))
			{
				using (IEnumerator<CharacterParams.TargetParams> enumerator = matchingParams.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						CharacterParams.TargetParams targetParams = enumerator.Current;
						this.modifiedParams.Remove(tag);
						if (this.tempParams.ContainsKey(tag))
						{
							this.tempParams.Values.ForEach(delegate(CharacterParams.TargetParams t)
							{
								this.AIParams.RemoveTarget(t);
							});
							this.tempParams.Remove(tag);
						}
						this.ResetParams(targetParams);
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x000121FC File Offset: 0x000103FC
		private void ChangeParams(CharacterParams.TargetParams targetParams, AIState state, float? priority = null)
		{
			if (targetParams == null)
			{
				return;
			}
			if (priority != null)
			{
				targetParams.Priority = priority.Value;
			}
			targetParams.State = state;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00012220 File Offset: 0x00010420
		private void ResetParams(CharacterParams.TargetParams targetParams)
		{
			if (targetParams != null)
			{
				targetParams.Reset();
			}
			bool flag = this.currentTargetingParams == targetParams;
			bool flag2 = flag;
			if (!flag2)
			{
				AIState state = this.State;
				bool flag3 = state == AIState.Idle || state == AIState.Patrol;
				flag2 = flag3;
			}
			if (flag2)
			{
				base.ResetAITarget();
				this.State = AIState.Idle;
				this.PreviousState = AIState.Idle;
			}
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00012274 File Offset: 0x00010474
		private void ChangeParams(Identifier tag, AIState state, float? priority = null, bool onlyExisting = false, bool ignoreAttacksIfNotInSameSub = false)
		{
			IEnumerable<CharacterParams.TargetParams> existingTargetParams = this.GetTargetParams(tag);
			if (existingTargetParams.None(null))
			{
				CharacterParams.TargetParams targetParams;
				if (!onlyExisting && !this.tempParams.ContainsKey(tag) && this.AIParams.TryAddNewTarget(tag, state, priority.GetValueOrDefault(10f), out targetParams))
				{
					this.tempParams.Add(tag, targetParams);
					return;
				}
			}
			else
			{
				foreach (CharacterParams.TargetParams targetParams2 in existingTargetParams)
				{
					if (priority != null)
					{
						targetParams2.Priority = Math.Max(targetParams2.Priority, priority.Value);
					}
					targetParams2.State = state;
					if (state == AIState.Attack)
					{
						targetParams2.IgnoreIfNotInSameSub = ignoreAttacksIfNotInSameSub;
						targetParams2.IgnoreInside = false;
						targetParams2.IgnoreOutside = false;
						targetParams2.IgnoreTargetInside = false;
						targetParams2.IgnoreTargetOutside = false;
						targetParams2.IgnoreIncapacitated = false;
					}
				}
				this.modifiedParams.TryAdd(tag, existingTargetParams);
			}
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00012374 File Offset: 0x00010574
		private void ChangeTargetState(Identifier tag, AIState state, float? priority = null)
		{
			this.isStateChanged = true;
			this.SetStateResetTimer();
			this.ChangeParams(tag, state, priority, false, false);
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00012390 File Offset: 0x00010590
		private void ChangeTargetState(Character target, AIState state, float? priority = null)
		{
			this.isStateChanged = true;
			this.SetStateResetTimer();
			if (!this.Character.IsPet || !target.IsHuman)
			{
				this.ChangeParams(target.SpeciesName, state, priority, false, !target.IsHuman);
			}
			if (target.IsHuman)
			{
				CharacterParams.TargetParams targetParams;
				if (this.AIParams.TryGetHighestPriorityTarget(Tags.Human, out targetParams))
				{
					priority = new float?(targetParams.Priority);
				}
				bool flag = state - AIState.Attack <= 1;
				if (flag)
				{
					this.ChangeParams(Tags.Weapon, state, priority, false, false);
					this.ChangeParams(Tags.ToolItem, state, priority, false, false);
				}
				if (state == AIState.Attack)
				{
					if (target.Submarine != null && this.Character.Submarine == null && (this.canAttackDoors || this.canAttackWalls))
					{
						this.ChangeParams(Tags.Room, state, priority / (float)2, false, false);
						if (this.canAttackWalls)
						{
							this.ChangeParams(Tags.Wall, state, priority / (float)2, false, false);
						}
						if (this.canAttackDoors && this.IsAggressiveBoarder)
						{
							this.ChangeParams(Tags.Door, state, priority / (float)2, false, false);
						}
					}
					this.ChangeParams(Tags.Provocative, state, priority, true, false);
				}
			}
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0001252E File Offset: 0x0001072E
		private void ResetOriginalState()
		{
			this.isStateChanged = false;
			this.modifiedParams.Keys.ForEachMod(delegate(Identifier tag)
			{
				this.TryResetOriginalState(tag);
			});
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00012554 File Offset: 0x00010754
		protected override void OnTargetChanged(AITarget previousTarget, AITarget newTarget)
		{
			base.OnTargetChanged(previousTarget, newTarget);
			if ((newTarget != null || this.wallTarget != null) && this.IsLatchedOnSub)
			{
				Structure wall = ((newTarget != null) ? newTarget.Entity : null) as Structure;
				if (wall == null)
				{
					EnemyAIController.WallTarget wallTarget = this.wallTarget;
					wall = ((wallTarget != null) ? wallTarget.Structure : null);
				}
				bool flag;
				if (((wall != null) ? wall.Bodies : null) != null)
				{
					if (!wall.Bodies.Contains(this.LatchOntoAI.AttachJoints[0].BodyB))
					{
						Submarine submarine = wall.Submarine;
						Body body;
						if (submarine == null)
						{
							body = null;
						}
						else
						{
							PhysicsBody physicsBody = submarine.PhysicsBody;
							body = ((physicsBody != null) ? physicsBody.FarseerBody : null);
						}
						flag = (body != this.LatchOntoAI.AttachJoints[0].BodyB);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = true;
				}
				bool releaseTarget = flag;
				if (!releaseTarget)
				{
					for (int i = 0; i < wall.Sections.Length; i++)
					{
						if (this.CanPassThroughHole(wall, i))
						{
							releaseTarget = true;
						}
					}
				}
				if (releaseTarget)
				{
					this.wallTarget = null;
					this.LatchOntoAI.DeattachFromBody(true, 1f);
				}
			}
			else
			{
				this.wallTarget = null;
			}
			if (newTarget == null)
			{
				return;
			}
			if (this.currentTargetingParams != null)
			{
				this.observeTimer = this.currentTargetingParams.Timer * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
			this.reachTimer = 0f;
			this.sinTime = 0f;
			if (this.breakCircling && this.strikeTimer <= 0f && this.CirclePhase != CirclePhase.CloseIn)
			{
				this.CirclePhase = CirclePhase.Start;
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x000126CC File Offset: 0x000108CC
		protected override void OnStateChanged(AIState from, AIState to)
		{
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			if (latchOntoAI != null)
			{
				latchOntoAI.DeattachFromBody(true, 0f);
			}
			if (this.disableTailCoroutine != null)
			{
				bool flag = from - AIState.HideTo <= 1;
				bool flag2 = flag;
				bool flag3 = flag2;
				if (flag3)
				{
					bool flag4 = to - AIState.HideTo <= 1;
					flag3 = flag4;
				}
				if (!flag3)
				{
					CoroutineManager.StopCoroutines(this.disableTailCoroutine);
					this.Character.AnimController.RestoreTemporarilyDisabled();
					this.disableTailCoroutine = null;
				}
			}
			if (to == AIState.Hiding)
			{
				this.ReleaseDragTargets();
			}
			this.Character.AnimController.ReleaseStuckLimbs();
			this.AttackLimb = null;
			this.movementMargin = 0f;
			base.ResetEscape();
			if (this.isStateChanged && to == AIState.Idle && from != to)
			{
				this.SetStateResetTimer();
			}
			this.blockCheckTimer = 0f;
			this.reachTimer = 0f;
			this.sinTime = 0f;
			if (this.breakCircling && this.strikeTimer <= 0f && this.CirclePhase != CirclePhase.CloseIn)
			{
				this.CirclePhase = CirclePhase.Start;
			}
			if (to != AIState.Idle)
			{
				this.playDeadTimer = 60f;
			}
			if (to == AIState.Attack)
			{
				this.Character.PlaySound(CharacterSound.SoundType.Attack, 1f, 3f);
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x000127FB File Offset: 0x000109FB
		private void SetStateResetTimer()
		{
			this.stateResetTimer = this.stateResetCooldown * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0001281C File Offset: 0x00010A1C
		private float GetPerceivingRange(AITarget target)
		{
			float maxSightOrSoundRange = Math.Max(target.SightRange * this.Sight, target.SoundRange * this.Hearing);
			if (this.AIParams.MaxPerceptionDistance >= 0f && maxSightOrSoundRange > this.AIParams.MaxPerceptionDistance)
			{
				return this.AIParams.MaxPerceptionDistance;
			}
			return maxSightOrSoundRange;
		}

		// Token: 0x06000202 RID: 514 RVA: 0x00012878 File Offset: 0x00010A78
		private bool CanPerceive(AITarget target, float dist = -1f, float distSquared = -1f, bool checkVisibility = false)
		{
			if (((target != null) ? target.Entity : null) == null)
			{
				return false;
			}
			if (checkVisibility)
			{
				Submarine mySub = this.Character.Submarine;
				Submarine targetSub = target.Entity.Submarine;
				checkVisibility = ((this.Character.IsPet && (mySub != null || targetSub != null)) || (mySub != null && (targetSub == null || (targetSub == mySub && !targetSub.Info.IsPlayer))));
			}
			bool insideSightRange;
			bool insideSoundRange;
			if (dist > 0f)
			{
				if (this.AIParams.MaxPerceptionDistance >= 0f && dist > this.AIParams.MaxPerceptionDistance)
				{
					return false;
				}
				insideSightRange = EnemyAIController.<CanPerceive>g__IsInRange|226_0(dist, target.SightRange, this.Sight);
				if (!checkVisibility && insideSightRange)
				{
					return true;
				}
				insideSoundRange = EnemyAIController.<CanPerceive>g__IsInRange|226_0(dist, target.SoundRange, this.Hearing);
			}
			else
			{
				if (distSquared < 0f)
				{
					distSquared = Vector2.DistanceSquared(this.Character.WorldPosition, target.WorldPosition);
				}
				if (this.AIParams.MaxPerceptionDistance >= 0f && distSquared > this.AIParams.MaxPerceptionDistance * this.AIParams.MaxPerceptionDistance)
				{
					return false;
				}
				insideSightRange = EnemyAIController.<CanPerceive>g__IsInRangeSqr|226_1(distSquared, target.SightRange, this.Sight);
				if (!checkVisibility && insideSightRange)
				{
					return true;
				}
				insideSoundRange = EnemyAIController.<CanPerceive>g__IsInRangeSqr|226_1(distSquared, target.SoundRange, this.Hearing);
			}
			if (!checkVisibility)
			{
				return insideSightRange || insideSoundRange;
			}
			if (!insideSightRange && !insideSoundRange)
			{
				return false;
			}
			Character c = target.Entity as Character;
			if (c == null || !base.VisibleHulls.Contains(c.CurrentHull))
			{
				Item i = target.Entity as Item;
				if (i == null || !base.VisibleHulls.Contains(i.CurrentHull))
				{
					if (dist > 0f)
					{
						return EnemyAIController.<CanPerceive>g__IsInRange|226_0(dist, target.SoundRange, this.Hearing / 2f);
					}
					if (distSquared < 0f)
					{
						distSquared = Vector2.DistanceSquared(this.Character.WorldPosition, target.WorldPosition);
					}
					return EnemyAIController.<CanPerceive>g__IsInRangeSqr|226_1(distSquared, target.SoundRange, this.Hearing / 2f);
				}
			}
			return insideSightRange || insideSoundRange;
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00012A80 File Offset: 0x00010C80
		public void ReevaluateAttacks()
		{
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			this.canAttackWalls = (latchOntoAI != null && latchOntoAI.AttachToSub);
			this.canAttackDoors = false;
			this.canAttackCharacters = false;
			this.canAttackItems = false;
			foreach (Limb limb in this.Character.AnimController.Limbs)
			{
				if (!limb.IsSevered && !limb.Disabled && limb.attack != null)
				{
					if (!this.canAttackWalls)
					{
						this.canAttackWalls = (limb.attack.StructureDamage > 0f || (limb.attack.Ranged && limb.attack.IsValidTarget(AttackTarget.Structure)));
					}
					if (!this.canAttackDoors)
					{
						this.canAttackDoors = ((limb.attack.ItemDamage > 0f || limb.attack.Ranged) && limb.attack.IsValidTarget(AttackTarget.Structure));
					}
					if (!this.canAttackItems)
					{
						this.canAttackItems = (this.canAttackDoors || ((limb.attack.ItemDamage > 0f || limb.attack.Ranged) && limb.attack.IsValidTarget(AttackTarget.Structure | AttackTarget.Item)));
					}
					if (!this.canAttackCharacters)
					{
						this.canAttackCharacters = limb.attack.IsValidTarget(AttackTarget.Character);
					}
				}
			}
			if (this.PathSteering != null)
			{
				this.PathSteering.CanBreakDoors = this.canAttackDoors;
			}
		}

		// Token: 0x06000204 RID: 516 RVA: 0x00012BFC File Offset: 0x00010DFC
		private bool IsPositionInsideAllowedZone(Vector2 pos, out Vector2 targetDir)
		{
			targetDir = Vector2.Zero;
			if (Level.Loaded == null)
			{
				return true;
			}
			if (Level.Loaded.LevelData.Biome.IsEndBiome)
			{
				return true;
			}
			if (this.AIParams.AvoidAbyss)
			{
				if (pos.Y < (float)Level.Loaded.AbyssStart)
				{
					targetDir = Vector2.UnitY;
				}
			}
			else if (this.AIParams.StayInAbyss)
			{
				if (pos.Y > (float)Level.Loaded.AbyssStart)
				{
					targetDir = -Vector2.UnitY;
				}
				else if (pos.Y < (float)Level.Loaded.AbyssEnd)
				{
					targetDir = Vector2.UnitY;
				}
			}
			float margin = 30000f;
			if (pos.X < -margin)
			{
				targetDir = Vector2.UnitX;
			}
			else if (pos.X > (float)Level.Loaded.Size.X + margin)
			{
				targetDir = -Vector2.UnitX;
			}
			return targetDir == Vector2.Zero;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00012D0C File Offset: 0x00010F0C
		private void SteerInsideLevel(float deltaTime)
		{
			if (base.SteeringManager is IndoorsSteeringManager)
			{
				return;
			}
			if (Level.Loaded == null)
			{
				return;
			}
			if (this.State == AIState.Attack && this.returnTimer <= 0f)
			{
				return;
			}
			float returnTime = 5f;
			Vector2 targetDir;
			if (!this.IsPositionInsideAllowedZone(base.WorldPosition, out targetDir))
			{
				this.returnDir = targetDir;
				this.returnTimer = returnTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
			if (this.returnTimer > 0f)
			{
				this.returnTimer -= deltaTime;
				base.SteeringManager.Reset();
				base.SteeringManager.SteeringManual(deltaTime, this.returnDir * 10f);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
		}

		// Token: 0x06000206 RID: 518 RVA: 0x00012DD4 File Offset: 0x00010FD4
		public override bool SteerThroughGap(Structure wall, WallSection section, Vector2 targetWorldPos, float deltaTime)
		{
			base.IsTryingToSteerThroughGap = true;
			this.wallTarget = null;
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			if (latchOntoAI != null)
			{
				latchOntoAI.DeattachFromBody(true, 2f);
			}
			this.Character.AnimController.ReleaseStuckLimbs();
			bool success = base.SteerThroughGap(wall, section, targetWorldPos, deltaTime);
			if (success)
			{
				base.SelectedAiTarget = ((this.Character.CurrentHull != null) ? section.gap.AiTarget : wall.AiTarget);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 1f);
			}
			base.IsSteeringThroughGap = success;
			return success;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00012E6C File Offset: 0x0001106C
		public override bool SteerThroughGap(Gap gap, Vector2 targetWorldPos, float deltaTime, float maxDistance = -1f)
		{
			bool success = base.SteerThroughGap(gap, targetWorldPos, deltaTime, maxDistance);
			if (success)
			{
				this.wallTarget = null;
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.DeattachFromBody(true, 2f);
				}
				this.Character.AnimController.ReleaseStuckLimbs();
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 1f);
			}
			base.IsSteeringThroughGap = success;
			return success;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x00012ED5 File Offset: 0x000110D5
		public bool CanPassThroughHole(Structure wall, int sectionIndex)
		{
			return base.CanPassThroughHole(wall, sectionIndex, this.requiredHoleCount);
		}

		// Token: 0x06000209 RID: 521 RVA: 0x00012EE8 File Offset: 0x000110E8
		public override bool Escape(float deltaTime)
		{
			EnemyAIController.<>c__DisplayClass235_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (base.SelectedAiTarget != null && (base.SelectedAiTarget.Entity == null || base.SelectedAiTarget.Entity.Removed))
			{
				this.State = AIState.Idle;
				return false;
			}
			AITargetMemory targetMemory = this.CurrentTargetMemory;
			if (targetMemory != null)
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) is Character)
				{
					targetMemory.Priority += CS$<>8__locals1.deltaTime * this.PriorityFearIncrement;
				}
			}
			bool isSteeringThroughGap = base.UpdateEscape(CS$<>8__locals1.deltaTime, this.canAttackDoors);
			if (!isSteeringThroughGap)
			{
				AITarget selectedAiTarget2 = base.SelectedAiTarget;
				Character targetCharacter = ((selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null) as Character;
				if (targetCharacter != null && targetCharacter.CurrentHull == this.Character.CurrentHull)
				{
					this.<Escape>g__SteerAwayFromTheEnemy|235_0(ref CS$<>8__locals1);
				}
				else if (this.canAttackDoors && base.HasValidPath(true, true, null))
				{
					WayPoint currentNode = this.PathSteering.CurrentPath.CurrentNode;
					Door door2;
					if ((door2 = ((currentNode != null) ? currentNode.ConnectedDoor : null)) == null)
					{
						WayPoint nextNode = this.PathSteering.CurrentPath.NextNode;
						door2 = ((nextNode != null) ? nextNode.ConnectedDoor : null);
					}
					Door door = door2;
					if (door != null && !door.CanBeTraversed && !door.HasAccess(this.Character))
					{
						AITarget doorAiTarget = door.Item.AiTarget;
						if (doorAiTarget != null && (base.SelectedAiTarget != doorAiTarget || this.State != AIState.Attack))
						{
							this.SelectTarget(doorAiTarget, this.CurrentTargetMemory.Priority);
							this.State = AIState.Attack;
							this.AttackLimb = null;
							return false;
						}
					}
				}
			}
			if (base.EscapeTarget == null)
			{
				AITarget selectedAiTarget3 = base.SelectedAiTarget;
				if (((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) is Character)
				{
					this.<Escape>g__SteerAwayFromTheEnemy|235_0(ref CS$<>8__locals1);
				}
				else
				{
					base.SteeringManager.SteeringWander(1f, this.Character.CurrentHull == null);
					if (this.Character.CurrentHull == null)
					{
						base.SteeringManager.SteeringAvoid(CS$<>8__locals1.deltaTime, this.avoidLookAheadDistance, 5f);
					}
				}
			}
			return isSteeringThroughGap;
		}

		// Token: 0x0600020A RID: 522 RVA: 0x000130F4 File Offset: 0x000112F4
		public Limb GetTargetLimb(Limb attackLimb, Character target, LimbType targetLimbType = LimbType.None)
		{
			this.targetLimbs.Clear();
			foreach (Limb limb in target.AnimController.Limbs)
			{
				if (limb.type == targetLimbType || targetLimbType == LimbType.None)
				{
					this.targetLimbs.Add(limb);
				}
			}
			if (this.targetLimbs.None(null))
			{
				this.targetLimbs.AddRange(target.AnimController.Limbs);
			}
			float closestDist = float.MaxValue;
			Limb targetLimb = null;
			foreach (Limb limb2 in this.targetLimbs)
			{
				if (!limb2.IsSevered && !limb2.Hidden)
				{
					float dist = Vector2.DistanceSquared(limb2.WorldPosition, attackLimb.WorldPosition) / Math.Max(limb2.AttackPriority, 0.1f);
					if (dist < closestDist)
					{
						closestDist = dist;
						targetLimb = limb2;
					}
				}
			}
			return targetLimb;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x000131F8 File Offset: 0x000113F8
		private static Character GetOwner(Item item)
		{
			Pickable pickable = item.GetComponent<Pickable>();
			if (pickable != null)
			{
				Character character;
				if ((character = pickable.Picker) == null)
				{
					Inventory inventory = item.FindParentInventory((Inventory i) => i.Owner is Character);
					character = (((inventory != null) ? inventory.Owner : null) as Character);
				}
				Character owner = character;
				if (owner != null)
				{
					AITarget target = owner.AiTarget;
					if (((target != null) ? target.Entity : null) != null && !target.Entity.Removed)
					{
						return owner;
					}
				}
			}
			return null;
		}

		// Token: 0x0600020C RID: 524 RVA: 0x00013278 File Offset: 0x00011478
		[CompilerGenerated]
		private void <SetUnattackableSubmarines>g__AddSubs|109_0(Submarine sub, ref EnemyAIController.<>c__DisplayClass109_0 A_2)
		{
			this.unattackableSubmarines.Add(sub);
			if (A_2.includeConnectedSubs)
			{
				foreach (Submarine connectedSub in sub.DockedTo)
				{
					this.unattackableSubmarines.Add(connectedSub);
				}
			}
		}

		// Token: 0x0600020D RID: 525 RVA: 0x000132E0 File Offset: 0x000114E0
		[CompilerGenerated]
		private void <.ctor>g__LoadSubElement|124_0(XElement subElement)
		{
			string a = subElement.Name.ToString().ToLowerInvariant();
			if (a == "latchonto")
			{
				this.LatchOntoAI = new LatchOntoAI(subElement, this);
				return;
			}
			if (a == "swarm" || a == "swarmbehavior")
			{
				this.SwarmBehavior = new SwarmBehavior(subElement, this);
				return;
			}
			if (!(a == "petbehavior"))
			{
				return;
			}
			this.PetBehavior = new PetBehavior(subElement, this);
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0001335C File Offset: 0x0001155C
		[CompilerGenerated]
		private bool <Update>g__IsCloseEnoughToTargetSub|137_0(float threshold)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			Submarine submarine;
			if (selectedAiTarget == null)
			{
				submarine = null;
			}
			else
			{
				Entity entity = selectedAiTarget.Entity;
				submarine = ((entity != null) ? entity.Submarine : null);
			}
			Submarine sub = submarine;
			return sub != null && sub != null && Vector2.DistanceSquared(this.Character.WorldPosition, sub.WorldPosition) < MathUtils.Pow((float)(Math.Max(sub.Borders.Size.X, sub.Borders.Size.Y) / 2) + threshold, 2f);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x00013424 File Offset: 0x00011624
		[CompilerGenerated]
		private bool <UpdatePatrol>g__PatrolNodeFilter|152_0(PathNode n)
		{
			return (this.AIParams.PatrolFlooded && (this.Character.CurrentHull == null || n.Waypoint.CurrentHull == null || n.Waypoint.CurrentHull.WaterPercentage >= 80f)) || (this.AIParams.PatrolDry && this.Character.CurrentHull != null && n.Waypoint.CurrentHull != null && n.Waypoint.CurrentHull.WaterPercentage <= 50f);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x000134B4 File Offset: 0x000116B4
		[CompilerGenerated]
		internal static bool <IsSameTarget>g__IsItemInCharacterInventory|158_0(AITarget potentialItem, AITarget potentialCharacter)
		{
			Item item = ((potentialItem != null) ? potentialItem.Entity : null) as Item;
			if (item != null)
			{
				Character character = ((potentialCharacter != null) ? potentialCharacter.Entity : null) as Character;
				if (character != null)
				{
					Inventory parentInventory = item.ParentInventory;
					return ((parentInventory != null) ? parentInventory.Owner : null) == character;
				}
			}
			return false;
		}

		// Token: 0x06000213 RID: 531 RVA: 0x00013504 File Offset: 0x00011704
		[CompilerGenerated]
		private float <GetAttackLimb>g__CalculatePriority|164_0(Limb limb, Vector2 attackPos)
		{
			float prio = 1f + limb.attack.Priority;
			if (this.Character.AnimController.SimplePhysicsEnabled)
			{
				return prio;
			}
			float distance = Vector2.Distance(limb.WorldPosition, attackPos);
			float maxDistance = Math.Max(limb.attack.Range * 3f, 1000f);
			if (distance > maxDistance)
			{
				return 0f;
			}
			float distanceFactor;
			if (limb.attack.Ranged)
			{
				float min = 100f;
				if (distance < min)
				{
					float t = MathUtils.InverseLerp(0f, min, distance);
					distanceFactor = MathHelper.Lerp(0.01f, 1f, t * t);
				}
				else
				{
					distanceFactor = MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(min, maxDistance, distance));
				}
			}
			else
			{
				if (distance <= limb.attack.Range)
				{
					if (!this.Character.InWater)
					{
						float verticalDistance = Math.Abs(limb.WorldPosition.Y - attackPos.Y);
						if (verticalDistance > limb.attack.DamageRange)
						{
							return 0f;
						}
					}
					return prio * 10f;
				}
				float min2 = limb.attack.Range;
				distanceFactor = MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(min2, maxDistance, distance));
			}
			return prio * distanceFactor;
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00013644 File Offset: 0x00011844
		[CompilerGenerated]
		internal static InvSlotType <GetEquippedItem>g__GetInvSlotForLimb|166_0(ref EnemyAIController.<>c__DisplayClass166_0 A_0)
		{
			LimbType type = A_0.limb.type;
			InvSlotType result;
			if (type != LimbType.LeftHand)
			{
				if (type != LimbType.RightHand)
				{
					if (type != LimbType.Head)
					{
						result = InvSlotType.None;
					}
					else
					{
						result = InvSlotType.Head;
					}
				}
				else
				{
					result = InvSlotType.RightHand;
				}
			}
			else
			{
				result = InvSlotType.LeftHand;
			}
			return result;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x00013698 File Offset: 0x00011898
		[CompilerGenerated]
		private void <UpdateWallTarget>g__DoRayCast|195_0(Vector2 rayStart, Vector2 rayEnd, ref EnemyAIController.<>c__DisplayClass195_0 A_3)
		{
			Body hitTarget = Submarine.CheckVisibility(rayStart, rayEnd, false, true, this.CanEnterSubmarine > CanEnterSubmarine.False, this.CanEnterSubmarine > CanEnterSubmarine.False, true, null);
			if (hitTarget != null && this.<UpdateWallTarget>g__IsValid|195_2(hitTarget, out A_3.wall, ref A_3))
			{
				int sectionIndex = A_3.wall.FindSectionIndex(ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition), false, false);
				if (sectionIndex >= 0)
				{
					this.wallHits.Add(new ValueTuple<Body, int, Vector2>(hitTarget, sectionIndex, this.<UpdateWallTarget>g__GetSectionPosition|195_1(A_3.wall, sectionIndex, ref A_3)));
				}
			}
		}

		// Token: 0x06000217 RID: 535 RVA: 0x00013714 File Offset: 0x00011914
		[CompilerGenerated]
		private Vector2 <UpdateWallTarget>g__GetSectionPosition|195_1(Structure wall, int sectionIndex, ref EnemyAIController.<>c__DisplayClass195_0 A_3)
		{
			float sectionDamage = wall.SectionDamage(sectionIndex);
			for (int i = sectionIndex - 2; i <= sectionIndex + 2; i++)
			{
				if (wall.SectionBodyDisabled(i))
				{
					if (this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.False && base.CanPassThroughHole(wall, i, A_3.requiredHoleCount))
					{
						sectionIndex = i;
						break;
					}
				}
				else if (wall.SectionDamage(i) > sectionDamage)
				{
					sectionIndex = i;
				}
			}
			return wall.SectionPosition(sectionIndex, false);
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00013780 File Offset: 0x00011980
		[CompilerGenerated]
		private bool <UpdateWallTarget>g__IsValid|195_2(Body hit, out Structure wall, ref EnemyAIController.<>c__DisplayClass195_0 A_3)
		{
			wall = null;
			if (Submarine.LastPickedFraction == 1f)
			{
				return false;
			}
			Structure w = hit.UserData as Structure;
			if (w == null)
			{
				return false;
			}
			if (w.Submarine == null)
			{
				return false;
			}
			if (w.Submarine != base.SelectedAiTarget.Entity.Submarine)
			{
				return false;
			}
			if (this.Character.Submarine == null)
			{
				if (w.Prefab.Tags.Contains("inner"))
				{
					if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.False)
					{
						return false;
					}
				}
				else if (!this.AIParams.TargetOuterWalls)
				{
					return false;
				}
			}
			wall = w;
			return true;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0001386A File Offset: 0x00011A6A
		[CompilerGenerated]
		internal static bool <CanPerceive>g__IsInRange|226_0(float dist, float range, float perception)
		{
			return dist <= range * perception;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x00013875 File Offset: 0x00011A75
		[CompilerGenerated]
		internal static bool <CanPerceive>g__IsInRangeSqr|226_1(float distSquared, float range, float perception)
		{
			return distSquared <= MathUtils.Pow2(range * perception);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x00013888 File Offset: 0x00011A88
		[CompilerGenerated]
		private void <Escape>g__SteerAwayFromTheEnemy|235_0(ref EnemyAIController.<>c__DisplayClass235_0 A_1)
		{
			if (base.SelectedAiTarget == null)
			{
				return;
			}
			Vector2 escapeDir = Vector2.Normalize(base.WorldPosition - base.SelectedAiTarget.WorldPosition);
			if (!MathUtils.IsValid(escapeDir))
			{
				escapeDir = Vector2.UnitY;
			}
			if (this.Character.CurrentHull != null && !this.Character.AnimController.InWater)
			{
				escapeDir = new Vector2((float)Math.Sign(escapeDir.X), 0f);
			}
			base.SteeringManager.Reset();
			base.SteeringManager.SteeringManual(A_1.deltaTime, escapeDir);
		}

		// Token: 0x04000128 RID: 296
		public static bool DisableEnemyAI;

		// Token: 0x04000129 RID: 297
		public static EnemyTargetingRestrictions TargetingRestrictions;

		// Token: 0x0400012A RID: 298
		private EnemyTargetingRestrictions previousTargetingRestrictions;

		// Token: 0x0400012B RID: 299
		private AIState _state;

		// Token: 0x0400012D RID: 301
		public bool TargetOutposts;

		// Token: 0x0400012E RID: 302
		private readonly float updateTargetsInterval = 1f;

		// Token: 0x0400012F RID: 303
		private readonly float updateMemoriesInverval = 1f;

		// Token: 0x04000130 RID: 304
		private readonly float attackLimbSelectionInterval = 3f;

		// Token: 0x04000131 RID: 305
		private const float minPriority = 10f;

		// Token: 0x04000132 RID: 306
		private SteeringManager outsideSteering;

		// Token: 0x04000133 RID: 307
		private SteeringManager insideSteering;

		// Token: 0x04000134 RID: 308
		private float updateTargetsTimer;

		// Token: 0x04000135 RID: 309
		private float updateMemoriesTimer;

		// Token: 0x04000136 RID: 310
		private float attackLimbSelectionTimer;

		// Token: 0x04000137 RID: 311
		private Limb _attackLimb;

		// Token: 0x04000138 RID: 312
		private Limb _previousAttackLimb;

		// Token: 0x04000139 RID: 313
		private double lastAttackUpdateTime;

		// Token: 0x0400013A RID: 314
		private Attack _activeAttack;

		// Token: 0x0400013B RID: 315
		private AITargetMemory currentTargetMemory;

		// Token: 0x0400013C RID: 316
		private float targetValue;

		// Token: 0x0400013D RID: 317
		private CharacterParams.TargetParams currentTargetingParams;

		// Token: 0x0400013E RID: 318
		private Dictionary<AITarget, AITargetMemory> targetMemories;

		// Token: 0x0400013F RID: 319
		private readonly int requiredHoleCount;

		// Token: 0x04000140 RID: 320
		private bool canAttackWalls;

		// Token: 0x04000141 RID: 321
		private bool canAttackDoors;

		// Token: 0x04000142 RID: 322
		private bool canAttackItems;

		// Token: 0x04000143 RID: 323
		private bool canAttackCharacters;

		// Token: 0x04000144 RID: 324
		private readonly float priorityFearIncreasement = 2f;

		// Token: 0x04000145 RID: 325
		private readonly float memoryFadeTime = 0.5f;

		// Token: 0x04000146 RID: 326
		private float avoidTimer;

		// Token: 0x04000147 RID: 327
		private float observeTimer;

		// Token: 0x04000148 RID: 328
		private float sweepTimer;

		// Token: 0x04000149 RID: 329
		private float circleRotation;

		// Token: 0x0400014A RID: 330
		private float circleDir;

		// Token: 0x0400014B RID: 331
		private bool inverseDir;

		// Token: 0x0400014C RID: 332
		private bool breakCircling;

		// Token: 0x0400014D RID: 333
		private float circleRotationSpeed;

		// Token: 0x0400014E RID: 334
		private Vector2 circleOffset;

		// Token: 0x0400014F RID: 335
		private float circleFallbackDistance;

		// Token: 0x04000150 RID: 336
		private float strikeTimer;

		// Token: 0x04000151 RID: 337
		private float aggressionIntensity;

		// Token: 0x04000152 RID: 338
		private CirclePhase CirclePhase;

		// Token: 0x04000153 RID: 339
		private float currentAttackIntensity;

		// Token: 0x04000154 RID: 340
		private float playDeadTimer;

		// Token: 0x04000155 RID: 341
		private const float PlayDeadCoolDown = 60f;

		// Token: 0x04000156 RID: 342
		private CoroutineHandle disableTailCoroutine;

		// Token: 0x04000157 RID: 343
		private readonly List<Body> myBodies;

		// Token: 0x0400015B RID: 347
		private readonly HashSet<Submarine> unattackableSubmarines = new HashSet<Submarine>();

		// Token: 0x0400015C RID: 348
		private bool reverse;

		// Token: 0x0400015D RID: 349
		private readonly float maxSteeringBuffer = 5000f;

		// Token: 0x0400015E RID: 350
		private readonly float minSteeringBuffer = 500f;

		// Token: 0x0400015F RID: 351
		private readonly float steeringBufferIncreaseSpeed = 100f;

		// Token: 0x04000160 RID: 352
		private float steeringBuffer;

		// Token: 0x04000161 RID: 353
		private CharacterParams.AIParams _aiParams;

		// Token: 0x04000162 RID: 354
		private readonly List<Identifier> _targetingTags = new List<Identifier>();

		// Token: 0x04000163 RID: 355
		private float movementMargin;

		// Token: 0x04000164 RID: 356
		private const float MaxDroppingInterval = 5f;

		// Token: 0x04000165 RID: 357
		private double lastDroppingTime;

		// Token: 0x04000166 RID: 358
		private const float MaxDroppingTime = 1f;

		// Token: 0x04000167 RID: 359
		private float droppingTimer;

		// Token: 0x04000168 RID: 360
		private readonly List<Hull> targetHulls = new List<Hull>();

		// Token: 0x04000169 RID: 361
		private readonly List<float> hullWeights = new List<float>();

		// Token: 0x0400016A RID: 362
		private Hull patrolTarget;

		// Token: 0x0400016B RID: 363
		private float newPatrolTargetTimer;

		// Token: 0x0400016C RID: 364
		private float patrolTimerMargin;

		// Token: 0x0400016D RID: 365
		private readonly float newPatrolTargetIntervalMin = 5f;

		// Token: 0x0400016E RID: 366
		private readonly float newPatrolTargetIntervalMax = 30f;

		// Token: 0x0400016F RID: 367
		private bool searchingNewHull;

		// Token: 0x04000170 RID: 368
		private Vector2 attackWorldPos;

		// Token: 0x04000171 RID: 369
		private Vector2 attackSimPos;

		// Token: 0x04000172 RID: 370
		private float reachTimer;

		// Token: 0x04000173 RID: 371
		private const float reachTimeOut = 10f;

		// Token: 0x04000174 RID: 372
		private readonly List<Limb> attackLimbs = new List<Limb>();

		// Token: 0x04000175 RID: 373
		private readonly List<float> weights = new List<float>();

		// Token: 0x04000176 RID: 374
		private const float VisibilityCheckStep = 0.2f;

		// Token: 0x04000177 RID: 375
		private double lastVisibilityCheckTime;

		// Token: 0x04000178 RID: 376
		private bool canSeeTarget;

		// Token: 0x04000179 RID: 377
		private float aimTimer;

		// Token: 0x0400017A RID: 378
		private float sinTime;

		// Token: 0x0400017B RID: 379
		private readonly float blockCheckInterval = 0.1f;

		// Token: 0x0400017C RID: 380
		private float blockCheckTimer;

		// Token: 0x0400017D RID: 381
		private bool isBlocked;

		// Token: 0x0400017E RID: 382
		private Vector2? attackVector;

		// Token: 0x0400017F RID: 383
		private EnemyAIController.WallTarget wallTarget;

		// Token: 0x04000180 RID: 384
		private readonly List<ValueTuple<Body, int, Vector2>> wallHits = new List<ValueTuple<Body, int, Vector2>>(3);

		// Token: 0x04000181 RID: 385
		private readonly List<AITarget> removals = new List<AITarget>();

		// Token: 0x04000182 RID: 386
		private readonly float targetIgnoreTime = 10f;

		// Token: 0x04000183 RID: 387
		private float targetIgnoreTimer;

		// Token: 0x04000184 RID: 388
		private readonly HashSet<AITarget> ignoredTargets = new HashSet<AITarget>();

		// Token: 0x04000185 RID: 389
		private readonly float stateResetCooldown = 10f;

		// Token: 0x04000186 RID: 390
		private float stateResetTimer;

		// Token: 0x04000187 RID: 391
		private bool isStateChanged;

		// Token: 0x04000188 RID: 392
		private readonly Dictionary<StatusEffect.AITrigger, CharacterParams.TargetParams> activeTriggers = new Dictionary<StatusEffect.AITrigger, CharacterParams.TargetParams>();

		// Token: 0x04000189 RID: 393
		private readonly HashSet<StatusEffect.AITrigger> inactiveTriggers = new HashSet<StatusEffect.AITrigger>();

		// Token: 0x0400018A RID: 394
		private readonly Dictionary<Identifier, IEnumerable<CharacterParams.TargetParams>> modifiedParams = new Dictionary<Identifier, IEnumerable<CharacterParams.TargetParams>>();

		// Token: 0x0400018B RID: 395
		private readonly Dictionary<Identifier, CharacterParams.TargetParams> tempParams = new Dictionary<Identifier, CharacterParams.TargetParams>();

		// Token: 0x0400018C RID: 396
		private readonly List<CharacterParams.TargetParams> tempParamsList = new List<CharacterParams.TargetParams>();

		// Token: 0x0400018D RID: 397
		private Vector2 returnDir;

		// Token: 0x0400018E RID: 398
		private float returnTimer;

		// Token: 0x0400018F RID: 399
		private readonly List<Limb> targetLimbs = new List<Limb>();

		// Token: 0x02000640 RID: 1600
		private class WallTarget
		{
			// Token: 0x0600652A RID: 25898 RVA: 0x00344109 File Offset: 0x00342309
			public WallTarget(Vector2 position, Structure structure = null, int sectionIndex = -1)
			{
				this.Position = position;
				this.Structure = structure;
				this.SectionIndex = sectionIndex;
			}

			// Token: 0x04003642 RID: 13890
			public Vector2 Position;

			// Token: 0x04003643 RID: 13891
			public Structure Structure;

			// Token: 0x04003644 RID: 13892
			public int SectionIndex;
		}
	}
}
