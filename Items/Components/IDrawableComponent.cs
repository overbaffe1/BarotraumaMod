using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F2 RID: 1522
	internal interface IDrawableComponent
	{
		// Token: 0x17001930 RID: 6448
		// (get) Token: 0x06006390 RID: 25488
		Vector2 DrawSize { get; }

		// Token: 0x06006391 RID: 25489
		void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null);
	}
}
