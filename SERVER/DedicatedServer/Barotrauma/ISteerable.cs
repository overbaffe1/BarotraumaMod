using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005F RID: 95
	internal interface ISteerable
	{
		// Token: 0x1700038C RID: 908
		// (get) Token: 0x06000D9A RID: 3482
		// (set) Token: 0x06000D9B RID: 3483
		Vector2 Steering { get; set; }

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06000D9C RID: 3484
		Vector2 Velocity { get; }

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06000D9D RID: 3485
		Vector2 SimPosition { get; }

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x06000D9E RID: 3486
		Vector2 WorldPosition { get; }
	}
}
