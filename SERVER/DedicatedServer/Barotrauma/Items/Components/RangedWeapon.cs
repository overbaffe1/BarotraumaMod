using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004BE RID: 1214
	internal class RangedWeapon : ItemComponent
	{
		// Token: 0x1700126B RID: 4715
		// (get) Token: 0x06004503 RID: 17667 RVA: 0x001B9D35 File Offset: 0x001B7F35
		// (set) Token: 0x06004504 RID: 17668 RVA: 0x001B9D3D File Offset: 0x001B7F3D
		public float ReloadTimer { get; private set; }

		// Token: 0x1700126C RID: 4716
		// (get) Token: 0x06004505 RID: 17669 RVA: 0x001B9D46 File Offset: 0x001B7F46
		// (set) Token: 0x06004506 RID: 17670 RVA: 0x001B9D58 File Offset: 0x001B7F58
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position of the barrel as an offset from the item's center (in pixels). Determines where the projectiles spawn.", "", false)]
		public string BarrelPos
		{
			get
			{
				return XMLExtensions.Vector2ToString(ConvertUnits.ToDisplayUnits(this.barrelPos));
			}
			set
			{
				this.barrelPos = ConvertUnits.ToSimUnits(XMLExtensions.ParseVector2(value, true));
			}
		}

		// Token: 0x1700126D RID: 4717
		// (get) Token: 0x06004507 RID: 17671 RVA: 0x001B9D6C File Offset: 0x001B7F6C
		// (set) Token: 0x06004508 RID: 17672 RVA: 0x001B9D74 File Offset: 0x001B7F74
		[Serialize(1f, IsPropertySaveable.No, "How long the user has to wait before they can fire the weapon again (in seconds).", "", false)]
		public float Reload
		{
			get
			{
				return this.reload;
			}
			set
			{
				this.reload = Math.Max(value, 0f);
			}
		}

		// Token: 0x1700126E RID: 4718
		// (get) Token: 0x06004509 RID: 17673 RVA: 0x001B9D87 File Offset: 0x001B7F87
		// (set) Token: 0x0600450A RID: 17674 RVA: 0x001B9D8F File Offset: 0x001B7F8F
		[Serialize(0f, IsPropertySaveable.No, "Weapons skill requirement to reload at normal speed.", "", false)]
		public float ReloadSkillRequirement { get; set; }

		// Token: 0x1700126F RID: 4719
		// (get) Token: 0x0600450B RID: 17675 RVA: 0x001B9D98 File Offset: 0x001B7F98
		// (set) Token: 0x0600450C RID: 17676 RVA: 0x001B9DA0 File Offset: 0x001B7FA0
		[Serialize(1f, IsPropertySaveable.No, "Reload time at 0 skill level. Reload time scales with skill level up to the Weapons skill requirement.", "", false)]
		public float ReloadNoSkill { get; set; }

		// Token: 0x17001270 RID: 4720
		// (get) Token: 0x0600450D RID: 17677 RVA: 0x001B9DA9 File Offset: 0x001B7FA9
		// (set) Token: 0x0600450E RID: 17678 RVA: 0x001B9DB1 File Offset: 0x001B7FB1
		[Serialize(false, IsPropertySaveable.No, "Tells the AI to hold the trigger down when it uses this weapon", "", false)]
		public bool HoldTrigger { get; set; }

		// Token: 0x17001271 RID: 4721
		// (get) Token: 0x0600450F RID: 17679 RVA: 0x001B9DBA File Offset: 0x001B7FBA
		// (set) Token: 0x06004510 RID: 17680 RVA: 0x001B9DC2 File Offset: 0x001B7FC2
		[Serialize(1, IsPropertySaveable.No, "How many projectiles the weapon launches when fired once.", "", false)]
		public int ProjectileCount { get; set; }

		// Token: 0x17001272 RID: 4722
		// (get) Token: 0x06004511 RID: 17681 RVA: 0x001B9DCB File Offset: 0x001B7FCB
		// (set) Token: 0x06004512 RID: 17682 RVA: 0x001B9DD3 File Offset: 0x001B7FD3
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles when used by a character with sufficient skills to use the weapon (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x17001273 RID: 4723
		// (get) Token: 0x06004513 RID: 17683 RVA: 0x001B9DDC File Offset: 0x001B7FDC
		// (set) Token: 0x06004514 RID: 17684 RVA: 0x001B9DE4 File Offset: 0x001B7FE4
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles when used by a character with insufficient skills to use the weapon (in degrees).", "", false)]
		public float UnskilledSpread { get; set; }

		// Token: 0x17001274 RID: 4724
		// (get) Token: 0x06004515 RID: 17685 RVA: 0x001B9DED File Offset: 0x001B7FED
		// (set) Token: 0x06004516 RID: 17686 RVA: 0x001B9DF5 File Offset: 0x001B7FF5
		[Serialize(0f, IsPropertySaveable.No, "The impulse applied to the physics body of the projectile (the higher the impulse, the faster the projectiles are launched). Sum of weapon + projectile.", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x17001275 RID: 4725
		// (get) Token: 0x06004517 RID: 17687 RVA: 0x001B9DFE File Offset: 0x001B7FFE
		// (set) Token: 0x06004518 RID: 17688 RVA: 0x001B9E06 File Offset: 0x001B8006
		[Serialize(0f, IsPropertySaveable.Yes, "Percentage of damage mitigation ignored when hitting armored body parts (deflecting limbs). Sum of weapon + projectile.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Penetration { get; private set; }

		// Token: 0x17001276 RID: 4726
		// (get) Token: 0x06004519 RID: 17689 RVA: 0x001B9E0F File Offset: 0x001B800F
		// (set) Token: 0x0600451A RID: 17690 RVA: 0x001B9E17 File Offset: 0x001B8017
		[Serialize(1f, IsPropertySaveable.Yes, "Weapon's damage modifier", "", false)]
		public float WeaponDamageModifier { get; private set; }

		// Token: 0x17001277 RID: 4727
		// (get) Token: 0x0600451B RID: 17691 RVA: 0x001B9E20 File Offset: 0x001B8020
		// (set) Token: 0x0600451C RID: 17692 RVA: 0x001B9E28 File Offset: 0x001B8028
		[Serialize(0f, IsPropertySaveable.Yes, "The time required for a charge-type turret to charge up before able to fire.", "", false)]
		public float MaxChargeTime { get; private set; }

		// Token: 0x17001278 RID: 4728
		// (get) Token: 0x0600451D RID: 17693 RVA: 0x001B9E31 File Offset: 0x001B8031
		// (set) Token: 0x0600451E RID: 17694 RVA: 0x001B9E39 File Offset: 0x001B8039
		[Serialize(1f, IsPropertySaveable.Yes, "Penalty multiplier to reload time when dual-wielding.", "", false)]
		public float DualWieldReloadTimePenaltyMultiplier { get; private set; }

		// Token: 0x17001279 RID: 4729
		// (get) Token: 0x0600451F RID: 17695 RVA: 0x001B9E42 File Offset: 0x001B8042
		// (set) Token: 0x06004520 RID: 17696 RVA: 0x001B9E4A File Offset: 0x001B804A
		[Serialize(0f, IsPropertySaveable.Yes, "Additive penalty to accuracy (spread angle) when dual-wielding.", "", false)]
		public float DualWieldAccuracyPenalty { get; private set; }

		// Token: 0x1700127A RID: 4730
		// (get) Token: 0x06004521 RID: 17697 RVA: 0x001B9E54 File Offset: 0x001B8054
		public Vector2 TransformedBarrelPos
		{
			get
			{
				Matrix bodyTransform = Matrix.CreateRotationZ((this.item.body == null) ? this.item.RotationRad : this.item.body.Rotation);
				Vector2 flippedPos = this.barrelPos;
				if (this.item.body != null && this.item.body.Dir < 0f)
				{
					flippedPos.X = -flippedPos.X;
				}
				return Vector2.Transform(flippedPos, bodyTransform) * this.item.Scale;
			}
		}

		// Token: 0x1700127B RID: 4731
		// (get) Token: 0x06004522 RID: 17698 RVA: 0x001B9EE1 File Offset: 0x001B80E1
		// (set) Token: 0x06004523 RID: 17699 RVA: 0x001B9EE9 File Offset: 0x001B80E9
		public Projectile LastProjectile { get; private set; }

		// Token: 0x06004524 RID: 17700 RVA: 0x001B9EF4 File Offset: 0x001B80F4
		public RangedWeapon(Item item, ContentXElement element) : base(item, element)
		{
			item.IsShootable = true;
			ContentXElement parent = element.Parent;
			if (parent != null)
			{
				item.RequireAimToUse = parent.GetAttributeBool("RequireAimToUse", true);
			}
			this.characterUsable = true;
			this.suitableProjectiles = element.GetAttributeIdentifierArray("suitableProjectiles", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			if (this.ReloadSkillRequirement > 0f && this.ReloadNoSkill <= this.reload)
			{
				DebugConsole.AddWarning("Invalid XML at " + item.Name + ": ReloadNoSkill is lower or equal than it's reload skill, despite having ReloadSkillRequirement.", item.Prefab.ContentPackage);
			}
		}

		// Token: 0x06004525 RID: 17701 RVA: 0x001B9F9A File Offset: 0x001B819A
		public override void Equip(Character character)
		{
			this.ReloadTimer = Math.Max(Math.Min(this.reload, 1f), this.ReloadTimer);
			this.IsActive = true;
		}

		// Token: 0x06004526 RID: 17702 RVA: 0x001B9FC4 File Offset: 0x001B81C4
		public override void Update(float deltaTime, Camera cam)
		{
			this.ReloadTimer -= deltaTime;
			if (this.ReloadTimer < 0f)
			{
				this.ReloadTimer = 0f;
				if (this.MaxChargeTime <= 0f)
				{
					this.IsActive = false;
					return;
				}
			}
			float previousChargeTime = this.currentChargeTime;
			float chargeDeltaTime = (this.tryingToCharge && this.ReloadTimer <= 0f) ? deltaTime : (-deltaTime);
			this.currentChargeTime = Math.Clamp(this.currentChargeTime + chargeDeltaTime, 0f, this.MaxChargeTime);
			this.tryingToCharge = false;
			if (this.currentChargeTime == 0f)
			{
				this.currentChargingState = RangedWeapon.ChargingState.Inactive;
				return;
			}
			if (this.currentChargeTime < previousChargeTime)
			{
				this.currentChargingState = RangedWeapon.ChargingState.WindingDown;
				return;
			}
			this.currentChargingState = RangedWeapon.ChargingState.WindingUp;
		}

		// Token: 0x06004527 RID: 17703 RVA: 0x001BA080 File Offset: 0x001B8280
		private float GetSpread(Character user)
		{
			float degreeOfFailure = MathHelper.Clamp(1f - base.DegreeOfSuccess(user), 0f, 1f);
			degreeOfFailure *= degreeOfFailure;
			float spread = MathHelper.Lerp(this.Spread, this.UnskilledSpread, degreeOfFailure) / (1f + user.GetStatValue(StatTypes.RangedSpreadReduction, true));
			if (user.IsDualWieldingRangedWeapons())
			{
				spread += Math.Max(0f, RangedWeapon.ApplyDualWieldPenaltyReduction(user, this.DualWieldAccuracyPenalty, 0f));
			}
			return MathHelper.ToRadians(spread);
		}

		// Token: 0x06004528 RID: 17704 RVA: 0x001BA100 File Offset: 0x001B8300
		private static float ApplyDualWieldPenaltyReduction(Character character, float originalPenalty, float neutralValue)
		{
			float statAdjustmentPrc = character.GetStatValue(StatTypes.DualWieldingPenaltyReduction, true);
			statAdjustmentPrc = MathHelper.Clamp(statAdjustmentPrc, 0f, 1f);
			return MathHelper.Lerp(originalPenalty, neutralValue, statAdjustmentPrc);
		}

		// Token: 0x06004529 RID: 17705 RVA: 0x001BA134 File Offset: 0x001B8334
		public override bool Use(float deltaTime, Character character = null)
		{
			this.tryingToCharge = true;
			if (character == null || character.Removed)
			{
				return false;
			}
			if ((this.item.RequireAimToUse && !character.IsKeyDown(InputType.Aim)) || this.ReloadTimer > 0f)
			{
				return false;
			}
			if (this.currentChargeTime < this.MaxChargeTime)
			{
				return false;
			}
			this.IsActive = true;
			float baseReloadTime = this.reload;
			float weaponSkill = character.GetSkillLevel(Tags.WeaponsSkill);
			bool applyReloadFailure = this.ReloadSkillRequirement > 0f && this.ReloadNoSkill > this.reload && weaponSkill < this.ReloadSkillRequirement;
			if (applyReloadFailure)
			{
				float reloadFailure = MathHelper.Clamp(1f - weaponSkill / this.ReloadSkillRequirement, 0f, 1f);
				baseReloadTime = MathHelper.Lerp(this.reload, this.ReloadNoSkill, reloadFailure);
			}
			if (character.IsDualWieldingRangedWeapons())
			{
				baseReloadTime *= Math.Max(1f, RangedWeapon.ApplyDualWieldPenaltyReduction(character, this.DualWieldReloadTimePenaltyMultiplier, 1f));
			}
			this.ReloadTimer = baseReloadTime / ((float)1 + ((character != null) ? new float?(character.GetStatValue(StatTypes.RangedAttackSpeed, true)) : null)).GetValueOrDefault();
			this.ReloadTimer /= 1f + this.item.GetQualityModifier(Quality.StatType.FiringRateMultiplier);
			this.currentChargeTime = 0f;
			AbilityRangedWeapon abilityRangedWeapon = new AbilityRangedWeapon(this.item);
			character.CheckTalents(AbilityEffectType.OnUseRangedWeapon, abilityRangedWeapon);
			if (this.item.AiTarget != null)
			{
				this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
				this.item.AiTarget.SightRange = this.item.AiTarget.MaxSightRange;
			}
			float degreeOfFailure = 1f - base.DegreeOfSuccess(character);
			degreeOfFailure *= degreeOfFailure;
			if (degreeOfFailure > Rand.Range(0f, 1f, Rand.RandSync.Unsynced))
			{
				base.ApplyStatusEffects(ActionType.OnFailure, 1f, character, null, null, null, null, 1f);
			}
			for (int i = 0; i < this.ProjectileCount; i++)
			{
				Projectile projectile = this.FindProjectile(true);
				if (projectile != null)
				{
					Vector2 barrelPos = this.TransformedBarrelPos + this.item.body.SimPosition;
					float rotation = (base.Item.body.Dir == 1f) ? base.Item.body.Rotation : (base.Item.body.Rotation - 3.1415927f);
					float spread = this.GetSpread(character) * projectile.GetSpreadFromPool();
					Projectile lastProjectile = this.LastProjectile;
					if (lastProjectile != projectile && lastProjectile != null)
					{
						Rope component = lastProjectile.Item.GetComponent<Rope>();
						if (component != null)
						{
							component.Snap();
						}
					}
					float rangedAttackMultiplier = (character != null) ? character.GetStatValue(StatTypes.RangedAttackMultiplier, true) : 0f;
					float damageMultiplier = (1f + this.item.GetQualityModifier(Quality.StatType.FirepowerMultiplier) + rangedAttackMultiplier) * this.WeaponDamageModifier;
					projectile.Launcher = this.item;
					this.ignoredBodies.Clear();
					if (!projectile.DamageUser)
					{
						foreach (Limb j in character.AnimController.Limbs)
						{
							if (!j.IsSevered)
							{
								this.ignoredBodies.Add(j.body.FarseerBody);
								this.ignoredBodies.Add(j.LagCompensatedBody.FarseerBody);
							}
						}
						foreach (Item heldItem in character.HeldItems)
						{
							Holdable holdable = heldItem.GetComponent<Holdable>();
							if (((holdable != null) ? holdable.Pusher : null) != null)
							{
								this.ignoredBodies.Add(holdable.Pusher.FarseerBody);
							}
						}
					}
					projectile.Item.body.Dir = base.Item.body.Dir;
					projectile.Shoot(character, character.AnimController.AimSourceSimPos, barrelPos, rotation + spread, this.ignoredBodies.ToList<Body>(), false, damageMultiplier, this.LaunchImpulse);
					Rope component2 = projectile.Item.GetComponent<Rope>();
					if (component2 != null)
					{
						component2.Attach(base.Item, projectile.Item);
					}
					if (projectile.Item.body != null)
					{
						if (i == 0)
						{
							base.Item.body.ApplyLinearImpulse(new Vector2((float)Math.Cos((double)projectile.Item.body.Rotation), (float)Math.Sin((double)projectile.Item.body.Rotation)) * base.Item.body.Mass * -50f, 64f);
						}
						projectile.Item.body.ApplyTorque(projectile.Item.body.Mass * degreeOfFailure * 20f * projectile.GetSpreadFromPool());
					}
					base.Item.RemoveContained(projectile.Item);
				}
				this.LastProjectile = projectile;
			}
			return true;
		}

		// Token: 0x0600452A RID: 17706 RVA: 0x001BA680 File Offset: 0x001B8880
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return this.characterUsable || character == null;
		}

		// Token: 0x0600452B RID: 17707 RVA: 0x001BA690 File Offset: 0x001B8890
		public Projectile FindProjectile(bool triggerOnUseOnContainers = false)
		{
			foreach (ItemContainer container in this.item.GetComponents<ItemContainer>())
			{
				foreach (Item containedItem in container.Inventory.AllItemsMod)
				{
					if (containedItem != null)
					{
						Projectile projectile = containedItem.GetComponent<Projectile>();
						if (this.IsSuitableProjectile(projectile))
						{
							return projectile;
						}
						ItemInventory ownInventory = containedItem.OwnInventory;
						IEnumerable<Item> containedSubItems = (ownInventory != null) ? ownInventory.AllItemsMod : null;
						if (containedSubItems != null)
						{
							foreach (Item subItem in containedSubItems)
							{
								if (subItem != null)
								{
									Projectile subProjectile = subItem.GetComponent<Projectile>();
									if (triggerOnUseOnContainers && subItem.Condition > 0f)
									{
										ItemContainer component = subItem.GetComponent<ItemContainer>();
										if (component != null)
										{
											component.Item.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, false, null);
										}
									}
									if (this.IsSuitableProjectile(subProjectile))
									{
										return subProjectile;
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x0600452C RID: 17708 RVA: 0x001BA818 File Offset: 0x001B8A18
		private bool IsSuitableProjectile(Projectile projectile)
		{
			Projectile projectile2 = projectile;
			return ((projectile2 != null) ? projectile2.Item : null) != null && (!this.suitableProjectiles.Any<Identifier>() || this.suitableProjectiles.Any((Identifier s) => projectile.Item.Prefab.Identifier == s || projectile.Item.HasTag(s)));
		}

		// Token: 0x0400211F RID: 8479
		private float reload;

		// Token: 0x04002121 RID: 8481
		private Vector2 barrelPos;

		// Token: 0x0400212E RID: 8494
		private readonly IReadOnlySet<Identifier> suitableProjectiles;

		// Token: 0x0400212F RID: 8495
		private RangedWeapon.ChargingState currentChargingState;

		// Token: 0x04002131 RID: 8497
		private float currentChargeTime;

		// Token: 0x04002132 RID: 8498
		private bool tryingToCharge;

		// Token: 0x04002133 RID: 8499
		private readonly List<Body> ignoredBodies = new List<Body>();

		// Token: 0x02000E07 RID: 3591
		private enum ChargingState
		{
			// Token: 0x04004189 RID: 16777
			Inactive,
			// Token: 0x0400418A RID: 16778
			WindingUp,
			// Token: 0x0400418B RID: 16779
			WindingDown
		}
	}
}
