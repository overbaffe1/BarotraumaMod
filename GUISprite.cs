using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000096 RID: 150
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	public class GUISprite : GUISelector<GUISpritePrefab>
	{
		// Token: 0x06001401 RID: 5121 RVA: 0x000BEEF6 File Offset: 0x000BD0F6
		public GUISprite(string identifier) : base(identifier)
		{
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06001402 RID: 5122 RVA: 0x000BEEFF File Offset: 0x000BD0FF
		[Nullable(2)]
		public UISprite Value
		{
			[NullableContext(2)]
			get
			{
				GUISpritePrefab activePrefab = this.Prefabs.ActivePrefab;
				if (activePrefab == null)
				{
					return null;
				}
				return activePrefab.Sprite;
			}
		}

		// Token: 0x06001403 RID: 5123 RVA: 0x000BEF17 File Offset: 0x000BD117
		[return: Nullable(2)]
		public static implicit operator UISprite(GUISprite reference)
		{
			return reference.Value;
		}

		// Token: 0x06001404 RID: 5124 RVA: 0x000BEF20 File Offset: 0x000BD120
		public void Draw(SpriteBatch spriteBatch, RectangleF rect, Color color, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			UISprite value = this.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(spriteBatch, rect, color, spriteEffects, null);
		}

		// Token: 0x06001405 RID: 5125 RVA: 0x000BEF4C File Offset: 0x000BD14C
		public void Draw(SpriteBatch spriteBatch, Rectangle rect, Color color, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			UISprite value = this.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(spriteBatch, rect, color, spriteEffects, null);
		}
	}
}
