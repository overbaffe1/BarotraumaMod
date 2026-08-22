using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000A3 RID: 163
	public class GUITextBlock : GUIComponent
	{
		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x0600147A RID: 5242 RVA: 0x000C1B6B File Offset: 0x000BFD6B
		public bool OverflowClipActive
		{
			get
			{
				return this.overflowClipActive;
			}
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x0600147B RID: 5243 RVA: 0x000C1B73 File Offset: 0x000BFD73
		// (set) Token: 0x0600147C RID: 5244 RVA: 0x000C1B7B File Offset: 0x000BFD7B
		public Vector2 TextOffset { get; set; }

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x0600147D RID: 5245 RVA: 0x000C1B84 File Offset: 0x000BFD84
		// (set) Token: 0x0600147E RID: 5246 RVA: 0x000C1B8C File Offset: 0x000BFD8C
		public Vector4 Padding
		{
			get
			{
				return this.padding;
			}
			set
			{
				this.padding = value;
				this.SetTextPos();
			}
		}

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x0600147F RID: 5247 RVA: 0x000C1B9B File Offset: 0x000BFD9B
		// (set) Token: 0x06001480 RID: 5248 RVA: 0x000C1BA3 File Offset: 0x000BFDA3
		public override GUIFont Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				if (base.Font == value)
				{
					return;
				}
				base.Font = value;
				if (this.text != null)
				{
					this.Text = this.text;
				}
				this.SetTextPos();
			}
		}

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06001481 RID: 5249 RVA: 0x000C1BD6 File Offset: 0x000BFDD6
		// (set) Token: 0x06001482 RID: 5250 RVA: 0x000C1BE0 File Offset: 0x000BFDE0
		public RichString Text
		{
			get
			{
				return this.text;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				ForceUpperCase forceUpperCase = this.forceUpperCase;
				RichString richString;
				switch (forceUpperCase)
				{
				case ForceUpperCase.Inherit:
					richString = value.CaseTiedToFontAndStyle(this.Font, base.Style);
					break;
				case ForceUpperCase.No:
					richString = value.CaseTiedToFontAndStyle(null, null);
					break;
				case ForceUpperCase.Yes:
					richString = value.ToUpper();
					break;
				default:
					<PrivateImplementationDetails>.ThrowSwitchExpressionException(forceUpperCase);
					break;
				}
				RichString newText = richString;
				if (this.Text == newText)
				{
					return;
				}
				if (this.autoScaleHorizontal || this.autoScaleVertical)
				{
					this.textScale = 1f;
				}
				this.text = newText;
				this.wrappedText = newText.SanitizedString;
				this.SetTextPos();
			}
		}

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x06001483 RID: 5251 RVA: 0x000C1C90 File Offset: 0x000BFE90
		public LocalizedString WrappedText
		{
			get
			{
				return this.wrappedText;
			}
		}

		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x06001484 RID: 5252 RVA: 0x000C1C98 File Offset: 0x000BFE98
		// (set) Token: 0x06001485 RID: 5253 RVA: 0x000C1CA0 File Offset: 0x000BFEA0
		public float TextDepth
		{
			get
			{
				return this.textDepth;
			}
			set
			{
				this.textDepth = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x06001486 RID: 5254 RVA: 0x000C1CB8 File Offset: 0x000BFEB8
		// (set) Token: 0x06001487 RID: 5255 RVA: 0x000C1CC0 File Offset: 0x000BFEC0
		public Vector2 TextPos
		{
			get
			{
				return this.textPos;
			}
			set
			{
				this.textPos = value;
				this.ClearCaretPositions();
			}
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x06001488 RID: 5256 RVA: 0x000C1CCF File Offset: 0x000BFECF
		// (set) Token: 0x06001489 RID: 5257 RVA: 0x000C1CD7 File Offset: 0x000BFED7
		public float TextScale
		{
			get
			{
				return this.textScale;
			}
			set
			{
				if (value != this.textScale)
				{
					this.textScale = value;
					this.SetTextPos();
				}
			}
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x0600148A RID: 5258 RVA: 0x000C1CEF File Offset: 0x000BFEEF
		// (set) Token: 0x0600148B RID: 5259 RVA: 0x000C1CF7 File Offset: 0x000BFEF7
		public bool AutoScaleHorizontal
		{
			get
			{
				return this.autoScaleHorizontal;
			}
			set
			{
				if (this.autoScaleHorizontal == value)
				{
					return;
				}
				this.autoScaleHorizontal = value;
				if (this.autoScaleHorizontal)
				{
					this.SetTextPos();
				}
			}
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x0600148C RID: 5260 RVA: 0x000C1D18 File Offset: 0x000BFF18
		// (set) Token: 0x0600148D RID: 5261 RVA: 0x000C1D20 File Offset: 0x000BFF20
		public bool AutoScaleVertical
		{
			get
			{
				return this.autoScaleVertical;
			}
			set
			{
				if (this.autoScaleVertical == value)
				{
					return;
				}
				this.autoScaleVertical = value;
				if (this.autoScaleVertical)
				{
					this.SetTextPos();
				}
			}
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x0600148E RID: 5262 RVA: 0x000C1D41 File Offset: 0x000BFF41
		// (set) Token: 0x0600148F RID: 5263 RVA: 0x000C1D49 File Offset: 0x000BFF49
		public ForceUpperCase ForceUpperCase
		{
			get
			{
				return this.forceUpperCase;
			}
			set
			{
				if (this.forceUpperCase == value)
				{
					return;
				}
				this.forceUpperCase = value;
				if (this.text != null)
				{
					this.Text = this.text;
				}
			}
		}

		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x06001490 RID: 5264 RVA: 0x000C1D76 File Offset: 0x000BFF76
		public Vector2 Origin
		{
			get
			{
				return this.origin;
			}
		}

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x06001491 RID: 5265 RVA: 0x000C1D7E File Offset: 0x000BFF7E
		// (set) Token: 0x06001492 RID: 5266 RVA: 0x000C1D86 File Offset: 0x000BFF86
		public Vector2 TextSize { get; private set; }

		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x06001493 RID: 5267 RVA: 0x000C1D8F File Offset: 0x000BFF8F
		// (set) Token: 0x06001494 RID: 5268 RVA: 0x000C1D97 File Offset: 0x000BFF97
		public Color TextColor
		{
			get
			{
				return this.textColor;
			}
			set
			{
				this.textColor = value;
			}
		}

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x06001495 RID: 5269 RVA: 0x000C1DA0 File Offset: 0x000BFFA0
		// (set) Token: 0x06001496 RID: 5270 RVA: 0x000C1DA8 File Offset: 0x000BFFA8
		public Color DisabledTextColor
		{
			get
			{
				return this.disabledTextColor;
			}
			set
			{
				this.disabledTextColor = value;
			}
		}

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x06001497 RID: 5271 RVA: 0x000C1DB4 File Offset: 0x000BFFB4
		// (set) Token: 0x06001498 RID: 5272 RVA: 0x000C1DDF File Offset: 0x000BFFDF
		public Color HoverTextColor
		{
			get
			{
				Color? color = this.hoverTextColor;
				if (color == null)
				{
					return this.textColor;
				}
				return color.GetValueOrDefault();
			}
			set
			{
				this.hoverTextColor = new Color?(value);
			}
		}

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x06001499 RID: 5273 RVA: 0x000C1DED File Offset: 0x000BFFED
		// (set) Token: 0x0600149A RID: 5274 RVA: 0x000C1DF5 File Offset: 0x000BFFF5
		public Color SelectedTextColor
		{
			get
			{
				return this.selectedTextColor;
			}
			set
			{
				this.selectedTextColor = value;
			}
		}

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x0600149B RID: 5275 RVA: 0x000C1DFE File Offset: 0x000BFFFE
		// (set) Token: 0x0600149C RID: 5276 RVA: 0x000C1E06 File Offset: 0x000C0006
		public Alignment TextAlignment
		{
			get
			{
				return this.textAlignment;
			}
			set
			{
				if (this.textAlignment == value)
				{
					return;
				}
				this.textAlignment = value;
				this.SetTextPos();
			}
		}

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x0600149D RID: 5277 RVA: 0x000C1E1F File Offset: 0x000C001F
		// (set) Token: 0x0600149E RID: 5278 RVA: 0x000C1E27 File Offset: 0x000C0027
		public bool Censor { get; set; }

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x0600149F RID: 5279 RVA: 0x000C1E30 File Offset: 0x000C0030
		public string CensoredText
		{
			get
			{
				return this.censoredText;
			}
		}

		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x060014A0 RID: 5280 RVA: 0x000C1E38 File Offset: 0x000C0038
		public ImmutableArray<RichTextData>? RichTextData
		{
			get
			{
				return this.text.RichTextData;
			}
		}

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x060014A1 RID: 5281 RVA: 0x000C1E48 File Offset: 0x000C0048
		public bool HasColorHighlight
		{
			get
			{
				return this.RichTextData != null;
			}
		}

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x060014A2 RID: 5282 RVA: 0x000C1E63 File Offset: 0x000C0063
		// (set) Token: 0x060014A3 RID: 5283 RVA: 0x000C1E6B File Offset: 0x000C006B
		public List<GUITextBlock.ClickableArea> ClickableAreas { get; private set; } = new List<GUITextBlock.ClickableArea>();

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x060014A4 RID: 5284 RVA: 0x000C1E74 File Offset: 0x000C0074
		// (set) Token: 0x060014A5 RID: 5285 RVA: 0x000C1E7C File Offset: 0x000C007C
		public bool Shadow { get; set; }

		// Token: 0x060014A6 RID: 5286 RVA: 0x000C1E88 File Offset: 0x000C0088
		public GUITextBlock(RectTransform rectT, RichString text, Color? textColor = null, GUIFont font = null, Alignment textAlignment = Alignment.Left, bool wrap = false, string style = "", Color? color = null) : base(style, rectT)
		{
			if (color != null)
			{
				this.color = color.Value;
			}
			if (textColor != null)
			{
				this.OverrideTextColor(textColor.Value);
			}
			GUIFont selectedFont = font ?? GUIStyle.Font;
			this.Font = selectedFont;
			this.textAlignment = textAlignment;
			this.Wrap = wrap;
			this.Text = (text ?? "");
			if (rectT.Rect.Height == 0 && !text.IsNullOrEmpty())
			{
				this.CalculateHeightFromText(0, false);
			}
			this.SetTextPos();
			base.RectTransform.ScaleChanged += this.SetTextPos;
			base.RectTransform.SizeChanged += this.SetTextPos;
			this.Enabled = true;
			this.Censor = false;
		}

		// Token: 0x060014A7 RID: 5287 RVA: 0x000C1F90 File Offset: 0x000C0190
		public void CalculateHeightFromText(int padding = 0, bool removeExtraSpacing = false)
		{
			if (this.wrappedText == null)
			{
				return;
			}
			base.RectTransform.Resize(new Point(base.RectTransform.Rect.Width, (int)this.Font.MeasureString(this.wrappedText, removeExtraSpacing).Y + padding), true);
		}

		// Token: 0x060014A8 RID: 5288 RVA: 0x000C1FE7 File Offset: 0x000C01E7
		public void SetRichText(LocalizedString richText)
		{
			this.Text = RichString.Rich(richText, null);
		}

		// Token: 0x060014A9 RID: 5289 RVA: 0x000C1FF8 File Offset: 0x000C01F8
		public override void ApplyStyle(GUIComponentStyle componentStyle)
		{
			if (componentStyle == null)
			{
				return;
			}
			base.ApplyStyle(componentStyle);
			this.padding = componentStyle.Padding;
			this.textColor = componentStyle.TextColor;
			this.hoverTextColor = new Color?(componentStyle.HoverTextColor);
			this.disabledTextColor = componentStyle.DisabledTextColor;
			this.selectedTextColor = componentStyle.SelectedTextColor;
			if (this.Font == null || !componentStyle.Font.IsEmpty)
			{
				this.Font = GUIStyle.Fonts[componentStyle.Font.AppendIfMissing("Font")];
			}
		}

		// Token: 0x060014AA RID: 5290 RVA: 0x000C2086 File Offset: 0x000C0286
		public void ClearCaretPositions()
		{
			this.cachedCaretPositions = ImmutableArray<Vector2>.Empty;
		}

		// Token: 0x060014AB RID: 5291 RVA: 0x000C2094 File Offset: 0x000C0294
		public void SetTextPos()
		{
			this.ClearCaretPositions();
			if (this.text == null)
			{
				return;
			}
			this.censoredText = (this.text.IsNullOrEmpty() ? "" : new string('•', this.text.Length));
			Rectangle rect = this.Rect;
			this.overflowClipActive = false;
			this.wrappedText = this.text.SanitizedString;
			this.TextSize = this.MeasureText(this.text.SanitizedString);
			if (this.Wrap && rect.Width > 0)
			{
				this.wrappedText = ToolBox.WrapText(this.text.SanitizedString, (float)rect.Width - this.padding.X - this.padding.Z, this.Font, this.textScale);
				this.TextSize = this.MeasureText(this.wrappedText);
			}
			else if (this.OverflowClip)
			{
				this.overflowClipActive = (this.TextSize.X > (float)rect.Width - this.padding.X - this.padding.Z);
			}
			Vector2 minSize = new Vector2(Math.Max((float)rect.Width - this.padding.X - this.padding.Z, 5f), Math.Max((float)rect.Height - this.padding.Y - this.padding.W, 5f));
			if (!this.autoScaleHorizontal)
			{
				minSize.X = float.MaxValue;
			}
			if (!this.Wrap && !this.autoScaleVertical)
			{
				minSize.Y = float.MaxValue;
			}
			if ((this.autoScaleHorizontal || this.autoScaleVertical) && this.textScale > 0.1f && (this.TextSize.X * this.textScale > minSize.X || this.TextSize.Y * this.textScale > minSize.Y))
			{
				this.TextScale = Math.Max(0.1f, Math.Min(minSize.X / this.TextSize.X, minSize.Y / this.TextSize.Y)) - 0.01f;
				return;
			}
			this.textPos = new Vector2(this.padding.X + ((float)rect.Width - this.padding.Z - this.padding.X) / 2f, this.padding.Y + ((float)rect.Height - this.padding.Y - this.padding.W) / 2f);
			this.origin = this.TextSize * 0.5f;
			this.origin.X = 0f;
			if (this.textAlignment.HasFlag(Alignment.Left))
			{
				this.textPos.X = this.padding.X;
			}
			if (this.textAlignment.HasFlag(Alignment.Right))
			{
				this.textPos.X = (float)rect.Width - this.padding.Z;
			}
			if (this.textAlignment.HasFlag(Alignment.Top))
			{
				this.textPos.Y = this.padding.Y;
				this.origin.Y = 0f;
			}
			if (this.textAlignment.HasFlag(Alignment.Bottom))
			{
				this.textPos.Y = (float)rect.Height - this.padding.W;
				this.origin.Y = this.TextSize.Y;
			}
			this.origin.X = (float)((int)this.origin.X);
			this.origin.Y = (float)((int)this.origin.Y);
			this.textPos.X = (float)((int)this.textPos.X);
			this.textPos.Y = (float)((int)this.textPos.Y);
		}

		// Token: 0x060014AC RID: 5292 RVA: 0x000C24BB File Offset: 0x000C06BB
		private Vector2 MeasureText(LocalizedString text)
		{
			return this.MeasureText(text.Value);
		}

		// Token: 0x060014AD RID: 5293 RVA: 0x000C24CC File Offset: 0x000C06CC
		private Vector2 MeasureText(string text)
		{
			if (this.Font == null)
			{
				return Vector2.Zero;
			}
			if (string.IsNullOrEmpty(text))
			{
				return this.Font.MeasureString(" ", false);
			}
			return this.Font.MeasureString(string.IsNullOrEmpty(text) ? " " : text, false);
		}

		// Token: 0x060014AE RID: 5294 RVA: 0x000C2527 File Offset: 0x000C0727
		protected override void SetAlpha(float a)
		{
			this.textColor = new Color(this.TextColor, a);
			if (this.hoverTextColor != null)
			{
				this.hoverTextColor = new Color?(new Color(this.hoverTextColor.Value, a));
			}
		}

		// Token: 0x060014AF RID: 5295 RVA: 0x000C2564 File Offset: 0x000C0764
		public void OverrideTextColor(Color color)
		{
			this.textColor = color;
			this.hoverTextColor = new Color?(color);
			this.selectedTextColor = color;
			this.disabledTextColor = color;
		}

		// Token: 0x060014B0 RID: 5296 RVA: 0x000C2588 File Offset: 0x000C0788
		public ImmutableArray<Vector2> GetAllCaretPositions()
		{
			string textDrawn = this.Censor ? this.CensoredText : this.Text.SanitizedValue;
			if (this.cachedCaretPositions.Any<Vector2>() && textDrawn == this.cachedCaretPositionsText)
			{
				return this.cachedCaretPositions;
			}
			float w = this.Wrap ? (((float)this.Rect.Width - this.Padding.X - this.Padding.Z) / this.TextScale) : float.PositiveInfinity;
			Vector2[] positions;
			string wrapped = this.Font.WrapText(textDrawn, w, out positions);
			int textWidth = (int)this.Font.MeasureString(wrapped, false).X;
			int alignmentXDiff = this.textAlignment.HasFlag(Alignment.Right) ? textWidth : (this.textAlignment.HasFlag(Alignment.Center) ? (textWidth / 2) : 0);
			this.cachedCaretPositions = (from p in positions
			select p - new Vector2((float)alignmentXDiff, 0f) into p
			select p * this.TextScale + this.TextPos - this.Origin * this.TextScale).ToImmutableArray<Vector2>();
			this.cachedCaretPositionsText = textDrawn;
			return this.cachedCaretPositions;
		}

		// Token: 0x060014B1 RID: 5297 RVA: 0x000C26C4 File Offset: 0x000C08C4
		public int GetCaretIndexFromScreenPos(in Vector2 pos)
		{
			Vector2 vector = pos - this.Rect.Location.ToVector2();
			return this.GetCaretIndexFromLocalPos(vector);
		}

		// Token: 0x060014B2 RID: 5298 RVA: 0x000C26FC File Offset: 0x000C08FC
		public int GetCaretIndexFromLocalPos(in Vector2 pos)
		{
			ImmutableArray<Vector2> positions = this.GetAllCaretPositions();
			if (positions.Length == 0)
			{
				return 0;
			}
			float closestXDist = float.PositiveInfinity;
			float closestYDist = float.PositiveInfinity;
			int closestIndex = -1;
			for (int i = 0; i < positions.Length; i++)
			{
				float xDist = Math.Abs(pos.X - positions[i].X);
				float yDist = Math.Abs(pos.Y - (positions[i].Y + this.Font.LineHeight * 0.5f));
				if (yDist < closestYDist || (MathUtils.NearlyEqual(yDist, closestYDist, 0.0001f) && xDist < closestXDist))
				{
					closestIndex = i;
					closestXDist = xDist;
					closestYDist = yDist;
				}
			}
			if (closestIndex < 0)
			{
				return this.Text.Length;
			}
			return closestIndex;
		}

		// Token: 0x060014B3 RID: 5299 RVA: 0x000C27BC File Offset: 0x000C09BC
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.ClickableAreas.Any<GUITextBlock.ClickableArea>())
			{
				GUIComponent mouseOn = GUI.MouseOn;
				if (mouseOn == null || mouseOn.IsParentOf(this, true) || GUI.MouseOn == this)
				{
					if (!this.Rect.Contains(PlayerInput.MousePosition))
					{
						return;
					}
					Vector2 mousePosition = PlayerInput.MousePosition;
					int index = this.GetCaretIndexFromScreenPos(mousePosition);
					foreach (GUITextBlock.ClickableArea clickableArea in this.ClickableAreas)
					{
						if (clickableArea.Data.StartIndex <= index && index <= clickableArea.Data.EndIndex)
						{
							GUI.MouseCursor = CursorState.Hand;
							if (PlayerInput.PrimaryMouseButtonClicked())
							{
								GUITextBlock.ClickableArea.OnClickDelegate onClick = clickableArea.OnClick;
								if (onClick != null)
								{
									onClick(this, clickableArea);
								}
							}
							if (!PlayerInput.SecondaryMouseButtonClicked())
							{
								break;
							}
							GUITextBlock.ClickableArea.OnClickDelegate onSecondaryClick = clickableArea.OnSecondaryClick;
							if (onSecondaryClick == null)
							{
								break;
							}
							onSecondaryClick(this, clickableArea);
							break;
						}
					}
				}
			}
		}

		// Token: 0x060014B4 RID: 5300 RVA: 0x000C28C4 File Offset: 0x000C0AC4
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			Color currColor = this.GetColor(this.State);
			Rectangle rect = this.Rect;
			base.Draw(spriteBatch);
			if (this.TextGetter != null)
			{
				this.Text = this.TextGetter();
			}
			string textToShow = this.Censor ? this.censoredText : (this.Wrap ? this.wrappedText.Value : this.text.SanitizedValue);
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (this.overflowClipActive)
			{
				Rectangle scissorRect = new Rectangle(rect.X + (int)this.padding.X, rect.Y, rect.Width - (int)this.padding.X - (int)this.padding.Z, rect.Height);
				if (!scissorRect.Intersects(prevScissorRect))
				{
					return;
				}
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, scissorRect);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			if (!string.IsNullOrEmpty(textToShow))
			{
				Vector2 pos = rect.Location.ToVector2() + this.textPos + this.TextOffset;
				if (this.RoundToNearestPixel)
				{
					pos.X = (float)((int)pos.X);
					pos.Y = (float)((int)pos.Y);
				}
				GUIComponent.ComponentState state = this.State;
				bool flag = state == GUIComponent.ComponentState.Hover || state == GUIComponent.ComponentState.HoverSelected;
				Color currentTextColor = flag ? this.HoverTextColor : this.TextColor;
				if (!this.enabled)
				{
					currentTextColor = this.disabledTextColor;
				}
				else if (this.State == GUIComponent.ComponentState.Selected)
				{
					currentTextColor = this.selectedTextColor;
				}
				if (!this.HasColorHighlight)
				{
					Color colorToShow = currentTextColor * ((float)currentTextColor.A / 255f);
					if (TextManager.DebugDraw && (!this.text.NestedStr.Loaded || this.text.NestedStr.Language == LanguageIdentifier.None))
					{
						colorToShow = Color.Magenta;
					}
					if (this.Shadow)
					{
						Vector2 shadowOffset = new Vector2((float)Math.Max(GUI.IntScale(2f), 1));
						this.Font.DrawString(spriteBatch, textToShow, pos + shadowOffset, Color.Black, 0f, this.origin, this.TextScale, SpriteEffects.None, this.textDepth, this.textAlignment, this.ForceUpperCase);
					}
					this.Font.DrawString(spriteBatch, textToShow, pos, colorToShow, 0f, this.origin, this.TextScale, SpriteEffects.None, this.textDepth, this.textAlignment, this.ForceUpperCase);
				}
				else
				{
					ImmutableArray<RichTextData>? richTextData;
					if (this.OverrideRichTextDataAlpha)
					{
						richTextData = this.RichTextData;
						if (richTextData != null)
						{
							richTextData.GetValueOrDefault().ForEach(delegate(RichTextData rt)
							{
								rt.Alpha = (float)currentTextColor.A / 255f;
							});
						}
					}
					GUIFont font = this.Font;
					string text = textToShow;
					Vector2 position = pos;
					Color color = currentTextColor * ((float)currentTextColor.A / 255f);
					float rotation = 0f;
					Vector2 vector = this.origin;
					float scale = this.TextScale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					float layerDepth = this.textDepth;
					richTextData = this.RichTextData;
					font.DrawStringWithColors(spriteBatch, text, position, color, rotation, vector, scale, spriteEffects, layerDepth, richTextData, 0, this.textAlignment, this.ForceUpperCase);
				}
				GUITextBlock.StrikethroughSettings strikethrough = this.Strikethrough;
				if (strikethrough != null)
				{
					strikethrough.Draw(spriteBatch, (float)((int)Math.Ceiling((double)(this.TextSize.X / 2f))), pos.X, (this.ForceUpperCase == ForceUpperCase.Yes) ? pos.Y : (pos.Y + GUI.Scale * 2f));
				}
			}
			if (this.overflowClipActive)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			if ((float)(this.OutlineColor.A * currColor.A) > 0f)
			{
				GUI.DrawRectangle(spriteBatch, rect, this.OutlineColor * ((float)currColor.A / 255f), false, 0f, 1f);
			}
		}

		// Token: 0x060014B5 RID: 5301 RVA: 0x000C2D18 File Offset: 0x000C0F18
		public static void AutoScaleAndNormalize(params GUITextBlock[] textBlocks)
		{
			GUITextBlock.AutoScaleAndNormalize(textBlocks.AsEnumerable<GUITextBlock>(), true, false, null);
		}

		// Token: 0x060014B6 RID: 5302 RVA: 0x000C2D3C File Offset: 0x000C0F3C
		public static void AutoScaleAndNormalize(bool scaleHorizontal = true, bool scaleVertical = false, params GUITextBlock[] textBlocks)
		{
			GUITextBlock.AutoScaleAndNormalize(textBlocks.AsEnumerable<GUITextBlock>(), scaleHorizontal, scaleVertical, null);
		}

		// Token: 0x060014B7 RID: 5303 RVA: 0x000C2D60 File Offset: 0x000C0F60
		public static void AutoScaleAndNormalize(IEnumerable<GUITextBlock> textBlocks, bool scaleHorizontal = true, bool scaleVertical = false, float? defaultScale = null)
		{
			if (!textBlocks.Any<GUITextBlock>())
			{
				return;
			}
			float minScale = Math.Max(textBlocks.First<GUITextBlock>().TextScale, 1f);
			foreach (GUITextBlock textBlock in textBlocks)
			{
				if (defaultScale != null)
				{
					textBlock.TextScale = defaultScale.Value;
				}
				textBlock.AutoScaleHorizontal = scaleHorizontal;
				textBlock.AutoScaleVertical = scaleVertical;
				minScale = Math.Min(textBlock.TextScale, minScale);
			}
			foreach (GUITextBlock textBlock2 in textBlocks)
			{
				textBlock2.AutoScaleHorizontal = false;
				textBlock2.AutoScaleVertical = false;
				textBlock2.TextScale = minScale;
			}
		}

		// Token: 0x04000A64 RID: 2660
		protected RichString text;

		// Token: 0x04000A65 RID: 2661
		protected Alignment textAlignment;

		// Token: 0x04000A66 RID: 2662
		private float textScale = 1f;

		// Token: 0x04000A67 RID: 2663
		protected Vector2 textPos;

		// Token: 0x04000A68 RID: 2664
		protected Vector2 origin;

		// Token: 0x04000A69 RID: 2665
		protected Color textColor;

		// Token: 0x04000A6A RID: 2666
		protected Color disabledTextColor;

		// Token: 0x04000A6B RID: 2667
		protected Color selectedTextColor;

		// Token: 0x04000A6C RID: 2668
		private LocalizedString wrappedText;

		// Token: 0x04000A6D RID: 2669
		private string censoredText;

		// Token: 0x04000A6E RID: 2670
		public GUITextBlock.TextGetterHandler TextGetter;

		// Token: 0x04000A6F RID: 2671
		public bool Wrap;

		// Token: 0x04000A70 RID: 2672
		public bool RoundToNearestPixel = true;

		// Token: 0x04000A71 RID: 2673
		private bool overflowClipActive;

		// Token: 0x04000A72 RID: 2674
		public bool OverflowClip;

		// Token: 0x04000A73 RID: 2675
		private float textDepth;

		// Token: 0x04000A75 RID: 2677
		private Vector4 padding;

		// Token: 0x04000A76 RID: 2678
		private bool autoScaleHorizontal;

		// Token: 0x04000A77 RID: 2679
		private bool autoScaleVertical;

		// Token: 0x04000A78 RID: 2680
		private ForceUpperCase forceUpperCase;

		// Token: 0x04000A7A RID: 2682
		private Color? hoverTextColor;

		// Token: 0x04000A7C RID: 2684
		public GUITextBlock.StrikethroughSettings Strikethrough;

		// Token: 0x04000A7D RID: 2685
		public bool OverrideRichTextDataAlpha = true;

		// Token: 0x04000A80 RID: 2688
		private ImmutableArray<Vector2> cachedCaretPositions = ImmutableArray<Vector2>.Empty;

		// Token: 0x04000A81 RID: 2689
		private string cachedCaretPositionsText;

		// Token: 0x0200097C RID: 2428
		// (Invoke) Token: 0x06007203 RID: 29187
		public delegate LocalizedString TextGetterHandler();

		// Token: 0x0200097D RID: 2429
		public class StrikethroughSettings
		{
			// Token: 0x17001A5C RID: 6748
			// (get) Token: 0x06007206 RID: 29190 RVA: 0x0036C0BC File Offset: 0x0036A2BC
			// (set) Token: 0x06007207 RID: 29191 RVA: 0x0036C0C4 File Offset: 0x0036A2C4
			public Color Color { get; set; } = GUIStyle.Red;

			// Token: 0x06007208 RID: 29192 RVA: 0x0036C0CD File Offset: 0x0036A2CD
			public StrikethroughSettings(Color? color = null, int thickness = 1, int expand = 0)
			{
				if (color != null)
				{
					this.Color = color.Value;
				}
				this.thickness = thickness;
				this.expand = expand;
			}

			// Token: 0x06007209 RID: 29193 RVA: 0x0036C109 File Offset: 0x0036A309
			public void Draw(SpriteBatch spriteBatch, float textSizeHalf, float xPos, float yPos)
			{
				spriteBatch.DrawLine(new Vector2(xPos - textSizeHalf - (float)this.expand, yPos), new Vector2(xPos + textSizeHalf + (float)this.expand, yPos), this.Color, (float)this.thickness);
			}

			// Token: 0x04004172 RID: 16754
			private int thickness;

			// Token: 0x04004173 RID: 16755
			private int expand;
		}

		// Token: 0x0200097E RID: 2430
		public struct ClickableArea
		{
			// Token: 0x04004174 RID: 16756
			public RichTextData Data;

			// Token: 0x04004175 RID: 16757
			public GUITextBlock.ClickableArea.OnClickDelegate OnClick;

			// Token: 0x04004176 RID: 16758
			public GUITextBlock.ClickableArea.OnClickDelegate OnSecondaryClick;

			// Token: 0x0200152F RID: 5423
			// (Invoke) Token: 0x06009CEC RID: 40172
			public delegate void OnClickDelegate(GUITextBlock textBlock, GUITextBlock.ClickableArea area);
		}
	}
}
