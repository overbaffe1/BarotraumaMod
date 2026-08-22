using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000098 RID: 152
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	public class GUISpriteSheet : GUISelector<GUISpriteSheetPrefab>
	{
		// Token: 0x06001408 RID: 5128 RVA: 0x000BEFA4 File Offset: 0x000BD1A4
		public GUISpriteSheet(string identifier) : base(identifier)
		{
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06001409 RID: 5129 RVA: 0x000BEFAD File Offset: 0x000BD1AD
		[Nullable(2)]
		public SpriteSheet Value
		{
			[NullableContext(2)]
			get
			{
				GUISpriteSheetPrefab activePrefab = this.Prefabs.ActivePrefab;
				if (activePrefab == null)
				{
					return null;
				}
				return activePrefab.SpriteSheet;
			}
		}

		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x0600140A RID: 5130 RVA: 0x000BEFC5 File Offset: 0x000BD1C5
		public int FrameCount
		{
			get
			{
				SpriteSheet value = this.Value;
				if (value == null)
				{
					return 1;
				}
				return value.FrameCount;
			}
		}

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x0600140B RID: 5131 RVA: 0x000BEFD8 File Offset: 0x000BD1D8
		public Point FrameSize
		{
			get
			{
				SpriteSheet value = this.Value;
				if (value == null)
				{
					return Point.Zero;
				}
				return value.FrameSize;
			}
		}

		// Token: 0x0600140C RID: 5132 RVA: 0x000BEFEF File Offset: 0x000BD1EF
		public void Draw(ISpriteBatch spriteBatch, Vector2 pos, float rotate = 0f, float scale = 1f, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			SpriteSheet value = this.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(spriteBatch, pos, rotate, scale, spriteEffects);
		}

		// Token: 0x0600140D RID: 5133 RVA: 0x000BF008 File Offset: 0x000BD208
		public void Draw(ISpriteBatch spriteBatch, Vector2 pos, Color color, Vector2 origin, float rotate = 0f, float scale = 1f, SpriteEffects spriteEffects = SpriteEffects.None, float? depth = null)
		{
			SpriteSheet value = this.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(spriteBatch, pos, color, origin, rotate, scale, spriteEffects, depth);
		}

		// Token: 0x0600140E RID: 5134 RVA: 0x000BF034 File Offset: 0x000BD234
		public void Draw(ISpriteBatch spriteBatch, int spriteIndex, Vector2 pos, Color color, Vector2 origin, float rotate, Vector2 scale, SpriteEffects spriteEffects = SpriteEffects.None, float? depth = null)
		{
			SpriteSheet value = this.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(spriteBatch, spriteIndex, pos, color, origin, rotate, scale, spriteEffects, depth);
		}

		// Token: 0x0600140F RID: 5135 RVA: 0x000BF060 File Offset: 0x000BD260
		[return: Nullable(2)]
		public static implicit operator SpriteSheet(GUISpriteSheet reference)
		{
			return reference.Value;
		}
	}
}
