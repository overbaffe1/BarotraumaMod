using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A6 RID: 166
	public class GUITickBox : GUIComponent
	{
		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06001515 RID: 5397 RVA: 0x000C4E62 File Offset: 0x000C3062
		// (set) Token: 0x06001516 RID: 5398 RVA: 0x000C4E6A File Offset: 0x000C306A
		public override bool Selected
		{
			get
			{
				return this.isSelected;
			}
			set
			{
				this.SetSelected(value, true);
			}
		}

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x06001517 RID: 5399 RVA: 0x000C4E74 File Offset: 0x000C3074
		// (set) Token: 0x06001518 RID: 5400 RVA: 0x000C4E7C File Offset: 0x000C307C
		public override GUIComponent.ComponentState State
		{
			get
			{
				return base.State;
			}
			set
			{
				base.State = value;
				GUIComponent guicomponent = this.box;
				this.TextBlock.State = value;
				guicomponent.State = value;
			}
		}

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x06001519 RID: 5401 RVA: 0x000C4EAA File Offset: 0x000C30AA
		// (set) Token: 0x0600151A RID: 5402 RVA: 0x000C4EB4 File Offset: 0x000C30B4
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
				GUIComponent guicomponent = this.box;
				this.TextBlock.Enabled = value;
				guicomponent.Enabled = value;
				this.enabled = value;
			}
		}

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x0600151B RID: 5403 RVA: 0x000C4EEE File Offset: 0x000C30EE
		// (set) Token: 0x0600151C RID: 5404 RVA: 0x000C4EFB File Offset: 0x000C30FB
		public Color TextColor
		{
			get
			{
				return this.text.TextColor;
			}
			set
			{
				this.text.TextColor = value;
			}
		}

		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x0600151D RID: 5405 RVA: 0x000C4F09 File Offset: 0x000C3109
		// (set) Token: 0x0600151E RID: 5406 RVA: 0x000C4F11 File Offset: 0x000C3111
		public override GUIFont Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				if (this.text != null)
				{
					this.text.Font = value;
				}
			}
		}

		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x0600151F RID: 5407 RVA: 0x000C4F2E File Offset: 0x000C312E
		public GUIFrame Box
		{
			get
			{
				return this.box;
			}
		}

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06001520 RID: 5408 RVA: 0x000C4F36 File Offset: 0x000C3136
		public GUITextBlock TextBlock
		{
			get
			{
				return this.text;
			}
		}

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06001521 RID: 5409 RVA: 0x000C4F3E File Offset: 0x000C313E
		// (set) Token: 0x06001522 RID: 5410 RVA: 0x000C4F46 File Offset: 0x000C3146
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				base.ToolTip = value;
				this.box.ToolTip = value;
				this.text.ToolTip = value;
			}
		}

		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001523 RID: 5411 RVA: 0x000C4F67 File Offset: 0x000C3167
		// (set) Token: 0x06001524 RID: 5412 RVA: 0x000C4F79 File Offset: 0x000C3179
		public LocalizedString Text
		{
			get
			{
				return this.text.Text;
			}
			set
			{
				this.text.Text = value;
			}
		}

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x06001525 RID: 5413 RVA: 0x000C4F8C File Offset: 0x000C318C
		// (set) Token: 0x06001526 RID: 5414 RVA: 0x000C4F94 File Offset: 0x000C3194
		public float ContentWidth { get; private set; }

		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x06001527 RID: 5415 RVA: 0x000C4F9D File Offset: 0x000C319D
		// (set) Token: 0x06001528 RID: 5416 RVA: 0x000C4FA5 File Offset: 0x000C31A5
		public GUISoundType SoundType { private get; set; } = GUISoundType.TickBox;

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001529 RID: 5417 RVA: 0x000C4FAE File Offset: 0x000C31AE
		// (set) Token: 0x0600152A RID: 5418 RVA: 0x000C4FB6 File Offset: 0x000C31B6
		public override bool PlaySoundOnSelect { get; set; } = true;

		// Token: 0x0600152B RID: 5419 RVA: 0x000C4FC0 File Offset: 0x000C31C0
		public GUITickBox(RectTransform rectT, LocalizedString label, GUIFont font = null, string style = "") : base(null, rectT)
		{
			this.CanBeFocused = true;
			this.HoverCursor = CursorState.Hand;
			this.layoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.box = new GUIFrame(new RectTransform(Vector2.One, this.layoutGroup.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.BothHeight)
			{
				IsFixedSize = true
			}, string.Empty, new Color?(Color.DarkGray))
			{
				HoverColor = Color.Gray,
				SelectedColor = Color.DarkGray,
				CanBeFocused = false
			};
			GUIStyle.Apply(this.box, (style == "") ? "GUITickBox" : style, null);
			if (this.box.RectTransform.MinSize.Y > 0)
			{
				base.RectTransform.MinSize = this.box.RectTransform.MinSize;
				base.RectTransform.MaxSize = this.box.RectTransform.MaxSize;
				base.RectTransform.Resize(new Point(base.RectTransform.NonScaledSize.X, base.RectTransform.MinSize.Y), true);
				this.box.RectTransform.MinSize = new Point(this.box.RectTransform.MinSize.Y);
				this.box.RectTransform.Resize(this.box.RectTransform.MinSize, true);
			}
			Vector2 textBlockScale = new Vector2((float)(this.Rect.Width - this.Rect.Height) / (float)Math.Max((double)this.Rect.Width, 1.0), 1f);
			this.text = new GUITextBlock(new RectTransform(textBlockScale, this.layoutGroup.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), label, null, font, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			GUIStyle.Apply(this.text, "GUITextBlock", this);
			this.Enabled = true;
			this.ResizeBox();
			rectT.ScaleChanged += this.ResizeBox;
			rectT.SizeChanged += this.ResizeBox;
		}

		// Token: 0x0600152C RID: 5420 RVA: 0x000C526E File Offset: 0x000C346E
		public void SetRadioButtonGroup(GUIRadioButtonGroup rbg)
		{
			this.radioButtonGroup = rbg;
		}

		// Token: 0x0600152D RID: 5421 RVA: 0x000C5278 File Offset: 0x000C3478
		public void ResizeBox()
		{
			Vector2 textBlockScale = new Vector2(Math.Max((float)(this.Rect.Width - this.box.Rect.Width), 0f) / Math.Max((float)this.Rect.Width, 1f), 1f);
			this.text.RectTransform.RelativeSize = textBlockScale;
			this.box.RectTransform.MinSize = new Point(this.Rect.Height);
			this.box.RectTransform.Resize(this.box.RectTransform.MinSize, true);
			this.text.SetTextPos();
			this.ContentWidth = (float)this.box.Rect.Width + this.text.Padding.X + this.text.TextSize.X + this.text.Padding.Z;
		}

		// Token: 0x0600152E RID: 5422 RVA: 0x000C5378 File Offset: 0x000C3578
		public void SetSelected(bool selected, bool callOnSelected = true)
		{
			if (selected == this.isSelected)
			{
				return;
			}
			if (this.radioButtonGroup != null && this.radioButtonGroup.SelectedRadioButton == this)
			{
				this.isSelected = true;
				return;
			}
			this.isSelected = selected;
			this.State = (this.isSelected ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None);
			if (selected && this.radioButtonGroup != null)
			{
				this.radioButtonGroup.SelectRadioButton(this);
			}
			if (callOnSelected)
			{
				GUITickBox.OnSelectedHandler onSelected = this.OnSelected;
				if (onSelected == null)
				{
					return;
				}
				onSelected(this);
			}
		}

		// Token: 0x0600152F RID: 5423 RVA: 0x000C53F4 File Offset: 0x000C35F4
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			base.Update(deltaTime);
			if (GUI.MouseOn == this && this.Enabled && PlayerInput.MousePosition.X < (float)this.Rect.X + this.ContentWidth)
			{
				this.State = (this.Selected ? GUIComponent.ComponentState.HoverSelected : GUIComponent.ComponentState.Hover);
				if (PlayerInput.PrimaryMouseButtonHeld())
				{
					this.State = GUIComponent.ComponentState.Selected;
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					if (this.radioButtonGroup == null)
					{
						this.Selected = !this.Selected;
					}
					else if (!this.isSelected)
					{
						this.Selected = true;
					}
					if (this.PlaySoundOnSelect)
					{
						SoundPlayer.PlayUISound(this.SoundType);
						return;
					}
				}
			}
			else
			{
				if (this.isSelected)
				{
					this.State = GUIComponent.ComponentState.Selected;
					return;
				}
				this.State = GUIComponent.ComponentState.None;
			}
		}

		// Token: 0x04000AA3 RID: 2723
		private readonly GUILayoutGroup layoutGroup;

		// Token: 0x04000AA4 RID: 2724
		private readonly GUIFrame box;

		// Token: 0x04000AA5 RID: 2725
		private readonly GUITextBlock text;

		// Token: 0x04000AA6 RID: 2726
		public GUITickBox.OnSelectedHandler OnSelected;

		// Token: 0x04000AA7 RID: 2727
		private GUIRadioButtonGroup radioButtonGroup;

		// Token: 0x02000987 RID: 2439
		// (Invoke) Token: 0x06007225 RID: 29221
		public delegate bool OnSelectedHandler(GUITickBox obj);
	}
}
