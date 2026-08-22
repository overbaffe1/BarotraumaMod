using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000E4 RID: 228
	internal abstract class FishSwimParams : SwimParams, IFishAnimation
	{
		// Token: 0x17000719 RID: 1817
		// (get) Token: 0x0600181F RID: 6175 RVA: 0x000C497B File Offset: 0x000C2B7B
		// (set) Token: 0x06001820 RID: 6176 RVA: 0x000C4983 File Offset: 0x000C2B83
		[Serialize(false, IsPropertySaveable.Yes, "Instead of linear movement (default), use a wave-like movement. Note: WaveAmplitude and WaveLength don't have any effect on this. It's synced with the movement speed.", "", false)]
		[Editable]
		public bool UseSineMovement { get; set; }

		// Token: 0x1700071A RID: 1818
		// (get) Token: 0x06001821 RID: 6177 RVA: 0x000C498C File Offset: 0x000C2B8C
		// (set) Token: 0x06001822 RID: 6178 RVA: 0x000C4994 File Offset: 0x000C2B94
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character be flipped depending on which direction it faces. Should usually be enabled on all characters that have distinctive upper and lower sides.", "", false)]
		public bool Flip { get; set; }

		// Token: 0x1700071B RID: 1819
		// (get) Token: 0x06001823 RID: 6179 RVA: 0x000C499D File Offset: 0x000C2B9D
		// (set) Token: 0x06001824 RID: 6180 RVA: 0x000C49A5 File Offset: 0x000C2BA5
		[Serialize(1f, IsPropertySaveable.Yes, "Reduces continuous flipping when the character abruptly changes direction.", "", false)]
		[Editable]
		public float FlipCooldown { get; set; }

		// Token: 0x1700071C RID: 1820
		// (get) Token: 0x06001825 RID: 6181 RVA: 0x000C49AE File Offset: 0x000C2BAE
		// (set) Token: 0x06001826 RID: 6182 RVA: 0x000C49B6 File Offset: 0x000C2BB6
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much it takes before the character flips. The timer starts when the character starts to move in the different direction.", "", false)]
		[Editable]
		public float FlipDelay { get; set; }

		// Token: 0x1700071D RID: 1821
		// (get) Token: 0x06001827 RID: 6183 RVA: 0x000C49BF File Offset: 0x000C2BBF
		// (set) Token: 0x06001828 RID: 6184 RVA: 0x000C49C7 File Offset: 0x000C2BC7
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "If enabled, the character will simply be mirrored horizontally when it wants to turn around. If disabled, it will rotate itself to face the other direction.", "", false)]
		public bool Mirror { get; set; }

		// Token: 0x1700071E RID: 1822
		// (get) Token: 0x06001829 RID: 6185 RVA: 0x000C49D0 File Offset: 0x000C2BD0
		// (set) Token: 0x0600182A RID: 6186 RVA: 0x000C49D8 File Offset: 0x000C2BD8
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Disabling this will make mirroring instantaneous.", "", false)]
		public bool MirrorLerp { get; set; }

		// Token: 0x1700071F RID: 1823
		// (get) Token: 0x0600182B RID: 6187 RVA: 0x000C49E1 File Offset: 0x000C2BE1
		// (set) Token: 0x0600182C RID: 6188 RVA: 0x000C49E9 File Offset: 0x000C2BE9
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaveAmplitude { get; set; }

		// Token: 0x17000720 RID: 1824
		// (get) Token: 0x0600182D RID: 6189 RVA: 0x000C49F2 File Offset: 0x000C2BF2
		// (set) Token: 0x0600182E RID: 6190 RVA: 0x000C49FA File Offset: 0x000C2BFA
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaveLength { get; set; }

		// Token: 0x17000721 RID: 1825
		// (get) Token: 0x0600182F RID: 6191 RVA: 0x000C4A03 File Offset: 0x000C2C03
		// (set) Token: 0x06001830 RID: 6192 RVA: 0x000C4A0B File Offset: 0x000C2C0B
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character face towards the direction it's heading.", "", false)]
		public bool RotateTowardsMovement { get; set; }

		// Token: 0x17000722 RID: 1826
		// (get) Token: 0x06001831 RID: 6193 RVA: 0x000C4A14 File Offset: 0x000C2C14
		// (set) Token: 0x06001832 RID: 6194 RVA: 0x000C4A1C File Offset: 0x000C2C1C
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the tail to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2000f, ValueStep = 1f)]
		public float TailTorque { get; set; }

		// Token: 0x17000723 RID: 1827
		// (get) Token: 0x06001833 RID: 6195 RVA: 0x000C4A25 File Offset: 0x000C2C25
		// (set) Token: 0x06001834 RID: 6196 RVA: 0x000C4A2D File Offset: 0x000C2C2D
		[Serialize(1f, IsPropertySaveable.Yes, "Multiplier applied based on the angle difference between the tail and the main limb. Increasing the value prevents snake-like characters from getting tangled on themselves. Default = 1 (no boost)", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 100f)]
		public float TailTorqueMultiplier { get; set; }

		// Token: 0x17000724 RID: 1828
		// (get) Token: 0x06001835 RID: 6197 RVA: 0x000C4A36 File Offset: 0x000C2C36
		// (set) Token: 0x06001836 RID: 6198 RVA: 0x000C4A43 File Offset: 0x000C2C43
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

		// Token: 0x17000725 RID: 1829
		// (get) Token: 0x06001837 RID: 6199 RVA: 0x000C4A51 File Offset: 0x000C2C51
		// (set) Token: 0x06001838 RID: 6200 RVA: 0x000C4A59 File Offset: 0x000C2C59
		[Serialize(false, IsPropertySaveable.Yes, "Should the animation be updated even if the character is not moving?", "", false)]
		[Editable]
		public bool UpdateAnimationWhenNotMoving { get; set; }

		// Token: 0x17000726 RID: 1830
		// (get) Token: 0x06001839 RID: 6201 RVA: 0x000C4A62 File Offset: 0x000C2C62
		// (set) Token: 0x0600183A RID: 6202 RVA: 0x000C4A6A File Offset: 0x000C2C6A
		public Dictionary<int, float> FootAnglesInRadians { get; set; } = new Dictionary<int, float>();

		// Token: 0x17000727 RID: 1831
		// (get) Token: 0x0600183B RID: 6203 RVA: 0x000C4A73 File Offset: 0x000C2C73
		// (set) Token: 0x0600183C RID: 6204 RVA: 0x000C4A93 File Offset: 0x000C2C93
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

		// Token: 0x17000728 RID: 1832
		// (get) Token: 0x0600183D RID: 6205 RVA: 0x000C4AA9 File Offset: 0x000C2CA9
		// (set) Token: 0x0600183E RID: 6206 RVA: 0x000C4AB1 File Offset: 0x000C2CB1
		public float TailAngleInRadians { get; private set; } = float.NaN;
	}
}
