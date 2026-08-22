using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000BC RID: 188
	public class UISprite
	{
		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x0600174D RID: 5965 RVA: 0x000E2E5C File Offset: 0x000E105C
		// (set) Token: 0x0600174E RID: 5966 RVA: 0x000E2E64 File Offset: 0x000E1064
		public Sprite Sprite { get; private set; }

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x0600174F RID: 5967 RVA: 0x000E2E6D File Offset: 0x000E106D
		// (set) Token: 0x06001750 RID: 5968 RVA: 0x000E2E75 File Offset: 0x000E1075
		public bool Tile { get; private set; }

		// Token: 0x170005D0 RID: 1488
		// (get) Token: 0x06001751 RID: 5969 RVA: 0x000E2E7E File Offset: 0x000E107E
		public bool Slice
		{
			get
			{
				return this.Slices != null;
			}
		}

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x06001752 RID: 5970 RVA: 0x000E2E89 File Offset: 0x000E1089
		// (set) Token: 0x06001753 RID: 5971 RVA: 0x000E2E91 File Offset: 0x000E1091
		public Rectangle[] Slices { get; set; }

		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x06001754 RID: 5972 RVA: 0x000E2E9A File Offset: 0x000E109A
		// (set) Token: 0x06001755 RID: 5973 RVA: 0x000E2EA2 File Offset: 0x000E10A2
		public Point NonSliceSize { get; set; }

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001756 RID: 5974 RVA: 0x000E2EAB File Offset: 0x000E10AB
		// (set) Token: 0x06001757 RID: 5975 RVA: 0x000E2EB3 File Offset: 0x000E10B3
		public bool MaintainAspectRatio { get; private set; }

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x06001758 RID: 5976 RVA: 0x000E2EBC File Offset: 0x000E10BC
		// (set) Token: 0x06001759 RID: 5977 RVA: 0x000E2EC4 File Offset: 0x000E10C4
		public bool MaintainBorderAspectRatio { get; private set; }

		// Token: 0x170005D5 RID: 1493
		// (get) Token: 0x0600175A RID: 5978 RVA: 0x000E2ECD File Offset: 0x000E10CD
		// (set) Token: 0x0600175B RID: 5979 RVA: 0x000E2ED5 File Offset: 0x000E10D5
		public bool CrossFadeIn { get; private set; } = true;

		// Token: 0x170005D6 RID: 1494
		// (get) Token: 0x0600175C RID: 5980 RVA: 0x000E2EDE File Offset: 0x000E10DE
		// (set) Token: 0x0600175D RID: 5981 RVA: 0x000E2EE6 File Offset: 0x000E10E6
		public bool CrossFadeOut { get; private set; } = true;

		// Token: 0x170005D7 RID: 1495
		// (get) Token: 0x0600175E RID: 5982 RVA: 0x000E2EEF File Offset: 0x000E10EF
		// (set) Token: 0x0600175F RID: 5983 RVA: 0x000E2EF7 File Offset: 0x000E10F7
		public TransitionMode TransitionMode { get; private set; }

		// Token: 0x06001760 RID: 5984 RVA: 0x000E2F00 File Offset: 0x000E1100
		public UISprite(ContentXElement element)
		{
			this.Sprite = new Sprite(element, "", "", false, 1f);
			this.MaintainAspectRatio = element.GetAttributeBool("maintainaspectratio", false);
			this.MaintainBorderAspectRatio = element.GetAttributeBool("maintainborderaspectratio", false);
			this.Tile = element.GetAttributeBool("tile", true);
			this.CrossFadeIn = element.GetAttributeBool("crossfadein", this.CrossFadeIn);
			this.CrossFadeOut = element.GetAttributeBool("crossfadeout", this.CrossFadeOut);
			string transitionMode = element.GetAttributeString("transition", string.Empty);
			TransitionMode transition;
			if (Enum.TryParse<TransitionMode>(transitionMode, true, out transition))
			{
				this.TransitionMode = transition;
			}
			string key = "slice";
			Vector4 zero = Vector4.Zero;
			Vector4 sliceVec = element.GetAttributeVector4(key, zero);
			this.Slices = null;
			if (sliceVec != Vector4.Zero)
			{
				this.minBorderScale = element.GetAttributeFloat("minborderscale", 0.1f);
				this.maxBorderScale = element.GetAttributeFloat("minborderscale", 10f);
				Rectangle slice = new Rectangle((int)sliceVec.X, (int)sliceVec.Y, (int)(sliceVec.Z - sliceVec.X), (int)(sliceVec.W - sliceVec.Y));
				this.NonSliceSize = new Point(this.Sprite.SourceRect.Width - slice.Width, this.Sprite.SourceRect.Height - slice.Height);
				this.Slices = new Rectangle[9];
				this.Slices[0] = new Rectangle(this.Sprite.SourceRect.Location, slice.Location - this.Sprite.SourceRect.Location);
				this.Slices[1] = new Rectangle(slice.Location.X, this.Slices[0].Y, slice.Width, this.Slices[0].Height);
				this.Slices[2] = new Rectangle(slice.Right, this.Slices[0].Y, this.Sprite.SourceRect.Right - slice.Right, this.Slices[0].Height);
				this.Slices[3] = new Rectangle(this.Slices[0].X, slice.Y, this.Slices[0].Width, slice.Height);
				this.Slices[4] = slice;
				this.Slices[5] = new Rectangle(this.Slices[2].X, slice.Y, this.Slices[2].Width, slice.Height);
				this.Slices[6] = new Rectangle(this.Slices[0].X, slice.Bottom, this.Slices[0].Width, this.Sprite.SourceRect.Bottom - slice.Bottom);
				this.Slices[7] = new Rectangle(this.Slices[1].X, slice.Bottom, this.Slices[1].Width, this.Sprite.SourceRect.Bottom - slice.Bottom);
				this.Slices[8] = new Rectangle(this.Slices[2].X, slice.Bottom, this.Slices[2].Width, this.Sprite.SourceRect.Bottom - slice.Bottom);
			}
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x000E3320 File Offset: 0x000E1520
		public float GetSliceBorderScale(Point drawSize)
		{
			if (!this.Slice)
			{
				return 1f;
			}
			Vector2 scale = new Vector2(MathHelper.Clamp((float)drawSize.X / (float)(this.Slices[0].Height + this.Slices[6].Height), 0f, 1f), MathHelper.Clamp((float)drawSize.Y / (float)(this.Slices[0].Width + this.Slices[2].Width), 0f, 1f));
			return MathHelper.Clamp(Math.Min(Math.Min(scale.X, scale.Y), GUI.SlicedSpriteScale), this.minBorderScale, this.maxBorderScale);
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x000E33E4 File Offset: 0x000E15E4
		public void Draw(SpriteBatch spriteBatch, RectangleF rect, Color color, SpriteEffects spriteEffects = SpriteEffects.None, Vector2? uvOffset = null)
		{
			this.Draw(spriteBatch, new Rectangle(rect.Location.ToPoint(), rect.Size.ToPoint()), color, spriteEffects, uvOffset);
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x000E3420 File Offset: 0x000E1620
		public void Draw(SpriteBatch spriteBatch, Rectangle rect, Color color, SpriteEffects spriteEffects = SpriteEffects.None, Vector2? uvOffset = null)
		{
			Vector2 value = uvOffset.GetValueOrDefault();
			if (uvOffset == null)
			{
				value = Vector2.Zero;
				uvOffset = new Vector2?(value);
			}
			if (this.Sprite.Texture == null)
			{
				GUI.DrawRectangle(spriteBatch, rect, Color.Magenta, false, 0f, 1f);
				return;
			}
			if (this.Slice)
			{
				Vector2 pos = new Vector2((float)rect.X, (float)rect.Y);
				float scale = this.MaintainBorderAspectRatio ? 1f : this.GetSliceBorderScale(rect.Size);
				float aspectScale = this.MaintainBorderAspectRatio ? Math.Min((float)rect.Width / (float)this.Sprite.SourceRect.Width, (float)rect.Height / (float)this.Sprite.SourceRect.Height) : 1f;
				int centerHeight = rect.Height - (int)((float)(this.Slices[0].Height + this.Slices[6].Height) * scale);
				int centerWidth = rect.Width - (int)((float)(this.Slices[0].Width + this.Slices[2].Width) * scale * aspectScale);
				for (int x = 0; x < 3; x++)
				{
					int width = (int)((x == 1) ? ((float)centerWidth) : ((float)this.Slices[x].Width * scale * aspectScale));
					if (width > 0)
					{
						for (int y = 0; y < 3; y++)
						{
							int height = (int)((y == 1) ? ((float)centerHeight) : ((float)this.Slices[x + y * 3].Height * scale));
							if (height > 0)
							{
								spriteBatch.Draw(this.Sprite.Texture, new Rectangle((int)pos.X, (int)pos.Y, width, height), new Rectangle?(this.Slices[x + y * 3]), color);
								pos.Y += (float)height;
							}
						}
						pos.X += (float)width;
						pos.Y = (float)rect.Y;
					}
				}
				return;
			}
			if (this.Tile)
			{
				Vector2 startPos = new Vector2((float)rect.X, (float)rect.Y);
				Sprite sprite = this.Sprite;
				Vector2 position = startPos;
				Vector2 targetSize = new Vector2((float)rect.Width, (float)rect.Height);
				float rotation = 0f;
				Color? color2 = new Color?(color);
				Vector2? startOffset = uvOffset;
				sprite.DrawTiled(spriteBatch, position, targetSize, rotation, null, color2, startOffset, null, null);
				return;
			}
			if (this.MaintainAspectRatio)
			{
				float scale2 = Math.Min((float)rect.Width / (float)this.Sprite.SourceRect.Width, (float)rect.Height / (float)this.Sprite.SourceRect.Height);
				spriteBatch.Draw(this.Sprite.Texture, rect.Center.ToVector2(), new Rectangle?(this.Sprite.SourceRect), color, 0f, this.Sprite.size / 2f, scale2, spriteEffects, 0f);
				return;
			}
			spriteBatch.Draw(this.Sprite.Texture, rect, new Rectangle?(this.Sprite.SourceRect), color, 0f, Vector2.Zero, spriteEffects, 0f);
		}

		// Token: 0x04000BE8 RID: 3048
		private readonly float minBorderScale = 0.1f;

		// Token: 0x04000BE9 RID: 3049
		private readonly float maxBorderScale = 10f;
	}
}
