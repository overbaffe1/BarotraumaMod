using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000089 RID: 137
	public class GUIImage : GUIComponent
	{
		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x060012C8 RID: 4808 RVA: 0x000B7549 File Offset: 0x000B5749
		public static bool LoadingTextures
		{
			get
			{
				return GUIImage.loadingTextures;
			}
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x060012C9 RID: 4809 RVA: 0x000B7550 File Offset: 0x000B5750
		public bool Crop
		{
			get
			{
				return this.crop;
			}
		}

		// Token: 0x060012CA RID: 4810 RVA: 0x000B7558 File Offset: 0x000B5758
		public void SetCrop(bool state, bool center = true)
		{
			this.crop = state;
			if (this.crop && this.sprite != null)
			{
				this.sourceRect.Width = Math.Min(this.sprite.SourceRect.Width, (int)((float)this.Rect.Width / this.Scale));
				this.sourceRect.Height = Math.Min(this.sprite.SourceRect.Height, (int)((float)this.Rect.Height / this.Scale));
				if (center)
				{
					this.sourceRect.X = (this.sprite.SourceRect.Width - this.sourceRect.Width) / 2;
					this.sourceRect.Y = (this.sprite.SourceRect.Height - this.sourceRect.Height) / 2;
				}
				this.origin = this.sourceRect.Size.ToVector2() / 2f;
				return;
			}
			this.origin = ((this.sprite == null) ? Vector2.Zero : (this.sprite.size / 2f));
		}

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x060012CB RID: 4811 RVA: 0x000B768D File Offset: 0x000B588D
		// (set) Token: 0x060012CC RID: 4812 RVA: 0x000B7695 File Offset: 0x000B5895
		public float Scale { get; set; }

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x060012CD RID: 4813 RVA: 0x000B769E File Offset: 0x000B589E
		// (set) Token: 0x060012CE RID: 4814 RVA: 0x000B76A6 File Offset: 0x000B58A6
		public Rectangle SourceRect
		{
			get
			{
				return this.sourceRect;
			}
			set
			{
				this.sourceRect = value;
			}
		}

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x060012CF RID: 4815 RVA: 0x000B76AF File Offset: 0x000B58AF
		// (set) Token: 0x060012D0 RID: 4816 RVA: 0x000B76B8 File Offset: 0x000B58B8
		public Sprite Sprite
		{
			get
			{
				return this.sprite;
			}
			set
			{
				if (this.sprite == value)
				{
					return;
				}
				this.sprite = value;
				this.sourceRect = ((value == null) ? Rectangle.Empty : value.SourceRect);
				this.origin = ((value == null) ? Vector2.Zero : (value.size / 2f));
				if (this.scaleToFit != GUIImage.ScalingMode.None)
				{
					this.RecalculateScale();
				}
			}
		}

		// Token: 0x060012D1 RID: 4817 RVA: 0x000B771C File Offset: 0x000B591C
		public GUIImage(RectTransform rectT, string style, bool scaleToFit) : this(rectT, null, null, scaleToFit ? GUIImage.ScalingMode.ScaleToFitSmallestExtent : GUIImage.ScalingMode.None, style)
		{
		}

		// Token: 0x060012D2 RID: 4818 RVA: 0x000B7744 File Offset: 0x000B5944
		public GUIImage(RectTransform rectT, string style, GUIImage.ScalingMode scaleToFit = GUIImage.ScalingMode.None) : this(rectT, null, null, scaleToFit, style)
		{
		}

		// Token: 0x060012D3 RID: 4819 RVA: 0x000B7764 File Offset: 0x000B5964
		public GUIImage(RectTransform rectT, Sprite sprite, bool scaleToFit, Rectangle? sourceRect = null) : this(rectT, sprite, sourceRect, scaleToFit ? GUIImage.ScalingMode.ScaleToFitSmallestExtent : GUIImage.ScalingMode.None, null)
		{
		}

		// Token: 0x060012D4 RID: 4820 RVA: 0x000B7778 File Offset: 0x000B5978
		public GUIImage(RectTransform rectT, Sprite sprite, Rectangle? sourceRect = null, GUIImage.ScalingMode scaleToFit = GUIImage.ScalingMode.None) : this(rectT, sprite, sourceRect, scaleToFit, null)
		{
		}

		// Token: 0x060012D5 RID: 4821 RVA: 0x000B7788 File Offset: 0x000B5988
		private GUIImage(RectTransform rectT, Sprite sprite, Rectangle? sourceRect, GUIImage.ScalingMode scaleToFit, string style) : base(style, rectT)
		{
			this.scaleToFit = scaleToFit;
			this.Sprite = sprite;
			if (sourceRect != null)
			{
				this.sourceRect = sourceRect.Value;
			}
			else
			{
				this.sourceRect = ((sprite == null) ? Rectangle.Empty : sprite.SourceRect);
			}
			if (style == null)
			{
				this.color = (this.hoverColor = (this.selectedColor = (this.pressedColor = (this.disabledColor = Color.White))));
			}
			if (scaleToFit == GUIImage.ScalingMode.None)
			{
				this.Scale = 1f;
			}
			else if (this.Sprite != null && !this.Sprite.LazyLoad)
			{
				rectT.SizeChanged += this.RecalculateScale;
			}
			this.Enabled = true;
		}

		// Token: 0x060012D6 RID: 4822 RVA: 0x000B7850 File Offset: 0x000B5A50
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible || this.loading)
			{
				return;
			}
			if (base.Parent != null)
			{
				this.State = base.Parent.State;
			}
			if (this.OverrideState != null)
			{
				this.State = this.OverrideState.Value;
			}
			if (this.Sprite != null && this.Sprite.LazyLoad && !this.lazyLoaded)
			{
				if (this.LoadAsynchronously)
				{
					GUIImage.loadingTextures = true;
					this.loading = true;
					TaskPool.Add("LoadTextureAsync", this.LoadTextureAsync(), delegate(Task task)
					{
						if (task.Exception != null)
						{
							Exception innerMost = task.Exception.GetInnermost();
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to load \"");
							defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Sprite.FilePath);
							defaultInterpolatedStringHandler.AppendLiteral("\"");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), innerMost, null, false, false);
						}
						this.loading = false;
						this.lazyLoaded = true;
						base.RectTransform.SizeChanged += this.RecalculateScale;
						this.RecalculateScale();
					});
					return;
				}
				this.Sprite.EnsureLazyLoaded(false);
				base.RectTransform.SizeChanged += this.RecalculateScale;
				this.RecalculateScale();
				this.lazyLoaded = true;
			}
			Color currentColor = this.GetColor(this.State);
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (this.BlendState != null || this.scaleToFit == GUIImage.ScalingMode.ScaleToFitLargestExtent)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, this.Rect);
				spriteBatch.Begin(SpriteSortMode.Deferred, this.BlendState, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			GUIComponentStyle style = base.Style;
			if (style != null)
			{
				using (List<UISprite>.Enumerator enumerator = style.Sprites[this.State].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						UISprite uiSprite = enumerator.Current;
						if (Math.Abs(this.Rotation) > 1E-45f)
						{
							float scale = Math.Min((float)this.Rect.Width / uiSprite.Sprite.size.X, (float)this.Rect.Height / uiSprite.Sprite.size.Y);
							spriteBatch.Draw(uiSprite.Sprite.Texture, this.Rect.Center.ToVector2(), new Rectangle?(uiSprite.Sprite.SourceRect), currentColor * ((float)currentColor.A / 255f), this.Rotation, uiSprite.Sprite.size / 2f, this.Scale * scale, this.SpriteEffects, 0f);
						}
						else
						{
							uiSprite.Draw(spriteBatch, this.Rect, currentColor * ((float)currentColor.A / 255f), this.SpriteEffects, null);
						}
					}
					goto IL_345;
				}
			}
			Sprite sprite = this.sprite;
			Texture2D texture2D = (sprite != null) ? sprite.Texture : null;
			if (texture2D != null && !texture2D.IsDisposed)
			{
				spriteBatch.Draw(this.sprite.Texture, new Vector2((float)this.Rect.X + (float)this.Rect.Width / 2f, (float)this.Rect.Y + (float)this.Rect.Height / 2f), new Rectangle?(this.sourceRect), currentColor * ((float)currentColor.A / 255f), this.Rotation, this.origin, this.Scale, this.SpriteEffects, 0f);
			}
			IL_345:
			if (this.BlendState != null || this.scaleToFit == GUIImage.ScalingMode.ScaleToFitLargestExtent)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
		}

		// Token: 0x060012D7 RID: 4823 RVA: 0x000B7C00 File Offset: 0x000B5E00
		private void RecalculateScale()
		{
			if (this.sourceRect == Rectangle.Empty && this.sprite != null)
			{
				this.sourceRect = this.sprite.SourceRect;
			}
			if (this.sprite == null || this.sprite.SourceRect.Width == 0 || this.sprite.SourceRect.Height == 0)
			{
				this.Scale = 1f;
				return;
			}
			if (this.scaleToFit == GUIImage.ScalingMode.ScaleToFitLargestExtent)
			{
				this.Scale = Math.Max((float)base.RectTransform.Rect.Width / (float)this.sprite.SourceRect.Width, (float)base.RectTransform.Rect.Height / (float)this.sprite.SourceRect.Height);
				return;
			}
			this.Scale = Math.Min((float)base.RectTransform.Rect.Width / (float)this.sprite.SourceRect.Width, (float)base.RectTransform.Rect.Height / (float)this.sprite.SourceRect.Height);
		}

		// Token: 0x060012D8 RID: 4824 RVA: 0x000B7D1C File Offset: 0x000B5F1C
		private Task<bool> LoadTextureAsync()
		{
			GUIImage.<LoadTextureAsync>d__36 <LoadTextureAsync>d__;
			<LoadTextureAsync>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<LoadTextureAsync>d__.<>4__this = this;
			<LoadTextureAsync>d__.<>1__state = -1;
			<LoadTextureAsync>d__.<>t__builder.Start<GUIImage.<LoadTextureAsync>d__36>(ref <LoadTextureAsync>d__);
			return <LoadTextureAsync>d__.<>t__builder.Task;
		}

		// Token: 0x04000964 RID: 2404
		private static readonly List<string> activeTextureLoads = new List<string>();

		// Token: 0x04000965 RID: 2405
		private static bool loadingTextures;

		// Token: 0x04000966 RID: 2406
		public float Rotation;

		// Token: 0x04000967 RID: 2407
		private Sprite sprite;

		// Token: 0x04000968 RID: 2408
		private Rectangle sourceRect;

		// Token: 0x04000969 RID: 2409
		private bool crop;

		// Token: 0x0400096A RID: 2410
		private readonly GUIImage.ScalingMode scaleToFit;

		// Token: 0x0400096B RID: 2411
		private bool lazyLoaded;

		// Token: 0x0400096C RID: 2412
		private bool loading;

		// Token: 0x0400096D RID: 2413
		public bool LoadAsynchronously;

		// Token: 0x0400096E RID: 2414
		private Vector2 origin;

		// Token: 0x04000970 RID: 2416
		public BlendState BlendState;

		// Token: 0x04000971 RID: 2417
		public GUIComponent.ComponentState? OverrideState;

		// Token: 0x0200094E RID: 2382
		public enum ScalingMode
		{
			// Token: 0x04004106 RID: 16646
			None,
			// Token: 0x04004107 RID: 16647
			ScaleToFitSmallestExtent,
			// Token: 0x04004108 RID: 16648
			ScaleToFitLargestExtent
		}
	}
}
