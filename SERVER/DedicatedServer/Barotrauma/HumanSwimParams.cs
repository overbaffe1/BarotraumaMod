using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000EB RID: 235
	internal abstract class HumanSwimParams : SwimParams, IHumanAnimation
	{
		// Token: 0x17000734 RID: 1844
		// (get) Token: 0x06001869 RID: 6249 RVA: 0x000C4BBF File Offset: 0x000C2DBF
		// (set) Token: 0x0600186A RID: 6250 RVA: 0x000C4BC7 File Offset: 0x000C2DC7
		[Header("Legs", null)]
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float LegMoveAmount { get; set; }

		// Token: 0x17000735 RID: 1845
		// (get) Token: 0x0600186B RID: 6251 RVA: 0x000C4BD0 File Offset: 0x000C2DD0
		// (set) Token: 0x0600186C RID: 6252 RVA: 0x000C4BD8 File Offset: 0x000C2DD8
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float LegCycleLength { get; set; }

		// Token: 0x17000736 RID: 1846
		// (get) Token: 0x0600186D RID: 6253 RVA: 0x000C4BE1 File Offset: 0x000C2DE1
		// (set) Token: 0x0600186E RID: 6254 RVA: 0x000C4BEE File Offset: 0x000C2DEE
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

		// Token: 0x17000737 RID: 1847
		// (get) Token: 0x0600186F RID: 6255 RVA: 0x000C4BFC File Offset: 0x000C2DFC
		// (set) Token: 0x06001870 RID: 6256 RVA: 0x000C4C04 File Offset: 0x000C2E04
		public float FootAngleInRadians { get; private set; }

		// Token: 0x17000738 RID: 1848
		// (get) Token: 0x06001871 RID: 6257 RVA: 0x000C4C0D File Offset: 0x000C2E0D
		// (set) Token: 0x06001872 RID: 6258 RVA: 0x000C4C15 File Offset: 0x000C2E15
		[Header("Arms", null)]
		[Serialize("0.5, 0.1", IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveAmount { get; set; }

		// Token: 0x17000739 RID: 1849
		// (get) Token: 0x06001873 RID: 6259 RVA: 0x000C4C1E File Offset: 0x000C2E1E
		// (set) Token: 0x06001874 RID: 6260 RVA: 0x000C4C26 File Offset: 0x000C2E26
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float HandCycleSpeed { get; set; }

		// Token: 0x1700073A RID: 1850
		// (get) Token: 0x06001875 RID: 6261 RVA: 0x000C4C2F File Offset: 0x000C2E2F
		// (set) Token: 0x06001876 RID: 6262 RVA: 0x000C4C37 File Offset: 0x000C2E37
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 HandMoveOffset { get; set; }

		// Token: 0x1700073B RID: 1851
		// (get) Token: 0x06001877 RID: 6263 RVA: 0x000C4C40 File Offset: 0x000C2E40
		// (set) Token: 0x06001878 RID: 6264 RVA: 0x000C4C48 File Offset: 0x000C2E48
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the arms.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 20f, DecimalCount = 2)]
		public float ArmMoveStrength { get; set; }

		// Token: 0x1700073C RID: 1852
		// (get) Token: 0x06001879 RID: 6265 RVA: 0x000C4C51 File Offset: 0x000C2E51
		// (set) Token: 0x0600187A RID: 6266 RVA: 0x000C4C59 File Offset: 0x000C2E59
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to move the hands.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandMoveStrength { get; set; }

		// Token: 0x1700073D RID: 1853
		// (get) Token: 0x0600187B RID: 6267 RVA: 0x000C4C62 File Offset: 0x000C2E62
		// (set) Token: 0x0600187C RID: 6268 RVA: 0x000C4C6A File Offset: 0x000C2E6A
		[Header("Other", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Is the head angle fixed or does the angle follow the mouse position?", "", false)]
		[Editable]
		public bool FixedHeadAngle { get; set; }
	}
}
