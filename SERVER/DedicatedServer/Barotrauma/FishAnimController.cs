using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A4 RID: 164
	internal class FishAnimController : AnimController
	{
		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x060013BD RID: 5053 RVA: 0x000AB005 File Offset: 0x000A9205
		// (set) Token: 0x060013BE RID: 5054 RVA: 0x000AB00D File Offset: 0x000A920D
		public override RagdollParams RagdollParams
		{
			get
			{
				return this.FishRagdollParams;
			}
			protected set
			{
				this.FishRagdollParams = (value as FishRagdollParams);
			}
		}

		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x060013BF RID: 5055 RVA: 0x000AB01B File Offset: 0x000A921B
		// (set) Token: 0x060013C0 RID: 5056 RVA: 0x000AB03C File Offset: 0x000A923C
		public FishRagdollParams FishRagdollParams
		{
			get
			{
				if (this._ragdollParams == null)
				{
					this._ragdollParams = FishRagdollParams.GetDefaultRagdollParams(this.character);
				}
				return this._ragdollParams;
			}
			protected set
			{
				this._ragdollParams = value;
			}
		}

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x060013C1 RID: 5057 RVA: 0x000AB045 File Offset: 0x000A9245
		// (set) Token: 0x060013C2 RID: 5058 RVA: 0x000AB066 File Offset: 0x000A9266
		public FishWalkParams FishWalkParams
		{
			get
			{
				if (this._fishWalkParams == null)
				{
					this._fishWalkParams = FishWalkParams.GetDefaultAnimParams(this.character);
				}
				return this._fishWalkParams;
			}
			set
			{
				this._fishWalkParams = value;
			}
		}

		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x060013C3 RID: 5059 RVA: 0x000AB06F File Offset: 0x000A926F
		// (set) Token: 0x060013C4 RID: 5060 RVA: 0x000AB090 File Offset: 0x000A9290
		public FishRunParams FishRunParams
		{
			get
			{
				if (this._fishRunParams == null)
				{
					this._fishRunParams = FishRunParams.GetDefaultAnimParams(this.character);
				}
				return this._fishRunParams;
			}
			set
			{
				this._fishRunParams = value;
			}
		}

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x060013C5 RID: 5061 RVA: 0x000AB099 File Offset: 0x000A9299
		// (set) Token: 0x060013C6 RID: 5062 RVA: 0x000AB0BA File Offset: 0x000A92BA
		public FishSwimSlowParams FishSwimSlowParams
		{
			get
			{
				if (this._fishSwimSlowParams == null)
				{
					this._fishSwimSlowParams = FishSwimSlowParams.GetDefaultAnimParams(this.character);
				}
				return this._fishSwimSlowParams;
			}
			set
			{
				this._fishSwimSlowParams = value;
			}
		}

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x060013C7 RID: 5063 RVA: 0x000AB0C3 File Offset: 0x000A92C3
		// (set) Token: 0x060013C8 RID: 5064 RVA: 0x000AB0E4 File Offset: 0x000A92E4
		public FishSwimFastParams FishSwimFastParams
		{
			get
			{
				if (this._fishSwimFastParams == null)
				{
					this._fishSwimFastParams = FishSwimFastParams.GetDefaultAnimParams(this.character);
				}
				return this._fishSwimFastParams;
			}
			set
			{
				this._fishSwimFastParams = value;
			}
		}

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x060013C9 RID: 5065 RVA: 0x000AB0ED File Offset: 0x000A92ED
		public IFishAnimation CurrentFishAnimation
		{
			get
			{
				return base.CurrentAnimationParams as IFishAnimation;
			}
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x060013CA RID: 5066 RVA: 0x000AB0FA File Offset: 0x000A92FA
		public new FishGroundedParams CurrentGroundedParams
		{
			get
			{
				return base.CurrentGroundedParams as FishGroundedParams;
			}
		}

		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x060013CB RID: 5067 RVA: 0x000AB107 File Offset: 0x000A9307
		public new FishSwimParams CurrentSwimParams
		{
			get
			{
				return base.CurrentSwimParams as FishSwimParams;
			}
		}

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x060013CC RID: 5068 RVA: 0x000AB114 File Offset: 0x000A9314
		public float? TailAngle
		{
			get
			{
				AnimationParams currentAnimationParams = base.CurrentAnimationParams;
				IFishAnimation currentFishAnimation = this.CurrentFishAnimation;
				return base.GetValidOrNull(currentAnimationParams, (currentFishAnimation != null) ? new float?(currentFishAnimation.TailAngleInRadians) : null);
			}
		}

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x060013CD RID: 5069 RVA: 0x000AB14C File Offset: 0x000A934C
		public float FootTorque
		{
			get
			{
				return base.CurrentAnimationParams.FootTorque;
			}
		}

		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x060013CE RID: 5070 RVA: 0x000AB159 File Offset: 0x000A9359
		public float HeadTorque
		{
			get
			{
				return base.CurrentAnimationParams.HeadTorque;
			}
		}

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x060013CF RID: 5071 RVA: 0x000AB166 File Offset: 0x000A9366
		public float TorsoTorque
		{
			get
			{
				return base.CurrentAnimationParams.TorsoTorque;
			}
		}

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x060013D0 RID: 5072 RVA: 0x000AB173 File Offset: 0x000A9373
		public float TailTorque
		{
			get
			{
				return this.CurrentFishAnimation.TailTorque;
			}
		}

		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x060013D1 RID: 5073 RVA: 0x000AB180 File Offset: 0x000A9380
		public float HeadMoveForce
		{
			get
			{
				return this.CurrentGroundedParams.HeadMoveForce;
			}
		}

		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x060013D2 RID: 5074 RVA: 0x000AB18D File Offset: 0x000A938D
		public float TorsoMoveForce
		{
			get
			{
				return this.CurrentGroundedParams.TorsoMoveForce;
			}
		}

		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x060013D3 RID: 5075 RVA: 0x000AB19A File Offset: 0x000A939A
		public float FootMoveForce
		{
			get
			{
				return this.CurrentGroundedParams.FootMoveForce;
			}
		}

		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x060013D4 RID: 5076 RVA: 0x000AB1A7 File Offset: 0x000A93A7
		// (set) Token: 0x060013D5 RID: 5077 RVA: 0x000AB1AF File Offset: 0x000A93AF
		public override GroundedMovementParams WalkParams
		{
			get
			{
				return this.FishWalkParams;
			}
			set
			{
				this.FishWalkParams = (value as FishWalkParams);
			}
		}

		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x060013D6 RID: 5078 RVA: 0x000AB1BD File Offset: 0x000A93BD
		// (set) Token: 0x060013D7 RID: 5079 RVA: 0x000AB1C5 File Offset: 0x000A93C5
		public override GroundedMovementParams RunParams
		{
			get
			{
				return this.FishRunParams;
			}
			set
			{
				this.FishRunParams = (value as FishRunParams);
			}
		}

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x060013D8 RID: 5080 RVA: 0x000AB1D3 File Offset: 0x000A93D3
		// (set) Token: 0x060013D9 RID: 5081 RVA: 0x000AB1DB File Offset: 0x000A93DB
		public override SwimParams SwimSlowParams
		{
			get
			{
				return this.FishSwimSlowParams;
			}
			set
			{
				this.FishSwimSlowParams = (value as FishSwimSlowParams);
			}
		}

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x060013DA RID: 5082 RVA: 0x000AB1E9 File Offset: 0x000A93E9
		// (set) Token: 0x060013DB RID: 5083 RVA: 0x000AB1F1 File Offset: 0x000A93F1
		public override SwimParams SwimFastParams
		{
			get
			{
				return this.FishSwimFastParams;
			}
			set
			{
				this.FishSwimFastParams = (value as FishSwimFastParams);
			}
		}

		// Token: 0x060013DC RID: 5084 RVA: 0x000AB1FF File Offset: 0x000A93FF
		public FishAnimController(Character character, string seed, FishRagdollParams ragdollParams = null) : base(character, seed, ragdollParams)
		{
		}

		// Token: 0x060013DD RID: 5085 RVA: 0x000AB20C File Offset: 0x000A940C
		protected override void UpdateAnim(float deltaTime)
		{
			if (Timing.TotalTime - this.character.SpawnTime < 0.10000000149011612)
			{
				return;
			}
			if (base.Frozen)
			{
				return;
			}
			if (base.MainLimb == null)
			{
				this.<UpdateAnim>g__ResetState|60_0();
				return;
			}
			base.UpdateConstantTorque(deltaTime);
			base.UpdateBlink(deltaTime);
			Limb mainLimb = base.MainLimb;
			this.levitatingCollider = (!base.IsHangingWithRope && !base.IsClimbing);
			if (!this.character.CanMove)
			{
				base.UpdateRagdollControlsMovement();
				if (this.character.IsDead && this.deathAnimTimer < this.deathAnimDuration)
				{
					this.deathAnimTimer += deltaTime;
					this.UpdateDying(deltaTime);
				}
				else if (!base.InWater && !base.CanWalk && this.character.AllowInput)
				{
					this.UpdateDying(deltaTime);
				}
				this.<UpdateAnim>g__ResetState|60_0();
				return;
			}
			this.deathAnimTimer = 0f;
			if (!base.Collider.Enabled)
			{
				Limb lowestLimb = base.FindLowestLimb();
				if (base.InWater)
				{
					base.Collider.SetTransformIgnoreContacts(new Vector2(base.Collider.SimPosition.X, base.MainLimb.SimPosition.Y), 0f, true);
				}
				else
				{
					base.Collider.SetTransformIgnoreContacts(new Vector2(base.Collider.SimPosition.X, Math.Max(lowestLimb.SimPosition.Y + (base.Collider.Radius + base.Collider.Height / 2f), base.Collider.SimPosition.Y)), 0f, true);
				}
				base.Collider.Enabled = true;
			}
			base.ResetPullJoints(null);
			if (this.strongestImpact > 0f)
			{
				this.character.Stun = MathHelper.Clamp(this.strongestImpact * 0.5f, this.character.Stun, 5f);
				this.strongestImpact = 0f;
			}
			if (base.Aiming)
			{
				base.TargetMovement = base.TargetMovement.ClampLength(2f);
			}
			if (base.IsClimbing)
			{
				base.UpdateClimbing();
			}
			if (this.inWater && !this.forceStanding)
			{
				base.Collider.FarseerBody.FixedRotation = false;
				this.UpdateSineAnim(deltaTime);
			}
			else if (this.RagdollParams.CanWalk && (this.currentHull != null || this.forceStanding))
			{
				if (this.CurrentGroundedParams != null)
				{
					float standAngle = this.CurrentGroundedParams.ColliderStandAngleInRadians * base.Dir;
					if (Math.Abs(MathUtils.GetShortestAngle(base.Collider.Rotation, standAngle)) > 0.001f)
					{
						base.Collider.AngularVelocity = MathUtils.GetShortestAngle(base.Collider.Rotation, standAngle) * 60f;
						base.Collider.FarseerBody.FixedRotation = false;
					}
					else
					{
						base.Collider.FarseerBody.FixedRotation = true;
					}
				}
				this.UpdateWalkAnim(deltaTime);
			}
			if (this.character.SelectedCharacter != null)
			{
				this.DragCharacter(this.character.SelectedCharacter, deltaTime);
				this.<UpdateAnim>g__ResetState|60_0();
				return;
			}
			if (this.character.AnimController.AnimationTestPose)
			{
				base.ApplyTestPose();
			}
			if (base.SimplePhysicsEnabled)
			{
				this.<UpdateAnim>g__ResetState|60_0();
				return;
			}
			if (!this.character.IsRemotelyControlled && (this.character.AIController == null || this.character.AIController.CanFlip) && !base.Aiming)
			{
				if (!this.inWater || (this.CurrentSwimParams != null && this.CurrentSwimParams.Mirror))
				{
					if (this.targetMovement.X > 0.1f && this.targetMovement.X > Math.Abs(this.targetMovement.Y) * 0.2f)
					{
						this.TargetDir = Direction.Right;
					}
					else if (this.targetMovement.X < -0.1f && this.targetMovement.X < -Math.Abs(this.targetMovement.Y) * 0.2f)
					{
						this.TargetDir = Direction.Left;
					}
				}
				else
				{
					float rotation = MathHelper.WrapAngle(base.Collider.Rotation);
					rotation = MathHelper.ToDegrees(rotation);
					if (rotation < 0f)
					{
						rotation += 360f;
					}
					if (rotation > 20f && rotation < 160f)
					{
						this.TargetDir = Direction.Left;
					}
					else if (rotation > 200f && rotation < 340f)
					{
						this.TargetDir = Direction.Right;
					}
				}
			}
			if (!base.IsStuck && this.CurrentFishAnimation.Flip)
			{
				AIController aicontroller = this.character.AIController;
				if (aicontroller == null || aicontroller.CanFlip)
				{
					this.flipCooldown -= deltaTime;
					if (this.TargetDir != Direction.None && this.TargetDir != this.dir)
					{
						this.flipTimer += deltaTime;
						float requiredSpeed = base.CurrentAnimationParams.MovementSpeed / 2f;
						if (base.CurrentHull != null)
						{
							requiredSpeed /= 2f;
						}
						bool isMovingFastEnough = Math.Abs(base.MainLimb.LinearVelocity.X) > requiredSpeed;
						bool isTryingToMoveHorizontally = Math.Abs(base.TargetMovement.X) > Math.Abs(base.TargetMovement.Y);
						if ((this.flipTimer > this.CurrentFishAnimation.FlipDelay && this.flipCooldown <= 0f && ((isMovingFastEnough && isTryingToMoveHorizontally) || base.IsMovingBackwards)) || this.character.IsRemotePlayer)
						{
							this.Flip();
							if (!this.inWater || (this.CurrentSwimParams != null && this.CurrentSwimParams.Mirror))
							{
								this.Mirror(this.CurrentSwimParams == null || this.CurrentSwimParams.MirrorLerp);
							}
							this.flipTimer = 0f;
							this.flipCooldown = this.CurrentFishAnimation.FlipCooldown;
						}
					}
					else
					{
						this.flipTimer = 0f;
					}
				}
			}
			this.<UpdateAnim>g__ResetState|60_0();
		}

		// Token: 0x060013DE RID: 5086 RVA: 0x000AB813 File Offset: 0x000A9A13
		private bool CanDrag(Character target)
		{
			return base.Mass / target.Mass > 0.1f;
		}

		// Token: 0x060013DF RID: 5087 RVA: 0x000AB82C File Offset: 0x000A9A2C
		public override void DragCharacter(Character target, float deltaTime)
		{
			if (target == null)
			{
				return;
			}
			Limb mouthLimb = base.GetLimb(LimbType.Head, true, false, false);
			if (mouthLimb == null)
			{
				return;
			}
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				Vector2 sourceSimPos = base.SimplePhysicsEnabled ? this.character.SimPosition : mouthLimb.SimPosition;
				Vector2 targetSimPos = target.SimPosition;
				if (this.character.Submarine != null && this.character.SelectedCharacter.Submarine == null)
				{
					targetSimPos -= this.character.Submarine.SimPosition;
				}
				else if (this.character.Submarine == null && this.character.SelectedCharacter.Submarine != null)
				{
					sourceSimPos -= this.character.SelectedCharacter.Submarine.SimPosition;
				}
				Body body = Submarine.CheckVisibility(sourceSimPos, targetSimPos, false, true, true, true, true, null);
				if (body != null)
				{
					this.character.DeselectCharacter();
					return;
				}
			}
			if (base.Character.CanEat)
			{
				Vector2 mouthPos = base.SimplePhysicsEnabled ? this.character.SimPosition : (base.GetMouthPosition() ?? Vector2.Zero);
				Vector2 attackSimPosition = (this.character.Submarine == null) ? ConvertUnits.ToSimUnits(target.WorldPosition) : target.SimPosition;
				Vector2 limbDiff = attackSimPosition - mouthPos;
				float extent = Math.Max(mouthLimb.body.GetMaxExtent(), 1f);
				bool tooFar = this.character.InWater ? (limbDiff.LengthSquared() > extent * extent) : (limbDiff.X > extent);
				if (tooFar)
				{
					this.character.DeselectCharacter();
					return;
				}
				float dmg = this.character.Params.EatingSpeed;
				float eatSpeed = dmg / ((float)Math.Sqrt((double)Math.Max(target.Mass, 1f)) * 10f);
				this.eatTimer += deltaTime * eatSpeed;
				float dragForce = MathHelper.Clamp(eatSpeed * 10f, 0f, 40f);
				if (dragForce > 0.1f)
				{
					Vector2 targetPos = mouthPos;
					if (target.Submarine != null && this.character.Submarine == null)
					{
						targetPos -= target.Submarine.SimPosition;
					}
					else if (target.Submarine == null && this.character.Submarine != null)
					{
						targetPos += this.character.Submarine.SimPosition;
					}
					target.AnimController.MainLimb.body.SmoothRotate(mouthLimb.Rotation, dragForce * 2f, true);
					if (!target.AnimController.SimplePhysicsEnabled)
					{
						target.AnimController.MainLimb.MoveToPos(targetPos, (float)(Math.Sin((double)this.eatTimer) + (double)dragForce), false);
					}
					target.AnimController.Collider.MoveToPos(targetPos, (float)(Math.Sin((double)this.eatTimer) + (double)dragForce), null);
				}
				if (base.InWater)
				{
					float pullStrength = (float)(Math.Sin((double)this.eatTimer) * Math.Max(Math.Sin((double)(this.eatTimer * 0.5f)), 0.0));
					mouthLimb.body.ApplyForce(limbDiff * mouthLimb.Mass * 50f * pullStrength, 64f);
				}
				else
				{
					float force = (float)Math.Sin((double)(this.eatTimer * 100f)) * mouthLimb.Mass;
					mouthLimb.body.ApplyLinearImpulse(Vector2.UnitY * force * mouthLimb.Params.EatImpulse, 64f);
					mouthLimb.body.ApplyTorque(-force * mouthLimb.Params.EatTorque);
				}
				Limb jaw = base.GetLimb(LimbType.Jaw, true, false, false);
				if (jaw != null)
				{
					jaw.body.ApplyTorque(-(float)Math.Sin((double)(this.eatTimer * 150f)) * jaw.Mass * 25f);
				}
				this.character.ApplyStatusEffects(ActionType.OnEating, deltaTime);
				if (target.IsDead)
				{
					float particleFrequency = MathHelper.Clamp(eatSpeed / 2f, 0.02f, 0.5f);
					if (Rand.Value(Rand.RandSync.Unsynced) < particleFrequency / 6f)
					{
						target.AnimController.MainLimb.AddDamage(target.SimPosition, dmg, 0f, 0f, false);
					}
					if (Rand.Value(Rand.RandSync.Unsynced) < particleFrequency)
					{
						target.AnimController.MainLimb.AddDamage(target.SimPosition, 0f, dmg, 0f, false);
					}
					if (this.eatTimer % 1f < 0.5f && (this.eatTimer - deltaTime * eatSpeed) % 1f > 0.5f)
					{
						IEnumerable<LimbJoint> limbJoints = target.AnimController.LimbJoints;
						Func<LimbJoint, bool> predicate;
						if ((predicate = FishAnimController.<>O.<0>__CanBeSevered) == null)
						{
							predicate = (FishAnimController.<>O.<0>__CanBeSevered = new Func<LimbJoint, bool>(FishAnimController.<DragCharacter>g__CanBeSevered|63_0));
						}
						IEnumerable<LimbJoint> nonSeveredJoints = limbJoints.Where(predicate);
						if (nonSeveredJoints.None(null))
						{
							if (base.Mass < target.AnimController.Mass)
							{
								CharacterInventory inventory = target.Inventory;
								if (inventory != null)
								{
									inventory.AllItemsMod.ForEach(delegate(Item it)
									{
										if (it != null)
										{
											it.Drop(null, true, true);
										}
									});
								}
							}
							EntitySpawner spawner = Entity.Spawner;
							if (spawner != null)
							{
								spawner.AddEntityToRemoveQueue(target);
							}
							EnemyAIController enemyAi = base.Character.AIController as EnemyAIController;
							if (enemyAi != null)
							{
								PetBehavior petBehavior = enemyAi.PetBehavior;
								if (petBehavior != null)
								{
									petBehavior.OnEat(target);
								}
							}
							this.character.DeselectCharacter();
							return;
						}
						target.AnimController.SeverLimbJoint(nonSeveredJoints.GetRandomUnsynced<LimbJoint>());
					}
				}
			}
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x000ABDD0 File Offset: 0x000A9FD0
		private void UpdateSineAnim(float deltaTime)
		{
			FishAnimController.<>c__DisplayClass65_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.CurrentSwimParams == null)
			{
				return;
			}
			this.movement = base.TargetMovement;
			bool isMoving = this.movement.LengthSquared() > 1E-05f;
			CS$<>8__locals1.mainLimb = base.MainLimb;
			float t = 0.5f;
			if (isMoving && !base.SimplePhysicsEnabled && this.CurrentSwimParams.RotateTowardsMovement)
			{
				Vector2 forward = VectorExtensions.Forward(base.Collider.Rotation + 1.5707964f, 1f);
				float dot = Vector2.Dot(forward, Vector2.Normalize(this.movement));
				if (dot < 0f)
				{
					t = MathHelper.Clamp((1f + dot) / 10f, 0.01f, 0.1f);
				}
			}
			if (base.Collider.BodyType == BodyType.Dynamic)
			{
				base.Collider.LinearVelocity = Vector2.Lerp(base.Collider.LinearVelocity, this.movement, t);
			}
			if (base.SimplePhysicsEnabled)
			{
				return;
			}
			CS$<>8__locals1.mainLimb.PullJointEnabled = true;
			if (!isMoving && !this.CurrentSwimParams.UpdateAnimationWhenNotMoving)
			{
				base.WalkPos = MathHelper.SmoothStep(base.WalkPos, 1.5707964f, deltaTime * 5f);
				CS$<>8__locals1.mainLimb.PullJointWorldAnchorB = base.Collider.SimPosition;
				if (this.aiming)
				{
					Vector2 mousePos = ConvertUnits.ToSimUnits(this.character.CursorPosition);
					Vector2 diff = (mousePos - (base.GetLimb(LimbType.Torso, true, false, false) ?? base.MainLimb).SimPosition) * base.Dir;
					base.TargetMovement = new Vector2(0f, -0.1f);
					float newRotation = MathHelper.WrapAngle(MathUtils.VectorToAngle(diff) - 1.5707964f * base.Dir);
					base.Collider.SmoothRotate(newRotation, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier * 2f, true);
					if (this.TorsoAngle != null)
					{
						Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
						if (torso != null)
						{
							this.SmoothRotateWithoutWrapping(torso, newRotation + this.TorsoAngle.Value * base.Dir, CS$<>8__locals1.mainLimb, this.TorsoTorque * 2f);
						}
					}
				}
			}
			else
			{
				Vector2 transformedMovement = this.Reverse ? (-this.movement) : this.movement;
				FishAnimController.<>c__DisplayClass65_1 CS$<>8__locals2;
				CS$<>8__locals2.movementAngle = MathUtils.VectorToAngle(transformedMovement) - 1.5707964f;
				float mainLimbAngle = 0f;
				if (CS$<>8__locals1.mainLimb.type == LimbType.Torso && this.TorsoAngle != null)
				{
					mainLimbAngle = this.TorsoAngle.Value;
				}
				else if (CS$<>8__locals1.mainLimb.type == LimbType.Head && this.HeadAngle != null)
				{
					mainLimbAngle = this.HeadAngle.Value;
				}
				mainLimbAngle *= base.Dir;
				while (CS$<>8__locals1.mainLimb.Rotation - (CS$<>8__locals2.movementAngle + mainLimbAngle) > 3.1415927f)
				{
					CS$<>8__locals2.movementAngle += 6.2831855f;
				}
				while (CS$<>8__locals1.mainLimb.Rotation - (CS$<>8__locals2.movementAngle + mainLimbAngle) < -3.1415927f)
				{
					CS$<>8__locals2.movementAngle -= 6.2831855f;
				}
				if (this.CurrentSwimParams.RotateTowardsMovement)
				{
					base.Collider.SmoothRotate(CS$<>8__locals2.movementAngle, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier, true);
					if (this.TorsoAngle != null)
					{
						Limb torso2 = base.GetLimb(LimbType.Torso, true, false, false);
						if (torso2 != null)
						{
							this.SmoothRotateWithoutWrapping(torso2, CS$<>8__locals2.movementAngle + this.TorsoAngle.Value * base.Dir, CS$<>8__locals1.mainLimb, this.TorsoTorque);
						}
					}
					if (this.HeadAngle != null)
					{
						Limb head = base.GetLimb(LimbType.Head, true, false, false);
						if (head != null)
						{
							this.SmoothRotateWithoutWrapping(head, CS$<>8__locals2.movementAngle + this.HeadAngle.Value * base.Dir, CS$<>8__locals1.mainLimb, this.HeadTorque);
						}
					}
					if (this.TailAngle != null)
					{
						bool isAngleApplied = false;
						foreach (Limb limb in base.Limbs)
						{
							if (!limb.IsSevered && limb.Params.ApplyTailAngle)
							{
								this.<UpdateSineAnim>g__RotateTail|65_0(limb, ref CS$<>8__locals1, ref CS$<>8__locals2);
								isAngleApplied = true;
							}
						}
						if (!isAngleApplied)
						{
							this.<UpdateSineAnim>g__RotateTail|65_0(base.GetLimb(LimbType.Tail, true, false, false), ref CS$<>8__locals1, ref CS$<>8__locals2);
						}
					}
				}
				else
				{
					CS$<>8__locals2.movementAngle = ((base.Dir > 0f) ? -1.5707964f : 1.5707964f);
					if (this.Reverse)
					{
						CS$<>8__locals2.movementAngle = MathUtils.WrapAngleTwoPi(CS$<>8__locals2.movementAngle - 3.1415927f);
					}
					if (CS$<>8__locals1.mainLimb.type == LimbType.Head && this.HeadAngle != null)
					{
						base.Collider.SmoothRotate(this.HeadAngle.Value * base.Dir, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier, true);
					}
					else if (CS$<>8__locals1.mainLimb.type == LimbType.Torso && this.TorsoAngle != null)
					{
						base.Collider.SmoothRotate(this.TorsoAngle.Value * base.Dir, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier, true);
					}
					if (this.TorsoAngle != null)
					{
						Limb torso3 = base.GetLimb(LimbType.Torso, true, false, false);
						if (torso3 != null)
						{
							torso3.body.SmoothRotate(this.TorsoAngle.Value * base.Dir, this.TorsoTorque, true);
						}
					}
					if (this.HeadAngle != null)
					{
						Limb head2 = base.GetLimb(LimbType.Head, true, false, false);
						if (head2 != null)
						{
							head2.body.SmoothRotate(this.HeadAngle.Value * base.Dir, this.HeadTorque, true);
						}
					}
					if (this.TailAngle != null)
					{
						bool isAngleApplied2 = false;
						foreach (Limb limb2 in base.Limbs)
						{
							if (!limb2.IsSevered && limb2.type == LimbType.Tail && limb2.Params.ApplyTailAngle)
							{
								this.<UpdateSineAnim>g__RotateTail|65_1(limb2, ref CS$<>8__locals1);
								isAngleApplied2 = true;
							}
						}
						if (!isAngleApplied2)
						{
							this.<UpdateSineAnim>g__RotateTail|65_1(base.GetLimb(LimbType.Tail, true, false, false), ref CS$<>8__locals1);
						}
					}
				}
				float waveLength = Math.Abs(this.CurrentSwimParams.WaveLength * this.RagdollParams.JointScale);
				float waveAmplitude = Math.Abs(this.CurrentSwimParams.WaveAmplitude * this.character.SpeedMultiplier);
				if (waveLength > 0f && waveAmplitude > 0f)
				{
					base.WalkPos -= transformedMovement.Length() / Math.Abs(waveLength);
					base.WalkPos = MathUtils.WrapAngleTwoPi(base.WalkPos);
				}
				foreach (Limb limb3 in base.Limbs)
				{
					if (!limb3.IsSevered)
					{
						LimbType type = limb3.type;
						bool flag = type - LimbType.LeftFoot <= 1;
						if (flag && this.CurrentSwimParams.FootAnglesInRadians.ContainsKey(limb3.Params.ID))
						{
							this.SmoothRotateWithoutWrapping(limb3, CS$<>8__locals2.movementAngle + this.CurrentSwimParams.FootAnglesInRadians[limb3.Params.ID] * base.Dir, CS$<>8__locals1.mainLimb, this.FootTorque);
						}
						if ((limb3.type == LimbType.Tail || limb3.Params.ApplySineMovement) && waveLength > 0f && waveAmplitude > 0f)
						{
							float waveRotation = (float)Math.Sin((double)(base.WalkPos * limb3.Params.SineFrequencyMultiplier));
							limb3.body.ApplyTorque(waveRotation * limb3.Mass * waveAmplitude * limb3.Params.SineAmplitudeMultiplier);
						}
						if (limb3.SteerForce > 0f && base.Collider.PhysEnabled)
						{
							Vector2 pullPos = limb3.PullJointWorldAnchorA;
							limb3.body.ApplyForce(this.movement * limb3.SteerForce * limb3.Mass * Math.Max(this.character.SpeedMultiplier, 1f), pullPos);
						}
					}
				}
				Vector2 mainLimbDiff = CS$<>8__locals1.mainLimb.PullJointWorldAnchorB - CS$<>8__locals1.mainLimb.SimPosition;
				if (this.CurrentSwimParams.UseSineMovement)
				{
					CS$<>8__locals1.mainLimb.PullJointWorldAnchorB = Vector2.SmoothStep(CS$<>8__locals1.mainLimb.PullJointWorldAnchorB, base.Collider.SimPosition, (mainLimbDiff.LengthSquared() > 10f) ? 1f : ((float)Math.Abs(Math.Sin((double)base.WalkPos))));
				}
				else
				{
					CS$<>8__locals1.mainLimb.PullJointWorldAnchorB = Vector2.Lerp(CS$<>8__locals1.mainLimb.PullJointWorldAnchorB, base.Collider.SimPosition, (mainLimbDiff.LengthSquared() > 10f) ? 1f : 0.5f);
				}
			}
			this.floorY = base.Limbs[0].SimPosition.Y;
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x000AC76C File Offset: 0x000AA96C
		private void UpdateWalkAnim(float deltaTime)
		{
			FishAnimController.<>c__DisplayClass66_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			this.movement = MathUtils.SmoothStep(this.movement, base.TargetMovement, 0.2f);
			if (base.Collider.BodyType == BodyType.Dynamic && this.onGround)
			{
				base.Collider.LinearVelocity = new Vector2(this.movement.X, (base.Collider.LinearVelocity.Y > 0f) ? (base.Collider.LinearVelocity.Y * 0.5f) : base.Collider.LinearVelocity.Y);
			}
			if (base.SimplePhysicsEnabled)
			{
				return;
			}
			Vector2 colliderBottom = base.GetColliderBottom();
			CS$<>8__locals1.movementAngle = 0f;
			CS$<>8__locals1.mainLimb = base.MainLimb;
			float mainLimbAngle = ((CS$<>8__locals1.mainLimb.type == LimbType.Torso) ? this.TorsoAngle.GetValueOrDefault() : this.HeadAngle.GetValueOrDefault()) * base.Dir;
			while (CS$<>8__locals1.mainLimb.Rotation - (CS$<>8__locals1.movementAngle + mainLimbAngle) > 3.1415927f)
			{
				CS$<>8__locals1.movementAngle += 6.2831855f;
			}
			while (CS$<>8__locals1.mainLimb.Rotation - (CS$<>8__locals1.movementAngle + mainLimbAngle) < -3.1415927f)
			{
				CS$<>8__locals1.movementAngle -= 6.2831855f;
			}
			float offset = 3.1415927f * this.CurrentGroundedParams.StepLiftOffset;
			if (this.character.AnimController.Dir < 0f)
			{
				offset += 3.1415927f * this.CurrentGroundedParams.StepLiftFrequency;
			}
			float stepLift = (base.TargetMovement.X == 0f) ? 0f : ((float)Math.Sin((double)(base.WalkPos * base.Dir * this.CurrentGroundedParams.StepLiftFrequency + offset)) * (this.CurrentGroundedParams.StepLiftAmount / 100f));
			float limpAmount = this.character.GetLegPenalty(0f);
			if (limpAmount > 0f)
			{
				float walkPosX = (float)Math.Cos((double)base.WalkPos);
				limpAmount = Math.Max(Math.Abs(walkPosX) * limpAmount, 0f) * Math.Min(Math.Abs(base.TargetMovement.X), 0.3f) * base.Dir;
			}
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			if (torso != null)
			{
				if (this.TorsoAngle != null)
				{
					this.SmoothRotateWithoutWrapping(torso, CS$<>8__locals1.movementAngle + this.TorsoAngle.Value * base.Dir, CS$<>8__locals1.mainLimb, this.TorsoTorque);
				}
				if (this.TorsoPosition != null && this.TorsoMoveForce > 0f)
				{
					Vector2 pos = colliderBottom + new Vector2(limpAmount, this.TorsoPosition.Value + stepLift);
					if (torso != CS$<>8__locals1.mainLimb)
					{
						pos.X = torso.SimPosition.X;
					}
					torso.MoveToPos(pos, this.TorsoMoveForce, false);
					torso.PullJointEnabled = true;
					torso.PullJointWorldAnchorB = pos;
				}
			}
			Limb head = base.GetLimb(LimbType.Head, true, false, false);
			if (head != null)
			{
				bool headFacingBackwards = false;
				if (this.HeadAngle != null && head != CS$<>8__locals1.mainLimb)
				{
					this.SmoothRotateWithoutWrapping(head, CS$<>8__locals1.movementAngle + this.HeadAngle.Value * base.Dir, CS$<>8__locals1.mainLimb, this.HeadTorque);
					if (Math.Sign(head.SimPosition.X - CS$<>8__locals1.mainLimb.SimPosition.X) != Math.Sign(base.Dir))
					{
						headFacingBackwards = true;
					}
				}
				if (this.HeadPosition != null && this.HeadMoveForce > 0f && !headFacingBackwards)
				{
					Vector2 pos2 = colliderBottom + new Vector2(limpAmount, this.HeadPosition.Value + stepLift * this.CurrentGroundedParams.StepLiftHeadMultiplier);
					if (head != CS$<>8__locals1.mainLimb)
					{
						pos2.X = head.SimPosition.X;
					}
					head.MoveToPos(pos2, this.HeadMoveForce, false);
					head.PullJointEnabled = true;
					head.PullJointWorldAnchorB = pos2;
				}
			}
			if (this.TailAngle != null)
			{
				bool isAngleApplied = false;
				foreach (Limb limb in base.Limbs)
				{
					if (!limb.IsSevered && limb.type == LimbType.Tail && limb.Params.ApplyTailAngle)
					{
						this.<UpdateWalkAnim>g__RotateTail|66_0(limb, ref CS$<>8__locals1);
						isAngleApplied = true;
					}
				}
				if (!isAngleApplied)
				{
					this.<UpdateWalkAnim>g__RotateTail|66_0(base.GetLimb(LimbType.Tail, true, false, false), ref CS$<>8__locals1);
				}
			}
			float prevWalkPos = base.WalkPos;
			base.WalkPos -= CS$<>8__locals1.mainLimb.LinearVelocity.X * (base.CurrentAnimationParams.CycleSpeed / this.RagdollParams.JointScale / 100f);
			Vector2 transformedStepSize = Vector2.Zero;
			if (Math.Abs(base.TargetMovement.X) > 0.01f)
			{
				transformedStepSize = new Vector2((float)Math.Cos((double)base.WalkPos) * this.StepSize.Value.X * 3f, (float)Math.Sin((double)base.WalkPos) * this.StepSize.Value.Y * 2f);
			}
			foreach (Limb limb2 in base.Limbs)
			{
				if (!limb2.IsSevered)
				{
					LimbType type = limb2.type;
					if (type - LimbType.LeftLeg > 1)
					{
						if (type - LimbType.LeftFoot <= 1)
						{
							Vector2 footPos = new Vector2(limb2.SimPosition.X, colliderBottom.Y);
							if (limb2.RefJointIndex > -1)
							{
								if (this.LimbJoints.Length <= limb2.RefJointIndex)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(281, 1);
									defaultInterpolatedStringHandler.AppendLiteral("Reference joint index ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(limb2.RefJointIndex);
									defaultInterpolatedStringHandler.AppendLiteral(" is out of array. This is probably due to a missing joint. If you just deleted a joint, don't do that without first removing the reference joint indices from the limbs. If this is not the case, please ensure that you have defined the index to the right joint.");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
								}
								else
								{
									footPos.X = this.LimbJoints[limb2.RefJointIndex].WorldAnchorA.X;
								}
							}
							footPos.X += limb2.StepOffset.X * base.Dir;
							footPos.Y += limb2.StepOffset.Y;
							if (limb2.type == LimbType.LeftFoot)
							{
								if (Math.Sign(Math.Sin((double)prevWalkPos)) > 0 && Math.Sign(transformedStepSize.Y) < 0)
								{
								}
								limb2.DebugRefPos = footPos + Vector2.UnitX * this.movement.X * 0.1f;
								limb2.DebugTargetPos = footPos + new Vector2(transformedStepSize.X + this.movement.X * 0.1f, (transformedStepSize.Y > 0f) ? transformedStepSize.Y : 0f);
								limb2.MoveToPos(limb2.DebugTargetPos, this.FootMoveForce, false);
							}
							else if (limb2.type == LimbType.RightFoot)
							{
								if (Math.Sign(Math.Sin((double)prevWalkPos)) < 0 && Math.Sign(transformedStepSize.Y) > 0)
								{
								}
								limb2.DebugRefPos = footPos + Vector2.UnitX * this.movement.X * 0.1f;
								limb2.DebugTargetPos = footPos + new Vector2(-transformedStepSize.X + this.movement.X * 0.1f, (-transformedStepSize.Y > 0f) ? (-transformedStepSize.Y) : 0f);
								limb2.MoveToPos(limb2.DebugTargetPos, this.FootMoveForce, false);
							}
							if (this.CurrentGroundedParams.FootAnglesInRadians.ContainsKey(limb2.Params.ID))
							{
								this.SmoothRotateWithoutWrapping(limb2, CS$<>8__locals1.movementAngle + this.CurrentGroundedParams.FootAnglesInRadians[limb2.Params.ID] * base.Dir, CS$<>8__locals1.mainLimb, this.FootTorque);
							}
						}
					}
					else if (Math.Abs(this.CurrentGroundedParams.LegTorque) > 0f)
					{
						limb2.body.ApplyTorque(limb2.Mass * this.CurrentGroundedParams.LegTorque * base.Dir);
					}
				}
			}
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x000AD020 File Offset: 0x000AB220
		private void UpdateDying(float deltaTime)
		{
			if (this.deathAnimDuration <= 0f)
			{
				return;
			}
			float noise = (PerlinNoise.GetPerlin(base.WalkPos * 0.002f, base.WalkPos * 0.003f) - 0.5f) * 5f;
			float animStrength = 1f - this.deathAnimTimer / this.deathAnimDuration;
			Limb baseLimb = base.GetLimb(LimbType.Head, true, false, false);
			if (baseLimb == base.MainLimb)
			{
				int connectedToHeadCount = base.GetConnectedLimbs(baseLimb).Count;
				if (connectedToHeadCount == 1)
				{
					baseLimb = null;
				}
				Limb torso = base.GetLimb(LimbType.Torso, false, false, false);
				if (torso != null)
				{
					int connectedToTorsoCount = base.GetConnectedLimbs(torso).Count;
					if (connectedToTorsoCount > connectedToHeadCount)
					{
						baseLimb = torso;
					}
				}
			}
			else if (baseLimb == null)
			{
				baseLimb = base.GetLimb(LimbType.Torso, true, false, false);
				if (baseLimb == null)
				{
					return;
				}
			}
			List<Limb> connectedToBaseLimb = base.GetConnectedLimbs(baseLimb);
			Limb tail = base.GetLimb(LimbType.Tail, true, false, false);
			if (baseLimb != null)
			{
				baseLimb.body.ApplyTorque((float)(Math.Sqrt((double)baseLimb.Mass) * (double)base.Dir * (Math.Sin((double)base.WalkPos) + (double)noise)) * 30f * animStrength);
			}
			if (tail != null && connectedToBaseLimb.Contains(tail))
			{
				tail.body.ApplyTorque((float)(Math.Sqrt((double)tail.Mass) * (double)(-(double)base.Dir) * (Math.Sin((double)base.WalkPos) + (double)noise)) * 30f * animStrength);
			}
			base.WalkPos += deltaTime * 10f * animStrength;
			Vector2 centerOfMass = base.GetCenterOfMass();
			foreach (Limb limb in base.Limbs)
			{
				if (connectedToBaseLimb.Contains(limb) && limb.type != LimbType.Head && limb.type != LimbType.Tail && !limb.IsSevered && limb.body.Enabled)
				{
					if (limb.Mass <= 0f)
					{
						string errorMsg = string.Concat(new string[]
						{
							"Creature death animation error: invalid limb mass on character \"",
							this.character.SpeciesName.ToString(),
							"\" (type: ",
							limb.type.ToString(),
							", mass: ",
							limb.Mass.ToString(),
							")"
						});
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("FishAnimController.UpdateDying:InvalidMass" + this.character.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						this.deathAnimTimer = this.deathAnimDuration;
						return;
					}
					Vector2 diff = centerOfMass - limb.SimPosition;
					if (!MathUtils.IsValid(diff))
					{
						string[] array = new string[5];
						array[0] = "Creature death animation error: invalid diff (center of mass: ";
						int num = 1;
						Vector2 vector = centerOfMass;
						array[num] = vector.ToString();
						array[2] = ", limb position: ";
						array[3] = limb.SimPosition.ToString();
						array[4] = ")";
						string errorMsg2 = string.Concat(array);
						DebugConsole.ThrowError(errorMsg2, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("FishAnimController.UpdateDying:InvalidDiff" + this.character.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
						this.deathAnimTimer = this.deathAnimDuration;
						return;
					}
					limb.body.ApplyForce(diff * (float)(Math.Sin((double)base.WalkPos) * Math.Sqrt((double)limb.Mass)) * 30f * animStrength, 10f);
				}
			}
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x000AD3B2 File Offset: 0x000AB5B2
		private void SmoothRotateWithoutWrapping(Limb limb, float angle, Limb referenceLimb, float torque)
		{
			angle = referenceLimb.body.WrapAngleToSameNumberOfRevolutions(angle);
			if (limb != null)
			{
				limb.body.SmoothRotate(angle, torque, false);
			}
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x000AD3D4 File Offset: 0x000AB5D4
		public override void Flip()
		{
			base.Flip();
			foreach (Limb i in base.Limbs)
			{
				if (!i.IsSevered && i.DoesFlip && this.RagdollParams.IsSpritesheetOrientationHorizontal)
				{
					i.body.SetTransformIgnoreContacts(i.SimPosition, i.body.Rotation + 3.1415927f * base.Dir, true);
				}
			}
		}

		// Token: 0x060013E5 RID: 5093 RVA: 0x000AD448 File Offset: 0x000AB648
		public void Mirror(bool lerp = true)
		{
			Vector2 centerOfMass = base.GetCenterOfMass();
			foreach (Limb i in base.Limbs)
			{
				if (!i.IsSevered)
				{
					float rotation = i.body.Rotation;
					if (i.DoesMirror)
					{
						if (this.RagdollParams.IsSpritesheetOrientationHorizontal)
						{
							rotation = -(i.body.Rotation + 3.1415927f);
						}
						else
						{
							rotation = -i.body.Rotation;
						}
					}
					base.TrySetLimbPosition(i, centerOfMass, new Vector2(centerOfMass.X - (i.SimPosition.X - centerOfMass.X), i.SimPosition.Y), rotation, lerp, true);
					i.body.PositionSmoothingFactor = new float?(0.8f);
				}
			}
			if (this.character.SelectedCharacter != null && this.CanDrag(this.character.SelectedCharacter))
			{
				float diff = this.character.SelectedCharacter.SimPosition.X - centerOfMass.X;
				if (diff < 100f)
				{
					this.character.SelectedCharacter.AnimController.SetPosition(new Vector2(centerOfMass.X - diff, this.character.SelectedCharacter.SimPosition.Y), lerp, true, false, true);
				}
			}
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x000AD599 File Offset: 0x000AB799
		[CompilerGenerated]
		private void <UpdateAnim>g__ResetState|60_0()
		{
			this.wasAiming = this.aiming;
			this.aiming = false;
			this.wasAimingMelee = this.aimingMelee;
			this.aimingMelee = false;
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x000AD5C4 File Offset: 0x000AB7C4
		[CompilerGenerated]
		internal static bool <DragCharacter>g__CanBeSevered|63_0(LimbJoint j)
		{
			if (!j.IsSevered && j.CanBeSevered)
			{
				Limb limb = j.LimbA;
				if (limb != null && !limb.IsSevered)
				{
					limb = j.LimbB;
					return limb != null && !limb.IsSevered;
				}
			}
			return false;
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x000AD60C File Offset: 0x000AB80C
		[CompilerGenerated]
		private void <UpdateSineAnim>g__RotateTail|65_0(Limb tail, ref FishAnimController.<>c__DisplayClass65_0 A_2, ref FishAnimController.<>c__DisplayClass65_1 A_3)
		{
			if (tail == null)
			{
				return;
			}
			float? mainLimbTargetAngle = null;
			if (A_2.mainLimb.type == LimbType.Torso)
			{
				mainLimbTargetAngle = this.TorsoAngle;
			}
			else if (A_2.mainLimb.type == LimbType.Head)
			{
				mainLimbTargetAngle = this.HeadAngle;
			}
			float torque = this.TailTorque;
			float maxMultiplier = this.CurrentSwimParams.TailTorqueMultiplier;
			if (mainLimbTargetAngle != null && maxMultiplier > 1f)
			{
				float diff = Math.Abs(A_2.mainLimb.Rotation - tail.Rotation);
				float offset = Math.Abs(mainLimbTargetAngle.Value - this.TailAngle.Value);
				torque *= MathHelper.Lerp(1f, maxMultiplier, MathUtils.InverseLerp(0f, 1.5707964f, diff - offset));
			}
			this.SmoothRotateWithoutWrapping(tail, A_3.movementAngle + this.TailAngle.Value * base.Dir, A_2.mainLimb, torque);
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x000AD6FC File Offset: 0x000AB8FC
		[CompilerGenerated]
		private void <UpdateSineAnim>g__RotateTail|65_1(Limb tail, ref FishAnimController.<>c__DisplayClass65_0 A_2)
		{
			if (tail != null)
			{
				tail.body.SmoothRotate(this.TailAngle.Value * base.Dir, this.TailTorque, true);
			}
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x000AD734 File Offset: 0x000AB934
		[CompilerGenerated]
		private void <UpdateWalkAnim>g__RotateTail|66_0(Limb tail, ref FishAnimController.<>c__DisplayClass66_0 A_2)
		{
			if (tail != null)
			{
				this.SmoothRotateWithoutWrapping(tail, A_2.movementAngle + this.TailAngle.Value * base.Dir, A_2.mainLimb, this.TailTorque);
			}
		}

		// Token: 0x04000952 RID: 2386
		private FishRagdollParams _ragdollParams;

		// Token: 0x04000953 RID: 2387
		private FishWalkParams _fishWalkParams;

		// Token: 0x04000954 RID: 2388
		private FishRunParams _fishRunParams;

		// Token: 0x04000955 RID: 2389
		private FishSwimSlowParams _fishSwimSlowParams;

		// Token: 0x04000956 RID: 2390
		private FishSwimFastParams _fishSwimFastParams;

		// Token: 0x04000957 RID: 2391
		private float flipTimer;

		// Token: 0x04000958 RID: 2392
		private float flipCooldown;

		// Token: 0x04000959 RID: 2393
		private float eatTimer;

		// Token: 0x0400095A RID: 2394
		public bool Reverse;

		// Token: 0x02000858 RID: 2136
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002F60 RID: 12128
			public static Func<LimbJoint, bool> <0>__CanBeSevered;
		}
	}
}
