using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000363 RID: 867
	public class AddedPunctuationLString : LocalizedString
	{
		// Token: 0x0600430E RID: 17166 RVA: 0x00252162 File Offset: 0x00250362
		[NullableContext(1)]
		public AddedPunctuationLString(char symbol, params LocalizedString[] nStrs)
		{
			this.nestedStrs = nStrs.ToImmutableArray<LocalizedString>();
			this.punctuationSymbol = symbol;
		}

		// Token: 0x17001197 RID: 4503
		// (get) Token: 0x0600430F RID: 17167 RVA: 0x00252182 File Offset: 0x00250382
		public override bool Loaded
		{
			get
			{
				return this.nestedStrs.All((LocalizedString s) => s.Loaded);
			}
		}

		// Token: 0x06004310 RID: 17168 RVA: 0x002521B0 File Offset: 0x002503B0
		public override void RetrieveValue()
		{
			string separator;
			if (GameSettings.CurrentConfig.Language == "French".ToLanguageIdentifier())
			{
				separator = ((this.punctuationSymbol == ':' || this.punctuationSymbol == ';' || this.punctuationSymbol == '!' || this.punctuationSymbol == '?') ? new string(new char[]
				{
					'\u00a0',
					this.punctuationSymbol,
					' '
				}) : new string(new char[]
				{
					this.punctuationSymbol,
					' '
				}));
			}
			else
			{
				separator = new string(new char[]
				{
					this.punctuationSymbol,
					' '
				});
			}
			this.cachedValue = string.Join(separator, from str in this.nestedStrs
			select str.Value);
			base.UpdateLanguage();
		}

		// Token: 0x04002338 RID: 9016
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<LocalizedString> nestedStrs;

		// Token: 0x04002339 RID: 9017
		private readonly char punctuationSymbol;
	}
}
