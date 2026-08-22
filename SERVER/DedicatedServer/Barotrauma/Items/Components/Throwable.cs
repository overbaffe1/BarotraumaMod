using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C2 RID: 1218
	internal class Throwable : Holdable
	{
		// Token: 0x17001297 RID: 4759
		// (get) Token: 0x06004571 RID: 17777 RVA: 0x001BCC38 File Offset: 0x001BAE38
		// (set) Token: 0x06004572 RID: 17778 RVA: 0x001BCC40 File Offset: 0x001BAE40
		public Character CurrentThrower { get; private set; }

		// Token: 0x17001298 RID: 4760
		// (get) Token: 0x06004573 RID: 17779 RVA: 0x001BCC49 File Offset: 0x001BAE49
		// (set) Token: 0x06004574 RID: 17780 RVA: 0x001BCC51 File Offset: 0x001BAE51
		[Serialize(1f, IsPropertySaveable.No, "The impulse applied to the physics body of the item when thrown. Higher values make the item be thrown faster.", "", false)]
		public float ThrowForce { get; set; }

		// Token: 0x06004575 RID: 17781 RVA: 0x001BCC5A File Offset: 0x001BAE5A
		public Throwable(Item item, ContentXElement element) : base(item, element)
		{
			if (this.aimPos == Vector2.Zero)
			{
				this.aimPos = new Vector2(0.45f, 0.1f);
			}
		}

		// Token: 0x06004576 RID: 17782 RVA: 0x001BCC96 File Offset: 0x001BAE96
		public override bool Use(float deltaTime, Character character = null)
		{
			return (this.characterUsable && !base.UsageDisabledByRangedWeapon(character)) || character == null;
		}

		// Token: 0x06004577 RID: 17783 RVA: 0x001BCCAF File Offset: 0x001BAEAF
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return false;
		}

		// Token: 0x06004578 RID: 17784 RVA: 0x001BCCB2 File Offset: 0x001BAEB2
		public override void Drop(Character dropper, bool setTransform = true)
		{
			base.Drop(dropper, setTransform);
			this.throwState = Throwable.ThrowState.None;
			this.throwAngle = -1.5707964f;
			base.Item.ResetWaterDragCoefficient();
		}

		// Token: 0x06004579 RID: 17785 RVA: 0x001BCCD9 File Offset: 0x001BAED9
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x0600457A RID: 17786 RVA: 0x001BCCE4 File Offset: 0x001BAEE4
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.item.body.Enabled)
			{
				return;
			}
			if (this.midAir)
			{
				if (this.item.body.FarseerBody.IsBullet && this.item.body.LinearVelocity.LengthSquared() < 25f)
				{
					this.item.body.FarseerBody.IsBullet = false;
				}
				if (this.item.body.LinearVelocity.LengthSquared() < 0.01f)
				{
					this.CurrentThrower = null;
					Dictionary<ActionType, List<StatusEffect>> statusEffectLists = this.statusEffectLists;
					if (statusEffectLists != null && statusEffectLists.ContainsKey(ActionType.OnImpact))
					{
						foreach (StatusEffect statusEffect in this.statusEffectLists[ActionType.OnImpact])
						{
							statusEffect.SetUser(null);
						}
					}
					Dictionary<ActionType, List<StatusEffect>> statusEffectLists2 = this.statusEffectLists;
					if (statusEffectLists2 != null && statusEffectLists2.ContainsKey(ActionType.OnBroken))
					{
						foreach (StatusEffect statusEffect2 in this.statusEffectLists[ActionType.OnBroken])
						{
							statusEffect2.SetUser(null);
						}
					}
					this.item.body.CollidesWith = (Category.Cat1 | Category.Cat3 | Category.Cat8);
					this.midAir = false;
					base.Item.ResetWaterDragCoefficient();
				}
				return;
			}
			if (this.picker == null || this.picker.Removed || !this.picker.HeldItems.Contains(this.item))
			{
				this.IsActive = false;
				return;
			}
			bool aim = false;
			if (!base.UsageDisabledByRangedWeapon(this.picker))
			{
				if (this.throwState != Throwable.ThrowState.Throwing)
				{
					if (this.picker.IsKeyDown(InputType.Aim))
					{
						if (this.picker.IsKeyDown(InputType.Shoot))
						{
							this.throwState = Throwable.ThrowState.Initiated;
						}
					}
					else if (this.throwState != Throwable.ThrowState.Initiated)
					{
						this.throwAngle = -1.5707964f;
					}
				}
				aim = (this.picker.IsKeyDown(InputType.Aim) && this.picker.CanAim);
			}
			if (this.picker.IsDead || !this.picker.AllowInput)
			{
				this.throwState = Throwable.ThrowState.None;
				aim = false;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, this.picker, null, null, null, null, 1f);
			if (this.picker == null || this.picker.Removed || !this.picker.HeldItems.Contains(this.item))
			{
				this.IsActive = false;
				return;
			}
			if (this.item.body.Dir != this.picker.AnimController.Dir)
			{
				this.item.FlipX(false, false);
			}
			AnimController ac = this.picker.AnimController;
			this.item.Submarine = this.picker.Submarine;
			if (this.throwState != Throwable.ThrowState.Throwing)
			{
				if (!aim && this.throwState != Throwable.ThrowState.Initiated)
				{
					this.throwAngle = -1.5707964f;
					ac.HoldItem(deltaTime, this.item, this.handlePos, this.holdPos, false, this.holdAngle, 0f, false, null);
					return;
				}
				this.throwAngle = Math.Min(this.throwAngle + deltaTime * 8f, 1.5707964f);
				ac.HoldItem(deltaTime, this.item, this.handlePos, this.aimPos, false, this.throwAngle, 0f, false, null);
				if (this.throwAngle >= 1.5707964f && this.throwState == Throwable.ThrowState.Initiated)
				{
					this.throwState = Throwable.ThrowState.Throwing;
					return;
				}
			}
			else
			{
				this.throwAngle = MathUtils.WrapAnglePi(this.throwAngle - deltaTime * 15f);
				ac.HoldItem(deltaTime, this.item, this.handlePos, this.aimPos, false, this.throwAngle, 0f, false, null);
				if (this.throwAngle < 0f)
				{
					Vector2 throwVector = Vector2.Normalize(this.picker.CursorWorldPosition - this.picker.WorldPosition);
					if (!MathUtils.IsValid(throwVector))
					{
						throwVector = Vector2.UnitY;
					}
					GameServer.Log(GameServer.CharacterLogName(this.picker) + " threw " + this.item.Name, ServerLog.MessageType.ItemInteraction);
					this.CurrentThrower = this.picker;
					Dictionary<ActionType, List<StatusEffect>> statusEffectLists3 = this.statusEffectLists;
					if (statusEffectLists3 != null && statusEffectLists3.ContainsKey(ActionType.OnImpact))
					{
						foreach (StatusEffect statusEffect3 in this.statusEffectLists[ActionType.OnImpact])
						{
							statusEffect3.SetUser(this.CurrentThrower);
						}
					}
					Dictionary<ActionType, List<StatusEffect>> statusEffectLists4 = this.statusEffectLists;
					if (statusEffectLists4 != null && statusEffectLists4.ContainsKey(ActionType.OnBroken))
					{
						foreach (StatusEffect statusEffect4 in this.statusEffectLists[ActionType.OnBroken])
						{
							statusEffect4.SetUser(this.CurrentThrower);
						}
					}
					this.item.Drop(this.CurrentThrower, GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer, true);
					this.item.WaterDragCoefficient = 0.5f;
					float throwForce = this.ThrowForce;
					float downwardsDotProduct = Vector2.Dot(-Vector2.UnitY, throwVector);
					if (downwardsDotProduct > 0f)
					{
						throwForce *= 1f - downwardsDotProduct * 0.7f;
					}
					this.item.body.ApplyLinearImpulse(throwVector * throwForce * this.item.body.Mass * 3f, 64f);
					this.item.body.CollidesWith = (Category.Cat1 | Category.Cat8);
					this.item.body.FarseerBody.IsBullet = true;
					this.midAir = true;
					Limb limb = ac.GetLimb(LimbType.Head, true, false, false);
					if (limb != null)
					{
						limb.body.ApplyLinearImpulse(throwVector * 10f, 64f);
					}
					Limb limb2 = ac.GetLimb(LimbType.Torso, true, false, false);
					if (limb2 != null)
					{
						limb2.body.ApplyLinearImpulse(throwVector * 10f, 64f);
					}
					Limb rightHand = ac.GetLimb(LimbType.RightHand, true, false, false);
					this.item.body.AngularVelocity = rightHand.body.AngularVelocity;
					this.throwAngle = -1.5707964f;
					this.IsActive = true;
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsServer)
					{
						GameMain.NetworkMember.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnSecondaryUse, this, this.CurrentThrower, null, null, null));
					}
					networkMember = GameMain.NetworkMember;
					if (networkMember == null || !networkMember.IsClient)
					{
						base.ApplyStatusEffects(ActionType.OnSecondaryUse, deltaTime, this.CurrentThrower, null, null, this.CurrentThrower, null, 1f);
					}
					this.throwState = Throwable.ThrowState.None;
				}
			}
		}

		// Token: 0x0400215E RID: 8542
		private const float ThrowAngleStart = -1.5707964f;

		// Token: 0x0400215F RID: 8543
		private const float ThrowAngleEnd = 1.5707964f;

		// Token: 0x04002160 RID: 8544
		private float throwAngle = -1.5707964f;

		// Token: 0x04002161 RID: 8545
		private bool midAir;

		// Token: 0x04002162 RID: 8546
		private Throwable.ThrowState throwState;

		// Token: 0x04002163 RID: 8547
		private const float ContinuousCollisionThreshold = 5f;

		// Token: 0x04002166 RID: 8550
		public const float WaterDragCoefficient = 0.5f;

		// Token: 0x02000E0F RID: 3599
		private enum ThrowState
		{
			// Token: 0x0400419D RID: 16797
			None,
			// Token: 0x0400419E RID: 16798
			Initiated,
			// Token: 0x0400419F RID: 16799
			Throwing
		}
	}
}
