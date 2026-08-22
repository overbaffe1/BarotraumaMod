using System;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A9 RID: 425
	internal class HumanoidAnimController : AnimController
	{
		// Token: 0x17000CB9 RID: 3257
		// (get) Token: 0x060030C6 RID: 12486 RVA: 0x001FF213 File Offset: 0x001FD413
		// (set) Token: 0x060030C7 RID: 12487 RVA: 0x001FF21B File Offset: 0x001FD41B
		public override RagdollParams RagdollParams
		{
			get
			{
				return this.HumanRagdollParams;
			}
			protected set
			{
				this.HumanRagdollParams = (value as HumanRagdollParams);
			}
		}

		// Token: 0x17000CBA RID: 3258
		// (get) Token: 0x060030C8 RID: 12488 RVA: 0x001FF22C File Offset: 0x001FD42C
		// (set) Token: 0x060030C9 RID: 12489 RVA: 0x001FF27B File Offset: 0x001FD47B
		public HumanRagdollParams HumanRagdollParams
		{
			get
			{
				if (this.character.Info == null)
				{
					if (this._ragdollParams == null)
					{
						this._ragdollParams = HumanRagdollParams.GetDefaultRagdollParams(this.character);
					}
					return this._ragdollParams;
				}
				return this.character.Info.Ragdoll as HumanRagdollParams;
			}
			protected set
			{
				if (this.character.Info == null)
				{
					this._ragdollParams = value;
					return;
				}
				this.character.Info.Ragdoll = value;
			}
		}

		// Token: 0x17000CBB RID: 3259
		// (get) Token: 0x060030CA RID: 12490 RVA: 0x001FF2A3 File Offset: 0x001FD4A3
		// (set) Token: 0x060030CB RID: 12491 RVA: 0x001FF2C4 File Offset: 0x001FD4C4
		public HumanWalkParams HumanWalkParams
		{
			get
			{
				if (this._humanWalkParams == null)
				{
					this._humanWalkParams = HumanWalkParams.GetDefaultAnimParams(this.character);
				}
				return this._humanWalkParams;
			}
			set
			{
				this._humanWalkParams = value;
			}
		}

		// Token: 0x17000CBC RID: 3260
		// (get) Token: 0x060030CC RID: 12492 RVA: 0x001FF2CD File Offset: 0x001FD4CD
		// (set) Token: 0x060030CD RID: 12493 RVA: 0x001FF2EE File Offset: 0x001FD4EE
		public HumanRunParams HumanRunParams
		{
			get
			{
				if (this._humanRunParams == null)
				{
					this._humanRunParams = HumanRunParams.GetDefaultAnimParams(this.character);
				}
				return this._humanRunParams;
			}
			set
			{
				this._humanRunParams = value;
			}
		}

		// Token: 0x17000CBD RID: 3261
		// (get) Token: 0x060030CE RID: 12494 RVA: 0x001FF2F7 File Offset: 0x001FD4F7
		// (set) Token: 0x060030CF RID: 12495 RVA: 0x001FF318 File Offset: 0x001FD518
		public HumanCrouchParams HumanCrouchParams
		{
			get
			{
				if (this._humanCrouchParams == null)
				{
					this._humanCrouchParams = HumanCrouchParams.GetDefaultAnimParams(this.character);
				}
				return this._humanCrouchParams;
			}
			set
			{
				this._humanCrouchParams = value;
			}
		}

		// Token: 0x17000CBE RID: 3262
		// (get) Token: 0x060030D0 RID: 12496 RVA: 0x001FF321 File Offset: 0x001FD521
		// (set) Token: 0x060030D1 RID: 12497 RVA: 0x001FF342 File Offset: 0x001FD542
		public HumanSwimSlowParams HumanSwimSlowParams
		{
			get
			{
				if (this._humanSwimSlowParams == null)
				{
					this._humanSwimSlowParams = HumanSwimSlowParams.GetDefaultAnimParams(this.character);
				}
				return this._humanSwimSlowParams;
			}
			set
			{
				this._humanSwimSlowParams = value;
			}
		}

		// Token: 0x17000CBF RID: 3263
		// (get) Token: 0x060030D2 RID: 12498 RVA: 0x001FF34B File Offset: 0x001FD54B
		// (set) Token: 0x060030D3 RID: 12499 RVA: 0x001FF36C File Offset: 0x001FD56C
		public HumanSwimFastParams HumanSwimFastParams
		{
			get
			{
				if (this._humanSwimFastParams == null)
				{
					this._humanSwimFastParams = HumanSwimFastParams.GetDefaultAnimParams(this.character);
				}
				return this._humanSwimFastParams;
			}
			set
			{
				this._humanSwimFastParams = value;
			}
		}

		// Token: 0x17000CC0 RID: 3264
		// (get) Token: 0x060030D4 RID: 12500 RVA: 0x001FF375 File Offset: 0x001FD575
		public new HumanGroundedParams CurrentGroundedParams
		{
			get
			{
				return base.CurrentGroundedParams as HumanGroundedParams;
			}
		}

		// Token: 0x17000CC1 RID: 3265
		// (get) Token: 0x060030D5 RID: 12501 RVA: 0x001FF382 File Offset: 0x001FD582
		public new HumanSwimParams CurrentSwimParams
		{
			get
			{
				return base.CurrentSwimParams as HumanSwimParams;
			}
		}

		// Token: 0x17000CC2 RID: 3266
		// (get) Token: 0x060030D6 RID: 12502 RVA: 0x001FF38F File Offset: 0x001FD58F
		public IHumanAnimation CurrentHumanAnimParams
		{
			get
			{
				return base.CurrentAnimationParams as IHumanAnimation;
			}
		}

		// Token: 0x17000CC3 RID: 3267
		// (get) Token: 0x060030D7 RID: 12503 RVA: 0x001FF39C File Offset: 0x001FD59C
		// (set) Token: 0x060030D8 RID: 12504 RVA: 0x001FF3A4 File Offset: 0x001FD5A4
		public override GroundedMovementParams WalkParams
		{
			get
			{
				return this.HumanWalkParams;
			}
			set
			{
				this.HumanWalkParams = (value as HumanWalkParams);
			}
		}

		// Token: 0x17000CC4 RID: 3268
		// (get) Token: 0x060030D9 RID: 12505 RVA: 0x001FF3B2 File Offset: 0x001FD5B2
		// (set) Token: 0x060030DA RID: 12506 RVA: 0x001FF3BA File Offset: 0x001FD5BA
		public override GroundedMovementParams RunParams
		{
			get
			{
				return this.HumanRunParams;
			}
			set
			{
				this.HumanRunParams = (value as HumanRunParams);
			}
		}

		// Token: 0x17000CC5 RID: 3269
		// (get) Token: 0x060030DB RID: 12507 RVA: 0x001FF3C8 File Offset: 0x001FD5C8
		// (set) Token: 0x060030DC RID: 12508 RVA: 0x001FF3D0 File Offset: 0x001FD5D0
		public override SwimParams SwimSlowParams
		{
			get
			{
				return this.HumanSwimSlowParams;
			}
			set
			{
				this.HumanSwimSlowParams = (value as HumanSwimSlowParams);
			}
		}

		// Token: 0x17000CC6 RID: 3270
		// (get) Token: 0x060030DD RID: 12509 RVA: 0x001FF3DE File Offset: 0x001FD5DE
		// (set) Token: 0x060030DE RID: 12510 RVA: 0x001FF3E6 File Offset: 0x001FD5E6
		public override SwimParams SwimFastParams
		{
			get
			{
				return this.HumanSwimFastParams;
			}
			set
			{
				this.HumanSwimFastParams = (value as HumanSwimFastParams);
			}
		}

		// Token: 0x17000CC7 RID: 3271
		// (get) Token: 0x060030DF RID: 12511 RVA: 0x001FF3F4 File Offset: 0x001FD5F4
		// (set) Token: 0x060030E0 RID: 12512 RVA: 0x001FF3FC File Offset: 0x001FD5FC
		public bool Crouching { get; set; }

		// Token: 0x17000CC8 RID: 3272
		// (get) Token: 0x060030E1 RID: 12513 RVA: 0x001FF405 File Offset: 0x001FD605
		public float HeadLeanAmount
		{
			get
			{
				return this.CurrentGroundedParams.HeadLeanAmount;
			}
		}

		// Token: 0x17000CC9 RID: 3273
		// (get) Token: 0x060030E2 RID: 12514 RVA: 0x001FF412 File Offset: 0x001FD612
		public float TorsoLeanAmount
		{
			get
			{
				return this.CurrentGroundedParams.TorsoLeanAmount;
			}
		}

		// Token: 0x17000CCA RID: 3274
		// (get) Token: 0x060030E3 RID: 12515 RVA: 0x001FF41F File Offset: 0x001FD61F
		public Vector2 FootMoveOffset
		{
			get
			{
				return this.CurrentGroundedParams.FootMoveOffset * this.RagdollParams.JointScale;
			}
		}

		// Token: 0x17000CCB RID: 3275
		// (get) Token: 0x060030E4 RID: 12516 RVA: 0x001FF43C File Offset: 0x001FD63C
		public float LegBendTorque
		{
			get
			{
				return this.CurrentGroundedParams.LegBendTorque * this.RagdollParams.JointScale;
			}
		}

		// Token: 0x17000CCC RID: 3276
		// (get) Token: 0x060030E5 RID: 12517 RVA: 0x001FF455 File Offset: 0x001FD655
		public Vector2 HandMoveOffset
		{
			get
			{
				return this.CurrentGroundedParams.HandMoveOffset * this.RagdollParams.JointScale;
			}
		}

		// Token: 0x17000CCD RID: 3277
		// (get) Token: 0x060030E6 RID: 12518 RVA: 0x001FF474 File Offset: 0x001FD674
		public override Vector2 AimSourceSimPos
		{
			get
			{
				float shoulderHeight = base.Collider.Height / 2f;
				if (this.inWater)
				{
					shoulderHeight += 0.4f;
				}
				else if (this.Crouching)
				{
					shoulderHeight -= 0.15f;
					if (this.Crouching && MathUtils.NearlyEqual(base.TargetMovement.X, 0f, 0.0001f))
					{
						shoulderHeight -= this.HumanCrouchParams.MoveDownAmountWhenStationary;
					}
				}
				return base.Collider.SimPosition + new Vector2((float)Math.Sin((double)(-(double)base.Collider.Rotation)), (float)Math.Cos((double)(-(double)base.Collider.Rotation))) * shoulderHeight;
			}
		}

		// Token: 0x060030E7 RID: 12519 RVA: 0x001FF530 File Offset: 0x001FD730
		public HumanoidAnimController(Character character, string seed, HumanRagdollParams ragdollParams = null) : base(character, seed, ragdollParams)
		{
			RagdollParams ragdollParams2 = this.RagdollParams;
			float? num;
			if (ragdollParams2 == null)
			{
				num = null;
			}
			else
			{
				ContentXElement mainElement = ragdollParams2.MainElement;
				num = ((mainElement != null) ? new float?(mainElement.GetAttributeFloat("movementlerp", 0.4f)) : null);
			}
			float? num2 = num;
			this.movementLerp = num2.GetValueOrDefault();
		}

		// Token: 0x060030E8 RID: 12520 RVA: 0x001FF591 File Offset: 0x001FD791
		public override void Recreate(RagdollParams ragdollParams = null)
		{
			base.Recreate(ragdollParams);
			this.CalculateLegLengths();
		}

		// Token: 0x060030E9 RID: 12521 RVA: 0x001FF5A0 File Offset: 0x001FD7A0
		private void CalculateLegLengths()
		{
			LimbType upperLegType = LimbType.RightThigh;
			LimbType lowerLegType = LimbType.RightLeg;
			LimbType footType = LimbType.RightFoot;
			LimbJoint waistJoint = base.GetJointBetweenLimbs(LimbType.Waist, upperLegType) ?? base.GetJointBetweenLimbs(LimbType.Torso, upperLegType);
			Vector2 localAnchorWaist = Vector2.Zero;
			Vector2 localAnchorKnee = Vector2.Zero;
			if (waistJoint != null)
			{
				localAnchorWaist = ((waistJoint.LimbA.type == upperLegType) ? waistJoint.LocalAnchorA : waistJoint.LocalAnchorB);
			}
			LimbJoint kneeJoint = base.GetJointBetweenLimbs(upperLegType, lowerLegType);
			if (kneeJoint != null)
			{
				localAnchorKnee = ((kneeJoint.LimbA.type == upperLegType) ? kneeJoint.LocalAnchorA : kneeJoint.LocalAnchorB);
			}
			this.upperLegLength = Vector2.Distance(localAnchorWaist, localAnchorKnee);
			LimbJoint ankleJoint = base.GetJointBetweenLimbs(lowerLegType, footType);
			if (ankleJoint == null || kneeJoint == null)
			{
				return;
			}
			this.lowerLegLength = Vector2.Distance((kneeJoint.LimbA.type == lowerLegType) ? kneeJoint.LocalAnchorA : kneeJoint.LocalAnchorB, (ankleJoint.LimbA.type == lowerLegType) ? ankleJoint.LocalAnchorA : ankleJoint.LocalAnchorB);
			this.lowerLegLength += Vector2.Distance((ankleJoint.LimbA.type == footType) ? ankleJoint.LocalAnchorA : ankleJoint.LocalAnchorB, base.GetLimb(footType, true, false, false).PullJointLocalAnchorA);
		}

		// Token: 0x060030EA RID: 12522 RVA: 0x001FF6D8 File Offset: 0x001FD8D8
		protected override void UpdateAnim(float deltaTime)
		{
			HumanoidAnimController.<>c__DisplayClass80_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (base.Frozen)
			{
				return;
			}
			if (base.MainLimb == null)
			{
				return;
			}
			base.UpdateConstantTorque(CS$<>8__locals1.deltaTime);
			base.UpdateBlink(CS$<>8__locals1.deltaTime);
			this.levitatingCollider = !base.IsHangingWithRope;
			if (this.onGround && this.character.CanMove)
			{
				Item selectedItem = this.character.SelectedItem;
				bool? flag;
				if (selectedItem == null)
				{
					flag = null;
				}
				else
				{
					Controller component = selectedItem.GetComponent<Controller>();
					flag = ((component != null) ? new bool?(component.ControlCharacterPose) : null);
				}
				bool? flag2 = flag;
				if (!flag2.GetValueOrDefault())
				{
					Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
					bool? flag3;
					if (selectedSecondaryItem == null)
					{
						flag3 = null;
					}
					else
					{
						Controller component2 = selectedSecondaryItem.GetComponent<Controller>();
						flag3 = ((component2 != null) ? new bool?(component2.ControlCharacterPose) : null);
					}
					flag2 = flag3;
					if (!flag2.GetValueOrDefault())
					{
						Item selectedSecondaryItem2 = this.character.SelectedSecondaryItem;
						if (((selectedSecondaryItem2 != null) ? selectedSecondaryItem2.GetComponent<Ladder>() : null) == null && (base.ForceSelectAnimationType == AnimationType.Crouch || base.ForceSelectAnimationType == AnimationType.NotDefined))
						{
							goto IL_119;
						}
					}
				}
				this.Crouching = false;
				IL_119:
				base.ColliderIndex = ((this.Crouching && !this.swimming) ? 1 : 0);
			}
			if (this.strongestImpact > 0f)
			{
				this.character.SetStun(MathHelper.Min(this.strongestImpact * 0.5f, 5f), false, false);
				this.strongestImpact = 0f;
				return;
			}
			if (this.character.IsDead)
			{
				if (this.deathAnimTimer < this.deathAnimDuration)
				{
					this.deathAnimTimer += CS$<>8__locals1.deltaTime;
					this.UpdateFallingProne(1f - this.deathAnimTimer / this.deathAnimDuration, true, true, true);
				}
			}
			else
			{
				this.deathAnimTimer = 0f;
			}
			if (!this.character.CanMove)
			{
				if (this.fallingProneAnimTimer < 1f && this.onGround)
				{
					this.fallingProneAnimTimer += CS$<>8__locals1.deltaTime;
					this.UpdateFallingProne(1f, true, true, true);
				}
				base.UpdateRagdollControlsMovement();
				return;
			}
			this.fallingProneAnimTimer = 0f;
			if (!base.Collider.Enabled)
			{
				Limb lowestLimb = base.FindLowestLimb();
				base.Collider.SetTransformIgnoreContacts(new Vector2(base.Collider.SimPosition.X, Math.Max(lowestLimb.SimPosition.Y + (base.Collider.Radius + base.Collider.Height / 2f), base.Collider.SimPosition.Y)), base.Collider.Rotation, true);
				base.Collider.FarseerBody.ResetDynamics();
				base.Collider.FarseerBody.LinearVelocity = base.MainLimb.LinearVelocity;
				base.Collider.Enabled = true;
			}
			if (this.swimming)
			{
				base.Collider.FarseerBody.FixedRotation = false;
			}
			else if (!base.Collider.FarseerBody.FixedRotation)
			{
				if (Math.Abs(MathUtils.GetShortestAngle(base.Collider.Rotation, 0f)) > 0.001f)
				{
					base.Collider.AngularVelocity = MathUtils.GetShortestAngle(base.Collider.Rotation, 0f) * 10f;
					base.Collider.FarseerBody.FixedRotation = false;
				}
				else
				{
					base.Collider.FarseerBody.FixedRotation = true;
				}
			}
			else
			{
				float angleDiff = MathUtils.GetShortestAngle(base.Collider.Rotation, 0f);
				if (Math.Abs(angleDiff) > 0.001f)
				{
					base.Collider.SetTransformIgnoreContacts(base.Collider.SimPosition, base.Collider.Rotation + angleDiff, true);
				}
			}
			if (this.character.AnimController.AnimationTestPose)
			{
				base.ApplyTestPose();
			}
			else if (this.character.SelectedBy == null)
			{
				if (this.character.LockHands)
				{
					Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
					Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
					Limb waist = base.GetLimb(LimbType.Waist, true, false, false) ?? base.GetLimb(LimbType.Torso, true, false, false);
					rightHand.Disabled = true;
					leftHand.Disabled = true;
					Vector2 midPos = waist.SimPosition;
					Matrix torsoTransform = Matrix.CreateRotationZ(waist.Rotation);
					midPos += Vector2.Transform(new Vector2(-0.3f * base.Dir, -0.2f), torsoTransform);
					if (rightHand.PullJointEnabled)
					{
						midPos = (midPos + rightHand.PullJointWorldAnchorB) / 2f;
					}
					base.HandIK(rightHand, midPos, base.CurrentAnimationParams.ArmIKStrength, base.CurrentAnimationParams.HandIKStrength, float.PositiveInfinity);
					base.HandIK(leftHand, midPos, base.CurrentAnimationParams.ArmIKStrength, base.CurrentAnimationParams.HandIKStrength, float.PositiveInfinity);
				}
				if (this.Anim != AnimController.Animation.UsingItem)
				{
					if (this.Anim != AnimController.Animation.UsingItemWhileClimbing)
					{
						base.ResetPullJoints(null);
					}
					else
					{
						base.ResetPullJoints((Limb l) => l.IsLowerBody);
					}
				}
			}
			if (base.SimplePhysicsEnabled)
			{
				this.UpdateStandingSimple();
				base.StopHangingWithRope();
				base.StopHoldingToRope();
				base.StopGettingDraggedWithRope();
				return;
			}
			if (this.character.SelectedCharacter != null)
			{
				this.DragCharacter(this.character.SelectedCharacter, CS$<>8__locals1.deltaTime);
			}
			if (this.Anim != AnimController.Animation.CPR)
			{
				this.cprAnimTimer = 0f;
				this.cprPumpTimer = 0f;
			}
			switch (this.Anim)
			{
			case AnimController.Animation.Climbing:
			case AnimController.Animation.UsingItemWhileClimbing:
				this.levitatingCollider = false;
				base.UpdateClimbing();
				this.<UpdateAnim>g__UpdateUseItemTimer|80_1(ref CS$<>8__locals1);
				goto IL_802;
			case AnimController.Animation.CPR:
				this.UpdateCPR(CS$<>8__locals1.deltaTime);
				goto IL_802;
			}
			this.<UpdateAnim>g__UpdateUseItemTimer|80_1(ref CS$<>8__locals1);
			this.swimmingStateLockTimer -= CS$<>8__locals1.deltaTime;
			if (this.forceStanding || this.character.AnimController.AnimationTestPose)
			{
				this.swimming = false;
			}
			else if (this.swimming != this.inWater && this.swimmingStateLockTimer <= 0f)
			{
				this.swimming = this.inWater;
				this.swimmingStateLockTimer = 0.5f;
			}
			Item selectedItem2 = this.character.SelectedItem;
			ItemPrefab itemPrefab = (selectedItem2 != null) ? selectedItem2.Prefab : null;
			if (itemPrefab != null && itemPrefab.GrabWhenSelected && this.character.SelectedItem.ParentInventory == null)
			{
				PhysicsBody body = this.character.SelectedItem.body;
				if (body == null || !body.Enabled)
				{
					Repairable component3 = this.character.SelectedItem.GetComponent<Repairable>();
					if (((component3 != null) ? component3.CurrentFixer : null) != this.character)
					{
						bool moving = this.character.IsKeyDown(InputType.Left) || this.character.IsKeyDown(InputType.Right);
						if (!(moving | ((this.character.InWater || this.character.IsClimbing) && (this.character.IsKeyDown(InputType.Up) || this.character.IsKeyDown(InputType.Down)))))
						{
							Vector2 handPos = this.character.SelectedItem.WorldPosition - Vector2.UnitY * ConvertUnits.ToDisplayUnits(base.ArmLength / 2f);
							handPos.Y = Math.Max(handPos.Y, (float)(this.character.SelectedItem.WorldRect.Y - this.character.SelectedItem.WorldRect.Height));
							base.UpdateUseItem(false, handPos);
						}
					}
				}
			}
			if (this.swimming)
			{
				this.UpdateSwimming();
			}
			else
			{
				if (this.character.SelectedItem != null)
				{
					Item selectedSecondaryItem3 = this.character.SelectedSecondaryItem;
					Controller controller = (selectedSecondaryItem3 != null) ? selectedSecondaryItem3.GetComponent<Controller>() : null;
					if (controller != null && controller.ControlCharacterPose && controller.UserInCorrectPosition)
					{
						goto IL_802;
					}
				}
				this.UpdateStanding();
			}
			IL_802:
			if (Timing.TotalTime > (double)base.FlipLockTime && this.TargetDir != this.dir && !base.IsStuck)
			{
				this.Flip();
			}
			foreach (Limb limb in base.Limbs)
			{
				limb.Disabled = false;
			}
			this.wasAiming = this.aiming;
			this.aiming = false;
			this.wasAimingMelee = this.aimingMelee;
			this.aimingMelee = false;
		}

		// Token: 0x060030EB RID: 12523 RVA: 0x001FFF64 File Offset: 0x001FE164
		private void UpdateStanding()
		{
			HumanGroundedParams currentGroundedParams = this.CurrentGroundedParams;
			if (currentGroundedParams == null)
			{
				return;
			}
			Limb leftFoot = base.GetLimb(LimbType.LeftFoot, true, false, false);
			Limb rightFoot = base.GetLimb(LimbType.RightFoot, true, false, false);
			Limb head = base.GetLimb(LimbType.Head, true, false, false);
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			Limb waist = base.GetLimb(LimbType.Waist, true, false, false);
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			Limb leftLeg = base.GetLimb(LimbType.LeftLeg, true, false, false);
			Limb rightLeg = base.GetLimb(LimbType.RightLeg, true, false, false);
			bool onSlopeThatMakesSlow = Math.Abs(this.floorNormal.X) > HumanoidAnimController.SlowlyWalkableSlopeNormalX;
			bool slowedDownBySlope = onSlopeThatMakesSlow && Math.Sign(this.floorNormal.X) == -Math.Sign(base.TargetMovement.X);
			bool onSlopeTooSteepToClimb = Math.Abs(this.floorNormal.X) > HumanoidAnimController.SteepestWalkableSlopeNormalX;
			float walkCycleMultiplier = 1f;
			if (this.Stairs != null || slowedDownBySlope)
			{
				base.TargetMovement = new Vector2(MathHelper.Clamp(base.TargetMovement.X, -1.7f, 1.7f), base.TargetMovement.Y);
				walkCycleMultiplier *= 1.5f;
			}
			float getUpForce = currentGroundedParams.GetUpForce / this.RagdollParams.JointScale;
			Vector2 colliderPos = base.GetColliderBottom();
			if (Math.Abs(base.TargetMovement.X) > 1f)
			{
				float slowdownAmount = 0f;
				if (this.currentHull != null)
				{
					this.surfaceY = ConvertUnits.ToSimUnits(this.currentHull.Surface);
					float bottomPos = Math.Max(colliderPos.Y, ConvertUnits.ToSimUnits(this.currentHull.Rect.Y - this.currentHull.Rect.Height));
					slowdownAmount = MathHelper.Clamp((this.surfaceY - bottomPos) / this.TorsoPosition.Value, 0f, 1f) * 1.5f;
				}
				float maxSpeed = Math.Max(base.TargetMovement.Length() - slowdownAmount, 1f);
				base.TargetMovement = Vector2.Normalize(base.TargetMovement) * maxSpeed;
			}
			float walkPosX = (float)Math.Cos((double)base.WalkPos);
			float walkPosY = (float)Math.Sin((double)base.WalkPos);
			Vector2 stepSize = this.StepSize.Value;
			stepSize.X *= walkPosX;
			stepSize.Y *= walkPosY;
			float footMid = colliderPos.X;
			Affliction herpes = this.character.CharacterHealth.GetAffliction("spaceherpes", false);
			float herpesAmount = (herpes == null) ? 0f : (herpes.Strength / herpes.Prefab.MaxStrength);
			float legDamage = this.character.GetLegPenalty(-0.1f) * 1.1f;
			float limpAmount = MathHelper.Lerp(0f, 1f, legDamage + herpesAmount);
			if (limpAmount > 0f)
			{
				footMid += Math.Max(Math.Abs(walkPosX) * limpAmount, 0f) * Math.Min(Math.Abs(base.TargetMovement.X), 0.3f) * base.Dir;
			}
			this.movement = (this.overrideTargetMovement ?? MathUtils.SmoothStep(this.movement, base.TargetMovement, this.movementLerp));
			if (Math.Abs(this.movement.X) < 0.005f)
			{
				this.movement.X = 0f;
			}
			this.movement.Y = 0f;
			if (head == null)
			{
				return;
			}
			if (torso == null)
			{
				return;
			}
			bool isNotRemote = true;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				isNotRemote = !this.character.IsRemotelyControlled;
			}
			if (this.onGround && isNotRemote)
			{
				float rotationFactor = (float)Math.Abs(Math.Cos((double)base.Collider.Rotation));
				base.Collider.LinearVelocity = new Vector2(this.movement.X * rotationFactor, (base.Collider.LinearVelocity.Y > 0f) ? (base.Collider.LinearVelocity.Y * 0.5f) : base.Collider.LinearVelocity.Y);
			}
			getUpForce *= Math.Max(head.SimPosition.Y - colliderPos.Y, 0.5f);
			torso.PullJointEnabled = true;
			head.PullJointEnabled = true;
			if (waist != null)
			{
				waist.PullJointEnabled = true;
			}
			bool onSlope = Math.Abs(this.movement.X) > 0.01f && Math.Abs(this.floorNormal.X) > 0.1f && Math.Sign(this.floorNormal.X) != Math.Sign(this.movement.X);
			bool movingHorizontally = !MathUtils.NearlyEqual(base.TargetMovement.X, 0f, 0.0001f);
			if (this.Stairs == null && onSlopeTooSteepToClimb && Math.Sign(this.targetMovement.X) != Math.Sign(this.floorNormal.X))
			{
				this.targetMovement.X = (float)Math.Sign(this.floorNormal.X) * 1.7f;
				this.movement = this.targetMovement;
			}
			if (this.Stairs != null || onSlope)
			{
				torso.PullJointWorldAnchorB = new Vector2(MathHelper.SmoothStep(torso.SimPosition.X, footMid + this.movement.X * this.TorsoLeanAmount, getUpForce * 0.8f), MathHelper.SmoothStep(torso.SimPosition.Y, colliderPos.Y + this.TorsoPosition.Value - Math.Abs(walkPosX * 0.05f), getUpForce * 2f));
				head.PullJointWorldAnchorB = new Vector2(MathHelper.SmoothStep(head.SimPosition.X, footMid + this.movement.X * this.HeadLeanAmount, getUpForce * 0.8f), MathHelper.SmoothStep(head.SimPosition.Y, colliderPos.Y + this.HeadPosition.Value - Math.Abs(walkPosX * 0.05f), getUpForce * 2f));
				if (waist != null)
				{
					waist.PullJointWorldAnchorB = waist.SimPosition - this.movement * 0.06f;
				}
			}
			else
			{
				if (!this.onGround)
				{
					this.movement = Vector2.Zero;
				}
				float offset = 3.1415927f * currentGroundedParams.StepLiftOffset;
				if (this.character.AnimController.Dir < 0f)
				{
					offset += 3.1415927f * currentGroundedParams.StepLiftFrequency;
				}
				float stepLift = (base.TargetMovement.X == 0f) ? 0f : ((float)Math.Sin((double)(base.WalkPos * base.Dir * currentGroundedParams.StepLiftFrequency + offset)) * (currentGroundedParams.StepLiftAmount / 100f));
				float y = colliderPos.Y + stepLift;
				if (!torso.Disabled)
				{
					if (this.TorsoPosition != null)
					{
						y += this.TorsoPosition.Value;
					}
					if (this.Crouching && !movingHorizontally)
					{
						y -= this.HumanCrouchParams.MoveDownAmountWhenStationary;
					}
					torso.PullJointWorldAnchorB = MathUtils.SmoothStep(torso.SimPosition, new Vector2(footMid + this.movement.X * this.TorsoLeanAmount, y), getUpForce);
				}
				if (!head.Disabled)
				{
					y = colliderPos.Y + stepLift * currentGroundedParams.StepLiftHeadMultiplier;
					if (this.HeadPosition != null)
					{
						y += this.HeadPosition.Value;
					}
					if (this.Crouching && !movingHorizontally)
					{
						y -= this.HumanCrouchParams.MoveDownAmountWhenStationary;
					}
					head.PullJointWorldAnchorB = MathUtils.SmoothStep(head.SimPosition, new Vector2(footMid + this.movement.X * this.HeadLeanAmount, y), getUpForce * 1.2f);
				}
				if (waist != null && !waist.Disabled)
				{
					waist.PullJointWorldAnchorB = waist.SimPosition + this.movement * 0.06f;
				}
			}
			if (this.TorsoAngle != null && !torso.Disabled)
			{
				float torsoAngle = this.TorsoAngle.Value;
				float herpesStrength = this.character.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.SpaceHerpesType, true);
				if (this.Crouching && !movingHorizontally && !base.Aiming)
				{
					torsoAngle -= this.HumanCrouchParams.ExtraTorsoAngleWhenStationary;
				}
				torsoAngle -= herpesStrength / 150f;
				torso.body.SmoothRotate(torsoAngle * base.Dir, currentGroundedParams.TorsoTorque, true);
			}
			if (!head.Disabled)
			{
				if (!base.Aiming && currentGroundedParams.FixedHeadAngle && this.HeadAngle != null)
				{
					float headAngle = this.HeadAngle.Value;
					if (this.Crouching && !movingHorizontally)
					{
						headAngle -= this.HumanCrouchParams.ExtraHeadAngleWhenStationary;
					}
					head.body.SmoothRotate(headAngle * base.Dir, currentGroundedParams.HeadTorque, true);
				}
				else
				{
					base.RotateHead(head);
				}
			}
			if (!this.onGround)
			{
				if ((leftFoot != null && (base.MainLimb.LinearVelocity - leftFoot.LinearVelocity).LengthSquared() > 25f) || (rightFoot != null && (base.MainLimb.LinearVelocity - rightFoot.LinearVelocity).LengthSquared() > 25f))
				{
					this.UpdateFallingProne(10f, false, false, true);
				}
				return;
			}
			Vector2 waistPos = (waist != null) ? waist.SimPosition : torso.SimPosition;
			if (movingHorizontally)
			{
				base.WalkPos -= MathHelper.ToRadians(base.CurrentAnimationParams.CycleSpeed) * walkCycleMultiplier * this.movement.X;
				for (int i = -1; i < 2; i += 2)
				{
					Limb foot = (i == -1) ? leftFoot : rightFoot;
					if (foot != null)
					{
						Vector2 footPos = stepSize * (float)(-(float)i);
						footPos += new Vector2((float)Math.Sign(this.movement.X) * this.FootMoveOffset.X, this.FootMoveOffset.Y);
						if (footPos.Y < 0f)
						{
							footPos.Y = -0.15f;
						}
						float footAfflictionStrength = this.character.CharacterHealth.GetAfflictionStrength(AfflictionPrefab.DamageType, foot, true);
						footPos.X *= MathHelper.Lerp(1f, 0.75f, MathHelper.Clamp(footAfflictionStrength / 50f, 0f, 1f));
						if (currentGroundedParams.FootLiftHorizontalFactor > 0f)
						{
							float xDiff = (foot.SimPosition.X - waistPos.X + this.FootMoveOffset.X) * base.Dir;
							float min = MathUtils.InverseLerp(1f, 0f, currentGroundedParams.FootLiftHorizontalFactor);
							float max = 1f + MathUtils.InverseLerp(0f, 1f, currentGroundedParams.FootLiftHorizontalFactor);
							float xFactor = MathHelper.Lerp(min, max, MathUtils.InverseLerp(this.RagdollParams.JointScale, -this.RagdollParams.JointScale, xDiff));
							footPos.Y *= xFactor;
						}
						if (onSlope && this.Stairs == null)
						{
							footPos.Y *= 2f;
						}
						footPos.Y = Math.Min(waistPos.Y - colliderPos.Y - 0.4f, footPos.Y);
						if ((i == 1 && Math.Sign(Math.Sin((double)base.WalkPos)) > 0 && Math.Sign(walkPosY) < 0) || (i == -1 && Math.Sign(Math.Sin((double)base.WalkPos)) < 0 && Math.Sign(walkPosY) > 0))
						{
							base.PlayImpactSound(foot);
						}
						if (!foot.Disabled)
						{
							foot.DebugRefPos = colliderPos;
							foot.DebugTargetPos = colliderPos + footPos;
							base.MoveLimb(foot, colliderPos + footPos, currentGroundedParams.FootMoveStrength, false);
							this.FootIK(foot, colliderPos + footPos, currentGroundedParams.LegBendTorque, currentGroundedParams.FootTorque, currentGroundedParams.FootAngleInRadians);
						}
					}
				}
				Vector2 handPos = torso.SimPosition;
				handPos.X = -walkPosX * currentGroundedParams.HandMoveAmount.X;
				float lowerY = currentGroundedParams.HandClampY;
				handPos.Y = lowerY + (float)Math.Abs(Math.Sin((double)base.WalkPos - 4.71238898038469) * (double)currentGroundedParams.HandMoveAmount.Y);
				Vector2 posAddition = new Vector2((float)Math.Sign(this.movement.X) * this.HandMoveOffset.X, this.HandMoveOffset.Y);
				if (rightHand != null && !rightHand.Disabled)
				{
					base.HandIK(rightHand, torso.SimPosition + posAddition + new Vector2(-handPos.X, (Math.Sign(walkPosX) == Math.Sign(base.Dir)) ? handPos.Y : lowerY), currentGroundedParams.ArmMoveStrength, currentGroundedParams.HandMoveStrength, float.PositiveInfinity);
				}
				if (leftHand != null && !leftHand.Disabled)
				{
					base.HandIK(leftHand, torso.SimPosition + posAddition + new Vector2(handPos.X, (Math.Sign(walkPosX) == Math.Sign(-base.Dir)) ? handPos.Y : lowerY), currentGroundedParams.ArmMoveStrength, currentGroundedParams.HandMoveStrength, float.PositiveInfinity);
					return;
				}
			}
			else
			{
				for (int j = -1; j < 2; j += 2)
				{
					Vector2 footPos2 = colliderPos;
					if (this.Crouching)
					{
						footPos2 = new Vector2((float)Math.Sign(stepSize.X * (float)j) * base.Dir * 0.35f, colliderPos.Y);
						if (Math.Sign(footPos2.X) != Math.Sign(base.Dir))
						{
							footPos2.Y += 0.15f;
						}
						footPos2.X += colliderPos.X;
					}
					else
					{
						float footPosX = stepSize.X * (float)j * 0.2f;
						if (this.CurrentGroundedParams.StepSizeWhenStanding != Vector2.Zero)
						{
							footPosX = (float)Math.Sign(stepSize.X) * this.CurrentGroundedParams.StepSizeWhenStanding.X * (float)j;
						}
						footPos2 = new Vector2(colliderPos.X + footPosX, colliderPos.Y - 0.1f);
					}
					if (this.Stairs == null && !onSlopeThatMakesSlow)
					{
						footPos2.Y = Math.Max(Math.Min(base.FloorY, footPos2.Y + 0.5f), footPos2.Y);
					}
					Limb foot2 = (j == -1) ? rightFoot : leftFoot;
					if (foot2 != null && !foot2.Disabled)
					{
						foot2.DebugRefPos = colliderPos;
						foot2.DebugTargetPos = footPos2;
						float footMoveForce = currentGroundedParams.FootMoveStrength;
						float legBendTorque = currentGroundedParams.LegBendTorque;
						if (this.Crouching)
						{
							legBendTorque = 100f;
							footMoveForce *= 2f;
						}
						base.MoveLimb(foot2, footPos2, footMoveForce, false);
						this.FootIK(foot2, footPos2, legBendTorque, currentGroundedParams.FootTorque, currentGroundedParams.FootAngleInRadians);
					}
				}
				for (int k = 0; k < 2; k++)
				{
					Limb hand = (k == 0) ? rightHand : leftHand;
					if (hand != null && !hand.Disabled)
					{
						LimbType armType = (k == 0) ? LimbType.RightArm : LimbType.LeftArm;
						LimbType foreArmType = (k == 0) ? LimbType.RightForearm : LimbType.LeftForearm;
						Limb arm = base.GetLimb(armType, true, false, false);
						if (arm != null && Math.Abs(arm.body.AngularVelocity) < 10f)
						{
							arm.body.SmoothRotate(MathHelper.Clamp(-arm.body.AngularVelocity, -0.5f, 0.5f), arm.Mass * 50f * currentGroundedParams.ArmMoveStrength, true);
						}
						if (Math.Abs(hand.body.AngularVelocity) < 10f)
						{
							Limb forearm = base.GetLimb(foreArmType, true, false, false) ?? hand;
							LimbJoint elbow = base.GetJointBetweenLimbs(armType, foreArmType) ?? base.GetJointBetweenLimbs(armType, hand.type);
							if (elbow != null)
							{
								float diff = elbow.JointAngle - ((base.Dir > 0f) ? elbow.LowerLimit : elbow.UpperLimit);
								forearm.body.ApplyTorque(MathHelper.Clamp(-diff, -1.5707964f, 1.5707964f) * forearm.Mass * 100f * currentGroundedParams.ArmMoveStrength);
							}
						}
						LimbJoint wrist = base.GetJointBetweenLimbs(foreArmType, hand.type);
						if (wrist != null)
						{
							hand.body.ApplyTorque(MathHelper.Clamp(-wrist.JointAngle, -1.5707964f, 1.5707964f) * hand.Mass * 100f * currentGroundedParams.HandMoveStrength);
						}
					}
				}
			}
		}

		// Token: 0x060030EC RID: 12524 RVA: 0x00201090 File Offset: 0x001FF290
		private void UpdateStandingSimple()
		{
			if (Math.Abs(this.movement.X) < 0.005f)
			{
				this.movement.X = 0f;
			}
			this.movement = MathUtils.SmoothStep(this.movement, base.TargetMovement, this.movementLerp);
			if (base.InWater)
			{
				base.Collider.LinearVelocity = this.movement;
				return;
			}
			if (this.onGround && (!this.character.IsRemotelyControlled || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)))
			{
				base.Collider.LinearVelocity = new Vector2(this.movement.X, (base.Collider.LinearVelocity.Y > 0f) ? (base.Collider.LinearVelocity.Y * 0.5f) : base.Collider.LinearVelocity.Y);
			}
		}

		// Token: 0x060030ED RID: 12525 RVA: 0x00201180 File Offset: 0x001FF380
		private void UpdateSwimming()
		{
			if (this.CurrentSwimParams == null)
			{
				return;
			}
			base.IgnorePlatforms = true;
			float surfaceLimiter = 1f;
			Limb head = base.GetLimb(LimbType.Head, true, false, false);
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			if (head == null)
			{
				return;
			}
			if (torso == null)
			{
				return;
			}
			if (this.currentHull != null && this.character.CurrentHull != null)
			{
				float surfacePos = base.GetSurfaceY();
				float surfaceThreshold = ConvertUnits.ToDisplayUnits(base.Collider.SimPosition.Y + 1f);
				surfaceLimiter = Math.Max(1f, surfaceThreshold - surfacePos);
			}
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			Limb leftFoot = base.GetLimb(LimbType.LeftFoot, true, false, false);
			Limb rightFoot = base.GetLimb(LimbType.RightFoot, true, false, false);
			float rotation = MathHelper.WrapAngle(base.Collider.Rotation);
			rotation = MathHelper.ToDegrees(rotation);
			if (rotation < 0f)
			{
				rotation += 360f;
			}
			float targetSpeed = base.TargetMovement.Length();
			if (targetSpeed > 0.1f && !this.character.IsRemotelyControlled && !base.Aiming && !base.IsUsingItem)
			{
				Item selectedItem = this.character.SelectedItem;
				bool? flag;
				if (selectedItem == null)
				{
					flag = null;
				}
				else
				{
					Controller component = selectedItem.GetComponent<Controller>();
					flag = ((component != null) ? new bool?(component.ControlCharacterPose) : null);
				}
				bool? flag2 = flag;
				if (!flag2.GetValueOrDefault())
				{
					Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
					bool? flag3;
					if (selectedSecondaryItem == null)
					{
						flag3 = null;
					}
					else
					{
						Controller component2 = selectedSecondaryItem.GetComponent<Controller>();
						flag3 = ((component2 != null) ? new bool?(component2.ControlCharacterPose) : null);
					}
					flag2 = flag3;
					if (!flag2.GetValueOrDefault())
					{
						if (rotation > 20f && rotation < 170f)
						{
							this.TargetDir = Direction.Left;
						}
						else if (rotation > 190f && rotation < 340f)
						{
							this.TargetDir = Direction.Right;
						}
					}
				}
			}
			if (base.Aiming)
			{
				Vector2 mousePos = ConvertUnits.ToSimUnits(this.character.CursorPosition);
				Vector2 diff = (mousePos - torso.SimPosition) * base.Dir;
				if (diff.LengthSquared() > MathUtils.Pow2(0.4f))
				{
					float newRotation = MathHelper.WrapAngle(MathUtils.VectorToAngle(diff) - 0.7853982f * base.Dir);
					base.Collider.SmoothRotate(newRotation, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier, true);
				}
			}
			else if (targetSpeed > 0.1f)
			{
				float newRotation2 = MathUtils.VectorToAngle(base.TargetMovement) - 1.5707964f;
				base.Collider.SmoothRotate(newRotation2, this.CurrentSwimParams.SteerTorque * this.character.SpeedMultiplier, true);
			}
			torso.body.MoveToPos(base.Collider.SimPosition + new Vector2((float)Math.Sin((double)(-(double)base.Collider.Rotation)), (float)Math.Cos((double)(-(double)base.Collider.Rotation))) * 0.4f, 5f, null);
			this.movement = MathUtils.SmoothStep(this.movement, base.TargetMovement, 0.3f);
			if (this.TorsoAngle != null)
			{
				torso.body.SmoothRotate(base.Collider.Rotation + this.TorsoAngle.Value * base.Dir, this.CurrentSwimParams.TorsoTorque, true);
			}
			else
			{
				torso.body.SmoothRotate(base.Collider.Rotation, this.CurrentSwimParams.TorsoTorque, true);
			}
			if (!base.Aiming && this.CurrentSwimParams.FixedHeadAngle && this.HeadAngle != null)
			{
				head.body.SmoothRotate(base.Collider.Rotation + this.HeadAngle.Value * base.Dir, this.CurrentSwimParams.HeadTorque, true);
			}
			else if (this.character.FollowCursor)
			{
				base.RotateHead(head);
			}
			if (surfaceLimiter > 1f && base.TargetMovement.Y > 0f)
			{
				if (base.TargetMovement.X == 0f)
				{
					head.body.SmoothRotate(0f, 5f, true);
					base.WalkPos += 0.05f;
				}
				else
				{
					base.TargetMovement = new Vector2((float)Math.Sqrt((double)(targetSpeed * targetSpeed - base.TargetMovement.Y * base.TargetMovement.Y)) * (float)Math.Sign(base.TargetMovement.X), Math.Max(base.TargetMovement.Y, base.TargetMovement.Y * 0.2f));
					head.body.ApplyTorque(base.Dir);
				}
				this.movement.Y = this.movement.Y * Math.Max(0f, 1f - (surfaceLimiter - 1f) / 50f);
			}
			bool isNotRemote = true;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				isNotRemote = !this.character.IsRemotelyControlled;
			}
			if (isNotRemote)
			{
				float t = this.movementLerp;
				if (targetSpeed > 1E-05f && !base.SimplePhysicsEnabled)
				{
					Vector2 forward = VectorExtensions.Forward(base.Collider.Rotation + 1.5707964f, 1f);
					float dot = Vector2.Dot(forward, Vector2.Normalize(this.movement));
					if (dot < 0f)
					{
						t = MathHelper.Clamp((1f + dot) / 10f, 0.01f, 0.1f);
					}
				}
				Vector2 targetVelocity = this.movement;
				if (surfaceLimiter > 50f)
				{
					targetVelocity.Y = Math.Min(base.Collider.LinearVelocity.Y, this.movement.Y);
				}
				base.Collider.LinearVelocity = Vector2.Lerp(base.Collider.LinearVelocity, targetVelocity, t);
			}
			base.WalkPos += this.movement.Length();
			this.legCyclePos += Math.Min(this.movement.LengthSquared() + base.Collider.AngularVelocity, 1f);
			this.handCyclePos += MathHelper.ToRadians(this.CurrentSwimParams.HandCycleSpeed) * (float)Math.Sign(this.movement.X);
			float legMoveMultiplier = 1f;
			if (this.movement.LengthSquared() < 0.001f)
			{
				legMoveMultiplier = 0.3f;
				this.legCyclePos += 0.4f;
				this.handCyclePos += 0.1f;
			}
			Limb waist = base.GetLimb(LimbType.Waist, true, false, false) ?? base.GetLimb(LimbType.Torso, true, false, false);
			Vector2 footPos = (waist == null) ? Vector2.Zero : (waist.SimPosition - new Vector2((float)Math.Sin((double)(-(double)base.Collider.Rotation)), (float)Math.Cos((double)(-(double)base.Collider.Rotation))) * (this.upperLegLength + this.lowerLegLength));
			Vector2 transformedFootPos = new Vector2((float)Math.Sin((double)(this.legCyclePos / this.CurrentSwimParams.LegCycleLength)) * this.CurrentSwimParams.LegMoveAmount * legMoveMultiplier, 0f);
			transformedFootPos = Vector2.Transform(transformedFootPos, Matrix.CreateRotationZ(base.Collider.Rotation));
			float legTorque = this.CurrentSwimParams.LegTorque * this.character.SpeedMultiplier * (1.2f - this.character.GetLegPenalty(0f));
			if (rightFoot != null && !rightFoot.Disabled)
			{
				this.FootIK(rightFoot, footPos - transformedFootPos, legTorque, this.CurrentSwimParams.FootTorque, this.CurrentSwimParams.FootAngleInRadians);
			}
			if (leftFoot != null && !leftFoot.Disabled)
			{
				this.FootIK(leftFoot, footPos + transformedFootPos, legTorque, this.CurrentSwimParams.FootTorque, this.CurrentSwimParams.FootAngleInRadians);
			}
			Vector2 handPos = (torso.SimPosition + head.SimPosition) / 2f;
			if ((!this.headInWater && base.TargetMovement.X == 0f && base.TargetMovement.Y > 0f) || base.TargetMovement.LengthSquared() < 0.001f)
			{
				handPos += MathUtils.RotatePoint(Vector2.UnitX * base.Dir * 0.2f, torso.Rotation);
				float wobbleAmount = 0.1f;
				if (rightHand != null && !rightHand.Disabled)
				{
					base.MoveLimb(rightHand, new Vector2(handPos.X + (float)Math.Sin((double)(this.handCyclePos / 1.5f)) * wobbleAmount, handPos.Y + (float)Math.Sin((double)(this.handCyclePos / 3.5f)) * wobbleAmount - 0.25f), this.CurrentSwimParams.ArmMoveStrength, false);
				}
				if (leftHand != null && !leftHand.Disabled)
				{
					base.MoveLimb(leftHand, new Vector2(handPos.X + (float)Math.Sin((double)(this.handCyclePos / 2f)) * wobbleAmount, handPos.Y + (float)Math.Sin((double)(this.handCyclePos / 3f)) * wobbleAmount - 0.25f), this.CurrentSwimParams.ArmMoveStrength, false);
				}
				return;
			}
			handPos += head.LinearVelocity.ClampLength(1f) * 0.1f;
			Vector2 handMoveAmount = this.CurrentSwimParams.HandMoveAmount.Flip();
			Vector2 handMoveOffset = this.CurrentSwimParams.HandMoveOffset.Flip();
			float handPosX = (float)Math.Cos((double)this.handCyclePos) * handMoveAmount.X * base.CurrentAnimationParams.CycleSpeed;
			float handPosY = (float)Math.Sin((double)this.handCyclePos) * handMoveAmount.Y * base.CurrentAnimationParams.CycleSpeed;
			Matrix rotationMatrix = Matrix.CreateRotationZ(torso.Rotation);
			if (rightHand != null && !rightHand.Disabled)
			{
				Vector2 rightHandPos = new Vector2(-handPosX, -handPosY) + handMoveOffset;
				rightHandPos.X = ((base.Dir == 1f) ? Math.Max(0.3f, rightHandPos.X) : Math.Min(-0.3f, rightHandPos.X));
				rightHandPos = Vector2.Transform(rightHandPos, rotationMatrix);
				float speedMultiplier = Math.Min(this.character.SpeedMultiplier * (1f - base.Character.GetRightHandPenalty()), 1f);
				if (this.character.Inventory != null && this.character.Inventory.GetItemInLimbSlot(InvSlotType.RightHand) != null)
				{
					speedMultiplier = Math.Min(speedMultiplier, 0.1f);
				}
				base.HandIK(rightHand, handPos + rightHandPos, this.CurrentSwimParams.ArmMoveStrength * speedMultiplier, this.CurrentSwimParams.HandMoveStrength * speedMultiplier, float.PositiveInfinity);
				LimbJoint wrist = base.GetJointBetweenLimbs(LimbType.RightForearm, LimbType.RightHand);
				if (wrist != null)
				{
					rightHand.body.ApplyTorque(MathHelper.Clamp(-wrist.JointAngle, -1.5707964f, 1.5707964f) * rightHand.Mass * 100f * this.CurrentSwimParams.HandMoveStrength);
				}
			}
			if (leftHand != null && !leftHand.Disabled)
			{
				Vector2 leftHandPos = new Vector2(handPosX, handPosY) + handMoveOffset;
				leftHandPos.X = ((base.Dir == 1f) ? Math.Max(0.3f, leftHandPos.X) : Math.Min(-0.3f, leftHandPos.X));
				leftHandPos = Vector2.Transform(leftHandPos, rotationMatrix);
				float speedMultiplier2 = Math.Min(this.character.SpeedMultiplier * (1f - base.Character.GetLeftHandPenalty()), 1f);
				if (this.character.Inventory != null && this.character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand) != null)
				{
					speedMultiplier2 = Math.Min(speedMultiplier2, 0.1f);
				}
				base.HandIK(leftHand, handPos + leftHandPos, this.CurrentSwimParams.ArmMoveStrength * speedMultiplier2, this.CurrentSwimParams.HandMoveStrength * speedMultiplier2, float.PositiveInfinity);
				LimbJoint wrist2 = base.GetJointBetweenLimbs(LimbType.LeftForearm, LimbType.LeftHand);
				if (wrist2 != null)
				{
					leftHand.body.ApplyTorque(MathHelper.Clamp(-wrist2.JointAngle, -1.5707964f, 1.5707964f) * leftHand.Mass * 100f * this.CurrentSwimParams.HandMoveStrength);
				}
			}
		}

		// Token: 0x060030EE RID: 12526 RVA: 0x00201E18 File Offset: 0x00200018
		private void UpdateFallingProne(float strength, bool moveHands = true, bool moveTorso = true, bool moveLegs = true)
		{
			if (strength <= 0f)
			{
				return;
			}
			Limb head = base.GetLimb(LimbType.Head, true, false, false);
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			if (moveHands && head != null && head.LinearVelocity.LengthSquared() > 1f && !head.IsSevered)
			{
				Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
				Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
				Vector2 protectPos = head.SimPosition + Vector2.Normalize(head.LinearVelocity);
				if (rightHand != null && !rightHand.IsSevered)
				{
					base.HandIK(rightHand, protectPos, strength * 0.1f, 1f, float.PositiveInfinity);
				}
				if (leftHand != null && !leftHand.IsSevered)
				{
					base.HandIK(leftHand, protectPos, strength * 0.1f, 1f, float.PositiveInfinity);
				}
			}
			if (torso == null)
			{
				return;
			}
			if (moveTorso && !base.InWater)
			{
				float fallDirection = (float)Math.Sign(torso.body.AngularVelocity - torso.body.LinearVelocity.X - base.Dir * 0.01f);
				float torque = MathF.Cos(torso.Rotation) * fallDirection * 5f * strength;
				torso.body.ApplyTorque(torque * torso.body.Mass);
			}
			if (moveLegs)
			{
				for (int i = 0; i < 2; i++)
				{
					Limb thigh = (i == 0) ? base.GetLimb(LimbType.LeftThigh, true, false, false) : base.GetLimb(LimbType.RightThigh, true, false, false);
					if (thigh != null && !thigh.IsSevered)
					{
						float thighDiff = Math.Abs(MathUtils.GetShortestAngle(torso.Rotation, thigh.Rotation));
						float diff = torso.Rotation - thigh.Rotation;
						if (MathUtils.IsValid(diff))
						{
							float thighTorque = thighDiff * thigh.Mass * (float)Math.Sign(diff) * 5f;
							thigh.body.ApplyTorque(thighTorque * strength);
						}
						Limb leg = (i == 0) ? base.GetLimb(LimbType.LeftLeg, true, false, false) : base.GetLimb(LimbType.RightLeg, true, false, false);
						if (leg != null && !leg.IsSevered)
						{
							float legDiff = Math.Abs(MathUtils.GetShortestAngle(torso.Rotation, leg.Rotation));
							diff = torso.Rotation - leg.Rotation;
							if (MathUtils.IsValid(diff))
							{
								float legTorque = legDiff * leg.Mass * (float)Math.Sign(diff) * 5f;
								leg.body.ApplyTorque(legTorque * strength);
							}
						}
					}
				}
			}
		}

		// Token: 0x060030EF RID: 12527 RVA: 0x00202090 File Offset: 0x00200290
		private void UpdateCPR(float deltaTime)
		{
			if (this.character.SelectedCharacter == null || (!this.character.SelectedCharacter.IsUnconscious && !this.character.SelectedCharacter.IsDead && this.character.SelectedCharacter.Stun <= 0f))
			{
				this.Anim = AnimController.Animation.None;
				return;
			}
			Character target = this.character.SelectedCharacter;
			this.Crouching = true;
			Vector2 offset = Vector2.UnitX * -base.Dir * 0.75f;
			Vector2 diff = target.SimPosition + offset - this.character.SimPosition;
			Limb targetHead = target.AnimController.GetLimb(LimbType.Head, true, false, false);
			Limb targetTorso = target.AnimController.GetLimb(LimbType.Torso, true, false, false);
			if (targetTorso == null)
			{
				this.Anim = AnimController.Animation.None;
				return;
			}
			Limb head = base.GetLimb(LimbType.Head, true, false, false);
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			Vector2 headDiff = (targetHead == null) ? diff : (targetHead.SimPosition - this.character.SimPosition);
			this.targetMovement = new Vector2(diff.X, 0f);
			if (Math.Abs(this.targetMovement.X) < 0.1f)
			{
				this.targetMovement.X = 0f;
			}
			this.TargetDir = ((headDiff.X > 0f) ? Direction.Right : Direction.Left);
			if (this.cprAnimTimer <= 0f && target.AnimController.Direction == this.TargetDir)
			{
				target.AnimController.Flip();
			}
			HumanoidAnimController humanoidAnimController = target.AnimController as HumanoidAnimController;
			if (humanoidAnimController != null)
			{
				humanoidAnimController.UpdateFallingProne(1f, false, false, true);
			}
			head.Disabled = true;
			torso.Disabled = true;
			this.UpdateStanding();
			Vector2 handPos = targetTorso.SimPosition + Vector2.UnitY * 0.2f;
			base.Grab(handPos, handPos);
			Vector2 colliderPos = base.GetColliderBottom();
			float prevVitality = target.Vitality;
			bool wasCritical = prevVitality < 0f;
			float cprBoost = this.character.GetStatValue(StatTypes.CPRBoost, true);
			float skill = this.character.GetSkillLevel(Tags.MedicalSkill);
			bool oxygenAvailable = target.OxygenAvailable >= 30f;
			if (oxygenAvailable)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null || !networkMember.IsClient)
				{
					target.Oxygen += deltaTime * 0.5f;
					if (cprBoost >= 1f)
					{
						target.Oxygen = Math.Max(target.Oxygen, -10f);
					}
					if (target.Oxygen < -10f)
					{
						float stabilizationAmount = skill * CPRSettings.Active.StabilizationPerSkill;
						stabilizationAmount = MathHelper.Clamp(stabilizationAmount, CPRSettings.Active.StabilizationMin, CPRSettings.Active.StabilizationMax);
						target.Oxygen += stabilizationAmount * deltaTime;
					}
				}
			}
			if (targetHead != null && head != null)
			{
				head.PullJointWorldAnchorB = new Vector2(targetHead.SimPosition.X, targetHead.SimPosition.Y + 0.8f);
				head.PullJointEnabled = true;
			}
			torso.PullJointWorldAnchorB = new Vector2(torso.SimPosition.X, colliderPos.Y + (this.TorsoPosition.Value - 0.1f));
			torso.PullJointEnabled = true;
			if (this.cprPumpTimer >= 1f)
			{
				torso.body.ApplyLinearImpulse(new Vector2(0f, -20f), 64f);
				targetTorso.body.ApplyLinearImpulse(new Vector2(0f, -20f), 64f);
				target.DisableImpactDamageTimer = 0.15f;
				this.cprPumpTimer = 0f;
				if (skill < CPRSettings.Active.DamageSkillThreshold)
				{
					target.LastDamageSource = null;
					target.DamageLimb(targetTorso.WorldPosition, targetTorso, new Affliction[]
					{
						CPRSettings.Active.InsufficientSkillAffliction.Instantiate((CPRSettings.Active.DamageSkillThreshold - skill) * CPRSettings.Active.DamageSkillMultiplier, this.character)
					}, 0f, true, Vector2.Zero, null, 1f, true, 0f, false, false, true);
				}
				if (oxygenAvailable && this.cprAnimTimer > 2f)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null || !networkMember.IsClient)
					{
						float reviveChance = skill * CPRSettings.Active.ReviveChancePerSkill;
						reviveChance = (float)Math.Pow((double)reviveChance, (double)CPRSettings.Active.ReviveChanceExponent);
						reviveChance = MathHelper.Clamp(reviveChance, CPRSettings.Active.ReviveChanceMin, CPRSettings.Active.ReviveChanceMax);
						reviveChance *= 1f + cprBoost;
						if (Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) <= reviveChance)
						{
							target.Oxygen = Math.Max(target.Oxygen + 10f, 10f);
							LuaCsSetup.Instance.EventService.PublishEvent<IEventHumanCPRSuccess>(delegate(IEventHumanCPRSuccess x)
							{
								x.OnCharacterCPRSuccess(this);
							});
						}
						else
						{
							LuaCsSetup.Instance.EventService.PublishEvent<IEventHumanCPRFailed>(delegate(IEventHumanCPRFailed x)
							{
								x.OnCharacterCPRFailed(this);
							});
						}
					}
				}
			}
			this.cprPumpTimer += deltaTime;
			this.cprAnimTimer += deltaTime;
			if (!target.IsDead || !oxygenAvailable)
			{
				target.CharacterHealth.RecalculateVitality();
				if (wasCritical && target.Vitality > 0f && Timing.TotalTime > (double)(this.lastReviveTime + 10f))
				{
					CharacterInfo info = this.character.Info;
					if (info != null)
					{
						info.ApplySkillGain(Tags.MedicalSkill, SkillSettings.Current.SkillIncreasePerCprRevive, false, 2f, false);
					}
					AchievementManager.OnCharacterRevived(target, this.character);
					this.lastReviveTime = (float)Timing.TotalTime;
					target.ForgiveAttacker(this.character);
				}
			}
		}

		// Token: 0x060030F0 RID: 12528 RVA: 0x00202644 File Offset: 0x00200844
		public override void DragCharacter(Character target, float deltaTime)
		{
			if (target == null)
			{
				return;
			}
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			Limb leftHand = base.GetLimb(LimbType.LeftHand, true, false, false);
			Limb rightHand = base.GetLimb(LimbType.RightHand, true, false, false);
			Limb limb;
			if ((limb = target.AnimController.GetLimb(LimbType.LeftForearm, true, false, false)) == null)
			{
				limb = (target.AnimController.GetLimb(LimbType.Torso, true, false, false) ?? target.AnimController.MainLimb);
			}
			Limb targetLeftHand = limb;
			Limb limb2;
			if ((limb2 = target.AnimController.GetLimb(LimbType.RightForearm, true, false, false)) == null)
			{
				limb2 = (target.AnimController.GetLimb(LimbType.Torso, true, false, false) ?? target.AnimController.MainLimb);
			}
			Limb targetRightHand = limb2;
			if (!target.AllowInput)
			{
				target.AnimController.ResetPullJoints(null);
			}
			Item selectedItem = target.SelectedItem;
			Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
			bool flag;
			if (controller == null || !controller.ControlCharacterPose)
			{
				Item selectedSecondaryItem = target.SelectedSecondaryItem;
				controller = ((selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Controller>() : null);
				flag = (controller != null && controller.ControlCharacterPose);
			}
			else
			{
				flag = true;
			}
			bool targetPoseControlled = flag;
			if (base.IsClimbing)
			{
				if (target.AllowInput && (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient))
				{
					this.character.DeselectCharacter();
					return;
				}
				Limb targetTorso = target.AnimController.GetLimb(LimbType.Torso, true, false, false);
				if (targetTorso == null)
				{
					targetTorso = target.AnimController.MainLimb;
				}
				if (target.AnimController.Dir != base.Dir)
				{
					target.AnimController.Flip();
				}
				Vector2 transformedTorsoPos = torso.SimPosition;
				if (this.character.Submarine == null && target.Submarine != null)
				{
					transformedTorsoPos -= target.Submarine.SimPosition;
				}
				else if (this.character.Submarine != null && target.Submarine == null)
				{
					transformedTorsoPos += this.character.Submarine.SimPosition;
				}
				else if (this.character.Submarine != null && target.Submarine != null && this.character.Submarine != target.Submarine)
				{
					transformedTorsoPos += this.character.Submarine.SimPosition;
					transformedTorsoPos -= target.Submarine.SimPosition;
				}
				targetTorso.PullJointEnabled = true;
				targetTorso.PullJointWorldAnchorB = transformedTorsoPos + Vector2.UnitX * -base.Dir * 0.2f;
				targetTorso.PullJointMaxForce = 5000f;
				if (!targetLeftHand.IsSevered)
				{
					targetLeftHand.PullJointEnabled = true;
					targetLeftHand.PullJointWorldAnchorB = transformedTorsoPos + new Vector2(1f * base.Dir, 1f) * 0.2f;
					targetLeftHand.PullJointMaxForce = 5000f;
				}
				if (!targetRightHand.IsSevered)
				{
					targetRightHand.PullJointEnabled = true;
					targetRightHand.PullJointWorldAnchorB = transformedTorsoPos + new Vector2(1f * base.Dir, 1f) * 0.2f;
					targetRightHand.PullJointMaxForce = 5000f;
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					base.Collider.ResetDynamics();
				}
				target.AnimController.IgnorePlatforms = true;
				return;
			}
			else
			{
				leftHand.Disabled = true;
				if (!this.inWater)
				{
					rightHand.Disabled = true;
				}
				for (int i = 0; i < 2; i++)
				{
					Limb targetLimb = target.AnimController.GetLimb(LimbType.Torso, true, false, false);
					if (i == 0)
					{
						if (!targetLeftHand.IsSevered)
						{
							targetLimb = targetLeftHand;
						}
						else if (!targetRightHand.IsSevered)
						{
							targetLimb = targetRightHand;
						}
					}
					else if (!targetRightHand.IsSevered)
					{
						targetLimb = targetRightHand;
					}
					else if (!targetLeftHand.IsSevered)
					{
						targetLimb = targetLeftHand;
					}
					Limb pullLimb = (i == 0) ? leftHand : rightHand;
					if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
					{
						Vector2 sourceSimPos = pullLimb.SimPosition;
						Vector2 targetSimPos = targetLimb.SimPosition;
						if (this.character.Submarine != null && this.character.SelectedCharacter.Submarine == null)
						{
							targetSimPos -= this.character.Submarine.SimPosition;
						}
						else if (this.character.Submarine == null && this.character.SelectedCharacter.Submarine != null)
						{
							sourceSimPos -= this.character.SelectedCharacter.Submarine.SimPosition;
						}
						else if (this.character.Submarine != null && this.character.SelectedCharacter.Submarine != null && this.character.Submarine != this.character.SelectedCharacter.Submarine)
						{
							targetSimPos += this.character.SelectedCharacter.Submarine.SimPosition;
							targetSimPos -= this.character.Submarine.SimPosition;
						}
						Body body = Submarine.CheckVisibility(sourceSimPos, targetSimPos, false, true, true, true, true, null);
						if (body != null)
						{
							this.character.DeselectCharacter();
							return;
						}
					}
					if (i <= 0 || !this.inWater)
					{
						Vector2 diff = ConvertUnits.ToSimUnits(targetLimb.WorldPosition - pullLimb.WorldPosition);
						pullLimb.PullJointEnabled = true;
						Vector2 targetAnchor;
						float targetForce;
						if (targetLimb.type == LimbType.Torso || targetLimb == target.AnimController.MainLimb)
						{
							pullLimb.PullJointMaxForce = 5000f;
							if (!this.character.CanRunWhileDragging())
							{
								this.targetMovement *= MathHelper.Clamp(base.Mass / target.Mass, 0.5f, 1f);
							}
							Vector2 shoulderPos = this.rightShoulder.WorldAnchorA;
							float targetDist = Vector2.Distance(targetLimb.SimPosition, shoulderPos);
							Vector2 dragDir = (targetLimb.SimPosition - shoulderPos) / targetDist;
							if (!MathUtils.IsValid(dragDir))
							{
								dragDir = -Vector2.UnitY;
							}
							if (!base.InWater)
							{
								dragDir = Vector2.Lerp(dragDir, -Vector2.UnitY, 0.5f);
							}
							Vector2 pullLimbAnchor = shoulderPos + dragDir * Math.Min(targetDist, (this.upperArmLength + this.forearmLength) * 2f);
							targetAnchor = shoulderPos + dragDir * (this.upperArmLength + this.forearmLength);
							targetForce = 200f;
							if (target.Submarine != this.character.Submarine)
							{
								if (this.character.Submarine == null)
								{
									pullLimbAnchor += target.Submarine.SimPosition;
									targetAnchor -= target.Submarine.SimPosition;
								}
								else if (target.Submarine == null)
								{
									pullLimbAnchor -= this.character.Submarine.SimPosition;
									targetAnchor += this.character.Submarine.SimPosition;
								}
								else
								{
									pullLimbAnchor -= target.Submarine.SimPosition;
									pullLimbAnchor += this.character.Submarine.SimPosition;
									targetAnchor -= this.character.Submarine.SimPosition;
									targetAnchor += target.Submarine.SimPosition;
								}
							}
							if (Vector2.DistanceSquared(pullLimb.PullJointWorldAnchorA, pullLimbAnchor) > 2500f)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(123, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Attempted to move the anchor B of a limb's pull joint extremely far from the limb in ");
								defaultInterpolatedStringHandler.AppendFormatted("DragCharacter");
								defaultInterpolatedStringHandler.AppendLiteral(". ");
								defaultInterpolatedStringHandler.AppendLiteral("Character in sub: ");
								defaultInterpolatedStringHandler.AppendFormatted<bool>(this.character.Submarine != null);
								defaultInterpolatedStringHandler.AppendLiteral(", target in sub: ");
								defaultInterpolatedStringHandler.AppendFormatted<bool>(target.Submarine != null);
								defaultInterpolatedStringHandler.AppendLiteral(".");
								string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
								GameAnalyticsManager.AddErrorEventOnce("DragCharacter:PullJointTooFar", GameAnalyticsManager.ErrorSeverity.Warning, errorMsg);
							}
							else
							{
								pullLimb.PullJointWorldAnchorB = pullLimbAnchor;
							}
						}
						else
						{
							pullLimb.PullJointWorldAnchorB = pullLimb.SimPosition + diff;
							pullLimb.PullJointMaxForce = 5000f;
							targetAnchor = targetLimb.SimPosition - diff;
							targetForce = 5000f;
						}
						if (!targetPoseControlled)
						{
							targetLimb.PullJointEnabled = true;
							targetLimb.PullJointMaxForce = targetForce;
							targetLimb.PullJointWorldAnchorB = targetAnchor;
							targetLimb.Disabled = true;
							target.AnimController.movement = -diff;
						}
					}
				}
				float dist = ConvertUnits.ToSimUnits(Vector2.Distance(target.WorldPosition, base.WorldPosition));
				if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && dist > 1.4f && target.AllowInput && Vector2.Dot(target.WorldPosition - base.WorldPosition, target.AnimController.TargetMovement) > 0f)
				{
					this.character.DeselectCharacter();
					return;
				}
				if (!this.character.CanRunWhileDragging() && Vector2.Dot(target.WorldPosition - base.WorldPosition, this.targetMovement) < 0f)
				{
					this.targetMovement *= MathHelper.Clamp(1.5f - dist, 0f, 1f);
				}
				if (!target.AllowInput)
				{
					target.AnimController.Stairs = this.Stairs;
					target.AnimController.IgnorePlatforms = base.IgnorePlatforms;
					target.AnimController.TargetMovement = base.TargetMovement;
					return;
				}
				if (target is AICharacter && target != Character.Controlled && !targetPoseControlled)
				{
					if (target.AnimController.Dir > 0f == base.WorldPosition.X > target.WorldPosition.X)
					{
						target.AnimController.LockFlipping(0.5f);
					}
					else
					{
						target.AnimController.TargetDir = ((base.WorldPosition.X > target.WorldPosition.X) ? Direction.Right : Direction.Left);
					}
					Vector2 movement = this.character.SimPosition + Vector2.UnitX * 0.5f * (float)Math.Sign(target.SimPosition.X - this.character.SimPosition.X) - target.SimPosition;
					target.AnimController.TargetMovement = ((movement.LengthSquared() > 0.01f) ? movement : Vector2.Zero);
				}
				return;
			}
		}

		// Token: 0x060030F1 RID: 12529 RVA: 0x0020306B File Offset: 0x0020126B
		public void Crouch()
		{
			this.Crouching = true;
			this.character.SetInput(InputType.Crouch, false, true);
		}

		// Token: 0x060030F2 RID: 12530 RVA: 0x00203084 File Offset: 0x00201284
		private void FootIK(Limb foot, Vector2 pos, float legTorque, float footTorque, float footAngle)
		{
			if (!MathUtils.IsValid(pos))
			{
				string str = "Invalid foot position in FootIK (";
				Vector2 vector = pos;
				string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("FootIK:InvalidPos", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			Limb upperLeg;
			Limb lowerLeg;
			if (foot.type == LimbType.LeftFoot)
			{
				upperLeg = base.GetLimb(LimbType.LeftThigh, true, false, false);
				lowerLeg = base.GetLimb(LimbType.LeftLeg, true, false, false);
			}
			else
			{
				upperLeg = base.GetLimb(LimbType.RightThigh, true, false, false);
				lowerLeg = base.GetLimb(LimbType.RightLeg, true, false, false);
			}
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			LimbJoint waistJoint = base.GetJointBetweenLimbs(LimbType.Waist, upperLeg.type) ?? base.GetJointBetweenLimbs(LimbType.Torso, upperLeg.type);
			Vector2 waistPos = Vector2.Zero;
			if (waistJoint != null)
			{
				waistPos = ((waistJoint.LimbA == upperLeg) ? waistJoint.WorldAnchorA : waistJoint.WorldAnchorB);
			}
			float c = Vector2.Distance(pos, waistPos);
			c = Math.Max(c, Math.Abs(this.upperLegLength - this.lowerLegLength));
			float legAngle = MathUtils.VectorToAngle(pos - waistPos) + 1.5707964f;
			if (!MathUtils.IsValid(legAngle))
			{
				string[] array = new string[8];
				array[0] = "Invalid leg angle (";
				array[1] = legAngle.ToString();
				array[2] = ") in FootIK. Waist pos: ";
				int num = 3;
				Vector2 vector = waistPos;
				array[num] = vector.ToString();
				array[4] = ", target pos: ";
				int num2 = 5;
				vector = pos;
				array[num2] = vector.ToString();
				array[6] = "\n";
				array[7] = Environment.StackTrace.CleanupStackTrace();
				string errorMsg2 = string.Concat(array);
				GameAnalyticsManager.AddErrorEventOnce("FootIK:InvalidAngle", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
				return;
			}
			while (torso.Rotation - legAngle > 3.1415927f)
			{
				legAngle += 6.2831855f;
			}
			while (torso.Rotation - legAngle < -3.1415927f)
			{
				legAngle -= 6.2831855f;
			}
			float upperLegAngle = (c >= this.upperLegLength + this.lowerLegLength) ? 0f : MathUtils.SolveTriangleSSS(this.lowerLegLength, this.upperLegLength, c);
			float lowerLegAngle = (c >= this.upperLegLength + this.lowerLegLength) ? 0f : MathUtils.SolveTriangleSSS(this.upperLegLength, this.lowerLegLength, c);
			upperLeg.body.SmoothRotate(legAngle + upperLegAngle * base.Dir, upperLeg.Mass * legTorque, false);
			lowerLeg.body.SmoothRotate(legAngle - lowerLegAngle * base.Dir, lowerLeg.Mass * legTorque, false);
			foot.body.SmoothRotate(legAngle - (lowerLegAngle + footAngle) * base.Dir, foot.Mass * footTorque, false);
		}

		// Token: 0x060030F3 RID: 12531 RVA: 0x00203310 File Offset: 0x00201510
		public override void Flip()
		{
			if (base.Character == null || base.Character.Removed)
			{
				base.LogAccessedRemovedCharacterError();
				return;
			}
			base.Flip();
			base.WalkPos = -base.WalkPos;
			Limb torso = base.GetLimb(LimbType.Torso, true, false, false);
			if (torso == null)
			{
				return;
			}
			Matrix torsoTransform = Matrix.CreateRotationZ(torso.Rotation);
			foreach (Item heldItem in this.character.HeldItems)
			{
				if (((heldItem != null) ? heldItem.body : null) != null && !heldItem.Removed && heldItem.GetComponent<Holdable>() != null)
				{
					heldItem.FlipX(false, false);
				}
			}
			foreach (Limb limb in base.Limbs)
			{
				if (!limb.IsSevered)
				{
					bool mirror = false;
					bool wrapAngle = false;
					bool flipAngle;
					switch (limb.type)
					{
					case LimbType.LeftHand:
					case LimbType.RightHand:
					case LimbType.LeftArm:
					case LimbType.RightArm:
					case LimbType.LeftForearm:
					case LimbType.RightForearm:
						flipAngle = true;
						break;
					case LimbType.LeftLeg:
					case LimbType.RightLeg:
					case LimbType.LeftFoot:
					case LimbType.RightFoot:
					case LimbType.RightThigh:
					case LimbType.LeftThigh:
						mirror = (this.Crouching && !this.inWater);
						flipAngle = ((limb.DoesFlip || this.Crouching) && !this.inWater);
						wrapAngle = !this.inWater;
						break;
					case LimbType.Head:
					case LimbType.Torso:
					case LimbType.Tail:
					case LimbType.Legs:
						goto IL_16C;
					default:
						goto IL_16C;
					}
					IL_18E:
					Vector2 position = limb.SimPosition;
					if (!limb.PullJointEnabled && mirror)
					{
						Vector2 difference = limb.body.SimPosition - torso.SimPosition;
						difference = Vector2.Transform(difference, torsoTransform);
						difference.Y = -difference.Y;
						position = torso.SimPosition + Vector2.Transform(difference, -torsoTransform);
					}
					float angle = flipAngle ? (-limb.body.Rotation) : limb.body.Rotation;
					if (wrapAngle)
					{
						angle = MathUtils.WrapAnglePi(angle);
					}
					base.TrySetLimbPosition(limb, base.Collider.SimPosition, position, angle, false, true);
					goto IL_234;
					IL_16C:
					flipAngle = (limb.DoesFlip && !this.inWater);
					wrapAngle = !this.inWater;
					goto IL_18E;
				}
				IL_234:;
			}
		}

		// Token: 0x060030F4 RID: 12532 RVA: 0x00203574 File Offset: 0x00201774
		public override float GetSpeed(AnimationType type)
		{
			if (type != AnimationType.Crouch)
			{
				return base.GetSpeed(type);
			}
			if (!base.CanWalk)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.character.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral(" cannot crouch!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return 0f;
			}
			if (!base.IsMovingBackwards)
			{
				return this.HumanCrouchParams.MovementSpeed;
			}
			return this.HumanCrouchParams.MovementSpeed * this.HumanCrouchParams.BackwardsMovementMultiplier;
		}

		// Token: 0x060030F6 RID: 12534 RVA: 0x00203628 File Offset: 0x00201828
		[CompilerGenerated]
		private void <UpdateAnim>g__UpdateUseItemTimer|80_1(ref HumanoidAnimController.<>c__DisplayClass80_0 A_1)
		{
			if (base.IsUsingItem)
			{
				this.useItemTimer -= A_1.deltaTime;
				if (this.useItemTimer <= 0f)
				{
					base.StopUsingItem();
				}
			}
		}

		// Token: 0x04001946 RID: 6470
		private const float SteepestWalkableSlopeAngleDegrees = 55f;

		// Token: 0x04001947 RID: 6471
		private const float SlowlyWalkableSlopeAngleDegrees = 30f;

		// Token: 0x04001948 RID: 6472
		private static readonly float SteepestWalkableSlopeNormalX = MathF.Sin(MathHelper.ToRadians(55f));

		// Token: 0x04001949 RID: 6473
		private static readonly float SlowlyWalkableSlopeNormalX = MathF.Sin(MathHelper.ToRadians(30f));

		// Token: 0x0400194A RID: 6474
		private const float MaxSpeedOnStairs = 1.7f;

		// Token: 0x0400194B RID: 6475
		private const float SteepSlopePushMagnitude = 1.7f;

		// Token: 0x0400194C RID: 6476
		public const float BreakFromGrabDistance = 1.4f;

		// Token: 0x0400194D RID: 6477
		private HumanRagdollParams _ragdollParams;

		// Token: 0x0400194E RID: 6478
		private HumanWalkParams _humanWalkParams;

		// Token: 0x0400194F RID: 6479
		private HumanRunParams _humanRunParams;

		// Token: 0x04001950 RID: 6480
		private HumanCrouchParams _humanCrouchParams;

		// Token: 0x04001951 RID: 6481
		private HumanSwimSlowParams _humanSwimSlowParams;

		// Token: 0x04001952 RID: 6482
		private HumanSwimFastParams _humanSwimFastParams;

		// Token: 0x04001954 RID: 6484
		private float upperLegLength;

		// Token: 0x04001955 RID: 6485
		private float lowerLegLength;

		// Token: 0x04001956 RID: 6486
		private readonly float movementLerp;

		// Token: 0x04001957 RID: 6487
		private float cprAnimTimer;

		// Token: 0x04001958 RID: 6488
		private float cprPumpTimer;

		// Token: 0x04001959 RID: 6489
		private float fallingProneAnimTimer;

		// Token: 0x0400195A RID: 6490
		private const float FallingProneAnimDuration = 1f;

		// Token: 0x0400195B RID: 6491
		private bool swimming;

		// Token: 0x0400195C RID: 6492
		private float swimmingStateLockTimer;

		// Token: 0x0400195D RID: 6493
		private float handCyclePos;

		// Token: 0x0400195E RID: 6494
		private float legCyclePos;

		// Token: 0x0400195F RID: 6495
		private float lastReviveTime;
	}
}
