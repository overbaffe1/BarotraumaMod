using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A7 RID: 423
	internal abstract class AnimController : Ragdoll, ISerializableEntity
	{
		// Token: 0x17000C82 RID: 3202
		// (get) Token: 0x06003042 RID: 12354 RVA: 0x001F95F6 File Offset: 0x001F77F6
		// (set) Token: 0x06003043 RID: 12355 RVA: 0x001F95FE File Offset: 0x001F77FE
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000C83 RID: 3203
		// (get) Token: 0x06003044 RID: 12356 RVA: 0x001F9607 File Offset: 0x001F7807
		public string Name
		{
			get
			{
				return "AnimController";
			}
		}

		// Token: 0x17000C84 RID: 3204
		// (get) Token: 0x06003045 RID: 12357 RVA: 0x001F960E File Offset: 0x001F780E
		// (set) Token: 0x06003046 RID: 12358 RVA: 0x001F9616 File Offset: 0x001F7816
		public Vector2 RightHandIKPos { get; protected set; }

		// Token: 0x17000C85 RID: 3205
		// (get) Token: 0x06003047 RID: 12359 RVA: 0x001F961F File Offset: 0x001F781F
		// (set) Token: 0x06003048 RID: 12360 RVA: 0x001F9627 File Offset: 0x001F7827
		public Vector2 LeftHandIKPos { get; protected set; }

		// Token: 0x17000C86 RID: 3206
		// (get) Token: 0x06003049 RID: 12361 RVA: 0x001F9630 File Offset: 0x001F7830
		public bool IsAiming
		{
			get
			{
				return this.wasAiming;
			}
		}

		// Token: 0x17000C87 RID: 3207
		// (get) Token: 0x0600304A RID: 12362 RVA: 0x001F9638 File Offset: 0x001F7838
		public bool IsAimingMelee
		{
			get
			{
				return this.wasAimingMelee;
			}
		}

		// Token: 0x17000C88 RID: 3208
		// (get) Token: 0x0600304B RID: 12363 RVA: 0x001F9640 File Offset: 0x001F7840
		protected bool Aiming
		{
			get
			{
				return this.aiming || this.aimingMelee || ((double)this.FlipLockTime > Timing.TotalTime && this.character.IsKeyDown(InputType.Aim));
			}
		}

		// Token: 0x17000C89 RID: 3209
		// (get) Token: 0x0600304C RID: 12364 RVA: 0x001F9670 File Offset: 0x001F7870
		public float ArmLength
		{
			get
			{
				return this.upperArmLength + this.forearmLength;
			}
		}

		// Token: 0x17000C8A RID: 3210
		// (get) Token: 0x0600304D RID: 12365
		// (set) Token: 0x0600304E RID: 12366
		public abstract GroundedMovementParams WalkParams { get; set; }

		// Token: 0x17000C8B RID: 3211
		// (get) Token: 0x0600304F RID: 12367
		// (set) Token: 0x06003050 RID: 12368
		public abstract GroundedMovementParams RunParams { get; set; }

		// Token: 0x17000C8C RID: 3212
		// (get) Token: 0x06003051 RID: 12369
		// (set) Token: 0x06003052 RID: 12370
		public abstract SwimParams SwimSlowParams { get; set; }

		// Token: 0x17000C8D RID: 3213
		// (get) Token: 0x06003053 RID: 12371
		// (set) Token: 0x06003054 RID: 12372
		public abstract SwimParams SwimFastParams { get; set; }

		// Token: 0x17000C8E RID: 3214
		// (get) Token: 0x06003055 RID: 12373 RVA: 0x001F967F File Offset: 0x001F787F
		public AnimationParams CurrentAnimationParams
		{
			get
			{
				if (this.ForceSelectAnimationType != AnimationType.NotDefined)
				{
					return this.GetAnimationParamsFromType(this.ForceSelectAnimationType);
				}
				if (!base.InWater && this.CanWalk)
				{
					return this.CurrentGroundedParams;
				}
				return this.CurrentSwimParams;
			}
		}

		// Token: 0x17000C8F RID: 3215
		// (get) Token: 0x06003056 RID: 12374 RVA: 0x001F96B3 File Offset: 0x001F78B3
		// (set) Token: 0x06003057 RID: 12375 RVA: 0x001F96BB File Offset: 0x001F78BB
		public AnimationType ForceSelectAnimationType { get; set; }

		// Token: 0x17000C90 RID: 3216
		// (get) Token: 0x06003058 RID: 12376 RVA: 0x001F96C4 File Offset: 0x001F78C4
		public GroundedMovementParams CurrentGroundedParams
		{
			get
			{
				if (this.ForceSelectAnimationType != AnimationType.NotDefined)
				{
					return this.GetAnimationParamsFromType(this.ForceSelectAnimationType) as GroundedMovementParams;
				}
				if (!this.CanWalk)
				{
					return null;
				}
				HumanoidAnimController humanAnimController = this as HumanoidAnimController;
				if (humanAnimController != null && humanAnimController.Crouching)
				{
					return humanAnimController.HumanCrouchParams;
				}
				if (!this.IsMovingFast)
				{
					return this.WalkParams;
				}
				return this.RunParams;
			}
		}

		// Token: 0x17000C91 RID: 3217
		// (get) Token: 0x06003059 RID: 12377 RVA: 0x001F9723 File Offset: 0x001F7923
		public SwimParams CurrentSwimParams
		{
			get
			{
				if (this.ForceSelectAnimationType != AnimationType.NotDefined)
				{
					return this.GetAnimationParamsFromType(this.ForceSelectAnimationType) as SwimParams;
				}
				if (!this.IsMovingFast)
				{
					return this.SwimSlowParams;
				}
				return this.SwimFastParams;
			}
		}

		// Token: 0x17000C92 RID: 3218
		// (get) Token: 0x0600305A RID: 12378 RVA: 0x001F9754 File Offset: 0x001F7954
		public bool CanWalk
		{
			get
			{
				return this.RagdollParams.CanWalk;
			}
		}

		// Token: 0x17000C93 RID: 3219
		// (get) Token: 0x0600305B RID: 12379 RVA: 0x001F9764 File Offset: 0x001F7964
		public bool IsMovingBackwards
		{
			get
			{
				if (!base.InWater && Math.Sign(this.targetMovement.X) == -Math.Sign(base.Dir))
				{
					FishGroundedParams fishGroundedParams = this.CurrentAnimationParams as FishGroundedParams;
					if (fishGroundedParams == null || fishGroundedParams.Flip)
					{
						return this.Anim != AnimController.Animation.Climbing;
					}
				}
				return false;
			}
		}

		// Token: 0x17000C94 RID: 3220
		// (get) Token: 0x0600305C RID: 12380 RVA: 0x001F97BC File Offset: 0x001F79BC
		public bool IsMovingFast
		{
			get
			{
				if (base.InWater || !this.CanWalk)
				{
					return base.TargetMovement.LengthSquared() > MathUtils.Pow2(this.SwimSlowParams.MovementSpeed + 0.0001f);
				}
				float movementSpeed = this.IsClimbing ? base.TargetMovement.Y : base.TargetMovement.X;
				return Math.Abs(movementSpeed) > (this.WalkParams.MovementSpeed + this.RunParams.MovementSpeed) / 2f;
			}
		}

		// Token: 0x17000C95 RID: 3221
		// (get) Token: 0x0600305D RID: 12381 RVA: 0x001F9848 File Offset: 0x001F7A48
		public List<AnimationParams> AllAnimParams
		{
			get
			{
				if (this.CanWalk)
				{
					List<AnimationParams> anims = new List<AnimationParams>
					{
						this.WalkParams,
						this.RunParams,
						this.SwimSlowParams,
						this.SwimFastParams
					};
					HumanoidAnimController humanAnimController = this as HumanoidAnimController;
					if (humanAnimController != null)
					{
						anims.Add(humanAnimController.HumanCrouchParams);
					}
					return anims;
				}
				return new List<AnimationParams>
				{
					this.SwimSlowParams,
					this.SwimFastParams
				};
			}
		}

		// Token: 0x17000C96 RID: 3222
		// (get) Token: 0x0600305E RID: 12382 RVA: 0x001F98C8 File Offset: 0x001F7AC8
		public bool IsUsingItem
		{
			get
			{
				return this.Anim == AnimController.Animation.UsingItem || this.Anim == AnimController.Animation.UsingItemWhileClimbing;
			}
		}

		// Token: 0x17000C97 RID: 3223
		// (get) Token: 0x0600305F RID: 12383 RVA: 0x001F98DE File Offset: 0x001F7ADE
		public bool IsClimbing
		{
			get
			{
				return this.Anim == AnimController.Animation.Climbing || this.Anim == AnimController.Animation.UsingItemWhileClimbing;
			}
		}

		// Token: 0x17000C98 RID: 3224
		// (get) Token: 0x06003060 RID: 12384 RVA: 0x001F98F4 File Offset: 0x001F7AF4
		public Vector2 AimSourceWorldPos
		{
			get
			{
				Vector2 sourcePos = this.character.AnimController.AimSourcePos;
				if (this.character.Submarine != null)
				{
					sourcePos += this.character.Submarine.Position;
				}
				return sourcePos;
			}
		}

		// Token: 0x17000C99 RID: 3225
		// (get) Token: 0x06003061 RID: 12385 RVA: 0x001F9937 File Offset: 0x001F7B37
		public Vector2 AimSourcePos
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.AimSourceSimPos);
			}
		}

		// Token: 0x17000C9A RID: 3226
		// (get) Token: 0x06003062 RID: 12386 RVA: 0x001F9944 File Offset: 0x001F7B44
		public virtual Vector2 AimSourceSimPos
		{
			get
			{
				return base.Collider.SimPosition;
			}
		}

		// Token: 0x06003063 RID: 12387 RVA: 0x001F9954 File Offset: 0x001F7B54
		protected float? GetValidOrNull(AnimationParams p, float? v)
		{
			if (p == null)
			{
				return null;
			}
			if (v == null)
			{
				return null;
			}
			if (!MathUtils.IsValid(v.Value))
			{
				return null;
			}
			return new float?(v.Value);
		}

		// Token: 0x06003064 RID: 12388 RVA: 0x001F99A8 File Offset: 0x001F7BA8
		protected Vector2? GetValidOrNull(AnimationParams p, Vector2 v)
		{
			if (p == null)
			{
				return null;
			}
			return new Vector2?(v);
		}

		// Token: 0x17000C9B RID: 3227
		// (get) Token: 0x06003065 RID: 12389 RVA: 0x001F99C8 File Offset: 0x001F7BC8
		public override float? HeadPosition
		{
			get
			{
				AnimationParams currentGroundedParams = this.CurrentGroundedParams;
				GroundedMovementParams currentGroundedParams2 = this.CurrentGroundedParams;
				return this.GetValidOrNull(currentGroundedParams, ((currentGroundedParams2 != null) ? new float?(currentGroundedParams2.HeadPosition) : null) * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x17000C9C RID: 3228
		// (get) Token: 0x06003066 RID: 12390 RVA: 0x001F9A30 File Offset: 0x001F7C30
		public override float? TorsoPosition
		{
			get
			{
				AnimationParams currentGroundedParams = this.CurrentGroundedParams;
				GroundedMovementParams currentGroundedParams2 = this.CurrentGroundedParams;
				return this.GetValidOrNull(currentGroundedParams, ((currentGroundedParams2 != null) ? new float?(currentGroundedParams2.TorsoPosition) : null) * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x17000C9D RID: 3229
		// (get) Token: 0x06003067 RID: 12391 RVA: 0x001F9A98 File Offset: 0x001F7C98
		public override float? HeadAngle
		{
			get
			{
				AnimationParams currentAnimationParams = this.CurrentAnimationParams;
				AnimationParams currentAnimationParams2 = this.CurrentAnimationParams;
				return this.GetValidOrNull(currentAnimationParams, (currentAnimationParams2 != null) ? new float?(currentAnimationParams2.HeadAngleInRadians) : null);
			}
		}

		// Token: 0x17000C9E RID: 3230
		// (get) Token: 0x06003068 RID: 12392 RVA: 0x001F9AD0 File Offset: 0x001F7CD0
		public override float? TorsoAngle
		{
			get
			{
				AnimationParams currentAnimationParams = this.CurrentAnimationParams;
				AnimationParams currentAnimationParams2 = this.CurrentAnimationParams;
				return this.GetValidOrNull(currentAnimationParams, (currentAnimationParams2 != null) ? new float?(currentAnimationParams2.TorsoAngleInRadians) : null);
			}
		}

		// Token: 0x17000C9F RID: 3231
		// (get) Token: 0x06003069 RID: 12393 RVA: 0x001F9B08 File Offset: 0x001F7D08
		public virtual Vector2? StepSize
		{
			get
			{
				return this.GetValidOrNull(this.CurrentGroundedParams, this.CurrentGroundedParams.StepSize * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x17000CA0 RID: 3232
		// (get) Token: 0x0600306A RID: 12394 RVA: 0x001F9B31 File Offset: 0x001F7D31
		// (set) Token: 0x0600306B RID: 12395 RVA: 0x001F9B39 File Offset: 0x001F7D39
		public bool AnimationTestPose { get; set; }

		// Token: 0x17000CA1 RID: 3233
		// (get) Token: 0x0600306C RID: 12396 RVA: 0x001F9B42 File Offset: 0x001F7D42
		// (set) Token: 0x0600306D RID: 12397 RVA: 0x001F9B4A File Offset: 0x001F7D4A
		public float WalkPos { get; protected set; }

		// Token: 0x0600306E RID: 12398 RVA: 0x001F9B54 File Offset: 0x001F7D54
		public AnimController(Character character, string seed, RagdollParams ragdollParams = null) : base(character, seed, ragdollParams)
		{
			this.SerializableProperties = SerializableProperty.GetProperties(this);
		}

		// Token: 0x0600306F RID: 12399 RVA: 0x001F9BAE File Offset: 0x001F7DAE
		public void UpdateAnimations(float deltaTime)
		{
			this.UpdateTemporaryAnimations();
			this.UpdateAnim(deltaTime);
			this.CheckRopeState();
		}

		// Token: 0x06003070 RID: 12400
		protected abstract void UpdateAnim(float deltaTime);

		// Token: 0x06003071 RID: 12401
		public abstract void DragCharacter(Character target, float deltaTime);

		// Token: 0x06003072 RID: 12402 RVA: 0x001F9BC4 File Offset: 0x001F7DC4
		public virtual float GetSpeed(AnimationType type)
		{
			GroundedMovementParams movementParams;
			switch (type)
			{
			case AnimationType.Walk:
				if (!this.CanWalk)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.character.SpeciesName);
					defaultInterpolatedStringHandler.AppendLiteral(" cannot walk!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return 0f;
				}
				movementParams = this.WalkParams;
				break;
			case AnimationType.Run:
				if (!this.CanWalk)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.character.SpeciesName);
					defaultInterpolatedStringHandler2.AppendLiteral(" cannot run!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					return 0f;
				}
				movementParams = this.RunParams;
				break;
			case AnimationType.SwimSlow:
				return this.SwimSlowParams.MovementSpeed;
			case AnimationType.SwimFast:
				return this.SwimFastParams.MovementSpeed;
			default:
				throw new NotImplementedException(type.ToString());
			}
			if (!this.IsMovingBackwards)
			{
				return movementParams.MovementSpeed;
			}
			return movementParams.MovementSpeed * movementParams.BackwardsMovementMultiplier;
		}

		// Token: 0x06003073 RID: 12403 RVA: 0x001F9CD4 File Offset: 0x001F7ED4
		public float GetCurrentSpeed(bool useMaxSpeed)
		{
			AnimationType animType;
			if (base.InWater || !this.CanWalk)
			{
				if (useMaxSpeed)
				{
					animType = AnimationType.SwimFast;
				}
				else
				{
					animType = AnimationType.SwimSlow;
				}
			}
			else if (useMaxSpeed)
			{
				animType = AnimationType.Run;
			}
			else
			{
				HumanoidAnimController humanAnimController = this as HumanoidAnimController;
				if (humanAnimController != null && humanAnimController.Crouching)
				{
					animType = AnimationType.Crouch;
				}
				else
				{
					animType = AnimationType.Walk;
				}
			}
			return this.GetSpeed(animType);
		}

		// Token: 0x06003074 RID: 12404 RVA: 0x001F9D24 File Offset: 0x001F7F24
		public AnimationParams GetAnimationParamsFromType(AnimationType type)
		{
			switch (type)
			{
			case AnimationType.Walk:
				if (!this.CanWalk)
				{
					return null;
				}
				return this.WalkParams;
			case AnimationType.Run:
				if (!this.CanWalk)
				{
					return null;
				}
				return this.RunParams;
			case AnimationType.SwimSlow:
				return this.SwimSlowParams;
			case AnimationType.SwimFast:
				return this.SwimFastParams;
			case AnimationType.Crouch:
			{
				HumanoidAnimController humanAnimController = this as HumanoidAnimController;
				if (humanAnimController != null)
				{
					return humanAnimController.HumanCrouchParams;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Animation params of type ");
				defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(type);
				defaultInterpolatedStringHandler.AppendLiteral(" not implemented for non-humanoids!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return null;
			}
			}
			return null;
		}

		// Token: 0x06003075 RID: 12405 RVA: 0x001F9DCF File Offset: 0x001F7FCF
		public float GetHeightFromFloor()
		{
			return base.GetColliderBottom().Y - base.FloorY;
		}

		// Token: 0x17000CA2 RID: 3234
		// (get) Token: 0x06003076 RID: 12406 RVA: 0x001F9DE3 File Offset: 0x001F7FE3
		public bool IsAboveFloor
		{
			get
			{
				return this.GetHeightFromFloor() > -0.1f;
			}
		}

		// Token: 0x17000CA3 RID: 3235
		// (get) Token: 0x06003077 RID: 12407 RVA: 0x001F9DF2 File Offset: 0x001F7FF2
		// (set) Token: 0x06003078 RID: 12408 RVA: 0x001F9DFA File Offset: 0x001F7FFA
		public float FlipLockTime { get; private set; }

		// Token: 0x06003079 RID: 12409 RVA: 0x001F9E03 File Offset: 0x001F8003
		public void LockFlipping(float time = 0.2f)
		{
			this.FlipLockTime = (float)Timing.TotalTime + time;
		}

		// Token: 0x0600307A RID: 12410 RVA: 0x001F9E14 File Offset: 0x001F8014
		protected void UpdateConstantTorque(float deltaTime)
		{
			foreach (Limb limb in base.Limbs)
			{
				if (!limb.IsSevered && Math.Abs(limb.Params.ConstantTorque) > 0f)
				{
					float movementFactor = Math.Max(this.character.AnimController.Collider.LinearVelocity.Length() * 0.5f, 1f);
					limb.body.SmoothRotate(base.MainLimb.Rotation + MathHelper.ToRadians(limb.Params.ConstantAngle) * base.Dir, limb.Mass * limb.Params.ConstantTorque * movementFactor, true);
				}
			}
		}

		// Token: 0x0600307B RID: 12411 RVA: 0x001F9ED4 File Offset: 0x001F80D4
		protected void UpdateBlink(float deltaTime)
		{
			foreach (Limb limb in base.Limbs)
			{
				if (!limb.IsSevered && limb.Params.BlinkFrequency > 0f && (limb.InWater || !limb.Params.OnlyBlinkInWater))
				{
					limb.UpdateBlink(deltaTime, base.MainLimb.Rotation);
				}
			}
		}

		// Token: 0x0600307C RID: 12412 RVA: 0x001F9F3C File Offset: 0x001F813C
		public void UpdateUseItem(bool allowMovement, Vector2 handWorldPos)
		{
			this.useItemTimer = 0.05f;
			this.StartUsingItem();
			bool flag = !allowMovement;
			bool flag2 = flag;
			if (flag2)
			{
				Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
				Controller controller = (selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Controller>() : null;
				bool flag3;
				if (controller != null)
				{
					Direction direction = controller.Direction;
					if (direction - Direction.Left <= 1)
					{
						flag3 = true;
						goto IL_54;
					}
				}
				flag3 = false;
				IL_54:
				flag2 = !flag3;
			}
			if (flag2)
			{
				base.TargetMovement = Vector2.Zero;
				this.TargetDir = ((handWorldPos.X > this.character.WorldPosition.X) ? Direction.Right : Direction.Left);
				if (base.InWater)
				{
					float sqrDist = Vector2.DistanceSquared(this.character.WorldPosition, handWorldPos);
					if (sqrDist > MathUtils.Pow(ConvertUnits.ToDisplayUnits(this.upperArmLength + this.forearmLength), 2f))
					{
						base.TargetMovement = this.<UpdateUseItem>g__GetTargetMovement|115_0(Vector2.Normalize(handWorldPos - this.character.WorldPosition));
					}
				}
				else
				{
					float distX = Math.Abs(handWorldPos.X - this.character.WorldPosition.X);
					if (distX > ConvertUnits.ToDisplayUnits(this.upperArmLength + this.forearmLength))
					{
						base.TargetMovement = this.<UpdateUseItem>g__GetTargetMovement|115_0(Vector2.UnitX * (float)Math.Sign(handWorldPos.X - this.character.WorldPosition.X));
					}
				}
			}
			if (!this.character.Enabled)
			{
				return;
			}
			Vector2 handSimPos = ConvertUnits.ToSimUnits(handWorldPos);
			if (this.character.Submarine != null)
			{
				handSimPos -= this.character.Submarine.SimPosition;
			}
			LimbJoint limbJoint = this.rightShoulder;
			Vector2 vector;
			if (limbJoint == null)
			{
				LimbJoint limbJoint2 = this.leftShoulder;
				vector = ((limbJoint2 != null) ? limbJoint2.WorldAnchorA : base.MainLimb.SimPosition);
			}
			else
			{
				vector = limbJoint.WorldAnchorA;
			}
			Vector2 refPos = vector;
			Vector2 diff = handSimPos - refPos;
			float dist = diff.Length();
			float maxDist = this.ArmLength * 0.9f;
			if (dist > maxDist)
			{
				handSimPos = refPos + diff / dist * maxDist;
			}
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			if (leftHand != null)
			{
				leftHand.Disabled = true;
				leftHand.PullJointEnabled = true;
				leftHand.PullJointWorldAnchorB = handSimPos;
			}
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			if (rightHand != null)
			{
				rightHand.Disabled = true;
				rightHand.PullJointEnabled = true;
				rightHand.PullJointWorldAnchorB = handSimPos;
			}
			if (!this.inWater && this.character.WorldPosition.Y - handWorldPos.Y > ConvertUnits.ToDisplayUnits(this.CurrentGroundedParams.TorsoPosition) / 4f)
			{
				HumanoidAnimController humanoidAnimController = this as HumanoidAnimController;
				if (humanoidAnimController != null)
				{
					humanoidAnimController.Crouch();
					humanoidAnimController.ForceSelectAnimationType = AnimationType.Crouch;
				}
			}
		}

		// Token: 0x0600307D RID: 12413 RVA: 0x001FA1E4 File Offset: 0x001F83E4
		public void Grab(Vector2 rightHandPos, Vector2 leftHandPos)
		{
			for (int i = 0; i < 2; i++)
			{
				Limb pullLimb = (i == 0) ? base.GetLimb(LimbType.LeftHand, true, false, false) : base.GetLimb(LimbType.RightHand, true, false, false);
				pullLimb.Disabled = true;
				pullLimb.PullJointEnabled = true;
				pullLimb.PullJointWorldAnchorB = ((i == 0) ? rightHandPos : leftHandPos);
				pullLimb.PullJointMaxForce = 500f;
			}
		}

		// Token: 0x0600307E RID: 12414 RVA: 0x001FA240 File Offset: 0x001F8440
		public void HoldItem(float deltaTime, Item item, Vector2[] handlePos, Vector2 itemPos, bool aim, float holdAngle, float itemAngleRelativeToHoldAngle = 0f, bool aimMelee = false, Vector2? targetPos = null)
		{
			this.aimingMelee = aimMelee;
			if (this.character.Stun > 0f || this.character.IsIncapacitated)
			{
				aim = false;
			}
			Matrix itemTransform = Matrix.CreateRotationZ(item.body.Rotation);
			this.transformedHandlePos[0] = Vector2.Transform(handlePos[0], itemTransform);
			this.transformedHandlePos[1] = Vector2.Transform(handlePos[1], itemTransform);
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false) ?? base.MainLimb;
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			Item selectedItem = this.character.SelectedItem;
			Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
			bool usingController = controller != null && !controller.AllowAiming;
			if (!usingController)
			{
				Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
				controller = ((selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Controller>() : null);
				usingController = (controller != null && !controller.AllowAiming);
			}
			bool isClimbing = this.character.IsClimbing && Math.Abs(this.character.AnimController.TargetMovement.Y) > 0.01f;
			Holdable holdable = item.GetComponent<Holdable>();
			float torsoRotation = torso.Rotation;
			CharacterInventory inventory = this.character.Inventory;
			Item rightHandItem = (inventory != null) ? inventory.GetItemInLimbSlot(InvSlotType.RightHand) : null;
			bool equippedInRightHand = rightHandItem == item && rightHand != null && !rightHand.IsSevered;
			CharacterInventory inventory2 = this.character.Inventory;
			Item leftHandItem = (inventory2 != null) ? inventory2.GetItemInLimbSlot(InvSlotType.LeftHand) : null;
			bool equippedInLeftHand = leftHandItem == item && leftHand != null && !leftHand.IsSevered;
			float itemAngle;
			if (aim && !isClimbing && !usingController && this.character.Stun <= 0f && itemPos != Vector2.Zero && !this.character.IsIncapacitated)
			{
				Vector2 value = targetPos.GetValueOrDefault();
				if (targetPos == null)
				{
					value = ConvertUnits.ToSimUnits(this.character.SmoothedCursorPosition);
					targetPos = new Vector2?(value);
				}
				Vector2 diff = holdable.Aimable ? ((targetPos.Value - this.AimSourceSimPos) * base.Dir) : MathUtils.RotatePoint(Vector2.UnitX, torsoRotation);
				holdAngle = MathUtils.VectorToAngle(new Vector2(diff.X, diff.Y * base.Dir)) - torsoRotation * base.Dir;
				holdAngle += this.GetAimWobble(rightHand, leftHand, item);
				itemAngle = torsoRotation + holdAngle * base.Dir;
				if (holdable.ControlPose)
				{
					bool flag3;
					if (equippedInLeftHand && rightHandItem != item)
					{
						bool? flag;
						if (rightHandItem == null)
						{
							flag = null;
						}
						else
						{
							Holdable component = rightHandItem.GetComponent<Holdable>();
							flag = ((component != null) ? new bool?(component.ControlPose) : null);
						}
						bool? flag2 = flag;
						flag3 = flag2.GetValueOrDefault();
					}
					else
					{
						flag3 = false;
					}
					if (!flag3 && base.TargetMovement == Vector2.Zero && this.inWater)
					{
						torso.body.AngularVelocity -= torso.body.AngularVelocity * 0.1f;
						torso.body.ApplyForce(torso.body.LinearVelocity * -0.5f, 64f);
					}
					this.aiming = true;
				}
			}
			else if (holdable.UseHandRotationForHoldAngle)
			{
				if (equippedInRightHand)
				{
					itemAngle = rightHand.Rotation + holdAngle * base.Dir;
				}
				else if (equippedInLeftHand)
				{
					itemAngle = leftHand.Rotation + holdAngle * base.Dir;
				}
				else
				{
					itemAngle = torsoRotation + holdAngle * base.Dir;
				}
			}
			else
			{
				itemAngle = torsoRotation + holdAngle * base.Dir;
			}
			if (this.rightShoulder == null)
			{
				return;
			}
			Vector2 transformedHoldPos = this.rightShoulder.WorldAnchorA;
			if (itemPos == Vector2.Zero || isClimbing || usingController)
			{
				if (equippedInRightHand)
				{
					transformedHoldPos = rightHand.PullJointWorldAnchorA - this.transformedHandlePos[0];
					itemAngle = rightHand.Rotation + (holdAngle - rightHand.Params.GetSpriteOrientation() + 1.5707964f) * base.Dir;
				}
				else if (equippedInLeftHand)
				{
					transformedHoldPos = leftHand.PullJointWorldAnchorA - this.transformedHandlePos[1];
					itemAngle = leftHand.Rotation + (holdAngle - leftHand.Params.GetSpriteOrientation() + 1.5707964f) * base.Dir;
				}
			}
			else
			{
				if (equippedInRightHand)
				{
					transformedHoldPos = this.rightShoulder.WorldAnchorA;
					rightHand.Disabled = true;
				}
				if (equippedInLeftHand)
				{
					if (this.leftShoulder == null)
					{
						return;
					}
					transformedHoldPos = this.leftShoulder.WorldAnchorA;
					leftHand.Disabled = true;
				}
				itemPos.X *= base.Dir;
				transformedHoldPos += Vector2.Transform(itemPos, Matrix.CreateRotationZ(itemAngle));
			}
			item.body.ResetDynamics();
			Vector2 currItemPos = equippedInRightHand ? (rightHand.PullJointWorldAnchorA - this.transformedHandlePos[0]) : (leftHand.PullJointWorldAnchorA - this.transformedHandlePos[1]);
			if (!MathUtils.IsValid(currItemPos))
			{
				string[] array = new string[22];
				array[0] = "Attempted to move the item \"";
				array[1] = ((item != null) ? item.ToString() : null);
				array[2] = "\" to an invalid position in HumanidAnimController.HoldItem: ";
				int num = 3;
				Vector2 value = currItemPos;
				array[num] = value.ToString();
				array[4] = ", rightHandPos: ";
				array[5] = rightHand.PullJointWorldAnchorA.ToString();
				array[6] = ", leftHandPos: ";
				array[7] = leftHand.PullJointWorldAnchorA.ToString();
				array[8] = ", handlePos[0]: ";
				int num2 = 9;
				value = handlePos[0];
				array[num2] = value.ToString();
				array[10] = ", handlePos[1]: ";
				int num3 = 11;
				value = handlePos[1];
				array[num3] = value.ToString();
				array[12] = ", transformedHandlePos[0]: ";
				int num4 = 13;
				value = this.transformedHandlePos[0];
				array[num4] = value.ToString();
				array[14] = ", transformedHandlePos[1]:";
				int num5 = 15;
				value = this.transformedHandlePos[1];
				array[num5] = value.ToString();
				array[16] = ", item pos: ";
				array[17] = item.SimPosition.ToString();
				array[18] = ", itemAngle: ";
				array[19] = itemAngle.ToString();
				array[20] = ", collider pos: ";
				array[21] = this.character.SimPosition.ToString();
				string errorMsg = string.Concat(array);
				DebugConsole.Log(errorMsg);
				GameAnalyticsManager.AddErrorEventOnce("HumanoidAnimController.HoldItem:InvalidPos:" + this.character.Name + item.Name, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			float targetAngle = MathUtils.WrapAngleTwoPi(itemAngle + itemAngleRelativeToHoldAngle * base.Dir);
			float currentRotation = MathUtils.WrapAngleTwoPi(item.body.Rotation);
			float itemRotation = MathHelper.SmoothStep(currentRotation, targetAngle, deltaTime * 25f);
			if (this.previousDirection != this.dir || Math.Abs(targetAngle - currentRotation) > 3.1415927f)
			{
				itemRotation = targetAngle;
			}
			item.SetTransform(currItemPos, itemRotation, true, false, null);
			this.previousDirection = this.dir;
			if (holdable.Pusher != null)
			{
				if (this.character.Stun > 0f || this.character.IsIncapacitated)
				{
					holdable.Pusher.Enabled = false;
				}
				else if (!holdable.Pusher.Enabled)
				{
					holdable.Pusher.Enabled = true;
					holdable.Pusher.ResetDynamics();
					holdable.Pusher.SetTransform(currItemPos, itemAngle, true);
				}
				else
				{
					holdable.Pusher.TargetPosition = new Vector2?(currItemPos);
					holdable.Pusher.TargetRotation = new float?(itemRotation);
					holdable.Pusher.MoveToTargetPosition(true);
				}
			}
			if (!isClimbing && !this.character.IsIncapacitated && itemPos != Vector2.Zero && (aim || !holdable.UseHandRotationForHoldAngle))
			{
				for (int i = 0; i < 2; i++)
				{
					if (this.character.Inventory.IsInLimbSlot(item, (i == 0) ? InvSlotType.RightHand : InvSlotType.LeftHand))
					{
						this.HandIK((i == 0) ? rightHand : leftHand, transformedHoldPos + this.transformedHandlePos[i], this.CurrentAnimationParams.ArmIKStrength, this.CurrentAnimationParams.HandIKStrength, 15f);
					}
				}
			}
		}

		// Token: 0x0600307F RID: 12415 RVA: 0x001FAAB8 File Offset: 0x001F8CB8
		private float GetAimWobble(Limb rightHand, Limb leftHand, Item heldItem)
		{
			float wobbleStrength = 0f;
			CharacterInventory inventory = this.character.Inventory;
			if (((inventory != null) ? inventory.GetItemInLimbSlot(InvSlotType.RightHand) : null) == heldItem)
			{
				wobbleStrength += base.Character.CharacterHealth.GetLimbDamage(rightHand, AfflictionPrefab.DamageType);
			}
			CharacterInventory inventory2 = this.character.Inventory;
			if (((inventory2 != null) ? inventory2.GetItemInLimbSlot(InvSlotType.LeftHand) : null) == heldItem)
			{
				wobbleStrength += base.Character.CharacterHealth.GetLimbDamage(leftHand, AfflictionPrefab.DamageType);
			}
			if (wobbleStrength <= 0.1f)
			{
				return 0f;
			}
			wobbleStrength = Math.Min(wobbleStrength, 1f);
			float lowFreqNoise = PerlinNoise.GetPerlin((float)Timing.TotalTime / 320f, (float)Timing.TotalTime / 240f) - 0.5f;
			float highFreqNoise = PerlinNoise.GetPerlin((float)Timing.TotalTime / 40f, (float)Timing.TotalTime / 50f) - 0.5f;
			return (lowFreqNoise * 1f + highFreqNoise * 0.1f) * wobbleStrength;
		}

		// Token: 0x06003080 RID: 12416 RVA: 0x001FABA8 File Offset: 0x001F8DA8
		public void HandIK(Limb hand, Vector2 pos, float armTorque = 1f, float handTorque = 1f, float maxAngularVelocity = float.PositiveInfinity)
		{
			Vector2 shoulderPos;
			Limb arm;
			Limb forearm;
			if (hand.type == LimbType.LeftHand)
			{
				if (this.leftShoulder == null)
				{
					return;
				}
				shoulderPos = this.leftShoulder.WorldAnchorA;
				arm = base.GetLimb(LimbType.LeftArm, true, false, false);
				forearm = base.GetLimb(LimbType.LeftForearm, true, false, false);
				this.LeftHandIKPos = pos;
			}
			else
			{
				if (this.rightShoulder == null)
				{
					return;
				}
				shoulderPos = this.rightShoulder.WorldAnchorA;
				arm = base.GetLimb(LimbType.RightArm, true, false, false);
				forearm = base.GetLimb(LimbType.RightForearm, true, false, false);
				this.RightHandIKPos = pos;
			}
			if (arm == null)
			{
				return;
			}
			float c = Vector2.Distance(pos, shoulderPos);
			c = MathHelper.Clamp(c, Math.Abs(this.upperArmLength - this.forearmLength), this.forearmLength + this.upperArmLength - 0.01f);
			float armAngle = MathUtils.VectorToAngle(pos - shoulderPos) + arm.Params.GetSpriteOrientation() - 1.5707964f;
			float upperArmAngle = MathUtils.SolveTriangleSSS(this.forearmLength, this.upperArmLength, c) * base.Dir;
			float lowerArmAngle = MathUtils.SolveTriangleSSS(this.upperArmLength, this.forearmLength, c) * base.Dir;
			while (arm.Rotation - armAngle > 3.1415927f)
			{
				armAngle += 6.2831855f;
			}
			while (arm.Rotation - armAngle < -3.1415927f)
			{
				armAngle -= 6.2831855f;
			}
			if (((arm != null) ? arm.body : null) != null && Math.Abs(arm.body.AngularVelocity) < maxAngularVelocity)
			{
				arm.body.SmoothRotate(armAngle - upperArmAngle, 100f * armTorque * arm.Mass, false);
			}
			float forearmAngle = armAngle + lowerArmAngle;
			if (((forearm != null) ? forearm.body : null) != null && Math.Abs(forearm.body.AngularVelocity) < maxAngularVelocity)
			{
				forearm.body.SmoothRotate(forearmAngle, 100f * handTorque * forearm.Mass, false);
			}
			if (((hand != null) ? hand.body : null) != null && Math.Abs(hand.body.AngularVelocity) < maxAngularVelocity)
			{
				float handAngle = (forearm != null) ? forearmAngle : armAngle;
				hand.body.SmoothRotate(handAngle, 10f * handTorque * hand.Mass, false);
			}
		}

		// Token: 0x06003081 RID: 12417 RVA: 0x001FADBC File Offset: 0x001F8FBC
		protected void UpdateClimbing()
		{
			Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
			Ladder ladder = (selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Ladder>() : null;
			if (this.character.IsIncapacitated)
			{
				this.Anim = AnimController.Animation.None;
				return;
			}
			if (ladder == null)
			{
				this.StopClimbing();
				return;
			}
			this.onGround = false;
			base.IgnorePlatforms = true;
			bool climbFast = !this.character.Params.ForceSlowClimbing && this.character.AnimController.IsMovingFast;
			GroundedMovementParams animParams = climbFast ? this.RunParams : this.WalkParams;
			bool slide = animParams.SlideSpeed > animParams.ClimbSpeed && this.targetMovement.Y < -0.1f && climbFast;
			float maxClimbingSpeed = (climbFast && !this.character.Params.ForceSlowClimbing) ? this.RunParams.ClimbSpeed : this.WalkParams.ClimbSpeed;
			Vector2 tempTargetMovement = base.TargetMovement;
			tempTargetMovement.Y = Math.Clamp(tempTargetMovement.Y, slide ? (-animParams.SlideSpeed) : (-maxClimbingSpeed), maxClimbingSpeed);
			this.movement = MathUtils.SmoothStep(this.movement, tempTargetMovement, 0.3f);
			Limb leftFoot = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.LeftFoot);
			Limb rightFoot = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.RightFoot);
			Limb head = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.Head);
			Limb torso = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.Torso);
			Limb leftHand = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.LeftHand);
			Limb rightHand = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.RightHand);
			Vector2 ladderSimPos = ConvertUnits.ToSimUnits((float)ladder.Item.Rect.X + (float)ladder.Item.Rect.Width / 2f, (float)ladder.Item.Rect.Y);
			Vector2 ladderSimSize = ConvertUnits.ToSimUnits(ladder.Item.Rect.Size.ToVector2());
			Ladder lowestNearbyLadder = this.<UpdateClimbing>g__GetLowestNearbyLadder|123_0(ladder, 16f);
			if (lowestNearbyLadder != null && lowestNearbyLadder != ladder)
			{
				ladderSimSize.Y = ConvertUnits.ToSimUnits(ladder.Item.WorldRect.Y - (lowestNearbyLadder.Item.WorldRect.Y - lowestNearbyLadder.Item.Rect.Size.Y));
			}
			float stepHeight = ConvertUnits.ToSimUnits(animParams.ClimbStepHeight);
			if (this.currentHull == null && ladder.Item.Submarine != null)
			{
				ladderSimPos += ladder.Item.Submarine.SimPosition;
			}
			else
			{
				Hull currentHull = this.currentHull;
				if (((currentHull != null) ? currentHull.Submarine : null) != null && this.currentHull.Submarine != ladder.Item.Submarine && ladder.Item.Submarine != null)
				{
					ladderSimPos += ladder.Item.Submarine.SimPosition - this.currentHull.Submarine.SimPosition;
				}
				else
				{
					Hull currentHull2 = this.currentHull;
					if (((currentHull2 != null) ? currentHull2.Submarine : null) != null && ladder.Item.Submarine == null)
					{
						ladderSimPos -= this.currentHull.Submarine.SimPosition;
					}
				}
			}
			float bottomPos = base.Collider.SimPosition.Y - base.ColliderHeightFromFloor - base.Collider.Radius - base.Collider.Height / 2f;
			float torsoPos = this.TorsoPosition.GetValueOrDefault();
			float bodyMoveForce = animParams.ClimbBodyMoveForce;
			if (torso != null)
			{
				base.MoveLimb(torso, new Vector2(ladderSimPos.X - 0.35f * base.Dir, bottomPos + torsoPos), bodyMoveForce, false);
			}
			if (head != null)
			{
				float headPos = this.HeadPosition.GetValueOrDefault();
				base.MoveLimb(head, new Vector2(ladderSimPos.X - 0.2f * base.Dir, bottomPos + headPos), bodyMoveForce, false);
			}
			base.Collider.MoveToPos(new Vector2(ladderSimPos.X - 0.1f * base.Dir, base.Collider.SimPosition.Y), bodyMoveForce, null);
			Vector2 handPos = new Vector2(ladderSimPos.X, bottomPos + torsoPos + this.movement.Y * 0.1f - ladderSimPos.Y);
			if (climbFast)
			{
				handPos.Y -= stepHeight;
			}
			float handMoveForce = animParams.ClimbHandMoveForce;
			handPos.Y = Math.Min(-0.5f, handPos.Y);
			if (this.Aiming)
			{
				CharacterInventory inventory = this.character.Inventory;
				bool? flag;
				if (inventory == null)
				{
					flag = null;
				}
				else
				{
					Item itemInLimbSlot = inventory.GetItemInLimbSlot(InvSlotType.RightHand);
					if (itemInLimbSlot == null)
					{
						flag = null;
					}
					else
					{
						Holdable component = itemInLimbSlot.GetComponent<Holdable>();
						flag = ((component != null) ? new bool?(component.ControlPose) : null);
					}
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault() && Math.Abs(this.movement.Y) <= 0.01f)
				{
					goto IL_542;
				}
			}
			if (rightHand != null)
			{
				base.MoveLimb(rightHand, new Vector2(slide ? (handPos.X + ladderSimSize.X * 0.75f) : handPos.X, (slide ? (handPos.Y + stepHeight) : MathUtils.Round(handPos.Y, stepHeight * 2f)) + ladderSimPos.Y), handMoveForce, false);
				rightHand.body.ApplyTorque(base.Dir * 2f);
			}
			IL_542:
			if (this.Aiming)
			{
				CharacterInventory inventory2 = this.character.Inventory;
				bool? flag3;
				if (inventory2 == null)
				{
					flag3 = null;
				}
				else
				{
					Item itemInLimbSlot2 = inventory2.GetItemInLimbSlot(InvSlotType.LeftHand);
					if (itemInLimbSlot2 == null)
					{
						flag3 = null;
					}
					else
					{
						Holdable component2 = itemInLimbSlot2.GetComponent<Holdable>();
						flag3 = ((component2 != null) ? new bool?(component2.ControlPose) : null);
					}
				}
				bool? flag2 = flag3;
				if (flag2.GetValueOrDefault() && Math.Abs(this.movement.Y) <= 0.01f)
				{
					goto IL_638;
				}
			}
			if (leftHand != null)
			{
				base.MoveLimb(leftHand, new Vector2(handPos.X - ladderSimSize.X * (slide ? 1f : 0.5f), (slide ? (handPos.Y + stepHeight) : (MathUtils.Round(handPos.Y - stepHeight, stepHeight * 2f) + stepHeight)) + ladderSimPos.Y), handMoveForce, false);
				leftHand.body.ApplyTorque(base.Dir * 2f);
			}
			IL_638:
			float stepHeightAdjustment = stepHeight * 2.7f;
			Vector2 footPos = new Vector2(handPos.X - base.Dir * 0.05f, bottomPos + base.ColliderHeightFromFloor - stepHeightAdjustment - ladderSimPos.Y);
			if (climbFast)
			{
				footPos.Y += stepHeight;
			}
			Limb leftLeg = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.LeftLeg);
			Limb rightLeg = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.RightLeg);
			if (footPos.Y > -ladderSimSize.Y - 0.2f && leftFoot != null && rightFoot != null && leftLeg != null && rightLeg != null)
			{
				Limb limb;
				if ((limb = this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.Waist)) == null)
				{
					limb = (this.<UpdateClimbing>g__GetClimbingLimb|123_1(LimbType.Torso) ?? base.MainLimb);
				}
				Limb refLimb = limb;
				bool leftLegBackwards = Math.Abs(leftLeg.body.Rotation - refLimb.body.Rotation) > 3.1415927f;
				bool rightLegBackwards = Math.Abs(rightLeg.body.Rotation - refLimb.body.Rotation) > 3.1415927f;
				float footMoveForce = animParams.ClimbFootMoveForce;
				if (slide)
				{
					if (!leftLegBackwards)
					{
						base.MoveLimb(leftFoot, new Vector2(footPos.X - ladderSimSize.X * 0.5f, footPos.Y + ladderSimPos.Y), footMoveForce, true);
					}
					if (!rightLegBackwards)
					{
						base.MoveLimb(rightFoot, new Vector2(footPos.X, footPos.Y + ladderSimPos.Y), footMoveForce, true);
					}
				}
				else
				{
					float leftFootPos = MathUtils.Round(footPos.Y + stepHeight, stepHeight * 2f) - stepHeight;
					float prevLeftFootPos = MathUtils.Round(this.prevFootPos + stepHeight, stepHeight * 2f) - stepHeight;
					if (!leftLegBackwards)
					{
						base.MoveLimb(leftFoot, new Vector2(footPos.X, leftFootPos + ladderSimPos.Y), footMoveForce, true);
					}
					float rightFootPos = MathUtils.Round(footPos.Y, stepHeight * 2f);
					float prevRightFootPos = MathUtils.Round(this.prevFootPos, stepHeight * 2f);
					if (!rightLegBackwards)
					{
						base.MoveLimb(rightFoot, new Vector2(footPos.X, rightFootPos + ladderSimPos.Y), footMoveForce, true);
					}
					if (Math.Abs(leftFootPos - prevLeftFootPos) > stepHeight && (double)leftFoot.LastImpactSoundTime < Timing.TotalTime - 0.4000000059604645)
					{
						string soundTag = "footstep_armor_heavy";
						Vector2 worldPosition = leftFoot.WorldPosition;
						Hull currentHull3 = this.currentHull;
						SoundPlayer.PlaySound(soundTag, worldPosition, null, null, currentHull3);
						leftFoot.LastImpactSoundTime = (float)Timing.TotalTime;
					}
					if (Math.Abs(rightFootPos - prevRightFootPos) > stepHeight && (double)rightFoot.LastImpactSoundTime < Timing.TotalTime - 0.4000000059604645)
					{
						string soundTag2 = "footstep_armor_heavy";
						Vector2 worldPosition2 = rightFoot.WorldPosition;
						Hull currentHull3 = this.currentHull;
						SoundPlayer.PlaySound(soundTag2, worldPosition2, null, null, currentHull3);
						rightFoot.LastImpactSoundTime = (float)Timing.TotalTime;
					}
					this.prevFootPos = footPos.Y;
				}
				if (!leftLegBackwards)
				{
					leftLeg.body.ApplyTorque(base.Dir * -8f);
				}
				if (!rightLegBackwards)
				{
					rightLeg.body.ApplyTorque(base.Dir * -8f);
				}
			}
			float movementFactor = handPos.Y / stepHeight * 3.1415927f;
			movementFactor = 0.8f + (float)Math.Abs(Math.Sin((double)movementFactor));
			Vector2 subSpeed = (this.currentHull != null || ladder.Item.Submarine == null) ? Vector2.Zero : ladder.Item.Submarine.Velocity;
			Vector2 climbForce = new Vector2(0f, this.movement.Y) * movementFactor;
			if (!base.InWater)
			{
				climbForce.Y += 0.3f * movementFactor;
			}
			if (this.character.SimPosition.Y > ladderSimPos.Y)
			{
				climbForce.Y = Math.Min(0f, climbForce.Y);
			}
			float minHeightFromFloor = base.ColliderHeightFromFloor / 2f + base.Collider.Height;
			if (this.floorFixture != null && !this.floorFixture.CollisionCategories.HasFlag(Category.Cat4) && !this.floorFixture.CollisionCategories.HasFlag(Category.Cat3) && this.character.SimPosition.Y < this.standOnFloorY + minHeightFromFloor)
			{
				climbForce.Y = MathHelper.Clamp((this.standOnFloorY + minHeightFromFloor - this.character.SimPosition.Y) * 5f, climbForce.Y, 1f);
			}
			base.Collider.ApplyForce((climbForce * 20f + subSpeed * 50f) * base.Collider.Mass, 64f);
			if (head != null && this.character.IsHumanoid)
			{
				if (this.Aiming)
				{
					this.RotateHead(head);
				}
				else
				{
					if (this.Anim == AnimController.Animation.UsingItemWhileClimbing)
					{
						Item selectedItem = this.character.SelectedItem;
						if (selectedItem != null)
						{
							Vector2 diff = (selectedItem.WorldPosition - head.WorldPosition) * base.Dir;
							float targetRotation = MathHelper.WrapAngle(MathUtils.VectorToAngle(diff) - 0.7853982f * base.Dir);
							head.body.SmoothRotate(targetRotation, animParams.HeadTorque, true);
							goto IL_BE6;
						}
					}
					float movementMultiplier = this.targetMovement.Y >= 0f;
					head.body.SmoothRotate(0.7853982f * movementMultiplier * base.Dir, animParams.HeadTorque, true);
				}
			}
			IL_BE6:
			if (ladder.Item.Prefab.Triggers.None(null))
			{
				this.character.ReleaseSecondaryItem();
				return;
			}
			Rectangle trigger = ladder.Item.Prefab.Triggers.FirstOrDefault<Rectangle>();
			trigger = ladder.Item.TransformTrigger(trigger, false);
			bool isRemote = false;
			bool isClimbing = true;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				isRemote = this.character.IsRemotelyControlled;
			}
			if (!isRemote && (this.character.IsKeyDown(InputType.Left) || this.character.IsKeyDown(InputType.Right)) && !this.character.IsKeyDown(InputType.Up) && !this.character.IsKeyDown(InputType.Down))
			{
				isClimbing = false;
			}
			if (!isClimbing)
			{
				this.character.StopClimbing();
				base.IgnorePlatforms = false;
			}
		}

		// Token: 0x06003082 RID: 12418 RVA: 0x001FBA7C File Offset: 0x001F9C7C
		protected void RotateHead(Limb head)
		{
			Vector2 mousePos = ConvertUnits.ToSimUnits(this.character.CursorPosition);
			Vector2 dir = (mousePos - head.SimPosition) * base.Dir;
			float rot = MathUtils.VectorToAngle(dir);
			LimbJoint neckJoint = this.GetJointBetweenLimbs(LimbType.Head, LimbType.Torso);
			if (neckJoint != null)
			{
				float offset = MathUtils.WrapAnglePi(base.GetLimb(LimbType.Torso, true, false, false).body.Rotation);
				float lowerLimit = neckJoint.LowerLimit + offset;
				float upperLimit = neckJoint.UpperLimit + offset;
				float min = Math.Min(lowerLimit, upperLimit);
				float max = Math.Max(lowerLimit, upperLimit);
				rot = Math.Clamp(rot, min, max);
			}
			head.body.SmoothRotate(rot, this.CurrentAnimationParams.HeadTorque, true);
		}

		// Token: 0x06003083 RID: 12419 RVA: 0x001FBB34 File Offset: 0x001F9D34
		public void ApplyPose(Vector2 leftHandPos, Vector2 rightHandPos, Vector2 leftFootPos, Vector2 rightFootPos, float footMoveForce = 10f)
		{
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			Limb waist = base.GetLimb(LimbType.Waist, true, false, false) ?? base.GetLimb(LimbType.Torso, true, false, false);
			if (waist == null)
			{
				return;
			}
			Vector2 midPos = waist.SimPosition;
			if (leftHand != null)
			{
				leftHand.Disabled = true;
				leftHandPos.X *= base.Dir;
				leftHandPos += midPos;
				this.HandIK(leftHand, leftHandPos, 1f, 1f, float.PositiveInfinity);
			}
			if (rightHand != null)
			{
				rightHand.Disabled = true;
				rightHandPos.X *= base.Dir;
				rightHandPos += midPos;
				this.HandIK(rightHand, rightHandPos, 1f, 1f, float.PositiveInfinity);
			}
			Limb leftFoot = base.GetLimb(LimbType.LeftFoot, true, false, false);
			if (leftFoot != null)
			{
				leftFoot.Disabled = true;
				leftFootPos = new Vector2(waist.SimPosition.X + leftFootPos.X * base.Dir, base.GetColliderBottom().Y + leftFootPos.Y);
				base.MoveLimb(leftFoot, leftFootPos, Math.Abs(leftFoot.SimPosition.X - leftFootPos.X) * footMoveForce * leftFoot.Mass, true);
			}
			Limb rightFoot = base.GetLimb(LimbType.RightFoot, true, false, false);
			if (rightFoot != null)
			{
				rightFoot.Disabled = true;
				rightFootPos = new Vector2(waist.SimPosition.X + rightFootPos.X * base.Dir, base.GetColliderBottom().Y + rightFootPos.Y);
				base.MoveLimb(rightFoot, rightFootPos, Math.Abs(rightFoot.SimPosition.X - rightFootPos.X) * footMoveForce * rightFoot.Mass, true);
			}
		}

		// Token: 0x06003084 RID: 12420 RVA: 0x001FBCE8 File Offset: 0x001F9EE8
		public void ApplyTestPose()
		{
			Limb waist = base.GetLimb(LimbType.Waist, true, false, false) ?? base.GetLimb(LimbType.Torso, true, false, false);
			if (waist != null)
			{
				this.ApplyPose(new Vector2(-0.75f, -0.2f), new Vector2(0.75f, -0.2f), new Vector2(-this.WalkParams.StepSize.X * 0.5f, -0.1f * this.RagdollParams.JointScale), new Vector2(this.WalkParams.StepSize.X * 0.5f, -0.1f * this.RagdollParams.JointScale), 10f);
			}
		}

		// Token: 0x06003085 RID: 12421 RVA: 0x001FBD98 File Offset: 0x001F9F98
		protected void CalculateArmLengths()
		{
			Limb rightForearm = base.GetLimb(LimbType.RightForearm, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			if (rightHand == null)
			{
				return;
			}
			LimbJoint limbJoint;
			if ((limbJoint = this.GetJointBetweenLimbs(LimbType.Torso, LimbType.RightArm)) == null && (limbJoint = this.GetJointBetweenLimbs(LimbType.Head, LimbType.RightArm)) == null)
			{
				limbJoint = (this.GetJoint(LimbType.RightArm, new LimbType[]
				{
					LimbType.RightHand,
					LimbType.RightForearm
				}) ?? this.GetJointBetweenLimbs(LimbType.Torso, LimbType.RightHand));
			}
			this.rightShoulder = limbJoint;
			LimbJoint limbJoint2;
			if ((limbJoint2 = this.GetJointBetweenLimbs(LimbType.Torso, LimbType.LeftArm)) == null && (limbJoint2 = this.GetJointBetweenLimbs(LimbType.Head, LimbType.LeftArm)) == null)
			{
				limbJoint2 = (this.GetJoint(LimbType.LeftArm, new LimbType[]
				{
					LimbType.LeftHand,
					LimbType.LeftForearm
				}) ?? this.GetJointBetweenLimbs(LimbType.Torso, LimbType.LeftHand));
			}
			this.leftShoulder = limbJoint2;
			Vector2 localAnchorShoulder = Vector2.Zero;
			Vector2 localAnchorElbow = Vector2.Zero;
			if (this.rightShoulder != null)
			{
				localAnchorShoulder = ((this.rightShoulder.LimbA.type == LimbType.RightArm) ? this.rightShoulder.LocalAnchorA : this.rightShoulder.LocalAnchorB);
			}
			LimbJoint rightElbow = (rightForearm == null) ? this.GetJointBetweenLimbs(LimbType.RightArm, LimbType.RightHand) : this.GetJointBetweenLimbs(LimbType.RightArm, LimbType.RightForearm);
			if (rightElbow != null)
			{
				localAnchorElbow = ((rightElbow.LimbA.type == LimbType.RightArm) ? rightElbow.LocalAnchorA : rightElbow.LocalAnchorB);
			}
			this.upperArmLength = Vector2.Distance(localAnchorShoulder, localAnchorElbow);
			if (rightElbow != null)
			{
				if (rightForearm == null)
				{
					this.forearmLength = Vector2.Distance(rightHand.PullJointLocalAnchorA, (rightElbow.LimbA.type == LimbType.RightHand) ? rightElbow.LocalAnchorA : rightElbow.LocalAnchorB);
					return;
				}
				LimbJoint rightWrist = this.GetJointBetweenLimbs(LimbType.RightForearm, LimbType.RightHand);
				if (rightWrist != null)
				{
					this.forearmLength = Vector2.Distance((rightElbow.LimbA.type == LimbType.RightForearm) ? rightElbow.LocalAnchorA : rightElbow.LocalAnchorB, (rightWrist.LimbA.type == LimbType.RightForearm) ? rightWrist.LocalAnchorA : rightWrist.LocalAnchorB);
					this.forearmLength += Vector2.Distance(rightHand.PullJointLocalAnchorA, (rightWrist.LimbA.type == LimbType.RightHand) ? rightWrist.LocalAnchorA : rightWrist.LocalAnchorB);
				}
			}
		}

		// Token: 0x06003086 RID: 12422 RVA: 0x001FBF98 File Offset: 0x001FA198
		protected LimbJoint GetJointBetweenLimbs(LimbType limbTypeA, LimbType limbTypeB)
		{
			return this.LimbJoints.FirstOrDefault((LimbJoint lj) => (lj.LimbA.type == limbTypeA && lj.LimbB.type == limbTypeB) || (lj.LimbB.type == limbTypeA && lj.LimbA.type == limbTypeB));
		}

		// Token: 0x06003087 RID: 12423 RVA: 0x001FBFD0 File Offset: 0x001FA1D0
		protected LimbJoint GetJoint(LimbType matchingType, IEnumerable<LimbType> ignoredTypes)
		{
			return this.LimbJoints.FirstOrDefault((LimbJoint lj) => (lj.LimbA.type == matchingType && ignoredTypes.None((LimbType t) => lj.LimbB.type == t)) || (lj.LimbB.type == matchingType && ignoredTypes.None((LimbType t) => lj.LimbB.type == t)));
		}

		// Token: 0x06003088 RID: 12424 RVA: 0x001FC008 File Offset: 0x001FA208
		public override void Recreate(RagdollParams ragdollParams = null)
		{
			base.Recreate(ragdollParams);
			if (base.Character.Params.CanInteract)
			{
				this.CalculateArmLengths();
			}
		}

		// Token: 0x06003089 RID: 12425 RVA: 0x001FC02C File Offset: 0x001FA22C
		public void RecreateAndRespawn(RagdollParams ragdollParams = null)
		{
			Vector2 pos = this.character.WorldPosition;
			this.Recreate(ragdollParams);
			this.character.TeleportTo(pos);
		}

		// Token: 0x0600308A RID: 12426 RVA: 0x001FC058 File Offset: 0x001FA258
		protected void CheckRopeState()
		{
			if (!this.shouldHangWithRope)
			{
				base.StopHangingWithRope();
			}
			if (!this.shouldHoldToRope)
			{
				base.StopHoldingToRope();
			}
			if (!this.shouldBeDraggedWithRope)
			{
				base.StopGettingDraggedWithRope();
			}
			this.shouldHoldToRope = false;
			this.shouldHangWithRope = false;
			this.shouldBeDraggedWithRope = false;
		}

		// Token: 0x0600308B RID: 12427 RVA: 0x001FC0A4 File Offset: 0x001FA2A4
		private void StartAnimation(AnimController.Animation animation)
		{
			if (animation == AnimController.Animation.UsingItem)
			{
				this.Anim = (this.IsClimbing ? AnimController.Animation.UsingItemWhileClimbing : AnimController.Animation.UsingItem);
				return;
			}
			if (animation == AnimController.Animation.Climbing)
			{
				this.Anim = (this.IsUsingItem ? AnimController.Animation.UsingItemWhileClimbing : AnimController.Animation.Climbing);
				return;
			}
			this.Anim = animation;
		}

		// Token: 0x0600308C RID: 12428 RVA: 0x001FC0DB File Offset: 0x001FA2DB
		private void StopAnimation(AnimController.Animation animation)
		{
			if (animation == AnimController.Animation.UsingItem)
			{
				this.Anim = (this.IsClimbing ? AnimController.Animation.Climbing : AnimController.Animation.None);
				return;
			}
			if (animation == AnimController.Animation.Climbing)
			{
				this.Anim = (this.IsUsingItem ? AnimController.Animation.UsingItem : AnimController.Animation.None);
				return;
			}
			this.Anim = AnimController.Animation.None;
		}

		// Token: 0x0600308D RID: 12429 RVA: 0x001FC112 File Offset: 0x001FA312
		public void StartUsingItem()
		{
			this.StartAnimation(AnimController.Animation.UsingItem);
		}

		// Token: 0x0600308E RID: 12430 RVA: 0x001FC11B File Offset: 0x001FA31B
		public void StartClimbing()
		{
			this.StartAnimation(AnimController.Animation.Climbing);
		}

		// Token: 0x0600308F RID: 12431 RVA: 0x001FC124 File Offset: 0x001FA324
		public void StopUsingItem()
		{
			this.StopAnimation(AnimController.Animation.UsingItem);
		}

		// Token: 0x06003090 RID: 12432 RVA: 0x001FC12D File Offset: 0x001FA32D
		public void StopClimbing()
		{
			this.StopAnimation(AnimController.Animation.Climbing);
		}

		// Token: 0x06003091 RID: 12433 RVA: 0x001FC138 File Offset: 0x001FA338
		public bool TryLoadTemporaryAnimation(StatusEffect.AnimLoadInfo animLoadInfo, bool throwErrors)
		{
			AnimationType animType = animLoadInfo.Type;
			AnimController.AnimSwap animSwap;
			if (this.tempAnimations.TryGetValue(animType, out animSwap))
			{
				string fileName;
				if (animLoadInfo.File.TryGet(out fileName) && animSwap.TemporaryAnimation.FileNameWithoutExtension.Equals(fileName, StringComparison.OrdinalIgnoreCase))
				{
					animSwap.IsActive = true;
					return true;
				}
				ContentPath contentPath;
				if (animLoadInfo.File.TryGet(out contentPath) && animSwap.TemporaryAnimation.Path == contentPath)
				{
					animSwap.IsActive = true;
					return true;
				}
				if (animSwap.Priority >= animLoadInfo.Priority)
				{
					return true;
				}
				this.tempAnimations.Remove(animType);
			}
			AnimationParams defaultAnimation = this.GetAnimationParamsFromType(animType);
			if (defaultAnimation == null)
			{
				return false;
			}
			AnimationParams tempParams;
			if (!this.TryLoadAnimation(animType, animLoadInfo.File, out tempParams, throwErrors))
			{
				return false;
			}
			this.defaultAnimations.TryAdd(animType, defaultAnimation);
			this.tempAnimations.Add(animType, new AnimController.AnimSwap(tempParams, animLoadInfo.Priority));
			return true;
		}

		// Token: 0x06003092 RID: 12434 RVA: 0x001FC220 File Offset: 0x001FA420
		private void UpdateTemporaryAnimations()
		{
			if (this.tempAnimations.None(null))
			{
				return;
			}
			foreach (KeyValuePair<AnimationType, AnimController.AnimSwap> keyValuePair in this.tempAnimations)
			{
				AnimationType animationType2;
				AnimController.AnimSwap animSwap3;
				keyValuePair.Deconstruct(out animationType2, out animSwap3);
				AnimationType animationType = animationType2;
				AnimController.AnimSwap animSwap = animSwap3;
				if (!animSwap.IsActive)
				{
					AnimationParams defaultAnimation;
					if (this.defaultAnimations.TryGetValue(animSwap.AnimationType, out defaultAnimation))
					{
						this.TrySwapAnimParams(defaultAnimation);
						this.expiredAnimations.Add(animationType);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(115, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[AnimController] Failed to find the default animation parameters of type ");
						defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animSwap.AnimationType);
						defaultInterpolatedStringHandler.AppendLiteral(". Cannot swap back the default animations!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						this.tempAnimations.Clear();
					}
				}
			}
			foreach (AnimationType anim in this.expiredAnimations)
			{
				this.tempAnimations.Remove(anim);
			}
			this.expiredAnimations.Clear();
			foreach (AnimController.AnimSwap animSwap2 in this.tempAnimations.Values)
			{
				animSwap2.IsActive = false;
			}
		}

		// Token: 0x06003093 RID: 12435 RVA: 0x001FC3B4 File Offset: 0x001FA5B4
		public bool TryLoadAnimation(AnimationType animationType, Either<string, ContentPath> file, out AnimationParams animParams, bool throwErrors)
		{
			animParams = null;
			if (this.character.IsHumanoid)
			{
				HumanoidAnimController humanAnimController = this as HumanoidAnimController;
				if (humanAnimController != null)
				{
					switch (animationType)
					{
					case AnimationType.Walk:
						humanAnimController.WalkParams = HumanWalkParams.GetAnimParams(this.character, file, throwErrors);
						animParams = humanAnimController.WalkParams;
						goto IL_260;
					case AnimationType.Run:
						humanAnimController.RunParams = HumanRunParams.GetAnimParams(this.character, file, throwErrors);
						animParams = humanAnimController.RunParams;
						goto IL_260;
					case AnimationType.SwimSlow:
						humanAnimController.SwimSlowParams = HumanSwimSlowParams.GetAnimParams(this.character, file, throwErrors);
						animParams = humanAnimController.SwimSlowParams;
						goto IL_260;
					case AnimationType.SwimFast:
						humanAnimController.SwimFastParams = HumanSwimFastParams.GetAnimParams(this.character, file, throwErrors);
						animParams = humanAnimController.SwimFastParams;
						goto IL_260;
					case AnimationType.Crouch:
						humanAnimController.HumanCrouchParams = HumanCrouchParams.GetAnimParams(this.character, file, throwErrors);
						animParams = humanAnimController.HumanCrouchParams;
						goto IL_260;
					default:
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[AnimController] Animation of type ");
						defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animationType);
						defaultInterpolatedStringHandler.AppendLiteral(" not implemented!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						goto IL_260;
					}
					}
				}
			}
			switch (animationType)
			{
			case AnimationType.Walk:
				if (this.CanWalk)
				{
					this.character.AnimController.WalkParams = FishWalkParams.GetAnimParams(this.character, file, throwErrors);
					animParams = this.character.AnimController.WalkParams;
				}
				break;
			case AnimationType.Run:
				if (this.CanWalk)
				{
					this.character.AnimController.RunParams = FishRunParams.GetAnimParams(this.character, file, throwErrors);
					animParams = this.character.AnimController.RunParams;
				}
				break;
			case AnimationType.SwimSlow:
				this.character.AnimController.SwimSlowParams = FishSwimSlowParams.GetAnimParams(this.character, file, throwErrors);
				animParams = this.character.AnimController.SwimSlowParams;
				break;
			case AnimationType.SwimFast:
				this.character.AnimController.SwimFastParams = FishSwimFastParams.GetAnimParams(this.character, file, throwErrors);
				animParams = this.character.AnimController.SwimFastParams;
				break;
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[AnimController] Animation of type ");
				defaultInterpolatedStringHandler2.AppendFormatted<AnimationType>(animationType);
				defaultInterpolatedStringHandler2.AppendLiteral(" not implemented!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				break;
			}
			}
			IL_260:
			bool success = animParams != null;
			string fileName;
			if (!file.TryGet(out fileName))
			{
				ContentPath contentPath;
				if (file.TryGet(out contentPath))
				{
					fileName = contentPath.Value;
					if (success)
					{
						success = (contentPath == animParams.Path);
					}
				}
			}
			else if (success)
			{
				success = animParams.FileNameWithoutExtension.Equals(fileName, StringComparison.OrdinalIgnoreCase);
			}
			if (success)
			{
				DebugConsole.NewMessage("Animation " + fileName + " successfully loaded for " + this.character.DisplayName, new Color?(Color.LightGreen), true);
			}
			else if (throwErrors)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(36, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Animation ");
				defaultInterpolatedStringHandler3.AppendFormatted(fileName);
				defaultInterpolatedStringHandler3.AppendLiteral(" for ");
				defaultInterpolatedStringHandler3.AppendFormatted(this.character.DisplayName);
				defaultInterpolatedStringHandler3.AppendLiteral(" could not be loaded!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
			}
			return success;
		}

		// Token: 0x06003094 RID: 12436 RVA: 0x001FC6F8 File Offset: 0x001FA8F8
		protected bool TrySwapAnimParams(AnimationParams newParams)
		{
			AnimationType animationType = newParams.AnimationType;
			if (this.character.IsHumanoid)
			{
				HumanoidAnimController humanAnimController = this as HumanoidAnimController;
				if (humanAnimController != null)
				{
					switch (animationType)
					{
					case AnimationType.Walk:
					{
						HumanWalkParams newWalkParams = newParams as HumanWalkParams;
						if (newWalkParams != null)
						{
							humanAnimController.WalkParams = newWalkParams;
						}
						return true;
					}
					case AnimationType.Run:
					{
						HumanRunParams newRunParams = newParams as HumanRunParams;
						if (newRunParams != null)
						{
							humanAnimController.HumanRunParams = newRunParams;
							return false;
						}
						return false;
					}
					case AnimationType.SwimSlow:
					{
						HumanSwimSlowParams newSwimSlowParams = newParams as HumanSwimSlowParams;
						if (newSwimSlowParams != null)
						{
							humanAnimController.HumanSwimSlowParams = newSwimSlowParams;
						}
						return true;
					}
					case AnimationType.SwimFast:
					{
						HumanSwimFastParams newSwimFastParams = newParams as HumanSwimFastParams;
						if (newSwimFastParams != null)
						{
							humanAnimController.HumanSwimFastParams = newSwimFastParams;
						}
						return true;
					}
					case AnimationType.Crouch:
					{
						HumanCrouchParams newCrouchParams = newParams as HumanCrouchParams;
						if (newCrouchParams != null)
						{
							humanAnimController.HumanCrouchParams = newCrouchParams;
						}
						return true;
					}
					default:
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[AnimController] Animation of type ");
						defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animationType);
						defaultInterpolatedStringHandler.AppendLiteral(" not implemented!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						return false;
					}
					}
				}
			}
			switch (animationType)
			{
			case AnimationType.Walk:
			{
				FishWalkParams walkParams = newParams as FishWalkParams;
				if (walkParams != null)
				{
					this.character.AnimController.WalkParams = walkParams;
				}
				return true;
			}
			case AnimationType.Run:
			{
				FishRunParams runParams = newParams as FishRunParams;
				if (runParams != null)
				{
					this.character.AnimController.RunParams = runParams;
				}
				return true;
			}
			case AnimationType.SwimSlow:
			{
				FishSwimSlowParams swimSlowParams = newParams as FishSwimSlowParams;
				if (swimSlowParams != null)
				{
					this.character.AnimController.SwimSlowParams = swimSlowParams;
				}
				return true;
			}
			case AnimationType.SwimFast:
			{
				FishSwimFastParams swimFastParams = newParams as FishSwimFastParams;
				if (swimFastParams != null)
				{
					this.character.AnimController.SwimFastParams = swimFastParams;
				}
				return true;
			}
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[AnimController] Animation of type ");
				defaultInterpolatedStringHandler2.AppendFormatted<AnimationType>(animationType);
				defaultInterpolatedStringHandler2.AppendLiteral(" not implemented!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				break;
			}
			}
			return false;
		}

		// Token: 0x06003095 RID: 12437 RVA: 0x001FC8C9 File Offset: 0x001FAAC9
		[CompilerGenerated]
		private Vector2 <UpdateUseItem>g__GetTargetMovement|115_0(Vector2 dir)
		{
			return dir * this.GetCurrentSpeed(false) * Math.Max(this.character.SpeedMultiplier, 1f);
		}

		// Token: 0x06003096 RID: 12438 RVA: 0x001FC8F4 File Offset: 0x001FAAF4
		[CompilerGenerated]
		private Ladder <UpdateClimbing>g__GetLowestNearbyLadder|123_0(Ladder currentLadder, float threshold = 16f)
		{
			foreach (Ladder ladder in Ladder.List)
			{
				if (ladder != currentLadder && ladder.Item.IsInteractable(this.character) && Math.Abs(ladder.Item.WorldPosition.X - currentLadder.Item.WorldPosition.X) <= threshold && ladder.Item.WorldPosition.Y <= currentLadder.Item.WorldPosition.Y && (float)(currentLadder.Item.WorldRect.Y - currentLadder.Item.Rect.Height - ladder.Item.WorldRect.Y) <= threshold)
				{
					return ladder;
				}
			}
			return null;
		}

		// Token: 0x06003097 RID: 12439 RVA: 0x001FC9EC File Offset: 0x001FABEC
		[CompilerGenerated]
		private Limb <UpdateClimbing>g__GetClimbingLimb|123_1(LimbType limbType)
		{
			if (base.HasMultipleLimbsOfSameType)
			{
				return base.GetLimb(limbType, true, false, true) ?? base.GetLimb(limbType, true, true, false);
			}
			return base.GetLimb(limbType, true, false, false);
		}

		// Token: 0x04001927 RID: 6439
		protected LimbJoint rightShoulder;

		// Token: 0x04001928 RID: 6440
		protected LimbJoint leftShoulder;

		// Token: 0x04001929 RID: 6441
		protected float upperArmLength;

		// Token: 0x0400192A RID: 6442
		protected float forearmLength;

		// Token: 0x0400192B RID: 6443
		protected float useItemTimer;

		// Token: 0x0400192C RID: 6444
		protected bool aiming;

		// Token: 0x0400192D RID: 6445
		protected bool wasAiming;

		// Token: 0x0400192E RID: 6446
		protected bool aimingMelee;

		// Token: 0x0400192F RID: 6447
		protected bool wasAimingMelee;

		// Token: 0x04001930 RID: 6448
		protected readonly Dictionary<AnimationType, AnimController.AnimSwap> tempAnimations = new Dictionary<AnimationType, AnimController.AnimSwap>();

		// Token: 0x04001931 RID: 6449
		protected readonly HashSet<AnimationType> expiredAnimations = new HashSet<AnimationType>();

		// Token: 0x04001933 RID: 6451
		protected float deathAnimTimer;

		// Token: 0x04001934 RID: 6452
		protected float deathAnimDuration = 5f;

		// Token: 0x04001935 RID: 6453
		public AnimController.Animation Anim;

		// Token: 0x04001939 RID: 6457
		private Direction previousDirection;

		// Token: 0x0400193A RID: 6458
		private readonly Vector2[] transformedHandlePos = new Vector2[2];

		// Token: 0x0400193B RID: 6459
		private float prevFootPos;

		// Token: 0x0400193C RID: 6460
		private readonly Dictionary<AnimationType, AnimationParams> defaultAnimations = new Dictionary<AnimationType, AnimationParams>();

		// Token: 0x02000E93 RID: 3731
		protected class AnimSwap
		{
			// Token: 0x060084B1 RID: 33969 RVA: 0x003A02A1 File Offset: 0x0039E4A1
			public AnimSwap(AnimationParams temporaryAnimation, float priority)
			{
				this.AnimationType = temporaryAnimation.AnimationType;
				this.TemporaryAnimation = temporaryAnimation;
				this.Priority = priority;
				this.IsActive = true;
			}

			// Token: 0x0400529F RID: 21151
			public readonly AnimationType AnimationType;

			// Token: 0x040052A0 RID: 21152
			public readonly AnimationParams TemporaryAnimation;

			// Token: 0x040052A1 RID: 21153
			public readonly float Priority;

			// Token: 0x040052A2 RID: 21154
			public bool IsActive;
		}

		// Token: 0x02000E94 RID: 3732
		public enum Animation
		{
			// Token: 0x040052A4 RID: 21156
			None,
			// Token: 0x040052A5 RID: 21157
			Climbing,
			// Token: 0x040052A6 RID: 21158
			UsingItem,
			// Token: 0x040052A7 RID: 21159
			Struggle,
			// Token: 0x040052A8 RID: 21160
			CPR,
			// Token: 0x040052A9 RID: 21161
			UsingItemWhileClimbing
		}
	}
}
