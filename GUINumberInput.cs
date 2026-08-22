using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200008E RID: 142
	internal class GUINumberInput : GUIComponent
	{
		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x060013A2 RID: 5026 RVA: 0x000BD3FD File Offset: 0x000BB5FD
		// (set) Token: 0x060013A3 RID: 5027 RVA: 0x000BD405 File Offset: 0x000BB605
		public GUITextBox TextBox { get; private set; }

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x060013A4 RID: 5028 RVA: 0x000BD40E File Offset: 0x000BB60E
		// (set) Token: 0x060013A5 RID: 5029 RVA: 0x000BD416 File Offset: 0x000BB616
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				base.ToolTip = value;
				this.TextBox.ToolTip = value;
			}
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x060013A6 RID: 5030 RVA: 0x000BD42B File Offset: 0x000BB62B
		// (set) Token: 0x060013A7 RID: 5031 RVA: 0x000BD433 File Offset: 0x000BB633
		public GUIButton PlusButton { get; private set; }

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x060013A8 RID: 5032 RVA: 0x000BD43C File Offset: 0x000BB63C
		// (set) Token: 0x060013A9 RID: 5033 RVA: 0x000BD444 File Offset: 0x000BB644
		public GUIButton MinusButton { get; private set; }

		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x060013AA RID: 5034 RVA: 0x000BD44D File Offset: 0x000BB64D
		// (set) Token: 0x060013AB RID: 5035 RVA: 0x000BD455 File Offset: 0x000BB655
		public GUINumberInput.ButtonVisibility PlusMinusButtonVisibility
		{
			get
			{
				return this._plusMinusButtonVisibility;
			}
			set
			{
				if (this._plusMinusButtonVisibility != value)
				{
					this._plusMinusButtonVisibility = value;
					this.UpdatePlusMinusButtonVisibility();
				}
			}
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x000BD470 File Offset: 0x000BB670
		private void UpdatePlusMinusButtonVisibility()
		{
			switch (this.PlusMinusButtonVisibility)
			{
			case GUINumberInput.ButtonVisibility.Automatic:
				if (this.inputType != NumberType.Int)
				{
					if (this.inputType == NumberType.Float)
					{
						float? num = this.MinValueFloat;
						float num2 = float.MinValue;
						if (num.GetValueOrDefault() > num2 & num != null)
						{
							num = this.MaxValueFloat;
							num2 = float.MaxValue;
							if (num.GetValueOrDefault() < num2 & num != null)
							{
								goto IL_7F;
							}
						}
					}
					this.HidePlusMinusButtons();
					return;
				}
				IL_7F:
				this.ShowPlusMinusButtons();
				return;
			case GUINumberInput.ButtonVisibility.Manual:
				return;
			case GUINumberInput.ButtonVisibility.ForceVisible:
				this.ShowPlusMinusButtons();
				return;
			case GUINumberInput.ButtonVisibility.ForceHidden:
				this.HidePlusMinusButtons();
				return;
			default:
				return;
			}
		}

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x060013AD RID: 5037 RVA: 0x000BD50A File Offset: 0x000BB70A
		// (set) Token: 0x060013AE RID: 5038 RVA: 0x000BD512 File Offset: 0x000BB712
		public NumberType InputType
		{
			get
			{
				return this.inputType;
			}
			set
			{
				if (this.inputType == value)
				{
					return;
				}
				this.inputType = value;
				this.UpdatePlusMinusButtonVisibility();
			}
		}

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x060013AF RID: 5039 RVA: 0x000BD52B File Offset: 0x000BB72B
		// (set) Token: 0x060013B0 RID: 5040 RVA: 0x000BD533 File Offset: 0x000BB733
		public float? MinValueFloat
		{
			get
			{
				return this.minValueFloat;
			}
			set
			{
				this.minValueFloat = value;
				this.ClampFloatValue();
				this.UpdatePlusMinusButtonVisibility();
			}
		}

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x060013B1 RID: 5041 RVA: 0x000BD548 File Offset: 0x000BB748
		// (set) Token: 0x060013B2 RID: 5042 RVA: 0x000BD550 File Offset: 0x000BB750
		public float? MaxValueFloat
		{
			get
			{
				return this.maxValueFloat;
			}
			set
			{
				this.maxValueFloat = value;
				this.ClampFloatValue();
				this.UpdatePlusMinusButtonVisibility();
			}
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x060013B3 RID: 5043 RVA: 0x000BD565 File Offset: 0x000BB765
		// (set) Token: 0x060013B4 RID: 5044 RVA: 0x000BD570 File Offset: 0x000BB770
		public float FloatValue
		{
			get
			{
				return this.floatValue;
			}
			set
			{
				if (Math.Abs(value - this.floatValue) < 0.0001f && MathUtils.NearlyEqual(value, this.floatValue, 0.0001f))
				{
					return;
				}
				this.floatValue = value;
				this.ClampFloatValue();
				float newValue = this.floatValue;
				this.UpdateText();
				this.floatValue = newValue;
				GUINumberInput.OnValueChangedHandler onValueChanged = this.OnValueChanged;
				if (onValueChanged == null)
				{
					return;
				}
				onValueChanged(this);
			}
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x060013B5 RID: 5045 RVA: 0x000BD5D7 File Offset: 0x000BB7D7
		// (set) Token: 0x060013B6 RID: 5046 RVA: 0x000BD5DF File Offset: 0x000BB7DF
		public int DecimalsToDisplay
		{
			get
			{
				return this.decimalsToDisplay;
			}
			set
			{
				this.decimalsToDisplay = value;
				this.UpdateText();
			}
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x060013B7 RID: 5047 RVA: 0x000BD5EE File Offset: 0x000BB7EE
		// (set) Token: 0x060013B8 RID: 5048 RVA: 0x000BD5F6 File Offset: 0x000BB7F6
		public int? MinValueInt
		{
			get
			{
				return this.minValueInt;
			}
			set
			{
				this.minValueInt = value;
				this.ClampIntValue();
			}
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x060013B9 RID: 5049 RVA: 0x000BD605 File Offset: 0x000BB805
		// (set) Token: 0x060013BA RID: 5050 RVA: 0x000BD60D File Offset: 0x000BB80D
		public int? MaxValueInt
		{
			get
			{
				return this.maxValueInt;
			}
			set
			{
				this.maxValueInt = value;
				this.ClampIntValue();
			}
		}

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x060013BB RID: 5051 RVA: 0x000BD61C File Offset: 0x000BB81C
		// (set) Token: 0x060013BC RID: 5052 RVA: 0x000BD624 File Offset: 0x000BB824
		public int IntValue
		{
			get
			{
				return this.intValue;
			}
			set
			{
				if (value == this.intValue)
				{
					return;
				}
				this.intValue = value;
				this.ClampIntValue();
				this.UpdateText();
			}
		}

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x060013BD RID: 5053 RVA: 0x000BD643 File Offset: 0x000BB843
		// (set) Token: 0x060013BE RID: 5054 RVA: 0x000BD64C File Offset: 0x000BB84C
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				this.PlusButton.Enabled = true;
				this.MinusButton.Enabled = true;
				if (this.InputType == NumberType.Int)
				{
					this.ClampIntValue();
				}
				else
				{
					this.ClampFloatValue();
				}
				this.TextBox.Enabled = value;
				if (!value)
				{
					this.PlusButton.Enabled = false;
					this.MinusButton.Enabled = false;
				}
			}
		}

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x060013BF RID: 5055 RVA: 0x000BD6AE File Offset: 0x000BB8AE
		// (set) Token: 0x060013C0 RID: 5056 RVA: 0x000BD6BB File Offset: 0x000BB8BB
		public bool Readonly
		{
			get
			{
				return this.TextBox.Readonly;
			}
			set
			{
				this.TextBox.Readonly = value;
				this.PlusButton.Enabled = !value;
				this.MinusButton.Enabled = !value;
			}
		}

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x060013C1 RID: 5057 RVA: 0x000BD6E7 File Offset: 0x000BB8E7
		// (set) Token: 0x060013C2 RID: 5058 RVA: 0x000BD6EF File Offset: 0x000BB8EF
		public override GUIFont Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				if (this.TextBox != null)
				{
					this.TextBox.Font = value;
				}
			}
		}

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x060013C3 RID: 5059 RVA: 0x000BD70C File Offset: 0x000BB90C
		// (set) Token: 0x060013C4 RID: 5060 RVA: 0x000BD714 File Offset: 0x000BB914
		public GUILayoutGroup LayoutGroup { get; private set; }

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x060013C5 RID: 5061 RVA: 0x000BD71D File Offset: 0x000BB91D
		private bool IsPressedTimerRunning
		{
			get
			{
				return this.pressedTimer > 0f;
			}
		}

		// Token: 0x060013C6 RID: 5062 RVA: 0x000BD72C File Offset: 0x000BB92C
		public GUINumberInput(RectTransform rectT, NumberType inputType, string style = "", Alignment textAlignment = Alignment.Center, float? relativeButtonAreaWidth = null, GUINumberInput.ButtonVisibility buttonVisibility = GUINumberInput.ButtonVisibility.Automatic, [TupleElementNames(new string[]
		{
			"PlusButton",
			"MinusButton"
		})] ValueTuple<GUIButton, GUIButton>? customPlusMinusButtons = null) : base(style, rectT)
		{
			GUINumberInput <>4__this = this;
			this.LayoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			float _relativeButtonAreaWidth = relativeButtonAreaWidth ?? MathHelper.Clamp((float)this.Rect.Height / (float)this.Rect.Width, 0.1f, 0.25f);
			this.TextBox = new GUITextBox(new RectTransform(new Vector2(1f - _relativeButtonAreaWidth, 1f), this.LayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, textAlignment, false, "GUITextBoxNoIcon", null, false, true)
			{
				ClampText = false
			};
			this.TextBox.CaretColor = new Color?(this.TextBox.TextColor);
			this.TextBox.OnTextChanged += this.TextChanged;
			this.TextBox.OnDeselected += delegate(GUITextBox sender, Keys key)
			{
				if (inputType == NumberType.Int)
				{
					<>4__this.ClampIntValue();
				}
				else
				{
					<>4__this.ClampFloatValue();
				}
				GUINumberInput.OnValueEnteredHandler onValueEntered = <>4__this.OnValueEntered;
				if (onValueEntered == null)
				{
					return;
				}
				onValueEntered(<>4__this);
			};
			GUITextBox textBox4 = this.TextBox;
			textBox4.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(textBox4.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
			{
				if (inputType == NumberType.Int)
				{
					<>4__this.ClampIntValue();
				}
				else
				{
					<>4__this.ClampFloatValue();
				}
				GUINumberInput.OnValueEnteredHandler onValueEntered = <>4__this.OnValueEntered;
				if (onValueEntered != null)
				{
					onValueEntered(<>4__this);
				}
				return true;
			}));
			if (customPlusMinusButtons != null)
			{
				this.PlusButton = customPlusMinusButtons.Value.Item1;
				this.MinusButton = customPlusMinusButtons.Value.Item2;
			}
			else
			{
				GUIFrame buttonArea = new GUIFrame(new RectTransform(new Vector2(_relativeButtonAreaWidth, 1f), this.LayoutGroup.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null);
				this.PlusButton = new GUIButton(new RectTransform(new Vector2(1f, 0.5f), buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null);
				GUIStyle.Apply(this.PlusButton, "PlusButton", this);
				this.MinusButton = new GUIButton(new RectTransform(new Vector2(1f, 0.5f), buttonArea.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null);
				GUIStyle.Apply(this.MinusButton, "MinusButton", this);
			}
			this.PlusButton.ClickSound = GUISoundType.Increase;
			GUIButton plusButton = this.PlusButton;
			plusButton.OnButtonDown = (GUIButton.OnButtonDownHandler)Delegate.Combine(plusButton.OnButtonDown, new GUIButton.OnButtonDownHandler(delegate()
			{
				<>4__this.pressedTimer = <>4__this.pressedDelay;
				return true;
			}));
			GUIButton plusButton2 = this.PlusButton;
			plusButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(plusButton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object data)
			{
				<>4__this.IncreaseValue();
				return true;
			}));
			GUIButton plusButton3 = this.PlusButton;
			plusButton3.OnPressed = (GUIButton.OnPressedHandler)Delegate.Combine(plusButton3.OnPressed, new GUIButton.OnPressedHandler(delegate()
			{
				if (!<>4__this.IsPressedTimerRunning)
				{
					<>4__this.IncreaseValue();
				}
				return true;
			}));
			this.MinusButton.ClickSound = GUISoundType.Decrease;
			GUIButton minusButton = this.MinusButton;
			minusButton.OnButtonDown = (GUIButton.OnButtonDownHandler)Delegate.Combine(minusButton.OnButtonDown, new GUIButton.OnButtonDownHandler(delegate()
			{
				<>4__this.pressedTimer = <>4__this.pressedDelay;
				return true;
			}));
			GUIButton minusButton2 = this.MinusButton;
			minusButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(minusButton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object data)
			{
				<>4__this.ReduceValue();
				return true;
			}));
			GUIButton minusButton3 = this.MinusButton;
			minusButton3.OnPressed = (GUIButton.OnPressedHandler)Delegate.Combine(minusButton3.OnPressed, new GUIButton.OnPressedHandler(delegate()
			{
				if (!<>4__this.IsPressedTimerRunning)
				{
					<>4__this.ReduceValue();
				}
				return true;
			}));
			this.PlusMinusButtonVisibility = buttonVisibility;
			if (inputType == NumberType.Int)
			{
				this.UpdateText();
				GUITextBox textBox2 = this.TextBox;
				textBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(textBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox txtBox, string txt)
				{
					<>4__this.UpdateText();
					<>4__this.TextBox.Deselect();
					return true;
				}));
				this.TextBox.OnDeselected += delegate(GUITextBox txtBox, Keys key)
				{
					<>4__this.UpdateText();
				};
			}
			else if (inputType == NumberType.Float)
			{
				this.UpdateText();
				this.TextBox.OnDeselected += delegate(GUITextBox txtBox, Keys key)
				{
					<>4__this.UpdateText();
				};
				GUITextBox textBox3 = this.TextBox;
				textBox3.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(textBox3.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox txtBox, string txt)
				{
					<>4__this.UpdateText();
					<>4__this.TextBox.Deselect();
					return true;
				}));
			}
			this.InputType = inputType;
			NumberType numberType = this.InputType;
			if (numberType != NumberType.Int)
			{
				if (numberType == NumberType.Float)
				{
					this.TextBox.textFilterFunction = ((string text) => new string((from c in text
					where char.IsDigit(c) || c == '.' || c == '-'
					select c).ToArray<char>()));
				}
			}
			else
			{
				this.TextBox.textFilterFunction = ((string text) => new string((from c in text
				where char.IsNumber(c) || c == '-'
				select c).ToArray<char>()));
			}
			base.RectTransform.MinSize = new Point(Math.Max(rectT.MinSize.X, this.TextBox.RectTransform.MinSize.X), Math.Max(rectT.MinSize.Y, this.TextBox.RectTransform.MinSize.Y));
			this.LayoutGroup.Recalculate();
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x000BDC8C File Offset: 0x000BBE8C
		private void HidePlusMinusButtons()
		{
			this.PlusButton.Parent.Visible = (this.MinusButton.Parent.Visible = false);
			this.PlusButton.Parent.IgnoreLayoutGroups = (this.MinusButton.Parent.IgnoreLayoutGroups = true);
			this.TextBox.RectTransform.RelativeSize = Vector2.One;
			this.LayoutGroup.Recalculate();
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x000BDD04 File Offset: 0x000BBF04
		private void ShowPlusMinusButtons()
		{
			this.PlusButton.Parent.Visible = (this.MinusButton.Parent.Visible = true);
			this.PlusButton.Parent.IgnoreLayoutGroups = (this.MinusButton.Parent.IgnoreLayoutGroups = false);
			this.TextBox.RectTransform.RelativeSize = new Vector2(1f - this.PlusButton.Parent.RectTransform.RelativeSize.X, 1f);
			this.LayoutGroup.Recalculate();
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x000BDDA0 File Offset: 0x000BBFA0
		private void ReduceValue()
		{
			if (this.inputType == NumberType.Int)
			{
				this.IntValue -= ((this.ValueStep > 0f) ? ((int)this.ValueStep) : 1);
				this.ClampIntValue();
				return;
			}
			if (this.maxValueFloat != null && this.minValueFloat != null)
			{
				this.FloatValue -= ((this.ValueStep > 0f) ? this.ValueStep : this.Round());
				this.ClampFloatValue();
			}
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x000BDE28 File Offset: 0x000BC028
		private void IncreaseValue()
		{
			if (this.inputType == NumberType.Int)
			{
				this.IntValue += ((this.ValueStep > 0f) ? ((int)this.ValueStep) : 1);
				this.ClampIntValue();
				return;
			}
			if (this.inputType == NumberType.Float)
			{
				this.FloatValue += ((this.ValueStep > 0f) ? this.ValueStep : this.Round());
				this.ClampFloatValue();
			}
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x000BDEA0 File Offset: 0x000BC0A0
		private float Round()
		{
			if (this.maxValueFloat == null || this.minValueFloat == null)
			{
				return 0f;
			}
			float onePercent = MathHelper.Lerp(this.minValueFloat.Value, this.maxValueFloat.Value, 0.01f);
			float diff = this.maxValueFloat.Value - this.minValueFloat.Value;
			int decimals = (int)MathHelper.Lerp(3f, 0f, MathUtils.InverseLerp(10f, 1000f, diff));
			return MathHelper.Clamp((float)Math.Round((double)onePercent, decimals), 0.1f, 1000f);
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x000BDF40 File Offset: 0x000BC140
		private bool TextChanged(GUITextBox textBox, string text)
		{
			NumberType numberType = this.InputType;
			int newIntValue;
			if (numberType != NumberType.Int)
			{
				if (numberType == NumberType.Float)
				{
					float newFloatValue;
					if (string.IsNullOrWhiteSpace(text) || text == "-")
					{
						this.floatValue = 0f;
					}
					else if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out newFloatValue))
					{
						this.floatValue = newFloatValue;
					}
				}
			}
			else if (string.IsNullOrWhiteSpace(text) || text == "-")
			{
				this.intValue = 0;
			}
			else if (int.TryParse(text, out newIntValue))
			{
				this.intValue = newIntValue;
			}
			GUINumberInput.OnValueChangedHandler onValueChanged = this.OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(this);
			}
			return true;
		}

		// Token: 0x060013CD RID: 5069 RVA: 0x000BDFE0 File Offset: 0x000BC1E0
		private void ClampFloatValue()
		{
			if (this.MinValueFloat != null)
			{
				this.floatValue = ((this.WrapAround && this.MinValueFloat != null && this.floatValue < this.MinValueFloat.Value) ? this.MaxValueFloat.Value : Math.Max(this.floatValue, this.MinValueFloat.Value));
				GUIComponent minusButton = this.MinusButton;
				bool enabled;
				if (!this.WrapAround)
				{
					float num = this.floatValue;
					float? num2 = this.MinValueFloat;
					enabled = (num > num2.GetValueOrDefault() & num2 != null);
				}
				else
				{
					enabled = true;
				}
				minusButton.Enabled = enabled;
			}
			if (this.MaxValueFloat != null)
			{
				this.floatValue = ((this.WrapAround && this.MaxValueFloat != null && this.floatValue > this.MaxValueFloat.Value) ? this.MinValueFloat.Value : Math.Min(this.floatValue, this.MaxValueFloat.Value));
				GUIComponent plusButton = this.PlusButton;
				bool enabled2;
				if (!this.WrapAround)
				{
					float num3 = this.floatValue;
					float? num2 = this.MaxValueFloat;
					enabled2 = (num3 < num2.GetValueOrDefault() & num2 != null);
				}
				else
				{
					enabled2 = true;
				}
				plusButton.Enabled = enabled2;
			}
			if (this.Readonly)
			{
				this.PlusButton.Enabled = (this.MinusButton.Enabled = false);
			}
		}

		// Token: 0x060013CE RID: 5070 RVA: 0x000BE158 File Offset: 0x000BC358
		private void ClampIntValue()
		{
			if (this.MinValueInt != null && this.intValue < this.MinValueInt.Value)
			{
				this.intValue = ((this.WrapAround && this.MaxValueInt != null) ? this.MaxValueInt.Value : Math.Max(this.intValue, this.MinValueInt.Value));
				this.UpdateText();
			}
			if (this.MaxValueInt != null && this.intValue > this.MaxValueInt.Value)
			{
				this.intValue = ((this.WrapAround && this.MinValueInt != null) ? this.MinValueInt.Value : Math.Min(this.intValue, this.MaxValueInt.Value));
				this.UpdateText();
			}
			if (this.Readonly)
			{
				this.PlusButton.Enabled = (this.MinusButton.Enabled = false);
				return;
			}
			GUIComponent plusButton = this.PlusButton;
			bool enabled;
			if (!this.WrapAround && this.MaxValueInt != null)
			{
				int num = this.intValue;
				int? num2 = this.MaxValueInt;
				enabled = (num < num2.GetValueOrDefault() & num2 != null);
			}
			else
			{
				enabled = true;
			}
			plusButton.Enabled = enabled;
			GUIComponent minusButton = this.MinusButton;
			bool enabled2;
			if (!this.WrapAround && this.MinValueInt != null)
			{
				int num3 = this.intValue;
				int? num2 = this.MinValueInt;
				enabled2 = (num3 > num2.GetValueOrDefault() & num2 != null);
			}
			else
			{
				enabled2 = true;
			}
			minusButton.Enabled = enabled2;
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x000BE2F8 File Offset: 0x000BC4F8
		private void UpdateText()
		{
			NumberType numberType = this.InputType;
			if (numberType != NumberType.Int)
			{
				if (numberType == NumberType.Float)
				{
					this.TextBox.Text = this.FloatValue.Format(this.decimalsToDisplay);
					return;
				}
			}
			else
			{
				this.TextBox.Text = this.IntValue.ToString();
			}
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x000BE349 File Offset: 0x000BC549
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.IsPressedTimerRunning)
			{
				this.pressedTimer -= deltaTime;
			}
		}

		// Token: 0x040009CA RID: 2506
		public GUINumberInput.OnValueEnteredHandler OnValueEntered;

		// Token: 0x040009CB RID: 2507
		public GUINumberInput.OnValueChangedHandler OnValueChanged;

		// Token: 0x040009CF RID: 2511
		private GUINumberInput.ButtonVisibility _plusMinusButtonVisibility;

		// Token: 0x040009D0 RID: 2512
		private NumberType inputType;

		// Token: 0x040009D1 RID: 2513
		private float? minValueFloat;

		// Token: 0x040009D2 RID: 2514
		private float? maxValueFloat;

		// Token: 0x040009D3 RID: 2515
		private float floatValue;

		// Token: 0x040009D4 RID: 2516
		private int decimalsToDisplay = 1;

		// Token: 0x040009D5 RID: 2517
		private int? minValueInt;

		// Token: 0x040009D6 RID: 2518
		private int? maxValueInt;

		// Token: 0x040009D7 RID: 2519
		private int intValue;

		// Token: 0x040009D9 RID: 2521
		public bool WrapAround;

		// Token: 0x040009DA RID: 2522
		public float ValueStep;

		// Token: 0x040009DB RID: 2523
		private float pressedTimer;

		// Token: 0x040009DC RID: 2524
		private readonly float pressedDelay = 0.5f;

		// Token: 0x0200096A RID: 2410
		// (Invoke) Token: 0x060071B0 RID: 29104
		public delegate void OnValueEnteredHandler(GUINumberInput numberInput);

		// Token: 0x0200096B RID: 2411
		// (Invoke) Token: 0x060071B4 RID: 29108
		public delegate void OnValueChangedHandler(GUINumberInput numberInput);

		// Token: 0x0200096C RID: 2412
		public enum ButtonVisibility
		{
			// Token: 0x04004155 RID: 16725
			Automatic,
			// Token: 0x04004156 RID: 16726
			Manual,
			// Token: 0x04004157 RID: 16727
			ForceVisible,
			// Token: 0x04004158 RID: 16728
			ForceHidden
		}
	}
}
