using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000AA RID: 170
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class IMEPreviewTextHandler
	{
		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001586 RID: 5510 RVA: 0x000C9FA6 File Offset: 0x000C81A6
		public bool HasText
		{
			get
			{
				return !string.IsNullOrEmpty(this.previewText);
			}
		}

		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x06001587 RID: 5511 RVA: 0x000C9FB6 File Offset: 0x000C81B6
		// (set) Token: 0x06001588 RID: 5512 RVA: 0x000C9FBE File Offset: 0x000C81BE
		public GUIFont Font { get; set; }

		// Token: 0x06001589 RID: 5513 RVA: 0x000C9FC7 File Offset: 0x000C81C7
		public IMEPreviewTextHandler(GUIFont font)
		{
			this.Font = font;
		}

		// Token: 0x0600158A RID: 5514 RVA: 0x000C9FE1 File Offset: 0x000C81E1
		public void Reset()
		{
			this.textSize = Vector2.Zero;
			this.previewText = string.Empty;
			this.richTextData = null;
			this.isSectioned = false;
		}

		// Token: 0x0600158B RID: 5515 RVA: 0x000CA00C File Offset: 0x000C820C
		public void UpdateText(string text, int start, int length)
		{
			this.isSectioned = (start >= 0 && length > 0);
			this.richTextData = null;
			if (string.IsNullOrEmpty(text))
			{
				this.Reset();
				return;
			}
			this.previewText = text;
			this.textSize = this.Font.MeasureString(text, false);
			if (!this.isSectioned)
			{
				return;
			}
			string coloredText = ToolBox.ColorSectionOfString(text, start, length, GUIStyle.Orange);
			RichString richString = RichString.Rich(coloredText, null);
			this.previewText = richString.SanitizedValue;
			this.richTextData = richString.RichTextData;
		}

		// Token: 0x0600158C RID: 5516 RVA: 0x000CA0A8 File Offset: 0x000C82A8
		public void DrawIMEPreview(SpriteBatch spriteBatch, Vector2 position, GUITextBlock textBlock)
		{
			if (!this.HasText)
			{
				return;
			}
			int inflate = GUI.IntScale(3f);
			RectangleF rect = new RectangleF(position, this.textSize);
			rect.Inflate(inflate, inflate);
			RectangleF borderRect = rect;
			borderRect.Inflate(1, 1);
			GUI.DrawFilledRectangle(spriteBatch, borderRect, Color.White, 0.02f);
			GUI.DrawFilledRectangle(spriteBatch, rect, Color.Black, 0.01f);
			this.Font.DrawStringWithColors(spriteBatch, this.previewText, position, this.isSectioned ? GUIStyle.TextColorNormal : GUIStyle.Orange, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f, this.richTextData, 0, textBlock.TextAlignment, textBlock.ForceUpperCase);
		}

		// Token: 0x04000ACD RID: 2765
		private string previewText = string.Empty;

		// Token: 0x04000ACE RID: 2766
		private Vector2 textSize;

		// Token: 0x04000ACF RID: 2767
		private bool isSectioned;

		// Token: 0x04000AD0 RID: 2768
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private ImmutableArray<RichTextData>? richTextData;
	}
}
