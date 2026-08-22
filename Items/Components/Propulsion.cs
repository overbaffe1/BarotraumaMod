using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005EF RID: 1519
	internal class Propulsion : ItemComponent
	{
		// Token: 0x17001929 RID: 6441
		// (get) Token: 0x06006377 RID: 25463 RVA: 0x0033D442 File Offset: 0x0033B642
		// (set) Token: 0x06006378 RID: 25464 RVA: 0x0033D44A File Offset: 0x0033B64A
		[Serialize(Propulsion.UseEnvironment.Both, IsPropertySaveable.No, "Can the item be used in air, underwater or both.", "", false)]
		public Propulsion.UseEnvironment UsableIn { get; set; }

		// Token: 0x1700192A RID: 6442
		// (get) Token: 0x06006379 RID: 25465 RVA: 0x0033D453 File Offset: 0x0033B653
		// (set) Token: 0x0600637A RID: 25466 RVA: 0x0033D45B File Offset: 0x0033B65B
		[Serialize(0f, IsPropertySaveable.No, "The force to apply to the user's body.", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float Force { get; set; }

		// Token: 0x1700192B RID: 6443
		// (get) Token: 0x0600637B RID: 25467 RVA: 0x0033D464 File Offset: 0x0033B664
		// (set) Token: 0x0600637C RID: 25468 RVA: 0x0033D46C File Offset: 0x0033B66C
		[Serialize(true, IsPropertySaveable.No, "If the item is held in RightHand or LeftHand, apply extra force there", "", false)]
		public bool ApplyToHands { get; set; }

		// Token: 0x1700192C RID: 6444
		// (get) Token: 0x0600637D RID: 25469 RVA: 0x0033D475 File Offset: 0x0033B675
		// (set) Token: 0x0600637E RID: 25470 RVA: 0x0033D47D File Offset: 0x0033B67D
		[Serialize("", IsPropertySaveable.No, "The name of the particle prefab the item emits when used.", "", false)]
		public string Particles
		{
			get
			{
				return this.particles;
			}
			set
			{
				this.particles = value;
			}
		}

		// Token: 0x0600637F RID: 25471 RVA: 0x0033D486 File Offset: 0x0033B686
		public Propulsion(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06006380 RID: 25472 RVA: 0x0033D490 File Offset: 0x0033B690
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character == null || character.Removed)
			{
				return false;
			}
			if (!character.IsKeyDown(InputType.Aim) || character.Stun > 0f)
			{
				return false;
			}
			if (this.UsableIn == Propulsion.UseEnvironment.None)
			{
				return false;
			}
			this.IsActive = true;
			this.useState = 0.1f;
			if (character.AnimController.InWater)
			{
				if (this.UsableIn == Propulsion.UseEnvironment.Air)
				{
					return true;
				}
			}
			else if (this.UsableIn == Propulsion.UseEnvironment.Water)
			{
				return true;
			}
			Vector2 dir = character.CursorPosition - character.Position;
			if (!MathUtils.IsValid(dir))
			{
				return true;
			}
			float length = 200f;
			dir = dir.ClampLength(length) / length;
			Vector2 propulsion = dir * this.Force * character.PropulsionSpeedMultiplier * (1f + character.GetStatValue(StatTypes.PropulsionSpeed, true));
			if (character.AnimController.InWater && this.Force > 0f)
			{
				character.AnimController.TargetMovement = dir;
			}
			foreach (Limb limb in character.AnimController.Limbs)
			{
				if (limb.WearingItems.Find((WearableSprite w) => w.WearableComponent.Item == this.item) != null)
				{
					limb.body.ApplyForce(propulsion, 64f);
				}
			}
			character.AnimController.Collider.ApplyForce(propulsion, 64f);
			if (this.ApplyToHands)
			{
				if (character.Inventory.IsInLimbSlot(this.item, InvSlotType.RightHand))
				{
					Limb limb2 = character.AnimController.GetLimb(LimbType.RightHand, true, false, false);
					if (limb2 != null)
					{
						limb2.body.ApplyForce(propulsion, 64f);
					}
				}
				if (character.Inventory.IsInLimbSlot(this.item, InvSlotType.LeftHand))
				{
					Limb limb3 = character.AnimController.GetLimb(LimbType.LeftHand, true, false, false);
					if (limb3 != null)
					{
						limb3.body.ApplyForce(propulsion, 64f);
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(this.particles))
			{
				GameMain.ParticleManager.CreateParticle(this.particles, this.item.WorldPosition, this.item.body.Rotation + ((this.item.body.Dir > 0f) ? 0f : 3.1415927f), 0f, this.item.CurrentHull, 0f, null);
			}
			return true;
		}

		// Token: 0x06006381 RID: 25473 RVA: 0x0033D6D8 File Offset: 0x0033B8D8
		public override void Update(float deltaTime, Camera cam)
		{
			this.useState -= deltaTime;
			if (this.useState <= 0f)
			{
				this.IsActive = false;
			}
			if (this.item.AiTarget != null && this.IsActive)
			{
				this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
			}
		}

		// Token: 0x04003394 RID: 13204
		private float useState;

		// Token: 0x04003398 RID: 13208
		private string particles;

		// Token: 0x02001495 RID: 5269
		public enum UseEnvironment
		{
			// Token: 0x04006645 RID: 26181
			Air,
			// Token: 0x04006646 RID: 26182
			Water,
			// Token: 0x04006647 RID: 26183
			Both,
			// Token: 0x04006648 RID: 26184
			None
		}
	}
}
