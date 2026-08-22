using System;
using System.Collections.Generic;
using System.Text;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B5 RID: 1461
	internal class ItemLabel : ItemComponent, IDrawableComponent, IHasExtraTextPickerEntries, IServerSerializable, INetSerializable
	{
		// Token: 0x170016DB RID: 5851
		// (get) Token: 0x06005AD9 RID: 23257 RVA: 0x002E98A9 File Offset: 0x002E7AA9
		// (set) Token: 0x06005ADA RID: 23258 RVA: 0x002E98B1 File Offset: 0x002E7AB1
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "The amount of padding around the text in pixels (left,top,right,bottom).", "", false)]
		public Vector4 Padding
		{
			get
			{
				return this.padding;
			}
			set
			{
				this.padding = value;
				this.TextBlock.Padding = value * this.item.Scale;
			}
		}

		// Token: 0x170016DC RID: 5852
		// (get) Token: 0x06005ADB RID: 23259 RVA: 0x002E98D6 File Offset: 0x002E7AD6
		// (set) Token: 0x06005ADC RID: 23260 RVA: 0x002E98E0 File Offset: 0x002E7AE0
		[Serialize("", IsPropertySaveable.Yes, "The text displayed in the label.", "Label.", true)]
		[Editable(MaxLength = 100)]
		public string Text
		{
			get
			{
				return this.text;
			}
			set
			{
				if (value == this.text || this.item.Rect.Width < 5)
				{
					return;
				}
				if (this.TextBlock.Rect.Width != this.item.Rect.Width || this.textBlock.Rect.Height != this.item.Rect.Height)
				{
					this.textBlock = null;
				}
				this.text = value;
				this.SetDisplayText(value);
				this.UpdateScrollingText();
			}
		}

		// Token: 0x170016DD RID: 5853
		// (get) Token: 0x06005ADD RID: 23261 RVA: 0x002E996E File Offset: 0x002E7B6E
		// (set) Token: 0x06005ADE RID: 23262 RVA: 0x002E9976 File Offset: 0x002E7B76
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Whether or not to skip localization and always display the raw value.", "", false)]
		public bool IgnoreLocalization
		{
			get
			{
				return this.ignoreLocalization;
			}
			set
			{
				this.ignoreLocalization = value;
				this.SetDisplayText(this.Text);
			}
		}

		// Token: 0x170016DE RID: 5854
		// (get) Token: 0x06005ADF RID: 23263 RVA: 0x002E998B File Offset: 0x002E7B8B
		// (set) Token: 0x06005AE0 RID: 23264 RVA: 0x002E9993 File Offset: 0x002E7B93
		public LocalizedString DisplayText { get; private set; }

		// Token: 0x170016DF RID: 5855
		// (get) Token: 0x06005AE1 RID: 23265 RVA: 0x002E999C File Offset: 0x002E7B9C
		// (set) Token: 0x06005AE2 RID: 23266 RVA: 0x002E99A4 File Offset: 0x002E7BA4
		[Editable]
		[Serialize("0,0,0,255", IsPropertySaveable.Yes, "The color of the text displayed on the label (R,G,B,A).", "", true)]
		public Color TextColor
		{
			get
			{
				return this.textColor;
			}
			set
			{
				if (this.textBlock != null)
				{
					this.textBlock.TextColor = value;
				}
				this.textColor = value;
			}
		}

		// Token: 0x170016E0 RID: 5856
		// (get) Token: 0x06005AE3 RID: 23267 RVA: 0x002E99C1 File Offset: 0x002E7BC1
		// (set) Token: 0x06005AE4 RID: 23268 RVA: 0x002E99E4 File Offset: 0x002E7BE4
		[Editable(0f, 10f, 1)]
		[Serialize(1f, IsPropertySaveable.Yes, "The scale of the text displayed on the label.", "", true)]
		public float TextScale
		{
			get
			{
				if (this.textBlock != null)
				{
					return this.textBlock.TextScale / this.BaseToRealTextScaleFactor;
				}
				return 1f;
			}
			set
			{
				if (this.textBlock != null)
				{
					float prevScale = this.TextBlock.TextScale;
					this.textBlock.TextScale = MathHelper.Clamp(value * this.BaseToRealTextScaleFactor, 0.1f, 10f);
					if (!MathUtils.NearlyEqual(prevScale, this.TextBlock.TextScale, 0.0001f))
					{
						this.SetScrollingText();
					}
				}
			}
		}

		// Token: 0x170016E1 RID: 5857
		// (get) Token: 0x06005AE5 RID: 23269 RVA: 0x002E9A45 File Offset: 0x002E7C45
		// (set) Token: 0x06005AE6 RID: 23270 RVA: 0x002E9A50 File Offset: 0x002E7C50
		[Serialize(false, IsPropertySaveable.Yes, "Should the text scroll horizontally across the item if it's too long to be displayed all at once.", "", false)]
		public bool Scrollable
		{
			get
			{
				return this.scrollable;
			}
			set
			{
				this.scrollable = value;
				this.IsActive = (value || this.parseSpecialTextTagOnStart);
				this.TextBlock.Wrap = !this.scrollable;
				this.TextBlock.TextAlignment = (this.scrollable ? Alignment.CenterLeft : Alignment.Center);
			}
		}

		// Token: 0x170016E2 RID: 5858
		// (get) Token: 0x06005AE7 RID: 23271 RVA: 0x002E9AA3 File Offset: 0x002E7CA3
		// (set) Token: 0x06005AE8 RID: 23272 RVA: 0x002E9AAB File Offset: 0x002E7CAB
		[Serialize(20f, IsPropertySaveable.Yes, "How fast the text scrolls across the item (only valid if Scrollable is set to true).", "", false)]
		public float ScrollSpeed { get; set; }

		// Token: 0x170016E3 RID: 5859
		// (get) Token: 0x06005AE9 RID: 23273 RVA: 0x002E9AB4 File Offset: 0x002E7CB4
		private GUITextBlock TextBlock
		{
			get
			{
				if (this.textBlock == null)
				{
					this.RecreateTextBlock();
				}
				return this.textBlock;
			}
		}

		// Token: 0x06005AEA RID: 23274 RVA: 0x002E9ACA File Offset: 0x002E7CCA
		public ItemLabel(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06005AEB RID: 23275 RVA: 0x002E9AD4 File Offset: 0x002E7CD4
		public IEnumerable<string> GetExtraTextPickerEntries()
		{
			return ItemLabel.SpecialTextTags;
		}

		// Token: 0x06005AEC RID: 23276 RVA: 0x002E9ADC File Offset: 0x002E7CDC
		private void SetScrollingText()
		{
			if (!this.scrollable)
			{
				return;
			}
			float totalWidth = this.textBlock.Font.MeasureString(this.DisplayText, false).X * this.TextBlock.TextScale;
			float textAreaWidth = Math.Max((float)this.textBlock.Rect.Width - this.textBlock.Padding.X - this.textBlock.Padding.Z, 0f);
			if (totalWidth >= textAreaWidth)
			{
				this.needsScrolling = true;
				float spaceWidth = this.textBlock.Font.MeasureChar(' ').X * this.TextBlock.TextScale;
				this.scrollingText = new string(' ', (int)Math.Ceiling((double)(textAreaWidth / spaceWidth))) + this.DisplayText.Value;
				this.scrollPadding = 0f;
				this.charWidths = new float[this.scrollingText.Length];
				for (int i = 0; i < this.scrollingText.Length; i++)
				{
					float charWidth = this.TextBlock.Font.MeasureChar(this.scrollingText[i]).X * this.TextBlock.TextScale;
					this.scrollPadding = Math.Max(charWidth, this.scrollPadding);
					this.charWidths[i] = charWidth;
				}
				this.scrollIndex = MathHelper.Clamp(this.scrollIndex, 0, this.DisplayText.Length);
				return;
			}
			this.needsScrolling = false;
			this.TextBlock.Text = (this.scrollingText = this.DisplayText.Value);
			this.scrollPadding = 0f;
			this.scrollAmount = 0f;
			this.scrollIndex = 0;
		}

		// Token: 0x06005AED RID: 23277 RVA: 0x002E9CA0 File Offset: 0x002E7EA0
		private void SetDisplayText(string value)
		{
			if (ItemLabel.SpecialTextTags.Contains(value))
			{
				this.parseSpecialTextTagOnStart = true;
				this.IsActive = true;
			}
			this.DisplayText = (this.IgnoreLocalization ? value : TextManager.Get(value).Fallback(value, true));
			this.TextBlock.Text = this.DisplayText;
			if (Screen.Selected == GameMain.SubEditorScreen && this.Scrollable)
			{
				this.TextBlock.Text = ToolBox.LimitString(this.DisplayText, this.TextBlock.Font, this.item.Rect.Width);
			}
			this.SetScrollingText();
		}

		// Token: 0x170016E4 RID: 5860
		// (get) Token: 0x06005AEE RID: 23278 RVA: 0x002E9D5B File Offset: 0x002E7F5B
		private float BaseToRealTextScaleFactor
		{
			get
			{
				return 12f / GUIStyle.UnscaledSmallFont.Size;
			}
		}

		// Token: 0x06005AEF RID: 23279 RVA: 0x002E9D70 File Offset: 0x002E7F70
		private void RecreateTextBlock()
		{
			this.textBlock = new GUITextBlock(new RectTransform(this.item.Rect.Size, null, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(this.textColor), GUIStyle.UnscaledSmallFont, this.scrollable ? Alignment.CenterLeft : Alignment.Center, !this.scrollable, null, null)
			{
				TextDepth = this.item.SpriteDepth - 1E-05f,
				RoundToNearestPixel = false,
				TextScale = this.TextScale * this.BaseToRealTextScaleFactor,
				Padding = this.padding * this.item.Scale
			};
		}

		// Token: 0x06005AF0 RID: 23280 RVA: 0x002E9E38 File Offset: 0x002E8038
		private void ParseSpecialTextTag()
		{
			string a = this.text;
			if (a == "[CurrentLocationName]")
			{
				Level loaded = Level.Loaded;
				string text;
				if (loaded == null)
				{
					text = null;
				}
				else
				{
					Location startLocation = loaded.StartLocation;
					text = ((startLocation != null) ? startLocation.DisplayName.Value : null);
				}
				this.SetDisplayText(text ?? string.Empty);
				return;
			}
			if (a == "[CurrentBiomeName]")
			{
				Level loaded2 = Level.Loaded;
				string text2;
				if (loaded2 == null)
				{
					text2 = null;
				}
				else
				{
					LevelData levelData = loaded2.LevelData;
					if (levelData == null)
					{
						text2 = null;
					}
					else
					{
						Biome biome = levelData.Biome;
						text2 = ((biome != null) ? biome.DisplayName.Value : null);
					}
				}
				this.SetDisplayText(text2 ?? string.Empty);
				return;
			}
			if (!(a == "[CurrentSubName]"))
			{
				return;
			}
			Submarine submarine = this.item.Submarine;
			string text3;
			if (submarine == null)
			{
				text3 = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				text3 = ((info != null) ? info.DisplayName.Value : null);
			}
			this.SetDisplayText(text3 ?? string.Empty);
		}

		// Token: 0x06005AF1 RID: 23281 RVA: 0x002E9F1C File Offset: 0x002E811C
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.parseSpecialTextTagOnStart)
			{
				this.ParseSpecialTextTag();
				this.parseSpecialTextTagOnStart = false;
			}
			if (!this.scrollable)
			{
				this.IsActive = false;
				return;
			}
			if (this.scrollingText == null)
			{
				this.SetScrollingText();
			}
			if (!this.needsScrolling)
			{
				return;
			}
			this.scrollAmount -= deltaTime * this.ScrollSpeed;
			this.UpdateScrollingText();
		}

		// Token: 0x06005AF2 RID: 23282 RVA: 0x002E9F80 File Offset: 0x002E8180
		private void UpdateScrollingText()
		{
			if (!this.scrollable || !this.needsScrolling)
			{
				return;
			}
			float currLength = 0f;
			if (this.sb == null)
			{
				this.sb = new StringBuilder();
			}
			this.sb.Clear();
			float textAreaWidth = (float)this.textBlock.Rect.Width - this.textBlock.Padding.X - this.textBlock.Padding.Z;
			for (int i = this.scrollIndex; i < this.scrollingText.Length; i++)
			{
				if (i == this.scrollIndex && this.scrollAmount < -this.charWidths[i])
				{
					this.scrollIndex++;
					this.scrollAmount = 0f;
					if (this.scrollIndex >= this.scrollingText.Length)
					{
						this.scrollIndex = 0;
						break;
					}
				}
				else
				{
					if (this.scrollAmount + (currLength + this.charWidths[i] + this.scrollPadding) >= textAreaWidth)
					{
						break;
					}
					currLength += this.charWidths[i];
					this.sb.Append(this.scrollingText[i]);
				}
			}
			this.TextBlock.Text = this.sb.ToString();
		}

		// Token: 0x06005AF3 RID: 23283 RVA: 0x002EA0C0 File Offset: 0x002E82C0
		public override void OnScaleChanged()
		{
			this.RecreateTextBlock();
			this.SetDisplayText(this.Text);
			this.prevScale = this.item.Scale;
			this.prevRect = this.item.Rect;
		}

		// Token: 0x06005AF4 RID: 23284 RVA: 0x002EA0F8 File Offset: 0x002E82F8
		public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.item.ParentInventory != null)
			{
				return;
			}
			if (editing && (!MathUtils.NearlyEqual(this.prevScale, this.item.Scale, 0.0001f) || this.prevRect != this.item.Rect))
			{
				this.RecreateTextBlock();
				this.SetDisplayText(this.Text);
				this.prevScale = this.item.Scale;
				this.prevRect = this.item.Rect;
			}
			Vector2 drawPos = new Vector2(this.item.DrawPosition.X - (float)this.item.Rect.Width / 2f, -(this.item.DrawPosition.Y + (float)this.item.Rect.Height / 2f));
			this.textBlock.TextDepth = this.item.SpriteDepth - 0.0001f;
			this.textBlock.TextOffset = drawPos - this.textBlock.Rect.Location.ToVector2() + (editing ? Vector2.Zero : new Vector2(this.scrollAmount + this.scrollPadding, 0f));
			this.textBlock.DrawManually(spriteBatch, false, true);
		}

		// Token: 0x06005AF5 RID: 23285 RVA: 0x002EA253 File Offset: 0x002E8453
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Text = msg.ReadString();
		}

		// Token: 0x170016E5 RID: 5861
		// (get) Token: 0x06005AF6 RID: 23286 RVA: 0x002EA261 File Offset: 0x002E8461
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005AF7 RID: 23287 RVA: 0x002EA268 File Offset: 0x002E8468
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "set_text"))
			{
				if (!(name == "set_text_color"))
				{
					return;
				}
				if (signal.value != this.prevColorSignal)
				{
					this.TextColor = XMLExtensions.ParseColor(signal.value, false);
					this.prevColorSignal = signal.value;
				}
				return;
			}
			else
			{
				if (this.Text == signal.value)
				{
					return;
				}
				this.Text = signal.value;
				return;
			}
		}

		// Token: 0x04002E4E RID: 11854
		private GUITextBlock textBlock;

		// Token: 0x04002E4F RID: 11855
		private Color textColor;

		// Token: 0x04002E50 RID: 11856
		private float scrollAmount;

		// Token: 0x04002E51 RID: 11857
		private string scrollingText;

		// Token: 0x04002E52 RID: 11858
		private float scrollPadding;

		// Token: 0x04002E53 RID: 11859
		private int scrollIndex;

		// Token: 0x04002E54 RID: 11860
		private bool needsScrolling;

		// Token: 0x04002E55 RID: 11861
		private float[] charWidths;

		// Token: 0x04002E56 RID: 11862
		private float prevScale;

		// Token: 0x04002E57 RID: 11863
		private Rectangle prevRect;

		// Token: 0x04002E58 RID: 11864
		private StringBuilder sb;

		// Token: 0x04002E59 RID: 11865
		private Vector4 padding;

		// Token: 0x04002E5A RID: 11866
		private string text;

		// Token: 0x04002E5B RID: 11867
		private bool ignoreLocalization;

		// Token: 0x04002E5D RID: 11869
		private bool scrollable;

		// Token: 0x04002E5F RID: 11871
		private static readonly string[] SpecialTextTags = new string[]
		{
			"[CurrentLocationName]",
			"[CurrentBiomeName]",
			"[CurrentSubName]"
		};

		// Token: 0x04002E60 RID: 11872
		private bool parseSpecialTextTagOnStart;

		// Token: 0x04002E61 RID: 11873
		private const float BaseTextSize = 12f;

		// Token: 0x04002E62 RID: 11874
		private string prevColorSignal;
	}
}
