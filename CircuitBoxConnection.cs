using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000036 RID: 54
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class CircuitBoxConnection
	{
		// Token: 0x1700029B RID: 667
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x0005093F File Offset: 0x0004EB3F
		public string Name
		{
			get
			{
				return this.Connection.Name;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x060008F0 RID: 2288 RVA: 0x0005094C File Offset: 0x0004EB4C
		// (set) Token: 0x060008F1 RID: 2289 RVA: 0x00050954 File Offset: 0x0004EB54
		public CircuitBoxLabel Label { get; private set; }

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x060008F2 RID: 2290 RVA: 0x0005095D File Offset: 0x0004EB5D
		private static int Padding
		{
			get
			{
				return GUI.IntScale(8f);
			}
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0005096C File Offset: 0x0004EB6C
		public void SetLabel(LocalizedString label, CircuitBoxNode node)
		{
			this.Label = new CircuitBoxLabel(label, GUIStyle.SubHeadingFont);
			this.Length = this.Rect.Width + (float)CircuitBoxConnection.Padding + this.Label.Size.X;
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x000509B8 File Offset: 0x0004EBB8
		public void Draw(SpriteBatch spriteBatch, Vector2 drawPos, Vector2 parentPos, Color color)
		{
			CircuitBoxUI circuitBoxUi = this.CircuitBox.UI;
			if (circuitBoxUi == null)
			{
				return;
			}
			RectangleF drawRect = CircuitBoxNode.OverrideRectLocation(this.Rect, drawPos, parentPos);
			Vector2 cursorPos = circuitBoxUi.GetCursorPosition();
			cursorPos.Y = -cursorPos.Y;
			bool isMouseOver = drawRect.Contains(cursorPos);
			float xPos;
			if (this.IsOutput)
			{
				xPos = drawRect.Left - (float)CircuitBoxConnection.Padding - this.Label.Size.X;
			}
			else
			{
				xPos = drawRect.Right + (float)CircuitBoxConnection.Padding;
			}
			Vector2 stringPos = new Vector2(xPos, drawRect.Center.Y - this.Label.Size.Y / 2f);
			Vector2 pos2 = stringPos;
			LocalizedString value = this.Label.Value;
			Color color2 = GUIStyle.TextColorNormal;
			GUIFont font = this.Label.Font;
			GUI.DrawString(spriteBatch, pos2, value, color2, null, 0, font, ForceUpperCase.Inherit);
			if (this.knobSprite == null)
			{
				CircuitBoxUI.DrawRectangleWithBorder(spriteBatch, drawRect, GUIStyle.Blue * 0.3f, GUIStyle.Blue);
			}
			else
			{
				float scale = drawRect.Height / this.knobSprite.size.Y;
				Sprite sprite = this.knobSprite;
				if (sprite != null)
				{
					sprite.Draw(spriteBatch, drawRect.Center, color, 0f, scale, SpriteEffects.None, null);
				}
			}
			CircuitBoxOutputConnection output = this as CircuitBoxOutputConnection;
			bool flag;
			if (output == null)
			{
				CircuitBoxInputConnection input = this as CircuitBoxInputConnection;
				if (input == null)
				{
					flag = (this.Connection.Wires.Count > 0 || this.Connection.CircuitBoxConnections.Count > 0 || this.ExternallyConnectedFrom.Count > 0);
				}
				else
				{
					flag = (input.ExternallyConnectedTo.Count > 0);
				}
			}
			else
			{
				flag = (output.ExternallyConnectedFrom.Count > 0);
			}
			bool isScrewed = flag;
			if (isMouseOver)
			{
				UISprite value2 = GUIStyle.UIGlowCircular.Value;
				Sprite glowSprite = (value2 != null) ? value2.Sprite : null;
				if (glowSprite != null)
				{
					float glowScale = 40f / glowSprite.size.X;
					if (isScrewed)
					{
						glowScale *= 1.2f;
					}
					glowSprite.Draw(spriteBatch, this.position, GUIStyle.Yellow, glowSprite.size / 2f, 0f, glowScale, SpriteEffects.None, null);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			this.tooltip = none;
			if (ConnectionPanel.ShouldDebugDrawWiring)
			{
				LocalizedString tooltipText;
				Connection.DrawConnectionDebugInfo(spriteBatch, this.Connection, drawRect.Center, isScrewed ? 1.1f : 0.9f, out tooltipText);
				if (isMouseOver && !tooltipText.IsNullOrEmpty())
				{
					this.tooltip = Option.Some<LocalizedString>(tooltipText);
				}
			}
			if (!isScrewed)
			{
				return;
			}
			if (this.screwSprite != null)
			{
				float screwScale = drawRect.Height / this.screwSprite.size.Y;
				this.screwSprite.Draw(spriteBatch, drawRect.Center, color, 0f, screwScale, SpriteEffects.None, null);
			}
			if (this.connectorSprite != null)
			{
				float screwScale2 = drawRect.Height / this.connectorSprite.size.Y * 2f;
				Vector2 pos = drawRect.Center;
				this.connectorSprite.Draw(spriteBatch, pos, Color.White, this.connectorSprite.Origin, 3.1415927f / (this.IsOutput ? -2f : 2f), screwScale2, SpriteEffects.None, null);
			}
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x00050D3C File Offset: 0x0004EF3C
		public void DrawHUD(SpriteBatch spriteBatch, Camera camera)
		{
			LocalizedString text;
			if (!this.tooltip.TryUnwrap(out text))
			{
				return;
			}
			Vector2 drawPos = camera.WorldToScreen(new Vector2(this.Rect.Right, -this.Rect.Bottom));
			GUIComponent.DrawToolTip(spriteBatch, text, drawPos, null, null);
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x060008F6 RID: 2294
		public abstract bool IsOutput { get; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x060008F7 RID: 2295 RVA: 0x00050D9B File Offset: 0x0004EF9B
		// (set) Token: 0x060008F8 RID: 2296 RVA: 0x00050DA4 File Offset: 0x0004EFA4
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.Rect.X = value.X - this.Rect.Width / 2f;
				this.Rect.Y = value.Y - this.Rect.Height / 2f;
				this.position = value;
			}
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x060008F9 RID: 2297 RVA: 0x00050DFE File Offset: 0x0004EFFE
		// (set) Token: 0x060008FA RID: 2298 RVA: 0x00050E06 File Offset: 0x0004F006
		public float Length { get; private set; }

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x060008FB RID: 2299 RVA: 0x00050E10 File Offset: 0x0004F010
		public Vector2 AnchorPoint
		{
			get
			{
				return new Vector2(this.IsOutput ? (this.Rect.Right + 24f) : (this.Rect.Left - 24f), this.Rect.Center.Y);
			}
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x00050E60 File Offset: 0x0004F060
		protected CircuitBoxConnection(Vector2 position, Connection connection, CircuitBox circuitBox)
		{
			Option.UnspecifiedNone none = Option.None;
			this.tooltip = none;
			this.ExternallyConnectedFrom = new List<CircuitBoxConnection>();
			base..ctor();
			this.Connection = connection;
			this.Rect.Width = (this.Rect.Height = CircuitBoxConnection.Size);
			this.Position = position;
			this.CircuitBox = circuitBox;
			this.InitProjSpecific(circuitBox);
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x00050ECC File Offset: 0x0004F0CC
		private void InitProjSpecific(CircuitBox circuitBox)
		{
			this.Label = new CircuitBoxLabel(this.Connection.DisplayName, GUIStyle.SubHeadingFont);
			this.knobSprite = circuitBox.ConnectionSprite;
			this.screwSprite = circuitBox.ConnectionScrewSprite;
			this.connectorSprite = circuitBox.WireConnectorSprite;
			this.Length = this.Rect.Width + (float)CircuitBoxConnection.Padding + this.Label.Size.X;
		}

		// Token: 0x060008FE RID: 2302
		public abstract void ReceiveSignal(Signal signal);

		// Token: 0x060008FF RID: 2303 RVA: 0x00050F44 File Offset: 0x0004F144
		public bool Contains(Vector2 pos)
		{
			float x = this.Rect.X;
			float y = -(this.Rect.Y + this.Rect.Height);
			float width = this.Rect.Width;
			float height = this.Rect.Height;
			RectangleF rect = new RectangleF(x, y, width, height);
			return rect.Contains(pos);
		}

		// Token: 0x0400049E RID: 1182
		[Nullable(2)]
		private Sprite knobSprite;

		// Token: 0x0400049F RID: 1183
		[Nullable(2)]
		private Sprite screwSprite;

		// Token: 0x040004A0 RID: 1184
		[Nullable(2)]
		private Sprite connectorSprite;

		// Token: 0x040004A1 RID: 1185
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private Option<LocalizedString> tooltip;

		// Token: 0x040004A2 RID: 1186
		public readonly Connection Connection;

		// Token: 0x040004A3 RID: 1187
		public RectangleF Rect;

		// Token: 0x040004A4 RID: 1188
		private Vector2 position;

		// Token: 0x040004A5 RID: 1189
		public readonly List<CircuitBoxConnection> ExternallyConnectedFrom;

		// Token: 0x040004A6 RID: 1190
		public static readonly float Size = 32f;

		// Token: 0x040004A8 RID: 1192
		public readonly CircuitBox CircuitBox;
	}
}
