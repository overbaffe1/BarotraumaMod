using System;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009E RID: 158
	public class GUIScrollBar : GUIComponent
	{
		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x0600142A RID: 5162 RVA: 0x000BFCB8 File Offset: 0x000BDEB8
		// (set) Token: 0x0600142B RID: 5163 RVA: 0x000BFCBF File Offset: 0x000BDEBF
		public static GUIScrollBar DraggingBar { get; private set; }

		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x0600142C RID: 5164 RVA: 0x000BFCC7 File Offset: 0x000BDEC7
		// (set) Token: 0x0600142D RID: 5165 RVA: 0x000BFCCF File Offset: 0x000BDECF
		public GUIFrame Frame { get; private set; }

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x0600142E RID: 5166 RVA: 0x000BFCD8 File Offset: 0x000BDED8
		// (set) Token: 0x0600142F RID: 5167 RVA: 0x000BFCE0 File Offset: 0x000BDEE0
		public GUIButton Bar { get; private set; }

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x06001430 RID: 5168 RVA: 0x000BFCE9 File Offset: 0x000BDEE9
		// (set) Token: 0x06001431 RID: 5169 RVA: 0x000BFCF1 File Offset: 0x000BDEF1
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				base.ToolTip = value;
				this.Frame.ToolTip = value;
				this.Bar.ToolTip = value;
			}
		}

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x06001432 RID: 5170 RVA: 0x000BFD12 File Offset: 0x000BDF12
		// (set) Token: 0x06001433 RID: 5171 RVA: 0x000BFD1A File Offset: 0x000BDF1A
		public float MinValue
		{
			get
			{
				return this.minValue;
			}
			set
			{
				this.minValue = MathHelper.Clamp(value, 0f, 1f);
				this.BarScroll = Math.Max(this.minValue, this.barScroll);
			}
		}

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06001434 RID: 5172 RVA: 0x000BFD49 File Offset: 0x000BDF49
		// (set) Token: 0x06001435 RID: 5173 RVA: 0x000BFD51 File Offset: 0x000BDF51
		public float MaxValue
		{
			get
			{
				return this.maxValue;
			}
			set
			{
				this.maxValue = MathHelper.Clamp(value, 0f, 1f);
				this.BarScroll = Math.Min(this.maxValue, this.barScroll);
			}
		}

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x06001436 RID: 5174 RVA: 0x000BFD80 File Offset: 0x000BDF80
		public bool IsHorizontal
		{
			get
			{
				return this.isHorizontal;
			}
		}

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x06001437 RID: 5175 RVA: 0x000BFD88 File Offset: 0x000BDF88
		// (set) Token: 0x06001438 RID: 5176 RVA: 0x000BFD90 File Offset: 0x000BDF90
		public override bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				this.enabled = value;
				this.Bar.Enabled = value;
				base.Children.ForEach(delegate(GUIComponent c)
				{
					c.Enabled = value;
				});
				if (!this.enabled)
				{
					this.Bar.Selected = false;
				}
			}
		}

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x06001439 RID: 5177 RVA: 0x000BFDF2 File Offset: 0x000BDFF2
		public Vector4 Padding
		{
			get
			{
				GUIFrame frame = this.Frame;
				if (((frame != null) ? frame.Style : null) == null)
				{
					return Vector4.Zero;
				}
				return this.Frame.Style.Padding;
			}
		}

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x0600143A RID: 5178 RVA: 0x000BFE1E File Offset: 0x000BE01E
		// (set) Token: 0x0600143B RID: 5179 RVA: 0x000BFE28 File Offset: 0x000BE028
		public Vector2 Range
		{
			get
			{
				return this.range;
			}
			set
			{
				float oldBarScrollValue = this.BarScrollValue;
				this.range = value;
				this.BarScrollValue = oldBarScrollValue;
			}
		}

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x0600143C RID: 5180 RVA: 0x000BFE4C File Offset: 0x000BE04C
		// (set) Token: 0x0600143D RID: 5181 RVA: 0x000BFEA0 File Offset: 0x000BE0A0
		public float BarScrollValue
		{
			get
			{
				if (this.ScrollToValue == null)
				{
					return this.BarScroll * (this.Range.Y - this.Range.X) + this.Range.X;
				}
				return this.ScrollToValue(this, this.BarScroll);
			}
			set
			{
				if (this.ValueToScroll == null)
				{
					this.BarScroll = (value - this.Range.X) / (this.Range.Y - this.Range.X);
					return;
				}
				this.BarScroll = this.ValueToScroll(this, value);
			}
		}

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x0600143E RID: 5182 RVA: 0x000BFEF4 File Offset: 0x000BE0F4
		// (set) Token: 0x0600143F RID: 5183 RVA: 0x000BFF1C File Offset: 0x000BE11C
		public float BarScroll
		{
			get
			{
				if (this.step != 0f)
				{
					return MathUtils.RoundTowardsClosest(this.barScroll, this.step);
				}
				return this.barScroll;
			}
			set
			{
				if (float.IsNaN(value))
				{
					return;
				}
				this.barScroll = MathHelper.Clamp(value, this.minValue, this.maxValue);
				int newX = this.Bar.RectTransform.AbsoluteOffset.X;
				int newY = this.Bar.RectTransform.AbsoluteOffset.Y;
				float newScroll = (this.step == 0f) ? this.barScroll : MathUtils.RoundTowardsClosest(this.barScroll, this.step);
				if (this.isHorizontal)
				{
					newX = (int)(this.Padding.X + newScroll * ((float)(this.Frame.Rect.Width - this.Bar.Rect.Width) - this.Padding.X - this.Padding.Z));
					newX = MathHelper.Clamp(newX, (int)this.Padding.X, this.Frame.Rect.Width - this.Bar.Rect.Width - (int)this.Padding.Z);
				}
				else
				{
					newY = (int)(this.Padding.Y + newScroll * ((float)(this.Frame.Rect.Height - this.Bar.Rect.Height) - this.Padding.Y - this.Padding.W));
					newY = MathHelper.Clamp(newY, (int)this.Padding.Y, this.Frame.Rect.Height - this.Bar.Rect.Height - (int)this.Padding.W);
				}
				this.Bar.RectTransform.AbsoluteOffset = new Point(newX, newY);
			}
		}

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06001440 RID: 5184 RVA: 0x000C00DA File Offset: 0x000BE2DA
		// (set) Token: 0x06001441 RID: 5185 RVA: 0x000C00E2 File Offset: 0x000BE2E2
		public float Step
		{
			get
			{
				return this.step;
			}
			set
			{
				this.step = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06001442 RID: 5186 RVA: 0x000C00FA File Offset: 0x000BE2FA
		// (set) Token: 0x06001443 RID: 5187 RVA: 0x000C011A File Offset: 0x000BE31A
		public float StepValue
		{
			get
			{
				return this.step * (this.Range.Y - this.Range.X);
			}
			set
			{
				this.Step = value / (this.Range.Y - this.Range.X);
			}
		}

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06001444 RID: 5188 RVA: 0x000C013B File Offset: 0x000BE33B
		// (set) Token: 0x06001445 RID: 5189 RVA: 0x000C0143 File Offset: 0x000BE343
		public float BarSize
		{
			get
			{
				return this.barSize;
			}
			set
			{
				this.barSize = Math.Min(Math.Max(value, 0f), 1f);
				this.UpdateRect();
			}
		}

		// Token: 0x06001446 RID: 5190 RVA: 0x000C0168 File Offset: 0x000BE368
		public GUIScrollBar(RectTransform rectT, float barSize = 1f, Color? color = null, string style = "", bool? isHorizontal = null) : base(style, rectT)
		{
			this.CanBeFocused = true;
			this.isHorizontal = (isHorizontal ?? (this.Rect.Width > this.Rect.Height));
			this.Frame = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUIStyle.Apply(this.Frame, this.IsHorizontal ? "GUIFrameHorizontal" : "GUIFrameVertical", this);
			this.barSize = barSize;
			this.Bar = new GUIButton(new RectTransform(Vector2.One, rectT, this.IsHorizontal ? Anchor.CenterLeft : Anchor.TopCenter, null, null, null, ScaleBasis.Normal), Alignment.Center, null, color);
			if (style == null || style.Length != 0)
			{
				if (!(style == "GUISlider"))
				{
					this.HoverCursor = CursorState.Hand;
					this.Bar.HoverCursor = CursorState.Hand;
				}
				else
				{
					this.HoverCursor = CursorState.Hand;
					this.Bar.HoverCursor = CursorState.Hand;
				}
			}
			else
			{
				this.HoverCursor = CursorState.Default;
				this.Bar.HoverCursor = CursorState.Default;
			}
			GUIStyle.Apply(this.Bar, this.IsHorizontal ? "GUIButtonHorizontal" : "GUIButtonVertical", this);
			this.Bar.OnPressed = new GUIButton.OnPressedHandler(this.SelectBar);
			this.enabled = true;
			this.UpdateRect();
			this.BarScroll = 0f;
			rectT.SizeChanged += this.UpdateRect;
			rectT.ScaleChanged += this.UpdateRect;
			this.Bar.RectTransform.SizeChanged += delegate()
			{
				this.BarScroll = this.barScroll;
			};
		}

		// Token: 0x06001447 RID: 5191 RVA: 0x000C0358 File Offset: 0x000BE558
		private void UpdateRect()
		{
			Vector4 padding = this.Frame.Style.Padding;
			Point newSize = new Point((int)((float)this.Rect.Size.X - padding.X - padding.Z), (int)((float)this.Rect.Size.Y - padding.Y - padding.W));
			newSize = (this.IsHorizontal ? newSize.Multiply(new Vector2(this.BarSize, 1f)) : newSize.Multiply(new Vector2(1f, this.BarSize)));
			this.Bar.RectTransform.Resize(newSize, true);
			this.BarScroll = this.barScroll;
		}

		// Token: 0x06001448 RID: 5192 RVA: 0x000C0418 File Offset: 0x000BE618
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			if (!this.enabled)
			{
				return;
			}
			this.Frame.State = ((GUI.MouseOn == this.Frame) ? GUIComponent.ComponentState.Hover : GUIComponent.ComponentState.None);
			if (this.Frame.State == GUIComponent.ComponentState.Hover && PlayerInput.PrimaryMouseButtonHeld())
			{
				this.Frame.State = GUIComponent.ComponentState.Pressed;
			}
			if (this.IsBooleanSwitch && (!PlayerInput.PrimaryMouseButtonHeld() || (GUI.MouseOn != this && !base.IsParentOf(GUI.MouseOn, true))))
			{
				int dir = Math.Sign(this.barScroll - (this.minValue + this.maxValue) / 2f);
				if (dir == 0)
				{
					dir = 1;
				}
				if ((this.barScroll <= this.maxValue && dir > 0) || (this.barScroll > this.minValue && dir < 0))
				{
					this.BarScroll += (float)dir * 0.1f;
				}
			}
			if (GUIScrollBar.DraggingBar == this)
			{
				GUI.ForceMouseOn(this);
				if (this.dragStartPos == null)
				{
					this.dragStartPos = new Vector2?(PlayerInput.MousePosition);
				}
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					if (this.IsBooleanSwitch && GUI.MouseOn == this.Bar && Vector2.Distance(this.dragStartPos.Value, PlayerInput.MousePosition) < 5f)
					{
						this.BarScroll = ((this.BarScroll > 0.5f) ? 0f : 1f);
						GUIScrollBar.OnMovedHandler onMoved = this.OnMoved;
						if (onMoved != null)
						{
							onMoved(this, this.BarScroll);
						}
					}
					GUIScrollBar.OnMovedHandler onReleased = this.OnReleased;
					if (onReleased != null)
					{
						onReleased(this, this.BarScroll);
					}
					GUIScrollBar.DraggingBar = null;
					this.dragStartPos = null;
				}
				if ((this.isHorizontal && PlayerInput.MousePosition.X > (float)this.Rect.X && PlayerInput.MousePosition.X < (float)this.Rect.Right) || (!this.isHorizontal && PlayerInput.MousePosition.Y > (float)this.Rect.Y && PlayerInput.MousePosition.Y < (float)this.Rect.Bottom))
				{
					this.MoveButton(PlayerInput.MouseSpeed);
					return;
				}
			}
			else if (GUI.MouseOn == this.Frame && PlayerInput.PrimaryMouseButtonClicked())
			{
				GUIScrollBar draggingBar = GUIScrollBar.DraggingBar;
				if (draggingBar != null)
				{
					GUIScrollBar.OnMovedHandler onReleased2 = draggingBar.OnReleased;
					if (onReleased2 != null)
					{
						onReleased2(GUIScrollBar.DraggingBar, GUIScrollBar.DraggingBar.BarScroll);
					}
				}
				if (this.IsBooleanSwitch)
				{
					this.MoveButton(new Vector2((float)(Math.Sign(PlayerInput.MousePosition.X - (float)this.Bar.Rect.Center.X) * this.Rect.Width), (float)(Math.Sign(PlayerInput.MousePosition.Y - (float)this.Bar.Rect.Center.Y) * this.Rect.Height)));
					return;
				}
				float barScale = 1f;
				if (this.UnclampedBarSize > 0f)
				{
					barScale = this.UnclampedBarSize / this.BarSize;
				}
				this.MoveButton(new Vector2((float)(Math.Sign(PlayerInput.MousePosition.X - (float)this.Bar.Rect.Center.X) * this.Bar.Rect.Width) * barScale, (float)(Math.Sign(PlayerInput.MousePosition.Y - (float)this.Bar.Rect.Center.Y) * this.Bar.Rect.Height) * barScale));
				GUIScrollBar.OnMovedHandler onReleased3 = this.OnReleased;
				if (onReleased3 == null)
				{
					return;
				}
				onReleased3(this, this.BarScroll);
			}
		}

		// Token: 0x06001449 RID: 5193 RVA: 0x000C07D1 File Offset: 0x000BE9D1
		private bool SelectBar()
		{
			if (!this.enabled || !PlayerInput.PrimaryMouseButtonDown())
			{
				return false;
			}
			if (this.barSize >= 1f)
			{
				return false;
			}
			GUIScrollBar.DraggingBar = this;
			SoundPlayer.PlayUISound(GUISoundType.Select);
			return true;
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x000C0800 File Offset: 0x000BEA00
		public void MoveButton(Vector2 moveAmount)
		{
			float newScroll = this.barScroll;
			if (this.isHorizontal)
			{
				moveAmount.Y = 0f;
				newScroll += moveAmount.X / ((float)(this.Frame.Rect.Width - this.Bar.Rect.Width) - this.Padding.X - this.Padding.Z);
			}
			else
			{
				moveAmount.X = 0f;
				newScroll += moveAmount.Y / ((float)(this.Frame.Rect.Height - this.Bar.Rect.Height) - this.Padding.Y - this.Padding.W);
			}
			this.BarScroll = newScroll;
			if (moveAmount != Vector2.Zero && this.OnMoved != null)
			{
				this.OnMoved(this, this.BarScroll);
			}
		}

		// Token: 0x040009F3 RID: 2547
		private bool isHorizontal;

		// Token: 0x040009F6 RID: 2550
		private float barSize;

		// Token: 0x040009F7 RID: 2551
		private float barScroll;

		// Token: 0x040009F8 RID: 2552
		private float step;

		// Token: 0x040009F9 RID: 2553
		private Vector2? dragStartPos;

		// Token: 0x040009FA RID: 2554
		public GUIScrollBar.OnMovedHandler OnMoved;

		// Token: 0x040009FB RID: 2555
		public GUIScrollBar.OnMovedHandler OnReleased;

		// Token: 0x040009FC RID: 2556
		public bool IsBooleanSwitch;

		// Token: 0x040009FD RID: 2557
		private float minValue;

		// Token: 0x040009FE RID: 2558
		private float maxValue = 1f;

		// Token: 0x040009FF RID: 2559
		private Vector2 range;

		// Token: 0x04000A00 RID: 2560
		public GUIScrollBar.ScrollConversion ScrollToValue;

		// Token: 0x04000A01 RID: 2561
		public GUIScrollBar.ScrollConversion ValueToScroll;

		// Token: 0x04000A02 RID: 2562
		public float UnclampedBarSize;

		// Token: 0x02000974 RID: 2420
		// (Invoke) Token: 0x060071DB RID: 29147
		public delegate bool OnMovedHandler(GUIScrollBar scrollBar, float barScroll);

		// Token: 0x02000975 RID: 2421
		// (Invoke) Token: 0x060071DF RID: 29151
		public delegate float ScrollConversion(GUIScrollBar scrollBar, float f);
	}
}
