using System;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000BE RID: 190
	internal class VideoPlayer
	{
		// Token: 0x06001794 RID: 6036 RVA: 0x000E88A8 File Offset: 0x000E6AA8
		public VideoPlayer()
		{
			int screenWidth = (int)((float)GameMain.GraphicsWidth * 0.65f);
			this.scaledVideoResolution = new Point(screenWidth, (int)((float)screenWidth / 16f * 9f));
			int width = this.scaledVideoResolution.X;
			int height = this.scaledVideoResolution.Y;
			this.background = new GUIFrame(new RectTransform(Point.Zero, GUI.Canvas, Anchor.Center, null, ScaleBasis.Normal, false), null, new Color?(this.backgroundColor));
			this.videoFrame = new GUIFrame(new RectTransform(Point.Zero, this.background.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), ScaleBasis.Normal, false), "InnerFrame", null);
			if (this.useTextOnRightSide)
			{
				this.textFrame = new GUIFrame(new RectTransform(Point.Zero, this.videoFrame.RectTransform, Anchor.CenterLeft, new Pivot?(Pivot.CenterLeft), ScaleBasis.Normal, false), "TextFrame", null);
			}
			else
			{
				this.textFrame = new GUIFrame(new RectTransform(Point.Zero, this.videoFrame.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), ScaleBasis.Normal, false), "TextFrame", null);
			}
			this.videoView = new GUICustomComponent(new RectTransform(Point.Zero, this.videoFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch spriteBatch, GUICustomComponent guiCustomComponent)
			{
				this.DrawVideo(spriteBatch, guiCustomComponent.Rect);
			}, null);
			RectTransform rectT = new RectTransform(Point.Zero, this.textFrame.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), ScaleBasis.Normal, false);
			RichString text = string.Empty;
			GUIFont font = GUIStyle.LargeFont;
			this.title = new GUITextBlock(rectT, text, new Color?(new Color(253, 174, 0)), font, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(Point.Zero, this.textFrame.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), ScaleBasis.Normal, false);
			RichString text2 = string.Empty;
			font = GUIStyle.Font;
			this.textContent = new GUITextBlock(rectT2, text2, null, font, Alignment.TopLeft, false, "", null);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), this.textFrame.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal);
			RichString text3 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			this.objectiveTitle = new GUITextBlock(rectT3, text3, new Color?(Color.White), font, Alignment.CenterRight, false, "", null);
			this.objectiveTitle.Text = TextManager.Get("Tutorial.NewObjective");
			RectTransform rectT4 = new RectTransform(Point.Zero, this.textFrame.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), ScaleBasis.Normal, false);
			RichString text4 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			this.objectiveText = new GUITextBlock(rectT4, text4, new Color?(new Color(4, 180, 108)), font, Alignment.CenterRight, false, "", null);
			this.objectiveTitle.Visible = (this.objectiveText.Visible = false);
		}

		// Token: 0x06001795 RID: 6037 RVA: 0x000E8C2C File Offset: 0x000E6E2C
		public void Play()
		{
			this.IsPlaying = true;
		}

		// Token: 0x06001796 RID: 6038 RVA: 0x000E8C35 File Offset: 0x000E6E35
		public void Stop()
		{
			this.IsPlaying = false;
			if (this.currentVideo == null)
			{
				return;
			}
			this.currentVideo.Dispose();
			this.currentVideo = null;
		}

		// Token: 0x06001797 RID: 6039 RVA: 0x000E8C59 File Offset: 0x000E6E59
		private bool DisposeVideo(GUIButton button, object userData)
		{
			this.Stop();
			Action action = this.callbackOnStop;
			if (action != null)
			{
				action();
			}
			return true;
		}

		// Token: 0x06001798 RID: 6040 RVA: 0x000E8C73 File Offset: 0x000E6E73
		public void Update()
		{
			if (this.currentVideo == null)
			{
				return;
			}
			if (this.currentVideo.IsPlaying)
			{
				return;
			}
			this.currentVideo.Play();
		}

		// Token: 0x06001799 RID: 6041 RVA: 0x000E8C97 File Offset: 0x000E6E97
		public void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			if (!this.IsPlaying)
			{
				return;
			}
			this.background.AddToGUIUpdateList(ignoreChildren, order);
		}

		// Token: 0x0600179A RID: 6042 RVA: 0x000E8CAF File Offset: 0x000E6EAF
		public void LoadContent(string contentPath, VideoPlayer.VideoSettings videoSettings, VideoPlayer.TextSettings textSettings, Identifier contentId, bool startPlayback)
		{
			this.LoadContent(contentPath, videoSettings, textSettings, contentId, startPlayback, LocalizedString.EmptyString, null);
		}

		// Token: 0x0600179B RID: 6043 RVA: 0x000E8CC4 File Offset: 0x000E6EC4
		public void LoadContent(string contentPath, VideoPlayer.VideoSettings videoSettings, VideoPlayer.TextSettings textSettings, Identifier contentId, bool startPlayback, LocalizedString objective, Action onStop = null)
		{
			this.callbackOnStop = onStop;
			this.filePath = contentPath + videoSettings.File;
			if (!File.Exists(this.filePath))
			{
				DebugConsole.ThrowError("No video found at: " + this.filePath, null, null, false, false);
				this.DisposeVideo(null, null);
				return;
			}
			if (this.currentVideo != null)
			{
				this.currentVideo.Dispose();
				this.currentVideo = null;
			}
			this.currentVideo = this.CreateVideo();
			this.title.Text = ((textSettings != null) ? TextManager.Get(contentId) : string.Empty);
			this.textContent.Text = ((textSettings != null) ? textSettings.Text : string.Empty);
			this.objectiveText.Text = objective;
			this.AdjustFrames(videoSettings, textSettings);
			if (startPlayback)
			{
				this.Play();
			}
		}

		// Token: 0x0600179C RID: 6044 RVA: 0x000E8DB0 File Offset: 0x000E6FB0
		private void AdjustFrames(VideoPlayer.VideoSettings videoSettings, VideoPlayer.TextSettings textSettings)
		{
			int screenWidth = (int)((float)GameMain.GraphicsWidth * 0.55f);
			this.scaledVideoResolution = new Point(screenWidth, (int)((float)screenWidth / 16f * 9f));
			this.background.RectTransform.NonScaledSize = Point.Zero;
			this.videoFrame.RectTransform.NonScaledSize = Point.Zero;
			this.videoView.RectTransform.NonScaledSize = Point.Zero;
			this.title.RectTransform.NonScaledSize = Point.Zero;
			this.textFrame.RectTransform.NonScaledSize = Point.Zero;
			this.textContent.RectTransform.NonScaledSize = Point.Zero;
			this.objectiveText.RectTransform.NonScaledSize = Point.Zero;
			this.title.TextScale = (this.textContent.TextScale = (this.objectiveText.TextScale = (this.objectiveTitle.TextScale = GUI.Scale)));
			int scaledBorderSize = (int)((float)this.borderSize * GUI.Scale);
			int scaledTextWidth = 0;
			if (textSettings != null)
			{
				scaledTextWidth = (this.useTextOnRightSide ? ((int)((float)textSettings.Width * GUI.Scale)) : (this.scaledVideoResolution.X / 2));
			}
			int scaledTitleHeight = (int)((float)this.titleHeight * GUI.Scale);
			int scaledTextHeight = (int)((float)this.textHeight * GUI.Scale);
			int scaledObjectiveFrameHeight = (int)((float)this.objectiveFrameHeight * GUI.Scale);
			Point scaledButtonSize = new Point((int)((float)this.buttonSize.X * GUI.Scale), (int)((float)this.buttonSize.Y * GUI.Scale));
			this.background.RectTransform.NonScaledSize = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.videoFrame.RectTransform.NonScaledSize = this.scaledVideoResolution + new Point(scaledBorderSize, scaledBorderSize);
			this.videoView.RectTransform.NonScaledSize = this.scaledVideoResolution;
			this.videoFrame.RectTransform.AbsoluteOffset = new Point(0, this.videoFrame.RectTransform.NonScaledSize.Y);
			this.title.RectTransform.NonScaledSize = new Point(scaledTextWidth, scaledTitleHeight);
			this.title.RectTransform.AbsoluteOffset = new Point((int)(5f * GUI.Scale), (int)(10f * GUI.Scale));
			if (textSettings != null && !textSettings.Text.IsNullOrEmpty())
			{
				textSettings.Text = ToolBox.WrapText(textSettings.Text, (float)scaledTextWidth, GUIStyle.Font, 1f);
				int wrappedHeight = textSettings.Text.Value.Split('\n', StringSplitOptions.None).Length * scaledTextHeight;
				this.textFrame.RectTransform.NonScaledSize = new Point(scaledTextWidth + scaledBorderSize, wrappedHeight + scaledBorderSize + scaledButtonSize.Y + scaledTitleHeight);
				if (this.useTextOnRightSide)
				{
					this.textFrame.RectTransform.AbsoluteOffset = new Point(this.scaledVideoResolution.X + scaledBorderSize * 2, 0);
				}
				else
				{
					this.textFrame.RectTransform.AbsoluteOffset = new Point(0, this.scaledVideoResolution.Y + scaledBorderSize * 2);
				}
				this.textContent.RectTransform.NonScaledSize = new Point(scaledTextWidth, wrappedHeight);
				this.textContent.RectTransform.AbsoluteOffset = new Point(0, scaledBorderSize + scaledTitleHeight);
			}
			if (!this.objectiveText.Text.IsNullOrEmpty())
			{
				int scaledXOffset = (int)(-10f * GUI.Scale);
				this.objectiveTitle.RectTransform.AbsoluteOffset = new Point(scaledXOffset, this.textContent.RectTransform.Rect.Height + (int)((float)scaledTextHeight * 1.95f));
				this.objectiveText.RectTransform.AbsoluteOffset = new Point(scaledXOffset, this.textContent.RectTransform.Rect.Height + this.objectiveTitle.Rect.Height + (int)((float)scaledTextHeight * 2.25f));
				this.textFrame.RectTransform.NonScaledSize += new Point(0, scaledObjectiveFrameHeight);
				this.objectiveText.RectTransform.NonScaledSize = new Point(this.textFrame.Rect.Width, scaledTextHeight);
				this.objectiveTitle.Visible = (this.objectiveText.Visible = true);
			}
			else
			{
				this.textFrame.RectTransform.NonScaledSize += new Point(0, scaledBorderSize);
				this.objectiveTitle.Visible = (this.objectiveText.Visible = false);
			}
			if (this.okButton != null)
			{
				this.textFrame.RemoveChild(this.okButton);
				this.okButton = null;
			}
			if (textSettings != null)
			{
				if (this.useTextOnRightSide)
				{
					int totalFrameWidth = this.videoFrame.Rect.Width + this.textFrame.Rect.Width + scaledBorderSize * 2;
					int xOffset = this.videoFrame.Rect.Width / 2 + scaledBorderSize - (this.videoFrame.Rect.Width / 2 - this.textFrame.Rect.Width / 2);
					this.videoFrame.RectTransform.AbsoluteOffset = new Point(-xOffset, (int)(50f * GUI.Scale));
				}
				else
				{
					int totalFrameHeight = this.videoFrame.Rect.Height + this.textFrame.Rect.Height + scaledBorderSize * 2;
					int yOffset = this.videoFrame.Rect.Height / 2 + scaledBorderSize - (this.videoFrame.Rect.Height / 2 - this.textFrame.Rect.Height / 2);
					this.videoFrame.RectTransform.AbsoluteOffset = new Point(0, -yOffset);
				}
				this.okButton = new GUIButton(new RectTransform(scaledButtonSize, this.textFrame.RectTransform, Anchor.BottomRight, new Pivot?(Pivot.BottomRight), ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(scaledBorderSize, scaledBorderSize)
				}, TextManager.Get("OK"), Alignment.Center, "", null)
				{
					OnClicked = new GUIButton.OnClickedHandler(this.DisposeVideo)
				};
				return;
			}
			this.videoFrame.RectTransform.AbsoluteOffset = new Point(0, 0);
			this.okButton = new GUIButton(new RectTransform(scaledButtonSize, this.videoFrame.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(scaledBorderSize, scaledBorderSize)
			}, TextManager.Get("Back"), Alignment.Center, "", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(this.DisposeVideo)
			};
		}

		// Token: 0x0600179D RID: 6045 RVA: 0x000E9464 File Offset: 0x000E7664
		private Video CreateVideo()
		{
			Video video = null;
			try
			{
				video = Video.Load(GameMain.Instance.GraphicsDevice, GameMain.SoundManager, this.filePath);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error loading video content " + this.filePath + "!", e, null, false, false);
			}
			return video;
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x000E94C4 File Offset: 0x000E76C4
		private void DrawVideo(SpriteBatch spriteBatch, Rectangle rect)
		{
			if (!this.IsPlaying)
			{
				return;
			}
			spriteBatch.Draw(this.currentVideo.GetTexture(), rect, Color.White);
		}

		// Token: 0x0600179F RID: 6047 RVA: 0x000E94E6 File Offset: 0x000E76E6
		public void Remove()
		{
			if (this.currentVideo != null)
			{
				this.currentVideo.Dispose();
				this.currentVideo = null;
			}
		}

		// Token: 0x04000C0A RID: 3082
		public bool IsPlaying;

		// Token: 0x04000C0B RID: 3083
		private Video currentVideo;

		// Token: 0x04000C0C RID: 3084
		private string filePath;

		// Token: 0x04000C0D RID: 3085
		private GUIFrame background;

		// Token: 0x04000C0E RID: 3086
		private GUIFrame videoFrame;

		// Token: 0x04000C0F RID: 3087
		private GUIFrame textFrame;

		// Token: 0x04000C10 RID: 3088
		private GUITextBlock title;

		// Token: 0x04000C11 RID: 3089
		private GUITextBlock textContent;

		// Token: 0x04000C12 RID: 3090
		private GUITextBlock objectiveTitle;

		// Token: 0x04000C13 RID: 3091
		private GUITextBlock objectiveText;

		// Token: 0x04000C14 RID: 3092
		private GUICustomComponent videoView;

		// Token: 0x04000C15 RID: 3093
		private GUIButton okButton;

		// Token: 0x04000C16 RID: 3094
		private Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);

		// Token: 0x04000C17 RID: 3095
		private Action callbackOnStop;

		// Token: 0x04000C18 RID: 3096
		private Point scaledVideoResolution;

		// Token: 0x04000C19 RID: 3097
		private readonly int borderSize = 20;

		// Token: 0x04000C1A RID: 3098
		private readonly Point buttonSize = new Point(120, 30);

		// Token: 0x04000C1B RID: 3099
		private readonly int titleHeight = 30;

		// Token: 0x04000C1C RID: 3100
		private readonly int objectiveFrameHeight = 60;

		// Token: 0x04000C1D RID: 3101
		private readonly int textHeight = 25;

		// Token: 0x04000C1E RID: 3102
		private bool useTextOnRightSide;

		// Token: 0x02000A44 RID: 2628
		public class TextSettings
		{
			// Token: 0x060074A0 RID: 29856 RVA: 0x00373283 File Offset: 0x00371483
			public TextSettings(Identifier textTag, int width)
			{
				this.Text = TextManager.GetFormatted(textTag, Array.Empty<object>());
				this.Width = width;
			}

			// Token: 0x060074A1 RID: 29857 RVA: 0x003732A3 File Offset: 0x003714A3
			public TextSettings(XElement element)
			{
				this.Text = TextManager.GetFormatted(element.GetAttributeIdentifier("text", Identifier.Empty), Array.Empty<object>());
				this.Width = element.GetAttributeInt("width", 450);
			}

			// Token: 0x040043B7 RID: 17335
			public LocalizedString Text;

			// Token: 0x040043B8 RID: 17336
			public int Width;
		}

		// Token: 0x02000A45 RID: 2629
		public class VideoSettings
		{
			// Token: 0x060074A2 RID: 29858 RVA: 0x003732E1 File Offset: 0x003714E1
			public VideoSettings(string file)
			{
				this.File = file;
			}

			// Token: 0x040043B9 RID: 17337
			public readonly string File;
		}
	}
}
