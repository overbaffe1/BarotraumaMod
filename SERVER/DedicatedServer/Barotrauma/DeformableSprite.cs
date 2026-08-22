using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200028E RID: 654
	internal class DeformableSprite
	{
		// Token: 0x17000D77 RID: 3447
		// (get) Token: 0x06002DE4 RID: 11748 RVA: 0x00130066 File Offset: 0x0012E266
		public Vector2 Size
		{
			get
			{
				return this.Sprite.size;
			}
		}

		// Token: 0x17000D78 RID: 3448
		// (get) Token: 0x06002DE5 RID: 11749 RVA: 0x00130073 File Offset: 0x0012E273
		// (set) Token: 0x06002DE6 RID: 11750 RVA: 0x00130080 File Offset: 0x0012E280
		public Vector2 Origin
		{
			get
			{
				return this.Sprite.Origin;
			}
			set
			{
				this.Sprite.Origin = value;
			}
		}

		// Token: 0x17000D79 RID: 3449
		// (get) Token: 0x06002DE7 RID: 11751 RVA: 0x0013008E File Offset: 0x0012E28E
		// (set) Token: 0x06002DE8 RID: 11752 RVA: 0x00130096 File Offset: 0x0012E296
		public Sprite Sprite { get; private set; }

		// Token: 0x06002DE9 RID: 11753 RVA: 0x0013009F File Offset: 0x0012E29F
		public DeformableSprite(ContentXElement element, int? subdivisionsX = null, int? subdivisionsY = null, string filePath = "", bool lazyLoad = false, bool invert = false, float sourceRectScale = 1f)
		{
			this.Sprite = new Sprite(element, "", filePath, lazyLoad, sourceRectScale);
		}
	}
}
