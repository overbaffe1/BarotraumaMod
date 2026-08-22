using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000088 RID: 136
	public class GUIFrame : GUIComponent
	{
		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x060012C4 RID: 4804 RVA: 0x000B7458 File Offset: 0x000B5658
		// (set) Token: 0x060012C5 RID: 4805 RVA: 0x000B7460 File Offset: 0x000B5660
		public float OutlineThickness { get; set; }

		// Token: 0x060012C6 RID: 4806 RVA: 0x000B7469 File Offset: 0x000B5669
		public GUIFrame(RectTransform rectT, string style = "", Color? color = null) : base(style, rectT)
		{
			this.Enabled = true;
			if (color != null)
			{
				this.color = color.Value;
			}
		}

		// Token: 0x060012C7 RID: 4807 RVA: 0x000B7490 File Offset: 0x000B5690
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			Color currColor = this.GetColor(this.State);
			if (this.sprites != null)
			{
				if (this.sprites.Any((KeyValuePair<GUIComponent.ComponentState, List<UISprite>> s) => s.Value.Any<UISprite>()))
				{
					goto IL_75;
				}
			}
			GUI.DrawRectangle(spriteBatch, this.Rect, currColor * ((float)currColor.A / 255f), true, 0f, 1f);
			IL_75:
			base.Draw(spriteBatch);
			if (this.OutlineColor != Color.Transparent)
			{
				GUI.DrawRectangle(spriteBatch, this.Rect, this.OutlineColor, false, 0f, this.OutlineThickness);
			}
		}
	}
}
