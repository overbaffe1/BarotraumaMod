using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AF RID: 1455
	internal class RangedWeapon : ItemComponent
	{
		// Token: 0x17001660 RID: 5728
		// (get) Token: 0x06005954 RID: 22868 RVA: 0x002DFF12 File Offset: 0x002DE112
		// (set) Token: 0x06005955 RID: 22869 RVA: 0x002DFF1A File Offset: 0x002DE11A
		[Serialize("0.5, 1.5", IsPropertySaveable.No, "Pitch slides from X to Y over the charge time", "", false)]
		public Vector2 ChargeSoundWindupPitchSlide
		{
			get
			{
				return this._chargeSoundWindupPitchSlide;
			}
			set
			{
				this._chargeSoundWindupPitchSlide = new Vector2(MathHelper.Clamp(value.X, 0.25f, 4f), MathHelper.Clamp(value.Y, 0.25f, 4f));
			}
		}

		// Token: 0x17001661 RID: 5729
		// (get) Token: 0x06005956 RID: 22870 RVA: 0x002DFF51 File Offset: 0x002DE151
		public Vector2 BarrelScreenPos
		{
			get
			{
				return Screen.Selected.Cam.WorldToScreen(this.item.DrawPosition + ConvertUnits.ToDisplayUnits(this.TransformedBarrelPos));
			}
		}

		// Token: 0x17001662 RID: 5730
		// (get) Token: 0x06005957 RID: 22871 RVA: 0x002DFF7D File Offset: 0x002DE17D
		// (set) Token: 0x06005958 RID: 22872 RVA: 0x002DFF85 File Offset: 0x002DE185
		[Serialize(1f, IsPropertySaveable.No, "The scale of the crosshair sprite (if there is one).", "", false)]
		public float CrossHairScale { get; private set; }

		// Token: 0x06005959 RID: 22873 RVA: 0x002DFF90 File Offset: 0x002DE190
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.crossHairPosDirtyTimer -= deltaTime;
			this.currentCrossHairScale = (this.currentCrossHairPointerScale = ((cam == null) ? 1f : cam.Zoom));
			if (this.crosshairSprite != null)
			{
				Vector2 barrelDir = new ValueTuple<float, float>(MathF.Cos(this.item.body.TransformedRotation), -MathF.Sin(this.item.body.TransformedRotation));
				float mouseDist = Vector2.Distance(this.BarrelScreenPos, PlayerInput.MousePosition);
				this.crosshairPos = Vector2.Clamp(this.BarrelScreenPos + barrelDir * mouseDist, Vector2.Zero, new ValueTuple<float, float>((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight));
				float spread = this.GetSpread(character);
				Projectile projectile = this.FindProjectile(false);
				if (projectile != null)
				{
					spread += MathHelper.ToRadians(projectile.Spread);
				}
				float spreadAtRange = MathF.Sin(spread) * Vector2.Distance(this.BarrelScreenPos, this.crosshairPos);
				this.currentCrossHairPointerScale = MathHelper.Clamp(spreadAtRange / Math.Min(this.crosshairSprite.size.X, this.crosshairSprite.size.Y), 0.1f, 10f);
			}
			this.currentCrossHairScale *= this.CrossHairScale;
			this.crosshairPointerPos = PlayerInput.MousePosition;
		}

		// Token: 0x0600595A RID: 22874 RVA: 0x002E00ED File Offset: 0x002DE2ED
		public override void FlipX(bool relativeToSub)
		{
			this.crossHairPosDirtyTimer = 0.02f;
		}

		// Token: 0x0600595B RID: 22875 RVA: 0x002E00FA File Offset: 0x002DE2FA
		public override void FlipY(bool relativeToSub)
		{
			this.crossHairPosDirtyTimer = 0.02f;
		}

		// Token: 0x0600595C RID: 22876 RVA: 0x002E0108 File Offset: 0x002DE308
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			if (character == null || !character.IsKeyDown(InputType.Aim) || !character.CanAim)
			{
				return;
			}
			Item viewTargetItem = character.ViewTarget as Item;
			if (viewTargetItem != null && viewTargetItem.Prefab.FocusOnSelected)
			{
				return;
			}
			if (!character.HeldItems.Contains(this.item))
			{
				return;
			}
			base.DrawHUD(spriteBatch, character);
			GUI.HideCursor = ((this.crosshairSprite != null || this.crosshairPointerSprite != null) && GUI.MouseOn == null && !Inventory.IsMouseOnInventory && !GameMain.Instance.Paused);
			if (GUI.HideCursor && !character.AnimController.IsHoldingToRope)
			{
				if (this.crossHairPosDirtyTimer <= 0f)
				{
					Sprite sprite = this.crosshairSprite;
					if (sprite != null)
					{
						sprite.Draw(spriteBatch, this.crosshairPos, (this.ReloadTimer <= 0f) ? Color.White : (Color.White * 0.2f), 0f, this.currentCrossHairScale, SpriteEffects.None, null);
					}
				}
				Sprite sprite2 = this.crosshairPointerSprite;
				if (sprite2 != null)
				{
					sprite2.Draw(spriteBatch, this.crosshairPointerPos, 0f, this.currentCrossHairPointerScale, SpriteEffects.None);
				}
			}
			if (GameMain.DebugDraw)
			{
				Vector2 barrelPos = this.item.DrawPosition + ConvertUnits.ToDisplayUnits(this.TransformedBarrelPos);
				barrelPos = Screen.Selected.Cam.WorldToScreen(barrelPos);
				GUI.DrawLine(spriteBatch, barrelPos - Vector2.UnitY * 3f, barrelPos + Vector2.UnitY * 3f, Color.Red, 0f, 1f);
				GUI.DrawLine(spriteBatch, barrelPos - Vector2.UnitX * 3f, barrelPos + Vector2.UnitX * 3f, Color.Red, 0f, 1f);
			}
		}

		// Token: 0x0600595D RID: 22877 RVA: 0x002E02E6 File Offset: 0x002DE4E6
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.crosshairSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.crosshairSprite = null;
			Sprite sprite2 = this.crosshairPointerSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			this.crosshairSprite = null;
		}

		// Token: 0x17001663 RID: 5731
		// (get) Token: 0x0600595E RID: 22878 RVA: 0x002E031E File Offset: 0x002DE51E
		// (set) Token: 0x0600595F RID: 22879 RVA: 0x002E0326 File Offset: 0x002DE526
		public float ReloadTimer { get; private set; }

		// Token: 0x17001664 RID: 5732
		// (get) Token: 0x06005960 RID: 22880 RVA: 0x002E032F File Offset: 0x002DE52F
		// (set) Token: 0x06005961 RID: 22881 RVA: 0x002E0341 File Offset: 0x002DE541
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

		// Token: 0x17001665 RID: 5733
		// (get) Token: 0x06005962 RID: 22882 RVA: 0x002E0355 File Offset: 0x002DE555
		// (set) Token: 0x06005963 RID: 22883 RVA: 0x002E035D File Offset: 0x002DE55D
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

		// Token: 0x17001666 RID: 5734
		// (get) Token: 0x06005964 RID: 22884 RVA: 0x002E0370 File Offset: 0x002DE570
		// (set) Token: 0x06005965 RID: 22885 RVA: 0x002E0378 File Offset: 0x002DE578
		[Serialize(0f, IsPropertySaveable.No, "Weapons skill requirement to reload at normal speed.", "", false)]
		public float ReloadSkillRequirement { get; set; }

		// Token: 0x17001667 RID: 5735
		// (get) Token: 0x06005966 RID: 22886 RVA: 0x002E0381 File Offset: 0x002DE581
		// (set) Token: 0x06005967 RID: 22887 RVA: 0x002E0389 File Offset: 0x002DE589
		[Serialize(1f, IsPropertySaveable.No, "Reload time at 0 skill level. Reload time scales with skill level up to the Weapons skill requirement.", "", false)]
		public float ReloadNoSkill { get; set; }

		// Token: 0x17001668 RID: 5736
		// (get) Token: 0x06005968 RID: 22888 RVA: 0x002E0392 File Offset: 0x002DE592
		// (set) Token: 0x06005969 RID: 22889 RVA: 0x002E039A File Offset: 0x002DE59A
		[Serialize(false, IsPropertySaveable.No, "Tells the AI to hold the trigger down when it uses this weapon", "", false)]
		public bool HoldTrigger { get; set; }

		// Token: 0x17001669 RID: 5737
		// (get) Token: 0x0600596A RID: 22890 RVA: 0x002E03A3 File Offset: 0x002DE5A3
		// (set) Token: 0x0600596B RID: 22891 RVA: 0x002E03AB File Offset: 0x002DE5AB
		[Serialize(1, IsPropertySaveable.No, "How many projectiles the weapon launches when fired once.", "", false)]
		public int ProjectileCount { get; set; }

		// Token: 0x1700166A RID: 5738
		// (get) Token: 0x0600596C RID: 22892 RVA: 0x002E03B4 File Offset: 0x002DE5B4
		// (set) Token: 0x0600596D RID: 22893 RVA: 0x002E03BC File Offset: 0x002DE5BC
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles when used by a character with sufficient skills to use the weapon (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x1700166B RID: 5739
		// (get) Token: 0x0600596E RID: 22894 RVA: 0x002E03C5 File Offset: 0x002DE5C5
		// (set) Token: 0x0600596F RID: 22895 RVA: 0x002E03CD File Offset: 0x002DE5CD
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles when used by a character with insufficient skills to use the weapon (in degrees).", "", false)]
		public float UnskilledSpread { get; set; }

		// Token: 0x1700166C RID: 5740
		// (get) Token: 0x06005970 RID: 22896 RVA: 0x002E03D6 File Offset: 0x002DE5D6
		// (set) Token: 0x06005971 RID: 22897 RVA: 0x002E03DE File Offset: 0x002DE5DE
		[Serialize(0f, IsPropertySaveable.No, "The impulse applied to the physics body of the projectile (the higher the impulse, the faster the projectiles are launched). Sum of weapon + projectile.", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x1700166D RID: 5741
		// (get) Token: 0x06005972 RID: 22898 RVA: 0x002E03E7 File Offset: 0x002DE5E7
		// (set) Token: 0x06005973 RID: 22899 RVA: 0x002E03EF File Offset: 0x002DE5EF
		[Serialize(0f, IsPropertySaveable.Yes, "Percentage of damage mitigation ignored when hitting armored body parts (deflecting limbs). Sum of weapon + projectile.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Penetration { get; private set; }

		// Token: 0x1700166E RID: 5742
		// (get) Token: 0x06005974 RID: 22900 RVA: 0x002E03F8 File Offset: 0x002DE5F8
		// (set) Token: 0x06005975 RID: 22901 RVA: 0x002E0400 File Offset: 0x002DE600
		[Serialize(1f, IsPropertySaveable.Yes, "Weapon's damage modifier", "", false)]
		public float WeaponDamageModifier { get; private set; }

		// Token: 0x1700166F RID: 5743
		// (get) Token: 0x06005976 RID: 22902 RVA: 0x002E0409 File Offset: 0x002DE609
		// (set) Token: 0x06005977 RID: 22903 RVA: 0x002E0411 File Offset: 0x002DE611
		[Serialize(0f, IsPropertySaveable.Yes, "The time required for a charge-type turret to charge up before able to fire.", "", false)]
		public float MaxChargeTime { get; private set; }

		// Token: 0x17001670 RID: 5744
		// (get) Token: 0x06005978 RID: 22904 RVA: 0x002E041A File Offset: 0x002DE61A
		// (set) Token: 0x06005979 RID: 22905 RVA: 0x002E0422 File Offset: 0x002DE622
		[Serialize(1f, IsPropertySaveable.Yes, "Penalty multiplier to reload time when dual-wielding.", "", false)]
		public float DualWieldReloadTimePenaltyMultiplier { get; private set; }

		// Token: 0x17001671 RID: 5745
		// (get) Token: 0x0600597A RID: 22906 RVA: 0x002E042B File Offset: 0x002DE62B
		// (set) Token: 0x0600597B RID: 22907 RVA: 0x002E0433 File Offset: 0x002DE633
		[Serialize(0f, IsPropertySaveable.Yes, "Additive penalty to accuracy (spread angle) when dual-wielding.", "", false)]
		public float DualWieldAccuracyPenalty { get; private set; }

		// Token: 0x17001672 RID: 5746
		// (get) Token: 0x0600597C RID: 22908 RVA: 0x002E043C File Offset: 0x002DE63C
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

		// Token: 0x17001673 RID: 5747
		// (get) Token: 0x0600597D RID: 22909 RVA: 0x002E04C9 File Offset: 0x002DE6C9
		// (set) Token: 0x0600597E RID: 22910 RVA: 0x002E04D1 File Offset: 0x002DE6D1
		public Projectile LastProjectile { get; private set; }

		// Token: 0x0600597F RID: 22911 RVA: 0x002E04DC File Offset: 0x002DE6DC
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
			this.InitProjSpecific(element);
		}

		// Token: 0x06005980 RID: 22912 RVA: 0x002E05A0 File Offset: 0x002DE7A0
		private void InitProjSpecific(ContentXElement rangedWeaponElement)
		{
			foreach (ContentXElement subElement in rangedWeaponElement.Elements())
			{
				string textureDir = base.GetTextureDirectory(subElement);
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "crosshair"))
				{
					if (!(a == "crosshairpointer"))
					{
						if (!(a == "particleemitter"))
						{
							if (!(a == "particleemittercharge"))
							{
								if (a == "chargesound")
								{
									this.chargeSound = RoundSound.Load(subElement);
								}
							}
							else
							{
								this.particleEmitterCharges.Add(new ParticleEmitter(subElement));
							}
						}
						else
						{
							this.particleEmitters.Add(new ParticleEmitter(subElement));
						}
					}
					else
					{
						this.crosshairPointerSprite = new Sprite(subElement, textureDir, "", false, 1f);
					}
				}
				else
				{
					this.crosshairSprite = new Sprite(subElement, textureDir, "", false, 1f);
				}
			}
		}

		// Token: 0x06005981 RID: 22913 RVA: 0x002E06B0 File Offset: 0x002DE8B0
		public override void Equip(Character character)
		{
			this.ReloadTimer = Math.Max(Math.Min(this.reload, 1f), this.ReloadTimer);
			this.IsActive = true;
		}

		// Token: 0x06005982 RID: 22914 RVA: 0x002E06DC File Offset: 0x002DE8DC
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
			}
			else if (this.currentChargeTime < previousChargeTime)
			{
				this.currentChargingState = RangedWeapon.ChargingState.WindingDown;
			}
			else
			{
				this.currentChargingState = RangedWeapon.ChargingState.WindingUp;
			}
			this.UpdateProjSpecific(deltaTime);
		}

		// Token: 0x06005983 RID: 22915 RVA: 0x002E07A4 File Offset: 0x002DE9A4
		private void UpdateProjSpecific(float deltaTime)
		{
			float chargeRatio = this.currentChargeTime / this.MaxChargeTime;
			RangedWeapon.ChargingState chargingState = this.currentChargingState;
			if (chargingState - RangedWeapon.ChargingState.WindingUp <= 1)
			{
				Vector2 particlePos = this.item.WorldPosition + ConvertUnits.ToDisplayUnits(this.TransformedBarrelPos);
				float sizeMultiplier = Math.Clamp(chargeRatio, 0.1f, 1f);
				foreach (ParticleEmitter emitter in this.particleEmitterCharges)
				{
					emitter.Emit(deltaTime, particlePos, this.item.CurrentHull, 0f, 0f, 1f, sizeMultiplier, 1f, new Color?(emitter.Prefab.Properties.ColorMultiplier), null, false, null);
				}
				if (this.chargeSoundChannel == null || !this.chargeSoundChannel.IsPlaying)
				{
					if (this.chargeSound != null)
					{
						RoundSound sound = this.chargeSound;
						Vector2 worldPosition = this.item.WorldPosition;
						Hull currentHull = this.item.CurrentHull;
						this.chargeSoundChannel = SoundPlayer.PlaySound(sound, worldPosition, null, currentHull);
						if (this.chargeSoundChannel != null)
						{
							this.chargeSoundChannel.Looping = true;
							return;
						}
					}
				}
				else if (this.chargeSoundChannel != null)
				{
					this.chargeSoundChannel.FrequencyMultiplier = MathHelper.Lerp(this.ChargeSoundWindupPitchSlide.X, this.ChargeSoundWindupPitchSlide.Y, chargeRatio);
					this.chargeSoundChannel.Position = new Vector3?(new Vector3(this.item.WorldPosition, 0f));
					return;
				}
			}
			else if (this.chargeSoundChannel != null)
			{
				if (this.chargeSoundChannel.IsPlaying)
				{
					this.chargeSoundChannel.FadeOutAndDispose();
					this.chargeSoundChannel.Looping = false;
					return;
				}
				this.chargeSoundChannel = null;
			}
		}

		// Token: 0x06005984 RID: 22916 RVA: 0x002E0978 File Offset: 0x002DEB78
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

		// Token: 0x06005985 RID: 22917 RVA: 0x002E09F8 File Offset: 0x002DEBF8
		private static float ApplyDualWieldPenaltyReduction(Character character, float originalPenalty, float neutralValue)
		{
			float statAdjustmentPrc = character.GetStatValue(StatTypes.DualWieldingPenaltyReduction, true);
			statAdjustmentPrc = MathHelper.Clamp(statAdjustmentPrc, 0f, 1f);
			return MathHelper.Lerp(originalPenalty, neutralValue, statAdjustmentPrc);
		}

		// Token: 0x06005986 RID: 22918 RVA: 0x002E0A2C File Offset: 0x002DEC2C
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
			this.LaunchProjSpecific();
			return true;
		}

		// Token: 0x06005987 RID: 22919 RVA: 0x002E0F64 File Offset: 0x002DF164
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return this.characterUsable || character == null;
		}

		// Token: 0x06005988 RID: 22920 RVA: 0x002E0F74 File Offset: 0x002DF174
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

		// Token: 0x06005989 RID: 22921 RVA: 0x002E10FC File Offset: 0x002DF2FC
		private bool IsSuitableProjectile(Projectile projectile)
		{
			Projectile projectile2 = projectile;
			return ((projectile2 != null) ? projectile2.Item : null) != null && (!this.suitableProjectiles.Any<Identifier>() || this.suitableProjectiles.Any((Identifier s) => projectile.Item.Prefab.Identifier == s || projectile.Item.HasTag(s)));
		}

		// Token: 0x0600598A RID: 22922 RVA: 0x002E1154 File Offset: 0x002DF354
		private void LaunchProjSpecific()
		{
			Vector2 particlePos = this.item.WorldPosition + ConvertUnits.ToDisplayUnits(this.TransformedBarrelPos);
			float rotation = this.item.body.Rotation;
			if (this.item.body.Dir < 0f)
			{
				rotation += 3.1415927f;
			}
			foreach (ParticleEmitter emitter in this.particleEmitters)
			{
				emitter.Emit(1f, particlePos, this.item.CurrentHull, rotation, -rotation, 1f, 1f, 1f, null, null, false, null);
			}
		}

		// Token: 0x04002D8B RID: 11659
		protected Sprite crosshairSprite;

		// Token: 0x04002D8C RID: 11660
		protected Sprite crosshairPointerSprite;

		// Token: 0x04002D8D RID: 11661
		protected Vector2 crosshairPos;

		// Token: 0x04002D8E RID: 11662
		protected Vector2 crosshairPointerPos;

		// Token: 0x04002D8F RID: 11663
		protected float currentCrossHairScale;

		// Token: 0x04002D90 RID: 11664
		protected float currentCrossHairPointerScale;

		// Token: 0x04002D91 RID: 11665
		private RoundSound chargeSound;

		// Token: 0x04002D92 RID: 11666
		private SoundChannel chargeSoundChannel;

		// Token: 0x04002D93 RID: 11667
		private Vector2 _chargeSoundWindupPitchSlide;

		// Token: 0x04002D94 RID: 11668
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x04002D95 RID: 11669
		private readonly List<ParticleEmitter> particleEmitterCharges = new List<ParticleEmitter>();

		// Token: 0x04002D96 RID: 11670
		private float crossHairPosDirtyTimer;

		// Token: 0x04002D98 RID: 11672
		private float reload;

		// Token: 0x04002D9A RID: 11674
		private Vector2 barrelPos;

		// Token: 0x04002DA7 RID: 11687
		private readonly IReadOnlySet<Identifier> suitableProjectiles;

		// Token: 0x04002DA8 RID: 11688
		private RangedWeapon.ChargingState currentChargingState;

		// Token: 0x04002DAA RID: 11690
		private float currentChargeTime;

		// Token: 0x04002DAB RID: 11691
		private bool tryingToCharge;

		// Token: 0x04002DAC RID: 11692
		private readonly List<Body> ignoredBodies = new List<Body>();

		// Token: 0x020013B3 RID: 5043
		private enum ChargingState
		{
			// Token: 0x0400633C RID: 25404
			Inactive,
			// Token: 0x0400633D RID: 25405
			WindingUp,
			// Token: 0x0400633E RID: 25406
			WindingDown
		}
	}
}
