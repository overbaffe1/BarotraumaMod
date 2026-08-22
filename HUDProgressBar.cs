using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200002F RID: 47
	internal class HUDProgressBar
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060007CD RID: 1997 RVA: 0x00047F18 File Offset: 0x00046118
		// (set) Token: 0x060007CE RID: 1998 RVA: 0x00047F20 File Offset: 0x00046120
		public float Progress
		{
			get
			{
				return this.progress;
			}
			set
			{
				this.progress = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060007CF RID: 1999 RVA: 0x00047F38 File Offset: 0x00046138
		// (set) Token: 0x060007D0 RID: 2000 RVA: 0x00047F40 File Offset: 0x00046140
		public Vector2 WorldPosition
		{
			get
			{
				return this.worldPosition;
			}
			set
			{
				this.worldPosition = value;
				if (this.parentSub != null)
				{
					this.worldPosition -= this.parentSub.DrawPosition;
				}
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060007D1 RID: 2001 RVA: 0x00047F6D File Offset: 0x0004616D
		// (set) Token: 0x060007D2 RID: 2002 RVA: 0x00047F75 File Offset: 0x00046175
		public LocalizedString Text { get; private set; }

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x060007D3 RID: 2003 RVA: 0x00047F7E File Offset: 0x0004617E
		// (set) Token: 0x060007D4 RID: 2004 RVA: 0x00047F88 File Offset: 0x00046188
		public string TextTag
		{
			get
			{
				return this.textTag;
			}
			set
			{
				if (this.textTag == value)
				{
					return;
				}
				this.textTag = value;
				this.Text = (string.IsNullOrEmpty(this.textTag) ? string.Empty : TextManager.Get(this.textTag));
			}
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x00047FD5 File Offset: 0x000461D5
		public HUDProgressBar(Vector2 worldPosition, string textTag, Submarine parentSubmarine = null) : this(worldPosition, parentSubmarine, GUIStyle.Red, GUIStyle.Green, textTag)
		{
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00047FF4 File Offset: 0x000461F4
		public HUDProgressBar(Vector2 worldPosition, Submarine parentSubmarine, Color emptyColor, Color fullColor, string textTag)
		{
			this.emptyColor = emptyColor;
			this.fullColor = fullColor;
			this.parentSub = parentSubmarine;
			this.WorldPosition = worldPosition;
			this.Size = new Vector2(100f, 20f);
			this.FadeTimer = 1f;
			if (!string.IsNullOrEmpty(textTag))
			{
				this.textTag = textTag;
				this.Text = TextManager.Get(textTag).Fallback(textTag, true);
			}
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x0004806F File Offset: 0x0004626F
		public void Update(float deltatime)
		{
			this.FadeTimer -= deltatime;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00048080 File Offset: 0x00046280
		public void Draw(SpriteBatch spriteBatch, Camera cam)
		{
			float a = Math.Min(this.FadeTimer, 1f);
			Vector2 pos = new Vector2(this.WorldPosition.X - this.Size.X / 2f, this.WorldPosition.Y + this.Size.Y / 2f);
			if (this.parentSub != null)
			{
				pos += this.parentSub.DrawPosition;
			}
			pos = cam.WorldToScreen(pos);
			Color color = Color.Lerp(this.emptyColor, this.fullColor, this.progress);
			GUI.DrawProgressBar(spriteBatch, new Vector2(pos.X, -pos.Y), this.Size, this.progress, color * a, Color.White * a * 0.8f, 0f);
			if (!this.Text.IsNullOrEmpty())
			{
				Vector2 textSize = GUIStyle.SmallFont.MeasureString(this.Text, false);
				Vector2 textPos = new Vector2(pos.X + (this.Size.X - textSize.X) / 2f, pos.Y - textSize.Y * 1.2f);
				Vector2 pos2 = textPos - Vector2.One;
				LocalizedString text = this.Text;
				Color color2 = Color.Black * a;
				GUIFont smallFont = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch, pos2, text, color2, null, 0, smallFont, ForceUpperCase.Inherit);
				Vector2 pos3 = textPos;
				LocalizedString text2 = this.Text;
				Color color3 = Color.White * a;
				smallFont = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch, pos3, text2, color3, null, 0, smallFont, ForceUpperCase.Inherit);
			}
		}

		// Token: 0x04000410 RID: 1040
		private float progress;

		// Token: 0x04000411 RID: 1041
		public float FadeTimer;

		// Token: 0x04000412 RID: 1042
		private Color fullColor;

		// Token: 0x04000413 RID: 1043
		private Color emptyColor;

		// Token: 0x04000414 RID: 1044
		private Vector2 worldPosition;

		// Token: 0x04000415 RID: 1045
		public Vector2 Size;

		// Token: 0x04000416 RID: 1046
		private readonly Submarine parentSub;

		// Token: 0x04000418 RID: 1048
		private string textTag;
	}
}
