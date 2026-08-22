using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000AC RID: 172
	internal class Attack : ISerializableEntity
	{
		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x0600149C RID: 5276 RVA: 0x000B71DE File Offset: 0x000B53DE
		// (set) Token: 0x0600149D RID: 5277 RVA: 0x000B71E6 File Offset: 0x000B53E6
		[Serialize(AttackContext.Any, IsPropertySaveable.Yes, "The attack will be used only in this context.", "", false)]
		[Editable]
		public AttackContext Context { get; private set; }

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x0600149E RID: 5278 RVA: 0x000B71EF File Offset: 0x000B53EF
		// (set) Token: 0x0600149F RID: 5279 RVA: 0x000B71F7 File Offset: 0x000B53F7
		[Serialize(AttackTarget.Any, IsPropertySaveable.Yes, "Does the attack target only specific targets?", "", false)]
		[Editable]
		public AttackTarget TargetType { get; private set; }

		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x060014A0 RID: 5280 RVA: 0x000B7200 File Offset: 0x000B5400
		// (set) Token: 0x060014A1 RID: 5281 RVA: 0x000B7208 File Offset: 0x000B5408
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "To which limb is the attack aimed at? If not defined or set to none, the closest limb is used (default).", "", false)]
		[Editable]
		public LimbType TargetLimbType { get; private set; }

		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x060014A2 RID: 5282 RVA: 0x000B7211 File Offset: 0x000B5411
		// (set) Token: 0x060014A3 RID: 5283 RVA: 0x000B7219 File Offset: 0x000B5419
		[Serialize(HitDetection.Distance, IsPropertySaveable.Yes, "Collision detection is more accurate, but it only affects targets that are in contact with the limb.", "", false)]
		[Editable]
		public HitDetection HitDetectionType { get; private set; }

		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x060014A4 RID: 5284 RVA: 0x000B7222 File Offset: 0x000B5422
		// (set) Token: 0x060014A5 RID: 5285 RVA: 0x000B722A File Offset: 0x000B542A
		[Serialize(AIBehaviorAfterAttack.FallBack, IsPropertySaveable.Yes, "The preferred AI behavior after the attack.", "", false)]
		[Editable]
		public AIBehaviorAfterAttack AfterAttack { get; set; }

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x060014A6 RID: 5286 RVA: 0x000B7233 File Offset: 0x000B5433
		// (set) Token: 0x060014A7 RID: 5287 RVA: 0x000B723B File Offset: 0x000B543B
		[Serialize(0f, IsPropertySaveable.Yes, "A delay before reacting after performing an attack.", "", false)]
		[Editable]
		public float AfterAttackDelay { get; set; }

		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x060014A8 RID: 5288 RVA: 0x000B7244 File Offset: 0x000B5444
		// (set) Token: 0x060014A9 RID: 5289 RVA: 0x000B724C File Offset: 0x000B544C
		[Serialize(AIBehaviorAfterAttack.FallBack, IsPropertySaveable.Yes, "Secondary AI behavior after the attack. The character first executes the AfterAttack behavior, then after AfterAttackSecondaryDelay passes, switches to this one. Ignored if AfterAttackSecondaryDelay is 0 or less.", "", false)]
		[Editable]
		public AIBehaviorAfterAttack AfterAttackSecondary { get; set; }

		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x060014AA RID: 5290 RVA: 0x000B7255 File Offset: 0x000B5455
		// (set) Token: 0x060014AB RID: 5291 RVA: 0x000B725D File Offset: 0x000B545D
		[Serialize(0f, IsPropertySaveable.Yes, "How long the character executes the AfterAttack before switching to AfterAttackSecondary. The secondary behavior is ignored if this value is 0 or less.", "", false)]
		[Editable]
		public float AfterAttackSecondaryDelay { get; set; }

		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x060014AC RID: 5292 RVA: 0x000B7266 File Offset: 0x000B5466
		// (set) Token: 0x060014AD RID: 5293 RVA: 0x000B726E File Offset: 0x000B546E
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI try to turn around when aiming with this attack?", "", false)]
		[Editable]
		public bool Reverse { get; private set; }

		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x060014AE RID: 5294 RVA: 0x000B7277 File Offset: 0x000B5477
		// (set) Token: 0x060014AF RID: 5295 RVA: 0x000B727F File Offset: 0x000B547F
		[Serialize(true, IsPropertySaveable.Yes, "Should the rope attached to this limb snap upon choosing a new attack?", "", false)]
		[Editable]
		public bool SnapRopeOnNewAttack { get; private set; }

		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x060014B0 RID: 5296 RVA: 0x000B7288 File Offset: 0x000B5488
		// (set) Token: 0x060014B1 RID: 5297 RVA: 0x000B7290 File Offset: 0x000B5490
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI try to steer away from the target when aiming with this attack? Best combined with PassiveAggressive behavior.", "", false)]
		[Editable]
		public bool Retreat { get; private set; }

		// Token: 0x170005F1 RID: 1521
		// (get) Token: 0x060014B2 RID: 5298 RVA: 0x000B7299 File Offset: 0x000B5499
		// (set) Token: 0x060014B3 RID: 5299 RVA: 0x000B72A8 File Offset: 0x000B54A8
		[Serialize(0f, IsPropertySaveable.Yes, "The min distance from the attack limb to the target before the AI tries to attack.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float Range
		{
			get
			{
				return this._range * this.RangeMultiplier;
			}
			set
			{
				this._range = value;
			}
		}

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x060014B4 RID: 5300 RVA: 0x000B72B1 File Offset: 0x000B54B1
		// (set) Token: 0x060014B5 RID: 5301 RVA: 0x000B72C0 File Offset: 0x000B54C0
		[Serialize(0f, IsPropertySaveable.Yes, "The min distance from the attack limb to the target to do damage. In distance-based hit detection, the hit will be registered as soon as the target is within the damage range, unless the attack duration has expired.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float DamageRange
		{
			get
			{
				return this._damageRange * this.RangeMultiplier;
			}
			set
			{
				this._damageRange = value;
			}
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x060014B6 RID: 5302 RVA: 0x000B72C9 File Offset: 0x000B54C9
		// (set) Token: 0x060014B7 RID: 5303 RVA: 0x000B72D1 File Offset: 0x000B54D1
		[Serialize(0f, IsPropertySaveable.Yes, "Used by enemy AI to determine the minimum range required for the attack to hit.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float MinRange { get; private set; }

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x060014B8 RID: 5304 RVA: 0x000B72DA File Offset: 0x000B54DA
		// (set) Token: 0x060014B9 RID: 5305 RVA: 0x000B72E2 File Offset: 0x000B54E2
		[Serialize(0.25f, IsPropertySaveable.Yes, "An approximation of the attack duration. Effectively defines the time window in which the hit can be registered. If set to too low value, it's possible that the attack won't hit the target in time.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float Duration { get; private set; }

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x060014BA RID: 5306 RVA: 0x000B72EB File Offset: 0x000B54EB
		// (set) Token: 0x060014BB RID: 5307 RVA: 0x000B72F3 File Offset: 0x000B54F3
		[Serialize(5f, IsPropertySaveable.Yes, "How long the AI must wait before it can use this attack again.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float CoolDown { get; set; } = 5f;

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x060014BC RID: 5308 RVA: 0x000B72FC File Offset: 0x000B54FC
		// (set) Token: 0x060014BD RID: 5309 RVA: 0x000B7304 File Offset: 0x000B5504
		[Serialize(0f, IsPropertySaveable.Yes, "When the attack cooldown is running and when there are other valid attacks possible for the character to use, the secondary cooldown is used instead of the regular cooldown. Does not have an effect, if set to 0 or less than the regular cooldown value.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float SecondaryCoolDown { get; set; }

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x060014BE RID: 5310 RVA: 0x000B730D File Offset: 0x000B550D
		// (set) Token: 0x060014BF RID: 5311 RVA: 0x000B7315 File Offset: 0x000B5515
		[Serialize(0f, IsPropertySaveable.Yes, "A random factor applied to all cooldowns. Example: 0.1 -> adds a random value between -10% and 10% of the cooldown. Min 0 (default), Max 1 (could disable or double the cooldown in extreme cases).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float CoolDownRandomFactor { get; private set; }

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x060014C0 RID: 5312 RVA: 0x000B731E File Offset: 0x000B551E
		// (set) Token: 0x060014C1 RID: 5313 RVA: 0x000B7326 File Offset: 0x000B5526
		[Serialize(false, IsPropertySaveable.Yes, "When set to true, causes the enemy AI to use the fast movement animations when the attack is on cooldown.", "", false)]
		[Editable]
		public bool FullSpeedAfterAttack { get; private set; }

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x060014C2 RID: 5314 RVA: 0x000B732F File Offset: 0x000B552F
		// (set) Token: 0x060014C3 RID: 5315 RVA: 0x000B733E File Offset: 0x000B553E
		[Serialize(0f, IsPropertySaveable.Yes, "How much damage the attack does to submarine walls.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float StructureDamage
		{
			get
			{
				return this._structureDamage * this.DamageMultiplier;
			}
			set
			{
				this._structureDamage = value;
			}
		}

		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x060014C4 RID: 5316 RVA: 0x000B7347 File Offset: 0x000B5547
		// (set) Token: 0x060014C5 RID: 5317 RVA: 0x000B734F File Offset: 0x000B554F
		[Serialize(false, IsPropertySaveable.Yes, "If the attack causes an explosion of wall damage shrapnel, should some of the shrapnel be launched as projectiles that can go through walls?", "", false)]
		[Editable]
		public bool CreateWallDamageProjectiles { get; private set; }

		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x060014C6 RID: 5318 RVA: 0x000B7358 File Offset: 0x000B5558
		// (set) Token: 0x060014C7 RID: 5319 RVA: 0x000B7360 File Offset: 0x000B5560
		[Serialize(true, IsPropertySaveable.Yes, "Whether or not damaging structures with the attack causes damage particles to emit.", "", false)]
		[Editable]
		public bool EmitStructureDamageParticles { get; private set; }

		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x060014C8 RID: 5320 RVA: 0x000B7369 File Offset: 0x000B5569
		// (set) Token: 0x060014C9 RID: 5321 RVA: 0x000B7378 File Offset: 0x000B5578
		[Serialize(0f, IsPropertySaveable.Yes, "How much damage the attack does to items.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float ItemDamage
		{
			get
			{
				return this._itemDamage * this.DamageMultiplier;
			}
			set
			{
				this._itemDamage = value;
			}
		}

		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x060014CA RID: 5322 RVA: 0x000B7381 File Offset: 0x000B5581
		// (set) Token: 0x060014CB RID: 5323 RVA: 0x000B7389 File Offset: 0x000B5589
		[Serialize(0f, IsPropertySaveable.Yes, "Percentage of damage mitigation ignored when hitting armored body parts (deflecting limbs).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Penetration { get; private set; }

		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x060014CC RID: 5324 RVA: 0x000B7394 File Offset: 0x000B5594
		// (set) Token: 0x060014CD RID: 5325 RVA: 0x000B73BF File Offset: 0x000B55BF
		public float DamageMultiplier
		{
			get
			{
				float? damageMultiplier = this._damageMultiplier;
				if (damageMultiplier == null)
				{
					return this.initialDamageMultiplier;
				}
				return damageMultiplier.GetValueOrDefault();
			}
			set
			{
				if (this._damageMultiplier == null)
				{
					this.SetInitialDamageMultiplier(value);
				}
				this._damageMultiplier = new float?(value);
			}
		}

		// Token: 0x060014CE RID: 5326 RVA: 0x000B73E1 File Offset: 0x000B55E1
		public void ResetDamageMultiplier()
		{
			this._damageMultiplier = new float?(this.initialDamageMultiplier);
		}

		// Token: 0x060014CF RID: 5327 RVA: 0x000B73F4 File Offset: 0x000B55F4
		public void SetInitialDamageMultiplier(float value)
		{
			this.initialDamageMultiplier = value;
		}

		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x060014D0 RID: 5328 RVA: 0x000B73FD File Offset: 0x000B55FD
		// (set) Token: 0x060014D1 RID: 5329 RVA: 0x000B7405 File Offset: 0x000B5605
		public float RangeMultiplier { get; set; } = 1f;

		// Token: 0x17000600 RID: 1536
		// (get) Token: 0x060014D2 RID: 5330 RVA: 0x000B740E File Offset: 0x000B560E
		// (set) Token: 0x060014D3 RID: 5331 RVA: 0x000B7416 File Offset: 0x000B5616
		public float ImpactMultiplier { get; set; } = 1f;

		// Token: 0x17000601 RID: 1537
		// (get) Token: 0x060014D4 RID: 5332 RVA: 0x000B741F File Offset: 0x000B561F
		// (set) Token: 0x060014D5 RID: 5333 RVA: 0x000B742E File Offset: 0x000B562E
		[Serialize(0f, IsPropertySaveable.Yes, "How much damage the attack does to level walls.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float LevelWallDamage
		{
			get
			{
				return this._levelWallDamage * this.DamageMultiplier;
			}
			set
			{
				this._levelWallDamage = value;
			}
		}

		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x060014D6 RID: 5334 RVA: 0x000B7437 File Offset: 0x000B5637
		// (set) Token: 0x060014D7 RID: 5335 RVA: 0x000B743F File Offset: 0x000B563F
		[Serialize(false, IsPropertySaveable.Yes, "Sets whether or not the attack is ranged or not.", "", false)]
		[Editable]
		public bool Ranged { get; set; }

		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x060014D8 RID: 5336 RVA: 0x000B7448 File Offset: 0x000B5648
		// (set) Token: 0x060014D9 RID: 5337 RVA: 0x000B7450 File Offset: 0x000B5650
		[Serialize(false, IsPropertySaveable.Yes, "When enabled the attack will not be launched when there's a friendly character in the way. Only affects ranged attacks.", "", false)]
		[Editable]
		public bool AvoidFriendlyFire { get; set; }

		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x060014DA RID: 5338 RVA: 0x000B7459 File Offset: 0x000B5659
		// (set) Token: 0x060014DB RID: 5339 RVA: 0x000B7461 File Offset: 0x000B5661
		[Serialize(20f, IsPropertySaveable.Yes, "Used by enemy AI to determine how accurately the attack needs to be aimed for the attack to trigger. Only affects ranged attacks.", "", false)]
		[Editable]
		public float RequiredAngle { get; set; }

		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x060014DC RID: 5340 RVA: 0x000B746A File Offset: 0x000B566A
		// (set) Token: 0x060014DD RID: 5341 RVA: 0x000B7472 File Offset: 0x000B5672
		[Serialize(0f, IsPropertySaveable.Yes, "By default uses the same value as RequiredAngle. Use if you want to allow selecting the attack but not shooting until the angle is smaller. Only affects ranged attacks.", "", false)]
		[Editable]
		public float RequiredAngleToShoot { get; set; }

		// Token: 0x17000606 RID: 1542
		// (get) Token: 0x060014DE RID: 5342 RVA: 0x000B747B File Offset: 0x000B567B
		// (set) Token: 0x060014DF RID: 5343 RVA: 0x000B7483 File Offset: 0x000B5683
		[Serialize(0f, IsPropertySaveable.Yes, "How much the attack limb is rotated towards the target. Default 0 = no rotation. Only affects ranged attacks.", "", false)]
		[Editable]
		public float AimRotationTorque { get; set; }

		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x060014E0 RID: 5344 RVA: 0x000B748C File Offset: 0x000B568C
		// (set) Token: 0x060014E1 RID: 5345 RVA: 0x000B7494 File Offset: 0x000B5694
		[Serialize(-1, IsPropertySaveable.Yes, "Reference to the limb we apply the aim rotation to. By default same as the attack limb. Only affects ranged attacks.", "", false)]
		[Editable]
		public int RotationLimbIndex { get; set; }

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x060014E2 RID: 5346 RVA: 0x000B749D File Offset: 0x000B569D
		// (set) Token: 0x060014E3 RID: 5347 RVA: 0x000B74A5 File Offset: 0x000B56A5
		[Serialize(0f, IsPropertySaveable.Yes, "How much the held weapon is swayed back and forth while aiming. Only affects monsters using ranged weapons (items). Default 0 means the weapon is not swayed at all.", "", false)]
		[Editable]
		public float SwayAmount { get; set; }

		// Token: 0x17000609 RID: 1545
		// (get) Token: 0x060014E4 RID: 5348 RVA: 0x000B74AE File Offset: 0x000B56AE
		// (set) Token: 0x060014E5 RID: 5349 RVA: 0x000B74B6 File Offset: 0x000B56B6
		[Serialize(5f, IsPropertySaveable.Yes, "How fast the held weapon is swayed back and forth while aiming. Only affects monsters using ranged weapons (items).", "", false)]
		[Editable]
		public float SwayFrequency { get; set; }

		// Token: 0x1700060A RID: 1546
		// (get) Token: 0x060014E6 RID: 5350 RVA: 0x000B74BF File Offset: 0x000B56BF
		// (set) Token: 0x060014E7 RID: 5351 RVA: 0x000B74C7 File Offset: 0x000B56C7
		[Serialize(0f, IsPropertySaveable.No, "Legacy functionality. Behaves otherwise the same as stuns defined as afflictions, but explosions only apply the stun once instead of dividing it between the limbs.", "", false)]
		public float Stun { get; set; }

		// Token: 0x1700060B RID: 1547
		// (get) Token: 0x060014E8 RID: 5352 RVA: 0x000B74D0 File Offset: 0x000B56D0
		// (set) Token: 0x060014E9 RID: 5353 RVA: 0x000B74D8 File Offset: 0x000B56D8
		[Serialize(false, IsPropertySaveable.Yes, "Can damage only Humans.", "", false)]
		[Editable]
		public bool OnlyHumans { get; set; }

		// Token: 0x1700060C RID: 1548
		// (get) Token: 0x060014EA RID: 5354 RVA: 0x000B74E1 File Offset: 0x000B56E1
		// (set) Token: 0x060014EB RID: 5355 RVA: 0x000B74F4 File Offset: 0x000B56F4
		[Serialize("", IsPropertySaveable.Yes, "List of limb indices to apply the force into.", "", false)]
		[Editable]
		public string ApplyForceOnLimbs
		{
			get
			{
				return string.Join<int>(", ", this.ForceOnLimbIndices);
			}
			set
			{
				this.ForceOnLimbIndices.Clear();
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				foreach (string limbIndexStr in value.Split(',', StringSplitOptions.None))
				{
					int limbIndex;
					if (int.TryParse(limbIndexStr.Trim(), out limbIndex))
					{
						this.ForceOnLimbIndices.Add(limbIndex);
					}
				}
			}
		}

		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x060014EC RID: 5356 RVA: 0x000B754C File Offset: 0x000B574C
		// (set) Token: 0x060014ED RID: 5357 RVA: 0x000B7554 File Offset: 0x000B5754
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the attacking limb (or limbs defined using ApplyForceOnLimbs). The direction of the force is towards the target that's being attacked.", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float Force { get; private set; }

		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x060014EE RID: 5358 RVA: 0x000B755D File Offset: 0x000B575D
		// (set) Token: 0x060014EF RID: 5359 RVA: 0x000B7565 File Offset: 0x000B5765
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldStart { get; private set; }

		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x060014F0 RID: 5360 RVA: 0x000B756E File Offset: 0x000B576E
		// (set) Token: 0x060014F1 RID: 5361 RVA: 0x000B7576 File Offset: 0x000B5776
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldMiddle { get; private set; }

		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x060014F2 RID: 5362 RVA: 0x000B757F File Offset: 0x000B577F
		// (set) Token: 0x060014F3 RID: 5363 RVA: 0x000B7587 File Offset: 0x000B5787
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldEnd { get; private set; }

		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x060014F4 RID: 5364 RVA: 0x000B7590 File Offset: 0x000B5790
		public bool HasRootForce
		{
			get
			{
				return this.RootForceWorldStart != Vector2.Zero || this.RootForceWorldMiddle != Vector2.Zero || this.RootForceWorldEnd != Vector2.Zero;
			}
		}

		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x060014F5 RID: 5365 RVA: 0x000B75C8 File Offset: 0x000B57C8
		// (set) Token: 0x060014F6 RID: 5366 RVA: 0x000B75D0 File Offset: 0x000B57D0
		[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "Applied to the main limb. The transition smoothing of the applied force.", "", false)]
		[Editable]
		public TransitionMode RootTransitionEasing { get; private set; }

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x060014F7 RID: 5367 RVA: 0x000B75D9 File Offset: 0x000B57D9
		// (set) Token: 0x060014F8 RID: 5368 RVA: 0x000B75E1 File Offset: 0x000B57E1
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the attacking limb (or limbs defined using ApplyForceOnLimbs)", "", false)]
		[Editable(MinValueFloat = -10000f, MaxValueFloat = 10000f)]
		public float Torque { get; private set; }

		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x060014F9 RID: 5369 RVA: 0x000B75EA File Offset: 0x000B57EA
		// (set) Token: 0x060014FA RID: 5370 RVA: 0x000B75F2 File Offset: 0x000B57F2
		[Serialize(false, IsPropertySaveable.Yes, "Only apply the force once during the attacks lifetime.", "", false)]
		[Editable]
		public bool ApplyForcesOnlyOnce { get; private set; }

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x060014FB RID: 5371 RVA: 0x000B75FB File Offset: 0x000B57FB
		// (set) Token: 0x060014FC RID: 5372 RVA: 0x000B7603 File Offset: 0x000B5803
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the target the attack hits. The direction of the impulse is from this limb towards the target (use negative values to pull the target closer).", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float TargetImpulse { get; private set; }

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x060014FD RID: 5373 RVA: 0x000B760C File Offset: 0x000B580C
		// (set) Token: 0x060014FE RID: 5374 RVA: 0x000B7614 File Offset: 0x000B5814
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the target, in world space coordinates(i.e. 0, -1 pushes the target downwards). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 TargetImpulseWorld { get; private set; }

		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x060014FF RID: 5375 RVA: 0x000B761D File Offset: 0x000B581D
		// (set) Token: 0x06001500 RID: 5376 RVA: 0x000B7625 File Offset: 0x000B5825
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the target the attack hits. The direction of the force is from this limb towards the target (use negative values to pull the target closer).", "", false)]
		[Editable(-1000f, 1000f, 1)]
		public float TargetForce { get; private set; }

		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x06001501 RID: 5377 RVA: 0x000B762E File Offset: 0x000B582E
		// (set) Token: 0x06001502 RID: 5378 RVA: 0x000B7636 File Offset: 0x000B5836
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the target, in world space coordinates(i.e. 0, -1 pushes the target downwards). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 TargetForceWorld { get; private set; }

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x06001503 RID: 5379 RVA: 0x000B763F File Offset: 0x000B583F
		// (set) Token: 0x06001504 RID: 5380 RVA: 0x000B7647 File Offset: 0x000B5847
		[Serialize(1f, IsPropertySaveable.Yes, "Affects the strength of the impact effects the limb causes when it hits a submarine.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float SubmarineImpactMultiplier { get; private set; }

		// Token: 0x1700061A RID: 1562
		// (get) Token: 0x06001505 RID: 5381 RVA: 0x000B7650 File Offset: 0x000B5850
		// (set) Token: 0x06001506 RID: 5382 RVA: 0x000B7658 File Offset: 0x000B5858
		[Serialize(0f, IsPropertySaveable.Yes, "How likely the attack causes target limbs to be severed.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SeverLimbsProbability { get; set; }

		// Token: 0x1700061B RID: 1563
		// (get) Token: 0x06001507 RID: 5383 RVA: 0x000B7661 File Offset: 0x000B5861
		public float StickChance
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x1700061C RID: 1564
		// (get) Token: 0x06001508 RID: 5384 RVA: 0x000B7668 File Offset: 0x000B5868
		// (set) Token: 0x06001509 RID: 5385 RVA: 0x000B7670 File Offset: 0x000B5870
		[Serialize(0f, IsPropertySaveable.Yes, "Used by enemy AI to determine the priority when selecting attacks. When random attacks are disabled on the character it is multiplied with distance to determine the which attack to use. Only attacks that are currently valid are taken into consideration when making the decision.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Priority { get; private set; }

		// Token: 0x1700061D RID: 1565
		// (get) Token: 0x0600150A RID: 5386 RVA: 0x000B7679 File Offset: 0x000B5879
		// (set) Token: 0x0600150B RID: 5387 RVA: 0x000B7681 File Offset: 0x000B5881
		[Serialize(false, IsPropertySaveable.Yes, "Triggers the 'blink' animation on the attacking limbs when the attack executes. Used e.g. by abyss monsters to make their jaws close when attacking.", "", false)]
		[Editable]
		public bool Blink { get; private set; }

		// Token: 0x1700061E RID: 1566
		// (get) Token: 0x0600150C RID: 5388 RVA: 0x000B768A File Offset: 0x000B588A
		public IEnumerable<StatusEffect> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x1700061F RID: 1567
		// (get) Token: 0x0600150D RID: 5389 RVA: 0x000B7692 File Offset: 0x000B5892
		public string Name
		{
			get
			{
				return "Attack";
			}
		}

		// Token: 0x17000620 RID: 1568
		// (get) Token: 0x0600150E RID: 5390 RVA: 0x000B7699 File Offset: 0x000B5899
		// (set) Token: 0x0600150F RID: 5391 RVA: 0x000B76A1 File Offset: 0x000B58A1
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; } = new Dictionary<Identifier, SerializableProperty>();

		// Token: 0x17000621 RID: 1569
		// (get) Token: 0x06001510 RID: 5392 RVA: 0x000B76AA File Offset: 0x000B58AA
		// (set) Token: 0x06001511 RID: 5393 RVA: 0x000B76B2 File Offset: 0x000B58B2
		public List<PropertyConditional> Conditionals { get; private set; } = new List<PropertyConditional>();

		// Token: 0x06001512 RID: 5394 RVA: 0x000B76BC File Offset: 0x000B58BC
		public void SetUser(Character user)
		{
			if (this.statusEffects == null)
			{
				return;
			}
			foreach (StatusEffect statusEffect in this.statusEffects)
			{
				statusEffect.SetUser(user);
			}
		}

		// Token: 0x17000622 RID: 1570
		// (get) Token: 0x06001513 RID: 5395 RVA: 0x000B7718 File Offset: 0x000B5918
		// (set) Token: 0x06001514 RID: 5396 RVA: 0x000B7720 File Offset: 0x000B5920
		public Item SourceItem { get; set; }

		// Token: 0x06001515 RID: 5397 RVA: 0x000B772C File Offset: 0x000B592C
		public List<Affliction> GetMultipliedAfflictions(float multiplier)
		{
			List<Affliction> multipliedAfflictions = new List<Affliction>();
			foreach (Affliction affliction in this.Afflictions.Keys)
			{
				multipliedAfflictions.Add(affliction.CreateMultiplied(multiplier, affliction));
			}
			return multipliedAfflictions;
		}

		// Token: 0x06001516 RID: 5398 RVA: 0x000B7794 File Offset: 0x000B5994
		public float GetStructureDamage(float deltaTime)
		{
			if (this.Duration != 0f)
			{
				return this.StructureDamage * deltaTime;
			}
			return this.StructureDamage;
		}

		// Token: 0x06001517 RID: 5399 RVA: 0x000B77B2 File Offset: 0x000B59B2
		public float GetLevelWallDamage(float deltaTime)
		{
			if (this.Duration != 0f)
			{
				return this.LevelWallDamage * deltaTime;
			}
			return this.LevelWallDamage;
		}

		// Token: 0x06001518 RID: 5400 RVA: 0x000B77D0 File Offset: 0x000B59D0
		public float GetItemDamage(float deltaTime, float multiplier = 1f)
		{
			float dmg = this.ItemDamage * multiplier;
			if (this.Duration != 0f)
			{
				return dmg * deltaTime;
			}
			return dmg;
		}

		// Token: 0x06001519 RID: 5401 RVA: 0x000B77F8 File Offset: 0x000B59F8
		public float GetTotalCharacterDamage()
		{
			float totalDamage = 0f;
			foreach (Affliction affliction in this.Afflictions.Keys)
			{
				float afflictionVitalityDecrease = affliction.GetVitalityDecrease(null);
				if (affliction.AffectedByAttackMultipliers)
				{
					afflictionVitalityDecrease *= this.DamageMultiplier;
				}
				totalDamage += afflictionVitalityDecrease;
			}
			return totalDamage;
		}

		// Token: 0x0600151A RID: 5402 RVA: 0x000B7870 File Offset: 0x000B5A70
		public Attack(float damage, float bleedingDamage, float burnDamage, float structureDamage, float itemDamage, float range = 0f)
		{
			if (damage > 0f)
			{
				this.Afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(damage, null), null);
			}
			if (bleedingDamage > 0f)
			{
				this.Afflictions.Add(AfflictionPrefab.Bleeding.Instantiate(bleedingDamage, null), null);
			}
			if (burnDamage > 0f)
			{
				this.Afflictions.Add(AfflictionPrefab.Burn.Instantiate(burnDamage, null), null);
			}
			this.Range = range;
			this.DamageRange = range;
			this.LevelWallDamage = structureDamage;
			this.StructureDamage = structureDamage;
			this.ItemDamage = itemDamage;
		}

		// Token: 0x0600151B RID: 5403 RVA: 0x000B797A File Offset: 0x000B5B7A
		public Attack(ContentXElement element, string parentDebugName, Item sourceItem) : this(element, parentDebugName)
		{
			this.SourceItem = sourceItem;
		}

		// Token: 0x0600151C RID: 5404 RVA: 0x000B798C File Offset: 0x000B5B8C
		public Attack(ContentXElement element, string parentDebugName)
		{
			this.Deserialize(element, parentDebugName);
			if (element.GetAttribute("damage") != null || element.GetAttribute("bluntdamage") != null || element.GetAttribute("burndamage") != null || element.GetAttribute("bleedingdamage") != null)
			{
				DebugConsole.ThrowError("Error in Attack (" + parentDebugName + ") - Define damage as afflictions instead of using the damage attribute (e.g. <Affliction identifier=\"internaldamage\" strength=\"10\" />).", null, element.ContentPackage, false, false);
			}
			if (element.GetAttribute("LevelWallDamage") == null)
			{
				this.LevelWallDamage = this._structureDamage;
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "statuseffect"))
				{
					if (!(a == "affliction"))
					{
						if (a == "conditional")
						{
							this.Conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
						}
					}
					else if (subElement.GetAttribute("name") != null)
					{
						DebugConsole.ThrowError("Error in Attack (" + parentDebugName + ") - define afflictions using identifiers instead of names.", null, element.ContentPackage, false, false);
						string afflictionName = subElement.GetAttributeString("name", "").ToLowerInvariant();
						AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Name.Equals(afflictionName, StringComparison.OrdinalIgnoreCase));
						if (afflictionPrefab == null)
						{
							DebugConsole.ThrowError(string.Concat(new string[]
							{
								"Error in Attack (",
								parentDebugName,
								") - Affliction prefab \"",
								afflictionName,
								"\" not found."
							}), null, element.ContentPackage, false, false);
						}
					}
					else
					{
						Identifier afflictionIdentifier = subElement.GetAttributeIdentifier("identifier", "");
						AfflictionPrefab afflictionPrefab;
						if (!AfflictionPrefab.Prefabs.TryGet(afflictionIdentifier, out afflictionPrefab))
						{
							DebugConsole.ThrowError(string.Concat(new string[]
							{
								"Error in Attack (",
								parentDebugName,
								") - Affliction prefab \"",
								afflictionIdentifier.ToString(),
								"\" not found."
							}), null, element.ContentPackage, false, false);
						}
					}
				}
				else
				{
					this.statusEffects.Add(StatusEffect.Load(subElement, parentDebugName));
				}
			}
			if (this.SecondaryCoolDown > this.CoolDown)
			{
				DebugConsole.AddWarning("Potentially misconfigured attack in " + parentDebugName + ". Secondary cooldown should not be longer than the primary cooldown.", element.ContentPackage);
			}
		}

		// Token: 0x0600151D RID: 5405 RVA: 0x000B7C78 File Offset: 0x000B5E78
		public void ReloadAfflictions(ContentXElement element, string parentDebugName)
		{
			this.Afflictions.Clear();
			foreach (ContentXElement subElement in element.GetChildElements("affliction"))
			{
				Identifier afflictionIdentifier = subElement.GetAttributeIdentifier("identifier", "");
				AfflictionPrefab afflictionPrefab;
				if (!AfflictionPrefab.Prefabs.TryGet(afflictionIdentifier, out afflictionPrefab))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in an Attack defined in \"");
					defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
					defaultInterpolatedStringHandler.AppendLiteral("\" - could not find an affliction with the identifier \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(afflictionIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else
				{
					Affliction affliction = afflictionPrefab.Instantiate(0f, null);
					affliction.Deserialize(subElement);
					this.Afflictions.Add(affliction, subElement);
				}
			}
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x000B7D78 File Offset: 0x000B5F78
		public void Serialize(ContentXElement element)
		{
			SerializableProperty.SerializeProperties(this, element, true, false);
			foreach (KeyValuePair<Affliction, XElement> affliction in this.Afflictions)
			{
				if (affliction.Value != null)
				{
					affliction.Key.Serialize(affliction.Value);
				}
			}
		}

		// Token: 0x0600151F RID: 5407 RVA: 0x000B7DF0 File Offset: 0x000B5FF0
		public void Deserialize(ContentXElement element, string parentDebugName)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ReloadAfflictions(element, parentDebugName);
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x000B7E0C File Offset: 0x000B600C
		public AttackResult DoDamage(Character attacker, IDamageable target, Vector2 worldPosition, float deltaTime, bool playSound = true, PhysicsBody sourceBody = null, Limb sourceLimb = null)
		{
			Character targetCharacter = target as Character;
			if (this.OnlyHumans && targetCharacter != null && !targetCharacter.IsHuman)
			{
				return default(AttackResult);
			}
			this.SetUser(attacker);
			Vector2 impulseDirection = this.GetImpulseDirection(target as ISpatialEntity, worldPosition, this.SourceItem);
			AttackResult attackResult = (target != null) ? target.AddDamage(attacker, worldPosition, this, impulseDirection, deltaTime, playSound) : default(AttackResult);
			ActionType conditionalEffectType = (attackResult.Damage > 0f) ? ActionType.OnSuccess : ActionType.OnFailure;
			ActionType additionalEffectType = ActionType.OnUse;
			if (targetCharacter != null && targetCharacter.IsDead)
			{
				additionalEffectType = ActionType.OnEating;
			}
			foreach (StatusEffect effect in this.statusEffects)
			{
				effect.sourceBody = sourceBody;
				if (effect.HasTargetType(StatusEffect.TargetType.This) || effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					ISerializableEntity t = sourceLimb ?? attacker;
					if (additionalEffectType != ActionType.OnEating)
					{
						effect.Apply(conditionalEffectType, deltaTime, attacker, t, new Vector2?(worldPosition));
					}
					effect.Apply(additionalEffectType, deltaTime, attacker, t, new Vector2?(worldPosition));
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Parent))
				{
					if (additionalEffectType != ActionType.OnEating)
					{
						effect.Apply(conditionalEffectType, deltaTime, attacker, attacker, null);
					}
					effect.Apply(additionalEffectType, deltaTime, attacker, attacker, null);
				}
				if (targetCharacter != null)
				{
					if (effect.HasTargetType(StatusEffect.TargetType.Limb))
					{
						if (additionalEffectType != ActionType.OnEating)
						{
							effect.Apply(conditionalEffectType, deltaTime, targetCharacter, attackResult.HitLimb, null);
						}
						effect.Apply(additionalEffectType, deltaTime, targetCharacter, attackResult.HitLimb, null);
					}
					if (effect.HasTargetType(StatusEffect.TargetType.AllLimbs))
					{
						Limb[] targets = targetCharacter.AnimController.Limbs;
						if (additionalEffectType != ActionType.OnEating)
						{
							effect.Apply(conditionalEffectType, deltaTime, targetCharacter, targets, null);
						}
						effect.Apply(additionalEffectType, deltaTime, targetCharacter, targets, null);
					}
				}
				Entity targetEntity = target as Entity;
				if (targetEntity != null)
				{
					if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
					{
						this.targets.Clear();
						effect.AddNearbyTargets(worldPosition, this.targets);
						if (additionalEffectType != ActionType.OnEating)
						{
							effect.Apply(conditionalEffectType, deltaTime, targetEntity, this.targets, null);
						}
						effect.Apply(additionalEffectType, deltaTime, targetEntity, this.targets, null);
					}
					if (effect.HasTargetType(StatusEffect.TargetType.UseTarget))
					{
						if (additionalEffectType != ActionType.OnEating)
						{
							effect.Apply(conditionalEffectType, deltaTime, targetEntity, targetEntity as ISerializableEntity, new Vector2?(worldPosition));
						}
						effect.Apply(additionalEffectType, deltaTime, targetEntity, targetEntity as ISerializableEntity, new Vector2?(worldPosition));
					}
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Contained))
				{
					this.targets.Clear();
					this.targets.AddRange(attacker.Inventory.AllItems);
					if (additionalEffectType != ActionType.OnEating)
					{
						effect.Apply(conditionalEffectType, deltaTime, attacker, this.targets, null);
					}
					effect.Apply(additionalEffectType, deltaTime, attacker, this.targets, null);
				}
			}
			return attackResult;
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x000B8150 File Offset: 0x000B6350
		public AttackResult DoDamageToLimb(Character attacker, Limb targetLimb, Vector2 worldPosition, float deltaTime, bool playSound = true, PhysicsBody sourceBody = null, Limb sourceLimb = null)
		{
			if (targetLimb == null)
			{
				return default(AttackResult);
			}
			if (this.OnlyHumans && targetLimb.character != null && !targetLimb.character.IsHuman)
			{
				return default(AttackResult);
			}
			this.SetUser(attacker);
			float penetration = this.Penetration;
			Item sourceItem = this.SourceItem;
			RangedWeapon rangedWeapon;
			if ((rangedWeapon = ((sourceItem != null) ? sourceItem.GetComponent<RangedWeapon>() : null)) == null)
			{
				Item sourceItem2 = this.SourceItem;
				if (sourceItem2 == null)
				{
					rangedWeapon = null;
				}
				else
				{
					Projectile component = sourceItem2.GetComponent<Projectile>();
					if (component == null)
					{
						rangedWeapon = null;
					}
					else
					{
						Item launcher = component.Launcher;
						rangedWeapon = ((launcher != null) ? launcher.GetComponent<RangedWeapon>() : null);
					}
				}
			}
			RangedWeapon weapon = rangedWeapon;
			float? penetrationValue = (weapon != null) ? new float?(weapon.Penetration) : null;
			if (penetrationValue != null)
			{
				penetration += penetrationValue.Value;
			}
			Vector2 impulseDirection = this.GetImpulseDirection(targetLimb, worldPosition, this.SourceItem);
			AttackResult attackResult = targetLimb.character.ApplyAttack(attacker, worldPosition, this, deltaTime, impulseDirection, playSound, targetLimb, penetration);
			ActionType conditionalEffectType = (attackResult.Damage > 0f) ? ActionType.OnSuccess : ActionType.OnFailure;
			foreach (StatusEffect effect in this.statusEffects)
			{
				effect.sourceBody = sourceBody;
				if (effect.HasTargetType(StatusEffect.TargetType.This) || effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					effect.Apply(conditionalEffectType, deltaTime, attacker, sourceLimb ?? attacker, null);
					effect.Apply(ActionType.OnUse, deltaTime, attacker, sourceLimb ?? attacker, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Parent))
				{
					effect.Apply(conditionalEffectType, deltaTime, attacker, attacker, null);
					effect.Apply(ActionType.OnUse, deltaTime, attacker, attacker, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.UseTarget))
				{
					effect.Apply(conditionalEffectType, deltaTime, targetLimb.character, targetLimb.character, null);
					effect.Apply(ActionType.OnUse, deltaTime, targetLimb.character, targetLimb.character, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Limb))
				{
					effect.Apply(conditionalEffectType, deltaTime, targetLimb.character, targetLimb, null);
					effect.Apply(ActionType.OnUse, deltaTime, targetLimb.character, targetLimb, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.AllLimbs))
				{
					Limb[] targets = targetLimb.character.AnimController.Limbs;
					effect.Apply(conditionalEffectType, deltaTime, targetLimb.character, targets, null);
					effect.Apply(ActionType.OnUse, deltaTime, targetLimb.character, targets, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
				{
					this.targets.Clear();
					effect.AddNearbyTargets(worldPosition, this.targets);
					effect.Apply(conditionalEffectType, deltaTime, targetLimb.character, this.targets, null);
					effect.Apply(ActionType.OnUse, deltaTime, targetLimb.character, this.targets, null);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Contained))
				{
					this.targets.Clear();
					this.targets.AddRange(attacker.Inventory.AllItems);
					effect.Apply(conditionalEffectType, deltaTime, attacker, this.targets, null);
					effect.Apply(ActionType.OnUse, deltaTime, attacker, this.targets, null);
				}
			}
			return attackResult;
		}

		// Token: 0x06001522 RID: 5410 RVA: 0x000B84F4 File Offset: 0x000B66F4
		private Vector2 GetImpulseDirection(ISpatialEntity target, Vector2 sourceWorldPosition, Item sourceItem)
		{
			Vector2 impulseDirection = Vector2.Zero;
			if (target != null)
			{
				impulseDirection = target.WorldPosition - sourceWorldPosition;
			}
			if (((sourceItem != null) ? sourceItem.body : null) != null && sourceItem.body.Enabled && sourceItem.body.LinearVelocity.LengthSquared() > 0f)
			{
				impulseDirection = sourceItem.body.LinearVelocity;
			}
			else
			{
				Projectile projectileComponent = (sourceItem != null) ? sourceItem.GetComponent<Projectile>() : null;
				if (projectileComponent != null)
				{
					impulseDirection = new Vector2(MathF.Cos(this.SourceItem.Rotation), MathF.Sin(this.SourceItem.Rotation));
				}
			}
			if (impulseDirection.LengthSquared() > 0.0001f)
			{
				impulseDirection = Vector2.Normalize(impulseDirection);
			}
			return impulseDirection;
		}

		// Token: 0x17000623 RID: 1571
		// (get) Token: 0x06001523 RID: 5411 RVA: 0x000B85A7 File Offset: 0x000B67A7
		// (set) Token: 0x06001524 RID: 5412 RVA: 0x000B85AF File Offset: 0x000B67AF
		public float AttackTimer { get; private set; }

		// Token: 0x17000624 RID: 1572
		// (get) Token: 0x06001525 RID: 5413 RVA: 0x000B85B8 File Offset: 0x000B67B8
		// (set) Token: 0x06001526 RID: 5414 RVA: 0x000B85C0 File Offset: 0x000B67C0
		public float CoolDownTimer { get; set; }

		// Token: 0x17000625 RID: 1573
		// (get) Token: 0x06001527 RID: 5415 RVA: 0x000B85C9 File Offset: 0x000B67C9
		// (set) Token: 0x06001528 RID: 5416 RVA: 0x000B85D1 File Offset: 0x000B67D1
		public float CurrentRandomCoolDown { get; private set; }

		// Token: 0x17000626 RID: 1574
		// (get) Token: 0x06001529 RID: 5417 RVA: 0x000B85DA File Offset: 0x000B67DA
		// (set) Token: 0x0600152A RID: 5418 RVA: 0x000B85E2 File Offset: 0x000B67E2
		public float SecondaryCoolDownTimer { get; set; }

		// Token: 0x17000627 RID: 1575
		// (get) Token: 0x0600152B RID: 5419 RVA: 0x000B85EB File Offset: 0x000B67EB
		// (set) Token: 0x0600152C RID: 5420 RVA: 0x000B85F3 File Offset: 0x000B67F3
		public bool IsRunning { get; private set; }

		// Token: 0x17000628 RID: 1576
		// (get) Token: 0x0600152D RID: 5421 RVA: 0x000B85FC File Offset: 0x000B67FC
		// (set) Token: 0x0600152E RID: 5422 RVA: 0x000B8604 File Offset: 0x000B6804
		public float AfterAttackTimer { get; set; }

		// Token: 0x0600152F RID: 5423 RVA: 0x000B8610 File Offset: 0x000B6810
		public void UpdateCoolDown(float deltaTime)
		{
			this.CoolDownTimer -= deltaTime;
			this.SecondaryCoolDownTimer -= deltaTime;
			if (this.CoolDownTimer < 0f)
			{
				this.CoolDownTimer = 0f;
			}
			if (this.SecondaryCoolDownTimer < 0f)
			{
				this.SecondaryCoolDownTimer = 0f;
			}
		}

		// Token: 0x06001530 RID: 5424 RVA: 0x000B8669 File Offset: 0x000B6869
		public void UpdateAttackTimer(float deltaTime, Character character)
		{
			this.IsRunning = true;
			this.AttackTimer += deltaTime;
			if (this.AttackTimer >= this.Duration)
			{
				this.ResetAttackTimer();
				this.SetCoolDown(!character.IsPlayer);
			}
		}

		// Token: 0x06001531 RID: 5425 RVA: 0x000B86A3 File Offset: 0x000B68A3
		public void ResetAttackTimer()
		{
			this.AfterAttackTimer = 0f;
			this.AttackTimer = 0f;
			this.IsRunning = false;
		}

		// Token: 0x06001532 RID: 5426 RVA: 0x000B86C4 File Offset: 0x000B68C4
		public void SetCoolDown(bool applyRandom)
		{
			if (applyRandom)
			{
				float randomFraction = this.CoolDown * this.CoolDownRandomFactor;
				this.CurrentRandomCoolDown = MathHelper.Lerp(-randomFraction, randomFraction, Rand.Value(Rand.RandSync.Unsynced));
				this.CoolDownTimer = this.CoolDown + this.CurrentRandomCoolDown;
				randomFraction = this.SecondaryCoolDown * this.CoolDownRandomFactor;
				this.SecondaryCoolDownTimer = this.SecondaryCoolDown + MathHelper.Lerp(-randomFraction, randomFraction, Rand.Value(Rand.RandSync.Unsynced));
				return;
			}
			this.CoolDownTimer = this.CoolDown;
			this.SecondaryCoolDownTimer = this.SecondaryCoolDown;
			this.CurrentRandomCoolDown = 0f;
		}

		// Token: 0x06001533 RID: 5427 RVA: 0x000B8756 File Offset: 0x000B6956
		public void ResetCoolDown()
		{
			this.CoolDownTimer = 0f;
			this.SecondaryCoolDownTimer = 0f;
			this.CurrentRandomCoolDown = 0f;
		}

		// Token: 0x06001534 RID: 5428 RVA: 0x000B8779 File Offset: 0x000B6979
		public bool IsValidContext(AttackContext context)
		{
			return this.Context == context || this.Context == AttackContext.Any || this.Context == AttackContext.NotDefined;
		}

		// Token: 0x06001535 RID: 5429 RVA: 0x000B8798 File Offset: 0x000B6998
		public bool IsValidContext(IEnumerable<AttackContext> contexts)
		{
			using (IEnumerator<AttackContext> enumerator = contexts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case AttackContext.Water:
						if (this.Context == AttackContext.Ground)
						{
							return false;
						}
						break;
					case AttackContext.Ground:
						if (this.Context == AttackContext.Water)
						{
							return false;
						}
						break;
					case AttackContext.Inside:
						if (this.Context == AttackContext.Outside)
						{
							return false;
						}
						break;
					case AttackContext.Outside:
						if (this.Context == AttackContext.Inside)
						{
							return false;
						}
						break;
					}
				}
			}
			return true;
		}

		// Token: 0x06001536 RID: 5430 RVA: 0x000B882C File Offset: 0x000B6A2C
		public bool IsValidTarget(AttackTarget targetType)
		{
			return this.TargetType == AttackTarget.Any || this.TargetType.HasAnyFlag(targetType);
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x000B8844 File Offset: 0x000B6A44
		public bool IsValidTarget(Entity target)
		{
			switch (this.TargetType)
			{
			case AttackTarget.Character:
				return target is Character;
			case AttackTarget.Structure:
				return target is Structure || target is Hull || target is Item;
			case AttackTarget.Item:
				return target is Item;
			}
			return this.IsValidTarget(Attack.GetAttackTargetTypeFromEntity(target));
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x000B88BC File Offset: 0x000B6ABC
		private static AttackTarget GetAttackTargetTypeFromEntity(Entity entity)
		{
			AttackTarget result;
			if (!(entity is Character))
			{
				if (!(entity is Item))
				{
					if (!(entity is Structure))
					{
						if (!(entity is Hull))
						{
							result = AttackTarget.Any;
						}
						else
						{
							result = AttackTarget.Structure;
						}
					}
					else
					{
						result = AttackTarget.Structure;
					}
				}
				else
				{
					result = AttackTarget.Item;
				}
			}
			else
			{
				result = AttackTarget.Character;
			}
			return result;
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x000B8900 File Offset: 0x000B6B00
		public Vector2 CalculateAttackPhase(TransitionMode easing = TransitionMode.Linear)
		{
			float t = this.AttackTimer / this.Duration;
			return MathUtils.Bezier(this.RootForceWorldStart, this.RootForceWorldMiddle, this.RootForceWorldEnd, ToolBox.GetEasing(easing, t));
		}

		// Token: 0x040009DD RID: 2525
		private float _range;

		// Token: 0x040009DE RID: 2526
		private float _damageRange;

		// Token: 0x040009E5 RID: 2533
		private float _structureDamage;

		// Token: 0x040009E8 RID: 2536
		private float _itemDamage;

		// Token: 0x040009EA RID: 2538
		private float? _damageMultiplier;

		// Token: 0x040009EB RID: 2539
		private float initialDamageMultiplier = 1f;

		// Token: 0x040009EE RID: 2542
		private float _levelWallDamage;

		// Token: 0x04000A09 RID: 2569
		public readonly List<int> ForceOnLimbIndices = new List<int>();

		// Token: 0x04000A0A RID: 2570
		public readonly Dictionary<Affliction, XElement> Afflictions = new Dictionary<Affliction, XElement>();

		// Token: 0x04000A0C RID: 2572
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x04000A0E RID: 2574
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();
	}
}
