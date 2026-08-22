using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using EventInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000A5 RID: 165
	public class GUITextBox : GUIComponent, IKeyboardSubscriber
	{
		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060014BC RID: 5308 RVA: 0x000C2E3C File Offset: 0x000C103C
		// (remove) Token: 0x060014BD RID: 5309 RVA: 0x000C2E74 File Offset: 0x000C1074
		public event TextBoxEvent OnSelected;

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060014BE RID: 5310 RVA: 0x000C2EAC File Offset: 0x000C10AC
		// (remove) Token: 0x060014BF RID: 5311 RVA: 0x000C2EE4 File Offset: 0x000C10E4
		public event TextBoxEvent OnDeselected;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x060014C0 RID: 5312 RVA: 0x000C2F1C File Offset: 0x000C111C
		// (remove) Token: 0x060014C1 RID: 5313 RVA: 0x000C2F54 File Offset: 0x000C1154
		public event TextBoxEvent OnKeyHit;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x060014C2 RID: 5314 RVA: 0x000C2F8C File Offset: 0x000C118C
		// (remove) Token: 0x060014C3 RID: 5315 RVA: 0x000C2FC4 File Offset: 0x000C11C4
		public event GUITextBox.OnTextChangedHandler OnTextChanged;

		// Token: 0x1700054D RID: 1357
		// (set) Token: 0x060014C4 RID: 5316 RVA: 0x000C2FFC File Offset: 0x000C11FC
		public GUITextBox.OnTextChangedHandler OnTextChangedDelegate
		{
			set
			{
				this.OnTextChanged += ((GUITextBox a, string b) => value(a, b));
			}
		}

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x060014C5 RID: 5317 RVA: 0x000C3028 File Offset: 0x000C1228
		// (set) Token: 0x060014C6 RID: 5318 RVA: 0x000C3030 File Offset: 0x000C1230
		public bool CaretEnabled { get; set; }

		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x060014C7 RID: 5319 RVA: 0x000C3039 File Offset: 0x000C1239
		// (set) Token: 0x060014C8 RID: 5320 RVA: 0x000C3041 File Offset: 0x000C1241
		public Color? CaretColor { get; set; }

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x060014C9 RID: 5321 RVA: 0x000C304A File Offset: 0x000C124A
		// (set) Token: 0x060014CA RID: 5322 RVA: 0x000C3052 File Offset: 0x000C1252
		public int CaretIndex
		{
			get
			{
				return this._caretIndex;
			}
			set
			{
				if (value >= 0)
				{
					this._caretIndex = value;
					this.caretPosDirty = true;
				}
			}
		}

		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x060014CB RID: 5323 RVA: 0x000C3068 File Offset: 0x000C1268
		public Vector2 CaretScreenPos
		{
			get
			{
				return this.Rect.Location.ToVector2() + this.caretPos;
			}
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x060014CC RID: 5324 RVA: 0x000C3096 File Offset: 0x000C1296
		private bool IsLeftToRight
		{
			get
			{
				return this.selectionStartIndex <= this.selectionEndIndex;
			}
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x060014CD RID: 5325 RVA: 0x000C30A9 File Offset: 0x000C12A9
		public GUIFrame Frame
		{
			get
			{
				return this.frame;
			}
		}

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x060014CE RID: 5326 RVA: 0x000C30B1 File Offset: 0x000C12B1
		// (set) Token: 0x060014CF RID: 5327 RVA: 0x000C30BE File Offset: 0x000C12BE
		public GUITextBlock.TextGetterHandler TextGetter
		{
			get
			{
				return this.textBlock.TextGetter;
			}
			set
			{
				this.textBlock.TextGetter = value;
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x060014D0 RID: 5328 RVA: 0x000C30CC File Offset: 0x000C12CC
		// (set) Token: 0x060014D1 RID: 5329 RVA: 0x000C30D4 File Offset: 0x000C12D4
		public new bool Selected
		{
			get
			{
				return this.selected;
			}
			set
			{
				if (!this.selected && value)
				{
					this.Select(-1, false);
					return;
				}
				if (this.selected && !value)
				{
					this.Deselect();
				}
			}
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x060014D2 RID: 5330 RVA: 0x000C30FD File Offset: 0x000C12FD
		// (set) Token: 0x060014D3 RID: 5331 RVA: 0x000C310A File Offset: 0x000C130A
		public bool Wrap
		{
			get
			{
				return this.textBlock.Wrap;
			}
			set
			{
				this.textBlock.Wrap = value;
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x060014D4 RID: 5332 RVA: 0x000C3118 File Offset: 0x000C1318
		public GUITextBlock TextBlock
		{
			get
			{
				return this.textBlock;
			}
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x060014D5 RID: 5333 RVA: 0x000C3120 File Offset: 0x000C1320
		// (set) Token: 0x060014D6 RID: 5334 RVA: 0x000C3128 File Offset: 0x000C1328
		public bool ClampText { get; set; }

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x060014D7 RID: 5335 RVA: 0x000C3131 File Offset: 0x000C1331
		// (set) Token: 0x060014D8 RID: 5336 RVA: 0x000C313C File Offset: 0x000C133C
		public int? MaxTextLength
		{
			get
			{
				return this.maxTextLength;
			}
			set
			{
				this.textBlock.OverflowClip = (value != null);
				this.maxTextLength = value;
				int length = this.Text.Length;
				int? num = this.MaxTextLength;
				if (length > num.GetValueOrDefault() & num != null)
				{
					this.SetText(this.Text.Substring(0, this.maxTextLength.Value), true);
				}
			}
		}

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x060014D9 RID: 5337 RVA: 0x000C31A6 File Offset: 0x000C13A6
		// (set) Token: 0x060014DA RID: 5338 RVA: 0x000C31B3 File Offset: 0x000C13B3
		public bool OverflowClip
		{
			get
			{
				return this.textBlock.OverflowClip;
			}
			set
			{
				this.textBlock.OverflowClip = value;
			}
		}

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x060014DB RID: 5339 RVA: 0x000C31C1 File Offset: 0x000C13C1
		// (set) Token: 0x060014DC RID: 5340 RVA: 0x000C31CC File Offset: 0x000C13CC
		public override bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				GUIComponent guicomponent = this.frame;
				this.textBlock.Enabled = value;
				guicomponent.Enabled = value;
				this.enabled = value;
				if (this.icon != null)
				{
					this.icon.Enabled = value;
				}
				if (!this.enabled && this.Selected)
				{
					this.Deselect();
				}
			}
		}

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x060014DD RID: 5341 RVA: 0x000C3226 File Offset: 0x000C1426
		// (set) Token: 0x060014DE RID: 5342 RVA: 0x000C3233 File Offset: 0x000C1433
		public bool Censor
		{
			get
			{
				return this.textBlock.Censor;
			}
			set
			{
				this.textBlock.Censor = value;
			}
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x060014DF RID: 5343 RVA: 0x000C3241 File Offset: 0x000C1441
		// (set) Token: 0x060014E0 RID: 5344 RVA: 0x000C324C File Offset: 0x000C144C
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				GUIComponent guicomponent = this.textBlock;
				this.caretAndSelectionRenderer.ToolTip = value;
				guicomponent.ToolTip = value;
				base.ToolTip = value;
			}
		}

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x060014E1 RID: 5345 RVA: 0x000C327C File Offset: 0x000C147C
		// (set) Token: 0x060014E2 RID: 5346 RVA: 0x000C329A File Offset: 0x000C149A
		public override GUIFont Font
		{
			get
			{
				GUITextBlock guitextBlock = this.textBlock;
				return ((guitextBlock != null) ? guitextBlock.Font : null) ?? base.Font;
			}
			set
			{
				base.Font = value;
				if (this.textBlock == null)
				{
					return;
				}
				this.textBlock.Font = value;
				this.imePreviewTextHandler.Font = this.Font;
			}
		}

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x060014E3 RID: 5347 RVA: 0x000C32C9 File Offset: 0x000C14C9
		// (set) Token: 0x060014E4 RID: 5348 RVA: 0x000C32D1 File Offset: 0x000C14D1
		public override Color Color
		{
			get
			{
				return this.color;
			}
			set
			{
				this.color = value;
				this.textBlock.Color = this.color;
			}
		}

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x060014E5 RID: 5349 RVA: 0x000C32EB File Offset: 0x000C14EB
		// (set) Token: 0x060014E6 RID: 5350 RVA: 0x000C32F8 File Offset: 0x000C14F8
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

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x060014E7 RID: 5351 RVA: 0x000C3306 File Offset: 0x000C1506
		// (set) Token: 0x060014E8 RID: 5352 RVA: 0x000C330E File Offset: 0x000C150E
		public override Color HoverColor
		{
			get
			{
				return base.HoverColor;
			}
			set
			{
				base.HoverColor = value;
				this.textBlock.HoverColor = value;
			}
		}

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x060014E9 RID: 5353 RVA: 0x000C3323 File Offset: 0x000C1523
		// (set) Token: 0x060014EA RID: 5354 RVA: 0x000C3330 File Offset: 0x000C1530
		public Vector4 Padding
		{
			get
			{
				return this.textBlock.Padding;
			}
			set
			{
				this.textBlock.Padding = value;
			}
		}

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x060014EB RID: 5355 RVA: 0x000C333E File Offset: 0x000C153E
		// (set) Token: 0x060014EC RID: 5356 RVA: 0x000C3346 File Offset: 0x000C1546
		public Color SelectionColor { get; set; } = Color.White * 0.25f;

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x060014ED RID: 5357 RVA: 0x000C334F File Offset: 0x000C154F
		// (set) Token: 0x060014EE RID: 5358 RVA: 0x000C3361 File Offset: 0x000C1561
		public string Text
		{
			get
			{
				return this.textBlock.Text.SanitizedValue;
			}
			set
			{
				this.SetText(value, false);
				this.CaretIndex = this.Text.Length;
				GUITextBox.OnTextChangedHandler onTextChanged = this.OnTextChanged;
				if (onTextChanged == null)
				{
					return;
				}
				onTextChanged(this, this.Text);
			}
		}

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x060014EF RID: 5359 RVA: 0x000C3395 File Offset: 0x000C1595
		public string WrappedText
		{
			get
			{
				return this.textBlock.WrappedText.Value;
			}
		}

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x060014F0 RID: 5360 RVA: 0x000C33A7 File Offset: 0x000C15A7
		// (set) Token: 0x060014F1 RID: 5361 RVA: 0x000C33AF File Offset: 0x000C15AF
		public bool Readonly { get; set; }

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x060014F2 RID: 5362 RVA: 0x000C33B8 File Offset: 0x000C15B8
		// (set) Token: 0x060014F3 RID: 5363 RVA: 0x000C33C0 File Offset: 0x000C15C0
		public override bool PlaySoundOnSelect { get; set; } = true;

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x060014F4 RID: 5364 RVA: 0x000C33CC File Offset: 0x000C15CC
		public bool IsIMEActive
		{
			get
			{
				IMEPreviewTextHandler imepreviewTextHandler = this.imePreviewTextHandler;
				return imepreviewTextHandler != null && imepreviewTextHandler.HasText;
			}
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x000C33EC File Offset: 0x000C15EC
		public GUITextBox(RectTransform rectT, string text = "", Color? textColor = null, GUIFont font = null, Alignment textAlignment = Alignment.Left, bool wrap = false, string style = "", Color? color = null, bool createClearButton = false, bool createPenIcon = true) : base(style, rectT)
		{
			this.HoverCursor = CursorState.IBeam;
			this.CanBeFocused = true;
			this.color = (color ?? Color.White);
			this.frame = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.Center, null, null, null, ScaleBasis.Normal), style, color);
			GUIStyle.Apply(this.frame, (style == "") ? "GUITextBox" : style, null);
			this.textBlock = new GUITextBlock(new RectTransform(Vector2.One, this.frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), text ?? "", textColor, font, textAlignment, wrap, "", null);
			this.imePreviewTextHandler = new IMEPreviewTextHandler(this.textBlock.Font);
			GUIStyle.Apply(this.textBlock, "", this);
			if (font != null)
			{
				this.textBlock.Font = font;
			}
			this.CaretEnabled = true;
			this.caretPosDirty = true;
			this.caretAndSelectionRenderer = new GUICustomComponent(new RectTransform(Vector2.One, this.frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawCaretAndSelection), null);
			int clearButtonWidth = 0;
			if (createClearButton)
			{
				GUIButton clearButton = new GUIButton(new RectTransform(new Vector2(0.6f, 0.6f), this.frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point(5, 0)
				}, Alignment.Center, "GUICancelButton", null)
				{
					OnClicked = delegate(GUIButton bt, object userdata)
					{
						this.Text = "";
						this.frame.Flash(new Color?(Color.White), 1.5f, false, false, null);
						return true;
					}
				};
				this.textBlock.RectTransform.MaxSize = new Point(this.frame.Rect.Width - clearButton.Rect.Height - clearButton.RectTransform.AbsoluteOffset.X * 2, int.MaxValue);
				clearButtonWidth = (int)((float)clearButton.Rect.Width * 1.2f);
			}
			GUIComponentStyle selfStyle = base.Style;
			if (selfStyle != null && selfStyle.ChildStyles.ContainsKey("textboxicon".ToIdentifier()) && createPenIcon)
			{
				this.icon = new GUIImage(new RectTransform(new Vector2(0.6f, 0.6f), this.frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point(5 + clearButtonWidth, 0)
				}, null, true);
				this.icon.ApplyStyle(base.Style.ChildStyles["textboxicon".ToIdentifier()]);
				this.textBlock.RectTransform.MaxSize = new Point(this.frame.Rect.Width - this.icon.Rect.Height - clearButtonWidth - this.icon.RectTransform.AbsoluteOffset.X * 2, int.MaxValue);
			}
			this.Font = this.textBlock.Font;
			this.Enabled = true;
			rectT.SizeChanged += delegate()
			{
				if (this.icon != null)
				{
					this.textBlock.RectTransform.MaxSize = new Point(this.frame.Rect.Width - this.icon.Rect.Height - this.icon.RectTransform.AbsoluteOffset.X * 2, int.MaxValue);
				}
				this.caretPosDirty = true;
			};
			rectT.ScaleChanged += delegate()
			{
				if (this.icon != null)
				{
					this.textBlock.RectTransform.MaxSize = new Point(this.frame.Rect.Width - this.icon.Rect.Height - this.icon.RectTransform.AbsoluteOffset.X * 2, int.MaxValue);
				}
				this.caretPosDirty = true;
			};
		}

		// Token: 0x060014F6 RID: 5366 RVA: 0x000C37D4 File Offset: 0x000C19D4
		private bool SetText(string text, bool store = true)
		{
			if (this.textFilterFunction != null)
			{
				text = this.textFilterFunction(text);
			}
			if (this.Text == text)
			{
				return false;
			}
			this.textBlock.Text = text;
			this.ClearSelection();
			if (this.Text == null)
			{
				this.textBlock.Text = "";
			}
			if (this.Text != "")
			{
				if (this.maxTextLength != null)
				{
					int length = this.textBlock.Text.Length;
					int? num = this.maxTextLength;
					if (length > num.GetValueOrDefault() & num != null)
					{
						this.textBlock.Text = this.Text.Substring(0, this.maxTextLength.Value);
					}
				}
				else if (!this.Wrap)
				{
					while (this.ClampText && this.textBlock.Text.Length > 0 && this.Font.MeasureString(this.textBlock.Text, false).X * this.TextBlock.TextScale > (float)((int)((float)this.textBlock.Rect.Width - this.textBlock.Padding.X - this.textBlock.Padding.Z)))
					{
						this.textBlock.Text = this.Text.Substring(0, this.textBlock.Text.Length - 1);
					}
				}
			}
			if (store)
			{
				this.memento.Store(this.Text);
			}
			return true;
		}

		// Token: 0x060014F7 RID: 5367 RVA: 0x000C3988 File Offset: 0x000C1B88
		private void CalculateCaretPos()
		{
			this.CaretIndex = Math.Clamp(this.CaretIndex, 0, this.textBlock.Text.Length);
			ImmutableArray<Vector2> caretPositions = this.textBlock.GetAllCaretPositions();
			if (this.CaretIndex >= caretPositions.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(104, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Caret index was outside the bounds of the calculated caret positions. Index: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.CaretIndex);
				defaultInterpolatedStringHandler.AppendLiteral(", caret positions: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(caretPositions.Length);
				defaultInterpolatedStringHandler.AppendLiteral(", text: ");
				defaultInterpolatedStringHandler.AppendFormatted<RichString>(this.textBlock.Text);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.caretPos = caretPositions[this.CaretIndex];
			this.caretPosDirty = false;
		}

		// Token: 0x060014F8 RID: 5368 RVA: 0x000C3A54 File Offset: 0x000C1C54
		public void Select(int forcedCaretIndex = -1, bool ignoreSelectSound = false)
		{
			this.skipUpdate = true;
			if (this.memento.Current == null)
			{
				this.memento.Store(this.Text);
			}
			int caretIndex;
			if (forcedCaretIndex != -1)
			{
				caretIndex = forcedCaretIndex;
			}
			else
			{
				GUITextBlock guitextBlock = this.textBlock;
				Vector2 mousePosition = PlayerInput.MousePosition;
				caretIndex = guitextBlock.GetCaretIndexFromScreenPos(mousePosition);
			}
			this.CaretIndex = caretIndex;
			this.CalculateCaretPos();
			this.ClearSelection();
			TextBoxEvent onSelected = this.OnSelected;
			if (onSelected != null)
			{
				onSelected(this, Keys.None);
			}
			if (!this.selected && this.PlaySoundOnSelect && !ignoreSelectSound)
			{
				SoundPlayer.PlayUISound(GUISoundType.Select);
			}
			this.selected = true;
			GUI.KeyboardDispatcher.Subscriber = this;
		}

		// Token: 0x060014F9 RID: 5369 RVA: 0x000C3AF0 File Offset: 0x000C1CF0
		public void Deselect()
		{
			this.memento.Clear();
			this.selected = false;
			if (GUI.KeyboardDispatcher.Subscriber == this)
			{
				GUI.KeyboardDispatcher.Subscriber = null;
			}
			TextBoxEvent onDeselected = this.OnDeselected;
			if (onDeselected != null)
			{
				onDeselected(this, Keys.None);
			}
			this.imePreviewTextHandler.Reset();
		}

		// Token: 0x060014FA RID: 5370 RVA: 0x000C3B45 File Offset: 0x000C1D45
		public override void Flash(Color? color = null, float flashDuration = 1.5f, bool useRectangleFlash = false, bool useCircularFlash = false, Vector2? flashRectOffset = null)
		{
			this.frame.Flash(color, flashDuration, useRectangleFlash, useCircularFlash, flashRectOffset);
		}

		// Token: 0x060014FB RID: 5371 RVA: 0x000C3B5C File Offset: 0x000C1D5C
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			if (this.flashTimer > 0f)
			{
				this.flashTimer -= deltaTime;
			}
			if (!this.Enabled)
			{
				return;
			}
			if (this.skipUpdate)
			{
				this.skipUpdate = false;
				return;
			}
			if ((this.MouseRect.Contains(PlayerInput.MousePosition) && (GUI.MouseOn == null || (!(GUI.MouseOn is GUIButton) && GUI.IsMouseOn(this)))) || this.isSelecting)
			{
				this.State = GUIComponent.ComponentState.Hover;
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					this.mouseHeldInside = true;
					this.Select(-1, false);
				}
				else
				{
					this.isSelecting = PlayerInput.PrimaryMouseButtonHeld();
				}
				if (PlayerInput.DoubleClicked())
				{
					this.SelectAll();
				}
				if (this.isSelecting && !MathUtils.NearlyEqual(PlayerInput.MouseSpeed.X, 0f, 0.0001f))
				{
					GUITextBlock guitextBlock = this.textBlock;
					Vector2 mousePosition = PlayerInput.MousePosition;
					this.CaretIndex = guitextBlock.GetCaretIndexFromScreenPos(mousePosition);
					this.CalculateCaretPos();
					this.CalculateSelection();
				}
			}
			else
			{
				if ((PlayerInput.PrimaryMouseButtonClicked() || PlayerInput.SecondaryMouseButtonClicked()) && this.selected)
				{
					if (!this.mouseHeldInside)
					{
						this.Deselect();
					}
					this.mouseHeldInside = false;
				}
				this.isSelecting = false;
				this.State = GUIComponent.ComponentState.None;
			}
			if (this.mouseHeldInside && !PlayerInput.PrimaryMouseButtonHeld())
			{
				this.mouseHeldInside = false;
			}
			if (this.CaretEnabled)
			{
				this.HandleCaretBoundsOverflow();
				this.caretTimer += deltaTime;
				this.caretVisible = (this.caretTimer * 1000f % 1000f < 500f);
				if (this.caretVisible && this.caretPosDirty)
				{
					this.CalculateCaretPos();
				}
			}
			if (GUI.KeyboardDispatcher.Subscriber == this)
			{
				this.State = GUIComponent.ComponentState.Selected;
				Character.DisableControls = true;
				if (this.OnEnterPressed != null && PlayerInput.KeyHit(Keys.Enter))
				{
					this.OnEnterPressed(this, this.Text);
				}
			}
			else if (this.Selected)
			{
				this.Deselect();
			}
			this.textBlock.State = this.State;
		}

		// Token: 0x060014FC RID: 5372 RVA: 0x000C3D6C File Offset: 0x000C1F6C
		private void HandleCaretBoundsOverflow()
		{
			if (this.textBlock.OverflowClipActive)
			{
				this.CalculateCaretPos();
				float left = (float)this.textBlock.Rect.X + this.textBlock.Padding.X;
				if (this.CaretScreenPos.X < left)
				{
					float diff = left - this.CaretScreenPos.X;
					this.textBlock.TextPos = new Vector2(this.textBlock.TextPos.X + diff, this.textBlock.TextPos.Y);
					this.CalculateCaretPos();
				}
				float right = (float)this.textBlock.Rect.Right - this.textBlock.Padding.Z;
				if (this.CaretScreenPos.X > right)
				{
					float diff2 = this.CaretScreenPos.X - right;
					this.textBlock.TextPos = new Vector2(this.textBlock.TextPos.X - diff2, this.textBlock.TextPos.Y);
					this.CalculateCaretPos();
				}
			}
		}

		// Token: 0x060014FD RID: 5373 RVA: 0x000C3E84 File Offset: 0x000C2084
		private void DrawCaretAndSelection(SpriteBatch spriteBatch, GUICustomComponent customComponent)
		{
			if (!base.Visible)
			{
				return;
			}
			if (!this.Selected)
			{
				return;
			}
			if (this.caretVisible)
			{
				GUI.DrawLine(spriteBatch, new Vector2((float)(this.Rect.X + (int)this.caretPos.X + 2), (float)this.Rect.Y + this.caretPos.Y + 3f), new Vector2((float)(this.Rect.X + (int)this.caretPos.X + 2), (float)this.Rect.Y + this.caretPos.Y + this.Font.LineHeight * this.textBlock.TextScale - 3f), this.CaretColor ?? (this.textBlock.TextColor * ((float)this.textBlock.TextColor.A / 255f)), 0f, 1f);
			}
			if (this.selectedCharacters > 0)
			{
				this.DrawSelectionRect(spriteBatch);
			}
		}

		// Token: 0x060014FE RID: 5374 RVA: 0x000C3FA8 File Offset: 0x000C21A8
		private void DrawSelectionRect(SpriteBatch spriteBatch)
		{
			GUITextBox.<>c__DisplayClass129_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			ImmutableArray<Vector2> characterPositions = this.textBlock.GetAllCaretPositions();
			int endIndex;
			int startIndex;
			if (!this.IsLeftToRight)
			{
				int num = this.selectionEndIndex;
				int num2 = this.selectionStartIndex;
				endIndex = num2;
				startIndex = num;
			}
			else
			{
				int num3 = this.selectionStartIndex;
				int num2 = this.selectionEndIndex;
				endIndex = num2;
				startIndex = num3;
			}
			endIndex--;
			Vector2 topLeft = characterPositions[startIndex];
			for (int i = startIndex + 1; i <= endIndex; i++)
			{
				Vector2 currPos = characterPositions[i];
				if (!MathUtils.NearlyEqual(topLeft.Y, currPos.Y, 0.0001f))
				{
					Vector2 bottomRight = characterPositions[i - 1];
					bottomRight += this.Font.MeasureChar(this.Text[i - 1]) * this.TextBlock.TextScale;
					this.<DrawSelectionRect>g__drawRect|129_0(topLeft, bottomRight, ref CS$<>8__locals1);
					topLeft = currPos;
				}
			}
			Vector2 finalBottomRight = characterPositions[endIndex];
			if (this.Text.Length > endIndex)
			{
				finalBottomRight += this.Font.MeasureChar(this.Text[endIndex]) * this.TextBlock.TextScale;
			}
			this.<DrawSelectionRect>g__drawRect|129_0(topLeft, finalBottomRight, ref CS$<>8__locals1);
		}

		// Token: 0x060014FF RID: 5375 RVA: 0x000C40EC File Offset: 0x000C22EC
		public void ReceiveTextInput(char inputChar)
		{
			this.ReceiveTextInput(inputChar.ToString());
		}

		// Token: 0x06001500 RID: 5376 RVA: 0x000C40FC File Offset: 0x000C22FC
		public void ReceiveTextInput(string input)
		{
			if (this.Readonly)
			{
				return;
			}
			if (this.selectedCharacters > 0)
			{
				this.RemoveSelectedText();
			}
			using (new GUITextBox.TextPosPreservation(this))
			{
				if (this.SetText(this.Text.Insert(this.CaretIndex, input), true))
				{
					this.CaretIndex = Math.Min(this.Text.Length, this.CaretIndex + input.Length);
					GUITextBox.OnTextChangedHandler onTextChanged = this.OnTextChanged;
					if (onTextChanged != null)
					{
						onTextChanged(this, this.Text);
					}
					IMEPreviewTextHandler imepreviewTextHandler = this.imePreviewTextHandler;
					if (imepreviewTextHandler != null)
					{
						imepreviewTextHandler.Reset();
					}
				}
			}
		}

		// Token: 0x06001501 RID: 5377 RVA: 0x000C41AC File Offset: 0x000C23AC
		public void ReceiveCommandInput(char command)
		{
			if (this.IsIMEActive)
			{
				return;
			}
			if (this.Text == null)
			{
				this.Text = "";
			}
			if (PlayerInput.IsAltDown())
			{
				return;
			}
			if (command <= '\u0003')
			{
				if (command != '\u0001')
				{
					if (command != '\u0003')
					{
						return;
					}
				}
				else
				{
					if (PlayerInput.IsCtrlDown())
					{
						this.SelectAll();
						return;
					}
					return;
				}
			}
			else if (command != '\b')
			{
				if (command != '\u0012')
				{
					switch (command)
					{
					case '\u0016':
					{
						if (this.Readonly)
						{
							return;
						}
						string text = this.GetCopiedText();
						this.RemoveSelectedText();
						if (!this.SetText(this.Text.Insert(this.CaretIndex, text), true))
						{
							return;
						}
						this.CaretIndex = Math.Min(this.Text.Length, this.CaretIndex + text.Length);
						GUITextBox.OnTextChangedHandler onTextChanged = this.OnTextChanged;
						if (onTextChanged == null)
						{
							return;
						}
						onTextChanged(this, this.Text);
						return;
					}
					case '\u0017':
					case '\u0019':
						return;
					case '\u0018':
						this.CopySelectedText();
						if (!this.Readonly)
						{
							this.RemoveSelectedText();
							return;
						}
						return;
					case '\u001a':
					{
						if (this.Readonly || SubEditorScreen.IsSubEditor())
						{
							return;
						}
						string text = this.memento.Undo();
						if (!(text != this.Text))
						{
							return;
						}
						this.ClearSelection();
						this.SetText(text, false);
						this.CaretIndex = this.Text.Length;
						GUITextBox.OnTextChangedHandler onTextChanged2 = this.OnTextChanged;
						if (onTextChanged2 == null)
						{
							return;
						}
						onTextChanged2(this, this.Text);
						return;
					}
					default:
						return;
					}
				}
				else
				{
					if (this.Readonly || SubEditorScreen.IsSubEditor())
					{
						return;
					}
					string text = this.memento.Redo();
					if (!(text != this.Text))
					{
						return;
					}
					this.ClearSelection();
					this.SetText(text, false);
					this.CaretIndex = this.Text.Length;
					GUITextBox.OnTextChangedHandler onTextChanged3 = this.OnTextChanged;
					if (onTextChanged3 == null)
					{
						return;
					}
					onTextChanged3(this, this.Text);
					return;
				}
			}
			else
			{
				if (this.Readonly)
				{
					return;
				}
				using (new GUITextBox.TextPosPreservation(this))
				{
					if (PlayerInput.KeyDown(Keys.LeftControl) || PlayerInput.KeyDown(Keys.RightControl))
					{
						this.SetText(string.Empty, false);
						this.CaretIndex = this.Text.Length;
					}
					else if (this.selectedCharacters > 0)
					{
						this.RemoveSelectedText();
					}
					else if (this.Text.Length > 0 && this.CaretIndex > 0)
					{
						int caretIndex = this.CaretIndex;
						this.CaretIndex = caretIndex - 1;
						this.SetText(this.Text.Remove(this.CaretIndex, 1), true);
						this.CalculateCaretPos();
						this.ClearSelection();
					}
					GUITextBox.OnTextChangedHandler onTextChanged4 = this.OnTextChanged;
					if (onTextChanged4 == null)
					{
						return;
					}
					onTextChanged4(this, this.Text);
					return;
				}
			}
			this.CopySelectedText();
		}

		// Token: 0x06001502 RID: 5378 RVA: 0x000C446C File Offset: 0x000C266C
		public void ReceiveEditingInput(string text, int start, int length)
		{
			if (string.IsNullOrEmpty(text))
			{
				this.imePreviewTextHandler.Reset();
				return;
			}
			this.imePreviewTextHandler.UpdateText(text, start, length);
		}

		// Token: 0x06001503 RID: 5379 RVA: 0x000C4490 File Offset: 0x000C2690
		public void ReceiveSpecialInput(Keys key)
		{
			if (this.IsIMEActive)
			{
				return;
			}
			if (key != Keys.Tab)
			{
				switch (key)
				{
				case Keys.Left:
					if (this.isSelecting)
					{
						this.InitSelectionStart();
					}
					this.CaretIndex = Math.Max(this.CaretIndex - 1, 0);
					this.caretTimer = 0f;
					this.<ReceiveSpecialInput>g__HandleSelection|135_0();
					break;
				case Keys.Up:
				{
					if (this.isSelecting)
					{
						this.InitSelectionStart();
					}
					float lineHeight = this.Font.LineHeight * this.TextBlock.TextScale;
					GUITextBlock guitextBlock = this.textBlock;
					Vector2 vector = new Vector2(this.caretPos.X, this.caretPos.Y - lineHeight * 0.5f);
					int newIndex = guitextBlock.GetCaretIndexFromLocalPos(vector);
					Vector2 requestedCharPos;
					this.textBlock.Font.WrapText(this.textBlock.Text.SanitizedValue, this.GetWrapWidth(), newIndex, out requestedCharPos);
					requestedCharPos *= this.TextBlock.TextScale;
					if (MathUtils.NearlyEqual(requestedCharPos.Y, this.caretPos.Y, 0.0001f))
					{
						newIndex = 0;
					}
					this.CaretIndex = newIndex;
					this.caretTimer = 0f;
					this.<ReceiveSpecialInput>g__HandleSelection|135_0();
					break;
				}
				case Keys.Right:
					if (this.isSelecting)
					{
						this.InitSelectionStart();
					}
					this.CaretIndex = Math.Min(this.CaretIndex + 1, this.Text.Length);
					this.caretTimer = 0f;
					this.<ReceiveSpecialInput>g__HandleSelection|135_0();
					break;
				case Keys.Down:
				{
					if (this.isSelecting)
					{
						this.InitSelectionStart();
					}
					float lineHeight = this.Font.LineHeight * this.TextBlock.TextScale;
					GUITextBlock guitextBlock2 = this.textBlock;
					Vector2 vector = new Vector2(this.caretPos.X, this.caretPos.Y + lineHeight * 1.5f);
					int newIndex = guitextBlock2.GetCaretIndexFromLocalPos(vector);
					Vector2 requestedCharPos2;
					this.textBlock.Font.WrapText(this.textBlock.Text.SanitizedValue, this.GetWrapWidth(), newIndex, out requestedCharPos2);
					requestedCharPos2 *= this.TextBlock.TextScale;
					if (MathUtils.NearlyEqual(requestedCharPos2.Y, this.caretPos.Y, 0.0001f))
					{
						newIndex = this.Text.Length;
					}
					this.CaretIndex = newIndex;
					this.caretTimer = 0f;
					this.<ReceiveSpecialInput>g__HandleSelection|135_0();
					break;
				}
				default:
					if (key == Keys.Delete)
					{
						if (!this.Readonly)
						{
							if (this.selectedCharacters > 0)
							{
								this.RemoveSelectedText();
							}
							else if (this.Text.Length > 0 && this.CaretIndex < this.Text.Length)
							{
								this.SetText(this.Text.Remove(this.CaretIndex, 1), true);
								GUITextBox.OnTextChangedHandler onTextChanged = this.OnTextChanged;
								if (onTextChanged != null)
								{
									onTextChanged(this, this.Text);
								}
								this.caretPosDirty = true;
							}
						}
					}
					break;
				}
			}
			else
			{
				SerializableEntityEditor editor = (from p in base.RectTransform.GetParents()
				select p.GUIComponent as SerializableEntityEditor).FirstOrDefault((SerializableEntityEditor e) => e != null);
				if (editor != null)
				{
					List<GUITextBox> allTextBoxes = GUITextBox.<ReceiveSpecialInput>g__GetAndSortTextBoxes|135_3(editor).ToList<GUITextBox>();
					if (allTextBoxes.Any<GUITextBox>())
					{
						int currentIndex = allTextBoxes.IndexOf(this);
						int nextIndex = Math.Min(allTextBoxes.Count - 1, currentIndex + 1);
						GUITextBox next = allTextBoxes[nextIndex];
						if (next != this)
						{
							next.Select(-1, false);
							next.Flash(new Color?(Color.White * 0.5f), 0.5f, false, false, null);
						}
						else
						{
							GUIListBox listBox = (from p in base.RectTransform.GetParents()
							select p.GUIComponent as GUIListBox).FirstOrDefault((GUIListBox lb) => lb != null);
							if (listBox != null)
							{
								listBox.SelectNext(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
								while (GUITextBox.<ReceiveSpecialInput>g__SelectNextTextBox|135_4(listBox) == null)
								{
									GUIComponent previous = listBox.SelectedComponent;
									listBox.SelectNext(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
									if (listBox.SelectedComponent == previous)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
			if (this.caretPosDirty)
			{
				this.CalculateCaretPos();
			}
			TextBoxEvent onKeyHit = this.OnKeyHit;
			if (onKeyHit == null)
			{
				return;
			}
			onKeyHit(this, key);
		}

		// Token: 0x06001504 RID: 5380 RVA: 0x000C4911 File Offset: 0x000C2B11
		public void SelectAll()
		{
			this.CaretIndex = 0;
			this.CalculateCaretPos();
			this.selectionStartIndex = 0;
			this.CaretIndex = this.Text.Length;
			this.CalculateSelection();
		}

		// Token: 0x06001505 RID: 5381 RVA: 0x000C493E File Offset: 0x000C2B3E
		private void CopySelectedText()
		{
			Clipboard.SetText(this.selectedText);
		}

		// Token: 0x06001506 RID: 5382 RVA: 0x000C494B File Offset: 0x000C2B4B
		private void ClearSelection()
		{
			this.selectedCharacters = 0;
			this.selectionStartIndex = -1;
			this.selectionEndIndex = -1;
			this.selectedText = string.Empty;
		}

		// Token: 0x06001507 RID: 5383 RVA: 0x000C4970 File Offset: 0x000C2B70
		private string GetCopiedText()
		{
			return Clipboard.GetText();
		}

		// Token: 0x06001508 RID: 5384 RVA: 0x000C4984 File Offset: 0x000C2B84
		private void RemoveSelectedText()
		{
			if (this.selectedText.Length == 0)
			{
				return;
			}
			int targetCaretIndex = Math.Max(0, Math.Min(this.selectionEndIndex, Math.Min(this.selectionStartIndex, this.Text.Length - 1)));
			int selectionLength = Math.Min(this.Text.Length - targetCaretIndex, this.selectedText.Length);
			this.SetText(this.Text.Remove(targetCaretIndex, selectionLength), true);
			this.CaretIndex = targetCaretIndex;
			this.ClearSelection();
			GUITextBox.OnTextChangedHandler onTextChanged = this.OnTextChanged;
			if (onTextChanged == null)
			{
				return;
			}
			onTextChanged(this, this.Text);
		}

		// Token: 0x06001509 RID: 5385 RVA: 0x000C4A24 File Offset: 0x000C2C24
		private float GetWrapWidth()
		{
			if (!this.Wrap)
			{
				return float.PositiveInfinity;
			}
			return ((float)this.textBlock.Rect.Width - this.textBlock.Padding.X - this.textBlock.Padding.Z) / this.TextBlock.TextScale;
		}

		// Token: 0x0600150A RID: 5386 RVA: 0x000C4A7E File Offset: 0x000C2C7E
		private void InitSelectionStart()
		{
			if (this.caretPosDirty)
			{
				this.CalculateCaretPos();
			}
			if (this.selectionStartIndex == -1)
			{
				this.selectionStartIndex = this.CaretIndex;
			}
		}

		// Token: 0x0600150B RID: 5387 RVA: 0x000C4AA3 File Offset: 0x000C2CA3
		public void DrawIMEPreview(SpriteBatch spriteBatch)
		{
			this.imePreviewTextHandler.DrawIMEPreview(spriteBatch, this.CaretScreenPos, this.textBlock);
		}

		// Token: 0x0600150C RID: 5388 RVA: 0x000C4AC0 File Offset: 0x000C2CC0
		private void CalculateSelection()
		{
			string textDrawn = this.Censor ? this.textBlock.CensoredText : this.WrappedText;
			this.InitSelectionStart();
			this.selectionEndIndex = Math.Min(this.CaretIndex, textDrawn.Length);
			this.selectedCharacters = Math.Abs(this.selectionStartIndex - this.selectionEndIndex);
			try
			{
				this.selectedText = this.Text.Substring(this.IsLeftToRight ? this.selectionStartIndex : this.selectionEndIndex, Math.Min(this.selectedCharacters, this.Text.Length));
			}
			catch (ArgumentOutOfRangeException exception)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("GUITextBox: Invalid selection: (");
				defaultInterpolatedStringHandler.AppendFormatted<ArgumentOutOfRangeException>(exception);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
		}

		// Token: 0x0600150D RID: 5389 RVA: 0x000C4BAC File Offset: 0x000C2DAC
		public void ResetDelegates()
		{
			this.OnKeyHit = null;
			this.OnEnterPressed = null;
			this.OnTextChanged = null;
		}

		// Token: 0x06001511 RID: 5393 RVA: 0x000C4CE4 File Offset: 0x000C2EE4
		[CompilerGenerated]
		private void <DrawSelectionRect>g__drawRect|129_0(Vector2 topLeft, Vector2 bottomRight, ref GUITextBox.<>c__DisplayClass129_0 A_3)
		{
			int minWidth = GUI.IntScale(5f);
			if (this.OverflowClip)
			{
				topLeft.X = Math.Max(topLeft.X, 0f);
			}
			if (bottomRight.X - topLeft.X < (float)minWidth)
			{
				bottomRight.X = topLeft.X + (float)minWidth;
			}
			GUI.DrawRectangle(A_3.spriteBatch, this.Rect.Location.ToVector2() + topLeft, bottomRight - topLeft, this.SelectionColor, true, 0f, 1f);
		}

		// Token: 0x06001512 RID: 5394 RVA: 0x000C4D7C File Offset: 0x000C2F7C
		[CompilerGenerated]
		internal static IEnumerable<GUITextBox> <ReceiveSpecialInput>g__GetAndSortTextBoxes|135_3(GUIComponent parent)
		{
			return from t in parent.GetAllChildren<GUITextBox>()
			orderby t.Rect.Y, t.Rect.X
			select t;
		}

		// Token: 0x06001513 RID: 5395 RVA: 0x000C4DD8 File Offset: 0x000C2FD8
		[CompilerGenerated]
		internal static GUITextBox <ReceiveSpecialInput>g__SelectNextTextBox|135_4(GUIListBox listBox)
		{
			if (((listBox != null) ? listBox.SelectedComponent : null) == null)
			{
				return null;
			}
			IEnumerable<GUITextBox> textBoxes = GUITextBox.<ReceiveSpecialInput>g__GetAndSortTextBoxes|135_3(listBox.SelectedComponent);
			if (textBoxes.Any<GUITextBox>())
			{
				GUITextBox next = textBoxes.First<GUITextBox>();
				next.Select(-1, false);
				next.Flash(new Color?(Color.White * 0.5f), 0.5f, false, false, null);
				return next;
			}
			return null;
		}

		// Token: 0x06001514 RID: 5396 RVA: 0x000C4E45 File Offset: 0x000C3045
		[CompilerGenerated]
		private void <ReceiveSpecialInput>g__HandleSelection|135_0()
		{
			if (this.isSelecting)
			{
				this.InitSelectionStart();
				this.CalculateSelection();
				return;
			}
			this.ClearSelection();
		}

		// Token: 0x04000A84 RID: 2692
		private bool caretVisible;

		// Token: 0x04000A85 RID: 2693
		private float caretTimer;

		// Token: 0x04000A86 RID: 2694
		private readonly GUIFrame frame;

		// Token: 0x04000A87 RID: 2695
		private readonly GUITextBlock textBlock;

		// Token: 0x04000A88 RID: 2696
		private readonly GUIImage icon;

		// Token: 0x04000A89 RID: 2697
		public Func<string, string> textFilterFunction;

		// Token: 0x04000A8A RID: 2698
		public GUITextBox.OnEnterHandler OnEnterPressed;

		// Token: 0x04000A8F RID: 2703
		public bool DeselectAfterMessage = true;

		// Token: 0x04000A90 RID: 2704
		private int? maxTextLength;

		// Token: 0x04000A91 RID: 2705
		private int _caretIndex;

		// Token: 0x04000A92 RID: 2706
		private bool caretPosDirty;

		// Token: 0x04000A93 RID: 2707
		protected Vector2 caretPos;

		// Token: 0x04000A94 RID: 2708
		private bool isSelecting;

		// Token: 0x04000A95 RID: 2709
		private string selectedText = string.Empty;

		// Token: 0x04000A96 RID: 2710
		private int selectedCharacters;

		// Token: 0x04000A97 RID: 2711
		private int selectionStartIndex;

		// Token: 0x04000A98 RID: 2712
		private int selectionEndIndex;

		// Token: 0x04000A99 RID: 2713
		private readonly GUICustomComponent caretAndSelectionRenderer;

		// Token: 0x04000A9A RID: 2714
		private bool mouseHeldInside;

		// Token: 0x04000A9B RID: 2715
		private readonly Memento<string> memento = new Memento<string>();

		// Token: 0x04000A9C RID: 2716
		private bool skipUpdate;

		// Token: 0x04000A9D RID: 2717
		private bool selected;

		// Token: 0x04000AA2 RID: 2722
		private readonly IMEPreviewTextHandler imePreviewTextHandler;

		// Token: 0x02000981 RID: 2433
		// (Invoke) Token: 0x06007210 RID: 29200
		public delegate bool OnEnterHandler(GUITextBox textBox, string text);

		// Token: 0x02000982 RID: 2434
		// (Invoke) Token: 0x06007214 RID: 29204
		public delegate bool OnTextChangedHandler(GUITextBox textBox, string text);

		// Token: 0x02000983 RID: 2435
		[CompilerFeatureRequired("RefStructs")]
		private readonly ref struct TextPosPreservation
		{
			// Token: 0x17001A5D RID: 6749
			// (get) Token: 0x06007217 RID: 29207 RVA: 0x0036C1D4 File Offset: 0x0036A3D4
			private GUITextBlock textBlock
			{
				get
				{
					return this.textBox.TextBlock;
				}
			}

			// Token: 0x06007218 RID: 29208 RVA: 0x0036C1E1 File Offset: 0x0036A3E1
			public TextPosPreservation(GUITextBox tb)
			{
				this.textBox = tb;
				this.wasOverflowClipActive = tb.TextBlock.OverflowClipActive;
				this.textPos = tb.TextBlock.TextPos;
			}

			// Token: 0x06007219 RID: 29209 RVA: 0x0036C20C File Offset: 0x0036A40C
			public void Dispose()
			{
				if (this.textBlock.OverflowClipActive && this.wasOverflowClipActive && !MathUtils.NearlyEqual(this.textBlock.TextPos, this.textPos, 0.0001f))
				{
					this.textBlock.TextPos = this.textPos;
				}
			}

			// Token: 0x0400417A RID: 16762
			private readonly GUITextBox textBox;

			// Token: 0x0400417B RID: 16763
			private readonly bool wasOverflowClipActive;

			// Token: 0x0400417C RID: 16764
			private readonly Vector2 textPos;
		}
	}
}
