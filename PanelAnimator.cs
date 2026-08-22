using System;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200011B RID: 283
	public class PanelAnimator
	{
		// Token: 0x17000A35 RID: 2613
		// (get) Token: 0x06002698 RID: 9880 RVA: 0x00199411 File Offset: 0x00197611
		// (set) Token: 0x06002699 RID: 9881 RVA: 0x0019941E File Offset: 0x0019761E
		public bool LeftEnabled
		{
			get
			{
				return this.leftButton.Enabled;
			}
			set
			{
				this.leftButton.Enabled = value;
			}
		}

		// Token: 0x17000A36 RID: 2614
		// (get) Token: 0x0600269A RID: 9882 RVA: 0x0019942C File Offset: 0x0019762C
		// (set) Token: 0x0600269B RID: 9883 RVA: 0x00199439 File Offset: 0x00197639
		public bool RightEnabled
		{
			get
			{
				return this.rightButton.Enabled;
			}
			set
			{
				this.rightButton.Enabled = value;
			}
		}

		// Token: 0x0600269C RID: 9884 RVA: 0x00199448 File Offset: 0x00197648
		public PanelAnimator(RectTransform rectTransform, GUIFrame leftFrame, GUIComponent middleFrame, GUIFrame rightFrame)
		{
			this.container = new GUIScissorComponent(rectTransform);
			this.leftFrame = leftFrame;
			this.middleFrame = middleFrame;
			this.rightFrame = rightFrame;
			this.<.ctor>g__own|16_0(leftFrame);
			this.leftButton = this.<.ctor>g__makeButton|16_1(delegate
			{
				this.LeftVisible = !this.LeftVisible;
			});
			this.<.ctor>g__own|16_0(middleFrame);
			this.rightButton = this.<.ctor>g__makeButton|16_1(delegate
			{
				this.RightVisible = !this.RightVisible;
			});
			this.<.ctor>g__own|16_0(rightFrame);
		}

		// Token: 0x0600269D RID: 9885 RVA: 0x001994D8 File Offset: 0x001976D8
		public void Update()
		{
			if (!this.LeftEnabled)
			{
				this.LeftVisible = false;
			}
			if (!this.RightEnabled)
			{
				this.RightVisible = false;
			}
			PanelAnimator.<Update>g__updateState|17_0(ref this.leftAnimState, this.LeftVisible);
			PanelAnimator.<Update>g__updateState|17_0(ref this.rightAnimState, this.RightVisible);
			int height = this.container.RectTransform.NonScaledSize.Y;
			int buttonY = height / 2 - this.leftButton.RectTransform.NonScaledSize.Y / 2;
			this.leftFrame.RectTransform.AbsoluteOffset = new Point((int)((float)(-(float)PanelAnimator.<Update>g__width|17_1(this.leftFrame)) * this.leftAnimState), 0);
			this.leftButton.RectTransform.AbsoluteOffset = this.leftFrame.RectTransform.AbsoluteOffset + new Point(PanelAnimator.<Update>g__width|17_1(this.leftFrame), buttonY);
			this.leftButton.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = (this.LeftVisible ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			});
			this.rightFrame.RectTransform.AbsoluteOffset = new Point((int)((float)PanelAnimator.<Update>g__width|17_1(this.container) + (float)PanelAnimator.<Update>g__width|17_1(this.rightFrame) * (this.rightAnimState - 1f)), 0);
			this.rightButton.RectTransform.AbsoluteOffset = this.rightFrame.RectTransform.AbsoluteOffset + new Point(-PanelAnimator.<Update>g__width|17_1(this.rightButton), buttonY);
			this.rightButton.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = (this.RightVisible ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
			});
			this.middleFrame.RectTransform.AbsoluteOffset = new Point(this.leftButton.RectTransform.AbsoluteOffset.X + PanelAnimator.<Update>g__width|17_1(this.leftButton), 0);
			this.middleFrame.RectTransform.NonScaledSize = new Point(this.rightButton.RectTransform.AbsoluteOffset.X - this.middleFrame.RectTransform.AbsoluteOffset.X, height);
		}

		// Token: 0x0600269E RID: 9886 RVA: 0x001996DC File Offset: 0x001978DC
		[CompilerGenerated]
		private void <.ctor>g__own|16_0(GUIComponent component)
		{
			component.RectTransform.Parent = this.container.Content.RectTransform;
			component.RectTransform.Anchor = Anchor.TopLeft;
			component.RectTransform.Pivot = Pivot.TopLeft;
			component.GetAllChildren<GUIDropDown>().ForEach(delegate(GUIDropDown dd)
			{
				dd.RefreshListBoxParent();
			});
		}

		// Token: 0x0600269F RID: 9887 RVA: 0x00199748 File Offset: 0x00197948
		[CompilerGenerated]
		private GUIButton <.ctor>g__makeButton|16_1(Action action)
		{
			return new GUIButton(new RectTransform(new Vector2(0.01f, 1f), this.container.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(20, 0),
				MaxSize = new Point(int.MaxValue, (int)(150f * GUI.Scale))
			}, Alignment.Center, "UIToggleButton", null)
			{
				OnClicked = delegate(GUIButton _, object __)
				{
					action();
					return false;
				}
			};
		}

		// Token: 0x060026A2 RID: 9890 RVA: 0x00199816 File Offset: 0x00197A16
		[CompilerGenerated]
		internal static void <Update>g__updateState|17_0(ref float state, bool visible)
		{
			state = MathHelper.Lerp(state, visible ? 0f : 1f, 0.5f);
		}

		// Token: 0x060026A3 RID: 9891 RVA: 0x00199835 File Offset: 0x00197A35
		[CompilerGenerated]
		internal static int <Update>g__width|17_1(GUIComponent c)
		{
			return c.RectTransform.NonScaledSize.X;
		}

		// Token: 0x04001370 RID: 4976
		private readonly GUIScissorComponent container;

		// Token: 0x04001371 RID: 4977
		private readonly GUIFrame leftFrame;

		// Token: 0x04001372 RID: 4978
		private readonly GUIComponent middleFrame;

		// Token: 0x04001373 RID: 4979
		private readonly GUIFrame rightFrame;

		// Token: 0x04001374 RID: 4980
		private readonly GUIButton leftButton;

		// Token: 0x04001375 RID: 4981
		private readonly GUIButton rightButton;

		// Token: 0x04001376 RID: 4982
		private float leftAnimState = 1f;

		// Token: 0x04001377 RID: 4983
		private float rightAnimState;

		// Token: 0x04001378 RID: 4984
		public bool LeftVisible = true;

		// Token: 0x04001379 RID: 4985
		public bool RightVisible;
	}
}
