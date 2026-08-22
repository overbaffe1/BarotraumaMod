using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020001E1 RID: 481
	internal interface IFishAnimation
	{
		// Token: 0x17000D90 RID: 3472
		// (get) Token: 0x0600333C RID: 13116
		// (set) Token: 0x0600333D RID: 13117
		string FootAngles { get; set; }

		// Token: 0x17000D91 RID: 3473
		// (get) Token: 0x0600333E RID: 13118
		// (set) Token: 0x0600333F RID: 13119
		Dictionary<int, float> FootAnglesInRadians { get; set; }

		// Token: 0x17000D92 RID: 3474
		// (get) Token: 0x06003340 RID: 13120
		// (set) Token: 0x06003341 RID: 13121
		float TailAngle { get; set; }

		// Token: 0x17000D93 RID: 3475
		// (get) Token: 0x06003342 RID: 13122
		float TailAngleInRadians { get; }

		// Token: 0x17000D94 RID: 3476
		// (get) Token: 0x06003343 RID: 13123
		// (set) Token: 0x06003344 RID: 13124
		float TailTorque { get; set; }

		// Token: 0x17000D95 RID: 3477
		// (get) Token: 0x06003345 RID: 13125
		// (set) Token: 0x06003346 RID: 13126
		bool Flip { get; set; }

		// Token: 0x17000D96 RID: 3478
		// (get) Token: 0x06003347 RID: 13127
		// (set) Token: 0x06003348 RID: 13128
		float FlipCooldown { get; set; }

		// Token: 0x17000D97 RID: 3479
		// (get) Token: 0x06003349 RID: 13129
		// (set) Token: 0x0600334A RID: 13130
		float FlipDelay { get; set; }
	}
}
