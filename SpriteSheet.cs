using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200013D RID: 317
	public class SpriteSheet : Sprite
	{
		// Token: 0x06002946 RID: 10566 RVA: 0x001C7ECC File Offset: 0x001C60CC
		public override void Draw(ISpriteBatch spriteBatch, Vector2 pos, Color color, Vector2 origin, float rotate, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			if (base.texture == null)
			{
				return;
			}
			spriteBatch.Draw(base.texture, pos + this.offset, new Rectangle?(this.sourceRects[0]), color, this.rotation + rotate, origin, scale, spriteEffect, (depth == null) ? this.depth : depth.Value);
		}

		// Token: 0x06002947 RID: 10567 RVA: 0x001C7F34 File Offset: 0x001C6134
		public void Draw(ISpriteBatch spriteBatch, int spriteIndex, Vector2 pos, Color color, Vector2 origin, float rotate, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			if (base.texture == null)
			{
				return;
			}
			spriteBatch.Draw(base.texture, pos + this.offset, new Rectangle?(this.sourceRects[MathHelper.Clamp(spriteIndex, 0, this.sourceRects.Length - 1)]), color, this.rotation + rotate, origin, scale, spriteEffect, (depth == null) ? this.depth : depth.Value);
		}

		// Token: 0x06002948 RID: 10568 RVA: 0x001C7FAD File Offset: 0x001C61AD
		public int GetAnimatedSpriteIndex(float animationSpeed, bool animatePaused = false)
		{
			return (int)(Math.Floor((animatePaused ? Timing.TotalTime : Timing.TotalTimeUnpaused) * (double)animationSpeed) % (double)this.FrameCount);
		}

		// Token: 0x17000A99 RID: 2713
		// (get) Token: 0x06002949 RID: 10569 RVA: 0x001C7FCF File Offset: 0x001C61CF
		public int FrameCount
		{
			get
			{
				return this.sourceRects.Length - this.emptyFrames;
			}
		}

		// Token: 0x17000A9A RID: 2714
		// (get) Token: 0x0600294A RID: 10570 RVA: 0x001C7FE0 File Offset: 0x001C61E0
		// (set) Token: 0x0600294B RID: 10571 RVA: 0x001C7FE8 File Offset: 0x001C61E8
		public Point FrameSize { get; private set; }

		// Token: 0x0600294C RID: 10572 RVA: 0x001C7FF4 File Offset: 0x001C61F4
		public SpriteSheet(ContentXElement element, string path = "", string file = "") : base(element, path, file, false, 1f)
		{
			int columnCount = Math.Max(element.GetAttributeInt("columns", 1), 1);
			int rowCount = Math.Max(element.GetAttributeInt("rows", 1), 1);
			string key = "origin";
			Vector2 vector = new Vector2(0.5f, 0.5f);
			this.origin = element.GetAttributeVector2(key, vector);
			this.emptyFrames = element.GetAttributeInt("emptyframes", 0);
			this.Init(columnCount, rowCount);
		}

		// Token: 0x0600294D RID: 10573 RVA: 0x001C8073 File Offset: 0x001C6273
		public SpriteSheet(string filePath, int columnCount, int rowCount, Vector2 origin, Rectangle? sourceRect = null) : base(filePath, origin)
		{
			this.origin = origin;
			if (sourceRect != null)
			{
				base.SourceRect = sourceRect.Value;
			}
			this.Init(columnCount, rowCount);
		}

		// Token: 0x0600294E RID: 10574 RVA: 0x001C80A4 File Offset: 0x001C62A4
		private void Init(int columnCount, int rowCount)
		{
			this.sourceRects = new Rectangle[rowCount * columnCount];
			float cellWidth = (float)(base.SourceRect.Width / columnCount);
			float cellHeight = (float)(base.SourceRect.Height / rowCount);
			this.FrameSize = new Point((int)cellWidth, (int)cellHeight);
			for (int x = 0; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					this.sourceRects[x + y * columnCount] = new Rectangle((int)((float)base.SourceRect.X + (float)x * cellWidth), (int)((float)base.SourceRect.Y + (float)y * cellHeight), (int)cellWidth, (int)cellHeight);
				}
			}
			this.origin.X = this.origin.X * cellWidth;
			this.origin.Y = this.origin.Y * cellHeight;
		}

		// Token: 0x0400151D RID: 5405
		private Rectangle[] sourceRects;

		// Token: 0x0400151E RID: 5406
		private int emptyFrames;
	}
}
