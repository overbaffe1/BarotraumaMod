using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A8 RID: 680
	[NullableContext(1)]
	[Nullable(0)]
	public class RichString
	{
		// Token: 0x17000DAC RID: 3500
		// (get) Token: 0x06002EE4 RID: 12004 RVA: 0x00139187 File Offset: 0x00137387
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

		// Token: 0x17000DAD RID: 3501
		// (get) Token: 0x06002EE5 RID: 12005 RVA: 0x0013919D File Offset: 0x0013739D
		public int Length
		{
			get
			{
				return this.SanitizedValue.Length;
			}
		}

		// Token: 0x17000DAE RID: 3502
		// (get) Token: 0x06002EE6 RID: 12006 RVA: 0x001391AA File Offset: 0x001373AA
		// (set) Token: 0x06002EE7 RID: 12007 RVA: 0x001391B2 File Offset: 0x001373B2
		public LocalizedString NestedStr { get; private set; }

		// Token: 0x17000DAF RID: 3503
		// (get) Token: 0x06002EE8 RID: 12008 RVA: 0x001391BB File Offset: 0x001373BB
		public bool Loaded
		{
			get
			{
				return this.loaded;
			}
		}

		// Token: 0x17000DB0 RID: 3504
		// (get) Token: 0x06002EE9 RID: 12009 RVA: 0x001391C3 File Offset: 0x001373C3
		// (set) Token: 0x06002EEA RID: 12010 RVA: 0x001391CB File Offset: 0x001373CB
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

		// Token: 0x06002EEB RID: 12011 RVA: 0x001391D4 File Offset: 0x001373D4
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
		}

		// Token: 0x06002EEC RID: 12012 RVA: 0x0013922A File Offset: 0x0013742A
		public static RichString Rich(LocalizedString str, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, string> postProcess = null)
		{
			return new RichString(str, true, postProcess);
		}

		// Token: 0x06002EED RID: 12013 RVA: 0x00139234 File Offset: 0x00137434
		public static RichString Plain(LocalizedString str)
		{
			return new RichString(str, false, null);
		}

		// Token: 0x06002EEE RID: 12014 RVA: 0x0013923E File Offset: 0x0013743E
		public static implicit operator LocalizedString(RichString richStr)
		{
			return richStr.NestedStr;
		}

		// Token: 0x06002EEF RID: 12015 RVA: 0x00139246 File Offset: 0x00137446
		public static implicit operator RichString(LocalizedString lStr)
		{
			return RichString.Plain(lStr ?? string.Empty);
		}

		// Token: 0x06002EF0 RID: 12016 RVA: 0x0013925C File Offset: 0x0013745C
		public static implicit operator RichString(string str)
		{
			return str;
		}

		// Token: 0x06002EF1 RID: 12017 RVA: 0x00139269 File Offset: 0x00137469
		protected virtual bool MustRetrieveValue()
		{
			return this.NestedStr.Loaded != this.loaded || this.language != GameSettings.CurrentConfig.Language || this.languageVersion != TextManager.LanguageVersion;
		}

		// Token: 0x06002EF2 RID: 12018 RVA: 0x001392A8 File Offset: 0x001374A8
		public void RetrieveValue()
		{
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

		// Token: 0x06002EF3 RID: 12019 RVA: 0x00139337 File Offset: 0x00137537
		public RichString ToUpper()
		{
			return new RichString(this.NestedStr.ToUpper(), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x06002EF4 RID: 12020 RVA: 0x00139355 File Offset: 0x00137555
		public RichString ToLower()
		{
			return new RichString(this.NestedStr.ToLower(), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x06002EF5 RID: 12021 RVA: 0x00139373 File Offset: 0x00137573
		public RichString Replace(string from, string to, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new RichString(this.NestedStr.Replace(from, to, stringComparison), this.shouldParseRichTextData, this.postProcess);
		}

		// Token: 0x06002EF6 RID: 12022 RVA: 0x00139399 File Offset: 0x00137599
		public override string ToString()
		{
			return this.SanitizedValue;
		}

		// Token: 0x06002EF7 RID: 12023 RVA: 0x001393A1 File Offset: 0x001375A1
		public bool Contains(string str, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return this.SanitizedValue.Contains(str, stringComparison);
		}

		// Token: 0x06002EF8 RID: 12024 RVA: 0x001393B0 File Offset: 0x001375B0
		public bool Contains(char chr, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return this.SanitizedValue.Contains(chr, stringComparison);
		}

		// Token: 0x06002EF9 RID: 12025 RVA: 0x001393BF File Offset: 0x001375BF
		[NullableContext(2)]
		public static bool operator ==(RichString a, RichString b)
		{
			return ((a != null) ? a.SanitizedValue : null) == ((b != null) ? b.SanitizedValue : null);
		}

		// Token: 0x06002EFA RID: 12026 RVA: 0x001393DE File Offset: 0x001375DE
		[NullableContext(2)]
		public static bool operator !=(RichString a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x06002EFB RID: 12027 RVA: 0x001393EA File Offset: 0x001375EA
		[NullableContext(2)]
		public static bool operator ==(RichString a, LocalizedString b)
		{
			return ((a != null) ? a.SanitizedValue : null) == ((b != null) ? b.Value : null);
		}

		// Token: 0x06002EFC RID: 12028 RVA: 0x00139409 File Offset: 0x00137609
		[NullableContext(2)]
		public static bool operator !=(RichString a, LocalizedString b)
		{
			return !(a == b);
		}

		// Token: 0x06002EFD RID: 12029 RVA: 0x00139415 File Offset: 0x00137615
		[NullableContext(2)]
		public static bool operator ==(LocalizedString a, RichString b)
		{
			return ((a != null) ? a.Value : null) == ((b != null) ? b.SanitizedValue : null);
		}

		// Token: 0x06002EFE RID: 12030 RVA: 0x00139434 File Offset: 0x00137634
		[NullableContext(2)]
		public static bool operator !=(LocalizedString a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x06002EFF RID: 12031 RVA: 0x00139440 File Offset: 0x00137640
		[NullableContext(2)]
		public static bool operator ==(RichString a, string b)
		{
			return ((a != null) ? a.SanitizedValue : null) == b;
		}

		// Token: 0x06002F00 RID: 12032 RVA: 0x00139454 File Offset: 0x00137654
		[NullableContext(2)]
		public static bool operator !=(RichString a, string b)
		{
			return !(a == b);
		}

		// Token: 0x06002F01 RID: 12033 RVA: 0x00139460 File Offset: 0x00137660
		[NullableContext(2)]
		public static bool operator ==(string a, RichString b)
		{
			return a == ((b != null) ? b.SanitizedValue : null);
		}

		// Token: 0x06002F02 RID: 12034 RVA: 0x00139474 File Offset: 0x00137674
		[NullableContext(2)]
		public static bool operator !=(string a, RichString b)
		{
			return !(a == b);
		}

		// Token: 0x04001785 RID: 6021
		protected bool loaded;

		// Token: 0x04001786 RID: 6022
		protected LanguageIdentifier language = LanguageIdentifier.None;

		// Token: 0x04001787 RID: 6023
		private int languageVersion;

		// Token: 0x04001788 RID: 6024
		protected string cachedSanitizedValue = "";

		// Token: 0x04001789 RID: 6025
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private readonly Func<string, string> postProcess;

		// Token: 0x0400178A RID: 6026
		private readonly bool shouldParseRichTextData;

		// Token: 0x0400178B RID: 6027
		private readonly LocalizedString originalStr;

		// Token: 0x0400178D RID: 6029
		public readonly LocalizedString SanitizedString;
	}
}
