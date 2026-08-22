using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F1 RID: 1521
	internal class Throwable : Holdable
	{
		// Token: 0x1700192E RID: 6446
		// (get) Token: 0x06006386 RID: 25478 RVA: 0x0033D771 File Offset: 0x0033B971
		// (set) Token: 0x06006387 RID: 25479 RVA: 0x0033D779 File Offset: 0x0033B979
		public Character CurrentThrower { get; private set; }

		// Token: 0x1700192F RID: 6447
		// (get) Token: 0x06006388 RID: 25480 RVA: 0x0033D782 File Offset: 0x0033B982
		// (set) Token: 0x06006389 RID: 25481 RVA: 0x0033D78A File Offset: 0x0033B98A
		[Serialize(1f, IsPropertySaveable.No, "The impulse applied to the physics body of the item when thrown. Higher values make the item be thrown faster.", "", false)]
		public float ThrowForce { get; set; }

		// Token: 0x0600638A RID: 25482 RVA: 0x0033D793 File Offset: 0x0033B993
		public Throwable(Item item, ContentXElement element) : base(item, element)
		{
			if (this.aimPos == Vector2.Zero)
			{
				this.aimPos = new Vector2(0.45f, 0.1f);
			}
		}

		// Token: 0x0600638B RID: 25483 RVA: 0x0033D7CF File Offset: 0x0033B9CF
		public override bool Use(float deltaTime, Character character = null)
		{
			return (this.characterUsable && !base.UsageDisabledByRangedWeapon(character)) || character == null;
		}

		// Token: 0x0600638C RID: 25484 RVA: 0x0033D7E8 File Offset: 0x0033B9E8
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return false;
		}

		// Token: 0x0600638D RID: 25485 RVA: 0x0033D7EB File Offset: 0x0033B9EB
		public override void Drop(Character dropper, bool setTransform = true)
		{
			base.Drop(dropper, setTransform);
			this.throwState = Throwable.ThrowState.None;
			this.throwAngle = -1.5707964f;
			base.Item.ResetWaterDragCoefficient();
		}

		// Token: 0x0600638E RID: 25486 RVA: 0x0033D812 File Offset: 0x0033BA12
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x0600638F RID: 25487 RVA: 0x0033D81C File Offset: 0x0033BA1C
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

		// Token: 0x0400339A RID: 13210
		private const float ThrowAngleStart = -1.5707964f;

		// Token: 0x0400339B RID: 13211
		private const float ThrowAngleEnd = 1.5707964f;

		// Token: 0x0400339C RID: 13212
		private float throwAngle = -1.5707964f;

		// Token: 0x0400339D RID: 13213
		private bool midAir;

		// Token: 0x0400339E RID: 13214
		private Throwable.ThrowState throwState;

		// Token: 0x0400339F RID: 13215
		private const float ContinuousCollisionThreshold = 5f;

		// Token: 0x040033A2 RID: 13218
		public const float WaterDragCoefficient = 0.5f;

		// Token: 0x02001496 RID: 5270
		private enum ThrowState
		{
			// Token: 0x0400664A RID: 26186
			None,
			// Token: 0x0400664B RID: 26187
			Initiated,
			// Token: 0x0400664C RID: 26188
			Throwing
		}
	}
}
