using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000026 RID: 38
	internal class Attack : ISerializableEntity
	{
		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000396 RID: 918 RVA: 0x0002353E File Offset: 0x0002173E
		// (set) Token: 0x06000397 RID: 919 RVA: 0x00023546 File Offset: 0x00021746
		[Serialize("StructureBlunt", IsPropertySaveable.Yes, "Name of the sound effect the attack makes when it hits a structure.", "", false)]
		[Editable]
		public string StructureSoundType { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000398 RID: 920 RVA: 0x0002354F File Offset: 0x0002174F
		// (set) Token: 0x06000399 RID: 921 RVA: 0x00023557 File Offset: 0x00021757
		[Serialize(AttackContext.Any, IsPropertySaveable.Yes, "The attack will be used only in this context.", "", false)]
		[Editable]
		public AttackContext Context { get; private set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600039A RID: 922 RVA: 0x00023560 File Offset: 0x00021760
		// (set) Token: 0x0600039B RID: 923 RVA: 0x00023568 File Offset: 0x00021768
		[Serialize(AttackTarget.Any, IsPropertySaveable.Yes, "Does the attack target only specific targets?", "", false)]
		[Editable]
		public AttackTarget TargetType { get; private set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x0600039C RID: 924 RVA: 0x00023571 File Offset: 0x00021771
		// (set) Token: 0x0600039D RID: 925 RVA: 0x00023579 File Offset: 0x00021779
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "To which limb is the attack aimed at? If not defined or set to none, the closest limb is used (default).", "", false)]
		[Editable]
		public LimbType TargetLimbType { get; private set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600039E RID: 926 RVA: 0x00023582 File Offset: 0x00021782
		// (set) Token: 0x0600039F RID: 927 RVA: 0x0002358A File Offset: 0x0002178A
		[Serialize(HitDetection.Distance, IsPropertySaveable.Yes, "Collision detection is more accurate, but it only affects targets that are in contact with the limb.", "", false)]
		[Editable]
		public HitDetection HitDetectionType { get; private set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060003A0 RID: 928 RVA: 0x00023593 File Offset: 0x00021793
		// (set) Token: 0x060003A1 RID: 929 RVA: 0x0002359B File Offset: 0x0002179B
		[Serialize(AIBehaviorAfterAttack.FallBack, IsPropertySaveable.Yes, "The preferred AI behavior after the attack.", "", false)]
		[Editable]
		public AIBehaviorAfterAttack AfterAttack { get; set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060003A2 RID: 930 RVA: 0x000235A4 File Offset: 0x000217A4
		// (set) Token: 0x060003A3 RID: 931 RVA: 0x000235AC File Offset: 0x000217AC
		[Serialize(0f, IsPropertySaveable.Yes, "A delay before reacting after performing an attack.", "", false)]
		[Editable]
		public float AfterAttackDelay { get; set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060003A4 RID: 932 RVA: 0x000235B5 File Offset: 0x000217B5
		// (set) Token: 0x060003A5 RID: 933 RVA: 0x000235BD File Offset: 0x000217BD
		[Serialize(AIBehaviorAfterAttack.FallBack, IsPropertySaveable.Yes, "Secondary AI behavior after the attack. The character first executes the AfterAttack behavior, then after AfterAttackSecondaryDelay passes, switches to this one. Ignored if AfterAttackSecondaryDelay is 0 or less.", "", false)]
		[Editable]
		public AIBehaviorAfterAttack AfterAttackSecondary { get; set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060003A6 RID: 934 RVA: 0x000235C6 File Offset: 0x000217C6
		// (set) Token: 0x060003A7 RID: 935 RVA: 0x000235CE File Offset: 0x000217CE
		[Serialize(0f, IsPropertySaveable.Yes, "How long the character executes the AfterAttack before switching to AfterAttackSecondary. The secondary behavior is ignored if this value is 0 or less.", "", false)]
		[Editable]
		public float AfterAttackSecondaryDelay { get; set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060003A8 RID: 936 RVA: 0x000235D7 File Offset: 0x000217D7
		// (set) Token: 0x060003A9 RID: 937 RVA: 0x000235DF File Offset: 0x000217DF
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI try to turn around when aiming with this attack?", "", false)]
		[Editable]
		public bool Reverse { get; private set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060003AA RID: 938 RVA: 0x000235E8 File Offset: 0x000217E8
		// (set) Token: 0x060003AB RID: 939 RVA: 0x000235F0 File Offset: 0x000217F0
		[Serialize(true, IsPropertySaveable.Yes, "Should the rope attached to this limb snap upon choosing a new attack?", "", false)]
		[Editable]
		public bool SnapRopeOnNewAttack { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x060003AC RID: 940 RVA: 0x000235F9 File Offset: 0x000217F9
		// (set) Token: 0x060003AD RID: 941 RVA: 0x00023601 File Offset: 0x00021801
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI try to steer away from the target when aiming with this attack? Best combined with PassiveAggressive behavior.", "", false)]
		[Editable]
		public bool Retreat { get; private set; }

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x060003AE RID: 942 RVA: 0x0002360A File Offset: 0x0002180A
		// (set) Token: 0x060003AF RID: 943 RVA: 0x00023619 File Offset: 0x00021819
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

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x060003B0 RID: 944 RVA: 0x00023622 File Offset: 0x00021822
		// (set) Token: 0x060003B1 RID: 945 RVA: 0x00023631 File Offset: 0x00021831
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

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x060003B2 RID: 946 RVA: 0x0002363A File Offset: 0x0002183A
		// (set) Token: 0x060003B3 RID: 947 RVA: 0x00023642 File Offset: 0x00021842
		[Serialize(0f, IsPropertySaveable.Yes, "Used by enemy AI to determine the minimum range required for the attack to hit.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float MinRange { get; private set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x060003B4 RID: 948 RVA: 0x0002364B File Offset: 0x0002184B
		// (set) Token: 0x060003B5 RID: 949 RVA: 0x00023653 File Offset: 0x00021853
		[Serialize(0.25f, IsPropertySaveable.Yes, "An approximation of the attack duration. Effectively defines the time window in which the hit can be registered. If set to too low value, it's possible that the attack won't hit the target in time.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float Duration { get; private set; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060003B6 RID: 950 RVA: 0x0002365C File Offset: 0x0002185C
		// (set) Token: 0x060003B7 RID: 951 RVA: 0x00023664 File Offset: 0x00021864
		[Serialize(5f, IsPropertySaveable.Yes, "How long the AI must wait before it can use this attack again.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float CoolDown { get; set; } = 5f;

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x060003B8 RID: 952 RVA: 0x0002366D File Offset: 0x0002186D
		// (set) Token: 0x060003B9 RID: 953 RVA: 0x00023675 File Offset: 0x00021875
		[Serialize(0f, IsPropertySaveable.Yes, "When the attack cooldown is running and when there are other valid attacks possible for the character to use, the secondary cooldown is used instead of the regular cooldown. Does not have an effect, if set to 0 or less than the regular cooldown value.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float SecondaryCoolDown { get; set; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060003BA RID: 954 RVA: 0x0002367E File Offset: 0x0002187E
		// (set) Token: 0x060003BB RID: 955 RVA: 0x00023686 File Offset: 0x00021886
		[Serialize(0f, IsPropertySaveable.Yes, "A random factor applied to all cooldowns. Example: 0.1 -> adds a random value between -10% and 10% of the cooldown. Min 0 (default), Max 1 (could disable or double the cooldown in extreme cases).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float CoolDownRandomFactor { get; private set; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060003BC RID: 956 RVA: 0x0002368F File Offset: 0x0002188F
		// (set) Token: 0x060003BD RID: 957 RVA: 0x00023697 File Offset: 0x00021897
		[Serialize(false, IsPropertySaveable.Yes, "When set to true, causes the enemy AI to use the fast movement animations when the attack is on cooldown.", "", false)]
		[Editable]
		public bool FullSpeedAfterAttack { get; private set; }

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060003BE RID: 958 RVA: 0x000236A0 File Offset: 0x000218A0
		// (set) Token: 0x060003BF RID: 959 RVA: 0x000236AF File Offset: 0x000218AF
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

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x000236B8 File Offset: 0x000218B8
		// (set) Token: 0x060003C1 RID: 961 RVA: 0x000236C0 File Offset: 0x000218C0
		[Serialize(false, IsPropertySaveable.Yes, "If the attack causes an explosion of wall damage shrapnel, should some of the shrapnel be launched as projectiles that can go through walls?", "", false)]
		[Editable]
		public bool CreateWallDamageProjectiles { get; private set; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x000236C9 File Offset: 0x000218C9
		// (set) Token: 0x060003C3 RID: 963 RVA: 0x000236D1 File Offset: 0x000218D1
		[Serialize(true, IsPropertySaveable.Yes, "Whether or not damaging structures with the attack causes damage particles to emit.", "", false)]
		[Editable]
		public bool EmitStructureDamageParticles { get; private set; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060003C4 RID: 964 RVA: 0x000236DA File Offset: 0x000218DA
		// (set) Token: 0x060003C5 RID: 965 RVA: 0x000236E9 File Offset: 0x000218E9
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

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060003C6 RID: 966 RVA: 0x000236F2 File Offset: 0x000218F2
		// (set) Token: 0x060003C7 RID: 967 RVA: 0x000236FA File Offset: 0x000218FA
		[Serialize(0f, IsPropertySaveable.Yes, "Percentage of damage mitigation ignored when hitting armored body parts (deflecting limbs).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Penetration { get; private set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060003C8 RID: 968 RVA: 0x00023704 File Offset: 0x00021904
		// (set) Token: 0x060003C9 RID: 969 RVA: 0x0002372F File Offset: 0x0002192F
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

		// Token: 0x060003CA RID: 970 RVA: 0x00023751 File Offset: 0x00021951
		public void ResetDamageMultiplier()
		{
			this._damageMultiplier = new float?(this.initialDamageMultiplier);
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00023764 File Offset: 0x00021964
		public void SetInitialDamageMultiplier(float value)
		{
			this.initialDamageMultiplier = value;
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060003CC RID: 972 RVA: 0x0002376D File Offset: 0x0002196D
		// (set) Token: 0x060003CD RID: 973 RVA: 0x00023775 File Offset: 0x00021975
		public float RangeMultiplier { get; set; } = 1f;

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060003CE RID: 974 RVA: 0x0002377E File Offset: 0x0002197E
		// (set) Token: 0x060003CF RID: 975 RVA: 0x00023786 File Offset: 0x00021986
		public float ImpactMultiplier { get; set; } = 1f;

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060003D0 RID: 976 RVA: 0x0002378F File Offset: 0x0002198F
		// (set) Token: 0x060003D1 RID: 977 RVA: 0x0002379E File Offset: 0x0002199E
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

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060003D2 RID: 978 RVA: 0x000237A7 File Offset: 0x000219A7
		// (set) Token: 0x060003D3 RID: 979 RVA: 0x000237AF File Offset: 0x000219AF
		[Serialize(false, IsPropertySaveable.Yes, "Sets whether or not the attack is ranged or not.", "", false)]
		[Editable]
		public bool Ranged { get; set; }

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060003D4 RID: 980 RVA: 0x000237B8 File Offset: 0x000219B8
		// (set) Token: 0x060003D5 RID: 981 RVA: 0x000237C0 File Offset: 0x000219C0
		[Serialize(false, IsPropertySaveable.Yes, "When enabled the attack will not be launched when there's a friendly character in the way. Only affects ranged attacks.", "", false)]
		[Editable]
		public bool AvoidFriendlyFire { get; set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x000237C9 File Offset: 0x000219C9
		// (set) Token: 0x060003D7 RID: 983 RVA: 0x000237D1 File Offset: 0x000219D1
		[Serialize(20f, IsPropertySaveable.Yes, "Used by enemy AI to determine how accurately the attack needs to be aimed for the attack to trigger. Only affects ranged attacks.", "", false)]
		[Editable]
		public float RequiredAngle { get; set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060003D8 RID: 984 RVA: 0x000237DA File Offset: 0x000219DA
		// (set) Token: 0x060003D9 RID: 985 RVA: 0x000237E2 File Offset: 0x000219E2
		[Serialize(0f, IsPropertySaveable.Yes, "By default uses the same value as RequiredAngle. Use if you want to allow selecting the attack but not shooting until the angle is smaller. Only affects ranged attacks.", "", false)]
		[Editable]
		public float RequiredAngleToShoot { get; set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060003DA RID: 986 RVA: 0x000237EB File Offset: 0x000219EB
		// (set) Token: 0x060003DB RID: 987 RVA: 0x000237F3 File Offset: 0x000219F3
		[Serialize(0f, IsPropertySaveable.Yes, "How much the attack limb is rotated towards the target. Default 0 = no rotation. Only affects ranged attacks.", "", false)]
		[Editable]
		public float AimRotationTorque { get; set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060003DC RID: 988 RVA: 0x000237FC File Offset: 0x000219FC
		// (set) Token: 0x060003DD RID: 989 RVA: 0x00023804 File Offset: 0x00021A04
		[Serialize(-1, IsPropertySaveable.Yes, "Reference to the limb we apply the aim rotation to. By default same as the attack limb. Only affects ranged attacks.", "", false)]
		[Editable]
		public int RotationLimbIndex { get; set; }

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060003DE RID: 990 RVA: 0x0002380D File Offset: 0x00021A0D
		// (set) Token: 0x060003DF RID: 991 RVA: 0x00023815 File Offset: 0x00021A15
		[Serialize(0f, IsPropertySaveable.Yes, "How much the held weapon is swayed back and forth while aiming. Only affects monsters using ranged weapons (items). Default 0 means the weapon is not swayed at all.", "", false)]
		[Editable]
		public float SwayAmount { get; set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060003E0 RID: 992 RVA: 0x0002381E File Offset: 0x00021A1E
		// (set) Token: 0x060003E1 RID: 993 RVA: 0x00023826 File Offset: 0x00021A26
		[Serialize(5f, IsPropertySaveable.Yes, "How fast the held weapon is swayed back and forth while aiming. Only affects monsters using ranged weapons (items).", "", false)]
		[Editable]
		public float SwayFrequency { get; set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060003E2 RID: 994 RVA: 0x0002382F File Offset: 0x00021A2F
		// (set) Token: 0x060003E3 RID: 995 RVA: 0x00023837 File Offset: 0x00021A37
		[Serialize(0f, IsPropertySaveable.No, "Legacy functionality. Behaves otherwise the same as stuns defined as afflictions, but explosions only apply the stun once instead of dividing it between the limbs.", "", false)]
		public float Stun { get; set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060003E4 RID: 996 RVA: 0x00023840 File Offset: 0x00021A40
		// (set) Token: 0x060003E5 RID: 997 RVA: 0x00023848 File Offset: 0x00021A48
		[Serialize(false, IsPropertySaveable.Yes, "Can damage only Humans.", "", false)]
		[Editable]
		public bool OnlyHumans { get; set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x00023851 File Offset: 0x00021A51
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x00023864 File Offset: 0x00021A64
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

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060003E8 RID: 1000 RVA: 0x000238BC File Offset: 0x00021ABC
		// (set) Token: 0x060003E9 RID: 1001 RVA: 0x000238C4 File Offset: 0x00021AC4
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the attacking limb (or limbs defined using ApplyForceOnLimbs). The direction of the force is towards the target that's being attacked.", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float Force { get; private set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060003EA RID: 1002 RVA: 0x000238CD File Offset: 0x00021ACD
		// (set) Token: 0x060003EB RID: 1003 RVA: 0x000238D5 File Offset: 0x00021AD5
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldStart { get; private set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060003EC RID: 1004 RVA: 0x000238DE File Offset: 0x00021ADE
		// (set) Token: 0x060003ED RID: 1005 RVA: 0x000238E6 File Offset: 0x00021AE6
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldMiddle { get; private set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060003EE RID: 1006 RVA: 0x000238EF File Offset: 0x00021AEF
		// (set) Token: 0x060003EF RID: 1007 RVA: 0x000238F7 File Offset: 0x00021AF7
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the main limb. In world space coordinates(i.e. 0, 1 pushes the character upwards a bit). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 RootForceWorldEnd { get; private set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x00023900 File Offset: 0x00021B00
		public bool HasRootForce
		{
			get
			{
				return this.RootForceWorldStart != Vector2.Zero || this.RootForceWorldMiddle != Vector2.Zero || this.RootForceWorldEnd != Vector2.Zero;
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060003F1 RID: 1009 RVA: 0x00023938 File Offset: 0x00021B38
		// (set) Token: 0x060003F2 RID: 1010 RVA: 0x00023940 File Offset: 0x00021B40
		[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "Applied to the main limb. The transition smoothing of the applied force.", "", false)]
		[Editable]
		public TransitionMode RootTransitionEasing { get; private set; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x00023949 File Offset: 0x00021B49
		// (set) Token: 0x060003F4 RID: 1012 RVA: 0x00023951 File Offset: 0x00021B51
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the attacking limb (or limbs defined using ApplyForceOnLimbs)", "", false)]
		[Editable(MinValueFloat = -10000f, MaxValueFloat = 10000f)]
		public float Torque { get; private set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x0002395A File Offset: 0x00021B5A
		// (set) Token: 0x060003F6 RID: 1014 RVA: 0x00023962 File Offset: 0x00021B62
		[Serialize(false, IsPropertySaveable.Yes, "Only apply the force once during the attacks lifetime.", "", false)]
		[Editable]
		public bool ApplyForcesOnlyOnce { get; private set; }

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x0002396B File Offset: 0x00021B6B
		// (set) Token: 0x060003F8 RID: 1016 RVA: 0x00023973 File Offset: 0x00021B73
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the target the attack hits. The direction of the impulse is from this limb towards the target (use negative values to pull the target closer).", "", false)]
		[Editable(MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public float TargetImpulse { get; private set; }

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x0002397C File Offset: 0x00021B7C
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x00023984 File Offset: 0x00021B84
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the target, in world space coordinates(i.e. 0, -1 pushes the target downwards). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 TargetImpulseWorld { get; private set; }

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x0002398D File Offset: 0x00021B8D
		// (set) Token: 0x060003FC RID: 1020 RVA: 0x00023995 File Offset: 0x00021B95
		[Serialize(0f, IsPropertySaveable.Yes, "Applied to the target the attack hits. The direction of the force is from this limb towards the target (use negative values to pull the target closer).", "", false)]
		[Editable(-1000f, 1000f, 1)]
		public float TargetForce { get; private set; }

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060003FD RID: 1021 RVA: 0x0002399E File Offset: 0x00021B9E
		// (set) Token: 0x060003FE RID: 1022 RVA: 0x000239A6 File Offset: 0x00021BA6
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Applied to the target, in world space coordinates(i.e. 0, -1 pushes the target downwards). The attacker's facing direction is taken into account.", "", false)]
		[Editable]
		public Vector2 TargetForceWorld { get; private set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x000239AF File Offset: 0x00021BAF
		// (set) Token: 0x06000400 RID: 1024 RVA: 0x000239B7 File Offset: 0x00021BB7
		[Serialize(1f, IsPropertySaveable.Yes, "Affects the strength of the impact effects the limb causes when it hits a submarine.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float SubmarineImpactMultiplier { get; private set; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000401 RID: 1025 RVA: 0x000239C0 File Offset: 0x00021BC0
		// (set) Token: 0x06000402 RID: 1026 RVA: 0x000239C8 File Offset: 0x00021BC8
		[Serialize(0f, IsPropertySaveable.Yes, "How likely the attack causes target limbs to be severed.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SeverLimbsProbability { get; set; }

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x000239D1 File Offset: 0x00021BD1
		public float StickChance
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x000239D8 File Offset: 0x00021BD8
		// (set) Token: 0x06000405 RID: 1029 RVA: 0x000239E0 File Offset: 0x00021BE0
		[Serialize(0f, IsPropertySaveable.Yes, "Used by enemy AI to determine the priority when selecting attacks. When random attacks are disabled on the character it is multiplied with distance to determine the which attack to use. Only attacks that are currently valid are taken into consideration when making the decision.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float Priority { get; private set; }

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000406 RID: 1030 RVA: 0x000239E9 File Offset: 0x00021BE9
		// (set) Token: 0x06000407 RID: 1031 RVA: 0x000239F1 File Offset: 0x00021BF1
		[Serialize(false, IsPropertySaveable.Yes, "Triggers the 'blink' animation on the attacking limbs when the attack executes. Used e.g. by abyss monsters to make their jaws close when attacking.", "", false)]
		[Editable]
		public bool Blink { get; private set; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000408 RID: 1032 RVA: 0x000239FA File Offset: 0x00021BFA
		public IEnumerable<StatusEffect> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x00023A02 File Offset: 0x00021C02
		public string Name
		{
			get
			{
				return "Attack";
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x0600040A RID: 1034 RVA: 0x00023A09 File Offset: 0x00021C09
		// (set) Token: 0x0600040B RID: 1035 RVA: 0x00023A11 File Offset: 0x00021C11
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; } = new Dictionary<Identifier, SerializableProperty>();

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600040C RID: 1036 RVA: 0x00023A1A File Offset: 0x00021C1A
		// (set) Token: 0x0600040D RID: 1037 RVA: 0x00023A22 File Offset: 0x00021C22
		public List<PropertyConditional> Conditionals { get; private set; } = new List<PropertyConditional>();

		// Token: 0x0600040E RID: 1038 RVA: 0x00023A2C File Offset: 0x00021C2C
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

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x00023A88 File Offset: 0x00021C88
		// (set) Token: 0x06000410 RID: 1040 RVA: 0x00023A90 File Offset: 0x00021C90
		public Item SourceItem { get; set; }

		// Token: 0x06000411 RID: 1041 RVA: 0x00023A9C File Offset: 0x00021C9C
		public List<Affliction> GetMultipliedAfflictions(float multiplier)
		{
			List<Affliction> multipliedAfflictions = new List<Affliction>();
			foreach (Affliction affliction in this.Afflictions.Keys)
			{
				multipliedAfflictions.Add(affliction.CreateMultiplied(multiplier, affliction));
			}
			return multipliedAfflictions;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00023B04 File Offset: 0x00021D04
		public float GetStructureDamage(float deltaTime)
		{
			if (this.Duration != 0f)
			{
				return this.StructureDamage * deltaTime;
			}
			return this.StructureDamage;
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00023B22 File Offset: 0x00021D22
		public float GetLevelWallDamage(float deltaTime)
		{
			if (this.Duration != 0f)
			{
				return this.LevelWallDamage * deltaTime;
			}
			return this.LevelWallDamage;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00023B40 File Offset: 0x00021D40
		public float GetItemDamage(float deltaTime, float multiplier = 1f)
		{
			float dmg = this.ItemDamage * multiplier;
			if (this.Duration != 0f)
			{
				return dmg * deltaTime;
			}
			return dmg;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00023B68 File Offset: 0x00021D68
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

		// Token: 0x06000416 RID: 1046 RVA: 0x00023BE0 File Offset: 0x00021DE0
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

		// Token: 0x06000417 RID: 1047 RVA: 0x00023CEA File Offset: 0x00021EEA
		public Attack(ContentXElement element, string parentDebugName, Item sourceItem) : this(element, parentDebugName)
		{
			this.SourceItem = sourceItem;
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x00023CFC File Offset: 0x00021EFC
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
			this.InitProjSpecific(element);
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

		// Token: 0x06000419 RID: 1049 RVA: 0x00023FF0 File Offset: 0x000221F0
		private void InitProjSpecific(ContentXElement element)
		{
			if (element.GetAttribute("sound") != null)
			{
				DebugConsole.ThrowError("Error in attack (" + ((element != null) ? element.ToString() : null) + ") - sounds should be defined as child elements, not as attributes.", null, null, false, false);
				return;
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "particleemitter"))
				{
					if (a == "sound")
					{
						this.sound = RoundSound.Load(subElement);
					}
				}
				else
				{
					this.particleEmitter = new ParticleEmitter(subElement);
				}
			}
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x000240B0 File Offset: 0x000222B0
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

		// Token: 0x0600041B RID: 1051 RVA: 0x000241B0 File Offset: 0x000223B0
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

		// Token: 0x0600041C RID: 1052 RVA: 0x00024228 File Offset: 0x00022428
		public void Deserialize(ContentXElement element, string parentDebugName)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ReloadAfflictions(element, parentDebugName);
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00024244 File Offset: 0x00022444
		public AttackResult DoDamage(Character attacker, IDamageable target, Vector2 worldPosition, float deltaTime, bool playSound = true, PhysicsBody sourceBody = null, Limb sourceLimb = null)
		{
			Character targetCharacter = target as Character;
			if (this.OnlyHumans && targetCharacter != null && !targetCharacter.IsHuman)
			{
				return default(AttackResult);
			}
			this.SetUser(attacker);
			this.DamageParticles(deltaTime, worldPosition);
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

		// Token: 0x0600041E RID: 1054 RVA: 0x00024594 File Offset: 0x00022794
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
			this.DamageParticles(deltaTime, worldPosition);
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

		// Token: 0x0600041F RID: 1055 RVA: 0x00024940 File Offset: 0x00022B40
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

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000420 RID: 1056 RVA: 0x000249F3 File Offset: 0x00022BF3
		// (set) Token: 0x06000421 RID: 1057 RVA: 0x000249FB File Offset: 0x00022BFB
		public float AttackTimer { get; private set; }

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x00024A04 File Offset: 0x00022C04
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x00024A0C File Offset: 0x00022C0C
		public float CoolDownTimer { get; set; }

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x00024A15 File Offset: 0x00022C15
		// (set) Token: 0x06000425 RID: 1061 RVA: 0x00024A1D File Offset: 0x00022C1D
		public float CurrentRandomCoolDown { get; private set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000426 RID: 1062 RVA: 0x00024A26 File Offset: 0x00022C26
		// (set) Token: 0x06000427 RID: 1063 RVA: 0x00024A2E File Offset: 0x00022C2E
		public float SecondaryCoolDownTimer { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000428 RID: 1064 RVA: 0x00024A37 File Offset: 0x00022C37
		// (set) Token: 0x06000429 RID: 1065 RVA: 0x00024A3F File Offset: 0x00022C3F
		public bool IsRunning { get; private set; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x0600042A RID: 1066 RVA: 0x00024A48 File Offset: 0x00022C48
		// (set) Token: 0x0600042B RID: 1067 RVA: 0x00024A50 File Offset: 0x00022C50
		public float AfterAttackTimer { get; set; }

		// Token: 0x0600042C RID: 1068 RVA: 0x00024A5C File Offset: 0x00022C5C
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

		// Token: 0x0600042D RID: 1069 RVA: 0x00024AB5 File Offset: 0x00022CB5
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

		// Token: 0x0600042E RID: 1070 RVA: 0x00024AEF File Offset: 0x00022CEF
		public void ResetAttackTimer()
		{
			this.AfterAttackTimer = 0f;
			this.AttackTimer = 0f;
			this.IsRunning = false;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x00024B10 File Offset: 0x00022D10
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

		// Token: 0x06000430 RID: 1072 RVA: 0x00024BA2 File Offset: 0x00022DA2
		public void ResetCoolDown()
		{
			this.CoolDownTimer = 0f;
			this.SecondaryCoolDownTimer = 0f;
			this.CurrentRandomCoolDown = 0f;
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00024BC8 File Offset: 0x00022DC8
		private void DamageParticles(float deltaTime, Vector2 worldPosition)
		{
			ParticleEmitter particleEmitter = this.particleEmitter;
			if (particleEmitter != null)
			{
				particleEmitter.Emit(deltaTime, worldPosition, null, 0f, 0f, 1f, 1f, 1f, null, null, false, null);
			}
			if (this.sound != null)
			{
				SoundPlayer.PlaySound(this.sound, worldPosition, null, null);
			}
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00024C2D File Offset: 0x00022E2D
		public bool IsValidContext(AttackContext context)
		{
			return this.Context == context || this.Context == AttackContext.Any || this.Context == AttackContext.NotDefined;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00024C4C File Offset: 0x00022E4C
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

		// Token: 0x06000434 RID: 1076 RVA: 0x00024CE0 File Offset: 0x00022EE0
		public bool IsValidTarget(AttackTarget targetType)
		{
			return this.TargetType == AttackTarget.Any || this.TargetType.HasAnyFlag(targetType);
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00024CF8 File Offset: 0x00022EF8
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

		// Token: 0x06000436 RID: 1078 RVA: 0x00024D70 File Offset: 0x00022F70
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

		// Token: 0x06000437 RID: 1079 RVA: 0x00024DB4 File Offset: 0x00022FB4
		public Vector2 CalculateAttackPhase(TransitionMode easing = TransitionMode.Linear)
		{
			float t = this.AttackTimer / this.Duration;
			return MathUtils.Bezier(this.RootForceWorldStart, this.RootForceWorldMiddle, this.RootForceWorldEnd, ToolBox.GetEasing(easing, t));
		}

		// Token: 0x04000239 RID: 569
		private RoundSound sound;

		// Token: 0x0400023A RID: 570
		private ParticleEmitter particleEmitter;

		// Token: 0x04000246 RID: 582
		private float _range;

		// Token: 0x04000247 RID: 583
		private float _damageRange;

		// Token: 0x0400024E RID: 590
		private float _structureDamage;

		// Token: 0x04000251 RID: 593
		private float _itemDamage;

		// Token: 0x04000253 RID: 595
		private float? _damageMultiplier;

		// Token: 0x04000254 RID: 596
		private float initialDamageMultiplier = 1f;

		// Token: 0x04000257 RID: 599
		private float _levelWallDamage;

		// Token: 0x04000272 RID: 626
		public readonly List<int> ForceOnLimbIndices = new List<int>();

		// Token: 0x04000273 RID: 627
		public readonly Dictionary<Affliction, XElement> Afflictions = new Dictionary<Affliction, XElement>();

		// Token: 0x04000275 RID: 629
		private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();

		// Token: 0x04000277 RID: 631
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();
	}
}
