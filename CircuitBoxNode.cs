using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200003B RID: 59
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxNode : CircuitBoxSelectable
	{
		// Token: 0x06000935 RID: 2357 RVA: 0x00052D04 File Offset: 0x00050F04
		protected void UpdateDrawRects()
		{
			RectangleF drawRect = new RectangleF(this.Position - this.Size / 2f, this.Size);
			drawRect.Y = -drawRect.Y;
			drawRect.Y -= drawRect.Height;
			this.DrawRect = drawRect;
			this.TopDrawRect = new RectangleF(drawRect.X, drawRect.Y - 47f, drawRect.Width, 48f);
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x00052D87 File Offset: 0x00050F87
		public void OnUICreated()
		{
			this.Size = CircuitBoxNode.CalculateSize(this.Connectors);
			this.UpdatePositions();
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x00052DA5 File Offset: 0x00050FA5
		public virtual void OnResized(RectangleF drawRect)
		{
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x00052DA8 File Offset: 0x00050FA8
		public void DrawBackground(SpriteBatch spriteBatch, RectangleF drawRect, RectangleF topDrawRect, Color color)
		{
			UISprite nodeFrameSprite = this.CircuitBox.NodeFrameSprite;
			if (nodeFrameSprite != null)
			{
				nodeFrameSprite.Draw(spriteBatch, drawRect, color, SpriteEffects.None, null);
			}
			UISprite nodeTopSprite = this.CircuitBox.NodeTopSprite;
			if (nodeTopSprite == null)
			{
				return;
			}
			nodeTopSprite.Draw(spriteBatch, topDrawRect, color, SpriteEffects.None, null);
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x00052DFC File Offset: 0x00050FFC
		public void Draw(SpriteBatch spriteBatch, Vector2 drawPos, Color color)
		{
			RectangleF drawRect = CircuitBoxNode.OverrideRectLocation(this.DrawRect, drawPos, this.Position);
			RectangleF topDrawRect = CircuitBoxNode.OverrideRectLocation(this.TopDrawRect, drawPos, this.Position);
			this.DrawBackground(spriteBatch, drawRect, topDrawRect, color);
			this.DrawHeader(spriteBatch, topDrawRect, color);
			this.DrawBody(spriteBatch, drawRect, color);
			this.DrawConnectors(spriteBatch, drawPos);
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x00052E54 File Offset: 0x00051054
		public void DrawHUD(SpriteBatch spriteBatch, Camera camera)
		{
			foreach (CircuitBoxConnection c in this.Connectors)
			{
				c.DrawHUD(spriteBatch, camera);
			}
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x00052E88 File Offset: 0x00051088
		public virtual void DrawHeader(SpriteBatch spriteBatch, RectangleF rect, Color color)
		{
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x00052E8A File Offset: 0x0005108A
		public virtual void DrawBody(SpriteBatch spriteBatch, RectangleF rect, Color color)
		{
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x00052E8C File Offset: 0x0005108C
		public void DrawConnectors(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			Color color = Color.White * CircuitBoxNode.Opacity;
			foreach (CircuitBoxConnection c in this.Connectors)
			{
				c.Draw(spriteBatch, drawPos, this.Position, color);
			}
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x00052ED8 File Offset: 0x000510D8
		public void DrawSelection(SpriteBatch spriteBatch, Color color)
		{
			int pad = GUI.IntScale(8f);
			RectangleF rect = this.Rect;
			rect.Y = -rect.Y;
			rect.Y -= rect.Height;
			rect.Inflate(pad, pad);
			GUI.DrawFilledRectangle(spriteBatch, rect, color * CircuitBoxNode.Opacity, 0f);
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x00052F38 File Offset: 0x00051138
		public static RectangleF OverrideRectLocation(RectangleF rect, Vector2 overridePos, Vector2 originalPos)
		{
			rect.Location -= new Vector2(originalPos.X, -originalPos.Y);
			rect.Location += new Vector2(overridePos.X, -overridePos.Y);
			return rect;
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000940 RID: 2368 RVA: 0x00052F8E File Offset: 0x0005118E
		public virtual bool IsResizable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000941 RID: 2369 RVA: 0x00052F91 File Offset: 0x00051191
		// (set) Token: 0x06000942 RID: 2370 RVA: 0x00052F99 File Offset: 0x00051199
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = new Vector2(Math.Clamp(value.X, -4096f, 4096f), Math.Clamp(value.Y, -4096f, 4096f));
				this.UpdatePositions();
			}
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x00052FD6 File Offset: 0x000511D6
		public CircuitBoxNode(CircuitBox circuitBox)
		{
			this.CircuitBox = circuitBox;
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x00052FF0 File Offset: 0x000511F0
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"Size",
			"Pos"
		})]
		public ValueTuple<Vector2, Vector2> ResizeBy(CircuitBoxResizeDirection directions, Vector2 amount)
		{
			Vector2 newSize = this.Size;
			Vector2 newPos = this.Position;
			amount.Y = -amount.Y;
			if (directions.HasFlag(CircuitBoxResizeDirection.Down))
			{
				newSize.Y += amount.Y;
				newSize.Y = Math.Max(newSize.Y, CircuitBoxLabelNode.MinSize.Y);
				newPos = new Vector2(newPos.X, newPos.Y - (newSize.Y - this.Size.Y) / 2f);
			}
			if (directions.HasFlag(CircuitBoxResizeDirection.Right))
			{
				newSize.X += amount.X;
				newSize.X = Math.Max(newSize.X, CircuitBoxLabelNode.MinSize.X);
				newPos = new Vector2(newPos.X + (newSize.X - this.Size.X) / 2f, newPos.Y);
			}
			if (directions.HasFlag(CircuitBoxResizeDirection.Left))
			{
				newSize.X -= amount.X;
				newSize.X = Math.Max(newSize.X, CircuitBoxLabelNode.MinSize.X);
				newPos = new Vector2(newPos.X + (this.Size.X - newSize.X) / 2f, newPos.Y);
			}
			return new ValueTuple<Vector2, Vector2>(newSize, newPos);
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x00053164 File Offset: 0x00051364
		public void ApplyResize(Vector2 newSize, Vector2 newPos)
		{
			if (!MathUtils.IsValid(newSize))
			{
				return;
			}
			this.Size = newSize;
			this.Position = newPos;
			this.UpdatePositions();
			this.OnResized(this.DrawRect);
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x00053190 File Offset: 0x00051390
		public static Vector2 CalculateSize(IReadOnlyList<CircuitBoxConnection> conns)
		{
			Vector2 leftSize = Vector2.Zero;
			Vector2 rightSize = Vector2.Zero;
			foreach (CircuitBoxConnection c in conns)
			{
				if (c.IsOutput)
				{
					rightSize.X = MathF.Max(rightSize.X, c.Length);
				}
				else
				{
					leftSize.X = MathF.Max(leftSize.X, c.Length);
				}
				if (c.IsOutput)
				{
					rightSize.Y += CircuitBoxConnection.Size;
				}
				else
				{
					leftSize.Y += CircuitBoxConnection.Size;
				}
			}
			return new Vector2(leftSize.X + 64f + rightSize.X, 64f + MathF.Max(leftSize.Y, rightSize.Y));
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x00053270 File Offset: 0x00051470
		protected void UpdatePositions()
		{
			Vector2 rectStart = this.Position - this.Size / 2f;
			Vector2 rectSize = this.Size;
			rectSize.Y += 48f;
			this.Rect = new RectangleF(rectStart, rectSize);
			this.UpdateDrawRects();
			int leftIndex = 0;
			int rightIndex = 0;
			int inputCount = 0;
			int outputCount = 0;
			foreach (CircuitBoxConnection c3 in this.Connectors)
			{
				if (c3.IsOutput)
				{
					outputCount++;
				}
				else
				{
					inputCount++;
				}
			}
			Vector2 drawPos = this.Position;
			drawPos.Y = -drawPos.Y;
			foreach (CircuitBoxConnection c2 in from c in this.Connectors
			orderby c.Connection.DisplayOrder
			select c)
			{
				bool isOutput = c2.IsOutput;
				int yIndex = isOutput ? rightIndex : leftIndex;
				int count = isOutput ? outputCount : inputCount;
				float totalHeight = (float)count * CircuitBoxConnection.Size / 2f;
				float y = (float)yIndex * CircuitBoxConnection.Size - totalHeight;
				float halfWidth = this.Rect.Width / 2f - CircuitBoxConnection.Size / 2f;
				halfWidth -= 16f;
				float xOffset = c2.IsOutput ? halfWidth : (-halfWidth);
				Vector2 inputPos = drawPos + new Vector2(xOffset, y + c2.Rect.Height / 2f);
				c2.Position = inputPos;
				if (isOutput)
				{
					rightIndex++;
				}
				else
				{
					leftIndex++;
				}
			}
		}

		// Token: 0x040004C6 RID: 1222
		public RectangleF DrawRect;

		// Token: 0x040004C7 RID: 1223
		private RectangleF TopDrawRect;

		// Token: 0x040004C8 RID: 1224
		public Vector2 Size;

		// Token: 0x040004C9 RID: 1225
		public RectangleF Rect;

		// Token: 0x040004CA RID: 1226
		private Vector2 position;

		// Token: 0x040004CB RID: 1227
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxConnection> Connectors = ImmutableArray<CircuitBoxConnection>.Empty;

		// Token: 0x040004CC RID: 1228
		public static float Opacity = 0.8f;

		// Token: 0x040004CD RID: 1229
		public readonly CircuitBox CircuitBox;
	}
}
