using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200007E RID: 126
	public class GUIButton : GUIComponent
	{
		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x060011C0 RID: 4544 RVA: 0x000B14DD File Offset: 0x000AF6DD
		public GUITextBlock TextBlock
		{
			get
			{
				return this.textBlock;
			}
		}

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x060011C1 RID: 4545 RVA: 0x000B14E5 File Offset: 0x000AF6E5
		public GUIFrame Frame
		{
			get
			{
				return this.frame;
			}
		}

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x060011C2 RID: 4546 RVA: 0x000B14ED File Offset: 0x000AF6ED
		// (set) Token: 0x060011C3 RID: 4547 RVA: 0x000B14F8 File Offset: 0x000AF6F8
		public override bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				if (value == this.enabled)
				{
					return;
				}
				GUIComponent guicomponent = this.frame;
				this.textBlock.Enabled = value;
				guicomponent.Enabled = value;
				this.enabled = value;
			}
		}

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x060011C4 RID: 4548 RVA: 0x000B1532 File Offset: 0x000AF732
		// (set) Token: 0x060011C5 RID: 4549 RVA: 0x000B153A File Offset: 0x000AF73A
		public override Color Color
		{
			get
			{
				return base.Color;
			}
			set
			{
				base.Color = value;
				this.frame.Color = value;
			}
		}

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x060011C6 RID: 4550 RVA: 0x000B154F File Offset: 0x000AF74F
		// (set) Token: 0x060011C7 RID: 4551 RVA: 0x000B1557 File Offset: 0x000AF757
		public override Color HoverColor
		{
			get
			{
				return base.HoverColor;
			}
			set
			{
				base.HoverColor = value;
				this.frame.HoverColor = value;
			}
		}

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x060011C8 RID: 4552 RVA: 0x000B156C File Offset: 0x000AF76C
		// (set) Token: 0x060011C9 RID: 4553 RVA: 0x000B1574 File Offset: 0x000AF774
		public override Color SelectedColor
		{
			get
			{
				return base.SelectedColor;
			}
			set
			{
				base.SelectedColor = value;
				this.frame.SelectedColor = value;
			}
		}

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x060011CA RID: 4554 RVA: 0x000B1589 File Offset: 0x000AF789
		// (set) Token: 0x060011CB RID: 4555 RVA: 0x000B1591 File Offset: 0x000AF791
		public override Color PressedColor
		{
			get
			{
				return base.PressedColor;
			}
			set
			{
				base.PressedColor = value;
				this.frame.PressedColor = value;
			}
		}

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x060011CC RID: 4556 RVA: 0x000B15A6 File Offset: 0x000AF7A6
		// (set) Token: 0x060011CD RID: 4557 RVA: 0x000B15AE File Offset: 0x000AF7AE
		public override Color OutlineColor
		{
			get
			{
				return base.OutlineColor;
			}
			set
			{
				base.OutlineColor = value;
				if (this.frame != null)
				{
					this.frame.OutlineColor = value;
				}
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x060011CE RID: 4558 RVA: 0x000B15CB File Offset: 0x000AF7CB
		// (set) Token: 0x060011CF RID: 4559 RVA: 0x000B15D8 File Offset: 0x000AF7D8
		public Color TextColor
		{
			get
			{
				return this.textBlock.TextColor;
			}
			set
			{
				this.textBlock.TextColor = value;
			}
		}

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x060011D0 RID: 4560 RVA: 0x000B15E6 File Offset: 0x000AF7E6
		// (set) Token: 0x060011D1 RID: 4561 RVA: 0x000B15F3 File Offset: 0x000AF7F3
		public Color HoverTextColor
		{
			get
			{
				return this.textBlock.HoverTextColor;
			}
			set
			{
				this.textBlock.HoverTextColor = value;
			}
		}

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x060011D2 RID: 4562 RVA: 0x000B1601 File Offset: 0x000AF801
		// (set) Token: 0x060011D3 RID: 4563 RVA: 0x000B160E File Offset: 0x000AF80E
		public Color SelectedTextColor
		{
			get
			{
				return this.textBlock.SelectedTextColor;
			}
			set
			{
				this.textBlock.SelectedTextColor = value;
			}
		}

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x060011D4 RID: 4564 RVA: 0x000B161C File Offset: 0x000AF81C
		public Color DisabledTextColor
		{
			get
			{
				return this.textBlock.DisabledTextColor;
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x060011D5 RID: 4565 RVA: 0x000B1629 File Offset: 0x000AF829
		public override float FlashTimer
		{
			get
			{
				return this.Frame.FlashTimer;
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x060011D6 RID: 4566 RVA: 0x000B1636 File Offset: 0x000AF836
		// (set) Token: 0x060011D7 RID: 4567 RVA: 0x000B1651 File Offset: 0x000AF851
		public override GUIFont Font
		{
			get
			{
				if (this.textBlock != null)
				{
					return this.textBlock.Font;
				}
				return GUIStyle.Font;
			}
			set
			{
				base.Font = value;
				if (this.textBlock != null)
				{
					this.textBlock.Font = value;
				}
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x060011D8 RID: 4568 RVA: 0x000B166E File Offset: 0x000AF86E
		// (set) Token: 0x060011D9 RID: 4569 RVA: 0x000B1680 File Offset: 0x000AF880
		public LocalizedString Text
		{
			get
			{
				return this.textBlock.Text;
			}
			set
			{
				this.textBlock.Text = value;
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x060011DA RID: 4570 RVA: 0x000B1693 File Offset: 0x000AF893
		// (set) Token: 0x060011DB RID: 4571 RVA: 0x000B16A0 File Offset: 0x000AF8A0
		public ForceUpperCase ForceUpperCase
		{
			get
			{
				return this.textBlock.ForceUpperCase;
			}
			set
			{
				this.textBlock.ForceUpperCase = value;
			}
		}

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x060011DC RID: 4572 RVA: 0x000B16AE File Offset: 0x000AF8AE
		// (set) Token: 0x060011DD RID: 4573 RVA: 0x000B16B6 File Offset: 0x000AF8B6
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				base.ToolTip = value;
				this.textBlock.ToolTip = value;
			}
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x060011DE RID: 4574 RVA: 0x000B16CB File Offset: 0x000AF8CB
		// (set) Token: 0x060011DF RID: 4575 RVA: 0x000B16D4 File Offset: 0x000AF8D4
		public bool RequireHold
		{
			get
			{
				return this.requireHold;
			}
			set
			{
				this.requireHold = value;
				if (value)
				{
					if (this.holdOverlay == null)
					{
						this.holdOverlay = new GUIFrame(new RectTransform(new Vector2(0.5f, 1f), this.Frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null)
						{
							Color = GUIStyle.Yellow * 0.33f,
							CanBeFocused = false,
							IgnoreLayoutGroups = true,
							Visible = true
						};
						return;
					}
				}
				else if (this.holdOverlay != null)
				{
					this.holdOverlay.Visible = false;
				}
			}
		}

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x060011E0 RID: 4576 RVA: 0x000B1789 File Offset: 0x000AF989
		// (set) Token: 0x060011E1 RID: 4577 RVA: 0x000B1791 File Offset: 0x000AF991
		public float HoldDurationSeconds { get; set; } = 5f;

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x060011E2 RID: 4578 RVA: 0x000B179A File Offset: 0x000AF99A
		// (set) Token: 0x060011E3 RID: 4579 RVA: 0x000B17A2 File Offset: 0x000AF9A2
		public bool Pulse { get; set; }

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x060011E4 RID: 4580 RVA: 0x000B17AB File Offset: 0x000AF9AB
		// (set) Token: 0x060011E5 RID: 4581 RVA: 0x000B17B3 File Offset: 0x000AF9B3
		public GUISoundType ClickSound { get; set; } = GUISoundType.Select;

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x060011E6 RID: 4582 RVA: 0x000B17BC File Offset: 0x000AF9BC
		// (set) Token: 0x060011E7 RID: 4583 RVA: 0x000B17C4 File Offset: 0x000AF9C4
		public override bool PlaySoundOnSelect { get; set; } = true;

		// Token: 0x060011E8 RID: 4584 RVA: 0x000B17CD File Offset: 0x000AF9CD
		public GUIButton(RectTransform rectT, Alignment textAlignment = Alignment.Center, string style = "", Color? color = null) : this(rectT, LocalizedString.EmptyString, textAlignment, style, color)
		{
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x000B17E0 File Offset: 0x000AF9E0
		public GUIButton(RectTransform rectT, LocalizedString text, Alignment textAlignment = Alignment.Center, string style = "", Color? color = null) : base(style, rectT)
		{
			this.CanBeFocused = true;
			this.HoverCursor = CursorState.Hand;
			this.frame = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), style, null)
			{
				CanBeFocused = false
			};
			if (style != null)
			{
				GUIStyle.Apply(this.frame, (style == "") ? "GUIButton" : style, null);
			}
			if (color != null)
			{
				this.color = (this.frame.Color = color.Value);
			}
			GUIComponentStyle selfStyle = base.Style;
			this.textBlock = new GUITextBlock(new RectTransform(Vector2.One, rectT, Anchor.Center, null, null, null, ScaleBasis.Normal), RichString.Rich(text, null), null, null, textAlignment, false, null, null)
			{
				TextColor = ((selfStyle != null) ? selfStyle.TextColor : Color.Black),
				HoverTextColor = ((selfStyle != null) ? selfStyle.HoverTextColor : Color.Black),
				SelectedTextColor = ((selfStyle != null) ? selfStyle.SelectedTextColor : Color.Black),
				CanBeFocused = false
			};
			if (rectT.Rect.Height == 0 && !text.IsNullOrEmpty())
			{
				base.RectTransform.Resize(new Point(base.RectTransform.Rect.Width, (int)this.Font.MeasureString(this.textBlock.Text, false).Y), true);
				RectTransform rectTransform = base.RectTransform;
				RectTransform rectTransform2 = this.textBlock.RectTransform;
				Point minSize = new Point(0, Math.Max(rectT.MinSize.Y, this.Rect.Height));
				rectTransform2.MinSize = minSize;
				rectTransform.MinSize = minSize;
				this.TextBlock.SetTextPos();
			}
			GUIStyle.Apply(this.textBlock, "", this);
			this.Enabled = true;
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x000B1A1D File Offset: 0x000AFC1D
		public override void ApplyStyle(GUIComponentStyle style)
		{
			base.ApplyStyle(style);
			GUIFrame guiframe = this.frame;
			if (guiframe == null)
			{
				return;
			}
			guiframe.ApplyStyle(style);
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x000B1A37 File Offset: 0x000AFC37
		public override void Flash(Color? color = null, float flashDuration = 1.5f, bool useRectangleFlash = false, bool useCircularFlash = false, Vector2? flashRectInflate = null)
		{
			this.Frame.Flash(color, flashDuration, useRectangleFlash, useCircularFlash, flashRectInflate);
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x000B1A4C File Offset: 0x000AFC4C
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (this.Pulse && this.pulseTimer > 1f)
			{
				Rectangle expandRect = this.Rect;
				float expand = this.pulseExpand * 20f * GUI.Scale;
				expandRect.Inflate(expand, expand);
				GUIStyle.EndRoundButtonPulse.Draw(spriteBatch, expandRect, ToolBox.GradientLerp(this.pulseExpand, new Color[]
				{
					Color.White,
					Color.White,
					Color.Transparent
				}), SpriteEffects.None);
			}
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000B1AD4 File Offset: 0x000AFCD4
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			base.Update(deltaTime);
			if (this.Rect.Contains(PlayerInput.MousePosition) && this.CanBeSelected && this.CanBeFocused && this.Enabled && GUI.IsMouseOn(this))
			{
				this.State = (this.Selected ? GUIComponent.ComponentState.HoverSelected : GUIComponent.ComponentState.Hover);
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					GUIButton.OnButtonDownHandler onButtonDown = this.OnButtonDown;
					if (onButtonDown != null)
					{
						onButtonDown();
					}
				}
				if (PlayerInput.PrimaryMouseButtonHeld())
				{
					if (this.RequireHold)
					{
						this.holdTimer += deltaTime;
					}
					if (this.OnPressed != null)
					{
						if (this.OnPressed())
						{
							this.State = GUIComponent.ComponentState.Pressed;
						}
					}
					else
					{
						this.State = GUIComponent.ComponentState.Pressed;
					}
				}
				else if (PlayerInput.PrimaryMouseButtonClicked())
				{
					if (!this.RequireHold || this.holdTimer > this.HoldDurationSeconds)
					{
						if (this.PlaySoundOnSelect)
						{
							SoundPlayer.PlayUISound(this.ClickSound);
						}
						if (this.OnClicked != null)
						{
							if (this.OnClicked(this, this.UserData))
							{
								this.State = GUIComponent.ComponentState.Selected;
							}
						}
						else
						{
							this.Selected = !this.Selected;
						}
					}
				}
				else
				{
					this.holdTimer = 0f;
				}
			}
			else
			{
				this.holdTimer = 0f;
				if (!this.ExternalHighlight)
				{
					this.State = (this.Selected ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None);
				}
				else
				{
					this.State = GUIComponent.ComponentState.Hover;
				}
			}
			if (this.RequireHold)
			{
				float width = MathHelper.Clamp(this.holdTimer / this.HoldDurationSeconds, 0f, 1f);
				if (!MathUtils.NearlyEqual(width, this.holdOverlay.RectTransform.RelativeSize.X, 0.0001f))
				{
					this.holdOverlay.RectTransform.RelativeSize = new Vector2(width, 1f);
				}
				this.holdOverlay.Color = ((this.holdTimer >= this.HoldDurationSeconds) ? (Color.Green * 0.33f) : (Color.Red * 0.33f));
			}
			foreach (GUIComponent child in base.Children)
			{
				child.State = this.State;
			}
			if (this.Pulse)
			{
				this.pulseTimer += deltaTime;
				if (this.pulseTimer > 1f)
				{
					if (!this.flashed)
					{
						this.flashed = true;
						this.Frame.Flash(new Color?(Color.White * 0.2f), 0.8f, true, false, null);
					}
					this.pulseExpand += deltaTime;
					if (this.pulseExpand > 1f)
					{
						this.pulseTimer = 0f;
						this.pulseExpand = 0f;
						this.flashed = false;
					}
				}
			}
		}

		// Token: 0x040008E4 RID: 2276
		protected GUITextBlock textBlock;

		// Token: 0x040008E5 RID: 2277
		protected GUIFrame frame;

		// Token: 0x040008E6 RID: 2278
		public GUIButton.OnClickedHandler OnClicked;

		// Token: 0x040008E7 RID: 2279
		public GUIButton.OnPressedHandler OnPressed;

		// Token: 0x040008E8 RID: 2280
		public GUIButton.OnButtonDownHandler OnButtonDown;

		// Token: 0x040008E9 RID: 2281
		public bool CanBeSelected = true;

		// Token: 0x040008EA RID: 2282
		private GUIComponent holdOverlay;

		// Token: 0x040008EB RID: 2283
		private bool requireHold;

		// Token: 0x040008ED RID: 2285
		private float holdTimer;

		// Token: 0x040008EF RID: 2287
		private float pulseTimer;

		// Token: 0x040008F0 RID: 2288
		private float pulseExpand;

		// Token: 0x040008F1 RID: 2289
		private bool flashed;

		// Token: 0x02000931 RID: 2353
		// (Invoke) Token: 0x0600711E RID: 28958
		public delegate bool OnClickedHandler(GUIButton button, object obj);

		// Token: 0x02000932 RID: 2354
		// (Invoke) Token: 0x06007122 RID: 28962
		public delegate bool OnPressedHandler();

		// Token: 0x02000933 RID: 2355
		// (Invoke) Token: 0x06007126 RID: 28966
		public delegate bool OnButtonDownHandler();
	}
}
