using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000EC RID: 236
	internal abstract class HumanGroundedParams : GroundedMovementParams, IHumanAnimation
	{
		// Token: 0x1700073E RID: 1854
		// (get) Token: 0x0600187E RID: 6270 RVA: 0x000C4C7B File Offset: 0x000C2E7B
		// (set) Token: 0x0600187F RID: 6271 RVA: 0x000C4C83 File Offset: 0x000C2E83
		[Header("Standing", null)]
		[Serialize(0.3f, IsPropertySaveable.Yes, "How much force is used to force the character upright.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float GetUpForce { get; set; }

		// Token: 0x1700073F RID: 1855
		// (get) Token: 0x06001880 RID: 6272 RVA: 0x000C4C8C File Offset: 0x000C2E8C
		// (set) Token: 0x06001881 RID: 6273 RVA: 0x000C4C94 File Offset: 0x000C2E94
		[Serialize(0.25f, IsPropertySaveable.Yes, "How much the character's head leans forwards when moving.", "", false)]
		[Editable(DecimalCount = 2)]
		public float HeadLeanAmount { get; set; }

		// Token: 0x17000740 RID: 1856
		// (get) Token: 0x06001882 RID: 6274 RVA: 0x000C4C9D File Offset: 0x000C2E9D
		// (set) Token: 0x06001883 RID: 6275 RVA: 0x000C4CA5 File Offset: 0x000C2EA5
		[Serialize(0.25f, IsPropertySaveable.Yes, "How much the character's torso leans forwards when moving.", "", false)]
		[Editable(DecimalCount = 2)]
		public float TorsoLeanAmount { get; set; }

		// Token: 0x17000741 RID: 1857
		// (get) Token: 0x06001884 RID: 6276 RVA: 0x000C4CAE File Offset: 0x000C2EAE
		// (set) Token: 0x06001885 RID: 6277 RVA: 0x000C4CB6 File Offset: 0x000C2EB6
		[Header("Legs", null)]
		[Serialize(15f, IsPropertySaveable.Yes, "How much force is used to move the feet to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FootMoveStrength { get; set; }

		// Token: 0x17000742 RID: 1858
		// (get) Token: 0x06001886 RID: 6278 RVA: 0x000C4CBF File Offset: 0x000C2EBF
		// (set) Token: 0x06001887 RID: 6279 RVA: 0x000C4CC7 File Offset: 0x000C2EC7
		[Serialize(0f, IsPropertySaveable.Yes, "How much the horizontal difference of waist and the foot positions has an effect to lifting the foot.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float FootLiftHorizontalFactor { get; set; }

		// Token: 0x17000743 RID: 1859
		// (get) Token: 0x06001888 RID: 6280 RVA: 0x000C4CD0 File Offset: 0x000C2ED0
		// (set) Token: 0x06001889 RID: 6281 RVA: 0x000C4CD8 File Offset: 0x000C2ED8
		[Serialize("0,0", IsPropertySaveable.Yes, "Normally the character's feet are positioned at a scaled-down version of it's normal step position - this can be used to override that value if you want to e.g. make the character to spread out it's feet more when standing.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.01f)]
		public Vector2 StepSizeWhenStanding { get; set; }

		// Token: 0x17000744 RID: 1860
		// (get) Token: 0x0600188A RID: 6282 RVA: 0x000C4CE1 File Offset: 0x000C2EE1
		// (set) Token: 0x0600188B RID: 6283 RVA: 0x000C4CEE File Offset: 0x000C2EEE
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float FootAngle
		{
			get
			{
				return MathHelper.ToDegrees(this.FootAngleInRadians);
			}
			set
			{
				this.FootAngleInRadians = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000745 RID: 1861
		// (get) Token: 0x0600188C RID: 6284 RVA: 0x000C4CFC File Offset: 0x000C2EFC
		// (set) Token: 0x0600188D RID: 6285 RVA: 0x000C4D04 File Offset: 0x000C2F04
		public float FootAngleInRadians { get; private set; }

		// Token: 0x17000746 RID: 1862
		// (get) Token: 0x0600188E RID: 6286 RVA: 0x000C4D0D File Offset: 0x000C2F0D
		// (set) Token: 0x0600188F RID: 6287 RVA: 0x000C4D15 File Offset: 0x000C2F15
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Added to the calculated foot positions, e.g. a value of {-1.0, 0.0f} would make the character \"drag\" their feet one unit behind them.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 FootMoveOffset { get; set; }

		// Token: 0x17000747 RID: 1863
		// (get) Token: 0x06001890 RID: 6288 RVA: 0x000C4D1E File Offset: 0x000C2F1E
		// (set) Token: 0x06001891 RID: 6289 RVA: 0x000C4D26 File Offset: 0x000C2F26
		[Serialize(10f, IsPropertySaveable.Yes, "How much torque is used to bend the characters legs when taking a step.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float LegBendTorque { get; set; }

		// Token: 0x17000748 RID: 1864
		// (get) Token: 0x06001892 RID: 6290 RVA: 0x000C4D2F File Offset: 0x000C2F2F
		// (set) Token: 0x06001893 RID: 6291 RVA: 0x000C4D37 File Offset: 0x000C2F37
		[Header("Arms", null)]
		[Serialize("0.4, 0.15", IsPropertySaveable.Yes, "How much the hands move along each axis.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveAmount { get; set; }

		// Token: 0x17000749 RID: 1865
		// (get) Token: 0x06001894 RID: 6292 RVA: 0x000C4D40 File Offset: 0x000C2F40
		// (set) Token: 0x06001895 RID: 6293 RVA: 0x000C4D48 File Offset: 0x000C2F48
		[Serialize("-0.15, 0.0", IsPropertySaveable.Yes, "Added to the calculated hand positions, e.g. a value of {-1.0, 0.0f} would make the character \"drag\" their hands one unit behind them.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveOffset { get; set; }

		// Token: 0x1700074A RID: 1866
		// (get) Token: 0x06001896 RID: 6294 RVA: 0x000C4D51 File Offset: 0x000C2F51
		// (set) Token: 0x06001897 RID: 6295 RVA: 0x000C4D59 File Offset: 0x000C2F59
		[Serialize(-1f, IsPropertySaveable.Yes, "The position of the hands is clamped below this (relative to the position of the character's torso).", "", false)]
		[Editable(DecimalCount = 2)]
		public float HandClampY { get; set; }

		// Token: 0x1700074B RID: 1867
		// (get) Token: 0x06001898 RID: 6296 RVA: 0x000C4D62 File Offset: 0x000C2F62
		// (set) Token: 0x06001899 RID: 6297 RVA: 0x000C4D6A File Offset: 0x000C2F6A
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the arms.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ArmMoveStrength { get; set; }

		// Token: 0x1700074C RID: 1868
		// (get) Token: 0x0600189A RID: 6298 RVA: 0x000C4D73 File Offset: 0x000C2F73
		// (set) Token: 0x0600189B RID: 6299 RVA: 0x000C4D7B File Offset: 0x000C2F7B
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the hands.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandMoveStrength { get; set; }

		// Token: 0x1700074D RID: 1869
		// (get) Token: 0x0600189C RID: 6300 RVA: 0x000C4D84 File Offset: 0x000C2F84
		// (set) Token: 0x0600189D RID: 6301 RVA: 0x000C4D8C File Offset: 0x000C2F8C
		[Header("Other", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Is the head angle fixed or does the angle follow the mouse position?", "", false)]
		[Editable]
		public bool FixedHeadAngle { get; set; }
	}
}
