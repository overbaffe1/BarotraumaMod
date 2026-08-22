using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004BD RID: 1213
	internal class Propulsion : ItemComponent
	{
		// Token: 0x17001268 RID: 4712
		// (get) Token: 0x060044F9 RID: 17657 RVA: 0x001B9AAA File Offset: 0x001B7CAA
		// (set) Token: 0x060044FA RID: 17658 RVA: 0x001B9AB2 File Offset: 0x001B7CB2
		[Serialize(Propulsion.UseEnvironment.Both, IsPropertySaveable.No, "Can the item be used in air, underwater or both.", "", false)]
		public Propulsion.UseEnvironment UsableIn { get; set; }

		// Token: 0x17001269 RID: 4713
		// (get) Token: 0x060044FB RID: 17659 RVA: 0x001B9ABB File Offset: 0x001B7CBB
		// (set) Token: 0x060044FC RID: 17660 RVA: 0x001B9AC3 File Offset: 0x001B7CC3
		[Serialize(0f, IsPropertySaveable.No, "The force to apply to the user's body.", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float Force { get; set; }

		// Token: 0x1700126A RID: 4714
		// (get) Token: 0x060044FD RID: 17661 RVA: 0x001B9ACC File Offset: 0x001B7CCC
		// (set) Token: 0x060044FE RID: 17662 RVA: 0x001B9AD4 File Offset: 0x001B7CD4
		[Serialize(true, IsPropertySaveable.No, "If the item is held in RightHand or LeftHand, apply extra force there", "", false)]
		public bool ApplyToHands { get; set; }

		// Token: 0x060044FF RID: 17663 RVA: 0x001B9ADD File Offset: 0x001B7CDD
		public Propulsion(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004500 RID: 17664 RVA: 0x001B9AE8 File Offset: 0x001B7CE8
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
			return true;
		}

		// Token: 0x06004501 RID: 17665 RVA: 0x001B9CBC File Offset: 0x001B7EBC
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

		// Token: 0x0400211B RID: 8475
		private float useState;

		// Token: 0x02000E06 RID: 3590
		public enum UseEnvironment
		{
			// Token: 0x04004184 RID: 16772
			Air,
			// Token: 0x04004185 RID: 16773
			Water,
			// Token: 0x04004186 RID: 16774
			Both,
			// Token: 0x04004187 RID: 16775
			None
		}
	}
}
