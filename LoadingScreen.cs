using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000AB RID: 171
	internal sealed class LoadingScreen
	{
		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x0600158D RID: 5517 RVA: 0x000CA161 File Offset: 0x000C8361
		public bool PlayingSplashScreen
		{
			get
			{
				return this.currSplashScreen != null || this.PendingSplashScreens.Count > 0;
			}
		}

		// Token: 0x0600158E RID: 5518 RVA: 0x000CA17B File Offset: 0x000C837B
		private void SetSelectedTip(LocalizedString tip)
		{
			this.selectedTip = RichString.Rich(tip, null);
			this.selectedTipString = string.Empty;
			this.selectedTipRichTextData = null;
		}

		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x0600158F RID: 5519 RVA: 0x000CA1A1 File Offset: 0x000C83A1
		// (set) Token: 0x06001590 RID: 5520 RVA: 0x000CA1A9 File Offset: 0x000C83A9
		public bool WaitForLanguageSelection { get; set; }

		// Token: 0x06001591 RID: 5521 RVA: 0x000CA1B4 File Offset: 0x000C83B4
		public LoadingScreen(GraphicsDevice graphics)
		{
			this.defaultBackgroundTexture = new Sprite("Content/Map/LocationPortraits/MainMenu1.png", Vector2.Zero);
			this.decorativeMap = new SpriteSheet("Content/Map/MapHUD.png", 6, 5, Vector2.Zero, new Rectangle?(new Rectangle(0, 0, 2048, 640)));
			this.decorativeGraph = new SpriteSheet("Content/Map/MapHUD.png", 4, 10, Vector2.Zero, new Rectangle?(new Rectangle(1025, 1259, 1024, 732)));
			this.overlay = new Sprite("Content/UI/MainMenuVignette.png", Vector2.Zero);
			this.noiseSprite = new Sprite("Content/UI/noise.png", Vector2.Zero);
			this.SetSelectedTip(TextManager.Get("LoadingScreenTip"));
		}

		// Token: 0x06001592 RID: 5522 RVA: 0x000CA290 File Offset: 0x000C8490
		public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, float deltaTime)
		{
			if (GameSettings.CurrentConfig.EnableSplashScreen)
			{
				try
				{
					this.DrawSplashScreen(spriteBatch, graphics);
					if (this.currSplashScreen != null || this.PendingSplashScreens.Count > 0)
					{
						return;
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Playing splash screen video failed", e, null, false, false);
					this.DisableSplashScreen();
				}
			}
			this.drawn = true;
			if (this.currentBackgroundTexture == null)
			{
				this.currentBackgroundTexture = this.defaultBackgroundTexture;
			}
			float overlayScale = Math.Min((float)GameMain.GraphicsWidth / this.overlay.size.X, (float)GameMain.GraphicsHeight / this.overlay.size.Y);
			Rectangle drawArea = new Rectangle((int)(this.overlay.size.X * overlayScale / 2f), 0, (int)((float)GameMain.GraphicsWidth - this.overlay.size.X * overlayScale / 2f), GameMain.GraphicsHeight);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, GUI.SamplerState, null, null, null, null);
			if (this.currentBackgroundTexture.Texture != null)
			{
				GUI.DrawBackgroundSprite(spriteBatch, this.currentBackgroundTexture, Color.White, new Rectangle?(drawArea), SpriteEffects.None);
			}
			this.overlay.Draw(spriteBatch, Vector2.Zero, 0f, overlayScale, SpriteEffects.None);
			double noiseT = Timing.TotalTime * 0.019999999552965164;
			float noiseStrength = (float)PerlinNoise.CalculatePerlin(noiseT, noiseT, 0.0);
			float noiseScale = (float)PerlinNoise.CalculatePerlin(noiseT * 5.0, noiseT * 2.0, 0.0) * 4f;
			Sprite sprite = this.noiseSprite;
			Vector2 zero = Vector2.Zero;
			Vector2 targetSize = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight);
			float rotation = 0f;
			Vector2? startOffset = new Vector2?(new Vector2(Rand.Range(0f, (float)this.noiseSprite.SourceRect.Width, Rand.RandSync.Unsynced), Rand.Range(0f, (float)this.noiseSprite.SourceRect.Height, Rand.RandSync.Unsynced)));
			Color? color = new Color?(Color.White * noiseStrength * 0.1f);
			Vector2? textureScale = new Vector2?(Vector2.One * noiseScale);
			sprite.DrawTiled(spriteBatch, zero, targetSize, rotation, null, color, startOffset, textureScale, null);
			Vector2 textPos = new Vector2((float)((int)((float)GameMain.GraphicsWidth * 0.05f)), (float)((int)((float)GameMain.GraphicsHeight * 0.75f)));
			if (this.WaitForLanguageSelection)
			{
				this.DrawLanguageSelectionPrompt(spriteBatch, graphics);
			}
			else
			{
				float loadState = this.LoadState;
				LocalizedString loadText;
				if (loadState >= 100f)
				{
					loadText = TextManager.Get("PressAnyKey");
				}
				else
				{
					loadText = TextManager.Get("Loading");
					if (loadState >= 0f)
					{
						LocalizedString left = loadText;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<float>(loadState, "N0");
						defaultInterpolatedStringHandler.AppendLiteral(" %");
						loadText = left + defaultInterpolatedStringHandler.ToStringAndClear();
					}
				}
				if (GUIStyle.LargeFont.HasValue)
				{
					GUIStyle.LargeFont.DrawString(spriteBatch, loadText.ToUpper(), textPos, Color.White, ForceUpperCase.Inherit, false);
					textPos.Y += GUIStyle.LargeFont.MeasureString(loadText.ToUpper(), false).Y * 1.2f;
				}
				if (GUIStyle.Font.HasValue && this.selectedTip != null && !this.selectedTip.SanitizedValue.IsNullOrEmpty())
				{
					if (this.selectedTipString.IsNullOrEmpty())
					{
						this.selectedTipString = this.selectedTip.SanitizedValue;
						this.selectedTipRichTextData = this.selectedTip.RichTextData;
					}
					string wrappedTip = ToolBox.WrapText(this.selectedTipString, (float)GameMain.GraphicsWidth * 0.3f, GUIStyle.Font.Value, 1f);
					string[] lines = wrappedTip.Split('\n', StringSplitOptions.None);
					float lineHeight = GUIStyle.Font.MeasureString(this.selectedTipString, false).Y;
					ImmutableArray<RichTextData>? left2 = this.selectedTipRichTextData;
					ImmutableArray<RichTextData>? right = null;
					if (left2 != right)
					{
						int rtdOffset = 0;
						for (int i = 0; i < lines.Length; i++)
						{
							GUIFont font = GUIStyle.Font;
							string text = lines[i];
							Vector2 position = new Vector2(textPos.X, (float)((int)(textPos.Y + (float)i * lineHeight)));
							Color white = Color.White;
							float rotation2 = 0f;
							Vector2 zero2 = Vector2.Zero;
							float scale = 1f;
							SpriteEffects spriteEffects = SpriteEffects.None;
							float layerDepth = 0f;
							right = new ImmutableArray<RichTextData>?(this.selectedTipRichTextData.Value);
							font.DrawStringWithColors(spriteBatch, text, position, white, rotation2, zero2, scale, spriteEffects, layerDepth, right, rtdOffset, Alignment.TopLeft, ForceUpperCase.Inherit);
							rtdOffset += lines[i].Length;
						}
					}
					else
					{
						for (int j = 0; j < lines.Length; j++)
						{
							GUIStyle.Font.DrawString(spriteBatch, lines[j], new Vector2(textPos.X, (float)((int)(textPos.Y + (float)j * lineHeight))), new Color(228, 217, 167, 255), ForceUpperCase.Inherit, false);
						}
					}
				}
			}
			GUI.DrawMessageBoxesOnly(spriteBatch);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, null);
			Vector2 decorativeScale = new Vector2((float)GameMain.GraphicsHeight / 1080f);
			float noiseVal = (float)PerlinNoise.CalculatePerlin(Timing.TotalTime * 0.25, Timing.TotalTime * 0.5, 0.0);
			if (!this.WaitForLanguageSelection)
			{
				this.decorativeGraph.Draw(spriteBatch, (int)((float)this.decorativeGraph.FrameCount * noiseVal), new Vector2((float)GameMain.GraphicsWidth * 0.001f, textPos.Y), Color.White, new Vector2(0f, (float)this.decorativeMap.FrameSize.Y), 0f, decorativeScale, SpriteEffects.FlipVertically, null);
			}
			this.decorativeMap.Draw(spriteBatch, (int)((float)this.decorativeMap.FrameCount * noiseVal), new Vector2((float)GameMain.GraphicsWidth * 0.99f, (float)GameMain.GraphicsHeight * 0.01f), Color.White, new Vector2((float)this.decorativeMap.FrameSize.X, 0f), 0f, decorativeScale, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, null);
			if (noiseVal < 0.2f)
			{
				this.randText = new string[]
				{
					"NIL",
					"black white gray",
					"Sometimes we would have had time to scream",
					"e8m106]af",
					"NO"
				}.GetRandomUnsynced<string>();
			}
			else if (noiseVal < 0.3f)
			{
				this.randText = ToolBox.RandomSeed(9);
			}
			else if (noiseVal < 0.5f)
			{
				this.randText = string.Concat(new string[]
				{
					Rand.Int(100, Rand.RandSync.Unsynced).ToString().PadLeft(2, '0'),
					" ",
					Rand.Int(100, Rand.RandSync.Unsynced).ToString().PadLeft(2, '0'),
					" ",
					Rand.Int(100, Rand.RandSync.Unsynced).ToString().PadLeft(2, '0'),
					" ",
					Rand.Int(100, Rand.RandSync.Unsynced).ToString().PadLeft(2, '0')
				});
			}
			if (GUIStyle.LargeFont.HasValue)
			{
				Vector2 textSize = GUIStyle.LargeFont.MeasureString(this.randText, false);
				GUIStyle.LargeFont.DrawString(spriteBatch, this.randText, new Vector2((float)GameMain.GraphicsWidth * 0.95f - textSize.X, (float)GameMain.GraphicsHeight * 0.06f), Color.White * (1f - noiseVal), ForceUpperCase.Inherit, false);
			}
			spriteBatch.End();
		}

		// Token: 0x06001593 RID: 5523 RVA: 0x000CAA54 File Offset: 0x000C8C54
		private unsafe void DrawLanguageSelectionPrompt(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
		{
			if (this.AvailableLanguages == null)
			{
				return;
			}
			if (this.languageSelectionFont == null)
			{
				this.languageSelectionFont = new ScalableFont("Content/Fonts/NotoSans/NotoSans-Bold.ttf", (uint)(30f * ((float)GameMain.GraphicsHeight / 1080f)), graphicsDevice, false, TextManager.SpeciallyHandledCharCategory.None);
			}
			if (this.languageSelectionFontCJK == null)
			{
				this.languageSelectionFontCJK = new ScalableFont("Content/Fonts/NotoSans/NotoSansCJKsc-Bold.otf", (uint)(30f * ((float)GameMain.GraphicsHeight / 1080f)), graphicsDevice, true, TextManager.SpeciallyHandledCharCategory.None);
			}
			if (this.languageSelectionCursor == null)
			{
				this.languageSelectionCursor = new Sprite("Content/UI/cursor.png", Vector2.Zero);
			}
			Vector2 textPos = new Vector2((float)((int)((float)GameMain.GraphicsWidth * 0.05f)), (float)((int)((float)GameMain.GraphicsHeight * 0.3f)));
			Vector2 textSpacing = new Vector2(0f, (float)GameMain.GraphicsHeight * 0.5f / (float)this.AvailableLanguages.Length);
			foreach (LanguageIdentifier language in this.AvailableLanguages)
			{
				string localizedLanguageName = TextManager.GetTranslatedLanguageName(language);
				ScalableFont font = TextManager.IsCJK(localizedLanguageName) ? this.languageSelectionFontCJK : this.languageSelectionFont;
				Vector2 textSize = font.MeasureString(localizedLanguageName, false);
				bool hover = PlayerInput.MousePosition.X > textPos.X && PlayerInput.MousePosition.X < textPos.X + textSize.X && PlayerInput.MousePosition.Y > textPos.Y && PlayerInput.MousePosition.Y < textPos.Y + textSize.Y;
				font.DrawString(spriteBatch, localizedLanguageName, textPos, hover ? Color.White : (Color.White * 0.6f), ForceUpperCase.Inherit, false);
				if (hover && PlayerInput.PrimaryMouseButtonClicked())
				{
					GameSettings.Config config = *GameSettings.CurrentConfig;
					config.Language = language;
					GameSettings.SetCurrentConfig(config);
					this.SetSelectedTip(TextManager.Get("LoadingScreenTip"));
					this.WaitForLanguageSelection = false;
					ScalableFont scalableFont = this.languageSelectionFont;
					if (scalableFont != null)
					{
						scalableFont.Dispose();
					}
					this.languageSelectionFont = null;
					ScalableFont scalableFont2 = this.languageSelectionFontCJK;
					if (scalableFont2 != null)
					{
						scalableFont2.Dispose();
					}
					this.languageSelectionFontCJK = null;
					break;
				}
				textPos += textSpacing;
			}
			this.languageSelectionCursor.Draw(spriteBatch, PlayerInput.LatestMousePosition, 0f, 0.5f, SpriteEffects.None);
		}

		// Token: 0x06001594 RID: 5524 RVA: 0x000CAC94 File Offset: 0x000C8E94
		private void DrawSplashScreen(SpriteBatch spriteBatch, GraphicsDevice graphics)
		{
			if (this.currSplashScreen == null)
			{
				LoadingScreen.PendingSplashScreen newSplashScreen;
				if (!this.PendingSplashScreens.TryDequeue(out newSplashScreen))
				{
					return;
				}
				string fileName = newSplashScreen.Filename;
				try
				{
					this.currSplashScreen = Video.Load(graphics, GameMain.SoundManager, fileName);
					this.currSplashScreen.AudioGain = newSplashScreen.Gain;
					this.videoStartTime = DateTime.Now;
				}
				catch (Exception e)
				{
					this.DisableSplashScreen();
					DebugConsole.ThrowError("Playing the splash screen \"" + fileName + "\" failed.", e, null, false, false);
					this.PendingSplashScreens.Clear();
					this.currSplashScreen = null;
				}
			}
			if (this.currSplashScreen == null)
			{
				return;
			}
			if (this.currSplashScreen.IsPlaying)
			{
				graphics.Clear(Color.Black);
				float videoAspectRatio = (float)this.currSplashScreen.Width / (float)this.currSplashScreen.Height;
				int width;
				int height;
				if ((float)GameMain.GraphicsHeight * videoAspectRatio > (float)GameMain.GraphicsWidth)
				{
					width = GameMain.GraphicsWidth;
					height = (int)((float)GameMain.GraphicsWidth / videoAspectRatio);
				}
				else
				{
					width = (int)((float)GameMain.GraphicsHeight * videoAspectRatio);
					height = GameMain.GraphicsHeight;
				}
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				spriteBatch.Draw(this.currSplashScreen.GetTexture(), new Rectangle(GameMain.GraphicsWidth / 2 - width / 2, GameMain.GraphicsHeight / 2 - height / 2, width, height), new Rectangle?(new Rectangle(0, 0, this.currSplashScreen.Width, this.currSplashScreen.Height)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				spriteBatch.End();
				if (DateTime.Now > this.videoStartTime + new TimeSpan(0, 0, 0, 0, 500) && GameMain.WindowActive && (PlayerInput.KeyHit(Keys.Escape) || PlayerInput.KeyHit(Keys.Space) || PlayerInput.KeyHit(Keys.Enter) || PlayerInput.PrimaryMouseButtonDown()))
				{
					this.currSplashScreen.Dispose();
					this.currSplashScreen = null;
					return;
				}
			}
			else if (DateTime.Now > this.videoStartTime + new TimeSpan(0, 0, 0, 0, 1500))
			{
				this.currSplashScreen.Dispose();
				this.currSplashScreen = null;
			}
		}

		// Token: 0x06001595 RID: 5525 RVA: 0x000CAEC8 File Offset: 0x000C90C8
		private unsafe void DisableSplashScreen()
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.EnableSplashScreen = false;
			GameSettings.SetCurrentConfig(config);
		}

		// Token: 0x06001596 RID: 5526 RVA: 0x000CAEEF File Offset: 0x000C90EF
		public IEnumerable<CoroutineStatus> DoLoading(IEnumerable<CoroutineStatus> loader)
		{
			LoadingScreen.<DoLoading>d__32 <DoLoading>d__ = new LoadingScreen.<DoLoading>d__32(-2);
			<DoLoading>d__.<>4__this = this;
			<DoLoading>d__.<>3__loader = loader;
			return <DoLoading>d__;
		}

		// Token: 0x04000AD1 RID: 2769
		private readonly Sprite defaultBackgroundTexture;

		// Token: 0x04000AD2 RID: 2770
		private readonly Sprite overlay;

		// Token: 0x04000AD3 RID: 2771
		private readonly SpriteSheet decorativeGraph;

		// Token: 0x04000AD4 RID: 2772
		private readonly SpriteSheet decorativeMap;

		// Token: 0x04000AD5 RID: 2773
		private Sprite currentBackgroundTexture;

		// Token: 0x04000AD6 RID: 2774
		private readonly Sprite noiseSprite;

		// Token: 0x04000AD7 RID: 2775
		private string randText = "";

		// Token: 0x04000AD8 RID: 2776
		private Sprite languageSelectionCursor;

		// Token: 0x04000AD9 RID: 2777
		private ScalableFont languageSelectionFont;

		// Token: 0x04000ADA RID: 2778
		private ScalableFont languageSelectionFontCJK;

		// Token: 0x04000ADB RID: 2779
		private Video currSplashScreen;

		// Token: 0x04000ADC RID: 2780
		private DateTime videoStartTime;

		// Token: 0x04000ADD RID: 2781
		public readonly ConcurrentQueue<LoadingScreen.PendingSplashScreen> PendingSplashScreens = new ConcurrentQueue<LoadingScreen.PendingSplashScreen>();

		// Token: 0x04000ADE RID: 2782
		private RichString selectedTip;

		// Token: 0x04000ADF RID: 2783
		private string selectedTipString;

		// Token: 0x04000AE0 RID: 2784
		private ImmutableArray<RichTextData>? selectedTipRichTextData;

		// Token: 0x04000AE1 RID: 2785
		public float LoadState;

		// Token: 0x04000AE3 RID: 2787
		public LanguageIdentifier[] AvailableLanguages;

		// Token: 0x04000AE4 RID: 2788
		private bool drawn;

		// Token: 0x02000998 RID: 2456
		public struct PendingSplashScreen
		{
			// Token: 0x0600725E RID: 29278 RVA: 0x0036CB50 File Offset: 0x0036AD50
			public PendingSplashScreen(string filename, float gain)
			{
				this.Filename = filename;
				this.Gain = gain;
			}

			// Token: 0x040041AF RID: 16815
			public string Filename;

			// Token: 0x040041B0 RID: 16816
			public float Gain;
		}
	}
}
