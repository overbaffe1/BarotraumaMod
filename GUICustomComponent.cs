using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000085 RID: 133
	internal class GUICustomComponent : GUIComponent
	{
		// Token: 0x0600128C RID: 4748 RVA: 0x000B638C File Offset: 0x000B458C
		public GUICustomComponent(RectTransform rectT, Action<SpriteBatch, GUICustomComponent> onDraw = null, Action<float, GUICustomComponent> onUpdate = null) : base(null, rectT)
		{
			this.OnDraw = onDraw;
			this.OnUpdate = onUpdate;
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x000B63A4 File Offset: 0x000B45A4
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (this.HideElementsOutsideFrame)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, this.Rect);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			Action<SpriteBatch, GUICustomComponent> onDraw = this.OnDraw;
			if (onDraw != null)
			{
				onDraw(spriteBatch, this);
			}
			if (this.HideElementsOutsideFrame)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
		}

		// Token: 0x0600128E RID: 4750 RVA: 0x000B6452 File Offset: 0x000B4652
		protected override void Update(float deltaTime)
		{
			if (base.Visible)
			{
				Action<float, GUICustomComponent> onUpdate = this.OnUpdate;
				if (onUpdate == null)
				{
					return;
				}
				onUpdate(deltaTime, this);
			}
		}

		// Token: 0x0400094B RID: 2379
		public Action<SpriteBatch, GUICustomComponent> OnDraw;

		// Token: 0x0400094C RID: 2380
		public Action<float, GUICustomComponent> OnUpdate;

		// Token: 0x0400094D RID: 2381
		public bool HideElementsOutsideFrame;
	}
}
