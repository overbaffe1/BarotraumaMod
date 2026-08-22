using System;

namespace Barotrauma
{
	// Token: 0x020001F1 RID: 497
	[Flags]
	public enum InvSlotType
	{
		// Token: 0x0400114C RID: 4428
		None = 0,
		// Token: 0x0400114D RID: 4429
		Any = 1,
		// Token: 0x0400114E RID: 4430
		RightHand = 2,
		// Token: 0x0400114F RID: 4431
		LeftHand = 4,
		// Token: 0x04001150 RID: 4432
		Head = 8,
		// Token: 0x04001151 RID: 4433
		InnerClothes = 16,
		// Token: 0x04001152 RID: 4434
		OuterClothes = 32,
		// Token: 0x04001153 RID: 4435
		Headset = 64,
		// Token: 0x04001154 RID: 4436
		Card = 128,
		// Token: 0x04001155 RID: 4437
		Bag = 256,
		// Token: 0x04001156 RID: 4438
		HealthInterface = 512
	}
}
