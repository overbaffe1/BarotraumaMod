using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000092 RID: 146
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	public class GUIFont : GUISelector<GUIFontPrefab>
	{
		// Token: 0x060013E4 RID: 5092 RVA: 0x000BEB4C File Offset: 0x000BCD4C
		public GUIFont(string identifier) : base(identifier)
		{
		}

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x060013E5 RID: 5093 RVA: 0x000BEB55 File Offset: 0x000BCD55
		public bool HasValue
		{
			get
			{
				return this.Value != null;
			}
		}

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x060013E6 RID: 5094 RVA: 0x000BEB63 File Offset: 0x000BCD63
		[Nullable(2)]
		public ScalableFont Value
		{
			[NullableContext(2)]
			get
			{
				GUIFontPrefab activePrefab = this.Prefabs.ActivePrefab;
				if (activePrefab == null)
				{
					return null;
				}
				return activePrefab.Font;
			}
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x000BEB7B File Offset: 0x000BCD7B
		[return: Nullable(2)]
		public static implicit operator ScalableFont(GUIFont reference)
		{
			return reference.Value;
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x060013E8 RID: 5096 RVA: 0x000BEB84 File Offset: 0x000BCD84
		public bool ForceUpperCase
		{
			get
			{
				GUIFontPrefab activePrefab = this.Prefabs.ActivePrefab;
				ScalableFont scalableFont = (activePrefab != null) ? activePrefab.Font : null;
				return scalableFont != null && scalableFont.ForceUpperCase;
			}
		}

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x060013E9 RID: 5097 RVA: 0x000BEBB4 File Offset: 0x000BCDB4
		public uint Size
		{
			get
			{
				ScalableFont value = this.Value;
				if (value == null)
				{
					return 0U;
				}
				return value.Size;
			}
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x000BEBC7 File Offset: 0x000BCDC7
		[return: Nullable(2)]
		private ScalableFont GetFontForStr(LocalizedString str)
		{
			return this.GetFontForStr(str.Value);
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x000BEBD5 File Offset: 0x000BCDD5
		[return: Nullable(2)]
		public ScalableFont GetFontForStr(string str)
		{
			GUIFontPrefab activePrefab = this.Prefabs.ActivePrefab;
			if (activePrefab == null)
			{
				return null;
			}
			return activePrefab.GetFontForCategory(TextManager.GetSpeciallyHandledCategories(str));
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x000BEBF4 File Offset: 0x000BCDF4
		public void DrawString(SpriteBatch sb, LocalizedString text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, float layerDepth)
		{
			this.DrawString(sb, text.Value, position, color, rotation, origin, scale, spriteEffects, layerDepth);
		}

		// Token: 0x060013ED RID: 5101 RVA: 0x000BEC1C File Offset: 0x000BCE1C
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, float layerDepth)
		{
			ScalableFont fontForStr = this.GetFontForStr(text);
			if (fontForStr == null)
			{
				return;
			}
			fontForStr.DrawString(sb, text, position, color, rotation, origin, scale, spriteEffects, layerDepth, Alignment.TopLeft, Barotrauma.ForceUpperCase.Inherit);
		}

		// Token: 0x060013EE RID: 5102 RVA: 0x000BEC4C File Offset: 0x000BCE4C
		public void DrawString(SpriteBatch sb, LocalizedString text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffects, float layerDepth, Alignment alignment = Alignment.TopLeft)
		{
			this.DrawString(sb, text.Value, position, color, rotation, origin, scale, spriteEffects, layerDepth, alignment, Barotrauma.ForceUpperCase.Inherit);
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x000BEC78 File Offset: 0x000BCE78
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffects, float layerDepth, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			ScalableFont fontForStr = this.GetFontForStr(text);
			if (fontForStr == null)
			{
				return;
			}
			fontForStr.DrawString(sb, text, position, color, rotation, origin, scale, spriteEffects, layerDepth, alignment, forceUpperCase);
		}

		// Token: 0x060013F0 RID: 5104 RVA: 0x000BECA9 File Offset: 0x000BCEA9
		public void DrawString(SpriteBatch sb, LocalizedString text, Vector2 position, Color color, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit, bool italics = false)
		{
			this.DrawString(sb, text.Value, position, color, forceUpperCase, italics);
		}

		// Token: 0x060013F1 RID: 5105 RVA: 0x000BECBF File Offset: 0x000BCEBF
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit, bool italics = false)
		{
			ScalableFont fontForStr = this.GetFontForStr(text);
			if (fontForStr == null)
			{
				return;
			}
			fontForStr.DrawString(sb, text, position, color, forceUpperCase, italics);
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x000BECDC File Offset: 0x000BCEDC
		public void DrawStringWithColors(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffects, float layerDepth, [Nullable(new byte[]
		{
			0,
			1
		})] in ImmutableArray<RichTextData>? richTextData, int rtdOffset = 0, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			ScalableFont fontForStr = this.GetFontForStr(text);
			if (fontForStr == null)
			{
				return;
			}
			fontForStr.DrawStringWithColors(sb, text, position, color, rotation, origin, scale, spriteEffects, layerDepth, richTextData, rtdOffset, alignment, forceUpperCase);
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x000BED11 File Offset: 0x000BCF11
		public Vector2 MeasureString(LocalizedString str, bool removeExtraSpacing = false)
		{
			ScalableFont fontForStr = this.GetFontForStr(str);
			if (fontForStr == null)
			{
				return Vector2.Zero;
			}
			return fontForStr.MeasureString(str, removeExtraSpacing);
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x000BED2C File Offset: 0x000BCF2C
		public Vector2 MeasureChar(char c)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<char>(c);
			ScalableFont fontForStr = this.GetFontForStr(defaultInterpolatedStringHandler.ToStringAndClear());
			if (fontForStr == null)
			{
				return Vector2.Zero;
			}
			return fontForStr.MeasureChar(c);
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x000BED67 File Offset: 0x000BCF67
		public string WrapText(string text, float width)
		{
			ScalableFont fontForStr = this.GetFontForStr(text);
			return ((fontForStr != null) ? fontForStr.WrapText(text, width) : null) ?? text;
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x000BED83 File Offset: 0x000BCF83
		public string WrapText(string text, float width, int requestCharPos, out Vector2 requestedCharPos)
		{
			requestedCharPos = default(Vector2);
			ScalableFont fontForStr = this.GetFontForStr(text);
			return ((fontForStr != null) ? fontForStr.WrapText(text, width, requestCharPos, out requestedCharPos) : null) ?? text;
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x000BEDAC File Offset: 0x000BCFAC
		public string WrapText(string text, float width, out Vector2[] allCharPositions)
		{
			ScalableFont scalableFont = this.GetFontForStr(text);
			if (scalableFont != null)
			{
				return scalableFont.WrapText(text, width, out allCharPositions);
			}
			allCharPositions = (from _ in Enumerable.Range(0, text.Length + 1)
			select Vector2.Zero).ToArray<Vector2>();
			return text;
		}

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x060013F8 RID: 5112 RVA: 0x000BEE08 File Offset: 0x000BD008
		public float LineHeight
		{
			get
			{
				ScalableFont value = this.Value;
				if (value == null)
				{
					return 0f;
				}
				return value.LineHeight;
			}
		}
	}
}
