using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C7 RID: 199
	internal class BrokenItemSprite
	{
		// Token: 0x060019EF RID: 6639 RVA: 0x001069B7 File Offset: 0x00104BB7
		public BrokenItemSprite(Sprite sprite, float maxCondition, bool fadeIn, Point offset)
		{
			this.Sprite = sprite;
			this.MaxConditionPercentage = MathHelper.Clamp(maxCondition, 0f, 100f);
			this.FadeIn = fadeIn;
			this.Offset = offset;
		}

		// Token: 0x04000D46 RID: 3398
		public readonly float MaxConditionPercentage;

		// Token: 0x04000D47 RID: 3399
		public readonly Sprite Sprite;

		// Token: 0x04000D48 RID: 3400
		public readonly bool FadeIn;

		// Token: 0x04000D49 RID: 3401
		public readonly Point Offset;
	}
}
