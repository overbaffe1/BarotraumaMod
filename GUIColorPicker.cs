using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000080 RID: 128
	[NullableContext(1)]
	[Nullable(0)]
	public class GUIColorPicker : GUIComponent, IDisposable
	{
		// Token: 0x060011F5 RID: 4597 RVA: 0x000B2154 File Offset: 0x000B0354
		public GUIColorPicker(RectTransform rectT, [Nullable(2)] string style = null) : base(style, rectT)
		{
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x000B2194 File Offset: 0x000B0394
		private void Init()
		{
			int tWidth = this.Rect.Width;
			int sliceWidth = this.Rect.Width / 8;
			int mainWidth = tWidth - sliceWidth;
			int hueWidth = sliceWidth;
			this.MainArea = new Rectangle(0, 0, mainWidth, this.Rect.Height);
			this.HueArea = new Rectangle(mainWidth, 0, hueWidth, this.Rect.Height);
			this.colorData = new Color[this.MainArea.Width * this.MainArea.Height];
			if (this.mainTexture == null)
			{
				int width = this.MainArea.Width;
				int height = this.MainArea.Height;
				this.GenerateGradient(ref this.colorData, width, height, new Func<float, float, Color>(this.DrawHVArea));
				this.mainTexture = this.CreateGradientTexture(this.colorData, this.MainArea.Width, this.MainArea.Height);
			}
			if (this.hueTexture == null)
			{
				int width2 = this.HueArea.Width;
				int height2 = this.HueArea.Height;
				Color[] hueData = new Color[width2 * height2];
				this.GenerateGradient(ref hueData, width2, height2, new Func<float, float, Color>(this.DrawHueArea));
				this.hueTexture = this.CreateGradientTexture(hueData, width2, height2);
			}
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x000B22D4 File Offset: 0x000B04D4
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (this.mainTexture == null || this.hueTexture == null || !this.isInitialized)
			{
				return;
			}
			Rectangle mainArea = this.MainArea;
			Rectangle hueArea = this.HueArea;
			hueArea.Location += this.Rect.Location;
			mainArea.Location += this.Rect.Location;
			Vector2 mainLocation = mainArea.Location.ToVector2();
			Vector2 hueLocation = hueArea.Location.ToVector2();
			spriteBatch.Draw(this.mainTexture, mainLocation, Color.White);
			spriteBatch.Draw(this.hueTexture, hueLocation, Color.White);
			float hueY = hueLocation.Y + this.SelectedHue / 360f * (float)hueArea.Height;
			spriteBatch.DrawLine((float)hueArea.Left, hueY, (float)hueArea.Right, hueY, this.transparentWhite, 3f);
			spriteBatch.DrawLine((float)hueArea.Left, hueY, (float)hueArea.Right, hueY, this.transparentBlack, 1f);
			float saturationX = mainLocation.X + this.SelectedSaturation * (float)this.MainArea.Width;
			float valueY = mainLocation.Y + (1f - this.SelectedValue) * (float)this.MainArea.Height;
			spriteBatch.DrawLine(saturationX, (float)mainArea.Top, saturationX, (float)mainArea.Bottom, this.transparentWhite, 3f);
			spriteBatch.DrawLine((float)mainArea.Left, valueY, (float)mainArea.Right, valueY, this.transparentWhite, 3f);
			spriteBatch.DrawLine(saturationX, (float)mainArea.Top, saturationX, (float)mainArea.Bottom, this.transparentBlack, 1f);
			spriteBatch.DrawLine((float)mainArea.Left, valueY, (float)mainArea.Right, valueY, this.transparentBlack, 1f);
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x000B24CC File Offset: 0x000B06CC
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (!this.isInitialized)
			{
				this.Init();
				this.isInitialized = true;
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				this.mouseHeld = false;
			}
			if (GUI.MouseOn != this)
			{
				return;
			}
			Rectangle mainArea = this.MainArea;
			Rectangle hueArea = this.HueArea;
			hueArea.Location += this.Rect.Location;
			mainArea.Location += this.Rect.Location;
			if (PlayerInput.PrimaryMouseButtonDown())
			{
				this.mouseHeld = true;
				if (hueArea.Contains(PlayerInput.MousePosition))
				{
					this.selectedRect = this.HueArea;
				}
				else if (mainArea.Contains(PlayerInput.MousePosition))
				{
					this.selectedRect = this.MainArea;
				}
				else
				{
					this.mouseHeld = false;
				}
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				this.mouseHeld = false;
			}
			if (this.mouseHeld && (PlayerInput.MouseSpeed != Vector2.Zero || PlayerInput.PrimaryMouseButtonDown()))
			{
				if (this.selectedRect == this.HueArea)
				{
					Vector2 pos = PlayerInput.MousePosition - hueArea.Location.ToVector2();
					this.SelectedHue = Math.Clamp(pos.Y / (float)hueArea.Height * 360f, 0f, 360f);
					this.RefreshHue();
				}
				else if (this.selectedRect == this.MainArea)
				{
					float num;
					float num2;
					(PlayerInput.MousePosition - mainArea.Location.ToVector2()).Deconstruct(out num, out num2);
					float x = num;
					float y = num2;
					this.SelectedSaturation = Math.Clamp(x / (float)mainArea.Width, 0f, 1f);
					this.SelectedValue = Math.Clamp(1f - y / (float)mainArea.Height, 0f, 1f);
				}
				this.CurrentColor = ToolBoxCore.HSVToRGB(this.SelectedHue, this.SelectedSaturation, this.SelectedValue);
				GUIColorPicker.OnColorSelectedHandler onColorSelected = this.OnColorSelected;
				if (onColorSelected == null)
				{
					return;
				}
				onColorSelected(this, this.CurrentColor);
			}
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x000B26F8 File Offset: 0x000B08F8
		public void Dispose()
		{
			Texture2D texture2D = this.mainTexture;
			if (texture2D != null)
			{
				texture2D.Dispose();
			}
			this.mainTexture = null;
			Texture2D texture2D2 = this.hueTexture;
			if (texture2D2 != null)
			{
				texture2D2.Dispose();
			}
			this.hueTexture = null;
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x000B272C File Offset: 0x000B092C
		public void RefreshHue()
		{
			if (this.colorData == null || this.mainTexture == null)
			{
				return;
			}
			this.GenerateGradient(ref this.colorData, this.mainTexture.Width, this.mainTexture.Height, new Func<float, float, Color>(this.DrawHVArea));
			this.mainTexture.SetData<Color>(this.colorData);
		}

		// Token: 0x060011FB RID: 4603 RVA: 0x000B278C File Offset: 0x000B098C
		private Texture2D CreateGradientTexture(Color[] data, int width, int height)
		{
			Texture2D texture = new Texture2D(GameMain.GraphicsDeviceManager.GraphicsDevice, width, height);
			texture.SetData<Color>(data);
			return texture;
		}

		// Token: 0x060011FC RID: 4604 RVA: 0x000B27B4 File Offset: 0x000B09B4
		private void GenerateGradient(ref Color[] data, int width, int height, Func<float, float, Color> algorithm)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float relativeX = (float)x / (float)width;
					float relativeY = (float)y / (float)height;
					data[y * width + x] = algorithm(relativeX, relativeY);
				}
			}
		}

		// Token: 0x060011FD RID: 4605 RVA: 0x000B27FA File Offset: 0x000B09FA
		private Color DrawHVArea(float x, float y)
		{
			return ToolBoxCore.HSVToRGB(this.SelectedHue, x, 1f - y);
		}

		// Token: 0x060011FE RID: 4606 RVA: 0x000B280F File Offset: 0x000B0A0F
		private Color DrawHueArea(float x, float y)
		{
			return ToolBoxCore.HSVToRGB(y * 360f, 1f, 1f);
		}

		// Token: 0x040008F6 RID: 2294
		[Nullable(2)]
		public GUIColorPicker.OnColorSelectedHandler OnColorSelected;

		// Token: 0x040008F7 RID: 2295
		public float SelectedHue;

		// Token: 0x040008F8 RID: 2296
		public float SelectedSaturation;

		// Token: 0x040008F9 RID: 2297
		public float SelectedValue;

		// Token: 0x040008FA RID: 2298
		public Color CurrentColor = Color.Black;

		// Token: 0x040008FB RID: 2299
		private Rectangle MainArea;

		// Token: 0x040008FC RID: 2300
		private Rectangle HueArea;

		// Token: 0x040008FD RID: 2301
		[Nullable(2)]
		private Texture2D mainTexture;

		// Token: 0x040008FE RID: 2302
		[Nullable(2)]
		private Texture2D hueTexture;

		// Token: 0x040008FF RID: 2303
		[Nullable(2)]
		private Color[] colorData;

		// Token: 0x04000900 RID: 2304
		private Rectangle selectedRect;

		// Token: 0x04000901 RID: 2305
		private bool mouseHeld;

		// Token: 0x04000902 RID: 2306
		private bool isInitialized;

		// Token: 0x04000903 RID: 2307
		private readonly Color transparentWhite = Color.White * 0.8f;

		// Token: 0x04000904 RID: 2308
		private readonly Color transparentBlack = Color.Black * 0.8f;

		// Token: 0x02000938 RID: 2360
		// (Invoke) Token: 0x06007130 RID: 28976
		[NullableContext(0)]
		public delegate bool OnColorSelectedHandler(GUIColorPicker component, Color color);
	}
}
