using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000091 RID: 145
	[NullableContext(1)]
	[Nullable(0)]
	public class GUIFontPrefab : GUIPrefab
	{
		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x060013D5 RID: 5077 RVA: 0x000BE46E File Offset: 0x000BC66E
		[Nullable(2)]
		public ScalableFont Font
		{
			[NullableContext(2)]
			get
			{
				if (this.Language != GameSettings.CurrentConfig.Language)
				{
					this.LoadFont();
				}
				return this.font;
			}
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x000BE494 File Offset: 0x000BC694
		[NullableContext(2)]
		public ScalableFont GetFontForCategory(TextManager.SpeciallyHandledCharCategory category)
		{
			if (this.Language != GameSettings.CurrentConfig.Language)
			{
				this.LoadFont();
			}
			if (this.font == null)
			{
				return null;
			}
			if (this.specialHandlingFonts == null)
			{
				return this.font;
			}
			if (this.font.SpeciallyHandledCharCategory.HasFlag(category))
			{
				return this.font;
			}
			ScalableFont resultFont;
			if (this.specialHandlingFonts.TryGetValue(category, out resultFont))
			{
				return resultFont;
			}
			if (!this.specialHandlingFonts.TryGetValue(TextManager.SpeciallyHandledCharCategory.CJK, out resultFont))
			{
				return this.font;
			}
			return resultFont;
		}

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x060013D7 RID: 5079 RVA: 0x000BE525 File Offset: 0x000BC725
		// (set) Token: 0x060013D8 RID: 5080 RVA: 0x000BE52D File Offset: 0x000BC72D
		public LanguageIdentifier Language { get; private set; }

		// Token: 0x060013D9 RID: 5081 RVA: 0x000BE536 File Offset: 0x000BC736
		public GUIFontPrefab(ContentXElement element, UIStyleFile file) : base(element, file)
		{
			this.element = element;
			this.LoadFont();
		}

		// Token: 0x060013DA RID: 5082 RVA: 0x000BE550 File Offset: 0x000BC750
		public void LoadFont()
		{
			string fontPath = this.GetFontFilePath(this.element);
			uint size = this.GetFontSize(this.element, 14U);
			bool dynamicLoading = this.GetFontDynamicLoading(this.element);
			TextManager.SpeciallyHandledCharCategory shcc = this.GetShcc(this.element);
			ScalableFont scalableFont = this.font;
			if (scalableFont != null)
			{
				scalableFont.Dispose();
			}
			ImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont> immutableDictionary = this.specialHandlingFonts;
			if (immutableDictionary != null)
			{
				immutableDictionary.Values.ForEach(delegate(ScalableFont f)
				{
					f.Dispose();
				});
			}
			this.font = new ScalableFont(fontPath, size, GameMain.Instance.GraphicsDevice, dynamicLoading, shcc)
			{
				ForceUpperCase = this.element.GetAttributeBool("forceuppercase", false)
			};
			Dictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont> fallbackFonts = new Dictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont>();
			foreach (TextManager.SpeciallyHandledCharCategory flag in TextManager.SpeciallyHandledCharCategories)
			{
				if (!shcc.HasFlag(flag))
				{
					ScalableFont extractedFont = this.ExtractFont(flag, this.element);
					if (extractedFont != null)
					{
						fallbackFonts.Add(flag, extractedFont);
					}
				}
			}
			fallbackFonts.Values.ForEach(delegate(ScalableFont ff)
			{
				ff.ForceUpperCase = this.font.ForceUpperCase;
			});
			this.specialHandlingFonts = fallbackFonts.ToImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont>();
			this.Language = GameSettings.CurrentConfig.Language;
		}

		// Token: 0x060013DB RID: 5083 RVA: 0x000BE6A8 File Offset: 0x000BC8A8
		public override void Dispose()
		{
			ScalableFont scalableFont = this.font;
			if (scalableFont != null)
			{
				scalableFont.Dispose();
			}
			this.font = null;
			ImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont> immutableDictionary = this.specialHandlingFonts;
			if (immutableDictionary != null)
			{
				immutableDictionary.Values.ForEach(delegate(ScalableFont f)
				{
					f.Dispose();
				});
			}
			this.specialHandlingFonts = null;
		}

		// Token: 0x060013DC RID: 5084 RVA: 0x000BE70C File Offset: 0x000BC90C
		[return: Nullable(2)]
		private ScalableFont ExtractFont(TextManager.SpeciallyHandledCharCategory flag, ContentXElement element)
		{
			GUIFontPrefab.<>c__DisplayClass13_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.flag = flag;
			foreach (ContentXElement subElement in element.Elements().Reverse<ContentXElement>())
			{
				Identifier identifier = subElement.NameAsIdentifier();
				if (!(identifier != "override") && ScalableFont.ExtractShccFromXElement(subElement).HasFlag(CS$<>8__locals1.flag))
				{
					XElement xelement = subElement;
					ScalableFont scalableFont = this.font;
					uint overrideFontSize = this.GetFontSize(xelement, (scalableFont != null) ? scalableFont.Size : 14U);
					return new ScalableFont(subElement, overrideFontSize, GameMain.Instance.GraphicsDevice);
				}
			}
			TextManager.SpeciallyHandledCharCategory flag2 = CS$<>8__locals1.flag;
			ScalableFont result;
			if (flag2 != TextManager.SpeciallyHandledCharCategory.CJK)
			{
				if (flag2 != TextManager.SpeciallyHandledCharCategory.Cyrillic)
				{
					result = null;
				}
				else
				{
					result = this.<ExtractFont>g__hardcodedFallback|13_0("Content/Fonts/Oswald-Bold.ttf", ref CS$<>8__locals1);
				}
			}
			else
			{
				result = this.<ExtractFont>g__hardcodedFallback|13_0("Content/Fonts/NotoSans/NotoSansCJKsc-Bold.otf", ref CS$<>8__locals1);
			}
			return result;
		}

		// Token: 0x060013DD RID: 5085 RVA: 0x000BE814 File Offset: 0x000BCA14
		[return: Nullable(2)]
		private string GetFontFilePath(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				if (this.IsValidOverride(subElement))
				{
					ContentPath attributeContentPath = subElement.GetAttributeContentPath("file");
					return (attributeContentPath != null) ? attributeContentPath.Value : null;
				}
			}
			ContentPath attributeContentPath2 = element.GetAttributeContentPath("file");
			if (attributeContentPath2 == null)
			{
				return null;
			}
			return attributeContentPath2.Value;
		}

		// Token: 0x060013DE RID: 5086 RVA: 0x000BE89C File Offset: 0x000BCA9C
		private uint GetFontSize(XElement element, uint defaultSize = 14U)
		{
			foreach (XElement subElement in element.Elements())
			{
				if (this.IsValidOverride(subElement))
				{
					uint overrideFontSize = this.GetFontSize(subElement, 0U);
					if (overrideFontSize > 0U)
					{
						return (uint)Math.Round((double)(overrideFontSize * GameSettings.CurrentConfig.Graphics.TextScale));
					}
				}
			}
			foreach (XElement subElement2 in element.Elements())
			{
				if (subElement2.Name.ToString().Equals("size", StringComparison.OrdinalIgnoreCase))
				{
					Point maxResolution = subElement2.GetAttributePoint("maxresolution", new Point(int.MaxValue, int.MaxValue));
					if (GameMain.GraphicsWidth <= maxResolution.X && GameMain.GraphicsHeight <= maxResolution.Y)
					{
						int rawSize = base.ParseSize(subElement2, "size");
						return (uint)Math.Round((double)((float)rawSize * GameSettings.CurrentConfig.Graphics.TextScale));
					}
				}
			}
			return (uint)Math.Round((double)(defaultSize * GameSettings.CurrentConfig.Graphics.TextScale));
		}

		// Token: 0x060013DF RID: 5087 RVA: 0x000BE9F4 File Offset: 0x000BCBF4
		private bool GetFontDynamicLoading(XElement element)
		{
			foreach (XElement subElement in element.Elements())
			{
				if (this.IsValidOverride(subElement))
				{
					return subElement.GetAttributeBool("dynamicloading", false);
				}
			}
			return element.GetAttributeBool("dynamicloading", false);
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x000BEA60 File Offset: 0x000BCC60
		private TextManager.SpeciallyHandledCharCategory GetShcc(XElement element)
		{
			foreach (XElement subElement in element.Elements())
			{
				if (this.IsValidOverride(subElement))
				{
					return ScalableFont.ExtractShccFromXElement(subElement);
				}
			}
			return ScalableFont.ExtractShccFromXElement(element);
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x000BEAC0 File Offset: 0x000BCCC0
		private bool IsValidOverride(XElement element)
		{
			if (!element.IsOverride())
			{
				return false;
			}
			Identifier[] languages = element.GetAttributeIdentifierArray("language", Array.Empty<Identifier>(), true);
			return languages.Any((Identifier l) => l.ToLanguageIdentifier() == GameSettings.CurrentConfig.Language);
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x000BEB21 File Offset: 0x000BCD21
		[CompilerGenerated]
		private ScalableFont <ExtractFont>g__hardcodedFallback|13_0(string path, ref GUIFontPrefab.<>c__DisplayClass13_0 A_2)
		{
			ScalableFont scalableFont = this.font;
			return new ScalableFont(path, (scalableFont != null) ? scalableFont.Size : 0U, GameMain.Instance.GraphicsDevice, true, A_2.flag);
		}

		// Token: 0x040009DF RID: 2527
		private readonly ContentXElement element;

		// Token: 0x040009E0 RID: 2528
		[Nullable(2)]
		private ScalableFont font;

		// Token: 0x040009E1 RID: 2529
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private ImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ScalableFont> specialHandlingFonts;
	}
}
