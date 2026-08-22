using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200009D RID: 157
	public sealed class GUIScissorComponent : GUIComponent
	{
		// Token: 0x06001423 RID: 5155 RVA: 0x000BFA40 File Offset: 0x000BDC40
		public GUIScissorComponent(RectTransform rectT) : base(null, rectT)
		{
			this.Content = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			rectT.ChildrenChanged += this.CheckForChildren;
		}

		// Token: 0x06001424 RID: 5156 RVA: 0x000BFAAB File Offset: 0x000BDCAB
		private void CheckForChildren(RectTransform rectT)
		{
			if (rectT == this.Content.RectTransform)
			{
				return;
			}
			throw new InvalidOperationException("Children were found in GUIScissorComponent, Add them to GUIScissorComponent.Content instead.");
		}

		// Token: 0x06001425 RID: 5157 RVA: 0x000BFAC6 File Offset: 0x000BDCC6
		public override void DrawChildren(SpriteBatch spriteBatch, bool recursive)
		{
		}

		// Token: 0x06001426 RID: 5158 RVA: 0x000BFAC8 File Offset: 0x000BDCC8
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			RasterizerState prevRasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, this.Rect);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			foreach (GUIComponent child in this.Content.Children)
			{
				if (child.Visible)
				{
					child.DrawManually(spriteBatch, true, true);
				}
			}
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, prevRasterizerState, null, null);
		}

		// Token: 0x06001427 RID: 5159 RVA: 0x000BFBB0 File Offset: 0x000BDDB0
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			GUIScissorComponent.ClampChildMouseRects(this.Content);
		}

		// Token: 0x06001428 RID: 5160 RVA: 0x000BFBC4 File Offset: 0x000BDDC4
		private static void ClampChildMouseRects(GUIComponent child)
		{
			child.ClampMouseRectToParent = true;
			if (child is GUIListBox)
			{
				return;
			}
			foreach (GUIComponent grandChild in child.Children)
			{
				GUIScissorComponent.ClampChildMouseRects(grandChild);
			}
		}

		// Token: 0x06001429 RID: 5161 RVA: 0x000BFC20 File Offset: 0x000BDE20
		public override void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			if (!base.Visible)
			{
				return;
			}
			base.UpdateOrder = order;
			GUI.AddToUpdateList(this);
			if (ignoreChildren)
			{
				Action<GUIComponent> onAddedToGUIUpdateList = this.OnAddedToGUIUpdateList;
				if (onAddedToGUIUpdateList == null)
				{
					return;
				}
				onAddedToGUIUpdateList(this);
				return;
			}
			else
			{
				foreach (GUIComponent child in this.Content.Children)
				{
					if (child.Visible)
					{
						child.AddToGUIUpdateList(false, order);
					}
				}
				Action<GUIComponent> onAddedToGUIUpdateList2 = this.OnAddedToGUIUpdateList;
				if (onAddedToGUIUpdateList2 == null)
				{
					return;
				}
				onAddedToGUIUpdateList2(this);
				return;
			}
		}

		// Token: 0x040009F1 RID: 2545
		public GUIComponent Content;
	}
}
