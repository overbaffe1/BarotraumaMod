using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001E7 RID: 487
	internal abstract class HumanSwimParams : SwimParams, IHumanAnimation
	{
		// Token: 0x17000D9B RID: 3483
		// (get) Token: 0x06003365 RID: 13157 RVA: 0x0020B3B3 File Offset: 0x002095B3
		// (set) Token: 0x06003366 RID: 13158 RVA: 0x0020B3BB File Offset: 0x002095BB
		[Header("Legs", null)]
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float LegMoveAmount { get; set; }

		// Token: 0x17000D9C RID: 3484
		// (get) Token: 0x06003367 RID: 13159 RVA: 0x0020B3C4 File Offset: 0x002095C4
		// (set) Token: 0x06003368 RID: 13160 RVA: 0x0020B3CC File Offset: 0x002095CC
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float LegCycleLength { get; set; }

		// Token: 0x17000D9D RID: 3485
		// (get) Token: 0x06003369 RID: 13161 RVA: 0x0020B3D5 File Offset: 0x002095D5
		// (set) Token: 0x0600336A RID: 13162 RVA: 0x0020B3E2 File Offset: 0x002095E2
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

		// Token: 0x17000D9E RID: 3486
		// (get) Token: 0x0600336B RID: 13163 RVA: 0x0020B3F0 File Offset: 0x002095F0
		// (set) Token: 0x0600336C RID: 13164 RVA: 0x0020B3F8 File Offset: 0x002095F8
		public float FootAngleInRadians { get; private set; }

		// Token: 0x17000D9F RID: 3487
		// (get) Token: 0x0600336D RID: 13165 RVA: 0x0020B401 File Offset: 0x00209601
		// (set) Token: 0x0600336E RID: 13166 RVA: 0x0020B409 File Offset: 0x00209609
		[Header("Arms", null)]
		[Serialize("0.5, 0.1", IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveAmount { get; set; }

		// Token: 0x17000DA0 RID: 3488
		// (get) Token: 0x0600336F RID: 13167 RVA: 0x0020B412 File Offset: 0x00209612
		// (set) Token: 0x06003370 RID: 13168 RVA: 0x0020B41A File Offset: 0x0020961A
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float HandCycleSpeed { get; set; }

		// Token: 0x17000DA1 RID: 3489
		// (get) Token: 0x06003371 RID: 13169 RVA: 0x0020B423 File Offset: 0x00209623
		// (set) Token: 0x06003372 RID: 13170 RVA: 0x0020B42B File Offset: 0x0020962B
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveOffset { get; set; }

		// Token: 0x17000DA2 RID: 3490
		// (get) Token: 0x06003373 RID: 13171 RVA: 0x0020B434 File Offset: 0x00209634
		// (set) Token: 0x06003374 RID: 13172 RVA: 0x0020B43C File Offset: 0x0020963C
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the arms.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 20f, DecimalCount = 2)]
		public float ArmMoveStrength { get; set; }

		// Token: 0x17000DA3 RID: 3491
		// (get) Token: 0x06003375 RID: 13173 RVA: 0x0020B445 File Offset: 0x00209645
		// (set) Token: 0x06003376 RID: 13174 RVA: 0x0020B44D File Offset: 0x0020964D
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the hands.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandMoveStrength { get; set; }

		// Token: 0x17000DA4 RID: 3492
		// (get) Token: 0x06003377 RID: 13175 RVA: 0x0020B456 File Offset: 0x00209656
		// (set) Token: 0x06003378 RID: 13176 RVA: 0x0020B45E File Offset: 0x0020965E
		[Header("Other", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Is the head angle fixed or does the angle follow the mouse position?", "", false)]
		[Editable]
		public bool FixedHeadAngle { get; set; }
	}
}
