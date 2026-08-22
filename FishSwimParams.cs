using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001E0 RID: 480
	internal abstract class FishSwimParams : SwimParams, IFishAnimation
	{
		// Token: 0x17000D80 RID: 3456
		// (get) Token: 0x0600331B RID: 13083 RVA: 0x0020B16F File Offset: 0x0020936F
		// (set) Token: 0x0600331C RID: 13084 RVA: 0x0020B177 File Offset: 0x00209377
		[Serialize(false, IsPropertySaveable.Yes, "Instead of linear movement (default), use a wave-like movement. Note: WaveAmplitude and WaveLength don't have any effect on this. It's synced with the movement speed.", "", false)]
		[Editable]
		public bool UseSineMovement { get; set; }

		// Token: 0x17000D81 RID: 3457
		// (get) Token: 0x0600331D RID: 13085 RVA: 0x0020B180 File Offset: 0x00209380
		// (set) Token: 0x0600331E RID: 13086 RVA: 0x0020B188 File Offset: 0x00209388
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character be flipped depending on which direction it faces. Should usually be enabled on all characters that have distinctive upper and lower sides.", "", false)]
		public bool Flip { get; set; }

		// Token: 0x17000D82 RID: 3458
		// (get) Token: 0x0600331F RID: 13087 RVA: 0x0020B191 File Offset: 0x00209391
		// (set) Token: 0x06003320 RID: 13088 RVA: 0x0020B199 File Offset: 0x00209399
		[Serialize(1f, IsPropertySaveable.Yes, "Reduces continuous flipping when the character abruptly changes direction.", "", false)]
		[Editable]
		public float FlipCooldown { get; set; }

		// Token: 0x17000D83 RID: 3459
		// (get) Token: 0x06003321 RID: 13089 RVA: 0x0020B1A2 File Offset: 0x002093A2
		// (set) Token: 0x06003322 RID: 13090 RVA: 0x0020B1AA File Offset: 0x002093AA
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much it takes before the character flips. The timer starts when the character starts to move in the different direction.", "", false)]
		[Editable]
		public float FlipDelay { get; set; }

		// Token: 0x17000D84 RID: 3460
		// (get) Token: 0x06003323 RID: 13091 RVA: 0x0020B1B3 File Offset: 0x002093B3
		// (set) Token: 0x06003324 RID: 13092 RVA: 0x0020B1BB File Offset: 0x002093BB
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If enabled, the character will simply be mirrored horizontally when it wants to turn around. If disabled, it will rotate itself to face the other direction.", "", false)]
		public bool Mirror { get; set; }

		// Token: 0x17000D85 RID: 3461
		// (get) Token: 0x06003325 RID: 13093 RVA: 0x0020B1C4 File Offset: 0x002093C4
		// (set) Token: 0x06003326 RID: 13094 RVA: 0x0020B1CC File Offset: 0x002093CC
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Disabling this will make mirroring instantaneous.", "", false)]
		public bool MirrorLerp { get; set; }

		// Token: 0x17000D86 RID: 3462
		// (get) Token: 0x06003327 RID: 13095 RVA: 0x0020B1D5 File Offset: 0x002093D5
		// (set) Token: 0x06003328 RID: 13096 RVA: 0x0020B1DD File Offset: 0x002093DD
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaveAmplitude { get; set; }

		// Token: 0x17000D87 RID: 3463
		// (get) Token: 0x06003329 RID: 13097 RVA: 0x0020B1E6 File Offset: 0x002093E6
		// (set) Token: 0x0600332A RID: 13098 RVA: 0x0020B1EE File Offset: 0x002093EE
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaveLength { get; set; }

		// Token: 0x17000D88 RID: 3464
		// (get) Token: 0x0600332B RID: 13099 RVA: 0x0020B1F7 File Offset: 0x002093F7
		// (set) Token: 0x0600332C RID: 13100 RVA: 0x0020B1FF File Offset: 0x002093FF
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character face towards the direction it's heading.", "", false)]
		public bool RotateTowardsMovement { get; set; }

		// Token: 0x17000D89 RID: 3465
		// (get) Token: 0x0600332D RID: 13101 RVA: 0x0020B208 File Offset: 0x00209408
		// (set) Token: 0x0600332E RID: 13102 RVA: 0x0020B210 File Offset: 0x00209410
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the tail to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2000f, ValueStep = 1f)]
		public float TailTorque { get; set; }

		// Token: 0x17000D8A RID: 3466
		// (get) Token: 0x0600332F RID: 13103 RVA: 0x0020B219 File Offset: 0x00209419
		// (set) Token: 0x06003330 RID: 13104 RVA: 0x0020B221 File Offset: 0x00209421
		[Serialize(1f, IsPropertySaveable.Yes, "Multiplier applied based on the angle difference between the tail and the main limb. Increasing the value prevents snake-like characters from getting tangled on themselves. Default = 1 (no boost)", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 100f)]
		public float TailTorqueMultiplier { get; set; }

		// Token: 0x17000D8B RID: 3467
		// (get) Token: 0x06003331 RID: 13105 RVA: 0x0020B22A File Offset: 0x0020942A
		// (set) Token: 0x06003332 RID: 13106 RVA: 0x0020B237 File Offset: 0x00209437
		[Serialize(null, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string FootAngles
		{
			get
			{
				return AnimationParams.ParseFootAngles(this.FootAnglesInRadians);
			}
			set
			{
				AnimationParams.SetFootAngles(this.FootAnglesInRadians, value);
			}
		}

		// Token: 0x17000D8C RID: 3468
		// (get) Token: 0x06003333 RID: 13107 RVA: 0x0020B245 File Offset: 0x00209445
		// (set) Token: 0x06003334 RID: 13108 RVA: 0x0020B24D File Offset: 0x0020944D
		[Serialize(false, IsPropertySaveable.Yes, "Should the animation be updated even if the character is not moving?", "", false)]
		[Editable]
		public bool UpdateAnimationWhenNotMoving { get; set; }

		// Token: 0x17000D8D RID: 3469
		// (get) Token: 0x06003335 RID: 13109 RVA: 0x0020B256 File Offset: 0x00209456
		// (set) Token: 0x06003336 RID: 13110 RVA: 0x0020B25E File Offset: 0x0020945E
		public Dictionary<int, float> FootAnglesInRadians { get; set; } = new Dictionary<int, float>();

		// Token: 0x17000D8E RID: 3470
		// (get) Token: 0x06003337 RID: 13111 RVA: 0x0020B267 File Offset: 0x00209467
		// (set) Token: 0x06003338 RID: 13112 RVA: 0x0020B287 File Offset: 0x00209487
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float TailAngle
		{
			get
			{
				if (!float.IsNaN(this.TailAngleInRadians))
				{
					return MathHelper.ToDegrees(this.TailAngleInRadians);
				}
				return float.NaN;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this.TailAngleInRadians = MathHelper.ToRadians(value);
				}
			}
		}

		// Token: 0x17000D8F RID: 3471
		// (get) Token: 0x06003339 RID: 13113 RVA: 0x0020B29D File Offset: 0x0020949D
		// (set) Token: 0x0600333A RID: 13114 RVA: 0x0020B2A5 File Offset: 0x002094A5
		public float TailAngleInRadians { get; private set; } = float.NaN;
	}
}
