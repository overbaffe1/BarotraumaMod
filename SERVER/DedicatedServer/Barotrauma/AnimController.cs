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
	// Token: 0x020000A3 RID: 163
	internal abstract class AnimController : Ragdoll, ISerializableEntity
	{
		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001367 RID: 4967 RVA: 0x000A7CA9 File Offset: 0x000A5EA9
		// (set) Token: 0x06001368 RID: 4968 RVA: 0x000A7CB1 File Offset: 0x000A5EB1
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06001369 RID: 4969 RVA: 0x000A7CBA File Offset: 0x000A5EBA
		public string Name
		{
			get
			{
				return "AnimController";
			}
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x0600136A RID: 4970 RVA: 0x000A7CC1 File Offset: 0x000A5EC1
		// (set) Token: 0x0600136B RID: 4971 RVA: 0x000A7CC9 File Offset: 0x000A5EC9
		public Vector2 RightHandIKPos { get; protected set; }

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x0600136C RID: 4972 RVA: 0x000A7CD2 File Offset: 0x000A5ED2
		// (set) Token: 0x0600136D RID: 4973 RVA: 0x000A7CDA File Offset: 0x000A5EDA
		public Vector2 LeftHandIKPos { get; protected set; }

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x0600136E RID: 4974 RVA: 0x000A7CE3 File Offset: 0x000A5EE3
		public bool IsAiming
		{
			get
			{
				return this.wasAiming;
			}
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x0600136F RID: 4975 RVA: 0x000A7CEB File Offset: 0x000A5EEB
		public bool IsAimingMelee
		{
			get
			{
				return this.wasAimingMelee;
			}
		}

		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001370 RID: 4976 RVA: 0x000A7CF3 File Offset: 0x000A5EF3
		protected bool Aiming
		{
			get
			{
				return this.aiming || this.aimingMelee || ((double)this.FlipLockTime > Timing.TotalTime && this.character.IsKeyDown(InputType.Aim));
			}
		}

		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x06001371 RID: 4977 RVA: 0x000A7D23 File Offset: 0x000A5F23
		public float ArmLength
		{
			get
			{
				return this.upperArmLength + this.forearmLength;
			}
		}

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x06001372 RID: 4978
		// (set) Token: 0x06001373 RID: 4979
		public abstract GroundedMovementParams WalkParams { get; set; }

		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x06001374 RID: 4980
		// (set) Token: 0x06001375 RID: 4981
		public abstract GroundedMovementParams RunParams { get; set; }

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001376 RID: 4982
		// (set) Token: 0x06001377 RID: 4983
		public abstract SwimParams SwimSlowParams { get; set; }

		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x06001378 RID: 4984
		// (set) Token: 0x06001379 RID: 4985
		public abstract SwimParams SwimFastParams { get; set; }

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x0600137A RID: 4986 RVA: 0x000A7D32 File Offset: 0x000A5F32
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

		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x0600137B RID: 4987 RVA: 0x000A7D66 File Offset: 0x000A5F66
		// (set) Token: 0x0600137C RID: 4988 RVA: 0x000A7D6E File Offset: 0x000A5F6E
		public AnimationType ForceSelectAnimationType { get; set; }

		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x0600137D RID: 4989 RVA: 0x000A7D78 File Offset: 0x000A5F78
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

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x0600137E RID: 4990 RVA: 0x000A7DD7 File Offset: 0x000A5FD7
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

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x0600137F RID: 4991 RVA: 0x000A7E08 File Offset: 0x000A6008
		public bool CanWalk
		{
			get
			{
				return this.RagdollParams.CanWalk;
			}
		}

		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x06001380 RID: 4992 RVA: 0x000A7E18 File Offset: 0x000A6018
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

		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001381 RID: 4993 RVA: 0x000A7E70 File Offset: 0x000A6070
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

		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x06001382 RID: 4994 RVA: 0x000A7EFC File Offset: 0x000A60FC
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

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x06001383 RID: 4995 RVA: 0x000A7F7C File Offset: 0x000A617C
		public bool IsUsingItem
		{
			get
			{
				return this.Anim == AnimController.Animation.UsingItem || this.Anim == AnimController.Animation.UsingItemWhileClimbing;
			}
		}

		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x06001384 RID: 4996 RVA: 0x000A7F92 File Offset: 0x000A6192
		public bool IsClimbing
		{
			get
			{
				return this.Anim == AnimController.Animation.Climbing || this.Anim == AnimController.Animation.UsingItemWhileClimbing;
			}
		}

		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001385 RID: 4997 RVA: 0x000A7FA8 File Offset: 0x000A61A8
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

		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x06001386 RID: 4998 RVA: 0x000A7FEB File Offset: 0x000A61EB
		public Vector2 AimSourcePos
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.AimSourceSimPos);
			}
		}

		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x06001387 RID: 4999 RVA: 0x000A7FF8 File Offset: 0x000A61F8
		public virtual Vector2 AimSourceSimPos
		{
			get
			{
				return base.Collider.SimPosition;
			}
		}

		// Token: 0x06001388 RID: 5000 RVA: 0x000A8008 File Offset: 0x000A6208
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

		// Token: 0x06001389 RID: 5001 RVA: 0x000A805C File Offset: 0x000A625C
		protected Vector2? GetValidOrNull(AnimationParams p, Vector2 v)
		{
			if (p == null)
			{
				return null;
			}
			return new Vector2?(v);
		}

		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x0600138A RID: 5002 RVA: 0x000A807C File Offset: 0x000A627C
		public override float? HeadPosition
		{
			get
			{
				AnimationParams currentGroundedParams = this.CurrentGroundedParams;
				GroundedMovementParams currentGroundedParams2 = this.CurrentGroundedParams;
				return this.GetValidOrNull(currentGroundedParams, ((currentGroundedParams2 != null) ? new float?(currentGroundedParams2.HeadPosition) : null) * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x0600138B RID: 5003 RVA: 0x000A80E4 File Offset: 0x000A62E4
		public override float? TorsoPosition
		{
			get
			{
				AnimationParams currentGroundedParams = this.CurrentGroundedParams;
				GroundedMovementParams currentGroundedParams2 = this.CurrentGroundedParams;
				return this.GetValidOrNull(currentGroundedParams, ((currentGroundedParams2 != null) ? new float?(currentGroundedParams2.TorsoPosition) : null) * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x0600138C RID: 5004 RVA: 0x000A814C File Offset: 0x000A634C
		public override float? HeadAngle
		{
			get
			{
				AnimationParams currentAnimationParams = this.CurrentAnimationParams;
				AnimationParams currentAnimationParams2 = this.CurrentAnimationParams;
				return this.GetValidOrNull(currentAnimationParams, (currentAnimationParams2 != null) ? new float?(currentAnimationParams2.HeadAngleInRadians) : null);
			}
		}

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x0600138D RID: 5005 RVA: 0x000A8184 File Offset: 0x000A6384
		public override float? TorsoAngle
		{
			get
			{
				AnimationParams currentAnimationParams = this.CurrentAnimationParams;
				AnimationParams currentAnimationParams2 = this.CurrentAnimationParams;
				return this.GetValidOrNull(currentAnimationParams, (currentAnimationParams2 != null) ? new float?(currentAnimationParams2.TorsoAngleInRadians) : null);
			}
		}

		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x0600138E RID: 5006 RVA: 0x000A81BC File Offset: 0x000A63BC
		public virtual Vector2? StepSize
		{
			get
			{
				return this.GetValidOrNull(this.CurrentGroundedParams, this.CurrentGroundedParams.StepSize * this.RagdollParams.JointScale);
			}
		}

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x0600138F RID: 5007 RVA: 0x000A81E5 File Offset: 0x000A63E5
		// (set) Token: 0x06001390 RID: 5008 RVA: 0x000A81ED File Offset: 0x000A63ED
		public bool AnimationTestPose { get; set; }

		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x06001391 RID: 5009 RVA: 0x000A81F6 File Offset: 0x000A63F6
		// (set) Token: 0x06001392 RID: 5010 RVA: 0x000A81FE File Offset: 0x000A63FE
		public float WalkPos { get; protected set; }

		// Token: 0x06001393 RID: 5011 RVA: 0x000A8208 File Offset: 0x000A6408
		public AnimController(Character character, string seed, RagdollParams ragdollParams = null) : base(character, seed, ragdollParams)
		{
			this.SerializableProperties = SerializableProperty.GetProperties(this);
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x000A8262 File Offset: 0x000A6462
		public void UpdateAnimations(float deltaTime)
		{
			this.UpdateTemporaryAnimations();
			this.UpdateAnim(deltaTime);
			this.CheckRopeState();
		}

		// Token: 0x06001395 RID: 5013
		protected abstract void UpdateAnim(float deltaTime);

		// Token: 0x06001396 RID: 5014
		public abstract void DragCharacter(Character target, float deltaTime);

		// Token: 0x06001397 RID: 5015 RVA: 0x000A8278 File Offset: 0x000A6478
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

		// Token: 0x06001398 RID: 5016 RVA: 0x000A8388 File Offset: 0x000A6588
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

		// Token: 0x06001399 RID: 5017 RVA: 0x000A83D8 File Offset: 0x000A65D8
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

		// Token: 0x0600139A RID: 5018 RVA: 0x000A8483 File Offset: 0x000A6683
		public float GetHeightFromFloor()
		{
			return base.GetColliderBottom().Y - base.FloorY;
		}

		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x0600139B RID: 5019 RVA: 0x000A8497 File Offset: 0x000A6697
		public bool IsAboveFloor
		{
			get
			{
				return this.GetHeightFromFloor() > -0.1f;
			}
		}

		// Token: 0x17000595 RID: 1429
		// (get) Token: 0x0600139C RID: 5020 RVA: 0x000A84A6 File Offset: 0x000A66A6
		// (set) Token: 0x0600139D RID: 5021 RVA: 0x000A84AE File Offset: 0x000A66AE
		public float FlipLockTime { get; private set; }

		// Token: 0x0600139E RID: 5022 RVA: 0x000A84B7 File Offset: 0x000A66B7
		public void LockFlipping(float time = 0.2f)
		{
			this.FlipLockTime = (float)Timing.TotalTime + time;
		}

		// Token: 0x0600139F RID: 5023 RVA: 0x000A84C8 File Offset: 0x000A66C8
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

		// Token: 0x060013A0 RID: 5024 RVA: 0x000A8588 File Offset: 0x000A6788
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

		// Token: 0x060013A1 RID: 5025 RVA: 0x000A85F0 File Offset: 0x000A67F0
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

		// Token: 0x060013A2 RID: 5026 RVA: 0x000A8898 File Offset: 0x000A6A98
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

		// Token: 0x060013A3 RID: 5027 RVA: 0x000A88F4 File Offset: 0x000A6AF4
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

		// Token: 0x060013A4 RID: 5028 RVA: 0x000A916C File Offset: 0x000A736C
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

		// Token: 0x060013A5 RID: 5029 RVA: 0x000A925C File Offset: 0x000A745C
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

		// Token: 0x060013A6 RID: 5030 RVA: 0x000A9470 File Offset: 0x000A7670
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
							goto IL_B1E;
						}
					}
					float movementMultiplier = this.targetMovement.Y >= 0f;
					head.body.SmoothRotate(0.7853982f * movementMultiplier * base.Dir, animParams.HeadTorque, true);
				}
			}
			IL_B1E:
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

		// Token: 0x060013A7 RID: 5031 RVA: 0x000AA068 File Offset: 0x000A8268
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

		// Token: 0x060013A8 RID: 5032 RVA: 0x000AA120 File Offset: 0x000A8320
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

		// Token: 0x060013A9 RID: 5033 RVA: 0x000AA2D4 File Offset: 0x000A84D4
		public void ApplyTestPose()
		{
			Limb waist = base.GetLimb(LimbType.Waist, true, false, false) ?? base.GetLimb(LimbType.Torso, true, false, false);
			if (waist != null)
			{
				this.ApplyPose(new Vector2(-0.75f, -0.2f), new Vector2(0.75f, -0.2f), new Vector2(-this.WalkParams.StepSize.X * 0.5f, -0.1f * this.RagdollParams.JointScale), new Vector2(this.WalkParams.StepSize.X * 0.5f, -0.1f * this.RagdollParams.JointScale), 10f);
			}
		}

		// Token: 0x060013AA RID: 5034 RVA: 0x000AA384 File Offset: 0x000A8584
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

		// Token: 0x060013AB RID: 5035 RVA: 0x000AA584 File Offset: 0x000A8784
		protected LimbJoint GetJointBetweenLimbs(LimbType limbTypeA, LimbType limbTypeB)
		{
			return this.LimbJoints.FirstOrDefault((LimbJoint lj) => (lj.LimbA.type == limbTypeA && lj.LimbB.type == limbTypeB) || (lj.LimbB.type == limbTypeA && lj.LimbA.type == limbTypeB));
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x000AA5BC File Offset: 0x000A87BC
		protected LimbJoint GetJoint(LimbType matchingType, IEnumerable<LimbType> ignoredTypes)
		{
			return this.LimbJoints.FirstOrDefault((LimbJoint lj) => (lj.LimbA.type == matchingType && ignoredTypes.None((LimbType t) => lj.LimbB.type == t)) || (lj.LimbB.type == matchingType && ignoredTypes.None((LimbType t) => lj.LimbB.type == t)));
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x000AA5F4 File Offset: 0x000A87F4
		public override void Recreate(RagdollParams ragdollParams = null)
		{
			base.Recreate(ragdollParams);
			if (base.Character.Params.CanInteract)
			{
				this.CalculateArmLengths();
			}
		}

		// Token: 0x060013AE RID: 5038 RVA: 0x000AA618 File Offset: 0x000A8818
		public void RecreateAndRespawn(RagdollParams ragdollParams = null)
		{
			Vector2 pos = this.character.WorldPosition;
			this.Recreate(ragdollParams);
			this.character.TeleportTo(pos);
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x000AA644 File Offset: 0x000A8844
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

		// Token: 0x060013B0 RID: 5040 RVA: 0x000AA690 File Offset: 0x000A8890
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

		// Token: 0x060013B1 RID: 5041 RVA: 0x000AA6C7 File Offset: 0x000A88C7
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

		// Token: 0x060013B2 RID: 5042 RVA: 0x000AA6FE File Offset: 0x000A88FE
		public void StartUsingItem()
		{
			this.StartAnimation(AnimController.Animation.UsingItem);
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x000AA707 File Offset: 0x000A8907
		public void StartClimbing()
		{
			this.StartAnimation(AnimController.Animation.Climbing);
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x000AA710 File Offset: 0x000A8910
		public void StopUsingItem()
		{
			this.StopAnimation(AnimController.Animation.UsingItem);
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x000AA719 File Offset: 0x000A8919
		public void StopClimbing()
		{
			this.StopAnimation(AnimController.Animation.Climbing);
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x000AA724 File Offset: 0x000A8924
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

		// Token: 0x060013B7 RID: 5047 RVA: 0x000AA80C File Offset: 0x000A8A0C
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

		// Token: 0x060013B8 RID: 5048 RVA: 0x000AA9A0 File Offset: 0x000A8BA0
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

		// Token: 0x060013B9 RID: 5049 RVA: 0x000AACE4 File Offset: 0x000A8EE4
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

		// Token: 0x060013BA RID: 5050 RVA: 0x000AAEB5 File Offset: 0x000A90B5
		[CompilerGenerated]
		private Vector2 <UpdateUseItem>g__GetTargetMovement|115_0(Vector2 dir)
		{
			return dir * this.GetCurrentSpeed(false) * Math.Max(this.character.SpeedMultiplier, 1f);
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x000AAEE0 File Offset: 0x000A90E0
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

		// Token: 0x060013BC RID: 5052 RVA: 0x000AAFD8 File Offset: 0x000A91D8
		[CompilerGenerated]
		private Limb <UpdateClimbing>g__GetClimbingLimb|123_1(LimbType limbType)
		{
			if (base.HasMultipleLimbsOfSameType)
			{
				return base.GetLimb(limbType, true, false, true) ?? base.GetLimb(limbType, true, true, false);
			}
			return base.GetLimb(limbType, true, false, false);
		}

		// Token: 0x0400093C RID: 2364
		protected LimbJoint rightShoulder;

		// Token: 0x0400093D RID: 2365
		protected LimbJoint leftShoulder;

		// Token: 0x0400093E RID: 2366
		protected float upperArmLength;

		// Token: 0x0400093F RID: 2367
		protected float forearmLength;

		// Token: 0x04000940 RID: 2368
		protected float useItemTimer;

		// Token: 0x04000941 RID: 2369
		protected bool aiming;

		// Token: 0x04000942 RID: 2370
		protected bool wasAiming;

		// Token: 0x04000943 RID: 2371
		protected bool aimingMelee;

		// Token: 0x04000944 RID: 2372
		protected bool wasAimingMelee;

		// Token: 0x04000945 RID: 2373
		protected readonly Dictionary<AnimationType, AnimController.AnimSwap> tempAnimations = new Dictionary<AnimationType, AnimController.AnimSwap>();

		// Token: 0x04000946 RID: 2374
		protected readonly HashSet<AnimationType> expiredAnimations = new HashSet<AnimationType>();

		// Token: 0x04000948 RID: 2376
		protected float deathAnimTimer;

		// Token: 0x04000949 RID: 2377
		protected float deathAnimDuration = 5f;

		// Token: 0x0400094A RID: 2378
		public AnimController.Animation Anim;

		// Token: 0x0400094E RID: 2382
		private Direction previousDirection;

		// Token: 0x0400094F RID: 2383
		private readonly Vector2[] transformedHandlePos = new Vector2[2];

		// Token: 0x04000950 RID: 2384
		private float prevFootPos;

		// Token: 0x04000951 RID: 2385
		private readonly Dictionary<AnimationType, AnimationParams> defaultAnimations = new Dictionary<AnimationType, AnimationParams>();

		// Token: 0x02000853 RID: 2131
		protected class AnimSwap
		{
			// Token: 0x0600548D RID: 21645 RVA: 0x001F0B16 File Offset: 0x001EED16
			public AnimSwap(AnimationParams temporaryAnimation, float priority)
			{
				this.AnimationType = temporaryAnimation.AnimationType;
				this.TemporaryAnimation = temporaryAnimation;
				this.Priority = priority;
				this.IsActive = true;
			}

			// Token: 0x04002F50 RID: 12112
			public readonly AnimationType AnimationType;

			// Token: 0x04002F51 RID: 12113
			public readonly AnimationParams TemporaryAnimation;

			// Token: 0x04002F52 RID: 12114
			public readonly float Priority;

			// Token: 0x04002F53 RID: 12115
			public bool IsActive;
		}

		// Token: 0x02000854 RID: 2132
		public enum Animation
		{
			// Token: 0x04002F55 RID: 12117
			None,
			// Token: 0x04002F56 RID: 12118
			Climbing,
			// Token: 0x04002F57 RID: 12119
			UsingItem,
			// Token: 0x04002F58 RID: 12120
			Struggle,
			// Token: 0x04002F59 RID: 12121
			CPR,
			// Token: 0x04002F5A RID: 12122
			UsingItemWhileClimbing
		}
	}
}
