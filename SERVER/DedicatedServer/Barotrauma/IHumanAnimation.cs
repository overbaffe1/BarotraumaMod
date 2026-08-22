using System;

namespace Barotrauma
{
	// Token: 0x020000ED RID: 237
	public interface IHumanAnimation
	{
		// Token: 0x1700074E RID: 1870
		// (get) Token: 0x0600189F RID: 6303
		// (set) Token: 0x060018A0 RID: 6304
		float FootAngle { get; set; }

		// Token: 0x1700074F RID: 1871
		// (get) Token: 0x060018A1 RID: 6305
		float FootAngleInRadians { get; }

		// Token: 0x17000750 RID: 1872
		// (get) Token: 0x060018A2 RID: 6306
		// (set) Token: 0x060018A3 RID: 6307
		float ArmMoveStrength { get; set; }

		// Token: 0x17000751 RID: 1873
		// (get) Token: 0x060018A4 RID: 6308
		// (set) Token: 0x060018A5 RID: 6309
		float HandMoveStrength { get; set; }

		// Token: 0x17000752 RID: 1874
		// (get) Token: 0x060018A6 RID: 6310
		// (set) Token: 0x060018A7 RID: 6311
		bool FixedHeadAngle { get; set; }
	}
}
