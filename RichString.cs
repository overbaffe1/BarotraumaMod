using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000374 RID: 884
	[NullableContext(1)]
	[Nullable(0)]
	public class RichString
	{
		// Token: 0x170011AE RID: 4526
		// (get) Token: 0x06004379 RID: 17273 RVA: 0x002535C7 File Offset: 0x002517C7
		public string SanitizedValue
		{
			get
			{
				if (this.MustRetrieveValue())
				{
					this.RetrieveValue();
				}
				return this.cachedSanitizedValue;
			}
		}

		// Token: 0x170011AF RID: 4527
		// (get) Token: 0x0600437A RID: 17274 RVA: 0x002535DD File Offset: 0x002517DD
		public int Length
		{
			get
			{
				return this.SanitizedValue.Length;
			}
		}

		// Token: 0x170011B0 RID: 4528
		// (get) Token: 0x0600437B RID: 17275 RVA: 0x002535EA File Offset: 0x002517EA
		// (set) Token: 0x0600437C RID: 17276 RVA: 0x002535F2 File Offset: 0x002517F2
		public LocalizedString NestedStr { get; private set; }

		// Token: 0x170011B1 RID: 4529
		// (get) Token: 0x0600437D RID: 17277 RVA: 0x002535FB File Offset: 0x002517FB
		public bool Loaded
		{
			get
			{
				return this.loaded;
			}
		}

		// Token: 0x170011B2 RID: 4530
		// (get) Token: 0x0600437E RID: 17278 RVA: 0x00253604 File Offset: 0x00251804
		private bool FontOrStyleForceUpperCase
		{
			get
			{
				GUIFont guifont = this.font;
				if (guifont == null || !guifont.ForceUpperCase)
				{
					GUIComponentStyle guicomponentStyle = this.componentStyle;
					return guicomponentStyle != null && guicomponentStyle.ForceUpperCase;
				}
				return true;
			}
		}

		// Token: 0x170011B3 RID: 4531
		// (get) Token: 0x0600437F RID: 17279 RVA: 0x00253637 File Offset: 0x00251837
		// (set) Token: 0x06004380 RID: 17280 RVA: 0x0025363F File Offset: 0x0025183F
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<RichTextData>? RichTextData { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] private set; }

		// Token: 0x06004381 RID: 17281 RVA: 0x00253648 File Offset: 0x00251848
		[NullableContext(2)]
		private RichString([Nullable(1)] LocalizedString nestedStr, bool shouldParseRichTextData, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, string> postProcess = null, GUIFont font = null, GUIComponentStyle componentStyle = null) : this(nestedStr, shouldParseRichTextData, postProcess)
		{
			this.font = font;
			this.componentStyle = componentStyle;
		}

		// Token: 0x06004382 RID: 17282 RVA: 0x00253664 File Offset: 0x00251864
		private RichString(LocalizedString nestedStr, bool shouldParseRichTextData, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, string> postProcess = null)
		{
			this.originalStr = nestedStr;
			this.NestedStr = this.originalStr;
			this.shouldParseRichTextData = shouldParseRichTextData;
			this.postProcess = postProcess;
			this.SanitizedString = new StripRichTagsLString(this);
			this.font = null;
			this.componentStyle = null;
		}

		// Token: 0x06004383 RID: 17283 RVA: 0x002536C8 File Offset: 0x002518C8
		public static RichString Rich(LocalizedString str, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, string> postProcess = null)
		{
			return new RichString(str, true, postProcess);
		}

		// Token: 0x06004384 RID: 17284 RVA: 0x002536D2 File Offset: 0x002518D2
		public static RichString Plain(LocalizedString str)
		{
			return new RichString(str, false, null);
		}

		// Token: 0x06004385 RID: 17285 RVA: 0x002536DC File Offset: 0x002518DC
		public static implicit operator LocalizedString(RichString richStr)
		{
			return richStr.NestedStr;
		}

		// Token: 0x06004386 RID: 17286 RVA: 0x002536E4 File Offset: 0x002518E4
		public static implicit operator RichString(LocalizedString lStr)
		{
			return RichString.Plain(lStr ?? string.Empty);
		}

		// Token: 0x06004387 RID: 17287 RVA: 0x002536FA File Offset: 0x002518FA
		public static implicit operator RichString(string str)
		{
			return str;
		}

		// Token: 0x06004388 RID: 17288 RVA: 0x00253708 File Offset: 0x00251908
		protected virtual bool MustRetrieveValue()
		{
			return this.NestedStr.Loaded != this.loaded || this.language != GameSettings.CurrentConfig.Language || this.languageVersion != TextManager.LanguageVersion || this.FontOrStyleForceUpperCase != this.forceUpperCase;
		}

		// Token: 0x06004389 RID: 17289 RVA: 0x00253760 File Offset: 0x00251960
		public void RetrieveValue()
		{
			this.NestedStr = (this.FontOrStyleForceUpperCase ? this.originalStr.ToUpper() : this.originalStr);
			this.forceUpperCase = this.FontOrStyleForceUpperCase;
			if (this.shouldParseRichTextData)
			{
				this.RichTextData = Barotrauma.RichTextData.GetRichTextData(this.NestedStr.Value, out this.cachedSanitizedValue);
			}
			else
			{
				this.cachedSanitizedValue = this.NestedStr.Value;
			}
			if (this.postProcess != null)
			{
				this.cachedSanitizedValue = this.postProcess(this.cachedSanitizedValue);
			}
			this.language = GameSettings.CurrentConfig.Language;
			this.languageVersion = TextManager.LanguageVersion;
			this.loaded = this.NestedStr.Loaded;
		}

		// Token: 0x0600438A RID: 17290 RVA: 0x0025381C File Offset: 0x00251A1C
		[NullableContext(2)]
		[return: Nullable(1)]
		public RichString CaseTiedToFontAndStyle(GUIFont font, GUIComponentStyle componentStyle)
		{
			return new RichString(this.originalStr, this.shouldParseRichTextData, this.postProcess, font, componentStyle);
		}

		// Token: 0x0600438B RID: 17291 RVA: 0x00253837 File Offset: 0x00251A37
		public RichString ToUpper()
		{
			return new RichString(this.NestedStr.ToUpper(), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x0600438C RID: 17292 RVA: 0x00253855 File Offset: 0x00251A55
		public RichString ToLower()
		{
			return new RichString(this.NestedStr.ToLower(), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x0600438D RID: 17293 RVA: 0x00253873 File Offset: 0x00251A73
		public RichString Replace(string from, string to, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new RichString(this.NestedStr.Replace(from, to, stringComparison), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x0600438E RID: 17294 RVA: 0x00253899 File Offset: 0x00251A99
		public override string ToString()
		{
			return this.SanitizedValue;
		}

		// Token: 0x0600438F RID: 17295 RVA: 0x002538A1 File Offset: 0x00251AA1
		public bool Contains(string str, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return this.SanitizedValue.Contains(str, stringComparison);
		}

		// Token: 0x06004390 RID: 17296 RVA: 0x002538B0 File Offset: 0x00251AB0
		public bool Contains(char chr, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return this.SanitizedValue.Contains(chr, stringComparison);
		}

		// Token: 0x06004391 RID: 17297 RVA: 0x002538C0 File Offset: 0x00251AC0
		[NullableContext(2)]
		public static bool operator ==(RichString a, RichString b)
		{
			return ((a != null) ? a.SanitizedValue : null) == ((b != null) ? b.SanitizedValue : null) && ((a != null) ? a.font : null) == ((b != null) ? b.font : null) && ((a != null) ? a.componentStyle : null) == ((b != null) ? b.componentStyle : null);
		}

		// Token: 0x06004392 RID: 17298 RVA: 0x00253922 File Offset: 0x00251B22
		[NullableContext(2)]
		public static bool operator !=(RichString a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x06004393 RID: 17299 RVA: 0x0025392E File Offset: 0x00251B2E
		[NullableContext(2)]
		public static bool operator ==(RichString a, LocalizedString b)
		{
			return ((a != null) ? a.SanitizedValue : null) == ((b != null) ? b.Value : null);
		}

		// Token: 0x06004394 RID: 17300 RVA: 0x0025394D File Offset: 0x00251B4D
		[NullableContext(2)]
		public static bool operator !=(RichString a, LocalizedString b)
		{
			return !(a == b);
		}

		// Token: 0x06004395 RID: 17301 RVA: 0x00253959 File Offset: 0x00251B59
		[NullableContext(2)]
		public static bool operator ==(LocalizedString a, RichString b)
		{
			return ((a != null) ? a.Value : null) == ((b != null) ? b.SanitizedValue : null);
		}

		// Token: 0x06004396 RID: 17302 RVA: 0x00253978 File Offset: 0x00251B78
		[NullableContext(2)]
		public static bool operator !=(LocalizedString a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x06004397 RID: 17303 RVA: 0x00253984 File Offset: 0x00251B84
		[NullableContext(2)]
		public static bool operator ==(RichString a, string b)
		{
			return ((a != null) ? a.SanitizedValue : null) == b;
		}

		// Token: 0x06004398 RID: 17304 RVA: 0x00253998 File Offset: 0x00251B98
		[NullableContext(2)]
		public static bool operator !=(RichString a, string b)
		{
			return !(a == b);
		}

		// Token: 0x06004399 RID: 17305 RVA: 0x002539A4 File Offset: 0x00251BA4
		[NullableContext(2)]
		public static bool operator ==(string a, RichString b)
		{
			return a == ((b != null) ? b.SanitizedValue : null);
		}

		// Token: 0x0600439A RID: 17306 RVA: 0x002539B8 File Offset: 0x00251BB8
		[NullableContext(2)]
		public static bool operator !=(string a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x04002365 RID: 9061
		protected bool loaded;

		// Token: 0x04002366 RID: 9062
		protected LanguageIdentifier language = LanguageIdentifier.None;

		// Token: 0x04002367 RID: 9063
		private int languageVersion;

		// Token: 0x04002368 RID: 9064
		protected string cachedSanitizedValue = "";

		// Token: 0x04002369 RID: 9065
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private readonly Func<string, string> postProcess;

		// Token: 0x0400236A RID: 9066
		private readonly bool shouldParseRichTextData;

		// Token: 0x0400236B RID: 9067
		private readonly LocalizedString originalStr;

		// Token: 0x0400236D RID: 9069
		public readonly LocalizedString SanitizedString;

		// Token: 0x0400236E RID: 9070
		[Nullable(2)]
		private readonly GUIFont font;

		// Token: 0x0400236F RID: 9071
		[Nullable(2)]
		private readonly GUIComponentStyle componentStyle;

		// Token: 0x04002370 RID: 9072
		private bool forceUpperCase;
	}
}
