using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001E8 RID: 488
	internal abstract class HumanGroundedParams : GroundedMovementParams, IHumanAnimation
	{
		// Token: 0x17000DA5 RID: 3493
		// (get) Token: 0x0600337A RID: 13178 RVA: 0x0020B46F File Offset: 0x0020966F
		// (set) Token: 0x0600337B RID: 13179 RVA: 0x0020B477 File Offset: 0x00209677
		[Header("Standing", null)]
		[Serialize(0.3f, IsPropertySaveable.Yes, "How much force is used to force the character upright.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float GetUpForce { get; set; }

		// Token: 0x17000DA6 RID: 3494
		// (get) Token: 0x0600337C RID: 13180 RVA: 0x0020B480 File Offset: 0x00209680
		// (set) Token: 0x0600337D RID: 13181 RVA: 0x0020B488 File Offset: 0x00209688
		[Serialize(0.25f, IsPropertySaveable.Yes, "How much the character's head leans forwards when moving.", "", false)]
		[Editable(DecimalCount = 2)]
		public float HeadLeanAmount { get; set; }

		// Token: 0x17000DA7 RID: 3495
		// (get) Token: 0x0600337E RID: 13182 RVA: 0x0020B491 File Offset: 0x00209691
		// (set) Token: 0x0600337F RID: 13183 RVA: 0x0020B499 File Offset: 0x00209699
		[Serialize(0.25f, IsPropertySaveable.Yes, "How much the character's torso leans forwards when moving.", "", false)]
		[Editable(DecimalCount = 2)]
		public float TorsoLeanAmount { get; set; }

		// Token: 0x17000DA8 RID: 3496
		// (get) Token: 0x06003380 RID: 13184 RVA: 0x0020B4A2 File Offset: 0x002096A2
		// (set) Token: 0x06003381 RID: 13185 RVA: 0x0020B4AA File Offset: 0x002096AA
		[Header("Legs", null)]
		[Serialize(15f, IsPropertySaveable.Yes, "How much force is used to move the feet to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FootMoveStrength { get; set; }

		// Token: 0x17000DA9 RID: 3497
		// (get) Token: 0x06003382 RID: 13186 RVA: 0x0020B4B3 File Offset: 0x002096B3
		// (set) Token: 0x06003383 RID: 13187 RVA: 0x0020B4BB File Offset: 0x002096BB
		[Serialize(0f, IsPropertySaveable.Yes, "How much the horizontal difference of waist and the foot positions has an effect to lifting the foot.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float FootLiftHorizontalFactor { get; set; }

		// Token: 0x17000DAA RID: 3498
		// (get) Token: 0x06003384 RID: 13188 RVA: 0x0020B4C4 File Offset: 0x002096C4
		// (set) Token: 0x06003385 RID: 13189 RVA: 0x0020B4CC File Offset: 0x002096CC
		[Serialize("0,0", IsPropertySaveable.Yes, "Normally the character's feet are positioned at a scaled-down version of it's normal step position - this can be used to override that value if you want to e.g. make the character to spread out it's feet more when standing.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.01f)]
		public Vector2 StepSizeWhenStanding { get; set; }

		// Token: 0x17000DAB RID: 3499
		// (get) Token: 0x06003386 RID: 13190 RVA: 0x0020B4D5 File Offset: 0x002096D5
		// (set) Token: 0x06003387 RID: 13191 RVA: 0x0020B4E2 File Offset: 0x002096E2
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

		// Token: 0x17000DAC RID: 3500
		// (get) Token: 0x06003388 RID: 13192 RVA: 0x0020B4F0 File Offset: 0x002096F0
		// (set) Token: 0x06003389 RID: 13193 RVA: 0x0020B4F8 File Offset: 0x002096F8
		public float FootAngleInRadians { get; private set; }

		// Token: 0x17000DAD RID: 3501
		// (get) Token: 0x0600338A RID: 13194 RVA: 0x0020B501 File Offset: 0x00209701
		// (set) Token: 0x0600338B RID: 13195 RVA: 0x0020B509 File Offset: 0x00209709
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Added to the calculated foot positions, e.g. a value of {-1.0, 0.0f} would make the character \"drag\" their feet one unit behind them.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 FootMoveOffset { get; set; }

		// Token: 0x17000DAE RID: 3502
		// (get) Token: 0x0600338C RID: 13196 RVA: 0x0020B512 File Offset: 0x00209712
		// (set) Token: 0x0600338D RID: 13197 RVA: 0x0020B51A File Offset: 0x0020971A
		[Serialize(10f, IsPropertySaveable.Yes, "How much torque is used to bend the characters legs when taking a step.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float LegBendTorque { get; set; }

		// Token: 0x17000DAF RID: 3503
		// (get) Token: 0x0600338E RID: 13198 RVA: 0x0020B523 File Offset: 0x00209723
		// (set) Token: 0x0600338F RID: 13199 RVA: 0x0020B52B File Offset: 0x0020972B
		[Header("Arms", null)]
		[Serialize("0.4, 0.15", IsPropertySaveable.Yes, "How much the hands move along each axis.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveAmount { get; set; }

		// Token: 0x17000DB0 RID: 3504
		// (get) Token: 0x06003390 RID: 13200 RVA: 0x0020B534 File Offset: 0x00209734
		// (set) Token: 0x06003391 RID: 13201 RVA: 0x0020B53C File Offset: 0x0020973C
		[Serialize("-0.15, 0.0", IsPropertySaveable.Yes, "Added to the calculated hand positions, e.g. a value of {-1.0, 0.0f} would make the character \"drag\" their hands one unit behind them.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveOffset { get; set; }

		// Token: 0x17000DB1 RID: 3505
		// (get) Token: 0x06003392 RID: 13202 RVA: 0x0020B545 File Offset: 0x00209745
		// (set) Token: 0x06003393 RID: 13203 RVA: 0x0020B54D File Offset: 0x0020974D
		[Serialize(-1f, IsPropertySaveable.Yes, "The position of the hands is clamped below this (relative to the position of the character's torso).", "", false)]
		[Editable(DecimalCount = 2)]
		public float HandClampY { get; set; }

		// Token: 0x17000DB2 RID: 3506
		// (get) Token: 0x06003394 RID: 13204 RVA: 0x0020B556 File Offset: 0x00209756
		// (set) Token: 0x06003395 RID: 13205 RVA: 0x0020B55E File Offset: 0x0020975E
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the arms.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ArmMoveStrength { get; set; }

		// Token: 0x17000DB3 RID: 3507
		// (get) Token: 0x06003396 RID: 13206 RVA: 0x0020B567 File Offset: 0x00209767
		// (set) Token: 0x06003397 RID: 13207 RVA: 0x0020B56F File Offset: 0x0020976F
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the hands.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandMoveStrength { get; set; }

		// Token: 0x17000DB4 RID: 3508
		// (get) Token: 0x06003398 RID: 13208 RVA: 0x0020B578 File Offset: 0x00209778
		// (set) Token: 0x06003399 RID: 13209 RVA: 0x0020B580 File Offset: 0x00209780
		[Header("Other", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Is the head angle fixed or does the angle follow the mouse position?", "", false)]
		[Editable]
		public bool FixedHeadAngle { get; set; }
	}
}
