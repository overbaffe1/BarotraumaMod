using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000060 RID: 96
	internal class LatchOntoAI
	{
		// Token: 0x17000390 RID: 912
		// (get) Token: 0x06000D9F RID: 3487 RVA: 0x00086E5C File Offset: 0x0008505C
		// (set) Token: 0x06000DA0 RID: 3488 RVA: 0x00086E64 File Offset: 0x00085064
		public bool AttachToSub { get; private set; }

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x06000DA1 RID: 3489 RVA: 0x00086E6D File Offset: 0x0008506D
		// (set) Token: 0x06000DA2 RID: 3490 RVA: 0x00086E75 File Offset: 0x00085075
		public bool AttachToWalls { get; private set; }

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06000DA3 RID: 3491 RVA: 0x00086E7E File Offset: 0x0008507E
		// (set) Token: 0x06000DA4 RID: 3492 RVA: 0x00086E86 File Offset: 0x00085086
		public bool AttachToCharacters { get; private set; }

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06000DA5 RID: 3493 RVA: 0x00086E8F File Offset: 0x0008508F
		// (set) Token: 0x06000DA6 RID: 3494 RVA: 0x00086E97 File Offset: 0x00085097
		public Submarine TargetSubmarine { get; private set; }

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x00086EA0 File Offset: 0x000850A0
		// (set) Token: 0x06000DA8 RID: 3496 RVA: 0x00086EA8 File Offset: 0x000850A8
		public Structure TargetWall { get; private set; }

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06000DA9 RID: 3497 RVA: 0x00086EB1 File Offset: 0x000850B1
		// (set) Token: 0x06000DAA RID: 3498 RVA: 0x00086EB9 File Offset: 0x000850B9
		public Character TargetCharacter { get; private set; }

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06000DAB RID: 3499 RVA: 0x00086EC2 File Offset: 0x000850C2
		public List<Joint> AttachJoints { get; } = new List<Joint>();

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06000DAC RID: 3500 RVA: 0x00086ECA File Offset: 0x000850CA
		// (set) Token: 0x06000DAD RID: 3501 RVA: 0x00086ED2 File Offset: 0x000850D2
		public Vector2? AttachPos { get; private set; }

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x06000DAE RID: 3502 RVA: 0x00086EDB File Offset: 0x000850DB
		public bool IsAttached
		{
			get
			{
				return this.AttachJoints.Count > 0;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x06000DAF RID: 3503 RVA: 0x00086EEB File Offset: 0x000850EB
		public bool IsAttachedToSub
		{
			get
			{
				return this.IsAttached && this.TargetSubmarine != null && this.TargetCharacter == null;
			}
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x00086F08 File Offset: 0x00085108
		public LatchOntoAI(XElement element, EnemyAIController enemyAI)
		{
			this.AttachToWalls = element.GetAttributeBool("AttachToWalls", false);
			this.AttachToSub = element.GetAttributeBool("AttachToSub", false);
			this.AttachToCharacters = element.GetAttributeBool("AttachToCharacters", false);
			this.minDeattachSpeed = element.GetAttributeFloat("minDeattachSpeed", 5f);
			this.maxDeattachSpeed = Math.Max(this.minDeattachSpeed, element.GetAttributeFloat("maxDeattachSpeed", 8f));
			this.maxAttachDuration = element.GetAttributeFloat("maxAttachDuration", -1f);
			this.coolDown = element.GetAttributeFloat("coolDown", 2f);
			this.damageOnDetach = element.GetAttributeFloat("damageOnDetach", 0f);
			this.detachStun = element.GetAttributeFloat("detachStun", 0f);
			this.localAttachPos = ConvertUnits.ToSimUnits(element.GetAttributeVector2("localAttachPos", Vector2.Zero));
			this.attachLimbRotation = MathHelper.ToRadians(element.GetAttributeFloat("attachLimbRotation", 0f));
			this.weld = element.GetAttributeBool("weld", true);
			this.freezeWhenLatched = element.GetAttributeBool("freezeWhenLatched", false);
			string limbString = element.GetAttributeString("attachlimb", null);
			this.attachLimb = enemyAI.Character.AnimController.Limbs.FirstOrDefault((Limb l) => string.Equals(l.Name, limbString, StringComparison.OrdinalIgnoreCase));
			LimbType attachLimbType;
			if (this.attachLimb == null && Enum.TryParse<LimbType>(limbString, out attachLimbType))
			{
				this.attachLimb = enemyAI.Character.AnimController.GetLimb(attachLimbType, true, false, false);
			}
			if (this.attachLimb == null)
			{
				this.attachLimb = enemyAI.Character.AnimController.MainLimb;
			}
			this.character = enemyAI.Character;
			Character character = enemyAI.Character;
			character.OnDeath = (Character.OnDeathHandler)Delegate.Combine(character.OnDeath, new Character.OnDeathHandler(this.OnCharacterDeath));
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x00087108 File Offset: 0x00085308
		public void SetAttachTarget(Structure wall, Vector2 attachPos, Vector2 attachSurfaceNormal)
		{
			if (!this.AttachToSub)
			{
				return;
			}
			if (wall == null)
			{
				return;
			}
			Submarine sub = wall.Submarine;
			if (sub == null)
			{
				return;
			}
			this.Reset();
			this.TargetWall = wall;
			this.TargetSubmarine = sub;
			this.targetBody = this.TargetSubmarine.PhysicsBody.FarseerBody;
			this.attachSurfaceNormal = attachSurfaceNormal;
			this._attachPos = attachPos;
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x00087168 File Offset: 0x00085368
		public void SetAttachTarget(Character target)
		{
			if (!this.AttachToCharacters)
			{
				return;
			}
			if (target.Submarine != this.character.Submarine)
			{
				return;
			}
			this.Reset();
			this.TargetCharacter = target;
			this.targetBody = target.AnimController.MainLimb.body.FarseerBody;
			this.attachSurfaceNormal = Vector2.Normalize(this.character.WorldPosition - target.WorldPosition);
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x000871DC File Offset: 0x000853DC
		public void SetAttachTarget(VoronoiCell levelWall)
		{
			if (!this.AttachToWalls)
			{
				return;
			}
			this.Reset();
			foreach (GraphEdge edge in levelWall.Edges)
			{
				Vector2 intersection;
				if (MathUtils.GetLineSegmentIntersection(edge.Point1, edge.Point2, this.character.WorldPosition, levelWall.Center, out intersection))
				{
					this.attachSurfaceNormal = edge.GetNormal(levelWall);
					this.targetBody = levelWall.Body;
					this._attachPos = ConvertUnits.ToSimUnits(intersection);
					break;
				}
			}
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x00087284 File Offset: 0x00085484
		public void Update(EnemyAIController enemyAI, float deltaTime)
		{
			if ((this.TargetCharacter != null && this.character.Submarine != this.TargetCharacter.Submarine) || (this.character.Submarine != null && this.TargetSubmarine != null && this.TargetCharacter == null))
			{
				this.DeattachFromBody(true, 0f);
				return;
			}
			if (this.IsAttached)
			{
				this.latchedDuration += deltaTime;
				if (this.freezeWhenLatched)
				{
					Body body = this.targetBody;
					if (body != null && body.BodyType == BodyType.Static && this.latchedDuration > 5f)
					{
						foreach (Limb limb in this.character.AnimController.Limbs)
						{
							limb.body.LinearVelocity = Vector2.Zero;
							limb.body.AngularVelocity = 0f;
						}
					}
				}
				if (Math.Sign(this.attachLimb.Dir) != Math.Sign(this.jointDir))
				{
					Joint attachJoint = this.AttachJoints[0];
					WeldJoint weldJoint = attachJoint as WeldJoint;
					if (weldJoint != null)
					{
						weldJoint.LocalAnchorA = new Vector2(-weldJoint.LocalAnchorA.X, weldJoint.LocalAnchorA.Y);
						weldJoint.ReferenceAngle = -weldJoint.ReferenceAngle;
					}
					else
					{
						RevoluteJoint revoluteJoint = attachJoint as RevoluteJoint;
						if (revoluteJoint != null)
						{
							revoluteJoint.LocalAnchorA = new Vector2(-revoluteJoint.LocalAnchorA.X, revoluteJoint.LocalAnchorA.Y);
							revoluteJoint.ReferenceAngle = -revoluteJoint.ReferenceAngle;
						}
					}
					this.jointDir = this.attachLimb.Dir;
				}
				for (int i = 0; i < this.AttachJoints.Count; i++)
				{
					if (Vector2.DistanceSquared(this.AttachJoints[i].WorldAnchorB, this.AttachJoints[i].BodyA.Position) > 100f)
					{
						this.DeattachFromBody(true, 0f);
						return;
					}
				}
				if (this.TargetCharacter != null)
				{
					Limb attackLimb = enemyAI.AttackLimb;
					if (((attackLimb != null) ? attackLimb.attack : null) == null)
					{
						this.DeattachFromBody(true, 1f);
					}
					else
					{
						float range = enemyAI.AttackLimb.attack.DamageRange * 2f;
						if (Vector2.DistanceSquared(this.TargetCharacter.WorldPosition, enemyAI.AttackLimb.WorldPosition) > range * range)
						{
							this.DeattachFromBody(true, 1f);
						}
						else
						{
							this.TargetCharacter.Latchers.Add(this);
						}
					}
				}
			}
			if (this.attachCooldown > 0f)
			{
				this.attachCooldown -= deltaTime;
			}
			if (this.deattachCheckTimer > 0f)
			{
				this.deattachCheckTimer -= deltaTime;
			}
			if (this.TargetCharacter != null)
			{
				this._attachPos = this.character.SimPosition;
			}
			Vector2 transformedAttachPos = this._attachPos;
			if (this.character.Submarine == null && this.TargetSubmarine != null)
			{
				transformedAttachPos += ConvertUnits.ToSimUnits(this.TargetSubmarine.Position);
			}
			if (transformedAttachPos != Vector2.Zero)
			{
				this.AttachPos = new Vector2?(transformedAttachPos);
			}
			AIState state = enemyAI.State;
			if (state != AIState.Idle)
			{
				if (state != AIState.Attack && state != AIState.Aggressive)
				{
					this.DeattachFromBody(true, 0f);
				}
				else if (!enemyAI.IsSteeringThroughGap && !(this._attachPos == Vector2.Zero) && (this.AttachToSub || this.AttachToCharacters) && enemyAI.AttackLimb != null && this.targetBody != null && (!this.IsAttached || this.AttachJoints[0].BodyB != this.targetBody))
				{
					Character targetCharacter = this.TargetCharacter;
					Vector2 referencePos = (targetCharacter != null) ? targetCharacter.WorldPosition : ConvertUnits.ToDisplayUnits(transformedAttachPos);
					if (Vector2.DistanceSquared(referencePos, enemyAI.AttackLimb.WorldPosition) < enemyAI.AttackLimb.attack.DamageRange * enemyAI.AttackLimb.attack.DamageRange)
					{
						this.AttachToBody(transformedAttachPos, null, null);
					}
				}
			}
			else
			{
				if (this.AttachToWalls && this.character.Submarine == null && Level.Loaded != null)
				{
					if (!this.IsAttached)
					{
						this.raycastTimer -= deltaTime;
						if (this.raycastTimer < 0f)
						{
							this._attachPos = Vector2.Zero;
							List<VoronoiCell> cells = Level.Loaded.GetCells(this.character.WorldPosition, 1);
							if (cells.Count > 0)
							{
								float closestDist = 40000f;
								foreach (VoronoiCell cell in cells)
								{
									foreach (GraphEdge edge in cell.Edges)
									{
										Vector2 intersection;
										if (MathUtils.GetLineSegmentIntersection(edge.Point1, edge.Point2, this.character.WorldPosition, cell.Center, out intersection))
										{
											Vector2 potentialAttachPos = ConvertUnits.ToSimUnits(intersection);
											float distSqr = Vector2.DistanceSquared(this.character.SimPosition, potentialAttachPos);
											if (distSqr < closestDist)
											{
												this.attachSurfaceNormal = edge.GetNormal(cell);
												this.targetBody = cell.Body;
												this._attachPos = potentialAttachPos;
												closestDist = distSqr;
												break;
											}
											break;
										}
									}
								}
							}
							this.raycastTimer = 5f;
						}
					}
				}
				else
				{
					this._attachPos = Vector2.Zero;
				}
				if (this._attachPos == Vector2.Zero || this.targetBody == null)
				{
					this.DeattachFromBody(false, 0f);
				}
				else if (this.attachCooldown <= 0f)
				{
					float squaredDistance = Vector2.DistanceSquared(this.character.SimPosition, this._attachPos);
					float targetDistance = Math.Max(Math.Max(this.character.AnimController.Collider.Radius, this.character.AnimController.Collider.Width), this.character.AnimController.Collider.Height) * 1.2f;
					if (squaredDistance < targetDistance * targetDistance)
					{
						this.AttachToBody(this._attachPos, null, null);
						enemyAI.SteeringManager.Reset();
					}
					else
					{
						this.DeattachFromBody(false, 0f);
						enemyAI.SteeringManager.SteeringAvoid(deltaTime, 1f, 0.1f);
						enemyAI.SteeringManager.SteeringSeek(this._attachPos, 1f);
					}
				}
				else if (this.IsAttached)
				{
					enemyAI.SteeringManager.Reset();
				}
			}
			if (this.IsAttached && this.targetBody != null && this.deattachCheckTimer <= 0f)
			{
				this.attachCooldown = this.coolDown;
				bool deattach = false;
				if (this.maxAttachDuration > 0f)
				{
					deattach = true;
				}
				if (!deattach && this.TargetWall != null && this.TargetSubmarine != null)
				{
					int targetSection = this.TargetWall.FindSectionIndex(this.attachLimb.WorldPosition, true, true);
					if (enemyAI.CanPassThroughHole(this.TargetWall, targetSection))
					{
						deattach = true;
					}
					if (!deattach)
					{
						float velocity = (this.TargetSubmarine.Velocity == Vector2.Zero) ? 0f : this.TargetSubmarine.Velocity.Length();
						deattach = (velocity > this.maxDeattachSpeed);
						if (!deattach && velocity > this.minDeattachSpeed)
						{
							float velocityFactor = (this.maxDeattachSpeed - this.minDeattachSpeed <= 0f) ? ((float)Math.Sign(Math.Abs(velocity) - this.minDeattachSpeed)) : ((Math.Abs(velocity) - this.minDeattachSpeed) / (this.maxDeattachSpeed - this.minDeattachSpeed));
							if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < velocityFactor)
							{
								deattach = true;
								this.character.AddDamage(this.character.WorldPosition, new List<Affliction>
								{
									AfflictionPrefab.InternalDamage.Instantiate(this.damageOnDetach, null)
								}, this.detachStun, true, null, null, 1f);
								this.attachCooldown = Math.Max(this.detachStun * 2f, this.coolDown);
							}
						}
					}
					this.deattachCheckTimer = 5f;
				}
				if (deattach)
				{
					this.DeattachFromBody(true, 0f);
				}
			}
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x00087B38 File Offset: 0x00085D38
		public void AttachToBody(Vector2 attachPos, Vector2? forceAttachSurfaceNormal = null, Vector2? forceColliderSimPosition = null)
		{
			if (this.attachLimb == null)
			{
				return;
			}
			if (this.targetBody == null)
			{
				return;
			}
			if (this.attachCooldown > 0f)
			{
				return;
			}
			PhysicsBody collider = this.character.AnimController.Collider;
			if (this.AttachJoints.Count > 0)
			{
				if (this.AttachJoints[0].BodyB == this.targetBody)
				{
					return;
				}
				this.DeattachFromBody(false, 0f);
			}
			this.jointDir = this.attachLimb.Dir;
			if (forceAttachSurfaceNormal != null)
			{
				this.attachSurfaceNormal = forceAttachSurfaceNormal.Value;
			}
			if (forceColliderSimPosition != null)
			{
				this.character.TeleportTo(ConvertUnits.ToDisplayUnits(forceColliderSimPosition.Value));
			}
			Vector2 transformedLocalAttachPos = this.localAttachPos * this.attachLimb.Scale * this.attachLimb.Params.Ragdoll.LimbScale;
			if (this.jointDir < 0f)
			{
				transformedLocalAttachPos.X = -transformedLocalAttachPos.X;
			}
			float angle = MathUtils.VectorToAngle(-this.attachSurfaceNormal) - 1.5707964f + this.attachLimbRotation * this.attachLimb.Dir;
			angle = this.attachLimb.body.WrapAngleToSameNumberOfRevolutions(angle);
			this.attachLimb.body.SetTransform(attachPos + this.attachSurfaceNormal * transformedLocalAttachPos.Length(), angle, true);
			WeldJoint limbJoint = new WeldJoint(this.attachLimb.body.FarseerBody, this.targetBody, transformedLocalAttachPos, this.targetBody.GetLocalPoint(attachPos), false)
			{
				FrequencyHz = 10f,
				DampingRatio = 0.5f,
				KinematicBodyB = true,
				CollideConnected = false
			};
			GameMain.World.Add(limbJoint);
			this.AttachJoints.Add(limbJoint);
			Vector2 colliderFront = collider.GetLocalFront(null);
			if (this.jointDir < 0f)
			{
				colliderFront.X = -colliderFront.X;
			}
			collider.SetTransform(attachPos + this.attachSurfaceNormal * colliderFront.Length(), MathUtils.VectorToAngle(-this.attachSurfaceNormal) - 1.5707964f, true);
			Joint joint;
			if (!this.weld)
			{
				RevoluteJoint revoluteJoint = new RevoluteJoint(collider.FarseerBody, this.targetBody, colliderFront, this.targetBody.GetLocalPoint(attachPos), false);
				revoluteJoint.MotorEnabled = true;
				joint = revoluteJoint;
				revoluteJoint.MaxMotorTorque = 0.25f;
			}
			else
			{
				WeldJoint weldJoint = new WeldJoint(collider.FarseerBody, this.targetBody, colliderFront, this.targetBody.GetLocalPoint(attachPos), false);
				weldJoint.FrequencyHz = 10f;
				weldJoint.DampingRatio = 0.5f;
				weldJoint.KinematicBodyB = true;
				joint = weldJoint;
				weldJoint.CollideConnected = false;
			}
			Joint colliderJoint = joint;
			GameMain.World.Add(colliderJoint);
			this.AttachJoints.Add(colliderJoint);
			Character targetCharacter = this.TargetCharacter;
			if (targetCharacter != null)
			{
				targetCharacter.Latchers.Add(this);
			}
			if (this.maxAttachDuration > 0f)
			{
				this.deattachCheckTimer = this.maxAttachDuration;
			}
			if (this.TargetCharacter != null)
			{
				GameMain.Server.CreateEntityEvent(this.character, new Character.LatchedOntoTargetEventData(this.character, this.TargetCharacter, this.attachSurfaceNormal, attachPos));
				return;
			}
			if (this.TargetWall != null)
			{
				GameMain.Server.CreateEntityEvent(this.character, new Character.LatchedOntoTargetEventData(this.character, this.TargetWall, this.attachSurfaceNormal, attachPos));
				return;
			}
			VoronoiCell cell = this.targetBody.UserData as VoronoiCell;
			if (cell != null)
			{
				GameMain.Server.CreateEntityEvent(this.character, new Character.LatchedOntoTargetEventData(this.character, cell, this.attachSurfaceNormal, attachPos));
			}
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x00087EE4 File Offset: 0x000860E4
		public void DeattachFromBody(bool reset, float cooldown = 0f)
		{
			bool wasAttached = this.IsAttached;
			foreach (Joint joint in this.AttachJoints)
			{
				GameMain.World.Remove(joint);
			}
			this.AttachJoints.Clear();
			if (cooldown > 0f)
			{
				this.attachCooldown = cooldown;
			}
			Character targetCharacter = this.TargetCharacter;
			if (targetCharacter != null)
			{
				targetCharacter.Latchers.Remove(this);
			}
			if (reset)
			{
				this.Reset();
			}
			if (wasAttached)
			{
				GameMain.Server.CreateEntityEvent(this.character, new Character.LatchedOntoTargetEventData());
			}
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x00087F9C File Offset: 0x0008619C
		private void Reset()
		{
			Character targetCharacter = this.TargetCharacter;
			if (targetCharacter != null)
			{
				targetCharacter.Latchers.Remove(this);
			}
			this.TargetCharacter = null;
			this.TargetWall = null;
			this.TargetSubmarine = null;
			this.targetBody = null;
			this.AttachPos = null;
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x00087FEC File Offset: 0x000861EC
		private void OnCharacterDeath(Character character, CauseOfDeath causeOfDeath)
		{
			this.DeattachFromBody(true, 0f);
			character.OnDeath = (Character.OnDeathHandler)Delegate.Remove(character.OnDeath, new Character.OnDeathHandler(this.OnCharacterDeath));
		}

		// Token: 0x04000651 RID: 1617
		private const float RaycastInterval = 5f;

		// Token: 0x04000652 RID: 1618
		private float raycastTimer;

		// Token: 0x04000653 RID: 1619
		private Body targetBody;

		// Token: 0x04000654 RID: 1620
		private Vector2 attachSurfaceNormal;

		// Token: 0x04000655 RID: 1621
		private readonly Character character;

		// Token: 0x0400065C RID: 1628
		private readonly float minDeattachSpeed;

		// Token: 0x0400065D RID: 1629
		private readonly float maxDeattachSpeed;

		// Token: 0x0400065E RID: 1630
		private readonly float maxAttachDuration;

		// Token: 0x0400065F RID: 1631
		private readonly float coolDown;

		// Token: 0x04000660 RID: 1632
		private readonly float damageOnDetach;

		// Token: 0x04000661 RID: 1633
		private readonly float detachStun;

		// Token: 0x04000662 RID: 1634
		private readonly bool weld;

		// Token: 0x04000663 RID: 1635
		private float deattachCheckTimer;

		// Token: 0x04000664 RID: 1636
		private Vector2 _attachPos;

		// Token: 0x04000665 RID: 1637
		private float attachCooldown;

		// Token: 0x04000666 RID: 1638
		private readonly Limb attachLimb;

		// Token: 0x04000667 RID: 1639
		private Vector2 localAttachPos;

		// Token: 0x04000668 RID: 1640
		private readonly float attachLimbRotation;

		// Token: 0x04000669 RID: 1641
		private float jointDir;

		// Token: 0x0400066A RID: 1642
		private float latchedDuration;

		// Token: 0x0400066B RID: 1643
		private readonly bool freezeWhenLatched;
	}
}
