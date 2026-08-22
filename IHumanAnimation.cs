using System;

namespace Barotrauma
{
	// Token: 0x020001E9 RID: 489
	public interface IHumanAnimation
	{
		// Token: 0x17000DB5 RID: 3509
		// (get) Token: 0x0600339B RID: 13211
		// (set) Token: 0x0600339C RID: 13212
		float FootAngle { get; set; }

		// Token: 0x17000DB6 RID: 3510
		// (get) Token: 0x0600339D RID: 13213
		float FootAngleInRadians { get; }

		// Token: 0x17000DB7 RID: 3511
		// (get) Token: 0x0600339E RID: 13214
		// (set) Token: 0x0600339F RID: 13215
		float ArmMoveStrength { get; set; }

		// Token: 0x17000DB8 RID: 3512
		// (get) Token: 0x060033A0 RID: 13216
		// (set) Token: 0x060033A1 RID: 13217
		float HandMoveStrength { get; set; }

		// Token: 0x17000DB9 RID: 3513
		// (get) Token: 0x060033A2 RID: 13218
		// (set) Token: 0x060033A3 RID: 13219
		bool FixedHeadAngle { get; set; }
	}
}
