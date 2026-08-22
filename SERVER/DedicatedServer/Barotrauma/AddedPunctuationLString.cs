using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000297 RID: 663
	public class AddedPunctuationLString : LocalizedString
	{
		// Token: 0x06002E79 RID: 11897 RVA: 0x00137E52 File Offset: 0x00136052
		[NullableContext(1)]
		public AddedPunctuationLString(char symbol, params LocalizedString[] nStrs)
		{
			this.nestedStrs = nStrs.ToImmutableArray<LocalizedString>();
			this.punctuationSymbol = symbol;
		}

		// Token: 0x17000D95 RID: 3477
		// (get) Token: 0x06002E7A RID: 11898 RVA: 0x00137E72 File Offset: 0x00136072
		public override bool Loaded
		{
			get
			{
				return this.nestedStrs.All((LocalizedString s) => s.Loaded);
			}
		}

		// Token: 0x06002E7B RID: 11899 RVA: 0x00137EA0 File Offset: 0x001360A0
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

		// Token: 0x04001758 RID: 5976
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<LocalizedString> nestedStrs;

		// Token: 0x04001759 RID: 5977
		private readonly char punctuationSymbol;
	}
}
