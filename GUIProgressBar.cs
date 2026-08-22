using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200009B RID: 155
	public class GUIProgressBar : GUIComponent
	{
		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x06001414 RID: 5140 RVA: 0x000BF156 File Offset: 0x000BD356
		// (set) Token: 0x06001415 RID: 5141 RVA: 0x000BF15E File Offset: 0x000BD35E
		public bool IsHorizontal
		{
			get
			{
				return this.isHorizontal;
			}
			set
			{
				this.isHorizontal = value;
			}
		}

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x06001416 RID: 5142 RVA: 0x000BF167 File Offset: 0x000BD367
		// (set) Token: 0x06001417 RID: 5143 RVA: 0x000BF170 File Offset: 0x000BD370
		public float BarSize
		{
			get
			{
				return this.barSize;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					GameAnalyticsManager.AddErrorEventOnce("GUIProgressBar.BarSize_setter", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to set the BarSize of a GUIProgressBar to an invalid value (" + value.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace());
					return;
				}
				this.barSize = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x06001418 RID: 5144 RVA: 0x000BF1C8 File Offset: 0x000BD3C8
		public GUIProgressBar(RectTransform rectT, float barSize, Color? color = null, string style = "", bool showFrame = true) : base(style, rectT)
		{
			if (color != null)
			{
				this.color = color.Value;
			}
			this.isHorizontal = (this.Rect.Width > this.Rect.Height);
			this.frame = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUIStyle.Apply(this.frame, "", this);
			this.slider = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUIStyle.Apply(this.slider, "Slider", this);
			this.showFrame = showFrame;
			this.barSize = barSize;
			this.Enabled = true;
		}

		// Token: 0x06001419 RID: 5145 RVA: 0x000BF2CC File Offset: 0x000BD4CC
		public Rectangle GetSliderRect(float fillAmount)
		{
			Rectangle sliderArea = new Rectangle(this.frame.Rect.X + (int)base.Style.Padding.X, this.frame.Rect.Y + (int)base.Style.Padding.Y, (int)((float)this.frame.Rect.Width - base.Style.Padding.X - base.Style.Padding.Z), (int)((float)this.frame.Rect.Height - base.Style.Padding.Y - base.Style.Padding.W));
			Vector4 sliceBorderSizes = Vector4.Zero;
			if (this.slider.sprites.ContainsKey(this.slider.State))
			{
				UISprite uisprite = this.slider.sprites[this.slider.State].First<UISprite>();
				if (uisprite != null && uisprite.Slice)
				{
					Rectangle[] slices = this.slider.sprites[this.slider.State].First<UISprite>().Slices;
					sliceBorderSizes = new Vector4((float)slices[0].Width, (float)slices[0].Height, (float)slices[8].Width, (float)slices[8].Height);
					sliceBorderSizes *= this.slider.sprites[this.slider.State].First<UISprite>().GetSliceBorderScale(sliderArea.Size);
				}
			}
			Rectangle sliderRect = this.IsHorizontal ? new Rectangle(sliderArea.X + (int)sliceBorderSizes.X, sliderArea.Y, (int)Math.Round((double)(((float)sliderArea.Width - sliceBorderSizes.X - sliceBorderSizes.Z) * fillAmount)), sliderArea.Height) : new Rectangle(sliderArea.X, (int)Math.Round((double)((float)sliderArea.Bottom - ((float)sliderArea.Height - sliceBorderSizes.Y - sliceBorderSizes.W) * fillAmount - sliceBorderSizes.W)), sliderArea.Width, (int)Math.Round((double)(((float)sliderArea.Height - sliceBorderSizes.Y - sliceBorderSizes.W) * fillAmount)));
			sliderRect.Width = Math.Max(sliderRect.Width, 1);
			sliderRect.Height = Math.Max(sliderRect.Height, 1);
			return sliderRect;
		}

		// Token: 0x0600141A RID: 5146 RVA: 0x000BF544 File Offset: 0x000BD744
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			if (this.ProgressGetter != null)
			{
				float newSize = MathHelper.Clamp(this.ProgressGetter(), 0f, 1f);
				if (!MathUtils.IsValid(newSize))
				{
					GameAnalyticsManager.AddErrorEventOnce("GUIProgressBar.Draw:GetProgress", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
					{
						"ProgressGetter of a GUIProgressBar (",
						this.ProgressGetter.Target.ToString(),
						" - ",
						this.ProgressGetter.Method.ToString(),
						") returned an invalid value (",
						newSize.ToString(),
						")\n",
						Environment.StackTrace.CleanupStackTrace()
					}));
				}
				else
				{
					this.BarSize = newSize;
				}
			}
			Rectangle sliderRect = this.GetSliderRect(this.barSize);
			this.slider.RectTransform.AbsoluteOffset = new Point((int)base.Style.Padding.X, (int)base.Style.Padding.Y);
			this.slider.RectTransform.MaxSize = new Point((int)((float)this.Rect.Width - base.Style.Padding.X + base.Style.Padding.Z), (int)((float)this.Rect.Height - base.Style.Padding.Y + base.Style.Padding.W));
			this.frame.Visible = this.showFrame;
			this.slider.Visible = (this.BarSize > 0f);
			if (this.showFrame)
			{
				if (base.AutoDraw)
				{
					this.frame.DrawAuto(spriteBatch);
				}
				else
				{
					this.frame.DrawManually(spriteBatch, false, true);
				}
			}
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (this.BarSize <= 1f)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, sliderRect);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			Color currColor = this.GetColor(this.State);
			this.slider.Color = currColor;
			if (base.AutoDraw)
			{
				this.slider.DrawAuto(spriteBatch);
			}
			else
			{
				this.slider.DrawManually(spriteBatch, false, true);
			}
			this.frame.Visible = false;
			this.slider.Visible = false;
			if (this.BarSize <= 1f)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			}
		}

		// Token: 0x040009E8 RID: 2536
		private bool isHorizontal;

		// Token: 0x040009E9 RID: 2537
		private readonly GUIFrame frame;

		// Token: 0x040009EA RID: 2538
		private readonly GUIFrame slider;

		// Token: 0x040009EB RID: 2539
		private float barSize;

		// Token: 0x040009EC RID: 2540
		private readonly bool showFrame;

		// Token: 0x040009ED RID: 2541
		public GUIProgressBar.ProgressGetterHandler ProgressGetter;

		// Token: 0x02000972 RID: 2418
		// (Invoke) Token: 0x060071D3 RID: 29139
		public delegate float ProgressGetterHandler();
	}
}
