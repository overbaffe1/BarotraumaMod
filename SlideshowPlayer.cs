using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200011D RID: 285
	internal class SlideshowPlayer : GUIComponent
	{
		// Token: 0x17000A3A RID: 2618
		// (get) Token: 0x060026D8 RID: 9944 RVA: 0x0019E275 File Offset: 0x0019C475
		public bool LastTextShown
		{
			get
			{
				return this.state >= this.slideshowPrefab.Slides.Length;
			}
		}

		// Token: 0x17000A3B RID: 2619
		// (get) Token: 0x060026D9 RID: 9945 RVA: 0x0019E292 File Offset: 0x0019C492
		public bool Finished
		{
			get
			{
				return this.state > this.slideshowPrefab.Slides.Length;
			}
		}

		// Token: 0x060026DA RID: 9946 RVA: 0x0019E2AC File Offset: 0x0019C4AC
		public SlideshowPlayer(RectTransform rectT, SlideshowPrefab prefab) : base(null, rectT)
		{
			this.slideshowPrefab = prefab;
			this.overlayColor = Color.Black;
			this.textColor = Color.Transparent;
			this.pressAnyKeyText = TextManager.Get("pressanykey");
			this.RefreshText();
		}

		// Token: 0x060026DB RID: 9947 RVA: 0x0019E2E9 File Offset: 0x0019C4E9
		public void Restart()
		{
			this.state = 0;
		}

		// Token: 0x060026DC RID: 9948 RVA: 0x0019E2F2 File Offset: 0x0019C4F2
		public void Finish()
		{
			this.state = this.slideshowPrefab.Slides.Length + 1;
		}

		// Token: 0x060026DD RID: 9949 RVA: 0x0019E30C File Offset: 0x0019C50C
		protected override void Update(float deltaTime)
		{
			if (this.slideshowPrefab.Slides.IsEmpty)
			{
				return;
			}
			SlideshowPrefab.Slide slide = this.slideshowPrefab.Slides[Math.Min(this.state, this.slideshowPrefab.Slides.Length - 1)];
			if (!base.Visible || (this.Finished && this.timer > slide.FadeOutDuration))
			{
				return;
			}
			this.timer += deltaTime;
			if (this.state == 0)
			{
				this.overlayColor = Color.Lerp(Color.Black, Color.White, Math.Min((this.timer - slide.FadeInDelay) / slide.FadeInDuration, 1f));
			}
			else
			{
				this.overlayColor = Color.Lerp(Color.Transparent, Color.White, Math.Min((this.timer - slide.FadeInDelay) / slide.FadeInDuration, 1f));
			}
			if (this.timer > slide.TextFadeInDelay)
			{
				this.textColor = Color.Lerp(Color.Transparent, Color.White, Math.Min((this.timer - slide.TextFadeInDelay) / slide.TextFadeInDuration, 1f));
				if (SlideshowPlayer.<Update>g__AnyKeyHit|14_0())
				{
					if (this.timer > slide.TextFadeInDelay + slide.FadeInDuration)
					{
						this.overlayColor = (this.textColor = Color.Transparent);
						this.timer = 0f;
						this.state++;
						this.RefreshText();
					}
					else
					{
						this.timer = slide.TextFadeInDelay + slide.TextFadeInDuration;
					}
				}
			}
			else
			{
				this.textColor = Color.Transparent;
				if (SlideshowPlayer.<Update>g__AnyKeyHit|14_0())
				{
					this.timer = slide.TextFadeInDelay + slide.TextFadeInDuration;
				}
			}
			if (this.state >= this.slideshowPrefab.Slides.Length)
			{
				this.overlayColor = Color.Lerp(Color.White, Color.Transparent, Math.Min(this.timer / slide.FadeOutDuration, 1f));
				this.textColor = Color.Lerp(Color.White, Color.Transparent, Math.Min(this.timer / slide.FadeOutDuration, 1f));
				if (this.timer >= slide.FadeOutDuration)
				{
					this.state++;
					this.RefreshText();
				}
			}
		}

		// Token: 0x060026DE RID: 9950 RVA: 0x0019E55C File Offset: 0x0019C75C
		private void RefreshText()
		{
			if (this.slideshowPrefab.Slides.IsEmpty)
			{
				return;
			}
			SlideshowPrefab.Slide slide = this.slideshowPrefab.Slides[Math.Min(this.state, this.slideshowPrefab.Slides.Length - 1)];
			LocalizedString text = slide.Text;
			string find = "[submarine]";
			Submarine mainSub = Submarine.MainSub;
			string value;
			if ((value = ((mainSub != null) ? mainSub.Info.Name : null)) == null)
			{
				GameSession gameSession = GameMain.GameSession;
				string text2;
				if (gameSession == null)
				{
					text2 = null;
				}
				else
				{
					SubmarineInfo submarineInfo = gameSession.SubmarineInfo;
					text2 = ((submarineInfo != null) ? submarineInfo.Name : null);
				}
				value = (text2 ?? "Unknown");
			}
			LocalizedString localizedString = text.Replace(find, value, StringComparison.Ordinal);
			string find2 = "[location]";
			Level loaded = Level.Loaded;
			string text3;
			if (loaded == null)
			{
				text3 = null;
			}
			else
			{
				Submarine startOutpost = loaded.StartOutpost;
				text3 = ((startOutpost != null) ? startOutpost.Info.Name : null);
			}
			this.currentText = localizedString.Replace(find2, text3 ?? "Unknown", StringComparison.Ordinal);
		}

		// Token: 0x060026DF RID: 9951 RVA: 0x0019E644 File Offset: 0x0019C844
		protected override void Draw(SpriteBatch spriteBatch)
		{
			SlideshowPlayer.<>c__DisplayClass16_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (this.slideshowPrefab.Slides.IsEmpty)
			{
				return;
			}
			SlideshowPrefab.Slide slide = this.slideshowPrefab.Slides[Math.Min(this.state, this.slideshowPrefab.Slides.Length - 1)];
			if (this.Finished && this.timer > slide.FadeOutDuration)
			{
				return;
			}
			Sprite overlaySprite = slide.Portrait;
			if (overlaySprite != null)
			{
				Sprite prevPortrait = null;
				if (this.state > 0 && this.state < this.slideshowPrefab.Slides.Length)
				{
					prevPortrait = this.slideshowPrefab.Slides[this.state - 1].Portrait;
					SlideshowPlayer.<Draw>g__DrawOverlay|16_0(prevPortrait, Color.White, ref CS$<>8__locals1);
				}
				if (((prevPortrait != null) ? prevPortrait.Texture : null) != overlaySprite.Texture)
				{
					SlideshowPlayer.<Draw>g__DrawOverlay|16_0(overlaySprite, this.overlayColor, ref CS$<>8__locals1);
				}
			}
			else
			{
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), this.overlayColor, true, 0f, 1f);
			}
			if (!this.currentText.IsNullOrEmpty() && this.textColor.A > 0)
			{
				Sprite backgroundSprite = GUIStyle.GetComponentStyle("CommandBackground").GetDefaultSprite();
				Vector2 centerPos = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight) / 2f;
				LocalizedString wrappedText = ToolBox.WrapText(this.currentText, (float)(GameMain.GraphicsWidth / 3), GUIStyle.Font, 1f);
				Vector2 textSize = GUIStyle.Font.MeasureString(wrappedText, false);
				Vector2 textPos = centerPos - textSize / 2f;
				backgroundSprite.Draw(CS$<>8__locals1.spriteBatch, centerPos, Color.White * ((float)this.textColor.A / 255f), backgroundSprite.size / 2f, 0f, new Vector2((float)(GameMain.GraphicsWidth / 2) / backgroundSprite.size.X, textSize.Y / backgroundSprite.size.Y * 2f), SpriteEffects.None, null);
				GUI.DrawString(CS$<>8__locals1.spriteBatch, textPos + Vector2.One, wrappedText, Color.Black * ((float)this.textColor.A / 255f), null, 0, null, ForceUpperCase.Inherit);
				GUI.DrawString(CS$<>8__locals1.spriteBatch, textPos, wrappedText, this.textColor, null, 0, null, ForceUpperCase.Inherit);
				if (this.timer > slide.TextFadeInDelay * 2f)
				{
					float alpha = Math.Min(this.timer - slide.TextFadeInDelay * 2f, 1f);
					Vector2 bottomTextPos = centerPos + new Vector2(0f, textSize.Y / 2f + 40f * GUI.Scale) - GUIStyle.Font.MeasureString(this.pressAnyKeyText, false) / 2f;
					GUI.DrawString(CS$<>8__locals1.spriteBatch, bottomTextPos + Vector2.One, this.pressAnyKeyText, Color.Black * ((float)this.textColor.A / 255f) * alpha, null, 0, null, ForceUpperCase.Inherit);
					GUI.DrawString(CS$<>8__locals1.spriteBatch, bottomTextPos, this.pressAnyKeyText, this.textColor * alpha, null, 0, null, ForceUpperCase.Inherit);
				}
			}
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x0019E9CC File Offset: 0x0019CBCC
		[CompilerGenerated]
		internal static bool <Update>g__AnyKeyHit|14_0()
		{
			return PlayerInput.GetKeyboardState.GetPressedKeys().Any((Keys k) => PlayerInput.KeyHit(k)) || PlayerInput.PrimaryMouseButtonClicked();
		}

		// Token: 0x060026E1 RID: 9953 RVA: 0x0019EA14 File Offset: 0x0019CC14
		[CompilerGenerated]
		internal static void <Draw>g__DrawOverlay|16_0(Sprite sprite, Color color, ref SlideshowPlayer.<>c__DisplayClass16_0 A_2)
		{
			if (sprite.Texture == null)
			{
				return;
			}
			GUI.DrawBackgroundSprite(A_2.spriteBatch, sprite, color, null, SpriteEffects.None);
		}

		// Token: 0x040013A4 RID: 5028
		private readonly SlideshowPrefab slideshowPrefab;

		// Token: 0x040013A5 RID: 5029
		private readonly LocalizedString pressAnyKeyText;

		// Token: 0x040013A6 RID: 5030
		private int state;

		// Token: 0x040013A7 RID: 5031
		private Color overlayColor;

		// Token: 0x040013A8 RID: 5032
		private Color textColor;

		// Token: 0x040013A9 RID: 5033
		private float timer;

		// Token: 0x040013AA RID: 5034
		private LocalizedString currentText;
	}
}
