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
	// Token: 0x02000167 RID: 359
	internal class LatchOntoAI
	{
		// Token: 0x17000ACF RID: 2767
		// (get) Token: 0x06002AFD RID: 11005 RVA: 0x001DAB84 File Offset: 0x001D8D84
		// (set) Token: 0x06002AFE RID: 11006 RVA: 0x001DAB8C File Offset: 0x001D8D8C
		public bool AttachToSub { get; private set; }

		// Token: 0x17000AD0 RID: 2768
		// (get) Token: 0x06002AFF RID: 11007 RVA: 0x001DAB95 File Offset: 0x001D8D95
		// (set) Token: 0x06002B00 RID: 11008 RVA: 0x001DAB9D File Offset: 0x001D8D9D
		public bool AttachToWalls { get; private set; }

		// Token: 0x17000AD1 RID: 2769
		// (get) Token: 0x06002B01 RID: 11009 RVA: 0x001DABA6 File Offset: 0x001D8DA6
		// (set) Token: 0x06002B02 RID: 11010 RVA: 0x001DABAE File Offset: 0x001D8DAE
		public bool AttachToCharacters { get; private set; }

		// Token: 0x17000AD2 RID: 2770
		// (get) Token: 0x06002B03 RID: 11011 RVA: 0x001DABB7 File Offset: 0x001D8DB7
		// (set) Token: 0x06002B04 RID: 11012 RVA: 0x001DABBF File Offset: 0x001D8DBF
		public Submarine TargetSubmarine { get; private set; }

		// Token: 0x17000AD3 RID: 2771
		// (get) Token: 0x06002B05 RID: 11013 RVA: 0x001DABC8 File Offset: 0x001D8DC8
		// (set) Token: 0x06002B06 RID: 11014 RVA: 0x001DABD0 File Offset: 0x001D8DD0
		public Structure TargetWall { get; private set; }

		// Token: 0x17000AD4 RID: 2772
		// (get) Token: 0x06002B07 RID: 11015 RVA: 0x001DABD9 File Offset: 0x001D8DD9
		// (set) Token: 0x06002B08 RID: 11016 RVA: 0x001DABE1 File Offset: 0x001D8DE1
		public Character TargetCharacter { get; private set; }

		// Token: 0x17000AD5 RID: 2773
		// (get) Token: 0x06002B09 RID: 11017 RVA: 0x001DABEA File Offset: 0x001D8DEA
		public List<Joint> AttachJoints { get; } = new List<Joint>();

		// Token: 0x17000AD6 RID: 2774
		// (get) Token: 0x06002B0A RID: 11018 RVA: 0x001DABF2 File Offset: 0x001D8DF2
		// (set) Token: 0x06002B0B RID: 11019 RVA: 0x001DABFA File Offset: 0x001D8DFA
		public Vector2? AttachPos { get; private set; }

		// Token: 0x17000AD7 RID: 2775
		// (get) Token: 0x06002B0C RID: 11020 RVA: 0x001DAC03 File Offset: 0x001D8E03
		public bool IsAttached
		{
			get
			{
				return this.AttachJoints.Count > 0;
			}
		}

		// Token: 0x17000AD8 RID: 2776
		// (get) Token: 0x06002B0D RID: 11021 RVA: 0x001DAC13 File Offset: 0x001D8E13
		public bool IsAttachedToSub
		{
			get
			{
				return this.IsAttached && this.TargetSubmarine != null && this.TargetCharacter == null;
			}
		}

		// Token: 0x06002B0E RID: 11022 RVA: 0x001DAC30 File Offset: 0x001D8E30
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

		// Token: 0x06002B0F RID: 11023 RVA: 0x001DAE30 File Offset: 0x001D9030
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

		// Token: 0x06002B10 RID: 11024 RVA: 0x001DAE90 File Offset: 0x001D9090
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

		// Token: 0x06002B11 RID: 11025 RVA: 0x001DAF04 File Offset: 0x001D9104
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

		// Token: 0x06002B12 RID: 11026 RVA: 0x001DAFAC File Offset: 0x001D91AC
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

		// Token: 0x06002B13 RID: 11027 RVA: 0x001DB860 File Offset: 0x001D9A60
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
		}

		// Token: 0x06002B14 RID: 11028 RVA: 0x001DBB64 File Offset: 0x001D9D64
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
		}

		// Token: 0x06002B15 RID: 11029 RVA: 0x001DBC00 File Offset: 0x001D9E00
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

		// Token: 0x06002B16 RID: 11030 RVA: 0x001DBC50 File Offset: 0x001D9E50
		private void OnCharacterDeath(Character character, CauseOfDeath causeOfDeath)
		{
			this.DeattachFromBody(true, 0f);
			character.OnDeath = (Character.OnDeathHandler)Delegate.Remove(character.OnDeath, new Character.OnDeathHandler(this.OnCharacterDeath));
		}

		// Token: 0x04001664 RID: 5732
		private const float RaycastInterval = 5f;

		// Token: 0x04001665 RID: 5733
		private float raycastTimer;

		// Token: 0x04001666 RID: 5734
		private Body targetBody;

		// Token: 0x04001667 RID: 5735
		private Vector2 attachSurfaceNormal;

		// Token: 0x04001668 RID: 5736
		private readonly Character character;

		// Token: 0x0400166F RID: 5743
		private readonly float minDeattachSpeed;

		// Token: 0x04001670 RID: 5744
		private readonly float maxDeattachSpeed;

		// Token: 0x04001671 RID: 5745
		private readonly float maxAttachDuration;

		// Token: 0x04001672 RID: 5746
		private readonly float coolDown;

		// Token: 0x04001673 RID: 5747
		private readonly float damageOnDetach;

		// Token: 0x04001674 RID: 5748
		private readonly float detachStun;

		// Token: 0x04001675 RID: 5749
		private readonly bool weld;

		// Token: 0x04001676 RID: 5750
		private float deattachCheckTimer;

		// Token: 0x04001677 RID: 5751
		private Vector2 _attachPos;

		// Token: 0x04001678 RID: 5752
		private float attachCooldown;

		// Token: 0x04001679 RID: 5753
		private readonly Limb attachLimb;

		// Token: 0x0400167A RID: 5754
		private Vector2 localAttachPos;

		// Token: 0x0400167B RID: 5755
		private readonly float attachLimbRotation;

		// Token: 0x0400167C RID: 5756
		private float jointDir;

		// Token: 0x0400167D RID: 5757
		private float latchedDuration;

		// Token: 0x0400167E RID: 5758
		private readonly bool freezeWhenLatched;
	}
}
