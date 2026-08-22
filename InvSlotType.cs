using System;

namespace Barotrauma
{
	// Token: 0x020002DB RID: 731
	[Flags]
	public enum InvSlotType
	{
		// Token: 0x04001FD5 RID: 8149
		None = 0,
		// Token: 0x04001FD6 RID: 8150
		Any = 1,
		// Token: 0x04001FD7 RID: 8151
		RightHand = 2,
		// Token: 0x04001FD8 RID: 8152
		LeftHand = 4,
		// Token: 0x04001FD9 RID: 8153
		Head = 8,
		// Token: 0x04001FDA RID: 8154
		InnerClothes = 16,
		// Token: 0x04001FDB RID: 8155
		OuterClothes = 32,
		// Token: 0x04001FDC RID: 8156
		Headset = 64,
		// Token: 0x04001FDD RID: 8157
		Card = 128,
		// Token: 0x04001FDE RID: 8158
		Bag = 256,
		// Token: 0x04001FDF RID: 8159
		HealthInterface = 512
	}
}
